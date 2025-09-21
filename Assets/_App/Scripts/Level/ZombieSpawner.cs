using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PvZ.Core;
using PvZ.Zombies;
using PvZ.Managers;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PvZ.Level
{
    public class ZombieSpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] private Transform[] spawnPoints;

        [Header("Spawn Limits")]
        [SerializeField] private int maxConcurrentZombies = 20;
        [SerializeField] private float minSpawnInterval = 1f;

        // Current state
        private LevelConfiguration currentLevel;
        private WaveData currentWave;
        private int currentWaveIndex = 0;
        private bool isSpawning = false;
        private List<GameObject> activeZombies;
        private Coroutine spawnCoroutine;
        private EntityManager entityManager;


        #region Unity Lifecycle

        private void Awake()
        {
            InitializeSpawner();
        }

        private void Start()
        {
            entityManager = EntityManager.Instance;
        }

        private void Update()
        {
            if (isSpawning)
            {
                UpdateActiveZombies();
            }
        }

        #endregion

        #region Initialization

        private void InitializeSpawner() => activeZombies = new List<GameObject>();

        #endregion

        #region Public Methods

        public void StartLevel(LevelConfiguration levelConfig)
        {
            if (levelConfig == null)
                return;

            currentLevel = levelConfig;
            currentWaveIndex = 0;

            PrewarmZombiePools();

            StartNextWave();
        }

        public void StartNextWave()
        {
            if (currentLevel == null || currentWaveIndex >= currentLevel.waves.Length)
            {
                return;
            }

            currentWave = currentLevel.waves[currentWaveIndex];

            if (currentWave == null)
            {
                Debug.LogError($"[ZombieSpawner] Wave {currentWaveIndex} is null!");
                return;
            }


            // Start spawning after a delay
            StartCoroutine(StartWaveWithDelay());
        }

        public void StopSpawning()
        {
            isSpawning = false;

            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }

        public void ClearAllZombies()
        {
            // Return all active zombies to pool instead of destroying them
            ZombiePool.Instance?.ReturnAllActiveZombies();
            activeZombies.Clear();
        }

        #endregion

        #region Wave Management

        private IEnumerator StartWaveWithDelay()
        {
            yield return new WaitForSeconds(currentLevel.timeBetweenWaves);

            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnWave());
        }

        private IEnumerator SpawnWave()
        {
            if (currentWave.zombieSpawns == null || currentWave.zombieSpawns.Length == 0)
            {
                CompleteCurrentWave();
                yield break;
            }

            // Create spawn queue
            List<ZombieData> spawnQueue = CreateSpawnQueue();

            // Spawn zombies with intervals
            foreach (var zombieData in spawnQueue)
            {
                // Wait for spawn conditions
                yield return new WaitUntil(() => CanSpawnZombie());

                SpawnZombie(zombieData);

                // Wait for spawn interval
                float spawnInterval = Mathf.Max(minSpawnInterval, 1f / currentWave.spawnRate);
                yield return new WaitForSeconds(spawnInterval);
            }

            // Wait for all zombies to be defeated
            yield return new WaitUntil(() => activeZombies.Count == 0);

            CompleteCurrentWave();
        }

        private List<ZombieData> CreateSpawnQueue()
        {
            List<ZombieData> spawnQueue = new List<ZombieData>();

            foreach (var spawnData in currentWave.zombieSpawns)
            {
                for (int i = 0; i < spawnData.count; i++)
                {
                    spawnQueue.Add(spawnData.zombieData);
                }
            }

            // Shuffle the spawn queue for variety
            for (int i = 0; i < spawnQueue.Count; i++)
            {
                ZombieData temp = spawnQueue[i];
                int randomIndex = Random.Range(i, spawnQueue.Count);
                spawnQueue[i] = spawnQueue[randomIndex];
                spawnQueue[randomIndex] = temp;
            }

            return spawnQueue;
        }

        private bool CanSpawnZombie()
        {
            return activeZombies.Count < maxConcurrentZombies;
        }

        private Transform GetValidSpawnPoint()
        {
            // Filter out null spawn points
            var validSpawnPoints = new List<Transform>();
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    validSpawnPoints.Add(spawnPoint);
                }
            }

            if (validSpawnPoints.Count == 0)
            {
                return null;
            }

            // Return random valid spawn point
            return validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
        }

        private void SpawnZombie(ZombieData zombieData)
        {
            if (zombieData == null)
            {
                Debug.LogError("[ZombieSpawner] Zombie data is null!");
                return;
            }

            // Check if spawn points are available
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[ZombieSpawner] No spawn points available! Cannot spawn zombie.");
                return;
            }

            // Get zombie from pool
            ZombieController zombieController = ZombiePool.Instance?.GetZombie(zombieData);
            if (zombieController == null)
            {
                Debug.LogError($"[ZombieSpawner] Failed to get zombie {zombieData.zombieID} from pool!");
                return;
            }

            // Choose random spawn point (row)
            Transform spawnPoint = GetValidSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogError("[ZombieSpawner] No valid spawn point available!");
                ZombiePool.Instance?.ReturnZombie(zombieController);
                return;
            }

            // Position zombie at spawn point
            zombieController.transform.position = spawnPoint.position;
            zombieController.transform.rotation = spawnPoint.rotation;

            // Add to active zombies list
            activeZombies.Add(zombieController.gameObject);

            // Register with entity manager
            entityManager?.RegisterEntity(zombieController);
        }


        private void OnZombieDied(GameObject zombie)
        {
            if (activeZombies.Contains(zombie))
            {
                activeZombies.Remove(zombie);
            }
        }

        private void CompleteCurrentWave()
        {
            isSpawning = false;


            currentWaveIndex++;

            // Start next wave or complete level
            if (currentWaveIndex < currentLevel.waves.Length)
            {
                StartCoroutine(DelayedNextWave());
            }
            else
            {
                // All waves completed - LevelManager will detect this via GetCurrentWaveIndex
            }
        }

        private IEnumerator DelayedNextWave()
        {
            yield return new WaitForSeconds(currentLevel.timeBetweenWaves);
            StartNextWave();
        }

        #endregion

        #region Pool Management

        private void PrewarmZombiePools()
        {
            if (currentLevel?.waves == null || ZombiePool.Instance == null) return;

            // Collect all unique zombie types from all waves
            HashSet<ZombieData> uniqueZombieTypes = new HashSet<ZombieData>();

            foreach (var wave in currentLevel.waves)
            {
                if (wave?.zombieSpawns == null) continue;

                foreach (var spawnData in wave.zombieSpawns)
                {
                    if (spawnData?.zombieData != null)
                    {
                        uniqueZombieTypes.Add(spawnData.zombieData);
                    }
                }
            }

            // Prewarm pools for each zombie type
            foreach (var zombieData in uniqueZombieTypes)
            {
                int prewarmCount = CalculatePrewarmCount(zombieData);
                ZombiePool.Instance.PrewarmPool(zombieData, prewarmCount);

            }
        }

        private int CalculatePrewarmCount(ZombieData zombieData)
        {
            int totalSpawns = 0;

            // Count total spawns of this zombie type across all waves
            foreach (var wave in currentLevel.waves)
            {
                if (wave?.zombieSpawns == null) continue;

                foreach (var spawnData in wave.zombieSpawns)
                {
                    if (spawnData?.zombieData == zombieData)
                    {
                        totalSpawns += spawnData.count;
                    }
                }
            }

            // Prewarm with 25% of total spawns, minimum 2, maximum 10
            return Mathf.Clamp(Mathf.CeilToInt(totalSpawns * 0.25f), 2, 10);
        }

        #endregion

        #region Update Methods

        private void UpdateActiveZombies()
        {
            // Remove null zombies from list
            activeZombies.RemoveAll(zombie => zombie == null);
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            if (spawnPoints == null || spawnPoints.Length == 0) return;

            // Draw assigned spawn points
            Gizmos.color = Color.green; // Green for assigned spawn points
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.left * 2f);

#if UNITY_EDITOR
                    // Draw spawn point label
                    Handles.Label(spawnPoint.position + Vector3.up * 0.5f, spawnPoint.name);
#endif
                }
            }

            // Draw warning for null spawn points
            Gizmos.color = Color.red;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] == null)
                {
                    Vector3 warningPos = transform.position + Vector3.right * (i * 2f);
                    Gizmos.DrawWireCube(warningPos, Vector3.one * 0.5f);
#if UNITY_EDITOR
                    Handles.Label(warningPos + Vector3.up, $"NULL {i}");
#endif
                }
            }
        }

        #endregion

        #region Public Getters

        public int GetCurrentWaveIndex() => currentWaveIndex;
        public int GetTotalWaves() => currentLevel?.waves?.Length ?? 0;
        public int GetActiveZombieCount() => activeZombies.Count;
        public bool IsSpawning() => isSpawning;
        public WaveData GetCurrentWave() => currentWave;
        public bool IsWaveCompleted() => !isSpawning && activeZombies.Count == 0;
        public bool AreAllWavesCompleted() => currentWaveIndex >= GetTotalWaves();

        #endregion
    }
}
