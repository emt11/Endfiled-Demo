# PartH 功能接入

更新日期：2026-11-10

来源：`D:\Downloads\鬼鬼鬼ii2026年教程\PartH\PartH\源码`

本轮提取教程新增行为并接入现有 `Assets/Scripts`，保留当前动画、连招、输入、属性和对象池实现，未直接覆盖同名文件

## 功能与代码位置

| 教程内容 | 当前实现 |
| --- | --- |
| `GDamager`、`Health` | [DamageReceiver](../Assets/Scripts/Runtime/MeleeSystem/HitDetection/DamageReceiver.cs) 统一处理伤害、无敌、霸体、死亡及反馈，生命值仍由 `CharacterAttribute` 持有 |
| `Defense`、`DefenseAsset` | [RuntimeDefense](../Assets/Scripts/Runtime/MeleeSystem/Defense/RuntimeDefense.cs) 保存角色本次防御状态，[DefenseAsset](../Assets/Scripts/Runtime/MeleeSystem/Defense/DefenseAsset.cs) 保存共享配置 |
| 受击及被弹反后的动作 | [DamageReaction](../Assets/Scripts/Runtime/MeleeSystem/DamageReaction/DamageReaction.cs) 配置动画和取消时间，[MeleeCombatController](../Assets/Scripts/Runtime/MeleeSystem/Combat/MeleeCombatController.cs) 在 Action 状态中跟踪实际动画实例 |
| `Targeting` | [MeleeTargeting](../Assets/Scripts/Runtime/MeleeSystem/Targeting/MeleeTargeting.cs) 搜索、去重、锁定最近目标及循环切换，失效目标自动重新搜索 |
| 防御、锁定输入 | [MeleeInputHandler](../Assets/Scripts/Runtime/MeleeSystem/Input/MeleeInputHandler.cs) 绑定现有输入资产中的 Space、MMB、MSB，禁用时解绑并结束防御、清除锁定 |
| 锁定相机 | [ThirdPersonCamera](../Assets/Scripts/Runtime/Character/Camera/ThirdPersonCamera.cs) 跟随移动目标，继续使用现有相机碰撞处理；解锁保留当前朝向 |
| `FXAsset`、`RuntimePoolFX` | [FXAsset](../Assets/Scripts/Runtime/FX/FXAsset.cs)、[RuntimePoolFXUnit](../Assets/Scripts/Runtime/GamePool/PoolUnit/RuntimePoolFXUnit.cs) 复用现有对象池，支持定时或粒子完成后回收 |

未引入空的 `GCombatDirector`、单一实现的 `IDefense` / `IPoolFX`、旧版状态机和重复的输入绑定代码，教程中的处决、反制分支原本只有占位注释，本轮也未扩展为新玩法

## PartH 补充相机代码

补充来源：`D:\Downloads\鬼鬼鬼ii2026年教程\PartH补充代码\GMainCamera.cs`

该目录仅包含相机脚本，功能合并到已有 `ThirdPersonCamera`，继续使用原脚本 GUID、场景挂载和 `MeleeInputHandler` 的锁定通知，不需要再挂载 `GMainCamera`

| 教程接口 | 当前接口 |
| --- | --- |
| `ResetCameraTranckTarget(target)` | `SetTrackingTarget(target)` |
| `FraezeCamera(frozen)` | `SetFrozen(frozen)`，暂停自由观察和锁定旋转，位置跟随与碰撞仍更新 |
| `SetCameraFocusTarget(target)` / `CameraLockMode()` | `SetFocusTarget(target)`，目标引用是锁定状态的唯一来源 |
| `UnLockMode()` | `SetFocusTarget(null)`，从当前朝向恢复自由观察，保留调用方的冻结设置 |
| `Look_Key` / `QuaternionUtils.ClampAngle` | 使用已有 `LookInput` / `AngleUtils` |

教程的 `SetCameraFocusTarget` 先赋锁定模式再判断是否未锁定，分支无法执行；`UnLockMode` 没有立即清除锁定模式。现有目标引用驱动方式已经避免这两个问题，因此不引入教程的第二套模式标志和拼写错误接口

本次补充修正：

