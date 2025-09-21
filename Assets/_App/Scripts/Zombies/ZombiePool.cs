using UnityEngine;
using System.Collections.Generic;
using PvZ.Core;

namespace PvZ.Zombies
{
    public class ZombiePool : MonoBehaviour
    {
        public static ZombiePool Instance { get; private set; }
        
        [Header("Pool Settings")]
        [SerializeField] private int initialPoolSize = 30;
        [SerializeField] private int maxPoolSize = 100;
        [SerializeField] private bool allowPoolExpansion = true;
        [SerializeField] private Transform poolParent;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        
        // Pool collections
        private Dictionary<string, Queue<ZombieController>> pools;
        private Dictionary<string, List<ZombieController>> activeZombies;
        private Dictionary<string, ZombieData> zombieDataCache;
        
        // Statistics
        private int totalCreated = 0;
        private int totalReused = 0;
        private int totalReturned = 0;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            PrewarmPools();
        }
        
        private void Update()
        {
            if (showDebugInfo)
            {
                CheckActiveZombies();
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializePool()
        {
            pools = new Dictionary<string, Queue<ZombieController>>();
            activeZombies = new Dictionary<string, List<ZombieController>>();
            zombieDataCache = new Dictionary<string, ZombieData>();
            
            // Create pool parent if not assigned
            if (poolParent == null)
            {
                GameObject poolParentGO = new GameObject("ZombiePool");
                poolParentGO.transform.SetParent(transform);
                poolParent = poolParentGO.transform;
            }
            
            Debug.Log("[ZombiePool] Initialized successfully!");
        }
        
        private void PrewarmPools()
        {
            // This would be called after GameDataManager is initialized
            // For now, we'll skip prewarming and create pools on demand
        }
        
        #endregion
        
        #region Pool Management
        
        public ZombieController GetZombie(ZombieData zombieData)
        {
            if (zombieData == null)
            {
                Debug.LogWarning("[ZombiePool] Cannot get zombie: ZombieData is null!");
                return null;
            }
            
            string zombieID = zombieData.zombieID.ToString();
            
            // Cache zombie data for future use
            if (!zombieDataCache.ContainsKey(zombieID))
            {
                zombieDataCache[zombieID] = zombieData;
            }
            
            // Create pool if it doesn't exist
            if (!pools.ContainsKey(zombieID))
            {
                CreatePool(zombieData);
            }
            
            // Get zombie from pool
            ZombieController zombie = GetFromPool(zombieID);
            
            if (zombie == null)
            {
                Debug.LogWarning($"[ZombiePool] Failed to get zombie {zombieID} from pool!");
                return null;
            }
            
            // Add to active zombies
            if (!activeZombies.ContainsKey(zombieID))
            {
                activeZombies[zombieID] = new List<ZombieController>();
            }
            activeZombies[zombieID].Add(zombie);
            
            // Setup zombie with data
            zombie.Initialize(zombieData);
            
            // Setup return callback
            zombie.OnZombieDied += () => ReturnZombie(zombie);
            
            totalReused++;
            
            if (showDebugInfo)
            {
                Debug.Log($"[ZombiePool] Retrieved zombie {zombieID} from pool. Active: {activeZombies[zombieID].Count}");
            }
            
            return zombie;
        }
        
        public void ReturnZombie(ZombieController zombie)
        {
            if (zombie == null) return;
            
            ZombieData data = zombie.ZombieData;
            if (data == null)
            {
                // If we can't identify the zombie type, just destroy it
                Destroy(zombie.gameObject);
                return;
            }
            
            string zombieID = data.zombieID.ToString();
            
            // Remove from active zombies
            if (activeZombies.ContainsKey(zombieID))
            {
                activeZombies[zombieID].Remove(zombie);
            }
            
            // Return to pool
            ReturnToPool(zombieID, zombie);
            
            totalReturned++;
            
            if (showDebugInfo)
            {
                Debug.Log($"[ZombiePool] Returned zombie {zombieID} to pool. Pool size: {pools[zombieID].Count}");
            }
        }
        
        private void CreatePool(ZombieData zombieData)
        {
            string zombieID = zombieData.zombieID.ToString();
            
            pools[zombieID] = new Queue<ZombieController>();
            activeZombies[zombieID] = new List<ZombieController>();
            
            // Pre-populate pool with some instances
            int prewarmCount = Mathf.Min(initialPoolSize / 10, 3); // Start with smaller number for zombies
            for (int i = 0; i < prewarmCount; i++)
            {
                CreateNewZombie(zombieData);
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[ZombiePool] Created pool for {zombieID} with {prewarmCount} instances");
            }
        }
        
        private ZombieController GetFromPool(string zombieID)
        {
            Queue<ZombieController> pool = pools[zombieID];
            
            if (pool.Count > 0)
            {
                // Get from pool
                ZombieController zombie = pool.Dequeue();
                zombie.gameObject.SetActive(true);
                return zombie;
            }
            else
            {
                // Create new if pool is empty and expansion is allowed
                if (allowPoolExpansion && GetTotalPoolSize() < maxPoolSize)
                {
                    ZombieData data = zombieDataCache[zombieID];
                    return CreateNewZombie(data, false); // Don't add to pool immediately
                }
                else
                {
                    Debug.LogWarning($"[ZombiePool] Pool for {zombieID} is empty and cannot expand!");
                    return null;
                }
            }
        }
        
        private void ReturnToPool(string zombieID, ZombieController zombie)
        {
            if (!pools.ContainsKey(zombieID))
            {
                Debug.LogWarning($"[ZombiePool] No pool exists for zombie {zombieID}");
                Destroy(zombie.gameObject);
                return;
            }
            
            // Reset zombie state
            ResetZombie(zombie);
            
            // Add back to pool
            pools[zombieID].Enqueue(zombie);
            zombie.gameObject.SetActive(false);
            zombie.transform.SetParent(poolParent);
        }
        
        private ZombieController CreateNewZombie(ZombieData zombieData, bool addToPool = true)
        {
            if (zombieData.prefab == null)
            {
                Debug.LogError($"[ZombiePool] ZombieData {zombieData.zombieID.ToString()} has no prefab assigned!");
                return null;
            }
            
            GameObject zombieGO = Instantiate(zombieData.prefab, poolParent);
            ZombieController controller = zombieGO.GetComponent<ZombieController>();
            
            if (controller == null)
            {
                Debug.LogError($"[ZombiePool] Zombie prefab {zombieData.prefab.name} has no ZombieController component!");
                Destroy(zombieGO);
                return null;
            }
            
            // Initially disable and add to pool if requested
            zombieGO.SetActive(false);
            if (addToPool)
            {
                pools[zombieData.zombieID.ToString()].Enqueue(controller);
            }
            
            totalCreated++;
            
            return controller;
        }
        
        private void ResetZombie(ZombieController zombie)
        {
            // Clear any event listeners to prevent memory leaks
            zombie.OnZombieDied = null;
            
            // Reset position and rotation
            zombie.transform.position = Vector3.zero;
            zombie.transform.rotation = Quaternion.identity;
            
            // Reset physics
            Rigidbody rb = zombie.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            // Reset any trails or particle effects
            TrailRenderer trail = zombie.GetComponentInChildren<TrailRenderer>();
            if (trail != null)
            {
                trail.Clear();
            }
            
            ParticleSystem[] particles = zombie.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                ps.Stop();
                ps.Clear();
            }
            
            // Reset animator
            Animator animator = zombie.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }
            
            // Reset any other zombie-specific components
            zombie.IsActive = false;
        }
        
