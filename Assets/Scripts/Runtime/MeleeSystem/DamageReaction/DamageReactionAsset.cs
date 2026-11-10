using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [CreateAssetMenu(fileName = "New Damage Reactions", menuName = "GP/Melee System/Damage Reactions")]
    [Icon(ScriptGizmosPath.HealthGizmosPath)]
    public class DamageReactionAsset : ScriptableObject
    {
        [SerializeField, LabelText("受击反应（按顺序匹配）")]
        private DamageReaction[] m_Reactions = Array.Empty<DamageReaction>();

        public AnimationAsset FindReaction(HitDirection direction, int damage)
        {
            return FindReactionResult(direction, damage).Animation;
        }

        public DamageReactionResult FindReactionResult(HitDirection direction, int damage)
        {
            if (m_Reactions == null) return default;
            foreach (var reaction in m_Reactions)
            {
                if (reaction != null && reaction.Matches(direction, damage) && reaction.Animation)
                    return reaction.Result;
            }
            return default;
        }
    }
}
