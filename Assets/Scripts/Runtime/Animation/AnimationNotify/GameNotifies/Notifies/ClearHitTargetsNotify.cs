using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable, LabelText("清理已命中目标")]
    public class ClearHitTargetsNotify : AnimationNotify
    {
        public override void TriggerNotify(GameObject user)
        {
            EventCenter<ClearHitTargetsEvent>.Publish(new ClearHitTargetsEvent(user));
        }
    }
}
