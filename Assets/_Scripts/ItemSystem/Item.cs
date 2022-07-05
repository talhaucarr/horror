using System;
using _Scripts.PickupSystem;
using EPOOutline;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.ItemSystem
{
    public abstract class Item : MonoBehaviour, IInteractable, IPickupable
    {
        [BHeader("Item Settings")]
        [SerializeField] private ItemSettings itemSettings;

        [BHeader("Outline")] 
        [SerializeField] private Outlinable outline;
        [SerializeField] private Color outlineColor;
        
        [BHeader("Components")]
        [SerializeField] protected Rigidbody rb;
        [SerializeField] protected Collider itemCollider;

        public ItemSettings ItemSettings => itemSettings;

        private void Start()
        {
            
        }

        protected virtual void EnableOutline()
        {
            if (outline)
            {
                outline.enabled = true;
                outline.OutlineParameters.Color = outlineColor;
            }
                
        }

        protected virtual void DisableOutline()
        {
            if (outline)
                outline.enabled = false;
        }

        public void Interact()
        {
            EnableOutline();
        }

        public void EndInteraction()
        {
            DisableOutline();
        }

        public virtual void Pickup(Transform spawnPosition)
        {
            itemCollider.enabled = false;
            Transform itemTransform;
            (itemTransform = transform).SetParent(spawnPosition);
            itemTransform.localPosition = Vector3.zero;
            itemTransform.localRotation = Quaternion.identity;
            rb.isKinematic = true;
        }

        public virtual void Drop(Vector3 velocity, Transform cameraTransform)
        {
            itemCollider.enabled = true;
            transform.SetParent(null);
            rb.isKinematic = false;
            rb.velocity = velocity;
            rb.AddForce(cameraTransform.forward * 2, ForceMode.Impulse);
            rb.AddForce(cameraTransform.up * 2, ForceMode.Impulse);
            float random = Random.Range(-1f, 1f);
            rb.AddTorque(new Vector3(random, random, random) * 10);
        }
        
    }
}
