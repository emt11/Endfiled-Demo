using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    public enum AttackPhase { None, Windup, Strike, Recovery }
    public enum CombatStatus { Idle, Attacking, Action }

    [DisallowMultipleComponent, Icon(ScriptGizmosPath.MeleeGizmosPath)]
    public class MeleeCombatController : MonoBehaviour, IWarpInteraction
    {
        [SerializeField, LabelText("初始武器")] private WeaponAsset m_StartingWeapon;
        [SerializeField, LabelText("命中盒挂载点")]
        [Tooltip("绑定当前角色的武器节点；未绑定的部位使用对应 Humanoid 骨骼")]
        private WeaponAttachmentBinding[] m_WeaponAttachmentBindings = Array.Empty<WeaponAttachmentBinding>();
        [SerializeField, LabelText("锁定目标")] private Transform m_FocusTarget;

        private AnimationSource m_AnimationSource;
        private DamageReceiver m_DamageReceiver;
        private readonly MeleeInputBuffer m_InputBuffer = new MeleeInputBuffer();
        private readonly HashSet<GameObject> m_HitTargets = new HashSet<GameObject>();
        private readonly Dictionary<object, HitboxWindowEvent> m_HitboxWindows =
            new Dictionary<object, HitboxWindowEvent>();
        private readonly HashSet<object> m_InputWindows = new HashSet<object>();
        private readonly Dictionary<WeaponAttachmentPoint, HitDirection> m_HitDirections =
            new Dictionary<WeaponAttachmentPoint, HitDirection>();
        private readonly List<Hitbox> m_HitboxSnapshot = new List<Hitbox>();
        private readonly List<HitboxHit> m_HitSnapshot = new List<HitboxHit>();

        private MeleeStateMachine m_StateMachine;
        private ComboActionAsset m_CurrentAction;
        private int m_CurrentNodeId = ComboTreeAsset.InvalidNodeId;
        private IAnimationState m_AttackAnimationState;
        private IAnimationState m_ReactionAnimationState;
        private float m_ReactionStartTime;
        private float m_ReactionCancelAfter;
        private int m_ReactionVersion;
        private bool m_IsStartingReaction;
        private bool m_IsStartingAttack;
        private int m_AttackVersion;
        private bool m_HasStruck;
        private IEventToken m_HitboxWindowSubscription;
        private IEventToken m_InputWindowSubscription;
        private IEventToken m_ClearHitsSubscription;

        public RuntimeWeapon CurrentWeapon { get; private set; }
        public Transform FocusTarget => m_FocusTarget;
        public CombatStatus CombatStatus => m_StateMachine?.Status ?? CombatStatus.Idle;
        public AttackPhase AttackPhase { get; private set; } = AttackPhase.None;
        public bool IsInputWindowOpen => m_InputWindows.Count > 0;
        public bool IsAttackAnimationRunning => m_AnimationSource && m_AttackAnimationState != null
            && !m_AttackAnimationState.IsDone
            && ReferenceEquals(m_AnimationSource.RunningState, m_AttackAnimationState);

        public event Action<RuntimeWeapon> WeaponChanged;

        private void Awake()
        {
            m_AnimationSource = GetComponentInChildren<AnimationSource>();
            m_DamageReceiver = GetComponent<DamageReceiver>();
            m_StateMachine = new MeleeStateMachine(this);
        }

        private void OnEnable()
        {
            // 打击窗口事件订阅
            m_HitboxWindowSubscription = EventCenter<HitboxWindowEvent>.Subscribe(HandleHitboxWindow);
            // 接招窗口事件订阅
            m_InputWindowSubscription = EventCenter<ComboInputWindowEvent>.Subscribe(HandleInputWindow);
            // 清空命中记录事件订阅
            m_ClearHitsSubscription = EventCenter<ClearHitTargetsEvent>.Subscribe(HandleClearHitTargets);
        }

        private void Start()
        {
            if (m_StartingWeapon) EquipWeapon(m_StartingWeapon);
        }

        // 动画通知在更新后送达；LateUpdate 使用本帧骨骼姿态执行查询
        private void LateUpdate()
        {
            m_StateMachine?.Tick();
        }

        private void OnDisable()
        {
            m_HitboxWindowSubscription?.Unsubscribe();
            m_InputWindowSubscription?.Unsubscribe();
            m_ClearHitsSubscription?.Unsubscribe();
            m_HitboxWindowSubscription = m_InputWindowSubscription = m_ClearHitsSubscription = null;
            CancelAttack();
            m_StateMachine?.EnterIdle();
        }

        private void OnDestroy()
        {
            m_StateMachine?.Clear();
            CurrentWeapon?.Dispose();
            CurrentWeapon = null;
            WeaponChanged = null;
        }

        public void BufferInput(MeleeInputCommand command)
        {
            if (!isActiveAndEnabled || CurrentWeapon == null || !CurrentWeapon.IsEquipped) return;
            if (m_DamageReceiver && m_DamageReceiver.IsDead) return;
            if (CombatStatus == CombatStatus.Action && !TryCancelReaction()) return;
            if (m_DamageReceiver) m_DamageReceiver.EndDefense();
            m_InputBuffer.Write(command, Time.time);
        }

        public void SetFocusTarget(IDamageable target)
        {
            var targetObject = target is UnityEngine.Object unityObject && !unityObject
                ? null : target?.DamageableObject;
            m_FocusTarget = targetObject ? targetObject.transform : null;
        }

        public void EquipWeapon(WeaponAsset asset)
        {
            if (!asset) { UnequipWeapon(); return; }
            if (!m_AnimationSource) return;
            if (CurrentWeapon?.Asset == asset && CurrentWeapon.IsEquipped) return;
            CancelAttack();
            CurrentWeapon?.Dispose();
            CurrentWeapon = new RuntimeWeapon(m_AnimationSource, asset, m_WeaponAttachmentBindings);
            CurrentWeapon.Equip();
            WeaponChanged?.Invoke(CurrentWeapon);
        }

        public void UnequipWeapon()
        {
            CancelAttack();
            CurrentWeapon?.Dispose();
            CurrentWeapon = null;
            WeaponChanged?.Invoke(null);
        }

        public void CancelAttack()
        {
            if (CombatStatus == CombatStatus.Attacking) m_StateMachine.EnterIdle();
            else EndAttack();
            // 动画退出回调也可能写入输入，取消操作结束时一并丢弃
            m_InputBuffer.Clear();
        }

        public void SetActionActive(bool active)
        {
            if (!isActiveAndEnabled || m_StateMachine == null) return;
            if (active)
            {
                if (CombatStatus == CombatStatus.Action) return;
                m_AttackVersion++;
                m_StateMachine.EnterAction();
                m_InputBuffer.Clear();
            }
            else if (CombatStatus == CombatStatus.Action)
            {
                m_InputBuffer.Clear();
                m_StateMachine.EnterIdle();
            }
        }

        public bool PlayReaction(DamageReactionResult reaction)
        {
            if (!isActiveAndEnabled || !m_AnimationSource || !reaction.IsValid || m_IsStartingReaction) return false;
            if (m_DamageReceiver && m_DamageReceiver.IsDead) return false;
            m_IsStartingReaction = true;
            try
            {
                EndReaction();
                var version = m_ReactionVersion;
                SetActionActive(true);
                if (!isActiveAndEnabled || CombatStatus != CombatStatus.Action || version != m_ReactionVersion) return false;

                var previousState = m_AnimationSource.RunningState;
                m_AnimationSource.PlayAnimation(reaction.Animation);
                var state = m_AnimationSource.RunningState;
                if (!isActiveAndEnabled || CombatStatus != CombatStatus.Action || version != m_ReactionVersion)
                {
                    if (state != null && !ReferenceEquals(state, previousState)) m_AnimationSource.ForceStopAnimation();
                    return false;
                }
                if (state == null || state.IsDone || ReferenceEquals(state, previousState))
                {
                    SetActionActive(false);
                    return false;
                }

                m_ReactionAnimationState = state;
                m_ReactionStartTime = Time.time;
                m_ReactionCancelAfter = reaction.CancelAfter;
                return true;
            }
            finally
            {
                m_IsStartingReaction = false;
            }
        }

        public bool TryCancelReaction()
        {
            if (CombatStatus != CombatStatus.Action || m_ReactionAnimationState == null
                || m_ReactionCancelAfter < 0f || Time.time - m_ReactionStartTime < m_ReactionCancelAfter) return false;
            SetActionActive(false);
            return true;
        }

        public WarpInteractionResult WarpInteraction()
        {
            var animation = m_CurrentAction ? m_CurrentAction.Animation : null;
            if (!m_FocusTarget || !animation || !animation.WarpAsset) return default;
            var targetSource = m_FocusTarget.GetComponentInChildren<AnimationSource>();
            if (targetSource) animation.PlaySyncAnimation(targetSource);
            return new WarpInteractionResult
            {
                ValidResult = true,
                WarpingAsset = animation.WarpAsset,
                WarpingPoints = new[] { new WarpingPoint(m_FocusTarget, m_FocusTarget.position) }
            };
        }

        internal void EndAttack()
        {
            var animationState = m_AttackAnimationState;
            ResetAttackContext();
            // 只停止本次攻击拥有的动画；先清上下文，防止退出通知再次取消同一次攻击
            if (m_AnimationSource && animationState != null && !animationState.IsDone
                && ReferenceEquals(m_AnimationSource.RunningState, animationState))
                m_AnimationSource.ForceStopAnimation();
        }

        internal void UpdateReaction()
        {
            if (m_ReactionAnimationState == null) return;
            if (!m_AnimationSource || m_ReactionAnimationState.IsDone
                || !ReferenceEquals(m_AnimationSource.RunningState, m_ReactionAnimationState))
                SetActionActive(false);
        }

        internal void EndReaction()
        {
            var state = m_ReactionAnimationState;
            m_ReactionVersion++;
            m_ReactionAnimationState = null;
            if (m_AnimationSource && state != null && !state.IsDone
                && ReferenceEquals(m_AnimationSource.RunningState, state))
                m_AnimationSource.ForceStopAnimation();
        }

        private void ResetAttackContext()
        {
            m_AttackVersion++;
            m_CurrentNodeId = ComboTreeAsset.InvalidNodeId;
            m_CurrentAction = null;
            m_AttackAnimationState = null;
            AttackPhase = AttackPhase.None;
            ClearAttackWindows();
        }

        /// <summary>
        /// 消费缓存输入
        /// </summary>
        internal void ProcessBufferedInput()
        {
            if (m_IsStartingAttack || !isActiveAndEnabled || CombatStatus == CombatStatus.Action) return;
            if (m_DamageReceiver && m_DamageReceiver.IsDead) return;
            if (CombatStatus == CombatStatus.Attacking && (!IsInputWindowOpen || !IsAttackAnimationRunning)) return;
            var tree = CurrentWeapon?.Asset.ComboTree;
            if (!tree || !m_AnimationSource || !CurrentWeapon.IsEquipped) return;
            if (!m_InputBuffer.TryConsume(Time.time, out var command)) return;
            if (!tree.TryGetNextAction(m_CurrentNodeId, command.Trigger, command.Signal, out var nodeId, out var action))
                return;
            if (!action || !action.Animation || !action.Animation.AnimationClip) return;

            StartAttack(nodeId, action);
        }

        private void StartAttack(int nodeId, ComboActionAsset action)
        {
            var weapon = CurrentWeapon;
            var version = m_AttackVersion;
            var previousAnimationState = m_AnimationSource.RunningState;
            m_IsStartingAttack = true;
            try
            {
                // PlayAnimation 会同步退出旧动画通知；先让它完成，再建立新攻击上下文
                m_AnimationSource.PlayAnimation(action.Animation);
                var state = m_AnimationSource.RunningState;
                if (state == null || state.IsDone || ReferenceEquals(state, previousAnimationState)) return;
                // 旧动画的同步退出回调可能换武器、停用组件或切入特殊动作
                if (!isActiveAndEnabled || CurrentWeapon != weapon || m_AttackVersion != version
                    || CombatStatus == CombatStatus.Action)
                {
                    m_AnimationSource.ForceStopAnimation();
                    return;
                }

                // 连招换招只重建攻击上下文，不退出整个 Attack 状态
                ResetAttackContext();
                m_CurrentNodeId = nodeId;
                m_CurrentAction = action;
                m_AttackAnimationState = state;
                AttackPhase = AttackPhase.Windup;
                m_StateMachine.EnterAttack();
                if (action.Animation.UseAnimationWarp) m_AnimationSource.Warp(this);
            }
            finally
            {
                m_IsStartingAttack = false;
            }
        }

        internal void ProcessHitboxes()
        {
            if (CurrentWeapon == null || !m_CurrentAction) return;
            var weapon = CurrentWeapon;
            var action = m_CurrentAction;
            var attackState = m_AttackAnimationState;
            m_HitboxSnapshot.Clear();
            m_HitboxSnapshot.AddRange(weapon.ActiveHitboxes);
            foreach (var hitbox in m_HitboxSnapshot)
            {
                if (!hitbox || !hitbox.isActiveAndEnabled || !hitbox.IsDetecting) continue;
                m_HitSnapshot.Clear();
                m_HitSnapshot.AddRange(hitbox.Detect());
                foreach (var hit in m_HitSnapshot)
                {
                    // 受伤回调可能换武器、停用角色或打断当前攻击
                    if (!isActiveAndEnabled || CurrentWeapon != weapon || m_CurrentAction != action
                        || !ReferenceEquals(m_AttackAnimationState, attackState)) return;
                    var targetObject = hit.Target?.DamageableObject;
                    if (!targetObject || targetObject == gameObject || !m_HitTargets.Add(targetObject)) continue;
                    m_HitDirections.TryGetValue(hitbox.AttachmentPoint, out var direction);
                    var context = new DamageContext(action.Damage, ValueAttributeOperation.Subtract,
                        hit.Point, hitbox.transform.rotation, direction, transform.position,
                        action.CanBeBlocked, action.StatusEffect);
                    var result = hit.Target.TakeDamage(context);
                    if (!result.WasApplied) continue;
                    if (result.Posture == DefensePosture.None)
                        action.PlayHitFeedback(hit.Point, hitbox.transform.rotation);
                    if (result.Posture == DefensePosture.PerfectParry)
                    {
                        // 伤害回调可能已经替换了当前动作，不让旧命中结果打断新的动作
                        if (!isActiveAndEnabled || CurrentWeapon != weapon || m_CurrentAction != action
                            || !ReferenceEquals(m_AttackAnimationState, attackState)) return;
                        CancelAttack();
                        if (!isActiveAndEnabled || CurrentWeapon != weapon || CombatStatus != CombatStatus.Idle) return;
                        if (weapon.Asset.ParriedReactions)
                            PlayReaction(weapon.Asset.ParriedReactions.FindReactionResult(HitDirection.Any, action.Damage));
                        return;
                    }
                }
            }
        }

        private void ClearAttackWindows()
        {
            CurrentWeapon?.DisableAllHitboxes();
            m_HitboxWindows.Clear();
            m_InputWindows.Clear();
            m_HitDirections.Clear();
            m_HitTargets.Clear();
            m_HasStruck = false;
        }

        private bool IsOwnEvent(GameObject sender)
        {
            // 通知系统传入 Animator 的根对象，角色控制器也可以位于该根对象的子层级
            return sender && sender == transform.root.gameObject;
        }

        private void HandleHitboxWindow(HitboxWindowEvent context)
        {
            if (m_IsStartingAttack || !IsOwnEvent(context.Sender) || context.Window == null
                || CombatStatus != CombatStatus.Attacking || !m_CurrentAction || CurrentWeapon == null) return;
            if (context.IsOpen)
            {
                var points = m_CurrentAction.GetAllowedHitboxPoints(context.AttachmentPoints);
                if (points == WeaponAttachmentPoint.None) return;
                m_HitboxWindows[context.Window] = new HitboxWindowEvent(true, points, context.Direction,
                    context.Sender, context.Window);
                m_HasStruck = true;
            }
            else
            {
                m_HitboxWindows.Remove(context.Window);
            }

            var enabledPoints = WeaponAttachmentPoint.None;
            m_HitDirections.Clear();
            foreach (var window in m_HitboxWindows.Values)
            {
                enabledPoints |= window.AttachmentPoints;
            }
            CurrentWeapon.SetHitboxesEnabled(WeaponAttachmentPoint.All & ~enabledPoints, false);
            CurrentWeapon.SetHitboxesEnabled(enabledPoints, true);
            foreach (var hitbox in CurrentWeapon.ActiveHitboxes)
            {
                foreach (var window in m_HitboxWindows.Values)
                    if ((window.AttachmentPoints & hitbox.AttachmentPoint) != 0)
                        m_HitDirections[hitbox.AttachmentPoint] = window.Direction;
            }
            UpdateAttackPhase();  
        }

        private void HandleInputWindow(ComboInputWindowEvent context)
        {
            if (m_IsStartingAttack || !IsOwnEvent(context.Sender) || context.Window == null
                || CombatStatus != CombatStatus.Attacking) return;
            if (context.IsOpen) m_InputWindows.Add(context.Window);
            else m_InputWindows.Remove(context.Window);
            UpdateAttackPhase();
        }

        private void HandleClearHitTargets(ClearHitTargetsEvent context)
        {
            if (!m_IsStartingAttack && IsOwnEvent(context.Sender) && CombatStatus == CombatStatus.Attacking)
                m_HitTargets.Clear();
        }

        private void UpdateAttackPhase()
        {
            if (CurrentWeapon != null && CurrentWeapon.ActiveHitboxes.Count > 0)
                AttackPhase = AttackPhase.Strike;
            else if (IsInputWindowOpen || m_HasStruck)
                AttackPhase = AttackPhase.Recovery;
            else
                AttackPhase = AttackPhase.Windup;
        }
    }
}
