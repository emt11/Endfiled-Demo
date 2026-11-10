using System;
using UnityEngine;

namespace GP
{
    public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
    {
        private static readonly object s_Lock = new object();
        private static T s_Instance;
        protected abstract bool IsPersistent { get; }

        public static T Instance
        {
            get
            {
                if (!GameManager.GameRunning) return null;

                if (s_Instance != null) return s_Instance;

                lock (s_Lock)
                {
                    s_Instance = FindAnyObjectByType<T>();

                    if (s_Instance != null) return s_Instance;

                    var singletonObj = new GameObject(typeof(T).Name);

                    s_Instance = singletonObj.AddComponent<T>();
                }

                return s_Instance;
            }
        }

        public static event Action ApplicationQuitting;

        public static bool SingletonIsValid =>
            s_Instance != null && s_Instance.gameObject != null;

        private void Awake()
        {
            // 以下辅助手动创建单例脚本于场景中
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this as T;

            if (IsPersistent)
                DontDestroyOnLoad(gameObject);

            AwakeSingleton();
        }

        protected virtual void OnDestroy()
        {
            if (s_Instance == this) s_Instance = null;
        }

        protected virtual void AwakeSingleton() { }

        protected virtual void ForceWakeUp() { }

        protected virtual void OnApplicationQuit()
        {
            if (s_Instance != this) return;

            ApplicationQuitting?.Invoke();

            s_Instance = null;
        }
    }
}
