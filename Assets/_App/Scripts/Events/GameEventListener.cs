using UnityEngine;
using UnityEngine.Events;

namespace PvZ.Events
{
    public class GameEventListener : MonoBehaviour
    {
        [Header("Event Configuration")]
        [SerializeField] private GameEvent gameEvent;
        [SerializeField] private UnityEvent response;
        
        [Header("Debug")]
        [SerializeField] private bool logWhenTriggered = false;
        
        #region Unity Lifecycle
        
        private void OnEnable()
        {
            if (gameEvent != null)
            {
                gameEvent.RegisterListener(this);
            }
        }
        
        private void OnDisable()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }
        
        #endregion
        
        #region Event Handling
        
        public void OnEventRaised()
        {
            if (logWhenTriggered)
            {
                Debug.Log($"GameEventListener on '{gameObject.name}' triggered by event '{gameEvent.name}'");
            }
            
            response?.Invoke();
        }
        
        #endregion
        
        #region Public Methods
        
        public void SetEvent(GameEvent newEvent)
        {
            // Unregister from old event
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
            
            // Register to new event
            gameEvent = newEvent;
            
            if (gameEvent != null && gameObject.activeInHierarchy)
            {
                gameEvent.RegisterListener(this);
            }
        }
        
        public void AddResponseMethod(UnityAction action)
        {
            response.AddListener(action);
        }
        
        public void RemoveResponseMethod(UnityAction action)
        {
            response.RemoveListener(action);
        }
        
        #endregion
        
        #region Debug
        
#if UNITY_EDITOR
        [ContextMenu("Test Response")]
        private void DebugTestResponse()
        {
            OnEventRaised();
        }
#endif
        
        #endregion
    }
}
