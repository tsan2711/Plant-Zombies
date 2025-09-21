using UnityEngine;
using PvZ.Core;

namespace PvZ.Projectiles
{
    [CreateAssetMenu(fileName = "New Projectile", menuName = "PvZ/Projectile Data")]
    public class ProjectileData : ScriptableObject
    {
        [Header("Basic Info")]
        public ProjectileID projectileID;
        public string displayName;
        public GameObject prefab;
        
        [Header("Movement")]
        public float speed;
        public float lifeTime;
        
        [Header("Combat")]
        public float damage;
        public bool destroyOnHit = true;
    }
}
