using System;
using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    [Serializable]
    public class ComboTreeNodeDictionary : GPDictionary<int, ComboTreeNode> { }

    [CreateAssetMenu(fileName = "New Combo Tree", menuName = "GP/Melee System/Combo Tree")]
    public class ComboTreeAsset : ScriptableObject
    {
        public const int InvalidNodeId = -1;

        [SerializeField] private List<int> m_RootIds = new List<int>();
        [SerializeField]
        private ComboTreeNodeDictionary m_Nodes = new ComboTreeNodeDictionary();
        [NonSerialized] private IReadOnlyList<int> m_RootIdsView;

        public IReadOnlyList<int> RootIds => m_RootIdsView ??= m_RootIds.AsReadOnly();

        public int AddRootNode(ComboActionData actionData)
        {
            return InsertNode(null, m_RootIds.Count, actionData);
        }

        public int AddChildNode(ComboActionData actionData, int parentId)
        {
            var parent = GetNode(parentId);
            return parent == null ? InvalidNodeId : InsertNode(parent, parent.ChildIds.Count, actionData);
        }

        public int AddChildNode(ComboActionData actionData, int parentId, int index)
        {
            var parent = GetNode(parentId);
            return parent == null ? InvalidNodeId : InsertNode(parent, index, actionData);
        }

        public int AddSiblingNode(ComboActionData actionData, int siblingId, bool insertAfter = true)
        {
            var sibling = GetNode(siblingId);
            if (sibling == null) return InvalidNodeId;
            var parent = GetNode(sibling.ParentId);
            var index = parent != null ? parent.IndexOfChild(siblingId) : m_RootIds.IndexOf(siblingId);
            if (index < 0) return InvalidNodeId;
            return InsertNode(parent, index + (insertAfter ? 1 : 0), actionData);
        }

        public bool RemoveNode(int nodeId)
        {
            var node = GetNode(nodeId);
            if (node == null) return false;
            var parent = GetNode(node.ParentId);
            if (parent != null) parent.RemoveChild(nodeId);
            else m_RootIds.Remove(nodeId);

            var pendingNodes = new Stack<ComboTreeNode>();
            var visited = new HashSet<int>();
            pendingNodes.Push(node);
            while (pendingNodes.Count > 0)
            {
                var current = pendingNodes.Pop();
                if (!visited.Add(current.Id)) continue;
                foreach (var childId in current.ChildIds)
                {
                    var child = GetNode(childId);
                    if (child != null) pendingNodes.Push(child);
                }
                m_Nodes.Remove(current.Id);
            }
            return true;
        }

        public int GetParentId(int childId) => GetNode(childId)?.ParentId ?? InvalidNodeId;

        public IReadOnlyList<int> GetChildIds(int parentId)
        {
            return GetNode(parentId)?.ChildIds ?? Array.Empty<int>();
        }

        public ComboTreeNode GetNode(int nodeId)
        {
            return m_Nodes.TryGetValue(nodeId, out var node) ? node : null;
        }

        public ComboActionData GetActionData(int nodeId) => GetNode(nodeId)?.ActionData;
        public bool ContainsNode(int nodeId) => nodeId != InvalidNodeId && m_Nodes.ContainsKey(nodeId);

        public bool TryGetNextAction(int previousNodeId, MeleeInputTrigger trigger,
            MeleeInputSignal signal, out int nodeId, out ComboActionAsset action)
        {
            nodeId = InvalidNodeId;
            action = null;
            if (signal == MeleeInputSignal.None) return false;
            if (previousNodeId != InvalidNodeId && !ContainsNode(previousNodeId)) return false;

            var candidates = previousNodeId == InvalidNodeId ? RootIds : GetChildIds(previousNodeId);
            // 只有到达叶节点才回到起手动作；分支不匹配时不会跳到其他起手
            if (candidates.Count == 0) candidates = RootIds;
            foreach (var candidateId in candidates)
            {
                var data = GetActionData(candidateId);
                if (data == null || !data.MatchesInput(trigger, signal)) continue;
                nodeId = candidateId;
                action = data.Action;
                return true;
            }
            return false;
        }

        private int InsertNode(ComboTreeNode parent, int index, ComboActionData actionData)
        {
            var count = parent != null ? parent.ChildIds.Count : m_RootIds.Count;
            if (actionData == null || index < 0 || index > count) return InvalidNodeId;
            int id;
            do
            {
                id = Guid.NewGuid().GetHashCode() & int.MaxValue;
            }
            while (m_Nodes.ContainsKey(id));

            m_Nodes.Add(id, new ComboTreeNode(id, parent?.Id ?? InvalidNodeId, actionData));
            if (parent == null) m_RootIds.Insert(index, id);
            else parent.InsertChild(index, id);
            return id;
        }
    }
}
