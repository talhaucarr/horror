using UnityEngine;
using UnityEngine.Events;

namespace razz
{
    //You can add new tags here with category name and interaction name, then add it in InteractionTypes too
    public static class Tags
    {
        [TagField(categoryName = "")] public const int Unselected = 0;

        [TagField(categoryName = "Default")] public const int Animated = 10;
        [TagField(categoryName = "Manual")] public const int Button = 20;
        [TagField(categoryName = "Manual")] public const int Switch = 21;
        [TagField(categoryName = "Manual")] public const int Rotator = 22;
        [TagField(categoryName = "Manual")] public const int Hit = 23;
        [TagField(categoryName = "Manual")] public const int Force = 24;
        [TagField(categoryName = "Touch")] public const int Vertical = 30;
        [TagField(categoryName = "Touch")] public const int HorizontalUp = 31;
        [TagField(categoryName = "Touch")] public const int HorizontalDown = 32;
        [TagField(categoryName = "Touch")] public const int Still = 33;
        [TagField(categoryName = "Distance")] public const int CrossHair = 40;
        [TagField(categoryName = "Climbable")] public const int Ladder = 50;
        [TagField(categoryName = "Multiple")] public const int Cockpit = 60;
        [TagField(categoryName = "Self")] public const int Itch = 70;
        [TagField(categoryName = "Pickable")] public const int OneHand = 80;
        [TagField(categoryName = "Pickable")] public const int TwoHands = 81;
        [TagField(categoryName = "Push - Pull")] public const int Push = 90;
        [TagField(categoryName = "Cover")] public const int Stand = 100;
        [TagField(categoryName = "Cover")] public const int Crouch = 101;
    }

    //InteractorObject is handler class for all kinds of interactions. Since its all in one, it has a lot of cache. 
    //You can divide or delete unnecessary codes.
    public class InteractorObject : MonoBehaviour
    {
        public enum InteractionTypes
        {
            DefaultAnimated = Tags.Animated,
            ManualButton = Tags.Button,
            ManualSwitch = Tags.Switch,
            ManualRotator = Tags.Rotator,
            ManualHit = Tags.Hit,
            ManualForce = Tags.Force,
            TouchVertical = Tags.Vertical,
            TouchHorizontalUp = Tags.HorizontalUp,
            TouchHorizontalDown = Tags.HorizontalDown,
            TouchStill = Tags.Still,
            DistanceCrosshair = Tags.CrossHair,
            ClimbableLadder = Tags.Ladder,
            MultipleCockpit = Tags.Cockpit,
            SelfItch = Tags.Itch,
            PickableOne = Tags.OneHand,
            PickableTwo = Tags.TwoHands,
            Push = Tags.Push,
            CoverStand = Tags.Stand,
            CoverCrouch = Tags.Crouch
        };

        //TagFilter is custom attribute, it will give drop down menu with categories filled with Tags class
        //And return assigned int. You can remove it and use regular enum  property on inspector.
        [TagFilter(typeof(Tags))] public int interaction;
        [HideInInspector] public InteractionTypes interactionType;

        private void OnGUI()
        {
            //If no Tags (InteractionType in this case) selected, paint it red to highlight because we need it selected.
            if (interaction == 0)
                GUI.color = Color.red;
            else
                GUI.color = Color.white;
        }

        #region InteractorObject Variables

        //Core
        private Interactor _interactor;
        [HideInInspector] public InteractorTarget[] childTargets;
        [HideInInspector] public InteractorObject[] otherInteractorObjects;
        [Space(5f)]
        [Header("Options")]
        [Tooltip("If this interaction needs to pause on half.")] 
        public bool pauseOnInteraction;
        [Tooltip("If this interaction interruptible.")] 
        public bool interruptible;
        [Tooltip("Interaction animation easing, will change with AnimationCurve in the future")] 
        public EaseType easeType = EaseType.QuadIn;
        [Tooltip("If this object interactable without Y axis checks on effectors.")] 
        public bool zOnly;
        [HideInInspector] public bool usable;
        [HideInInspector] public bool used;
        [HideInInspector] public bool[] useableEffectors;
        [HideInInspector] public bool usedBy;

