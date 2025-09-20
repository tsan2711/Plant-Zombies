using UnityEngine;
using PvZ.Plants;
using PvZ.Core;

namespace PvZ.Level
{
    [CreateAssetMenu(fileName = "New Level Config", menuName = "PvZ/Level/Level Configuration")]
    public class LevelConfiguration : ScriptableObject
    {
        [Header("Level Info")]
        public string levelID;
        public string levelName;
        [TextArea(3, 5)]
        public string description;
        public Sprite levelIcon;
        public int levelIndex;
        
        [Header("Environment")]
        public LevelEnvironment environment;
        public string sceneName;
        public GameObject levelPrefab;
        public Color ambientColor = Color.white;
        public AudioClip backgroundMusic;
        
        [Header("Wave Settings")]
        public WaveData[] waves;
        public float timeBetweenWaves = 15f;
        public bool hasInfiniteWaves = false;
        public WaveData infiniteWaveTemplate;
        
        [Header("Available Plants")]
        public PlantData[] availablePlants;
        public PlantData[] startingPlants;
        public int maxSelectedPlants = 6;
        
        [Header("Resources")]
        public int startingSun = 50;
        public int maxSun = 9990;
        public float sunGenerationRate = 1f;
        public float naturalSunSpawnRate = 25f; // Seconds between natural sun drops
        
        [Header("Level Modifiers")]
        public LevelModifier[] modifiers;
        
        [Header("Victory Conditions")]
        public LevelVictoryCondition victoryCondition;
        public int requiredWaves = 0;
        public float survivalTime = 0f;
        public int targetScore = 0;
        
        [Header("Failure Conditions")]
        public LevelFailureCondition failureCondition;
        public int maxZombiesInHouse = 5;
        public float timeLimit = 0f;
        
        [Header("Difficulty")]
        public LevelDifficulty difficulty;
        [Range(0.5f, 3f)]
        public float difficultyMultiplier = 1f;
        
        [Header("Rewards")]
        public int baseScoreReward = 1000;
        public int sunReward = 100;
        public string[] unlockedContent;
        
        #region Utility Methods
        
        public int GetTotalWaves()
        {
            return hasInfiniteWaves ? int.MaxValue : waves.Length;
        }
        
        public WaveData GetWave(int waveIndex)
        {
            if (waves == null || waves.Length == 0)
                return null;
            
            if (waveIndex < waves.Length)
                return waves[waveIndex];
            
            // Return infinite wave template if available
            return hasInfiniteWaves ? infiniteWaveTemplate : null;
        }
        
        public bool IsPlantAvailable(string plantID)
        {
            if (availablePlants == null) return false;
            
            foreach (var plant in availablePlants)
            {
                if (plant != null && plant.plantID == plantID)
                    return true;
            }
            return false;
        }
        
        public PlantData[] GetStartingPlants()
        {
            return startingPlants ?? new PlantData[0];
        }
        
        public float GetDifficultyModifiedValue(float baseValue)
        {
            return baseValue * difficultyMultiplier;
        }
        
        public bool HasModifier(string modifierID)
        {
            if (modifiers == null) return false;
            
            foreach (var modifier in modifiers)
            {
                if (modifier.modifierID == modifierID)
                    return true;
            }
            return false;
        }
        
        #endregion
        
        #region Validation
        
        public bool ValidateConfiguration()
        {
            bool isValid = true;
            
            // Check basic info
            if (string.IsNullOrEmpty(levelID))
            {
                Debug.LogError($"Level {name} has no levelID!");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"Level {levelID} has no sceneName!");
                isValid = false;
            }
            
            // Check waves
            if (waves == null || waves.Length == 0)
            {
                Debug.LogError($"Level {levelID} has no waves!");
                isValid = false;
            }
            
            // Check plants
            if (availablePlants == null || availablePlants.Length == 0)
            {
                Debug.LogWarning($"Level {levelID} has no available plants!");
            }
            
            return isValid;
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class LevelModifier
    {
        public string modifierID;
        public string displayName;
        [TextArea(2, 3)]
        public string description;
        public bool isActive = true;
        public float value = 1f;
        public string[] affectedSystems;
    }
    
    public enum LevelEnvironment
    {
        Day,
        Night,
        Pool,
        Fog,
        Roof,
        Desert,
        Winter,
        Space
    }
    
    public enum LevelDifficulty
    {
        Easy,
        Normal,
        Hard,
        Expert,
        Nightmare
    }
    
    public enum LevelVictoryCondition
    {
        SurviveAllWaves,
        SurviveTime,
        ReachScore,
        DefeatBoss,
        CollectItems
    }
    
    public enum LevelFailureCondition
    {
        ZombiesReachHouse,
        TimeLimit,
        HealthReachesZero,
        FailObjective
    }
}
