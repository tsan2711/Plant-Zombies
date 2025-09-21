using UnityEngine;
using PvZ.Core;

namespace PvZ.Projectiles
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(AudioSource))]
    public class ProjectileController : MonoBehaviour, IDamageDealer
    {
        [Header("Components")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Collider col;
        [SerializeField] private AudioSource audioSource;
        
        // IDamageDealer Properties
        public float Damage => projectileData?.damage ?? 0f;
        public DamageType DamageType => DamageType.Normal;
        public IEntity Owner { get; private set; }
        
        // Public Properties
        public ProjectileData ProjectileData => projectileData;
        public Vector3 Direction { get; private set; }
        public bool IsActive { get; private set; }
        
        // Private Fields
        private ProjectileData projectileData;
        private float launchTime;
        
        #region Initialization
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            
            if (col == null)
                col = GetComponent<Collider>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
        
        public void Initialize(ProjectileData data, Vector3 startPosition, Vector3 direction, IEntity owner)
        {
            projectileData = data;
            Direction = direction.normalized;
            Owner = owner;
            
            transform.position = startPosition;
            launchTime = Time.time;
            IsActive = true;
            
            SetupMovement();
        }
        
        private void SetupMovement()
        {
            if (rb != null)
            {
                rb.useGravity = false;
                rb.linearVelocity = Direction * projectileData.speed;
            }
            
            // Rotate projectile to face the direction it's moving
            if (Direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(Direction);
            }
        }
        
        #endregion
        
        #region Update Loop
        
        private void Update()
        {
            if (!IsActive) return;
            
            CheckLifetime();
        }
        
        private void CheckLifetime()
        {
            if (Time.time - launchTime >= projectileData.lifeTime)
            {
                DestroyProjectile();
            }
        }
        
        #endregion
        
        #region Collision
        
        private void OnTriggerEnter(Collider other)
        {
            if (!IsActive) return;
            
            IEntity target = other.GetComponent<IEntity>();
            if (target != null && target != Owner)
            {
                target.TakeDamage(Damage, this);
                
                if (projectileData.destroyOnHit)
                {
                    DestroyProjectile();
                }
            }
        }
        
        #endregion
        
        #region Destruction
        
        private void DestroyProjectile()
        {
            IsActive = false;
            
            // Return to pool if using ProjectilePool
            if (ProjectilePool.Instance != null)
            {
                ProjectilePool.Instance.ReturnProjectile(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        public void ResetProjectile()
        {
            IsActive = false;
            projectileData = null;
            Owner = null;
            Direction = Vector3.zero;
            
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }

        #endregion

        void OnValidate()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();

            if (col == null)
                col = GetComponent<Collider>();

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
    }
}