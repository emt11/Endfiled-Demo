using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cysharp.Threading.Tasks;
using GP;
using UnityEditor;
using UnityEngine;

public static class StatusEffectRegressionTests
{
    private const string PhaseKey = "ARPG.StatusEffectTests.Phase";
    private static readonly List<UnityEngine.Object> s_Assets = new List<UnityEngine.Object>();
    private static readonly List<RuntimeStatusEffectManager> s_Managers = new List<RuntimeStatusEffectManager>();
    private static readonly Report s_Report = new Report();

    [Serializable]
    private class Report
    {
        public int passed;
        public List<string> failures = new List<string>();
    }

    [Serializable]
    private class ProbeStrategy : IStatusEffectStrategy
    {
        public int applied;
        public int ticks;
        public int removed;
        public Action<RuntimeStatusEffect> apply;
        public Action<RuntimeStatusEffect> tick;
        public Action<RuntimeStatusEffect> remove;
        public void OnApplyEffect(RuntimeStatusEffect effect) { applied++; apply?.Invoke(effect); }
        public void TickEffect(RuntimeStatusEffect effect) { ticks++; tick?.Invoke(effect); }
        public void OnRemoveEffect(RuntimeStatusEffect effect) { removed++; remove?.Invoke(effect); }
    }

    [InitializeOnLoadMethod]
    private static void Restore()
    {
        string phase = SessionState.GetString(PhaseKey, "");
        if (phase == "entering") EditorApplication.playModeStateChanged += OnPlayModeChanged;
        if (phase == "finished") EditorApplication.delayCall += () => EditorApplication.Exit(SessionState.GetInt(PhaseKey + ".Exit", 1));
    }

    public static void Run()
    {
        SessionState.SetString(PhaseKey, "entering");
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetString(PhaseKey, "") == "finished")
        {
            EditorApplication.Exit(SessionState.GetInt(PhaseKey + ".Exit", 1));
            return;
        }

