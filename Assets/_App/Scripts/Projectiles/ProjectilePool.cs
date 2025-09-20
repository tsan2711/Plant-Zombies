using UnityEngine;
using System.Collections.Generic;
using PvZ.Core;

namespace PvZ.Projectiles
{
    public class ProjectilePool : MonoBehaviour
    {
        public static ProjectilePool Instance { get; private set; }
        
        [Header("Pool Settings")]
        [SerializeField] private int initialPoolSize = 50;
        [SerializeField] private int maxPoolSize = 200;
        [SerializeField] private bool allowPoolExpansion = true;
        [SerializeField] private Transform poolParent;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        
        // Pool collections
        private Dictionary<string, Queue<ProjectileController>> pools;
        private Dictionary<string, List<ProjectileController>> activeProjectiles;
        private Dictionary<string, ProjectileData> projectileDataCache;
        
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
                CheckActiveProjectiles();
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializePool()
        {
            pools = new Dictionary<string, Queue<ProjectileController>>();
            activeProjectiles = new Dictionary<string, List<ProjectileController>>();
            projectileDataCache = new Dictionary<string, ProjectileData>();
            
            // Create pool parent if not assigned
            if (poolParent == null)
            {
                GameObject poolParentGO = new GameObject("ProjectilePool");
                poolParentGO.transform.SetParent(transform);
                poolParent = poolParentGO.transform;
            }
            
            Debug.Log("ProjectilePool initialized successfully!");
        }
        
        private void PrewarmPools()
        {
            // This would be called after GameDataManager is initialized
            // For now, we'll skip prewarming and create pools on demand
        }
        
        #endregion
        
        #region Pool Management
        
        public ProjectileController GetProjectile(ProjectileData projectileData)
        {
            if (projectileData == null)
            {
                Debug.LogWarning("Cannot get projectile: ProjectileData is null!");
                return null;
            }
            
            string projectileID = projectileData.projectileID;
            
            // Cache projectile data for future use
            if (!projectileDataCache.ContainsKey(projectileID))
            {
                projectileDataCache[projectileID] = projectileData;
            }
            
            // Create pool if it doesn't exist
            if (!pools.ContainsKey(projectileID))
            {
                CreatePool(projectileData);
            }
            
            // Get projectile from pool
            ProjectileController projectile = GetFromPool(projectileID);
            
            // Add to active projectiles
            if (!activeProjectiles.ContainsKey(projectileID))
            {
                activeProjectiles[projectileID] = new List<ProjectileController>();
            }
            activeProjectiles[projectileID].Add(projectile);
            
            totalReused++;
            
            if (showDebugInfo)
            {
                Debug.Log($"Retrieved projectile {projectileID} from pool. Active: {activeProjectiles[projectileID].Count}");
            }
            
            return projectile;
        }
        
        public void ReturnProjectile(ProjectileController projectile)
        {
            if (projectile == null) return;
            
            ProjectileData data = projectile.ProjectileData;
            if (data == null)
            {
                // If we can't identify the projectile type, just destroy it
                Destroy(projectile.gameObject);
                return;
            }
            
            string projectileID = data.projectileID;
            
            // Remove from active projectiles
            if (activeProjectiles.ContainsKey(projectileID))
            {
                activeProjectiles[projectileID].Remove(projectile);
            }
            
            // Return to pool
            ReturnToPool(projectileID, projectile);
            
            totalReturned++;
            
            if (showDebugInfo)
            {
                Debug.Log($"Returned projectile {projectileID} to pool. Pool size: {pools[projectileID].Count}");
            }
        }
        
        private void CreatePool(ProjectileData projectileData)
        {
            string projectileID = projectileData.projectileID;
            
            pools[projectileID] = new Queue<ProjectileController>();
            activeProjectiles[projectileID] = new List<ProjectileController>();
            
            // Pre-populate pool with some instances
            int prewarmCount = Mathf.Min(initialPoolSize / 10, 5); // Start with smaller number
            for (int i = 0; i < prewarmCount; i++)
            {
                CreateNewProjectile(projectileData);
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Created pool for {projectileID} with {prewarmCount} instances");
            }
        }
        
        private ProjectileController GetFromPool(string projectileID)
        {
            Queue<ProjectileController> pool = pools[projectileID];
            
            if (pool.Count > 0)
            {
                // Get from pool
                ProjectileController projectile = pool.Dequeue();
                projectile.gameObject.SetActive(true);
                return projectile;
            }
            else
            {
                // Create new if pool is empty and expansion is allowed
                if (allowPoolExpansion && GetTotalPoolSize() < maxPoolSize)
                {
                    ProjectileData data = projectileDataCache[projectileID];
                    return CreateNewProjectile(data);
                }
                else
                {
                    Debug.LogWarning($"Pool for {projectileID} is empty and cannot expand!");
                    return null;
                }
            }
        }
        
        private void ReturnToPool(string projectileID, ProjectileController projectile)
        {
            if (!pools.ContainsKey(projectileID))
            {
                Debug.LogWarning($"No pool exists for projectile {projectileID}");
                Destroy(projectile.gameObject);
                return;
            }
            
            // Reset projectile state
            ResetProjectile(projectile);
            
            // Add back to pool
            pools[projectileID].Enqueue(projectile);
            projectile.gameObject.SetActive(false);
            projectile.transform.SetParent(poolParent);
        }
        
