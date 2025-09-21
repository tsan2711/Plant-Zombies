using UnityEngine;
using PvZ.Core;
using PvZ.Projectiles;

namespace PvZ.Plants
{
    [CreateAssetMenu(fileName = "New Plant", menuName = "PvZ/Plant Data")]
    public class PlantData : ScriptableObject
    {
        [Header("Basic Info")]
        public AnimalID plantID;
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
        
        [Header("Detection Settings")]
        [Tooltip("Number of rows this plant can detect zombies:\n" +
                 "0 = Only current row\n" +
                 "1 = Current row + 1 row before/after (total 3 rows)\n" +
                 "2 = Current row + 2 rows before/after (total 5 rows)")]
        public int detectionRows = 1;
        
        [Header("Projectile Settings")]
        public ProjectileData projectileData;
        public int projectileCount = 1;
        public float projectileSpread = 0f;
        
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
