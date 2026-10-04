using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows.UitkPages
{
    public sealed class UitkSinglePageList : UitkPageFlow
    {
        private SinglePageList<IUitkPage> _flow;

        public override bool IsInTransition => _flow != null && _flow.IsInTransition;

        public Option<IUitkPage> CurrentPage => _flow.CurrentPage;

        public ListFast<IUitkPage>.ReadOnly Pages => _flow.Pages;

        public async UnityTask AddPageAsync(string assetKey, PageContext context, CancellationToken token)
        {
            var slotOpt = await RentPageAsync(assetKey, context, token);

            if (slotOpt.TryGetValue(out var slot) == false)
            {
                return;
            }

            if (token.IsCancellationRequested)
            {
                ReturnPage(slot.Page, context);
                return;
            }

            var result = await _flow.AddAsync(slot.Page, context, token);

            if (result == false)
            {
                ReturnPage(slot.Page, context);
            }
        }

        public async UnityTask ShowPageAtIndexAsync(int index, PageContext context, CancellationToken token)
        {
            if (TryGetSlotAt(index, out var slot) == false)
            {
                return;
            }

            var hasOldPage = _flow.CurrentPage.TryGetValue(out var oldPage);

            ShowSlot(slot);
            BeginBlocking();

            bool result;

            try
            {
                result = await _flow.ShowAsync(index, context, token);
            }
            finally
            {
                EndBlocking();
            }

            if (token.IsCancellationRequested || result == false)
            {
                HideSlot(slot);
                return;
            }

            if (hasOldPage && oldPage != slot.Page && TryGetSlot(oldPage, out var oldSlot))
            {
                HideSlot(oldSlot);
            }

            ApplyFocus(slot);
        }

        public async UnityTask HideActivePageAsync(PageContext context, CancellationToken token)
        {
            if (_flow.CurrentPage.TryGetValue(out var page) == false)
            {
                return;
            }

            BeginBlocking();

            bool result;

            try
            {
                result = await _flow.HideAsync(context, token);
            }
            finally
            {
                EndBlocking();
            }

            if (token.IsCancellationRequested == false && result && TryGetSlot(page, out var slot))
            {
                HideSlot(slot);
            }
        }

        protected override void OnInitialize(in InitializationContext context)
        {
            _flow = new(Context);

            var subscriber = context.Subscriber.WithState(this);
            AddPageMessage.Async.Subscribe(in subscriber, HandleAsync);
            ShowPageAtIndexMessage.Async.Subscribe(in subscriber, HandleAsync);
            HideActivePageMessage.Async.Subscribe(in subscriber, HandleAsync);

            var processHub = context.ProcessHub.WithState(this);
            IsInTransitionRequest.Register(in processHub, Process);
            GetPageIndexRequest.Register(in processHub, Process);
            GetCurrentPageRequest.Register(in processHub, Process);
            GetPageListRequest.Register(in processHub, Process);
            GetPageCollectionRequest.Register(in processHub, Process);
        }

        protected override void OnDispose()
        {
            _flow?.Dispose();
        }

        private bool TryGetSlotAt(int index, out UitkPageSlot slot)
        {
            var pages = _flow.Pages;

            if ((uint)index < (uint)pages.Count)
            {
                return TryGetSlot(pages[index], out slot);
            }

            slot = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkSinglePageList list
            , AddPageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (AddPageMessage)msg;
            return list.AddPageAsync(message.AssetKey, message.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkSinglePageList list
            , ShowPageAtIndexMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (ShowPageAtIndexMessage)msg;
            return list.ShowPageAtIndexAsync(message.Index, message.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkSinglePageList list
            , HideActivePageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (HideActivePageMessage)msg;
            return list.HideActivePageAsync(message.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Process(UitkSinglePageList list, IsInTransitionRequest _)
            => list.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Process(UitkSinglePageList list, GetPageIndexRequest request)
            => list._flow.IndexOf(request.Page);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<IUitkPage> Process(UitkSinglePageList list, GetCurrentPageRequest _)
            => list._flow.CurrentPage;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ListFast<IUitkPage>.ReadOnly Process(UitkSinglePageList list, GetPageListRequest _)
            => list._flow.Pages;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyCollection<IUitkPage> Process(UitkSinglePageList list, GetPageCollectionRequest _)
            => list._flow.PageCollection;
    }
}
