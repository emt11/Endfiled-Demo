using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    public class PoolManager : SingletonMono<PoolManager>
    {
        private const int DefaultPrewarmCount = 5;
        private readonly Dictionary<EntityId, RuntimePool> m_Pools = new();
        protected override bool IsPersistent => true;

        public Transform PoolRoot { get; private set; }

        protected override void OnDestroy()
        {
            foreach (var pool in m_Pools)
                pool.Value.ClearPool();

            m_Pools.Clear();
            base.OnDestroy();
        }

        public IPoolUnit TakeUnit(PoolAsset poolAsset)
        {
            if (!poolAsset || !poolAsset.Prefab || poolAsset.PoolMaxSize <= 0) return null;

            if (!m_Pools.TryGetValue(poolAsset.PoolId, out var pool))
                pool = CreatePool(poolAsset);

            if (pool == null) return null;

            if (pool.AvailableUnitCount == 0)
                EnsureAvailableUnits(poolAsset);

            return pool.TakeUnit();
        }

        public bool EnsureAvailableUnits(PoolAsset poolAsset, int targetAvailableCount = DefaultPrewarmCount)
        {
            if (!poolAsset || !poolAsset.Prefab || poolAsset.PoolMaxSize <= 0 || targetAvailableCount <= 0)
                return false;

            if (!m_Pools.TryGetValue(poolAsset.PoolId, out var pool))
                pool = CreatePool(poolAsset);

            if (pool == null) return false;

            var availableCapacity = poolAsset.PoolMaxSize - pool.AvailableUnitCount - pool.ActiveUnitCount; // 剩余容量
            var countToLoad = Mathf.Min(targetAvailableCount - pool.AvailableUnitCount, availableCapacity); // 需要加载的数量
            if (countToLoad <= 0) return false;

            var previousAliveCount = pool.AvailableUnitCount;
            pool.LoadUnit(countToLoad);
            return pool.AvailableUnitCount > previousAliveCount;
        }

        public RuntimePool GetPool(EntityId poolId)
        {
            return m_Pools.GetValueOrDefault(poolId);
        }

        public RuntimePool CreatePool(PoolAsset poolAsset)
        {
            if (!poolAsset || !poolAsset.Prefab || poolAsset.PoolMaxSize <= 0) return null;
            if (m_Pools.TryGetValue(poolAsset.PoolId, out var existing)) return existing;
            var pool = poolAsset.CreateRuntimePool(PoolRoot);
            m_Pools.Add(pool.PoolId, pool);
            return pool;
        }

        protected override void AwakeSingleton()
        {
            PoolRoot = transform;
        }
    }
}
