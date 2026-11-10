using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable]
    public class WeaponAttachment
    {
        [SerializeField, LabelText("检测器预制体")]
        private Hitbox m_HitboxPrefab;
        [SerializeField, LabelText("挂载部位")]
        private WeaponAttachmentPoint m_AttachmentPoint;

        public WeaponAttachmentPoint AttachmentPoint => m_AttachmentPoint;

        public Hitbox CreateHitbox(Animator animator, Transform parent = null)
        {
            if (!m_HitboxPrefab || !animator) return null;

            if (!parent)
            {
                if (!animator.isHuman || !TryGetBone(out var bone)) return null;
                parent = animator.GetBoneTransform(bone);
                if (!parent) return null;
            }

            var instance = UnityEngine.Object.Instantiate(m_HitboxPrefab, parent, false);
            instance.transform.localScale = m_HitboxPrefab.transform.localScale;
            instance.transform.SetLocalPositionAndRotation(m_HitboxPrefab.transform.localPosition, m_HitboxPrefab.transform.localRotation);
            return instance;
        }

        public bool TryGetBone(out HumanBodyBones bone)
        {
            bone = m_AttachmentPoint switch
            {
                WeaponAttachmentPoint.LeftHand => HumanBodyBones.LeftHand,
                WeaponAttachmentPoint.RightHand => HumanBodyBones.RightHand,
                WeaponAttachmentPoint.LeftFoot => HumanBodyBones.LeftFoot,
                WeaponAttachmentPoint.RightFoot => HumanBodyBones.RightFoot,
                WeaponAttachmentPoint.Head => HumanBodyBones.Head,
                WeaponAttachmentPoint.Hips => HumanBodyBones.Hips,
                WeaponAttachmentPoint.Spine => HumanBodyBones.Spine,
                _ => HumanBodyBones.LastBone
            };
            // 每个实例只对应一个挂载部位；组合标记仅用于动作和窗口筛选
            return bone != HumanBodyBones.LastBone;
        }
    }

    [Serializable]
    public struct WeaponAttachmentBinding
    {
        [SerializeField, LabelText("挂载部位")]
        private WeaponAttachmentPoint m_AttachmentPoint;
        [SerializeField, LabelText("挂载节点")]
        private Transform m_Target;

        public WeaponAttachmentPoint AttachmentPoint => m_AttachmentPoint;
        public Transform Target => m_Target;
    }

    [Flags]
    public enum WeaponAttachmentPoint
    {
        None = 0,
        [InspectorName("左手")] LeftHand = 1 << 1,
        [InspectorName("右手")] RightHand = 1 << 2,
        [InspectorName("左脚")] LeftFoot = 1 << 3,
        [InspectorName("右脚")] RightFoot = 1 << 4,
        [InspectorName("头部")] Head = 1 << 5,
        [InspectorName("髋部")] Hips = 1 << 6,
        [InspectorName("脊柱")] Spine = 1 << 7,
        All = LeftHand | RightHand | LeftFoot | RightFoot | Head | Hips | Spine
    }
}
