using UnityEngine;

namespace VirtualShowcase.Utilities
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null) print(typeof(T) + " is missing.");

                return _instance;
            }
        }

        #region Event Functions

        protected virtual void Awake()
        {
            if (_instance != null)
                Destroy(gameObject);
            else
            {
                _instance = this as T;
                DontDestroyOnLoad(_instance);
            }
        }


        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        #endregion
    }
}