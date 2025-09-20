namespace PvZ.Core
{
    public enum DamageType
    {
        Normal,
        Fire,
        Ice,
        Explosive,
        Poison,
        Electric
    }

    public enum PlantType
    {
        Shooter,
        Defensive,
        Support,
        Explosive,
        Melee,
        Special
    }

    public enum ZombieType
    {
        Basic,
        Armored,
        Fast,
        Flying,
        Giant,
        Special
    }

    public enum ProjectileMovementType
    {
        Straight,
        Arc,
        Homing,
        Lobbed,
        Boomerang
    }

    public enum ZombieMovementType
    {
        Walking,
        Running,
        Flying,
        Underground,
        Jumping
    }

    public enum ZombieState
    {
        Walking,
        Eating,
        Attacking,
        Dying,
        Special
    }

    public enum AbilityTriggerType
    {
        OnSpawn,
        OnDeath,
        OnDamage,
        OnInterval,
        OnManualActivation
    }
}
