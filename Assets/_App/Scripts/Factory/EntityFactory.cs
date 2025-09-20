using UnityEngine;
using PvZ.Core;
using PvZ.Plants;
using PvZ.Zombies;
using PvZ.Projectiles;
using PvZ.Managers;

namespace PvZ.Factory
{
    /// <summary>
    /// Factory pattern for creating all game entities
    /// </summary>
    public static class EntityFactory
    {
        #region Plant Creation
        
        public static PlantController CreatePlant(PlantData plantData, Vector3 position, Transform parent = null)
        {
            if (plantData == null)
            {
                Debug.LogError("Cannot create plant: PlantData is null!");
                return null;
            }
            
            if (plantData.prefab == null)
            {
                Debug.LogError($"Cannot create plant {plantData.plantID}: Prefab is null!");
                return null;
            }
            
            // Instantiate the plant prefab
            GameObject plantGO = Object.Instantiate(plantData.prefab, position, Quaternion.identity, parent);
            
            // Get or add PlantController
            PlantController controller = plantGO.GetComponent<PlantController>();
            if (controller == null)
            {
                controller = plantGO.AddComponent<PlantController>();
            }
            
            // Set the plant data
            controller.SetPlantData(plantData);
            
            // Register with managers
            EntityManager.Instance?.RegisterEntity(controller);
            
            // Raise event
            GameEventManager.Instance?.RaisePlantPlanted(controller);
            
            Debug.Log($"Created plant: {plantData.plantID} at {position}");
            return controller;
        }
        
        public static PlantController CreatePlant(string plantID, Vector3 position, Transform parent = null)
        {
            var plantData = GameDataManager.Instance?.GetPlant(plantID);
            if (plantData == null)
            {
                Debug.LogError($"Cannot create plant: PlantData for ID '{plantID}' not found!");
                return null;
            }
            
            return CreatePlant(plantData, position, parent);
        }
        
        #endregion
        
        #region Zombie Creation
        
        public static ZombieController CreateZombie(ZombieData zombieData, Vector3 position, Transform parent = null)
        {
            if (zombieData == null)
            {
                Debug.LogError("Cannot create zombie: ZombieData is null!");
                return null;
            }
            
            if (zombieData.prefab == null)
            {
                Debug.LogError($"Cannot create zombie {zombieData.zombieID}: Prefab is null!");
                return null;
            }
            
            // Instantiate the zombie prefab
            GameObject zombieGO = Object.Instantiate(zombieData.prefab, position, Quaternion.identity, parent);
            
            // Get or add ZombieController
            ZombieController controller = zombieGO.GetComponent<ZombieController>();
            if (controller == null)
            {
                controller = zombieGO.AddComponent<ZombieController>();
            }
            
            // Set the zombie data
            controller.SetZombieData(zombieData);
            
            // Register with managers
            EntityManager.Instance?.RegisterEntity(controller);
            
            // Raise event
            GameEventManager.Instance?.RaiseZombieSpawned(controller);
            
            Debug.Log($"Created zombie: {zombieData.zombieID} at {position}");
            return controller;
        }
        
        public static ZombieController CreateZombie(string zombieID, Vector3 position, Transform parent = null)
        {
            var zombieData = GameDataManager.Instance?.GetZombie(zombieID);
            if (zombieData == null)
            {
                Debug.LogError($"Cannot create zombie: ZombieData for ID '{zombieID}' not found!");
                return null;
            }
            
            return CreateZombie(zombieData, position, parent);
        }
        
        #endregion
        
        #region Projectile Creation
        
        public static ProjectileController CreateProjectile(ProjectileData projectileData, Vector3 position, Vector3 direction, IEntity owner)
        {
            if (projectileData == null)
            {
                Debug.LogError("Cannot create projectile: ProjectileData is null!");
                return null;
            }
            
            // Use projectile pool for better performance
            ProjectileController projectile = ProjectilePool.Instance?.GetProjectile(projectileData);
            
            if (projectile != null)
            {
                // Initialize the projectile
                projectile.Initialize(projectileData, position, direction, owner);
                
                // Raise event
                GameEventManager.Instance?.RaiseProjectileFired(projectile);
                
                return projectile;
            }
            else
            {
                // Fallback: create directly if pool is not available
                return CreateProjectileDirectly(projectileData, position, direction, owner);
            }
        }
        
        public static ProjectileController CreateProjectile(string projectileID, Vector3 position, Vector3 direction, IEntity owner)
        {
            var projectileData = GameDataManager.Instance?.GetProjectile(projectileID);
            if (projectileData == null)
            {
                Debug.LogError($"Cannot create projectile: ProjectileData for ID '{projectileID}' not found!");
                return null;
            }
            
            return CreateProjectile(projectileData, position, direction, owner);
        }
        
