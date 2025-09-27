using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Reflection;
using PvZ.Core;
using PvZ.Plants;
using PvZ.Managers;

namespace PvZ.Placement
{
    /// <summary>
    /// Controller for handling plant placement through mouse input
    /// This script manages the placement mode, mouse detection, and plant selection
    /// </summary>
    public class PlantPlacementController : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private LayerMask groundLayerMask = 1; // Layer for ground tiles
        [SerializeField] private float raycastDistance = 100f;
        
        [Header("Placement Settings")]
        [SerializeField] private bool usePlacementMode = true;
        [SerializeField] private KeyCode togglePlacementKey = KeyCode.Space;
        [SerializeField] private KeyCode cancelPlacementKey = KeyCode.Escape;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject placementPreviewPrefab;
        [SerializeField] private bool showMousePreview = true;
        [SerializeField] private Color validPlacementColor = Color.green;
        [SerializeField] private Color invalidPlacementColor = Color.red;
        
        [Header("Audio")]
        [SerializeField] private AudioClip placementSuccessSound;
        [SerializeField] private AudioClip placementFailSound;
        [SerializeField] private AudioClip selectionChangeSound;
        
        // Properties
        public bool IsPlacementMode { get; private set; }
        public PlantData SelectedPlantData { get; private set; }
        
        // Private fields
        private Camera mainCamera;
        private AudioSource audioSource;
        private GameObject previewObject;
        private GroundPlantContainer hoveredContainer;
        private Mouse mouse;
        private Keyboard keyboard;
        
