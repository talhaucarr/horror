using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace razz
{
    [CustomEditor(typeof(Interactor))]
    public class InteractorEditor : Editor
    {
        #region Variables
        private Interactor _script { get { return target as Interactor; } }
        private Interactor.EffectorLink _effectorlink;

        //GUI
        private string[] _effectorTabs;
        private bool[] _toggleBottomElements;
        private bool _defaultBottom = true;
        public static float opacityValue = 1f;
        public static int selectedEffectorTab;

        //Copy Paste jobs
        private bool _copiedTab;
        private bool _tempEnabled;
        private float _hRangeOff, _hRange, _vRangeOff, _vRange, _effectorDist;
        private Vector3 _effectorPos;

        //Styles
        private GUISkin _skin;
        private GUIStyle _background;
        private GUIStyle _smallButton;
        private GUIStyle _tabButtonStyle;
        private GUIStyle _addButton;
        private GUIStyle _deleteButton;
        private GUIStyle _copyButton;
        private GUIStyle _pasteButton;
        private GUIStyle _enableButton;
        private GUIStyle _textField;
        private GUIStyle _dropDown;
        private GUIStyle _hLine;
        private static GUIStyle _sceneText;
        private Texture2D _logoBig;
        private Texture2D _logoSmall;
        private Color _textColor = new Color(0.723f, 0.821f, 0.849f);
        private Color _textFieldColor = new Color(0.172f, 0.192f, 0.2f);
        private Color _GuiColor = new Color(0.71f, 0.82f, 0.88f, 0.96f);
        private Color _defaultGuiColor;
        private Color _defaultTextColor = new Color(0, 0, 0, 0); //Populated for OnEnable use to get default colors.
        private Color _defaultMiniButtonTextColor;
        private Color _defaultToolbarButtonTextColor;
        private Color _defaultTextFieldColor;
        private Color _changeHandleColor = new Color(0.65f, 0.65f, 0.65f);
        private float _verticalSpace = 4f;
        private float _logoSpace;
        private float _logoAreaSpace;
        private Rect _logo;
        private Rect windowRect;
        float windowWidth, windowY; //float windowX;

        //Installing Integrations
        private bool _installCore;
        private bool _installDefault;
        private bool _installFik;
        private bool _defaultFiles;

        //Temporary variables for calculations
        private static Vector3 _tempVecA, _tempVecB;
        private Color _tempColor;

        //Gizmo calculations
        private static Vector3 _colCenter;
        private static Vector3 _offsetCenter;
        private static Vector3 _offsetCenterRot;
        private static Vector3 _posOffsetWithRot;
        private static Vector3 _colCenterWithPos;
        private static Vector3 _colCenterWithScaleWithZMoved;
        private static Vector3 _colCenterWithScaleWithYMoved;
        private static List<Vector3> _arcPoints;
        private static Vector3 _tempPoint;
        private static float _colDiameter, _colDiameterZ, _colDiameterY, _pointDist, _pointAngle, _pointAngle2, _halfAngle, _angleDist, _angleEdge, _angleColDiameter, _angleRest, _edge, _angleSlice, _arcAngle, _topAngle;
        private static int _end, _midPointA, _midPointB;
        private static Vector3[] _midPoints, _midLine;
        private static string _halfLabel, _fullLabel;

        //Effector Data
        private bool _enabled, _debug, _logoChange;
        private string _effectorName;
        private Interactor.FullBodyBipedEffector _effectorType;
        private Vector3 _posOffset;
        private float _angleXZ, _angleYZ, _angleOffset, _angleOffsetYZ, _radius, _raycastDistance;
        #endregion

        private void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedoRefresh;
            GetStyles();
            RefreshTabNames();
            RefreshSpawnerWindowSoft();
            _arcPoints = new List<Vector3>();
            _midPoints = new Vector3[3];
            _midLine = new Vector3[2];
            _toggleBottomElements = new bool[4];
            _logoSpace = _logoBig.height + 10f;

#if UNITY_2019_1_OR_NEWER
            if (_script.debug) SceneView.duringSceneGui += ShowHandles;
#else
            if (_script.debug) SceneView.onSceneGUIDelegate += ShowHandles;
#endif

            SceneView.RepaintAll();

            if (_script.interactorVersion < 0.70f)
            {
                //TODO_UpdateTo_0.70 release version
                Debug.Log("Please update to the latest version. This version is for internal test. You may experience bugs.");
                Debug.Log("Textures sizes are for preview version. You can download 4K versions.");
            }
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoRefresh;
        }

        private void OnSceneGUI()
        {
            if (!_script.enabled) return;

            if (_script.effectorLinks.Count == 0) return;

            if (selectedEffectorTab >= _script.effectorLinks.Count)
            {
                selectedEffectorTab = _script.effectorLinks.Count - 1;
            }
        }

        #region Refresh and Style Methods
        private void GetStyles()
        {
            _skin = Resources.Load<GUISkin>("InteractorGUISkin");
            _logoBig = Resources.Load<Texture2D>("Images/TopLogoResized");
            _logoSmall = Resources.Load<Texture2D>("Images/TopLogoSmall");
            _background = _skin.GetStyle("BackgroundStyle");
            _smallButton = _skin.GetStyle("SmallButton");
            _tabButtonStyle = _skin.GetStyle("Button");
            _addButton = _skin.GetStyle("MiniButtonAddStyle");
            _deleteButton = _skin.GetStyle("MiniButtonDeleteStyle");
            _copyButton = _skin.GetStyle("MiniButtonCopyStyle");
            _pasteButton = _skin.GetStyle("MiniButtonPasteStyle");
            _enableButton = _skin.GetStyle("MiniButtonEnableStyle");
            _textField = _skin.GetStyle("TextField");
            _dropDown = _skin.GetStyle("DropDownField");
            _hLine = _skin.GetStyle("HorizontalLine");
            _sceneText = _skin.GetStyle("SceneText");
        }

        private void GetDefaultColors()
        {
            //These're throwing null exception in OnEnable so its cached this way.
            if (_defaultTextColor.a == 0)
            {
                _defaultTextColor = EditorStyles.label.normal.textColor;
                _defaultTextFieldColor = EditorStyles.textField.normal.textColor;
                _defaultMiniButtonTextColor = EditorStyles.miniButton.normal.textColor;
                _defaultToolbarButtonTextColor = EditorStyles.toolbarButton.normal.textColor;
            }
        }

        private void SetGUIColors()
        {
            EditorStyles.label.normal.textColor = _textColor;
            EditorStyles.miniButton.normal.textColor = _textFieldColor;
            EditorStyles.toolbarButton.normal.textColor = _textFieldColor;
            if (EditorGUIUtility.isProSkin)
                EditorStyles.textField.normal.textColor = _textColor;
            else
                EditorStyles.textField.normal.textColor = _textFieldColor;
        }

        private void ResetGUIColors()
        {
            EditorStyles.label.normal.textColor = _defaultTextColor;
            EditorStyles.textField.normal.textColor = _defaultTextFieldColor;
            EditorStyles.miniButton.normal.textColor = _defaultMiniButtonTextColor;
            EditorStyles.toolbarButton.normal.textColor = _defaultToolbarButtonTextColor;
        }

        public static void Repainter()
        {
            Editor[] ed = (Editor[])Resources.FindObjectsOfTypeAll<Editor>();
            for (int i = 0; i < ed.Length; i++)
            {
                if (ed[i].GetType() == typeof(InteractorEditor))
                {
                    (ed[i].target as Interactor).selectedTab = selectedEffectorTab;
                    ed[i].Repaint();
                }
            }
        }

        public void RefreshTabNames()
        {
            _effectorTabs = new string[_script.effectorLinks.Count];

            for (int i = 0; i < _script.effectorLinks.Count; i++)
            {
                if (_script.effectorLinks[i].effectorName == "")
                {
                    _script.effectorLinks[i].effectorName = i.ToString();
                }

                _effectorTabs[i] = _script.effectorLinks[i].effectorName;
            }
        }

        public void RefreshTabNames(string _name)
        {
            _effectorTabs[selectedEffectorTab] = _script.effectorLinks[selectedEffectorTab].effectorName;
        }

        private void RefreshSpawnerWindowSoft()
        {
            if (InteractorTargetSpawner.Instance)
            {
                InteractorTargetSpawner.Instance.Repainter();
            }
        }

        private void RefreshSpawnerWindowHard()
        {
            if (InteractorTargetSpawner.Instance)
            {
                InteractorTargetSpawner.Instance.RefreshTabs();
            }
            else
            {
                InteractorTargetSpawner Instance = EditorWindow.GetWindow<InteractorTargetSpawner>();
                InteractorTargetSpawner.Instance.RefreshTabs();
                Instance.Close();
            }
        }

        private void TopRightLinks()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
#if UNITY_2019_3_OR_NEWER
            GUILayout.Space(0f);
#else
            GUILayout.Space(2f);
#endif
            if (GUILayout.Button(new GUIContent(Links.onlineDocName, Links.onlineDocDesc), _smallButton))
            {
                Links.OnlineDocumentataion();
            }
            if (GUILayout.Button(new GUIContent(Links.forumName, Links.forumDesc), _smallButton))
            {
                Links.Forum();
            }
            if (GUILayout.Button(new GUIContent(Links.messageName, Links.messageDesc), _smallButton))
            {
                Links.Message();
            }
            if (GUILayout.Button(new GUIContent(Links.storeName, Links.storeDesc), _smallButton))
            {
                Links.Store();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void UndoRedoRefresh()
        {
            RefreshTabNames();
            SceneView.RepaintAll();
            RefreshSpawnerWindowSoft();
        }

        private void DrawBackground()
        {
#if UNITY_2019_3_OR_NEWER
            windowRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, windowY + 33f);
#else
            windowRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, windowY + 30f);
#endif
            GUI.Box(windowRect, "", _background);
        }

        private void ToggleBottomArea(int pushedButton)
        {
            _defaultBottom = false;

            for (int i = 0; i < _toggleBottomElements.Length; i++)
            {
                if (i == pushedButton)
                {
                    if (_toggleBottomElements[i])
                    {
                        _toggleBottomElements[i] = false;
                        _defaultBottom = true;
                        continue;
                    }
                    else
                    {
                        _toggleBottomElements[i] = true;
                        continue;
                    }
                }
                else
                    _toggleBottomElements[i] = false;
            }
        }

        private void BottomArea()
        {
            int active = 0;
            for (int i = 0; i < _toggleBottomElements.Length; i++)
            {
                if (_toggleBottomElements[i])
                {
                    active = i;
                }
            }

            _defaultGuiColor = GUI.color;
            GUI.color = _GuiColor;

            switch (active)
            {
                case 0:
#if UNITY_2019_3_OR_NEWER
                    GUILayout.Space(_verticalSpace - 1);
                    _script.selfInteractionObject = (GameObject)EditorGUILayout.ObjectField("Self Interaction (If exist):", _script.selfInteractionObject, typeof(GameObject), true);
                    GUILayout.Space(_verticalSpace - 1);
#else
                    GUILayout.Space(_verticalSpace);
                    _script.selfInteractionObject = (GameObject)EditorGUILayout.ObjectField("Self Interaction :", _script.selfInteractionObject, typeof(GameObject), true);
                    GUILayout.Space(_verticalSpace);
#endif
                    break;
                case 1:
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Space(_verticalSpace);
                    opacityValue = EditorGUILayout.Slider("Gizmo Opacity :", opacityValue, 0, 1f, GUILayout.Width(windowWidth));
                    GUILayout.Space(_verticalSpace);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(this, "Interactor Opacity");
                        SceneView.RepaintAll();
                    }
                    break;
                case 2:
                    GUILayout.Space(_verticalSpace);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Raycast Distance :");
                    EditorGUI.BeginChangeCheck();
                    _raycastDistance = EditorGUILayout.FloatField(_script.raycastDistance, GUILayout.Width(50f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_script, "Raycast Distance Change");
                        _script.raycastDistance = _raycastDistance;
                    }
                    EditorGUILayout.EndHorizontal();
#if UNITY_2019_3_OR_NEWER
                    GUILayout.Space(2f);
#else
                    GUILayout.Space(_verticalSpace);
#endif
                    break;
                case 3:
                    GUILayout.Space(_verticalSpace);
                    EditorGUILayout.BeginHorizontal();
                    if (WindowGUI.ButtonValue(Links.interactorScriptName, Links.interactorScriptDesc))
                    {
                        Links.InteractorScript();
                    }
                    if (WindowGUI.ButtonValue(Links.interactorEditorScriptName, Links.interactorEditorScriptDesc))
                    {
                        Links.InteractorEditorScript();
                    }
                    EditorGUILayout.EndHorizontal();
#if UNITY_2019_3_OR_NEWER
                    GUILayout.Space(2f);
#else
                    GUILayout.Space(5f);
#endif
                    break;
            }
            GUI.color = _defaultGuiColor;
        }
        #endregion

        public override void OnInspectorGUI()
        {
            GetDefaultColors();
            GUI.skin = _skin;
            DrawBackground();

            SetGUIColors();

            _logoChange = _script.logoChange;
            if (!_logoChange && _script.effectorLinks.Count != 0)
            {
                _logo = new Rect(1f, 4f, _logoBig.width, _logoBig.height);
                _logoSpace = _logoBig.height + 10f;
                _logoAreaSpace = _logoSmall.height - 16f;
                GUI.DrawTexture(_logo, _logoBig);
                TopRightLinks();

                if (Event.current.type == EventType.MouseUp && _logo.Contains(Event.current.mousePosition))
                {
                    _script.logoChange = !_logoChange;
                    EditorUtility.SetDirty(_script);
                    this.Repaint();
                }
            }
            else if (_script.effectorLinks.Count != 0)
            {
                _logo = new Rect(8f, 0, _logoSmall.width, _logoSmall.height);
                _logoSpace = _logoSmall.height;
                _logoAreaSpace = _logoBig.height + 12f; //16 is same space with big logo
                GUI.DrawTexture(_logo, _logoSmall);

                if (Event.current.type == EventType.MouseUp && _logo.Contains(Event.current.mousePosition))
                {
                    _script.logoChange = !_logoChange;
                    EditorUtility.SetDirty(_script);
                    this.Repaint();
                }
            }
            #region Front Page
            else
            {
                _logo = new Rect((EditorGUIUtility.currentViewWidth - _logoBig.width) * 0.5f, 3f, _logoBig.width, _logoBig.height);
                GUI.DrawTexture(_logo, _logoBig);
                TopRightLinks();
                GUILayout.Space(2f);

#if UNITY_2019_3_OR_NEWER
                windowY = 256f;
#else
                windowY = 245f;
#endif
                _tabButtonStyle.fixedWidth = EditorGUIUtility.currentViewWidth;

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                int a = _tabButtonStyle.border.left;
                _tabButtonStyle.border.left = 2;
                GUILayout.BeginArea(new Rect(0, _logoSpace, EditorGUIUtility.currentViewWidth + 20, 100f));
                if (GUILayout.Button("Start Interactor", _tabButtonStyle))
                {
                    _script.effectorLinks.Add(new Interactor.EffectorLink());
                    _script.effectorLinks[0].effectorName = "New Effector";
                    RefreshTabNames();
                    RefreshSpawnerWindowSoft();
                    SceneView.RepaintAll();
                }
                _tabButtonStyle.border.left = a;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.EndArea();

                GUILayout.Space(55f);

                if (windowRect.Contains(Event.current.mousePosition))
                {
                    this.Repaint();
                }

                GUILayout.Space(10f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Click Button to add your first effector and start.");
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(30f);
                EditorGUILayout.LabelField("Install Integrations", _hLine);

                CheckDefaultFiles();

                EditorGUILayout.BeginHorizontal();
                if (WindowGUI.ButtonValue("Final IK", "Before installing the integration, if you don't imported Final IK yet, import it first. Installation will look for default Final IK folder locations. You can change back to Interactor IK."))
                {
                    if (!_defaultFiles)
                    {
                        Debug.Log("Final IK integration already installed.");
                        return;
                    }
                    _installFik = true;
                }
                EditorGUI.BeginDisabledGroup(true);
                if (WindowGUI.ButtonValue("Unity Animation Rigging", "Since it is using Burst and they are both changing constantly (Preview packages), this will take time."))
                {
                    Debug.Log("Unity Animation Rigging is not supported yet because it is in early development.");
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (WindowGUI.ButtonValue("Default Interactor IK", "If you already installed Final IK integration, this will change back to default Interactor IK with its examples."))
                {
                    if (_defaultFiles)
                    {
                        Debug.Log("Default Interactor IK already installed.");
                        return;
                    }
                    _installDefault = true;
                }
                if (WindowGUI.ButtonValue("Delete Examples", "Delete examples folder (with everything inside), all example scripts and change core scripts to empty ones without any interactions. This action is irreversible, you need to reimport Interactor to go back."))
                {
                    if (EditorUtility.DisplayDialog("Warning!", "If you continue this operation, \"Assets/Interactor/Examples\" and \"Assets/Interactor/Integrations\" folders will be removed with all files within!\n\nIncluding your own files, all example scripts, all example models/textures/scenes and all example shaders.\n\nIf you need anything from that folder, move them to another folder first. Also all interaction loops and example dependencies will be removed from core scripts to clean start.\n\nThis action is irreversible, you need to reimport Interactor to go back.", "Remove Examples", "Abort"))
                    {
                        _installCore = true;
                    }
                    else
                    {
                        Debug.Log("Examples removal aborted.");
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10f);

                ResetGUIColors();

                if (Event.current.type == EventType.Repaint)
                {
                    InstallCore();
                    InstallDefault();
                    InstallFinalIK();
                }
                return;
            }
            #endregion

            GUILayout.Space(_logoSpace);

            #region Effector Page
#if UNITY_2019_3_OR_NEWER
            GUILayout.BeginArea(new Rect(0, _logoSpace, windowWidth + 23, 100f));
            EditorGUI.BeginChangeCheck();
            _tabButtonStyle.fixedWidth = (windowWidth + 23) / (_script.effectorLinks.Count);
#else
            GUILayout.BeginArea(new Rect(0, _logoSpace, windowWidth + 20, 100f));
            EditorGUI.BeginChangeCheck();
            _tabButtonStyle.fixedWidth = (windowWidth + 20) / (_script.effectorLinks.Count);
#endif
            selectedEffectorTab = GUILayout.Toolbar(_script.selectedTab, _effectorTabs, _tabButtonStyle);
            _effectorlink = _script.effectorLinks[selectedEffectorTab];
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Interactor Tab Change");
                if (selectedEffectorTab >= _script.effectorLinks.Count)
                {
                    selectedEffectorTab = _script.effectorLinks.Count - 1;
                }
                _script.selectedTab = selectedEffectorTab;
                GUI.FocusControl(null);
                SceneView.RepaintAll();
                RefreshSpawnerWindowSoft();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add New", _addButton))
            {
                _script.effectorLinks.Insert(selectedEffectorTab + 1, new Interactor.EffectorLink());
                _script.effectorLinks[selectedEffectorTab + 1].effectorName = "New Effector";
                RefreshTabNames();
                RefreshSpawnerWindowHard();
                if (selectedEffectorTab < _script.effectorLinks.Count)
                {
                    selectedEffectorTab++;
                    _script.selectedTab = selectedEffectorTab;
                }
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button("Delete This", _deleteButton))
            {
                _script.effectorLinks.RemoveAt(selectedEffectorTab);
                RefreshTabNames();
                if (_script.effectorLinks.Count >= 0)
                {
                    RefreshSpawnerWindowHard();
                }
                if (selectedEffectorTab > 0)
                {
                    selectedEffectorTab--;
                    _script.selectedTab = selectedEffectorTab;
                }
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button("Copy", _copyButton))
            {
                _tempEnabled = _script.effectorLinks[selectedEffectorTab].enabled;
                _effectorPos = _script.effectorLinks[selectedEffectorTab].posOffset;
                _hRange = _script.effectorLinks[selectedEffectorTab].angleXZ;
                _hRangeOff = _script.effectorLinks[selectedEffectorTab].angleOffset;
                _vRange = _script.effectorLinks[selectedEffectorTab].angleYZ;
                _vRangeOff = _script.effectorLinks[selectedEffectorTab].angleOffsetYZ;
                _effectorDist = _script.effectorLinks[selectedEffectorTab].radius;
                _copiedTab = true;
            }
            if (GUILayout.Button("Paste", _pasteButton))
            {
                if (_copiedTab)
                {
                    Undo.RecordObject(_script, "Interactor Effector Paste");
                    _script.effectorLinks[selectedEffectorTab].enabled = _tempEnabled;
                    _script.effectorLinks[selectedEffectorTab].posOffset = _effectorPos;
                    _script.effectorLinks[selectedEffectorTab].angleXZ = _hRange;
                    _script.effectorLinks[selectedEffectorTab].angleOffset = _hRangeOff;
                    _script.effectorLinks[selectedEffectorTab].angleYZ = _vRange;
                    _script.effectorLinks[selectedEffectorTab].angleOffsetYZ = _vRangeOff;
                    _script.effectorLinks[selectedEffectorTab].radius = _effectorDist;
                    EditorUtility.SetDirty(_script);
                    _copiedTab = false;
                }
                else
                {
                    Debug.Log("You need to copy from an effector first.");
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            _enabled = GUILayout.Toggle(_effectorlink.enabled, " Enabled", _enableButton);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Interactor Effector Change");
                GUI.FocusControl(null);
                _effectorlink.enabled = _enabled;
                SceneView.RepaintAll();
            }
            GUILayout.EndArea();

            GUILayout.Space(_logoAreaSpace);

            _defaultGuiColor = GUI.color;
            GUI.color = _GuiColor;

            EditorGUI.BeginChangeCheck();
            GUILayout.Space(_verticalSpace);
            _effectorName = EditorGUILayout.TextField("Name :", _effectorlink.effectorName, _textField);
            GUILayout.Space(_verticalSpace);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Interactor Name Change");
                _effectorlink.effectorName = _effectorName;
                RefreshTabNames(_effectorName);
                RefreshSpawnerWindowSoft();
                SceneView.RepaintAll();
            }

            if (_script.sphereCol == null) _script.sphereCol = _script.GetComponent<SphereCollider>();

            EditorGUI.BeginChangeCheck();
            _effectorType = (Interactor.FullBodyBipedEffector)EditorGUILayout.EnumPopup("Effector Type :", _effectorlink.effectorType, _dropDown);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector Type Change");
                _effectorlink.effectorType = _effectorType;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _posOffset = EditorGUILayout.Vector3Field("Effector Position :", _effectorlink.posOffset);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector Position Change");
                _effectorlink.posOffset = _posOffset;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _angleXZ = EditorGUILayout.Slider("Horizontal Angle :", _effectorlink.angleXZ, 0, 360f, GUILayout.Width(windowWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector H.Angle Change");
                _effectorlink.angleXZ = _angleXZ;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _angleOffset = EditorGUILayout.Slider("Horizontal Offset :", _effectorlink.angleOffset, -180f, 180f, GUILayout.Width(windowWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector H.Offset Change");
                _effectorlink.angleOffset = _angleOffset;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _angleYZ = EditorGUILayout.Slider("Vertical Angle :", _effectorlink.angleYZ, 0, 360f, GUILayout.Width(windowWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector V.Angle Change");
                _effectorlink.angleYZ = _angleYZ;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _angleOffsetYZ = EditorGUILayout.Slider("Vertical Offset :", _effectorlink.angleOffsetYZ, -180f, 180f, GUILayout.Width(windowWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector V.Offset Change");
                _effectorlink.angleOffsetYZ = _angleOffsetYZ;
                SceneView.RepaintAll();
            }

            GUILayout.Space(_verticalSpace);

            EditorGUI.BeginChangeCheck();
            _radius = EditorGUILayout.Slider("Effector Range Distance :", _effectorlink.radius, 0, _script.sphereCol.radius, GUILayout.Width(windowWidth));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Effector Distance Change");
                _effectorlink.radius = _radius;
                SceneView.RepaintAll();
            }
            GUI.color = _defaultGuiColor;

            GUILayout.Space(31f);

            EditorGUILayout.LabelField("Other Options", _hLine);

            EditorGUILayout.BeginHorizontal();
            if (WindowGUI.ButtonValue("Spawner Window", "If you like to edit main Interactor script, this goes into codes."))
            {
                Debug.Log("Please update to the latest version. This version is for internal test.");
            }
            if (WindowGUI.ButtonValue("Self Interaction", "Show Self Interaction object. If you want to use one, assign below."))
            {
                ToggleBottomArea(0);
            }
            if (WindowGUI.ButtonValue("Gizmo Opacity", "Show slider for Gizmo opacity."))
            {
                ToggleBottomArea(1);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (WindowGUI.ButtonValue("Raycast Distance", "Raycast lenght for distance interaction."))
            {
                ToggleBottomArea(2);
            }
            if (WindowGUI.ButtonValue("Codes", "Open Interactor or InteractorEditor scripts."))
            {
                ToggleBottomArea(3);
            }
            EditorGUILayout.EndHorizontal();

            if (_defaultBottom)
            {
                GUILayout.Space(26f);
            }
            else
            {
                BottomArea();
            }

            //
            //You can expose properties from Interactor (_script) here, you can use GUIStyles cached in GetStyles()
            //
            //Example
            //_effectorlink.effectorName = EditorGUILayout.TextField("Name :", _effectorlink.effectorName, _textField);
            //_effectorlink.enabled = GUILayout.Toggle(_effectorlink.enabled, " Enabled", _enableButton);
            //

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            _debug = GUILayout.Toggle(_script.debug, "Debug View ");
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Interactor Debug");
                _script.debug = _debug;

#if UNITY_2019_1_OR_NEWER
                if (_debug)
                    SceneView.duringSceneGui += ShowHandles;
                else
                    SceneView.duringSceneGui -= ShowHandles;
#else
                if (_debug)
                    SceneView.onSceneGUIDelegate += ShowHandles;
                else
                    SceneView.onSceneGUIDelegate -= ShowHandles;
#endif

                SceneView.RepaintAll();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (windowRect.Contains(Event.current.mousePosition))
            {
                this.Repaint();
            }

            ResetGUIColors();

            //Getting UI parameters to auto adjust
            if (Event.current.type == EventType.Repaint)
            {
                //windowX = GUILayoutUtility.GetLastRect().x;
                windowY = GUILayoutUtility.GetLastRect().y;
                windowWidth = GUILayoutUtility.GetLastRect().width;
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_script);
            }
            #endregion
        }

        #region Integrations and File Handling
        private void CheckDefaultFiles()
        {
            if (DefaultFiles.defaultFiles == 0)
            {
                _defaultFiles = true;
            }
            else
            {
                _defaultFiles = false;
            }
        }

        private void InstallCore()
        {
            if (!_installCore) return;

            if (InteractorTargetSpawner.Instance)
                InteractorTargetSpawner.Instance.Close();

            _installCore = false;
            if (!_defaultFiles)
            {
                Debug.Log("Before deleting example folders and changing to Core, please change integrations to Default(InteractorIK) first.");
                return;
            }

            string examplePath = "Assets/Interactor/Examples";
            if (System.IO.Directory.Exists(examplePath))
            {
                string corepath = "Assets/Interactor/Integrations/CorePack.unitypackage";
                if (!System.IO.File.Exists(corepath))
                {
                    Debug.Log("Integrations/CorePack not found. Aborting.");
                    return;
                }

                FileDeletion(examplePath, true);
                FileDeletion("Assets/Interactor/Scripts/Editor/DefaultFiles.cs", false);
                FileDeletion("Assets/Interactor/Scripts/InteractorHelpers/PathMover.cs", false);
                FileDeletion("Assets/Interactor/Scripts/InteractorIK.cs", false);
                AssetDatabase.ImportPackage(corepath, false);
                FileDeletion("Assets/Interactor/Integrations", true);
                AssetDatabase.Refresh();

                if (System.IO.Directory.Exists(examplePath))
                {
                    Debug.Log("Something went wrong.");
                }
                else
                {
                    Debug.Log("Examples and Integrations folders deleted and all example scripts removed successfully from core scripts.");
                }
            }
            else
            {
                Debug.Log("Examples folder not found on default path, can not proceed.");
            }
        }

        private void InstallDefault()
        {
            if (!_installDefault) return;

            if (InteractorTargetSpawner.Instance)
                InteractorTargetSpawner.Instance.Close();

            _installDefault = false;
            string defaultPack = "Assets/Interactor/Integrations/DefaultPack.unitypackage";
            if (System.IO.File.Exists(defaultPack))
            {
                //Remove FinalIK integrtion only files, since InteractorIK doesnt use them.
                RemoveFinalIKFiles();
                AssetDatabase.ImportPackage(defaultPack, false);
                AssetDatabase.Refresh();
                Debug.Log("Interactor IK successfully installed, integration changed back to default.");
            }
            else
            {
                Debug.Log("DefaultPack is missing in Integrations folder. Please reimport Integration Packs(Assets/Interactor/Integrations/) or do it manually.");
            }
        }

        private void InstallFinalIK()
        {
            if (!_installFik) return;

            if (InteractorTargetSpawner.Instance)
                InteractorTargetSpawner.Instance.Close();

            _installFik = false;
            string finalIKpath = "Assets/Plugins/RootMotion/FinalIK";
            string finalIKInteractionSystem = "Assets/Plugins/RootMotion/FinalIK/InteractionSystem/InteractionSystem.cs";
            string finalIKInteractionEffector = "Assets/Plugins/RootMotion/FinalIK/InteractionSystem/InteractionEffector.cs";
            string sourceIntegrationFile = "Assets/Interactor/Integrations/IntegrationInjection.cs";
            string FinalIKPack = "Assets/Interactor/Integrations/FinalIKPack.unitypackage";

            if (System.IO.Directory.Exists(finalIKpath))
            {
                if (!System.IO.File.Exists(finalIKInteractionSystem))
                {
                    Debug.Log(finalIKInteractionSystem + " not found to integrate. Aborting.");
                    return;
                }
                if (!System.IO.File.Exists(finalIKInteractionEffector))
                {
                    Debug.Log(finalIKInteractionEffector + " not found to integrate. Aborting.");
                    return;
                }
                if (!System.IO.File.Exists(sourceIntegrationFile))
                {
                    Debug.Log(sourceIntegrationFile + " not found to integrate. Aborting.");
                    return;
                }

                if (System.IO.File.Exists(FinalIKPack))
                {
                    //Remove InteractorIK only files, since FinalIK doesnt use them.
                    RemoveDefaultFiles();
                    //Integrate Interactor needed methods into FinalIK, if not done before.
                    IntegrationCodeInject(sourceIntegrationFile, 3, 24, 8, 2, 8, finalIKInteractionSystem, 199);
                    IntegrationCodeInject(sourceIntegrationFile, 25, 102, 8, 2, 30, finalIKInteractionEffector, 195);
                    AssetDatabase.ImportPackage(FinalIKPack, false);
                    AssetDatabase.Refresh();
                    Debug.Log("Final IK integration successfully installed.");
                }
                else
                {
                    Debug.Log("Package is missing. Please reimport Integration Packs(Assets/Interactor/Integrations/) or do it manually.");
                }
            }
            else
            {
                Debug.Log("FinalIK not found, if you already imported Final IK package in your project, try installing the integration manually. Located in Assets/Interactor/Integrations folder. Read Online Documentation for methods to add into FinalIK.");
            }
        }

        private void RemoveDefaultFiles()
        {
            string interactorTarget = "Assets/Interactor/Scripts/InteractorTarget.cs";
            string interactorTargetEditor = "Assets/Interactor/Scripts/Editor/InteractorTargetEditor.cs";
            string interactorIK = "Assets/Interactor/Scripts/InteractorIK.cs";

            FileDeletion(interactorTarget, false);
            FileDeletion(interactorTargetEditor, false);
            FileDeletion(interactorIK, false);
        }

        private void RemoveFinalIKFiles()
        {
            string interactorOverride = "Assets/Interactor/Scripts/InteractorOverride.cs";

            FileDeletion(interactorOverride, false);
        }

        private void FileDeletion(string path, bool folder)
        {
            if (folder)
            {
                if (System.IO.Directory.Exists(path))
                {
                    FileUtil.DeleteFileOrDirectory(path);
                    FileUtil.DeleteFileOrDirectory(path + ".meta");
                    Debug.Log(path + " folder removed.");
                }
                else
                {
                    Debug.Log(path + " folder not found.");
                }
            }
            else
            {
                if (System.IO.File.Exists(path))
                {
                    FileUtil.DeleteFileOrDirectory(path);
                    FileUtil.DeleteFileOrDirectory(path + ".meta");
                    Debug.Log(path + " removed.");
                }
                else
                {
                    Debug.Log(path + " not found.");
                }
            }
        }

        //Copy overloaded method(polymorphic) from integration source file to destination files
        //source path, source start line to copy, source end line to copy, char deletion start from lines to remove comment, char deletion count
        //source line to check destionation one if already installed before, destination path, destination empty line
        private void IntegrationCodeInject(string source, int sourceStartLine, int sourceEndLine, int removeFrom, int removeCount, int checkLine, string dest, int destStartLine)
        {
            //Source line count, since its starting from line 1
            string[] sLines = new string[sourceEndLine + 1];
            if (System.IO.File.Exists(source))
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(source);

                for (int i = 1; i <= sourceEndLine; i++)
                {
                    sLines[i] = reader.ReadLine();

                    //Comment removal, but leaves whitespaces
                    if (sLines[i].Length >= removeFrom + removeCount)
                    {
                        sLines[i] = sLines[i].Remove(removeFrom, removeCount);
                    }
                }
            }
            else
            {
                Debug.Log("Integration source file not found to inject Interactor method :" + dest);
                return;
            }

            if (System.IO.File.Exists(dest))
            {
                string[] dLines = System.IO.File.ReadAllLines(dest);
                int destEndLine = destStartLine + (sourceEndLine - sourceStartLine);

                //Check if FinalIK method injection already done before?
                //dLines start from 0 so its -1
                if (sLines[checkLine] == dLines[destStartLine + checkLine - sourceStartLine - 1])
                {
                    Debug.Log("Code injection for " + dest + " integration aborted since its already done on Line " + destStartLine);
                    return;
                }

                System.IO.StreamWriter writer = new System.IO.StreamWriter(dest);

                //Source writing starts from 0
                for (int i = 0; i < dLines.Length + (destEndLine - destStartLine); ++i)
                {
                    if (i < (destStartLine - 1))
                    {
                        writer.WriteLine(dLines[i]);
                    }
                    else if (i >= (destStartLine - 1) && i <= (destEndLine - 1))
                    {
                        writer.WriteLine(sLines[i - (destStartLine - 1) + sourceStartLine]);
                    }
                    else
                    {
                        writer.WriteLine(dLines[i - (destEndLine - destStartLine)]);
                    }
                }

                writer.Close();
                Debug.Log("Interactor overloaded method successfully injected into " + dest + " on line " + destStartLine);
            }
            else
            {
                Debug.Log("Final IK file not found to inject Interactor method :" + dest);
                return;
            }
        }
        #endregion

        #region Sphere & Handles
        private void ShowHandles(SceneView sceneView)
        {
            if (_script == null || _script.effectorLinks.Count == 0) return;
            if (!_script.effectorLinks[selectedEffectorTab].enabled) return;
            if (!_script.debug)
            {
#if UNITY_2019_1_OR_NEWER
                if (_script.debug) SceneView.duringSceneGui -= ShowHandles;
#else
                if (_script.debug) SceneView.onSceneGUIDelegate -= ShowHandles;
#endif
                return;
            }

            EditorGUI.BeginChangeCheck();
            _tempColor = HandleUtility.handleMaterial.color;
            _tempColor.a *= opacityValue;
            HandleUtility.handleMaterial.color = _tempColor * _changeHandleColor;
            //Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;

            _tempVecB = Handles.PositionHandle(_offsetCenterRot, Quaternion.LookRotation(_script.transform.forward));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_script, "Change Interactor Offset");
                _tempVecA = Quaternion.Inverse(_script.transform.rotation) * (_tempVecB - _script.transform.position - _colCenter);
                _script.effectorLinks[selectedEffectorTab].posOffset.x = Mathf.Round(_tempVecA.x * 1000f) / 1000f;
                _script.effectorLinks[selectedEffectorTab].posOffset.y = Mathf.Round(_tempVecA.y * 1000f) / 1000f;
                _script.effectorLinks[selectedEffectorTab].posOffset.z = Mathf.Round(_tempVecA.z * 1000f) / 1000f;
            }
            HandleUtility.handleMaterial.color = _tempColor;
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Active | GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.NonSelected | GizmoType.Selected)]
        private static void Gizmos(Interactor script, GizmoType gizmoType)
        {
            if (script.effectorLinks.Count > 0 && selectedEffectorTab < script.effectorLinks.Count)
            {
                Draw3dSphere(script.effectorLinks[selectedEffectorTab], selectedEffectorTab, script);
            }
        }

        private static void Draw3dSphere(Interactor.EffectorLink effectorLink, int index, Interactor script)
        {
            if (!script.debug) return;
            if (!script.effectorLinks[selectedEffectorTab].enabled) return;

            if (script.sphereCol == null)
                script.sphereCol = script.GetComponent<SphereCollider>();

            _colCenter = script.sphereCol.center;
            _colCenter = (_colCenter.x * script.transform.right) + (_colCenter.y * script.transform.up) + (_colCenter.z * script.transform.forward);

            _colDiameter = script.sphereCol.radius;

            _tempVecA = Vector3.ClampMagnitude(effectorLink.posOffset, _colDiameter - effectorLink.radius);

            effectorLink.posOffset.x = Mathf.Round(_tempVecA.x * 1000f) / 1000f;
            effectorLink.posOffset.y = Mathf.Round(_tempVecA.y * 1000f) / 1000f;
            effectorLink.posOffset.z = Mathf.Round(_tempVecA.z * 1000f) / 1000f;

            _posOffsetWithRot = effectorLink.posOffset;

            _colDiameterZ = Mathf.Sqrt((_colDiameter * _colDiameter) - (_posOffsetWithRot.x * _posOffsetWithRot.x));
            _colDiameterY = Mathf.Sqrt((_colDiameter * _colDiameter) - (_posOffsetWithRot.y * _posOffsetWithRot.y));

            _offsetCenter = _colCenter + script.transform.position + effectorLink.posOffset;
            _offsetCenterRot = script.transform.position + _colCenter + (script.transform.right * _posOffsetWithRot.x) + (script.transform.forward * _posOffsetWithRot.z) + (script.transform.up * _posOffsetWithRot.y);

            _colCenterWithPos = (_colCenter + script.transform.position);

            _colCenterWithScaleWithZMoved = _colCenterWithPos + (script.transform.right * _posOffsetWithRot.x);
            _colCenterWithScaleWithYMoved = _colCenterWithPos + (script.transform.up * _posOffsetWithRot.y);

            Handles.Label(_offsetCenterRot, effectorLink.effectorName);

            if (effectorLink.targetActive && Application.isPlaying)
            {
                float dist = Vector3.Distance(effectorLink.targetPosition, _offsetCenterRot);
                Vector3 tangents = (effectorLink.targetPosition + _offsetCenterRot + new Vector3(0, dist * 0.5f, 0)) / 2;

                Handles.DrawBezier(effectorLink.targetPosition, _offsetCenterRot, tangents, tangents, Color.white, null, 10f);
            }

            //Blue horizontal circle for Z plane
            Handles.color = new Color(0, 0, 1, 0.02f * opacityValue);
            Handles.DrawSolidDisc(script.transform.position + _colCenter + (script.transform.up * _posOffsetWithRot.y), script.transform.up, _colDiameterY);
            Handles.color = new Color(0, 0, 1, 0.25f * opacityValue);
            Handles.DrawWireDisc(script.transform.position + _colCenter + (script.transform.up * _posOffsetWithRot.y), script.transform.up, _colDiameterY);

            //Horizontal origin
            Handles.color = new Color(0.3f, 0.3f, 1, 0.7f * opacityValue);
            Handles.DrawWireDisc(_colCenter + script.transform.position, script.transform.up, _colDiameter);
            Handles.DrawDottedLine(script.transform.position + _colCenter + script.transform.forward * -_colDiameter, script.transform.position + _colCenter + script.transform.forward * _colDiameter, 2f);
            Handles.DrawDottedLine(script.transform.position + _colCenter + script.transform.right * -_colDiameter, script.transform.position + _colCenter + script.transform.right * _colDiameter, 4f);

            Handles.color = new Color(0, 0, 1, 0.1f * opacityValue);

            //Blue effector radius
            Handles.DrawSolidArc(_offsetCenterRot,
                script.transform.up,
                Quaternion.AngleAxis(effectorLink.angleOffset - 90f, script.transform.up) * script.transform.forward,
                effectorLink.angleXZ,
                effectorLink.radius);

            //Green vertical circle for Y plane
            Handles.color = new Color(0, 1, 0, 0.02f * opacityValue);
            Handles.DrawSolidDisc(script.transform.position + _colCenter + (script.transform.right * _posOffsetWithRot.x), script.transform.right, _colDiameterZ);
            Handles.color = new Color(0, 1, 0, 0.25f * opacityValue);
            Handles.DrawWireDisc(script.transform.position + _colCenter + (script.transform.right * _posOffsetWithRot.x), script.transform.right, _colDiameterZ);

            //Vertical origin
            Handles.color = new Color(0.3f, 1, 0.3f, 0.5f * opacityValue);
            Handles.DrawWireDisc(_colCenter + script.transform.position, script.transform.right, _colDiameter);
            Handles.DrawDottedLine(script.transform.position + _colCenter + script.transform.up * -_colDiameter, script.transform.position + _colCenter + script.transform.up * _colDiameter, 4f);
            Handles.DrawDottedLine(script.transform.position + _colCenter + script.transform.forward * -_colDiameter, script.transform.position + _colCenter + script.transform.forward * _colDiameter, 4f);

            Handles.color = new Color(0, 1, 0, 0.1f * opacityValue);
            //Green effector radius
            Handles.DrawSolidArc(_offsetCenterRot,
                script.transform.right,
                Quaternion.AngleAxis(effectorLink.angleOffsetYZ - 90f, script.transform.right) * script.transform.forward,
                effectorLink.angleYZ,
                effectorLink.radius);

            //Green effector disk
            Color handleColor = new Color(0.5f, 1, 0.5f, 0.1f * opacityValue);
            DrawTriggerDisk(effectorLink,
                _offsetCenter,
                _offsetCenterRot,
                _colCenterWithPos,
                _colCenterWithScaleWithZMoved,
                _colDiameterZ,
                effectorLink.angleOffsetYZ,
                effectorLink.angleYZ,
                script.transform.right,
                script.transform.forward,
                handleColor,
                true, script);
            //Blue effector disk
            handleColor = new Color(0.5f, 0.5f, 1, 0.1f * opacityValue);
            DrawTriggerDisk(effectorLink,
                _offsetCenter,
                _offsetCenterRot,
                _colCenterWithPos,
                _colCenterWithScaleWithYMoved,
                _colDiameterY,
                effectorLink.angleOffset,
                effectorLink.angleXZ,
                script.transform.up,
                script.transform.forward,
                handleColor,
                false, script);
        }

        private static void DrawTriggerDisk(Interactor.EffectorLink effectorLink,
            Vector3 offsetCenter,
            Vector3 offsetCenter2,
            Vector3 colCenterWithPos,
            Vector3 colCenterWithMovedScale,
            float colDiameterZ,
            float angleOffset,
            float angleAxis,
            Vector3 axis,
            Vector3 axis2,
            Color handleColor,
            bool vertical, Interactor script)
        {
            Handles.color = handleColor;
            if (_arcPoints == null)
            {
                _arcPoints = new List<Vector3>();
                _midPoints = new Vector3[3];
                _midLine = new Vector3[2];
                _sceneText = Resources.Load<GUISkin>("InteractorGUISkin").GetStyle("SceneText");
            }
            _arcPoints.Clear();
            _arcPoints.Add(offsetCenter2);
            _pointDist = Vector3.Distance(offsetCenter2, colCenterWithMovedScale);


            _halfAngle = angleAxis * 0.5f;
            _end = Mathf.CeilToInt(angleAxis / 5);
            if (_end < 3)
                _end = 3;

            for (int i = 0; i < _end; i++)
            {
                _angleSlice = (angleAxis / (_end - 1));
                _arcAngle = _angleSlice * i;
                _topAngle = _arcAngle + angleOffset;
                if (_topAngle >= 360 || _topAngle == 180)
                {
                    _topAngle -= 0.001f;
                }
                else if (_topAngle == 0 || _topAngle == -180)
                {
                    _topAngle += 0.001f;
                }
                _angleRest = 180 - _topAngle;

                if (vertical)
                {   //For vertical disk quarters
                    _pointAngle2 = Vector3.Angle(offsetCenter2 - colCenterWithMovedScale, script.transform.up);
                    //Left Bottom
                    if (offsetCenter.z > (colCenterWithPos.z))
                    {
                        _angleColDiameter = _angleRest + _pointAngle2;
                    }
                    //Right Bottom
                    else
                    {
                        _angleColDiameter = _angleRest - _pointAngle2;
                    }
                }
                else
                {
                    //For horizontal disk quarters
                    _pointAngle = Vector3.Angle(colCenterWithMovedScale - offsetCenter2, script.transform.right);

                    if (offsetCenter.z > (colCenterWithPos.z))
                    {
                        _angleColDiameter = _angleRest + _pointAngle;
                    }
                    else
                    {
                        _angleColDiameter = _angleRest - _pointAngle;
                    }
                }

                _angleDist = Mathf.Asin(_pointDist * Mathf.Sin(Mathf.Deg2Rad * _angleColDiameter) * (1 / colDiameterZ));
                _angleDist = Mathf.Rad2Deg * _angleDist;
                _angleEdge = (180 - _angleDist - _angleColDiameter);
                _edge = colDiameterZ * Mathf.Sin(Mathf.Deg2Rad * _angleEdge) * (1 / Mathf.Sin(Mathf.Deg2Rad * _angleColDiameter));
                _tempPoint = offsetCenter2 + (Quaternion.AngleAxis(_topAngle - 90f, axis) * axis2 * _edge);
                _arcPoints.Add(_tempPoint);

                //For debugging gizmo
                //Handles.Label(tempPoint, i.ToString());


                /*//Draw fading out side lines for middle line
                Vector3[] testLine = new Vector3[2];

                if (i <= _end * 0.5f && i > (_end * 0.5f) - 5)
                {
                    testLine[0] = _arcPoints[0];
                    testLine[1] = tempPoint;
                    float i_f = i * i;
                    float end_f = _end * _end;
                    float alpha = i_f / (end_f);
                    Handles.color = new Color(1, handleColor.g * 0.5f, handleColor.b * 0.5f, alpha * 0.5f * opacityValue);
                    Handles.DrawAAPolyLine(testLine);
                }
                else if(i > _end * 0.5f && i < (_end * 0.5f) + 4)
                {
                    testLine[0] = _arcPoints[0];
                    testLine[1] = tempPoint;
                    float i_f = i * i;
                    float end_f = _end * _end;
                    float alpha = ((_end - i - 1) * (_end - i - 1) / (end_f));
                    Handles.color = new Color(1, handleColor.g * 0.5f, handleColor.b * 0.5f, alpha * 0.5f * opacityValue);
                    Handles.DrawAAPolyLine(testLine);
                }
                Handles.color = handleColor;
                */
            }
            _arcPoints.Add(offsetCenter2);
            Handles.DrawAAConvexPolygon(_arcPoints.ToArray());

            //Draw middle polygon, middle line and its angle
            float _endFloat = _end;
            _midPoints[0] = _arcPoints[0];
            if ((_endFloat * 0.5f) % 1 != 0.5f)
            {
                _midPointA = Mathf.CeilToInt(_endFloat * 0.5f);
                _midPointB = _midPointA + 1;
                _midPoints[1] = _arcPoints[_midPointA];
                _midPoints[2] = _arcPoints[_midPointB];
            }
            else
            {
                _midPointA = Mathf.CeilToInt(_endFloat * 0.5f) - 1;
                _midPointB = _midPointA + 1;
                _midPoints[1] = (_arcPoints[_midPointA] + _arcPoints[_midPointB]) * 0.5f;
                _midPoints[2] = (_arcPoints[_midPointB] + _arcPoints[_midPointB + 1]) * 0.5f;
            }
            _midLine[0] = _midPoints[0];
            _midLine[1] = (_midPoints[1] + _midPoints[2]) * 0.5f;

            handleColor *= 0.5f;
            handleColor.a = 0.2f * opacityValue;
            Handles.color = handleColor;
            Handles.DrawAAConvexPolygon(_midPoints);
            Handles.color = new Color(1f, 1f, 1f, 0.2f * opacityValue);
            Handles.DrawAAPolyLine(_midLine);

            _halfLabel = (_halfAngle).ToString();
            Handles.Label(_midLine[1], _halfLabel, _sceneText);
            _fullLabel = (_halfAngle * 2).ToString();
            Handles.Label(_arcPoints[1], "0", _sceneText);
            Handles.Label(_arcPoints[_arcPoints.Count - 2], _fullLabel, _sceneText);
        }
        #endregion
    }
}