        //DeafultAnimated
        public float defaultAnimatedDistance = 1.8f;
        [Tooltip("Extra raycast to check if object has any obstacles between target and effector. Used by TouchStill and DefaultAnimated")]
        public bool obstacleRaycast;
        [Tooltip("If all targets arent child of this InteractorObject, select a parent who has all targets.")]
        public GameObject otherTargetsRoot;
        [Tooltip("If there is pivot object between target and parent, assign here to rotate itself to effector while interacting.")]
        public GameObject pivot;
        [Tooltip("Rotate pivot on X axis")]
        public bool pivotX = true;
        [Tooltip("Rotate pivot on Y axis")]
        public bool pivotY = true;
        [Tooltip("Rotate pivot on Z axis")]
        public bool pivotZ = true;
        [Tooltip("If highlighted material is another object, assign here. If it is this object leave empty.")]
        public MeshRenderer outlineMat;
        //Gets true automatically by VehiclePartControls if it has animation on Vehicle Animator
        [HideInInspector] public bool isVehiclePartwithAnimation;
        //Gets its hash id automatically by VehiclePartControls if it has animation on Vehicle Animator
        [HideInInspector] public int vehiclePartId;

        //TouchVertical, TouchHorizontal
        [Space(5f)]
        [Header("Touch Settings")]
        [Tooltip("Height amount for TouchVertical raycast")]
        public Vector3 touchHeight = new Vector3(0, 0.85f, 0);
        [Tooltip("Forward amount for TouchVertical raycast")]
        public float touchForward = 0.2f;
        [Tooltip("Target rotation for TouchVertical")]
        public float touchRotation = -50f;
        [Tooltip("Ray lenght for TouchVertical raycast")]
        public float touchRayLength = 0.45f;
        [Tooltip("Target lerp between raycast hit and effector position for TouchVertical")]
        [Range(0,1)]
        public float touchLerp = 0.06f;
        [Tooltip("Time needs to pass before TouchVertical starts, to prevent stutter.")]
        public float touchVCooldown = 0.2f;
        [Tooltip("Forward amount for TouchHorizontal raycast (Multiplied by 3 to check earlier if upper collider ends but puts target on this forward amount.)")]
        public float touchHorizontalForward = 0.3f;
        [Tooltip("Right amount for TouchHorizontal raycast")]
        public float touchHorizontalRight = 0.1f;
        [Tooltip("Ray lenght for TouchHorizontal raycast")]
        public float touchHorizontalRayLenght = 2f;
        [Tooltip("Time needs to pass before TouchHorizontal starts, to prevent stutter.")]
        public float touchHCooldown = 0.2f;

        private float _defaultTouchVCooldown;
        private float _defaultTouchHCooldown;

        //Pick, Push
        private Rigidbody _rigidbody;
        private bool _hasRigid;
        private Transform _parentTransform;
        private Vector3 _pickPos;
        private float _holdWeight, _holdWeightVel;
        private bool _pickDone;
        private bool _rotating;
        private Vector3 _rotateTo;
        private Vector3 _tempRotation;

        [HideInInspector] public Collider col;
        [HideInInspector] public bool picked;
        [HideInInspector] public bool pickable;
        [HideInInspector] public bool pushed;

        [Tooltip("Target lerp between raycast hit and effector position")]
        [Range(0, 1)]
        public float twoHandCloser;
        [Tooltip("Two handed object target position on player. It doesn't need any component but just transform to get position.")]
        public Transform holdPoint;
        

        //ManualHit
        private HitHandler _hitHandler;
        [HideInInspector] public bool hitObjUseable;
        [HideInInspector] public bool hitDone;

        //Outline Material
        private int _propertyIdFirstOutline;
        private int _propertyIdSecondOutline;
        private Color _firstOutline;
        private Color _secondOutline;
        private bool _hasOutlineMat;
        [HideInInspector] public Material thisMat;

        //AutoMovers
        [HideInInspector] public AutoMover[] autoMovers;
        [HideInInspector] public bool hasAutomover;

        //SelfInteraction
        [HideInInspector] public bool selfActive;
        [HideInInspector] public PathMover[] pathMovers;

        //Switches & Rotators
        [HideInInspector] public InteractiveSwitch[] interactiveSwitches;
        [HideInInspector] public bool hasInteractiveSwitch;
        [HideInInspector] public InteractiveRotator[] interactiveRotators;
        [HideInInspector] public bool hasInteractiveRotator;

        //Info Text
        public GameObject info;

