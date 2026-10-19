#if UNITY_EDITOR || DEVELOPMENT_BUILD
#if ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO_DEV
#define __PUBSUB_INCLUDE_CALLER_INFO__
#endif
#endif

#if ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO
#define __PUBSUB_INCLUDE_CALLER_INFO__
#endif

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Internals;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PubSub;
using EncosyTower.Tasks;

using ETDBG = EncosyTower.Debugging;

namespace EncosyTower.PageFlows.Flows
{
    /// <summary>Runs the attach, detach and transition steps that the page stacks and page lists share.</summary>
    /// <remarks>
    /// A stack or a list owns one <see cref="PageFlow"/>. Use it directly only to build a custom page flow type.
    /// All page flow operations and callbacks run on the Unity main thread; see <see cref="IPageFlow"/>.
    /// </remarks>
    public sealed class PageFlow
    {
        private readonly MessageSubscriber _subscriber;
        private readonly MessagePublisher _publisher;
        private readonly ILogger _logger;
        private readonly PageFlowScope _flowScope;
        private readonly Option<IPageFlowScopeCollectionApplier> _flowScopeCollectionApplier;
        private readonly Option<IPageOwnershipTracker> _ownershipTracker;
        private readonly IPageFlowBackend _backend;
        private readonly bool _warnNoSubscriber;

        /// <summary>Creates the shared steps of a page flow.</summary>
        /// <param name="subscriber">The subscriber the flow and its pages receive messages from.</param>
        /// <param name="publisher">The publisher the flow publishes its messages with.</param>
        /// <param name="flowScope">The scope the flow publishes and receives its messages in.</param>
        /// <param name="flowScopeCollectionApplier">The applier that sets the scope collection of pages.</param>
        /// <param name="ownershipTracker">The tracker that keeps a page attached to one flow at a time, if any.</param>
        /// <param name="warnNoSubscriber">
        /// Whether a message that no subscriber received is added to the result of the call. See
        /// <see cref="IPageFlowContext.WarnNoSubscriber"/>.
        /// </param>
        /// <param name="logger">The logger the flow uses.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        public PageFlow(
              MessageSubscriber subscriber
            , MessagePublisher publisher
            , PageFlowScope flowScope
            , Option<IPageFlowScopeCollectionApplier> flowScopeCollectionApplier
            , Option<IPageOwnershipTracker> ownershipTracker
            , bool warnNoSubscriber
            , [NotNull] ILogger logger
        ) : this(
              subscriber
            , publisher
            , flowScope
            , flowScopeCollectionApplier
            , ownershipTracker
            , warnNoSubscriber
            , logger
            , backend: null
        )
        {
        }

