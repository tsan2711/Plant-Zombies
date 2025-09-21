using UnityEngine;
using System.Collections.Generic;
using PvZ.Core;
using PvZ.Projectiles;
using PvZ.Managers;

namespace PvZ.Zombies
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    public class ZombieController : MonoBehaviour, IEntity, ITargetable, IDamageDealer
    {
        [Header("Zombie Configuration")]
        [SerializeField] private ZombieData zombieData;
        
        [Header("Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Collider col;
        
        // IEntity Properties
        public string ID => zombieData.zombieID.ToString();
        public float Health { get; set; }
        public float MaxHealth => zombieData.health;
        public Vector3 Position { get; set; }
        public bool IsActive { get; set; }
        
        // IDamageDealer Properties
        public float Damage => zombieData.damage;
        public DamageType DamageType => DamageType.Normal;
        public IEntity Owner => this;
        
        // Public Properties
        public ZombieData ZombieData => zombieData;
        public ZombieStateMachine StateMachine { get; private set; }
        
        // Private Fields
        private List<ZombieAbility> activeAbilities;
        private EntityManager entityManager;
        private float groanTimer;
        private float groanInterval = 3f;
        private bool hasReachedHouse = false;
        
        // Events
        public System.Action OnZombieDied;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            InitializeStateMachine();
            InitializeAbilities();
        }
        
        private void Start()
        {
            InitializeZombie();
            RegisterWithManagers();
        }
        
        private void Update()
        {
            if (!IsActive) return;
            
            UpdateStateMachine();
            UpdateAbilities();
            UpdateGroaning();
            UpdatePosition();
        }
        
        private void OnDestroy()
        {
            UnregisterFromManagers();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            
            if (col == null)
                col = GetComponent<Collider>();
        }
        
        private void InitializeStateMachine()
        {
            StateMachine = new ZombieStateMachine(this);
        }
        
        private void InitializeAbilities()
        {
            activeAbilities = new List<ZombieAbility>();
            
            if (zombieData.abilities != null)
            {
                foreach (var abilityData in zombieData.abilities)
                {
                    var ability = new ZombieAbility(abilityData, this);
                    activeAbilities.Add(ability);
                }
            }
        }
        
        private void InitializeZombie()
        {
            Health = zombieData.health;
            Position = transform.position;
            IsActive = true;
            
            // Set animator controller
            if (animator != null && zombieData.animatorController != null)
            {
                animator.runtimeAnimatorController = zombieData.animatorController;
            }
            
            // Start state machine
            StateMachine.Start(ZombieState.Walking);
            
            // Trigger spawn abilities
            TriggerAbilities(AbilityTriggerType.OnSpawn);
        }
        
        private void RegisterWithManagers()
        {
            entityManager = EntityManager.Instance;
            entityManager?.RegisterEntity(this);
        }
        
        private void UnregisterFromManagers()
        {
            entityManager?.UnregisterEntity(this);
        }
        
        #endregion
        
        #region State Machine Updates
        
        private void UpdateStateMachine()
        {
            StateMachine.Update();
        }
        
        private void UpdateAbilities()
        {
            foreach (var ability in activeAbilities)
            {
                ability.Update();
            }
        }
        
        private void UpdateGroaning()
        {
            groanTimer += Time.deltaTime;
            if (groanTimer >= groanInterval)
            {
                PlayGroanSound();
                groanTimer = 0f;
                groanInterval = Random.Range(2f, 5f); // Randomize next groan
            }
        }
        
        private void UpdatePosition()
        {
            Position = transform.position;
        }
        
        #endregion
        
        #region Movement
        
        public void MoveForward()
        {
            Vector3 moveDirection = Vector3.left; // Zombies move left towards house
            float moveDistance = zombieData.moveSpeed * Time.deltaTime;
            
            transform.Translate(moveDirection * moveDistance);
        }
        
        public bool HasReachedEnd()
        {
            // Check if zombie has reached the left edge of the screen
            return transform.position.x <= -10f; // Adjust based on your game boundaries
        }
        
        public void ReachHouse()
        {
            if (hasReachedHouse) return;
            
            hasReachedHouse = true;
            // Trigger game over or lose life logic
            GameManager.Instance?.ZombieReachedHouse();
            Die();
        }
        
        #endregion
        
        #region Combat
        
        public IEntity FindNearbyPlant()
        {
            float detectionRange = zombieData.attackRange;
            var colliders = Physics.OverlapSphere(transform.position, detectionRange);
            
            foreach (var collider in colliders)
            {
                var plant = collider.GetComponent<IEntity>();
                if (plant != null && collider.CompareTag("Plant"))
                {
                    return plant;
                }
            }
            
            return null;
        }
        
        public ITargetable FindAttackTarget()
        {
            float attackRange = zombieData.attackRange;
            var colliders = Physics.OverlapSphere(transform.position, attackRange);
            
            foreach (var collider in colliders)
            {
                var targetable = collider.GetComponent<ITargetable>();
                if (targetable != null && collider.CompareTag("Plant"))
                {
                    return targetable;
                }
            }
            
            return null;
        }
        
        public void LaunchProjectile(ITargetable target)
        {
            if (zombieData.projectileData == null) return;
            
            Vector3 direction = (target.GetTargetPosition() - transform.position).normalized;
            
            if (ProjectilePool.Instance != null)
            {
                var projectile = ProjectilePool.Instance.GetProjectile(zombieData.projectileData);
                if (projectile != null)
                {
                    projectile.Initialize(zombieData.projectileData, transform.position, direction, this);
                }
            }
            
            PlaySound(zombieData.attackSound);
        }
        
        #endregion
        
        #region IEntity Implementation
        
        public void TakeDamage(float damage, IDamageDealer dealer)
        {
            if (!IsActive) return;
            
            Health -= damage;
            Health = Mathf.Max(0, Health);
            
            // Trigger on damage abilities
            TriggerAbilities(AbilityTriggerType.OnDamage);
            
            if (Health <= 0)
            {
                Die();
            }
        }
        
        
        public void Die()
        {
            if (!IsActive) return;
            
            IsActive = false;
            StateMachine.ChangeState(ZombieState.Dying);
            
            // Trigger death event for spawner
            OnZombieDied?.Invoke();
        }
        
        #endregion
        
        #region ITargetable Implementation
        
        public Vector3 GetTargetPosition()
        {
            return transform.position;
        }
        
        public bool IsValidTarget()
        {
            return IsActive && Health > 0;
        }
        
        public float GetPriority()
        {
            // Higher priority for zombies closer to the house
            float distanceFromHouse = transform.position.x + 10f; // Assuming house is at x = -10
            return 1f / Mathf.Max(distanceFromHouse, 0.1f);
        }
        
        #endregion
        
        #region Animation & Audio
        
        public void SetAnimationTrigger(string triggerName)
        {
            if (animator != null)
            {
                animator.SetTrigger(triggerName);
            }
        }
        
        public bool IsDeathAnimationComplete()
        {
            if (animator == null) return true;
            
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName("Die") && stateInfo.normalizedTime >= 1.0f;
        }
        
        private void PlayGroanSound()
        {
            if (zombieData.groanSounds != null && zombieData.groanSounds.Length > 0)
            {
                var randomGroan = zombieData.groanSounds[Random.Range(0, zombieData.groanSounds.Length)];
                PlaySound(randomGroan);
            }
        }
        
        public void PlayEatSound()
        {
            PlaySound(zombieData.eatSound);
        }
        
        public void PlayDeathSound()
        {
            PlaySound(zombieData.deathSound);
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        #endregion
        
        #region Abilities
        
        private void TriggerAbilities(AbilityTriggerType triggerType)
        {
            foreach (var ability in activeAbilities)
            {
                if (ability.Data.triggerType == triggerType)
                {
                    ability.Activate();
                }
            }
        }
        
        #endregion
        
        #region Rewards
        
        public void DropRewards()
        {
            // // Drop points
            // ScoreManager.Instance?.AddScore(zombieData.pointValue);
            
            // // Drop sun
            // if (Random.value <= zombieData.sunDropChance)
            // {
            //     SunManager.Instance?.DropSun(transform.position, zombieData.sunDropAmount);
            // }
        }
        
        #endregion
        
        #region Cleanup
        
        public void DestroyZombie()
        {
            Destroy(gameObject);
        }
        
        #endregion
        
        #region Public Methods
        
        public void SetZombieData(ZombieData data)
        {
            zombieData = data;
            InitializeZombie();
        }
        
        public void Initialize(ZombieData data)
        {
            zombieData = data;
            InitializeZombie();
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmosSelected()
        {
            if (zombieData == null) return;
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, zombieData.attackRange);
        }
        
        #endregion

        void OnValidate()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();

            if (col == null)
                col = GetComponent<Collider>();

            if (animator == null)
                animator = GetComponent<Animator>();

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
    }
}
