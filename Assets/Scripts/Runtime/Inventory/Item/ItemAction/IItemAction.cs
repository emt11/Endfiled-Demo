using Sirenix.OdinInspector;

namespace GP
{
    public interface IItemAction
    {
        public ItemActionCategory ActionCategory { get; }

        public bool CanExecute(RuntimeItem runtimeItem);
        public void Execute(RuntimeItem runtimeItem);
    }

    public enum ItemActionCategory
    {
        None,
        [LabelText("出售")] Sell,
        [LabelText("使用")] Use,
        [LabelText("装备")] Equip,
        [LabelText("销毁")] Destroy,
        [LabelText("分解")] Decompose
    }
}