        if (state != PlayModeStateChange.EnteredPlayMode) return;
        SessionState.SetString(PhaseKey, "running");
        RunAsync().Forget();
    }

    private static async UniTask RunAsync()
    {
        Action<Exception> errorHandler = exception => s_Report.failures.Add("Unexpected async exception: " + exception);
        UniTaskScheduler.UnobservedTaskException += errorHandler;
        float previousCapture = Time.captureDeltaTime;
        float previousTimeScale = Time.timeScale;
        Time.captureDeltaTime = 0.125f;
        try
        {
            await Check("构造参数和初始化要求", () =>
            {
                Throws<ArgumentNullException>(() => new RuntimeStatusEffectManager(null));
                var attributes = CreateAttributes(false);
                var manager = CreateManager(attributes);
                var asset = CreateEffect();
                Throws<InvalidOperationException>(() => manager.AddStatusEffect(asset));
                Throws<ArgumentNullException>(() => new RuntimeStatusEffect(null, attributes));
                Throws<ArgumentNullException>(() => new RuntimeStatusEffect(asset, null));
                Throws<ArgumentOutOfRangeException>(() => new RuntimeStatusEffect(asset, attributes, 0));
                Equal(false, manager.AddStatusEffect(null));
                Equal(false, manager.AddStatusEffect(asset, -1));
                return UniTask.CompletedTask;
            });
            await Check("即时效果实际扣血且立即从管理器清理", () =>
            {
                var attributes = CreateAttributes();
                var manager = CreateManager(attributes);
                var asset = CreateEffect(new PeriodicStatusEffectStrategy(), duration: 0);
                Equal(true, manager.AddStatusEffect(asset));
                Value(attributes, 990);
                Equal(0, manager.Count);
                Equal(true, manager.AddStatusEffect(asset));
                Value(attributes, 980);
                return UniTask.CompletedTask;
            });
            await Check("持续时间为零只执行一次", () =>
            {
                var probe = new ProbeStrategy();
                var asset = CreateEffect(probe, duration: 0);
                Set(asset, "m_HasDuration", true);
                var manager = CreateManager(CreateAttributes());
                manager.AddStatusEffect(asset);
                Equal(1, probe.applied); Equal(1, probe.ticks); Equal(1, probe.removed);
                Equal(0, manager.Count);
                return UniTask.CompletedTask;
            });
            await Check("周期按到期前的时间触发且补发长帧", async () =>
            {
                var probe = new ProbeStrategy();
                var asset = CreateEffect(probe, duration: 0.25f, interval: 0.0625f);
                var manager = CreateManager(CreateAttributes());
                manager.AddStatusEffect(asset);
                var effect = Get(manager, asset);
                int completed = 0;
                effect.Completed += _ => completed++;
                Equal(1, probe.ticks);
                await Frames(4);
                Equal(4, probe.ticks); Equal(1, probe.removed); Equal(1, completed);
                Equal(true, effect.IsCompleted); Equal(false, effect.IsRunning);
                Equal(0d, effect.RemainingTime); Equal(0, effect.StackCount); Equal(0, effect.EffectValue);
                Equal(0, manager.Count);
                manager.AddStatusEffect(asset);
                Equal(false, ReferenceEquals(effect, Get(manager, asset)));
            });
            await Check("间隔为零不忙循环，等待持续时间结束", async () =>
            {
                var probe = new ProbeStrategy();
                var asset = CreateEffect(probe, duration: 0.25f, interval: 0);
                var manager = CreateManager(CreateAttributes());
                manager.AddStatusEffect(asset);
                Equal(1, probe.ticks); Equal(1, manager.Count);
                await Frames(4);
                Equal(1, probe.ticks); Equal(1, probe.removed); Equal(0, manager.Count);
            });
            await Check("首次层数和叠层上限正确且不重复应用", () =>
            {
                var probe = new ProbeStrategy();
                var asset = CreateEffect(probe);
                var manager = CreateManager(CreateAttributes());
                manager.AddStatusEffect(asset, 3);
                var effect = Get(manager, asset);
                Equal(3, effect.StackCount); Equal(30, effect.EffectValue);
                double remaining = effect.RemainingTime;
                Equal(true, manager.AddStatusEffect(asset, int.MaxValue));
                Equal(5, effect.StackCount); Equal(50, effect.EffectValue);
                Equal(remaining, effect.RemainingTime);
                Equal(false, manager.AddStatusEffect(asset));
                Equal(1, probe.applied); Equal(1, probe.ticks);
                Equal(true, manager.RemoveStatusEffect(asset, 2)); Equal(3, effect.StackCount);
                Equal(false, manager.RemoveStatusEffect(asset, 0));
                Equal(true, manager.RemoveStatusEffect(asset, int.MaxValue));
                Equal(1, probe.removed); Equal(0, manager.Count);
                return UniTask.CompletedTask;
            });
            await Check("叠层在下一次周期生效", async () =>
            {
                var attributes = CreateAttributes();
                var manager = CreateManager(attributes);
                var asset = CreateEffect(new PeriodicStatusEffectStrategy(), duration: 0.5f, interval: 0.125f);
                manager.AddStatusEffect(asset);
                Value(attributes, 990);
                manager.AddStatusEffect(asset, 2);
                Value(attributes, 990);
                await Frames(2);
                Expect(attributes.TryGetValues(ValueAttributeName.Health, out int value, out _) && value <= 960, "Next tick did not use current stacks");
            });
            await Check("不可叠层和单层数值模式", () =>
            {
                var asset = CreateEffect(new ProbeStrategy());
                Set(asset, "m_IsStackable", false);
                var manager = CreateManager(CreateAttributes());
                manager.AddStatusEffect(asset, 99);
                Equal(1, Get(manager, asset).StackCount);
                Equal(false, manager.AddStatusEffect(asset));
                var single = CreateEffect(new ProbeStrategy());
                Set(single, "m_StackValue", StatusEffectStackValue.Single);
                manager.AddStatusEffect(single, 4);
                Equal(10, Get(manager, single).EffectValue);
                return UniTask.CompletedTask;
            });
            await Check("统一叠层计算处理零层和溢出", () =>
            {
                var asset = CreateEffect(new ProbeStrategy());
                Set(asset, "m_StatusEffectValue", int.MaxValue);
                Equal(0, asset.GetStatusEffectValue(0)); Equal(0, asset.GetStatusEffectValue(-1));
                Equal(int.MaxValue, asset.GetStatusEffectValue(5));
                var effect = new RuntimeStatusEffect(asset, CreateAttributes(), 5);
                Equal(int.MaxValue, effect.EffectValue);
                effect.CancelStatusEffect();
                Equal(0, effect.EffectValue);
                return UniTask.CompletedTask;
            });
            await Check("驱散立即清理且之后不再触发", async () =>
            {
                var probe = new ProbeStrategy();
                var asset = CreateEffect(probe);
                var manager = CreateManager(CreateAttributes());
                manager.AddStatusEffect(asset);
                var effect = Get(manager, asset);
                int completed = 0; effect.Completed += _ => completed++;
                Equal(true, manager.RemoveStatusEffect(asset));
                effect.CancelStatusEffect();
                await Frames(3);
                Equal(1, probe.ticks); Equal(1, probe.removed); Equal(1, completed);
                Equal(false, manager.RemoveStatusEffect(asset)); Equal(0, manager.Count);
            });
            await Check("类型驱散只移除匹配的效果", () =>
            {
                var manager = CreateManager(CreateAttributes());
                var penalty = CreateEffect(new ProbeStrategy());
                var gain = CreateEffect(new ProbeStrategy()); Set(gain, "m_StatusEffectType", StatusEffectType.Gain);
                manager.AddStatusEffect(penalty); manager.AddStatusEffect(gain);
                Equal(false, manager.RemoveStatusEffectByType(StatusEffectType.None));
                Equal(true, manager.RemoveStatusEffectByType(StatusEffectType.Penalty));
                Equal(1, manager.Count); Get(manager, gain);
                return UniTask.CompletedTask;
            });
            await Check("同名不同资产独立管理", () =>
            {
                var manager = CreateManager(CreateAttributes());
                manager.AddStatusEffect(CreateEffect(new ProbeStrategy()));
                manager.AddStatusEffect(CreateEffect(new ProbeStrategy()));
                Equal(2, manager.Count);
                return UniTask.CompletedTask;
            });
            await Check("批量清理可以重用，销毁后禁止添加", () =>
            {
                var probe = new ProbeStrategy();
                var manager = CreateManager(CreateAttributes());
                var asset = CreateEffect(probe);
                probe.remove = _ => Equal(false, manager.AddStatusEffect(asset));
                manager.AddStatusEffect(asset);
                manager.Clear(); manager.Clear();
                Equal(0, manager.Count); Equal(1, probe.removed);
                probe.remove = null;
                manager.AddStatusEffect(asset); Equal(1, manager.Count);
                manager.Dispose(); manager.Dispose();
                Equal(0, manager.Count); Equal(2, probe.removed);
                Throws<ObjectDisposedException>(() => manager.AddStatusEffect(asset));
                return UniTask.CompletedTask;
            });
            await Check("共享周期策略不会混淆不同角色", () =>
            {
                var asset = CreateEffect(new PeriodicStatusEffectStrategy());
                var first = CreateAttributes(); var second = CreateAttributes();
                var firstManager = CreateManager(first); var secondManager = CreateManager(second);
                firstManager.AddStatusEffect(asset, 3); secondManager.AddStatusEffect(asset);
                Value(first, 970); Value(second, 990);
                firstManager.Clear(); Equal(1, secondManager.Count);
                return UniTask.CompletedTask;
            });
            await Check("治疗到满值不会把效果移除", () =>
            {
                var strategy = new PeriodicStatusEffectStrategy();
                Set(strategy, "m_EffectValueOperation", ValueAttributeOperation.Add);
                var attributes = CreateAttributes(); var manager = CreateManager(attributes);
                manager.AddStatusEffect(CreateEffect(strategy));
                Value(attributes, 1000); Equal(1, manager.Count);
                return UniTask.CompletedTask;
            });
            await Check("属性事件中取消不会执行后续策略", () =>
            {
                var attributes = CreateAttributes(); var manager = CreateManager(attributes);
                var after = new ProbeStrategy(); var asset = CreateEffect(new PeriodicStatusEffectStrategy());
                Set(asset, "m_StatusEffectStrategies", new List<IStatusEffectStrategy> { new PeriodicStatusEffectStrategy(), after });
                attributes.ValueChanged += (_, __, ___) => manager.RemoveStatusEffect(asset);
                manager.AddStatusEffect(asset);
                Value(attributes, 990); Equal(0, after.ticks); Equal(1, after.removed); Equal(0, manager.Count);
                return UniTask.CompletedTask;
            });
            await Check("应用过程中取消，先返回应用回调再清理", () =>
            {
                var order = new List<string>();
                var first = new ProbeStrategy(); var second = new ProbeStrategy();
                first.apply = effect => { order.Add("apply"); effect.CancelStatusEffect(); order.Add("returned"); };
                first.remove = _ => order.Add("remove");
                var asset = CreateEffect(first);
                Set(asset, "m_StatusEffectStrategies", new List<IStatusEffectStrategy> { first, second });
                var manager = CreateManager(CreateAttributes()); manager.AddStatusEffect(asset);
                Equal("apply,returned,remove", string.Join(",", order));
                Equal(0, first.ticks); Equal(0, second.applied); Equal(0, second.removed); Equal(0, manager.Count);
                return UniTask.CompletedTask;
            });
            await Check("清理回调重加同一资产不会被旧完成通知删除", () =>
            {
                var probe = new ProbeStrategy(); var asset = CreateEffect(probe);
                var manager = CreateManager(CreateAttributes());
                manager.AddStatusEffect(asset); var old = Get(manager, asset);
                probe.remove = _ => { probe.remove = null; manager.AddStatusEffect(asset); };
                manager.RemoveStatusEffect(asset);
                Equal(1, manager.Count); Equal(false, ReferenceEquals(old, Get(manager, asset)));
                return UniTask.CompletedTask;
            });
            await Check("目标属性释放后自动结束效果", async () =>
            {
                var attributes = CreateAttributes(); var manager = CreateManager(attributes);
                var probe = new ProbeStrategy(); manager.AddStatusEffect(CreateEffect(probe));
                attributes.Deinitialize(); await Frames(2);
                Equal(0, manager.Count); Equal(1, probe.ticks); Equal(1, probe.removed);
            });
            await Check("暂停时不消耗游戏持续时间", async () =>
            {
                var manager = CreateManager(CreateAttributes()); var probe = new ProbeStrategy();
                var asset = CreateEffect(probe, duration: 0.25f, interval: 0.0625f);
                Time.timeScale = 0;
                manager.AddStatusEffect(asset); double remaining = Get(manager, asset).RemainingTime;
                await Frames(3);
                Equal(1, probe.ticks); Equal(remaining, Get(manager, asset).RemainingTime);
                Time.timeScale = 1;
                await Frames(4); Equal(4, probe.ticks); Equal(0, manager.Count);
            });
            await Check("策略列表在启动后修改不会影响已有实例", () =>
            {
                var original = new ProbeStrategy(); var asset = CreateEffect(original);
                var manager = CreateManager(CreateAttributes()); manager.AddStatusEffect(asset);
                Set(asset, "m_StatusEffectStrategies", new List<IStatusEffectStrategy>());
                manager.Clear(); Equal(1, original.removed);
                return UniTask.CompletedTask;
            });
            await Check("空策略和非法时间不会留下管理器记录", () =>
            {
                var manager = CreateManager(CreateAttributes()); var asset = CreateEffect();
                Set(asset, "m_StatusEffectStrategies", new List<IStatusEffectStrategy> { null });
                Throws<ArgumentException>(() => manager.AddStatusEffect(asset)); Equal(0, manager.Count);
                Set(asset, "m_StatusEffectStrategies", new List<IStatusEffectStrategy>());
                Set(asset, "m_TickInterval", float.NaN);
                Throws<ArgumentOutOfRangeException>(() => manager.AddStatusEffect(asset)); Equal(0, manager.Count);
                return UniTask.CompletedTask;
            });
            await Check("策略异常退出仍清理已进入的策略和管理器", () =>
            {
                var observed = new List<Exception>();
                Action<Exception> expected = exception => observed.Add(exception);
                UniTaskScheduler.UnobservedTaskException -= errorHandler;
                UniTaskScheduler.UnobservedTaskException += expected;
                try
                {
                    var probe = new ProbeStrategy { apply = _ => throw new InvalidOperationException("Expected probe failure") };
                    var manager = CreateManager(CreateAttributes());
                    manager.AddStatusEffect(CreateEffect(probe));
                    Equal(1, probe.applied); Equal(0, probe.ticks); Equal(1, probe.removed);
                    Equal(0, manager.Count); Equal(1, observed.Count);
                }
                finally
                {
                    UniTaskScheduler.UnobservedTaskException -= expected;
                    UniTaskScheduler.UnobservedTaskException += errorHandler;
                }
                return UniTask.CompletedTask;
            });
            foreach (var manager in s_Managers) manager.Dispose();
            await Frames(3);
        }
        catch (Exception exception) { s_Report.failures.Add("Harness: " + exception); }
        finally
        {
            Time.timeScale = previousTimeScale;
            Time.captureDeltaTime = previousCapture;
            foreach (var manager in s_Managers) manager.Dispose();
            foreach (var asset in s_Assets) UnityEngine.Object.DestroyImmediate(asset);
            UniTaskScheduler.UnobservedTaskException -= errorHandler;
            File.WriteAllText("results.json", JsonUtility.ToJson(s_Report, true));
            SessionState.SetInt(PhaseKey + ".Exit", s_Report.failures.Count == 0 ? 0 : 1);
            SessionState.SetString(PhaseKey, "finished");
            EditorApplication.isPlaying = false;
        }
    }

    private static async UniTask Check(string name, Func<UniTask> test)
    {
        try { await test(); s_Report.passed++; }
        catch (Exception exception) { s_Report.failures.Add(name + ": " + exception); }
        finally { Time.timeScale = 1; foreach (var manager in s_Managers) manager.Clear(); }
    }

    private static async UniTask Frames(int count)
    {
        for (int i = 0; i < count; i++) await UniTask.NextFrame(PlayerLoopTiming.LastPostLateUpdate);
    }

    private static CharacterAttribute CreateAttributes(bool initialize = true)
    {
        var asset = ScriptableObject.CreateInstance<ValueAttributeAsset>(); s_Assets.Add(asset);
        Set(asset, "m_AttributeName", ValueAttributeName.Health); Set(asset, "m_BaseValue", 1000);
        var attributes = new CharacterAttribute();
        Set(attributes, "m_ValueAttributeAssetList", new List<ValueAttributeAsset> { asset });
        if (initialize) attributes.Initialize();
        return attributes;
    }

    private static StatusEffectAsset CreateEffect(IStatusEffectStrategy strategy = null, float duration = 1f, float interval = 0.125f)
    {
        var asset = ScriptableObject.CreateInstance<StatusEffectAsset>(); s_Assets.Add(asset);
        Set(asset, "m_StatusEffectName", "TestEffect"); Set(asset, "m_StatusEffectType", StatusEffectType.Penalty);
        Set(asset, "m_StatusEffectValue", 10); Set(asset, "m_IsStackable", true); Set(asset, "m_StackMaxCount", 5);
        Set(asset, "m_HasDuration", duration > 0); Set(asset, "m_Duration", duration); Set(asset, "m_TickInterval", interval);
        Set(asset, "m_StatusEffectStrategies", strategy == null ? new List<IStatusEffectStrategy>() : new List<IStatusEffectStrategy> { strategy });
        return asset;
    }

    private static RuntimeStatusEffectManager CreateManager(CharacterAttribute attributes)
    {
        var manager = new RuntimeStatusEffectManager(attributes); s_Managers.Add(manager); return manager;
    }

    private static RuntimeStatusEffect Get(RuntimeStatusEffectManager manager, StatusEffectAsset asset)
    {
        Expect(manager.TryGetStatusEffect(asset, out var effect), "Missing running effect"); return effect;
    }

    private static void Value(CharacterAttribute attributes, int expected)
    {
        Expect(attributes.TryGetValues(ValueAttributeName.Health, out int current, out _), "Missing health"); Equal(expected, current);
    }

    private static void Set(object instance, string name, object value)
    {
        var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null) throw new Exception("Missing field: " + name);
        field.SetValue(instance, value);
    }

    private static void Equal<T>(T expected, T actual)
    {
        Expect(EqualityComparer<T>.Default.Equals(expected, actual), $"Expected {expected}, actual {actual}");
    }

    private static void Expect(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }
}
