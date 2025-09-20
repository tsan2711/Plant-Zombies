using UnityEngine;

namespace PvZ.Core
{
    /// <summary>
    /// Interface cơ bản cho tất cả entities trong game
    /// </summary>
    public interface IEntity
    {
        string ID { get; }
        float Health { get; set; }
        float MaxHealth { get; }
        Vector3 Position { get; set; }
        bool IsActive { get; set; }
        
        void TakeDamage(float damage, IDamageDealer dealer);
        void Die();
    }
}
