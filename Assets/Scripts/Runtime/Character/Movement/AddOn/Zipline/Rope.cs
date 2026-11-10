using UnityEngine;
using UnityEngine.Serialization;

namespace GP
{
    [RequireComponent(typeof(LineRenderer), typeof(CapsuleCollider))]
    public class Rope : MonoBehaviour
    {
        [FormerlySerializedAs("_ropeDirectionType")]
        [SerializeField] private RopeDirectionType m_RopeDirectionType;
        [SerializeField] private Transform m_ZiplinePoint;
        [FormerlySerializedAs("_startPosition")]
        [SerializeField] private Transform m_StartPosition;
        [FormerlySerializedAs("_endPosition")]
        [SerializeField] private Transform m_EndPosition;

        private LineRenderer m_LineRenderer;
        private CapsuleCollider m_CapsuleTrigger;

        public Vector3 MoveDirection => (m_EndPosition.position - m_StartPosition.position).normalized;

        public Transform StartPosition => m_StartPosition;
        public Transform EndPosition => m_EndPosition;
        public Transform ZiplinePoint => m_ZiplinePoint;
        public RopeDirectionType RopeDirection => m_RopeDirectionType;

        private void Awake()
        {
            m_LineRenderer = GetComponent<LineRenderer>();
            m_CapsuleTrigger = GetComponent<CapsuleCollider>();

        }

        private void Start()
        {
            if (!m_StartPosition || !m_EndPosition) return;
            m_CapsuleTrigger.isTrigger = true;

            m_LineRenderer.SetPosition(0, m_StartPosition.position);
            m_LineRenderer.SetPosition(1, m_EndPosition.position);
        }

        public Vector3 GetPointOnRope(Vector3 characterPosition)
        {
            return characterPosition.GetClosestPointOnSegment(m_StartPosition.position, m_EndPosition.position);
        }

        public bool IsInverseDirection(Vector3 position, float angle)
        {
            // 如果玩家在终点或者中间段，方向是相对的则反转。

            var startPoint = Vector3.Distance(position, m_StartPosition.position) > 3.5f;
            var endPoint = Vector3.Distance(position, m_EndPosition.position) > 3.5f;

            if (endPoint && startPoint && angle < 0f)
                return true;

            if (!endPoint)
                return true;

            return false;
        }

        private void OnDrawGizmos()
        {
            if (!m_StartPosition || !m_EndPosition) return;

            Gizmos.DrawLine(m_StartPosition.position, m_EndPosition.position);
        }
        public enum RopeDirectionType
        {
            Horizontal, Vertical
        }

    }
}
