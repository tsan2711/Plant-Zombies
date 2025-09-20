using UnityEngine;
using System.Collections.Generic;
using PvZ.Core;
using PvZ.Managers;

namespace PvZ.Projectiles
{
    public class ProjectileController : MonoBehaviour, IDamageDealer
    {
        [Header("Components")]
        [SerializeField] private Rigidbody2D rb2d;
        [SerializeField] private Collider2D col2d;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AudioSource audioSource;
        
        // IDamageDealer Properties
        public float Damage => projectileData?.damage ?? 0f;
        public DamageType DamageType => projectileData?.damageType ?? DamageType.Normal;
        public IEntity Owner { get; private set; }
        
        // Public Properties
        public ProjectileData ProjectileData => projectileData;
        public Vector3 Direction { get; private set; }
        public bool IsActive { get; private set; }
        
        // Private Fields
        private ProjectileData projectileData;
        private Vector3 initialPosition;
        private float launchTime;
        private float travelTime;
        private List<IEntity> hitTargets;
        private int bounceCount;
        private ITargetable homingTarget;
        private TrailRenderer trail;
        
        #region Initialization
        
        private void Awake()
        {
            InitializeComponents();
            hitTargets = new List<IEntity>();
        }
        
        private void InitializeComponents()
        {
            if (rb2d == null)
                rb2d = GetComponent<Rigidbody2D>();
            
            if (col2d == null)
                col2d = GetComponent<Collider2D>();
            
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
        
        public void Initialize(ProjectileData data, Vector3 startPosition, Vector3 direction, IEntity owner)
        {
            projectileData = data;
            initialPosition = startPosition;
            Direction = direction.normalized;
            Owner = owner;
            
            transform.position = startPosition;
            launchTime = Time.time;
            travelTime = 0f;
            IsActive = true;
            bounceCount = 0;
            
            SetupComponents();
            SetupMovement();
            SetupVisual();
            PlayLaunchSound();
            
            // Find homing target if applicable
            if (data.isHoming)
            {
                FindHomingTarget();
            }
        }
        
        private void SetupComponents()
        {
            // Setup sprite
            if (spriteRenderer != null && projectileData.sprite != null)
            {
                spriteRenderer.sprite = projectileData.sprite;
            }
            
            // Setup trail
            if (projectileData.trailPrefab != null)
            {
                trail = Instantiate(projectileData.trailPrefab, transform);
            }
            
            // Setup physics
            if (rb2d != null)
            {
                rb2d.gravityScale = projectileData.useGravity ? projectileData.gravityScale : 0f;
            }
        }
        
        private void SetupMovement()
        {
            if (rb2d != null)
            {
                switch (projectileData.movementType)
                {
                    case ProjectileMovementType.Straight:
                        rb2d.velocity = Direction * projectileData.speed;
                        break;
                    case ProjectileMovementType.Arc:
                        SetupArcMovement();
                        break;
                    case ProjectileMovementType.Lobbed:
                        SetupLobbedMovement();
                        break;
                    default:
                        rb2d.velocity = Direction * projectileData.speed;
                        break;
                }
            }
        }
        
        private void SetupArcMovement()
        {
            // Calculate arc trajectory
            Vector3 velocity = Direction * projectileData.speed;
            velocity.y += 5f; // Add upward velocity for arc
            rb2d.velocity = velocity;
        }
        
        private void SetupLobbedMovement()
        {
            // High arc trajectory
            Vector3 velocity = Direction * projectileData.speed * 0.7f;
            velocity.y += 8f; // Higher upward velocity
            rb2d.velocity = velocity;
        }
        
        private void SetupVisual()
        {
            if (projectileData.rotateTowardsDirection)
            {
                float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
        
        #endregion
        
        #region Update Loop
        
        private void Update()
        {
            if (!IsActive) return;
            
            travelTime += Time.deltaTime;
            
            UpdateMovement();
            UpdateHoming();
            UpdateRotation();
            CheckLifetime();
        }
        
        private void UpdateMovement()
        {
            if (projectileData.movementType == ProjectileMovementType.Homing && homingTarget != null)
            {
                UpdateHomingMovement();
            }
            else if (projectileData.movementType == ProjectileMovementType.Boomerang)
            {
                UpdateBoomerangMovement();
            }
        }
        
        private void UpdateHomingMovement()
        {
            if (homingTarget == null || !homingTarget.IsValidTarget()) return;
            
            Vector3 targetPosition = homingTarget.GetTargetPosition();
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            
            // Gradually turn towards target
            Vector3 currentVelocity = rb2d.velocity;
            Vector3 targetVelocity = directionToTarget * projectileData.speed;
            
            Vector3 newVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 
                projectileData.homingStrength * Time.deltaTime);
            
            rb2d.velocity = newVelocity;
            Direction = newVelocity.normalized;
        }
        
        private void UpdateBoomerangMovement()
        {
            // Implement boomerang logic
            float progress = travelTime / (projectileData.lifeTime * 0.5f);
            if (progress > 1f)
            {
                // Return to owner
                Vector3 ownerPosition = Owner?.Position ?? initialPosition;
                Vector3 returnDirection = (ownerPosition - transform.position).normalized;
                rb2d.velocity = returnDirection * projectileData.speed;
            }
        }
        
        private void UpdateHoming()
        {
            if (!projectileData.isHoming) return;
            
            if (homingTarget == null || !homingTarget.IsValidTarget())
            {
                FindHomingTarget();
            }
        }
        
        private void UpdateRotation()
        {
            if (projectileData.rotateTowardsDirection && rb2d.velocity.magnitude > 0.1f)
            {
                Vector3 velocity = rb2d.velocity;
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
        
        private void CheckLifetime()
        {
            if (travelTime >= projectileData.lifeTime)
            {
                DestroyProjectile();
            }
        }
        
        #endregion
        
        #region Collision Handling
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleCollision(other);
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleCollision(collision.collider);
        }
        
        private void HandleCollision(Collider2D other)
        {
            if (!IsActive) return;
            
            // Check if this is a valid target
            IEntity target = other.GetComponent<IEntity>();
            
            if (target != null && IsValidTarget(target, other))
            {
                HitTarget(target, other);
            }
            else if (ShouldBounce(other))
            {
                HandleBounce(other);
            }
            else if (ShouldDestroyOnHit(other))
            {
                DestroyProjectile();
            }
        }
        
        private bool IsValidTarget(IEntity target, Collider2D collider)
        {
            // Don't hit the owner
            if (target == Owner) return false;
            
            // Don't hit the same target multiple times unless piercing
            if (!projectileData.piercing && hitTargets.Contains(target)) return false;
            
            // Check if already hit max pierce targets
            if (projectileData.piercing && hitTargets.Count >= projectileData.maxPierceTargets) return false;
            
            // Check tag compatibility (plants can't hit plants, zombies can't hit zombies)
            return IsTargetCompatible(collider);
        }
        
        private bool IsTargetCompatible(Collider2D collider)
        {
            // If owner is a plant, target zombies
            if (Owner != null && Owner.GetType().Namespace == "PvZ.Plants")
            {
                return collider.CompareTag("Zombie") || collider.CompareTag("GroundZombie") || collider.CompareTag("AirZombie");
            }
            // If owner is a zombie, target plants
            else if (Owner != null && Owner.GetType().Namespace == "PvZ.Zombies")
            {
                return collider.CompareTag("Plant");
            }
            
            return false;
        }
        
        private void HitTarget(IEntity target, Collider2D collider)
        {
            // Add to hit targets list
            hitTargets.Add(target);
            
            // Deal damage
            target.TakeDamage(Damage, this);
            
            // Apply hit effects
            ApplyHitEffects(target, collider);
            
            // Play hit sound and particles
            PlayHitSound();
            SpawnHitParticles();
            
            // Handle splash damage
            if (projectileData.splashRadius > 0)
            {
                ApplySplashDamage(collider.transform.position);
            }
            
            // Handle knockback
            if (projectileData.knockbackForce > 0)
            {
                ApplyKnockback(target, collider);
            }
            
            // Destroy if not piercing or reached max targets
            if (!projectileData.piercing || 
                (projectileData.piercing && hitTargets.Count >= projectileData.maxPierceTargets))
            {
                if (projectileData.destroyOnHit)
                {
                    DestroyProjectile();
                }
            }
        }
        
        private bool ShouldBounce(Collider2D other)
        {
            return projectileData.bounces && 
                   bounceCount < projectileData.maxBounces && 
                   (other.CompareTag("Wall") || other.CompareTag("Ground"));
        }
        
        private void HandleBounce(Collider2D other)
        {
            bounceCount++;
            
            // Simple bounce logic - reverse Y velocity for ground, X velocity for walls
            if (other.CompareTag("Ground"))
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, -rb2d.velocity.y);
            }
            else if (other.CompareTag("Wall"))
            {
                rb2d.velocity = new Vector2(-rb2d.velocity.x, rb2d.velocity.y);
            }
            
            Direction = rb2d.velocity.normalized;
        }
        
        private bool ShouldDestroyOnHit(Collider2D other)
        {
            return other.CompareTag("Wall") || other.CompareTag("Ground") || other.CompareTag("Obstacle");
        }
        
        #endregion
        
        #region Effects
        
        private void ApplyHitEffects(IEntity target, Collider2D collider)
        {
            if (projectileData.onHitEffects == null) return;
            
            foreach (var effectData in projectileData.onHitEffects)
            {
                ApplyEffect(effectData, target, collider.transform.position);
            }
        }
        
        private void ApplyDestroyEffects()
        {
            if (projectileData.onDestroyEffects == null) return;
            
            foreach (var effectData in projectileData.onDestroyEffects)
            {
                ApplyEffect(effectData, null, transform.position);
            }
        }
        
        private void ApplyEffect(ProjectileEffectData effectData, IEntity target, Vector3 position)
        {
            // Spawn effect prefab
            if (effectData.effectPrefab != null)
            {
                GameObject effect = Instantiate(effectData.effectPrefab, position, Quaternion.identity);
                Destroy(effect, effectData.duration);
            }
            
            // Play sound
            if (effectData.soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(effectData.soundEffect, position);
            }
            
            // Apply effect based on type
            switch (effectData.effectType)
            {
                case ProjectileEffectType.Damage:
                    target?.TakeDamage(effectData.value, this);
                    break;
                case ProjectileEffectType.Explosion:
                    ApplyExplosionEffect(effectData, position);
                    break;
                // Add more effect types as needed
            }
        }
        
        private void ApplySplashDamage(Vector3 center)
        {
            var colliders = Physics2D.OverlapCircleAll(center, projectileData.splashRadius);
            
            foreach (var collider in colliders)
            {
                var target = collider.GetComponent<IEntity>();
                if (target != null && IsTargetCompatible(collider) && !hitTargets.Contains(target))
                {
                    float distance = Vector3.Distance(center, collider.transform.position);
                    float damageMultiplier = 1f - (distance / projectileData.splashRadius);
                    float splashDamage = Damage * damageMultiplier;
                    
                    target.TakeDamage(splashDamage, this);
                }
            }
        }
        
        private void ApplyKnockback(IEntity target, Collider2D collider)
        {
            var rb = collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockbackDirection = (collider.transform.position - transform.position).normalized;
                rb.AddForce(knockbackDirection * projectileData.knockbackForce, ForceMode2D.Impulse);
            }
        }
        
