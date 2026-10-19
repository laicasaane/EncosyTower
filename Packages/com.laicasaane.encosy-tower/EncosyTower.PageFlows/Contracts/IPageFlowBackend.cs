namespace EncosyTower.PageFlows
{
    /// <summary>Provides the platform-specific operations used by a page flow.</summary>
    public interface IPageFlowBackend
    {
        /// <summary>Enables rendering for a page after preparation and before its show callbacks.</summary>
        /// <param name="page">The page whose rendering the backend activates.</param>
        /// <remarks>
        /// The flow calls this even when show animation is disabled. Preserve transition-controlled opacity.
        /// All page flow operations and callbacks run on the Unity main thread; see <see cref="IPageFlow"/>.
        /// </remarks>
        void ActivatePageRendering(IPage page);
    }
}
