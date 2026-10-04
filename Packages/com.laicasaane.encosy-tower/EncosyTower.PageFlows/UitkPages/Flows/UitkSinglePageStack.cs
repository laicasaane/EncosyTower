using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows.UitkPages
{
    public sealed class UitkSinglePageStack : UitkPageFlow
    {
        private SinglePageStack<IUitkPage> _flow;

        public override bool IsInTransition => _flow != null && _flow.IsInTransition;

        public Option<IUitkPage> CurrentPage => _flow.CurrentPage;

        public async UnityTask ShowPageAsync(string assetKey, PageContext context, CancellationToken token)
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

            var hasOldPage = _flow.CurrentPage.TryGetValue(out var oldPage);

            if (hasOldPage == false)
            {
                RecordFocus();
            }

            ShowSlot(slot);
            BeginBlocking();

            bool result;

            try
            {
                result = await _flow.PushAsync(slot.Page, context, token);
            }
            finally
            {
                EndBlocking();
            }

            if (token.IsCancellationRequested || result == false)
            {
                ReturnPage(slot.Page, context);
                return;
            }

            if (hasOldPage)
            {
                ReturnPage(oldPage, context);
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
                result = await _flow.PopAsync(context, token);
            }
            finally
            {
                EndBlocking();
            }

            if (token.IsCancellationRequested == false && result)
            {
                ReturnPage(page, context);
                RestoreFocus();
            }
        }

        protected override void OnInitialize(in InitializationContext context)
        {
            _flow = new(Context);

            var subscriber = context.Subscriber.WithState(this);
            ShowPageMessage.Async.Subscribe(in subscriber, HandleAsync);
            HideActivePageMessage.Async.Subscribe(in subscriber, HandleAsync);

            var processHub = context.ProcessHub.WithState(this);
            IsInTransitionRequest.Register(in processHub, Process);
            GetCurrentPageRequest.Register(in processHub, Process);
        }

        protected override void OnDispose()
        {
            _flow?.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkSinglePageStack stack
            , ShowPageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (ShowPageMessage)msg;
            return stack.ShowPageAsync(message.AssetKey, message.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UitkSinglePageStack stack
            , HideActivePageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (HideActivePageMessage)msg;
            return stack.HideActivePageAsync(message.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Process(UitkSinglePageStack stack, IsInTransitionRequest _)
            => stack.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<IUitkPage> Process(UitkSinglePageStack stack, GetCurrentPageRequest _)
            => stack._flow.CurrentPage;
    }
}
