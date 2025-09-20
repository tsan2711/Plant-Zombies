# Plants vs Zombies - Data-Driven Architecture

## Tổng quan hệ thống

Hệ thống Plants vs Zombies này được thiết kế theo kiến trúc data-driven với loose coupling thông qua interfaces, sử dụng Unity ScriptableObjects để quản lý dữ liệu và các pattern hiện đại.

## Cấu trúc thư mục

```
_App/Scripts/
├── Core/                     # Interfaces cơ bản và enums
│   ├── IEntity.cs           # Interface chính cho tất cả entities
│   ├── IDamageDealer.cs     # Interface cho entities gây damage
│   ├── IProjectileLauncher.cs # Interface cho entities bắn projectile
│   ├── ITargetable.cs       # Interface cho entities có thể bị target
│   ├── ITargetingSystem.cs  # Interface cho hệ thống targeting
│   └── GameEnums.cs         # Tất cả enums của game
├── Plants/                   # Hệ thống Plants
│   ├── PlantData.cs         # ScriptableObject cho plant data
│   ├── PlantController.cs   # Controller chính cho plants (thay thế Plant.cs)
│   ├── PlantAbilityData.cs  # ScriptableObject cho plant abilities
│   ├── PlantAbility.cs      # Runtime logic cho abilities
│   └── PlantTargetingSystem.cs # Targeting system cho plants
├── Zombies/                  # Hệ thống Zombies
│   ├── ZombieData.cs        # ScriptableObject cho zombie data
│   ├── ZombieController.cs  # Controller chính cho zombies
│   ├── ZombieAbilityData.cs # ScriptableObject cho zombie abilities
│   ├── ZombieAbility.cs     # Runtime logic cho zombie abilities
│   ├── ZombieArmorData.cs   # ScriptableObject cho zombie armor
│   └── ZombieStateMachine.cs # State machine cho zombie AI
├── Projectiles/              # Hệ thống Projectiles
│   ├── ProjectileData.cs    # ScriptableObject cho projectile data
│   ├── ProjectileController.cs # Controller chính cho projectiles
│   ├── ProjectileEffectData.cs # ScriptableObject cho projectile effects
│   └── ProjectilePool.cs    # Object pooling cho projectiles
├── Managers/                 # Hệ thống Managers
│   ├── GameDataManager.cs   # Quản lý tất cả ScriptableObject data
│   ├── EntityManager.cs     # Quản lý tất cả entities trong scene
│   └── GameManager.cs       # Manager chính của game
├── Events/                   # Hệ thống Events
│   ├── GameEvent.cs         # ScriptableObject event system
│   ├── GameEventListener.cs # Component để listen events
│   ├── ParameterizedGameEvent.cs # Events với parameters
│   └── GameEventManager.cs  # Manager cho common events
├── Level/                    # Hệ thống Level và Wave
│   ├── LevelConfiguration.cs # ScriptableObject cho level config
│   └── WaveData.cs          # ScriptableObject cho wave data
└── Factory/                  # Factory Pattern
    └── EntityFactory.cs     # Factory để tạo tất cả entities
```

## Kiến trúc chính

### 1. Core Interfaces
- **IEntity**: Interface cơ bản cho tất cả entities (Health, Position, TakeDamage, Die)
- **IDamageDealer**: Interface cho entities có thể gây damage
- **IProjectileLauncher**: Interface cho entities có thể bắn projectile
- **ITargetable**: Interface cho entities có thể bị target

### 2. Data-Driven Design
Tất cả game data được lưu trong ScriptableObjects:
- `PlantData`: Stats, abilities, projectiles của plants
- `ZombieData`: Stats, armor, abilities của zombies
- `ProjectileData`: Movement, damage, effects của projectiles
- `LevelConfiguration`: Waves, available plants, modifiers
- `WaveData`: Zombie spawns, timing, events

### 3. Loose Coupling
- Các hệ thống giao tiếp qua interfaces
- Event system cho decoupled communication
- Factory pattern cho entity creation
- Manager pattern cho centralized control

### 4. Performance Optimization
- Object pooling cho projectiles
- Efficient entity management
- Optimized targeting system
- Cached lookups trong GameDataManager

## Cách sử dụng

### Tạo Plant mới
1. Tạo PlantData ScriptableObject: `Right Click > Create > PvZ > Plant Data`
2. Cấu hình stats, abilities, projectiles
3. Assign prefab với PlantController component
4. Add vào GameDataManager.allPlants array

