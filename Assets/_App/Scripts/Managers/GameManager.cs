using UnityEngine;
using PvZ.Level;
using PvZ.Events;
using PvZ.Factory;
using PvZ.Plants;
using PvZ.Zombies;
using PvZ.Projectiles;
using PvZ.Placement;
using PvZ.Core;

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
        [SerializeField] private PlantPlacementController plantPlacementController;
        
        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private bool isPaused = false;
        
        // Game Statistics
        public int CurrentWave { get; private set; } = 0;
        public int ZombiesKilled { get; private set; } = 0;
        public int PlantsPlanted { get; private set; } = 0;
        public float GameTime { get; private set; } = 0f;
        
        // Resource System
        [Header("Resources")]
        [SerializeField] private int startingSun = 50;
        public int CurrentSun { get; private set; } = 50;
        
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
            
            if (plantPlacementController == null)
                plantPlacementController = FindAnyObjectByType<PlantPlacementController>();
            
            Debug.Log("GameManager initialized successfully!");
        }
        
        #endregion
        
        #region Game Data Access
        
        /// <summary>
        /// Get the GameDataManager instance
        /// </summary>
        public GameDataManager GetGameData()
        {
            if (gameDataManager == null)
            {
                Debug.LogError("GameDataManager is not assigned to GameManager!");
            }
            return gameDataManager;
        }
        
        // Convenience methods for common data access
        public PlantData GetPlant(AnimalID id) => gameDataManager?.GetPlant(id);
        public ZombieData GetZombie(ZombieID id) => gameDataManager?.GetZombie(id);
        public ProjectileData GetProjectile(ProjectileID id) => gameDataManager?.GetProjectile(id);
        public PlantAbilityData GetPlantAbility(string id) => gameDataManager?.GetPlantAbility(id);
        public ZombieAbilityData GetZombieAbility(string id) => gameDataManager?.GetZombieAbility(id);
        public ProjectileEffectData GetProjectileEffect(EffectID id) => gameDataManager?.GetProjectileEffect(id);
        
        // Filtered queries
        public PlantData[] GetPlantsByType(PvZ.Core.PlantType plantType) => gameDataManager?.GetPlantsByType(plantType);
        public ZombieData[] GetZombiesByType(PvZ.Core.ZombieType zombieType) => gameDataManager?.GetZombiesByType(zombieType);
        public PlantData[] GetUnlockedPlants() => gameDataManager?.GetUnlockedPlants();
        public ZombieData[] GetZombiesForWave(int waveNumber) => gameDataManager?.GetZombiesForWave(waveNumber);
        public PlantData[] GetPlantsWithCost(int maxCost) => gameDataManager?.GetPlantsWithCost(maxCost);
        
        // Statistics
        public int GetTotalPlantsCount() => gameDataManager?.GetTotalPlantsCount() ?? 0;
        public int GetTotalZombiesCount() => gameDataManager?.GetTotalZombiesCount() ?? 0;
        public int GetTotalProjectilesCount() => gameDataManager?.GetTotalProjectilesCount() ?? 0;
        public int GetUnlockedPlantsCount() => gameDataManager?.GetUnlockedPlantsCount() ?? 0;
        public float GetAveragePlantCost() => gameDataManager?.GetAveragePlantCost() ?? 0f;
        
        // Data validation
        public bool HasPlant(AnimalID id) => gameDataManager?.HasPlant(id) ?? false;
        public bool HasZombie(ZombieID id) => gameDataManager?.HasZombie(id) ?? false;
        public bool HasProjectile(ProjectileID id) => gameDataManager?.HasProjectile(id) ?? false;
        
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
            InitializeResources();
            
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
        
        #region Resource Management
        
        private void InitializeResources()
        {
            CurrentSun = startingSun;
        }
        
        public bool CanAffordPlant(int cost)
        {
            return CurrentSun >= cost;
        }
        
        public bool SpendSun(int amount)
        {
            if (CurrentSun >= amount)
            {
                CurrentSun -= amount;
                Debug.Log($"Spent {amount} sun. Remaining: {CurrentSun}");
                return true;
            }
            
            Debug.Log($"Not enough sun to spend {amount}. Current: {CurrentSun}");
            return false;
        }
        
        public void AddSun(int amount)
        {
            CurrentSun += amount;
            Debug.Log($"Added {amount} sun. Total: {CurrentSun}");
        }
        
        #endregion
        
        #region Plant Placement
        
        public bool TryPlacePlant(PlantData plantData)
        {
            if (plantData == null)
            {
                Debug.LogError("Cannot place plant: PlantData is null");
                return false;
            }
            
            if (!CanAffordPlant(plantData.cost))
            {
                Debug.Log($"Cannot afford plant {plantData.displayName} (Cost: {plantData.cost}, Current: {CurrentSun})");
                return false;
            }
            
            // Placement sẽ được xử lý qua PlantPlacementController và GroundPlantContainer
            // Chỉ cần kiểm tra resources ở đây
            Debug.Log($"Plant {plantData.displayName} is ready to be placed. Use PlantPlacementController to place it.");
            return true;
        }
        
        public void SelectPlantForPlacement(PlantData plantData)
        {
            if (plantPlacementController != null)
            {
                plantPlacementController.SelectPlant(plantData);
            }
            else
            {
                Debug.LogError("PlantPlacementController not found!");
            }
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
        public PlantPlacementController PlacementController => plantPlacementController;
        
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
