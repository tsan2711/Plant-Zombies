using UnityEngine;
using PvZ.Core;

namespace PvZ.Zombies
{
    [CreateAssetMenu(fileName = "New Zombie Ability", menuName = "PvZ/Zombie Ability")]
    public class ZombieAbilityData : ScriptableObject
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
        public ZombieAbilityEffectData[] effects;
        
        [Header("Visual")]
        public GameObject effectPrefab;
        public AudioClip soundEffect;
    }

    [System.Serializable]
    public class ZombieAbilityEffectData
    {
        public EffectID effectID;
        public float value;
        public float range;
        public bool affectsSelf;
        public bool affectsAllies;
        public bool affectsEnemies;
    }
}
