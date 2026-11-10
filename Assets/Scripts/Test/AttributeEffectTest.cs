using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace GP
{
    [MovedFrom(true, "", null, "AttributeEffectTest")]
    public class AttributeEffectTest : MonoBehaviour
    {
        [FormerlySerializedAs("attributes")]
        [SerializeField] private CharacterAttribute m_Attributes = new CharacterAttribute();
        [FormerlySerializedAs("poison")]
        [SerializeField] private StatusEffectAsset m_Poison;

        private RuntimeStatusEffectManager m_Effects;

        private void Awake()
        {
            EnsureRuntimeInitialized();
        }

        private void OnDestroy()
        {
            m_Effects?.Dispose();
            m_Effects = null;
            if (m_Attributes == null) return;
            m_Attributes.ValueChanged -= ValueChanged;
            m_Attributes.ValueReachedZero -= OnReachedZero;
            m_Attributes.Deinitialize();
        }

        private bool EnsureRuntimeInitialized()
        {
            // ContextMenu 在编辑模式下也能调用，但此时 Awake 尚未创建运行时数据
            if (!Application.isPlaying)
            {
                Debug.LogWarning("请先进入 Play Mode，再执行属性和效果测试。", this);
                return false;
            }

            if (m_Effects != null && m_Attributes != null && m_Attributes.IsInitialized) return true;
            if (m_Attributes == null)
            {
                Debug.LogWarning("请先配置角色属性。", this);
                return false;
            }

            // Play Mode 中脚本重载后，运行时字段可能丢失；重建时避免重复订阅
            m_Effects?.Dispose();
            m_Attributes.Initialize();
            m_Attributes.ValueChanged -= ValueChanged;
            m_Attributes.ValueReachedZero -= OnReachedZero;
            m_Attributes.ValueChanged += ValueChanged;
            m_Attributes.ValueReachedZero += OnReachedZero;

            m_Effects = new RuntimeStatusEffectManager(m_Attributes);
            m_Attributes.NotifyCurrentState();
            return true;
        }

        private void ValueChanged(ValueAttributeName name, int current, int max)
        {
            Debug.Log($"{name}：{current}/{max}");
        }

        private void OnReachedZero(ValueAttributeName name)
        {
            Debug.Log($"{name} 归零");
        }

        private bool HasPoisonAsset()
        {
            if (m_Poison != null) return true;
            Debug.LogWarning("请先在 Inspector 中给 Poison 指定效果资产。", this);
            return false;
        }

        [ContextMenu("扣血20")]
        private void Damage()
        {
            if (!EnsureRuntimeInitialized()) return;
            m_Attributes.ModifyCurrentValue(new ValueAttributeChangeInput(
                ValueAttributeName.Health, 20, ValueAttributeOperation.Subtract));
        }

        [ContextMenu("增加上限50")]
        private void AddBonus()
        {
            if (!EnsureRuntimeInitialized()) return;
            m_Attributes.AddModifier(new ValueAttributeChangeInput(
                ValueAttributeName.Health, 50, ValueAttributeOperation.Add));
        }

        [ContextMenu("移除上限加成50")]
        private void RemoveBonus()
        {
            if (!EnsureRuntimeInitialized()) return;
            m_Attributes.RemoveModifier(new ValueAttributeChangeInput(
                ValueAttributeName.Health, 50, ValueAttributeOperation.Add));
        }

        [ContextMenu("恢复满血")]
        private void Restore()
        {
            if (!EnsureRuntimeInitialized()) return;
            m_Attributes.RestoreToMaxValue(ValueAttributeName.Health);
        }

        [ContextMenu("生命归零")]
        private void Zero()
        {
            if (!EnsureRuntimeInitialized()) return;
            m_Attributes.SetCurrentValueToZero(ValueAttributeName.Health);
        }

        [ContextMenu("添加一层中毒")]
        private void AddPoison()
        {
            if (!EnsureRuntimeInitialized() || !HasPoisonAsset()) return;
            m_Effects.AddStatusEffect(m_Poison);
            ShowState();
        }

        [ContextMenu("驱散中毒")]
        private void RemovePoison()
        {
            if (!EnsureRuntimeInitialized() || !HasPoisonAsset()) return;
            m_Effects.RemoveStatusEffect(m_Poison);
            ShowState();
        }

        [ContextMenu("查看状态")]
        private void ShowState()
        {
            if (!EnsureRuntimeInitialized()) return;
            if (!m_Attributes.TryGetValues(ValueAttributeName.Health, out int current, out int max))
            {
                Debug.LogWarning("角色属性列表中没有 Health，请添加生命属性资产。", this);
                return;
            }

            Debug.Log($"生命：{current}/{max}，效果数量：{m_Effects.Count}");

            if (m_Effects.TryGetStatusEffect(m_Poison, out var effect))
                Debug.Log($"中毒层数：{effect.StackCount}，单次效果值：{effect.EffectValue}，剩余：{effect.RemainingTime:F2}秒");
        }
    }

}
