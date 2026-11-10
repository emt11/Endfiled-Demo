# Scripts 优化交付记录

日期：2026-11-10。范围：Assets/Scripts、关联测试与文档。实现保持现有模块，未引入新框架或依赖。

## 实际修改

- ThirdPersonLocomotion 的 UnityEditor / Handles 均受 UNITY_EDITOR 保护。
- 普通动作根运动遵循 UseRootMotion；Animator 应用路径不向移动组件重复输出位移和旋转，结束、禁用和取消清理输出。Warping 在有效窗口独占移动与旋转，窗口外恢复正常移动。
- WarpingPoint 保存局部锚点，跟随目标移动和旋转；同帧推进阶段，跳过已过期窗口，瞬时窗口无除零；目标失效、取消和动画完成清理上下文。移除无用阶段重置接口。
- 分解先聚合同一物品所有 DecomposeAction 的产物，按当前容量检查；重复条目合并，不可堆叠物品逐个计入格子。不预支原物品消耗释放的空间；所有分类完成写入后再发事件，最后沿用原消费流程。
- 货币以 CurrencyCategory 为身份，名称仅显示；同币种不同面额累计同一余额，None 拒绝入账和扣款。
- 连招树行绑定节点 ID，右键删除不依赖选中行；重建、Undo/Redo 重新解析详情，空行清除标签。
- 游泳退出恢复碰撞体、重力、地面检测和阻力，清理游泳状态；滑索只响应当前触发器退出，角色各自持有方向反转状态，退出清理持有引用。
- ItemAsset 图标改为 Sprite，提供 Icon；移除闲置动画播放重载与 ResetAnimationState。物品行为依赖统一执行前校验，减少重复判空与静默返回。
- 背包调试 Inspector 移到 Editor/Inventory；Motion Warping 采样移到 Editor/Animation，在临时模型上使用原算法，并支持 Undo 与资源标脏。
- 完成约定命名、文件移动、输入类型及战斗事件拆分；统一手写代码格式、成员顺序、nameof 绑定并清理未使用 using。为保持既有字段初始化顺序，有冲突的字段块保留原顺序。
- AGENTS.md 增补排序、格式、校验职责和旧连招资源排除规则；Docs/Scripts.md 新增当前实现并标记历史内容。

## 公开接口变化

| 位置 | 当前接口或变化 |
| --- | --- |
| CharacterBag | 新增 CanAddItems / TryAddItems(IReadOnlyList<ItemTable>)；GetItems / GetCurrencies 返回只读快照。 |
| CharacterBag | GetCurrency / TrySpendCurrency / RemoveCurrency 的币种参数使用 CurrencyCategory；资产参数的 RemoveCurrency 重载保留。 |
| CharacterBag.CurrencyChanged | Action<CurrencyCategory, int>，参数分别为币种与余额。 |
| CurrencyAsset / RuntimeCurrency | 删除 CurrencyAssetId；RuntimeCurrency.Category 暴露币种。 |
| ItemAsset | Icon 为只读 Sprite；仅为本次图标字段改名保留 m_Sprite 别名。 |
| IAnimationState | 新增只读 UseRootMotion；删除无调用方的 ResetAnimationState。 |
| OneShotAnimationParent | 删除接收却未使用 InterruptRequest 的播放重载，正常单参数播放接口保留。 |
| AnimationSource.WarpingRunning | 表示有效窗口正在接管变换，不把等待窗口视为正在 Warping。 |
| PoolManager | AppendPoolUnit 改名 EnsureAvailableUnits；targetAvailableCount 表示目标空闲数量。 |
| Rope | 删除角色状态 SetInverseDirection；MoveDirection 始终表示基础方向。 |
| ComboTreeAssetEditor | RemoveNode(int nodeId) 显式指定删除目标；行 BindItem 接收 ID 与数据。 |
| 验证组件 | TestPool 改名 AnimationPlaybackTest，保留 GUID 和类型改名标记；AttributeEffectTest 使用 GP 命名空间并保留本次字段改名兼容。 |

私有方法同步改名为 StartAttack、HandleItemAdded / HandleItemRemoved、HandleLocomotionModeChanged、InitializeCamera、UpdateMovement / UpdateRotation。反射测试调用同步更新。

## 验证结果

| 验证 | 结果 |
| --- | --- |
| 当前项目 Editor 脚本编译 | 通过，编译后 Console 无错误。 |
| 当前项目 StandaloneWindows64 Player 脚本编译 | 通过，生成 Assembly-CSharp.dll 等运行时程序集；不是完整 Player 构建。 |
| Scripts 综合验证（独立工程 Edit Mode + Play Mode） | 43 通过，1 失败；本次新增的 8 项检查全部通过。 |
| 近战状态机及输入验证（独立工程 Edit Mode） | 30 通过，0 失败；连招使用当前结构数据。 |
| 文件与引用 | 11 个脚本移动保留原 GUID，无原脚本 GUID 丢失。39 个受保护资源及 meta 的 SHA256 全部不变。 |
| 明确保留项 | EquipWeaponAction 注释文件、GP_InputAction 生成文件字节不变；未实现冲刺。 |

综合验证剩余失败是原有 Poison.asset 的 PerioicStatusEffectStrategy 托管引用无法解析。修改前快照中的 PeriodicStatusEffectStrategy 也没有该旧名的 MovedFrom 标记，本次未新增或删除其迁移。遵循“只处理本次新产生的序列化兼容需求”，未修改资产、补旧迁移或削弱断言；综合运行器仍报告失败并返回非零退出码。

结果文件：

- [综合回归 JSON](../artifacts/scripts_optimization/results.json)
- [综合回归日志](../artifacts/scripts_optimization/unity-editor.log)
- [近战回归 JSON](../artifacts/scripts_optimization/melee-results.json)
- [移动 GUID 与资源审计](../artifacts/scripts_optimization/change-audit.json)

## 未验证与用户资源工作

- 未运行原项目 SampleScene 的实际游戏流程，未验证真实角色骨骼、根运动观感、碰撞与音效，也未打完整 Player 包。自动化 Play Mode 检查使用临时对象。
- 原项目尚未定义 Water / Zipline 标签；本次仅在独立验证工程配置测试标签，真实场景使用这些模式前需要对应配置。
- 旧连招树和动作资源等待用户手动替换。未处理 m_TreeDataLink → m_Nodes、m_ComboActionAsset → m_Action、m_ActionTriggerSignal → m_InputSignal；没有为它们增加迁移标记、兼容分支或迁移测试，没有重新序列化旧资产。
- 原场景、预制体及 ScriptableObject 均未保存或改写；编辑器自动刷新已恢复。

源码、文档与测试修改前快照保存在系统临时目录 ARPG-Optimization-VSzDOp，最终审计以该快照为基准。
