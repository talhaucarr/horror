using UnityEngine;

namespace razz
{
    public class InteractorTarget : MonoBehaviour
    {
        public Interactor.FullBodyBipedEffector effectorType;
        [Tooltip("Enable if you want to match each child bones rotation with effector bones.")]
        public bool matchChildrenBones = true;
        [Space(5f)]
        [Header("Override Settings")]
        [Tooltip("If this target has different parameters than Effectors, select this and fill parameters below.")]
        public bool overrideEffector;
        //TODO Only radius is available right now
        [Tooltip("Only distance for now")]
        public float overridenDistance = 0;

        [HideInInspector] public Quaternion[] boneRotations;
        [HideInInspector] public bool secondTheme;

        private void Start()
        {
            if (matchChildrenBones)
            {
                Transform[] _childrenTransforms = GetComponentsInChildren<Transform>();
                boneRotations = new Quaternion[_childrenTransforms.Length];

                for (int i = 0; i < _childrenTransforms.Length; i++)
                {
                    boneRotations[i] = _childrenTransforms[i].localRotation;
                }
            }
        }
    }
}
