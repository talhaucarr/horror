using UnityEngine;

namespace razz
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Animator _animator;
        private PlayerState _stateRef;
        private float _defaultGroundCheckDistance;
        private const float _half = 0.5f;
        private float _turnAmount;
        private float _forwardAmount;
        private Vector3 _groundNormal;
        private float _capsuleHeight;
        private Vector3 _capsuleCenter;
        private CapsuleCollider _capsuleCol;
        private float _climbPos;
        private Ray _testHeightRay;
        private float _testHeightFloat;
        
        [SerializeField] private LayerMask m_raycastLayerMaskforRagdoll;
        [SerializeField] private float m_MovingTurnSpeed = 360;
        [SerializeField] private float m_StationaryTurnSpeed = 180;
        [SerializeField] private float m_JumpPower = 12f;
        [Range(1f, 4f)] [SerializeField] private float m_GravityMultiplier = 2f;
        [SerializeField] private float m_RunCycleLegOffset = 0.2f;
        [SerializeField] private float m_GroundCheckDistance = 0.1f;

        public float moveSpeedMultiplier = 1f;
        public float animSpeedMultiplier = 1f;
        public float climbSpeed = 1f;
        public float climbMax = 3f;
        public float charScaleY = 1f;
        public bool debugHeight;
        
        void Start()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCol = GetComponent<CapsuleCollider>();
            _capsuleHeight = _capsuleCol.height;
            _capsuleCenter = _capsuleCol.center;

            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            _defaultGroundCheckDistance = m_GroundCheckDistance;

            _stateRef = PlayerState.singlePlayerState;
        }

        public void Move(Vector3 move, bool crouch, bool jump)
        {
            if (move.magnitude > 1f) move.Normalize();
            move = transform.InverseTransformDirection(move);
            CheckGroundStatus();
            move = Vector3.ProjectOnPlane(move, _groundNormal);
            _turnAmount = Mathf.Atan2(move.x, move.z);
            _forwardAmount = move.z;

            ApplyExtraTurnRotation();

            if (_stateRef.playerGrounded)
            {
                HandleGroundedMovement(crouch, jump);
            }
            else
            {
                HandleAirborneMovement();
            }

            ScaleCapsuleForCrouching(crouch);
            PreventStandingInLowHeadroom();
            UpdateAnimator(move);
        }

        public void Move(Vector3 move, bool crouch, bool jump, bool climb, bool use, bool changed, bool clicked)
        {
            if (move.magnitude > 1f) move.Normalize();

            move = transform.InverseTransformDirection(move);
            CheckGroundStatus();
            move = Vector3.ProjectOnPlane(move, _groundNormal);

            _turnAmount = Mathf.Atan2(move.x, move.z);
            _forwardAmount = move.z;

            if (_forwardAmount != 0)
                _stateRef.playerMoving = true;
            else
                _stateRef.playerMoving = false;

            if (move.magnitude == 0 && !crouch && !jump && !climb && !use && _stateRef.playerGrounded && !clicked)
                _stateRef.playerIdle = true;
            else
                _stateRef.playerIdle = false;

            ApplyExtraTurnRotation();

            if (_stateRef.playerGrounded)
                HandleGroundedMovement(crouch, jump);
            else if (!_stateRef.playerClimbing)
                HandleAirborneMovement();

            ScaleCapsuleForCrouching(crouch);
            PreventStandingInLowHeadroom();

            if (changed)
            {
                _stateRef.playerChanging = true;
                ControllerChange();
            }
            else
                _stateRef.playerChanging = false;

            if (climb && !crouch && !_stateRef.playerClimbing)
            {
                _stateRef.playerClimbing = true;
                _climbPos = _rigidbody.position.y;
                _rigidbody.useGravity = false;
                _stateRef.playerGrounded = false;
                _animator.applyRootMotion = false;
            }
            else if (climb && _stateRef.playerClimbing)
            {
                _stateRef.playerClimbing = false;
                _rigidbody.useGravity = true;
                _animator.applyRootMotion = true;
            }

            Climber();
            UpdateAnimator(move);
        }

        void Climber()
        {
            if (!_stateRef.playerClimbing) return;

            if (PlayerState.singlePlayerState.rePos)
            {
                Vector3 playerPos;
                playerPos.x = _rigidbody.position.x;
                playerPos.y = _rigidbody.position.y;
                playerPos.z = _rigidbody.position.z;
                PlayerState.singlePlayerState.targetPosition.y = playerPos.y;

                Quaternion playerRot = _rigidbody.rotation;

                if (Mathf.Abs(playerPos.x - PlayerState.singlePlayerState.targetPosition.x) > 0.01f || Mathf.Abs(playerPos.z - PlayerState.singlePlayerState.targetPosition.z) > 0.01f)
                {
                    playerPos.x = Mathf.MoveTowards(_rigidbody.position.x, PlayerState.singlePlayerState.targetPosition.x, Time.fixedDeltaTime * 0.1f);
                    playerPos.z = Mathf.MoveTowards(_rigidbody.position.z, PlayerState.singlePlayerState.targetPosition.z, Time.fixedDeltaTime * 0.1f);

                    playerRot = Quaternion.RotateTowards(playerRot, PlayerState.singlePlayerState.targetRotation, Time.fixedDeltaTime * 2f);

                    if (_forwardAmount > 0)
                    {
                        _rigidbody.position = playerPos;
                        _rigidbody.rotation = playerRot;
                    }
                }
                else
                {
                    PlayerState.singlePlayerState.rePos = false;
                }
            }

            if (_rigidbody.position.y + 0.75f > PlayerState.singlePlayerState.targetTopPosition.y)
            {
                _rigidbody.velocity = Vector3.zero;

                Vector3 playerPos;
                playerPos.x = _rigidbody.position.x;
                playerPos.y = _rigidbody.position.y;
                playerPos.z = _rigidbody.position.z;

                Quaternion playerRot = _rigidbody.rotation;

                if (Mathf.Abs(playerPos.x - PlayerState.singlePlayerState.targetTopPosition.x) > 0.1f || Mathf.Abs(playerPos.z - PlayerState.singlePlayerState.targetTopPosition.z) > 0.1f)
                {
                    float moveForwardSpeed = 1f;
                    playerPos.x = Mathf.MoveTowards(_rigidbody.position.x, PlayerState.singlePlayerState.targetTopPosition.x, Time.fixedDeltaTime * moveForwardSpeed);
                    playerPos.z = Mathf.MoveTowards(_rigidbody.position.z, PlayerState.singlePlayerState.targetTopPosition.z, Time.fixedDeltaTime * moveForwardSpeed);

                    playerRot = Quaternion.RotateTowards(playerRot, PlayerState.singlePlayerState.targetTopRotation, Time.fixedDeltaTime * 2f);

                    if (_forwardAmount > 0)
                    {
                        _rigidbody.position = playerPos;
                        _rigidbody.rotation = playerRot;
                    }
                }
                else
                {
                    
                    PlayerState.singlePlayerState.playerClimbed = true;
                    _animator.SetBool("Climb", false);
                    _rigidbody.useGravity = true;
                    _stateRef.playerClimbing = false;
                    _animator.applyRootMotion = true;
                }
            }
            else if (_rigidbody.position.y < _climbPos)
            {
                _rigidbody.useGravity = true;
                _stateRef.playerClimbing = false;
                _animator.applyRootMotion = true;
            }
            else 
                _rigidbody.velocity = new Vector3(0, climbSpeed * _forwardAmount, 0);
        }

        public void Dash()
        {
            _rigidbody.AddForce(10000f, 15000f, 0);
        }

        void ControllerChange()
        {
            if (_stateRef.playerOnVehicle)
                ExitVehicle();
            else
                EnterVehicle();
        }

        public void EnterVehicle()
        {
            VehicleController _cc;
            VehicleBasicInput _vehicleinput;
            BikeController _bc;
            BikeBasicInput _bikeinput;
            Rigidbody _rb;

            if (_cc = _stateRef.enteredVehicle.GetComponent<VehicleController>())
            {
                _vehicleinput = _stateRef.enteredVehicle.GetComponent<VehicleBasicInput>();
                _rb = _stateRef.enteredVehicle.GetComponent<Rigidbody>();

                _stateRef.playerOnVehicle = true;
                _vehicleinput.enabled = true;
                _rb.isKinematic = false;
                _stateRef.playerCollider.enabled = false;
                _stateRef.playerTransform.parent = _cc.sitPos;
                _stateRef.playerTransform.position = _cc.sitPos.position;
                _stateRef.playerTransform.rotation = _cc.sitPos.rotation;
                _stateRef.playerRigidbody.isKinematic = true;
            }
            else if (_bc = _stateRef.enteredVehicle.GetComponent<BikeController>())
            {
                _bikeinput = _stateRef.enteredVehicle.GetComponent<BikeBasicInput>();

                _stateRef.playerOnVehicle = true;
                _bikeinput.enabled = true;
                _stateRef.playerCollider.enabled = false;
                _stateRef.playerTransform.parent = _bc.sitPos;
                _stateRef.playerTransform.position = _bc.sitPos.position;
                _stateRef.playerTransform.rotation = _bc.sitPos.rotation;
                _stateRef.playerRigidbody.isKinematic = true;
            }
        }

        public void ExitVehicle()
        {
            VehicleBasicInput _vehicleinput;
            BikeBasicInput _bikeinput;
            Rigidbody _rb;

            if (_stateRef.enteredVehicle.GetComponent<VehicleController>())
            {
                _vehicleinput = _stateRef.enteredVehicle.GetComponent<VehicleBasicInput>();
                _rb = _stateRef.enteredVehicle.GetComponent<Rigidbody>();

                _stateRef.playerOnVehicle = false;
                _vehicleinput.enabled = false;
                _rb.isKinematic = true;
                _stateRef.playerTransform.parent = null;
                _stateRef.playerCollider.enabled = true;
                _stateRef.playerRigidbody.isKinematic = false;
            }
            else if (_stateRef.enteredVehicle.GetComponent<BikeController>())
            {
                _bikeinput = _stateRef.enteredVehicle.GetComponent<BikeBasicInput>();

                _stateRef.playerOnVehicle = false;
                _bikeinput.enabled = false;
                _stateRef.playerTransform.parent = null;
                _stateRef.playerTransform.position += -_stateRef.playerTransform.forward * 0.02f;
                _stateRef.playerCollider.enabled = true;
                _stateRef.playerRigidbody.isKinematic = false;
            }

            _stateRef.playerChangable = false;
        }

        void ScaleCapsuleForCrouching(bool crouch)
        {
            if (_stateRef.playerGrounded && crouch && !_stateRef.playerClimbing)
            {
                if (_stateRef.playerCrouching) return;
                _capsuleCol.height = _capsuleCol.height / 2f;
                _capsuleCol.center = _capsuleCol.center / 2f;
                _stateRef.playerCrouching = true;
            }
            else if (!_stateRef.playerClimbing)
            {
                Ray crouchRay = new Ray(_rigidbody.position + Vector3.up * _capsuleCol.radius * _half, Vector3.up);
                float crouchRayLength = (_capsuleHeight * charScaleY) - _capsuleCol.radius * _half;

                if (Physics.SphereCast(crouchRay, _capsuleCol.radius * _half, crouchRayLength, m_raycastLayerMaskforRagdoll.value, QueryTriggerInteraction.Ignore))
                {
                    _stateRef.playerCrouching = true;
                    return;
                }
                _capsuleCol.height = _capsuleHeight;
                _capsuleCol.center = _capsuleCenter;
                _stateRef.playerCrouching = false;
            }
        }

        void OnDrawGizmos()
        {
            if (debugHeight)
            {
                Gizmos.DrawRay(_testHeightRay.origin, Vector3.up * _testHeightFloat);
            }
        }

        void PreventStandingInLowHeadroom()
        {
            if (!_stateRef.playerCrouching && !_stateRef.playerClimbing)
            {
                Ray crouchRay = new Ray(_rigidbody.position + Vector3.up * _capsuleCol.radius * _half, Vector3.up);
                float crouchRayLength = (_capsuleHeight * charScaleY) - _capsuleCol.radius * _half;

                _testHeightRay = crouchRay;
                _testHeightFloat = crouchRayLength;

                if (Physics.SphereCast(crouchRay, _capsuleCol.radius * _half, crouchRayLength, m_raycastLayerMaskforRagdoll.value, QueryTriggerInteraction.Ignore))
                {
                    _stateRef.playerCrouching = true;
                }
            }
        }

        void UpdateAnimator(Vector3 move)
        {
            _animator.SetFloat("Forward", _forwardAmount, 0.1f, Time.deltaTime);
            if (!_stateRef.playerClimbing)
            {
                _animator.SetFloat("Turn", _turnAmount, 0.1f, Time.deltaTime);
            }
            _animator.SetBool("Crouch", _stateRef.playerCrouching);
            _animator.SetBool("OnGround", _stateRef.playerGrounded);
            _animator.SetBool("Climb", _stateRef.playerClimbing);
            if (!_stateRef.playerGrounded)
            {
                _animator.SetFloat("Jump", _rigidbody.velocity.y);
            }

            float runCycle = Mathf.Repeat(
                    _animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < _half ? 1 : -1) * _forwardAmount;

            if (_stateRef.playerGrounded) _animator.SetFloat("JumpLeg", jumpLeg);

            if (_stateRef.playerGrounded && move.magnitude > 0)
                _animator.speed = animSpeedMultiplier;
            else
                _animator.speed = 1;
        }

        void HandleAirborneMovement()
        {
            Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
            _rigidbody.AddForce(extraGravityForce);
            m_GroundCheckDistance = _rigidbody.velocity.y < 0 ? _defaultGroundCheckDistance : 0.01f;
        }

        void HandleGroundedMovement(bool crouch, bool jump)
        {
            if (jump && !crouch && !_stateRef.playerClimbing && _animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
            {
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, m_JumpPower, _rigidbody.velocity.z);
                _stateRef.playerGrounded = false;
                _animator.applyRootMotion = false;
                m_GroundCheckDistance = 0.1f;
            }
        }

        void ApplyExtraTurnRotation()
        {
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, _forwardAmount);
            transform.Rotate(0, _turnAmount * turnSpeed * Time.deltaTime, 0);
        }

        public void OnAnimatorMove()
        {
            if (_stateRef.playerGrounded && !_stateRef.playerClimbing && Time.deltaTime > 0)
            {
                Vector3 moveForward = transform.forward * _animator.GetFloat("motionZ") * Time.deltaTime;
                Vector3 v = ((_animator.deltaPosition + moveForward) * moveSpeedMultiplier) / Time.deltaTime;
                v.y = _rigidbody.velocity.y;
                _rigidbody.velocity = v;
            }
        }

        void CheckGroundStatus()
        {
            RaycastHit hitInfo;

            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
            {
                _groundNormal = hitInfo.normal;
                _stateRef.playerGrounded = true;
                _animator.applyRootMotion = true;
            }
            else
            {
                _stateRef.playerGrounded = false;
                _groundNormal = Vector3.up;
                _animator.applyRootMotion = false;
            }
        }
    }
}
