using UnityEngine;
using System.Collections.Generic;
using PvZ.Core;

namespace PvZ.Events
{
    /// <summary>
    /// Centralized manager for common game events
    /// </summary>
    public class GameEventManager : MonoBehaviour
    {
        public static GameEventManager Instance { get; private set; }
        
        [Header("Core Game Events")]
        public GameEvent onGameStart;
        public GameEvent onGamePause;
        public GameEvent onGameResume;
        public GameEvent onGameOver;
        public GameEvent onLevelComplete;
        public GameEvent onWaveStart;
        public GameEvent onWaveComplete;
        
        [Header("Entity Events")]
        public EntityGameEvent onPlantPlanted;
        public EntityGameEvent onPlantDestroyed;
        public EntityGameEvent onZombieSpawned;
        public EntityGameEvent onZombieKilled;
        public EntityGameEvent onProjectileFired;
        
        [Header("Resource Events")]
        public IntGameEvent onSunChanged;
        public IntGameEvent onScoreChanged;
        public FloatGameEvent onHealthChanged;
        
        [Header("Special Events")]
        public Vector3GameEvent onExplosion;
        public StringGameEvent onPowerUpActivated;
        public GameEvent onBossDefeated;
        
        // Event history for debugging
        private Queue<EventLogEntry> eventHistory;
        private int maxHistorySize = 100;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeEventManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeEventManager()
        {
            eventHistory = new Queue<EventLogEntry>();
            Debug.Log("GameEventManager initialized!");
        }
        
        #endregion
        
        #region Event Raising Methods
        
        public void RaiseGameStart()
        {
            onGameStart?.Raise();
            LogEvent("GameStart");
        }
        
        public void RaiseGamePause()
        {
            onGamePause?.Raise();
            LogEvent("GamePause");
        }
        
        public void RaiseGameResume()
        {
            onGameResume?.Raise();
            LogEvent("GameResume");
        }
        
        public void RaiseGameOver()
        {
            onGameOver?.Raise();
            LogEvent("GameOver");
        }
        
        public void RaiseLevelComplete()
        {
            onLevelComplete?.Raise();
            LogEvent("LevelComplete");
        }
        
        public void RaiseWaveStart()
        {
            onWaveStart?.Raise();
            LogEvent("WaveStart");
        }
        
        public void RaiseWaveComplete()
        {
            onWaveComplete?.Raise();
            LogEvent("WaveComplete");
        }
        
        public void RaisePlantPlanted(IEntity plant)
        {
            onPlantPlanted?.Raise(plant);
            LogEvent("PlantPlanted", plant?.ID);
        }
        
        public void RaisePlantDestroyed(IEntity plant)
        {
            onPlantDestroyed?.Raise(plant);
            LogEvent("PlantDestroyed", plant?.ID);
        }
        
        public void RaiseZombieSpawned(IEntity zombie)
        {
            onZombieSpawned?.Raise(zombie);
            LogEvent("ZombieSpawned", zombie?.ID);
        }
        
        public void RaiseZombieKilled(IEntity zombie)
        {
            onZombieKilled?.Raise(zombie);
            LogEvent("ZombieKilled", zombie?.ID);
        }
        
        public void RaiseProjectileFired(IEntity projectile)
        {
            onProjectileFired?.Raise(projectile);
            LogEvent("ProjectileFired", projectile?.ID);
        }
        
        public void RaiseSunChanged(int newAmount)
        {
            onSunChanged?.Raise(newAmount);
            LogEvent("SunChanged", newAmount.ToString());
        }
        
        public void RaiseScoreChanged(int newScore)
        {
            onScoreChanged?.Raise(newScore);
            LogEvent("ScoreChanged", newScore.ToString());
        }
        
        public void RaiseHealthChanged(float newHealth)
        {
            onHealthChanged?.Raise(newHealth);
            LogEvent("HealthChanged", newHealth.ToString("F1"));
        }
        
        public void RaiseExplosion(Vector3 position)
        {
            onExplosion?.Raise(position);
            LogEvent("Explosion", position.ToString());
        }
        
