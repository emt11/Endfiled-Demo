using System;
using UnityEngine;

namespace GP
{
    /// <summary>角色属性与状态效果的 Unity 宿主，供背包、技能等组件访问。</summary>
    [DisallowMultipleComponent]
    public class CharacterAttributeComponent : MonoBehaviour
    {
        [SerializeField]
        private CharacterAttribute m_CharacterAttribute = new CharacterAttribute();

        public CharacterAttribute CharacterAttribute => m_CharacterAttribute;
        public RuntimeStatusEffectManager RuntimeStatusEffectManager { get; private set; }

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Deinitialize();
        }

        /// <summary>可重复调用，也供对象池复用及其他组件在 Awake 中访问。</summary>
        public void Initialize()
        {
            if (m_CharacterAttribute == null)
                throw new InvalidOperationException("请先配置角色属性。");
            if (m_CharacterAttribute.IsInitialized && RuntimeStatusEffectManager != null) return;

            RuntimeStatusEffectManager?.Dispose();
            m_CharacterAttribute.Initialize();
            RuntimeStatusEffectManager = new RuntimeStatusEffectManager(m_CharacterAttribute);
        }

        public void Deinitialize()
        {
            RuntimeStatusEffectManager?.Dispose();
            RuntimeStatusEffectManager = null;
            m_CharacterAttribute?.Deinitialize();
        }
    }
}
