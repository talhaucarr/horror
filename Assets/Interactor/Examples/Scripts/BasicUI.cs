using UnityEngine;

namespace razz
{
    public class BasicUI : MonoBehaviour
    {
        private Interactor _interactor;
        private Texture2D _crosshair;
        private GUISkin _skin;
        private GUIStyle _line, _label, _scenelabel;

        public int selected = 0;
        public bool crosshairEnable = true;
        
        private void Start()
        {
            _interactor = FindObjectOfType<Interactor>();
            _skin = Resources.Load<GUISkin>("InteractorGUISkin");
            _line = _skin.GetStyle("HorizontalLine");
            _label = _skin.GetStyle("label");
            _scenelabel = _skin.GetStyle("LabelSceneview");
            _crosshair = new Texture2D(2,2);
        }

        private void Update()
        {
            if (_interactor == null) return;

            int i = 0;
            if (_interactor.selfInteractionEnabled)
            {
                i = 1;
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if(_interactor.selectedByUI < _interactor.intOjbComponents.Count)
                _interactor.selectedByUI++;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if (_interactor.selectedByUI > i)
                    _interactor.selectedByUI--;
            }
        }

        private void OnGUI()
        {
            if (crosshairEnable)
            {
                Crosshair();
            }

            if (_interactor == null) return;
            if (_interactor.intOjbComponents.Count <= 0) return;
            if (_interactor.selfInteractionEnabled && _interactor.intOjbComponents.Count <= 1) return;

            ShowIntObjList();
        }

        private void Crosshair()
        {
            GUILayout.BeginArea(new Rect((Screen.width * 0.5f) - 1f, (Screen.height * 0.5f) - 1f, (Screen.width * 0.5f) + 1f, (Screen.height * 0.5f) + 1f));
            GUILayout.Label(_crosshair);
            GUILayout.EndArea();
        }

        private void ShowIntObjList()
        {
            GUILayout.Space(10f);
            GUILayout.Label("  Interaction Objects in Range (Closest Sorted): ", _scenelabel);
            GUILayout.Space(3f);
            GUILayout.Label("", _line);

            int i = 0;
            if (_interactor.selfInteractionEnabled)
            {
                i = 1;
            }
            
            while (i <= _interactor.intOjbComponents.Count)
            {
                GUI.color = Color.white;

                if (i == _interactor.selectedByUI)
                {
                    GUI.color = Color.red;
                }

                if (i == _interactor.intOjbComponents.Count)
                {
                    if (i > 1)
                    GUILayout.Label("\n  ALL Objects", _scenelabel);
                }
                else
                {
                    GUILayout.Label("\n  " + _interactor.intOjbComponents[i].interactorObject.name, _scenelabel);

                    if (_interactor.intOjbComponents[i].interactorObject.used && i == _interactor.selectedByUI)
                    {
                        GUI.color = Color.white;

                        if (_interactor.intOjbComponents[i].interactorObject.interactionType == InteractorObject.InteractionTypes.DistanceCrosshair)
                        {
                            GUI.Label(new Rect(Screen.width / 2 - 40, Screen.height / 3 * 2, 150, 50), "Left Click to Use", _label);
                        }
                        else
                        {
                            GUI.Label(new Rect(Screen.width / 2 - 40, Screen.height / 3 * 2, 150, 50), "Press F to Use", _label);
                        }
                    }
                }

                i++;
            }
        }
    }
}
