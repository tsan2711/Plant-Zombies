using UnityEngine;
using PvZ.Projectiles;

namespace PvZ.Core
{
    /// <summary>
    /// Interface cho entities có thể bắn projectile
    /// </summary>
    public interface IProjectileLauncher
    {
        void LaunchProjectile(ProjectileData projectileData, Vector3 startPosition, Vector3 direction);
        bool CanLaunch { get; }
        float LaunchCooldown { get; }
    }
}
