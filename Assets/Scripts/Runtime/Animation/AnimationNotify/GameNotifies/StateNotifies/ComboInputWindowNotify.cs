using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable, LabelText("连招输入窗口")]
    public class ComboInputWindowNotify : AnimationStateNotify
    {
        public override void OnEnterNotify(GameObject user)
        {
            EventCenter<ComboInputWindowEvent>.Publish(new ComboInputWindowEvent(true, user, this));
        }

        public override void OnTickNotify(GameObject user) { }

        public override void OnExitNotify(GameObject user)
        {
            EventCenter<ComboInputWindowEvent>.Publish(new ComboInputWindowEvent(false, user, this));
        }
    }
}
