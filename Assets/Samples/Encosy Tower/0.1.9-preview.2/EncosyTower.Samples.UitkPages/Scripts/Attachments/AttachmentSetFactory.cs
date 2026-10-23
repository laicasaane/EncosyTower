using System;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Codex;
using EncosyTower.Samples.UitkPages.State;
using ThrowHelper = EncosyTower.Samples.UitkPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UitkPages.Attachments
{
    /// <summary>Builds the three flows of an <see cref="AttachmentSet"/> and tears them down again.</summary>
    internal sealed class AttachmentSetFactory
    {
        private readonly UitkPageCodex _codex;
        private readonly AppState _state;
        private readonly Action<PageFlowScope, PageFlowKind> _onFlowCreated;
        private readonly Action<PageFlowScope> _onFlowDropped;

        public AttachmentSetFactory(
              UitkPageCodex codex
            , AppState state
            , Action<PageFlowScope, PageFlowKind> onFlowCreated
            , Action<PageFlowScope> onFlowDropped
        )
        {
            _codex = codex;
            _state = state;
            _onFlowCreated = onFlowCreated;
            _onFlowDropped = onFlowDropped;
        }

        public AttachmentSet Create()
        {
            var navigationBar = CreateFlow<UitkPageStackSingle>(
                  PageFlowKind.PageStackSingle
                , PageAttachments.NAVIGATION_BAR_FLOW
            );

            var tabs = CreateFlow<UitkPageListSingle>(PageFlowKind.PageListSingle, PageAttachments.TABS_FLOW);
            var cards = CreateFlow<UitkPageListMulti>(PageFlowKind.PageListMulti, PageAttachments.CARDS_FLOW);

            return new AttachmentSet(_state, navigationBar, tabs, cards);
        }

        public void Release(AttachmentSet set)
        {
            set.NavigationBarFlow.FlowElement.RemoveFromHierarchy();
            set.TabsFlow.FlowElement.RemoveFromHierarchy();
            set.CardsFlow.FlowElement.RemoveFromHierarchy();

            _onFlowDropped?.Invoke(set.NavigationBarScope);
            _onFlowDropped?.Invoke(set.TabsScope);
            _onFlowDropped?.Invoke(set.CardsScope);

            set.Dispose();
        }

        private T CreateFlow<T>(PageFlowKind kind, string identifier)
            where T : UitkPageFlow
        {
            if (UitkPageFlow.TryCreate(kind, out var created) && created is T flow)
            {
                flow.Initialize(
                      identifier: identifier
                    , containerName: null
                    , codex: _codex
                    , context: _codex.FlowContext.CloneWithoutOwner()
                    , behaviourParent: _codex.transform
                );

                _onFlowCreated?.Invoke(flow.Context.FlowScope, kind);
                return flow;
            }

            ThrowHelper.ThrowFlowCreationFailed(identifier);
            return null;
        }
    }
}
