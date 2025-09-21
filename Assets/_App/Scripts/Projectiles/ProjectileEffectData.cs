using UnityEngine;
using PvZ.Core;

namespace PvZ.Projectiles
{
    [CreateAssetMenu(fileName = "New Projectile Effect", menuName = "PvZ/Projectile Effect")]
    public class ProjectileEffectData : ScriptableObject
    {
        [Header("Basic Info")]
        public EffectID effectID;
        public string displayName;
        
        [Header("Effect Properties")]
        public ProjectileEffectType effectType;
        public float value;
    }
    
    public enum ProjectileEffectType
    {
        Damage,
        Slow,
        Freeze
    }
}
