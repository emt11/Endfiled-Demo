namespace GP
{
    /// <summary>策略只执行单次操作，计时和取消由 RuntimeStatusEffect 统一管理。</summary>
    public interface IStatusEffectStrategy
    {
        void OnApplyEffect(RuntimeStatusEffect effect);
        void TickEffect(RuntimeStatusEffect effect);
        void OnRemoveEffect(RuntimeStatusEffect effect);
    }
}
