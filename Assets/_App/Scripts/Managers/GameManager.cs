using UnityEngine;
using PvZ.Level;
using PvZ.Events;
using PvZ.Factory;
using PvZ.Plants;
using PvZ.Zombies;
using PvZ.Projectiles;

namespace PvZ.Managers
{
    /// <summary>
    /// Main game manager that coordinates all systems
    /// Referenced by other systems for centralized control
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Configuration")]
        [SerializeField] private GameDataManager gameDataManager;
        [SerializeField] private LevelConfiguration currentLevel;
        
        [Header("Managers")]
        [SerializeField] private EntityManager entityManager;
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private GameEventManager eventManager;
        
        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private bool isPaused = false;
        
        // Game Statistics
        public int CurrentWave { get; private set; } = 0;
        public int ZombiesKilled { get; private set; } = 0;
        public int PlantsPlanted { get; private set; } = 0;
        public float GameTime { get; private set; } = 0f;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGameManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            if (!isPaused && currentState == GameState.Playing)
            {
                GameTime += Time.deltaTime;
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeGameManager()
        {
            // Initialize data manager
            if (gameDataManager != null)
            {
                gameDataManager.Initialize();
            }
            
            // Initialize other managers if not found
            if (entityManager == null)
                entityManager = FindAnyObjectByType<EntityManager>();
            
            if (projectilePool == null)
                projectilePool = FindAnyObjectByType<ProjectilePool>();
            
            if (eventManager == null)
                eventManager = FindAnyObjectByType<GameEventManager>();
            
            Debug.Log("GameManager initialized successfully!");
        }
        
        #endregion
        
        #region Game State Management
        
        public void StartGame()
        {
            if (currentLevel == null)
            {
                Debug.LogError("Cannot start game: No level configuration assigned!");
                return;
            }
            
            ChangeGameState(GameState.Playing);
            ResetGameStatistics();
            
            eventManager?.RaiseGameStart();
        }
        
        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            
            isPaused = true;
            Time.timeScale = 0f;
            
            eventManager?.RaiseGamePause();
            
            Debug.Log("Game paused");
        }
        
        public void ResumeGame()
        {
            if (currentState != GameState.Playing) return;
            
            isPaused = false;
            Time.timeScale = 1f;
            
            eventManager?.RaiseGameResume();
            
            Debug.Log("Game resumed");
        }
        
        public void EndGame(bool victory)
        {
            ChangeGameState(victory ? GameState.Victory : GameState.GameOver);
            
            if (victory)
            {
                eventManager?.RaiseLevelComplete();
                Debug.Log("Level completed successfully!");
            }
            else
            {
                eventManager?.RaiseGameOver();
                Debug.Log("Game over!");
            }
            
            // Stop time
            Time.timeScale = 0f;
        }
        
        public void RestartGame()
        {
            // Reset time scale
            Time.timeScale = 1f;
            
            // Clear all entities
            entityManager?.ClearAllEntities();
            
            // Reset statistics
            ResetGameStatistics();
            
            // Restart the current level
            StartGame();
        }
        
        private void ChangeGameState(GameState newState)
        {
            var previousState = currentState;
            currentState = newState;
            
            Debug.Log($"Game state changed from {previousState} to {newState}");
        }
        
        #endregion
        
        #region Level Management
        
        public void LoadLevel(LevelConfiguration levelConfig)
        {
            if (levelConfig == null)
            {
                Debug.LogError("Cannot load level: LevelConfiguration is null!");
                return;
            }
            
            // Basic validation
            if (levelConfig.waves == null || levelConfig.waves.Length == 0)
            {
                Debug.LogError($"Cannot load level: No waves configured for {levelConfig.stage}");
                return;
            }
            
            currentLevel = levelConfig;
            Debug.Log($"Level loaded: {levelConfig.stage}");
        }
        
        public void NextWave()
        {
            if (currentLevel == null) return;
            
            CurrentWave++;
            
            var waveData = currentLevel.GetWave(CurrentWave - 1);
            if (waveData != null)
            {
                eventManager?.RaiseWaveStart();
                Debug.Log($"Wave {CurrentWave} started (Wave #{waveData.waveNumber})");
            }
            else
            {
                // No more waves - victory!
                EndGame(true);
            }
        }
        
        public void CompleteWave()
        {
            eventManager?.RaiseWaveComplete();
            Debug.Log($"Wave {CurrentWave} completed!");
            
            // Check if this was the last wave
            if (currentLevel != null && CurrentWave >= currentLevel.waves.Length)
            {
                EndGame(true);
            }
        }
        
        #endregion
        
        #region Entity Events
        
        public void OnPlantPlanted(PlantController plant)   
        {
            PlantsPlanted++;
            Debug.Log($"Plant planted: {plant.ID}. Total: {PlantsPlanted}");
        }
        
        public void OnZombieKilled(ZombieController zombie)
        {
            ZombiesKilled++;
            Debug.Log($"Zombie killed: {zombie.ID}. Total: {ZombiesKilled}");
        }
        
        public void ZombieReachedHouse()
        {
            Debug.Log("Zombie reached the house!");
            
            // Simple failure condition - any zombie reaching house ends game
            EndGame(false);
        }
        
        #endregion
        
        #region Statistics
        
        private void ResetGameStatistics()
        {
            CurrentWave = 0;
            ZombiesKilled = 0;
            PlantsPlanted = 0;
            GameTime = 0f;
        }
        
        public GameStatistics GetGameStatistics()
        {
            return new GameStatistics
            {
                currentWave = CurrentWave,
                zombiesKilled = ZombiesKilled,
                plantsPlanted = PlantsPlanted,
                gameTime = GameTime,
                currentLevel = currentLevel?.stage.ToString() ?? "Unknown"
            };
        }
        
        #endregion
        
        #region Properties
        
        public GameState CurrentState => currentState;
        public bool IsPaused => isPaused;
        public LevelConfiguration CurrentLevel => currentLevel;
        public GameDataManager GameData => gameDataManager;
        
        #endregion
        
        #region Debug
        
#if UNITY_EDITOR
        [ContextMenu("Start Debug Game")]
        private void DebugStartGame()
        {
            if (currentLevel != null)
            {
                StartGame();
            }
            else
            {
                Debug.LogWarning("No level configuration assigned for debug start!");
            }
        }
        
        [ContextMenu("End Game - Victory")]
        private void DebugEndGameVictory()
        {
            EndGame(true);
        }
        
        [ContextMenu("End Game - Defeat")]
        private void DebugEndGameDefeat()
        {
            EndGame(false);
        }
        
        [ContextMenu("Print Statistics")]
        private void DebugPrintStatistics()
        {
            var stats = GetGameStatistics();
            Debug.Log($"Game Statistics:\n" +
                     $"Level: {stats.currentLevel}\n" +
                     $"Wave: {stats.currentWave}\n" +
                     $"Zombies Killed: {stats.zombiesKilled}\n" +
                     $"Plants Planted: {stats.plantsPlanted}\n" +
                     $"Game Time: {stats.gameTime:F1}s");
        }
#endif
        
        #endregion
    }
    
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        Victory,
        GameOver
    }
    
    [System.Serializable]
    public class GameStatistics
    {
        public string currentLevel;
        public int currentWave;
        public int zombiesKilled;
        public int plantsPlanted;
        public float gameTime;
    }
}
