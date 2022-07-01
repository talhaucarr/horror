using System;
using UnityEngine;

namespace Utilities
{
    public class ScaleChanger : MonoBehaviour
    {
        [SerializeField] private Vector3 scale;

        private void Awake()
        {
            if(scale == Vector3.zero) return;
            transform.localScale = scale;
        }

        public void ChangeScale(Vector3 vector3)
        {
            transform.localScale = vector3;
        }
    }
}
