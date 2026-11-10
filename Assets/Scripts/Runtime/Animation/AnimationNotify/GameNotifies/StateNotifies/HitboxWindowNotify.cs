using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable, LabelText("近战打击窗口")]
    public class HitboxWindowNotify : AnimationStateNotify
    {
        [SerializeField, LabelText("检测部位")]
        private WeaponAttachmentPoint m_AttachmentPoints;
        [SerializeField, LabelText("打击方向")]
        private HitDirection m_HitDirection;

        public override void OnEnterNotify(GameObject user)
        {
            EventCenter<HitboxWindowEvent>.Publish(
                new HitboxWindowEvent(true, m_AttachmentPoints, m_HitDirection, user, this));
        }

        public override void OnTickNotify(GameObject user) { }

        public override void OnExitNotify(GameObject user)
        {
            EventCenter<HitboxWindowEvent>.Publish(
                new HitboxWindowEvent(false, m_AttachmentPoints, m_HitDirection, user, this));
        }
    }
}
