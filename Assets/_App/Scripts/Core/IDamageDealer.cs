namespace PvZ.Core
{
    /// <summary>
    /// Interface cho entities có thể gây damage
    /// </summary>
    public interface IDamageDealer
    {
        float Damage { get; }
        DamageType DamageType { get; }
        IEntity Owner { get; }
    }
}
