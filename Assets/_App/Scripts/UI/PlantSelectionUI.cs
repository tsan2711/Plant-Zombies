using UnityEngine;
using System.Collections.Generic;
using PvZ.Plants;
using PvZ.Managers;
using PvZ.Placement;

namespace PvZ.UI
{
    /// <summary>
    /// Main UI controller for plant selection and sun display
    /// </summary>
    public class PlantSelectionUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Transform plantButtonContainer;
        [SerializeField] private GameObject plantButtonPrefab;
        [SerializeField] private SunManager sunManager;
        
        [Header("Plant Selection")]
        [SerializeField] private List<PlantData> availablePlants = new List<PlantData>();
        [SerializeField] private bool autoLoadPlantsFromGameData = true;
        
        [Header("Audio")]
        [SerializeField] private AudioClip plantSelectSound;
        [SerializeField] private AudioClip cannotAffordSound;
        
        // Private fields
        private List<PlantButton> plantButtons = new List<PlantButton>();
        private PlantButton selectedPlantButton;
        private AudioSource audioSource;
        
        // Singleton instance
        public static PlantSelectionUI Instance { get; private set; }
        
        // Properties
        public PlantData SelectedPlantData => selectedPlantButton?.PlantData;
        public bool HasSelectedPlant => selectedPlantButton != null;
        
