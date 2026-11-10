using System;
using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    [Serializable]
    public class ComboTreeNode
    {
        [SerializeField] private int m_Id;
        [SerializeField] private int m_ParentId;
        [SerializeField] private List<int> m_ChildIds = new List<int>();
        [SerializeField] private ComboActionData m_ComboActionData;
        [NonSerialized] private IReadOnlyList<int> m_ChildIdsView;

        public int Id => m_Id;
        public int ParentId => m_ParentId;
        public IReadOnlyList<int> ChildIds => m_ChildIdsView ??= m_ChildIds.AsReadOnly();
        public ComboActionData ActionData => m_ComboActionData;

        public ComboTreeNode(int id, int parentId, ComboActionData actionData)
        {
            m_Id = id;
            m_ParentId = parentId;
            m_ComboActionData = actionData;
        }

        internal void InsertChild(int index, int childId)
        {
            if (index < 0 || index > m_ChildIds.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (!m_ChildIds.Contains(childId)) m_ChildIds.Insert(index, childId);
        }

        internal void RemoveChild(int childId)
        {
            m_ChildIds.Remove(childId);
        }

        internal int IndexOfChild(int childId)
        {
            return m_ChildIds.IndexOf(childId);
        }
    }

    [Serializable]
    public class ComboActionData
    {
        [SerializeField]
        private ComboActionAsset m_Action;

        public ComboActionAsset Action
        {
            get => m_Action;
            set => m_Action = value;
        }

        public bool MatchesInput(MeleeInputTrigger trigger, MeleeInputSignal signal)
        {
            return m_Action && m_Action.MatchesInput(trigger, signal);
        }
    }
}
