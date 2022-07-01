using UnityEngine;
using System.Collections.Generic;

namespace razz
{
    [HelpURL("https://negengames.com/interactor")]
    [RequireComponent(typeof(SphereCollider))]
    public class Interactor : MonoBehaviour
    {
        #region Interactor Variables
        public class IntObjComponents
        {
            public InteractorObject interactorObject;
            public float distance;
        }

        public class EffectorStatus
        {
            public bool connected;
            public InteractorObject connectedTo;
            public InteractorTarget connectedTarget;
        }

        public static EffectorStatus[] effectors;
        public static bool anyConnected;
        public List<EffectorLink> effectorLinks = new List<EffectorLink>();

        public enum FullBodyBipedEffector
        {
            Body,
            LeftShoulder,
            RightShoulder,
            LeftThigh,
            RightThigh,
            LeftHand,
            RightHand,
            LeftFoot,
            RightFoot
        }

        //This is the list of all interaction objects in sphere area. Its public because UI needs these too.
        [HideInInspector] public List<IntObjComponents> intOjbComponents = new List<IntObjComponents>();
        [HideInInspector] public SphereCollider sphereCol;
        [HideInInspector] public Vector3 sphereColWithRotScale;
        [HideInInspector] public GameObject selfInteractionObject;
        [HideInInspector] public bool selfInteractionEnabled = false;
        [HideInInspector] public int selectedByUI = 0;

        //InteractorIK deals with ik interactions
        private InteractorIK _interactorIK;
        //Active self interaction target
        private InteractorTarget _selfActiveTarget;

        private Transform _playerTransform;
        private Vector3 _playerCenter;
        private bool _disconnectOnce;
        private bool _connectOnce;
        
        //These are for raycast calculations
        private RaycastHit _lookHit;
        private Ray _mousePosRay;
        private Camera _mainCam;
        private GameObject _activeDistanceIntObj;
        private int _layerMask;

        //Exposed properties
        [SerializeField] public float raycastDistance = 20f; 
        
#if UNITY_EDITOR
        [SerializeField] public bool debug = true;
        [HideInInspector] public int selectedTab;
        [HideInInspector] public string savePath;
        [HideInInspector] public float maxRadius;
        [HideInInspector][SerializeField] public bool logoChange;
        [HideInInspector] public readonly float interactorVersion = 0.65f;

        //Different debug line colors for each effector. 
        //Max 8 colors right now, can be increased if needed.
        public static Color ColorForArrayPlace(int arrayPlace, bool active)
        {
            Color debugColor;

            switch (arrayPlace)
            {
                case 0:
                    debugColor = Color.blue;
                    break;
                case 1:
                    debugColor = Color.red;
                    break;
                case 2:
                    debugColor = Color.magenta;
                    break;
                case 3:
                    debugColor = Color.green;
                    break;
                case 4:
                    debugColor = Color.yellow;
                    break;
                case 5:
                    debugColor = Color.cyan;
                    break;
                case 6:
                    debugColor = Color.black;
                    break;
                case 7:
                    debugColor = Color.gray;
                    break;
                default:
                    debugColor = Color.white;
                    break;
            }
            
            if (!active)
            {
                debugColor.a = 0.15f;
            }

            return debugColor;
        }
#endif
        #endregion

        private void Awake()
        {
            _playerTransform = this.transform;
            sphereCol = GetComponent<SphereCollider>();
            sphereCol.isTrigger = true;

            effectors = new EffectorStatus[effectorLinks.Count];
            for (int i = 0; i < effectors.Length; i++)
            {
                effectors[i] = new EffectorStatus();
            }
        }

        private void Start()
        {
            //Layermask for raycasts to not hit player colliders or interactor sphere trigger.
            _layerMask = ~LayerMask.GetMask("Player");

            if (!(_mainCam = Camera.main))
            {
                Debug.Log("Could not find main camera");
            }

            if (!(_interactorIK = GetComponent<InteractorIK>()))
            {
                Debug.Log("There is no InteractorIK on " + this.gameObject.name);
            }
            
            if (selfInteractionObject != null && selfInteractionObject.activeInHierarchy)
            {
                IntObjEnter(selfInteractionObject);
                selfInteractionEnabled = true;
                selectedByUI = 1;
            }

            sphereColWithRotScale = (sphereCol.center.x * _playerTransform.right) + (sphereCol.center.y * _playerTransform.up) + (sphereCol.center.z * _playerTransform.forward);

            for (int i = 0; i < effectorLinks.Count; i++)
            {
                effectorLinks[i].Initiate(_interactorIK, _playerTransform, _layerMask, sphereColWithRotScale, i, this);
            }
        }

        private void FixedUpdate()
        {
            _connectOnce = false;
            _disconnectOnce = false;
            DistanceObjRay();

            _playerCenter = _playerTransform.position + sphereCol.center;

            if (intOjbComponents.Count <= 0) return;

            for (int i = 0; i < intOjbComponents.Count; i++)
            {
                //If interaction is self or distance, do not check its position.
                //They will always stay on top of list. Self will stay 0 and wont display on list
                //Distance will stay 1 on list and will be first selection on list.
                if (selfInteractionEnabled && i == 0)
                    continue;

                if (intOjbComponents[i].interactorObject.interactionType == InteractorObject.InteractionTypes.DistanceCrosshair)
                    continue;

                //Get all position updates in interaction obj list
                PositionUpdate(i);
            }

            if (intOjbComponents.Count > 1)
            {
                //Sort list depending on their distances
                intOjbComponents.Sort(DistanceSort);
            }

            //Effectors needs properly positioned and rotated sphere trigger position, if not centered.
            sphereColWithRotScale = (sphereCol.center.x * _playerTransform.right) + (sphereCol.center.y * _playerTransform.up) + (sphereCol.center.z * _playerTransform.forward);

            for (int i = 0; i < effectorLinks.Count; i++)
            {
                if (!effectorLinks[i].enabled) continue;

                effectorLinks[i].Update(intOjbComponents, -1, sphereColWithRotScale);
            }
        }

        #region Raycast & Distance Updates
        private void DistanceObjRay()
        {
            _mousePosRay = _mainCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(_mousePosRay, out _lookHit, raycastDistance, _layerMask))
            {
                if (_lookHit.collider.gameObject != _activeDistanceIntObj)
                {
                    if (_activeDistanceIntObj != null) IntObjExit(_activeDistanceIntObj);

                    IntObjEnter(_lookHit.collider.gameObject);
                }
            }
            else if (_activeDistanceIntObj != null)
            {
                IntObjExit(_activeDistanceIntObj);
            }
        }

        private void PositionUpdate(int i)
        {
            intOjbComponents[i].distance = Vector3.Distance(_playerCenter, intOjbComponents[i].interactorObject.transform.position);
        }

        public int DistanceSort(IntObjComponents compare1, IntObjComponents compare2)
        {
            return compare1.distance.CompareTo(compare2.distance);
        }
        #endregion

        #region Add Interaction
        //Adds new interaction objects to list. This is for Self or distance interactions.
        private void IntObjEnter(GameObject Object)
        {
            InteractorObject intObj;
            if (!(intObj = Object.GetComponent<InteractorObject>())) return;

            if (intObj.interactionType != InteractorObject.InteractionTypes.DistanceCrosshair && intObj.interactionType != InteractorObject.InteractionTypes.SelfItch) return;

            if (intOjbComponents.Count > 0)
            {
                for (int i = 0; i < intOjbComponents.Count; i++)
                {
                    if (intOjbComponents[i].interactorObject == intObj) return;
                }
            }

            IntObjComponents temp = new IntObjComponents();
            //This is for keeping self interaction at first position
            //distance interaction at second position on interaction list, 
            //which is sorted by distance except these two.
            if (intObj.interactionType == InteractorObject.InteractionTypes.SelfItch)
            {
                temp.distance = -1;
            }
            else
            {
                temp.distance = 0;
            }
            temp.interactorObject = intObj;

            if (intObj.interactionType == InteractorObject.InteractionTypes.DistanceCrosshair) _activeDistanceIntObj = Object;

            intOjbComponents.Add(temp);
        }

        //Adds new interaction objects to list. This is for regular interaction objects with collider.
        private void OnTriggerEnter(Collider collidedObj)
        {
            InteractorObject intObj;
            if (!(intObj = collidedObj.GetComponent<InteractorObject>())) return;

            if (intObj.interactionType == InteractorObject.InteractionTypes.DistanceCrosshair) return;

            if (intOjbComponents.Count > 0)
            {
                for (int i = 0; i < intOjbComponents.Count; i++)
                {
                    if (intOjbComponents[i].interactorObject == intObj) return;
                }
            }

            IntObjComponents temp = new IntObjComponents();
            temp.distance = Vector3.Distance(sphereCol.center, intObj.transform.position);
            temp.interactorObject = intObj;
            
            intOjbComponents.Add(temp);
        }
        #endregion

        #region Remove Interaction
        //Removes interaction objects from list. This is for distance interactions.
        private void IntObjExit(GameObject Object)
        {
            InteractorObject intObj;
            if (!(intObj = Object.GetComponent<InteractorObject>())) return;

            if (intObj.interactionType != InteractorObject.InteractionTypes.DistanceCrosshair) return;

            if (intOjbComponents.Count > 0)
            {
                for (int i = 0; i < intOjbComponents.Count; i++)
                {
                    if (intOjbComponents[i].interactorObject == intObj)
                    {
                        //Illegal disconnection procedure. Doesnt enter actual update, only Exited section of
                        //Update to disconnect interaction which should be already disconnected in the 
                        //first place but something went wrong (Like  running fast).
                        for (int a = 0; a < effectorLinks.Count; a++)
                        {
                            effectorLinks[a].Update(intOjbComponents, i, sphereColWithRotScale);
                        }
                        
                        if (Object == _activeDistanceIntObj)
                        {
                            _activeDistanceIntObj = null;
                        }
                        intOjbComponents.Remove(intOjbComponents[i]);
                    }
                }
            }
        }

        //Removes interaction objects from list. This is for regular interaction objects with collider.
        private void OnTriggerExit(Collider collidedObj)
        {
            InteractorObject intObj;
            if (!(intObj = collidedObj.GetComponent<InteractorObject>())) return;

            if (intObj.interactionType == InteractorObject.InteractionTypes.DistanceCrosshair) return;

            if (intOjbComponents.Count > 0)
            {
                for (int i = 0; i < intOjbComponents.Count; i++)
                {
                    if (intOjbComponents[i].interactorObject == intObj)
                    {
                        //Illegal disconnection procedure. Same with other above.
                        for (int a = 0; a < effectorLinks.Count; a++)
                        {
                            effectorLinks[a].Update(intOjbComponents, i, sphereColWithRotScale);
                        }

                        if (intOjbComponents[i].interactorObject.usable)
                        {
                            intOjbComponents[i].interactorObject.Use(false);
                        }

                        intOjbComponents.Remove(intOjbComponents[i]);
                    }
                }
            }
        }
        #endregion

        #region Connect & Disconnect Effectors

