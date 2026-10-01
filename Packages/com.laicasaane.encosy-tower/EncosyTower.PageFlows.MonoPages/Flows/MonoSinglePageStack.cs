using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class MonoSinglePageStack : MonoPageFlow
    {
        private SinglePageStack<IMonoPage> _flow;

        public async UnityTask ShowPageAsync(string assetKey, PageContext context, CancellationToken token)
        {
            var pageKey = MakePageKey(assetKey);

            if (token.IsCancellationRequested)
            {
                return;
            }

            var identifierOpt = await RentPageAsync(pageKey, context, token);

            if (identifierOpt.TryGetValue(out var identifier) == false)
            {
                return;
            }

            if (token.IsCancellationRequested)
            {
                ReturnPageToPool(identifier, context);
                return;
            }

            identifier.GameObject.SetActive(true);
            identifier.Transform.SetParent(RectTransform);

            var oldPageOpt = _flow.CurrentPage;
            var result = await _flow.PushAsync(identifier.Page, context, token);

            if (token.IsCancellationRequested || result == false)
            {
                ReturnPageToPool(identifier, context);
                return;
            }

            if (oldPageOpt.TryGetValue(out var oldPage))
            {
                ReturnPageToPool(oldPage, context);
            }
        }

        public async UnityTask HideActivePageAsync(PageContext context, CancellationToken token)
        {
            var pageOpt = _flow.CurrentPage;

            if (pageOpt.TryGetValue(out var page) == false)
            {
                return;
            }

            var result = await _flow.PopAsync(context, token);

            if (token.IsCancellationRequested == false && result)
            {
                ReturnPageToPool(page, context);
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
              MonoSinglePageStack stack
            , ShowPageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (ShowPageMessage)msg;
            return stack.ShowPageAsync(message.AssetKey, message.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              MonoSinglePageStack stack
            , HideActivePageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (HideActivePageMessage)msg;
            return stack.HideActivePageAsync(message.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Process(MonoSinglePageStack stack, IsInTransitionRequest _)
            => stack._flow.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<IMonoPage> Process(MonoSinglePageStack stack, GetCurrentPageRequest _)
            => stack._flow.CurrentPage;
    }
}
