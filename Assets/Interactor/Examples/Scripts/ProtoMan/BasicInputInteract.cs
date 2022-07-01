using UnityEngine;

namespace razz
{
    //Simplified BasicInput.cs for just starting some of interactions
    public class BasicInputInteract : MonoBehaviour
    {
        private Interactor _interactor;
        private bool _use;
        private bool _click;

        private void Start()
        {
            _interactor = FindObjectOfType<Interactor>();
        }

        private void Update()
        {
            if (!_use)
            {
                _use = Input.GetKeyDown(KeyCode.F);
            }

            if (!_click && PlayerState.singlePlayerState.playerUsable)
            {
                _click = Input.GetKeyDown(KeyCode.Mouse0);

                if (_click && !PlayerState.singlePlayerState.playerUsing)
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
        }

        private void FixedUpdate()
        {
            if (_use)
            {
                _interactor.StartStopInteractions(false);
            }

            if (_click)
            {
                _interactor.StartStopInteractions(true);
            }

            _use = false;
            _click = false;
        }
    }
}
