using UnityEditor;
using UnityEngine;

namespace GP
{
    [CustomEditor(typeof(CharacterBag))]
    public class CharacterBagEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!Application.isPlaying) return;
            var bag = (CharacterBag)target;
            foreach (var currency in bag.GetCurrencies())
                EditorGUILayout.LabelField(currency.Category.ToString(), currency.CurrencyWorth.ToString());
            foreach (var item in bag.GetItems())
            {
                EditorGUILayout.LabelField(item.ItemAsset.ItemName, item.ItemStack.ToString());
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawAction(item, ItemActionCategory.Use, "使用");
                    DrawAction(item, ItemActionCategory.Sell, "出售");
                    DrawAction(item, ItemActionCategory.Decompose, "分解");
                }
            }
        }

        public override bool RequiresConstantRepaint() => Application.isPlaying;

        private static void DrawAction(RuntimeItem item, ItemActionCategory category, string label)
        {
            using (new EditorGUI.DisabledScope(!item.HasItemAction(category)))
                if (GUILayout.Button(label)) item.TryExecuteAction(category);
        }
    }
}
