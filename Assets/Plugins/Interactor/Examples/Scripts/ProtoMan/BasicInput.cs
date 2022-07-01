using UnityEngine;

namespace razz
{
    [RequireComponent(typeof(PlayerController))]
    public class BasicInput : MonoBehaviour
    {
        private PlayerController _playerController;
        private Interactor _interactor;
        private Transform _camTransform;
        private Vector3 _camForward;
        private Vector3 _move;
        private bool _jump;

        public bool m_Climb;
        public bool m_Climbable;
        public bool m_Use;
        public bool m_Usable;
        public bool m_Changed;
        public bool m_Changable;
        public bool onVehicle;
        public bool m_Click;

        private void Start()
        {
            if (Camera.main != null)
            {
                _camTransform = Camera.main.transform;
            }
            _playerController = GetComponent<PlayerController>();
            _interactor = FindObjectOfType<Interactor>();
        }

        private void Update()
        {
            if (!_jump)
            {
                _jump = Input.GetButtonDown("Jump");
            }

            if (!m_Use)
            {
                m_Use = Input.GetKeyDown(KeyCode.F);

                if (m_Use && (PlayerState.singlePlayerState.playerChangable || PlayerState.singlePlayerState.playerClimable))
                {
                    if (!m_Climb && PlayerState.singlePlayerState.playerClimable)
                    {
                        m_Use = false;
                        PlayerState.singlePlayerState.playerClimable = false;

                        if (!PlayerState.singlePlayerState.playerGrounded && !PlayerState.singlePlayerState.playerClimbing) return;

                        m_Climb = true;
                    }
                    else
                    {
                        m_Changed = true;
                        m_Use = false;
                    }
                }
            }

            if (!m_Click && PlayerState.singlePlayerState.playerUsable)
            {
                m_Click = Input.GetKeyDown(KeyCode.Mouse0);

                if (m_Click && !PlayerState.singlePlayerState.playerUsing)
                {
                    PlayerState.singlePlayerState.playerUsing = true;
                }
            }

            //Press T to stop all interactions
            if (Input.GetKeyDown(KeyCode.T))
            {
                _interactor.DisconnectAll();
                Debug.Log("Stopped all interactions.");
            }

            //Press Y to move player upwards if its stuck
            if (Input.GetKeyDown(KeyCode.Y))
            {
                GetComponent<PlayerController>().Dash();
            }
        }

        private void FixedUpdate()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);

            if (PlayerState.singlePlayerState.playerClimbing || PlayerState.singlePlayerState.playerPushing)
            {
                h = 0;
                v = Mathf.Clamp01(v);
            }

            if (_camTransform != null)
            {
                _camForward = Vector3.Scale(_camTransform.forward, new Vector3(1, 0, 1)).normalized;
                _move = v * _camForward + h * _camTransform.right;
            }
            else
            {
                _move = v * Vector3.forward + h * Vector3.right;
            }
            
            if (Input.GetKey(KeyCode.LeftShift) || PlayerState.singlePlayerState.playerClimbing)
            {
                _move *= 2f;
            }
            else if (PlayerState.singlePlayerState.playerPushing)
            {
                _move *= 0.4f;
            }
            else
            {
                _move *= 0.5f;
            }

            if (m_Climb || m_Changed || PlayerState.singlePlayerState.playerClimbed)
            {
                _interactor.StartStopInteractions(false);
                PlayerState.singlePlayerState.playerClimbed = false;
            }

            if (m_Use)
            {
                _interactor.StartStopInteractions(false);
            }

            if (m_Click)
            {
                _interactor.StartStopInteractions(true);
            }

            if (PlayerState.singlePlayerState.playerOnVehicle)
            {
                _playerController.Move(Vector3.zero, false, false, false, false, m_Changed, false);
            }
            else
            {
                _playerController.Move(_move, crouch, _jump, m_Climb, m_Use, m_Changed, m_Click);
            }
            
            _jump = false;
            m_Climb = false;
            m_Use = false;
            m_Changed = false;
            m_Click = false;
        }
    }
}
