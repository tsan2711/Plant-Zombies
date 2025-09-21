using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PvZ.Plants;
using PvZ.Zombies;
using PvZ.Projectiles;
using PvZ.Core;

namespace PvZ.Managers
{
    [CreateAssetMenu(fileName = "Game Data Manager", menuName = "PvZ/Managers/Game Data")]
    public class GameDataManager : ScriptableObject
    {
        // Singleton instance
        private static GameDataManager _instance;
        public static GameDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameDataManager>("GameDataManager");
                    if (_instance == null)
                    {
                        Debug.LogError("GameDataManager not found in Resources folder! Please create one or move existing one to Resources folder.");
                    }
                    else
                    {
                        _instance.Initialize();
                    }
                }
                return _instance;
            }
        }
        
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
        private Dictionary<AnimalID, PlantData> plantLookup;
        private Dictionary<ZombieID, ZombieData> zombieLookup;
        private Dictionary<ProjectileID, ProjectileData> projectileLookup;
        private Dictionary<string, PlantAbilityData> plantAbilityLookup;
        private Dictionary<string, ZombieAbilityData> zombieAbilityLookup;
        private Dictionary<EffectID, ProjectileEffectData> effectLookup;
        
        private bool isInitialized = false;
        
        #region Singleton Management
        
        /// <summary>
        /// Set the singleton instance manually (useful for testing or custom setup)
        /// </summary>
        public static void SetInstance(GameDataManager instance)
        {
            _instance = instance;
            if (_instance != null)
            {
                _instance.Initialize();
            }
        }
        
        /// <summary>
        /// Reset the singleton instance (useful for testing or when reloading)
        /// </summary>
        public static void ResetInstance()
        {
            _instance = null;
        }
        
        #endregion
        
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
            plantLookup = new Dictionary<AnimalID, PlantData>();
            if (allPlants != null)
            {
                foreach (var plant in allPlants)
                {
                    if (plant != null && plant.plantID != AnimalID.None)
                    {
                        plantLookup[plant.plantID] = plant;
                    }
                }
            }
            
            // Build zombie lookup
            zombieLookup = new Dictionary<ZombieID, ZombieData>();
            if (allZombies != null)
            {
                foreach (var zombie in allZombies)
                {
                    if (zombie != null && zombie.zombieID != ZombieID.None)
                    {
                        zombieLookup[zombie.zombieID] = zombie;
                    }
                }
            }
            
            // Build projectile lookup
            projectileLookup = new Dictionary<ProjectileID, ProjectileData>();
            if (allProjectiles != null)
            {
                foreach (var projectile in allProjectiles)
                {
                    if (projectile != null && projectile.projectileID != ProjectileID.None)
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
            effectLookup = new Dictionary<EffectID, ProjectileEffectData>();
            if (allProjectileEffects != null)
            {
                foreach (var effect in allProjectileEffects)
                {
                    if (effect != null && effect.effectID != EffectID.None)
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
                
                if (plant.plantID == AnimalID.None)
                    Debug.LogWarning($"Plant {plant.name} has no plantID set!");
                
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
                
                if (zombie.zombieID == ZombieID.None)
                    Debug.LogWarning($"Zombie {zombie.name} has no zombieID set!");
                
                if (zombie.prefab == null)
                    Debug.LogWarning($"Zombie {zombie.zombieID} has no prefab assigned!");
            }
        }
        
        private void ValidateProjectiles()
        {
            foreach (var projectile in allProjectiles)
            {
                if (projectile == null) continue;
                
                if (projectile.projectileID == ProjectileID.None)
                    Debug.LogWarning($"Projectile {projectile.name} has no projectileID set!");
                
                if (projectile.prefab == null)
                    Debug.LogWarning($"Projectile {projectile.projectileID} has no prefab assigned!");
            }
        }
        
        #endregion
        
        #region Data Retrieval
        
        public PlantData GetPlant(AnimalID id)
        {
            if (!isInitialized) Initialize();
            
            plantLookup.TryGetValue(id, out PlantData plant);
            if (plant == null)
                Debug.LogWarning($"Plant with ID '{id}' not found!");
            
            return plant;
        }
        
        public ZombieData GetZombie(ZombieID id)
        {
            if (!isInitialized) Initialize();
            
            zombieLookup.TryGetValue(id, out ZombieData zombie);
            if (zombie == null)
                Debug.LogWarning($"Zombie with ID '{id}' not found!");
            
            return zombie;
        }
        
        public ProjectileData GetProjectile(ProjectileID id)
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
        
        public ProjectileEffectData GetProjectileEffect(EffectID id)
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
            return validPlants?.Any() == true ? (float)validPlants.Average(p => p.cost) : 0f;
        }
        
        #endregion
        
        #region Runtime Data Management
        
        public void RefreshData()
        {
            isInitialized = false;
            Initialize();
        }
        
        public bool HasPlant(AnimalID id)
        {
            if (!isInitialized) Initialize();
            return plantLookup.ContainsKey(id);
        }
        
        public bool HasZombie(ZombieID id)
        {
            if (!isInitialized) Initialize();
            return zombieLookup.ContainsKey(id);
        }
        
        public bool HasProjectile(ProjectileID id)
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
