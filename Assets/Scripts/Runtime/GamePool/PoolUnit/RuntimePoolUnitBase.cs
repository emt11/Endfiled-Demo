using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    public class RuntimePoolUnitBase : MonoBehaviour, IPoolUnit
    {
        [SerializeField, BoxGroup("Set", showLabel: false)]
        protected RecycleBehavior m_RecycleBehavior;
        [SerializeField, BoxGroup("Set", showLabel: false), ShowIf(nameof(RecycleByTime))]
        protected float m_AutoRecycleTime;
        protected int m_UnitId;
        protected EntityId m_PoolId;
        protected RuntimePool m_RuntimePool;
        protected CancellationTokenSource m_CancellationTokenSource;

        protected float m_RecycleTime; // 实际回收时间

        public int UnitId => m_UnitId;
        public EntityId PoolId => m_PoolId;
        public RecycleBehavior RecycleBehavior => m_RecycleBehavior;

        protected virtual void OnDisable() => CancelRecycleTimer();
        protected virtual void OnDestroy() => CancelRecycleTimer();

        public void InitializeUnit(int unitId, EntityId poolId, RuntimePool pool)
        {
            m_UnitId = unitId;
            m_PoolId = poolId;
            m_RuntimePool = pool;
        }

        public void RecycleUnit()
        {
            m_RuntimePool?.RecycleUnit(this);
        }

        public GameObject GetUnitGameObject()
        {
            return gameObject;
        }

        protected async UniTaskVoid RecycleAfterDelayAsync()
        {
            CancelRecycleTimer();
            var source = new CancellationTokenSource();
            m_CancellationTokenSource = source;
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0f, m_RecycleTime)), DelayType.DeltaTime,
                    PlayerLoopTiming.Update, source.Token);
                RecycleUnit();
            }
            catch (OperationCanceledException) when (source.IsCancellationRequested) { }
            finally
            {
                if (ReferenceEquals(m_CancellationTokenSource, source)) m_CancellationTokenSource = null;
                source.Dispose();
            }
        }

        private void CancelRecycleTimer()
        {
            var source = m_CancellationTokenSource;
            m_CancellationTokenSource = null;
            source?.Cancel();
        }

        protected bool RecycleByTime() => m_RecycleBehavior == RecycleBehavior.ByTime;
    }
}
