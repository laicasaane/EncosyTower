#if UNITY_UGUI

using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.PageFlows.UguiPages
{
    /// <summary>A <see cref="UguiPageBehaviour"/> that also receives the scope collection of its codex.</summary>
    /// <typeparam name="TFlowScopeCollection">The scope collection struct of the codex.</typeparam>
    public abstract class UguiPageBehaviour<TFlowScopeCollection> : UguiPageBehaviour
        , IPageNeedsFlowScopeCollection<TFlowScopeCollection>
        where TFlowScopeCollection : struct, IPageFlowScopeCollection
    {
        private Option<TFlowScopeCollection> _flowScopeCollection;

        /// <summary>Gets the scope collection of the codex, set when the page attaches.</summary>
        /// <remarks>It is not cleared on detach; the old value stays until the next attach sets it.</remarks>
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

#endif
