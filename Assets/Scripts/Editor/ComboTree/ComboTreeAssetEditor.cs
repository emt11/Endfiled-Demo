using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using TreeView = UnityEngine.UIElements.TreeView;

namespace GP
{
    [CustomEditor(typeof(ComboTreeAsset))]
    public class ComboTreeAssetEditor : Editor
    {
        private VisualElement m_Root; // 根容器
        private Button m_AddRootNodeButton; // 添加节点按钮
        private Button m_AddChildNodeButton; // 添加子节点按钮，这个按钮在没有任何节点时无法被点击
        private TreeView m_TreeView; // 树状体容器
        private VisualElement m_ComboNodeView; // 单个Combo节点容器

        private ComboTreeAsset m_ComboTree; // 连招树资产
        private ComboActionData m_CurrentSelectedComboActionData; // 当前选中的ComboTree中的某一个Node对象

        private readonly List<TreeViewItemData<ComboActionData>> m_TreeComboData = new(); // 存储树状图中的内容

        private int m_SelectedNodeId = ComboTreeAsset.InvalidNodeId;
        private bool m_IsRebuilding;

        public TreeView TreeView => m_TreeView;

        private void OnEnable()
        {
            m_ComboTree = target as ComboTreeAsset;
            Undo.undoRedoPerformed += RebuildTreeView;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= RebuildTreeView;
            if (m_AddRootNodeButton != null) m_AddRootNodeButton.clicked -= AddRootNode;
            if (m_AddChildNodeButton != null) m_AddChildNodeButton.clicked -= AddChildNode;
            if (m_TreeView != null) m_TreeView.selectionChanged -= OnSelectionChanged;
        }

        public override VisualElement CreateInspectorGUI()
        {
            m_Root = new VisualElement();

            m_ComboNodeView = new VisualElement()
            {
                style =
                {
                    marginTop = 10f,
                }
            };

            m_AddRootNodeButton = new Button()
            {
                text = "添加节点",
                style =
                {
                    marginTop = 5,
                    marginBottom = 5,
                    height = 50,
                    width = 110,
                    color = new StyleColor(Color.red),
                    alignSelf= Align.Center,
                    borderBottomLeftRadius = 20f,
                    borderBottomRightRadius = 20f,
                    borderTopLeftRadius =  20f,
                    borderTopRightRadius = 20f,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 15f
                }
            };
            m_AddRootNodeButton.tooltip = "添加根节点或者同层级子节点";
            m_AddRootNodeButton.clicked += AddRootNode;

            m_AddChildNodeButton = new Button()
            {
                text = "添加子节点🦇",
                style =
                {
                    marginTop = 5,
                    marginBottom = 5,
                    height = 50,
                    width = 110,
                    color = new StyleColor(Color.red),
                    alignSelf =  Align.Center,
                    borderBottomLeftRadius = 20f,
                    borderBottomRightRadius = 20f,
                    borderTopLeftRadius =  20f,
                    borderTopRightRadius = 20f,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 15f,
                },
                enabledSelf = false,
            };
            m_AddChildNodeButton.tooltip = "添加当前选中节点的子节点";
            m_AddChildNodeButton.clicked += AddChildNode;

            ShowTreeView();
            m_AddChildNodeButton.enabledSelf = m_TreeView.selectedIndex != -1;
            m_Root.Add(m_AddRootNodeButton);
            m_Root.Add(m_AddChildNodeButton);

            return m_Root;
        }

        public void RemoveNode(int nodeId)
        {
            if (m_ComboTree.GetActionData(nodeId) == null) return;
            Undo.RecordObject(m_ComboTree, "Remove Combo Node");
            if (!m_ComboTree.RemoveNode(nodeId)) return;
            EditorUtility.SetDirty(m_ComboTree);
            RebuildTreeView();
        }

        private void RebuildTreeView()
        {
            if (m_TreeView == null) return;

            m_TreeComboData.Clear();

            // 读取最新数据
            serializedObject.Update();

            var rootNodeIds = m_ComboTree.RootIds;

            foreach (var rootId in rootNodeIds)
            {
                m_TreeComboData.AddRange(GetTreeComboData(rootId));
            }

            serializedObject.ApplyModifiedProperties();

            // 使用 SetRootItems 更新树的数据
            m_IsRebuilding = true;
            try
            {
                m_TreeView.SetRootItems(m_TreeComboData);
                m_TreeView.Rebuild();
                m_CurrentSelectedComboActionData = m_ComboTree.GetActionData(m_SelectedNodeId);
                if (m_CurrentSelectedComboActionData == null)
                {
                    m_TreeView.ClearSelection();
                    HideComboView();
                }
                else
                {
                    m_TreeView.SetSelectionById(m_SelectedNodeId);
                    m_AddChildNodeButton.enabledSelf = true;
                    ShowComboNodeView();
                }
            }
            finally { m_IsRebuilding = false; }
        }

        private void ShowTreeView()
        {
            m_TreeView = new TreeView()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    minHeight = 500, // 保证至少有 200px 高度
                    flexGrow = 1, // 允许在父容器中拉伸

                },
                makeItem = MakeItem, // 构建Node可视化元素模板
                bindItem = BindItem, // 绑定数据到可视化元素
                selectionType = SelectionType.Single,
                reorderable = false, // 是否可以通过鼠标来拖拽元素来改变它们的顺序
                showAlternatingRowBackgrounds = AlternatingRowBackground.All, // 此属性控制集合视图行的背景色是否交替显示
            };

            m_TreeView.selectionChanged += OnSelectionChanged;

