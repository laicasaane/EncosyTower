using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages
{
    public interface IUitkPage : IPage
    {
        VisualElement PageRoot { get; }
    }

    public interface IUitkPageHasFocus : IUitkPage
    {
        PageFocusMode FocusMode { get; }

        string FocusElementName { get; }
    }

    internal interface IUitkPageSlotReceiver
    {
        void SetSlot(TemplateContainer slot);
    }
}
