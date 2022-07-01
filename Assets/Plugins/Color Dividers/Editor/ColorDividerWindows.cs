using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ColorDividerWindows : EditorWindow
{
    [MenuItem("GameObject/Recolor Divider", false, 10)]
    static void Init()
    {
        if (Selection.count == 1)
        {
            EditorWindow window = EditorWindow.CreateInstance<ColorDividerWindows>();
            window.ShowPopup();
        }
    }

    [MenuItem("GameObject/Recolor Divider", true, 10)]
    static bool DividerTest()
    {
         if(Selection.count == 1)
         {
            if (Selection.activeGameObject.name.StartsWith(ColorDividerSettings.Instance.NameStartsWith))
            {
                return true;
            }
         }
        return false;
    }

    private GameObject _go;
    private bool _movedToMouse = false;
    private Color32 _color = new Color32();
    private Vector2 _offset = new Vector2(0,0);

    void OnGUI()
    {
        ReColorBackGround();
        if (!_go)
        {
            _go = Selection.activeGameObject;
            Color hexColor = new Color();
            if (ColorUtility.TryParseHtmlString("#" + _go.name.Split('#')[1], out hexColor))
            {
                _color = hexColor;
            }
        }

        var e = Event.current;

        if (!_movedToMouse)
        {
            _movedToMouse = true;
            MoveToMouse(e);
        }

        // Close the window if ESC is pressed
        CloseWindowOnEscape(e);

        EditorGUILayout.LabelField("Hierarchy Divider", EditorStyles.wordWrappedLabel);
        _color = EditorGUILayout.ColorField(_color);
        _go.name = _go.name.Split('#')[0] + "#" + ColorUtility.ToHtmlStringRGB(_color);
        GUILayout.Space(25);
        if (GUILayout.Button("Agree!")) this.Close();
    }

    //private void OnLostFocus()
    //{
    //    Close();
    //}

    private void CloseWindowOnEscape(Event e)
    {
        if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Escape) Close();
    }

    private void MoveToMouse(Event e)
    {
        Vector2 mousePos = e.mousePosition;
        position = new Rect(mousePos.x, mousePos.y, 250, 150);
    }

    private void ReColorBackGround()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.grey);
        tex.Apply();
        GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), tex, ScaleMode.StretchToFill);
    }
}