        // Events
        public System.Action<PlantData> OnPlantSelected;
        public System.Action OnPlantDeselected;
        
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
                Debug.LogWarning("Multiple PlantSelectionUI instances found! Destroying duplicate.");
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            InitializeUI();
        }
        
        private void Update()
        {
            UpdatePlantButtons();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            // Get audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            
            // Find SunManager if not assigned
            if (sunManager == null)
                sunManager = SunManager.Instance;
            
            // Find container if not assigned
            if (plantButtonContainer == null)
                plantButtonContainer = transform.Find("PlantContainer");
        }
        
        private void InitializeUI()
        {
            LoadAvailablePlants();
            CreatePlantButtons();
            SetupEventListeners();
        }
        
        #endregion
        
        #region Plant Loading
        
        private void LoadAvailablePlants()
        {
            if (autoLoadPlantsFromGameData)
            {
                var gameDataManager = GameManager.Instance?.GetGameData();
                if (gameDataManager != null)
                {
                    var unlockedPlants = gameDataManager.GetUnlockedPlants();
                    availablePlants = new List<PlantData>(unlockedPlants);
                    Debug.Log($"Loaded {availablePlants.Count} plants from GameDataManager");
                }
                else
                {
                    Debug.LogWarning("GameDataManager not found! Using manually assigned plants.");
                }
            }
            
            if (availablePlants.Count == 0)
            {
                Debug.LogWarning("No plants available for selection!");
            }
        }
        
        #endregion
        
        #region Plant Button Creation
        
        private void CreatePlantButtons()
        {
            if (plantButtonContainer == null)
            {
                Debug.LogError("Plant button container not found!");
                return;
            }
            
            if (plantButtonPrefab == null)
            {
                Debug.LogError("Plant button prefab not assigned!");
                return;
            }
            
            // Clear existing buttons
            ClearPlantButtons();
            
            // Create buttons for each available plant
            foreach (var plantData in availablePlants)
            {
                if (plantData == null) continue;
                
                CreatePlantButton(plantData);
            }
            
            Debug.Log($"Created {plantButtons.Count} plant buttons");
        }
        
        private void CreatePlantButton(PlantData plantData)
        {
            GameObject buttonGO = Instantiate(plantButtonPrefab, plantButtonContainer);
            PlantButton plantButton = buttonGO.GetComponent<PlantButton>();
            
            if (plantButton == null)
            {
                plantButton = buttonGO.AddComponent<PlantButton>();
            }
            
            // Setup plant data
            plantButton.SetPlantData(plantData);
            
            // Subscribe to events
            plantButton.OnPlantSelected += OnPlantButtonSelected;
            
            // Add to list
            plantButtons.Add(plantButton);
        }
        
        private void ClearPlantButtons()
        {
            foreach (var button in plantButtons)
            {
                if (button != null)
                {
                    button.OnPlantSelected -= OnPlantButtonSelected;
                    if (button.gameObject != null)
                        Destroy(button.gameObject);
                }
            }
            
            plantButtons.Clear();
        }
        
        #endregion
        
        #region Event Handling
        
        private void SetupEventListeners()
        {
            // Listen to sun changes
            if (sunManager != null)
            {
                sunManager.OnSunChanged += OnSunChanged;
            }
        }
        
        private void OnPlantButtonSelected(PlantButton plantButton)
        {
            if (plantButton == null || plantButton.PlantData == null) return;
            
            // Check if can afford
            int currentSun = sunManager?.GetCurrentSun() ?? 0;
            if (!plantButton.CanAfford)
            {
                PlayCannotAffordSound();
                Debug.Log($"Cannot afford {plantButton.PlantData.displayName} (Cost: {plantButton.PlantData.cost}, Sun: {currentSun})");
                return;
            }
            
            // Deselect previous selection
            if (selectedPlantButton != null)
            {
                selectedPlantButton.SetSelected(false);
            }
            
            // Select new plant
            selectedPlantButton = plantButton;
            selectedPlantButton.SetSelected(true);
            
            // Play sound
            PlaySelectSound();
            
            // Notify placement controller
            var placementController = FindFirstObjectByType<PlantPlacementController>();
            if (placementController != null)
            {
                placementController.SelectPlant(plantButton.PlantData);
            }
            
            // Notify listeners
            OnPlantSelected?.Invoke(plantButton.PlantData);
            
            Debug.Log($"Selected plant: {plantButton.PlantData.displayName}");
        }
        
        private void OnSunChanged(int newSunAmount)
        {
            // Update all plant buttons with new sun amount
            foreach (var button in plantButtons)
            {
                if (button != null)
                {
                    button.UpdateResourceAvailability(newSunAmount);
                }
            }
        }
        
        #endregion
        
        #region Plant Selection Management
        
        public void DeselectPlant()
        {
            if (selectedPlantButton != null)
            {
                selectedPlantButton.SetSelected(false);
                selectedPlantButton = null;
            }
            
            OnPlantDeselected?.Invoke();
        }
        
        public void SelectPlantByIndex(int index)
        {
            if (index >= 0 && index < plantButtons.Count)
            {
                OnPlantButtonSelected(plantButtons[index]);
            }
        }
        
        public void SelectPlantByData(PlantData plantData)
        {
            foreach (var button in plantButtons)
            {
                if (button.PlantData == plantData)
                {
                    OnPlantButtonSelected(button);
                    break;
                }
            }
        }
        
        #endregion
        
        #region Plant Placement Handling
        
        public void OnPlantPlaced()
        {
            if (selectedPlantButton == null) return;
            
            Debug.Log($"OnPlantPlaced called for {selectedPlantButton.PlantData.displayName} - Resources already spent by GroundPlantContainer");
            
            // Resources are now spent by GroundPlantContainer.ConsumeResources() 
            // before prefab creation - no need to spend again here!
            
            // Just start cooldown since plant was successfully placed
            selectedPlantButton.ForceStartCooldown();
            Debug.Log($"Plant placed successfully, cooldown started for: {selectedPlantButton.PlantData.displayName}");
            
            // Optionally deselect after placement
            // DeselectPlant();
        }
        
        #endregion
        
        #region Updates
        
        private void UpdatePlantButtons()
        {
            // Update resource availability for all buttons
            if (sunManager != null)
            {
                int currentSun = sunManager.GetCurrentSun();
                foreach (var button in plantButtons)
                {
                    if (button != null)
                    {
                        button.UpdateResourceAvailability(currentSun);
                    }
                }
            }
        }
        
        #endregion
        
        #region Audio
        
        private void PlaySelectSound()
        {
            if (plantSelectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(plantSelectSound);
            }
        }
        
        private void PlayCannotAffordSound()
        {
            if (cannotAffordSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(cannotAffordSound);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Refresh the plant list from GameDataManager
        /// </summary>
        public void RefreshPlantList()
        {
            LoadAvailablePlants();
            CreatePlantButtons();
        }
        
        /// <summary>
        /// Add a new plant to the selection
        /// </summary>
        public void AddPlant(PlantData plantData)
        {
            if (plantData == null || availablePlants.Contains(plantData)) return;
            
            availablePlants.Add(plantData);
            CreatePlantButton(plantData);
        }
        
        /// <summary>
        /// Remove a plant from the selection
        /// </summary>
        public void RemovePlant(PlantData plantData)
        {
            if (plantData == null) return;
            
            // Remove from list
            availablePlants.Remove(plantData);
            
            // Remove button
            for (int i = plantButtons.Count - 1; i >= 0; i--)
            {
                var button = plantButtons[i];
                if (button != null && button.PlantData == plantData)
                {
                    button.OnPlantSelected -= OnPlantButtonSelected;
                    plantButtons.RemoveAt(i);
                    
                    if (button == selectedPlantButton)
                    {
                        selectedPlantButton = null;
                        OnPlantDeselected?.Invoke();
                    }
                    
                    if (button.gameObject != null)
                        Destroy(button.gameObject);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Get the currently selected plant button
        /// </summary>
        public PlantButton GetSelectedButton()
        {
            return selectedPlantButton;
        }
        
        /// <summary>
        /// Get all plant buttons
        /// </summary>
        public List<PlantButton> GetAllPlantButtons()
        {
            return new List<PlantButton>(plantButtons);
        }
        
        #endregion
        
        #region Debug
        
        [ContextMenu("Refresh Plant List")]
        private void DebugRefreshPlantList()
        {
            RefreshPlantList();
        }
        
        [ContextMenu("Deselect Plant")]
        private void DebugDeselectPlant()
        {
            DeselectPlant();
        }
        
        #endregion
    }
}
