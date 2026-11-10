using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using GP;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

public static class ScriptCleanupRegressionTests
{
    private const string PhaseKey = "ARPG.ScriptsCleanupTests.Phase";
    private static readonly List<Object> s_Objects = new();
    private static readonly Report s_Report = new();

    [Serializable]
    private class Report
    {
        public int passed;
        public List<string> passedTests = new();
        public List<string> failures = new();
    }

    private struct TestEvent : IEventContext { }

    private class ProbeAction : IItemAction
    {
        public ItemActionCategory ActionCategory { get; set; } = ItemActionCategory.Use;
        public bool CanRun = true;
        public int Executions;
        public Action<RuntimeItem> Callback;
        public bool CanExecute(RuntimeItem item) => CanRun;
        public void Execute(RuntimeItem item) { Executions++; Callback?.Invoke(item); }
    }

    private class TickProbe : IEnhancedInputInteractionTick
    {
        public event Action InteractionStarted;
        public event Action InteractionTriggered;
        public bool IsPendingRemoval { get; set; }
        public int Ticks;
        public Action Callback;
        public bool BeginInteraction() { InteractionStarted?.Invoke(); return true; }
        public bool EndInteraction() { InteractionTriggered?.Invoke(); return true; }
        public void ResetInteraction() { }
        public void TickInteraction() { Ticks++; Callback?.Invoke(); }
    }

    private class WarpProbe : IWarpInteraction
    {
        public WarpInteractionResult Result;
        public WarpInteractionResult WarpInteraction() => Result;
    }

    [InitializeOnLoadMethod]
    private static void Restore()
    {
        string phase = SessionState.GetString(PhaseKey, "");
        if (phase == "entering") EditorApplication.playModeStateChanged += OnPlayModeChanged;
        if (phase == "finished") EditorApplication.delayCall += Exit;
    }