        public void RaisePowerUpActivated(string powerUpID)
        {
            onPowerUpActivated?.Raise(powerUpID);
            LogEvent("PowerUpActivated", powerUpID);
        }
        
        public void RaiseBossDefeated()
        {
            onBossDefeated?.Raise();
            LogEvent("BossDefeated");
        }
        
        #endregion
        
        #region Event Logging
        
        private void LogEvent(string eventName, string parameter = null)
        {
            var logEntry = new EventLogEntry
            {
                eventName = eventName,
                parameter = parameter,
                timestamp = Time.time
            };
            
            eventHistory.Enqueue(logEntry);
            
            // Maintain history size
            while (eventHistory.Count > maxHistorySize)
            {
                eventHistory.Dequeue();
            }
            
#if UNITY_EDITOR && false // Set to true for verbose logging
            Debug.Log($"Event: {eventName}" + (parameter != null ? $" ({parameter})" : ""));
#endif
        }
        
        public EventLogEntry[] GetEventHistory()
        {
            return eventHistory.ToArray();
        }
        
        public void ClearEventHistory()
        {
            eventHistory.Clear();
        }
        
        #endregion
        
        #region Batch Event Operations
        
        public void RaiseMultipleEvents(params GameEvent[] events)
        {
            foreach (var gameEvent in events)
            {
                gameEvent?.Raise();
            }
        }
        
        public void RaiseDelayedEvent(GameEvent gameEvent, float delay)
        {
            if (gameEvent != null)
            {
                StartCoroutine(RaiseEventAfterDelay(gameEvent, delay));
            }
        }
        
        private System.Collections.IEnumerator RaiseEventAfterDelay(GameEvent gameEvent, float delay)
        {
            yield return new WaitForSeconds(delay);
            gameEvent.Raise();
        }
        
        #endregion
        
        #region Event Validation
        
        public bool ValidateEvents()
        {
            bool allValid = true;
            
            // Check core events
            if (onGameStart == null) { Debug.LogWarning("onGameStart event not assigned!"); allValid = false; }
            if (onGameOver == null) { Debug.LogWarning("onGameOver event not assigned!"); allValid = false; }
            
            // Check entity events
            if (onPlantPlanted == null) { Debug.LogWarning("onPlantPlanted event not assigned!"); allValid = false; }
            if (onZombieKilled == null) { Debug.LogWarning("onZombieKilled event not assigned!"); allValid = false; }
            
            // Check resource events
            if (onSunChanged == null) { Debug.LogWarning("onSunChanged event not assigned!"); allValid = false; }
            if (onScoreChanged == null) { Debug.LogWarning("onScoreChanged event not assigned!"); allValid = false; }
            
            return allValid;
        }
        
        #endregion
        
        #region Debug
        
#if UNITY_EDITOR
        [ContextMenu("Validate All Events")]
        private void DebugValidateEvents()
        {
            bool valid = ValidateEvents();
            Debug.Log($"Event validation: {(valid ? "PASSED" : "FAILED")}");
        }
        
        [ContextMenu("Print Event History")]
        private void DebugPrintEventHistory()
        {
            var history = GetEventHistory();
            Debug.Log($"Event History ({history.Length} events):");
            
            foreach (var entry in history)
            {
                string logMessage = $"[{entry.timestamp:F2}s] {entry.eventName}";
                if (!string.IsNullOrEmpty(entry.parameter))
                {
                    logMessage += $" ({entry.parameter})";
                }
                Debug.Log(logMessage);
            }
        }
        
        [ContextMenu("Test All Events")]
        private void DebugTestAllEvents()
        {
            Debug.Log("Testing all events...");
            
            RaiseGameStart();
            RaiseSunChanged(100);
            RaiseScoreChanged(500);
            RaiseExplosion(Vector3.zero);
            RaisePowerUpActivated("TestPowerUp");
            RaiseGameOver();
        }
#endif
        
        #endregion
    }
    
    [System.Serializable]
    public class EventLogEntry
    {
        public string eventName;
        public string parameter;
        public float timestamp;
    }
}
