using UnityEngine;
using PvZ.Core;

namespace PvZ.Plants
{
    [CreateAssetMenu(fileName = "New Plant Ability", menuName = "PvZ/Plant Ability")]
    public class PlantAbilityData : ScriptableObject
    {
        [Header("Basic Info")]
        public string abilityID;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Timing")]
        public float cooldown;
        public float duration;
        public AbilityTriggerType triggerType;
        
        [Header("Effects")]
        public AbilityEffectData[] effects;
        
        [Header("Visual")]
        public GameObject effectPrefab;
        public AudioClip soundEffect;
    }

    [System.Serializable]
    public class AbilityEffectData
    {
        public string effectID;
        public float value;
        public float range;
        public bool affectsSelf;
        public bool affectsAllies;
        public bool affectsEnemies;
    }
}
