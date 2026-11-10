# 近战状态机与输入回归验证

运行：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./Tests/MeleeSystem/Run-Tests.ps1 -UnityEditorPath 'D:\CODE\Unity\Unity Hub\Editor\6000.5.8f1\Editor\Unity.exe'
```

脚本在系统临时目录创建独立 Unity 工程，复制当前生产源码及所需依赖，使用真实的 Controller、State、连招树、事件中心和 Playable 动画对象执行检查。结果保存在输出目录中的 `results.json`，完整日志为 `editor.log`。

覆盖攻击启动与完成、连招窗口、动画所有权、取消攻击、Action 的开始与结束、Action 期间换武器，以及旧动画同步退出回调对攻击启动的干扰。

输入检查覆盖旧配置字段读取、命令覆盖与过期、无效输入、Tap、蓄力模式下的短按、松开蓄力、自动蓄力，以及换绑、解绑和释放时的订阅清理。通过 Input System 的模拟键盘事件驱动真实按钮；蓄力阈值使用 0 秒和 60 秒区分已达到和未达到的情况。

检查在 Edit Mode 中同步执行，手动驱动组件生命周期、状态 Tick、交互轮询和通知；输入夹具仅在独立工程内临时打开单例运行标记和 Input System 的 `runPlayerUpdatesInEditMode` 开关，并在释放时恢复。不修改原工程场景，不代替实际 Play Mode 下的输入时序、骨骼姿态、物理命中和动画帧顺序验证。