        private void ApplyExplosionEffect(ProjectileEffectData effectData, Vector3 center)
        {
            var colliders = Physics2D.OverlapCircleAll(center, effectData.range);
            
            foreach (var collider in colliders)
            {
                var target = collider.GetComponent<IEntity>();
                if (target != null && IsTargetCompatible(collider))
                {
                    target.TakeDamage(effectData.value, this);
                }
            }
        }
        
        #endregion
        
        #region Homing
        
        private void FindHomingTarget()
        {
            float closestDistance = projectileData.homingRange;
            ITargetable closestTarget = null;
            
            var colliders = Physics2D.OverlapCircleAll(transform.position, projectileData.homingRange);
            
            foreach (var collider in colliders)
            {
                var targetable = collider.GetComponent<ITargetable>();
                if (targetable != null && targetable.IsValidTarget() && IsTargetCompatible(collider))
                {
                    float distance = Vector3.Distance(transform.position, targetable.GetTargetPosition());
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = targetable;
                    }
                }
            }
            
            homingTarget = closestTarget;
        }
        
        #endregion
        
        #region Audio & Visual
        
        private void PlayLaunchSound()
        {
            if (projectileData.launchSound != null)
            {
                AudioSource.PlayClipAtPoint(projectileData.launchSound, transform.position);
            }
        }
        