        // Events
        public System.Action<PlantData> OnPlantSelected;
        public System.Action<GroundPlantContainer, PlantController> OnPlantPlaced;
        public System.Action OnPlacementModeChanged;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            InitializeInput();
        }
        
        private void Start()
        {
            InitializePlacementSystem();
        }
        
        private void Update()
        {
            HandleKeyboardInput();
            HandleMouseInput();
            UpdatePlacementPreview();
        }
        
        private void OnDestroy()
        {
            CleanupPreview();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            // Get main camera
            mainCamera = Camera.main;
            if (mainCamera == null)
                mainCamera = FindFirstObjectByType<Camera>();
            
            // Get or add audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        private void InitializeInput()
        {
            // Initialize Input System devices
            mouse = Mouse.current;
            keyboard = Keyboard.current;
            
            if (mouse == null)
                Debug.LogWarning("Mouse device not found! Mouse input won't work.");
            
            if (keyboard == null)
                Debug.LogWarning("Keyboard device not found! Keyboard shortcuts won't work.");
        }
        
        private void InitializePlacementSystem()
        {
            // Start in placement mode if specified
            if (usePlacementMode)
            {
                SetPlacementMode(true);
            }
        }
        
        #endregion
        
        #region Input Handling
        
        private void HandleKeyboardInput()
        {
            if (keyboard == null) return;
            
            // Toggle placement mode
            if (Input.GetKeyDown(togglePlacementKey))
            {
                TogglePlacementMode();
            }
            
            // Cancel placement
            if (Input.GetKeyDown(cancelPlacementKey))
            {
                if (IsPlacementMode)
                {
                    CancelPlacement();
                }
            }
            
            // Number keys for plant selection (1-9)
            HandleNumberKeyPlantSelection();
        }
        
        private void HandleNumberKeyPlantSelection()
        {
            if (keyboard == null) return;
            
            for (int i = 1; i <= 9; i++)
            {
                var key = (Key)((int)Key.Digit1 + i - 1);
                if (keyboard[key].wasPressedThisFrame)
                {
                    SelectPlantByIndex(i - 1);
                    break;
                }
            }
        }
        
        private void HandleMouseInput()
        {
            if (mouse == null || !IsPlacementMode) return;
            
            // Left click to place plant
            if (mouse.leftButton.wasPressedThisFrame)
            {
                TryPlaceAtMousePosition();
            }
            
            // Right click to cancel placement
            if (mouse.rightButton.wasPressedThisFrame)
            {
                CancelPlacement();
            }
            
            // Update hovered container
            UpdateHoveredContainer();
        }
        
        #endregion
        
        #region Mouse Detection & Raycasting
        
        private void UpdateHoveredContainer()
        {
            var newHovered = GetContainerAtMousePosition();
            
            if (newHovered != hoveredContainer)
            {
                // Clear previous hover
                if (hoveredContainer != null)
                {
                    // Container handles its own mouse exit
                }
                
                hoveredContainer = newHovered;
                
                // Set new hover
                if (hoveredContainer != null)
                {
                    // Container handles its own mouse enter
                }
            }
        }
        
        private GroundPlantContainer GetContainerAtMousePosition()
        {
            if (mouse == null || mainCamera == null) return null;
            
            Vector2 mousePos = mouse.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundLayerMask))
            {
                return hit.collider.GetComponent<GroundPlantContainer>();
            }
            
            return null;
        }
        
        private Vector3 GetWorldPositionAtMouse()
        {
            if (mouse == null || mainCamera == null) return Vector3.zero;
            
            Vector2 mousePos = mouse.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundLayerMask))
            {
                return hit.point;
            }
            
            return Vector3.zero;
        }
        
        #endregion
        
        #region Plant Selection
        
        public void SelectPlant(PlantData plantData)
        {
            if (plantData == null)
            {
                Debug.LogWarning("Attempted to select null plant data");
                return;
            }
            
            SelectedPlantData = plantData;
            
            // Enable placement mode when plant is selected
            SetPlacementMode(true);
            
            // Play selection sound
            PlaySound(selectionChangeSound);
            
            // Notify listeners
            OnPlantSelected?.Invoke(plantData);
            
            Debug.Log($"Selected plant: {plantData.displayName}");
        }
        
        public void SelectPlantByIndex(int index)
        {
            // Get available plants from GameManager or plant selection UI
            var availablePlants = GetAvailablePlants();
            
            if (availablePlants != null && index >= 0 && index < availablePlants.Count)
            {
                SelectPlant(availablePlants[index]);
            }
            else
            {
                Debug.LogWarning($"Plant index {index} is out of range or no plants available");
            }
        }
        
        private List<PlantData> GetAvailablePlants()
        {
            // Get available plants from GameDataManager
            var gameDataManager = GameManager.Instance?.GetGameData();
            if (gameDataManager != null)
            {
                var unlockedPlants = gameDataManager.GetUnlockedPlants();
                return new List<PlantData>(unlockedPlants);
            }
            
            return new List<PlantData>();
        }
        
        #endregion
        
        #region Plant Placement
        
        private void TryPlaceAtMousePosition()
        {
            var container = GetContainerAtMousePosition();
            if (container != null)
            {
                container.TryPlacePlant();
            }
            else
            {
                Debug.Log("No valid ground container found at mouse position");
                PlaySound(placementFailSound);
            }
        }
        
        public void HandlePlantPlaced(GroundPlantContainer container, PlantController plant)
        {
            if (container == null || plant == null) return;
            
            // Play success sound
            PlaySound(placementSuccessSound);
            
            // Notify UI that plant was placed
            var uiAdapterGO = GameObject.Find("UIAdapter");
            if (uiAdapterGO != null)
            {
                var uiAdapter = uiAdapterGO.GetComponent<MonoBehaviour>();
                if (uiAdapter != null)
                {
                    var method = uiAdapter.GetType().GetMethod("NotifyPlantPlaced");
                    method?.Invoke(uiAdapter, null);
                }
            }
            
            // Notify listeners
            OnPlantPlaced?.Invoke(container, plant);
            
            // Optionally exit placement mode after successful placement
            // SetPlacementMode(false);
            
            Debug.Log($"Plant placed successfully at {container.GridPosition}");
        }
        
        #endregion
        
        #region Placement Mode
        
        public void SetPlacementMode(bool enabled)
        {
            if (IsPlacementMode == enabled) return;
            
            IsPlacementMode = enabled;
            
            if (enabled)
            {
                EnablePlacementMode();
            }
            else
            {
                DisablePlacementMode();
            }
            
            OnPlacementModeChanged?.Invoke();
        }
        
        public void TogglePlacementMode()
        {
            SetPlacementMode(!IsPlacementMode);
        }
        
        private void EnablePlacementMode()
        {
            Debug.Log("Placement mode enabled");
            CreatePlacementPreview();
        }
        
        private void DisablePlacementMode()
        {
            Debug.Log("Placement mode disabled");
            CleanupPreview();
        }
        
        public void CancelPlacement()
        {
            SelectedPlantData = null;
            SetPlacementMode(false);
        }
        
        #endregion
        
        #region Preview System
        
        private void CreatePlacementPreview()
        {
            if (!showMousePreview) return;
            
            CleanupPreview(); // Remove existing preview
            
            // Get selected plant from UI
            PlantData selectedPlant = GetSelectedPlantFromUI();
            if (selectedPlant == null) return;
            
            if (placementPreviewPrefab != null)
            {
                previewObject = Instantiate(placementPreviewPrefab);
            }
            else if (selectedPlant.prefab != null)
            {
                // Use the plant prefab as preview
                previewObject = Instantiate(selectedPlant.prefab);
                
                // Disable gameplay components
                var plantController = previewObject.GetComponent<PlantController>();
                if (plantController != null)
                    plantController.enabled = false;
                
                // Disable colliders to prevent interference
                var colliders = previewObject.GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                {
                    col.enabled = false;
                }
                
                // Make it semi-transparent
                MakePreviewTransparent();
            }
        }
        
        private void UpdatePlacementPreview()
        {
            if (!IsPlacementMode) return;
            
            // Check if we need to create/recreate preview
            PlantData selectedPlant = GetSelectedPlantFromUI();
            if (selectedPlant == null)
            {
                CleanupPreview();
                return;
            }
            
            if (previewObject == null)
            {
                CreatePlacementPreview();
            }
            
            if (previewObject == null) return;
            
            Vector3 worldPos = GetWorldPositionAtMouse();
            if (worldPos != Vector3.zero)
            {
                previewObject.transform.position = worldPos + Vector3.up * 0.1f;
                
                // Update preview color based on validity
                bool canPlace = hoveredContainer != null && hoveredContainer.CanPlaceHere &&
                               CanAffordPlant(selectedPlant);
                UpdatePreviewColor(canPlace);
            }
        }
        
        private void MakePreviewTransparent()
        {
            if (previewObject == null) return;
            
            var renderers = previewObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    // Make material transparent
                    material.color = new Color(material.color.r, material.color.g, material.color.b, 0.5f);
                }
            }
        }
        
        private void UpdatePreviewColor(bool canPlace)
        {
            if (previewObject == null) return;
            
            Color targetColor = canPlace ? validPlacementColor : invalidPlacementColor;
            targetColor.a = 0.5f; // Keep transparency
            
            var renderers = previewObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    material.color = targetColor;
                }
            }
        }
        
        private void CleanupPreview()
        {
            if (previewObject != null)
            {
                DestroyImmediate(previewObject);
                previewObject = null;
            }
        }
        
        #endregion
        
        #region Audio
        
        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Get the currently selected plant data from UI
        /// </summary>
        public PlantData GetSelectedPlantData()
        {
            return GetSelectedPlantFromUI();
        }
        
        /// <summary>
        /// Get selected plant from PlantSelectionUI
        /// </summary>
        private PlantData GetSelectedPlantFromUI()
        {
            var uiAdapterGO = GameObject.Find("UIAdapter");
            if (uiAdapterGO != null)
            {
                var uiAdapter = uiAdapterGO.GetComponent<MonoBehaviour>();
                if (uiAdapter != null)
                {
                    var method = uiAdapter.GetType().GetMethod("GetSelectedPlantData");
                    if (method != null)
                    {
                        var result = method.Invoke(uiAdapter, null);
                        if (result is PlantData plantData)
                        {
                            return plantData;
                        }
                    }
                }
            }
            
            // Fallback to internal selected plant
            return SelectedPlantData;
        }
        
        /// <summary>
        /// Check if a specific plant type can be placed
        /// </summary>
        public bool CanPlacePlant(PlantData plantData)
        {
            if (plantData == null) return false;
            
            return CanAffordPlant(plantData);
        }
        
        /// <summary>
        /// Check if player can afford a plant
        /// </summary>
        private bool CanAffordPlant(PlantData plantData)
        {
            if (plantData == null) return false;
            
            // Check with SunManager through UIAdapter
            var uiAdapterGO = GameObject.Find("UIAdapter");
            if (uiAdapterGO != null)
            {
                var uiAdapter = uiAdapterGO.GetComponent<MonoBehaviour>();
                if (uiAdapter != null)
                {
                    var method = uiAdapter.GetType().GetMethod("CanAffordPlant");
                    if (method != null)
                    {
                        var result = method.Invoke(uiAdapter, new object[] { plantData.cost });
                        if (result is bool canAfford)
                        {
                            return canAfford;
                        }
                    }
                }
            }
            
            // Fallback to GameManager
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                return gameManager.CanAffordPlant(plantData.cost);
            }
            
            return true;
        }
        
        /// <summary>
        /// Force select a plant and enter placement mode
        /// </summary>
        public void StartPlacement(PlantData plantData)
        {
            SelectPlant(plantData);
            SetPlacementMode(true);
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmos()
        {
            if (!IsPlacementMode) return;
            
            // Draw raycast from camera to mouse position
            if (mouse != null && mainCamera != null)
            {
                Vector2 mousePos = mouse.position.ReadValue();
                Ray ray = mainCamera.ScreenPointToRay(mousePos);
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * raycastDistance);
                
                // Draw hit point if any
                if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundLayerMask))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(hit.point, 0.2f);
                }
            }
        }
        
        #endregion
    }
}
