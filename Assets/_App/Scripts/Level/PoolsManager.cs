using UnityEngine;
using PvZ.Projectiles;
using PvZ.Zombies;

namespace PvZ.Level
{
    /// <summary>
    /// Manager to ensure all necessary pools are created and initialized
    /// </summary>
    public class PoolsManager : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private bool createProjectilePool = true;
        [SerializeField] private bool createZombiePool = true;
        
        private void Awake()
        {
            InitializePools();
        }
        
        private void InitializePools()
        {
            CreateProjectilePool();
            CreateZombiePool();
            
        }
        
        private void CreateProjectilePool()
        {
            if (!createProjectilePool) return;
            
            if (ProjectilePool.Instance == null)
            {
                GameObject poolGO = new GameObject("ProjectilePool");
                poolGO.transform.SetParent(transform);
                poolGO.AddComponent<ProjectilePool>();
                
            }
        }
        
        private void CreateZombiePool()
        {
            if (!createZombiePool) return;
            
            if (ZombiePool.Instance == null)
            {
                GameObject poolGO = new GameObject("ZombiePool");
                poolGO.transform.SetParent(transform);
                poolGO.AddComponent<ZombiePool>();

            }
        }
        
        #region Public Methods
        
        public void ClearAllPools()
        {
            ProjectilePool.Instance?.ClearAllPools();
            ZombiePool.Instance?.ClearAllPools();
            
 
        }
        
        public void GetPoolStatistics()
        {
            if (ProjectilePool.Instance != null)
            {
                var projectileStats = ProjectilePool.Instance.GetPoolStatistics();
                Debug.Log($"[PoolsManager] ProjectilePool - Pools: {projectileStats.Count}");
                foreach (var stat in projectileStats)
                {
                    Debug.Log($"  {stat.Key}: {stat.Value.activeCount} active, {stat.Value.pooledCount} pooled");
                }
            }
            
            if (ZombiePool.Instance != null)
            {
                var zombieStats = ZombiePool.Instance.GetPoolStatistics();
                Debug.Log($"[PoolsManager] ZombiePool - Pools: {zombieStats.Count}");
                foreach (var stat in zombieStats)
                {
                    Debug.Log($"  {stat.Key}: {stat.Value.activeCount} active, {stat.Value.pooledCount} pooled");
                }
            }
        }
        
        #endregion
        
        #region Debug
        
#if UNITY_EDITOR
        [ContextMenu("Clear All Pools")]
        private void DebugClearAllPools()
        {
            ClearAllPools();
        }
        
        [ContextMenu("Print Pool Statistics")]
        private void DebugPrintStatistics()
        {
            GetPoolStatistics();
        }
#endif
        
        #endregion
    }
}
