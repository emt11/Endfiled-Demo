using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GP;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

// Copied into a temporary Unity project's Editor folder by Run-Tests.ps1.
public static class MeleeStateMachineRegressionTests
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;

    [Serializable]
    private sealed class Report
    {
        public List<string> passed = new List<string>();
        public List<string> failures = new List<string>();
    }

    [Serializable]
    private sealed class ExitCallbackNotify : AnimationStateNotify
    {
        public Action callback;
        public override void OnEnterNotify(GameObject user) { }
        public override void OnTickNotify(GameObject user) { }
        public override void OnExitNotify(GameObject user) => callback();
    }

    public static void Run()
    {
        var report = new Report();
        Check(report, "Idle cannot enter Attack without a running attack", f =>
        {
            f.Machine.EnterAttack();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            f.AssertClean();
        });
        Check(report, "Buffered input starts the root action", f =>
        {
            f.StartAttack();
            Equal(CombatStatus.Attacking, f.Controller.CombatStatus);
            Equal(AttackPhase.Windup, f.Controller.AttackPhase);
            Equal(f.FirstNode, Get<int>(f.Controller, "m_CurrentNodeId"));
            Equal(f.FirstAction, Get<ComboActionAsset>(f.Controller, "m_CurrentAction"));
            Equal(true, f.Controller.IsAttackAnimationRunning);
        });
        Check(report, "Closed input window retains the command until it opens", f =>
        {
            f.StartAttack();
            f.Buffer();
            f.Machine.Tick();
            Equal(f.FirstNode, Get<int>(f.Controller, "m_CurrentNodeId"));
            f.OpenInputWindow();
            f.Machine.Tick();
            Equal(f.SecondNode, Get<int>(f.Controller, "m_CurrentNodeId"));
        });
        Check(report, "Combo resets windows and targets without stopping the new animation", f =>
        {
            f.StartAttack();
            var previousAnimation = f.Source.RunningState;
            var attackMode = f.Machine.CurrentState;
            Get<HashSet<GameObject>>(f.Controller, "m_HitTargets").Add(f.Root);
            f.OpenInputWindow();
            f.Buffer();
            f.Machine.Tick();
            Equal(f.SecondNode, Get<int>(f.Controller, "m_CurrentNodeId"));
            Equal(f.SecondAction, Get<ComboActionAsset>(f.Controller, "m_CurrentAction"));
            Equal(attackMode, f.Machine.CurrentState);
            Equal(false, ReferenceEquals(previousAnimation, f.Source.RunningState));
            Equal(false, Get<bool>(f.Source.RunningState, "m_IsInterrupting"));
            Equal(false, f.Controller.IsInputWindowOpen);
            Equal(0, Get<HashSet<GameObject>>(f.Controller, "m_HitTargets").Count);
            Equal(AttackPhase.Windup, f.Controller.AttackPhase);
        });
        Check(report, "Natural completion returns to Idle and clears attack context", f =>
        {
            f.StartAttack();
            f.OpenInputWindow();
            Set(f.Source.RunningState, "m_IsDone", true);
            f.Machine.Tick();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            f.AssertClean();
        });
        Check(report, "Cancel stops the owned animation and clears buffered input", f =>
        {
            f.StartAttack();
            var animation = f.Source.RunningState;
            f.Buffer();
            f.Controller.CancelAttack();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            Equal(true, Get<bool>(animation, "m_IsInterrupting"));
            f.AssertClean();
            f.Machine.Tick();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
        });
        Check(report, "Animation replacement preserves the external animation", f =>
        {
            f.StartAttack();
            f.Source.PlayAnimation(f.SecondAnimation);
            var externalAnimation = f.Source.RunningState;
            f.Machine.Tick();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            Equal(false, Get<bool>(externalAnimation, "m_IsInterrupting"));
            f.AssertClean();
        });
        Check(report, "Cancel also discards input written by animation exit callbacks", f =>
        {
            f.OnFirstAnimationExit(() => f.Buffer());
            f.StartAttack();
            f.ActivateFirstNotify();
            f.Controller.CancelAttack();
            f.Machine.Tick();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            f.AssertClean();
        });
        Check(report, "Entering Action ends the attack and suspends input processing", f =>
        {
            f.StartAttack();
            var animation = f.Source.RunningState;
            f.Controller.SetActionActive(true);
            f.Buffer();
            f.Machine.Tick();
            Equal(CombatStatus.Action, f.Controller.CombatStatus);
            Equal(true, Get<bool>(animation, "m_IsInterrupting"));
            f.AssertClean();
            f.Controller.SetActionActive(false);
            f.Machine.Tick();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
        });
        Check(report, "Cancel, equip and unequip preserve Action", f =>
        {
            f.Controller.SetActionActive(true);
            f.Controller.CancelAttack();
            Equal(CombatStatus.Action, f.Controller.CombatStatus);
            f.Controller.EquipWeapon(f.NewWeapon());
            Equal(CombatStatus.Action, f.Controller.CombatStatus);
            f.Controller.UnequipWeapon();
            Equal(CombatStatus.Action, f.Controller.CombatStatus);
            f.Controller.SetActionActive(false);
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
        });
        Check(report, "Ending an inactive Action does not cancel an attack", f =>
        {
            f.StartAttack();
            var animation = f.Source.RunningState;
            f.Controller.SetActionActive(false);
            Equal(CombatStatus.Attacking, f.Controller.CombatStatus);
            Equal(animation, f.Source.RunningState);
            Equal(false, Get<bool>(animation, "m_IsInterrupting"));
        });
        Check(report, "Disabling the controller releases Action", f =>
        {
            f.Controller.SetActionActive(true);
            Invoke(f.Controller, "OnDisable");
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            f.AssertClean();
        });
        Check(report, "Failed animation playback does not advance the combo", f =>
        {
            Set(f.Source, "m_OneShotAnimationParent", ScriptPlayable<OneShotAnimationParent>.Null);
            f.StartAttack();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            f.AssertClean();
        });
        Check(report, "Cancellation in an old animation callback cannot resurrect the attack", f =>
        {
            f.OnFirstAnimationExit(() => f.Controller.CancelAttack());
            f.StartAttack();
            f.ActivateFirstNotify();
            f.OpenInputWindow();
            f.Buffer();
            f.Machine.Tick();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            Equal(true, Get<bool>(f.Source.RunningState, "m_IsInterrupting"));
            f.AssertClean();
        });
        Check(report, "Weapon change in an old animation callback aborts the pending attack", f =>
        {
            var replacement = f.NewWeapon();
            f.OnFirstAnimationExit(() => f.Controller.EquipWeapon(replacement));
            f.StartAttack();
            f.ActivateFirstNotify();
            f.OpenInputWindow();
            f.Buffer();
            f.Machine.Tick();
            Equal(replacement, f.Controller.CurrentWeapon.Asset);
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            f.AssertClean();
        });
        Check(report, "Action in an old animation callback aborts the pending attack", f =>
        {
            f.OnFirstAnimationExit(() => f.Controller.SetActionActive(true));
            f.StartAttack();
            f.ActivateFirstNotify();
            f.OpenInputWindow();
            f.Buffer();
            f.Machine.Tick();
            Equal(CombatStatus.Action, f.Controller.CombatStatus);
            f.AssertClean();
        });
        Check(report, "Repeated Action activation is idempotent", f =>
        {
            f.Controller.SetActionActive(true);
            var actionMode = f.Machine.CurrentState;
            f.Controller.SetActionActive(true);
            Equal(actionMode, f.Machine.CurrentState);
            Equal(CombatStatus.Action, f.Controller.CombatStatus);
            f.Controller.SetActionActive(false);
            f.Controller.SetActionActive(false);
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
        });
        Check(report, "Disabling in an old animation callback aborts the pending attack", f =>
        {
            f.OnFirstAnimationExit(() =>
            {
                f.Controller.enabled = false;
                Invoke(f.Controller, "OnDisable");
            });
            f.StartAttack();
            f.ActivateFirstNotify();
            f.OpenInputWindow();
            f.Buffer();
            f.Machine.Tick();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            Equal(true, Get<bool>(f.Source.RunningState, "m_IsInterrupting"));
            f.AssertClean();
        });
        Check(report, "An Action that starts and ends during playback still invalidates the attack", f =>
        {
            f.OnFirstAnimationExit(() =>
            {
                f.Controller.SetActionActive(true);
                f.Controller.SetActionActive(false);
            });
            f.StartAttack();
            f.ActivateFirstNotify();
            f.OpenInputWindow();
            f.Buffer();
            f.Machine.Tick();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            Equal(true, Get<bool>(f.Source.RunningState, "m_IsInterrupting"));
            f.AssertClean();
        });
        Check(report, "Clearing the state machine releases an owned attack", f =>
        {
            f.StartAttack();
            var animation = f.Source.RunningState;
            f.Machine.Clear();
            Equal<IMeleeState>(null, f.Machine.CurrentState);
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            Equal(true, Get<bool>(animation, "m_IsInterrupting"));
            f.AssertClean();
        });

        MeleeInputRegressionTests.Run(report.passed, report.failures);
        MeleeCombatRegressionTests.Run(report.passed, report.failures);
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "results.json"), JsonUtility.ToJson(report, true));
        Debug.Log("Melee regression tests: " + report.passed.Count + " passed, " + report.failures.Count + " failed.");
        EditorApplication.Exit(report.failures.Count == 0 ? 0 : 1);
    }

    private static void Check(Report report, string name, Action<Fixture> test)
    {
        try
        {
            using (var fixture = new Fixture()) test(fixture);
            report.passed.Add(name);
        }
        catch (Exception exception)
        {
            report.failures.Add(name + ": " + exception);
        }
    }

    internal sealed class Fixture : IDisposable
    {
        public readonly GameObject Root;
        public readonly AnimationSource Source;
        public readonly MeleeCombatController Controller;
        public readonly AnimationAsset FirstAnimation;
        public readonly AnimationAsset SecondAnimation;
        public readonly ComboActionAsset FirstAction;
        public readonly ComboActionAsset SecondAction;
        public readonly ComboTreeAsset Tree;
        public readonly int FirstNode;
        public readonly int SecondNode;
        private readonly List<Object> m_Assets = new List<Object>();
        public MeleeStateMachine Machine => Get<MeleeStateMachine>(Controller, "m_StateMachine");

        public Fixture()
        {
            Root = new GameObject("Melee regression fixture");
            Root.AddComponent<Animator>();
            Source = Root.AddComponent<AnimationSource>();
            if (!Source.SourceAnimator) Invoke(Source, "Awake");
            Controller = Root.AddComponent<MeleeCombatController>();
            if (Machine == null) Invoke(Controller, "Awake");
            if (Get<IEventToken>(Controller, "m_InputWindowSubscription") == null) Invoke(Controller, "OnEnable");
            FirstAnimation = NewAnimation();
            SecondAnimation = NewAnimation();
            FirstAction = NewAction(FirstAnimation);
            SecondAction = NewAction(SecondAnimation);
            Tree = NewAsset<ComboTreeAsset>();
            FirstNode = Tree.AddRootNode(new ComboActionData { Action = FirstAction });
            SecondNode = Tree.AddChildNode(new ComboActionData { Action = SecondAction }, FirstNode);
            Controller.EquipWeapon(NewWeapon());
        }

        public WeaponAsset NewWeapon()
        {
            var asset = NewAsset<WeaponAsset>();
            Set(asset, "m_ComboTree", Tree);
            return asset;
        }

        public void Buffer() => Controller.BufferInput(new MeleeInputCommand(MeleeInputTrigger.Tap, MeleeInputSignal.LeftAttack));
        public void StartAttack() { Buffer(); Machine.Tick(); }
        public void OpenInputWindow() => EventCenter<ComboInputWindowEvent>.Publish(new ComboInputWindowEvent(true, Root, new object()));

        public void OnFirstAnimationExit(Action callback)
        {
            var notify = new ExitCallbackNotify { callback = callback };
            Set(notify, "m_StartFrame", 0);
            Set(notify, "m_EndFrame", 10);
            Set(FirstAnimation.StateNotifyList, "m_StateNotifies", new List<AnimationStateNotify> { notify });
        }

        public void ActivateFirstNotify()
        {
            var parent = Get<ScriptPlayable<OneShotAnimationParent>>(Source, "m_OneShotAnimationParent").GetBehaviour();
            var notify = Get<ScriptPlayable<AnimationNotifyPlayable>>(parent, "m_NotifyPlayable").GetBehaviour();
            notify.OnNotify(Playable.Null, new AnimationNotifyFrameData("test", 0, -1, 0, 0), Source.RunningState);
        }

        public void AssertClean()
        {
            Equal(ComboTreeAsset.InvalidNodeId, Get<int>(Controller, "m_CurrentNodeId"));
            Equal<ComboActionAsset>(null, Get<ComboActionAsset>(Controller, "m_CurrentAction"));
            Equal<IAnimationState>(null, Get<IAnimationState>(Controller, "m_AttackAnimationState"));
            Equal(AttackPhase.None, Controller.AttackPhase);
            Equal(false, Controller.IsInputWindowOpen);
            Equal(0, Get<Dictionary<object, HitboxWindowEvent>>(Controller, "m_HitboxWindows").Count);
            Equal(0, Get<HashSet<GameObject>>(Controller, "m_HitTargets").Count);
        }

        private AnimationAsset NewAnimation()
        {
            var clip = new AnimationClip { frameRate = 30 };
            clip.SetCurve("", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0, 0, 1, 0));
            m_Assets.Add(clip);
            var animation = NewAsset<AnimationAsset>();
            Set(animation, "m_AnimationClip", clip);
            return animation;
        }

        private ComboActionAsset NewAction(AnimationAsset animation)
        {
            var action = NewAsset<ComboActionAsset>();
            Set(action, "m_AnimationAsset", animation);
            Set(action, "m_InputSignal", MeleeInputSignal.LeftAttack);
            return action;
        }

        internal T NewAsset<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            m_Assets.Add(asset);
            return asset;
        }

        public void Dispose()
        {
            var receiver = Root.GetComponent<DamageReceiver>();
            if (receiver && receiver.Defense != null) Invoke(receiver, "OnDisable");
            Invoke(Controller, "OnDisable");
            Invoke(Controller, "OnDestroy");
            Invoke(Source, "OnDestroy");
            Object.DestroyImmediate(Root);
            foreach (var asset in m_Assets) Object.DestroyImmediate(asset);
        }
    }

    private static FieldInfo Field(object target, string name)
    {
        for (var type = target.GetType(); type != null; type = type.BaseType)
        {
            var field = type.GetField(name, Fields);
            if (field != null) return field;
        }
        throw new MissingFieldException(target.GetType().Name, name);
    }

    internal static T Get<T>(object target, string name) => (T)Field(target, name).GetValue(target);
    internal static void Set(object target, string name, object value) => Field(target, name).SetValue(target, value);
    internal static void Invoke(object target, string name) => target.GetType().GetMethod(name, Fields).Invoke(target, null);
    internal static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception("Expected " + expected + ", got " + actual);
    }
}
