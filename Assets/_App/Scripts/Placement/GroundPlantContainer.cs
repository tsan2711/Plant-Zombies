using UnityEngine;
using PvZ.Core;
using PvZ.Plants;
using PvZ.Factory;
using PvZ.Managers;
using PvZ.UI;

namespace PvZ.Placement
{
    /// <summary>
    /// Container for a ground tile where plants can be placed
    /// Attach this script to ground tiles in your scene
    /// </summary>
    public class GroundPlantContainer : MonoBehaviour
    {
        [Header("Container Settings")]
        [SerializeField] private bool canPlantHere = true;
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private PlantType[] allowedPlantTypes = { PlantType.Shooter };
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject highlightEffect;
        [SerializeField] private Color availableColor = Color.green;
        [SerializeField] private Color occupiedColor = Color.red;
        [SerializeField] private Color unavailableColor = Color.gray;
        
        [Header("Components")]
        [SerializeField] private Collider groundCollider;
        [SerializeField] private Renderer groundRenderer;
        
        // Properties
        public bool IsOccupied => currentPlant != null;
        public bool CanPlaceHere => canPlantHere && !IsOccupied;
        public Vector2Int GridPosition => gridPosition;
        public Vector3 PlantPosition => transform.position + Vector3.up * 1f;
        
        // Private fields
        private PlantController currentPlant;
        private Material originalMaterial;
        private PlantPlacementController placementController;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            RegisterWithManager();
        }
        
        private void OnMouseEnter()
        {
            if (placementController != null && placementController.IsPlacementMode)
            {
                ShowHighlight();
            }
        }
        
        private void OnMouseExit()
        {
            if (placementController != null && placementController.IsPlacementMode)
            {
                HideHighlight();
            }
        }
        
        private void OnMouseDown()
        {
            if (placementController != null && placementController.IsPlacementMode)
            {
                TryPlacePlant();
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            // Get collider if not assigned
            if (groundCollider == null)
                groundCollider = GetComponent<Collider>();
            
            // Get renderer if not assigned
            if (groundRenderer == null)
                groundRenderer = GetComponent<Renderer>();
            
            // Store original material
            if (groundRenderer != null)
                originalMaterial = groundRenderer.material;
            
            // Ensure collider is set up for mouse detection
            if (groundCollider != null)
            {
                groundCollider.isTrigger = false; // Should be solid for raycast
            }
        }
        
        private void RegisterWithManager()
        {
            placementController = FindFirstObjectByType<PlantPlacementController>();
            if (placementController == null)
            {
                Debug.LogWarning("PlantPlacementController not found! Ground container won't function properly.");
            }
        }
        
        #endregion
        
        #region Plant Placement
        
        /// <summary>
        /// Try to place the currently selected plant at this position
        /// </summary>
        public void TryPlacePlant()
        {
            // Basic checks
            if (!CanPlaceHere)
            {
                return;
            }
            
            if (placementController == null)
            {
                return;
            }
            
            var selectedPlantData = placementController.GetSelectedPlantData();
            if (selectedPlantData == null)
            {
                return;
            }
            
            // Simplified validation - allow all plant types for now
            // if (!IsPlantTypeAllowed(selectedPlantData.plantType))
            // {
            //     return;
            // }
            
            // Resource check already handled by UI, just place the plant
            PlacePlant(selectedPlantData);
        }
        
        /// <summary>
        /// Actually place the plant at this position
        /// </summary>
        private void PlacePlant(PlantData plantData)
        {
            // SPEND RESOURCES FIRST - before creating prefab
            bool resourcesConsumed = ConsumeResources(plantData);
            if (!resourcesConsumed)
            {
                return;
            }
            
            Vector3 spawnPosition = PlantPosition;
            
            // Try EntityFactory first
            var plant = EntityFactory.CreatePlant(plantData, spawnPosition, transform);
            
            // Fallback: Direct instantiation if EntityFactory fails
            if (plant == null && plantData?.prefab != null)
            {
                try
                {
                    var plantGO = Instantiate(plantData.prefab, spawnPosition, Quaternion.identity, transform);
                    plant = plantGO.GetComponent<PlantController>();
                    
                    if (plant == null)
                    {
                        plant = plantGO.AddComponent<PlantController>();
                    }
                    
                    plant.SetPlantData(plantData);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to create plant: {e.Message}");
                    RefundResources(plantData);
                    return;
                }
            }
            
            if (plant != null)
            {
                // FIX: Ensure plant is positioned correctly after creation
                plant.transform.position = spawnPosition;
                
                currentPlant = plant;
                
                // Notify placement controller (but don't spend resources again)
                placementController?.HandlePlantPlaced(this, plant);
                
                // Update visual state
                HideHighlight();
            }
            else
            {
                // Refund resources if everything failed
                RefundResources(plantData);
            }
        }
        
        /// <summary>
        /// Remove the plant from this container
        /// </summary>
        public void RemovePlant()
        {
            if (currentPlant != null)
            {
                // Destroy the plant
                if (currentPlant.gameObject != null)
                {
                    Destroy(currentPlant.gameObject);
                }
                
                currentPlant = null;
                Debug.Log($"Removed plant from {gridPosition}");
            }
        }
        
        #endregion
        
        #region Validation
        
        private bool IsPlantTypeAllowed(PlantType plantType)
        {
            foreach (var allowedType in allowedPlantTypes)
            {
                if (allowedType == plantType)
                    return true;
            }
            return false;
        }
        
        private bool CanAffordPlant(PlantData plantData)
        {
            // Simplified - resource check is handled by UI system
            // Just return true since UI already validates before allowing placement
            return true;
        }
        
        private bool ConsumeResources(PlantData plantData)
        {
            // Try to spend resources through SunManager first
            var sunManager = SunManager.Instance;
            if (sunManager != null)
            {
                return sunManager.SpendSun(plantData.cost);
            }
            
            // Fallback to GameManager
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                return gameManager.SpendSun(plantData.cost);
            }
            
            return false;
        }
        
