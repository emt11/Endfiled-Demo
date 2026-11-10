using GP;

public class PoolTimerProbe : RuntimePoolUnitBase
{
    public void StartTimer(float duration)
    {
        m_RecycleTime = duration;
        RecycleAfterDelayAsync().Forget();
    }
}
