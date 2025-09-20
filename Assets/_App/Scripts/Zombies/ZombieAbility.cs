using UnityEngine;
using PvZ.Core;

namespace PvZ.Zombies
{
    public class ZombieAbility
    {
        public ZombieAbilityData Data { get; private set; }
        public ZombieController Owner { get; private set; }
        
        private float lastActivationTime;
        private float activationEndTime;
        private bool isActive;
        
        public bool IsOnCooldown => Time.time < lastActivationTime + Data.cooldown;
        public bool IsActive => isActive && Time.time < activationEndTime;
        public float CooldownRemaining => Mathf.Max(0, lastActivationTime + Data.cooldown - Time.time);
        
        public ZombieAbility(ZombieAbilityData data, ZombieController owner)
        {
            Data = data;
            Owner = owner;
            lastActivationTime = -data.cooldown; // Allow immediate use
        }
        
        public void Update()
        {
            // Check for trigger conditions
            CheckTriggerConditions();
            
            // Update active effects
            if (IsActive)
            {
                UpdateActiveEffects();
            }
            else if (isActive)
            {
                // Ability just ended
                EndAbility();
            }
        }
        
        public bool CanActivate()
        {
            return !IsOnCooldown && Owner.IsActive;
        }
        
        public void Activate()
        {
            if (!CanActivate()) return;
            
            lastActivationTime = Time.time;
            activationEndTime = Time.time + Data.duration;
            isActive = true;
            
            StartAbility();
        }
        
        private void CheckTriggerConditions()
        {
            if (Data.triggerType == AbilityTriggerType.OnInterval && CanActivate())
            {
                Activate();
            }
        }
        
        private void StartAbility()
        {
            // Apply instant effects
            ApplyEffects();
            
            // Spawn visual effects
            if (Data.effectPrefab != null)
            {
                GameObject effect = Object.Instantiate(Data.effectPrefab, Owner.transform.position, Quaternion.identity);
                if (Data.duration > 0)
                {
                    Object.Destroy(effect, Data.duration);
                }
            }
            
            // Play sound effect
            if (Data.soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(Data.soundEffect, Owner.transform.position);
            }
        }
        
        private void UpdateActiveEffects()
        {
            // Update continuous effects during ability duration
            foreach (var effect in Data.effects)
            {
                ApplyEffect(effect);
            }
        }
        
        private void EndAbility()
        {
            isActive = false;
            // Clean up any lingering effects
        }
        
        private void ApplyEffects()
        {
            foreach (var effect in Data.effects)
            {
                ApplyEffect(effect);
            }
        }
        
        private void ApplyEffect(ZombieAbilityEffectData effect)
        {
            switch (effect.effectID.ToLower())
            {
                case "heal":
                    ApplyHealEffect(effect);
                    break;
                case "damage":
                    ApplyDamageEffect(effect);
                    break;
                case "speedboost":
                    ApplySpeedBoostEffect(effect);
                    break;
                case "summon":
                    ApplySummonEffect(effect);
                    break;
                case "rage":
                    ApplyRageEffect(effect);
                    break;
                default:
                    Debug.LogWarning($"Unknown effect type: {effect.effectID}");
                    break;
            }
        }
        
        private void ApplyHealEffect(ZombieAbilityEffectData effect)
        {
            if (effect.affectsSelf)
            {
                float newHealth = Owner.Health + effect.value;
                Owner.Health = Mathf.Min(newHealth, Owner.MaxHealth);
            }
            
            if (effect.affectsAllies)
            {
                // Find and heal nearby zombies
                var nearbyZombies = FindNearbyZombies(effect.range);
                foreach (var zombie in nearbyZombies)
                {
                    float newHealth = zombie.Health + effect.value;
                    zombie.Health = Mathf.Min(newHealth, zombie.MaxHealth);
                }
            }
        }
        
        private void ApplyDamageEffect(ZombieAbilityEffectData effect)
        {
            if (effect.affectsEnemies)
            {
                // Find and damage nearby plants
                var nearbyPlants = FindNearbyPlants(effect.range);
                foreach (var plant in nearbyPlants)
                {
                    plant.TakeDamage(effect.value, Owner);
                }
            }
        }
        
        private void ApplySpeedBoostEffect(ZombieAbilityEffectData effect)
        {
            // This would require a buff/debuff system to implement properly
            Debug.Log($"Speed boost effect applied to zombie: {effect.value}");
        }
        
        private void ApplySummonEffect(ZombieAbilityEffectData effect)
        {
            // Summon additional zombies
            Debug.Log($"Summon effect triggered: {effect.value} zombies");
            // Implementation would depend on zombie spawning system
        }
        
        private void ApplyRageEffect(ZombieAbilityEffectData effect)
        {
            // Increase damage and speed temporarily
            Debug.Log($"Rage effect applied: {effect.value}x multiplier");
        }
        
        private ZombieController[] FindNearbyZombies(float range)
        {
            var colliders = Physics2D.OverlapCircleAll(Owner.transform.position, range);
            var zombies = new System.Collections.Generic.List<ZombieController>();
            
            foreach (var collider in colliders)
            {
                var zombie = collider.GetComponent<ZombieController>();
                if (zombie != null && zombie != Owner)
                {
                    zombies.Add(zombie);
                }
            }
            
            return zombies.ToArray();
        }
        
        private IEntity[] FindNearbyPlants(float range)
        {
            var colliders = Physics2D.OverlapCircleAll(Owner.transform.position, range);
            var plants = new System.Collections.Generic.List<IEntity>();
            
            foreach (var collider in colliders)
            {
                var plant = collider.GetComponent<IEntity>();
                if (plant != null && collider.CompareTag("Plant"))
                {
                    plants.Add(plant);
                }
            }
            
            return plants.ToArray();
        }
    }
}