### Tạo Zombie mới
1. Tạo ZombieData ScriptableObject: `Right Click > Create > PvZ > Zombie Data`
2. Cấu hình stats, armor, abilities, movement
3. Assign prefab với ZombieController component
4. Add vào GameDataManager.allZombies array

### Tạo Projectile mới
1. Tạo ProjectileData ScriptableObject: `Right Click > Create > PvZ > Projectile Data`
2. Cấu hình movement, damage, effects
3. Assign prefab với ProjectileController component
4. Add vào GameDataManager.allProjectiles array

### Tạo Level mới
1. Tạo LevelConfiguration: `Right Click > Create > PvZ > Level > Level Configuration`
2. Tạo WaveData cho từng wave: `Right Click > Create > PvZ > Level > Wave Data`
3. Cấu hình available plants, wave progression, victory conditions

### Sử dụng Factory Pattern
```csharp
// Tạo plant
var plant = EntityFactory.CreatePlant("peashooter", position);

// Tạo zombie
var zombie = EntityFactory.CreateZombie("basic_zombie", position);

// Tạo projectile (thường được gọi tự động từ plants)
var projectile = EntityFactory.CreateProjectile("pea", position, direction, owner);
```

### Sử dụng Event System
```csharp
// Raise events
GameEventManager.Instance.RaisePlantPlanted(plant);
GameEventManager.Instance.RaiseZombieKilled(zombie);

// Listen to events với GameEventListener component trên GameObject
// Hoặc subscribe trực tiếp trong code
```

## Extension Points

### Custom Plant Behaviors
Implement `IPlantBehavior` để tạo behaviors đặc biệt:
```csharp
public interface IPlantBehavior
{
    void OnPlantSpawned(PlantController plant);
    void OnPlantUpdate(PlantController plant);
    void OnPlantDestroyed(PlantController plant);
}
```

### Custom Zombie Behaviors
Extend `ZombieStateMachine` với custom states:
```csharp
public class CustomZombieState : IZombieState
{
    public void Enter(ZombieController zombie) { }
    public void Update(ZombieController zombie) { }
    public void Exit(ZombieController zombie) { }
}
```

### Custom Projectile Effects
Tạo custom effects bằng cách extend `ProjectileEffectData`:
```csharp
[CreateAssetMenu(fileName = "Custom Effect", menuName = "PvZ/Custom Projectile Effect")]
public class CustomEffectData : ProjectileEffectData
{
    // Custom effect properties
}
```

## Debug Features

### GameDataManager
- Context menu: "Rebuild Lookups", "Validate All Data", "Print Statistics"
- Runtime validation của tất cả data

### EntityManager
- Real-time entity tracking
- Debug GUI showing entity counts
- Spatial queries cho targeting

### ProjectilePool
- Pool statistics và reuse rates
- Debug GUI showing pool status
- Context menu để clear pools

### Event System
- Event history logging
- Debug validation của event assignments
- Test methods cho tất cả events

## Best Practices

1. **Data Organization**: Tạo folders riêng cho mỗi loại ScriptableObject
2. **Naming Convention**: Sử dụng consistent naming cho IDs (e.g., "peashooter", "basic_zombie")
3. **Performance**: Sử dụng object pooling cho frequently spawned objects
4. **Testing**: Sử dụng debug methods và context menus để test features
5. **Validation**: Luôn validate data trước khi build

## Lợi ích của Architecture này

1. **Maintainable**: Code tách biệt rõ ràng theo chức năng
2. **Extensible**: Dễ dàng thêm plants/zombies/projectiles mới
3. **Data-Driven**: Designers có thể tạo content mà không cần code
4. **Performance**: Object pooling và optimized systems
5. **Debuggable**: Comprehensive debug tools và logging
6. **Scalable**: Architecture hỗ trợ projects lớn

## Missing Managers

Một số managers chưa được implement nhưng có thể thêm vào dễ dàng:
- `SunManager`: Quản lý sun currency
- `ScoreManager`: Quản lý scoring system
- `AudioManager`: Quản lý audio centralized
- `UIManager`: Quản lý UI elements
- `SaveManager`: Lưu/load game progress

Các managers này có thể được tạo theo pattern tương tự như EntityManager và GameManager.
