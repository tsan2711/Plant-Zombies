using UnityEngine;
using PvZ.Core;
using PvZ.Projectiles;

namespace PvZ.Plants
{
    [CreateAssetMenu(fileName = "New Plant", menuName = "PvZ/Plant Data")]
    public class PlantData : ScriptableObject
    {
        [Header("Basic Info")]
        public string plantID;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;
        public GameObject prefab;
        public int cost;
        
        [Header("Combat Stats")]
        public float health;
        public float damage;
        public float attackSpeed;
        public float range;
        
        [Header("Projectile Settings")]
        public ProjectileData projectileData;
        public int projectileCount = 1;
        public float projectileSpread = 0f;
        public Transform[] launchPoints;
        
        [Header("Special Abilities")]
        public PlantAbilityData[] abilities;
        public PlantType plantType;
        public bool canAttackAir = false;
        public bool canAttackGround = true;
        
        [Header("Audio")]
        public AudioClip plantSound;
        public AudioClip attackSound;
        public AudioClip destroySound;
        
        [Header("Animation")]
        public RuntimeAnimatorController animatorController;
        
        [Header("Balancing")]
        public float cooldownTime = 7.5f;
        public bool isUnlocked = true;
        public int levelRequirement = 1;
    }
}
