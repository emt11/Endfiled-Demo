using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace GP
{
    [CreateAssetMenu(fileName = "New Motion Warping Asset", menuName = "GP/Animation/Motion Warping Asset")]
    public class MotionWarpingAsset : ScriptableObject
    {
        [Space, SerializeField, BoxGroup("Animation Settings", showLabel: false), LabelText("采样剪辑")]
        private AnimationClip m_SampleClip;
        [SerializeField, BoxGroup("Animation Settings", showLabel: false), LabelText("采样模型")]
        private GameObject m_SampleModel;

        [Space, SerializeField, BoxGroup("Warp Settings", showLabel: false), LabelText("是否忽略 Y 轴高度")]
        private bool m_IgnoreY;
        [SerializeField, BoxGroup("Warp Settings", showLabel: false), LabelText("同步扭曲旋转")]
        private bool m_WarpRotation;
        [SerializeField, BoxGroup("Warp Settings", showLabel: false), LabelText("是否为配合动画")]
        private bool m_IsSyncAnimate;

        /// <summary>
        /// 一个动画可以包含多个 Warping 阶段，例如攻击动画里：
        /// 1. 起手阶段向目标靠近
        /// 2. 命中阶段再次微调位置
        /// 3. 收招阶段重新对齐
        /// 每个 MotionWarpWindow 就是一个这样的阶段
        /// </summary>
        [Space, SerializeField, BoxGroup("Windows Settings", showLabel: false)]
        private List<MotionWarpWindow> m_WarpingWindows = new List<MotionWarpWindow>();

        public GameObject SampleModel => m_SampleModel;
        public AnimationClip SampleClip => m_SampleClip;
        public float FrameRate => m_SampleClip.frameRate;
        public float Length => m_SampleClip.length;
        public bool IgnoreY => m_IgnoreY;
        public bool WarpRotation => m_WarpRotation;
        public bool IsSyncAnimate => m_IsSyncAnimate;
        public IReadOnlyList<MotionWarpWindow> WarpingWindows => m_WarpingWindows;

    }

    [Serializable]
    public class MotionWarpWindow
    {
        [SerializeField, LabelText("扭曲窗口")]
        private Vector2Int m_WarpWindow; // 从 5 ~ 10
        [SerializeField, LabelText("目标点偏移量")]
        private Vector3 m_TOffset; // 最终扭曲到目标点的偏移量
        [SerializeField, ReadOnly, LabelText("帧数据")]
        [FormerlySerializedAs("m_WarpPhaseFrameDatas")]
        private WarpFrameData[] m_WarpPhaseFrameData;

        public int Phase { get; private set; } // 表示当前是属于第几阶段
        public Vector2Int WarpWindow => m_WarpWindow;
        public Vector3 TOffset => m_TOffset;
        public float ZOffset => m_TOffset.z;

        public void WriteData(int phase, WarpFrameData[] frameData)
        {
            Phase = phase;
            m_WarpPhaseFrameData = frameData;
        }

        public Vector3 GetWarpFrameMotionData(int frame)
        {
            if (m_WarpPhaseFrameData == null || m_WarpPhaseFrameData.Length == 0) return Vector3.zero;
            var f = frame - m_WarpWindow.x;
            f = Mathf.Clamp(f, 0, m_WarpPhaseFrameData.Length - 1);
            return m_WarpPhaseFrameData[f].MotionData;
        }

        public void ClearData()
        {
            m_WarpPhaseFrameData = Array.Empty<WarpFrameData>();
        }
    }

    [Serializable]
    public class WarpFrameData
    {
        [SerializeField, LabelText("帧")] private int m_Frame;
        [SerializeField, LabelText("运动量")] private Vector3 m_MotionData;

        public int Frame => m_Frame;
        public Vector3 MotionData => m_MotionData;

        public WarpFrameData(int frame, Vector3 motionData)
        {
            m_Frame = frame;
            m_MotionData = motionData;
        }

        public bool ValidateFrame(int frame)
        {
            return frame == m_Frame;
        }
    }
}
