using UnityEngine;

namespace _Scripts.ItemSystem
{
    public class ItemSettings : ScriptableObject
    {
        [SerializeField] private string itemName;
        [SerializeField] private string itemDescription;

        public string ItemName => itemName;
        public string ItemDescription => itemDescription;
    }
}
