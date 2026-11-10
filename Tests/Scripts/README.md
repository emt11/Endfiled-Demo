# Scripts 综合回归

从项目根目录执行：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Tests\Scripts\Run-Tests.ps1 -UnityEditorPath 'D:\CODE\Unity\Unity Hub\Editor\6000.5.8f1\Editor\Unity.exe' -ReportDirectory .\artifacts\scripts_validation
```

运行器把当前脚本、测试所需依赖及 ScriptableObject 资源复制到独立临时 Unity 项目，先编译 Player 脚本，再在 Edit Mode 和 Play Mode 中运行回归（当前共 44 项），然后退出 Unity。覆盖事件释放、背包/货币/商店、输入生命周期、池计时器、移动及相机、一次性动画、连招树 Undo/Redo、既有序列化兼容检查等。新增检查集中覆盖批量分解、按币种记账、Warping 移动锚点与窗口推进、临时模型采样及 Undo、行 ID 删除、根运动输出隔离和移动模式退出。

业务场景和原项目资源不会保存或重写。结果保存在 `results.json`，Unity 输出保存在 `unity-editor.log`；测试失败会返回非零退出码。

可传入 `-ValidationProjectPath` 复用该运行器创建的临时验证项目。只有带有正确来源标记的验证目录才允许复用；运行器会刷新其中的测试副本。

纯属性模型和状态效果有独立测试入口：`Tests/ValueAttribute/Run-Tests.ps1`、`Tests/StatusEffect/Run-Tests.ps1`。

测试工程单独配置 Water / Zipline 标签，不改原项目 TagManager。连招检查用当前结构创建数据，不包含已排除的旧连招字段迁移。

2026-11-10 本次执行：43 项通过、1 项失败。失败为既有 Poison.asset 中 PerioicStatusEffectStrategy 无对应类型迁移；修改前源码已无该迁移，原资源及该检查保留，运行器继续以非零退出码报告。详见 [交付记录](../../Docs/ScriptsOptimization.md)。
