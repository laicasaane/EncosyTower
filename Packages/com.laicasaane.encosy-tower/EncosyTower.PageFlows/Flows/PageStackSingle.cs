using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Internals;
using EncosyTower.Tasks;

using ETDBG = EncosyTower.Debugging;

namespace EncosyTower.PageFlows.Flows
{
    /// <summary>A page stack that holds at most one page.</summary>
    /// <remarks>
    /// <para>
    /// A push hides and removes the current page, then shows the pushed page; a pop hides and removes the current
    /// page. All page flow operations and callbacks run on the Unity main thread; see <see cref="IPageFlow"/>.
    /// </para>
    /// <para>The stack does not check whether the page is already in it; push a new page instance.</para>
    /// </remarks>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public class PageStackSingle<TPage> : IPageStackSingle<TPage>, IDisposable
        where TPage : class, IPage
    {
        private readonly ConcurrentStack<TPage> _pages = new();
        private readonly PageAbortRegistry _aborts = new();
        private readonly ILogger _logger;
        private readonly PageFlow _flow;

        /// <summary>Creates an empty stack.</summary>
        /// <param name="context">The context that supplies the services and settings of the flow.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
        public PageStackSingle([NotNull] IPageFlowContext context) : this(context, backend: null)
        {
        }

        /// <summary>Creates an empty stack.</summary>
        /// <param name="context">The context that supplies the services and settings of the flow.</param>
        /// <param name="backend">The platform backend, or <c>null</c> for no rendering integration.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
        public PageStackSingle([NotNull] IPageFlowContext context, IPageFlowBackend backend)
        {
            ETDBG.ThrowHelper.ThrowIfNull(context);

            _logger = context.Logger ?? DevLogger.Default;
            _flow = new PageFlow(
                  context.Subscriber
                , context.Publisher
                , context.FlowScope
                , context.FlowScopeCollectionApplier
                , context.OwnershipTracker
                , context.WarnNoSubscriber
                , _logger
                , backend
            );
        }

        /// <inheritdoc/>
        public Option<TPage> CurrentPage => Option.SomeIf(_pages.TryPeek(out var page), page);

        /// <inheritdoc/>
        public bool IsInTransition { get; private set; }

        /// <inheritdoc/>
        public Success<PageAbortError> AbortCurrent(PageAbortMode mode)
            => _aborts.AbortCurrent(mode);

        internal PageAbortState BeginCall(ref PageContext context, PageAbortCallKind kind)
            => _aborts.BeginCall(ref context, kind, _logger);

        internal void EndCall(PageAbortState state, in Success<PageFlowError> result)
            => _aborts.EndCall(state, result);

        /// <summary>Clears the stack. It does not detach or release the pages.</summary>
        public void Dispose()
        {
            _pages.Clear();
        }

        /// <inheritdoc/>
        public async UnityTask<PagePopResult<TPage>> PopAsync(PageContext context, CancellationToken token)
        {
            var abort = BeginCall(ref context, PageAbortCallKind.Abortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                var pop = await PopCoreAsync(context, abort, token);
                result = abort.MergeSoftErrors(pop.Result);
                pop = pop with { Result = result };
                return pop;
            }
            catch (Exception ex)
            {
                result = PageFlowError.Exception(ex);
                throw;
            }
            finally
            {
                EndCall(abort, result);
            }
        }

        /// <inheritdoc/>
        /// <remarks>The stack does not check whether the page is already in it; push a new page instance.</remarks>
        public async UnityTask<PagePushResult<TPage>> PushAsync(
              [NotNull] TPage page
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.Abortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                var push = await PushCoreAsync(page, context, abort, token);
                result = abort.MergeSoftErrors(push.Result);
                push = push with { Result = result };
                return push;
            }
            catch (Exception ex)
            {
                result = PageFlowError.Exception(ex);
                throw;
            }
            finally
            {
                EndCall(abort, result);
            }
        }