        //Starts or stops selected interactions
        public void StartStopInteractions(bool click)
        {
            //Run for every effector
            for (int i = 0; i < effectorLinks.Count; i++)
            {
                if (!effectorLinks[i].enabled) continue;

                //If already did a ConnectAll or DisconnectedAll in this frame, stop loop. 
                //To prevent multiple effectors to toggle themself.
                if (_connectOnce || _disconnectOnce)
                {
                    ResetSelection();
                    return;
                }

                effectorLinks[i].StartStopInteractionThis(intOjbComponents, selectedByUI, click);
            }
            ResetSelection();
        }

        //Resets UI selection after StartStopInteractions() done
        private void ResetSelection()
        {
            if (selfInteractionEnabled)
            {
                selectedByUI = 1;
                return;
            }
            selectedByUI = 0;
        }

        //Connects an effector with given array place and connected InteractorObject
        public static void Connect(int arrayPlace, InteractorObject connectedTo, InteractorTarget connectedTarget)
        {
            effectors[arrayPlace].connected = true;
            effectors[arrayPlace].connectedTo = connectedTo;
            effectors[arrayPlace].connectedTarget = connectedTarget;
            anyConnected = true;
        }

        //Disconnects an effector with given array place of it
        public static void Disconnect(int arrayPlace)
        {
            effectors[arrayPlace].connected = false;
            //effectors[arrayPlace].connectedTo = null;
            anyConnected = false;

            for (int i = 0; i < effectors.Length; i++)
            {
                if (effectors[i].connected)
                {
                    anyConnected = true;
                }
            }
        }

        //Connects all effectors to given targets and InteractorObject. Runs only once in same frame.
        public void ConnectAll(InteractorTarget[] allTargets, InteractorObject connectedTo)
        {
            if (_connectOnce) return;

            for (int i = 0; i < effectors.Length; i++)
            {
                for (int a = 0; a < allTargets.Length; a++)
                {
                    if ((int)effectorLinks[i].effectorType == (int)allTargets[a].effectorType)
                    {
                        Connect(i, connectedTo, allTargets[a]);
                        effectorLinks[i].ConnectThis(effectors[i].connectedTo);
                    }
                }
            }
            _connectOnce = true;
        }

        //Disconnects all connected effectors. Runs only once in same frame, 
        //because it can be called  more then once unnecessarily from multiple effectors.
        public void DisconnectAll()
        {
            if (_disconnectOnce) return;
            if (!anyConnected)
            {
                _disconnectOnce = true;
                return;
            }

            for (int i = 0; i < effectors.Length; i++)
            {
                if (effectors[i].connected)
                {
                    Disconnect(i);
                    effectorLinks[i].DisconnectThis();
                }
            }

            //ResetPlayerStates();

            anyConnected = false;
            _disconnectOnce = true;
        }
        #endregion

        [System.Serializable]
        public class EffectorLink
        {
            #region Effector Variables

            private Interactor _interactor;
            private InteractorObject _interactorObject;
            private InteractorIK _interactorIK;
            private Transform _playerTransform;
            private bool _initiated;
            private int _this;
            //If any interaction gets in position with successful check then selfPossible gets false
            //to make itself not possible. That way we disable conflicts.
            private bool _selfPossible;
            //To check sending events once per interaction for needed interactions
            private bool _eventSent;
            private Vector3 _effectorWorldSpace;
            private Vector3 _sphereColWithRotScale;
            private int _layerMask;
            private InteractorObject.InteractionTypes focusedObjectIntType;
            private InteractorObject _softInteractedObj;
            private InteractorTarget[] _allTargets;
            private InteractorTarget _closestTarget;

            //Temporary holders for offset calculations
            private Vector3 _offsetTempVector;
            private float _tempRadius;
            private float _targetAngleY, _targetAngleY2, _targetAngleZ, _targetAngleZ2, _targetDistance;
            private float _maxAngleZ, _minAngleZ, _maxAngleY, _minAngleY;
            private bool _reverseZ, _reverseY;
            private float _distanceToTarget;
            private float _shortestDistToTarget;
            private RaycastHit hit;

            [HideInInspector] public bool targetActive;
            [HideInInspector] public Vector3 targetPosition;

            //For ProtoTruck example or Parts to animate
            private VehicleBasicInput _vehicleInput;
            private VehiclePartControls _vehiclePartCont;
            private TurretAim[] _childTurrets;
            private bool _vehiclePartsActive;

            #region Exposed Effector Specs
            [SerializeField] public bool enabled = true;
            [SerializeField] public string effectorName;
            [SerializeField] public FullBodyBipedEffector effectorType;
            [SerializeField] public Vector3 posOffset = Vector3.zero;
            [SerializeField][Range(-180f, 180f)] public float angleOffset;
            [SerializeField][Range(0f, 360f)] public float angleXZ = 45f;
            [SerializeField][Range(-180f, 180f)] public float angleOffsetYZ;
            [SerializeField][Range(0f, 360f)] public float angleYZ = 45f;
            [SerializeField] public float radius = 0.1f;
            //public float minDistance = 0.05f;
            #endregion
            #endregion

            #region Initiation

            public void Initiate(InteractorIK interactorIK, Transform playerTransform, LayerMask layermask, Vector3 sphereColWithRotScale, int effectorArrayPlace, Interactor interactor)
            {
                _interactor = interactor;
                _interactorIK = interactorIK;
                _playerTransform = playerTransform;
                _layerMask = layermask;
                _sphereColWithRotScale = sphereColWithRotScale;
                _this = effectorArrayPlace;
                targetActive = false;

                if (_vehicleInput = FindObjectOfType<VehicleBasicInput>())
                {
                    _vehiclePartCont = _vehicleInput.vehPartControl;
                    if (_vehiclePartCont != null)
                    {
                        _vehiclePartsActive = true;
                    }
                    _childTurrets = FindObjectsOfType<TurretAim>();
                }

                _initiated = true;
            }
            #endregion

            #region Effector Checks

            //Selects a method depends on if its Z only or both axis
            public bool EffectorCheck(Transform target, int i, bool zOnly)
            {
                if (!zOnly)
                    return EffectorCheck_YZ(target, i);
                else
                    return EffectorCheck_Z(target, i);
            }

