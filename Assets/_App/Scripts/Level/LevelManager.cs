using UnityEngine;
using PvZ.Core;
using PvZ.Managers;

namespace PvZ.Level
{
    /// <summary>
    /// Manages level progression, wave events, and coordination between spawner and game systems.
    /// 
    /// Usage Example:
    /// 
    /// // Subscribe to events
    /// levelManager.OnWaveStarted += (waveNumber) => Debug.Log($"Wave {waveNumber} started!");
    /// levelManager.OnWaveCompleted += (waveNumber) => Debug.Log($"Wave {waveNumber} completed!");
    /// levelManager.OnAllWavesCompleted += () => Debug.Log("All waves completed!");
    /// levelManager.OnLevelStarted += (level) => Debug.Log($"Level {level.levelName} started!");
    /// levelManager.OnLevelCompleted += (level) => Debug.Log($"Level {level.levelName} completed!");
    /// 
    /// // Start a level
    /// levelManager.StartLevel(myLevelConfig);
    /// 
    /// // Get current progress
    /// float progress = levelManager.GetWaveProgress(); // 0.0 to 1.0
    /// int currentWave = levelManager.GetCurrentWave();
    /// int totalWaves = levelManager.GetTotalWaves();
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Configuration")]
        [SerializeField] private LevelConfiguration currentLevel;
        
        [Header("Components")]
        [SerializeField] private ZombieSpawner zombieSpawner;
        
        [Header("UI References")]
        [SerializeField] private GameObject waveStartUI;
        [SerializeField] private GameObject waveCompleteUI;
        [SerializeField] private GameObject levelCompleteUI;
        
        // Private Fields
        private GameManager gameManager;
        private bool levelInProgress = false;
        private int lastWaveIndex = -1;
        private bool lastWaveCompleted = false;
        