        //Delete if not needed, If you want to use, call it in interactor with SendUnityEvent()
        [Space(10)]
        public UnityEvent unityEvent;
        #endregion

        public void SendUnityEvent()
        {
            if (unityEvent != null) unityEvent.Invoke();
        }

        private void Awake()
        {
            //Unselected InteractionType
            if (interaction == 0)
            {
                Debug.Log(this.name + " has InteractorObject with unselected Interaction Type!");
                gameObject.SetActive(false);
                return;
            }

            if (!(_interactor = FindObjectOfType<Interactor>())) return;

            interactionType = (InteractionTypes)interaction;
            useableEffectors = new bool[_interactor.effectorLinks.Count];

            //Outline
            if (outlineMat)
            {
                thisMat = outlineMat.material;
                if (thisMat.HasProperty("_FirstOutlineColor"))
                {
                    //Instead of strings, we cache ids, much faster.
                    _propertyIdFirstOutline = Shader.PropertyToID("_FirstOutlineColor");
                    _firstOutline = thisMat.GetColor(_propertyIdFirstOutline);

                    _propertyIdSecondOutline = Shader.PropertyToID("_SecondOutlineColor");
                    _secondOutline = thisMat.GetColor(_propertyIdSecondOutline);

                    _hasOutlineMat = true;

                    _firstOutline.a = 0;
                    _secondOutline.a = 0;
                    thisMat.SetColor(_propertyIdFirstOutline, _firstOutline);
                    thisMat.SetColor(_propertyIdSecondOutline, _secondOutline);
                }
            }
            else if (GetComponentInParent<MeshRenderer>())
            {
                thisMat = GetComponentInParent<MeshRenderer>().material;
                if (thisMat.HasProperty("_FirstOutlineColor"))
                {
                    //Instead of strings, we cache ids, much faster.
                    _propertyIdFirstOutline = Shader.PropertyToID("_FirstOutlineColor");
                    _firstOutline = thisMat.GetColor(_propertyIdFirstOutline);

                    _propertyIdSecondOutline = Shader.PropertyToID("_SecondOutlineColor");
                    _secondOutline = thisMat.GetColor(_propertyIdSecondOutline);

                    _hasOutlineMat = true;

                    _firstOutline.a = 0;
                    _secondOutline.a = 0;
                    thisMat.SetColor(_propertyIdFirstOutline, _firstOutline);
                    thisMat.SetColor(_propertyIdSecondOutline, _secondOutline);
                }
            }

            //Touch
            _defaultTouchHCooldown = touchHCooldown;
            _defaultTouchVCooldown = touchVCooldown;

            //Pick, Push
            if (_rigidbody = GetComponent<Rigidbody>())
                _hasRigid = true;

            if (!(col = GetComponent<Collider>()) && interactionType == InteractionTypes.PickableOne && interactionType == InteractionTypes.PickableTwo)
            {
                Debug.Log(this.name + " has no collider!");
            }

            _parentTransform = transform.parent;

            //Get all targets on children
            if (otherTargetsRoot != null)
            {
                childTargets = otherTargetsRoot.GetComponentsInChildren<InteractorTarget>();
                otherInteractorObjects = otherTargetsRoot.GetComponentsInChildren<InteractorObject>();
            }
            else
            {
                childTargets = GetComponentsInChildren<InteractorTarget>();
                otherInteractorObjects = GetComponentsInChildren<InteractorObject>();
            }

            if (interactionType == InteractionTypes.SelfItch)
            {
                pathMovers = GetComponentsInChildren<PathMover>();

                bool checkPathMovers = false;
                for (int i = 0; i < childTargets.Length; i++)
                {
                    if (pathMovers[i] == null)
                    {
                        checkPathMovers = true;
                    }
                }

                if (checkPathMovers == true)
                {
                    Debug.Log(this.name + " is self Interaction without Path Mover.");
                    return;
                }
            }

            autoMovers = GetComponentsInChildren<AutoMover>();
            if (autoMovers != null && autoMovers.Length > 0)
            {
                hasAutomover = true;
            }

            interactiveSwitches = GetComponentsInChildren<InteractiveSwitch>();
            if (interactiveSwitches != null && interactiveSwitches.Length > 0)
            {
                hasInteractiveSwitch = true;
            }

            interactiveRotators = GetComponentsInChildren<InteractiveRotator>();
            if (interactiveRotators != null && interactiveRotators.Length > 0)
            {
                hasInteractiveRotator = true;
            }

            if (interactionType == InteractionTypes.ClimbableLadder || interactionType == InteractionTypes.DistanceCrosshair || interactionType == InteractionTypes.MultipleCockpit || interactionType == InteractionTypes.ManualButton || interactionType == InteractionTypes.PickableOne || interactionType == InteractionTypes.PickableTwo)
            {
                usable = true;
            }

            //ManualHit
            _hitHandler = GetComponentInChildren<HitHandler>();

            //If there is a info for this object, assign it as GameObject, deactivating it here for to be activated with interactions.
            Info(false);
        }

