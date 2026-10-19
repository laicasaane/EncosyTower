namespace EncosyTower.PageFlows
{
    /// <summary>How far the screen got before a page flow operation ended.</summary>
    public enum PageTransitionState
    {
        /// <summary>The operation ended before the transition start: the screen never changed.</summary>
        NotStarted,

        /// <summary>The transition ran to its end state.</summary>
        Completed,

        /// <summary>The transition started, then an abort took the screen back.</summary>
        Undone,
    }
}
