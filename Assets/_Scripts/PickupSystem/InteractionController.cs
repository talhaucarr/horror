using System;
using UnityEngine;
using _Scripts.FieldOfView;
using _Scripts.InputSystem;
using _Scripts.InventorySystem;
using _Scripts.ItemSystem;
using Random = UnityEngine.Random;

namespace _Scripts.PickupSystem
{
    public class InteractionController : MonoBehaviour
    {
        [BHeader("Input Controller")] 
        [SerializeField] private InputController inputController;
        [BHeader("Interact Settings")]
        [SerializeField] private Transform spawnPosition;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform cameraRef;//TODO kamera referansını düzgün refactor at.
        
        [Range(0,25)] 
        [SerializeField] private float pickupRange;
        [SerializeField] private LayerMask layer;
        
        
        private InventoryController _inventoryController;
        private GameObject _go;

        private void Start()
        {
            _inventoryController = GetComponent<InventoryController>();
            CreateItemSlot();
            
            inputController.DropEvent += Drop;
            inputController.InteractEvent += Pickup;
            inputController.ChangeWeaponEvent += ChangeWeapon;
        }

        private void FixedUpdate()
        {
            RaycastHit HitInfo;
            
            if (Physics.Raycast(cameraRef.position, cameraRef.forward, out HitInfo, pickupRange, layer))
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
        
        private void CreateItemSlot()
        {
            _inventoryController.CreateItemSlot(spawnPosition);
        }

        private void Pickup()
        {
            if(_go == null) return;
            if (!_go.transform.gameObject.TryGetComponent<Item>(out var pickupable)) return;
            
            _inventoryController.GetEmptyItemSlot(out var emptySlot);
            pickupable.Pickup(emptySlot);
            _inventoryController.AddItemToInventory(pickupable.ItemData, _go);
        }

        private void Drop()
        {
            var currentWeapon = _inventoryController.CurrentWeapon();
            
            if(currentWeapon == null) return;
            
            if (!currentWeapon.TryGetComponent<Item>(out var pickupable)) return;
            
            pickupable.Drop(rb.velocity, cameraRef);
            _inventoryController.RemoveItemFromInventory(pickupable.ItemData);
        }

        private void ChangeWeapon(int keyCode)
        {
            _inventoryController.ChangeWeapon(keyCode);
        }
    }
}
