using UnityEngine;

namespace razz
{
    //Really basic input class but hey, its tricycle for kids! Dont let adults use it.
    [RequireComponent(typeof(BikeController))]
    public class BikeBasicInput : MonoBehaviour
    {
        private BikeController m_Bike;

        private void Awake()
        {
            m_Bike = GetComponent<BikeController>();
        }

        private void FixedUpdate()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            m_Bike.Move(h, v, v);
        }

        private void OnDisable()
        {
            m_Bike.Move(0, 0, 0);
        }
    }
}
