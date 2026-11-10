using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GP
{
    public class RuntimePool
    {
        private const string PoolNameSuffix = "_POOL";
        private readonly PoolAsset m_PoolAsset;
        private readonly Transform m_UnitParent;
        private readonly Transform m_InactiveParent;
        private int m_NextUnitId;
        private readonly Queue<IPoolUnit> m_AvailableUnits = new();
        private readonly Dictionary<int, IPoolUnit> m_ActiveUnits = new();

        public PoolAsset PoolAsset => m_PoolAsset;
        public int UnitTotalCount => AvailableUnitCount + ActiveUnitCount;
        public int AvailableUnitCount => m_AvailableUnits.Count;
        public int ActiveUnitCount => m_ActiveUnits.Count;
        public EntityId PoolId => m_PoolAsset ? m_PoolAsset.PoolId : EntityId.None;

        public RuntimePool(PoolAsset poolAsset, Transform parent)
        {
            if (!poolAsset) throw new ArgumentNullException(nameof(poolAsset));

            m_PoolAsset = poolAsset;
            m_UnitParent = new GameObject($"{m_PoolAsset.PoolName}{PoolNameSuffix}").transform;
            m_UnitParent.SetParent(parent); // 注意 UnitParent 是存放池子中对象的 Parent， parent 指的是 PoolManager
            m_UnitParent.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var inactiveParent = new GameObject("InactiveUnits");
            inactiveParent.transform.SetParent(m_UnitParent, false);
            inactiveParent.SetActive(false);
            m_InactiveParent = inactiveParent.transform;
        }

        public IPoolUnit TakeUnit()
        {
            while (m_AvailableUnits.Count > 0)
            {
                var unit = m_AvailableUnits.Dequeue();
                var instance = unit.GetUnitGameObject();
                if (!instance) continue;
                m_ActiveUnits.Add(unit.UnitId, unit);
                instance.SetActive(true);
                return unit;
            }
            return null;
        }

        public void RecycleUnit(IPoolUnit unit)
        {
            if (unit == null || unit.PoolId != PoolId
                || !m_ActiveUnits.TryGetValue(unit.UnitId, out var activeUnit)
                || !ReferenceEquals(activeUnit, unit)) return;

            m_ActiveUnits.Remove(unit.UnitId);
            var instance = unit.GetUnitGameObject();
            if (!instance) return;
            instance.SetActive(false);
            if (UnitTotalCount >= m_PoolAsset.PoolMaxSize)
            {
                Object.Destroy(instance);
                return;
            }
            instance.transform.SetParent(m_UnitParent, false);
            instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            m_AvailableUnits.Enqueue(unit);
        }

        public void LoadUnit(int count)
        {
            if (!m_PoolAsset || count <= 0) return;
            count = Mathf.Min(count, m_PoolAsset.PoolMaxSize - UnitTotalCount);

            for (int i = 0; i < count; ++i)
            {
                IPoolUnit unit = CreateUnit();
                if (unit == null) continue;

                int id = m_NextUnitId++;
                unit.InitializeUnit(id, m_PoolAsset.PoolId, this);

                m_AvailableUnits.Enqueue(unit);
            }
        }

        public void ClearPool()
        {
            var units = new List<IPoolUnit>(m_AvailableUnits);
            units.AddRange(m_ActiveUnits.Values);
            m_AvailableUnits.Clear();
            m_ActiveUnits.Clear();
            foreach (var unit in units)
            {
                var instance = unit.GetUnitGameObject();
                if (!instance) continue;
                instance.SetActive(false);
                Object.Destroy(instance);
            }
        }

        private IPoolUnit CreateUnit()
        {
            if (!m_PoolAsset || !m_PoolAsset.Prefab || !m_UnitParent) return null;

            // 重用未激活的父物体，保证 InitializeUnit 先于第一次 OnEnable。
            var unit = Object.Instantiate(m_PoolAsset.Prefab, m_InactiveParent);
            unit.gameObject.SetActive(false);
            unit.transform.SetParent(m_UnitParent, false);
            return unit;
        }
    }

    public enum RecycleBehavior
    {
        ByTime,
        ByObjectSelf
    }
}
