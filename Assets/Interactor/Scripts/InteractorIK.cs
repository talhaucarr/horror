using UnityEngine;

namespace razz
{
	public class InteractorIK : MonoBehaviour
	{
		public enum IKPart
		{
			LeftFoot = 0,
			RightFoot = 1,
			LeftHand = 2,
			RightHand = 3,
			Body = 4,
			//LeftShoulder = 5,
			//RightShoulder = 6,
			//LeftThigh = 7,
			//RightThigh = 8
		};

		/*public enum FullBodyBipedEffector
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
		}*/

		public IKParts[] ikParts;

		private Animator _anim;
		//Used for holding IKParts' ikpart values as effector type int (FullBodyBipedEffector)
		private int[] _effectorOrder;

		private void Start()
		{
			if (ikParts.Length == 0) return;

			_anim = GetComponent<Animator>();
			//10 long array is enough to hold all parts
			_effectorOrder = new int[10];

			for (int i = 0; i < ikParts.Length; i++)
			{
				ikParts[i].Init(_anim);

				if (ikParts[i].matchChildrenBones)
				{
					ikParts[i].childrenBones = ikParts[i].boneTransform.GetComponentsInChildren<Transform>();
                }
			}
			SetEffectorOrder();
		}

		private void Update()
		{
			if (ikParts.Length == 0) return;

			for (int i = 0; i < ikParts.Length; i++)
			{
				ikParts[i].UpdateIK();
			}
		}

