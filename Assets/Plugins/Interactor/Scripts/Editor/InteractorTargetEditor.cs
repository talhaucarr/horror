using UnityEditor;
using UnityEngine;
using System.Linq;

namespace razz
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(InteractorTarget))]
    public class InteractorTargetEditor : Editor
    {
        private InteractorTarget _targetScript { get { return target as InteractorTarget; } }
        private Transform[] _children;
        private Transform _rootNode;
        private bool _init;
        private Color _boneColor = new Color(0, 0, 0, 0.75f);
        private Color _selectedChildrenColor = new Color(0.44f, 0.75f, 1f);
        private Color _targetColor = new Color(1f, 0.75f, 0.7f);
        private Color _circle = new Color(0.75f, 0.75f, 0.75f, 0.5f);
        private Color _selectedCircle = new Color(1f, 1f, 1f, 1f);
        private float _boneGizmosSize = 0.01f;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(string.Format("Bones :{0}", _children == null ? 0 : _children.Length));

            EditorGUILayout.Space();
            if (GUILayout.Button("Second Theme: " + _targetScript.secondTheme, GUILayout.Width(150f)))
            {
                _targetScript.secondTheme = !_targetScript.secondTheme;
                GetColors();
                SceneView.RepaintAll();
            }
            EditorGUILayout.HelpBox("Change bone gizmo theme on SceneView. Useful when it's hard to see because of background color.", MessageType.Info);
        }

        private void OnEnable()
        {
            //Does selection in project or in hierarchy
            if (Selection.activeObject != null)
                if (AssetDatabase.Contains(Selection.activeObject)) return;

            if (!_init)
            {
                _children = Selection.activeTransform.GetComponentsInChildren<Transform>();
                _init = true;
            }

            if (_rootNode == null)
            {
                _rootNode = _targetScript.transform;
            }

            GetColors();

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= ShowBonesBasic;
            SceneView.duringSceneGui += ShowBones;
#else
            SceneView.onSceneGUIDelegate -= ShowBonesBasic;
            SceneView.onSceneGUIDelegate += ShowBones;
#endif
        }

        private void OnDisable()
        {
            GetColors();
        }

        private void GetColors()
        {
            if (!_targetScript.secondTheme)
            {
                _boneColor = new Color(0, 0, 0, 0.75f);
                _selectedChildrenColor = new Color(0.44f, 0.75f, 1f);
                _targetColor = new Color(1f, 0.75f, 0.7f);
                _circle = new Color(0.75f, 0.75f, 0.75f, 0.5f);
                _selectedCircle = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                _boneColor = new Color(0.75f, 0.75f, 0.75f, 0.75f);
                _selectedChildrenColor = new Color(1f, 0.25f, 0.14f);
                _targetColor = new Color(1f, 0.25f, 0.7f);
                _circle = new Color(0.2f, 0.2f, 0.2f, 1f);
                _selectedCircle = new Color(0, 0, 0, 1f);
            }
        }

        private void ShowBones(SceneView sceneView)
        {
            if (_targetScript == null) return;
            if (_children.Length != 0 && Selection.activeObject != null)
            {
                if (Selection.activeGameObject != _targetScript.gameObject && !_children.Contains(Selection.activeGameObject.transform))
                {
#if UNITY_2019_1_OR_NEWER
                    SceneView.duringSceneGui += ShowBonesBasic;
                    SceneView.duringSceneGui -= ShowBones;
#else
                    SceneView.onSceneGUIDelegate += ShowBonesBasic;
                    SceneView.onSceneGUIDelegate -= ShowBones;
#endif
                }
            }
            else
            {
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui -= ShowBonesBasic;
                SceneView.duringSceneGui -= ShowBones;
#else
                SceneView.onSceneGUIDelegate -= ShowBonesBasic;
                SceneView.onSceneGUIDelegate -= ShowBones;
#endif
            }

            if (_rootNode != null && Selection.activeTransform != null)
            {
                Handles.color = _targetColor;
                if (Handles.Button(_targetScript.transform.position, Quaternion.LookRotation(_targetScript.transform.up, -_targetScript.transform.forward), _boneGizmosSize * 3f, _boneGizmosSize * 2f, Handles.CircleHandleCap))
                {
                    Selection.activeGameObject = _targetScript.gameObject;
                }

                foreach (var bone in _children)
                {
                    if (!bone.transform.parent) continue;

                    var start = bone.transform.parent.position;
                    var end = bone.transform.position;

                    if (Selection.activeGameObject == bone.gameObject)
                    {
                        Handles.color = _selectedCircle;
                    }
                    else
                    {
                        Handles.color = _circle;
                    }

                    if (Handles.Button(bone.transform.position, Quaternion.LookRotation(end - start), _boneGizmosSize, _boneGizmosSize, Handles.CircleHandleCap))
                    {
                        Selection.activeGameObject = bone.gameObject;
                    }

                    if (Selection.activeGameObject == _targetScript.gameObject)
                    {
                        Handles.color = _selectedChildrenColor;
                    }
                    else if (Selection.activeGameObject == bone.parent.gameObject)
                    {
                        Handles.color = _selectedChildrenColor;
                    }
                    else
                    {
                        Handles.color = _boneColor;
                    }

                    if (bone == _children[0]) continue;

                    Matrix4x4 matr = Handles.matrix;
                    Handles.matrix = Matrix4x4.TRS(start + (end - start) / 2, Quaternion.LookRotation(end - start), Vector3.one);
                    Handles.DrawWireCube(Vector3.zero, new Vector3(_boneGizmosSize, _boneGizmosSize, (end - start).magnitude));
                    Handles.matrix = matr;

                    if (bone.transform.parent.childCount == 1)
                        Handles.DrawAAPolyLine(15f, start, end);
                }
            }
        }

        private void ShowBonesBasic(SceneView sceneView)
        {
            if (_targetScript == null) return;
            if (_rootNode != null && Selection.activeTransform != null)
            {
                Handles.color = _targetColor;
                if (Handles.Button(_targetScript.transform.position, Quaternion.LookRotation(_targetScript.transform.up, -_targetScript.transform.forward), _boneGizmosSize * 2f, _boneGizmosSize * 2f, Handles.CircleHandleCap))
                {
                    Selection.activeGameObject = _targetScript.gameObject;
                }

                foreach (var bone in _children)
                {
                    if (!bone.transform.parent) continue;
                    if (bone == _children[0]) continue;

                    var start = bone.transform.parent.position;
                    var end = bone.transform.position;

                    Handles.color = _boneColor;
                    Handles.DrawAAPolyLine(3f, start, end);
                }
            }
            else
            {
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui -= ShowBonesBasic;
#else
                SceneView.onSceneGUIDelegate -= ShowBonesBasic;
#endif
            }
        }
    }
}