        #endregion
        
        #region Pool Utilities
        
        public void PrewarmPool(ZombieData zombieData, int count)
        {
            if (zombieData == null) return;
            
            string zombieID = zombieData.zombieID.ToString();
            
            if (!pools.ContainsKey(zombieID))
            {
                CreatePool(zombieData);
            }
            
            int currentCount = pools[zombieID].Count;
            int toCreate = Mathf.Max(0, count - currentCount);
            
            for (int i = 0; i < toCreate && GetTotalPoolSize() < maxPoolSize; i++)
            {
                CreateNewZombie(zombieData);
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[ZombiePool] Prewarmed pool for {zombieID}: {toCreate} new instances created");
            }
        }
        
        public void ClearPool(string zombieID)
        {
            if (!pools.ContainsKey(zombieID)) return;
            
            // Destroy all pooled instances
            while (pools[zombieID].Count > 0)
            {
                ZombieController zombie = pools[zombieID].Dequeue();
                if (zombie != null)
                {
                    Destroy(zombie.gameObject);
                }
            }
            
            // Destroy all active instances
            if (activeZombies.ContainsKey(zombieID))
            {
                foreach (var zombie in activeZombies[zombieID])
                {
                    if (zombie != null)
                    {
                        Destroy(zombie.gameObject);
                    }
                }
                activeZombies[zombieID].Clear();
            }
            
            pools.Remove(zombieID);
            activeZombies.Remove(zombieID);
            zombieDataCache.Remove(zombieID);
            
            Debug.Log($"[ZombiePool] Cleared pool for {zombieID}");
        }
        
