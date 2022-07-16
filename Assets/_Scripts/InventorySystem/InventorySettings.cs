using UnityEngine;

namespace _Scripts.InventorySystem
{
    [CreateAssetMenu(fileName = "Inventory Settings", menuName = "Inventory/InventorySettings")]
    public class InventorySettings : ScriptableObject
    {
        [SerializeField] private int _inventorySize = 4;
        
        public int InventorySize => _inventorySize;
    }
}