using UnityEngine;
using System.Collections.Generic;

namespace PvZ.Events
{
    [CreateAssetMenu(fileName = "New Game Event", menuName = "PvZ/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        [TextArea(2, 4)]
        public string description;
        
        private List<GameEventListener> listeners = new List<GameEventListener>();
        
        #region Event Management
        
        public void Raise()
        {
            // Iterate backwards to avoid issues if listeners are removed during iteration
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                if (listeners[i] != null)
                {
                    listeners[i].OnEventRaised();
                }
                else
                {
                    // Remove null listeners
                    listeners.RemoveAt(i);
                }
            }
            
#if UNITY_EDITOR
            Debug.Log($"Event '{name}' raised to {listeners.Count} listeners");
#endif
        }
        
        public void RegisterListener(GameEventListener listener)
        {
            if (listener == null) return;
            
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }
        
        public void UnregisterListener(GameEventListener listener)
        {
            if (listener == null) return;
            
            listeners.Remove(listener);
        }
        
        #endregion
        
        #region Debug
        
        public int GetListenerCount() => listeners.Count;
        
        public void ClearAllListeners()
        {
            listeners.Clear();
            Debug.Log($"All listeners cleared from event '{name}'");
        }
        
#if UNITY_EDITOR
        [ContextMenu("Raise Event")]
        private void DebugRaiseEvent()
        {
            Raise();
        }
        
        [ContextMenu("Print Listener Count")]
        private void DebugPrintListenerCount()
        {
            Debug.Log($"Event '{name}' has {listeners.Count} listeners");
        }
#endif
        
        #endregion
    }
}
