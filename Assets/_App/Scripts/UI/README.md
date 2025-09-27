# Plant Selection UI System

Hệ thống UI hoàn chỉnh để chọn plant và quản lý sun resource cho Plants vs Zombies game.

## 📋 **Tổng quan Components**

### **1. PlantButton.cs**
- UI button cho từng plant
- Hiển thị icon, cost, cooldown
- Visual feedback (available, selected, cooldown)
- Tự động cập nhật trạng thái dựa trên sun

### **2. SunManager.cs**
- Quản lý sun resource
- Hiển thị UI sun counter
- Thu thập sun từ sunflower hoặc sky
- Tích hợp với GameManager

### **3. PlantSelectionUI.cs**
- Controller chính cho plant selection
- Tạo plant buttons từ GameData
- Quản lý plant selection
- Kết nối với PlantPlacementController

## 🚀 **Cách Setup UI**

### **Bước 1: Tạo UI Canvas**
```
1. Tạo Canvas (Screen Space - Overlay)
2. Thêm CanvasScaler với Reference Resolution 1920x1080
3. Tạo UI structure:
   Canvas
   ├── SunDisplay
   │   ├── SunIcon (Image)
   │   └── SunText (TextMeshPro)
   └── PlantSelection
       ├── Background (Image)
       └── PlantContainer (Horizontal Layout Group)
```

### **Bước 2: Setup SunManager**
```csharp
1. Tạo GameObject "SunManager"
2. Attach script SunManager
3. Assign:
   - Sun Text: TextMeshPro hiển thị số sun
   - Sun Animator: (optional) cho animation
   - Sun Pickup Prefab: (optional) prefab cho sun rơi
   - Audio clips cho collect/spend sounds
```

### **Bước 3: Setup PlantSelectionUI**
```csharp
1. Tạo GameObject "PlantSelectionUI"
2. Attach script PlantSelectionUI
3. Assign:
   - Plant Button Container: Transform chứa buttons
   - Plant Button Prefab: Prefab cho plant button
   - Sun Manager: Reference to SunManager
   - Auto Load Plants From Game Data: true
```

### **Bước 4: Tạo PlantButton Prefab**
```
1. Tạo Button trong PlantContainer
2. Structure:
   PlantButton
   ├── Background (Image)
   ├── PlantIcon (Image)
   ├── CostText (TextMeshPro)
   ├── CooldownOverlay (Image - Fill type)
   └── CooldownText (TextMeshPro)
3. Attach script PlantButton
4. Tạo Prefab từ button này
5. Delete original button khỏi scene
6. Assign prefab vào PlantSelectionUI
```

## 🎮 **Cách sử dụng**

### **Auto Setup:**
```csharp
// UI sẽ tự động:
1. Load plants từ GameDataManager.GetUnlockedPlants()
2. Tạo buttons cho mỗi plant
3. Sync sun với GameManager
4. Update button states theo sun amount
```

### **Manual Control:**
```csharp
// Chọn plant bằng code
PlantSelectionUI.Instance.SelectPlantByIndex(0);
PlantSelectionUI.Instance.SelectPlantByData(plantData);

// Quản lý sun
SunManager.Instance.AddSun(50);
SunManager.Instance.SpendSun(100);
SunManager.Instance.GenerateSunFromSunflower(position);

// Events
PlantSelectionUI.Instance.OnPlantSelected += (plantData) => {
    Debug.Log($"Selected: {plantData.displayName}");
};
```

## 🔄 **Integration với Placement System**

### **Automatic Integration:**
1. **UI → Placement**: Khi click plant button → PlantPlacementController.SelectPlant()
2. **Placement → UI**: Khi place plant → UI.OnPlantPlaced() → spend sun + start cooldown
3. **Preview**: Preview object lấy từ plant đang selected trong UI

### **Flow hoàn chỉnh:**
```
1. Player click plant button
2. UI check sun → enable/disable button
3. UI notify PlantPlacementController
4. PlacementController tạo preview từ selected plant
5. Player click ground → place plant
6. UI spend sun và start cooldown
7. Button disabled cho đến hết cooldown
```

## 🎨 **Visual States**

### **Plant Button States:**
- **Available**: Trắng, có thể click
- **Selected**: Xanh lá, đang được chọn
- **Cannot Afford**: Xám, không đủ sun
- **Cooldown**: Đỏ, đang cooldown với timer

### **Sun Display:**
- **Add Sun**: Animation scale up
- **Spend Sun**: Animation shake
- **Color**: Đỏ nếu không đủ tiền cho plant selected

## 📝 **Dependencies**

### **Required:**
- `TextMeshPro` package
- `GameDataManager` với plant data
- `GameManager` với sun system
- `PlantPlacementController`

### **Optional:**
- Audio clips cho feedback
- Animations cho UI elements
- Sun pickup prefabs

## 🔧 **Configuration**

### **PlantSelectionUI Settings:**
```csharp
[SerializeField] private bool autoLoadPlantsFromGameData = true;
[SerializeField] private List<PlantData> availablePlants; // Manual override
```

### **SunManager Settings:**
```csharp
[SerializeField] private int sunFromSunflower = 25;
[SerializeField] private int sunFromSky = 50;
[SerializeField] private float sunCollectionSpeed = 5f;
```

### **PlantButton Settings:**
```csharp
[SerializeField] private Color availableColor = Color.white;
[SerializeField] private Color selectedColor = Color.green;
[SerializeField] private Color unavailableColor = Color.gray;
[SerializeField] private Color cooldownColor = Color.red;
```

## 🚨 **Troubleshooting**

### **Common Issues:**

1. **"No plants available"**
   - Check GameDataManager có plant data
   - Check autoLoadPlantsFromGameData = true
   - Manually assign plants nếu cần

2. **Buttons không respond**
   - Check Button component có EventSystem
   - Check GraphicRaycaster trên Canvas
   - Check Button.interactable = true

3. **Sun không sync**
   - Check SunManager.Instance không null
   - Check GameManager.Instance connection
   - Check OnSunChanged events

4. **Preview không hiển thị**
   - Check plant đã được selected
   - Check PlantData.prefab không null
   - Check PlantPlacementController reference

## 📱 **Mobile Support**

UI system hỗ trợ touch input:
- Touch plant button = mouse click
- Touch ground = mouse click placement
- Responsive UI với CanvasScaler

Hệ thống UI này hoàn toàn tự động và chỉ cần setup một lần!
