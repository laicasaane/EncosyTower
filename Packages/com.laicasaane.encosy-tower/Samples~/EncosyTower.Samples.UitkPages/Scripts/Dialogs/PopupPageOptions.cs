using EncosyTower.PageFlows;

namespace EncosyTower.Samples.UitkPages.Dialogs
{
    /// <summary>The <see cref="PageOptions"/> that the popups of the Book share.</summary>
    internal static class PopupPageOptions
    {
        /// <summary>Keeps the page below a popup as it is: neither its hide nor its show runs.</summary>
        /// <remarks>
        /// Uses <see cref="PageTransitionOptions.NoTransition"/> with <see cref="PageOptions.Options.forceUse"/> for
        /// the other page of the popup show and hide. It demonstrates how a popup keeps the lower popup in a
        /// multi-page stack.
        /// </remarks>
        public static readonly PageOptions KeepLowerPage = new() {
            showOptions = new() {
                hideOtherPage = new() {
                    forceUse = true,
                    transitionOptions = PageTransitionOptions.NoTransition,
                },
            },
            hideOptions = new() {
                showOtherPage = new() {
                    forceUse = true,
                    transitionOptions = PageTransitionOptions.NoTransition,
                },
            },
        };
    }
}
