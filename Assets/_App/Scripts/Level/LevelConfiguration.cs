using UnityEngine;
using PvZ.Plants;
using PvZ.Core;

namespace PvZ.Level
{
    [CreateAssetMenu(fileName = "New Level Config", menuName = "PvZ/Level/Level Configuration")]
    public class LevelConfiguration : ScriptableObject
    {
        [Header("Level Info")]
        public int stage;

        [Header("Wave Settings")]
        public WaveData[] waves;
        public float timeBetweenWaves = 15f;


        [Header("Resources")]
        public int startingSun = 50;

        public WaveData GetWave(int waveIndex)
        {
            if (waves == null || waves.Length == 0 || waveIndex >= waves.Length)
                return null;

            return waves[waveIndex];
        }

    }
}