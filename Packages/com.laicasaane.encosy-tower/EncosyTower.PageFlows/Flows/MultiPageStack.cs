using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Tasks;

using ETDBG = EncosyTower.Debugging;

namespace EncosyTower.PageFlows
{
    public sealed class MultiPageStack<TPage> : IMultiPageStack<TPage>, IDisposable
        where TPage : class, IPage
    {
        private readonly ConcurrentStack<TPage> _pages = new();
        private readonly ILogger _logger;
        private readonly PageFlow _flow;

        public MultiPageStack([NotNull] IPageFlowContext context)
        {
            ETDBG.ThrowHelper.ThrowIfNull(context);

            _logger = context.Logger ?? DevLogger.Default;
            _flow = new PageFlow(
                  context.TaskArrayPool
                , context.Subscriber
                , context.Publisher
                , context.FlowScope
                , context.FlowScopeCollectionApplier
                , context.WarnNoSubscriber
                , _logger
            );
        }

        public Option<TPage> CurrentPage => Option.SomeIf(_pages.TryPeek(out var page), page);

        public IReadOnlyCollection<TPage> PageCollection => _pages;

        public bool IsInTransition { get; private set; }

        public void Dispose()
        {
            _pages.Clear();
        }

        public async UnityTask<bool> PopAsync(PageContext context, CancellationToken token)
        {
            if (_pages.Count < 1)
            {
                _flow.LogWarningNoPageToPop();
                return default;
            }

            if (token.IsCancellationRequested)
            {
                return default;
            }

            var validation = await Validator.ValidateTransitionAsync(this, _logger, context.AsyncOperation, token);

            if (validation == false || token.IsCancellationRequested)
            {
                return default;
            }

            IsInTransition = true;

            _pages.TryPop(out var pageToHide);
            _pages.TryPeek(out var pageToShow);

            var result = await _flow.TransitionAsync(PageTransition.Hide, pageToHide, pageToShow, context, token);

            if (result)
            {
                result = await _flow.DetachAsync(this, pageToHide, context, token);
            }

            if (result == false && pageToHide is not null)
            {
                _pages.Push(pageToHide);
            }

            IsInTransition = false;
            return result;
        }

        public async UnityTask<bool> PushAsync([NotNull] TPage page, PageContext context, CancellationToken token)
        {
            ETDBG.ThrowHelper.ThrowIfNullOrUnityObjectInvalid(page);

            if (token.IsCancellationRequested)
            {
                return default;
            }

            var validation = await Validator.ValidateTransitionAsync(this, _logger, context.AsyncOperation, token);

            if (validation == false || token.IsCancellationRequested)
            {
                return default;
            }

            IsInTransition = true;

            _pages.TryPeek(out var pageToHide);
            _pages.Push(page);

            var result = await _flow.AttachAsync(this, page, context, token);

            if (result)
            {
                result = await _flow.TransitionAsync(PageTransition.Show, pageToHide, page, context, token);
            }

            if (result == false)
            {
                _pages.TryPop(out _);
            }

            IsInTransition = false;
            return result;
        }

        public async UnityTask<bool> PushAsync(
              [NotNull] Func<CancellationToken, UnityTask<TPage>> factory
            , PageContext context
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNull(factory);

            if (token.IsCancellationRequested)
            {
                return false;
            }

            var page = await factory(token);
            return await PushAsync(page, context, token);
        }

        public async UnityTask<bool> RemoveAllAsync(PageContext context, CancellationToken token)
        {
            var pages = _pages;
            var count = pages.Count;

            if (count < 1)
            {
                _flow.LogWarningNoPageToRemove();
                return false;
            }

            if (token.IsCancellationRequested)
            {
                return false;
            }

            var self = this;
            var flow = _flow;
            var taskArrayPool = flow.TaskArrayPool;
            var flowTasks = taskArrayPool.Rent(count);
            var pageTasks = taskArrayPool.Rent(count);
            var index = 0;
            var result = true;

            try
            {
                while (pages.TryPop(out var page))
                {
                    flowTasks[index] = flow.PublishDetachAsync(self, page, token).AsUnityTask();
                    pageTasks[index] = (page as IPageOnDetachFromFlowAsync)
                        ?.OnDetachFromFlowAsync(self, context, token).AsUnityTask()
                        ?? UnityTask.CompletedTask;

                    index += 1;
                }

                if (token.IsCancellationRequested == false)
                {
                    await UnityTask.WhenAll(flowTasks);
                }

                if (token.IsCancellationRequested == false)
                {
                    await UnityTask.WhenAll(pageTasks);
                }
            }
            catch (Exception ex)
            {
                result = false;
                flow.Logger.LogException(ex);
            }
            finally
            {
                taskArrayPool.Return(flowTasks, true);
                taskArrayPool.Return(pageTasks, true);
            }

            if (token.IsCancellationRequested || result == false)
            {
                return false;
            }

            _pages.Clear();
            return true;
        }
    }
}
