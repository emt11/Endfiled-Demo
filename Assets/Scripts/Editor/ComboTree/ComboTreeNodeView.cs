using UnityEngine.UIElements;

namespace GP
{
    public class ComboTreeNodeView : VisualElement
    {
        private readonly ComboTreeAssetEditor m_ComboTreeEditor;
        private readonly VisualElement m_Root;
        private readonly Label m_Label;
        private ComboActionData m_NodeActionData;
        private int m_NodeId = ComboTreeAsset.InvalidNodeId;

        public ComboTreeNodeView(ComboTreeAssetEditor comboTreeAssetEditor)
        {
            m_ComboTreeEditor = comboTreeAssetEditor;

            m_Root = new VisualElement();
            m_Label = new Label();

            m_Root.Add(m_Label);
            m_Label.AddManipulator(new ContextualMenuManipulator(OpenTreeNodeMenu));

            Add(m_Root);
        }

        public void BindItem(int nodeId, ComboActionData actionData)
        {
            m_NodeId = nodeId;
            m_NodeActionData = actionData;
            m_Label.text = actionData == null ? string.Empty : actionData.Action?.ActionName ?? "新节点";
        }

        private void OpenTreeNodeMenu(ContextualMenuPopulateEvent evt)
        {
            if (m_NodeActionData == null) return;
            int nodeId = m_NodeId;
            evt.menu.AppendAction("移除节点", _ => m_ComboTreeEditor.RemoveNode(nodeId));
        }
    }
}
