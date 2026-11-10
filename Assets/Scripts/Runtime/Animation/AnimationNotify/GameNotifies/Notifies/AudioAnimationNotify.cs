using System;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine;

namespace GP
{
    [MovedFrom(true, sourceNamespace: "GP", sourceAssembly: "Assembly-CSharp", sourceClassName: "AnimateNotify_SFX")]
    [Serializable]
    public class AudioAnimationNotify : AnimationNotify
    {
        [SerializeField] private AudioAsset m_AudioAsset;

        public override void TriggerNotify(GameObject user)
        {
            if (!m_AudioAsset) return;

            m_AudioAsset.PlaySoundWithPool();
        }
    }
}
