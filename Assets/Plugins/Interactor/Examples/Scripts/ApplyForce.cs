using UnityEngine;

namespace razz
{
    public class ApplyForce : MonoBehaviour
    {
        private Rigidbody _rb;
        private Vector3 _force = new Vector3(0, 1000f, 0);

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void ClickForce()
        {
            if (_rb)
            {
                _rb.AddForce(_force);
            }
        }
    }
}
