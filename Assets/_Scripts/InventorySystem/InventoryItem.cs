using System;
using _Scripts.ItemSystem;
using UnityEngine;

namespace _Scripts.InventorySystem
{
    [Serializable]
    public class InventoryItem
    {
        public GameObject RuntimeItemReference;
        public ItemData ItemData { get; private set; }
        public int StackSize { get; private set; }

        public InventoryItem(ItemData source, GameObject runtimeItemReference)
        {
            ItemData = source;
            RuntimeItemReference = runtimeItemReference;
            AddToStack();
        }

        public void AddToStack()
        {
            StackSize++;
        }

        public void RemoveFromStack()
        {
            StackSize--;
        }
        
        public void Use()
        {
            if (StackSize > 1)
            {
                RemoveFromStack();
            }
            else
            {
                ItemData = null;
            }
        }
        
        public void Use(int amount)
        {
            if (StackSize > amount)
            {
                StackSize -= amount;
            }
            else
            {
                ItemData = null;
            }
        }
    }
}
