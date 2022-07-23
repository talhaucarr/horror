using UnityEngine;

namespace _Scripts.CameraSystem
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        
        public Transform CameraTransform => cameraTransform;
    }
}
