using UnityEngine;

namespace razz
{
    [RequireComponent(typeof(VehicleController))]
    public class VehicleBasicInput : MonoBehaviour
    {
        private VehicleController _car;
        private Animator _vehicleAnimator;
        private float _h, _v, _handbrake;
        private bool _once;
        private bool _blocked;

        //This will get by Interactor effectors, since they already cached this input.
        [HideInInspector] public VehiclePartControls vehPartControl;

        private void Awake()
        {
            _car = GetComponent<VehicleController>();
            vehPartControl = GetComponent<VehiclePartControls>();
            _vehicleAnimator = GetComponent<Animator>();
        }

        //Called by BackDoor Animation Event
        public void SetBackDoorFalse()
        {
            _vehicleAnimator.SetBool("BackDoor", false);
            _once = false;
        }

        public void SetWindshield(bool value)
        {
            _vehicleAnimator.SetBool("Windshield", value);
        }

        public void Blocked(bool block)
        {
            _blocked = block;
        }

        private void Update()
        {
            _h = Input.GetAxis("Horizontal");
            _v = Input.GetAxis("Vertical");
            _handbrake = Input.GetAxis("Jump");

            //Opens backdoor for TruckExample scene
            if (Input.GetKeyDown(KeyCode.B) && !_once)
            {
                if (_blocked) return;

                _vehicleAnimator.SetBool("BackDoor", true);
                _once = true;
            }
        }

        private void FixedUpdate()
        {
            _car.Move(_h, _v, _v, _handbrake);

            _h = 0;
            _v = 0;
            _handbrake = 0;
        }

        private void OnDisable()
        {
            _car.Move(0, 0, 0, 0);
        }
    }
}
