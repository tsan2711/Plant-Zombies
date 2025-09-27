using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PvZ.Plants;
using PvZ.Managers;

namespace PvZ.UI
{
    /// <summary>
    /// UI Button component for selecting plants
    /// </summary>
    public class PlantButton : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image plantIcon;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextMeshProUGUI cooldownText;
        
        [Header("Visual States")]
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color unavailableColor = Color.gray;
        [SerializeField] private Color selectedColor = Color.green;
        [SerializeField] private Color cooldownColor = Color.red;
        
        // Properties
        public PlantData PlantData { get; private set; }
        public bool IsSelected { get; private set; }
        public bool IsAvailable => CanAfford && !IsOnCooldown;
        
        // Private fields
        private bool canAfford = true;
        private bool isOnCooldown = false;
        private float cooldownTime = 0f;
        private float currentCooldown = 0f;
        
        // Events
        public System.Action<PlantButton> OnPlantSelected;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            SetupButton();
        }
        
        private void Update()
        {
            UpdateCooldown();
            UpdateVisualState();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            // Auto-find components if not assigned
            if (button == null)
                button = GetComponent<Button>();
            
            if (plantIcon == null)
                plantIcon = transform.Find("PlantIcon")?.GetComponent<Image>();
            
            if (costText == null)
                costText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (cooldownOverlay == null)
                cooldownOverlay = transform.Find("CooldownOverlay")?.GetComponent<Image>();
            
            if (cooldownText == null)
                cooldownText = transform.Find("CooldownText")?.GetComponent<TextMeshProUGUI>();
        }
        
        private void SetupButton()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }
        }
        
        #endregion
        
        #region Plant Data Setup
        
        public void SetPlantData(PlantData plantData)
        {
            PlantData = plantData;
            
            if (plantData == null)
            {
                Debug.LogWarning("PlantData is null for PlantButton");
                return;
            }
            
            UpdateUI();
            cooldownTime = plantData.cooldownTime;
        }
        
        private void UpdateUI()
        {
            if (PlantData == null) return;
            
            // Update icon
            if (plantIcon != null && PlantData.icon != null)
            {
                plantIcon.sprite = PlantData.icon;
            }
            
            // Update cost text
            if (costText != null)
            {
                costText.text = PlantData.cost.ToString();
            }
            
            // Hide cooldown initially
            if (cooldownOverlay != null)
                cooldownOverlay.gameObject.SetActive(false);
            
            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);
        }
        
        #endregion
        
        #region Button Interaction
        
        private void OnButtonClick()
        {
            if (!IsAvailable || PlantData == null) return;
            
            // Notify listeners
            OnPlantSelected?.Invoke(this);
            
            // Start cooldown
            StartCooldown();
            
            Debug.Log($"Selected plant: {PlantData.displayName}");
        }
        
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            UpdateVisualState();
        }
        
        #endregion
        
        #region Cooldown System
        
        private void StartCooldown()
        {
            if (cooldownTime <= 0) return;
            
            isOnCooldown = true;
            currentCooldown = cooldownTime;
            
            if (cooldownOverlay != null)
                cooldownOverlay.gameObject.SetActive(true);
            
            if (cooldownText != null)
                cooldownText.gameObject.SetActive(true);
        }
        
        private void UpdateCooldown()
        {
            if (!isOnCooldown) return;
            
            currentCooldown -= Time.deltaTime;
            
            if (currentCooldown <= 0)
            {
                // Cooldown finished
                isOnCooldown = false;
                currentCooldown = 0;
                
                if (cooldownOverlay != null)
                    cooldownOverlay.gameObject.SetActive(false);
                
                if (cooldownText != null)
                    cooldownText.gameObject.SetActive(false);
            }
            else
            {
                // Update cooldown display
                if (cooldownText != null)
                {
                    cooldownText.text = Mathf.Ceil(currentCooldown).ToString();
                }
                
                if (cooldownOverlay != null)
                {
                    // Update fill amount for visual feedback
                    float fillAmount = currentCooldown / cooldownTime;
                    cooldownOverlay.fillAmount = fillAmount;
                }
            }
        }
        
        public bool IsOnCooldown => isOnCooldown;
        
        #endregion
        
        #region Resource Check
        
        public void UpdateResourceAvailability(int currentSun)
        {
            if (PlantData == null) return;
            
            canAfford = currentSun >= PlantData.cost;
            UpdateVisualState();
        }
        
        public bool CanAfford => canAfford;
        
        #endregion
        
        #region Visual State
        
        private void UpdateVisualState()
        {
            if (button == null) return;
            
            Color targetColor;
            bool interactable;
            
            if (IsSelected)
            {
                targetColor = selectedColor;
                interactable = true;
            }
            else if (isOnCooldown)
            {
                targetColor = cooldownColor;
                interactable = false;
            }
            else if (canAfford)
            {
                targetColor = availableColor;
                interactable = true;
            }
            else
            {
                targetColor = unavailableColor;
                interactable = false;
            }
            
            // Update button state
            button.interactable = interactable;
            
            // Update visual color
            if (plantIcon != null)
            {
                plantIcon.color = targetColor;
            }
            
            // Update cost text color
            if (costText != null)
            {
                costText.color = canAfford ? Color.white : Color.red;
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Force start cooldown (for external usage)
        /// </summary>
        public void ForceStartCooldown()
        {
            StartCooldown();
        }
        
        /// <summary>
        /// Get remaining cooldown time
        /// </summary>
        public float GetRemainingCooldown()
        {
            return isOnCooldown ? currentCooldown : 0f;
        }
        
        /// <summary>
        /// Check if this button can be used right now
        /// </summary>
        public bool CanUse()
        {
            return IsAvailable && PlantData != null;
        }
        
        #endregion
        
        #region Debug
        
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            
            // Auto-find components in editor
            if (button == null)
                button = GetComponent<Button>();
        }
        
        #endregion
    }
}
