using System;
using UnityEngine;

namespace GP
{
    public class CharacterTriggerAndCollision : MonoBehaviour
    {
        private ITrigger[] m_Triggers;

        public event Action<Collider> TriggerEntered;
        public event Action<Collider> TriggerStayed;
        public event Action<Collider> TriggerExited;

        public event Action<Collision> CollisionEntered;
        public event Action<Collision> CollisionStayed;
        public event Action<Collision> CollisionExited;

        private void Start()
        {
            m_Triggers = GetComponentsInChildren<ITrigger>();

            foreach (var triggerEvent in m_Triggers)
            {
                TriggerEntered += triggerEvent.TriggerEnter;
                TriggerStayed += triggerEvent.TriggerStay;
                TriggerExited += triggerEvent.TriggerExit;
            }
        }

        private void OnDestroy()
        {
            foreach (var triggerEvent in m_Triggers ?? Array.Empty<ITrigger>())
            {
                TriggerEntered -= triggerEvent.TriggerEnter;
                TriggerStayed -= triggerEvent.TriggerStay;
                TriggerExited -= triggerEvent.TriggerExit;
            }

            TriggerEntered = null;
            TriggerStayed = null;
            TriggerExited = null;
            CollisionEntered = null;
            CollisionStayed = null;
            CollisionExited = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            TriggerEntered?.Invoke(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TriggerStayed?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerExited?.Invoke(other);
        }

        private void OnCollisionEnter(Collision collision) => CollisionEntered?.Invoke(collision);
        private void OnCollisionStay(Collision collision) => CollisionStayed?.Invoke(collision);
        private void OnCollisionExit(Collision collision) => CollisionExited?.Invoke(collision);
    }
}
