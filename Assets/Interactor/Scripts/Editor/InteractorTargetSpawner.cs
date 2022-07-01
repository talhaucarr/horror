using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace razz
{
    public class InteractorTargetSpawner : EditorWindow
    {
        public static InteractorTargetSpawner Instance { get; private set; }
        public static MenuGUI menu;
        public static ToolbarGUI toolbargui;
        public static bool isOpen;

        private int selectedTab;
        private string[] effectorTabs;
        private Interactor.EffectorLink effectorlink;
        private string _savePath;
        private bool _initiated;

        private GUISkin _skin;
        private Color _textColor = new Color(0.823f, 0.921f, 0.949f);
        private Color _textFieldColor = new Color(0.172f, 0.192f, 0.2f);
        private Color _defaultTextColor = new Color(0, 0, 0, 0);
        private Color _defaultMiniButtonTextColor;
        private Color _defaultToolbarButtonTextColor;
        private Color _defaultTextFieldColor;
        private Vector2 _defaultPadding, _defaultOverflow;
        private float _defaultFixedWidth;
        private Color _defaultGuiColor;
        private Rect windowRect;

        [SerializeField] private static Interactor _interactor;
        [SerializeField] private static bool _excludePlayerMask;
        [SerializeField] private static int _surfaceRotation;
        [SerializeField] private static bool _addComponentsOnParent;
        [SerializeField] public static long rightClickTimer = 100;

        [SerializeField] private int[] _listPointer;
        [SerializeField] private SaveData _saveData;
        [SerializeField] private List<TabStruct> tabPrefabList;

        [HideInInspector] public static List<GameObject> prefabList = new List<GameObject>();

        [SerializeField]
        public struct TabStruct
        {
            [SerializeField]
            public List<GameObject> tabPrefabs;

            public TabStruct(List<GameObject> tabPrefab)
            {
                tabPrefabs = tabPrefab;
            }
        }

        [MenuItem("Window/Interactor/Interactor Target Spawner")]
        static void PrefabSpawnerPanel()
        {
            if (!isOpen && !SceneView.lastActiveSceneView.maximized)
            {
                Debug.Log("Please update to the latest version. This version is for internal test.");
            }
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedoRefresh;
            _defaultGuiColor = GUI.color;
            Instance = this;
            Init();
            if (!_initiated) return;

            _skin = Resources.Load<GUISkin>("InteractorGUISkin");

            if (toolbargui == null)
            {
                menu = new MenuGUI();
                toolbargui = new ToolbarGUI();
                toolbargui.Setup(menu);
            }
            else
            {
                toolbargui.Setup(menu);
            }
            isOpen = true;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying && _initiated && Application.isEditor && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (SceneView.lastActiveSceneView)
                {
                    if (!SceneView.lastActiveSceneView.maximized)
                    {
                        toolbargui.Disable(menu);
                        isOpen = false;
                    }
                }
            }
            Undo.undoRedoPerformed -= UndoRedoRefresh;
        }

        private void Init()
        {
            Instance.minSize = new Vector2(400f, 580f);
            Instance.maxSize = new Vector2(600f, 1200f);
            if (_interactor == null)
            {
                _interactor = FindObjectOfType<Interactor>();
                if (_interactor == null)
                {
                    Debug.Log("Can't find an Interactor script on any scene object.");
                    _initiated = false;
                    return;
                }
                tabPrefabList = new List<TabStruct>();
            }
            _initiated = SavePathCheck();
        }

        private void GetDefaultColors()
        {
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

        private bool SavePathCheck()
        {
            GUI.color = _defaultGuiColor;
            _savePath = _interactor.savePath;
            InteractorUtilities.SaveBundle saveBundle = new InteractorUtilities.SaveBundle();
            saveBundle = InteractorUtilities.LoadAsset(_savePath);
            _saveData = saveBundle.saveData;
            _savePath = saveBundle.savePath;

            if (_savePath == null || _savePath == "")
            {
                Debug.Log("Can't continue without a save file.");
                return false;
            }

            if (_interactor.savePath != _savePath)
            {
                _interactor.savePath = _savePath;
                EditorUtility.SetDirty(_interactor);
            }

            if (_saveData == null)
                Create(true);
            else
                Load();

            return true;
        }

        public void Repainter()
        {
            if (Instance != null)
            {
                selectedTab = InteractorEditor.selectedEffectorTab;
                Instance.Repaint();
            }
        }

        private void Load()
        {
            _excludePlayerMask = _saveData.excludePlayerMask;
            _surfaceRotation = _saveData.surfaceRotation;
            _addComponentsOnParent = _saveData.addComponentsOnParent;
            rightClickTimer = _saveData.rightClickTimer;
            tabPrefabList = new List<TabStruct>();

            int pointer = 0;
            for (int i = 0; i < _saveData.listPointer.Length; i++)
            {
                tabPrefabList.Add(new TabStruct(new List<GameObject>()));

                for (int a = 0; a < _saveData.listPointer[i]; a++)
                {
                    if (_saveData.tabPrefabs[pointer + a] == null)
                    {
                        tabPrefabList[i].tabPrefabs.Add(null);
                    }
                    else
                    {
                        tabPrefabList[i].tabPrefabs.Add(_saveData.tabPrefabs[pointer + a]);
                    }
                }
                pointer += _saveData.listPointer[i];
            }
        }

        private void Create(bool newfile)
        {
            _saveData = CreateInstance<SaveData>();
            _saveData.tabPrefabs = new List<GameObject>();
            for (int i = 0; i < _interactor.effectorLinks.Count; i++)
            {
                _saveData.tabPrefabs.Add(null);
            }
            _saveData.listPointer = new int[_interactor.effectorLinks.Count];

            AssetDatabase.CreateAsset(_saveData, _savePath);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            if (newfile)
            {
                Debug.Log("Save file created at " + _savePath);
            }
        }

        private void Delete()
        {
            if (_saveData != null)
            {
                AssetDatabase.DeleteAsset(_savePath);
                AssetDatabase.Refresh();
            }
        }

        public void Save()
        {
            _saveData.excludePlayerMask = _excludePlayerMask;
            _saveData.surfaceRotation = _surfaceRotation;
            _saveData.addComponentsOnParent = _addComponentsOnParent;
            _saveData.rightClickTimer = rightClickTimer;
            _listPointer = new int[tabPrefabList.Count];

            for (int i = 0; i < tabPrefabList.Count; i++)
            {
                _listPointer[i] = tabPrefabList[i].tabPrefabs.Count;
            }
            _saveData.listPointer = _listPointer;

            _saveData.tabPrefabs = new List<GameObject>();
            for (int i = 0; i < tabPrefabList.Count; i++)
            {
                for (int a = 0; a < tabPrefabList[i].tabPrefabs.Count; a++)
                {
                    if (tabPrefabList[i].tabPrefabs[a] == null)
                    {
                        _saveData.tabPrefabs.Add(null);
                    }
                    else
                    {
                        _saveData.tabPrefabs.Add(tabPrefabList[i].tabPrefabs[a]);
                    }
                }
            }

            EditorUtility.SetDirty(_saveData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static List<GameObject> GetPrefabList()
        {
            if (Instance == null)
                PrefabSpawnerPanel();

            prefabList.Clear();

            for (int i = 0; i < Instance.tabPrefabList.Count; i++)
            {
                if (Instance.tabPrefabList[i].tabPrefabs.Count > 0)
                {
                    if (Instance.tabPrefabList[i].tabPrefabs[0] != null)
                    {
                        for (int a = 0; a < Instance.tabPrefabList[i].tabPrefabs.Count; a++)
                        {
                            if (!prefabList.Contains(Instance.tabPrefabList[i].tabPrefabs[a]))
                            {
                                prefabList.Add(Instance.tabPrefabList[i].tabPrefabs[a]);
                            }
                        }
                    }
                }
            }

            if (prefabList.Count == 0)
            {
                Debug.Log("No prefabs attached!");
            }

            return prefabList;
        }

        public static void SpawnPrefab(GameObject selectedPrefab, Vector2 mousePos)
        {
            Debug.Log("Please update to the latest version. This version is for internal test.");
        }

        public void RefreshTabs()
        {
            if (!_initiated)
            {
                Debug.Log("InteractorTargetSpawner could not initiated.");
                if (toolbargui != null)
                {
                    toolbargui.Disable(menu);
                }
                Instance.Close();
                return;
            }

            if (tabPrefabList == null)
                tabPrefabList = new List<TabStruct>();

            int countDif = _interactor.effectorLinks.Count - tabPrefabList.Count;

            if (countDif > 0)
            {
                if (countDif > 1)
                {
                    for (int i = 0; i < countDif; i++)
                    {
                        tabPrefabList.Add(new TabStruct(new List<GameObject>()));
                    }
                }
                else
                {
                    if (tabPrefabList.Count == 0)
                    {
                        tabPrefabList.Add(new TabStruct(new List<GameObject>()));
                    }
                    else
                    {
                        tabPrefabList.Insert(InteractorEditor.selectedEffectorTab + 1, new TabStruct(new List<GameObject>()));
                        tabPrefabList[InteractorEditor.selectedEffectorTab + 1].tabPrefabs.Add(null);
                    }
                }

                Delete();
                Create(false);
                Save();
                Repaint();
            }
            else if (countDif < 0)
            {
                if (countDif < -1)
                {
                    tabPrefabList.RemoveAt(InteractorEditor.selectedEffectorTab);

                    for (int i = 1; i < -countDif; i++)
                    {
                        tabPrefabList.RemoveAt(tabPrefabList.Count - 1);
                    }
                }
                else
                {
                    tabPrefabList.RemoveAt(InteractorEditor.selectedEffectorTab);
                }

                Delete();
                Create(false);
                Save();
                Repaint();
            }
        }

        private void UndoRedoRefresh()
        {
            Load();
            Repaint();
        }

        private void OnGUI()
        {
            if (!_initiated)
            {
                Debug.Log("Please update to the latest version. This version is for internal test.");
            }

            if (_interactor == null)
            {
                Init();
                if (!_initiated) return;
            }

            if (Selection.activeTransform != _interactor.transform && menu.enabled)
            {
                menu.Disable();
            }

            if (_saveData == null && _savePath != null)
            {
                Close();
                EditorGUIUtility.ExitGUI();
                Debug.Log("SaveData is not exist.");
            }

            GetDefaultColors();
            GUI.skin = _skin;
            DrawBackground();
            SetGUIColors();

            GUIStyle tabButtonStyle = new GUIStyle(_skin.GetStyle("Button"));
            _defaultPadding.x = tabButtonStyle.padding.left;
            _defaultPadding.y = tabButtonStyle.padding.right;
            _defaultOverflow.x = tabButtonStyle.overflow.left;
            _defaultOverflow.y = tabButtonStyle.overflow.right;
            _defaultFixedWidth = tabButtonStyle.fixedWidth;

            tabButtonStyle.padding.left = 0;
            tabButtonStyle.padding.right = 0;
            tabButtonStyle.overflow.left = 0;
            tabButtonStyle.overflow.right = 0;
            tabButtonStyle.fixedWidth = EditorGUIUtility.currentViewWidth / (_interactor.effectorLinks.Count);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("SaveData : " + _saveData.name + ".asset", _skin.GetStyle("label"));

            if (EditorGUIUtility.isProSkin)
                GUI.color = Color.red;
            else
                GUI.color = new Color(0.75f, 0, 0);

            if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(19), GUILayout.Height(21)))
            {
                Undo.RecordObject(_interactor, "Save Path Change");
                _interactor.savePath = null;
                EditorUtility.SetDirty(_interactor);

                bool checkpath = SavePathCheck();
                if (!checkpath)
                {
                    _initiated = false;
                    ResetGUIColors();
                    return;
                }
            }
            GUI.color = _defaultGuiColor;
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            _interactor = (Interactor)EditorGUILayout.ObjectField("Interactor GameObject :", _interactor, typeof(Interactor), true);

            if (_interactor == null)
            {
                ResetGUIColors();
                return;
            }
            if (_interactor.effectorLinks.Count == 0)
            {
                GUILayout.Space(20f);
                EditorGUILayout.HelpBox("There is no effector on Interactor GameObject. Add effectors first.", MessageType.Error);
                ResetGUIColors();
                return;
            }

            if (tabPrefabList == null)
            {
                tabPrefabList = new List<TabStruct>();

                for (int i = 0; i < _interactor.effectorLinks.Count; i++)
                {
                    tabPrefabList.Add(new TabStruct(new List<GameObject>()));
                    tabPrefabList[i].tabPrefabs.Add(null);
                }
            }

            GUILayout.BeginVertical();
            effectorTabs = new string[_interactor.effectorLinks.Count];

            for (int i = 0; i < _interactor.effectorLinks.Count; i++)
            {
                if (_interactor.effectorLinks[i].effectorName == "")
                {
                    _interactor.effectorLinks[i].effectorName = i.ToString();
                }
                effectorTabs[i] = _interactor.effectorLinks[i].effectorName;
            }

            GUILayout.Space(10f);

            EditorGUI.BeginChangeCheck();
            selectedTab = GUILayout.Toolbar(InteractorEditor.selectedEffectorTab, effectorTabs, tabButtonStyle);
            if (EditorGUI.EndChangeCheck())
            {
                InteractorEditor.selectedEffectorTab = selectedTab;
                SceneView.RepaintAll();
                GUI.FocusControl(null);
                InteractorEditor.Repainter();
            }
            effectorlink = _interactor.effectorLinks[selectedTab];

            RefreshTabs();

            GUILayout.Space(5f);
            GUILayout.Label(" Place " + effectorlink.effectorName + " prefabs below.");
            GUILayout.Space(3f);

            GUILayout.BeginHorizontal();
            if (tabPrefabList[selectedTab].tabPrefabs.Count == 0)
            {
                tabPrefabList[selectedTab].tabPrefabs.Add(null);
            }

            EditorGUI.BeginChangeCheck();
            tabPrefabList[selectedTab].tabPrefabs[0] = (GameObject)EditorGUILayout.ObjectField(" Main Prefab", tabPrefabList[selectedTab].tabPrefabs[0], typeof(GameObject), true);

            if (EditorGUIUtility.isProSkin)
                GUI.color = Color.red;
            else
                GUI.color = new Color(0.75f, 0, 0);

            if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(21), GUILayout.Height(15)))
            {
                tabPrefabList[selectedTab].tabPrefabs.Clear();
                tabPrefabList[selectedTab].tabPrefabs.Add(null);
            }
            GUI.color = _defaultGuiColor;
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(_saveData, "Spawner Prefab Change");
                Save();
            }

            GUILayout.Space(5f);
            GUILayout.Label(" You can add alternative prefabs.");
            GUILayout.Space(5f);

            if (tabPrefabList[selectedTab].tabPrefabs[0] != null)
            {
                for (int i = 1; i < tabPrefabList[selectedTab].tabPrefabs.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    tabPrefabList[selectedTab].tabPrefabs[i] = (GameObject)EditorGUILayout.ObjectField("Alternative Prefab " + (i + 1), tabPrefabList[selectedTab].tabPrefabs[i], typeof(GameObject), true);

                    if (EditorGUIUtility.isProSkin)
                        GUI.color = Color.red;
                    else
                        GUI.color = new Color(0.75f, 0, 0);

                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(21), GUILayout.Height(15)))
                    {
                        tabPrefabList[selectedTab].tabPrefabs.RemoveAt(i);
                    }
                    GUI.color = _defaultGuiColor;
                    GUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RegisterCompleteObjectUndo(_saveData, "Spawner Prefab Change");
                        Save();
                    }
                }
                GUILayout.Space(5f);
                DropAreaGUI();
                GUILayout.Space(5f);
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Space(5f);
                Rect drop_area = GUILayoutUtility.GetRect(0.0f, 150.0f, GUILayout.ExpandWidth(true));
                GUI.Box(drop_area, "Main Prefabs is not exist.", _skin.GetStyle("DropArea"));
                GUILayout.Space(5f);
                EditorGUI.EndDisabledGroup();
            }

            GUILayout.Space(20f);

            if (tabPrefabList[selectedTab].tabPrefabs[0] == null)
            {
                EditorGUILayout.LabelField("Prefab Count : 0", _skin.GetStyle("HorizontalLine"));
            }
            else
            {
                EditorGUILayout.LabelField("Prefab Count : " + tabPrefabList[selectedTab].tabPrefabs.Count, _skin.GetStyle("HorizontalLine"));
            }

            GUILayout.Space(-10f);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            GUILayout.Label(" Calculate rotation on spawn? ");
            //GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            //GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10f);
            _surfaceRotation = WindowGUI.Slider(_surfaceRotation, 0f, 3f, 80f);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
            if (_surfaceRotation == 1)
            {
                GUILayout.Label("Surface Rotation", _skin.GetStyle("HorizontalLine"));
            }
            else if (_surfaceRotation == 2)
            {
                GUILayout.Label("Camera to Object Direction", _skin.GetStyle("HorizontalLine"));
            }
            else if(_surfaceRotation == 3)
            {
                GUILayout.Label("Camera to Object (Y only)", _skin.GetStyle("HorizontalLine"));
            }
            else
            {
                GUILayout.Label("Default Prefab Rotation", _skin.GetStyle("HorizontalLine"));
            }
            GUILayout.EndVertical();

            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label(" Exclude Player layer on raycasts? ");
            GUILayout.FlexibleSpace();
            _excludePlayerMask = (bool)EditorGUILayout.Toggle(_excludePlayerMask, _skin.GetStyle("toggle"), GUILayout.Width(40));
            GUILayout.Space(8f);
            GUILayout.EndHorizontal();

            GUILayout.Space(2f);

            GUILayout.BeginHorizontal();
            GUILayout.Label(" Add required components? ");
            GUILayout.FlexibleSpace();
            _addComponentsOnParent = (bool)EditorGUILayout.Toggle(_addComponentsOnParent, _skin.GetStyle("toggle"), GUILayout.Width(40));
            GUILayout.Space(8f);
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            GUILayout.Label(" Right click time (ms) ");
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
            rightClickTimer = (int)EditorGUILayout.IntField((int)rightClickTimer, _skin.GetStyle("textfield"), GUILayout.Width(40));
            GUILayout.EndVertical();
            GUILayout.Space(10f);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(_saveData, "Spawner Setting Change");
                Save();
            }

            if (windowRect.Contains(Event.current.mousePosition))
            {
                Repaint();
            }

            ResetGUIColors();

            tabButtonStyle.padding.left = (int)_defaultPadding.x;
            tabButtonStyle.padding.right = (int)_defaultPadding.y;
            tabButtonStyle.overflow.left = (int)_defaultOverflow.x;
            tabButtonStyle.overflow.right = (int)_defaultOverflow.y;
            tabButtonStyle.fixedWidth = _defaultFixedWidth;
        }

        private void DrawBackground()
        {
            windowRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, Instance.position.height);
            GUI.Box(windowRect, "", _skin.GetStyle("BackgroundStyle"));
        }

        private void DropAreaGUI()
        {
            Debug.Log("Please update to the latest version. This version is for internal test.");
        }
    }
}
