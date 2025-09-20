using UnityEngine;
using System;

namespace PvZ.Core
{
    /// <summary>
    /// Interface cho hệ thống targeting
    /// </summary>
    public interface ITargetingSystem
    {
        ITargetable FindBestTarget(Vector3 position, float range, Func<ITargetable, bool> filter = null);
        ITargetable[] FindAllTargetsInRange(Vector3 position, float range, Func<ITargetable, bool> filter = null);
    }
}
