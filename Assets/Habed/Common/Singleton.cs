using UnityEngine;

namespace Habed.Common
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        _instance = new GameObject().AddComponent<T>();
                        _instance.gameObject.name = "[" + _instance.GetType().Name + "]";
                    }

                    return _instance;
                }
                return _instance;
            }
        }
    }

    public abstract class SingletonPersistant<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        _instance = new GameObject().AddComponent<T>();
                        _instance.gameObject.name = "[" + _instance.GetType().Name + "]";
                        DontDestroyOnLoad(_instance.gameObject);
                    }

                    return _instance;
                }
                return _instance;
            }
        }
    }

    public abstract class Instancer<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>(includeInactive: true);

                    return _instance;
                }
                return _instance;
            }
        }
    }

    public abstract class InstancerPersistant<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance = null;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    return _instance;
                }
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected void Awake()
        {
            if (Instance != null && Instance.gameObject != gameObject)
            {
                Destroy(gameObject);
                return;
            }

            Instance = gameObject.GetComponent<T>();
            DontDestroyOnLoad(gameObject);
        }
    }
}