        /// <inheritdoc/>
        public async UnityTask<PagePushResult<TPage>> PushAsync(
              [NotNull] Func<CancellationToken, UnityTask<Result<TPage, PageFlowError>>> factory
            , [NotNull] Action<TPage> release
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.Abortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                var push = await PushCoreAsync(factory, release, context, abort, token);
                result = abort.MergeSoftErrors(push.Result);
                push = push with { Result = result };
                return push;
            }
            catch (Exception ex)
            {
                result = PageFlowError.Exception(ex);
                throw;
            }
            finally
            {
                EndCall(abort, result);
            }
        }

        private async UnityTask<PagePopResult<TPage>> PopCoreAsync(
              PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            if (_pages.Count < 1)
            {
                return new PagePopResult<TPage>(PageFlowError.NoPage(), Option.None);
            }

            if (abort.IsRequested)
            {
                return new PagePopResult<TPage>(
                      PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode)
                    , Option.None
                );
            }

            if (token.IsCancellationRequested)
            {
                return new PagePopResult<TPage>(PageFlowError.Cancelled(), Option.None);
            }

            var validation = await Validator.ValidateTransitionAsync(
                  this
                , _logger
                , context.AsyncOperation
                , abort
                , token
            );

            if (validation.IsFailure)
            {
                return new PagePopResult<TPage>(validation, Option.None);
            }

            if (_pages.Count < 1)
            {
                return new PagePopResult<TPage>(PageFlowError.NoPage(), Option.None);
            }

            if (token.IsCancellationRequested)
            {
                return new PagePopResult<TPage>(PageFlowError.Cancelled(), Option.None);
            }

            _aborts.EnterCall(abort, token);
            IsInTransition = true;

            TPage pageToHide = null;
            var popped = false;
            var hasCalledTransitionAsync = false;
            var detachStarted = false;

            try
            {
                popped = _pages.TryPop(out pageToHide);
                _pages.TryPeek(out var pageToShow);

                hasCalledTransitionAsync = true;
                var result = await _flow.TransitionAsync(
                      this
                    , PageTransition.Hide
                    , pageToHide
                    , pageToShow
                    , context
                    , token
                );

                if (result.TryGetFailure(out var transitionError))
                {
                    if (transitionError.IsAborted)
                    {
                        if (popped)
                        {
                            _pages.Push(pageToHide);
                            popped = false;
                        }

                        return new PagePopResult<TPage>(result, Option.None);
                    }

                    abort.BeginRecovering(transitionError);
                }

                var hasTransitionBegun = _flow.HasTransitionBegun;

                if (hasTransitionBegun == false)
                {
                    if (popped)
                    {
                        _pages.Push(pageToHide);
                        popped = false;
                    }

                    return new PagePopResult<TPage>(result, Option.None);
                }

                detachStarted = true;

                var detach = await _flow.ForceDetachAsync(this, pageToHide, context, token);

                if (detach.TryGetFailure(out var detachError))
                {
                    if (result.TryGetFailure(out var popError))
                    {
                        result = popError.WithSuppressed(detachError);
                    }
                    else
                    {
                        result = detachError.WithTransitionState(PageTransitionState.Completed);
                    }
                }

                return new PagePopResult<TPage>(result, Option.SomeIf(popped, pageToHide));
            }
            catch
            {
                if (abort.IsRequested)
                {
                    if (popped)
                    {
                        _pages.Push(pageToHide);
                        popped = false;
                    }

                    throw;
                }

                var hasTransitionBegun = hasCalledTransitionAsync && _flow.HasTransitionBegun;

                if (hasTransitionBegun == false)
                {
                    if (popped)
                    {
                        _pages.Push(pageToHide);
                        popped = false;
                    }
                }
                else if (detachStarted == false)
                {
                    detachStarted = true;
                    await _flow.ForceDetachOnThrowAsync(this, pageToHide, context, token);
                }

                throw;
            }
            finally
            {
                IsInTransition = false;
            }
        }

        private async UnityTask<PagePushResult<TPage>> PushCoreAsync(
              [NotNull] TPage page
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNullOrUnityObjectInvalid(page);

            if (abort.IsRequested)
            {
                return new PagePushResult<TPage>(
                      PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode)
                    , Option.None
                );
            }

            if (token.IsCancellationRequested)
            {
                return new PagePushResult<TPage>(PageFlowError.Cancelled(), Option.None);
            }

            var validation = await Validator.ValidateTransitionAsync(
                  this
                , _logger
                , context.AsyncOperation
                , abort
                , token
            );

            if (validation.IsFailure)
            {
                return new PagePushResult<TPage>(validation, Option.None);
            }

            if (token.IsCancellationRequested)
            {
                return new PagePushResult<TPage>(PageFlowError.Cancelled(), Option.None);
            }

            _aborts.EnterCall(abort, token);
            IsInTransition = true;

            TPage pageToHide = null;
            var hasPageToHide = false;
            var pushed = false;
            var attached = false;
            var hasCalledTransitionAsync = false;
            var detachStarted = false;

            try
            {
                hasPageToHide = _pages.TryPop(out pageToHide);
                _pages.Push(page);
                pushed = true;

                var result = await _flow.AttachAsync(this, page, context, token);

                if (result.IsFailure)
                {
                    var attachError = result.GetFailureOrThrow();

                    if (attachError.IsAborted == false)
                    {
                        abort.BeginRecovering(attachError);
                    }

                    if (pushed)
                    {
                        _pages.TryPop(out _);
                        pushed = false;
                    }

                    if (hasPageToHide)
                    {
                        _pages.Push(pageToHide);
                        hasPageToHide = false;
                    }

                    return new PagePushResult<TPage>(result, Option.None);
                }

                attached = true;
                hasCalledTransitionAsync = true;
                result = await _flow.TransitionAsync(
                      this
                    , PageTransition.Show
                    , pageToHide
                    , page
                    , context
                    , token
                );

                if (result.TryGetFailure(out var transitionError))
                {
                    if (transitionError.IsAborted)
                    {
                        try
                        {
                            if (attached)
                            {
                                attached = false;
                                var abortDetach = await _flow.ForceDetachInAbortAsync(this, page, context, token);

                                if (abortDetach.TryGetFailure(out var abortDetachError))
                                {
                                    result = transitionError.WithSuppressed(abortDetachError);
                                }
                            }
                        }
                        finally
                        {
                            if (pushed)
                            {
                                _pages.TryPop(out _);
                                pushed = false;
                            }

                            if (hasPageToHide)
                            {
                                _pages.Push(pageToHide);
                                hasPageToHide = false;
                            }
                        }

                        return new PagePushResult<TPage>(result, Option.None);
                    }

                    abort.BeginRecovering(transitionError);
                }

                var hasTransitionBegun = _flow.HasTransitionBegun;

                if (hasTransitionBegun == false)
                {
                    attached = false;

                    var notBegunDetach = await _flow.ForceDetachAsync(this, page, context, token);

                    if (notBegunDetach.TryGetFailure(out var notBegunDetachError))
                    {
                        if (result.TryGetFailure(out var notBegunError))
                        {
                            result = notBegunError.WithSuppressed(notBegunDetachError);
                        }
                        else
                        {
                            result = notBegunDetachError.WithTransitionState(PageTransitionState.Completed);
                        }
                    }

                    if (pushed)
                    {
                        _pages.TryPop(out _);
                        pushed = false;
                    }

                    if (hasPageToHide)
                    {
                        _pages.Push(pageToHide);
                        hasPageToHide = false;
                    }

                    return new PagePushResult<TPage>(result, Option.None);
                }

                var displaced = Option.SomeIf(hasPageToHide, pageToHide);

                if (hasPageToHide == false)
                {
                    return new PagePushResult<TPage>(result, displaced);
                }

                detachStarted = true;

                var detach = await _flow.ForceDetachAsync(this, pageToHide, context, token);

                if (detach.TryGetFailure(out var detachError))
                {
                    if (result.TryGetFailure(out var pushError))
                    {
                        result = pushError.WithSuppressed(detachError);
                    }
                    else
                    {
                        result = detachError.WithTransitionState(PageTransitionState.Completed);
                    }
                }

                return new PagePushResult<TPage>(result, displaced);
            }
            catch
            {
                if (abort.IsRequested)
                {
                    try
                    {
                        try
                        {
                            if (attached)
                            {
                                attached = false;

                                var abortDetach = await _flow.ForceDetachInAbortAsync(this, page, context, token);

                                if (abortDetach.TryGetFailure(out var abortDetachError))
                                {
                                    abortDetachError.Log(_logger);
                                }
                            }
                        }
                        finally
                        {
                            if (pushed)
                            {
                                _pages.TryPop(out _);
                                pushed = false;
                            }

                            if (hasPageToHide)
                            {
                                _pages.Push(pageToHide);
                                hasPageToHide = false;
                            }
                        }
                    }
                    catch (Exception inner)
                    {
                        _logger.LogException(inner);
                    }

                    throw;
                }

                var hasTransitionBegun = hasCalledTransitionAsync && _flow.HasTransitionBegun;

                if (hasTransitionBegun == false)
                {
                    try
                    {
                        if (attached)
                        {
                            attached = false;
                            await _flow.ForceDetachOnThrowAsync(this, page, context, token);
                        }
                    }
                    finally
                    {
                        if (pushed)
                        {
                            _pages.TryPop(out _);
                            pushed = false;
                        }

                        if (hasPageToHide)
                        {
                            _pages.Push(pageToHide);
                            hasPageToHide = false;
                        }
                    }
                }
                else if (hasPageToHide && detachStarted == false)
                {
                    detachStarted = true;
                    await _flow.ForceDetachOnThrowAsync(this, pageToHide, context, token);
                }

                throw;
            }
            finally
            {
                IsInTransition = false;
            }
        }

        private async UnityTask<PagePushResult<TPage>> PushCoreAsync(
              [NotNull] Func<CancellationToken, UnityTask<Result<TPage, PageFlowError>>> factory
            , [NotNull] Action<TPage> release
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNull(factory);
            ETDBG.ThrowHelper.ThrowIfNull(release);

            if (token.IsCancellationRequested)
            {
                return new PagePushResult<TPage>(PageFlowError.Cancelled(), Option.None);
            }

            var created = await factory(token);

            if (created.TryGetValue(out var page) == false)
            {
                return new PagePushResult<TPage>(created.GetErrorOrThrow(), Option.None);
            }

            PagePushResult<TPage> push;

            try
            {
                if (abort.IsRequested)
                {
                    push = new PagePushResult<TPage>(
                          PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode)
                        , Option.None
                    );
                }
                else
                {
                    push = await PushCoreAsync(page, context, abort, token);
                }
            }
            catch
            {
                if (page != null && Contains(page) == false)
                {
                    PageFlow.ReleaseOnThrow(page, release, _logger);
                }

                throw;
            }

            if (push.Result.TryGetFailure(out var error) && page != null && Contains(page) == false)
            {
                push = push with { Result = PageFlow.ReleaseOnFailure(in error, page, release) };
            }

            return push;
        }

        private bool Contains(TPage page)
        {
            foreach (var item in _pages)
            {
                if (ReferenceEquals(item, page))
                {
                    return true;
                }
            }

            return false;
        }

        async UnityTask<Success<PageFlowError>> IPageStackStrategy<TPage>.PopAsync(
              PageContext context
            , CancellationToken token
        )
        {
            var pop = await PopAsync(context, token);
            return pop.Result;
        }

        async UnityTask<Success<PageFlowError>> IPageStackStrategy<TPage>.PushAsync(
              [NotNull] TPage page
            , PageContext context
            , CancellationToken token
        )
        {
            var push = await PushAsync(page, context, token);
            return push.Result;
        }

        async UnityTask<Success<PageFlowError>> IPageStackStrategy<TPage>.PushAsync(
              [NotNull] Func<CancellationToken, UnityTask<Result<TPage, PageFlowError>>> factory
            , [NotNull] Action<TPage> release
            , PageContext context
            , CancellationToken token
        )
        {
            var push = await PushAsync(factory, release, context, token);
            return push.Result;
        }
    }
}
