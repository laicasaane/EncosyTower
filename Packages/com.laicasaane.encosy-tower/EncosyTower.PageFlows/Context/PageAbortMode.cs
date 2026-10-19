namespace EncosyTower.PageFlows
{
    /// <summary>How the screen goes back when a show or hide is aborted.</summary>
    public enum PageAbortMode
    {
        /// <summary>Run the opposite steps from the current state, with their animations.</summary>
        Reverse,

        /// <summary>
        /// Run the same steps with <see cref="PageTransitionOptions.ZeroDuration"/>, so the screen snaps back.
        /// </summary>
        Immediate,
    }
}
