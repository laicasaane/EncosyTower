using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Internals;
using EncosyTower.Tasks;

using ETDBG = EncosyTower.Debugging;

namespace EncosyTower.PageFlows.Flows
{
    /// <summary>A page stack that keeps every pushed page.</summary>
    /// <remarks>
    /// <para>
    /// A push hides the current page, which stays in the stack, and shows the pushed page; a pop hides and removes
    /// the current page, and shows the page below it. All page flow operations and callbacks run on the Unity main
    /// thread; see <see cref="IPageFlow"/>.
    /// </para>
    /// <para>The stack does not check whether the page is already in it; push a new page instance.</para>
    /// </remarks>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public sealed class PageStackMulti<TPage> : IPageStackMulti<TPage>, IDisposable
        where TPage : class, IPage
    {
        private readonly ConcurrentStack<TPage> _pages = new();
        private readonly PageAbortRegistry _aborts = new();
        private readonly ILogger _logger;
        private readonly PageFlow _flow;

        /// <summary>Creates an empty stack.</summary>
        /// <param name="context">The context that supplies the services and settings of the flow.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
        public PageStackMulti([NotNull] IPageFlowContext context) : this(context, backend: null)
        {
        }

        /// <summary>Creates an empty stack.</summary>
        /// <param name="context">The context that supplies the services and settings of the flow.</param>
        /// <param name="backend">The platform backend, or <c>null</c> for no rendering integration.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
        public PageStackMulti([NotNull] IPageFlowContext context, IPageFlowBackend backend)
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
        public IReadOnlyCollection<TPage> PageCollection => _pages;

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
        public async UnityTask<Success<PageFlowError>> PushAsync(
              [NotNull] TPage page
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.Abortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                result = await PushCoreAsync(page, context, abort, token);
                result = abort.MergeSoftErrors(result);
                return result;
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
        public async UnityTask<Success<PageFlowError>> PushAsync(
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
                result = await PushCoreAsync(factory, release, context, abort, token);
                result = abort.MergeSoftErrors(result);
                return result;
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
        public async UnityTask<Success<PageFlowError>> RemoveAllAsync(PageContext context, CancellationToken token)
        {
            var abort = BeginCall(ref context, PageAbortCallKind.NotAbortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                result = await RemoveAllCoreAsync(context, abort, token);
                result = abort.MergeSoftErrors(result);
                return result;
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

        internal async UnityTask<PagePopAllResult<TPage>> PopAllAsync(PageContext context, CancellationToken token)
        {
            var abort = BeginCall(ref context, PageAbortCallKind.Abortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                var pop = await PopAllCoreAsync(context, abort, token);

                if (pop.Thrown == null)
                {
                    result = abort.MergeSoftErrors(pop.Result);
                    pop = pop with { Result = result };
                }
                else
                {
                    result = PageFlowError.Exception(pop.Thrown.SourceException);
                }

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

        private async UnityTask<PagePopAllResult<TPage>> PopAllCoreAsync(
              PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            if (_pages.Count < 1)
            {
                return new PagePopAllResult<TPage>(PageFlowError.NoPage());
            }

            if (abort.IsRequested)
            {
                return new PagePopAllResult<TPage>(PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode));
            }

            if (token.IsCancellationRequested)
            {
                return new PagePopAllResult<TPage>(PageFlowError.Cancelled());
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
                return new PagePopAllResult<TPage>(validation);
            }

            var count = _pages.Count;

            if (count < 1)
            {
                return new PagePopAllResult<TPage>(PageFlowError.NoPage());
            }

            if (token.IsCancellationRequested)
            {
                return new PagePopAllResult<TPage>(PageFlowError.Cancelled());
            }

            _aborts.EnterCall(abort, token);
            IsInTransition = true;

            var pool = ArrayPool<TPage>.Shared;
            var snapshot = pool.Rent(count);
            var popped = false;
            var hasCalledTransitionAsync = false;
            var detachStarted = false;

            try
            {
                popped = true;

                for (var index = 0; index < count; index++)
                {
                    _pages.TryPop(out snapshot[index]);
                }

                hasCalledTransitionAsync = true;
                var result = await _flow.TransitionManyAsync(
                      this
                    , PageTransition.Hide
                    , snapshot
                    , count
                    , context
                    , token
                );

                if (result.TryGetFailure(out var transitionError))
                {
                    if (transitionError.IsAborted)
                    {
                        RestorePages(snapshot, count);
                        popped = false;
                        return new PagePopAllResult<TPage>(result);
                    }

                    abort.BeginRecovering(transitionError);
                }

                if (_flow.HasTransitionBegun == false)
                {
                    RestorePages(snapshot, count);
                    popped = false;
                    return new PagePopAllResult<TPage>(result);
                }

                detachStarted = true;

                var detach = await _flow.DetachAllAsync(this, snapshot, count, context, token);

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

                var applied = snapshot;
                snapshot = null;

                return new PagePopAllResult<TPage>(result, applied, count);
            }
            catch (Exception ex)
            {
                if (abort.IsRequested)
                {
                    if (popped)
                    {
                        RestorePages(snapshot, count);
                        popped = false;
                    }

                    throw;
                }

                var hasTransitionBegun = hasCalledTransitionAsync && _flow.HasTransitionBegun;

                if (hasTransitionBegun == false)
                {
                    if (popped)
                    {
                        RestorePages(snapshot, count);
                        popped = false;
                    }

                    throw;
                }

                if (detachStarted == false)
                {
                    detachStarted = true;

                    try
                    {
                        await _flow.DetachAllOnThrowAsync(this, snapshot, count, context, token);
                    }
                    catch (OperationCanceledException)
                    {
                        // The pages are detached by now; the result still carries the original exception.
                    }
                }

                var applied = snapshot;
                snapshot = null;

                return new PagePopAllResult<TPage>(
                      PageFlowError.Exception(ex)
                    , applied
                    , count
                    , ExceptionDispatchInfo.Capture(ex)
                );
            }
            finally
            {
                IsInTransition = false;

                if (snapshot != null)
                {
                    pool.Return(array: snapshot, clearArray: true);
                }
            }
        }

        private void RestorePages(TPage[] pages, int count)
        {
            for (var index = count - 1; index >= 0; index--)
            {
                _pages.Push(pages[index]);
            }
        }

        private async UnityTask<Success<PageFlowError>> PushCoreAsync(
              [NotNull] TPage page
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNullOrUnityObjectInvalid(page);

            if (abort.IsRequested)
            {
                return PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode);
            }

            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
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
                return validation;
            }

            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            _aborts.EnterCall(abort, token);
            IsInTransition = true;

            TPage pageToHide = null;
            var pushed = false;
            var attached = false;
            var hasCalledTransitionAsync = false;

            try
            {
                _pages.TryPeek(out pageToHide);
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

                    return result;
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
                        }

                        return result;
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
                }

                return result;
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
                    }
                }

                throw;
            }
            finally
            {
                IsInTransition = false;
            }
        }

