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

    // Game Entity IDs - Chuyển từ string sang enum để tránh lỗi typo
    public enum AnimalID
    {
        None,
        Chicken,
        Duck,
        Goat,
        Lizard,
        Owl,
        Panda,
        Wolf,
    }

    public enum ZombieID
    {
        None,
        BasicZombie,
        FlagZombie,
        ConeheadZombie,
        PoleVaultingZombie,
        BucketheadZombie,
        NewspaperZombie,
        ScreenDoorZombie,
        FootballZombie,
        DancingZombie,
        BackupDancerZombie,
        DuckyTubeZombie,
        SnorkelZombie,
        ZomboniZombie,
        ZombieBobsled,
        DolphinRiderZombie,
        JackInTheBoxZombie,
        BalloonZombie,
        DiggerZombie,
        PogoZombie,
        YetiZombie,
        BungeeeZombie,
        LadderZombie,
        CatapultZombie,
        GargantuarZombie,
        ImpZombie,
        DrZomboss
    }

    public enum ProjectileID
    {
        None,
        Wind,
        SmallFire,
        SmallIce,
        SmallLight,
        Wave,
        Star,
        Coconut,
        Butter,
        Melon,
        WinterMelon,
        Cabbage,
        Kernel
    }

    public enum LevelID
    {
        None,
        Level1_1,
        Level1_2,
        Level1_3,
        Level1_4,
        Level1_5,
        Level1_6,
        Level1_7,
        Level1_8,
        Level1_9,
        Level1_10,
        Level2_1,
        Level2_2,
        Level2_3,
        Level2_4,
        Level2_5,
        Level2_6,
        Level2_7,
        Level2_8,
        Level2_9,
        Level2_10,
        Level3_1,
        Level3_2,
        Level3_3,
        Level3_4,
        Level3_5,
        Level3_6,
        Level3_7,
        Level3_8,
        Level3_9,
        Level3_10,
        Survival_Day,
        Survival_Night,
        Survival_Pool,
        Survival_Fog,
        Survival_Roof,
        Challenge_Vasebreaker,
        Challenge_IZombie,
        Challenge_LastStand
    }

    public enum EffectID
    {
        None,
        Damage,
        Heal,
        Slow,
        Freeze,
        Burn,
        Poison,
        Stun,
        Knockback,
        Pierce,
        Explosion,
        Shield,
        SpeedBoost,
        AttackBoost,
        SunBoost,
        InstantKill
    }

    public enum ModifierID
    {
        None,
        DoubleHealth,
        DoubleDamage,
        DoubleSpeed,
        DoubleSpawn,
        NoSun,
        FastZombies,
        SlowZombies,
        DarkLevel,
        RainLevel,
        WindyLevel
    }
}
