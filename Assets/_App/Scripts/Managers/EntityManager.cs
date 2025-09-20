using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PvZ.Core;

namespace PvZ.Managers
{
    public class EntityManager : MonoBehaviour
    {
        public static EntityManager Instance { get; private set; }
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        
        // Entity collections
        private List<IEntity> allEntities;
        private List<ITargetable> targetableEntities;
        private Dictionary<string, List<IEntity>> entitiesByType;
        
        // Performance optimization
        private float updateInterval = 0.1f; // Update targeting every 0.1 seconds
        private float lastUpdateTime;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCollections();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                CleanupInvalidEntities();
                lastUpdateTime = Time.time;
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeCollections()
        {
            allEntities = new List<IEntity>();
            targetableEntities = new List<ITargetable>();
            entitiesByType = new Dictionary<string, List<IEntity>>();
        }
        
        #endregion
        
        #region Entity Registration
        
        public void RegisterEntity(IEntity entity)
        {
            if (entity == null || allEntities.Contains(entity))
                return;
            
            allEntities.Add(entity);
            
            // Register as targetable if applicable
            if (entity is ITargetable targetable)
            {
                targetableEntities.Add(targetable);
            }
            
            // Register by type
            string entityType = GetEntityType(entity);
            if (!entitiesByType.ContainsKey(entityType))
            {
                entitiesByType[entityType] = new List<IEntity>();
            }
            entitiesByType[entityType].Add(entity);
            
            if (showDebugInfo)
            {
                Debug.Log($"Registered entity: {entity.ID} of type {entityType}");
            }
        }
        
