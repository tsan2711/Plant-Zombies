using UnityEngine;
using PvZ.Core;

namespace PvZ.Zombies
{
    [CreateAssetMenu(fileName = "New Zombie Armor", menuName = "PvZ/Zombie Armor")]
    public class ZombieArmorData : ScriptableObject
    {
        [Header("Basic Info")]
        public string armorID;
        public string displayName;
        public Sprite armorSprite;
        
        [Header("Protection")]
        public float durability;
        public float damageReduction; // 0.0 to 1.0
        public DamageType[] resistantTo;
        public DamageType[] vulnerableTo;
        
        [Header("Effects")]
        public bool blocksHeadshots = true;
        public bool hasSpecialBreakEffect = false;
        public GameObject breakEffectPrefab;
        public AudioClip breakSound;
        
        [Header("Visual")]
        public GameObject[] damageLevels; // Different visual states as armor degrades
    }
}
