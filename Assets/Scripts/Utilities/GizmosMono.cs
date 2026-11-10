
using UnityEngine;

namespace GP
{
    public class GizmosMono : MonoBehaviour
    {
        [SerializeField] private GizmosShape m_GizmosShape;

        private BoxCollider m_BoxCollider;
        private SphereCollider m_SphereCollider;

        private void OnDrawGizmos()
        {
            var matrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            switch (m_GizmosShape)
            {
                case GizmosShape.Cube:
                    if (!m_BoxCollider) m_BoxCollider = GetComponent<BoxCollider>();
                    if (m_BoxCollider) Gizmos.DrawWireCube(m_BoxCollider.center, m_BoxCollider.size);
                    break;
                case GizmosShape.Sphere:
                    if (!m_SphereCollider) m_SphereCollider = GetComponent<SphereCollider>();
                    if (m_SphereCollider) Gizmos.DrawSphere(m_SphereCollider.center, m_SphereCollider.radius);
                    break;
            }
            Gizmos.matrix = matrix;
        }
    }

    public enum GizmosShape
    {
        Sphere,
        Cube,
    }
}
