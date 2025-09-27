# Plant Placement System

Hệ thống trồng cây hoàn chỉnh cho Plants vs Zombies game. Bao gồm các thành phần chính:

## Cấu trúc System

### 1. GroundPlantContainer
- **Mục đích**: Script gắn vào từng ô đất trong game
- **Chức năng**:
  - Xử lý click chuột để trồng cây
  - Quản lý trạng thái ô đất (trống/đã có cây)
  - Hiển thị feedback visual khi hover
  - Kiểm tra điều kiện trồng cây

### 2. PlantPlacementController
- **Mục đích**: Quản lý toàn bộ quá trình placement
- **Chức năng**:
  - Detect chuột trên màn hình
  - Quản lý mode placement
  - Hiển thị preview cây khi placement
  - Xử lý input keyboard/mouse
  - Quản lý cây được chọn

### 3. Existing Map Support
- **Mục đích**: Hỗ trợ map có sẵn
- **Chức năng**:
  - Mỗi ground object gắn GroundPlantContainer
  - Raycast detect script trên objects
  - Không cần tạo grid tự động
  - Linh hoạt với bất kỳ layout nào

## Cách Setup

### Bước 1: Setup Ground Objects
1. Với mỗi ground tile trong map có sẵn:
2. Attach script `GroundPlantContainer`
3. Cấu hình:
   - `Grid Position`: Vị trí logic (x, y)
   - `Can Plant Here`: true
   - `Allowed Plant Types`: { PlantType.Shooter }
   - Đảm bảo có Collider để raycast detect

### Bước 2: Setup PlantPlacementController
1. Tạo empty GameObject tên "PlantPlacementController"
2. Attach script `PlantPlacementController`
3. Cấu hình:
   - `Raycast Distance`: 100
   - `Use Placement Mode`: true
   - `Toggle Placement Key`: Space (hoặc key khác)
   - `Cancel Placement Key`: Escape (hoặc key khác)
   - Gán audio clips cho feedback

### Bước 3: Setup UIAdapter
1. Tạo GameObject "UIAdapter"
2. Attach script `UIAdapter`
3. Cấu hình:
   - `Plant Selection UI`: Assign PlantSelectionUI MonoBehaviour
   - `Sun Manager`: Assign SunManager MonoBehaviour
   - Hoặc để trống để auto-find

### Bước 4: Cập nhật GameManager
1. Trong `GameManager`, assign:
   - `Plant Placement Controller`
2. Không cần GridManager nữa

## Cách Sử dụng

### Cho Designer:
```csharp
// Chọn cây để trồng
GameManager.Instance.SelectPlantForPlacement(plantData);

// Kiểm tra có thể trồng không
bool canPlace = GameManager.Instance.CanAffordPlant(plantData.cost);

// Trồng cây tại vị trí cụ thể
GameManager.Instance.TryPlacePlant(plantData, new Vector2Int(2, 3));
```

### Keyboard Controls (Configurable):
- `Toggle Placement Key` (default Space): Toggle placement mode
- `Cancel Placement Key` (default Escape): Cancel placement
- `1-9`: Chọn cây nhanh (index 0-8)
- `Left Click`: Trồng cây
- `Right Click`: Cancel placement

### Mouse Controls:
- Hover: Hiển thị highlight trên ground tile
- Click: Trồng cây tại vị trí click

## Integration với UI

Để tích hợp với UI system:

```csharp
public class PlantSelectionUI : MonoBehaviour
{
    public void OnPlantButtonClicked(PlantData plantData)
    {
        // Chọn cây và bắt đầu placement mode
        GameManager.Instance.SelectPlantForPlacement(plantData);
    }
}
```

## Events

System hỗ trợ các events:
- `OnPlantSelected`: Khi chọn cây
- `OnPlantPlaced`: Khi trồng cây thành công
- `OnPlacementModeChanged`: Khi thay đổi placement mode

## Debug Features

- Grid visualization với Gizmos
- Console logging cho placement actions
- Context menu functions trong Editor

## Notes

- Script sử dụng Unity Input System mới (InputSystem package)
- Cần có GameDataManager để lấy danh sách plants
- Resource system (sun) được tích hợp sẵn
- Hỗ trợ multiple plant types (Normal, Aquatic, Defensive)