        /// <summary>Creates the shared steps of a page flow.</summary>
        /// <param name="subscriber">The subscriber the flow and its pages receive messages from.</param>
        /// <param name="publisher">The publisher the flow publishes its messages with.</param>
        /// <param name="flowScope">The scope the flow publishes and receives its messages in.</param>
        /// <param name="flowScopeCollectionApplier">The applier that sets the scope collection of pages.</param>
        /// <param name="ownershipTracker">The tracker that keeps a page attached to one flow at a time, if any.</param>
        /// <param name="warnNoSubscriber">
        /// Whether a message that no subscriber received is added to the result of the call. See
        /// <see cref="IPageFlowContext.WarnNoSubscriber"/>.
        /// </param>
        /// <param name="logger">The logger the flow uses.</param>
        /// <param name="backend">The platform backend, or <c>null</c> for no rendering integration.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        public PageFlow(
              MessageSubscriber subscriber
            , MessagePublisher publisher
            , PageFlowScope flowScope
            , Option<IPageFlowScopeCollectionApplier> flowScopeCollectionApplier
            , Option<IPageOwnershipTracker> ownershipTracker
            , bool warnNoSubscriber
            , [NotNull] ILogger logger
            , IPageFlowBackend backend
        )
        {
            ETDBG.ThrowHelper.ThrowIfNull(logger);

            _subscriber = subscriber;
            _publisher = publisher;
            _logger = logger;
            _flowScope = flowScope;
            _flowScopeCollectionApplier = flowScopeCollectionApplier;
            _ownershipTracker = ownershipTracker;
            _backend = backend;
            _warnNoSubscriber = warnNoSubscriber;
        }

        internal bool HasTransitionBegun { get; private set; }

        /// <summary>Attaches a page to a flow.</summary>
        /// <param name="flow">The flow the page attaches to.</param>
        /// <param name="page">The page to attach.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the page is attached; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The flow claims the page with its ownership tracker, sets the values of the <c>IPageNeeds*</c> interfaces,
        /// publishes <see cref="EncosyTower.PageFlows.PubSub.AttachPageMessage"/>, then calls
        /// <see cref="IPageOnAttachToFlowAsync.OnAttachToFlowAsync"/>.
        /// </para>
        /// <para>
        /// When a step fails or is aborted, the flow undoes the attach: it releases the claim, clears the flow scope,
        /// the subscriber and the publisher it set, and detaches the page if the attach had already been published.
        /// Errors from the undo are kept with the failure as suppressed errors.
        /// </para>
        /// <para>
        /// A token that is already cancelled returns the <c>Cancelled</c> case. A cancellation during the attach
        /// throws the <see cref="System.OperationCanceledException"/> after the undo.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="page"/> is <c>null</c>.</exception>
        public async UnityTask<Success<PageFlowError>> AttachAsync(
              IPageFlow flow
            , IPage page
            , PageContext context
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNull(page);

            var abort = context.Abort._state;
            var stepToken = abort?.GetCallbackToken(token) ?? token;

            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            if (TryClaim(flow, page) == false)
            {
                return PageFlowError.PageOwnedByAnotherFlow(page);
            }

            var attachPublished = false;
            var attachAccepted = false;
            ExceptionDispatchInfo cancelled = null;
            Success<PageFlowError> result;

            try
            {
                InitializeIPageNeedsInterfaces(page, this);

                var publisher = _publisher.Scope(_flowScope);
                Success<PublishError> published;

                try
                {
                    published = await AttachPageMessage.Async.Publish(
                          in publisher
                        , new AttachPageMessage(flow, page, token)
                        , GetPublishingContext(token)
                    );
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    attachPublished = true;
                    throw;
                }

                var publishFailed = TryGetPublishFailure<AttachPageMessage>(in published, abort, out var publishError);

                attachPublished = publishFailed == false;

                if (publishFailed)
                {
                    result = PageFlowError.FromPublish(publishError);
                }
                else if (IsAbortRequested(abort))
                {
                    result = PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode);
                }
                else if (token.IsCancellationRequested)
                {
                    result = PageFlowError.Cancelled();
                }
                else if (page is IPageOnAttachToFlowAsync onAttach
                    && await onAttach.OnAttachToFlowAsync(flow, WithPageOrigin(context, page), stepToken) == false
                )
                {
                    result = IsAbortRequested(abort)
                        ? PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode)
                        : PageFlowError.AttachRejected(page);
                }
                else if (IsAbortRequested(abort))
                {
                    attachAccepted = true;
                    result = PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode);
                }
                else
                {
                    return Success.Yes;
                }
            }
            catch (OperationCanceledException ex)
            {
                if (attachPublished && IsAbortRequested(abort) && token.IsCancellationRequested == false)
                {
                    // The abort's own cancellation.
                    result = PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode);
                }
                else
                {
                    cancelled = ExceptionDispatchInfo.Capture(ex);
                    result = PageFlowError.Cancelled();
                }
            }
            catch (Exception ex)
            {
                if (attachPublished && IsAbortRequested(abort))
                {
                    // The page threw after the abort was accepted: the result is Aborted, carrying the exception.
                    result = PageFlowError
                        .Aborted(abort.RequestOrigin, abort.RequestMode)
                        .WithSuppressed(PageFlowError.Exception(ex));
                }
                else
                {
                    result = PageFlowError.Exception(ex);
                }
            }

            var error = result.GetFailureOrThrow();

            if (error.IsAborted == false)
            {
                abort?.BeginRecovering(error);
            }

            try
            {
                if (attachAccepted)
                {
                    var detach = await ForceDetachAsync(flow, page, context, token);

                    if (detach.TryGetFailure(out var detachError))
                    {
                        result = error.WithSuppressed(detachError);
                    }
                }
                else if (attachPublished)
                {
                    var undo = await UndoAttachAsync(flow, page, abort, token);

                    if (undo.TryGetFailure(out var undoError))
                    {
                        result = error.WithSuppressed(undoError);
                    }
                }
                else
                {
                    DeinitializePage(flow, page);
                }
            }
            catch (OperationCanceledException) when (error.IsAborted && token.IsCancellationRequested == false)
            {
                // Only the abort token is cancelled: the cancellation is part of the abort, the result stays Aborted.
            }

            if (cancelled != null)
            {
                SetAsideSuppressed(abort, result);
                cancelled.Throw();
            }

            return result;
        }

        /// <summary>Detaches a page from a flow.</summary>
        /// <param name="flow">The flow the page detaches from.</param>
        /// <param name="page">The page to detach.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the page is detached; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The flow calls <see cref="IPageOnDetachFromFlowAsync.OnDetachFromFlowAsync"/>; when it returns
        /// <c>false</c>, the page stays attached. Then the flow publishes
        /// <see cref="EncosyTower.PageFlows.PubSub.DetachPageMessage"/>, clears the flow scope, the subscriber and the
        /// publisher of the page, and releases the ownership claim. The scope collection of the codex stays on the
        /// page.
        /// </para>
        /// <para>
        /// A token that is already cancelled returns the <c>Cancelled</c> case. A cancellation while the message is
        /// published clears the page and then throws the <see cref="System.OperationCanceledException"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="page"/> is <c>null</c>.</exception>
        public async UnityTask<Success<PageFlowError>> DetachAsync(
              IPageFlow flow
            , IPage page
            , PageContext context
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNull(page);

            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            try
            {
                if (page is IPageOnDetachFromFlowAsync onDetach)
                {
                    var detachResult = await onDetach.OnDetachFromFlowAsync(flow, WithPageOrigin(context, page), token);

                    if (detachResult == false)
                    {
                        return PageFlowError.DetachRejected(page);
                    }
                }

                var publisher = _publisher.Scope(_flowScope);
                Success<PublishError> published;

                try
                {
                    published = await DetachPageMessage.Async.Publish(
                          in publisher
                        , new DetachPageMessage(flow, page, token)
                        , GetPublishingContext(token)
                    );
                }
                catch (OperationCanceledException)
                {
                    DeinitializePage(flow, page);
                    throw;
                }

                if (TryGetPublishFailure<DetachPageMessage>(in published, context.Abort._state, out var publishError))
                {
                    return PageFlowError.FromPublish(publishError);
                }

                DeinitializePage(flow, page);
                return Success.Yes;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return PageFlowError.Exception(ex);
            }
        }

        internal async UnityTask<Success<PageFlowError>> ForceDetachAsync(
              IPageFlow flow
            , IPage page
            , PageContext context
            , CancellationToken token
        )
        {
            Success<PageFlowError> result = Success.Yes;
            ExceptionDispatchInfo cancelled = null;

            if (page is IPageOnDetachFromFlowAsync onDetach)
            {
                try
                {
                    var detachResult = await onDetach.OnDetachFromFlowAsync(flow, WithPageOrigin(context, page), token);

                    if (detachResult == false)
                    {
                        result = PageFlowError.DetachRejected(page);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    cancelled = ExceptionDispatchInfo.Capture(ex);
                }
                catch (Exception ex)
                {
                    result = PageFlowError.Exception(ex);
                }
            }

            try
            {
                var publisher = _publisher.Scope(_flowScope);

                var published = await DetachPageMessage.Async.Publish(
                      in publisher
                    , new DetachPageMessage(flow, page, token)
                    , GetPublishingContext(CancellationToken.None)
                );

                if (TryGetPublishFailure<DetachPageMessage>(in published, context.Abort._state, out var publishError))
                {
                    result = result.TryGetFailure(out var detachError)
                        ? detachError.WithSuppressed(PageFlowError.FromPublish(publishError))
                        : PageFlowError.FromPublish(publishError);
                }
            }
            catch (OperationCanceledException ex)
            {
                cancelled ??= ExceptionDispatchInfo.Capture(ex);
            }
            catch (Exception ex)
            {
                if (result.TryGetFailure(out var detachError))
                {
                    result = detachError.WithSuppressed(PageFlowError.Exception(ex));
                }
                else
                {
                    result = PageFlowError.Exception(ex);
                }
            }

            DeinitializePage(flow, page);

            if (cancelled != null)
            {
                if (result.TryGetFailure(out var droppedError))
                {
                    SetAside(context.Abort._state, droppedError);
                }

                cancelled.Throw();
            }

            return result;
        }

        internal async UnityTask ForceDetachOnThrowAsync(
              IPageFlow flow
            , IPage page
            , PageContext context
            , CancellationToken token
        )
        {
            try
            {
                var result = await ForceDetachAsync(flow, page, context, token);

                if (result.TryGetFailure(out var error))
                {
                    error.Log(_logger);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogException(ex);
            }
        }

        internal async UnityTask DetachAllOnThrowAsync(
              IPageFlow flow
            , IPage[] pages
            , int count
            , PageContext context
            , CancellationToken token
        )
        {
            try
            {
                var result = await DetachAllAsync(flow, pages, count, context, token);

                if (result.TryGetFailure(out var error))
                {
                    error.Log(_logger);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogException(ex);
            }
        }

        internal async UnityTask<Success<PageFlowError>> ForceDetachInAbortAsync(
              IPageFlow flow
            , IPage page
            , PageContext context
            , CancellationToken token
        )
        {
            try
            {
                return await ForceDetachAsync(flow, page, context, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested == false)
            {
                // Only the abort token is cancelled: the cancellation is part of the abort, the result stays Aborted.
                return Success.Yes;
            }
        }

        internal UnityTask<Success<PageFlowError>> DetachAllAsync(
              IPageFlow flow
            , IPage[] pages
            , PageContext context
            , CancellationToken token
        )
            => DetachAllAsync(flow, pages, pages.Length, context, token);

        internal async UnityTask<Success<PageFlowError>> DetachAllAsync(
              IPageFlow flow
            , IPage[] pages
            , int count
            , PageContext context
            , CancellationToken token
        )
        {
            var messageTaskPool = ArrayPool<UnityTask<Success<PublishError>>>.Shared;
            var messageTasks = messageTaskPool.Rent(count);
            var hookTaskPool = ArrayPool<UnityTask<bool>>.Shared;
            var hookTasks = hookTaskPool.Rent(count);
            var publisher = _publisher.Scope(_flowScope);
            var publishingContext = GetPublishingContext(CancellationToken.None);
            var abort = context.Abort._state;
            Success<PageFlowError> first = Success.Yes;
            List<PageFlowError> suppressed = null;
            ExceptionDispatchInfo cancelled = null;

            try
            {
                for (var index = 0; index < count; index++)
                {
                    var page = pages[index];
                    messageTasks[index] = UnityTask.FromResult<Success<PublishError>>(Success.Yes);
                    hookTasks[index] = UnityTask.FromResult(true);

                    try
                    {
                        messageTasks[index] = DetachPageMessage.Async.Publish(
                              in publisher
                            , new DetachPageMessage(flow, page, token)
                            , publishingContext
                        );
                    }
                    catch (Exception ex)
                    {
                        RecordDetachException(ex, ref first, ref suppressed, ref cancelled);
                    }

                    try
                    {
                        if (page is IPageOnDetachFromFlowAsync onDetach)
                        {
                            hookTasks[index] = onDetach.OnDetachFromFlowAsync(
                                  flow
                                , WithPageOrigin(context, page)
                                , token
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        RecordDetachException(ex, ref first, ref suppressed, ref cancelled);
                    }
                }

                for (var index = 0; index < count; index++)
                {
                    try
                    {
                        var published = await messageTasks[index];

                        if (TryGetPublishFailure<DetachPageMessage>(in published, abort, out var publishError))
                        {
                            RecordDetachFailure(PageFlowError.FromPublish(publishError), ref first, ref suppressed);
                        }
                    }
                    catch (Exception ex)
                    {
                        RecordDetachException(ex, ref first, ref suppressed, ref cancelled);
                    }
                }

                for (var index = 0; index < count; index++)
                {
                    try
                    {
                        if (await hookTasks[index] == false)
                        {
                            RecordDetachFailure(PageFlowError.DetachRejected(pages[index]), ref first, ref suppressed);
                        }
                    }
                    catch (Exception ex)
                    {
                        RecordDetachException(ex, ref first, ref suppressed, ref cancelled);
                    }
                }
            }
            finally
            {
                messageTaskPool.Return(array: messageTasks, clearArray: true);
                hookTaskPool.Return(array: hookTasks, clearArray: true);
            }

            for (var index = 0; index < count; index++)
            {
                DeinitializePage(flow, pages[index]);
            }

            Success<PageFlowError> result = Success.Yes;

            if (first.TryGetFailure(out var firstError))
            {
                result = WithSuppressedErrors(
                      firstError.WithTransitionState(PageTransitionState.Completed)
                    , suppressed
                );
            }

            if (cancelled != null)
            {
                if (result.TryGetFailure(out var droppedError))
                {
                    SetAside(abort, droppedError);
                }

                cancelled.Throw();
            }

            return result;
        }

        private static void RecordDetachException(
              Exception ex
            , ref Success<PageFlowError> first
            , ref List<PageFlowError> suppressed
            , ref ExceptionDispatchInfo cancelled
        )
        {
            if (ex is OperationCanceledException)
            {
                cancelled ??= ExceptionDispatchInfo.Capture(ex);
                return;
            }

            RecordDetachFailure(PageFlowError.Exception(ex), ref first, ref suppressed);
        }

        private static void RecordDetachFailure(
              in PageFlowError error
            , ref Success<PageFlowError> first
            , ref List<PageFlowError> suppressed
        )
        {
            if (first.IsSuccess)
            {
                first = error;
                return;
            }

            suppressed ??= new List<PageFlowError>();
            suppressed.Add(error);
        }

        internal static void PublishAndForget<TMessage>(
              MessagePublisher.Publisher<PageFlowScope> publisher
            , TMessage message
            , ILogger logger
            , Option<PageFlowError> unansweredError
        )
            where TMessage : IMessage
        {
            PublishAndObserveAsync(publisher, message, logger, unansweredError).Forget();
        }

        private static async UnityTask PublishAndObserveAsync<TMessage>(
              MessagePublisher.Publisher<PageFlowScope> publisher
            , TMessage message
            , ILogger logger
            , Option<PageFlowError> unansweredError
        )
            where TMessage : IMessage
        {
            try
            {
                var published = await publisher.PublishAsync(message, PublishingContext.Default(logger: logger));

                if (published.TryGetFailure(out var publishError) == false)
                {
                    return;
                }

                if (publishError.IsSubscriberNotFound == false)
                {
                    publishError.Log(logger);
                }
                else if (unansweredError.TryGetValue(out var error))
                {
                    error.Log(logger);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
        }

        internal static void ReleaseOnThrow<TPage>(TPage page, Action<TPage> release, ILogger logger)
        {
            try
            {
                release(page);
            }
            catch (Exception inner)
            {
                logger.LogException(inner);
            }
        }

        internal static PageFlowError ReleaseOnFailure<TPage>(in PageFlowError error, TPage page, Action<TPage> release)
        {
            try
            {
                release(page);
            }
            catch (Exception inner)
            {
                return WithSuppressedFailure(error, PageFlowError.Exception(inner));
            }

            return error;
        }

        internal static PageFlowError WithSuppressedFailure(PageFlowError error, in Success<PageFlowError> suppressed)
        {
            if (suppressed.TryGetFailure(out var failure))
            {
                return error.WithSuppressed(failure);
            }

            return error;
        }

        internal static Success<PageFlowError> MergeSoft(
              in Success<PageFlowError> result
            , IReadOnlyList<PageFlowError> soft
        )
        {
            var count = soft?.Count ?? 0;

            if (count < 1)
            {
                return result;
            }

            var start = 0;

            if (result.TryGetFailure(out var error) == false)
            {
                error = soft[0].WithTransitionState(PageTransitionState.Completed);
                start = 1;
            }

            for (var i = start; i < count; i++)
            {
                error = error.WithSuppressed(soft[i]);
            }

            return error;
        }

        internal static Success<PageFlowError> MergeSoft(in Success<PageFlowError> result, in PageFlowError soft)
            => MergeFailure(result, soft);

        // A failure met after the call was applied (G4): the first failure stays primary (D31).
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Success<PageFlowError> MergeFailure(in Success<PageFlowError> result, in PageFlowError error)
        {
            if (result.TryGetFailure(out var failure))
            {
                return failure.WithSuppressed(error);
            }

            return error.WithTransitionState(PageTransitionState.Completed);
        }

        // The first error, with every later one suppressed under it (count >= 1).
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static PageFlowError JoinErrors(IReadOnlyList<PageFlowError> errors)
        {
            var error = errors[0];
            var count = errors.Count;

            for (var i = 1; i < count; i++)
            {
                error = error.WithSuppressed(errors[i]);
            }

            return error;
        }

        // With no bound PageAbortState (a direct core call, default PageContext) unheard notifications are dropped.
        private bool TryGetPublishFailure<TMessage>(
              in Success<PublishError> published
            , PageAbortState abort
            , out PublishError publishError
        )
        {
            if (published.TryGetFailure(out publishError) == false)
            {
                return false;
            }

            if (publishError.IsSubscriberNotFound == false)
            {
                return true;
            }

            if (_warnNoSubscriber && abort != null)
            {
                abort.AddSoftError(PageFlowError.SubscriberNotFound(typeof(TMessage)));
            }

            return false;
        }

        internal static PageFlowError WithSuppressedErrors(PageFlowError error, IReadOnlyList<PageFlowError> suppressed)
        {
            if (suppressed == null)
            {
                return error;
            }

            var count = suppressed.Count;

            for (var i = 0; i < count; i++)
            {
                error = error.WithSuppressed(suppressed[i]);
            }

            return error;
        }

        // B4 (D85): an error the call cannot return because a cancellation is thrown travels with the call's abort
        // state;
        // the wrapper reports it, or the state logs it when the call ends. Without a bound state it is logged here
        // (D28).
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SetAside(PageAbortState abort, in PageFlowError error)
        {
            if (abort is { IsBound: true })
            {
                abort.SetAside(error);
                return;
            }

            error.Log(_logger);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SetAsideSuppressed(PageAbortState abort, in Success<PageFlowError> result)
        {
            if (result.TryGetFailure(out var error) == false)
            {
                return;
            }

            var suppressed = error.SuppressedErrors;
            var count = suppressed.Length;

            for (var i = 0; i < count; i++)
            {
                SetAside(abort, suppressed[i]);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SetAside(PageAbortState abort, List<PageFlowError> errors)
        {
            if (errors == null)
            {
                return;
            }

            var count = errors.Count;

            for (var i = 0; i < count; i++)
            {
                SetAside(abort, errors[i]);
            }
        }

        /// <summary>Publishes <see cref="EncosyTower.PageFlows.PubSub.DetachPageMessage"/> for a page.</summary>
        /// <param name="flow">The flow the page detaches from.</param>
        /// <param name="page">The page that detaches.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the subscribers handled the message, or none received it; otherwise a failure; see
        /// <see cref="PageFlowError"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="page"/> is <c>null</c>.</exception>
        /// <remarks>This call does not report unheard notifications.</remarks>
        public async UnityTask<Success<PageFlowError>> PublishDetachAsync(
              IPageFlow flow
            , IPage page
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNull(page);

            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            var publisher = _publisher.Scope(_flowScope);

            var published = await DetachPageMessage.Async.Publish(
                  in publisher
                , new DetachPageMessage(flow, page, token)
                , GetPublishingContext(token)
            );

            if (published.TryGetFailure(out var publishError) && publishError.IsSubscriberNotFound == false)
            {
                return PageFlowError.FromPublish(publishError);
            }

            return Success.Yes;
        }

        /// <summary>Runs the show and hide steps of one transition.</summary>
        /// <param name="flow">The flow that runs the transition.</param>
        /// <param name="transition">Whether the call shows or hides a page.</param>
        /// <param name="pageToHide">The page that hides, or <c>null</c>.</param>
        /// <param name="pageToShow">The page that shows, or <c>null</c>.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the transition ran to its end; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The steps run in the order described by <see cref="IPageTransition"/>: the
        /// <see cref="EncosyTower.PageFlows.PubSub.PageTransitionBeganMessage"/> messages, the before steps, the show
        /// and hide steps, the after steps, then the
        /// <see cref="EncosyTower.PageFlows.PubSub.PageTransitionEndedMessage"/> messages.
        /// </para>
        /// <para>
        /// The <see cref="PageOptions"/> of the pages replace the transition options of <paramref name="context"/>
        /// where they use <see cref="PageOptions.Options.forceUse"/>. A step that fails does not stop the later steps;
        /// the first failure is returned with the <see cref="PageTransitionState.Completed"/> state.
        /// </para>
        /// <para>
        /// When an abort is requested through <see cref="PageContext.Abort"/>, the flow undoes the transition, as
        /// <see cref="PageAbortMode"/> says, and returns the <c>Aborted</c> case with the
        /// <see cref="PageTransitionState.Undone"/> state. A cancellation throws the
        /// <see cref="System.OperationCanceledException"/> after the flow undoes its steps.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="flow"/> is <c>null</c>.</exception>
        public async UnityTask<Success<PageFlowError>> TransitionAsync(
              IPageFlow flow
            , PageTransition transition
            , IPage pageToHide
            , IPage pageToShow
            , PageContext context
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNull(flow);

            var callerContext = context;
            var abort = context.Abort._state;

            HasTransitionBegun = false;

            if (IsAbortRequested(abort))
            {
                return PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode);
            }

            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            IPageTransition pageToHideTransition = null;
            IPageTransition pageToShowTransition = null;

            try
            {
                context = ApplyPageOptionsToContext(transition, pageToHide, pageToShow, context);
                context = ProcessContext(pageToHide, pageToShow, context);

                if (pageToHide is IPageHasTransition hasHideTransition
                    && hasHideTransition.PageTransition is { } hideTransition
                )
                {
                    pageToHideTransition = hideTransition;
                }

                if (pageToShow is IPageHasTransition hasShowTransition
                    && hasShowTransition.PageTransition is { } showTransition
                )
                {
                    pageToShowTransition = showTransition;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var error = PageFlowError.Exception(ex);
                abort?.BeginRecovering(error);
                return error;
            }

            var parallelTasks = ArrayPool<UnityTask>.Shared.Rent(4);
            var sequentialTasks = ArrayPool<UnityTask>.Shared.Rent(2);
            var aborted = false;
            TransitionFailureState pending = default;

            HasTransitionBegun = true;

            try
            {
                pending = await RunForwardStepsAsync(
                      flow
                    , parallelTasks
                    , sequentialTasks
                    , pageToHide
                    , pageToShow
                    , pageToHideTransition
                    , pageToShowTransition
                    , context
                    , abort
                    , token
                );

                if (IsAbortRequested(abort))
                {
                    aborted = true;

                    pending = await RunUndoStepsAsync(
                          flow
                        , pending
                        , parallelTasks
                        , sequentialTasks
                        , transition
                        , pageToHide
                        , pageToShow
                        , pageToHideTransition
                        , pageToShowTransition
                        , callerContext
                        , abort
                        , token
                    );
                }
            }
            finally
            {
                ArrayPool<UnityTask>.Shared.Return(array: parallelTasks, clearArray: true);
                ArrayPool<UnityTask>.Shared.Return(array: sequentialTasks, clearArray: true);
            }

            if (pending.cancellation != null)
            {
                abort?.SetAsideSuppressedErrors();
                pending.cancellation.Throw();
            }

            if (aborted)
            {
                return WithSuppressedErrors(
                      PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode)
                        .WithTransitionState(PageTransitionState.Undone)
                    , abort?.SuppressedErrors
                );
            }

            if (pending.result.TryGetFailure(out var failure))
            {
                return failure.WithTransitionState(PageTransitionState.Completed);
            }

            return Success.Yes;
        }

        internal async UnityTask<Success<PageFlowError>> TransitionManyAsync(
              IPageFlow flow
            , PageTransition transition
            , IPage[] pages
            , int count
            , PageContext context
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNull(flow);

            var callerContext = context;
            var abort = context.Abort._state;
            var isShow = transition == PageTransition.Show;

            HasTransitionBegun = false;

            if (IsAbortRequested(abort))
            {
                return PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode);
            }

            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            var batch = PageTransitionBatch.Rent(flow, isShow, pages, count, abort);
            var aborted = false;
            PageTransitionBatchFailures failures = default;

            try
            {
                try
                {
                    for (var index = 0; index < count; index++)
                    {
                        var page = pages[index];
                        var pageToHide = batch.PageToHide(index: index, undo: false);
                        var pageToShow = batch.PageToShow(index: index, undo: false);
                        var pageContext = ApplyPageOptionsToContext(transition, pageToHide, pageToShow, callerContext);

                        batch.Contexts[index] = ProcessContext(pageToHide, pageToShow, pageContext);

                        batch.Transitions[index] = page is IPageHasTransition hasTransition
                            ? hasTransition.PageTransition
                            : null;
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    var error = PageFlowError.Exception(ex);
                    abort?.BeginRecovering(error);
                    return error;
                }

                HasTransitionBegun = true;

                failures = await RunForwardBatchAsync(failures, batch, token);

                if (IsAbortRequested(abort))
                {
                    aborted = true;
                    failures = await RunUndoBatchAsync(failures, batch, transition, callerContext, token);
                }
            }
            finally
            {
                batch.Return();
            }

            if (failures.primary.cancellation != null)
            {
                abort?.SetAsideSuppressedErrors();
                SetAside(abort, failures.suppressedErrors);
                failures.primary.cancellation.Throw();
            }

            if (aborted)
            {
                return WithSuppressedErrors(
                      PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode)
                        .WithTransitionState(PageTransitionState.Undone)
                    , abort?.SuppressedErrors
                );
            }

            if (failures.primary.result.TryGetFailure(out var failure))
            {
                return WithSuppressedErrors(
                      failure.WithTransitionState(PageTransitionState.Completed)
                    , failures.suppressedErrors
                );
            }

            return Success.Yes;
        }

        private async UnityTask<PageTransitionBatchFailures> RunForwardBatchAsync(
              PageTransitionBatchFailures failures
            , PageTransitionBatch batch
            , CancellationToken token
        )
        {
            var abort = batch.Abort;
            var count = batch.PageCount;
            var runningTasks = batch.RunningTasks;
            var stepToken = abort?.GetCallbackToken(token) ?? token;

            StartBatchMessages(failures: ref failures, batch: batch, isEnd: false, isUndo: false, token: token);
            failures = await ObserveBatchMessagesAsync(
                  failures: failures
                , batch: batch
                , isEnd: false
                , isUndo: false
                , token: token
            );

            if (IsAbortRequested(abort))
            {
                return failures;
            }

            for (var index = 0; index < count; index++)
            {
                runningTasks[index] = BeginTransitionAsync(
                      batch.PageTaskBuffers[index]
                    , batch.PageToHide(index: index, undo: false)
                    , batch.PageToShow(index: index, undo: false)
                    , batch.HideTransition(index: index, undo: false)
                    , batch.ShowTransition(index: index, undo: false)
                    , batch.Contexts[index]
                    , stepToken
                );
            }

            failures = await ObserveBatchTasksAsync(failures: failures, batch: batch, isUndo: false, token: token);

            if (IsAbortRequested(abort))
            {
                return failures;
            }

            for (var index = 0; index < count; index++)
            {
                runningTasks[index] = UnityTask.CompletedTask;

                try
                {
                    if (batch.IsShow)
                    {
                        _backend?.ActivatePageRendering(batch.Pages[index]);
                    }

                    runningTasks[index] = ParallelTransitionAsync(
                          batch.PageTaskBuffers[index]
                        , batch.PageToHide(index: index, undo: false)
                        , batch.PageToShow(index: index, undo: false)
                        , batch.HideTransition(index: index, undo: false)
                        , batch.ShowTransition(index: index, undo: false)
                        , batch.Contexts[index]
                        , stepToken
                    );
                }
                catch (Exception ex)
                {
                    RecordBatchException(failures: ref failures, ex: ex, abort: abort, token: token, isUndo: false);
                }
            }

            failures = await ObserveBatchTasksAsync(failures: failures, batch: batch, isUndo: false, token: token);

            if (IsAbortRequested(abort))
            {
                return failures;
            }

            for (var index = 0; index < count; index++)
            {
                var page = batch.Pages[index];
                var pageContext = batch.Contexts[index];
                var pageTransition = batch.Transitions[index];

                try
                {
                    if (batch.IsShow)
                    {
                        (page as IPageOnAfterShow)?.OnAfterShow(WithPageOrigin(pageContext, page));
                    }
                    else
                    {
                        (page as IPageOnAfterHide)?.OnAfterHide(WithPageOrigin(pageContext, page));
                    }
                }
                catch (Exception ex)
                {
                    RecordBatchException(failures: ref failures, ex: ex, abort: abort, token: token, isUndo: false);
                }

                if (IsAbortRequested(abort))
                {
                    return failures;
                }

                try
                {
                    pageTransition?.OnAfterTransition(CreateTransitionContext(
                          batch.IsShow ? PageTransition.Show : PageTransition.Hide
                        , pageContext
                        , pageTransition
                    ));
                }
                catch (Exception ex)
                {
                    RecordBatchException(failures: ref failures, ex: ex, abort: abort, token: token, isUndo: false);
                }

                if (IsAbortRequested(abort))
                {
                    return failures;
                }
            }

            abort?.CloseWindow();

            StartBatchMessages(failures: ref failures, batch: batch, isEnd: true, isUndo: false, token: token);
            return await ObserveBatchMessagesAsync(
                  failures: failures
                , batch: batch
                , isEnd: true
                , isUndo: false
                , token: token
            );
        }

        private async UnityTask<PageTransitionBatchFailures> RunUndoBatchAsync(
              PageTransitionBatchFailures failures
            , PageTransitionBatch batch
            , PageTransition transition
            , PageContext callerContext
            , CancellationToken token
        )
        {
            var abort = batch.Abort;
            var count = batch.PageCount;
            var runningTasks = batch.RunningTasks;
            var opposite = transition == PageTransition.Show ? PageTransition.Hide : PageTransition.Show;
            var undoSide = batch.IsShow ? PageTransition.Hide : PageTransition.Show;

            for (var index = 0; index < count; index++)
            {
                var undoContext = callerContext;

                try
                {
                    undoContext = ApplyPageOptionsToContext(
                          opposite
                        , batch.PageToHide(index: index, undo: true)
                        , batch.PageToShow(index: index, undo: true)
                        , callerContext
                    );

                    undoContext = ProcessContext(
                          batch.PageToHide(index: index, undo: true)
                        , batch.PageToShow(index: index, undo: true)
                        , undoContext
                    );
                }
                catch (Exception ex)
                {
                    RecordBatchException(failures: ref failures, ex: ex, abort: abort, token: token, isUndo: true);
                    undoContext = callerContext;
                }

                if (abort.RequestMode == PageAbortMode.Immediate)
                {
                    undoContext = undoContext with {
                        ShowOptions = undoContext.ShowOptions | PageTransitionOptions.ZeroDuration,
                        HideOptions = undoContext.HideOptions | PageTransitionOptions.ZeroDuration,
                    };
                }

                batch.Contexts[index] = undoContext;
            }

            for (var index = 0; index < count; index++)
            {
                runningTasks[index] = BeginTransitionAsync(
                      batch.PageTaskBuffers[index]
                    , batch.PageToHide(index: index, undo: true)
                    , batch.PageToShow(index: index, undo: true)
                    , batch.HideTransition(index: index, undo: true)
                    , batch.ShowTransition(index: index, undo: true)
                    , batch.Contexts[index]
                    , token
                );
            }

            failures = await ObserveBatchTasksAsync(failures: failures, batch: batch, isUndo: true, token: token);

            for (var index = 0; index < count; index++)
            {
                runningTasks[index] = ParallelTransitionAsync(
                      batch.PageTaskBuffers[index]
                    , batch.PageToHide(index: index, undo: true)
                    , batch.PageToShow(index: index, undo: true)
                    , batch.HideTransition(index: index, undo: true)
                    , batch.ShowTransition(index: index, undo: true)
                    , batch.Contexts[index]
                    , token
                );
            }

            failures = await ObserveBatchTasksAsync(failures: failures, batch: batch, isUndo: true, token: token);

            for (var index = 0; index < count; index++)
            {
                var page = batch.Pages[index];
                var pageContext = batch.Contexts[index];
                var pageTransition = batch.Transitions[index];

                try
                {
                    if (batch.IsShow)
                    {
                        (page as IPageOnAfterHide)?.OnAfterHide(WithPageOrigin(pageContext, page));
                    }
                    else
                    {
                        (page as IPageOnAfterShow)?.OnAfterShow(WithPageOrigin(pageContext, page));
                    }
                }
                catch (Exception ex)
                {
                    RecordBatchException(failures: ref failures, ex: ex, abort: abort, token: token, isUndo: true);
                }

                try
                {
                    pageTransition?.OnAfterTransition(CreateTransitionContext(undoSide, pageContext, pageTransition));
                }
                catch (Exception ex)
                {
                    RecordBatchException(failures: ref failures, ex: ex, abort: abort, token: token, isUndo: true);
                }
            }

            abort.CloseWindow();

            StartBatchMessages(failures: ref failures, batch: batch, isEnd: true, isUndo: true, token: token);
            return await ObserveBatchMessagesAsync(
                  failures: failures
                , batch: batch
                , isEnd: true
                , isUndo: true
                , token: token
            );
        }

        private void StartBatchMessages(
              ref PageTransitionBatchFailures failures
            , in PageTransitionBatch batch
            , bool isEnd
            , bool isUndo
            , CancellationToken token
        )
        {
            var count = batch.PageCount;
            var publishTasks = batch.PublishTasks;
            var publisher = _publisher.Scope(_flowScope);
            var publishingContext = GetPublishingContext(CancellationToken.None);
            var pageTransition = batch.IsShow ? PageTransition.Show : PageTransition.Hide;
            var transitionState = isUndo ? PageTransitionState.Undone : PageTransitionState.Completed;

            for (var index = 0; index < count; index++)
            {
                var page = batch.Pages[index];

                publishTasks[index] = UnityTask.FromResult<Success<PublishError>>(Success.Yes);

                try
                {
                    if (isEnd)
                    {
                        publishTasks[index] = PageTransitionEndedMessage.Async.Publish(
                              in publisher
                            , new PageTransitionEndedMessage(
                                  batch.Flow
                                , page
                                , pageTransition
                                , Option.None
                                , transitionState
                                , token
                            )
                            , publishingContext
                        );
                    }
                    else
                    {
                        publishTasks[index] = PageTransitionBeganMessage.Async.Publish(
                              in publisher
                            , new PageTransitionBeganMessage(batch.Flow, page, pageTransition, Option.None, token)
                            , publishingContext
                        );
                    }
                }
                catch (Exception ex)
                {
                    RecordBatchException(
                          failures: ref failures
                        , ex: ex
                        , abort: batch.Abort
                        , token: token
                        , isUndo: isUndo
                    );
                }
            }
        }

        private async UnityTask<PageTransitionBatchFailures> ObserveBatchMessagesAsync(
              PageTransitionBatchFailures failures
            , PageTransitionBatch batch
            , bool isEnd
            , bool isUndo
            , CancellationToken token
        )
        {
            var count = batch.PageCount;
            var publishTasks = batch.PublishTasks;

            for (var index = 0; index < count; index++)
            {
                try
                {
                    var published = await publishTasks[index];
                    PublishError publishError;
                    bool isFailure;

                    if (isEnd)
                    {
                        isFailure = TryGetPublishFailure<PageTransitionEndedMessage>(
                              in published
                            , batch.Abort
                            , out publishError
                        );
                    }
                    else
                    {
                        isFailure = TryGetPublishFailure<PageTransitionBeganMessage>(
                              in published
                            , batch.Abort
                            , out publishError
                        );
                    }

                    if (isFailure)
                    {
                        RecordBatchFailure(
                              failures: ref failures
                            , failure: PageFlowError.FromPublish(publishError)
                            , abort: batch.Abort
                            , isUndo: isUndo
                        );
                    }
                }
                catch (Exception ex)
                {
                    RecordBatchException(
                          failures: ref failures
                        , ex: ex
                        , abort: batch.Abort
                        , token: token
                        , isUndo: isUndo
                    );
                }
            }

            return failures;
        }

        private async UnityTask<PageTransitionBatchFailures> ObserveBatchTasksAsync(
              PageTransitionBatchFailures failures
            , PageTransitionBatch batch
            , bool isUndo
            , CancellationToken token
        )
        {
            var count = batch.PageCount;
            var runningTasks = batch.RunningTasks;

            for (var index = 0; index < count; index++)
            {
                try
                {
                    await runningTasks[index];
                }
                catch (Exception ex)
                {
                    RecordBatchException(
                          failures: ref failures
                        , ex: ex
                        , abort: batch.Abort
                        , token: token
                        , isUndo: isUndo
                    );
                }
            }

            return failures;
        }

        private void RecordBatchException(
              ref PageTransitionBatchFailures failures
            , Exception ex
            , PageAbortState abort
            , CancellationToken token
            , bool isUndo
        )
        {
            if (isUndo)
            {
                RecordUndoException(ref failures.primary, ex, token, abort);
                return;
            }

            if (ex is not OperationCanceledException && ShouldSuppress(in failures, abort))
            {
                failures.AddSuppressed(PageFlowError.Exception(ex));
                return;
            }

            RecordForwardException(ref failures.primary, ex, abort, token);
        }

        private void RecordBatchFailure(
              ref PageTransitionBatchFailures failures
            , in PageFlowError failure
            , PageAbortState abort
            , bool isUndo
        )
        {
            if (isUndo)
            {
                RecordUndoFailure(in failure, abort);
                return;
            }

            if (ShouldSuppress(in failures, abort))
            {
                failures.AddSuppressed(failure);
                return;
            }

            RecordForwardFailure(ref failures.primary, in failure, abort);
        }

        private static bool ShouldSuppress(in PageTransitionBatchFailures failures, PageAbortState abort)
            => IsAbortRequested(abort) == false
            && (failures.primary.cancellation != null || failures.primary.result.IsFailure);

        private async UnityTask<TransitionFailureState> RunForwardStepsAsync(
              IPageFlow flow
            , UnityTask[] parallelTasks
            , UnityTask[] sequentialTasks
            , IPage pageToHide
            , IPage pageToShow
            , IPageTransition pageToHideTransition
            , IPageTransition pageToShowTransition
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            var stepToken = abort?.GetCallbackToken(token) ?? token;
            var showTransitionContext = CreateTransitionContext(PageTransition.Show, context, pageToShowTransition);
            var hideTransitionContext = CreateTransitionContext(PageTransition.Hide, context, pageToHideTransition);
            TransitionFailureState pending = default;

            if (pageToHide != null)
            {
                try
                {
                    var published = await PublishTransitionBeganAsync(
                          flow
                        , pageToHide
                        , PageTransition.Hide
                        , pageToShow
                        , token
                    );

                    if (TryGetPublishFailure<PageTransitionBeganMessage>(in published, abort, out var publishError))
                    {
                        RecordForwardFailure(ref pending, PageFlowError.FromPublish(publishError), abort);
                    }
                }
                catch (Exception ex)
                {
                    RecordForwardException(ref pending, ex, abort, token);
                }
            }

            if (pageToShow != null)
            {
                try
                {
                    var published = await PublishTransitionBeganAsync(
                          flow
                        , pageToShow
                        , PageTransition.Show
                        , pageToHide
                        , token
                    );

                    if (TryGetPublishFailure<PageTransitionBeganMessage>(in published, abort, out var publishError))
                    {
                        RecordForwardFailure(ref pending, PageFlowError.FromPublish(publishError), abort);
                    }
                }
                catch (Exception ex)
                {
                    RecordForwardException(ref pending, ex, abort, token);
                }
            }

            if (IsAbortRequested(abort))
            {
                return pending;
            }

            try
            {
                await BeginTransitionAsync(
                      parallelTasks
                    , pageToHide
                    , pageToShow
                    , pageToHideTransition
                    , pageToShowTransition
                    , context
                    , stepToken
                );
            }
            catch (Exception ex)
            {
                RecordForwardException(ref pending, ex, abort, token);
            }

            if (IsAbortRequested(abort))
            {
                return pending;
            }

            try
            {
                if (pageToShow != null)
                {
                    _backend?.ActivatePageRendering(pageToShow);
                }

                if (context.SequentialTransition)
                {
                    await SequentialTransitionAsync(
                          sequentialTasks
                        , pageToHide
                        , pageToShow
                        , pageToHideTransition
                        , pageToShowTransition
                        , context
                        , stepToken
                    );
                }
                else
                {
                    await ParallelTransitionAsync(
                          parallelTasks
                        , pageToHide
                        , pageToShow
                        , pageToHideTransition
                        , pageToShowTransition
                        , context
                        , stepToken
                    );
                }
            }
            catch (Exception ex)
            {
                RecordForwardException(ref pending, ex, abort, token);
            }

            if (IsAbortRequested(abort))
            {
                return pending;
            }

            try
            {
                (pageToShow as IPageOnAfterShow)?.OnAfterShow(WithPageOrigin(context, pageToShow));
            }
            catch (Exception ex)
            {
                RecordForwardException(ref pending, ex, abort, token);
            }

            if (IsAbortRequested(abort))
            {
                return pending;
            }

            try
            {
                pageToShowTransition?.OnAfterTransition(showTransitionContext);
            }
            catch (Exception ex)
            {
                RecordForwardException(ref pending, ex, abort, token);
            }

            if (IsAbortRequested(abort))
            {
                return pending;
            }

            try
            {
                (pageToHide as IPageOnAfterHide)?.OnAfterHide(WithPageOrigin(context, pageToHide));
            }
            catch (Exception ex)
            {
                RecordForwardException(ref pending, ex, abort, token);
            }

            if (IsAbortRequested(abort))
            {
                return pending;
            }

            try
            {
                pageToHideTransition?.OnAfterTransition(hideTransitionContext);
            }
            catch (Exception ex)
            {
                RecordForwardException(ref pending, ex, abort, token);
            }

            if (IsAbortRequested(abort))
            {
                return pending;
            }

            abort?.CloseWindow();

            if (pageToHide != null)
            {
                try
                {
                    var published = await PublishTransitionEndedAsync(
                          flow
                        , pageToHide
                        , PageTransition.Hide
                        , pageToShow
                        , PageTransitionState.Completed
                        , token
                    );

                    if (TryGetPublishFailure<PageTransitionEndedMessage>(in published, abort, out var publishError))
                    {
                        RecordForwardFailure(ref pending, PageFlowError.FromPublish(publishError), abort);
                    }
                }
                catch (Exception ex)
                {
                    RecordForwardException(ref pending, ex, abort, token);
                }
            }

            if (pageToShow != null)
            {
                try
                {
                    var published = await PublishTransitionEndedAsync(
                          flow
                        , pageToShow
                        , PageTransition.Show
                        , pageToHide
                        , PageTransitionState.Completed
                        , token
                    );

                    if (TryGetPublishFailure<PageTransitionEndedMessage>(in published, abort, out var publishError))
                    {
                        RecordForwardFailure(ref pending, PageFlowError.FromPublish(publishError), abort);
                    }
                }
                catch (Exception ex)
                {
                    RecordForwardException(ref pending, ex, abort, token);
                }
            }

            return pending;
        }

        private UnityTask<Success<PublishError>> PublishTransitionBeganAsync(
              IPageFlow flow
            , IPage page
            , PageTransition transition
            , IPage otherPage
            , CancellationToken token
        )
        {
            var publisher = _publisher.Scope(_flowScope);

            return PageTransitionBeganMessage.Async.Publish(
                  in publisher
                , new PageTransitionBeganMessage(
                      flow
                    , page
                    , transition
                    , Option.SomeIf(otherPage != null, otherPage)
                    , token
                )
                , GetPublishingContext(CancellationToken.None)
            );
        }

        private UnityTask<Success<PublishError>> PublishTransitionEndedAsync(
              IPageFlow flow
            , IPage page
            , PageTransition transition
            , IPage otherPage
            , PageTransitionState state
            , CancellationToken token
        )
        {
            var publisher = _publisher.Scope(_flowScope);

            return PageTransitionEndedMessage.Async.Publish(
                  in publisher
                , new PageTransitionEndedMessage(
                      flow
                    , page
                    , transition
                    , Option.SomeIf(otherPage != null, otherPage)
                    , state
                    , token
                )
                , GetPublishingContext(CancellationToken.None)
            );
        }

        private void RecordForwardException(
              ref TransitionFailureState pending
            , Exception ex
            , PageAbortState abort
            , CancellationToken token
        )
        {
            if (ex is not OperationCanceledException)
            {
                RecordForwardFailure(ref pending, PageFlowError.Exception(ex), abort);
                return;
            }

            if (IsAbortRequested(abort))
            {
                if (token.IsCancellationRequested == false)
                {
                    // The abort's own cancellation: nothing is recorded.
                    return;
                }

                PreferCancellation(ref pending, ex, abort);
                return;
            }

            PreferCancellation(ref pending, ex, abort);

            if (abort != null)
            {
                abort.BeginRecovering(PageFlowError.Cancelled().WithTransitionState(PageTransitionState.Completed));
            }
        }

        private void RecordForwardFailure(
              ref TransitionFailureState pending
            , in PageFlowError failure
            , PageAbortState abort
        )
        {
            if (IsAbortRequested(abort))
            {
                abort.AddSuppressedError(failure);
                return;
            }

            KeepFirstFailure(ref pending, failure, abort);

            if (abort != null)
            {
                abort.BeginRecovering(failure.WithTransitionState(PageTransitionState.Completed));
            }
        }

        private async UnityTask<TransitionFailureState> RunUndoStepsAsync(
              IPageFlow flow
            , TransitionFailureState pending
            , UnityTask[] parallelTasks
            , UnityTask[] sequentialTasks
            , PageTransition transition
            , IPage pageToHide
            , IPage pageToShow
            , IPageTransition pageToHideTransition
            , IPageTransition pageToShowTransition
            , PageContext callerContext
            , PageAbortState abort
            , CancellationToken token
        )
        {
            var undoPageToHide = pageToShow;
            var undoPageToShow = pageToHide;
            var undoHideTransition = pageToShowTransition;
            var undoShowTransition = pageToHideTransition;
            var opposite = transition == PageTransition.Show ? PageTransition.Hide : PageTransition.Show;
            var undoContext = callerContext;

            try
            {
                undoContext = ApplyPageOptionsToContext(opposite, undoPageToHide, undoPageToShow, callerContext);
                undoContext = ProcessContext(undoPageToHide, undoPageToShow, undoContext);
            }
            catch (Exception ex)
            {
                RecordUndoException(ref pending, ex, token, abort);
                undoContext = callerContext;
            }

            if (abort.RequestMode == PageAbortMode.Immediate)
            {
                undoContext = undoContext with {
                    ShowOptions = undoContext.ShowOptions | PageTransitionOptions.ZeroDuration,
                    HideOptions = undoContext.HideOptions | PageTransitionOptions.ZeroDuration,
                };
            }

            var showTransitionContext = CreateTransitionContext(PageTransition.Show, undoContext, undoShowTransition);
            var hideTransitionContext = CreateTransitionContext(PageTransition.Hide, undoContext, undoHideTransition);

            try
            {
                await BeginTransitionAsync(
                      parallelTasks
                    , undoPageToHide
                    , undoPageToShow
                    , undoHideTransition
                    , undoShowTransition
                    , undoContext
                    , token
                );
            }
            catch (Exception ex)
            {
                RecordUndoException(ref pending, ex, token, abort);
            }

            try
            {
                if (undoContext.SequentialTransition)
                {
                    await SequentialTransitionAsync(
                          sequentialTasks
                        , undoPageToHide
                        , undoPageToShow
                        , undoHideTransition
                        , undoShowTransition
                        , undoContext
                        , token
                    );
                }
                else
                {
                    await ParallelTransitionAsync(
                          parallelTasks
                        , undoPageToHide
                        , undoPageToShow
                        , undoHideTransition
                        , undoShowTransition
                        , undoContext
                        , token
                    );
                }
            }
            catch (Exception ex)
            {
                RecordUndoException(ref pending, ex, token, abort);
            }

            try
            {
                (undoPageToShow as IPageOnAfterShow)?.OnAfterShow(WithPageOrigin(undoContext, undoPageToShow));
            }
            catch (Exception ex)
            {
                RecordUndoException(ref pending, ex, token, abort);
            }

            try
            {
                undoShowTransition?.OnAfterTransition(showTransitionContext);
            }
            catch (Exception ex)
            {
                RecordUndoException(ref pending, ex, token, abort);
            }

            try
            {
                (undoPageToHide as IPageOnAfterHide)?.OnAfterHide(WithPageOrigin(undoContext, undoPageToHide));
            }
            catch (Exception ex)
            {
                RecordUndoException(ref pending, ex, token, abort);
            }

            try
            {
                undoHideTransition?.OnAfterTransition(hideTransitionContext);
            }
            catch (Exception ex)
            {
                RecordUndoException(ref pending, ex, token, abort);
            }

            abort.CloseWindow();

            if (pageToHide != null)
            {
                try
                {
                    var published = await PublishTransitionEndedAsync(
                          flow
                        , pageToHide
                        , PageTransition.Hide
                        , pageToShow
                        , PageTransitionState.Undone
                        , token
                    );

                    if (TryGetPublishFailure<PageTransitionEndedMessage>(in published, abort, out var publishError))
                    {
                        RecordUndoFailure(PageFlowError.FromPublish(publishError), abort);
                    }
                }
                catch (Exception ex)
                {
                    RecordUndoException(ref pending, ex, token, abort);
                }
            }

            if (pageToShow != null)
            {
                try
                {
                    var published = await PublishTransitionEndedAsync(
                          flow
                        , pageToShow
                        , PageTransition.Show
                        , pageToHide
                        , PageTransitionState.Undone
                        , token
                    );

                    if (TryGetPublishFailure<PageTransitionEndedMessage>(in published, abort, out var publishError))
                    {
                        RecordUndoFailure(PageFlowError.FromPublish(publishError), abort);
                    }
                }
                catch (Exception ex)
                {
                    RecordUndoException(ref pending, ex, token, abort);
                }
            }

            return pending;
        }

        private void RecordUndoException(
              ref TransitionFailureState pending
            , Exception ex
            , CancellationToken token
            , PageAbortState abort
        )
        {
            if (ex is OperationCanceledException)
            {
                if (token.IsCancellationRequested == false)
                {
                    // Only the abort token is cancelled: the cancellation is part of the abort.
                    return;
                }

                PreferCancellation(ref pending, ex, abort);
                return;
            }

            RecordUndoFailure(PageFlowError.Exception(ex), abort);
        }

        private static void RecordUndoFailure(in PageFlowError failure, PageAbortState abort)
            => abort.AddSuppressedError(failure);

        // A later failure rides on the first as a suppressed error (D31); while a cancellation wins (Q22) it is set
        // aside (B4).
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void KeepFirstFailure(
              ref TransitionFailureState pending
            , in PageFlowError failure
            , PageAbortState abort
        )
        {
            if (pending.cancellation != null)
            {
                SetAside(abort, failure);
                return;
            }

            pending.result = pending.result.TryGetFailure(out var first)
                ? first.WithSuppressed(failure)
                : failure;
        }

        // Provisional Q22: a cancellation is never swallowed by an earlier failure; that failure is set aside (B4)
        // instead.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void PreferCancellation(ref TransitionFailureState pending, Exception ex, PageAbortState abort)
        {
            if (pending.cancellation != null)
            {
                return;
            }

            if (pending.result.TryGetFailure(out var failure))
            {
                SetAside(abort, failure);
                pending.result = Success.Yes;
            }

            pending.cancellation = ExceptionDispatchInfo.Capture(ex);
        }

        private async UnityTask<Success<PageFlowError>> UndoAttachAsync(
              IPageFlow flow
            , IPage page
            , PageAbortState abort
            , CancellationToken token
        )
        {
            Success<PageFlowError> result = Success.Yes;
            ExceptionDispatchInfo cancelled = null;

            try
            {
                var publisher = _publisher.Scope(_flowScope);

                var published = await DetachPageMessage.Async.Publish(
                      in publisher
                    , new DetachPageMessage(flow, page, token)
                    , GetPublishingContext(CancellationToken.None)
                );

                if (TryGetPublishFailure<DetachPageMessage>(in published, abort, out var publishError))
                {
                    result = PageFlowError.FromPublish(publishError);
                }
            }
            catch (OperationCanceledException ex)
            {
                cancelled = ExceptionDispatchInfo.Capture(ex);
            }
            catch (Exception ex)
            {
                result = PageFlowError.Exception(ex);
            }

            DeinitializePage(flow, page);

            if (cancelled != null)
            {
                if (result.TryGetFailure(out var droppedError))
                {
                    SetAside(abort, droppedError);
                }

                cancelled.Throw();
            }

            return result;
        }

        private void InitializeIPageNeedsInterfaces(IPage page, PageFlow flow)
        {
            if (page is IPageNeedsFlowScope flowScope)
            {
                flowScope.FlowScope = _flowScope;
            }

            if (_flowScopeCollectionApplier.TryGetValue(out var applier))
            {
                applier.ApplyTo(page);
            }

            if (page is IPageNeedsMessageSubscriber subscriber)
            {
                subscriber.Subscriber = flow._subscriber;
            }

            if (page is IPageNeedsMessagePublisher publisher)
            {
                publisher.Publisher = flow._publisher;
            }
        }

        internal static void DeinitializePage(IPage page)
        {
            if (page is IPageNeedsFlowScope flowId)
            {
                flowId.FlowScope = default;
            }

            if (page is IPageNeedsMessageSubscriber subscriber)
            {
                subscriber.Subscriber = default;
            }

            if (page is IPageNeedsMessagePublisher publisher)
            {
                publisher.Publisher = default;
            }
        }

        internal void DeinitializePage(IPageFlow flow, IPage page)
        {
            DeinitializePage(page);
            ReleaseClaim(flow, page);
        }

        private bool TryClaim(IPageFlow flow, IPage page)
        {
            return page is null
                || _ownershipTracker.TryGetValue(out var tracker) == false
                || tracker.TryClaim(page, flow, out _);
        }

        private void ReleaseClaim(IPageFlow flow, IPage page)
        {
            if (page is not null && _ownershipTracker.TryGetValue(out var tracker))
            {
                tracker.Release(page, flow);
            }
        }

        private static PageContext ProcessContext(IPage pageToHide, IPage pageToShow, PageContext context)
        {
            context = context with {
                ShowOptions = TrySetZeroDuration(
                      context.ShowOptions
                    , PageTransitionOptions.ZeroDurationWithOtherPage
                    , pageToHide is not null || pageToShow is null
                )
            };

            context = context with {
                ShowOptions = TrySetZeroDuration(
                      context.ShowOptions
                    , PageTransitionOptions.ZeroDurationWithoutOtherPage
                    , pageToHide is null || pageToShow is null
                )
            };

            context = context with {
                HideOptions = TrySetZeroDuration(
                      context.HideOptions
                    , PageTransitionOptions.ZeroDurationWithOtherPage
                    , pageToShow is not null || pageToHide is null
                )
            };

            context = context with {
                HideOptions = TrySetZeroDuration(
                      context.HideOptions
                    , PageTransitionOptions.ZeroDurationWithoutOtherPage
                    , pageToShow is null || pageToHide is null
                )
            };

            return context;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static PageTransitionOptions TrySetZeroDuration(
                  PageTransitionOptions options
                , PageTransitionOptions checkValue
                , bool checkCondition
            )
            {
                if (options.Contains(checkValue) && checkCondition)
                {
                    options = options.Unset(checkValue);
                    options |= PageTransitionOptions.ZeroDuration;
                }

                return options;
            }
        }

        private static PageContext ApplyPageOptionsToContext(
              PageTransition transition
            , IPage pageToHide
            , IPage pageToShow
            , PageContext context
        )
        {
            PageOptions pageToHideOptions = (pageToHide as IPageHasOptions)?.PageOptions ?? default;
            PageOptions pageToShowOptions = (pageToShow as IPageHasOptions)?.PageOptions ?? default;

            var hideOptions = PageOptions.SelectHideOptions(transition, pageToHideOptions, pageToShowOptions)
                .GetTransitionOptions()
                .GetValueOrDefault(context.HideOptions);

            var showOptions = PageOptions.SelectShowOptions(transition, pageToHideOptions, pageToShowOptions)
                .GetTransitionOptions()
                .GetValueOrDefault(context.ShowOptions);

            return context with {
                ShowOptions = showOptions,
                HideOptions = hideOptions,
            };
        }

        private static PageTransitionContext CreateTransitionContext(
              PageTransition side
            , in PageContext context
            , IPageTransition transition
        )
            => new() {
                Transition = side,
                ShowOptions = context.ShowOptions,
                HideOptions = context.HideOptions,
                Abort = context.Abort.WithOrigin(new PageAbortOrigin.Transition(transition)),
            };

        internal static PageContext WithPageOrigin(in PageContext context, IPage page)
            => context with { Abort = context.Abort.WithOrigin(new PageAbortOrigin.Page(page)) };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAbortRequested(PageAbortState abort)
            => abort != null && abort.IsRequested;

        private static async UnityTask BeginTransitionAsync(
              UnityTask[] tasks
            , IPage pageToHide
            , IPage pageToShow
            , IPageTransition pageToHideTransition
            , IPageTransition pageToShowTransition
            , PageContext context
            , CancellationToken token
        )
        {
            var showTransitionContext = CreateTransitionContext(PageTransition.Show, context, pageToShowTransition);
            var hideTransitionContext = CreateTransitionContext(PageTransition.Hide, context, pageToHideTransition);
            var showPageContext = WithPageOrigin(context, pageToShow);
            var hidePageContext = WithPageOrigin(context, pageToHide);

            tasks[0] = (pageToShow as IPageOnBeforeShowAsync)?.OnBeforeShowAsync(showPageContext, token)
                ?? UnityTask.CompletedTask;

            tasks[1] = pageToShowTransition
                ?.OnBeforeTransitionAsync(showTransitionContext, token)
                ?? UnityTask.CompletedTask;

            tasks[2] = (pageToHide as IPageOnBeforeHideAsync)?.OnBeforeHideAsync(hidePageContext, token)
                ?? UnityTask.CompletedTask;

            tasks[3] = pageToHideTransition
                ?.OnBeforeTransitionAsync(hideTransitionContext, token)
                ?? UnityTask.CompletedTask;

            await UnityTask.WhenAll(tasks);

            (pageToShow as IPageOnBeforeShow)?.OnBeforeShow(showPageContext);
            (pageToHide as IPageOnBeforeHide)?.OnBeforeHide(hidePageContext);
        }

        private static async UnityTask ParallelTransitionAsync(
              UnityTask[] tasks
            , IPage pageToHide
            , IPage pageToShow
            , IPageTransition pageToHideTransition
            , IPageTransition pageToShowTransition
            , PageContext context
            , CancellationToken token
        )
        {
            PageTransitionOptions showOptions = context.ShowOptions;
            PageTransitionOptions hideOptions = context.HideOptions;
            var withShow = showOptions.Contains(PageTransitionOptions.NoTransition) == false;
            var withHide = hideOptions.Contains(PageTransitionOptions.NoTransition) == false;
            var showTransitionContext = CreateTransitionContext(PageTransition.Show, context, pageToShowTransition);
            var hideTransitionContext = CreateTransitionContext(PageTransition.Hide, context, pageToHideTransition);
            var showPageContext = WithPageOrigin(context, pageToShow);
            var hidePageContext = WithPageOrigin(context, pageToHide);

            {
                tasks[0] = withShow && pageToShow is IPageOnShowAsync pageShow
                    ? pageShow.OnShowAsync(showPageContext, token)
                    : UnityTask.CompletedTask;

                if (pageToShowTransition is not null)
                {
                    withShow |= pageToShowTransition.ForceRunShow;

                    tasks[1] = withShow
                        ? pageToShowTransition.OnShowAsync(showTransitionContext, token)
                        : UnityTask.CompletedTask;
                }
                else
                {
                    tasks[1] = UnityTask.CompletedTask;
                }
            }

            {
                tasks[2] = withHide && pageToHide is IPageOnHideAsync pageHide
                    ? pageHide.OnHideAsync(hidePageContext, token)
                    : UnityTask.CompletedTask;

                if (pageToHideTransition is not null)
                {
                    withHide |= pageToHideTransition.ForceRunHide;

                    tasks[3] = withHide
                        ? pageToHideTransition.OnHideAsync(hideTransitionContext, token)
                        : UnityTask.CompletedTask;
                }
                else
                {
                    tasks[3] = UnityTask.CompletedTask;
                }
            }

            if (withShow || withHide)
            {
                await UnityTask.WhenAll(tasks);
            }
        }

        private static async UnityTask SequentialTransitionAsync(
              UnityTask[] tasks
            , IPage pageToHide
            , IPage pageToShow
            , IPageTransition pageToHideTransition
            , IPageTransition pageToShowTransition
            , PageContext context
            , CancellationToken token
        )
        {
            PageTransitionOptions showOptions = context.ShowOptions;
            PageTransitionOptions hideOptions = context.HideOptions;
            var withShow = showOptions.Contains(PageTransitionOptions.NoTransition) == false;
            var withHide = hideOptions.Contains(PageTransitionOptions.NoTransition) == false;
            var showTransitionContext = CreateTransitionContext(PageTransition.Show, context, pageToShowTransition);
            var hideTransitionContext = CreateTransitionContext(PageTransition.Hide, context, pageToHideTransition);
            var showPageContext = WithPageOrigin(context, pageToShow);
            var hidePageContext = WithPageOrigin(context, pageToHide);

            {
                tasks[0] = withShow && pageToShow is IPageOnShowAsync pageShow
                    ? pageShow.OnShowAsync(showPageContext, token)
                    : UnityTask.CompletedTask;

                if (pageToShowTransition is not null)
                {
                    withShow |= pageToShowTransition.ForceRunShow;

                    tasks[1] = withShow
                        ? pageToShowTransition.OnShowAsync(showTransitionContext, token)
                        : UnityTask.CompletedTask;
                }
                else
                {
                    tasks[1] = UnityTask.CompletedTask;
                }

                await UnityTask.WhenAll(tasks);
            }

            {
                tasks[0] = withHide && pageToHide is IPageOnHideAsync pageHide
                    ? pageHide.OnHideAsync(hidePageContext, token)
                    : UnityTask.CompletedTask;

                if (pageToHideTransition is not null)
                {
                    withHide |= pageToHideTransition.ForceRunHide;

                    tasks[1] = withHide
                        ? pageToHideTransition.OnHideAsync(hideTransitionContext, token)
                        : UnityTask.CompletedTask;
                }
                else
                {
                    tasks[1] = UnityTask.CompletedTask;
                }

                await UnityTask.WhenAll(tasks);
            }
        }

        private PublishingContext GetPublishingContext(
              CancellationToken token
#if __PUBSUB_INCLUDE_CALLER_INFO__
            , [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
            , [CallerFilePath] string filePath = ""
#endif
        )
        {
            return PublishingContext.Default(
                  logger: _logger
#if __PUBSUB_INCLUDE_CALLER_INFO__
                , callerInfo: new(lineNumber, memberName, filePath)
#endif
                , token: token
            );
        }

        private struct TransitionFailureState
        {
            public ExceptionDispatchInfo cancellation;
            public Success<PageFlowError> result;
        }

        private struct PageTransitionBatchFailures
        {
            public TransitionFailureState primary;
            public List<PageFlowError> suppressedErrors;

            public void AddSuppressed(in PageFlowError error)
            {
                suppressedErrors ??= new List<PageFlowError>();
                suppressedErrors.Add(error);
            }
        }

        private readonly struct PageTransitionBatch
        {
            public readonly IPageFlow Flow;
            public readonly bool IsShow;
            public readonly IPage[] Pages;
            public readonly int PageCount;
            public readonly PageAbortState Abort;
            public readonly PageContext[] Contexts;
            public readonly IPageTransition[] Transitions;
            public readonly UnityTask[][] PageTaskBuffers;
            public readonly UnityTask[] RunningTasks;
            public readonly UnityTask<Success<PublishError>>[] PublishTasks;

            private PageTransitionBatch(
                  IPageFlow flow
                , bool isShow
                , IPage[] pages
                , int pageCount
                , PageAbortState abort
                , PageContext[] contexts
                , IPageTransition[] transitions
                , UnityTask[][] pageTaskBuffers
                , UnityTask[] runningTasks
                , UnityTask<Success<PublishError>>[] publishTasks
            )
            {
                Flow = flow;
                IsShow = isShow;
                Pages = pages;
                PageCount = pageCount;
                Abort = abort;
                Contexts = contexts;
                Transitions = transitions;
                PageTaskBuffers = pageTaskBuffers;
                RunningTasks = runningTasks;
                PublishTasks = publishTasks;
            }

            public static PageTransitionBatch Rent(
                  IPageFlow flow
                , bool isShow
                , IPage[] pages
                , int pageCount
                , PageAbortState abort
            )
            {
                var pageTaskBuffers = ArrayPool<UnityTask[]>.Shared.Rent(pageCount);

                for (var index = 0; index < pageCount; index++)
                {
                    pageTaskBuffers[index] = ArrayPool<UnityTask>.Shared.Rent(4);
                }

                return new PageTransitionBatch(
                      flow
                    , isShow
                    , pages
                    , pageCount
                    , abort
                    , ArrayPool<PageContext>.Shared.Rent(pageCount)
                    , ArrayPool<IPageTransition>.Shared.Rent(pageCount)
                    , pageTaskBuffers
                    , ArrayPool<UnityTask>.Shared.Rent(pageCount)
                    , ArrayPool<UnityTask<Success<PublishError>>>.Shared.Rent(pageCount)
                );
            }

            public void Return()
            {
                for (var index = 0; index < PageCount; index++)
                {
                    ArrayPool<UnityTask>.Shared.Return(array: PageTaskBuffers[index], clearArray: true);
                }

                ArrayPool<UnityTask[]>.Shared.Return(array: PageTaskBuffers, clearArray: true);
                ArrayPool<PageContext>.Shared.Return(array: Contexts, clearArray: true);
                ArrayPool<IPageTransition>.Shared.Return(array: Transitions, clearArray: true);
                ArrayPool<UnityTask>.Shared.Return(array: RunningTasks, clearArray: true);
                ArrayPool<UnityTask<Success<PublishError>>>.Shared.Return(array: PublishTasks, clearArray: true);
            }

            public IPage PageToHide(int index, bool undo)
                => IsShow == undo ? Pages[index] : null;

            public IPage PageToShow(int index, bool undo)
                => IsShow != undo ? Pages[index] : null;

            public IPageTransition HideTransition(int index, bool undo)
                => IsShow == undo ? Transitions[index] : null;

            public IPageTransition ShowTransition(int index, bool undo)
                => IsShow != undo ? Transitions[index] : null;
        }
    }
}
