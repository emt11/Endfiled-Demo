using UnityEngine;

namespace GP
{
    [RequireComponent(typeof(ParticleSystem))]
    public class RuntimePoolFXUnit : RuntimePoolUnitBase
    {
        private ParticleSystem m_Particles;
        private bool m_IsPlaying;

        private void Awake()
        {
            m_Particles = GetComponent<ParticleSystem>();
        }

        private void LateUpdate()
        {
            // 等待子粒子和存活粒子全部结束，不能只按主发射器的 duration 回收
            if (m_IsPlaying && m_RecycleBehavior == RecycleBehavior.ByObjectSelf && !m_Particles.IsAlive(true))
                RecycleUnit();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_IsPlaying = false;
            if (m_Particles) m_Particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void Play(Vector3 position, Quaternion rotation)
        {
            m_Particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            transform.SetPositionAndRotation(position, rotation);
            m_IsPlaying = true;
            m_Particles.Play(true);
            if (m_RecycleBehavior == RecycleBehavior.ByTime)
            {
                m_RecycleTime = m_AutoRecycleTime;
                RecycleAfterDelayAsync().Forget();
            }
        }
    }
}
