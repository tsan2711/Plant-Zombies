using UnityEngine;
using System.Collections.Generic;

namespace PvZ.Events
{
    // Generic parameterized event
    public abstract class ParameterizedGameEvent<T> : ScriptableObject
    {
        [TextArea(2, 4)]
        public string description;
        
        private List<IParameterizedGameEventListener<T>> listeners = new List<IParameterizedGameEventListener<T>>();
        
        public void Raise(T parameter)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                if (listeners[i] != null)
                {
                    listeners[i].OnEventRaised(parameter);
                }
                else
                {
                    listeners.RemoveAt(i);
                }
            }
            
#if UNITY_EDITOR
            Debug.Log($"Parameterized Event '{name}' raised with parameter: {parameter}");
#endif
        }
        
        public void RegisterListener(IParameterizedGameEventListener<T> listener)
        {
            if (listener == null) return;
            
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }
        
        public void UnregisterListener(IParameterizedGameEventListener<T> listener)
        {
            if (listener == null) return;
            
            listeners.Remove(listener);
        }
        
        public int GetListenerCount() => listeners.Count;
    }
    
    // Interface for parameterized event listeners
    public interface IParameterizedGameEventListener<T>
    {
        void OnEventRaised(T parameter);
    }
    
    // Specific event types
    [CreateAssetMenu(fileName = "New Int Event", menuName = "PvZ/Events/Int Event")]
    public class IntGameEvent : ParameterizedGameEvent<int> { }
    
    [CreateAssetMenu(fileName = "New Float Event", menuName = "PvZ/Events/Float Event")]
    public class FloatGameEvent : ParameterizedGameEvent<float> { }
    
    [CreateAssetMenu(fileName = "New String Event", menuName = "PvZ/Events/String Event")]
    public class StringGameEvent : ParameterizedGameEvent<string> { }
    
    [CreateAssetMenu(fileName = "New Vector3 Event", menuName = "PvZ/Events/Vector3 Event")]
    public class Vector3GameEvent : ParameterizedGameEvent<Vector3> { }
    
    [CreateAssetMenu(fileName = "New Entity Event", menuName = "PvZ/Events/Entity Event")]
    public class EntityGameEvent : ParameterizedGameEvent<PvZ.Core.IEntity> { }
}
