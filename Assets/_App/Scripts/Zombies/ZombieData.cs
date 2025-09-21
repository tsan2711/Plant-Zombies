using UnityEngine;
using PvZ.Core;
using PvZ.Projectiles;

namespace PvZ.Zombies
{
    [CreateAssetMenu(fileName = "New Zombie", menuName = "PvZ/Zombie Data")]
    public class ZombieData : ScriptableObject
    {
        [Header("Basic Info")]
        public ZombieID zombieID;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public GameObject prefab;
        public Sprite icon;
        
        [Header("Movement")]
        public float moveSpeed;
        public float eatSpeed;
        public ZombieMovementType movementType;
        
        [Header("Combat Stats")]
        public float health;
        public float damage;
        public float attackRange;
        public float attackSpeed;
        
        [Header("Special Properties")]
        public ZombieType zombieType;
        public ZombieAbilityData[] abilities;
        public ProjectileData projectileData; // For shooting zombies
        
        [Header("Rewards")]
        public int pointValue;
        public float sunDropChance;
        public int sunDropAmount = 25;
        
        [Header("Audio")]
        public AudioClip[] groanSounds;
        public AudioClip attackSound;
        public AudioClip deathSound;
        public AudioClip eatSound;
        
        [Header("Animation")]
        public RuntimeAnimatorController animatorController;
        
        [Header("Pathfinding")]
        public bool canClimbLadders = false;
        public bool canJumpOverPlants = false;
        public bool canDigUnderground = false;
        public bool canFly = false;
        
        [Header("Spawning")]
        public float spawnWeight = 1f; // Higher weight = more likely to spawn
        public int minWaveToAppear = 1;
        public bool isBossZombie = false;
    }
}
