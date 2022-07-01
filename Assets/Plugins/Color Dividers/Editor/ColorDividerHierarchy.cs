using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine.Events;
using System;

[InitializeOnLoad]
public static class ColorDividerHierarchy
{
    static readonly ColorDividerSettings _settings;
    static readonly GUIStyle _style;

    static ColorDividerHierarchy()
    {
        _settings = ColorDividerSettings.Instance;
        _style = new GUIStyle();
        UpdateStyle();
        _settings.Changed.AddListener(UpdateStyle);
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
    }

    static void UpdateStyle()
    {
        _style.fontSize = _settings.FontSize;
        _style.fontStyle = _settings.FontStyle;
        _style.alignment = _settings.Alignment;
        _style.normal.textColor = _settings.TextColor;
        EditorApplication.RepaintHierarchyWindow();
    }

    static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        _style.normal.textColor = Color.black;

        if (gameObject)
        {
            if (gameObject.name.StartsWith(_settings.NameStartsWith, StringComparison.Ordinal))
            {
                Rect newRect = new Rect(selectionRect);
                newRect.width += 15;
                EditorGUI.DrawRect(newRect, _settings.BackgroundColor);

                if (gameObject.name.Contains("#"))
                {
                    string[] parts = gameObject.name.Replace(_settings.RemoveString, "").Split('#');

                    Color hexColor = new Color();
                    if (ColorUtility.TryParseHtmlString("#" + parts[1], out hexColor))
                    {
                        _style.normal.textColor = hexColor;
                    }
                    EditorGUI.LabelField(selectionRect, parts[0].ToUpperInvariant(), _style);
                }
                else
                {
                    EditorGUI.LabelField(selectionRect, gameObject.name.Replace(_settings.RemoveString, "").ToUpperInvariant(), _style);
                }
            }
            //EditorApplication.RepaintHierarchyWindow();
        }
    }
}
