using System;
using System.Collections.Generic;
using GP;
using UnityEngine;
using Object = UnityEngine.Object;
using static MeleeStateMachineRegressionTests;

public static class MeleeCombatRegressionTests
{
    public static void Run(List<string> passed, List<string> failures)
    {
        Check("Perfect parry, block and guard break resolve health and stamina", f =>
        {
            var receiver = AddReceiver(f);
            Equal(true, receiver.TryStartDefense());
            var result = receiver.TakeDamage(Hit(30));
            Equal(DefensePosture.PerfectParry, result.Posture);
            Equal(100, Value(f, ValueAttributeName.Health));
            Equal(100, Value(f, ValueAttributeName.Stamina));
            Equal(false, receiver.TryStartDefense());

            Set(receiver.Defense, "m_StartTime", Time.time - 1f);
            result = receiver.TakeDamage(Hit(30));
            Equal(DefensePosture.Blocked, result.Posture);
            Equal(0, result.HealthDamage);
            Equal(15, result.StaminaCost);
            Equal(85, Value(f, ValueAttributeName.Stamina));

            Attributes(f).ModifyCurrentValue(new ValueAttributeChangeInput(
                ValueAttributeName.Stamina, 80, ValueAttributeOperation.Subtract));
            result = receiver.TakeDamage(Hit(30));
            Equal(DefensePosture.GuardBreak, result.Posture);
            Equal(30, result.HealthDamage);
            Equal(5, result.StaminaCost);
            Equal(70, Value(f, ValueAttributeName.Health));
            Equal(false, receiver.Defense.IsDefending);
            Equal(false, receiver.TryStartDefense());
        });

        Check("Unblockable and rear hits keep their damage; weapon removal clears defense", f =>
        {
            var receiver = AddReceiver(f);
            var weapon = f.NewWeapon();
            Set(weapon, "m_Defense", f.NewAsset<DefenseAsset>());
            f.Controller.EquipWeapon(weapon);
            Equal(weapon.Defense, receiver.Defense.Asset);
            Equal(true, receiver.TryStartDefense());
            var result = receiver.TakeDamage(Hit(20, canBlock: false));
            Equal(DefensePosture.None, result.Posture);
            Equal(20, result.HealthDamage);
            result = receiver.TakeDamage(Hit(20, attacker: Vector3.back * 2));
            Equal(DefensePosture.None, result.Posture);
            Equal(60, Value(f, ValueAttributeName.Health));
            Equal(100, Value(f, ValueAttributeName.Stamina));
            f.Controller.UnequipWeapon();
            Equal(false, receiver.Defense.IsDefending);
        });

        Check("Invulnerability rejects damage; hyper armor still dies and reports once", f =>
        {
            var receiver = AddReceiver(f);
            var collider = f.Root.AddComponent<BoxCollider>();
            Set(receiver, "m_DeathColliders", new Collider[] { collider });
            receiver.SetInvulnerable(true);
            var result = receiver.TakeDamage(Hit(200));
            Equal(false, result.WasApplied);
            Equal(true, result.IsInvulnerable);
            Equal(100, Value(f, ValueAttributeName.Health));
            receiver.SetInvulnerable(false);
            receiver.SetHyperArmor(true);
            var deaths = 0;
            var token = EventCenter<TargetUnavailableEvent>.Subscribe(e => { if (e.Target == receiver) deaths++; });
            try
            {
                result = receiver.TakeDamage(Hit(200));
                Equal(true, result.IsDead);
                Equal(100, result.HealthDamage);
                Equal(false, collider.enabled);
                Equal(CombatStatus.Action, f.Controller.CombatStatus);
                Equal(false, receiver.TakeDamage(Hit(1)).WasApplied);
                f.Buffer(); f.Machine.Tick();
                Equal(CombatStatus.Action, f.Controller.CombatStatus);
                Equal(1, deaths);
            }
            finally { token.Unsubscribe(); }
        });

        Check("Attribute-driven death also ends guarding and notifies targeting", f =>
        {
            var receiver = AddReceiver(f);
            Equal(true, receiver.TryStartDefense());
            var notified = false;
            var token = EventCenter<TargetUnavailableEvent>.Subscribe(e => notified |= e.Target == receiver);
            try
            {
                Attributes(f).SetCurrentValueToZero(ValueAttributeName.Health);
                Equal(true, receiver.IsDead);
                Equal(false, receiver.Defense.IsDefending);
                Equal(true, notified);
                Equal(CombatStatus.Action, f.Controller.CombatStatus);
            }
            finally { token.Unsubscribe(); }
        });

        Check("Reaction completes, respects cancel time and preserves replacement animation", f =>
        {
            Equal(true, f.Controller.PlayReaction(new DamageReactionResult(f.FirstAnimation, 60f)));
            Equal(CombatStatus.Action, f.Controller.CombatStatus);
            Equal(false, f.Controller.TryCancelReaction());
            Set(f.Source.RunningState, "m_IsDone", true);
            f.Machine.Tick();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            Equal(true, f.Controller.PlayReaction(new DamageReactionResult(f.SecondAnimation, 0f)));
            Equal(true, f.Controller.TryCancelReaction());
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            f.Controller.PlayReaction(new DamageReactionResult(f.FirstAnimation));
            f.Source.PlayAnimation(f.SecondAnimation);
            var replacement = f.Source.RunningState;
            f.Machine.Tick();
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            Equal(false, Get<bool>(replacement, "m_IsInterrupting"));
        });

        Check("Disabling from attack exit cannot start a stale reaction", f =>
        {
            f.OnFirstAnimationExit(() =>
            {
                f.Controller.enabled = false;
                Invoke(f.Controller, "OnDisable");
            });
            f.StartAttack(); f.ActivateFirstNotify();
            Equal(false, f.Controller.PlayReaction(new DamageReactionResult(f.SecondAnimation)));
            Equal(CombatStatus.Idle, f.Controller.CombatStatus);
            Equal<IAnimationState>(null, Get<IAnimationState>(f.Controller, "m_ReactionAnimationState"));
        });

        Check("Targeting deduplicates child colliders and drops disabled, distant and destroyed targets", f =>
        {
            using var first = new Fixture();
            using var second = new Fixture();
            var firstReceiver = AddReceiver(first);
            var secondReceiver = AddReceiver(second);
            first.Root.transform.position = Vector3.forward * 2;
            second.Root.transform.position = Vector3.forward * 4;
            for (var i = 0; i < 20; i++)
            {
                var child = new GameObject("Target collider");
                child.transform.SetParent(first.Root.transform, false);
                child.AddComponent<BoxCollider>();
            }
            second.Root.AddComponent<BoxCollider>();
            Physics.SyncTransforms();
            var targeting = new MeleeTargeting(f.Root.transform, 10f, ~0);
            var changes = 0;
            targeting.FocusTargetChanged += _ => changes++;
            targeting.FindTargets();
            Equal<IDamageable>(firstReceiver, targeting.FocusTarget);
            Equal(2, Get<List<IDamageable>>(targeting, "m_Targets").Count);
            targeting.FindTargets(); Equal(1, changes);
            targeting.NextTarget(); Equal<IDamageable>(secondReceiver, targeting.FocusTarget);
            secondReceiver.enabled = false;
            targeting.RefreshFocusTarget(); Equal<IDamageable>(firstReceiver, targeting.FocusTarget);
            first.Root.transform.position = Vector3.forward * 20;
            Physics.SyncTransforms(); targeting.RefreshFocusTarget(); Equal<IDamageable>(null, targeting.FocusTarget);
            first.Root.transform.position = Vector3.forward * 2;
            Physics.SyncTransforms(); targeting.FindTargets();
            Object.DestroyImmediate(firstReceiver);
            targeting.RefreshFocusTarget(); Equal<IDamageable>(null, targeting.FocusTarget);
            f.Controller.SetFocusTarget(firstReceiver); Equal<Transform>(null, f.Controller.FocusTarget);
        });

        Check("A perfect parry stops the attack and starts the attacker reaction", f =>
        {
            using var defender = new Fixture();
            var receiver = AddReceiver(defender);
            defender.Root.transform.position = Vector3.forward * 2;
            defender.Root.transform.rotation = Quaternion.Euler(0, 180, 0);
            defender.Root.AddComponent<BoxCollider>();
            Equal(true, receiver.TryStartDefense());

            var reaction = new DamageReaction();
            Set(reaction, "m_HitDirection", HitDirection.Any);
            Set(reaction, "m_Animation", f.SecondAnimation);
            var reactions = f.NewAsset<DamageReactionAsset>();
            Set(reactions, "m_Reactions", new[] { reaction });
            Set(f.Controller.CurrentWeapon.Asset, "m_ParriedReactions", reactions);
            f.StartAttack();
            var boxObject = new GameObject("Hitbox");
            boxObject.transform.SetParent(f.Root.transform);
            boxObject.transform.position = defender.Root.transform.position;
            var hitbox = boxObject.AddComponent<Hitbox>();
            Set(hitbox, "m_LayerMask", (LayerMask)(~0));
            hitbox.BeginDetection();
            Get<List<Hitbox>>(f.Controller.CurrentWeapon, "m_ActiveHitboxes").Add(hitbox);
            Physics.SyncTransforms(); f.Machine.Tick();
            Equal(100, Value(defender, ValueAttributeName.Health));
            Equal(CombatStatus.Action, f.Controller.CombatStatus);
            Equal(f.Source.RunningState, Get<IAnimationState>(f.Controller, "m_ReactionAnimationState"));
            f.AssertClean();
        });

        void Check(string name, Action<Fixture> test)
        {
            try
            {
                using var fixture = new Fixture();
                test(fixture);
                passed.Add(name);
            }
            catch (Exception exception) { failures.Add(name + ": " + exception); }
        }
    }

