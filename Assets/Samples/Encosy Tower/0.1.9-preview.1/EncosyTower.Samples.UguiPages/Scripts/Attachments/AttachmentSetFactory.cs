using System;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.PageFlows.UguiPages.Codex;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.Attachments
{
    /// <summary>Creates, places, parks and destroys the three flows of an <see cref="AttachmentSet"/>.</summary>
    /// <remarks>Parked flows live under an inactive object until a screen borrows them.</remarks>
    internal sealed class AttachmentSetFactory
    {
        private const string PARKING = "[Attachments Parking]";

        private readonly UguiPageCodex _codex;
        private readonly RectTransform _parking;
        private readonly Action<PageFlowScope, PageFlowKind> _onFlowCreated;
        private readonly Action<PageFlowScope> _onFlowDropped;

        public AttachmentSetFactory(
              UguiPageCodex codex
            , Action<PageFlowScope, PageFlowKind> onFlowCreated
            , Action<PageFlowScope> onFlowDropped
        )
        {
            _codex = codex;
            _onFlowCreated = onFlowCreated;
            _onFlowDropped = onFlowDropped;

            var parking = new GameObject(PARKING, typeof(RectTransform));

            _parking = (RectTransform)parking.transform;
            _parking.SetParent(parent: codex.transform, worldPositionStays: false);
            parking.SetActive(false);
        }

        public void CreateFlows(AttachmentSet set)
        {
            set.NavigationBarFlow = CreateFlow<UguiPageStackSingle>(
                  set
                , PageAttachments.NAVIGATION_BAR_FLOW
                , PageFlowKind.PageStackSingle
            );

            set.TabsFlow = CreateFlow<UguiPageListSingle>(
                  set
                , PageAttachments.TABS_FLOW
                , PageFlowKind.PageListSingle
            );

            set.CardsFlow = CreateFlow<UguiPageListMulti>(
                  set
                , PageAttachments.CARDS_FLOW
                , PageFlowKind.PageListMulti
            );
        }

        public void Place(AttachmentSet set, RectTransform navigationBarHost, RectTransform detailHost, RectTransform cardsHost)
        {
            Place(set.NavigationBarFlow, navigationBarHost);
            Place(set.TabsFlow, detailHost);
            Place(set.CardsFlow, cardsHost);
        }

        public void Park(AttachmentSet set)
        {
            Park(set.NavigationBarFlow);
            Park(set.TabsFlow);
            Park(set.CardsFlow);
        }

        /// <summary>Destroys the flows of a set and reports their scopes as dropped.</summary>
        public void Release(AttachmentSet set)
        {
            set.InUse = false;

            RaiseFlowsDropped(set);

            Destroy(set.NavigationBarFlow);
            Destroy(set.TabsFlow);
            Destroy(set.CardsFlow);
        }

        /// <summary>Reports the scopes of a set whose flows were already destroyed elsewhere.</summary>
        public void RaiseFlowsDropped(AttachmentSet set)
        {
            var scopes = set.FlowScopes;
            var count = scopes.Count;

            for (var i = 0; i < count; i++)
            {
                _onFlowDropped?.Invoke(scopes[i]);
            }

            scopes.Clear();
        }

        private T CreateFlow<T>(AttachmentSet set, string identifier, PageFlowKind kind)
            where T : UguiPageFlow
        {
            var context = _codex.FlowContext.CloneWithoutOwner();
            context.autoInitializeOnAwake = false;

            var flow = UguiPageFlow.Create<T>(identifier, _parking, context);
            var scope = flow.Context.FlowScope;

            set.FlowScopes.Add(scope);

            _onFlowCreated?.Invoke(scope, kind);
            return flow;
        }

        private static void Place(UguiPageFlow flow, RectTransform host)
        {
            if (flow.IsValid() && host.IsValid())
            {
                flow.RectTransform.FillParent(host);
                flow.PoolTransform.FillParent(host);
            }
        }

        private void Park(UguiPageFlow flow)
        {
            if (flow.IsValid() && _parking.IsValid())
            {
                flow.RectTransform.SetParent(parent: _parking, worldPositionStays: false);
                flow.PoolTransform.SetParent(parent: _parking, worldPositionStays: false);
            }
        }

        private static void Destroy(UguiPageFlow flow)
        {
            if (flow.IsInvalid())
            {
                return;
            }

            if (flow.PoolTransform.IsValid())
            {
                UnityEngine.Object.Destroy(flow.PoolTransform.gameObject);
            }

            UnityEngine.Object.Destroy(flow.gameObject);
        }
    }
}
