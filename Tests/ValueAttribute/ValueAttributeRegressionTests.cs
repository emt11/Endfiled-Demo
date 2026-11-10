using System;
using System.Collections.Generic;
using System.IO;
using GP;
using UnityEditor;
using UnityEngine;

public static class ValueAttributeRegressionTests
{
    [Serializable]
    private class Report
    {
        public int passed;
        public List<string> failures = new List<string>();
    }

    private static readonly Report s_Report = new Report();

    public static void Run()
    {
        Check("空配置被拒绝", () => Throws<ArgumentNullException>(() => new RuntimeValueAttribute(null)));
        Check("负基础值归零", () => State(Create(-10), 0, 0));
        Check("初始化广播当前状态", () =>
        {
            var value = Create();
            value.ModifyCurrentValue(Input(30, ValueAttributeOperation.Subtract));
            int observed = -1;
            value.ValueChanged += (_, current, max) => observed = current;
            value.NotifyCurrentState();
            Equal(70, observed);
        });
        Check("当前值运算限制范围且不溢出", () =>
        {
            var value = Create(int.MaxValue);
            Equal(false, value.ModifyCurrentValue(Input(int.MaxValue)));
            State(value, int.MaxValue, int.MaxValue);
            Equal(true, value.ModifyCurrentValue(Input(int.MaxValue, ValueAttributeOperation.Subtract)));
            State(value, 0, int.MaxValue);
            Equal(false, value.ModifyCurrentValue(Input(1, ValueAttributeOperation.Subtract)));
        });
        Check("加成移除恢复原上限", () =>
        {
            var value = Create();
            var modifier = Input(25);
            Equal(true, value.AddModifier(modifier));
            State(value, 125, 125);
            Equal(true, value.RemoveModifier(modifier));
            State(value, 100, 100);
            Equal(false, value.RemoveModifier(modifier));
        });
        Check("同数值的加成减益按方向移除", () =>
        {
            var value = Create();
            value.AddModifier(Input(25));
            value.AddModifier(Input(25, ValueAttributeOperation.Subtract));
            value.RemoveModifier(Input(25));
            State(value, 75, 75);
            Equal(false, value.RemoveModifier(Input(25)));
            value.RemoveModifier(Input(25, ValueAttributeOperation.Subtract));
            State(value, 100, 100);
        });
        Check("同类型修饰器逐个移除", () =>
        {
            var value = Create();
            value.AddModifier(Input(20));
            value.AddModifier(Input(20));
            value.RemoveModifier(Input(20));
            State(value, 120, 120);
            value.RemoveModifier(Input(20));
            State(value, 100, 100);
        });
        Check("非满值增加上限不回血，降低上限裁剪", () =>
        {
            var value = Create();
            value.ModifyCurrentValue(Input(20, ValueAttributeOperation.Subtract));
            value.AddModifier(Input(50));
            State(value, 80, 150);
            value.AddModifier(Input(100, ValueAttributeOperation.Subtract));
            State(value, 50, 50);
        });
        Check("负上限裁剪后仍能准确移除修饰器", () =>
        {
            var value = Create();
            var modifier = Input(200, ValueAttributeOperation.Subtract);
            value.AddModifier(modifier);
            State(value, 0, 0);
            value.RestoreToMaxValue();
            State(value, 0, 0);
            value.RemoveModifier(modifier);
            State(value, 100, 100);
        });
        Check("超出int范围的修饰总和可逆", () =>
        {
            var value = Create(int.MaxValue);
            value.AddModifier(Input(int.MaxValue));
            value.AddModifier(Input(int.MaxValue));
            State(value, int.MaxValue, int.MaxValue);
            value.AddModifier(Input(int.MaxValue, ValueAttributeOperation.Subtract));
            value.RemoveModifier(Input(int.MaxValue));
            State(value, int.MaxValue, int.MaxValue);
            value.RemoveModifier(Input(int.MaxValue));
            State(value, 0, 0);
            value.RemoveModifier(Input(int.MaxValue, ValueAttributeOperation.Subtract));
            State(value, int.MaxValue, int.MaxValue);
        });
        Check("非法输入不修改状态或通知", () =>
        {
            var value = Create();
            int events = 0;
            value.ValueChanged += (_, current, max) => events++;
            var invalidInputs = new[]
            {
                new ValueAttributeChangeInput(ValueAttributeName.Mana, 10, ValueAttributeOperation.Add),
                Input(-1), Input(int.MinValue), Input(10, (ValueAttributeOperation)99)
            };
            foreach (var input in invalidInputs)
            {
                Equal(false, value.ModifyCurrentValue(input));
                Equal(false, value.AddModifier(input));
                Equal(false, value.RemoveModifier(input));
            }
            State(value, 100, 100);
            Equal(0, events);
            Equal(false, Input(-1).IsValid);
            Equal(false, Input(1, (ValueAttributeOperation)99).IsValid);
            Equal(true, Input(0).IsValid);
        });
        Check("重复归零和无变化操作不重复通知", () =>
        {
            var value = Create();
            int changes = 0;
            int zeros = 0;
            value.ValueChanged += (_, current, max) => changes++;
            value.ValueReachedZero += _ => zeros++;
            value.RestoreToMaxValue();
            Equal(false, value.AddModifier(Input(0)));
            Equal(false, value.RemoveModifier(Input(0)));
            value.SetCurrentValueToZero();
            value.SetCurrentValueToZero();
            Equal(1, changes);
            Equal(1, zeros);
            value.RestoreToMaxValue();
            State(value, 100, 100);
            value.AddModifier(Input(100, ValueAttributeOperation.Subtract));
            Equal(3, changes);
            Equal(2, zeros);
        });
        Check("事件回调读取已更新状态", () =>
        {
            var value = Create();
            var order = new List<string>();
            value.ValueChanged += (_, current, max) =>
            {
                State(value, current, max);
                order.Add("change");
            };
            value.ValueReachedZero += _ => order.Add("zero");
            value.SetCurrentValueToZero();
            Equal("change,zero", string.Join(",", order));
        });
        Check("变化回调回血后不发送过期归零通知", () =>
        {
            var value = Create();
            int zeros = 0;
            value.ValueChanged += (_, current, max) =>
            {
                if (current == 0) value.RestoreToMaxValue();
            };
            value.ValueReachedZero += _ => zeros++;
            value.SetCurrentValueToZero();
            State(value, 100, 100);
            Equal(0, zeros);
        });
        Check("底层方向匹配移除及合成入口", () =>
        {
            var modifier = new ValueAttributeModifierCalculator();
            modifier.AddModifier(ValueAttributeOperation.Add, 25);
            modifier.AddModifier(ValueAttributeOperation.Subtract, 25);
            Equal(true, modifier.RemoveModifier(ValueAttributeOperation.Subtract, 25));
            Equal(125, modifier.CalculateMaxValue(100));
            Equal(true, modifier.RemoveModifier(ValueAttributeOperation.Add, 25));
            Equal(100, modifier.CalculateMaxValue(100));
            Equal(false, modifier.RemoveModifier((ValueAttributeOperation)99, 25));
            Throws<ArgumentOutOfRangeException>(() => modifier.AddModifier(ValueAttributeOperation.Add, -1));
            Throws<ArgumentOutOfRangeException>(() => modifier.AddModifier((ValueAttributeOperation)99, 10));
            Equal(100, modifier.CalculateMaxValue(100));
        });
        Check("列表中间删除及重复修饰逐条撤销", () =>
        {
            var list = new ValueAttributeModifierCollection();
            list.AddModifier(ValueAttributeOperation.Subtract, 10);
            list.AddModifier(ValueAttributeOperation.Add, 20);
            list.AddModifier(ValueAttributeOperation.Add, 10);
            list.AddModifier(ValueAttributeOperation.Subtract, 10);
            Equal(true, list.RemoveModifier(ValueAttributeOperation.Add, 20));
            Equal(-10, list.ClampedTotalValue);
            Equal(true, list.RemoveModifier(ValueAttributeOperation.Subtract, 10));
            Equal(0, list.ClampedTotalValue);
            Equal(true, list.RemoveModifier(ValueAttributeOperation.Add, 10));
            Equal(-10, list.ClampedTotalValue);
            Equal(false, list.RemoveModifier(ValueAttributeOperation.Add, 10));
            Equal(-10, list.ClampedTotalValue);
            Equal(true, list.RemoveModifier(ValueAttributeOperation.Subtract, 10));
            Equal(0, list.ClampedTotalValue);
            Equal(false, list.RemoveModifier(ValueAttributeOperation.Subtract, 10));
        });
        Check("列表非法移除不改变累计值或记录", () =>
        {
            var list = new ValueAttributeModifierCollection();
            list.AddModifier(ValueAttributeOperation.Add, 10);
            Equal(false, list.RemoveModifier(ValueAttributeOperation.Add, -10));
            Equal(false, list.RemoveModifier((ValueAttributeOperation)99, 10));
            Equal(false, list.RemoveModifier(ValueAttributeOperation.Subtract, 10));
            Equal(10, list.ClampedTotalValue);
            Equal(true, list.RemoveModifier(ValueAttributeOperation.Add, 10));
            Equal(0, list.ClampedTotalValue);
        });
        Check("大整数累计后移除仍保留小额修饰", () =>
        {
            var list = new ValueAttributeModifierCollection();
            list.AddModifier(ValueAttributeOperation.Add, int.MaxValue);
            list.AddModifier(ValueAttributeOperation.Add, 1);
            Equal(int.MaxValue, list.ClampedTotalValue);
            Equal(true, list.RemoveModifier(ValueAttributeOperation.Add, int.MaxValue));
            Equal(1, list.ClampedTotalValue);
        });
        Check("修饰总和向正负边界裁剪", () =>
        {
            var list = new ValueAttributeModifierCollection();
            list.AddModifier(ValueAttributeOperation.Subtract, int.MaxValue);
            list.AddModifier(ValueAttributeOperation.Subtract, int.MaxValue);
            Equal(int.MinValue, list.ClampedTotalValue);
            list.RemoveModifier(ValueAttributeOperation.Subtract, int.MaxValue);
            Equal(-int.MaxValue, list.ClampedTotalValue);
            list.RemoveModifier(ValueAttributeOperation.Subtract, int.MaxValue);
            list.AddModifier(ValueAttributeOperation.Add, int.MaxValue);
            list.AddModifier(ValueAttributeOperation.Add, int.MaxValue);
            Equal(int.MaxValue, list.ClampedTotalValue);
        });
        Check("固定种子随机模型对照10000步", RandomizedModel);

        File.WriteAllText(Path.Combine(Application.dataPath, "..", "results.json"), JsonUtility.ToJson(s_Report, true));
        Debug.Log($"ValueAttribute: {s_Report.passed} passed, {s_Report.failures.Count} failed");
        EditorApplication.Exit(s_Report.failures.Count == 0 ? 0 : 1);
    }

