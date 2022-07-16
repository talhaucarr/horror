using System;
using UnityEngine;
using _Scripts.FieldOfView;
using _Scripts.InputSystem;
using _Scripts.InventorySystem;
using _Scripts.ItemSystem;
using Random = UnityEngine.Random;

namespace _Scripts.PickupSystem
{
    public class CharacterPickupController : MonoBehaviour
    {
        [BHeader("Input Controller")] 
        [SerializeField] private InputController inputController;
        [BHeader("Pickup Settings")]
        [SerializeField] private Transform spawnPosition;
        [Range(0,5)] 
        [SerializeField] private float pickupRange;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private LayerMask layer;
        [SerializeField] private Transform cameraRef;//TODO kamera referansını düzgün refactor at.
        
        private InventoryController _inventoryController;
        private GameObject _go;
        private GameObject _pickedItem;

        private void Start()
        {
            _inventoryController = GetComponent<InventoryController>();
            
            inputController.DropEvent += Drop;
            inputController.InteractEvent += Pickup;
            inputController.ChangeWeaponEvent += ChangeWeapon;
        }

        private void FixedUpdate()
        {
            RaycastHit HitInfo;
            
            if (Physics.Raycast(cameraRef.position, cameraRef.forward, out HitInfo, 10, layer))
            {
                _go = HitInfo.transform.gameObject;
                _go.GetComponent<IInteractable>().Interact();
                Debug.DrawRay(cameraRef.position, cameraRef.forward * 100.0f, Color.yellow);
            }
            else
            {
                if (_go != null)
                {
                    _go.GetComponent<IInteractable>().EndInteraction();
                    _go = null;
                }
            }
        }

        private void Pickup()
        {
            if (_go.transform.gameObject.TryGetComponent<Item>(out var pickupable))
            {
                if(_inventoryController.isFull) _go.SetActive(false);
                pickupable.Pickup(spawnPosition);
                _inventoryController.AddItemToInventory(pickupable.ItemData, _go);
            }
        }

        private void Drop()
        {
            var currentWeapon = _inventoryController.CurrentWeapon();
            if (currentWeapon.TryGetComponent<Item>(out var pickupable))
            {
                pickupable.Drop(rb.velocity, cameraRef);
                _inventoryController.RemoveItemFromInventory(pickupable.ItemData);
            }
        }

        private void ChangeWeapon(int keyCode)
        {
            _inventoryController.ChangeWeapon(keyCode);
        }
    }
}
