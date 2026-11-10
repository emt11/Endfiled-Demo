#if UNITY_EDITOR

namespace Polyart
{

    using UnityEngine;

    [ExecuteInEditMode]
    public class ScaleParticleSystem : MonoBehaviour
    {
        private void OnValidate()
        {
            SetParticleParams();
        }

        private void OnEnable()
        {
            SetParticleParams();
        }

        private void SetParticleParams()
        {
            float scale = transform.lossyScale.z;

            ParticleSystem particleSystem = GetComponent<ParticleSystem>();

            var main = particleSystem.main;

            main.startSizeMultiplier = scale;
            //main.startLifetimeMultiplier = scale;
        }

        private void Update()
        {
            SetParticleParams();
        }
    }
}
#else

using UnityEngine;

namespace Polyart
{

    public class ScaleParticleSystem : MonoBehaviour
    {

    }
}

#endif