using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
public class EditorScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
{
    [NonSerialized] protected static T _instance = null;
    [NonSerialized] protected static bool _isInitialized = false;

    public static T Instance
    {
        get
        {
            if (!_instance)
            {
                string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
                T asset = null;
                foreach (var t in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(t);
                    asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    if (asset != null)
                    {
                        //Debug.Log("Found Editor ScriptableSingleton");
                        break;
                    }
                }
                _instance = asset;
                _instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            return _instance;
        }
    }
}
