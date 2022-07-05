using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.InputSystem
{
    public class InputController : MonoBehaviour, Controls.IPlayerActions
    {
        public event Action InteractEvent;
        public event Action DropEvent;
        
        
        private Controls _controls;
        
        private void Start()
        {
            _controls = new Controls();
            _controls.Player.SetCallbacks(this);
            _controls.Player.Enable();
        }

        private void OnDestroy()
        {
            _controls.Player.Disable();
        }
        
        public void OnInteract(InputAction.CallbackContext context)
        {
            Debug.Log($"AGA ");
            if(!context.performed) return;
            InteractEvent?.Invoke();
        }

        public void OnDrop(InputAction.CallbackContext context)
        {
            if(!context.performed) return;
            DropEvent?.Invoke();
        }
    }
}
