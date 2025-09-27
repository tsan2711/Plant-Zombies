using UnityEngine;
using PvZ.Plants;

namespace PvZ.Placement
{
    /// <summary>
    /// Adapter to bridge PlantPlacementController with UI components
    /// Avoids direct type references that might cause compilation issues
    /// </summary>
    public class UIAdapter : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private MonoBehaviour plantSelectionUI;
        [SerializeField] private MonoBehaviour sunManager;
        
        // Singleton instance
        public static UIAdapter Instance { get; private set; }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Multiple UIAdapter instances found! Destroying duplicate.");
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            AutoFindUIComponents();
        }
        
        #endregion
        
        #region Auto-Find Components
        
        private void AutoFindUIComponents()
        {
            // Find PlantSelectionUI if not assigned
            if (plantSelectionUI == null)
            {
                var plantSelectionGO = GameObject.Find("PlantSelectionUI");
                if (plantSelectionGO != null)
                {
                    plantSelectionUI = plantSelectionGO.GetComponent<MonoBehaviour>();
                }
            }
            
            // Find SunManager if not assigned
            if (sunManager == null)
            {
                var sunManagerGO = GameObject.Find("SunManager");
                if (sunManagerGO != null)
                {
                    sunManager = sunManagerGO.GetComponent<MonoBehaviour>();
                }
            }
        }
        
        #endregion
        
        #region Plant Selection
        
        /// <summary>
        /// Get currently selected plant data from UI
        /// </summary>
        public PlantData GetSelectedPlantData()
        {
            if (plantSelectionUI == null) return null;
            
            try
            {
                var property = plantSelectionUI.GetType().GetProperty("SelectedPlantData");
                if (property != null)
                {
                    return property.GetValue(plantSelectionUI) as PlantData;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get SelectedPlantData: {e.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Notify UI that a plant was placed
        /// </summary>
        public void NotifyPlantPlaced()
        {
            if (plantSelectionUI == null) return;
            
            try
            {
                var method = plantSelectionUI.GetType().GetMethod("OnPlantPlaced");
                method?.Invoke(plantSelectionUI, null);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to call OnPlantPlaced: {e.Message}");
            }
        }
        
        #endregion
        
        #region Sun Management
        
        /// <summary>
        /// Check if player can afford a plant
        /// </summary>
        public bool CanAffordPlant(int cost)
        {
            if (sunManager == null) return true; // Default to true if no sun manager
            
            try
            {
                var method = sunManager.GetType().GetMethod("CanAfford");
                if (method != null)
                {
                    var result = method.Invoke(sunManager, new object[] { cost });
                    if (result is bool canAfford)
                    {
                        return canAfford;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to check CanAfford: {e.Message}");
            }
            
            return true; // Default to true if method fails
        }
        
        /// <summary>
        /// Get current sun amount
        /// </summary>
        public int GetCurrentSun()
        {
            if (sunManager == null) return 999; // Default high value
            
            try
            {
                var method = sunManager.GetType().GetMethod("GetCurrentSun");
                if (method != null)
                {
                    var result = method.Invoke(sunManager, null);
                    if (result is int sunAmount)
                    {
                        return sunAmount;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get CurrentSun: {e.Message}");
            }
            
            return 999; // Default high value
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Set UI references manually
        /// </summary>
        public void SetUIReferences(MonoBehaviour plantSelection, MonoBehaviour sun)
        {
            plantSelectionUI = plantSelection;
            sunManager = sun;
        }
        
        /// <summary>
        /// Check if UI components are connected
        /// </summary>
        public bool IsUIConnected()
        {
            return plantSelectionUI != null && sunManager != null;
        }
        
        #endregion
        
        #region Debug
        
        [ContextMenu("Test Get Selected Plant")]
        private void DebugGetSelectedPlant()
        {
            var plantData = GetSelectedPlantData();
            Debug.Log($"Selected Plant: {(plantData != null ? plantData.displayName : "None")}");
        }
        
        [ContextMenu("Test Can Afford")]
        private void DebugCanAfford()
        {
            bool canAfford = CanAffordPlant(50);
            Debug.Log($"Can afford 50 sun: {canAfford}");
        }
        
        [ContextMenu("Test Current Sun")]
        private void DebugCurrentSun()
        {
            int sun = GetCurrentSun();
            Debug.Log($"Current sun: {sun}");
        }
        
        #endregion
    }
}
