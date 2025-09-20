using UnityEngine;
using PvZ.Core;

namespace PvZ.Projectiles
{
    [CreateAssetMenu(fileName = "New Projectile Effect", menuName = "PvZ/Projectile Effect")]
    public class ProjectileEffectData : ScriptableObject
    {
        [Header("Basic Info")]
        public string effectID;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Effect Properties")]
        public ProjectileEffectType effectType;
        public float value;
        public float duration;
        public float range;
        
        [Header("Visual & Audio")]
        public GameObject effectPrefab;
        public AudioClip soundEffect;
        public Color effectColor = Color.white;
        
        [Header("Conditions")]
        public bool requiresTarget = true;
        public string[] validTargetTags = { "Zombie", "Plant" };
    }
    
    public enum ProjectileEffectType
    {
        Damage,
        Heal,
        Slow,
        Freeze,
        Burn,
        Poison,
        Stun,
        Knockback,
        Pierce,
        Explosion
    }
}
