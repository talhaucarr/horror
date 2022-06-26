using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        [NonSerialized] protected static T _instance = null;
        [NonSerialized] protected static bool _isInitialized = false;

        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    T[] resulsts = Resources.LoadAll<T>("ScriptableObjects/Singletons");
                    if (resulsts.Length != 1) { Debug.LogError($"There are more than 1, or 0 {typeof(T).Name}"); return null; }
                    _instance = resulsts[0];
                    _instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
                }
                return _instance;
            }
        }
    }
}