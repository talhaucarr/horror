using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// Singleton pattern with auto destroy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AutoSingleton<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// The instance.
        /// </summary>
        protected static T _instance;

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
                if (_instance != null) return _instance;

                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    _instance = new GameObject(typeof(T) + " Instance").AddComponent<T>();
                    Debug.Log(typeof(T).Name+ " An Empty Singleton Instance was reached. Creating a new instance");
                }
                return _instance;
            }
        }

        public static bool InstanceExists
        {
            get
            {
                if (_instance) return true;
                else return false;
            }
        }

        #endregion

        #region Methods

        protected virtual void Awake()
        {
            if (_instance != null)
            {
                Debug.LogError($"[{this.GetType().Name}]: Not null, destroy");
                Destroy(this);
            }
            else _instance = this as T;
        }

        #endregion
    }
}