        private async UnityTask<Success<PageFlowError>> PushCoreAsync(
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
                return PageFlowError.Cancelled();
            }

            var created = await factory(token);

            if (created.TryGetValue(out var page) == false)
            {
                return created.GetErrorOrThrow();
            }

            Success<PageFlowError> result;

            try
            {
                if (abort.IsRequested)
                {
                    result = PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode);
                }
                else
                {
                    result = await PushCoreAsync(page, context, abort, token);
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

            if (result.TryGetFailure(out var error) && page != null && Contains(page) == false)
            {
                result = PageFlow.ReleaseOnFailure(in error, page, release);
            }

            return result;
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

        private async UnityTask<Success<PageFlowError>> RemoveAllCoreAsync(
              PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            if (_pages.Count < 1)
            {
                return PageFlowError.NoPage();
            }

            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
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
                return validation;
            }

            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            var snapshot = _pages.ToArray();

            if (snapshot.Length < 1)
            {
                return PageFlowError.NoPage();
            }

            _aborts.EnterCall(abort, token);
            IsInTransition = true;

            try
            {
                return await _flow.DetachAllAsync(this, snapshot, context, token);
            }
            finally
            {
                _pages.Clear();
                IsInTransition = false;
            }
        }

        async UnityTask<Success<PageFlowError>> IPageStackStrategy<TPage>.PopAsync(
              PageContext context
            , CancellationToken token
        )
        {
            var pop = await PopAsync(context, token);
            return pop.Result;
        }
    }

    internal readonly record struct PagePopAllResult<TPage>(
          Success<PageFlowError> Result
        , TPage[] Pages
        , int Count
        , ExceptionDispatchInfo Thrown
    )
        where TPage : class, IPage
    {
        public PagePopAllResult(Success<PageFlowError> result)
            : this(Result: result, Pages: null, Count: 0, Thrown: null)
        {
        }

        public PagePopAllResult(Success<PageFlowError> result, TPage[] pages, int count)
            : this(Result: result, Pages: pages, Count: count, Thrown: null)
        {
        }
    }
}
