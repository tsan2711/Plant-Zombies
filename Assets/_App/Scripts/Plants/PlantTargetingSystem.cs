using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using PvZ.Core;

namespace PvZ.Plants
{
    [RequireComponent(typeof(Collider))]
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
            var colliders = Physics.OverlapSphere(position, range, zombieLayerMask);
            var validTargets = new List<ITargetable>();

            // Get plant data for detection settings
            var plantController = GetComponent<PlantController>();
            var plantData = plantController?.GetPlantData();
            int detectionRows = plantData?.detectionRows ?? 1;

            foreach (var collider in colliders)
            {
                var targetable = collider.GetComponent<ITargetable>();

                // if (targetable != null && targetable.IsValidTarget())
                // {
                // Check if target is within detection rows
                if (!IsWithinDetectionRows(position, targetable.GetTargetPosition(), detectionRows))
                    continue;

                // Apply custom filter if provided
                if (filter == null || filter(targetable))
                {
                    validTargets.Add(targetable);
                }
                // }
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

        private bool IsWithinDetectionRows(Vector3 plantPosition, Vector3 targetPosition, int detectionRows)
        {
            // Assuming rows are aligned along Z-axis (forward/backward)
            // You may need to adjust this based on your game's coordinate system
            float rowHeight = 2f; // Adjust this value based on your row spacing

            float plantRow = Mathf.RoundToInt(plantPosition.z / rowHeight);
            float targetRow = Mathf.RoundToInt(targetPosition.z / rowHeight);

            float rowDifference = Mathf.Abs(plantRow - targetRow);

            // detectionRows = 1 means detect current row + 1 row before and after (total 3 rows)
            // detectionRows = 2 means detect current row + 2 rows before and after (total 5 rows)
            return rowDifference <= detectionRows;
        }

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            var plantController = GetComponent<PlantController>();
            var plantData = plantController?.GetPlantData();

            if (plantData == null) return;

            // Draw targeting range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, plantData.range);

            // Draw detection rows
            if (plantData.detectionRows > 0)
            {
                Gizmos.color = Color.yellow;
                float rowHeight = 2f; // Same as in IsWithinDetectionRows

                // Draw current row (center)
                Vector3 currentRowCenter = transform.position;
                Gizmos.DrawWireCube(currentRowCenter, new Vector3(plantData.range * 2, 0.1f, rowHeight));

                // Draw detection rows before and after
                for (int i = 1; i <= plantData.detectionRows; i++)
                {
                    // Row in front
                    Vector3 frontRowCenter = transform.position + Vector3.forward * (i * rowHeight);
                    Gizmos.DrawWireCube(frontRowCenter, new Vector3(plantData.range * 2, 0.1f, rowHeight));

                    // Row behind
                    Vector3 backRowCenter = transform.position - Vector3.forward * (i * rowHeight);
                    Gizmos.DrawWireCube(backRowCenter, new Vector3(plantData.range * 2, 0.1f, rowHeight));
                }
            }
        }

        #endregion

    }
}
