using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.PageFlows.UitkPages
{
    public abstract class UitkPageElement<TFlowScopeCollection> : UitkPageElement
        , IPageNeedsFlowScopeCollection<TFlowScopeCollection>
        where TFlowScopeCollection : struct, IPageFlowScopeCollection
    {
        private Option<TFlowScopeCollection> _flowScopeCollection;

        public Option<TFlowScopeCollection> FlowScopeCollection
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _flowScopeCollection;
        }

        Option<TFlowScopeCollection> IPageNeedsFlowScopeCollection<TFlowScopeCollection>.FlowScopeCollection
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _flowScopeCollection = value;
        }
    }
}