            m_Root.Add(m_TreeView);

            RebuildTreeView();
        }

        private IEnumerable<TreeViewItemData<ComboActionData>> GetTreeComboData(int id)
        {
            var visited = new HashSet<int>();
            return BuildTreeRecursive(id, visited);
        }

        private List<TreeViewItemData<ComboActionData>> BuildTreeRecursive(
            int id,
            HashSet<int> visited)
        {
            var result = new List<TreeViewItemData<ComboActionData>>();

            // 【防线1】防环 + 防重复访问
            if (!visited.Add(id))
            {
                EMTLog.Error($"[ComboTree] 检测到环或重复节点: {id}，已跳过");
                return result;
            }

            // 【正常情况】节点不存在
            var comboData = m_ComboTree.GetActionData(id);
            if (comboData == null)
                return result;

            // 递归构建子树
            var subData = new List<TreeViewItemData<ComboActionData>>();
            var children = m_ComboTree.GetChildIds(id);

            if (children is { Count: > 0 })
            {
                foreach (var childId in children)
                {
                    subData.AddRange(BuildTreeRecursive(childId, visited));
                }
            }

            result.Add(new TreeViewItemData<ComboActionData>(id, comboData, subData));
            return result;
        }

        /// <summary>
        /// 构建可视化元素
        /// </summary>
        private VisualElement MakeItem()
        {
            var itemView = new ComboTreeNodeView(this);
            return itemView;
        }

        // 绑定TreeView中Item的内容数据
        private void BindItem(VisualElement element, int index)
        {
            var data = m_TreeView.GetItemDataForIndex<ComboActionData>(index);

            if (element is ComboTreeNodeView treeItemEditor)
            {
                treeItemEditor.BindItem(m_TreeView.GetIdForIndex(index), data);
            }
        }

        private void ShowComboNodeView()
        {
            if (m_CurrentSelectedComboActionData == null) return;

            m_ComboNodeView.Clear();

            var combo = new ObjectField("Combo")
            {
                objectType = typeof(ComboActionAsset),
                value = m_CurrentSelectedComboActionData.Action
            };
            combo.label = "动作资产映射";

            combo.RegisterValueChangedCallback(value =>
            {
                if (m_CurrentSelectedComboActionData == null) return;

                Undo.RecordObject(m_ComboTree, "Change Combo"); // Undo记录，方便撤回
                m_CurrentSelectedComboActionData.Action = (ComboActionAsset)value.newValue;
                EditorUtility.SetDirty(m_ComboTree);

                if (m_TreeView.selectedIndex != -1)
                {
                    // 当主要数据源为发生大的改变时，不要去重建树，而是刷新；只有在数据源发生改变时，再去重建树
                    m_TreeView.RefreshItem(m_TreeView.selectedIndex);
                }
            });

            m_ComboNodeView.style.display = DisplayStyle.Flex; // 显示
            m_ComboNodeView.Add(combo);
            m_Root.Add(m_ComboNodeView);
        }

        private void HideComboView()
        {
            m_CurrentSelectedComboActionData = null;
            m_ComboNodeView.Clear();
            m_ComboNodeView.style.display = DisplayStyle.None; // 隐藏
            m_AddChildNodeButton.enabledSelf = false;
        }

        private void AddRootNode()
        {
            var rootIds = m_TreeView.GetRootIds().ToArray();
            var selectedId = m_TreeView.selectedIndex switch
            {
                ComboTreeAsset.InvalidNodeId => rootIds.Length > 0 ? rootIds[^1] : ComboTreeAsset.InvalidNodeId,
                _ => m_TreeView.GetIdForIndex(m_TreeView.selectedIndex)
            };
            var comboData = new ComboActionData();

            serializedObject.Update();
            Undo.RecordObject(m_ComboTree, "Add Combo Root Or Sibling");
            var id = selectedId != ComboTreeAsset.InvalidNodeId
                        ? m_ComboTree.AddSiblingNode(comboData, selectedId)
                        : m_ComboTree.AddRootNode(comboData);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(m_ComboTree);

            RebuildTreeView();
            m_TreeView.SetSelectionById(id);
            m_TreeView.ScrollToItemById(id);
        }

        private void AddChildNode()
        {
            if (m_TreeView.selectedIndex == ComboTreeAsset.InvalidNodeId) return; // 如果当前未选择任何节点，直接返回

            var data = new ComboActionData();

            var parentId = m_TreeView.GetIdForIndex(m_TreeView.selectedIndex);
            serializedObject.Update();
            Undo.RecordObject(m_ComboTree, "Add Combo Child");

            var childId = m_ComboTree.AddChildNode(data, parentId);
            EditorUtility.SetDirty(m_ComboTree);

            serializedObject.ApplyModifiedProperties();

            RebuildTreeView();
            m_TreeView.SetSelectionById(childId);
            m_TreeView.ScrollToItemById(childId);
        }

        private void OnSelectionChanged(IEnumerable<object> itemData)
        {
            if (m_IsRebuilding) return;
            m_SelectedNodeId = m_TreeView.selectedIndex < 0
                ? ComboTreeAsset.InvalidNodeId : m_TreeView.GetIdForIndex(m_TreeView.selectedIndex);
            m_AddChildNodeButton.enabledSelf = m_TreeView.selectedIndex != -1;
            var item = itemData.FirstOrDefault();
            m_CurrentSelectedComboActionData = item as ComboActionData;
            if (m_CurrentSelectedComboActionData == null)
            {
                HideComboView();
                return;
            }
            ShowComboNodeView();
        }

    }

}
