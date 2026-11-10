using UnityEngine;

namespace GP
{
    public static class ComponentUtils
    {
        public static T GetOrCreateComponent<T>(this MonoBehaviour monoBehaviour) where T : Component
        {
            var component = monoBehaviour.GetComponent<T>();

            if (!component)
            {
                component = monoBehaviour.gameObject.AddComponent<T>();
            }
            return component;

        }

        public static T GetComponentSelfOrChildren<T>(this MonoBehaviour monoBehaviour) where T : Component
        {
            var component = monoBehaviour.GetComponent<T>();

            if (!component)
            {
                component = monoBehaviour.GetComponentInChildren<T>();
            }

            return component;
        }

        public static bool IsValidComponent<T>(this Component component) where T : Component
        {
            return component != null;
        }
    }
}
