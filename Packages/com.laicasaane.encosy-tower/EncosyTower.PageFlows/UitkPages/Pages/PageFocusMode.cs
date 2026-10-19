namespace EncosyTower.PageFlows.UitkPages
{
    /// <summary>How a flow moves focus after a page shows.</summary>
    public enum PageFocusMode
    {
        /// <summary>The flow leaves focus where it is.</summary>
        None,

        /// <summary>The first focusable element of the page takes focus.</summary>
        FirstFocusable,

        /// <summary>The element named by <see cref="IUitkPageHasFocus.FocusElementName"/> takes focus.</summary>
        Named,
    }
}