    public static void Run()
    {
        Check("Player 脚本编译隔离", () =>
        {
            var settings = new UnityEditor.Build.Player.ScriptCompilationSettings
            {
                target = BuildTarget.StandaloneWindows64,
                group = BuildTargetGroup.Standalone
            };
            var result = UnityEditor.Build.Player.PlayerBuildInterface.CompilePlayerScripts(settings, "PlayerScriptValidation");
            Expect(result.assemblies != null && result.assemblies.Any(file => Path.GetFileName(file) == "Assembly-CSharp.dll"),
                "Player script compilation did not produce Assembly-CSharp.dll");
        });
        EditModeChecks();
        File.WriteAllText("edit-results.json", JsonUtility.ToJson(s_Report, true));
        SessionState.SetString(PhaseKey, "entering");
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetString(PhaseKey, "") == "finished") { Exit(); return; }
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        JsonUtility.FromJsonOverwrite(File.ReadAllText("edit-results.json"), s_Report);
        SessionState.SetString(PhaseKey, "running");
        RunPlayModeChecks().Forget();
    }

    private static void Exit() => EditorApplication.Exit(SessionState.GetInt(PhaseKey + ".Exit", 1));

    private static void EditModeChecks()
    {
        Check("分解批量容量、重复产物与完整写入事件", () =>
        {
            var bag = Track(new GameObject("BatchBag")).AddComponent<CharacterBag>();
            var product = Item(stackable: false);
            ItemTable Entry(ItemAsset asset, int count)
            {
                var entry = new ItemTable(); Set(entry, "m_ItemAsset", asset); Set(entry, "m_ItemCount", count); return entry;
            }
            var action = new DecomposeAction();
            Set(action, "m_RefundItems", new List<ItemTable> { Entry(product, 1), Entry(product, 1) });
            var original = Item(stackable: false, actions: new List<IItemAction> { action });
            bag.AddItem(original);
            var source = bag.GetItems()[0];
            for (int i = 0; i < 18; i++) bag.AddItem(Item(stackable: false));
            Equal(false, source.TryExecuteAction(ItemActionCategory.Decompose));
            Equal(19, bag.GetItems().Count); Equal(1, source.ItemStack);
            bag.RemoveItem(bag.GetItems()[1]);
            int events = 0;
            bag.ItemAdded += (_, count) =>
            {
                events += count;
                Equal(20, bag.GetItems().Count);
                Equal(2, bag.GetItems().Count(item => item.ItemAsset == product));
            };
            Equal(true, source.TryExecuteAction(ItemActionCategory.Decompose));
            Equal(2, events); Equal(0, source.ItemStack); Equal(19, bag.GetItems().Count);
            var multiBag = Track(new GameObject("MultiDecompose")).AddComponent<CharacterBag>();
            var single = new DecomposeAction(); Set(single, "m_RefundItems", new List<ItemTable> { Entry(product, 1) });
            multiBag.AddItem(Item(stackable: false, actions: new List<IItemAction> { single, single }));
            var multiSource = multiBag.GetItems()[0];
            for (int i = 0; i < 18; i++) multiBag.AddItem(Item(stackable: false));
            Equal(false, multiSource.TryExecuteAction(ItemActionCategory.Decompose)); Equal(19, multiBag.GetItems().Count); Equal(1, multiSource.ItemStack);
            var stacked = Item(max: 3);
            Equal(false, bag.CanAddItems(new[] { Entry(stacked, 2), Entry(stacked, 2) }));
        });
        Check("币种共享面额、独立余额与事件身份", () =>
        {
            var bag = Track(new GameObject("Wallet")).AddComponent<CharacterBag>();
            var gold = Currency(10); var denomination = Currency(5); Set(denomination, "m_CurrencyName", "Other name");
            var silver = Currency(2); Set(silver, "m_Category", CurrencyCategory.Silver);
            var events = new List<(CurrencyCategory, int)>();
            bag.CurrencyChanged += (category, amount) => events.Add((category, amount));
            bag.AddCurrency(gold, 2); bag.AddCurrency(denomination); bag.AddCurrency(silver, 3);
            Equal(25, bag.GetCurrency(CurrencyCategory.Gold).CurrencyWorth);
            Equal(6, bag.GetCurrency(CurrencyCategory.Silver).CurrencyWorth);
            Equal((CurrencyCategory.Silver, 6), events[2]);
            bag.GetCurrency(CurrencyCategory.Gold).AddCurrency(silver); Equal(25, bag.GetCurrency(CurrencyCategory.Gold).CurrencyWorth);
            Equal(true, bag.TrySpendCurrency(CurrencyCategory.Gold, 5)); Equal((CurrencyCategory.Gold, 20), events[3]);
            Equal(false, bag.TrySpendCurrency(CurrencyCategory.None, 0));
        });
        Check("Warping 跟随局部锚点、跳帧、零窗口及失效清理", () =>
        {
            var character = Track(new GameObject("WarpCharacter")).transform;
            var target = Track(new GameObject("WarpTarget")).transform;
            target.position = new Vector3(4, 0, 2);
            var point = new WarpingPoint(target, target.TransformPoint(Vector3.right));
            target.SetPositionAndRotation(new Vector3(8, 1, 3), Quaternion.Euler(0, 90, 0));
            Near(target.TransformPoint(Vector3.right), point.GetWorldPosition());
            var asset = Track(ScriptableObject.CreateInstance<MotionWarpingAsset>());
            var first = new MotionWarpWindow(); Set(first, "m_WarpWindow", new Vector2Int(1, 3));
            var second = new MotionWarpWindow(); Set(second, "m_WarpWindow", new Vector2Int(6, 6));
            Set(asset, "m_WarpingWindows", new List<MotionWarpWindow> { first, second }); Set(asset, "m_IsSyncAnimate", true);
            var control = new AnimationMotionWarpingControl(character);
            var probe = new WarpProbe { Result = new WarpInteractionResult { ValidResult = true, WarpingAsset = asset, WarpingPoints = new[] { point, point } } };
            Equal(true, control.Interact(probe)); control.ProcessWarpingStatus(5); Equal(WarpingStatus.Waiting, control.WarpingStatus);
            control.ProcessWarpingStatus(6); Equal(WarpingStatus.Running, control.WarpingStatus);
            Near(point.GetWorldPosition(), control.WarpingTranslate(Vector3.zero, 6));
            control.CompleteFrame(6); Equal(false, control.EnableWarping);
            control.Interact(probe); control.ProcessWarpingStatus(10); Equal(false, control.EnableWarping);
            control.Interact(probe); control.InterruptWarping(); Equal(false, control.EnableWarping);
            control.Interact(probe); Object.DestroyImmediate(target.gameObject); control.ProcessWarpingStatus(1); Equal(false, control.EnableWarping);
        });
        Check("连招行删除使用绑定 ID，撤销后详情重新关联", () =>
        {
            var tree = Track(ScriptableObject.CreateInstance<ComboTreeAsset>());
            int a = tree.AddRootNode(new ComboActionData()); int b = tree.AddRootNode(new ComboActionData());
            var editor = (ComboTreeAssetEditor)Track(UnityEditor.Editor.CreateEditor(tree)); editor.CreateInspectorGUI();
            editor.TreeView.SetSelectionById(a);
            var row = new ComboTreeNodeView(editor); row.BindItem(b, tree.GetActionData(b));
            editor.RemoveNode(Get<int>(row, "m_NodeId")); Undo.FlushUndoRecordObjects();
            Expect(tree.GetActionData(a) != null && tree.GetActionData(b) == null, "Wrong node removed");
            Undo.PerformUndo(); Expect(tree.GetActionData(b) != null, "Undo did not restore B");
            Equal(true, ReferenceEquals(tree.GetActionData(a), Get<ComboActionData>(editor, "m_CurrentSelectedComboActionData")));
            Undo.PerformRedo(); Expect(tree.GetActionData(b) == null, "Redo did not remove B");
            row.BindItem(-1, null); Equal("", Get<UnityEngine.UIElements.Label>(row, "m_Label").text);
        });
        Check("Warping 采样不修改原模型且支持 Undo", () =>
        {
            var model = Track(new GameObject("SamplingModel")); model.transform.position = Vector3.right * 5;
            var child = new GameObject("Bone"); child.transform.SetParent(model.transform, false); child.transform.localPosition = Vector3.up;
            var clip = Track(new AnimationClip { frameRate = 1f });
            clip.SetCurve("", typeof(Transform), "localPosition.z", AnimationCurve.Linear(0, 0, 3, 3));
            clip.SetCurve("Bone", typeof(Transform), "localPosition.y", AnimationCurve.Linear(0, 10, 3, 20));
            var asset = Track(ScriptableObject.CreateInstance<MotionWarpingAsset>());
            var window = new MotionWarpWindow(); Set(window, "m_WarpWindow", new Vector2Int(0, 3));
            Set(asset, "m_SampleClip", clip); Set(asset, "m_SampleModel", model); Set(asset, "m_WarpingWindows", new List<MotionWarpWindow> { window });
            MotionWarpingAssetEditor.Sample(asset); Undo.FlushUndoRecordObjects();
            Near(Vector3.right * 5, model.transform.position); Near(Vector3.up, child.transform.localPosition);
            Expect(asset.WarpingWindows[0].GetWarpFrameMotionData(0).z > 0f, "Sampling produced no root motion");
            Undo.PerformUndo(); Near(Vector3.zero, asset.WarpingWindows[0].GetWarpFrameMotionData(0));
        });
        Check("批量注销事件并幂等释放令牌", () =>
        {
            int count = 0;
            var first = EventCenter<TestEvent>.Subscribe(_ => count++);
            var second = EventCenter<TestEvent>.Subscribe(_ => count++);
            var binder = (GameEventTokenBinder)typeof(EventCenter<TestEvent>).GetField("s_EventTokenBinder", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            EventCenter<TestEvent>.Publish(default); Equal(2, count);
            binder.DisposeAllEventTokens();
            Equal(true, first.IsDisposed); Equal(true, second.IsDisposed);
            first.Unsubscribe(); second.Unsubscribe();
            EventCenter<TestEvent>.Publish(default); Equal(2, count);
        });
        Check("重复订阅同一回调可逐条注销", () =>
        {
            int count = 0;
            EventCenter<TestEvent>.EventDelegate callback = _ => count++;
            var first = EventCenter<TestEvent>.Subscribe(callback);
            var second = EventCenter<TestEvent>.Subscribe(callback);
            first.Unsubscribe(); EventCenter<TestEvent>.Publish(default); Equal(1, count);
            second.Unsubscribe(); EventCenter<TestEvent>.Publish(default); Equal(1, count);
        });
        Check("堆叠合并保留唯一物品实例与正确事件", () =>
        {
            var asset = Item(max: 10); var inventory = new ItemInventory(null, ItemCategory.Consumable);
            RuntimeItem first = null, second = null; int events = 0;
            inventory.ItemAdded += (item, _) => { if (events++ == 0) first = item; else second = item; };
            Equal(3, inventory.AddRuntimeItem(asset, 3)); Equal(2, inventory.AddRuntimeItem(asset, 2));
            Equal(1, inventory.GetActiveItems().Count); Equal(true, ReferenceEquals(first, second)); Equal(5, first.ItemStack);
            Equal(0, inventory.AddRuntimeItem(first));
        });
        Check("容量限制仍允许合并已有堆叠", () =>
        {
            var inventory = new ItemInventory(null, ItemCategory.Consumable); var first = Item(max: 50);
            inventory.AddRuntimeItem(first, 1);
            for (int i = 1; i < 20; i++) Equal(1, inventory.AddRuntimeItem(Item(stackable: false)));
            Equal(true, inventory.IsInventoryFull); Equal(0, inventory.AddRuntimeItem(Item()));
            Equal(49, inventory.AddRuntimeItem(first, int.MaxValue)); Equal(20, inventory.GetActiveItems().Count);
            inventory.SetInventoryMaxCapacity(30); Equal(30, inventory.InventoryMaxCapacity);
        });
        Check("非法数量不修改库存且移除事件剩余量正确", () =>
        {
            var asset = Item(max: 10); var inventory = new ItemInventory(null, ItemCategory.Consumable);
            Equal(0, inventory.AddRuntimeItem(asset, -1)); inventory.AddRuntimeItem(asset, 5);
            var item = inventory.GetActiveItems()[0]; int remaining = -1;
            inventory.ItemRemoved += (_, value) => remaining = value;
            Equal(0, inventory.RemoveRuntimeItem(item, -1)); Equal(5, item.ItemStack);
            Equal(2, inventory.RemoveRuntimeItem(item, 2)); Equal(3, remaining);
            Equal(3, inventory.RemoveRuntimeItem(item, int.MaxValue)); Equal(0, remaining); Equal(0, inventory.GetActiveItems().Count);
        });
        Check("Pop不存在的物品安全且取出物品保留数量", () =>
        {
            var inventory = new ItemInventory(null, ItemCategory.Consumable);
            Equal<RuntimeItem>(null, inventory.Pop(123)); inventory.AddRuntimeItem(Item(max: 10), 4);
            var item = inventory.GetActiveItems()[0]; int remaining = -1;
            inventory.ItemRemoved += (_, count) => remaining = count;
            Equal(item, inventory.Pop(item.RuntimeItemId)); Equal(4, item.ItemStack); Equal(0, remaining); Equal(0, inventory.GetActiveItems().Count);
        });
        Check("背包转移数量与所有权正确且回调读到已提交状态", () =>
        {
            var source = Bag(); var target = Bag(); var asset = Item(max: 20);
            RuntimeItem sourceItem = null, targetItem = null;
            source.ItemAdded += (item, _) => sourceItem = item; source.AddItem(asset, 10);
            target.ItemAdded += (item, _) => { targetItem = item; Equal(7, sourceItem.ItemStack); Equal(3, item.ItemStack); };
            Equal(true, source.TransferItem(target, sourceItem, 3));
            Equal(target, targetItem.ItemInventory.CharacterBag); Equal(7, sourceItem.ItemStack);
            Equal(false, source.TransferItem(source, sourceItem)); Equal(false, source.TransferItem(target, sourceItem, 99));
        });
        Check("目标满堆时转移失败且来源物品不减少", () =>
        {
            var source = Bag(); var target = Bag(); var asset = Item(max: 3); RuntimeItem item = null;
            source.ItemAdded += (value, _) => item = value;
            source.AddItem(asset, 3); target.AddItem(asset, 3);
            Equal(false, source.TransferItem(target, item)); Equal(3, item.ItemStack);
        });
        Check("不可执行的物品行为不生效也不消耗", () =>
        {
            var action = new ProbeAction { CanRun = false }; var inventory = new ItemInventory(null, ItemCategory.Consumable);
            inventory.AddRuntimeItem(Item(actions: new List<IItemAction> { action })); var item = inventory.GetActiveItems()[0];
            Equal(false, item.TryExecuteAction(ItemActionCategory.Use)); Equal(0, action.Executions); Equal(1, item.ItemStack);
        });
        Check("物品行为可拒绝重入且装备不消耗物品", () =>
        {
            var action = new ProbeAction { ActionCategory = ItemActionCategory.Equip };
            action.Callback = item => Equal(false, item.TryExecuteAction(ItemActionCategory.Equip));
            var inventory = new ItemInventory(null, ItemCategory.Consumable);
            inventory.AddRuntimeItem(Item(actions: new List<IItemAction> { action })); var item = inventory.GetActiveItems()[0];
            Equal(true, item.TryExecuteAction(ItemActionCategory.Equip)); Equal(1, action.Executions); Equal(1, item.ItemStack);
        });
        Check("空物品行为与大整数数量安全", () =>
        {
            var inventory = new ItemInventory(null, ItemCategory.Consumable);
            inventory.AddRuntimeItem(Item(max: int.MaxValue, actions: new List<IItemAction> { null }), int.MaxValue);
            var item = inventory.GetActiveItems()[0]; Equal(false, item.AddItemStack(int.MaxValue)); Equal(int.MaxValue, item.ItemStack);
            Equal(false, item.RemoveItemStack(-1)); Throws<ArgumentNullException>(() => new RuntimeItem(null, inventory, 1));
        });
        Check("货币乘法和加法不溢出且拒绝透支", () =>
        {
            var asset = Currency(int.MaxValue); var currency = new RuntimeCurrency(asset);
            currency.AddCurrency(asset, int.MaxValue); Equal(99999, currency.CurrencyWorth);
            currency.AddCurrency(-1); currency.SubtractCurrency(-1); Equal(99999, currency.CurrencyWorth);
            Equal(false, currency.TrySpend(100000)); Equal(false, currency.TrySpend(-1)); Equal(true, currency.TrySpend(99999)); Equal(0, currency.CurrencyWorth);
        });
        Check("商店成功扣款减库存且满背包时退款", () =>
        {
            var bag = Bag(); var currency = Currency(10); bag.AddCurrency(currency, 2);
            var item = Item(max: 1); var stock = new ShopItem(); Set(stock, "m_ItemAsset", item); Set(stock, "m_ItemPrice", currency); Set(stock, "m_ItemCount", 3);
            var shop = Track(new GameObject("Shop")).AddComponent<Shop>(); Set(shop, "m_TargetCharacterBag", bag);
            Equal(true, shop.BuyItem(stock)); Equal(2, stock.ItemCount); Equal(10, bag.GetCurrency(currency.CurrencyCategory).CurrencyWorth);
            Equal(false, shop.BuyItem(stock)); Equal(2, stock.ItemCount); Equal(10, bag.GetCurrency(currency.CurrencyCategory).CurrencyWorth);
        });
        Check("零长度线段与投影边界返回有限位置", () =>
        {
            var point = new Vector3(3, 4, 5); Near(Vector3.one, point.GetClosestPointOnSegment(Vector3.one, Vector3.one));
            Near(Vector3.right * 2, new Vector3(2, 5, 0).GetClosestPointOnSegment(Vector3.zero, Vector3.right * 4));
            Near(Vector3.right * 4, (Vector3.right * 9).GetClosestPointOnSegment(Vector3.zero, Vector3.right * 4));
        });
        Check("音频随机选择包含最后一条并安全处理空数组", () =>
        {
            var asset = Track(ScriptableObject.CreateInstance<AudioAsset>()); Equal<AudioClip>(null, asset.GetSound());
            var first = Track(AudioClip.Create("First", 100, 1, 1000, false)); var last = Track(AudioClip.Create("Last", 100, 1, 1000, false));
            Set(asset, "m_Sounds", new[] { first, last }); var state = UnityEngine.Random.state; UnityEngine.Random.InitState(20261107);
            bool sawFirst = false, sawLast = false;
            for (int i = 0; i < 64; i++) { var clip = asset.GetSound(); sawFirst |= clip == first; sawLast |= clip == last; }
            UnityEngine.Random.state = state; Equal(true, sawFirst && sawLast);
        });
        Check("无效帧率被拒绝", () =>
        {
            foreach (double rate in new[] { 0d, -1d, double.NaN, double.PositiveInfinity })
                Throws<ArgumentException>(() => AnimationTimeUtility.ToFrames(1, rate));
            Equal(60, AnimationTimeUtility.ToFrames(1, 60));
        });
        Check("旧状态效果资产保留策略及持续时间", () =>
        {
            var asset = AssetDatabase.LoadAssetAtPath<StatusEffectAsset>("Assets/ScriptableObject/StatusEffect/Poison.asset");
            Expect(asset != null, "Poison asset missing"); Equal(true, asset.HasDuration); Equal(3f, asset.Duration);
            Equal(1, asset.StatusEffectStrategies.Count); Expect(asset.StatusEffectStrategies[0] is PeriodicStatusEffectStrategy, "Legacy strategy type was lost");
        });
        Check("旧物品和动画通知资产保留托管引用", () =>
        {
            var asset = AssetDatabase.LoadAssetAtPath<ItemAsset>("Assets/ScriptableObject/Inventory/鸡腿.asset");
            Expect(asset != null, "Item asset missing"); Equal(3, asset.MaxStackSize); Expect(asset.ItemActions[0] is ModifyAttributeAction, "Legacy item action was lost");
            var animation = AssetDatabase.LoadAssetAtPath<AnimationAsset>("Assets/ScriptableObject/Animations/New-AnimateAsset-Attack1.asset");
            Expect(animation != null, "Animation asset missing"); Expect(animation.NotifyList.Notifies.Any(notify => notify is AudioAnimationNotify), "Legacy audio notify was lost");
        });
        Check("原生预制体保留旧脚本与旧序列化字段", () =>
        {
            const string path = "Assets/GeneratedLegacyFixture.prefab";
            var root = Track(new GameObject("LegacyFixture")); root.SetActive(false);
            var locomotion = root.AddComponent<ThirdPersonLocomotion>();
            Set(Get<SwimmingLocomotion>(locomotion.LocomotionAddOnManager, "m_Swimming"), "m_HasStartedSwimming", true);
            var camera = root.AddComponent<ThirdPersonCamera>(); Set(camera, "m_TrackingDistance", 13f);
            var rope = root.AddComponent<Rope>(); Set(rope, "m_RopeDirectionType", Rope.RopeDirectionType.Vertical);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            string yaml = File.ReadAllText(path).Replace("m_HasStartedSwimming:", "m_IsStartSwimming:")
                .Replace("m_TrackingDistance:", "m_ByTrackTargetDistance:").Replace("m_RopeDirectionType:", "_ropeDirectionType:")
                .Replace("GP.ThirdPersonLocomotion", "GP.TP_Locomotion").Replace("GP.ThirdPersonCamera", "GP.TP_MainCamera");
            File.WriteAllText(path, yaml); AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var restored = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Equal(true, Get<bool>(Get<SwimmingLocomotion>(restored.GetComponent<ThirdPersonLocomotion>().LocomotionAddOnManager, "m_Swimming"), "m_HasStartedSwimming"));
            Equal(13f, Get<float>(restored.GetComponent<ThirdPersonCamera>(), "m_TrackingDistance"));
            Equal(Rope.RopeDirectionType.Vertical, restored.GetComponent<Rope>().RopeDirection);
        });
        Check("重命名脚本仍可解析主类", () =>
        {
            foreach (var pair in new Dictionary<string, Type>
            {
                ["Runtime/Character/Movement/ThirdPersonLocomotion.cs"] = typeof(ThirdPersonLocomotion),
                ["Runtime/Character/Camera/ThirdPersonCamera.cs"] = typeof(ThirdPersonCamera),
                ["Runtime/Inventory/Bag/Shop.cs"] = typeof(Shop),
                ["Test/AttributeEffectTest.cs"] = typeof(AttributeEffectTest)
            }) Equal(pair.Value, AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/" + pair.Key).GetClass());
        });
        Check("连招树编辑支持撤销与恢复", () =>
        {
            var tree = Track(ScriptableObject.CreateInstance<ComboTreeAsset>());
            var editor = Track(UnityEditor.Editor.CreateEditor(tree)) as ComboTreeAssetEditor; editor.CreateInspectorGUI();
            Invoke(editor, "AddRootNode"); Equal(1, tree.RootIds.Count); Undo.FlushUndoRecordObjects();
            Undo.PerformUndo(); Equal(0, tree.RootIds.Count); Undo.PerformRedo(); Equal(1, tree.RootIds.Count);
        });
        Check("四阶段扭曲初始化与无效请求返回值", () =>
        {
            var character = Track(new GameObject("WarpCharacter")).transform; var controller = new AnimationMotionWarpingControl(character);
            var asset = Track(ScriptableObject.CreateInstance<MotionWarpingAsset>()); var windows = new List<MotionWarpWindow>();
            for (int i = 0; i < 4; i++) windows.Add(new MotionWarpWindow()); Set(asset, "m_WarpingWindows", windows);
            var point = new WarpingPoint(character, Vector3.forward);
            var probe = new WarpProbe { Result = new WarpInteractionResult { WarpingAsset = asset, WarpingPoints = new[] { point, point, point, point }, ValidResult = true } };
            Equal(true, controller.Interact(probe)); Equal(true, controller.EnableWarping);
            probe.Result.WarpingPoints = new[] { point }; Equal(false, controller.Interact(probe));
            Near(Vector3.zero, new MotionWarpWindow().GetWarpFrameMotionData(0));
        });
    }

    private static async UniTask RunPlayModeChecks()
    {
        Action<Exception> errorHandler = exception => s_Report.failures.Add("Unexpected async exception: " + exception);
        Application.LogCallback logHandler = (condition, trace, type) => { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) s_Report.failures.Add(condition + "\n" + trace); };
        UniTaskScheduler.UnobservedTaskException += errorHandler; Application.logMessageReceived += logHandler;
        float previousCapture = Time.captureDeltaTime; Time.captureDeltaTime = 0.125f;
        try
        {
            await Frames(3);
            await CheckAsync("输入初始化幂等且启用不重复订阅", async () =>
            {
                var manager = GameInputManager.Instance; var original = manager.QKey; manager.InitializeInputs(); Equal(original, manager.QKey);
                var previousBackground = InputSystem.settings.backgroundBehavior;
                var previousEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                var keyboard = InputSystem.AddDevice<Keyboard>(); InputSystem.EnableDevice(keyboard);
                int presses = 0; Action pressed = () => presses++;
                original.ButtonPressed += pressed;
                try
                {
                    original.Enable(); original.Enable();
                    Equal(true, Get<GP_InputAction>(manager, "m_InputActions").asset.enabled);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Q)); InputSystem.Update(); Equal(1, presses);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
                }
                finally
                {
                    original.ButtonPressed -= pressed; InputSystem.RemoveDevice(keyboard);
                    InputSystem.settings.backgroundBehavior = previousBackground;
                    InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInput;
                }
                await Frames(1);
            });
            await CheckAsync("输入轮询允许回调添加交互且撤销移除有效", () =>
            {
                var manager = GameInputManager.Instance; var added = new TickProbe(); var first = new TickProbe();
                first.Callback = () => manager.AddInteractionTick(added);
                manager.AddInteractionTick(first); Invoke(manager, "TickInteractions"); Equal(0, added.Ticks);
                Invoke(manager, "TickInteractions"); Equal(1, added.Ticks);
                manager.RemoveInteractionTick(added); manager.AddInteractionTick(added); Invoke(manager, "TickInteractions"); Equal(2, added.Ticks);
                manager.RemoveInteractionTick(first); manager.RemoveInteractionTick(added); Invoke(manager, "TickInteractions");
                return UniTask.CompletedTask;
            });
            await CheckAsync("禁用输入取消长按且重新启用后可触发", async () =>
            {
                var manager = GameInputManager.Instance; var hold = new EnhancedInputHold(0.25f, true); int triggers = 0; hold.InteractionTriggered += () => triggers++;
                manager.QKey.SetInteraction(hold); hold.BeginInteraction(); manager.enabled = false;
                await Frames(4); Equal(0, triggers); manager.enabled = true; hold.BeginInteraction(); await Frames(4); Equal(1, triggers);
                manager.QKey.RemoveInteraction(hold);
            });
            await CheckAsync("销毁输入管理器安全释放全部包装器", () =>
            {
                var manager = GameInputManager.Instance; var button = manager.QKey; var movement = manager.MovementInput;
                Object.DestroyImmediate(manager.gameObject); Equal(true, button.IsDisposed); Equal(true, movement.IsDisposed);
                Equal(false, GameInputManager.SingletonIsValid); button.Dispose(); movement.Dispose();
                Expect(GameInputManager.Instance != null, "Manager could not be recreated"); return UniTask.CompletedTask;
            });
            await CheckAsync("对象池预加载上限重复回收与清理", async () =>
            {
                var pool = Pool(out _, max: 2); pool.LoadUnit(99); Equal(2, pool.UnitTotalCount);
                var unit = pool.TakeUnit(); Equal(1, pool.ActiveUnitCount); pool.RecycleUnit(unit); pool.RecycleUnit(unit);
                Equal(2, pool.AvailableUnitCount); Equal(2, pool.UnitTotalCount);
                var active = pool.TakeUnit(); var instance = active.GetUnitGameObject(); instance.transform.SetParent(null);
                pool.ClearPool(); Equal(0, pool.UnitTotalCount); await Frames(2); Equal(true, instance == null);
            });
            await CheckAsync("旧回收计时不会回收已重新借出的对象", async () =>
            {
                var pool = Pool(out _, max: 1); pool.LoadUnit(1);
                var unit = (PoolTimerProbe)pool.TakeUnit(); unit.StartTimer(0.25f); await Frames(1); unit.RecycleUnit();
                Equal(unit, pool.TakeUnit()); unit.StartTimer(1.5f); await Frames(5); Equal(1, pool.ActiveUnitCount);
                await Frames(10); Equal(0, pool.ActiveUnitCount); Equal(1, pool.AvailableUnitCount); pool.ClearPool();
            });
            await CheckAsync("空相机目标与零移动速度不会产生异常或NaN", () =>
            {
                var camera = Track(new GameObject("CameraRig")).AddComponent<ThirdPersonCamera>(); Invoke(camera, "LateUpdate");
                var locomotion = Locomotion(); Set(locomotion, "m_RunSpeed", 0f); Invoke(locomotion, "UpdateMovement");
                Expect(!float.IsNaN(locomotion.NormalizedLocomotionSpeed), "Normalized speed is NaN"); return UniTask.CompletedTask;
            });
            await CheckAsync("坡道检测保留外部输入冻结", () =>
            {
                var locomotion = Locomotion(); locomotion.SetInputFrozen(true); Invoke(locomotion, "CheckGround"); Equal(true, Get<bool>(locomotion, "m_FreezeInput"));
                return UniTask.CompletedTask;
            });
            await CheckAsync("根运动按帧位移和相对旋转应用", () =>
            {
                var locomotion = Locomotion(); var source = Track(new GameObject("AnimationSource")); source.SetActive(false);
                var animation = source.AddComponent<AnimationSource>(); Set(animation, "m_AutoCreate", false); source.SetActive(true); animation.enabled = false;
                Set(animation, "<RootMotion>k__BackingField", (Vector3.forward * 0.2f, Quaternion.Euler(0, 10, 0)));
                Set(animation, "<RootMotionWeight>k__BackingField", 1f); Set(locomotion, "m_AnimationSource", animation);
                locomotion.SetCharacterPosition(new Vector3(0, -100, 0)); locomotion.transform.rotation = Quaternion.Euler(0, 70, 0);
                Invoke(locomotion, "UpdateMovement"); Near(new Vector3(0, -100, 0.2f), locomotion.transform.position);
                Expect(Mathf.Abs(Mathf.DeltaAngle(80, locomotion.transform.eulerAngles.y)) < 0.01f, "Root rotation was treated as an absolute rotation");
                return UniTask.CompletedTask;
            });
            await CheckAsync("根运动开关、Animator 输出隔离和取消清理", async () =>
            {
                var source = Animation(out var asset); Set(asset, "m_UseRootMotion", false); source.PlayAnimation(asset);
                await Frames(1);
                Invoke(source, "OnAnimatorMove"); Equal(false, source.RunningState.UseRootMotion); Equal(0f, source.RootMotionWeight);
                Near(Vector3.zero, source.RootMotion.PositionDelta); Equal(Quaternion.identity, source.RootMotion.RotationDelta);
                Set(asset, "m_UseRootMotion", true); Set(source, "m_RootMotionByAnimator", true);
                Invoke(source, "OnAnimatorMove"); Equal(0f, source.RootMotionWeight); Near(Vector3.zero, source.RootMotion.PositionDelta);
                var warp = Track(ScriptableObject.CreateInstance<MotionWarpingAsset>());
                var window = new MotionWarpWindow(); Set(window, "m_WarpWindow", new Vector2Int(0, 20)); Set(warp, "m_WarpingWindows", new List<MotionWarpWindow> { window });
                var target = Track(new GameObject("Target")).transform; target.position = Vector3.forward * 10;
                source.Warp(new WarpProbe { Result = new WarpInteractionResult { ValidResult = true, WarpingAsset = warp, WarpingPoints = new[] { new WarpingPoint(target, target.position) } } });
                var control = Get<AnimationMotionWarpingControl>(source, "m_WarpingControl"); control.ProcessWarpingStatus(1);
                Equal(true, source.WarpingRunning);
                var locomotion = Locomotion(); Set(locomotion, "m_AnimationSource", source);
                var before = locomotion.transform.position; Invoke(locomotion, "DoMove", Vector3.forward); Near(before, locomotion.transform.position);
                source.ForceStopAnimation(); Equal(false, source.WarpingRunning); Equal(0f, source.RootMotionWeight);
                await Frames(2);
                Invoke(source, "OnAnimatorMove"); Equal(0f, source.RootMotionWeight);
            });
            await CheckAsync("游泳退出恢复参数且滑索方向属于角色", () =>
            {
                var locomotion = Locomotion();
                var swimming = Get<SwimmingLocomotion>(locomotion.LocomotionAddOnManager, "m_Swimming");
                var collider = locomotion.GetComponent<CharacterController>(); float height = collider.height; float radius = collider.radius; Vector3 center = collider.center;
                swimming.CutInLocomotion(); locomotion.SetCharacterColliderSize(1f, .2f, Vector3.up);
                locomotion.SetGravityEnabled(false); locomotion.SetGroundCheckEnabled(false);
                Set(swimming, "m_HasStartedSwimming", true); Set(swimming, "m_TriggerUnderWater", true);
                swimming.CutOutLocomotion(); Equal(height, collider.height); Equal(radius, collider.radius); Near(center, collider.center);
                Equal(true, Get<bool>(locomotion, "m_UseGravity")); Equal(true, Get<bool>(locomotion, "m_EnableGroundCheck"));
                Equal(Get<float>(locomotion, "m_AirDrag"), Get<float>(locomotion, "m_LocomotionGravityDrag"));
                Equal(false, Get<bool>(swimming, "m_HasStartedSwimming")); Equal(false, Get<bool>(swimming, "m_TriggerUnderWater"));
                var rope = Track(new GameObject("Rope")).AddComponent<Rope>();
                var start = Track(new GameObject("Start")).transform; var end = Track(new GameObject("End")).transform; end.position = Vector3.forward * 10;
                Set(rope, "m_StartPosition", start); Set(rope, "m_EndPosition", end);
                var first = new ZiplineLocomotion(); var second = new ZiplineLocomotion();
                Set(first, "m_Rope", rope); Set(second, "m_Rope", rope); Set(first, "m_IsDirectionReversed", true);
                var direction = typeof(ZiplineLocomotion).GetProperty("MoveDirection", BindingFlags.Instance | BindingFlags.NonPublic);
                Near(Vector3.back, (Vector3)direction.GetValue(first)); Near(Vector3.forward, (Vector3)direction.GetValue(second));
                first.Initialize(locomotion.LocomotionAddOnManager);
                Set(first, "m_TriggerObject", rope.gameObject); Set(first, "m_UsingZipline", true);
                var other = Track(new GameObject("OtherZipline")); other.tag = "Zipline";
                first.TriggerExit(other.AddComponent<BoxCollider>()); Equal(rope.gameObject, first.TriggerObject); Equal(true, Get<bool>(first, "m_UsingZipline"));
                first.OnDestroy();
                return UniTask.CompletedTask;
            });
            await CheckAsync("Animator代理重新启用后恢复事件订阅", () =>
            {
                var locomotion = Locomotion(); var model = Track(new GameObject("Model")); model.transform.SetParent(locomotion.transform);
                model.AddComponent<Animator>(); var agent = model.AddComponent<AnimatorAgent>(); agent.enabled = false; agent.enabled = true;
                var callback = Get<Action<LocomotionMode>>(locomotion.LocomotionAddOnManager, "LocomotionModeChanged");
                Expect(callback != null, "Mode subscription missing after re-enable"); callback(LocomotionMode.WaterLocomotion);
                Equal(1f, Get<float>(agent, "m_LocomotionModeValue")); return UniTask.CompletedTask;
            });
            await CheckAsync("相机碰撞选择最近障碍且禁用后恢复距离", () =>
            {
                var target = Track(new GameObject("CameraTarget")).transform; target.position = new Vector3(100, 20, 100);
                var rig = Track(new GameObject("CameraRig")); rig.SetActive(false);
                var child = new GameObject("Camera"); child.transform.SetParent(rig.transform, false); child.AddComponent<Camera>();
                var camera = rig.AddComponent<ThirdPersonCamera>(); Set(camera, "m_TrackingTarget", target); Set(camera, "m_TrackingDistance", 15f); Set(camera, "m_CameraCollisionMask", (LayerMask)(1 << 31));
                rig.SetActive(true); camera.enabled = false;
                for (int i = 0; i < 12; i++)
                {
                    var obstacle = Track(new GameObject("Obstacle")); obstacle.layer = 31; obstacle.transform.position = target.position - Vector3.forward * (i + 1);
                    obstacle.AddComponent<BoxCollider>().size = new Vector3(1, 2, 0.1f);
                }
                Physics.SyncTransforms(); Invoke(camera, "LateUpdate");
                Expect(Vector3.Distance(child.transform.position, target.position) < 1f, "Camera did not stop at the closest obstacle");
                Set(camera, "m_EnableCameraCollision", false); Invoke(camera, "LateUpdate"); Near(rig.transform.position, child.transform.position);
                return UniTask.CompletedTask;
            });
            await CheckAsync("零缓出动画按时完成且名称标签有效", async () =>
            {
                var source = Animation(out var asset); source.PlayAnimation(asset);
                Equal("TestAnimation", source.RunningState.AnimationName); Equal("Attack", source.RunningState.AnimationTag);
                await Frames(12); Equal(false, source.IsAnimationRunning);
            });
            await CheckAsync("零时长打断立即完成", async () =>
            {
                var source = Animation(out var asset); source.PlayAnimation(asset);
                source.RunningState.ForceInterruptAnimation(0f); await Frames(2); Equal(false, source.IsAnimationRunning);
            });
            await CheckAsync("组件工厂只添加一个目标组件", () =>
            {
                var box = GameManager.RequireGameObject<BoxCollider>(); Track(box.gameObject); Equal(1, box.GetComponents<BoxCollider>().Length);
                return UniTask.CompletedTask;
            });
            await Frames(3);
        }
        catch (Exception exception) { s_Report.failures.Add("Harness: " + exception); }
        finally
        {
            Cleanup(); Time.captureDeltaTime = previousCapture;
            Application.logMessageReceived -= logHandler; UniTaskScheduler.UnobservedTaskException -= errorHandler;
            File.WriteAllText("results.json", JsonUtility.ToJson(s_Report, true));
            SessionState.SetInt(PhaseKey + ".Exit", s_Report.failures.Count == 0 ? 0 : 1); SessionState.SetString(PhaseKey, "finished");
            EditorApplication.isPlaying = false;
        }
    }

    private static RuntimePool Pool(out PoolTimerProbe prefab, int max)
    {
        var root = Track(new GameObject("PoolRoot")); var prefabObject = Track(new GameObject("PoolPrefab")); prefabObject.SetActive(false);
        prefab = prefabObject.AddComponent<PoolTimerProbe>(); var asset = Track(ScriptableObject.CreateInstance<PoolAsset>());
        Set(asset, "m_Prefab", prefab); Set(asset, "m_PoolMaxSize", max); Set(asset, "m_PoolName", "Test");
        return new RuntimePool(asset, root.transform);
    }

    private static ThirdPersonLocomotion Locomotion()
    {
        var root = Track(new GameObject("Character")); var locomotion = root.AddComponent<ThirdPersonLocomotion>(); locomotion.enabled = false;
        return locomotion;
    }

    private static AnimationSource Animation(out AnimationAsset asset)
    {
        var clip = Track(new AnimationClip { frameRate = 30f });
        clip.SetCurve("", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0, 0, 1, 0));
        asset = Track(ScriptableObject.CreateInstance<AnimationAsset>()); Set(asset, "m_AnimationClip", clip);
        Set(asset, "m_EaseInDuration", 0f); Set(asset, "m_EaseOutDuration", 0f); Set(asset, "m_AnimationName", "TestAnimation"); Set(asset, "m_AnimationTag", "Attack");
        var root = Track(new GameObject("AnimationCharacter")); root.SetActive(false); root.AddComponent<Animator>();
        var source = root.AddComponent<AnimationSource>(); root.SetActive(true); return source;
    }

    private static ItemAsset Item(bool stackable = true, int max = 3, List<IItemAction> actions = null)
    {
        var asset = Track(ScriptableObject.CreateInstance<ItemAsset>());
        Set(asset, "m_ItemName", "TestItem"); Set(asset, "m_ItemCategory", ItemCategory.Consumable); Set(asset, "m_IsStackable", stackable); Set(asset, "m_MaxStackSize", max);
        if (actions != null) Set(asset, "m_ItemActions", actions);
        return asset;
    }

    private static CurrencyAsset Currency(int worth)
    {
        var asset = Track(ScriptableObject.CreateInstance<CurrencyAsset>()); Set(asset, "m_CurrencyName", "TestGold"); Set(asset, "m_Category", CurrencyCategory.Gold); Set(asset, "m_CurrencyValue", worth); return asset;
    }

    private static CharacterBag Bag() => Track(new GameObject("Bag")).AddComponent<CharacterBag>();
    private static T Track<T>(T instance) where T : Object { s_Objects.Add(instance); return instance; }
    private static void Cleanup() { for (int i = s_Objects.Count - 1; i >= 0; i--) if (s_Objects[i]) Object.DestroyImmediate(s_Objects[i]); s_Objects.Clear(); }
    private static async UniTask Frames(int count) { for (int i = 0; i < count; i++) await UniTask.NextFrame(PlayerLoopTiming.LastPostLateUpdate); }
    private static void Check(string name, Action action)
    {
        try { action(); s_Report.passed++; s_Report.passedTests.Add(name); }
        catch (Exception exception) { s_Report.failures.Add(name + ": " + exception); }
        finally { Cleanup(); }
    }
    private static async UniTask CheckAsync(string name, Func<UniTask> action)
    {
        try { await action(); s_Report.passed++; s_Report.passedTests.Add(name); }
        catch (Exception exception) { s_Report.failures.Add(name + ": " + exception); }
        finally { Cleanup(); }
    }
    private static FieldInfo Field(object instance, string name)
    {
        for (Type type = instance.GetType(); type != null; type = type.BaseType)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field != null) return field;
        }
        throw new MissingFieldException(instance.GetType().FullName, name);
    }
    private static void Set(object instance, string name, object value) => Field(instance, name).SetValue(instance, value);
    private static T Get<T>(object instance, string name) => (T)Field(instance, name).GetValue(instance);
    private static void Invoke(object instance, string name, params object[] args) => instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, args);
    private static void Near(Vector3 expected, Vector3 actual) => Expect(Vector3.Distance(expected, actual) < 0.001f, $"Expected {expected}, actual {actual}");
    private static void Equal<T>(T expected, T actual) => Expect(EqualityComparer<T>.Default.Equals(expected, actual), $"Expected {expected}, actual {actual}");
    private static void Expect(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new Exception("Expected " + typeof(T).Name); }
}