        public void ClearAllPools()
        {
            var zombieIDs = new List<string>(pools.Keys);
            foreach (string id in zombieIDs)
            {
                ClearPool(id);
            }
            
            Debug.Log("[ZombiePool] Cleared all zombie pools");
        }
        
        public void ReturnAllActiveZombies()
        {
            foreach (var kvp in activeZombies)
            {
                var zombieList = new List<ZombieController>(kvp.Value); // Create copy to avoid modification during iteration
                foreach (var zombie in zombieList)
                {
                    if (zombie != null && zombie.IsActive)
                    {
                        zombie.Die(); // This will trigger the return callback
                    }
                }
            }
        }
        
        private int GetTotalPoolSize()
        {
            int total = 0;
            foreach (var pool in pools.Values)
            {
                total += pool.Count;
            }
            
            foreach (var activeList in activeZombies.Values)
            {
                total += activeList.Count;
            }
            
            return total;
        }
        
        private void CheckActiveZombies()
        {
            // Clean up null references in active zombies
            foreach (var kvp in activeZombies)
            {
                kvp.Value.RemoveAll(z => z == null);
            }
        }
        
        #endregion
        
        #region Statistics
        
        public int GetPoolSize(string zombieID)
        {
            return pools.ContainsKey(zombieID) ? pools[zombieID].Count : 0;
        }
        
        public int GetActiveCount(string zombieID)
        {
            return activeZombies.ContainsKey(zombieID) ? activeZombies[zombieID].Count : 0;
        }
        
        public int GetTotalActiveZombies()
        {
            int total = 0;
            foreach (var activeList in activeZombies.Values)
            {
                total += activeList.Count;
            }
            return total;
        }
        
        public int GetTotalCreated() => totalCreated;
        public int GetTotalReused() => totalReused;
        public int GetTotalReturned() => totalReturned;
        
        public float GetReusePercentage()
        {
            return totalCreated > 0 ? (float)totalReused / totalCreated * 100f : 0f;
        }
        
        public Dictionary<string, ZombiePoolInfo> GetPoolStatistics()
        {
            var stats = new Dictionary<string, ZombiePoolInfo>();
            
            foreach (var poolKvp in pools)
            {
                string id = poolKvp.Key;
                stats[id] = new ZombiePoolInfo
                {
                    pooledCount = poolKvp.Value.Count,
                    activeCount = GetActiveCount(id),
                    totalCount = poolKvp.Value.Count + GetActiveCount(id)
                };
            }
            
            return stats;
        }
        
        #endregion
        
        #region Debug
        
        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Zombie Pool Debug Info", GUI.skin.box);
            GUILayout.Label($"Total Created: {totalCreated}");
            GUILayout.Label($"Total Reused: {totalReused}");
            GUILayout.Label($"Total Returned: {totalReturned}");
            GUILayout.Label($"Reuse Rate: {GetReusePercentage():F1}%");
            GUILayout.Label($"Total Pool Size: {GetTotalPoolSize()}");
            GUILayout.Label($"Total Active: {GetTotalActiveZombies()}");
            
            GUILayout.Space(10);
            GUILayout.Label("Pool Details:");
            
            var stats = GetPoolStatistics();
            foreach (var kvp in stats)
            {
                GUILayout.Label($"{kvp.Key}: {kvp.Value.pooledCount} pooled, {kvp.Value.activeCount} active");
            }
            
            GUILayout.EndArea();
        }
        
#if UNITY_EDITOR
        [ContextMenu("Clear All Pools")]
        private void DebugClearAllPools()
        {
            ClearAllPools();
        }
        
        [ContextMenu("Return All Active Zombies")]
        private void DebugReturnAllActiveZombies()
        {
            ReturnAllActiveZombies();
        }
        
        [ContextMenu("Print Pool Statistics")]
        private void DebugPrintStatistics()
        {
            Debug.Log($"[ZombiePool] Statistics - Created: {totalCreated}, Reused: {totalReused}, Returned: {totalReturned}");
            Debug.Log($"[ZombiePool] Reuse Rate: {GetReusePercentage():F1}%");
            
            var stats = GetPoolStatistics();
            foreach (var kvp in stats)
            {
                Debug.Log($"[ZombiePool] {kvp.Key}: {kvp.Value.pooledCount} pooled, {kvp.Value.activeCount} active, {kvp.Value.totalCount} total");
            }
        }
#endif
        
        #endregion
    }
    
    [System.Serializable]
    public class ZombiePoolInfo
    {
        public int pooledCount;
        public int activeCount;
        public int totalCount;
    }
}
