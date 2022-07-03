using UnityEngine;

namespace _Scripts.PickupSystem
{
    public interface IInteractable
    {
        void Interact();
        void EndInteraction();
    }
}