        private ProjectileController CreateNewProjectile(ProjectileData projectileData)
        {
            if (projectileData.prefab == null)
            {
                Debug.LogError($"ProjectileData {projectileData.projectileID} has no prefab assigned!");
                return null;
            }
            
            GameObject projectileGO = Instantiate(projectileData.prefab, poolParent);
            ProjectileController controller = projectileGO.GetComponent<ProjectileController>();
            
            if (controller == null)
            {
                controller = projectileGO.AddComponent<ProjectileController>();
            }
            
            // Initially disable and add to pool
            projectileGO.SetActive(false);
            pools[projectileData.projectileID].Enqueue(controller);
            
            totalCreated++;
            
            return controller;
        }
        
        private void ResetProjectile(ProjectileController projectile)
        {
            // Reset position and rotation
            projectile.transform.position = Vector3.zero;
            projectile.transform.rotation = Quaternion.identity;
            
            // Reset physics
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            // Reset any trails or particle effects
            TrailRenderer trail = projectile.GetComponentInChildren<TrailRenderer>();
            if (trail != null)
            {
                trail.Clear();
            }
            
            ParticleSystem[] particles = projectile.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                ps.Stop();
                ps.Clear();
            }
        }
        
        #endregion
        
        #region Pool Utilities
        
        public void PrewarmPool(ProjectileData projectileData, int count)
        {
            if (projectileData == null) return;
            
            string projectileID = projectileData.projectileID;
            
            if (!pools.ContainsKey(projectileID))
            {
                CreatePool(projectileData);
            }
            
            int currentCount = pools[projectileID].Count;
            int toCreate = Mathf.Max(0, count - currentCount);
            
            for (int i = 0; i < toCreate && GetTotalPoolSize() < maxPoolSize; i++)
            {
                CreateNewProjectile(projectileData);
            }
            
            Debug.Log($"Prewarmed pool for {projectileID}: {toCreate} new instances created");
        }
        
        public void ClearPool(string projectileID)
        {
            if (!pools.ContainsKey(projectileID)) return;
            
            // Destroy all pooled instances
            while (pools[projectileID].Count > 0)
            {
                ProjectileController projectile = pools[projectileID].Dequeue();
                if (projectile != null)
                {
                    Destroy(projectile.gameObject);
                }
            }
            
            // Destroy all active instances
            if (activeProjectiles.ContainsKey(projectileID))
            {
                foreach (var projectile in activeProjectiles[projectileID])
                {
                    if (projectile != null)
                    {
                        Destroy(projectile.gameObject);
                    }
                }
                activeProjectiles[projectileID].Clear();
            }
            
            pools.Remove(projectileID);
            activeProjectiles.Remove(projectileID);
            projectileDataCache.Remove(projectileID);
            
            Debug.Log($"Cleared pool for {projectileID}");
        }
        
        public void ClearAllPools()
        {
            var projectileIDs = new List<string>(pools.Keys);
            foreach (string id in projectileIDs)
            {
                ClearPool(id);
            }
            
            Debug.Log("Cleared all projectile pools");
        }
        
        private int GetTotalPoolSize()
        {
            int total = 0;
            foreach (var pool in pools.Values)
            {
                total += pool.Count;
            }
            
            foreach (var activeList in activeProjectiles.Values)
            {
                total += activeList.Count;
            }
            
            return total;
        }
        
        private void CheckActiveProjectiles()
        {
            // Clean up null references in active projectiles
            foreach (var kvp in activeProjectiles)
            {
                kvp.Value.RemoveAll(p => p == null);
            }
        }
        
        #endregion
        
        #region Statistics
        
        public int GetPoolSize(string projectileID)
        {
            return pools.ContainsKey(projectileID) ? pools[projectileID].Count : 0;
        }
        
        public int GetActiveCount(string projectileID)
        {
            return activeProjectiles.ContainsKey(projectileID) ? activeProjectiles[projectileID].Count : 0;
        }
        
        public int GetTotalCreated() => totalCreated;
        public int GetTotalReused() => totalReused;
        public int GetTotalReturned() => totalReturned;
        
        public float GetReusePercentage()
        {
            return totalCreated > 0 ? (float)totalReused / totalCreated * 100f : 0f;
        }
        
        public Dictionary<string, PoolInfo> GetPoolStatistics()
        {
            var stats = new Dictionary<string, PoolInfo>();
            
            foreach (var poolKvp in pools)
            {
                string id = poolKvp.Key;
                stats[id] = new PoolInfo
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
            
            GUILayout.BeginArea(new Rect(10, 220, 300, 300));
            GUILayout.Label("Projectile Pool Debug Info", GUI.skin.box);
            GUILayout.Label($"Total Created: {totalCreated}");
            GUILayout.Label($"Total Reused: {totalReused}");
            GUILayout.Label($"Total Returned: {totalReturned}");
            GUILayout.Label($"Reuse Rate: {GetReusePercentage():F1}%");
            GUILayout.Label($"Total Pool Size: {GetTotalPoolSize()}");
            
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
        
        [ContextMenu("Print Pool Statistics")]
        private void DebugPrintStatistics()
        {
            Debug.Log($"Pool Statistics - Created: {totalCreated}, Reused: {totalReused}, Returned: {totalReturned}");
            Debug.Log($"Reuse Rate: {GetReusePercentage():F1}%");
            
            var stats = GetPoolStatistics();
            foreach (var kvp in stats)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value.pooledCount} pooled, {kvp.Value.activeCount} active, {kvp.Value.totalCount} total");
            }
        }
#endif
        
        #endregion
    }
    
    [System.Serializable]
    public class PoolInfo
    {
        public int pooledCount;
        public int activeCount;
        public int totalCount;
    }
}
