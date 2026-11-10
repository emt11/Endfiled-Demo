using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [CreateAssetMenu(fileName = "New FX", menuName = "GP/FX/FX Asset")]
    public class FXAsset : ScriptableObject
    {
        [SerializeField, Required, LabelText("特效对象池")]
        private PoolAsset m_PoolAsset;

        public void Play(Vector3 position)
        {
            Play(position, Quaternion.identity);
        }

        public void Play(Vector3 position, Quaternion rotation)
        {
            if (!m_PoolAsset || !(m_PoolAsset.Prefab is RuntimePoolFXUnit))
            {
                Debug.LogError($"{name} 需要指定以 RuntimePoolFXUnit 为预制体的 PoolAsset", this);
                return;
            }
            var manager = PoolManager.Instance;
            if (!manager) return;
            if (manager.TakeUnit(m_PoolAsset) is RuntimePoolFXUnit unit) unit.Play(position, rotation);
        }
    }
}