        private void Start()
        {
            //Needs to be on start for InstantiateRandomAreaPool because it instantiates its children on awake. For ManualForce Truck Example.
            if (interactionType == InteractionTypes.ManualForce)
            {
                //If there is a InstantiateRandomAreaPool component, add its prefabs to childTargets array, since they arent parented.
                InstantiateRandomAreaPool _pool;
                if ((_pool = GetComponent<InstantiateRandomAreaPool>()))
                {
                    //If there are already child targets as children, add spawned prefabs with copying arrays.
                    if (childTargets.Length != 0)
                    {
                        InteractorTarget[] childTargetsCopy = new InteractorTarget[childTargets.Length + _pool.maxPrefabCount];

                        for (int i = 0; i < childTargets.Length; i++)
                        {
                            childTargetsCopy[i] = childTargets[i];
                        }

                        for (int j = 0; j < _pool.maxPrefabCount; j++)
                        {
                            childTargetsCopy[j + childTargets.Length] = _pool._prefabList[j].GetComponent<InteractorTarget>();
                        }

                        childTargets = childTargetsCopy;
                    }
                    else
                    {
                        InteractorTarget[] childTargetsCopy = new InteractorTarget[_pool.maxPrefabCount];

                        for (int j = 0; j < _pool.maxPrefabCount; j++)
                        {
                            childTargetsCopy[j] = _pool._prefabList[j].GetComponent<InteractorTarget>();
                        }

                        childTargets = childTargetsCopy;
                    }
                }
            }
        }

        //Main use method, used is true when used by effector
        public void Use(bool On)
        {
            if (!used && On)
            {
                Useable();
                used = true;
            }
            else if(used && On)
                return;

            if (used && !On)
            {
                NotUseable();
                used = false;
            }
            else if (!used && !On)
                return;
        }

        //Outline and info texts
        public void Useable()
        {
            if (_hasOutlineMat)
            {
                _firstOutline.a = 0.6f;
                _secondOutline.a = 0.4f;
                thisMat.SetColor(_propertyIdFirstOutline, _firstOutline);
                thisMat.SetColor(_propertyIdSecondOutline, _secondOutline);
            }
            Info(true);
        }
        //Outline and info texts
        public void NotUseable()
        {
            if (_hasOutlineMat)
            {
                _firstOutline.a = 0;
                _secondOutline.a = 0;
                thisMat.SetColor(_propertyIdFirstOutline, _firstOutline);
                thisMat.SetColor(_propertyIdSecondOutline, _secondOutline);
            }
            Info(false);
        }

        //Called by Interactor to rotate pivot if assigned any
        public void RotatePivot(Vector3 rotateTo)
        {
            if (pivot != null)
            {
                //If has a rigidbody and moving, we need to rotate target pivot until pick is done
                if (_hasRigid)
                {
                    if (!_rigidbody.IsSleeping())
                    {
                        //Bool for LateUpdate loop to rotate continuously and cache target to rotate
                        _rotating = true;
                        _rotateTo = rotateTo;
                    }
                }
                Rotate(rotateTo);
            }
        }
        private void Rotate(Vector3 target)
        {
            _tempRotation = Quaternion.LookRotation(target - transform.position, Vector3.up).eulerAngles;

            //If any axis not selected, get its own value, so dont rotate on that axis.
            if (!pivotX) _tempRotation.x = pivot.transform.eulerAngles.x;
            if (!pivotY) _tempRotation.y = pivot.transform.eulerAngles.y;
            if (!pivotZ) _tempRotation.z = pivot.transform.eulerAngles.z;

            //This rotates its InteractorTarget(with pivot) to the effector which started interaction
            //If there is a pivot, this is called at least one time or more if rigidbody is moving
            pivot.transform.eulerAngles = (_tempRotation);
        }

