namespace EncosyTower.PageFlows
{
    /// <summary>Keeps each page attached to at most one flow at a time.</summary>
    public interface IPageOwnershipTracker
    {
        /// <summary>Claims the page for the flow; false when another flow owns it.</summary>
        /// <param name="page">The page to claim.</param>
        /// <param name="flow">The flow that claims the page.</param>
        /// <param name="owner">The flow that owns the page after the call.</param>
        /// <returns>
        /// <c>true</c> when <paramref name="flow"/> owns the page; <c>false</c> when another flow owns it.
        /// </returns>
        bool TryClaim(IPage page, IPageFlow flow, out IPageFlow owner);

        /// <summary>Releases the page when the flow owns it.</summary>
        /// <param name="page">The page to release.</param>
        /// <param name="flow">The flow that releases the page.</param>
        void Release(IPage page, IPageFlow flow);
    }
}
