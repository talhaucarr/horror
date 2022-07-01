using UnityEngine;
using UnityEditor;

namespace razz
{
    [ExecuteInEditMode]
    public class MenuGUI : WindowGUI
    {
        private Interactor _interactor;
        private Interactor.EffectorLink _effectorlink;
        private bool _showFoldout;

        //Effector Data
        private string _effectorName;
        private Vector3 _posOffset;
        private float _angleXZ, _angleYZ, _angleOffset, _angleOffsetYZ, _radius, _opacity;

        //Styles
        private GUIStyle _horizontalsliderthumb;
        private GUIStyle _labelSceneview;
        private float _defaultThumbWidth;

        [HideInInspector] public bool enabled = false;
        public string WindowLabel => "Effector Settings";

        protected override void Window()
        {
            if (_interactor == null)
            {
                _interactor = GameObject.FindObjectOfType<Interactor>();
                if (_interactor == null)
                {
                    Disable();
                    return;
                }
            }
            if (_interactor.effectorLinks.Count == 0)
            {
                Disable();
                return;
            }

            _horizontalsliderthumb.fixedWidth = 30f;

            GUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("Slider");
            InteractorEditor.selectedEffectorTab = (int)Slider("", _interactor.selectedTab, 0, _interactor.effectorLinks.Count - 1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Interactor Tab Change");
                _interactor.selectedTab = InteractorEditor.selectedEffectorTab;
                GUI.FocusControl("Slider");

                if (InteractorTargetSpawner.Instance)
                {
                    InteractorTargetSpawner.Instance.Repainter();
                }
                InteractorEditor.Repainter();
            }

            _effectorlink = _interactor.effectorLinks[InteractorEditor.selectedEffectorTab];

            GUILayout.Space(12f);

            GUILayout.BeginHorizontal();
            GUILayout.Label(_interactor.effectorLinks[InteractorEditor.selectedEffectorTab].effectorName + " Position:", _labelSceneview);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            _posOffset = EditorGUILayout.Vector3Field("", _effectorlink.posOffset);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Interactor Position Change");
                _effectorlink.posOffset = _posOffset;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(10f);

            EditorGUI.BeginChangeCheck();
            _angleXZ = (int)Slider("H Angle: ", _effectorlink.angleXZ, 0f, 360f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Effector H.Angle Change");
                _effectorlink.angleXZ = _angleXZ;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();
            _angleOffset = (int)Slider("H Offset: ", _effectorlink.angleOffset, -180f, 180f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Effector H.Offset Change");
                _effectorlink.angleOffset = _angleOffset;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();
            _angleYZ = (int)Slider("V Angle: ", _effectorlink.angleYZ, 0f, 360f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Effector V.Angle Change");
                _effectorlink.angleYZ = _angleYZ;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();
            _angleOffsetYZ = (int)Slider("V Offset: ", _effectorlink.angleOffsetYZ, -180f, 180f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Effector V.Offset Change");
                _effectorlink.angleOffsetYZ = _angleOffsetYZ;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Dist:     " + _effectorlink.radius.ToString("F2"), _labelSceneview);
            _radius = Slider("", _effectorlink.radius, 0f, _interactor.sphereCol.radius, .01f);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_interactor, "Effector Distance Change");
                _effectorlink.radius = _radius;
                InteractorEditor.Repainter();
            }

            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            ToggleButton("Enabled:", ref _effectorlink.enabled);
            if (EditorGUI.EndChangeCheck())
            {
                //TODO Doesnt register correct Undo because value doesnt return like others.
                Undo.RecordObject(_interactor, "Effector Change");
            }

            EditorGUI.BeginChangeCheck();
            ToggleButton("Debug", ref _interactor.debug);
            if (EditorGUI.EndChangeCheck())
            {
                //TODO Doesnt register correct Undo because value doesnt return like others.
                Undo.RecordObject(_interactor, "Effector Debug Change");
            }
            Foldout("More", ref _showFoldout);
            GUILayout.EndHorizontal();

            if (_showFoldout)
            {
                GUILayout.Space(10f);
                EditorGUI.BeginChangeCheck();
                _effectorName = LabelField("Name: ", _effectorlink.effectorName);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_interactor, "Effector Name Change");
                    _effectorlink.effectorName = _effectorName;
                    if (InteractorTargetSpawner.Instance)
                    {
                        InteractorTargetSpawner.Instance.Repainter();
                    }
                    //TODO Doesnt register name change to Interactor tab because it needs to call RefreshTabNames()
                    InteractorEditor.Repainter();
                }

                GUILayout.Space(10f);

                _opacity = Slider("Opacity: ", InteractorEditor.opacityValue, 0f, 1f);
                InteractorEditor.opacityValue = _opacity;
                
                GUILayout.Space(10f);
            }

            _horizontalsliderthumb.fixedWidth = _defaultThumbWidth;
            if (GUI.changed)
            {
                EditorUtility.SetDirty(_interactor);
            }
        }

        private void Setup()
        {
            windowRect.width = 250;
            labelWidth = 90;
            sliderNumberWidth = 30;
            draggable = true;
        }

        public void Show()
        {
            enabled = !enabled;
            if (enabled)
            {
                GetStyles();
                Undo.undoRedoPerformed += UndoRedoRefresh;
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui += OnScene;
#else
                SceneView.onSceneGUIDelegate += OnScene;
#endif
                if (_interactor != null)
                {
                    Selection.activeTransform = _interactor.transform;
                }
                else
                {
                    _interactor = GameObject.FindObjectOfType<Interactor>();
                    Selection.activeTransform = _interactor.transform;
                }
            }
            else
            {
                Undo.undoRedoPerformed -= UndoRedoRefresh;
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui -= OnScene;
#else
                SceneView.onSceneGUIDelegate -= OnScene;
#endif
            }
        }

        public void Disable()
        {
            enabled = false;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif
            Undo.undoRedoPerformed -= UndoRedoRefresh;
        }

        private void UndoRedoRefresh()
        {
            SceneView.RepaintAll();
        }

        private void GetStyles()
        {
            _horizontalsliderthumb = Skin.GetStyle("horizontalsliderthumb");
            _defaultThumbWidth = _horizontalsliderthumb.fixedWidth;
            _labelSceneview = Skin.GetStyle("LabelSceneview");
        }

        public void OnScene(SceneView sceneView)
        {
            Setup();
            GUI.skin = Skin;
            windowRect = GUILayout.Window(0, windowRect, base.WindowBase, WindowLabel);

            if (windowRect.x < 0f)
            {
                windowRect.x = 0f;
            }
            if (windowRect.x + windowRect.width > sceneView.position.width)
            {
                windowRect.x = sceneView.position.width - windowRect.width;
            }
            if (windowRect.y < 17f)
            {
                windowRect.y = 17f;
            }
            if (windowRect.y + windowRect.height > sceneView.position.height)
            {
                windowRect.y = sceneView.position.height - windowRect.height;
            }
            if (windowRect.Contains(Event.current.mousePosition))
            {
                sceneView.Repaint();
            }

            if (!string.IsNullOrEmpty(base.windowTooltip))
            {
                Vector2 mouse = Input.mousePosition;
                mouse.y = Screen.height - mouse.y;
                GUI.Label(new Rect(50, Screen.height - 80, 1000, 1000), base.windowTooltip);
            }
        }
    }
}