        // Public Events - External systems can subscribe to these
        public System.Action<LevelConfiguration> OnLevelStarted;
        public System.Action<LevelConfiguration> OnLevelCompleted;
        public System.Action<int> OnWaveStarted;
        public System.Action<int> OnWaveCompleted;
        public System.Action OnAllWavesCompleted;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            gameManager = GameManager.Instance;
        }
        
        private void Update()
        {
            if (levelInProgress && zombieSpawner != null)
            {
                MonitorWaveProgress();
            }
        }
        
        private void OnDestroy()
        {
            RemoveEventListeners();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            if (zombieSpawner == null)
            {
                zombieSpawner = GetComponentInChildren<ZombieSpawner>();
                
                if (zombieSpawner == null)
                {
                    GameObject spawnerObj = new GameObject("ZombieSpawner");
                    spawnerObj.transform.SetParent(transform);
                    zombieSpawner = spawnerObj.AddComponent<ZombieSpawner>();
                }
            }
        }
        
        private void MonitorWaveProgress()
        {
            int currentWaveIndex = zombieSpawner.GetCurrentWaveIndex();
            
            // Check for wave start
            if (currentWaveIndex != lastWaveIndex)
            {
                if (currentWaveIndex < zombieSpawner.GetTotalWaves())
                {
                    HandleWaveStarted(currentWaveIndex + 1);
                }
                lastWaveIndex = currentWaveIndex;
                lastWaveCompleted = false;
            }
            
            // Check for wave completion
            if (!lastWaveCompleted && zombieSpawner.IsWaveCompleted())
            {
                if (currentWaveIndex < zombieSpawner.GetTotalWaves())
                {
                    HandleWaveCompleted(currentWaveIndex + 1);
                    lastWaveCompleted = true;
                }
            }
            
            // Check for all waves completion
            if (zombieSpawner.AreAllWavesCompleted())
            {
                HandleAllWavesCompleted();
            }
        }
        
        private void RemoveEventListeners()
        {
            // No longer needed since we don't use callbacks
        }
        
        #endregion
        
        #region Public Methods
        
        public void StartLevel(LevelConfiguration levelConfig = null)
        {
            if (levelConfig != null)
            {
                currentLevel = levelConfig;
            }
            
            if (currentLevel == null)
            {
                Debug.LogError("[LevelManager] No level configuration assigned!");
                return;
            }
            
            levelInProgress = true;
            lastWaveIndex = -1;
            lastWaveCompleted = false;
            
            
            // Initialize sun resources
            if (gameManager != null)
            {
                // gameManager.SetSun(currentLevel.startingSun);
            }
            
            // Start spawning zombies
            zombieSpawner.StartLevel(currentLevel);
            
            OnLevelStarted?.Invoke(currentLevel);
        }
        
        public void StopLevel()
        {
            levelInProgress = false;
            lastWaveIndex = -1;
            lastWaveCompleted = false;
            zombieSpawner.StopSpawning();
            
            Debug.Log("[LevelManager] Level stopped");
        }
        
        public void RestartLevel()
        {
            StopLevel();
            zombieSpawner.ClearAllZombies();
            
            // Wait a frame then restart
            StartCoroutine(RestartLevelCoroutine());
        }
        
        private System.Collections.IEnumerator RestartLevelCoroutine()
        {
            yield return null;
            StartLevel();
        }
        
        public void PauseLevel()
        {
            Time.timeScale = 0f;
        }
        
        public void ResumeLevel()
        {
            Time.timeScale = 1f;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleWaveStarted(int waveNumber)
        {
            Debug.Log($"[LevelManager] Wave {waveNumber} started!");
            
            // Show wave start UI
            if (waveStartUI != null)
            {
                waveStartUI.SetActive(true);
                // Auto-hide after 2 seconds
                StartCoroutine(HideUIAfterDelay(waveStartUI, 2f));
            }
            
            // Trigger public event
            OnWaveStarted?.Invoke(waveNumber);
        }
        
        private void HandleWaveCompleted(int waveNumber)
        {
            Debug.Log($"[LevelManager] Wave {waveNumber} completed!");
            
            // Award sun and score
            var waveData = currentLevel.GetWave(waveNumber - 1);
            if (waveData != null && gameManager != null)
            {
                // gameManager.AddSun(waveData.sunReward);
                // gameManager.AddScore(waveData.scoreReward);
            }
            
            // Show wave complete UI
            if (waveCompleteUI != null)
            {
                waveCompleteUI.SetActive(true);
                StartCoroutine(HideUIAfterDelay(waveCompleteUI, 3f));
            }
            
            // Trigger public event
            OnWaveCompleted?.Invoke(waveNumber);
        }
        
        private void HandleAllWavesCompleted()
        {
            levelInProgress = false;
            
            Debug.Log($"[LevelManager] Level {currentLevel.stage} completed!");
            
            // Show level complete UI
            if (levelCompleteUI != null)
            {
                levelCompleteUI.SetActive(true);
            }
            
            // Trigger events
            OnAllWavesCompleted?.Invoke();
            OnLevelCompleted?.Invoke(currentLevel);
        }
        
        private System.Collections.IEnumerator HideUIAfterDelay(GameObject ui, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (ui != null)
            {
                ui.SetActive(false);
            }
        }
        
        #endregion
        
        #region Public Methods - Wave Management
        
        public void ForceNextWave()
        {
            if (levelInProgress && zombieSpawner != null)
            {
                zombieSpawner.ClearAllZombies();
                zombieSpawner.StartNextWave();
            }
        }
        
        public void SetWaveCompletionRewards(int waveNumber, int sunReward, int scoreReward)
        {
            var waveData = currentLevel?.GetWave(waveNumber - 1);
            if (waveData != null)
            {
                waveData.sunReward = sunReward;
                waveData.scoreReward = scoreReward;
            }
        }
        
        public WaveData GetCurrentWaveData()
        {
            if (zombieSpawner != null)
            {
                return zombieSpawner.GetCurrentWave();
            }
            return null;
        }
        
        public float GetWaveProgress()
        {
            if (!levelInProgress || zombieSpawner == null) return 0f;
            
            int currentWave = zombieSpawner.GetCurrentWaveIndex();
            int totalWaves = zombieSpawner.GetTotalWaves();
            
            if (totalWaves == 0) return 0f;
            
            return (float)currentWave / totalWaves;
        }
        
        #endregion
        
        #region Public Getters
        
        public LevelConfiguration GetCurrentLevel() => currentLevel;
        public bool IsLevelInProgress() => levelInProgress;
        public int GetCurrentWave() => zombieSpawner?.GetCurrentWaveIndex() + 1 ?? 0;
        public int GetTotalWaves() => zombieSpawner?.GetTotalWaves() ?? 0;
        public int GetActiveZombieCount() => zombieSpawner?.GetActiveZombieCount() ?? 0;
        public ZombieSpawner GetZombieSpawner() => zombieSpawner;
        public bool IsSpawning() => zombieSpawner?.IsSpawning() ?? false;
        
        #endregion
        
        #region Debug
        
        [ContextMenu("Start Test Level")]
        private void StartTestLevel()
        {
            if (currentLevel != null)
            {
                StartLevel();
            }
            else
            {
                Debug.LogWarning("[LevelManager] No test level assigned!");
            }
        }
        
        [ContextMenu("Stop Level")]
        private void StopTestLevel()
        {
            StopLevel();
        }
        
        #endregion
    }
}