        private static ProjectileController CreateProjectileDirectly(ProjectileData projectileData, Vector3 position, Vector3 direction, IEntity owner)
        {
            if (projectileData.prefab == null)
            {
                Debug.LogError($"Cannot create projectile {projectileData.projectileID}: Prefab is null!");
                return null;
            }
            
            // Instantiate the projectile prefab
            GameObject projectileGO = Object.Instantiate(projectileData.prefab, position, Quaternion.identity);
            
            // Get or add ProjectileController
            ProjectileController controller = projectileGO.GetComponent<ProjectileController>();
            if (controller == null)
            {
                controller = projectileGO.AddComponent<ProjectileController>();
            }
            
            // Initialize the projectile
            controller.Initialize(projectileData, position, direction, owner);
            
            // Register with managers
            EntityManager.Instance?.RegisterEntity(controller);
            
            Debug.Log($"Created projectile directly: {projectileData.projectileID}");
            return controller;
        }
        
        #endregion
        
        #region Batch Creation
        
        public static PlantController[] CreateMultiplePlants(PlantData plantData, Vector3[] positions, Transform parent = null)
        {
            if (plantData == null || positions == null)
                return new PlantController[0];
            
            PlantController[] plants = new PlantController[positions.Length];
            
            for (int i = 0; i < positions.Length; i++)
            {
                plants[i] = CreatePlant(plantData, positions[i], parent);
            }
            
            return plants;
        }
        
        public static ZombieController[] CreateMultipleZombies(ZombieData zombieData, Vector3[] positions, Transform parent = null)
        {
            if (zombieData == null || positions == null)
                return new ZombieController[0];
            
            ZombieController[] zombies = new ZombieController[positions.Length];
            
            for (int i = 0; i < positions.Length; i++)
            {
                zombies[i] = CreateZombie(zombieData, positions[i], parent);
            }
            
            return zombies;
        }
        
        #endregion
        
        #region Advanced Creation Methods
        
        public static PlantController CreatePlantWithUpgrades(PlantData plantData, Vector3 position, string[] upgradeIDs, Transform parent = null)
        {
            var plant = CreatePlant(plantData, position, parent);
            
            if (plant != null && upgradeIDs != null)
            {
                // Apply upgrades (would need an upgrade system)
                foreach (string upgradeID in upgradeIDs)
                {
                    ApplyUpgradeToPlant(plant, upgradeID);
                }
            }
            
            return plant;
        }
        
        public static ZombieController CreateZombieWithModifiers(ZombieData zombieData, Vector3 position, float healthMultiplier, float speedMultiplier, Transform parent = null)
        {
            var zombie = CreateZombie(zombieData, position, parent);
            
            if (zombie != null)
            {
                // Apply modifiers
                zombie.Health *= healthMultiplier;
                // Would need to modify speed through zombie data or component
            }
            
            return zombie;
        }
        
        private static void ApplyUpgradeToPlant(PlantController plant, string upgradeID)
        {
            // Placeholder for upgrade system
            Debug.Log($"Applied upgrade {upgradeID} to plant {plant.ID}");
        }
        
        #endregion
        
        #region Destruction
        
        public static void DestroyEntity(IEntity entity)
        {
            if (entity == null) return;
            
            // Unregister from managers
            EntityManager.Instance?.UnregisterEntity(entity);
            
            // Raise appropriate events
            if (entity is PlantController)
            {
                GameEventManager.Instance?.RaisePlantDestroyed(entity);
            }
            else if (entity is ZombieController)
            {
                GameEventManager.Instance?.RaiseZombieKilled(entity);
            }
            
            // Destroy the game object
            if (entity is MonoBehaviour mb && mb != null)
            {
                Object.Destroy(mb.gameObject);
            }
        }
        
        public static void DestroyAllEntitiesOfType<T>() where T : class, IEntity
        {
            var entities = EntityManager.Instance?.GetAllEntities();
            if (entities == null) return;
            
            foreach (var entity in entities)
            {
                if (entity is T)
                {
                    DestroyEntity(entity);
                }
            }
        }
        
        #endregion
        
        #region Validation
        
        public static bool CanCreatePlant(string plantID, Vector3 position)
        {
            var plantData = GameDataManager.Instance?.GetPlant(plantID);
            if (plantData == null) return false;
            
            // Add position validation logic
            // Check if position is valid, not occupied, etc.
            
            return true;
        }
        
        public static bool CanCreateZombie(string zombieID, Vector3 position)
        {
            var zombieData = GameDataManager.Instance?.GetZombie(zombieID);
            if (zombieData == null) return false;
            
            // Add spawn validation logic
            
            return true;
        }
        
        #endregion
    }
}
