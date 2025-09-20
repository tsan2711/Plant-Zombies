using UnityEngine;
using PvZ.Core;

namespace PvZ.Projectiles
{
    [CreateAssetMenu(fileName = "New Projectile", menuName = "PvZ/Projectile Data")]
    public class ProjectileData : ScriptableObject
    {
        [Header("Basic Info")]
        public string projectileID;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public GameObject prefab;
        public Sprite sprite;
        
        [Header("Movement")]
        public float speed;
        public float lifeTime;
        public ProjectileMovementType movementType;
        public AnimationCurve trajectoryX;
        public AnimationCurve trajectoryY;
        
        [Header("Physics")]
        public bool useGravity = false;
        public float gravityScale = 1f;
        public bool bounces = false;
        public int maxBounces = 0;
        
        [Header("Combat")]
        public float damage;
        public DamageType damageType;
        public float splashRadius;
        public bool piercing;
        public int maxPierceTargets;
        
        [Header("Effects")]
        public ProjectileEffectData[] onHitEffects;
        public ProjectileEffectData[] onDestroyEffects;
        public bool destroyOnHit = true;
        
        [Header("Visual")]
        public bool rotateTowardsDirection = true;
        public TrailRenderer trailPrefab;
        public ParticleSystem hitParticles;
        public ParticleSystem destroyParticles;
        
        [Header("Audio")]
        public AudioClip launchSound;
        public AudioClip hitSound;
        public AudioClip destroySound;
        
        [Header("Homing (if applicable)")]
        public bool isHoming = false;
        public float homingStrength = 5f;
        public float homingRange = 10f;
        
        [Header("Special Properties")]
        public bool ignoresArmor = false;
        public bool canHitMultipleTargets = false;
        public float knockbackForce = 0f;
    }
}
