#if UNITY_UGUI

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UguiMultiPageList : UguiPageFlow
    {
        private MultiPageList<IUguiPage> _flow;

        public async UnityTask AddPageAsync(string assetKey, PageContext context, CancellationToken token)
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

            var result = await _flow.AddAsync(identifier.Page, context, token);

            if (result == false)
            {
                ReturnPageToPool(identifier, context);
                return;
            }

            identifier.Transform.SetParent(RectTransform);
        }

        public async UnityTask ShowPageAtIndexAsync(int index, PageContext context, CancellationToken token)
        {
            var identifierOpt = GetPageIdentifierAt(index);

            if (identifierOpt.TryGetValue(out var identifier) == false)
            {
                return;
            }

            identifier.GameObject.SetActive(true);

            var result = await _flow.ShowAsync(index, context, token);

            if (token.IsCancellationRequested || result == false)
            {
                identifier.GameObject.SetActive(false);
            }
        }

        public async UnityTask HidePageAtIndexAsync(int index, PageContext context, CancellationToken token)
        {
            var identifierOpt = GetPageIdentifierAt(index);

            if (identifierOpt.TryGetValue(out var identifier) == false)
            {
                return;
            }

            var result = await _flow.HideAsync(index, context, token);

            if (token.IsCancellationRequested == false && result)
            {
                identifier.GameObject.SetActive(false);
            }
        }

        protected override void OnInitialize(in InitializationContext context)
        {
            _flow = new(Context);

            var subscriber = context.Subscriber.WithState(this);
            AddPageMessage.Async.Subscribe(in subscriber, HandleAsync);
            ShowPageAtIndexMessage.Async.Subscribe(in subscriber, HandleAsync);
            HidePageAtIndexMessage.Async.Subscribe(in subscriber, HandleAsync);

            var processHub = context.ProcessHub.WithState(this);
            IsInTransitionRequest.Register(in processHub, Process);
            GetPageIndexRequest.Register(in processHub, Process);
            GetPageListRequest.Register(in processHub, Process);
            GetPageCollectionRequest.Register(in processHub, Process);
        }

        protected override void OnDispose()
        {
            _flow?.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UguiMultiPageList list
            , AddPageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (AddPageMessage)msg;
            return list.AddPageAsync(message.AssetKey, message.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UguiMultiPageList list
            , ShowPageAtIndexMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (ShowPageAtIndexMessage)msg;
            return list.ShowPageAtIndexAsync(message.Index, message.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UguiMultiPageList list
            , HidePageAtIndexMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (HidePageAtIndexMessage)msg;
            return list.HidePageAtIndexAsync(message.Index, message.Context, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Process(UguiMultiPageList list, IsInTransitionRequest _)
            => list._flow.IsInTransition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Process(UguiMultiPageList list, GetPageIndexRequest req)
            => list._flow.IndexOf(req.Page);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ListFast<IUguiPage>.ReadOnly Process(UguiMultiPageList list, GetPageListRequest _)
            => list._flow.Pages;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyCollection<IUguiPage> Process(UguiMultiPageList list, GetPageCollectionRequest _)
            => list._flow.PageCollection;
    }
}

#endif
