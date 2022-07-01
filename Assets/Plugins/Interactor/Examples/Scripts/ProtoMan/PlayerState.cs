using UnityEngine;

namespace razz
{
    public class PlayerState : MonoBehaviour
    {
        public static PlayerState singlePlayerState;

        public GameObject playerObject;
        public bool playerClimbing;
        public bool playerClimable;
        public bool playerClimbed;
        public bool playerChanging;
        public bool playerChangable;
        public bool playerOnVehicle;
        public bool playerUsing;
        public bool playerUsable;
        public bool playerCrouching;
        public bool playerIdle;
        public bool playerMoving;
        public bool playerGrounded;
        public bool playerPushing;
        public bool rePos;
        public Vector3 targetPosition;
        public Quaternion targetRotation;
        public Vector3 targetTopPosition;
        public Quaternion targetTopRotation;

        [HideInInspector] public Transform playerTransform;
        [HideInInspector] public Rigidbody playerRigidbody;
        [HideInInspector] public Transform vehicleTransform;
        [HideInInspector] public Vector3 vehiclePosition;
        [HideInInspector] public Quaternion vehicleRotation;
        [HideInInspector] public CapsuleCollider playerCollider;
        [HideInInspector] public PlayerController playerController;
        [HideInInspector] public BasicInput playerBasicInput;
        [HideInInspector] public VehicleBasicInput vehicleBasicInput;
        [HideInInspector] public VehicleController vehicleController;
        [HideInInspector] public BikeBasicInput bikeBasicInput;
        [HideInInspector] public BikeController bikeController;
        [HideInInspector] public Rigidbody vehicleRigidbody;
        [HideInInspector] public GameObject enteredVehicle;

        private void Awake()
        {
            if (!singlePlayerState) singlePlayerState = this;
            else if (singlePlayerState != this) Destroy(gameObject);

            if (!(playerController = playerObject.GetComponent<PlayerController>()))
            {
                Debug.Log("No playerObject.GetComponent<PlayerController>");
                return;
            }

            if (!(playerBasicInput = playerObject.GetComponent<BasicInput>()))
            {
                Debug.Log("No playerObject.GetComponent<BasicInput>");
                return;
            }

            if (!(playerTransform = playerObject.transform))
            {
                Debug.Log("No playerObject");
                return;
            }

            if (!(playerRigidbody = playerObject.GetComponent<Rigidbody>()))
            {
                Debug.Log("No playerObject.GetComponent<Rigidbody>");
                return;
            }

            if (!(playerCollider = playerObject.GetComponent<CapsuleCollider>()))
            {
                Debug.Log("No playerObject.GetComponent<Collider>");
                return;
            }
        }
    }
}
