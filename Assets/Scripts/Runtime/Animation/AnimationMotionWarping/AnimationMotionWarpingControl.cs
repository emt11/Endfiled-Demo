using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    public enum WarpingStatus
    {
        None,
        Waiting,
        Running,
        Completed
    }

    /// <summary>
    /// 运行时确定的扭曲目标点，例如敌人身上的锁定点
    /// </summary>
    public readonly struct WarpingPoint
    {
        public readonly Transform Transform;
        private readonly Vector3 m_LocalPosition;

        public static WarpingPoint None => default;

        public WarpingPoint(Transform transform, Vector3 position)
        {
            Transform = transform;
            m_LocalPosition = transform ? transform.InverseTransformPoint(position) : position;
        }

        public Vector3 GetLocalPosition()
        {
            return m_LocalPosition;
        }

        public Vector3 GetWorldPosition()
        {
            return Transform.TransformPoint(GetLocalPosition());
        }
    }

    public class RuntimeWarpingPhase
    {
        public MotionWarpWindow WarpPhase { get; }
        public WarpingPoint WarpPoint { get; }

        public RuntimeWarpingPhase(MotionWarpWindow warpWindow, WarpingPoint warpPoint)
        {
            WarpPhase = warpWindow;
            WarpPoint = warpPoint;
        }
    }

    public class AnimationMotionWarpingControl
    {
        private readonly Transform m_Character;
        private readonly List<RuntimeWarpingPhase> m_RuntimeWarpingPhases = new();
        private MotionWarpingAsset m_WarpingAsset;
        private RuntimeWarpingPhase m_CurrentWarpingPhase;
        private int m_PhaseIndex;
        private int m_PhaseStartFrame;
        private int m_PhaseEndFrame;

        public bool EnableWarping { get; private set; }
        public WarpingStatus WarpingStatus { get; private set; }

        public AnimationMotionWarpingControl(Transform character)
        {
            m_Character = character;
        }

        public void ProcessWarpingStatus(int frame)
        {
            if (!EnableWarping) return;
            while (m_PhaseIndex < m_RuntimeWarpingPhases.Count)
            {
                m_CurrentWarpingPhase = m_RuntimeWarpingPhases[m_PhaseIndex];
                if (!m_CurrentWarpingPhase.WarpPoint.Transform)
                {
                    InterruptWarping();
                    return;
                }
                var window = m_CurrentWarpingPhase.WarpPhase.WarpWindow;
                m_PhaseStartFrame = window.x;
                m_PhaseEndFrame = window.y;
                if (frame > m_PhaseEndFrame)
                {
                    // 跨过的窗口不再补做位移，同帧继续查找下一个有效阶段。
                    m_PhaseIndex++;
                    continue;
                }
                WarpingStatus = frame < m_PhaseStartFrame ? WarpingStatus.Waiting : WarpingStatus.Running;
                return;
            }
            InterruptWarping();
        }

        public bool CompleteFrame(int frame)
        {
            if (WarpingStatus != WarpingStatus.Running || frame < m_PhaseEndFrame) return false;
            m_PhaseIndex++;
            ProcessWarpingStatus(frame);
            return WarpingStatus == WarpingStatus.Running;
        }

        public Vector3 WarpingTranslate(Vector3 deltaPosition, int currentFrame)
        {
            var warpPoint = GetWarpPoint(); // 获取目标位置

            var remainingDistance = Vector3.Distance(new Vector3(m_Character.position.x, 0f, m_Character.position.z),
                new Vector3(warpPoint.x, 0f, warpPoint.z)); // 计算玩家与扭曲目标点的距离

            if (IsEndFrame(currentFrame))
            {
                return !m_WarpingAsset.IgnoreY
                    ? warpPoint
                    : new Vector3(warpPoint.x, m_Character.position.y, warpPoint.z);
            }

            var warpPosition = CalculateWarpPosition(currentFrame, remainingDistance, deltaPosition);
            var movementDirection = new Vector3(warpPoint.x - m_Character.position.x, 0f,
                warpPoint.z - m_Character.position.z).normalized;
            var newPosition = m_Character.position + movementDirection * warpPosition.magnitude;

            if (!m_WarpingAsset.IgnoreY)
            {
                var yDifference = warpPoint.y - m_Character.position.y;
                var remainingFrame = m_CurrentWarpingPhase.WarpPhase.WarpWindow.y - currentFrame; // 计算一下剩余帧
                var yValue = yDifference / remainingFrame;
                newPosition.y = m_Character.position.y + yValue;
            }
            else
            {
                newPosition.y = m_Character.position.y;
            }

            return newPosition;
        }

        public Quaternion WarpRotation(int frame)
        {
            if (!m_WarpingAsset.WarpRotation) return m_Character.rotation;

            var value = NormalizedTime(frame, m_CurrentWarpingPhase.WarpPhase.WarpWindow.x, m_CurrentWarpingPhase.WarpPhase.WarpWindow.y);

            var direction = m_CurrentWarpingPhase.WarpPoint.GetWorldPosition() - m_Character.position;

            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon) return m_Character.rotation;
            var targetRotation = Quaternion.LookRotation(direction);

            return Quaternion.Slerp(m_Character.rotation, targetRotation, value);
        }

        public bool Interact(IWarpInteraction interaction)
        {
            if (interaction == null) return false;

            var result = interaction.WarpInteraction();

            if (!result.IsValid()) return false;

            return SetWarpAsset(result.WarpingAsset, result.WarpingPoints);
        }

        public void InterruptWarping()
        {
            EnableWarping = false;
            WarpingStatus = WarpingStatus.Completed;
            m_RuntimeWarpingPhases.Clear();
            m_CurrentWarpingPhase = null;
            m_WarpingAsset = null;
            m_PhaseIndex = 0;
            m_PhaseStartFrame = m_PhaseEndFrame = 0;
        }

        private bool SetWarpAsset(MotionWarpingAsset warpAsset, WarpingPoint[] points)
        {
            if (!warpAsset || points == null || points.Length == 0) return false;
            var phases = warpAsset.WarpingWindows;
            if (phases.Count != points.Length) return false;
            for (int i = 0; i < phases.Count; i++)
            {
                if (phases[i] == null || !points[i].Transform
                    || phases[i].WarpWindow.x < 0 || phases[i].WarpWindow.y < phases[i].WarpWindow.x) return false;
            }
            InterruptWarping();
            m_WarpingAsset = warpAsset;
            for (int i = 0; i < phases.Count; i++)
                m_RuntimeWarpingPhases.Add(new RuntimeWarpingPhase(phases[i], points[i]));
            EnableWarping = true;
            WarpingStatus = WarpingStatus.Waiting;
            return true;
        }

        private Vector3 CalculateWarpPosition(int frame, float distance, Vector3 deltaPosition)
        {
            if (!InPhaseFrame(frame)) return Vector3.zero;

            var remainingMotionData = Mathf.Abs(m_CurrentWarpingPhase.WarpPhase.GetWarpFrameMotionData(frame).z);

            if (remainingMotionData <= Mathf.Epsilon) return Vector3.zero;
            var warpScale = distance / remainingMotionData;
            var warpPosition = deltaPosition * warpScale;

            if (float.IsNaN(warpPosition.x))
                warpPosition.x = 0f;
            if (float.IsNaN(warpPosition.y))
                warpPosition.y = 0f;
            if (float.IsNaN(warpPosition.z))
                warpPosition.z = 0f;

            // 拿到缩放后的根运动量，将其转换到角色的自身坐标系下
            var characterLocalPosition = m_Character.InverseTransformDirection(warpPosition);
            characterLocalPosition.x = Mathf.Clamp(characterLocalPosition.x, -1, 1f); // 限定 x 增量
            characterLocalPosition.z = Mathf.Max(0f, characterLocalPosition.z);

            warpPosition = m_Character.TransformDirection(characterLocalPosition);

            return warpPosition;
        }

        private Vector3 GetWarpPoint()
        {
            if (m_WarpingAsset.IsSyncAnimate)
            {
                var position = m_CurrentWarpingPhase.WarpPoint.GetWorldPosition() +
                               m_CurrentWarpingPhase.WarpPoint.Transform.rotation *
                               m_CurrentWarpingPhase.WarpPhase.TOffset;
                return position;
            }
            else
            {
                var direction = (m_CurrentWarpingPhase.WarpPoint.GetWorldPosition() - m_Character.position).normalized;
                var point = m_CurrentWarpingPhase.WarpPoint.GetWorldPosition() - direction * (m_CurrentWarpingPhase.WarpPhase.ZOffset + 0.25f);

                if (m_WarpingAsset.IgnoreY)
                {
                    point.y = m_Character.position.y;
                }

                return point;
            }
        }

        private static float NormalizedTime(int frame, int startFrame, int endFrame)
        {
            return endFrame == startFrame ? 1f : Mathf.Clamp01((frame - startFrame) / (float)(endFrame - startFrame));
        }

        private bool InPhaseFrame(int currentFrame)
        {
            return currentFrame >= m_PhaseStartFrame && currentFrame <= m_PhaseEndFrame;
        }

        private bool IsEndFrame(int currentFrame)
        {
            return currentFrame >= m_PhaseEndFrame;
        }

    }

    public interface IWarpInteraction
    {
        WarpInteractionResult WarpInteraction();
    }

    public struct WarpInteractionResult
    {
        public WarpingPoint[] WarpingPoints;
        public MotionWarpingAsset WarpingAsset;
        public bool ValidResult;

        public bool IsValid() => WarpingPoints != null && WarpingAsset && ValidResult;
    }
}