    private static void RandomizedModel()
    {
        var random = new System.Random(20261105);
        var value = Create();
        var modifiers = new List<ValueAttributeChangeInput>();
        int current = 100;
        int max = 100;
        for (int i = 0; i < 10000; i++)
        {
            var input = Input(random.Next(5) == 0 ? int.MaxValue : random.Next(1, 250),
                random.Next(2) == 0 ? ValueAttributeOperation.Add : ValueAttributeOperation.Subtract);
            int action = random.Next(6);
            if (action == 0)
            {
                long next = (long)current + (input.Operation == ValueAttributeOperation.Add ? input.Amount : -(long)input.Amount);
                int expected = (int)Math.Max(0, Math.Min(next, max));
                Equal(expected != current, value.ModifyCurrentValue(input));
                current = expected;
            }
            else if (action == 1 || action == 2)
            {
                if (action == 1)
                {
                    modifiers.Add(input);
                    Equal(true, value.AddModifier(input));
                }
                else
                {
                    if (modifiers.Count > 0 && random.Next(2) == 0)
                        input = modifiers[random.Next(modifiers.Count)];
                    int index = modifiers.FindLastIndex(item => item.Amount == input.Amount && item.Operation == input.Operation);
                    Equal(index >= 0, value.RemoveModifier(input));
                    if (index >= 0) modifiers.RemoveAt(index);
                }
                long sum = 100;
                foreach (var modifier in modifiers)
                    sum += modifier.Operation == ValueAttributeOperation.Add ? modifier.Amount : -(long)modifier.Amount;
                int nextMax = (int)Math.Max(0, Math.Min(sum, int.MaxValue));
                current = current == max ? nextMax : Math.Min(current, nextMax);
                max = nextMax;
            }
            else if (action == 3)
            {
                value.SetCurrentValueToZero();
                current = 0;
            }
            else if (action == 4)
            {
                value.RestoreToMaxValue();
                current = max;
            }
            else value.NotifyCurrentState();
            State(value, current, max);
        }
    }

    private static RuntimeValueAttribute Create(int source = 100)
    {
        var asset = ScriptableObject.CreateInstance<ValueAttributeAsset>();
        try
        {
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("m_AttributeName").intValue = (int)ValueAttributeName.Health;
            serialized.FindProperty("m_BaseValue").intValue = source;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return new RuntimeValueAttribute(asset);
        }
        finally { UnityEngine.Object.DestroyImmediate(asset); }
    }

    private static ValueAttributeChangeInput Input(int amount, ValueAttributeOperation operation = ValueAttributeOperation.Add)
    {
        return new ValueAttributeChangeInput(ValueAttributeName.Health, amount, operation);
    }

    private static void State(RuntimeValueAttribute value, int current, int max)
    {
        Equal(current, value.CurrentValue);
        Equal(max, value.MaxValue);
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"Expected {expected}, actual {actual}");
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new Exception($"Expected {typeof(T).Name}");
    }

    private static void Check(string name, Action action)
    {
        try
        {
            action();
            s_Report.passed++;
        }
        catch (Exception exception)
        {
            s_Report.failures.Add(name + ": " + exception);
        }
    }
}