            //Checks the target position if its ok to interact with given effector specs
            public bool EffectorCheck_YZ(Transform target, int i)
            {
                _tempRadius = radius;
                if (_interactorObject.childTargets[i].overrideEffector)
                {
                    _tempRadius = _interactorObject.childTargets[i].overridenDistance;
                }

                _offsetTempVector = target.position - _effectorWorldSpace;
                _offsetTempVector = Vector3.ProjectOnPlane(_offsetTempVector, _playerTransform.up);
                _targetAngleZ = Vector3.SignedAngle(_offsetTempVector, _playerTransform.right, _playerTransform.up);
                _targetAngleZ2 = -(180 + _targetAngleZ);
                _targetAngleZ = 180f - _targetAngleZ;

                _reverseZ = false;

                _maxAngleZ = angleOffset + angleXZ;
                _minAngleZ = angleOffset;
                _maxAngleY = angleOffsetYZ + angleYZ;
                _minAngleY = angleOffsetYZ;

                if (_targetAngleZ >= _maxAngleZ)
                {
                    _reverseZ = true;
                }

                _offsetTempVector = _effectorWorldSpace - target.position;
                _offsetTempVector = Vector3.ProjectOnPlane(_offsetTempVector, _playerTransform.right);
                _targetAngleY = Vector3.SignedAngle(_offsetTempVector, _playerTransform.up, _playerTransform.right);
                _targetAngleY2 = -(180 + _targetAngleY);
                _targetAngleY = 180f - _targetAngleY;

                _reverseY = false;

                if (_targetAngleY >= _maxAngleY)
                {
                    _reverseY = true;
                }

                _targetDistance = Vector3.Distance(_effectorWorldSpace, target.position);

                if (_targetDistance <= _tempRadius)
                {
                    if (!_reverseZ)
                    {
                        if (!_reverseY)
                        {
                            if (_targetAngleY >= _minAngleY && _targetAngleY <= _maxAngleY)
                            {
                                if (_targetAngleZ >= _minAngleZ && _targetAngleZ <= _maxAngleZ)
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (_targetAngleY2 >= _minAngleY && _targetAngleY2 <= _maxAngleY)
                            {
                                if (_targetAngleZ >= _minAngleZ && _targetAngleZ <= _maxAngleZ)
                                {
                                    return true;
                                }
                            }
                        }

                    }
                    else
                    {
                        if (!_reverseY)
                        {
                            if (_targetAngleY >= _minAngleY && _targetAngleY <= _maxAngleY)
                            {
                                if (_targetAngleZ2 >= _minAngleZ && _targetAngleZ2 <= _maxAngleZ)
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (_targetAngleY2 >= _minAngleY && _targetAngleY2 <= _maxAngleY)
                            {
                                if (_targetAngleZ2 >= _minAngleZ && _targetAngleZ2 <= _maxAngleZ)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }

            //Same with other medhod but doesnt account for Y plane so height and Y angles doesnt matter
            public bool EffectorCheck_Z(Transform target, int i)
            {
                _tempRadius = radius;
                if (_interactorObject.childTargets[i].overrideEffector)
                {
                    _tempRadius = _interactorObject.childTargets[i].overridenDistance;
                }

                _offsetTempVector = target.position - _effectorWorldSpace;
                _offsetTempVector = Vector3.ProjectOnPlane(_offsetTempVector, _playerTransform.up);
                _targetAngleZ = Vector3.SignedAngle(_offsetTempVector, _playerTransform.right, _playerTransform.up);
                _targetAngleZ2 = -(180 + _targetAngleZ);
                _targetAngleZ = 180f - _targetAngleZ;

                _reverseZ = false;

                _maxAngleZ = angleOffset + angleXZ;
                _minAngleZ = angleOffset;

                if (_targetAngleZ >= _maxAngleZ)
                {
                    _reverseZ = true;
                }

                Vector3 _effectorWorldSpaceNoY = _effectorWorldSpace;
                _effectorWorldSpaceNoY.y = target.position.y;
                _targetDistance = Vector3.Distance(_effectorWorldSpaceNoY, target.position);

                if (_targetDistance <= _tempRadius)
                {
                    if (!_reverseZ)
                    {
                        if (_targetAngleZ >= _minAngleZ && _targetAngleZ <= _maxAngleZ)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (_targetAngleZ2 >= _minAngleZ && _targetAngleZ2 <= _maxAngleZ)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            //Gets the closest target for same object to this effector
            private int ShortestTargetSameEffector(InteractorTarget[] allTargets)
            {
                _shortestDistToTarget = 5f;
                int targetPointer = -1;

                for (int i = 0; i < allTargets.Length; i++)
                {
                    if ((int)allTargets[i].effectorType != (int)effectorType) continue;

                    _distanceToTarget = Vector3.Distance(_effectorWorldSpace, allTargets[i].transform.position);

                    if (_distanceToTarget < _shortestDistToTarget)
                    {
                        _shortestDistToTarget = _distanceToTarget;
                        targetPointer = i;
                    }
                }
                return targetPointer;
            }

            //Gets the closest target for same object to given effector
            private InteractorTarget ShortestTargetSameEffector(InteractorTarget[] allTargets, FullBodyBipedEffector m_effectorType)
            {
                _shortestDistToTarget = 5f;
                int targetPointer = -1;

                for (int i = 0; i < allTargets.Length; i++)
                {
                    if ((int)allTargets[i].effectorType != (int)m_effectorType) continue;

                    _distanceToTarget = Vector3.Distance(_effectorWorldSpace, allTargets[i].transform.position);

                    if (_distanceToTarget < _shortestDistToTarget)
                    {
                        _shortestDistToTarget = _distanceToTarget;
                        targetPointer = i;
                    }
                }
                return allTargets[targetPointer];
            }

            //Gets the farthest target for same object to given effector
            private InteractorTarget FarthestTargetSameEffector(InteractorTarget[] allTargets, FullBodyBipedEffector m_effectorType)
            {
                _shortestDistToTarget = 1f;
                int targetPointer = -1;

                for (int i = 0; i < allTargets.Length; i++)
                {
                    if ((int)allTargets[i].effectorType != (int)m_effectorType) continue;

                    _distanceToTarget = Vector3.Distance(_effectorWorldSpace, allTargets[i].transform.position);

                    if (_distanceToTarget > _shortestDistToTarget)
                    {
                        _shortestDistToTarget = _distanceToTarget;
                        targetPointer = i;
                    }
                }
                return allTargets[targetPointer];
            }

            //Repositions the targets of two handed pickup object. Sends two raycasts from object center(with given "closer" offset) to left and right. 
            //TODO One raycast only.
            private void PickableTwoRetarget(Transform target, float closer)
            {
                //Closer is 0-1 value between object center and player position. It can be adjusted to fit better for object shape.
                //But if its too close to player, its raycast can miss hit so target wont change, which causes weird looks.
                Vector3 closerPos = Vector3.Lerp(effectors[_this].connectedTo.transform.position, _playerTransform.position, closer);

                //This means effector is at left side of player so its goint to be left hand position.
                if (posOffset.x < 0)
                {
                    Physics.Raycast(closerPos - _playerTransform.right, _playerTransform.right, out hit, 1f, _layerMask);

                    Debug.DrawRay(closerPos - _playerTransform.right, _playerTransform.right, Color.red, 10f);

                    if (hit.collider == effectors[_this].connectedTo.col)
                    {
                        target.transform.position = hit.point;
                        target.transform.rotation = Quaternion.LookRotation(-hit.normal, _playerTransform.forward);
                    }
                }
                //Right side
                else
                {
                    Physics.Raycast(closerPos + _playerTransform.right, -_playerTransform.right, out hit, 1f, _layerMask);

                    Debug.DrawRay(closerPos + _playerTransform.right, -_playerTransform.right, Color.blue, 10f);

                    if (hit.collider == effectors[_this].connectedTo.col)
                    {
                        target.transform.position = hit.point;
                        target.transform.rotation = Quaternion.LookRotation(-hit.normal, _playerTransform.forward);
                    }
                }
            }

            //Moves player to center and rotates full forward when starting to climb at bottom
            private void ReposClimbingPlayerBottom(InteractorObject usedTarget)
            {
                Vector3 lh = ShortestTargetSameEffector(usedTarget.childTargets, FullBodyBipedEffector.LeftHand).transform.position;
                Vector3 rh = ShortestTargetSameEffector(usedTarget.childTargets, FullBodyBipedEffector.RightHand).transform.position;

                Vector3 handsDir = rh - lh;

                Vector3 handsMiddle = (lh + rh) * 0.5f;

                handsMiddle.y = _playerTransform.position.y;

                Vector3 perpencicular = Vector3.Cross(Vector3.up * 0.5f, handsDir);

                Debug.DrawRay(handsMiddle, perpencicular, Color.red, 10f);

                PlayerState.singlePlayerState.targetPosition = handsMiddle + perpencicular;
                PlayerState.singlePlayerState.targetRotation = Quaternion.LookRotation(-perpencicular, Vector3.up);
                PlayerState.singlePlayerState.rePos = true;
            }

            //Moves player a little forward when ending climb at top
            private void ReposClimbingPlayerTop(InteractorObject usedTarget)
            {
                //Since we're starting to climb at bottom, farthest targets are top
                Vector3 lh = FarthestTargetSameEffector(usedTarget.childTargets, FullBodyBipedEffector.LeftHand).transform.position;
                Vector3 rh = FarthestTargetSameEffector(usedTarget.childTargets, FullBodyBipedEffector.RightHand).transform.position;
                
                Vector3 handsDir = rh - lh;
                //Top middle point based on left and right hand targets
                Vector3 handsMiddle = (lh + rh) * 0.5f;
                //How much player will be pushed forward on top
                float pushAmount = -0.4f;
                //Perpendicular angle which is player forward direction and its amount
                Vector3 perpendicular = Vector3.Cross(Vector3.up * pushAmount, handsDir);
                //Debugs with line when used the ladder
                Debug.DrawRay(handsMiddle, perpendicular, Color.red, 10f);
                //Sets top posiiton at player state so PlayerController gets from there
                PlayerState.singlePlayerState.targetTopPosition = handsMiddle + perpendicular;
                PlayerState.singlePlayerState.targetTopRotation = Quaternion.LookRotation(perpendicular, Vector3.up);
            }
            #endregion

            //This is the main loop for all effectors to check all interaction objects in sphere area. 
            //If an interaction possible with given effector specs(depends on interaction type), 
            //it calls methods on object, changes some bools on PlayerState or etc to tell interaction is possible.
            //Also this loops ends interactions when object leaves the sphere area or ends automatically.
            public void Update(List<IntObjComponents> interactionObjsInRange, int exited, Vector3 sphereColWithRotScale)
            {
                //This is for debug, used by InteractorEditor
                targetActive = false;

                if (!_initiated) return;
                if (!enabled) return;

                //When an object leaves sphere area, interactor class calls update with object list int. 
                //Main update loop doesnt run, only ends interactions or sends object disable calls, depending on interaction type.
                if (exited >= 0)
                {
                    _interactorObject = interactionObjsInRange[exited].interactorObject;

                    if (_interactorObject.interactionType == InteractorObject.InteractionTypes.DistanceCrosshair || _interactorObject.interactionType == InteractorObject.InteractionTypes.MultipleCockpit || _interactorObject.interactionType == InteractorObject.InteractionTypes.SelfItch || _interactorObject.interactionType == InteractorObject.InteractionTypes.ClimbableLadder || _interactorObject.interactionType == InteractorObject.InteractionTypes.ManualButton || _interactorObject.interactionType == InteractorObject.InteractionTypes.DefaultAnimated)
                    {
                        PlayerState.singlePlayerState.playerUsable = false;
                        _interactorObject.Use(false);
                        _interactorObject.usedBy = false;

                        if (_interactorObject.interactionType == InteractorObject.InteractionTypes.DistanceCrosshair && effectorType == FullBodyBipedEffector.Body)
                        {
                            focusedObjectIntType = 0;
                        }

                        if (_interactorObject.interactionType == InteractorObject.InteractionTypes.ClimbableLadder && _vehiclePartsActive)
                        {
                            _vehiclePartCont.Animate(_interactorObject.vehiclePartId, false);
                        }
                        return;
                    }

                    if (_interactorObject == effectors[_this].connectedTo && effectors[_this].connected)
                    {
                        _interactorIK.StopInteraction(effectorType);
                        DisconnectThis();
                    }

                    if (_interactorObject.interactionType == InteractorObject.InteractionTypes.ManualHit)
                    {
                        _interactorObject.hitObjUseable = false;
                    }

                    PlayerState.singlePlayerState.playerUsing = false;
                    return;
                }

                _sphereColWithRotScale = sphereColWithRotScale;
                //This is effectors world space position.
                _effectorWorldSpace = _playerTransform.position + _sphereColWithRotScale + ((_playerTransform.right * posOffset.x) + (_playerTransform.forward * posOffset.z) + (_playerTransform.up * posOffset.y));

                //Main loop for every interaction objects in sphere area
                for (int objPlaceInList = 0; objPlaceInList < interactionObjsInRange.Count; objPlaceInList++)
                {
                    _interactorObject = interactionObjsInRange[objPlaceInList].interactorObject;
                    _allTargets = _interactorObject.childTargets;

                    //Draws debug lines for all targets of all interaction objects in sphere area
                    DrawDebugLines(_allTargets);

                    switch (_interactorObject.interactionType)
                    {
                        case InteractorObject.InteractionTypes.DefaultAnimated:
                            {//Default (int)10-20
                                //To check only once per frame
                                if (effectorType == FullBodyBipedEffector.Body)
                                {
                                    if (interactionObjsInRange[objPlaceInList].distance <= _interactorObject.defaultAnimatedDistance)
                                    {
                                        //If selected, it will check if InteractorObject is blocked. Useful for doors below or above.
                                        //We cant let two same types of this run same time.
                                        if (_interactorObject.obstacleRaycast)
                                        {
                                            Physics.Raycast(_effectorWorldSpace, _interactorObject.transform.position - _effectorWorldSpace, out hit, interactionObjsInRange[objPlaceInList].distance + 0.1f, _layerMask);
                                            if (hit.collider != _interactorObject.col) continue;
                                        }

                                        if (_softInteractedObj != _interactorObject || !_interactorObject.usedBy)
                                        {
                                            //This sends Vehicle(VehiclePartControls) its cached id and sets its 
                                            //animation on, if it has parameters in Vehicle Animator with same name.
                                            if (_vehiclePartsActive)
                                            {
                                                _vehiclePartCont.Animate(_interactorObject.vehiclePartId, true);
                                            }

                                            //Start its events if there are any
                                            _interactorObject.SendUnityEvent();

                                            //This is soft connection because it doesnt need to effect other interactions.
                                            _softInteractedObj = _interactorObject;
                                            _interactorObject.usedBy = true;
                                        }
                                    }
                                    else if (interactionObjsInRange[objPlaceInList].distance > _interactorObject.defaultAnimatedDistance && _interactorObject == _softInteractedObj && _interactorObject.usedBy)
                                    {
                                        _interactorObject.usedBy = false;
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.ManualButton:
                            {//Manual (int)20-30
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;

                                    if (!effectors[_this].connected)
                                    {
                                        if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                        {
                                            DrawDebugLines(_allTargets[i], objPlaceInList);

                                            _interactorObject.useableEffectors[_this] = true;

                                            //If target already used by another effector
                                            if (_interactorObject.usedBy) continue;

                                            _interactorObject.Use(true);
                                            _selfPossible = false;
                                        }
                                        else
                                        {
                                            _interactorObject.useableEffectors[_this] = false;

                                            //InteractorObject can be used by another effector, do not deactivate.
                                            if (_interactorObject.UseableEffectorCount() > 0) continue;

                                            PlayerState.singlePlayerState.playerUsable = false;
                                            _interactorObject.Use(false);
                                        }
                                    }
                                    else if (_interactorObject == effectors[_this].connectedTo)
                                    {
                                        //If IK animation is on half way which means effector bone 
                                        //is in target position, fire its events if there are any.
                                        if (_interactorIK.GetProgress(effectorType) > 0.48f && !_eventSent)
                                        {
                                            effectors[_this].connectedTo.SendUnityEvent();
                                            _eventSent = true;
                                        }
                                        
                                        //Interaction anim is almost done, 
                                        //which means effector bone is back in deault position, end interaction.
                                        if (_interactorIK.GetProgress(effectorType) > 0.9f)
                                        {
                                            PlayerState.singlePlayerState.playerUsable = true;
                                            effectors[_this].connectedTo.Use(false);
                                            effectors[_this].connectedTo.usedBy = false;
                                            _eventSent = false;
                                            Disconnect(_this);
                                        }
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.ManualSwitch:
                            {
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;

                                    if (!effectors[_this].connected)
                                    {
                                        if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                        {
                                            DrawDebugLines(_allTargets[i], objPlaceInList);

                                            _interactorObject.useableEffectors[_this] = true;
                                            if (_interactorObject.usedBy) continue;

                                            _interactorObject.Use(true);
                                            _selfPossible = false;
                                        }
                                        else
                                        {
                                            _interactorObject.useableEffectors[_this] = false;
                                            if (_interactorObject.UseableEffectorCount() > 0) continue;

                                            PlayerState.singlePlayerState.playerUsable = false;
                                            _interactorObject.Use(false);
                                        }
                                    }
                                    else if (_interactorObject == effectors[_this].connectedTo)
                                    {
                                        //If target is not in position anymore end interaction.
                                        if (!EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                        {
                                            DisconnectThis();
                                        }

                                        //Interaction is paused so effector is on object now
                                        if (_interactorIK.IsPaused(effectorType))
                                        {
                                            //InteractorObject has Automover?
                                            if (effectors[_this].connectedTo.hasAutomover)
                                            {
                                                //If Automover didnt start yet, start
                                                if (!effectors[_this].connectedTo.autoMovers[i].started)
                                                {
                                                    effectors[_this].connectedTo.autoMovers[i].StartMovement();
                                                }
                                                //Automover started and came half in movement(animation in this case)
                                                //If so, run click
                                                else if (effectors[_this].connectedTo.autoMovers[i].half)
                                                {
                                                    //Check if InteractorObject has cached InteractiveSwitches
                                                    if (effectors[_this].connectedTo.hasInteractiveSwitch)
                                                    {
                                                        _interactorObject.interactiveSwitches[i].Click();
                                                        effectors[_this].connectedTo.autoMovers[i].half = false;
                                                    }
                                                    else
                                                    {
                                                        Debug.Log(effectors[_this].connectedTo.name + " has no InteractiveSwitch script on targets.");
                                                    }
                                                }
                                                //If Automover ended, end interaction
                                                else if (effectors[_this].connectedTo.autoMovers[i].ended)
                                                {
                                                    _interactorIK.ResumeInteraction(effectorType);
                                                    effectors[_this].connectedTo.autoMovers[i].ended = false;
                                                }
                                            }
                                            else
                                            {
                                                if (effectors[_this].connectedTo.hasInteractiveSwitch)
                                                {
                                                    _interactorObject.interactiveSwitches[i].Click();
                                                }
                                                else
                                                {
                                                    Debug.Log(effectors[_this].connectedTo.name + " has no InteractiveSwitch script on targets.");
                                                }
                                            }
                                        }
                                        //If all done, deactivate and reset
                                        else if (_interactorIK.GetProgress(effectorType) > 0.9f)
                                        {
                                            PlayerState.singlePlayerState.playerUsable = true;
                                            effectors[_this].connectedTo.Use(false);
                                            effectors[_this].connectedTo.usedBy = false;
                                            if (_interactorObject.hasAutomover)
                                            {
                                                effectors[_this].connectedTo.autoMovers[i].ResetBools();
                                            }
                                            Disconnect(_this);
                                        }
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.ManualRotator:
                            {
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;

                                    if (!effectors[_this].connected)
                                    {
                                        if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                        {
                                            DrawDebugLines(_allTargets[i], objPlaceInList);

                                            _interactorObject.useableEffectors[_this] = true;
                                            if (_interactorObject.usedBy) continue;

                                            _interactorObject.Use(true);
                                            _selfPossible = false;
                                        }
                                        else
                                        {
                                            _interactorObject.useableEffectors[_this] = false;
                                            if (_interactorObject.UseableEffectorCount() > 0) continue;

                                            PlayerState.singlePlayerState.playerUsable = false;
                                            _interactorObject.Use(false);
                                        }
                                    }
                                    else if (_interactorObject == effectors[_this].connectedTo)
                                    {
                                        if (!EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                        {
                                            DisconnectThis();
                                            continue;
                                        }

                                        if (_interactorIK.IsPaused(effectorType))
                                        {
                                            if (_interactorObject.hasAutomover)
                                            {
                                                if (!_interactorObject.autoMovers[i].started)
                                                {
                                                    _interactorObject.autoMovers[i].started = true;
                                                    if (_interactorObject.hasInteractiveRotator)
                                                    {
                                                        _interactorObject.interactiveRotators[i].active = true;
                                                    }
                                                    else
                                                    {
                                                        Debug.Log(_interactorObject.name + " has no InteractiveRotator script on targets.");
                                                    }
                                                }

                                                if (!_interactorObject.usedBy)
                                                {
                                                    _interactorObject.interactiveRotators[i].active = false;
                                                    _interactorIK.ResumeInteraction(effectorType);
                                                }
                                            }
                                            else
                                            {
                                                if (_interactorObject.hasInteractiveRotator)
                                                {
                                                    _interactorObject.interactiveRotators[i].active = true;
                                                }
                                                else
                                                {
                                                    Debug.Log(_interactorObject.name + " has no InteractiveRotator script on targets.");
                                                }

                                                if (!_interactorObject.usedBy)
                                                {
                                                    _interactorObject.interactiveRotators[i].active = false;
                                                    _interactorIK.ResumeInteraction(effectorType);
                                                }
                                            }
                                        }
                                        else if (_interactorIK.GetProgress(effectorType) > 0.9f)
                                        {
                                            PlayerState.singlePlayerState.playerUsable = true;
                                            effectors[_this].connectedTo.Use(false);
                                            if (_interactorObject.hasAutomover)
                                            {
                                                effectors[_this].connectedTo.autoMovers[i].ResetBools();
                                            }
                                            Disconnect(_this);
                                            //Send interacted object to camera for unlocking Y rotation
                                            //Because ManualRotator no longer needs that.
                                            FreeLookCam.LockCamY(_interactorObject.gameObject);
                                        }
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.ManualHit:
                            {
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;

                                    //Hit is LeftHand only, yet.
                                    if (effectorType == FullBodyBipedEffector.LeftHand && (int)_allTargets[i].effectorType == (int)FullBodyBipedEffector.LeftHand && !_interactorObject.usedBy)
                                    {
                                        //If hit object is in sphere area, set it useable which turns it to player each frame
                                        //This will move target accordingly and it can check its position
                                        _interactorObject.hitObjUseable = true;
                                        _interactorObject.Hit(_allTargets[i].transform, _effectorWorldSpace);
                                    }

                                    if (!effectors[_this].connected)
                                    {
                                        if (EffectorCheck(_allTargets[i].transform, i, true))
                                        {
                                            DrawDebugLines(_allTargets[i], objPlaceInList);
                                            //Stop rotating object, target is in position
                                            _interactorObject.hitObjUseable = false;
                                            //This is like other interactions, for other effectors. //TODO
                                            _interactorObject.useableEffectors[_this] = true;
                                            if (_interactorObject.usedBy) continue;

                                            _interactorObject.Use(true);
                                            _selfPossible = false;
                                        }
                                        else
                                        {
                                            _interactorObject.useableEffectors[_this] = false;
                                            if (_interactorObject.UseableEffectorCount() > 0) continue;

                                            PlayerState.singlePlayerState.playerUsable = false;
                                            _interactorObject.Use(false);
                                        }
                                    }
                                    else if (effectors[_this].connectedTo == _interactorObject)
                                    {
                                        _interactorObject.hitObjUseable = false;
                                        //Hit now since input changed usedBy
                                        //It will repos target
                                        _interactorObject.Hit(_allTargets[i].transform, _effectorWorldSpace);

                                        if (_interactorIK.IsPaused(effectorType))
                                        {
                                            //Effector is on repositioned target, now send back so it will look hit anim
                                            _interactorObject.HitPosDefault(_allTargets[i].transform, _effectorWorldSpace);
                                            //If hit done, end interaction
                                            if (_interactorObject.hitDone)
                                            {
                                                //Since its hit moment, send events now
                                                _interactorObject.SendUnityEvent();
                                                _interactorIK.ResumeInteraction(effectorType);
                                            }
                                        }
                                        //If interaction almost ended, reset all.
                                        if (_interactorIK.GetProgress(effectorType) > 0.9f)
                                        {
                                            {
                                                PlayerState.singlePlayerState.playerUsable = true;
                                                effectors[_this].connectedTo.Use(false);
                                                effectors[_this].connectedTo.usedBy = false;
                                                Disconnect(_this);
                                            }
                                        }

                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.ManualForce:
                            {
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    //This is player example and for fun, it will push the cubes, not cached (rigidbodies).
                                    //No connection here, but if effector connected it wont effect cubes.
                                    if (effectorType == FullBodyBipedEffector.LeftHand || effectorType == FullBodyBipedEffector.RightHand)
                                    {
                                        if (!effectors[_this].connected)
                                        {
                                            if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                            {
                                                _allTargets[i].GetComponent<Rigidbody>().AddForce((_allTargets[i].transform.position - _effectorWorldSpace), ForceMode.Impulse);
                                                Debug.DrawLine(_allTargets[i].transform.position, _effectorWorldSpace, Color.red, 3f);
                                            }
                                        }
                                    }
                                    //This is for ProtoTruck example and its using extra effector types
                                    //If you intent to use these effector types, remove those because it will conflict with player. 
                                    //LeftThigh and RightThigh are used for Turrets.
                                    else if (effectorType == FullBodyBipedEffector.LeftThigh || effectorType == FullBodyBipedEffector.RightThigh)
                                    {
                                        if (!effectors[_this].connected)
                                        {
                                            if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                            {
                                                if (_childTurrets.Length != 0)
                                                {
                                                    for (int k = 0; k < _childTurrets.Length; k++)
                                                    {
                                                        if ((int)_childTurrets[k].effector == (int)effectorType)
                                                        {
                                                            DrawDebugLines(_allTargets[i], objPlaceInList);
                                                            _childTurrets[k].Attack(_allTargets[i].transform);
                                                            Connect(_this, _interactorObject, _allTargets[i]);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (_interactorObject == effectors[_this].connectedTo)
                                        {
                                            if (!EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                            {
                                                Disconnect(_this);
                                            }
                                            else if (_allTargets[i] == effectors[_this].connectedTarget)
                                            {
                                                DrawDebugLines(_allTargets[i], objPlaceInList);
                                            }
                                        }
                                    }
                                    //This is for ProtoTruck back door example. If any object is in position, it will block the door so it wont work until InteractorObjects are out of position.
                                    else if (effectorType == FullBodyBipedEffector.RightShoulder)
                                    {
                                        if (!effectors[_this].connected)
                                        {
                                            if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                            {
                                                _vehicleInput.Blocked(true);
                                                Debug.DrawLine(_allTargets[i].transform.position, _effectorWorldSpace, Color.red, 3f);
                                                Connect(_this, _interactorObject, _allTargets[i]);
                                            }
                                        }
                                        else if (_interactorObject == effectors[_this].connectedTo)
                                        {
                                            if (!EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly) && _allTargets[i] == effectors[_this].connectedTarget)
                                            {
                                                _vehicleInput.Blocked(false);
                                                Disconnect(_this);
                                            }
                                            else if (_allTargets[i] == effectors[_this].connectedTarget)
                                            {
                                                DrawDebugLines(_allTargets[i], objPlaceInList);
                                            }
                                        }
                                    }
                                    //This is for ProtoTruck windshield example. If any InteractorObject is in position, shield anim will run.
                                    else if (effectorType == FullBodyBipedEffector.LeftShoulder)
                                    {
                                        if (!effectors[_this].connected)
                                        {
                                            if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                            {
                                                DrawDebugLines(_allTargets[i], objPlaceInList);
                                                _vehicleInput.SetWindshield(true);
                                                Debug.DrawLine(_allTargets[i].transform.position, _effectorWorldSpace, Color.red, 3f);
                                                Connect(_this, _interactorObject, _allTargets[i]);
                                            }
                                        }
                                        else if (_interactorObject == effectors[_this].connectedTo)
                                        {
                                            if (!EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly) && _allTargets[i] == effectors[_this].connectedTarget)
                                            {
                                                _vehicleInput.SetWindshield(false);
                                                Disconnect(_this);
                                            }
                                            else if (_allTargets[i] == effectors[_this].connectedTarget)
                                            {
                                                DrawDebugLines(_allTargets[i], objPlaceInList);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.TouchVertical:
                            {//Touch (int)30-40
                                //Touch interaction is automatic, no need for StartStopInteractionThis()
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;

                                    if (!effectors[_this].connected)
                                    {
                                        //Raycast for Vertical place in every 10 frame
                                        if (Time.frameCount % 10 == 0)
                                        {
                                            Physics.Raycast(_playerTransform.position + _interactorObject.touchHeight + _playerTransform.forward * _interactorObject.touchForward, -_playerTransform.right, out hit, _interactorObject.touchRayLength, _layerMask);
                                        }

                                        if (hit.collider == _interactorObject.col)
                                        {
                                            _allTargets[i].transform.position = hit.point;
                                            Vector3 tempRot = _allTargets[i].transform.rotation.eulerAngles;
                                            _allTargets[i].transform.rotation = Quaternion.Euler(new Vector3(tempRot.x, tempRot.y, hit.normal.y));

                                            if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                            {
                                                //To prevent interaction starting stutter(start stop, start stop)
                                                //wait a cooldown to start interaction
                                                if (_interactorObject.touchVCooldown > 0)
                                                {
                                                    _interactorObject.touchVCooldown -= Time.fixedDeltaTime;
                                                }
                                                else
                                                {
                                                    //Reset cooldown timers to their default time
                                                    _interactorObject.ResetTouchCooldowns();
                                                    _interactorIK.StartInteraction(_allTargets[i].effectorType, _allTargets[i], _interactorObject);
                                                    Connect(_this, _interactorObject, _allTargets[i]);
                                                    _interactorObject.SendUnityEvent();
                                                }
                                            }
                                        }
                                    }
                                    else if (_interactorObject == effectors[_this].connectedTo)
                                    {
                                        //Continue to raycast so target will move on surface in everyframe
                                        Physics.Raycast(_playerTransform.position + _interactorObject.touchHeight + _playerTransform.forward * _interactorObject.touchForward, -_playerTransform.right, out hit, _interactorObject.touchRayLength, _layerMask);

                                        if (hit.collider == _interactorObject.col)
                                        {
                                            hit.point = Vector3.Lerp(hit.point, _effectorWorldSpace, _interactorObject.touchLerp);
                                            Debug.DrawLine(_playerTransform.position + _interactorObject.touchHeight + _playerTransform.forward * _interactorObject.touchForward, hit.point, Color.red);
                                            _allTargets[i].transform.position = hit.point;

                                            Vector3 tempRot = _allTargets[i].transform.rotation.eulerAngles;
                                            _allTargets[i].transform.rotation = Quaternion.Euler(new Vector3(tempRot.x, tempRot.y, _interactorObject.touchRotation));

                                            if (!EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                            {
                                                if (!_interactorIK.IsPaused(FullBodyBipedEffector.LeftHand))
                                                {
                                                    _interactorIK.ReverseInteraction(FullBodyBipedEffector.LeftHand);
                                                }
                                                else
                                                {
                                                    _interactorIK.ResumeInteraction(FullBodyBipedEffector.LeftHand);
                                                }
                                                Disconnect(_this);
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            if (!_interactorIK.IsPaused(FullBodyBipedEffector.LeftHand))
                                            {
                                                _interactorIK.ReverseInteraction(FullBodyBipedEffector.LeftHand);
                                            }
                                            else
                                            {
                                                _interactorIK.ResumeInteraction(FullBodyBipedEffector.LeftHand);
                                            }
                                            Disconnect(_this);
                                            return;
                                        }
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.TouchHorizontalUp:
                            {
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;
                                    //If target is below the player, pass.
                                    if (_allTargets[i].transform.position.y < _playerTransform.position.y) continue;

                                    if (!effectors[_this].connected)
                                    {
                                        if (effectorType == FullBodyBipedEffector.RightHand)
                                        {
                                            if (Time.frameCount % 10 == 0)
                                            {
                                                Physics.Raycast(_playerTransform.position + _playerTransform.forward * _interactorObject.touchHorizontalForward + _playerTransform.right * _interactorObject.touchHorizontalRight, Vector3.up, out hit, _interactorObject.touchHorizontalRayLenght, _layerMask);
                                                Debug.DrawLine(_playerTransform.position + _playerTransform.forward * _interactorObject.touchHorizontalForward + _playerTransform.right * _interactorObject.touchHorizontalRight, hit.point);
                                            }

                                            if (hit.collider == _interactorObject.col)
                                            {
                                                _allTargets[i].transform.position = new Vector3(hit.point.x, _allTargets[i].transform.position.y, hit.point.z);
                                                Vector3 tempRot = _playerTransform.rotation.eulerAngles;
                                                Vector3 tempRot2 = _allTargets[i].transform.rotation.eulerAngles;
                                                _allTargets[i].transform.rotation = Quaternion.Euler(new Vector3(tempRot2.x, tempRot.y, tempRot2.z));

                                                if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                                {
                                                    if (_interactorObject.touchHCooldown > 0)
                                                    {
                                                        _interactorObject.touchHCooldown -= Time.fixedDeltaTime;
                                                    }
                                                    else
                                                    {
                                                        _interactorObject.ResetTouchCooldowns();
                                                        _interactorIK.StartInteraction(_allTargets[i].effectorType, _allTargets[i], _interactorObject);
                                                        Connect(_this, _interactorObject, _allTargets[i]);
                                                        _interactorObject.usedBy = true;
                                                        _interactorObject.SendUnityEvent();
                                                    }
                                                }
                                            }
                                        }
                                        //If RightHand used and this is not RightHand(which means Body)
                                        else if (_interactorObject.usedBy)
                                        {
                                            _interactorIK.StartInteraction(FullBodyBipedEffector.Body, _allTargets[i], _interactorObject);
                                            Connect(_this, _interactorObject, _allTargets[i]);
                                        }
                                    }
                                    else if (_interactorObject == effectors[_this].connectedTo)
                                    {
                                        if (effectorType == FullBodyBipedEffector.RightHand)
                                        {
                                            //Continue to raycast for RightHand, Body wont change.
                                            Physics.Raycast(_playerTransform.position + _playerTransform.forward * _interactorObject.touchHorizontalForward * 3f + _playerTransform.right * _interactorObject.touchHorizontalRight, Vector3.up, out hit, _interactorObject.touchHorizontalRayLenght, _layerMask);

                                            Debug.DrawLine(_playerTransform.position + _playerTransform.forward * _interactorObject.touchHorizontalForward * 3f + _playerTransform.right * _interactorObject.touchHorizontalRight, hit.point);

                                            if (hit.collider == _interactorObject.col)
                                            {
                                                Vector3 handNewPos = hit.point - _playerTransform.forward * _interactorObject.touchHorizontalForward * 2f;
                                                handNewPos.y = _allTargets[i].transform.position.y;
                                                _allTargets[i].transform.position = handNewPos;
                                                Vector3 tempRot = _playerTransform.rotation.eulerAngles;
                                                Vector3 tempRot2 = _allTargets[i].transform.rotation.eulerAngles;
                                                _allTargets[i].transform.rotation = Quaternion.Euler(new Vector3(tempRot2.x, tempRot.y, tempRot2.z));

                                                if (!EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                                {
                                                    if (!_interactorIK.IsPaused(FullBodyBipedEffector.RightHand))
                                                    {
                                                        _interactorIK.ReverseInteraction(FullBodyBipedEffector.RightHand);
                                                    }
                                                    else
                                                    {
                                                        _interactorIK.ResumeInteraction(FullBodyBipedEffector.RightHand);
                                                    }
                                                    _interactorObject.usedBy = false;
                                                    Disconnect(_this);
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                if (!_interactorIK.IsPaused(FullBodyBipedEffector.RightHand))
                                                {
                                                    _interactorIK.ReverseInteraction(FullBodyBipedEffector.RightHand);
                                                }
                                                else
                                                {
                                                    _interactorIK.ResumeInteraction(FullBodyBipedEffector.RightHand);
                                                }
                                                _interactorObject.usedBy = false;
                                                Disconnect(_this);
                                                return;
                                            }
                                        }
                                        //If this effector is Body and RightHand is already stopped, end Body too via object usedBy
                                        else if (!_interactorObject.usedBy)
                                        {
                                            if (!_interactorIK.IsPaused(FullBodyBipedEffector.Body))
                                            {
                                                _interactorIK.StopInteraction(FullBodyBipedEffector.Body);
                                            }
                                            else
                                            {
                                                _interactorIK.ResumeInteraction(FullBodyBipedEffector.Body);
                                            }
                                            _interactorObject.usedBy = false;
                                            Disconnect(_this);
                                            return;
                                        }
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.TouchHorizontalDown:
                            {
                                //TODO
                                break;
                            }
                        case InteractorObject.InteractionTypes.TouchStill:
                            {
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;

                                    if (!effectors[_this].connected)
                                    {
                                        if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                        {
                                            if (_interactorObject.obstacleRaycast)
                                            {
                                                if (Physics.Raycast(_effectorWorldSpace, _allTargets[i].transform.position - _effectorWorldSpace, Vector3.Distance(_allTargets[i].transform.position, _effectorWorldSpace), _layerMask)) continue;
                                            }

                                            _interactorObject.RotatePivot(_effectorWorldSpace);
                                            _interactorIK.StartInteraction(_allTargets[i].effectorType, _allTargets[i], _interactorObject);
                                            DrawDebugLines(_allTargets[i], objPlaceInList);
                                            Connect(_this, _interactorObject, _allTargets[i]);
                                            _interactorObject.SendUnityEvent();
                                        }
                                    }
                                    else if (_allTargets[i] == effectors[_this].connectedTarget && _interactorObject == effectors[_this].connectedTo)
                                    {
                                        if (!EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                        {
                                            //Resume if interaction reached to target, reverse if not.
                                            if (_interactorIK.IsPaused(effectorType))
                                            {
                                                _interactorIK.ResumeInteraction(effectorType);
                                            }
                                            else
                                            {
                                                _interactorIK.ReverseInteraction(effectorType);
                                            }
                                            Disconnect(_this);
                                            return;
                                        }
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.DistanceCrosshair:
                            {//Distance (int)40-50
                                if (effectorType == FullBodyBipedEffector.Body)
                                {
                                    _interactorObject.Use(true);
                                    _selfPossible = false;
                                    focusedObjectIntType = InteractorObject.InteractionTypes.DistanceCrosshair;
                                    PlayerState.singlePlayerState.playerUsable = true;
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.ClimbableLadder:
                            {//Climbable (int)50-60
                                //Get this effectors closest target
                                int targetPointer = ShortestTargetSameEffector(_allTargets);
                                if (targetPointer == -1) continue;

                                _closestTarget = _allTargets[targetPointer];

                                //All except LeftHand. LeftHand is starter here and its below this if().
                                if (effectorType != FullBodyBipedEffector.LeftHand)
                                {
                                    if (!effectors[_this].connected)
                                    {
                                        //usedBy on InteractorObject gets true when player uses input to start interaction by Left Hand. This is for other hand end both feet.
                                        if (_interactorObject.usedBy && !PlayerState.singlePlayerState.playerGrounded)
                                        {
                                            float progress = _interactorIK.GetProgress(effectorType);
                                            //If this effector didnt start any interaction or almost finished(which happens when its previous target is out of position)
                                            //If closestTarget is in position, start with that target.
                                            if (progress > 0.9f || progress == 0)
                                                if (EffectorCheck(_closestTarget.transform, targetPointer, _interactorObject.zOnly))
                                                {
                                                    Connect(_this, _interactorObject, _closestTarget);
                                                    //Focus object type used by StartStopInteractionThis() and usually sets at there but since these effectors are connecting here, we set them here.
                                                    focusedObjectIntType = InteractorObject.InteractionTypes.ClimbableLadder;

                                                    _interactorIK.StartInteraction(_closestTarget.effectorType, _closestTarget, _interactorObject);
                                                }
                                        }
                                    }
                                    else if (effectors[_this].connectedTo == _interactorObject)
                                    {
                                        //usedBy set false by LeftHand when interaction ended by user prematurely.
                                        if (!_interactorObject.usedBy || PlayerState.singlePlayerState.playerGrounded)
                                        {
                                            Disconnect(_this);

                                            _interactorIK.ReverseInteraction(effectorType);
                                            continue;
                                        }
                                        //If closest target isnt current target, resume interaction so next loops catch when anim is over 0.9 and switches to it.
                                        if (_closestTarget != effectors[_this].connectedTarget)
                                        {
                                            Disconnect(_this);

                                            _interactorIK.ResumeInteraction(effectorType);
                                            continue;
                                        }
                                        //When current target is out of position, continue anim (end interaction).
                                        if (!EffectorCheck(effectors[_this].connectedTarget.transform, targetPointer, _interactorObject.zOnly))
                                        {
                                            Disconnect(_this);
                                            _interactorIK.ResumeInteraction(effectorType);
                                        }
                                    }
                                    continue;
                                }

                                //Codes below are LeftHand Only
                                if (!effectors[_this].connected)
                                {
                                    float progress = _interactorIK.GetProgress(effectorType);
                                    if (progress > 0.9f || progress == 0)
                                        if (EffectorCheck(_closestTarget.transform, targetPointer, _interactorObject.zOnly))
                                        {
                                            DrawDebugLines(_closestTarget, objPlaceInList);

                                            Connect(_this, _interactorObject, _closestTarget);

                                            PlayerState.singlePlayerState.playerClimable = true;
                                            if (!_interactorObject.usedBy)
                                            {
                                                _interactorObject.Use(true);
                                            }

                                            //Fire ladder animation via VehiclePartController
                                            if (_vehiclePartsActive)
                                            {
                                                _vehiclePartCont.Animate(_interactorObject.vehiclePartId, true);
                                            }

                                            focusedObjectIntType = InteractorObject.InteractionTypes.ClimbableLadder;

                                            _interactorIK.StartInteraction(_closestTarget.effectorType, _closestTarget, _interactorObject);
                                        }
                                }
                                else if (effectors[_this].connectedTo == _interactorObject)
                                {
                                    if (_closestTarget != effectors[_this].connectedTarget)
                                    {
                                        Disconnect(_this);
                                        PlayerState.singlePlayerState.playerClimable = false;
                                        _interactorObject.Use(false);

                                        if (_interactorObject.usedBy)
                                        {
                                            _interactorIK.ResumeInteraction(effectorType);
                                        }
                                        else
                                        {
                                            _interactorIK.ReverseInteraction(effectorType);
                                        }
                                        continue;
                                    }

                                    if (!EffectorCheck(effectors[_this].connectedTarget.transform, targetPointer, _interactorObject.zOnly))
                                    {
                                        Disconnect(_this);
                                        PlayerState.singlePlayerState.playerClimable = false;
                                        _interactorObject.Use(false);

                                        if (_interactorIK.IsPaused(effectorType))
                                        {
                                            _interactorIK.ResumeInteraction(effectorType);
                                        }
                                        else
                                        {
                                            _interactorIK.ReverseInteraction(effectorType);
                                        }
                                    }
                                }

                                if (!PlayerState.singlePlayerState.playerClimbing && PlayerState.singlePlayerState.playerGrounded)
                                {
                                    _interactorIK.ResumeInteraction(FullBodyBipedEffector.LeftFoot);
                                    _interactorIK.ResumeInteraction(FullBodyBipedEffector.RightFoot);
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.MultipleCockpit:
                            {//Multiple (int)60-70
                             //Check effector position only for Body because its centered and its enough itself
                                if (!effectors[_this].connected && effectorType == FullBodyBipedEffector.Body)
                                {
                                    //Loop all targets and get Body
                                    for (int i = 0; i < _allTargets.Length; i++)
                                    {
                                        if ((int)_allTargets[i].effectorType == (int)FullBodyBipedEffector.Body)
                                        {
                                            if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                            {
                                                DrawDebugLines(_allTargets[i], objPlaceInList);

                                                //Set InteractorObject glow
                                                _interactorObject.Use(true);
                                                _selfPossible = false;
                                                //Set the vehicle on PlayerState to use (Truck or tricycle)
                                                PlayerState.singlePlayerState.enteredVehicle = _interactorObject.transform.root.gameObject;
                                                //Set the PlayerState that player can change, so now BasicInput waits for input. 
                                                //And when pressed, it will send PlayerController where it is actually changes.
                                                PlayerState.singlePlayerState.playerChangable = true;
                                            }
                                            else
                                            {
                                                //Deactivate object glow because Body effector is out of position.
                                                _interactorObject.Use(false);
                                                PlayerState.singlePlayerState.playerChangable = false;
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.SelfItch:
                            {//Self (int)70-80
                                //All targets for this effector, will randomly (with given odds) test their luck and if player is idle only one of them will run
                                if (_allTargets.Length <= 0) return;

                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;

                                    if (!effectors[_this].connected && _selfPossible && !_interactorObject.selfActive)
                                    {
                                        //"True" probability in every "pathMovers.odd" seconds
                                        bool chance = Random.Range(0, _interactorObject.pathMovers[i].odd * 1000) < 1000 * Time.fixedDeltaTime;

                                        if (chance)
                                        {
                                            if (PlayerState.singlePlayerState.playerIdle && !_interactorObject.selfActive && !_interactorIK.IsInInteraction(effectorType))
                                            {
                                                _interactorObject.pathMovers[i].StartMove(_interactorIK.GetDuration(effectorType));
                                                _interactorIK.StartInteraction(_allTargets[i].effectorType, _allTargets[i], _interactorObject);
                                                _interactor._selfActiveTarget = _allTargets[i];
                                                _interactorObject.selfActive = true;
                                                _interactorObject.SendUnityEvent();
                                            }
                                        }
                                    }
                                    else if (_interactorObject.selfActive && _allTargets[i] == _interactor._selfActiveTarget)
                                    {
                                        if (!PlayerState.singlePlayerState.playerIdle || !_interactorObject.pathMovers[i].playing)
                                        {
                                            _interactorObject.selfActive = false;
                                            _interactorIK.ResumeInteraction(effectorType);
                                        }
                                    }
                                }

                                //Since self interaction is always on first place at interactionObjsInRange list, it will be
                                //disabled if previous frame set it false (which means other interactions are possible).
                                //We set it true only here, so if this frame nothing sets it false, its possible to self interact next frame.
                                if (_interactor.selfInteractionEnabled) _selfPossible = true;

                                break;
                            }
                        case InteractorObject.InteractionTypes.PickableOne:
                            {//Pickables (int)80-90
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;

                                    //This effector available for picking?
                                    if (!effectors[_this].connected)
                                    {
                                        //Target is in good position for this effector?
                                        if (EffectorCheck(_interactorObject.transform, i, _interactorObject.zOnly))
                                        {
                                            //Its too late, its already picked by another effector.
                                            if (_interactorObject.picked) continue;

                                            //Tells InteractorObject that this effector can pick now.
                                            _interactorObject.useableEffectors[_this] = true;

                                            DrawDebugLines(_allTargets[i], objPlaceInList);
                                            _interactorObject.Use(true);
                                            _selfPossible = false;
                                        }
                                        else
                                        {
                                            _interactorObject.useableEffectors[_this] = false;
                                            //This effector is out of position now but before deactivating the object, it checks if any other effectors are still useable.
                                            bool tempBool2 = false;
                                            for (int j = 0; j < _interactorObject.useableEffectors.Length; j++)
                                            {
                                                if (_interactorObject.useableEffectors[j] == true)
                                                {
                                                    tempBool2 = true;
                                                }
                                            }
                                            if (tempBool2) continue;
                                            //No other effector is in position, deactivate object.
                                            _interactorObject.Use(false);
                                        }
                                    }
                                    //If this effector connected to this object and interaction is paused on half way, 
                                    //start pick method which sets transforms and deals with object rigidbody.
                                    //We're waiting for pause because it means hands is on object, half of animation.
                                    else if (effectors[_this].connectedTo == _interactorObject)
                                    {
                                        if (_interactorIK.IsPaused(effectorType))
                                        {
                                            //Sending this effectors bone to objects pick method so object can parent that
                                            _interactorObject.Pick(_interactorIK.GetBone(effectorType), effectors[_this].connectedTarget);
                                            _interactorObject.SendUnityEvent();
                                            //Picking is done, end the animation. Instead of ending, 
                                            //could be ResumeInteraction to continue but it can end 
                                            //with weird results for now.
                                            _interactorIK.StopInteraction(effectorType);
                                        }
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.PickableTwo:
                            {
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;

                                    if (!effectors[_this].connected)
                                    {
                                        if (EffectorCheck(_interactorObject.transform, i, _interactorObject.zOnly))
                                        {
                                            _interactorObject.useableEffectors[_this] = true;

                                            //Checks if any other effector is in position because this pick needs more than one effector
                                            if (_interactorObject.UseableEffectorCount() < 2) continue;

                                            DrawDebugLines(_allTargets[i], objPlaceInList);
                                            _interactorObject.Use(true);
                                            _selfPossible = false;
                                        }
                                        else
                                        {
                                            _interactorObject.ResetUseableEffectors();
                                            _interactorObject.Use(false);
                                        }
                                    }
                                    else if (effectors[_this].connectedTo == _interactorObject)
                                    {
                                        if (_interactorIK.IsPaused(effectorType) && !effectors[_this].connectedTo.picked && effectors[_this].connectedTo.pickable)
                                        {
                                            effectors[_this].connectedTo.Pick(_playerTransform, effectors[_this].connectedTarget);
                                            _interactorObject.SendUnityEvent();
                                        }
                                    }
                                    //When effector interacted another object, if it left two handed useableEffector true, set back false. 
                                    //Otherwise other effectors can pick it up while this effector interacting another.
                                    else if (_interactorObject.useableEffectors[_this])
                                    {
                                        _interactorObject.useableEffectors[_this] = false;
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.Push:
                            {//Push & Pull (int)90-100
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;

                                    if (!effectors[_this].connected)
                                    {
                                        if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                        {
                                            _interactorObject.useableEffectors[_this] = true;
                                            if (_interactorObject.UseableEffectorCount() < 2) continue;

                                            DrawDebugLines(_allTargets[i], objPlaceInList);
                                            _interactorObject.Use(true);
                                            _selfPossible = false;
                                        }
                                        else
                                        {
                                            _interactorObject.ResetUseableEffectors();
                                            _interactorObject.Use(false);
                                        }
                                    }
                                    else if (effectors[_this].connectedTo == _interactorObject)
                                    {
                                        if (_interactorIK.IsPaused(effectorType) && !PlayerState.singlePlayerState.playerPushing)
                                        {
                                            effectors[_this].connectedTo.PushStart(_playerTransform);
                                            PlayerState.singlePlayerState.playerPushing = true;
                                            _interactorObject.SendUnityEvent();
                                        }
                                    }
                                    else if (_interactorObject.useableEffectors[_this])
                                    {
                                        _interactorObject.useableEffectors[_this] = false;
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.CoverStand:
                            {//Cover Stand (int)100-110
                                //TODO
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;

                                    if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                    {
                                        DrawDebugLines(_allTargets[i], objPlaceInList);
                                    }
                                }
                                break;
                            }
                        case InteractorObject.InteractionTypes.CoverCrouch:
                            {
                                //TODO
                                for (int i = 0; i < _allTargets.Length; i++)
                                {
                                    if ((int)_allTargets[i].effectorType != (int)effectorType) continue;

                                    if (EffectorCheck(_allTargets[i].transform, i, _interactorObject.zOnly))
                                    {
                                        DrawDebugLines(_allTargets[i], objPlaceInList);
                                    }
                                }
                                break;
                            }
                        default:
                            break;
                    }
                }
            }

            //Starts or finishes avaiable manual interactions for this effector. Called by outside class (BasicInput class)
            public void StartStopInteractionThis(List<IntObjComponents> intOjbComponents, int selectedByUI, bool click)
            {
                //If this method called by mouse click. To seperate two different inputs (click and F)
                if (click)
                {
                    //Click is used just for DistanceCrosshair
                    if (intOjbComponents[selectedByUI].interactorObject.interactionType == InteractorObject.InteractionTypes.DistanceCrosshair)
                    {
                        //We dont want to enter here for all effectors, just need one.
                        if (effectorType != FullBodyBipedEffector.Body) return;

                        //Send VehiclePartController to animate its part via its animation state
                        if (_vehiclePartsActive)
                        {
                            _vehiclePartCont.Animate(intOjbComponents[selectedByUI].interactorObject.vehiclePartId, true);
                        }
                        intOjbComponents[selectedByUI].interactorObject.SendUnityEvent();
                        intOjbComponents[selectedByUI].interactorObject.Use(false);
                    }
                    return;
                }

                //Run F button selected interaction by UI selection. UI selection comes from BasicUI class.
                if (selectedByUI < intOjbComponents.Count)
                {
                    //This is for connected interactions, so they will disconnect and end.
                    if (effectors[_this].connected)
                    {
                        InteractorObject.InteractionTypes selectedIntType = intOjbComponents[selectedByUI].interactorObject.interactionType;
                        //Prevent using another interaction while having already one
                        if (intOjbComponents[selectedByUI].interactorObject != effectors[_this].connectedTo && focusedObjectIntType != selectedIntType)
                        {
                            //Permit some interactions (Picking one hand items while other hand is still using something else etc)
                            //Or using truck doors with crosshair while holding two handed pickup objects.
                            if (selectedIntType == InteractorObject.InteractionTypes.DistanceCrosshair || selectedIntType == InteractorObject.InteractionTypes.PickableOne || selectedIntType == InteractorObject.InteractionTypes.ManualRotator) return;

                            Disconnect(_this);
                            DisconnectThis();

                            //Update(intOjbComponents, -1, _sphereColWithRotScale);
                        }
                        //focus object is active interaction type
                        switch (focusedObjectIntType)
                        {
                            case InteractorObject.InteractionTypes.ManualButton:
                                //ManualButton interaction ends automatically in effector Update
                                break;

                            case InteractorObject.InteractionTypes.ManualSwitch:
                                //ManualSwitch interaction ends automatically in effector Update
                                break;

                            case InteractorObject.InteractionTypes.ManualRotator:
                                //Turn off InteractorObject usedBy, interaction ended.
                                effectors[_this].connectedTo.usedBy = false;
                                break;

                            case InteractorObject.InteractionTypes.ManualHit:
                                //ManualHit interaction ends automatically in effector Update
                                break;

                            case InteractorObject.InteractionTypes.DistanceCrosshair:
                                //DistanceCrosshair interaction ends automatically
                                break;

                            case InteractorObject.InteractionTypes.ClimbableLadder:
                                {
                                    //Climb interaction starts or ends with only LeftHand
                                    if (effectorType != FullBodyBipedEffector.LeftHand) return;
                                    //This is toggleable, since LeftHand connection is more complex 
                                    //(Connection can be already started in most cases because we're already touching ladder when we close)
                                    //To prevent bugs same codes goes to other side of StartStop below
                                    focusedObjectIntType = InteractorObject.InteractionTypes.ClimbableLadder;
                                    //If not climbind didnt started yet, start with usedBy so other effectors gonna start too.
                                    if (!effectors[_this].connectedTo.usedBy)
                                    {
                                        effectors[_this].connectedTo.usedBy = true;
                                        effectors[_this].connectedTo.Use(false);
                                        ReposClimbingPlayerBottom(effectors[_this].connectedTo);
                                        ReposClimbingPlayerTop(effectors[_this].connectedTo);
                                    }
                                    //If already climbing, end with usedBy same way
                                    else
                                    {
                                        effectors[_this].connectedTo.usedBy = false;
                                        Disconnect(_this);
                                        _interactorIK.StopAll();
                                    }
                                    break;
                                }
                            case InteractorObject.InteractionTypes.MultipleCockpit:
                                //Just disconnect all effectors, since we're using all of them (5 effectors)
                                _interactor.DisconnectAll();
                                //Toggling windshield anim if it is ProtoTruck
                                if (_vehiclePartsActive && PlayerState.singlePlayerState.enteredVehicle == _vehicleInput.gameObject)
                                {
                                    _vehiclePartCont.ToggleWindshield(false);
                                }
                                break;

                            case InteractorObject.InteractionTypes.PickableOne:
                                {
                                    //If this effector already using, and this object is not the one we're using, pass, else drop.
                                    if (effectors[_this].connectedTo != intOjbComponents[selectedByUI].interactorObject) return;

                                    if (effectors[_this].connectedTo.picked)
                                    {
                                        DisconnectThis();
                                    }
                                    Disconnect(_this);
                                    break;
                                }
                            case InteractorObject.InteractionTypes.PickableTwo:
                                {
                                    if (effectors[_this].connectedTo.picked)
                                    {
                                        effectors[_this].connectedTo.Drop();
                                    }
                                    Disconnect(_this);
                                    _interactorIK.StopInteraction(effectorType);
                                    break;
                                }
                            case InteractorObject.InteractionTypes.Push:
                                {
                                    PlayerState.singlePlayerState.playerPushing = false;
                                    effectors[_this].connectedTo.Use(false);
                                    effectors[_this].connectedTo.PushEnd();
                                    Disconnect(_this);
                                    _interactorIK.ResumeInteraction(effectorType);
                                    break;
                                }
                            default:
                                //Not implemented yet or unnecessary interaction calls drop here.
                                break;
                        }
                    }
                    //This part is mostly for interaction start by input. Since effector is not connected but is in position for a connection.
                    else
                    {
                        for (int j = 0; j < intOjbComponents[selectedByUI].interactorObject.childTargets.Length; j++)
                        {
                            if ((int)intOjbComponents[selectedByUI].interactorObject.childTargets[j].effectorType != (int)effectorType)
                                continue;

                            switch (intOjbComponents[selectedByUI].interactorObject.interactionType)
                            {
                                case InteractorObject.InteractionTypes.ManualButton:
                                    {
                                        //If already used or this effector cant use, pass. Else start interaction.
                                        if (intOjbComponents[selectedByUI].interactorObject.usedBy) continue;
                                        if (intOjbComponents[selectedByUI].interactorObject.useableEffectors[_this] == false) continue;

                                        focusedObjectIntType = InteractorObject.InteractionTypes.ManualButton;
                                        Connect(_this, intOjbComponents[selectedByUI].interactorObject, intOjbComponents[selectedByUI].interactorObject.childTargets[j]);
                                        //If used button is part of Vehicle
                                        if (_vehiclePartsActive)
                                        {
                                            _vehiclePartCont.Animate(intOjbComponents[selectedByUI].interactorObject.vehiclePartId, true);
                                        }
                                        
                                        intOjbComponents[selectedByUI].interactorObject.Use(false);
                                        intOjbComponents[selectedByUI].interactorObject.usedBy = true;
                                        intOjbComponents[selectedByUI].interactorObject.RotatePivot(_effectorWorldSpace);
                                        _interactorIK.StartInteraction(effectorType, intOjbComponents[selectedByUI].interactorObject.childTargets[j], intOjbComponents[selectedByUI].interactorObject);
                                        break;
                                    }
                                case InteractorObject.InteractionTypes.ManualSwitch:
                                    {
                                        if (intOjbComponents[selectedByUI].interactorObject.usedBy) continue;
                                        if (intOjbComponents[selectedByUI].interactorObject.useableEffectors[_this] == false) continue;

                                        focusedObjectIntType = InteractorObject.InteractionTypes.ManualSwitch;
                                        Connect(_this, intOjbComponents[selectedByUI].interactorObject, intOjbComponents[selectedByUI].interactorObject.childTargets[j]);
                                        intOjbComponents[selectedByUI].interactorObject.Use(false);
                                        intOjbComponents[selectedByUI].interactorObject.usedBy = true;
                                        _interactorIK.StartInteraction(effectorType, intOjbComponents[selectedByUI].interactorObject.childTargets[j], intOjbComponents[selectedByUI].interactorObject);
                                        break;
                                    }
                                case InteractorObject.InteractionTypes.ManualRotator:
                                    {
                                        if (intOjbComponents[selectedByUI].interactorObject.usedBy) continue;
                                        if (intOjbComponents[selectedByUI].interactorObject.useableEffectors[_this] == false) continue;

                                        focusedObjectIntType = InteractorObject.InteractionTypes.ManualRotator;
                                        Connect(_this, intOjbComponents[selectedByUI].interactorObject, intOjbComponents[selectedByUI].interactorObject.childTargets[j]);
                                        intOjbComponents[selectedByUI].interactorObject.Use(false);
                                        intOjbComponents[selectedByUI].interactorObject.usedBy = true;
                                        _interactorIK.StartInteraction(effectorType, intOjbComponents[selectedByUI].interactorObject.childTargets[j], intOjbComponents[selectedByUI].interactorObject);
                                        //Send interacted object to camera for locking Y rotation
                                        //Because ManualRotation uses Y axis for interact.
                                        FreeLookCam.LockCamY(intOjbComponents[selectedByUI].interactorObject.gameObject);
                                        break;
                                    }
                                case InteractorObject.InteractionTypes.ManualHit:
                                    {
                                        if (intOjbComponents[selectedByUI].interactorObject.usedBy) continue;
                                        if (intOjbComponents[selectedByUI].interactorObject.useableEffectors[_this] == false) continue;

                                        focusedObjectIntType = InteractorObject.InteractionTypes.ManualHit;
                                        Connect(_this, intOjbComponents[selectedByUI].interactorObject, intOjbComponents[selectedByUI].interactorObject.childTargets[j]);
                                        intOjbComponents[selectedByUI].interactorObject.Use(false);
                                        intOjbComponents[selectedByUI].interactorObject.usedBy = true;
                                        _interactorIK.StartInteraction(effectorType, intOjbComponents[selectedByUI].interactorObject.childTargets[j], intOjbComponents[selectedByUI].interactorObject);
                                        break;
                                    }
                                case InteractorObject.InteractionTypes.ClimbableLadder:
                                    {
                                        //Same with connected side of StartStopInteractionThis because its toggleable.
                                        if (effectorType != FullBodyBipedEffector.LeftHand) continue;

                                        focusedObjectIntType = InteractorObject.InteractionTypes.ClimbableLadder;

                                        if (!effectors[_this].connectedTo.usedBy)
                                        {
                                            effectors[_this].connectedTo.usedBy = true;
                                            effectors[_this].connectedTo.Use(false);
                                            ReposClimbingPlayerBottom(effectors[_this].connectedTo);
                                            ReposClimbingPlayerTop(effectors[_this].connectedTo);
                                        }
                                        else
                                        {
                                            effectors[_this].connectedTo.usedBy = false;
                                            Disconnect(_this);
                                            _interactorIK.StopAll();
                                        }
                                        break;
                                    }
                                case InteractorObject.InteractionTypes.MultipleCockpit:
                                    {
                                        //Check the Player State which is true when effector is in position
                                        if (!PlayerState.singlePlayerState.playerChangable) continue;

                                        //If any effectors connected, disconnect because we're gonna connect it.
                                        if (anyConnected) _interactor.DisconnectAll();

                                        _interactor.ConnectAll(intOjbComponents[selectedByUI].interactorObject.childTargets, intOjbComponents[selectedByUI].interactorObject);
                                        intOjbComponents[selectedByUI].interactorObject.SendUnityEvent();
                                        focusedObjectIntType = InteractorObject.InteractionTypes.MultipleCockpit;
                                        //Toggling windshield anim if it is ProtoTruck
                                        if (_vehiclePartsActive && PlayerState.singlePlayerState.enteredVehicle == _vehicleInput.gameObject)
                                        {
                                            _vehiclePartCont.ToggleWindshield(true);
                                        }
                                        break;
                                    }
                                case InteractorObject.InteractionTypes.PickableOne:
                                    {
                                        //If object already picked (maybe by other effectors), or this effector cant use, pass. Else connect. Picking will be done in effector Update.
                                        if (intOjbComponents[selectedByUI].interactorObject.picked) continue;
                                        if (intOjbComponents[selectedByUI].interactorObject.useableEffectors[_this] == false) continue;

                                        focusedObjectIntType = InteractorObject.InteractionTypes.PickableOne;
                                        Connect(_this, intOjbComponents[selectedByUI].interactorObject, intOjbComponents[selectedByUI].interactorObject.childTargets[j]);
                                        intOjbComponents[selectedByUI].interactorObject.Use(false);
                                        intOjbComponents[selectedByUI].interactorObject.picked = true;
                                        intOjbComponents[selectedByUI].interactorObject.RotatePivot(_effectorWorldSpace);
                                        _interactorIK.StartInteraction(effectorType, intOjbComponents[selectedByUI].interactorObject.childTargets[j], intOjbComponents[selectedByUI].interactorObject);
                                        break;
                                    }
                                case InteractorObject.InteractionTypes.PickableTwo:
                                    {
                                        //If this effector cant use or object has less than 2 useables, pass. Else connect.
                                        if (intOjbComponents[selectedByUI].interactorObject.useableEffectors[_this] == false) continue;
                                        if (intOjbComponents[selectedByUI].interactorObject.UseableEffectorCount() < 2) continue;

                                        focusedObjectIntType = InteractorObject.InteractionTypes.PickableTwo;
                                        Connect(_this, intOjbComponents[selectedByUI].interactorObject, intOjbComponents[selectedByUI].interactorObject.childTargets[j]);
                                        intOjbComponents[selectedByUI].interactorObject.Use(false);
                                        intOjbComponents[selectedByUI].interactorObject.pickable = true;

                                        PickableTwoRetarget(intOjbComponents[selectedByUI].interactorObject.childTargets[j].transform, intOjbComponents[selectedByUI].interactorObject.twoHandCloser);

                                        _interactorIK.StartInteraction(effectorType, intOjbComponents[selectedByUI].interactorObject.childTargets[j], intOjbComponents[selectedByUI].interactorObject);
                                        break;
                                    }
                                case InteractorObject.InteractionTypes.Push:
                                    {
                                        if (intOjbComponents[selectedByUI].interactorObject.useableEffectors[_this] == false) continue;
                                        if (intOjbComponents[selectedByUI].interactorObject.UseableEffectorCount() < 2) continue;

                                        focusedObjectIntType = InteractorObject.InteractionTypes.Push;
                                        Connect(_this, intOjbComponents[selectedByUI].interactorObject, intOjbComponents[selectedByUI].interactorObject.childTargets[j]);
                                        intOjbComponents[selectedByUI].interactorObject.Use(false);
                                        _interactorIK.StartInteraction(effectorType, intOjbComponents[selectedByUI].interactorObject.childTargets[j], intOjbComponents[selectedByUI].interactorObject);
                                        break;
                                    }
                                default:
                                    //Not yet implemented or unnecessary interaction calls drop here.
                                    break;
                            }
                        }
                    }
                }
                //This is similar and half implemented yet(Only works with ManualButtons). Runs when user select All Objects on UI.
                //Starts interactions of all possible effectors with all possible InteractorObjects(one per effector, obviously).
                else if (selectedByUI == intOjbComponents.Count)
                {
                    if (effectors[_this].connected)
                    {
                        switch (focusedObjectIntType)
                        {
                            case InteractorObject.InteractionTypes.ManualButton:
                                //ManualButton interaction ends automatically in effector Update
                                break;

                            default:
                                //Not yet implemented or unnecessary interaction calls drop here.
                                break;
                        }
                    }
                    else if (intOjbComponents.Count > 0)
                    {
                        for (int i = 0; i < intOjbComponents.Count; i++)
                        {
                            for (int j = 0; j < intOjbComponents[i].interactorObject.childTargets.Length; j++)
                            {
                                if ((int)intOjbComponents[i].interactorObject.childTargets[j].effectorType != (int)effectorType)
                                    continue;

                                if (effectors[_this].connected) continue;

                                switch (intOjbComponents[i].interactorObject.interactionType)
                                {
                                    case InteractorObject.InteractionTypes.ManualButton:
                                        {
                                            if (intOjbComponents[i].interactorObject.usedBy) continue;
                                            if (intOjbComponents[i].interactorObject.useableEffectors[_this] == false) continue;

                                            focusedObjectIntType = InteractorObject.InteractionTypes.ManualButton;
                                            if (_vehiclePartsActive)
                                            {
                                                _vehiclePartCont.Animate(intOjbComponents[i].interactorObject.vehiclePartId, true);
                                            }
                                            Connect(_this, intOjbComponents[i].interactorObject, intOjbComponents[i].interactorObject.childTargets[j]);
                                            intOjbComponents[i].interactorObject.Use(false);
                                            intOjbComponents[i].interactorObject.usedBy = true;
                                            intOjbComponents[i].interactorObject.RotatePivot(_effectorWorldSpace);
                                            _interactorIK.StartInteraction(effectorType, intOjbComponents[i].interactorObject.childTargets[j], intOjbComponents[i].interactorObject);
                                            break;
                                        }
                                    default:
                                        //Not yet implemented or unnecessary interaction calls drop here.
                                        break;
                                }
                            }
                        }
                    }
                    else
                        return;
                }
                else
                {
                    Debug.Log("UI selection error");
                }
            }

            public void ConnectThis(InteractorObject connectedTo)
            {
                focusedObjectIntType = connectedTo.interactionType;
                effectors[_this].connectedTo.Use(false);
                _interactorIK.StartInteraction(effectorType, effectors[_this].connectedTarget, effectors[_this].connectedTo);
            }

            //Ends interaction for this effector, called by Interactor with its Disconnect method
            public void DisconnectThis()
            {
                effectors[_this].connectedTo.Use(false);
                effectors[_this].connectedTo.usedBy = false;
                effectors[_this].connectedTo.Drop();
                if (effectors[_this].connectedTo.hasInteractiveRotator)
                {
                    effectors[_this].connectedTo.GetComponent<InteractiveRotator>().active = false;
                    FreeLookCam.LockCamY(effectors[_this].connectedTo.gameObject);
                }
                if (_interactorObject.hasAutomover)
                {
                    AutoMover auto = effectors[_this].connectedTarget.GetComponent<AutoMover>();
                    if (auto != null)
                    {
                        auto.ResetBools();
                    }
                }
                effectors[_this].connected = false;
                _interactorIK.StopInteraction(effectorType);
            }

            #region Debug Lines

            //Draws fainted lines for every interaction object's every same effector type target in sphere
            private void DrawDebugLines(InteractorTarget[] allTargets)
            {
#if UNITY_EDITOR
                if (_interactor.debug)
                {
                    for (int i = 0; i < allTargets.Length; i++)
                    {
                        if (allTargets[i].effectorType != effectorType) continue;

                        Debug.DrawLine(_effectorWorldSpace, allTargets[i].transform.position, ColorForArrayPlace(_this, false));
                    }
                }
#endif
            }
            //Draws possible interaction targets
            //Set by effector Update() when target passes the EffectorCheck()
            private void DrawDebugLines(InteractorTarget allTargets, int intObjPlaceInList)
            {
#if UNITY_EDITOR
                if (_interactor.debug)
                {
                    //If InteractorObjects list place equals to UI selection, set a position for 
                    //InteractorEditor to draw bezier.
                    if (intObjPlaceInList == _interactor.selectedByUI)
                    {
                        targetPosition = allTargets.transform.position;
                        targetActive = true;
                    }

                    Debug.DrawLine(_effectorWorldSpace, allTargets.transform.position, ColorForArrayPlace(_this, true));
                }
#endif
            }
            #endregion
        }
    }
}
