using UnityEngine;
using PvZ.Zombies;

namespace PvZ.Level
{
    [CreateAssetMenu(fileName = "New Wave", menuName = "PvZ/Level/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Header("Wave Info")]
        public int waveNumber;
        
        [Header("Wave Timing")]
        public float startDelay = 5f;
        public float duration = 60f;
        
        [Header("Zombie Spawning")]
        public ZombieSpawnData[] zombieSpawns;
        public float spawnRate = 1f;
        
        [Header("Wave Completion")]
        public WaveCompletionCondition completionCondition = WaveCompletionCondition.DefeatAllZombies;
        
        [Header("Rewards")]
        public int sunReward = 50;
        public int scoreReward = 100;
    }
    
    [System.Serializable]
    public class ZombieSpawnData
    {
        public ZombieData zombieData;
        public int count;
        [Range(0f, 1f)]
        public float spawnProbability = 1f;
    }
    
    public enum WaveCompletionCondition
    {
        DefeatAllZombies,
        SurviveForTime,
        DefeatBossZombie
    }
}
