using System;
using UnityEngine;
using _Scripts.FieldOfView;
using Random = UnityEngine.Random;

namespace _Scripts.PickupSystem
{
    public class CharacterPickupController : MonoBehaviour
    {
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

        private void FixedUpdate()
        {
            RaycastHit HitInfo;

            if (Input.GetKey(KeyCode.I))//TODO yeni input sisteme bağla
            {
                if (go.TryGetComponent<IPickupable>(out var pickupable))
                {
                    pickupable.Drop(rb.velocity, cameraRef);
                }
            }

            if (Physics.Raycast(cameraRef.position, cameraRef.forward, out HitInfo, 10, layer))
            {
                go = HitInfo.transform.gameObject;
                if (Input.GetKey(KeyCode.H))
                {
                    if (HitInfo.transform.gameObject.TryGetComponent<IPickupable>(out var pickupable))
                    {
                        pickupable.Pickup(spawnPosition);
                        
                    }
                }
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
            
        }

        private void Drop()
        {
            
        }
    }
}
