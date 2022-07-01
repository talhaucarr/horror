using UnityEngine;
using UnityEditor;

namespace razz
{
    [CustomEditor(typeof(PlayerState))]
    public class PlayerStateEditor : Editor
    {
        GUIStyle _boxStyle;
        GUIStyle _wrapStyle;

        public override void OnInspectorGUI()
        {
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box);
                _boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                _boxStyle.fontStyle = FontStyle.Bold;
                _boxStyle.alignment = TextAnchor.UpperLeft;
            }

            if (_wrapStyle == null)
            {
                _wrapStyle = new GUIStyle(GUI.skin.label);
                _wrapStyle.wordWrap = true;
            }

            GUILayout.Space(10);
            GUILayout.BeginVertical("Player State Manager", _boxStyle);
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Singleton Player State script to deliver states for other scripts.", _wrapStyle);
            GUILayout.EndVertical();
            GUILayout.Space(20);

            DrawDefaultInspector();
        }
    }
}
