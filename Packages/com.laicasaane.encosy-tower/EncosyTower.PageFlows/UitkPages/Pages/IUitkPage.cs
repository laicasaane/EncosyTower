using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages
{
    /// <summary>A page that a UI Toolkit page flow can show.</summary>
    public interface IUitkPage : IPage
    {
        /// <summary>Gets the root element of the page.</summary>
        VisualElement PageRoot { get; }
    }

    /// <summary>A page that says where focus goes after the page shows.</summary>
    public interface IUitkPageHasFocus : IUitkPage
    {
        /// <summary>Gets how the flow moves focus after the page shows.</summary>
        PageFocusMode FocusMode { get; }

        /// <summary>Gets the name of the element that takes focus with <see cref="PageFocusMode.Named"/>.</summary>
        string FocusElementName { get; }
    }

    internal interface IUitkPageSlotReceiver
    {
        void SetSlot(TemplateContainer slot);
    }
}
