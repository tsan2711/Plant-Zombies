using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using PvZ.Core;

namespace PvZ.Plants
{
    public class PlantTargetingSystem : MonoBehaviour, ITargetingSystem
    {
        [SerializeField] private LayerMask zombieLayerMask = -1;
        [SerializeField] private TargetingStrategy targetingStrategy = TargetingStrategy.Closest;
        
        public enum TargetingStrategy
        {
            Closest,
            Farthest,
            HighestHealth,
            LowestHealth,
            HighestPriority
        }
        
        public ITargetable FindBestTarget(Vector3 position, float range, Func<ITargetable, bool> filter = null)
        {
            var targets = FindAllTargetsInRange(position, range, filter);
            
            if (targets == null || targets.Length == 0)
                return null;
            
            return SelectBestTarget(targets, position);
        }
        
        public ITargetable[] FindAllTargetsInRange(Vector3 position, float range, Func<ITargetable, bool> filter = null)
        {
            var colliders = Physics2D.OverlapCircleAll(position, range, zombieLayerMask);
            var validTargets = new List<ITargetable>();
            
            foreach (var collider in colliders)
            {
                var targetable = collider.GetComponent<ITargetable>();
                
                if (targetable != null && targetable.IsValidTarget())
                {
                    // Apply custom filter if provided
                    if (filter == null || filter(targetable))
                    {
                        validTargets.Add(targetable);
                    }
                }
            }
            
            return validTargets.ToArray();
        }
        
        private ITargetable SelectBestTarget(ITargetable[] targets, Vector3 position)
        {
            switch (targetingStrategy)
            {
                case TargetingStrategy.Closest:
                    return targets.OrderBy(t => Vector3.Distance(position, t.GetTargetPosition())).First();
                
                case TargetingStrategy.Farthest:
                    return targets.OrderByDescending(t => Vector3.Distance(position, t.GetTargetPosition())).First();
                
                case TargetingStrategy.HighestHealth:
                    return targets.OrderByDescending(t => GetTargetHealth(t)).First();
                
                case TargetingStrategy.LowestHealth:
                    return targets.OrderBy(t => GetTargetHealth(t)).First();
                
                case TargetingStrategy.HighestPriority:
                    return targets.OrderByDescending(t => t.GetPriority()).First();
                
                default:
                    return targets[0];
            }
        }
        
        private float GetTargetHealth(ITargetable target)
        {
            var entity = target as IEntity;
            return entity?.Health ?? 0f;
        }
        
        public void SetTargetingStrategy(TargetingStrategy strategy)
        {
            targetingStrategy = strategy;
        }
        
        public TargetingStrategy GetTargetingStrategy()
        {
            return targetingStrategy;
        }
        
        #region Debug Visualization
        
        private void OnDrawGizmosSelected()
        {
            // Draw targeting range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, GetComponent<PlantController>()?.GetPlantData()?.range ?? 5f);
        }
        
        #endregion
    }
}
