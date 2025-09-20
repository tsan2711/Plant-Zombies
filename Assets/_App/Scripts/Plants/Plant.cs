using UnityEngine;
using System.Collections.Generic;
using PvZ.Core;
using PvZ.Projectiles;
using PvZ.Managers;

namespace PvZ.Plants
{
    public class PlantController : MonoBehaviour, IEntity, IProjectileLauncher, IDamageDealer
    {
        [Header("Plant Configuration")]
        [SerializeField] private PlantData plantData;
        
        [Header("Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Transform[] launchPoints;
        
        // IEntity Properties
        public string ID => plantData.plantID;
        public float Health { get; set; }
        public float MaxHealth => plantData.health;
        public Vector3 Position { get; set; }
        public bool IsActive { get; set; }
        
        // IProjectileLauncher Properties  
        public bool CanLaunch => Time.time >= lastAttackTime + (1f / plantData.attackSpeed);
        public float LaunchCooldown => 1f / plantData.attackSpeed;
        
        // IDamageDealer Properties
        public float Damage => plantData.damage;
        public DamageType DamageType => DamageType.Normal;
        IEntity IDamageDealer.Owner => this;
        
        // Private Fields
        private float lastAttackTime;
        private ITargetingSystem targetingSystem;
        private List<PlantAbility> activeAbilities;
        private EntityManager entityManager;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            InitializeAbilities();
        }
        
        private void Start()
        {
            InitializePlant();
            RegisterWithManagers();
        }
        
        private void Update()
        {
            if (!IsActive) return;
            
            UpdateTargeting();
            UpdateAbilities();
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
            
            if (launchPoints == null || launchPoints.Length == 0)
            {
                // Tạo launch point mặc định nếu không có
                GameObject launchPoint = new GameObject("LaunchPoint");
                launchPoint.transform.SetParent(transform);
                launchPoint.transform.localPosition = Vector3.right;
                launchPoints = new Transform[] { launchPoint.transform };
            }
            
            targetingSystem = GetComponent<ITargetingSystem>() ?? gameObject.AddComponent<PlantTargetingSystem>();
        }
        
        private void InitializeAbilities()
        {
            activeAbilities = new List<PlantAbility>();
            
            if (plantData.abilities != null)
            {
                foreach (var abilityData in plantData.abilities)
                {
                    var ability = new PlantAbility(abilityData, this);
                    activeAbilities.Add(ability);
                }
            }
        }
        
        private void InitializePlant()
        {
            Health = MaxHealth;
            Position = transform.position;
            IsActive = true;
            
            // Set animator controller
            if (animator != null && plantData.animatorController != null)
            {
                animator.runtimeAnimatorController = plantData.animatorController;
            }
            
            // Play plant sound
            PlaySound(plantData.plantSound);
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
        
        #region Combat System
        
        private void UpdateTargeting()
        {
            if (!CanLaunch || plantData.projectileData == null) return;
            
            var target = FindTarget();
            if (target != null)
            {
                AttackTarget(target);
            }
        }
        
        private ITargetable FindTarget()
        {
            return targetingSystem.FindBestTarget(transform.position, plantData.range, 
                target => IsValidTarget(target));
        }
        
        private bool IsValidTarget(ITargetable target)
        {
            // Check if target is in valid layer
            var targetEntity = target as MonoBehaviour;
            if (targetEntity == null) return false;
            
            // Check ground/air attack capabilities
            bool isGroundTarget = targetEntity.CompareTag("GroundZombie");
            bool isAirTarget = targetEntity.CompareTag("AirZombie");
            
            return (plantData.canAttackGround && isGroundTarget) || 
                   (plantData.canAttackAir && isAirTarget);
        }
        
        private void AttackTarget(ITargetable target)
        {
            Vector3 targetPosition = target.GetTargetPosition();
            Vector3 direction = (targetPosition - transform.position).normalized;
            
            // Launch projectiles
            for (int i = 0; i < plantData.projectileCount; i++)
            {
                Vector3 launchDirection = CalculateLaunchDirection(direction, i);
                Transform launchPoint = GetLaunchPoint(i);
                
                LaunchProjectile(plantData.projectileData, launchPoint.position, launchDirection);
            }
            
            lastAttackTime = Time.time;
            
            // Play attack animation and sound
            PlayAttackAnimation();
            PlaySound(plantData.attackSound);
        }
        
        private Vector3 CalculateLaunchDirection(Vector3 baseDirection, int projectileIndex)
        {
            if (plantData.projectileCount == 1) return baseDirection;
            
            float spreadAngle = plantData.projectileSpread;
            float angleStep = spreadAngle / (plantData.projectileCount - 1);
            float currentAngle = -spreadAngle * 0.5f + angleStep * projectileIndex;
            
            return Quaternion.AngleAxis(currentAngle, Vector3.up) * baseDirection;
        }
        
        private Transform GetLaunchPoint(int index)
        {
            if (launchPoints == null || launchPoints.Length == 0)
                return transform;
            
            return launchPoints[index % launchPoints.Length];
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
            
            // Trigger on death abilities
            TriggerAbilities(AbilityTriggerType.OnDeath);
            
            // Play death effects
            PlaySound(plantData.destroySound);
            
            // Start destruction sequence
            StartCoroutine(DeathSequence());
        }
        
        private System.Collections.IEnumerator DeathSequence()
        {
            // Play death animation
            if (animator != null)
            {
                animator.SetTrigger("Die");
                yield return new WaitForSeconds(1f); // Wait for animation
            }
            
            // Destroy the plant
            Destroy(gameObject);
        }
        
        #endregion
        
        #region IProjectileLauncher Implementation
        
        public void LaunchProjectile(ProjectileData projectileData, Vector3 startPosition, Vector3 direction)
        {
            if (ProjectilePool.Instance != null)
            {
                var projectile = ProjectilePool.Instance.GetProjectile(projectileData);
                if (projectile != null)
                {
                    projectile.Initialize(projectileData, startPosition, direction, this);
                }
            }
        }
        
        #endregion
        
        #region Abilities System
        
        private void UpdateAbilities()
        {
            foreach (var ability in activeAbilities)
            {
                ability.Update();
            }
        }
        
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
        
        #region Animation & Audio
        
        private void PlayAttackAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        public void SetPlantData(PlantData data)
        {
            plantData = data;
            InitializePlant();
        }
        
        public PlantData GetPlantData()
        {
            return plantData;
        }
        
        #endregion
    }
}
