using UnityEngine;
using UnityEngine.InputSystem;

namespace GP
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "GP", null, "TestPool")]
    public class AnimationPlaybackTest : MonoBehaviour
    {
        [SerializeField] private AnimationAsset m_AnimationAsset;

        private AnimationSource m_AnimationSource;

        private void Awake()
        {
            m_AnimationSource = GetComponent<AnimationSource>();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                m_AnimationSource.PlayAnimation(m_AnimationAsset);
            }
        }
    }
}
