using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PvZ.Plants;
using PvZ.Zombies;
using PvZ.Projectiles;

namespace PvZ.Managers
{
    [CreateAssetMenu(fileName = "Game Data Manager", menuName = "PvZ/Managers/Game Data")]
    public class GameDataManager : ScriptableObject
    {
        [Header("Plants")]
        public PlantData[] allPlants;
        
        [Header("Zombies")]
        public ZombieData[] allZombies;
        
        [Header("Projectiles")]
        public ProjectileData[] allProjectiles;
        
        [Header("Abilities")]
        public PlantAbilityData[] allPlantAbilities;
        public ZombieAbilityData[] allZombieAbilities;
        
        [Header("Effects")]
        public ProjectileEffectData[] allProjectileEffects;
        
        // Cached lookups for performance
        private Dictionary<string, PlantData> plantLookup;
        private Dictionary<string, ZombieData> zombieLookup;
        private Dictionary<string, ProjectileData> projectileLookup;
        private Dictionary<string, PlantAbilityData> plantAbilityLookup;
        private Dictionary<string, ZombieAbilityData> zombieAbilityLookup;
        private Dictionary<string, ProjectileEffectData> effectLookup;
        
        private bool isInitialized = false;
        
        #region Initialization
        
        public void Initialize()
        {
            if (isInitialized) return;
            
            BuildLookupDictionaries();
            ValidateData();
            isInitialized = true;
            
            Debug.Log("GameDataManager initialized successfully!");
        }
        
        private void BuildLookupDictionaries()
        {
            // Build plant lookup
            plantLookup = new Dictionary<string, PlantData>();
            if (allPlants != null)
            {
                foreach (var plant in allPlants)
                {
                    if (plant != null && !string.IsNullOrEmpty(plant.plantID))
                    {
                        plantLookup[plant.plantID] = plant;
                    }
                }
            }
            
            // Build zombie lookup
            zombieLookup = new Dictionary<string, ZombieData>();
            if (allZombies != null)
            {
                foreach (var zombie in allZombies)
                {
                    if (zombie != null && !string.IsNullOrEmpty(zombie.zombieID))
                    {
                        zombieLookup[zombie.zombieID] = zombie;
                    }
                }
            }
            
            // Build projectile lookup
            projectileLookup = new Dictionary<string, ProjectileData>();
            if (allProjectiles != null)
            {
                foreach (var projectile in allProjectiles)
                {
                    if (projectile != null && !string.IsNullOrEmpty(projectile.projectileID))
                    {
                        projectileLookup[projectile.projectileID] = projectile;
                    }
                }
            }
            
            // Build ability lookups
            plantAbilityLookup = new Dictionary<string, PlantAbilityData>();
            if (allPlantAbilities != null)
            {
                foreach (var ability in allPlantAbilities)
                {
                    if (ability != null && !string.IsNullOrEmpty(ability.abilityID))
                    {
                        plantAbilityLookup[ability.abilityID] = ability;
                    }
                }
            }
            
            zombieAbilityLookup = new Dictionary<string, ZombieAbilityData>();
            if (allZombieAbilities != null)
            {
                foreach (var ability in allZombieAbilities)
                {
                    if (ability != null && !string.IsNullOrEmpty(ability.abilityID))
                    {
                        zombieAbilityLookup[ability.abilityID] = ability;
                    }
                }
            }
            
            // Build effect lookup
            effectLookup = new Dictionary<string, ProjectileEffectData>();
            if (allProjectileEffects != null)
            {
                foreach (var effect in allProjectileEffects)
                {
                    if (effect != null && !string.IsNullOrEmpty(effect.effectID))
                    {
                        effectLookup[effect.effectID] = effect;
                    }
                }
            }
        }
        
        private void ValidateData()
        {
            ValidatePlants();
            ValidateZombies();
            ValidateProjectiles();
        }
        
        private void ValidatePlants()
        {
            foreach (var plant in allPlants)
            {
                if (plant == null) continue;
                
                if (string.IsNullOrEmpty(plant.plantID))
                    Debug.LogWarning($"Plant {plant.name} has empty plantID!");
                
                if (plant.prefab == null)
                    Debug.LogWarning($"Plant {plant.plantID} has no prefab assigned!");
                
                if (plant.projectileData != null && !projectileLookup.ContainsKey(plant.projectileData.projectileID))
                    Debug.LogWarning($"Plant {plant.plantID} references unknown projectile {plant.projectileData.projectileID}!");
            }
        }
        
        private void ValidateZombies()
        {
            foreach (var zombie in allZombies)
            {
                if (zombie == null) continue;
                
                if (string.IsNullOrEmpty(zombie.zombieID))
                    Debug.LogWarning($"Zombie {zombie.name} has empty zombieID!");
                
                if (zombie.prefab == null)
                    Debug.LogWarning($"Zombie {zombie.zombieID} has no prefab assigned!");
            }
        }
        
        private void ValidateProjectiles()
        {
            foreach (var projectile in allProjectiles)
            {
                if (projectile == null) continue;
                
                if (string.IsNullOrEmpty(projectile.projectileID))
                    Debug.LogWarning($"Projectile {projectile.name} has empty projectileID!");
                
                if (projectile.prefab == null)
                    Debug.LogWarning($"Projectile {projectile.projectileID} has no prefab assigned!");
            }
        }
        
