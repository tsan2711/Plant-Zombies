using UnityEngine;
using PvZ.Plants;
using PvZ.Managers;

namespace PvZ.Utils
{
    /// <summary>
    /// Simple validator to check PlantData setup
    /// </summary>
    public class PlantDataValidator : MonoBehaviour
    {
        [ContextMenu("Validate Plant Data")]
        public void ValidatePlantData()
        {
            var gameDataManager = GameManager.Instance?.GetGameData();
            if (gameDataManager == null)
            {
                Debug.LogError("GameManager.Instance or GameDataManager is null!");
                return;
            }
            
            var plants = gameDataManager.GetUnlockedPlants();
            Debug.Log($"Found {plants.Length} unlocked plants");
            
            foreach (var plant in plants)
            {
                if (plant == null)
                {
                    Debug.LogError("Found null PlantData in unlocked plants!");
                    continue;
                }
                
                string status = "✅";
                string issues = "";
                
                if (plant.prefab == null)
                {
                    status = "❌";
                    issues += " [No Prefab]";
                }
                
                if (string.IsNullOrEmpty(plant.displayName))
                {
                    status = "❌";
                    issues += " [No Display Name]";
                }
                
                if (plant.cost <= 0)
                {
                    status = "⚠️";
                    issues += " [Zero Cost]";
                }
                
                Debug.Log($"{status} {plant.displayName} (ID: {plant.plantID}, Cost: {plant.cost}){issues}");
            }
        }
        
        [ContextMenu("Test First Plant Creation")]
        public void TestFirstPlantCreation()
        {
            var gameDataManager = GameManager.Instance?.GetGameData();
            if (gameDataManager == null)
            {
                Debug.LogError("GameManager.Instance or GameDataManager is null!");
                return;
            }
            
            var plants = gameDataManager.GetUnlockedPlants();
            if (plants.Length == 0)
            {
                Debug.LogError("No unlocked plants found!");
                return;
            }
            
            var firstPlant = plants[0];
            Debug.Log($"Testing creation of: {firstPlant.displayName}");
            
            if (firstPlant.prefab == null)
            {
                Debug.LogError($"Cannot test {firstPlant.displayName}: prefab is null!");
                return;
            }
            
            // Test instantiation
            Vector3 testPos = transform.position + Vector3.up * 2f;
            GameObject testPlant = Instantiate(firstPlant.prefab, testPos, Quaternion.identity);
            
            if (testPlant != null)
            {
                Debug.Log($"✅ Successfully instantiated {firstPlant.displayName}");
                
                // Clean up after 2 seconds
                Destroy(testPlant, 2f);
            }
            else
            {
                Debug.LogError($"❌ Failed to instantiate {firstPlant.displayName}");
            }
        }
    }
}
