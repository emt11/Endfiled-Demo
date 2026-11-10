
using UnityEngine;

namespace GP
{
    public class AnimatorAgent : MonoBehaviour
    {
        private static readonly int s_LocomotionSpeedId = Animator.StringToHash("LocomotionSpeed");
        private static readonly int s_LocomotionModeId = Animator.StringToHash("LocomotionMode");
        private Animator m_Animator;
        private ThirdPersonLocomotion m_Locomotion;

        private float m_LocomotionModeValue;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_Locomotion = GetComponentInParent<ThirdPersonLocomotion>();
        }

        private void OnEnable()
        {
            if (m_Locomotion)
            {
                m_Locomotion.LocomotionAddOnManager.LocomotionModeChanged += UpdateLocomotionMode;
                UpdateLocomotionMode(m_Locomotion.LocomotionAddOnManager.LocomotionMode);
            }
        }

        private void Update()
        {
            if (!m_Animator || !m_Locomotion) return;
            m_Animator.SetFloat(s_LocomotionModeId, m_LocomotionModeValue, .1f, Time.smoothDeltaTime);
            m_Animator.SetFloat(s_LocomotionSpeedId, m_Locomotion.NormalizedLocomotionSpeed, .2f, Time.deltaTime);
        }

        private void OnDisable()
        {
            if (m_Locomotion)
            {
                m_Locomotion.LocomotionAddOnManager.LocomotionModeChanged -= UpdateLocomotionMode;
            }
        }

        private void UpdateLocomotionMode(LocomotionMode mode)
        {
            m_LocomotionModeValue = mode switch
            {
                LocomotionMode.None => 0f,
                LocomotionMode.WaterLocomotion => 1f,
                LocomotionMode.ZiplineLocomotion => 2f,
                _ => 0f
            };
        }
    }
}
