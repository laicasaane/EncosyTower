namespace EncosyTower.PageFlows
{
    /// <summary>What an IPageTransition member is called for.</summary>
    public readonly record struct PageTransitionContext
    {
        /// <summary>Show or Hide: which side this call is for.</summary>
        public PageTransition Transition { get; init; }

        /// <summary>The transition options of the page that shows.</summary>
        public PageTransitionOptions ShowOptions { get; init; }

        /// <summary>The transition options of the page that hides.</summary>
        public PageTransitionOptions HideOptions { get; init; }

        /// <summary>The handle of this call, with this transition as its origin.</summary>
        public PageAbortHandle Abort { get; init; }
    }
}