        #endregion
        
        #region Data Retrieval
        
        public PlantData GetPlant(string id)
        {
            if (!isInitialized) Initialize();
            
            plantLookup.TryGetValue(id, out PlantData plant);
            if (plant == null)
                Debug.LogWarning($"Plant with ID '{id}' not found!");
            
            return plant;
        }
        
        public ZombieData GetZombie(string id)
        {
            if (!isInitialized) Initialize();
            
            zombieLookup.TryGetValue(id, out ZombieData zombie);
            if (zombie == null)
                Debug.LogWarning($"Zombie with ID '{id}' not found!");
            
            return zombie;
        }
        
        public ProjectileData GetProjectile(string id)
        {
            if (!isInitialized) Initialize();
            
            projectileLookup.TryGetValue(id, out ProjectileData projectile);
            if (projectile == null)
                Debug.LogWarning($"Projectile with ID '{id}' not found!");
            
            return projectile;
        }
        
        public PlantAbilityData GetPlantAbility(string id)
        {
            if (!isInitialized) Initialize();
            
            plantAbilityLookup.TryGetValue(id, out PlantAbilityData ability);
            return ability;
        }
        
        public ZombieAbilityData GetZombieAbility(string id)
        {
            if (!isInitialized) Initialize();
            
            zombieAbilityLookup.TryGetValue(id, out ZombieAbilityData ability);
            return ability;
        }
        
        public ProjectileEffectData GetProjectileEffect(string id)
        {
            if (!isInitialized) Initialize();
            
            effectLookup.TryGetValue(id, out ProjectileEffectData effect);
            return effect;
        }
        
        #endregion
        
        #region Filtered Queries
        
        public PlantData[] GetPlantsByType(PvZ.Core.PlantType plantType)
        {
            if (!isInitialized) Initialize();
            
            return allPlants.Where(p => p != null && p.plantType == plantType).ToArray();
        }
        
        public ZombieData[] GetZombiesByType(PvZ.Core.ZombieType zombieType)
        {
            if (!isInitialized) Initialize();
            
            return allZombies.Where(z => z != null && z.zombieType == zombieType).ToArray();
        }
        
        public PlantData[] GetUnlockedPlants()
        {
            if (!isInitialized) Initialize();
            
            return allPlants.Where(p => p != null && p.isUnlocked).ToArray();
        }
        
        public ZombieData[] GetZombiesForWave(int waveNumber)
        {
            if (!isInitialized) Initialize();
            
            return allZombies.Where(z => z != null && z.minWaveToAppear <= waveNumber).ToArray();
        }
        
        public PlantData[] GetPlantsWithCost(int maxCost)
        {
            if (!isInitialized) Initialize();
            
            return allPlants.Where(p => p != null && p.cost <= maxCost && p.isUnlocked).ToArray();
        }
        
        #endregion
        
        #region Statistics
        
        public int GetTotalPlantsCount() => allPlants?.Length ?? 0;
        public int GetTotalZombiesCount() => allZombies?.Length ?? 0;
        public int GetTotalProjectilesCount() => allProjectiles?.Length ?? 0;
        
        public int GetUnlockedPlantsCount()
        {
            if (!isInitialized) Initialize();
            return allPlants?.Count(p => p != null && p.isUnlocked) ?? 0;
        }
        
        public float GetAveragePlantCost()
        {
            if (!isInitialized) Initialize();
            
            var validPlants = allPlants?.Where(p => p != null && p.isUnlocked);
            return validPlants?.Any() == true ? validPlants.Average(p => p.cost) : 0f;
        }
        
        #endregion
        
        #region Runtime Data Management
        
        public void RefreshData()
        {
            isInitialized = false;
            Initialize();
        }
        
        public bool HasPlant(string id)
        {
            if (!isInitialized) Initialize();
            return plantLookup.ContainsKey(id);
        }
        
        public bool HasZombie(string id)
        {
            if (!isInitialized) Initialize();
            return zombieLookup.ContainsKey(id);
        }
        
        public bool HasProjectile(string id)
        {
            if (!isInitialized) Initialize();
            return projectileLookup.ContainsKey(id);
        }
        
        #endregion
        
        #region Editor Support
        
#if UNITY_EDITOR
        [ContextMenu("Rebuild Lookups")]
        private void RebuildLookups()
        {
            isInitialized = false;
            Initialize();
            Debug.Log("Lookups rebuilt!");
        }
        
        [ContextMenu("Validate All Data")]
        private void ValidateAllData()
        {
            ValidateData();
            Debug.Log("Data validation complete!");
        }
        
        [ContextMenu("Print Statistics")]
        private void PrintStatistics()
        {
            Debug.Log($"Plants: {GetTotalPlantsCount()} total, {GetUnlockedPlantsCount()} unlocked");
            Debug.Log($"Zombies: {GetTotalZombiesCount()}");
            Debug.Log($"Projectiles: {GetTotalProjectilesCount()}");
            Debug.Log($"Average Plant Cost: {GetAveragePlantCost():F1}");
        }
#endif
        
        #endregion
    }
}
