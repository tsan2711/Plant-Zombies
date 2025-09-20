using UnityEngine;

namespace PvZ.Core
{
    /// <summary>
    /// Interface cho entities có thể bị target
    /// </summary>
    public interface ITargetable
    {
        Vector3 GetTargetPosition();
        bool IsValidTarget();
        float GetPriority();
    }
}
