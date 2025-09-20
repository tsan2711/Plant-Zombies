using UnityEngine;
using PvZ.Zombies;

namespace PvZ.Level
{
    [CreateAssetMenu(fileName = "New Wave", menuName = "PvZ/Level/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Header("Wave Info")]
        public int waveNumber;
        public string waveName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Timing")]
        public float startDelay = 5f;
        public float duration = 60f;
        public bool isEndlessWave = false;
        
        [Header("Zombie Spawning")]
        public ZombieSpawnData[] zombieSpawns;
        public AnimationCurve spawnRateCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public float baseSpawnRate = 1f;
        public int maxConcurrentZombies = 10;
        
        [Header("Special Events")]
        public WaveEventData[] waveEvents;
        
        [Header("Completion")]
        public WaveCompletionCondition completionCondition;
        public int requiredKills = 0;
        public float survivalTime = 0f;
        
        [Header("Rewards")]
        public int sunReward = 50;
        public int scoreReward = 100;
        public string[] unlockedPlants;
        
        public int GetTotalZombieCount()
        {
            int total = 0;
            foreach (var spawn in zombieSpawns)
            {
                total += spawn.count;
            }
            return total;
        }
        
        public float GetSpawnRateAtTime(float normalizedTime)
        {
            return baseSpawnRate * spawnRateCurve.Evaluate(normalizedTime);
        }
    }
    
    [System.Serializable]
    public class ZombieSpawnData
    {
        public ZombieData zombieData;
        public int count;
        public float weight = 1f;
        public float firstSpawnTime = 0f;
        public float lastSpawnTime = 1f;
        public int[] allowedLanes; // Empty means all lanes
        public bool isSpecialSpawn = false;
    }
    
    [System.Serializable]
    public class WaveEventData
    {
        public string eventID;
        public float triggerTime; // Normalized time (0-1)
        public WaveEventType eventType;
        public string parameters;
    }
    
    public enum WaveCompletionCondition
    {
        KillAllZombies,
        KillSpecificCount,
        SurviveTime,
        DefeatBoss
    }
    
    public enum WaveEventType
    {
        SpawnSpecialZombie,
        ChangeWeather,
        ActivatePowerUp,
        PlayDialogue,
        ChangeMusic,
        SpawnObstacle
    }
}
