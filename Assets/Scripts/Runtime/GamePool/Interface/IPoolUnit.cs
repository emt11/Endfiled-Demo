using UnityEngine;

namespace GP
{
    public interface IPoolUnit
    {
        int UnitId { get; }
        EntityId PoolId { get; }
        RecycleBehavior RecycleBehavior { get; }

        void InitializeUnit(int unitId, EntityId poolId, RuntimePool pool);
        void RecycleUnit();
        GameObject GetUnitGameObject();
    }
}
