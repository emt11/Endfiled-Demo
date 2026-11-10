# StatusEffect 回归验证

在项目根目录运行：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Tests\StatusEffect\Run-Tests.ps1 -UnityEditorPath 'D:\CODE\Unity\Unity Hub\Editor\6000.5.8f1\Editor\Unity.exe'
```

脚本复制属性模块、状态效果模块、UniTask 和 Odin Attributes 到系统临时目录中的独立 Unity 项目，进入 Play Mode 验证，保留日志和 JSON 结果。不修改当前项目的业务场景。

- 使用缩放后的游戏时间，开始时立即触发一次。之后只触发时间严格早于到期时刻的周期；长帧补发经过的周期。
- 不勾选持续性时执行一次并结束。间隔为零时仅首次执行，然后等待到期。
- 首次添加的层数限制到配置上限；后续叠层不刷新时间，也不额外触发。下一次周期读取最新层数。
- 策略可被多个角色共享，应只存配置；每个角色的数值、层数、计时和取消保存在运行时效果中。
- 同一配置资产的重复添加合并层数，不同资产各自独立。名称不作为运行时字典键。
- 对象池回收时先调用管理器 `Clear()`，再释放角色属性；角色销毁时调用 `Dispose()`。

覆盖即时/周期效果、叠层、计时边界、长帧、暂停、驱散、自然结束后重加、事件重入、异常清理和多个角色共享配置。
