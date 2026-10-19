using System.Runtime.CompilerServices;

namespace EncosyTower.PageFlows
{
    /// <summary>The built-in page ownership tracker; share one instance between flows through their contexts.</summary>
    /// <remarks>It holds each page weakly, so a tracked page can still be collected.</remarks>
    public sealed class PageOwnershipTracker : IPageOwnershipTracker
    {
        private readonly ConditionalWeakTable<IPage, IPageFlow> _owners = new();

        /// <inheritdoc/>
        public bool TryClaim(IPage page, IPageFlow flow, out IPageFlow owner)
        {
            if (_owners.TryGetValue(page, out owner))
            {
                return ReferenceEquals(owner, flow);
            }

            _owners.Add(page, flow);
            owner = flow;
            return true;
        }

        /// <inheritdoc/>
        public void Release(IPage page, IPageFlow flow)
        {
            if (_owners.TryGetValue(page, out var owner) && ReferenceEquals(owner, flow))
            {
                _owners.Remove(page);
            }
        }
    }
}
