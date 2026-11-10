using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "GP/Melee System/Weapon")]
    [Icon(ScriptGizmosPath.MeleeGizmosPath)]
    public class WeaponAsset : ScriptableObject
    {
        [SerializeField, LabelText("武器名称")]
        private string m_WeaponName;
        [SerializeField, LabelText("武器图标")]
        private Sprite m_Icon;
        [SerializeField, LabelText("武器类型")]
        private WeaponCategory m_Category = WeaponCategory.Melee;
        [SerializeField, TextArea, LabelText("武器描述")]
        private string m_Description = string.Empty;
        [SerializeField, LabelText("连招树")]
        private ComboTreeAsset m_ComboTree;
        [SerializeField, LabelText("武器挂载")]
        private WeaponAttachment[] m_Attachments = Array.Empty<WeaponAttachment>();
        [SerializeField, LabelText("受击反应")]
        private DamageReactionAsset m_DamageReactions;
        [SerializeField, LabelText("攻击被完美格挡后的反应")]
        private DamageReactionAsset m_ParriedReactions;
        [SerializeField, LabelText("防御配置")]
        private DefenseAsset m_Defense;
        [SerializeField, LabelText("左键输入")]
        private WeaponInputConfig m_PrimaryInput;
        [SerializeField, LabelText("右键输入")]
        private WeaponInputConfig m_SecondaryInput;
        [SerializeField, LabelText("F 键输入")]
        private WeaponInputConfig m_UtilityInput;

        public string WeaponName => m_WeaponName;
        public Sprite Icon => m_Icon;
        public WeaponCategory Category => m_Category;
        public string Description => m_Description;
        public ComboTreeAsset ComboTree => m_ComboTree;
        public IReadOnlyList<WeaponAttachment> Attachments => m_Attachments ?? Array.Empty<WeaponAttachment>();
        public DamageReactionAsset DamageReactions => m_DamageReactions;
        public DamageReactionAsset ParriedReactions => m_ParriedReactions;
        public DefenseAsset Defense => m_Defense;
        public WeaponInputConfig PrimaryInput => m_PrimaryInput;
        public WeaponInputConfig SecondaryInput => m_SecondaryInput;
        public WeaponInputConfig UtilityInput => m_UtilityInput;
    }

    public enum WeaponCategory
    {
        None = 0,
        [InspectorName("近战")] Melee = 1,
        [InspectorName("远程")] Ranged = 2
    }
}
