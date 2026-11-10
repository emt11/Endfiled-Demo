using System;
using System.Collections.Generic;
using System.Reflection;
using GP;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

// Runs only in the isolated Edit Mode validation project.
public static class MeleeInputRegressionTests
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;

    public static void Run(List<string> passed, List<string> failures)
    {
        Check(passed, failures, "Legacy weapon input fields retain their configuration", () =>
        {
            var weapon = ScriptableObject.CreateInstance<WeaponAsset>();
            try
            {
                JsonUtility.FromJsonOverwrite("{\"m_PrimaryInput\":{\"m_Trigger\":0,\"m_InputSignal\":1,\"m_AutoTrigger\":true,\"m_HoldDuration\":0.75},\"m_SecondaryInput\":{\"m_Trigger\":1,\"m_InputSignal\":2},\"m_UtilityInput\":{\"m_Trigger\":1,\"m_InputSignal\":0}}", weapon);
                Equal(MeleeInputTrigger.Charge, weapon.PrimaryInput.Mode);
                Equal(MeleeInputSignal.LeftAttack, weapon.PrimaryInput.InputSignal);
                Equal(true, weapon.PrimaryInput.AutoTrigger);
                Equal(0.75f, weapon.PrimaryInput.HoldDuration);
                Equal(MeleeInputTrigger.Tap, weapon.SecondaryInput.Mode);
                Equal(MeleeInputSignal.RightAttack, weapon.SecondaryInput.InputSignal);
                Equal(MeleeInputSignal.None, weapon.UtilityInput.InputSignal);
            }
            finally { Object.DestroyImmediate(weapon); }
        });
        Check(passed, failures, "Input buffer keeps the latest complete command", () =>
        {
            var buffer = new MeleeInputBuffer();
            buffer.Write(new MeleeInputCommand(MeleeInputTrigger.Tap, MeleeInputSignal.LeftAttack), 1f);
            buffer.Write(new MeleeInputCommand(MeleeInputTrigger.Charge, MeleeInputSignal.RightAttack), 1.1f);
            Equal(true, buffer.TryConsume(1.2f, out var command));
            AssertCommand(command, MeleeInputTrigger.Charge, MeleeInputSignal.RightAttack);
            Equal(false, buffer.TryConsume(1.2f, out _));
        });
        Check(passed, failures, "Invalid input cannot replace a valid buffered command", () =>
        {
            var buffer = new MeleeInputBuffer();
            var valid = new MeleeInputCommand(MeleeInputTrigger.Charge, MeleeInputSignal.RightAttack);
            buffer.Write(valid, 1f);
            buffer.Write(default, 1.1f);
            buffer.Write(new MeleeInputCommand((MeleeInputTrigger)99, MeleeInputSignal.LeftAttack), 1.1f);
            buffer.Write(valid, float.NaN);
            Equal(true, buffer.TryConsume(1.2f, out var command));
            AssertCommand(command, valid.Trigger, valid.Signal);
            buffer.Write(valid, 2f);
            Equal(false, buffer.TryConsume(2.6f, out _));
        });
        CheckInput(passed, failures, "Tap mode emits once on press", f =>
        {
            f.Binding.Bind(Config(MeleeInputTrigger.Tap, MeleeInputSignal.LeftAttack));
            f.Press();
            f.AssertSingle(MeleeInputTrigger.Tap, MeleeInputSignal.LeftAttack);
            f.Release();
            Equal(1, f.Commands.Count);
        });
        CheckInput(passed, failures, "Charge mode still emits Tap for a short press", f =>
        {
            f.Binding.Bind(Config(MeleeInputTrigger.Charge, MeleeInputSignal.RightAttack, 60f));
            f.Press();
            Equal(0, f.Commands.Count);
            f.Release();
            f.AssertSingle(MeleeInputTrigger.Tap, MeleeInputSignal.RightAttack);
        });
        CheckInput(passed, failures, "Charge on release does not emit an extra Tap", f =>
        {
            f.Binding.Bind(Config(MeleeInputTrigger.Charge, MeleeInputSignal.LeftAttack, 0f));
            f.Press();
            Equal(0, f.Commands.Count);
            f.Release();
            f.AssertSingle(MeleeInputTrigger.Charge, MeleeInputSignal.LeftAttack);
        });
        CheckInput(passed, failures, "Automatic Charge is emitted only once including release", f =>
        {
            f.Binding.Bind(Config(MeleeInputTrigger.Charge, MeleeInputSignal.RightAttack, 0f, true));
            f.Press();
            f.Tick();
            f.AssertSingle(MeleeInputTrigger.Charge, MeleeInputSignal.RightAttack);
            f.Tick();
            f.Release();
            Equal(1, f.Commands.Count);
        });
        CheckInput(passed, failures, "Rebinding cancels a pending hold without duplicate subscriptions", f =>
        {
            f.Binding.Bind(Config(MeleeInputTrigger.Charge, MeleeInputSignal.LeftAttack, 60f, true));
            f.Press();
            var config = Config(MeleeInputTrigger.Tap, MeleeInputSignal.RightAttack);
            f.Binding.Bind(config);
            f.Binding.Bind(config);
            f.Tick();
            f.Release();
            Equal(0, f.Commands.Count);
            f.Press();
            f.AssertSingle(MeleeInputTrigger.Tap, MeleeInputSignal.RightAttack);
            f.Binding.Unbind();
            f.Release();
            f.Press();
            Equal(1, f.Commands.Count);
        });
        CheckInput(passed, failures, "Disabled and invalid configurations remove the previous binding", f =>
        {
            f.Binding.Bind(Config(MeleeInputTrigger.Tap, MeleeInputSignal.LeftAttack));
            f.Binding.Bind(Config(MeleeInputTrigger.Tap, MeleeInputSignal.None));
            f.Press();
            f.Release();
            f.Binding.Bind(Config((MeleeInputTrigger)99, MeleeInputSignal.RightAttack));
            f.Press();
            f.Release();
            f.Binding.Bind(null);
            f.Press();
            Equal(0, f.Commands.Count);
        });
        CheckInput(passed, failures, "Disposing a binding cancels its pending hold", f =>
        {
            f.Binding.Bind(Config(MeleeInputTrigger.Charge, MeleeInputSignal.LeftAttack, 0f, true));
            f.Press();
            f.Binding.Dispose();
            f.Binding.Dispose();
            f.Tick();
            f.Release();
            f.Press();
            Equal(0, f.Commands.Count);
        });
    }

    private static WeaponInputConfig Config(MeleeInputTrigger mode, MeleeInputSignal signal,
        float holdDuration = 0.5f, bool autoTrigger = false)
    {
        var config = new WeaponInputConfig();
        Set(config, "m_Trigger", mode);
        Set(config, "m_InputSignal", signal);
        Set(config, "m_HoldDuration", holdDuration);
        Set(config, "m_AutoTrigger", autoTrigger);
        return config;
    }

    private static void CheckInput(List<string> passed, List<string> failures, string name, Action<InputFixture> test)
    {
        Check(passed, failures, name, () =>
        {
            using (var fixture = new InputFixture()) test(fixture);
        });
    }

    private static void Check(List<string> passed, List<string> failures, string name, Action test)
    {
        try { test(); passed.Add(name); }
        catch (Exception exception) { failures.Add(name + ": " + exception); }
    }

    private sealed class InputFixture : IDisposable
    {
        private readonly FieldInfo m_RunningField = typeof(GameManager).GetField("<GameRunning>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        private readonly bool m_PreviousRunning;
        private readonly object m_InputSystemManager;
        private readonly PropertyInfo m_PlayerUpdatesProperty;
        private readonly bool m_PreviousPlayerUpdates;
        private readonly GameObject m_ManagerObject;
        private readonly GameInputManager m_Manager;
        private readonly InputAction m_Action;
        private readonly EnhancedButton m_Button;
        private readonly Keyboard m_Keyboard;
        public readonly MeleeButtonBinding Binding;
        public readonly List<MeleeInputCommand> Commands = new List<MeleeInputCommand>();

        public InputFixture()
        {
            // Enable player input processing only within this isolated Edit Mode fixture.
            m_PreviousRunning = (bool)m_RunningField.GetValue(null);
            m_InputSystemManager = typeof(InputSystem).GetField("s_Manager", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            m_PlayerUpdatesProperty = m_InputSystemManager.GetType().GetProperty("runPlayerUpdatesInEditMode");
            m_PreviousPlayerUpdates = (bool)m_PlayerUpdatesProperty.GetValue(m_InputSystemManager);
            try
            {
                m_RunningField.SetValue(null, true);
                m_PlayerUpdatesProperty.SetValue(m_InputSystemManager, true);
                m_ManagerObject = new GameObject("Melee input regression fixture");
                m_Manager = m_ManagerObject.AddComponent<GameInputManager>();
                m_Action = new InputAction("Melee input regression", InputActionType.Button, "<Keyboard>/q");
                m_Button = new EnhancedButton(m_Action);
                m_Button.Enable();
                Binding = new MeleeButtonBinding(m_Button);
                Binding.CommandTriggered += Commands.Add;
                m_Keyboard = InputSystem.AddDevice<Keyboard>();
                InputSystem.EnableDevice(m_Keyboard);
                m_Action.Enable();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Press()
        {
            InputSystem.QueueStateEvent(m_Keyboard, new KeyboardState(Key.Q));
            InputSystem.Update();
        }

        public void Release()
        {
            InputSystem.QueueStateEvent(m_Keyboard, new KeyboardState());
            InputSystem.Update();
        }

        public void Tick() => typeof(GameInputManager).GetMethod("TickInteractions", Fields).Invoke(m_Manager, null);

        public void AssertSingle(MeleeInputTrigger trigger, MeleeInputSignal signal)
        {
            Equal(1, Commands.Count);
            AssertCommand(Commands[0], trigger, signal);
        }

        public void Dispose()
        {
            try
            {
                Binding?.Dispose();
                m_Button?.Dispose();
                m_Action?.Dispose();
                if (m_Keyboard != null) InputSystem.RemoveDevice(m_Keyboard);
                if (m_ManagerObject) Object.DestroyImmediate(m_ManagerObject);
            }
            finally
            {
                m_PlayerUpdatesProperty.SetValue(m_InputSystemManager, m_PreviousPlayerUpdates);
                m_RunningField.SetValue(null, m_PreviousRunning);
            }
        }
    }

    private static void Set(object target, string name, object value) => target.GetType().GetField(name, Fields).SetValue(target, value);

    private static void AssertCommand(MeleeInputCommand command, MeleeInputTrigger trigger, MeleeInputSignal signal)
    {
        Equal(trigger, command.Trigger);
        Equal(signal, command.Signal);
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception("Expected " + expected + ", got " + actual);
    }
}
