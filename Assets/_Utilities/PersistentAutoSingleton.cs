using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class PersistentAutoSingleton<T> : MonoBehaviour
    where T : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// The instance.
    /// </summary>
    protected static T _instance;
    protected static bool _isDestroyedByGameQuit = false;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static T Instance
    {
        get
        {
            if (_isDestroyedByGameQuit) return null;
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<T>();
            if (_instance == null)
                _instance = new GameObject(typeof(T)+ " Instance").AddComponent<T>();

            return _instance;
        }
    }

    #endregion

    #region Methods

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Debug.Log($"[{this.GetType().Name}]: Not null, destroy");
            Destroy(gameObject);
            return;
        }
        _instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if(_instance == this) _isDestroyedByGameQuit = true;
    }

    #endregion
}