        private void RefundResources(PlantData plantData)
        {
            // Try to refund resources through SunManager first
            var sunManager = SunManager.Instance;
            if (sunManager != null)
            {
                sunManager.AddSun(plantData.cost);
                return;
            }
            
            // Fallback to GameManager
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.AddSun(plantData.cost);
            }
        }
        
        #endregion
        
        #region Visual Feedback
        
        private void ShowHighlight()
        {
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(true);
            }
            
            // Change material color based on state
            if (groundRenderer != null)
            {
                Color highlightColor = GetHighlightColor();
                groundRenderer.material.color = highlightColor;
            }
        }
        
        private void HideHighlight()
        {
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(false);
            }
            
            // Restore original material
            if (groundRenderer != null && originalMaterial != null)
            {
                groundRenderer.material = originalMaterial;
            }
        }
        
        private Color GetHighlightColor()
        {
            if (!canPlantHere)
                return unavailableColor;
            
            if (IsOccupied)
                return occupiedColor;
            
            // Check if selected plant can be placed here
            if (placementController != null)
            {
                var selectedPlant = placementController.GetSelectedPlantData();
                if (selectedPlant != null)
                {
                    if (!IsPlantTypeAllowed(selectedPlant.plantType) || !CanAffordPlant(selectedPlant))
                        return unavailableColor;
                }
            }
            
            return availableColor;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Set the grid position for this container
        /// </summary>
        public void SetGridPosition(int x, int y)
        {
            gridPosition = new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Set whether plants can be placed here
        /// </summary>
        public void SetPlantable(bool plantable)
        {
            canPlantHere = plantable;
        }
        
        /// <summary>
        /// Set the allowed plant types for this container
        /// </summary>
        public void SetAllowedPlantTypes(PlantType[] types)
        {
            allowedPlantTypes = types;
        }
        
        /// <summary>
        /// Get the current plant in this container
        /// </summary>
        public PlantController GetCurrentPlant()
        {
            return currentPlant;
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmos()
        {
            // Draw grid position
            Gizmos.color = CanPlaceHere ? Color.green : (IsOccupied ? Color.red : Color.gray);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.1f, Vector3.one * 0.8f);
            
            // Draw grid coordinates
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"({gridPosition.x}, {gridPosition.y})");
            #endif
        }
        
        #endregion
    }
}
