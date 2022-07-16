using System;
using System.Collections.Generic;
using _Scripts.ItemSystem;
using UnityEngine;

namespace _Scripts.InventorySystem
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private InventorySettings inventorySettings;
        [SerializeField] public List<Transform> slotLists = new List<Transform>(); 
        private Dictionary<ItemData, InventoryItem> _itemDictionary = new Dictionary<ItemData, InventoryItem>();
        private List<InventoryItem> _inventoryItems = new List<InventoryItem>();

        public bool isFull;
        public InventorySettings InventorySettings => inventorySettings;
        
        private bool _isAnyWeaponEquipped = false;
        
        public InventoryItem GetItem(int index)
        {
            return _inventoryItems[index];
        }
        
        public InventoryItem GetItem(ItemData data)
        {
            if (_itemDictionary.TryGetValue(data, out InventoryItem value))
            {
                return value;
            }

            return null;
        }
        
        public void CreateItemSlot(Transform spawnParent)
        {
            for (int i = 0; i < inventorySettings.InventorySize; i++)
            {
                var emptyGameobject = new GameObject("ItemSlot");
                GameObject go = Instantiate(emptyGameobject, spawnParent);
                go.name = "InventoryItemSlot" + i;
                slotLists.Add(go.transform);
            }
        }
        
        public void GetEmptyItemSlot(out Transform emptySlot)
        {
            emptySlot = null;
            foreach (var slot in slotLists)
            {
                if (slot.childCount == 0)
                {
                    emptySlot = slot;
                    break;
                }
            }
        }

        public void AddItemToInventory(ItemData data, GameObject go)
        {
            if(_itemDictionary.Count == inventorySettings.InventorySize) return;

            if (_itemDictionary.TryGetValue(data, out InventoryItem value))
            {
                value.AddToStack();
            }
            else
            {
                InventoryItem item = new InventoryItem(data, go);
                _itemDictionary.Add(data, item);
                _inventoryItems.Add(item);
            }
            
            if(!_isAnyWeaponEquipped)
                EquipWeapon(go);
            else
                UnequipWeapon(go);
                
        }

        public void RemoveItemFromInventory(ItemData data)
        {
            if(_itemDictionary.TryGetValue(data, out InventoryItem value))
            {
                value.RemoveFromStack();
                if (value.StackSize == 0)
                {
                    _inventoryItems.Remove(value);
                    _itemDictionary.Remove(data);
                }
            }

            if (_inventoryItems.Count == 0) _isAnyWeaponEquipped = false; //if there are no items in the inventory, then no weapon is equipped
            //ChangeWeapon(1);//if there are no items in the inventory, then the weapon is changed to the first one in the list
        }

        public void ChangeWeapon(int index)
        {
            if(_inventoryItems.Count == 0) return;
            
            index--;//Inputed index is 1 based, but array is 0 based.
            if (index < 0 || index > _inventoryItems.Count) return;
            
            foreach (InventoryItem item in _inventoryItems)
            {
                item.RuntimeItemReference.SetActive(false);
            }
            
            if(slotLists[index].childCount > 0 ) slotLists[index].GetChild(0).gameObject.SetActive(true);
        }
        
        public GameObject CurrentWeapon()
        {
            foreach (InventoryItem item in _inventoryItems)
            {
                if (item.RuntimeItemReference.activeSelf)
                {
                    return item.RuntimeItemReference;
                }
            }
            return null;
        }

        private void EquipWeapon(GameObject go)
        {
            go.SetActive(true);
            _isAnyWeaponEquipped = true;
        }
        
        private void UnequipWeapon(GameObject go)
        {
            go.SetActive(false);
            //_isAnyWeaponEquipped = false;
        }
    }
}