    private static DamageReceiver AddReceiver(Fixture fixture)
    {
        var component = fixture.Root.AddComponent<CharacterAttributeComponent>();
        var health = fixture.NewAsset<ValueAttributeAsset>();
        var stamina = fixture.NewAsset<ValueAttributeAsset>();
        Set(health, "m_AttributeName", ValueAttributeName.Health); Set(health, "m_BaseValue", 100);
        Set(stamina, "m_AttributeName", ValueAttributeName.Stamina); Set(stamina, "m_BaseValue", 100);
        Set(component.CharacterAttribute, "m_ValueAttributeAssetList", new List<ValueAttributeAsset> { health, stamina });
        component.Initialize();
        var receiver = fixture.Root.AddComponent<DamageReceiver>();
        Set(receiver, "m_DefaultDefense", fixture.NewAsset<DefenseAsset>());
        Invoke(receiver, "Awake"); Invoke(receiver, "OnEnable");
        Set(fixture.Controller, "m_DamageReceiver", receiver);
        receiver.Defense.SetAsset(Get<DefenseAsset>(receiver, "m_DefaultDefense"));
        return receiver;
    }

    private static DamageContext Hit(int amount, bool canBlock = true, Vector3? attacker = null)
    {
        return new DamageContext(amount, ValueAttributeOperation.Subtract, Vector3.zero, Quaternion.identity,
            HitDirection.Forward, attacker ?? Vector3.forward * 2, canBlock);
    }

    private static CharacterAttribute Attributes(Fixture f) => f.Root.GetComponent<CharacterAttributeComponent>().CharacterAttribute;
    private static int Value(Fixture f, ValueAttributeName name)
    {
        Attributes(f).TryGetValues(name, out var value, out _);
        return value;
    }
}
