using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Serialization;
using UnityEngine;

namespace EncosyTower.PageFlows
{
    /// <summary>
    /// The options a page sets for its own show and hide, and for the other page that transitions with it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A flow reads the options from <see cref="IPageHasOptions.PageOptions"/> of the pages in a transition. The
    /// <see cref="Options"/> of a step replace the transition options of the call only when
    /// <see cref="Options.forceUse"/> is <c>true</c>.
    /// </para>
    /// <para>
    /// <see cref="PageTransitionOptions.NoTransition"/> does not stop a transition whose
    /// <see cref="IPageTransition.ForceRunShow"/> or <see cref="IPageTransition.ForceRunHide"/> is <c>true</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // On a popup page in a PageStackMulti: the page below it neither hides nor shows again.
    /// public PageOptions PageOptions => new() {
    ///     showOptions = new() { hideOtherPage = KeepLower },
    ///     hideOptions = new() { showOtherPage = KeepLower },
    /// };
    ///
    /// private static readonly PageOptions.Options KeepLower = new() {
    ///     forceUse = true,
    ///     transitionOptions = PageTransitionOptions.NoTransition,
    /// };
    /// </code>
    /// </example>
    [Serializable]
    public struct PageOptions
    {
        /// <summary>The options that apply when this page shows.</summary>
        [Tooltip("The options that apply when this page shows.")]
        public ShowOperationOptions showOptions;

        /// <summary>The options that apply when this page hides.</summary>
        [Tooltip("The options that apply when this page hides.")]
        public HideOperationOptions hideOptions;

        /// <summary>Whether a list rejects adding this page when it already holds the page.</summary>
        /// <remarks>
        /// Not overridden: the flow decides. Overridden to <c>true</c>, the page is rejected even when the flow
        /// allows duplicate pages. A flow that rejects duplicate pages rejects this page whatever this value is.
        /// </remarks>
        [Tooltip("Whether a list rejects adding this page when it already holds the page.")]
        public Overridable<bool> rejectDuplicatePages;

        /// <summary>Selects the options of the hide step of a transition.</summary>
        /// <param name="transition">Whether the transition shows or hides the page of the call.</param>
        /// <param name="pageToHideOptions">The options of the page that hides.</param>
        /// <param name="pageToShowOptions">The options of the page that shows.</param>
        /// <returns>
        /// <c>hideThisPage</c> of the page that hides for a hide transition; otherwise <c>hideOtherPage</c> of the
        /// page that shows.
        /// </returns>
        public static Options SelectHideOptions(
              PageTransition transition
            , in PageOptions pageToHideOptions
            , in PageOptions pageToShowOptions
        )
        {
            return transition == PageTransition.Hide
                ? pageToHideOptions.hideOptions.hideThisPage
                : pageToShowOptions.showOptions.hideOtherPage;
        }

        /// <summary>Selects the options of the show step of a transition.</summary>
        /// <param name="transition">Whether the transition shows or hides the page of the call.</param>
        /// <param name="pageToHideOptions">The options of the page that hides.</param>
        /// <param name="pageToShowOptions">The options of the page that shows.</param>
        /// <returns>
        /// <c>showOtherPage</c> of the page that hides for a hide transition; otherwise <c>showThisPage</c> of the
        /// page that shows.
        /// </returns>
        public static Options SelectShowOptions(
              PageTransition transition
            , in PageOptions pageToHideOptions
            , in PageOptions pageToShowOptions
        )
        {
            return transition == PageTransition.Hide
                ? pageToHideOptions.hideOptions.showOtherPage
                : pageToShowOptions.showOptions.showThisPage;
        }

        /// <summary>The options that apply when a page shows.</summary>
        [Serializable]
        public struct ShowOperationOptions
        {
            /// <summary>The options of the show of this page.</summary>
            [Tooltip("The options of the show of this page.")]
            public Options showThisPage;

            /// <summary>The options of the hide of the other page that hides because this page shows.</summary>
            [Tooltip("The options of the hide of the other page that hides because this page shows.")]
            public Options hideOtherPage;
        }

        /// <summary>The options that apply when a page hides.</summary>
        [Serializable]
        public struct HideOperationOptions
        {
            /// <summary>The options of the hide of this page.</summary>
            [Tooltip("The options of the hide of this page.")]
            public Options hideThisPage;

            /// <summary>The options of the show of the other page that shows because this page hides.</summary>
            [Tooltip("The options of the show of the other page that shows because this page hides.")]
            public Options showOtherPage;
        }

        /// <summary>The transition options of one step, and whether they replace the options of the call.</summary>
        [Serializable]
        public struct Options
        {
            /// <summary>Whether <c>transitionOptions</c> replaces the transition options of the call.</summary>
            [Tooltip("Whether transitionOptions replaces the transition options of the call.")]
            public bool forceUse;

            /// <summary>The transition options to use when <c>forceUse</c> is on.</summary>
            [Tooltip("The transition options to use when forceUse is on.")]
            public PageTransitionOptions transitionOptions;

            /// <summary>Gets the transition options when <c>forceUse</c> is on.</summary>
            /// <returns>The transition options, or none when <c>forceUse</c> is off.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly Option<PageTransitionOptions> GetTransitionOptions()
                => Option.SomeIf(forceUse, transitionOptions);

            /// <summary>Sets the transition options.</summary>
            /// <param name="value">The transition options to set; none keeps the current ones.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetTransitionOptions(Option<PageTransitionOptions> value)
                => transitionOptions = value.GetValueOrDefault(transitionOptions);
        }
    }
}