- 锁定和自由观察统一遵守 `m_CameraMinMaxPitch`，防止锁定高低目标后解锁时突然回到角度边界
- 碰撞从 `跟踪目标位置 + 跟踪偏移` 检测到预定相机位置，确保检测路径与实际跟随高度一致
- 不再把检测起点向后推进一个半径，避免检测球进入紧邻的薄墙后漏检；沿用单次最近命中查询，无障碍或关闭碰撞时恢复完整跟踪距离

场景继续采用父节点挂 `ThirdPersonCamera`、子节点挂实际 `Camera` 的结构，`MeleeInputHandler` 通过 `Camera.main` 查找时需给实际 Camera 所在对象设置 `MainCamera` 标签。碰撞层应选择环境障碍并排除角色自身；半径应大于零，零半径沿用射线检测

相机专项验证使用 [Scripts 回归入口](../Tests/Scripts/README.md) 的 `-TestFilter 相机`：独立工程 Play Mode 修复前 3 / 6 通过，新增的三个问题均复现；修复后 6 / 6 通过，覆盖移动锁定、冻结、目标销毁、解锁衔接、空目标、最近障碍、偏移及近处薄墙。报告分别为 `artifacts/parth_camera_baseline/results.json` 和 `artifacts/parth_camera_validation/results.json`

原项目 Unity 6000.5.8f1 Editor 编译完成，Console 无错误；本次未运行独立 Player 脚本编译、全量回归或原 SampleScene 交互验收，没有修改场景和配置资产

## 结算规则

- 普通受击：扣除 Health，再执行受击反应；无敌阻止扣血和附加效果，霸体保留扣血、附加效果与死亡处理，跳过防御和普通受击反应
- 受击反应由受击角色当前武器的 `DamageReactions` 决定，未配置时使用 `DamageReceiver` 的默认反应；原来由攻击者武器选择对方反应的调用已经移到受击端
- 防御只有在存在配置、有体力、冷却结束且攻击处于防御角度内时生效；重复按下不会刷新完美格挡窗口，重复松开不会延长冷却
- 完美格挡：不扣生命或体力，打断攻击者当前攻击，并播放攻击者武器的 `ParriedReactions`
- 普通格挡：不扣生命，按攻击伤害乘配置系数消耗体力；体力耗尽时破防，结束防御并承受本次伤害
- 不可防御攻击和防御角度外攻击保留原始伤害；格挡阻止本次命中附带的状态效果
- 默认防御角度为正面 180 度、完美格挡窗口 0.2 秒、冷却 0.5 秒、体力消耗系数 0.5，可在资产中调整
- 受击动作根据实际动画完成或替换情况退出，不用剪辑原始时长猜测结束；可取消时间按游戏时间秒数计算，未配置可取消时不能通过攻击输入提前结束
- Health 归零统一触发死亡处理，包含属性或状态效果直接造成的死亡；结束防御和攻击、清理状态效果、停止角色移动、播放死亡反馈并通知锁定方刷新目标，重复伤害不再触发死亡
- 本轮没有增加复活或角色重生流程

## Inspector 接入步骤

1. 受击角色根对象添加 `CharacterAttributeComponent` 和 `DamageReceiver`，配置 Health 属性；需要防御时再配置 Stamina 属性
2. 创建 `GP/Melee System/Defense` 资产，设置窗口、角度、消耗和三种防御反应；把它赋给武器的“防御配置”，无武器角色可以使用 `DamageReceiver` 的默认防御配置
3. 在武器上配置“受击反应”和“攻击被完美格挡后的反应”，反应方向 `Any` 可以匹配所有方向；反应数组仍按顺序取首个匹配项
4. 在 `DamageReceiver` 上按需配置死亡动画、死亡音效和死亡时禁用的碰撞体；不要把仍需要维持尸体物理的碰撞体放进禁用列表
5. 在 `ComboActionAsset` 上设置“允许被防御”、命中状态效果、命中音效和命中特效；已有动作默认允许防御，反馈为空时不播放
6. 玩家使用 `MeleeInputHandler`，配置锁定搜索半径和可锁定层；相机可显式指定，也可由 `Camera.main` 的父级查找 `ThirdPersonCamera`
7. 目标需要可被搜索层命中的 Collider 和父级 `IDamageable` 实现，多个子碰撞体只计作一个目标；当前未提供阵营筛选，可通过层区分可锁定对象
8. 特效预制体根节点添加 `ParticleSystem` 与 `RuntimePoolFXUnit`，创建 `PoolAsset` 并指定该组件和容量，再创建 `GP/FX/FX Asset` 引用对象池；音效沿用现有 `AudioAsset` 配置

