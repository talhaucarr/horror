using System;
using UnityEngine;
using _Scripts.FieldOfView;
using _Scripts.InputSystem;
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

        private FieldOfViewModule _fov;
        private bool _isTaken;
        private GameObject go;

        private void Start()
        {
            inputController.DropEvent += Drop;
            inputController.InteractEvent += Pickup;
        }

        private void FixedUpdate()
        {
            RaycastHit HitInfo;

            

            if (Physics.Raycast(cameraRef.position, cameraRef.forward, out HitInfo, 10, layer))
            {
                go = HitInfo.transform.gameObject;
                go.GetComponent<IInteractable>().Interact();
                Debug.DrawRay(cameraRef.position, cameraRef.forward * 100.0f, Color.yellow);
            }
            else
            {
                if(go != null)
                    go.GetComponent<IInteractable>().EndInteraction();
            }
            
        }

        private void Pickup()
        {
            
            if (go.transform.gameObject.TryGetComponent<IPickupable>(out var pickupable))
            {
                pickupable.Pickup(spawnPosition);
                        
            }
        }

        private void Drop()
        {
            if (go.TryGetComponent<IPickupable>(out var pickupable))
            {
                pickupable.Drop(rb.velocity, cameraRef);
            }
        }
    }
}
