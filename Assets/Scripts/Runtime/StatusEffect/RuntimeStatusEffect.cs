using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GP
{
    public class RuntimeStatusEffect
    {
        private readonly StatusEffectAsset m_Asset;
        private readonly StatusEffectType m_StatusEffectType;
        private readonly bool m_IsStackable;
        private readonly int m_MaxStackCount;
        private readonly StatusEffectStackValue m_StackMode;
        private readonly int m_BaseEffectValue;
        private readonly bool m_HasDuration;
        private readonly double m_Duration;
        private readonly double m_TickInterval;
        private readonly IStatusEffectStrategy[] m_Strategies;

        private double m_EndTime;
        private int m_AppliedStrategyCount;
        private bool m_IsInvokingStrategy;
        private bool m_IsCleanedUp;
        private CancellationTokenSource m_CancellationTokenSource;

        public StatusEffectAsset Asset => m_Asset;
        public CharacterAttribute TargetAttributes { get; }
        public double Duration => m_Duration;
        public StatusEffectType StatusEffectType => m_StatusEffectType;
        public int StackCount { get; private set; }
        public int EffectValue => StatusEffectAsset.CalculateEffectValue(m_BaseEffectValue, m_StackMode, StackCount);
        public bool IsRunning { get; private set; }
        public bool IsCompleted { get; private set; }

        /// <summary>在策略清理完成后通知；自然到期、主动取消和异常退出只通知一次</summary>
        public event Action<RuntimeStatusEffect> Completed;

        /// <summary>未启动时为完整持续时间，运行时为剩余时间，结束后为 0</summary>
        public double RemainingTime => IsCompleted ? 0d
            : IsRunning ? Math.Max(0d, m_EndTime - Time.timeAsDouble) : m_Duration;

        public RuntimeStatusEffect(StatusEffectAsset asset, CharacterAttribute targetAttributes, int stackCount = 1)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            if (targetAttributes == null) throw new ArgumentNullException(nameof(targetAttributes));
            if (stackCount <= 0) throw new ArgumentOutOfRangeException(nameof(stackCount));

            m_Asset = asset;
            TargetAttributes = targetAttributes;
            m_StatusEffectType = asset.StatusEffectType;
            m_IsStackable = asset.IsStackable;
            m_MaxStackCount = asset.StackMaxCount;
            m_StackMode = asset.StackValue;
            m_BaseEffectValue = asset.StatusEffectValue;
            m_HasDuration = asset.HasDuration;
            m_Duration = asset.Duration;
            m_TickInterval = asset.TickInterval;
            StackCount = Math.Min(stackCount, m_MaxStackCount);

            if (double.IsNaN(m_Duration) || double.IsInfinity(m_Duration))
                throw new ArgumentOutOfRangeException(nameof(asset), "效果持续时间必须是有限数值。");
            if (double.IsNaN(m_TickInterval) || double.IsInfinity(m_TickInterval))
                throw new ArgumentOutOfRangeException(nameof(asset), "效果触发间隔必须是有限数值。");

            var strategies = asset.StatusEffectStrategies;
            if (strategies == null)
                throw new ArgumentException("效果策略列表不能为 null。", nameof(asset));

            m_Strategies = new IStatusEffectStrategy[strategies.Count];
            for (int i = 0; i < strategies.Count; i++)
            {
                m_Strategies[i] = strategies[i]
                    ?? throw new ArgumentException("效果策略列表中存在空策略。", nameof(asset));
            }
        }

        public void ApplyStatusEffect()
        {
            if (IsRunning || IsCompleted) return;
            if (!TargetAttributes.IsInitialized)
                throw new InvalidOperationException("请先初始化目标角色属性。");

            IsRunning = true;
            m_EndTime = Time.timeAsDouble + m_Duration;
            m_CancellationTokenSource = new CancellationTokenSource();
            RunAsync(m_CancellationTokenSource.Token).Forget();
        }

        /// <summary>仅更新层数，不刷新持续时间，也不额外触发一次周期效果</summary>
        public bool AddStacks(int stackCount)
        {
            if (IsCompleted || !m_IsStackable || stackCount <= 0) return false;

            int updatedStackCount = (int)Math.Min((long)StackCount + stackCount, m_MaxStackCount);
            if (updatedStackCount == StackCount) return false;

            StackCount = updatedStackCount;
            return true;
        }

        public bool RemoveStacks(int stackCount)
        {
            if (IsCompleted || stackCount <= 0) return false;

            StackCount = Math.Max(0, StackCount - stackCount);
            if (StackCount == 0) CancelStatusEffect();
            return true;
        }

        public void CancelStatusEffect()
        {
            if (IsCompleted) return;

            IsRunning = false;
            IsCompleted = true;
            var cancellationTokenSource = m_CancellationTokenSource;
            m_CancellationTokenSource = null;
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();

            // 策略或属性事件可能同步取消效果，等当前策略返回后再执行移除逻辑
            if (!m_IsInvokingStrategy) Complete();
        }

        private async UniTask RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                double nextTickTime = Time.timeAsDouble + m_TickInterval;
                ApplyStrategies();
                if (IsCompleted) return;
                TickStrategies();

                // 首次立即触发；使用缩放后的游戏时间，在到期时刻之前补发长帧经过的周期
                while (!IsCompleted && m_HasDuration && Time.timeAsDouble < m_EndTime)
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Update, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!TargetAttributes.IsInitialized) return;

                    double currentTime = Time.timeAsDouble;
                    while (!IsCompleted && m_TickInterval > 0d && nextTickTime < m_EndTime && nextTickTime <= currentTime)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        double previousTickTime = nextTickTime;
                        nextTickTime += m_TickInterval;
                        if (nextTickTime <= previousTickTime)
                            throw new InvalidOperationException("效果触发间隔过小，无法推进计时。");

                        TickStrategies();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // 驱散、角色清理等主动取消属于正常结束
            }
            finally
            {
                CancelStatusEffect();
            }
        }

        private void ApplyStrategies()
        {
            m_IsInvokingStrategy = true;
            try
            {
                foreach (var strategy in m_Strategies)
                {
                    if (IsCompleted) break;
                    // 即使应用途中抛出异常，也让已经进入的策略有机会清理部分操作
                    m_AppliedStrategyCount++;
                    strategy.OnApplyEffect(this);
                }
            }
            finally
            {
                m_IsInvokingStrategy = false;
                if (IsCompleted) Complete();
            }
        }

        private void TickStrategies()
        {
            m_IsInvokingStrategy = true;
            try
            {
                foreach (var strategy in m_Strategies)
                {
                    if (IsCompleted) break;
                    if (!TargetAttributes.IsInitialized)
                    {
                        CancelStatusEffect();
                        break;
                    }

                    strategy.TickEffect(this);
                }
            }
            finally
            {
                m_IsInvokingStrategy = false;
                if (IsCompleted) Complete();
            }
        }

        private void Complete()
        {
            if (m_IsCleanedUp) return;
            m_IsCleanedUp = true;

            // 与应用顺序相反地释放；一个策略清理失败不影响其他策略和管理器收尾
            for (int i = m_AppliedStrategyCount - 1; i >= 0; i--)
            {
                try { m_Strategies[i].OnRemoveEffect(this); }
                catch (Exception exception) { Debug.LogException(exception); }
            }

            StackCount = 0;
            var completed = Completed;
            if (completed == null) return;
            foreach (Action<RuntimeStatusEffect> callback in completed.GetInvocationList())
            {
                try { callback(this); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
        }
    }
}
