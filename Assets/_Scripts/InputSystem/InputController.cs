using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.InputSystem
{
    public class InputController : MonoBehaviour, Controls.IPlayerActions
    {
        public event Action InteractEvent;
        public event Action DropEvent;
        public event Action<int> ChangeWeaponEvent; 

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
            if(!context.performed) return;
            InteractEvent?.Invoke();
        }

        public void OnDrop(InputAction.CallbackContext context)
        {
            if(!context.performed) return;
            DropEvent?.Invoke();
        }

        public void OnInventory1(InputAction.CallbackContext context)
        {
            if(!context.performed) return;
            int.TryParse(context.control.name, out var numKeyValue);
            ChangeWeaponEvent?.Invoke(numKeyValue);
        }
    }
}