		//Caching ikparts' part ints as FullBodyBipedEffector ints, so we dont have to check every call
		//which part is for which effector type
		private void SetEffectorOrder()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				_effectorOrder[AvatorGoalToEffector(ikParts[i].part)] = i;
			}
		}

		private int EffectorToIKpart(Interactor.FullBodyBipedEffector effector)
		{
			int i = _effectorOrder[(int)effector];

			if (ikParts[i] == null)
			{
				Debug.Log("Interactor has " + effector + ", but InteractorIK has not that part.");
				return -1;
			}
			return i;
		}

		public void StartInteraction(Interactor.FullBodyBipedEffector effector, InteractorTarget interactorTarget, InteractorObject interactorObject)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].pause = interactorObject.pauseOnInteraction;
			ikParts[i].easer = Ease.FromType(interactorObject.easeType);
			ikParts[i].interactorTarget = interactorTarget;
			ikParts[i].StartInteraction(interactorTarget.transform, interactorObject.interruptible);
		}

		public void PauseInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].PauseInteraction();
		}

		public void ResumeInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].ResumeInteraction();
		}

		public void ResumeAll()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].pause)
				{
					ikParts[i].ResumeInteraction();
				}
			}
		}

		public void ReverseInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].ReverseInteraction();
		}

		public void ReverseAll()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].enabled)
				{
					ikParts[i].ReverseInteraction();
				}
			}
		}

		public void StopInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].StopInteraction();
		}

		public void StopAll()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].enabled)
				{
					ikParts[i].StopInteraction();
				}
			}
		}

		public float GetProgress(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return 0;

			return ikParts[i].GetProgress();
		}

		public bool IsPaused(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return false;

			return ikParts[i].IsPaused();
		}

		public bool IsInInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return false;

			return ikParts[i].enabled;
		}

		public Transform GetBone(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return null;

			return ikParts[i].boneTransform;
		}

		public float GetDuration(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return 0;

			return ikParts[i].duration;
		}

		//Converting FullBodyBipedEffector int (Coming from Interactor) to AvatarGoal int (used by Unity IK)
		private int EffectorToAvatarGoal(Interactor.FullBodyBipedEffector effector)
		{
			switch ((int)effector)
			{
				case 0:
					return 4;
				case 1:
					return 5;
				case 2:
					return 6;
				case 3:
					return 7;
				case 4:
					return 8;
				case 5:
					return 2;
				case 6:
					return 3;
				case 7:
					return 0;
				case 8:
					return 1;
				default:
					return -1;
			}
		}
		//Converting AvatarGoal int (used by Unity IK) to FullBodyBipedEffector int (Coming from Interactor)
		private int AvatorGoalToEffector(IKPart part)
		{
			switch ((int)part)
			{
				case 0:
					return 7;
				case 1:
					return 8;
				case 2:
					return 5;
				case 3:
					return 6;
				case 4:
					return 0;
				case 5:
					return 1;
				case 6:
					return 2;
				case 7:
					return 3;
				case 8:
					return 4;
				default:
					return -1;
			}
		}

		private void SetAnimIK(IKPart part, Transform targetTransform, float weight)
		{
            //Unity AvatarGoals works only for 4 parts (Left & Right Hands, Feet)
            if ((int)part < 4)
            {
                _anim.SetIKPosition((AvatarIKGoal)part, targetTransform.position);
                _anim.SetIKPositionWeight((AvatarIKGoal)part, weight);

				//Rotations dealed in LateUpdate since we already know final target rotation.
                //_anim.SetIKRotation((AvatarIKGoal)part, targetTransform.rotation);
                //_anim.SetIKRotationWeight((AvatarIKGoal)part, weight);
                return;
            }
            //Body
            else if ((int)part == 4)
            {
                _anim.bodyPosition = targetTransform.position;
                return;
            }
        }

        public void SetLook(InteractorObject interactorObject, float weight)
        {
            _anim.SetLookAtPosition(interactorObject.transform.position);
            _anim.SetLookAtWeight(weight);
        }

		private void OnAnimatorIK(int layerIndex)
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].enabled && ikParts[i].targetTransform)
				{
					SetAnimIK(ikParts[i].part, ikParts[i].targetTransform, ikParts[i].weight);
				}
			}
		}

		private void LateUpdate()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].enabled && ikParts[i].targetTransform)
				{
					//We're changing bone rotation here instead of SetAnimIK because SetIKRotation needs
					//direction then it calculates target rotation, not goal rotation itself. 
					//We already have final bone rotation.
					if (ikParts[i].rotation)
					{
						ikParts[i].boneTransform.rotation = Quaternion.Lerp(ikParts[i].boneTransform.rotation, ikParts[i].targetTransform.rotation, ikParts[i].weight);
					}
					
					if (ikParts[i].matchChildrenBones)
					{
                        //This is for matching the effector bones to children bones of target. 
						//Its LateUpdate because it needs to be after Unity animation jobs done. 
                        //But it should be dealed with AvatarMask or some other way since this method 
						//still updates even matching done until interaction ends.
                        SetChildrenBones(ikParts[i]);
                    }
				}
			}
		}

        private void SetChildrenBones(IKParts ikpart)
        {
            if (ikpart.targetTransform == null || ikpart.interactorTarget == null || !ikpart.interactorTarget.matchChildrenBones) return;

            if (ikpart.childrenBones.Length != ikpart.interactorTarget.boneRotations.Length)
            {
                Debug.Log(ikpart.interactorTarget.name + " bone count doesnt match with effector bone.");
                return;
            }
            
            //Set first bone in the array is target itself. So we pass it to change others(children).
            for (int a = 1; a < ikpart.childrenBones.Length; a++)
            {
                ikpart.childrenBones[a].localRotation = Quaternion.Lerp(ikpart.childrenBones[a].localRotation, ikpart.interactorTarget.boneRotations[a], ikpart.weight);
            }
        }

		[System.Serializable]
		public class IKParts
		{
			public IKPart part;
			[Range(0, 1)]
			public float weight;
			public bool rotation;
			public Transform targetTransform;
			public float duration;
			public float multiplierForSecondHalf;
			public bool matchChildrenBones;

			[HideInInspector] public bool pause;
			[HideInInspector] public Transform boneTransform;
			[HideInInspector] public bool enabled;
			[HideInInspector] public bool interrupt;
			[HideInInspector] public Easer easer;
			[HideInInspector] public Transform[] childrenBones;
            [HideInInspector] public InteractorTarget interactorTarget;

			private AvatarIKGoal _avatarIKGoal;
			private float _elapsed;
			private bool _halfDone;
			private bool _interactReset;
			private float _lastWeightBeforeHalf;

			public void Init(Animator anim)
			{
				_avatarIKGoal = (AvatarIKGoal)part;
				boneTransform = anim.GetBoneTransform((HumanBodyBones)AvatarGoaltoHBB(_avatarIKGoal));

				//Setting default values for those shouldn't be zero
				if (multiplierForSecondHalf <= 0) multiplierForSecondHalf = 1f;
				if (duration <= 0) duration = 2f;
			}

			//Converts an int from Unity AvatarGoal value to Unity HumanBodyBones
			private int AvatarGoaltoHBB(AvatarIKGoal input)
			{
				switch ((int)input)
				{
					case 0:
						return 5;
					case 1:
						return 6;
					case 2:
						return 17;
					case 3:
						return 18;
					case 4:
						return 7;
					case 5:
						return 11;
					case 6:
						return 12;
					case 7:
						return 1;
					case 8:
						return 2;
					default:
						return -1;
				}
			}

			public void StartInteraction(Transform targetTransform, bool interrupt)
			{
				if (enabled && !interrupt) return;

				if (enabled)
				{
					ResetIK();
				}

				enabled = true;
				this.targetTransform = targetTransform;
				this.interrupt = interrupt;
			}

			public void PauseInteraction()
			{
				pause = true;
			}

			public void ResumeInteraction()
			{
				pause = false;
			}

			public void ReverseInteraction()
			{
				if (!_halfDone)
				{
					_halfDone = true;
					_elapsed = 0;
					pause = false;
				}
			}

			public void StopInteraction()
			{
				ResetIK();
				enabled = false;
			}

			public float GetProgress()
			{
				float calc;
				
				if (!_halfDone)
				{
					calc = _elapsed / duration;
				}
				else
				{
					float realduration = ((duration * 0.5f) + (duration * 0.5f * multiplierForSecondHalf));
					calc = (_elapsed + (duration * 0.5f)) / realduration;
				}
				return calc;
			}

			public bool IsPaused()
			{
				if (!_halfDone)
				{
					return false;
				}
				else
				{
					return pause;
				}
			}

			private void SetIK(Easer easer)
			{
				if (enabled)
				{
					if (_elapsed < (duration * 0.5f) && !_halfDone)
					{
						_elapsed = Mathf.MoveTowards(_elapsed, (duration * 0.5f), Time.deltaTime);
						weight = Mathf.Lerp(0, 1, easer(_elapsed / (duration * 0.5f)));
						_lastWeightBeforeHalf = weight;
					}
					else if (_elapsed >= (duration * 0.5f) && !_halfDone)
					{
						_halfDone = true;
						_elapsed = 0;
					}

					if (_elapsed < (duration * 0.5f * multiplierForSecondHalf) && _halfDone && !pause)
					{
						_elapsed = Mathf.MoveTowards(_elapsed, (duration * 0.5f * multiplierForSecondHalf), Time.deltaTime);
						weight = Mathf.Lerp(_lastWeightBeforeHalf, 0, easer(_elapsed / (duration * 0.5f * multiplierForSecondHalf)));
					}
					else if (_elapsed >= (duration * 0.5f * multiplierForSecondHalf) && _halfDone && !pause)
					{
						_interactReset = true;
					}

					if (_interactReset)
					{
						_elapsed = 0;
						enabled = false;
						_halfDone = false;
						_interactReset = false;
					}
				}
			}

			private void ResetIK()
			{
				_halfDone = false;
				_elapsed = 0;
				pause = false;
				weight = 0;
                interactorTarget = null;
                targetTransform = null;
            }

			public void UpdateIK()
			{
				if (!enabled) return;

				SetIK(easer);
			}
		}
	}
}
