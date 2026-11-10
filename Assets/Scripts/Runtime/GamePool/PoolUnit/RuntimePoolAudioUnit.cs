using UnityEngine;

namespace GP
{
    [RequireComponent(typeof(AudioSource))]
    public class RuntimePoolAudioUnit : RuntimePoolUnitBase, IPoolAudioUnit
    {
        private AudioSource m_AudioSource;

        private void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
        }

        public void Execute(AudioClip clip, AudioAsset audioAsset)
        {
            Execute(clip, audioAsset, Vector3.zero);
        }

        public void Execute(AudioClip clip, AudioAsset audioAsset, Vector3 position)
        {
            Execute(clip, audioAsset, position, Quaternion.identity);
        }

        public void Execute(AudioClip clip, AudioAsset audioAsset, Vector3 position, Quaternion rotation)
        {
            if (!m_AudioSource || !clip || !audioAsset)
            {
                RecycleUnit();
                return;
            }

            m_RecycleTime = m_RecycleBehavior switch
            {
                RecycleBehavior.ByTime => m_AutoRecycleTime,
                RecycleBehavior.ByObjectSelf => Mathf.Abs(audioAsset.Pitch) > Mathf.Epsilon
                    ? clip.length / Mathf.Abs(audioAsset.Pitch) : clip.length,
                _ => m_AutoRecycleTime
            };

            transform.SetPositionAndRotation(position, rotation);

            m_AudioSource.clip = clip;
            m_AudioSource.volume = audioAsset.Volume;
            m_AudioSource.pitch = audioAsset.Pitch;
            m_AudioSource.panStereo = audioAsset.StereoPan;
            m_AudioSource.Play();

            RecycleAfterDelayAsync().Forget();
        }
    }
}
