using UnityEngine;
using TMPro;
using PvZ.Managers;

namespace PvZ.UI
{
    /// <summary>
    /// Manages sun resource display and collection
    /// </summary>
    public class SunManager : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI sunText;
        [SerializeField] private Animator sunAnimator;
        
        [Header("Sun Collection")]
        [SerializeField] private GameObject sunPickupPrefab;
        [SerializeField] private Transform sunCollectionPoint;
        [SerializeField] private float sunCollectionSpeed = 5f;
        [SerializeField] private int sunFromSunflower = 25;
        [SerializeField] private int sunFromSky = 50;
        
        [Header("Audio")]
        [SerializeField] private AudioClip sunCollectSound;
        [SerializeField] private AudioClip sunSpendSound;
        
        // Private fields
        private AudioSource audioSource;
        private int currentSun;
        
        // Singleton instance
        public static SunManager Instance { get; private set; }
        
        // Events
        public System.Action<int> OnSunChanged;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                InitializeComponents();
            }
            else
            {
                Debug.LogWarning("Multiple SunManager instances found! Destroying duplicate.");
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            InitializeSun();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            // Get audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            
            // Auto-find sun text if not assigned
            if (sunText == null)
                sunText = GetComponentInChildren<TextMeshProUGUI>();
            
            // Auto-find collection point
            if (sunCollectionPoint == null)
                sunCollectionPoint = transform;
        }
        
        private void InitializeSun()
        {
            // Get starting sun from GameManager
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                currentSun = gameManager.CurrentSun;
                Debug.Log($"SunManager initialized with {currentSun} sun from GameManager");
            }
            else
            {
                currentSun = 50; // Default starting sun
                Debug.LogWarning($"GameManager not found! Using default {currentSun} sun");
            }
            
            UpdateSunDisplay();
            Debug.Log($"SunManager initialization complete. Sun display updated.");
        }
        
        #endregion
        
        #region Sun Management
        
        public int GetCurrentSun()
        {
            return currentSun;
        }
        
        public bool CanAfford(int cost)
        {
            return currentSun >= cost;
        }
        
        public bool SpendSun(int amount)
        {
            Debug.Log($"SunManager.SpendSun called: amount={amount}, currentSun={currentSun}");
            
            if (currentSun >= amount)
            {
                currentSun -= amount;
                Debug.Log($"Sun spent successfully! New amount: {currentSun}");
                
                UpdateSunDisplay();
                PlaySpendSound();
                
                // Sync with GameManager
                var gameManager = GameManager.Instance;
                if (gameManager != null)
                {
                    gameManager.SpendSun(amount);
                    Debug.Log($"Synced with GameManager. GameManager sun: {gameManager.CurrentSun}");
                }
                else
                {
                    Debug.LogWarning("GameManager.Instance is null during SpendSun!");
                }
                
                OnSunChanged?.Invoke(currentSun);
                return true;
            }
            else
            {
                Debug.LogWarning($"Not enough sun! Need {amount}, have {currentSun}");
            }
            
            return false;
        }
        
        public void AddSun(int amount)
        {
            currentSun += amount;
            UpdateSunDisplay();
            PlayCollectSound();
            
            // Sync with GameManager
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.AddSun(amount);
            }
            
            OnSunChanged?.Invoke(currentSun);
            
            Debug.Log($"Added {amount} sun. Total: {currentSun}");
        }
        
        #endregion
        
        #region Sun Collection
        
        /// <summary>
        /// Create a sun pickup at world position that moves to UI
        /// </summary>
        public void CreateSunPickup(Vector3 worldPosition, int sunValue = 25)
        {
            if (sunPickupPrefab == null)
            {
                // If no prefab, just add sun directly
                AddSun(sunValue);
                return;
            }
            
            GameObject sunPickup = Instantiate(sunPickupPrefab, worldPosition, Quaternion.identity);
            StartCoroutine(MoveSunToUI(sunPickup, sunValue));
        }
        
        private System.Collections.IEnumerator MoveSunToUI(GameObject sunPickup, int sunValue)
        {
            if (sunCollectionPoint == null)
            {
                AddSun(sunValue);
                if (sunPickup != null)
                    Destroy(sunPickup);
                yield break;
            }
            
            Vector3 startPos = sunPickup.transform.position;
            Vector3 targetPos = Camera.main.WorldToScreenPoint(sunCollectionPoint.position);
            targetPos = Camera.main.ScreenToWorldPoint(new Vector3(targetPos.x, targetPos.y, startPos.z));
            
            float journeyTime = 0f;
            float journeyLength = Vector3.Distance(startPos, targetPos);
            float speed = sunCollectionSpeed;
            
            while (journeyTime <= journeyLength / speed)
            {
                journeyTime += Time.deltaTime;
                float fractionOfJourney = (journeyTime * speed) / journeyLength;
                
                if (sunPickup != null)
                {
                    sunPickup.transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
                }
                else
                {
                    break;
                }
                
                yield return null;
            }
            
            // Sun reached UI, collect it
            AddSun(sunValue);
            
            if (sunPickup != null)
                Destroy(sunPickup);
        }
        
        /// <summary>
        /// Generate sun from sunflower
        /// </summary>
        public void GenerateSunFromSunflower(Vector3 sunflowerPosition)
        {
            CreateSunPickup(sunflowerPosition, sunFromSunflower);
        }
        
        /// <summary>
        /// Generate sun from sky (random drops)
        /// </summary>
        public void GenerateSunFromSky(Vector3 skyPosition)
        {
            CreateSunPickup(skyPosition, sunFromSky);
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateSunDisplay()
        {
            if (sunText != null)
            {
                sunText.text = currentSun.ToString();
                Debug.Log($"Updated sun display: {currentSun}");
            }
            else
            {
                Debug.LogError("sunText is null! Cannot update display");
            }
            
            // Play animation if available
            if (sunAnimator != null)
            {
                sunAnimator.SetTrigger("SunChanged");
            }
        }
        
        #endregion
        
        #region Audio
        
        private void PlayCollectSound()
        {
            if (sunCollectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(sunCollectSound);
            }
        }
        
        private void PlaySpendSound()
        {
            if (sunSpendSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(sunSpendSound);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Set sun amount directly (for testing/cheats)
        /// </summary>
        public void SetSun(int amount)
        {
            currentSun = amount;
            UpdateSunDisplay();
            OnSunChanged?.Invoke(currentSun);
            
            // Sync with GameManager
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                // Note: GameManager doesn't have SetSun method, so we work around it
                int difference = amount - gameManager.CurrentSun;
                if (difference > 0)
                {
                    gameManager.AddSun(difference);
                }
                else if (difference < 0)
                {
                    gameManager.SpendSun(-difference);
                }
            }
        }
        
        /// <summary>
        /// Sync with GameManager sun value
        /// </summary>
        public void SyncWithGameManager()
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                currentSun = gameManager.CurrentSun;
                UpdateSunDisplay();
                OnSunChanged?.Invoke(currentSun);
            }
        }
        
        #endregion
        
        #region Debug
        
        [ContextMenu("Add 100 Sun")]
        private void DebugAdd100Sun()
        {
            AddSun(100);
        }
        
        [ContextMenu("Spend 50 Sun")]
        private void DebugSpend50Sun()
        {
            SpendSun(50);
        }
        
        [ContextMenu("Generate Sky Sun")]
        private void DebugGenerateSkySun()
        {
            Vector3 skyPos = Camera.main.transform.position + Vector3.up * 5 + Vector3.right * Random.Range(-5f, 5f);
            GenerateSunFromSky(skyPos);
        }
        
        #endregion
    }
}
