using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [CreateAssetMenu(fileName = "New Audio Asset", menuName = "GP/Audio/Audio Asset")]
    public class AudioAsset : ScriptableObject
    {
        [Space, SerializeField, BoxGroup("Audio Settings", showLabel: false)]
        private float m_Volume = 1f;
        [SerializeField, BoxGroup("Audio Settings", showLabel: false)]
        private float m_Pitch = 1f;
        [SerializeField, BoxGroup("Audio Settings", showLabel: false)]
        private float m_StereoPan;

        [Space, SerializeField, BoxGroup("Audio Settings", showLabel: false)]
        private AudioClip[] m_Sounds;

        [SerializeField, BoxGroup("Pool", showLabel: false)]
        private bool m_UsePool;
        [SerializeField, BoxGroup("Pool", showLabel: false), ShowIf(nameof(m_UsePool))]
        private PoolAsset m_PoolAsset;

        public float Volume => m_Volume;
        public float Pitch => m_Pitch;
        public float StereoPan => m_StereoPan;
        public PoolAsset PoolAsset => m_PoolAsset;

        public AudioClip GetSound()
        {
            return m_Sounds == null || m_Sounds.Length == 0 ? null : m_Sounds[Random.Range(0, m_Sounds.Length)];
        }

        public void PlaySoundWithPool()
        {
            PlaySoundWithPool(Vector3.zero, Quaternion.identity);
        }

        public void PlaySoundWithPool(Vector3 position)
        {
            PlaySoundWithPool(position, Quaternion.identity);
        }

        public void PlaySoundWithPool(Vector3 position, Quaternion rotation)
        {
            if (!m_UsePool) return;
            var clip = GetSound();
            if (!clip) return;
            var manager = PoolManager.Instance;
            if (!manager) return;
            var unit = manager.TakeUnit(m_PoolAsset);
            if (unit is IPoolAudioUnit audioUnit) audioUnit.Execute(clip, this, position, rotation);
            else unit?.RecycleUnit();
        }
    }
}
