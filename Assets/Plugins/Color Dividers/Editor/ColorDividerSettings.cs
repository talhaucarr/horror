using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEngine.Events;
using System.Runtime.CompilerServices;

public class ColorDividerSettings : ScriptableObject
{

    [HideInInspector]
    public UnityEvent Changed;

    public string NameStartsWith = "---";
    public string RemoveString = "-";
    public FontStyle FontStyle = FontStyle.Bold;
    public int FontSize = 14;
    public TextAnchor Alignment = TextAnchor.MiddleCenter;
    public Color TextColor = Color.black;
    public Color BackgroundColor = Color.gray;

    static ColorDividerSettings _instance;
    public static ColorDividerSettings Instance => _instance ?? (_instance = LoadAsset());

    void OnValidate()
    {
        Changed?.Invoke();
    }

    private static ColorDividerSettings LoadAsset()
    {
        var path = GetAssetPath();
        var asset = AssetDatabase.LoadAssetAtPath<ColorDividerSettings>(path);

        if (asset == null)
        {
            asset = CreateInstance<ColorDividerSettings>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }

        return asset;
    }

    private static string GetAssetPath([CallerFilePath] string callerFilePath = null)
    {
        var folder = Path.GetDirectoryName(callerFilePath);

#if UNITY_EDITOR_WIN
        folder = folder.Substring(folder.LastIndexOf(@"\Assets\", StringComparison.Ordinal) + 1);
#else
        folder = folder.Substring(folder.LastIndexOf("/Assets/", StringComparison.Ordinal) + 1);
#endif

        return Path.Combine(folder, "HierarchyWindowGroupHeaderSettings.asset");
    }
    }