        //Returns how many effectors is in Use position for this object
        public int UseableEffectorCount()
        {
            int count = 0;
            for (int i = 0; i < useableEffectors.Length; i++)
            {
                if (useableEffectors[i])
                {
                    count++;
                }
            }
            return count;
        }
        //Resets useable array
        public void ResetUseableEffectors()
        {
            for (int i = 0; i < useableEffectors.Length; i++)
            {
                useableEffectors[i] = false;
            }
        }

        //Resets cooldown timers when touch starts
        public void ResetTouchCooldowns()
        {
            touchHCooldown = _defaultTouchHCooldown;
            touchVCooldown = _defaultTouchVCooldown;
        }

        //One or Two handed picks, two handed also uses late update loop on picking up
        public void Pick(Transform parentTransform, InteractorTarget childTarget)
        {
            if (_pickDone) return;

            if (_hasRigid)
            {
                _rigidbody.isKinematic = true;
                col.enabled = false;
            }

            if (interactionType == InteractionTypes.PickableTwo)
            {
                //Cache object position for pick up on LateUpdate
                _pickPos = transform.position;
            }
            else
            {
                //Since we dont have a Full Body IK, we're moving object depending on its child hand position
                //parentTransform is hand bone position
                transform.position += parentTransform.position - childTarget.transform.position;
                transform.rotation = parentTransform.localRotation;
                if (pivot) pivot.transform.localRotation = Quaternion.identity;
            }
            transform.parent = parentTransform;

            _holdWeight = 0f;
            _holdWeightVel = 0f;
            picked = true;
            _pickDone = true;
        }
        public void Drop()
        {
            transform.parent = _parentTransform;

            if (_hasRigid)
            {
                _rigidbody.isKinematic = false;
                col.enabled = true;
            }

            picked = false;
            ResetUseableEffectors();
            pickable = false;
            _pickDone = false;
        }

        //Only used by Push interaction
        public void PushStart(Transform parentTransform)
        {
            if (!pushed)
            {
                transform.parent = parentTransform.transform;
                //Child object with rigidbody works different on Unity versions(Even if it is kinematic)
                //So we dont set it false on newer versions, we dont need it on newer versions.
#if UNITY_2019_3_OR_NEWER
                if (_hasRigid) DestroyImmediate(_rigidbody);
#else
                if (_hasRigid) _rigidbody.isKinematic = false;
#endif
                pushed = true;
            }
        }
        public void PushEnd()
        {
            if (pushed)
            {
                transform.parent = _parentTransform;
#if UNITY_2019_3_OR_NEWER
#else
                if (_hasRigid) _rigidbody.isKinematic = true;
#endif
                used = false;
                pushed = false;
            }
        }

        //Only used by Hit interaction, explained in Interactor.EffectorLink Update
        public void Hit(Transform interactionTarget, Vector3 effectorPos)
        {
            if (hitObjUseable)
            {
                _hitHandler.HitHandlerRotate();
            }

            if (usedBy && !_hitHandler.moveOnce)
            {
                _hitHandler.moveOnce = true;
                hitDone = false;
                _hitHandler.HitPosMove(interactionTarget, effectorPos);
            }
        }
        //Only used by Hit interaction
        public void HitPosDefault(Transform interactionTarget, Vector3 effectorPos)
        {
            if (!_hitHandler.hitDone)
            {
                _hitHandler.HitPosDefault(interactionTarget, effectorPos);
            }
            else
            {
                hitDone = true;
            }
        }

        //You can delete this with its three references, just for info texts
        private void Info(bool on)
        {
            if (info != null)
            {
                if (on)
                    info.SetActive(true);
                else
                    info.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            //If this object is moving and targeted by Interactor, it needs to rotate until interaction is done
            if (_rotating)
            {
                if (_pickDone) _rotating = false;

                Rotate(_rotateTo);
            }

            //Only used by TwoHanded Pick
            if (picked)
            {
                if (holdPoint == null) return;

                _holdWeight = Mathf.SmoothDamp(_holdWeight, 1f, ref _holdWeightVel, 0.05f);
                transform.position = Vector3.Lerp(_pickPos, holdPoint.position, _holdWeight);
            }
        }
    }
}
