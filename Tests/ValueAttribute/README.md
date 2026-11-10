# ValueAttribute 回归验证

从项目根目录运行，使用与项目一致的 Unity Editor：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Tests\ValueAttribute\Run-Tests.ps1 -UnityEditorPath 'D:\CODE\Unity\Unity Hub\Editor\6000.5.8f1\Editor\Unity.exe'
```

脚本把属性数据模型、Odin Attributes DLL 和验证入口复制到系统临时目录中的独立 Unity 项目，在真实 Unity 中编译并运行。`CharacterAttributeComponent` 依赖状态效果模块，不属于本套纯属性模型测试；完整脚本回归会连同依赖编译该组件。结果和日志保存在脚本输出的临时目录。

当前接口及约定：

- `ModifyCurrentValue` 调整当前值；`AddModifier` 添加影响上限的修饰器；`RemoveModifier` 按属性名、数量和方向移除最近一个匹配项。
- `ValueAttributeChangeInput.Amount` 必须非负，方向由 `Add` / `Subtract` 表示。运行时入口拒绝非法输入；直接向底层修饰器集合传入非法数量或方向会抛出参数异常。
- 上限为 `Clamp(基础值 + 全部修饰值之和, 0, int.MaxValue)`。内部使用 `long` 保存未裁剪总和，保证溢出边界处仍能正确移除修饰器。
- 满值时跟随新上限；非满值时保留当前值并裁剪到新上限。`RestoreToMaxValue` 恢复到当前上限，保留修饰器；`SetCurrentValueToZero` 将当前值归零。
- 状态变化后才发送 `ValueChanged`。`ValueReachedZero` 只在当前值从正数变为零时触发；`NotifyCurrentState` 可显式广播当前状态。
- `ValueAttributeModifierCalculator` 负责计算上限；`ValueAttributeModifierCollection` 保存修饰器并提供 `TotalValue`、`ClampedTotalValue`。

20 项测试覆盖加成/减益移除、重复修饰器、上限裁剪、整数边界、非法输入、事件次数与顺序，以及固定种子的一万步随机模型对照。