        public void UnregisterEntity(IEntity entity)
        {
            if (entity == null)
                return;
            
            allEntities.Remove(entity);
            
            // Unregister from targetable
            if (entity is ITargetable targetable)
            {
                targetableEntities.Remove(targetable);
            }
            
            // Unregister from type collections
            string entityType = GetEntityType(entity);
            if (entitiesByType.ContainsKey(entityType))
            {
                entitiesByType[entityType].Remove(entity);
                
                // Clean up empty type collections
                if (entitiesByType[entityType].Count == 0)
                {
                    entitiesByType.Remove(entityType);
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Unregistered entity: {entity.ID} of type {entityType}");
            }
        }
        
        private string GetEntityType(IEntity entity)
        {
            if (entity == null) return "Unknown";
            
            var entityType = entity.GetType();
            if (entityType.Namespace == "PvZ.Plants")
                return "Plant";
            else if (entityType.Namespace == "PvZ.Zombies")
                return "Zombie";
            else if (entityType.Namespace == "PvZ.Projectiles")
                return "Projectile";
            else
                return entityType.Name;
        }
        
        #endregion
        
        #region Entity Queries
        
        public IEntity[] GetAllEntities()
        {
            return allEntities.Where(e => e != null && e.IsActive).ToArray();
        }
        
        public ITargetable[] GetAllTargetableEntities()
        {
            return targetableEntities.Where(t => t != null && t.IsValidTarget()).ToArray();
        }
        
        public IEntity[] GetEntitiesByType(string entityType)
        {
            if (entitiesByType.ContainsKey(entityType))
            {
                return entitiesByType[entityType].Where(e => e != null && e.IsActive).ToArray();
            }
            return new IEntity[0];
        }
        
        public IEntity[] GetAllPlants()
        {
            return GetEntitiesByType("Plant");
        }
        
        public IEntity[] GetAllZombies()
        {
            return GetEntitiesByType("Zombie");
        }
        
        public IEntity[] GetAllProjectiles()
        {
            return GetEntitiesByType("Projectile");
        }
        
        #endregion
        
        #region Targeting System
        
        public ITargetable FindNearestTarget(Vector3 position, float range, System.Func<ITargetable, bool> filter = null)
        {
            float closestDistance = range;
            ITargetable closestTarget = null;
            
            foreach (var target in targetableEntities)
            {
                if (target == null || !target.IsValidTarget())
                    continue;
                
                // Apply custom filter if provided
                if (filter != null && !filter(target))
                    continue;
                
                float distance = Vector3.Distance(position, target.GetTargetPosition());
                if (distance <= range && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }
            
            return closestTarget;
        }
        
        public ITargetable[] FindTargetsInRange(Vector3 position, float range, System.Func<ITargetable, bool> filter = null)
        {
            var targets = new List<ITargetable>();
            
            foreach (var target in targetableEntities)
            {
                if (target == null || !target.IsValidTarget())
                    continue;
                
                // Apply custom filter if provided
                if (filter != null && !filter(target))
                    continue;
                
                float distance = Vector3.Distance(position, target.GetTargetPosition());
                if (distance <= range)
                {
                    targets.Add(target);
                }
            }
            
            return targets.ToArray();
        }
        
        public ITargetable FindHighestPriorityTarget(Vector3 position, float range, System.Func<ITargetable, bool> filter = null)
        {
            float highestPriority = -1f;
            ITargetable priorityTarget = null;
            
            foreach (var target in targetableEntities)
            {
                if (target == null || !target.IsValidTarget())
                    continue;
                
                // Apply custom filter if provided
                if (filter != null && !filter(target))
                    continue;
                
                float distance = Vector3.Distance(position, target.GetTargetPosition());
                if (distance <= range)
                {
                    float priority = target.GetPriority();
                    if (priority > highestPriority)
                    {
                        highestPriority = priority;
                        priorityTarget = target;
                    }
                }
            }
            
            return priorityTarget;
        }
        
        public ITargetable FindRandomTarget(Vector3 position, float range, System.Func<ITargetable, bool> filter = null)
        {
            var validTargets = FindTargetsInRange(position, range, filter);
            
            if (validTargets.Length == 0)
                return null;
            
            int randomIndex = Random.Range(0, validTargets.Length);
            return validTargets[randomIndex];
        }
        
        #endregion
        
        #region Spatial Queries
        
        public IEntity[] GetEntitiesInRadius(Vector3 center, float radius, System.Func<IEntity, bool> filter = null)
        {
            var entitiesInRadius = new List<IEntity>();
            
            foreach (var entity in allEntities)
            {
                if (entity == null || !entity.IsActive)
                    continue;
                
                float distance = Vector3.Distance(center, entity.Position);
                if (distance <= radius)
                {
                    // Apply custom filter if provided
                    if (filter == null || filter(entity))
                    {
                        entitiesInRadius.Add(entity);
                    }
                }
            }
            
            return entitiesInRadius.ToArray();
        }
        
        public IEntity[] GetEntitiesInBounds(Bounds bounds, System.Func<IEntity, bool> filter = null)
        {
            var entitiesInBounds = new List<IEntity>();
            
            foreach (var entity in allEntities)
            {
                if (entity == null || !entity.IsActive)
                    continue;
                
                if (bounds.Contains(entity.Position))
                {
                    // Apply custom filter if provided
                    if (filter == null || filter(entity))
                    {
                        entitiesInBounds.Add(entity);
                    }
                }
            }
            
            return entitiesInBounds.ToArray();
        }
        
        #endregion
        
        #region Utility Methods
        
        private void CleanupInvalidEntities()
        {
            // Clean up null or inactive entities
            allEntities.RemoveAll(e => e == null || !e.IsActive);
            targetableEntities.RemoveAll(t => t == null || !t.IsValidTarget());
            
            // Clean up type collections
            var typesToRemove = new List<string>();
            foreach (var kvp in entitiesByType)
            {
                kvp.Value.RemoveAll(e => e == null || !e.IsActive);
                if (kvp.Value.Count == 0)
                {
                    typesToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var type in typesToRemove)
            {
                entitiesByType.Remove(type);
            }
        }
        
        public void ClearAllEntities()
        {
            allEntities.Clear();
            targetableEntities.Clear();
            entitiesByType.Clear();
            
            Debug.Log("All entities cleared from EntityManager");
        }
        
        #endregion
        
        #region Statistics
        
        public int GetEntityCount() => allEntities.Count;
        public int GetTargetableCount() => targetableEntities.Count;
        public int GetPlantCount() => GetEntitiesByType("Plant").Length;
        public int GetZombieCount() => GetEntitiesByType("Zombie").Length;
        public int GetProjectileCount() => GetEntitiesByType("Projectile").Length;
        
        public Dictionary<string, int> GetEntityCountByType()
        {
            var counts = new Dictionary<string, int>();
            foreach (var kvp in entitiesByType)
            {
                counts[kvp.Key] = kvp.Value.Count(e => e != null && e.IsActive);
            }
            return counts;
        }
        
        #endregion
        
        #region Debug
        
        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Entity Manager Debug Info", GUI.skin.box);
            GUILayout.Label($"Total Entities: {GetEntityCount()}");
            GUILayout.Label($"Targetable: {GetTargetableCount()}");
            GUILayout.Label($"Plants: {GetPlantCount()}");
            GUILayout.Label($"Zombies: {GetZombieCount()}");
            GUILayout.Label($"Projectiles: {GetProjectileCount()}");
            
            var typeCounts = GetEntityCountByType();
            foreach (var kvp in typeCounts)
            {
                GUILayout.Label($"{kvp.Key}: {kvp.Value}");
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