        private void PlayHitSound()
        {
            if (projectileData.hitSound != null)
            {
                AudioSource.PlayClipAtPoint(projectileData.hitSound, transform.position);
            }
        }
        
        private void SpawnHitParticles()
        {
            if (projectileData.hitParticles != null)
            {
                Instantiate(projectileData.hitParticles, transform.position, Quaternion.identity);
            }
        }
        
        private void SpawnDestroyParticles()
        {
            if (projectileData.destroyParticles != null)
            {
                Instantiate(projectileData.destroyParticles, transform.position, Quaternion.identity);
            }
        }
        
        #endregion
        
        #region Cleanup
        
        public void DestroyProjectile()
        {
            if (!IsActive) return;
            
            IsActive = false;
            
            // Apply destroy effects
            ApplyDestroyEffects();
            
            // Play destroy sound and particles
            if (projectileData.destroySound != null)
            {
                AudioSource.PlayClipAtPoint(projectileData.destroySound, transform.position);
            }
            
            SpawnDestroyParticles();
            
            // Return to pool or destroy
            ProjectilePool.Instance?.ReturnProjectile(this);
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmosSelected()
        {
            if (projectileData == null) return;
            
            // Draw splash radius
            if (projectileData.splashRadius > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, projectileData.splashRadius);
            }
            
            // Draw homing range
            if (projectileData.isHoming)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, projectileData.homingRange);
            }
        }
        
        #endregion
    }
}
