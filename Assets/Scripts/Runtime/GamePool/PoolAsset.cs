using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [CreateAssetMenu(fileName = "New Pool Asset", menuName = "GP/Game Pool/Object Pool")]
    public class PoolAsset : ScriptableObject
    {
        [SerializeField, BoxGroup("Pool", showLabel: false)]
        private string m_PoolName;
        [SerializeField, BoxGroup("Pool", showLabel: false)]
        private RuntimePoolUnitBase m_Prefab;
        [SerializeField, BoxGroup("Pool", showLabel: false)]
        private int m_PoolMaxSize;

        public string PoolName => m_PoolName;
        public RuntimePoolUnitBase Prefab => m_Prefab;
        public int PoolMaxSize => m_PoolMaxSize;
        public EntityId PoolId => GetEntityId();

        public RuntimePool CreateRuntimePool(Transform poolRoot)
        {
            return new RuntimePool(this, poolRoot);
        }
    }
}
