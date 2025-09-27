using UnityEngine;
using PvZ.Core;

namespace PvZ.Zombies
{
    [CreateAssetMenu(fileName = "New Zombie", menuName = "PvZ/Zombie Data")]
    public class ZombieData : ScriptableObject
    {
        [Header("Basic Info")]
        public ZombieID zombieID;
        public string displayName;
        public GameObject prefab;
        public Sprite icon;
        
        [Header("Stats")]
        public float health = 100f;
        public float moveSpeed = 1f;
        public float damage = 100f;
        
        [Header("Type")]
        public ZombieType zombieType = ZombieType.Basic;
        
        [Header("Spawning")]
        public int minWaveToAppear = 1;
        public bool isBossZombie = false;
    }
}
