using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using ThrowHelper = EncosyTower.Samples.UguiPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>Calls the page flows by publishing page messages, and reads each result from its receipt.</summary>
    internal sealed class FlowService
    {
        private readonly CodexContext _ctx;

        public FlowService(CodexContext ctx)
        {
            _ctx = ctx;
        }

        public PageContext CreateContext(
              object userData = null
            , PageAbortSource source = null
            , PageCallReceipt receipt = null
            , PageTransitionOptions show = default
            , PageTransitionOptions hide = default
            , int defaultIndex = 0
        )
            => new() {
                AsyncOperation = PageAsyncOperation.Sequential,
                DefaultIndex = defaultIndex,
                ShowOptions = show,
                HideOptions = hide,
                UserData = userData == null ? default : Option.Some(userData),
                Abort = source != null ? source.Handle : default,
                Receipt = receipt ?? new PageCallReceipt(),
            };

        public PublishingContext CreatePublishingContext()
            => PublishingContext.Default(logger: _ctx.Codex.FlowContext.Logger, token: _ctx.Token);

        public async UnityTask<Success<PageFlowError>> ShowAsync(
              PageFlowScope scope
            , AssetKey key
            , PageContext context
        )
        {
            var publisher = _ctx.Codex.FlowContext.Publisher.Scope(scope);

            var published = await ShowPageMessage.Async.Publish(
                  in publisher
                , new ShowPageMessage(key, context)
                , CreatePublishingContext()
            );

            if (published.TryGetFailure(out var error))
            {
                error.Log(_ctx.Codex.FlowContext.Logger);
            }

            return ReadResult(context.Receipt, key.Value);
        }

        public async UnityTask<Success<PageFlowError>> HideActiveAsync(PageFlowScope scope, PageContext context)
        {
            var publisher = _ctx.Codex.FlowContext.Publisher.Scope(scope);

            var published = await HideActivePageMessage.Async.Publish(
                  in publisher
                , new HideActivePageMessage(context)
                , CreatePublishingContext()
            );

            if (published.TryGetFailure(out var error))
            {
                error.Log(_ctx.Codex.FlowContext.Logger);
            }

            return ReadResult(context.Receipt, "HideActive");
        }

        public async UnityTask<Success<PageFlowError>> HideAllAsync(PageFlowScope scope, PageContext context)
        {
            var publisher = _ctx.Codex.FlowContext.Publisher.Scope(scope);

            var published = await HideAllPagesMessage.Async.Publish(
                  in publisher
                , new HideAllPagesMessage(context)
                , CreatePublishingContext()
            );

            if (published.TryGetFailure(out var error))
            {
                error.Log(_ctx.Codex.FlowContext.Logger);
            }

            return ReadResult(context.Receipt, "HideAll");
        }

        public async UnityTask<Success<PageFlowError>> ShowAtAsync(PageFlowScope scope, int index, object userData)
        {
            var context = CreateContext(userData: userData, defaultIndex: -1);
            var publisher = _ctx.Codex.FlowContext.Publisher.Scope(scope);

            var published = await ShowPageAtIndexMessage.Async.Publish(
                  in publisher
                , new ShowPageAtIndexMessage(index, context)
                , CreatePublishingContext()
            );

            if (published.TryGetFailure(out var error))
            {
                error.Log(_ctx.Codex.FlowContext.Logger);
            }

            return ReadResult(context.Receipt, "ShowAt");
        }

        public async UnityTask<Success<PageFlowError>> HideAtAsync(PageFlowScope scope, int index, object userData)
        {
            var context = CreateContext(userData: userData, defaultIndex: -1);
            var publisher = _ctx.Codex.FlowContext.Publisher.Scope(scope);

            var published = await HidePageAtIndexMessage.Async.Publish(
                  in publisher
                , new HidePageAtIndexMessage(index, context)
                , CreatePublishingContext()
            );

            if (published.TryGetFailure(out var error))
            {
                error.Log(_ctx.Codex.FlowContext.Logger);
            }

            return ReadResult(context.Receipt, "HideAt");
        }

        private Success<PageFlowError> ReadResult(PageCallReceipt receipt, string operation)
        {
            if (receipt.IsReported)
            {
                return receipt.Result;
            }

            if (_ctx.Token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            ThrowHelper.LogError_NoReceipt(operation);
            return PageFlowError.Undefined();
        }
    }
}
