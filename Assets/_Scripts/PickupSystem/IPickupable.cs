using UnityEngine;

namespace _Scripts.PickupSystem
{
    public interface IPickupable
    {
        void Pickup(Transform spawnPosition);
        void Drop(Vector3 velocity, Transform cameraTransform);
    }
}
