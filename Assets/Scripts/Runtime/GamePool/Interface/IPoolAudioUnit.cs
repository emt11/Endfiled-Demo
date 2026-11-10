using UnityEngine;

namespace GP
{
    public interface IPoolAudioUnit
    {
        void Execute(AudioClip clip, AudioAsset audioAsset);
        void Execute(AudioClip clip, AudioAsset audioAsset, Vector3 position);
        void Execute(AudioClip clip, AudioAsset audioAsset, Vector3 position, Quaternion rotation);
    }
}