| 默认输入 | 行为 |
| --- | --- |
| 按住空格 | 开始防御，松开结束防御 |
| 鼠标中键 | 锁定最近有效目标，再按一次解除锁定 |
| 鼠标滚轮 | 锁定时循环切换目标 |
| 原有攻击键 | 沿用武器输入配置，开始攻击时结束防御；受击动作达到可取消时间后可以衔接攻击 |

防御配置负责格挡判定及命中反应，本轮没有新增举盾待机动画或防御专用移动方式

粒子自回收使用 `ParticleSystem.IsAlive(true)`，会等待子粒子和残留粒子结束；循环粒子应使用定时回收或由调用者主动停止，复用前会清空旧粒子，位置与旋转分别使用 `Vector3` 和 `Quaternion`，相关 API 语义见 [Unity IsAlive 文档](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ParticleSystem.IsAlive.html)

## 接口变化

- `IDamageable.TakeDamage(in DamageContext)` 返回 `DamageResult`，新增 `IsDead`、`IsInvulnerable` 查询，取消由调用方传入反应动画的参数
- `DamageContext` 增加打击方向、攻击者位置及是否可防御；保留伤害数值、加减操作、命中位置／旋转、状态效果
- `DamageResult` 返回是否接受本次结算、结算后的死亡／无敌状态、防御姿态、实际生命伤害和体力消耗
- `DamageReceiver` 提供 `SetInvulnerable`、`SetHyperArmor`、`TryStartDefense`、`EndDefense`；生命值读写继续使用 `CharacterAttribute`
- `MeleeCombatController` 提供 `FocusTarget`、`SetFocusTarget`、`PlayReaction` 和 `TryCancelReaction`，原 `SetActionActive` 的手动控制用法保留
- 新增 `TargetUnavailableEvent`，在角色死亡或受击组件禁用时通知目标选择方
- `GameInputManager` 新增 `SpaceKey`、`MMB`、`ScrollInput`，未编辑 `.inputactions` 或生成包装类
- `ValueAttributeName.Stamina` 追加在枚举末尾，值为 4；`HitDirection.Any` 追加为 11，已有枚举值不变

## 资源边界与验证

本轮修改脚本和受影响文档，没有为原场景自动挂载组件或创建／替换战斗配置资产，新增配置需要按上面的步骤在 Inspector 中接入

保留已有脚本的 `.meta` / GUID，新增脚本由 Unity 生成元数据；旧连招字段迁移仍不在本轮范围内，不重新序列化旧连招资源

验证入口为 [近战回归](../Tests/MeleeSystem/README.md) 和 [Scripts 综合回归](../Tests/Scripts/README.md)

| 验证 | 本轮结果 |
| --- | --- |
| 原项目 Editor 脚本编译 | 通过，Console 无脚本编译错误 |
| 原项目 StandaloneWindows64 Player 脚本编译 | 通过，产出包含 Assembly-CSharp 的 41 个程序集；没有制作完整 Player 构建 |
| 独立工程近战 Edit Mode 回归 | 38 / 38 通过，包含新增 8 项 |
| 独立工程 Scripts Edit Mode / Play Mode 综合回归 | 46 / 47 通过，新增的空格输入生命周期、粒子回收和锁定相机共 3 项全部通过 |
| 既有资源问题 | Poison.asset 仍引用 PerioicStatusEffectStrategy，旧类型加载检查失败；保持原断言和非零退出码，没有在本轮修改该资源 |
| 原场景玩法与资源配置 | 未执行原 SampleScene 的交互验收，需配置防御、反应、死亡和特效资产后验证实际表现 |

结果文件：`artifacts/parth_validation/melee-results.json`、`artifacts/parth_validation/results.json`、`artifacts/parth_validation/unity-editor.log`

本轮新增代码遵循中文注释不加句末句号的约定；差异检查未发现新增空白错误，既有脚本 GUID 和生成的输入包装类保持不变，工作期间原场景与命中盒资源的独立编辑已保留
