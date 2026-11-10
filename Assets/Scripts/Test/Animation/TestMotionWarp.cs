using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace GP
{
    public class TestMotionWarp : MonoBehaviour, IWarpInteraction
    {
        [FormerlySerializedAs("Point")]
        [SerializeField] private Transform m_Point;
        [FormerlySerializedAs("AnimateAsset")]
        [SerializeField] private AnimationAsset m_AnimationAsset;

        private AnimationSource m_AnimationSource;

        private void Awake()
        {
            m_AnimationSource = GetComponent<AnimationSource>();
        }

        private void Update()
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (!WarpInteraction().IsValid()) return;

                // 播放动画会中断旧的扭曲，因此在播放之后初始化本次扭曲。
                m_AnimationSource.PlayAnimation(m_AnimationAsset);
                m_AnimationSource.Warp(this);
            }
        }

        public WarpInteractionResult WarpInteraction()
        {
            var result = new WarpInteractionResult()
            {
                WarpingPoints = null,
                WarpingAsset = null,
                ValidResult = false
            };

            if (!m_Point) return result;
            if (!m_AnimationAsset) return result;
            if (!m_AnimationAsset.UseAnimationWarp) return result;

            result.WarpingPoints = new[]
            {
                new WarpingPoint(m_Point, m_Point.position)
            };
            result.WarpingAsset = m_AnimationAsset.WarpAsset;
            result.ValidResult = true;

            return result;
        }
    }
}
