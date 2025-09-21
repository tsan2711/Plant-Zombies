using UnityEngine;
using PvZ.Core;

namespace PvZ.Plants
{
    public class PlantAbility
    {
        public PlantAbilityData Data { get; private set; }
        public PlantController Owner { get; private set; }
        
        private float lastActivationTime;
        private float activationEndTime;
        private bool isActive;
        
        public bool IsOnCooldown => Time.time < lastActivationTime + Data.cooldown;
        public bool IsActive => isActive && Time.time < activationEndTime;
        public float CooldownRemaining => Mathf.Max(0, lastActivationTime + Data.cooldown - Time.time);
        
        public PlantAbility(PlantAbilityData data, PlantController owner)
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
        
        private void ApplyEffect(AbilityEffectData effect)
        {
            switch (effect.effectID)
            {
                case EffectID.Heal:
                    ApplyHealEffect(effect);
                    break;
                case EffectID.Damage:
                    ApplyDamageEffect(effect);
                    break;
                case EffectID.SpeedBoost:
                    ApplySpeedBoostEffect(effect);
                    break;
                case EffectID.Shield:
                    ApplyShieldEffect(effect);
                    break;
                default:
                    Debug.LogWarning($"Unknown effect type: {effect.effectID}");
                    break;
            }
        }
        
        private void ApplyHealEffect(AbilityEffectData effect)
        {
            if (effect.affectsSelf)
            {
                float newHealth = Owner.Health + effect.value;
                Owner.Health = Mathf.Min(newHealth, Owner.MaxHealth);
            }
            
            if (effect.affectsAllies)
            {
                // Find and heal nearby allies
                var nearbyPlants = FindNearbyPlants(effect.range);
                foreach (var plant in nearbyPlants)
                {
                    float newHealth = plant.Health + effect.value;
                    plant.Health = Mathf.Min(newHealth, plant.MaxHealth);
                }
            }
        }
        
        private void ApplyDamageEffect(AbilityEffectData effect)
        {
            if (effect.affectsEnemies)
            {
                // Find and damage nearby enemies
                var nearbyEnemies = FindNearbyZombies(effect.range);
                foreach (var zombie in nearbyEnemies)
                {
                    zombie.TakeDamage(effect.value, Owner as IDamageDealer);
                }
            }
        }
        
        private void ApplySpeedBoostEffect(AbilityEffectData effect)
        {
            // This would require a buff/debuff system to implement properly
            Debug.Log($"Speed boost effect applied: {effect.value}");
        }
        
        private void ApplyShieldEffect(AbilityEffectData effect)
        {
            // This would require a shield/protection system
            Debug.Log($"Shield effect applied: {effect.value}");
        }
        
        private PlantController[] FindNearbyPlants(float range)
        {
            var colliders = Physics2D.OverlapCircleAll(Owner.transform.position, range);
            var plants = new System.Collections.Generic.List<PlantController>();
            
            foreach (var collider in colliders)
            {
                var plant = collider.GetComponent<PlantController>();
                if (plant != null && plant != Owner)
                {
                    plants.Add(plant);
                }
            }
            
            return plants.ToArray();
        }
        
        private IEntity[] FindNearbyZombies(float range)
        {
            var colliders = Physics2D.OverlapCircleAll(Owner.transform.position, range);
            var zombies = new System.Collections.Generic.List<IEntity>();
            
            foreach (var collider in colliders)
            {
                var zombie = collider.GetComponent<IEntity>();
                if (zombie != null && collider.CompareTag("Zombie"))
                {
                    zombies.Add(zombie);
                }
            }
            
            return zombies.ToArray();
        }
    }
}
