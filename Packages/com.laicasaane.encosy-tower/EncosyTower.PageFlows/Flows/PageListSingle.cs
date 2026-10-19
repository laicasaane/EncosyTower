using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Internals;
using EncosyTower.Tasks;

using ETDBG = EncosyTower.Debugging;

namespace EncosyTower.PageFlows.Flows
{
    /// <summary>A page list that shows at most one of its pages.</summary>
    /// <remarks>
    /// <para>
    /// Showing a page hides the current page. The page at <see cref="PageContext.DefaultIndex"/> cannot be hidden;
    /// hiding another page shows it. All page flow operations and callbacks run on the Unity main thread; see
    /// <see cref="IPageFlow"/>.
    /// </para>
    /// <para>
    /// Adding and removing pages are not abortable. Showing a page by index fails with the <c>AlreadyShown</c> case
    /// when the page at that index is already the current page.
    /// </para>
    /// </remarks>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public class PageListSingle<TPage> : IPageListSingle<TPage>, IDisposable
        where TPage : class, IPage
    {
        private readonly List<TPage> _pages = new();
        private readonly PageAbortRegistry _aborts = new();
        private readonly PageListCallTracker<TPage> _tracker = new();
        private readonly ILogger _logger;
        private readonly PageFlow _flow;
        private readonly bool _rejectDuplicatePages;
        private bool _isDisposed;

        /// <summary>Creates an empty list.</summary>
        /// <param name="context">The context that supplies the services and settings of the flow.</param>
        /// <remarks>
        /// The list reads <see cref="IPageFlowContext.RejectDuplicatePages"/> once, when it is created.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
        public PageListSingle([NotNull] IPageFlowContext context) : this(context, backend: null)
        {
        }

        /// <summary>Creates an empty list.</summary>
        /// <param name="context">The context that supplies the services and settings of the flow.</param>
        /// <param name="backend">The platform backend, or <c>null</c> for no rendering integration.</param>
        /// <remarks>
        /// The list reads <see cref="IPageFlowContext.RejectDuplicatePages"/> once, when it is created.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
        public PageListSingle([NotNull] IPageFlowContext context, IPageFlowBackend backend)
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
            _rejectDuplicatePages = context.RejectDuplicatePages;
        }

        /// <inheritdoc/>
        public Option<TPage> CurrentPage { get; private set; }

        /// <inheritdoc/>
        public ListFast<TPage>.ReadOnly Pages => _pages;

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

        /// <summary>Clears the list. It does not detach or release the pages.</summary>
        /// <remarks>An add that is still running ends with the <c>Cancelled</c> case.</remarks>
        public void Dispose()
        {
            _isDisposed = true;
            _pages.Clear();
            _tracker.Clear();
        }

        /// <inheritdoc/>
        public int IndexOf([NotNull] TPage page)
        {
            ETDBG.ThrowHelper.ThrowIfNullOrUnityObjectInvalid(page);
            return _pages.IndexOf(page);
        }

        /// <inheritdoc/>
        public async UnityTask<Success<PageFlowError>> AddAsync(
              [NotNull] TPage page
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.NotAbortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                result = await AddCoreAsync(page, context, abort, token);
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
        public async UnityTask<Success<PageFlowError>> AddAsync(
              [NotNull] Func<CancellationToken, UnityTask<Result<TPage, PageFlowError>>> factory
            , [NotNull] Action<TPage> release
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.NotAbortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                result = await AddCoreAsync(factory, release, context, abort, token);
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
        public async UnityTask<Success<PageFlowError>> HideAsync(PageContext context, CancellationToken token)
        {
            var abort = BeginCall(ref context, PageAbortCallKind.Abortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                result = await HideCoreAsync(context, abort, token);
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

        /// <inheritdoc/>
        public async UnityTask<Success<PageFlowError>> RemoveAsync(
              [NotNull] TPage page
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.NotAbortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                result = await RemoveCoreAsync(page, context, abort, token);
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
        public async UnityTask<Success<PageFlowError>> RemoveAsync(
              int index
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.NotAbortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                result = await RemoveCoreAsync(index, context, abort, token);
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
        /// <remarks>Fails with the <c>AlreadyShown</c> case when the page is already the current page.</remarks>
        public async UnityTask<Success<PageFlowError>> ShowAsync(
              [NotNull] TPage page
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.Abortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                var show = await ShowCoreAsync(page, context, abort, token);
                result = abort.MergeSoftErrors(show.Result);
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
        public async UnityTask<Success<PageFlowError>> ShowAsync(
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
                result = await ShowCoreAsync(factory, release, context, abort, token);
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
        /// <remarks>
        /// Fails with the <c>AlreadyShown</c> case when the page at <paramref name="index"/> is already the current
        /// page.
        /// </remarks>
        public async UnityTask<Success<PageFlowError>> ShowAsync(
              int index
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.Abortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                var show = await ShowCoreAsync(index, context, abort, token);
                result = abort.MergeSoftErrors(show.Result);
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

        internal async UnityTask<(Success<PageFlowError> Result, Option<TPage> Hidden)> ShowAndGetHiddenAsync(
              [NotNull] TPage page
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.Abortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                var show = await ShowCoreAsync(page, context, abort, token);
                result = abort.MergeSoftErrors(show.Result);
                show.Result = result;
                return show;
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

        private bool Contains(TPage page)
            => _pages.IndexOf(page) >= 0;

        private bool IsUnheld(TPage page)
            => Contains(page) == false && _tracker.IsAdding(page) == false;

        private bool IsCurrent(TPage page)
            => CurrentPage.TryGetValue(out var current) && ReferenceEquals(current, page);

        private bool TryRejectDuplicate(TPage page, out PageSettingLevel level)
        {
            level = default;

            if (_pages.IndexOf(page) < 0 && _tracker.IsAdding(page) == false)
            {
                return false;
            }

            if (_rejectDuplicatePages)
            {
                level = PageSettingLevel.Flow;
                return true;
            }

            if (page is IPageHasOptions hasOptions
                && hasOptions.PageOptions.rejectDuplicatePages.TryGetValue(out var reject)
                && reject
            )
            {
                level = PageSettingLevel.Page;
                return true;
            }

            return false;
        }

        private async UnityTask<Success<PageFlowError>> AddCoreAsync(
              [NotNull] TPage page
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNullOrUnityObjectInvalid(page);

            if (TryRejectDuplicate(page, out var level))
            {
                return PageFlowError.PageAlreadyAdded(page, level);
            }

            _tracker.BeginAdd(page);

            try
            {
                var result = await _flow.AttachAsync(this, page, context, token);

                if (_isDisposed)
                {
                    if (result.IsSuccess)
                    {
                        var disposedDetach = await _flow.ForceDetachAsync(this, page, context, token);
                        return PageFlow.WithSuppressedFailure(PageFlowError.Cancelled(), disposedDetach);
                    }

                    return PageFlowError.Cancelled();
                }

                if (result.IsFailure)
                {
                    return result;
                }

                if (token.IsCancellationRequested)
                {
                    var cancelledDetach = await _flow.ForceDetachAsync(this, page, context, token);
                    return PageFlow.WithSuppressedFailure(PageFlowError.Cancelled(), cancelledDetach);
                }

                _pages.Add(page);
                return Success.Yes;
            }
            finally
            {
                _tracker.EndAdd(page);
            }
        }

        private async UnityTask<Success<PageFlowError>> AddCoreAsync(
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
                result = await AddCoreAsync(page, context, abort, token);
            }
            catch
            {
                if (page != null && IsUnheld(page))
                {
                    PageFlow.ReleaseOnThrow(page, release, _logger);
                }

                throw;
            }

            if (result.TryGetFailure(out var error) && page != null && IsUnheld(page))
            {
                result = PageFlow.ReleaseOnFailure(in error, page, release);
            }

            return result;
        }

        private async UnityTask<Success<PageFlowError>> HideCoreAsync(
              PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            if (CurrentPage.TryGetValue(out var pageToHide) == false)
            {
                return PageFlowError.NoPage();
            }

            var index = _pages.IndexOf(pageToHide);

            if (index < 0)
            {
                return PageFlowError.PageNotFound(pageToHide);
            }

            if (_tracker.IsRemoving(pageToHide))
            {
                return PageFlowError.PageBeingRemoved(pageToHide);
            }

            if (index == context.DefaultIndex)
            {
                return PageFlowError.CannotHideDefaultPage(index);
            }

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

            index = _pages.IndexOf(pageToHide);

            if (index < 0)
            {
                return PageFlowError.PageNotFound(pageToHide);
            }

            if (_tracker.IsRemoving(pageToHide))
            {
                return PageFlowError.PageBeingRemoved(pageToHide);
            }

            if (IsCurrent(pageToHide) == false)
            {
                return PageFlowError.PageNotActive(pageToHide);
            }

            if (index == context.DefaultIndex)
            {
                return PageFlowError.CannotHideDefaultPage(index);
            }

            _aborts.EnterCall(abort, token);
            IsInTransition = true;

            var previousPage = CurrentPage;
            var pageToShow = (uint)context.DefaultIndex < (uint)_pages.Count ? _pages[context.DefaultIndex] : default;
            var hasCalledTransitionAsync = false;

            _tracker.BeginTransition(pageToHide, pageToShow);

            try
            {
                CurrentPage = default;

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
                        CurrentPage = previousPage;
                        return result;
                    }

                    abort.BeginRecovering(transitionError);
                }

                var hasTransitionBegun = _flow.HasTransitionBegun;

                CurrentPage = result.IsSuccess || hasTransitionBegun ? pageToShow : previousPage;
                return result;
            }
            catch
            {
                if (abort.IsRequested)
                {
                    CurrentPage = previousPage;
                    throw;
                }

                var hasTransitionBegun = hasCalledTransitionAsync && _flow.HasTransitionBegun;

                CurrentPage = hasTransitionBegun ? pageToShow : previousPage;
                throw;
            }
            finally
            {
                _tracker.EndTransition(pageToHide, pageToShow);
                IsInTransition = false;
            }
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

            var snapshot = new TPage[_pages.Count];
            var count = 0;

            for (var index = 0; index < snapshot.Length; index++)
            {
                var page = _pages[index];

                if (_tracker.IsRemoving(page))
                {
                    continue;
                }

                snapshot[count] = page;
                count++;
            }

            if (count < 1)
            {
                return PageFlowError.NoPage();
            }

            _aborts.EnterCall(abort, token);
            IsInTransition = true;

            for (var index = 0; index < count; index++)
            {
                _tracker.BeginRemove(snapshot[index]);
            }

            try
            {
                return await _flow.DetachAllAsync(this, snapshot, count, context, token);
            }
            finally
            {
                for (var index = 0; index < count; index++)
                {
                    _tracker.EndRemove(snapshot[index]);
                }

                if (CurrentPage.TryGetValue(out var current) && ContainsPage(snapshot, count, current))
                {
                    CurrentPage = default;
                }

                RemovePages(snapshot, count);
                IsInTransition = false;
            }
        }

        private static bool ContainsPage(TPage[] pages, int count, TPage page)
        {
            for (var index = 0; index < count; index++)
            {
                if (ReferenceEquals(pages[index], page))
                {
                    return true;
                }
            }

            return false;
        }

        private void RemovePages(TPage[] pages, int count)
        {
            for (var index = 0; index < count; index++)
            {
                _pages.Remove(pages[index]);
            }
        }

        private async UnityTask<Success<PageFlowError>> RemoveCoreAsync(
              [NotNull] TPage page
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNullOrUnityObjectInvalid(page);

            var index = _pages.IndexOf(page);

            if (index < 0)
            {
                return PageFlowError.PageNotFound(page);
            }

            if (_tracker.IsRemoving(page))
            {
                return PageFlowError.PageBeingRemoved(page);
            }

            if (_tracker.IsInTransition(page))
            {
                return PageFlowError.PageInTransition(page);
            }

            _tracker.BeginRemove(page);

            try
            {
                var result = await _flow.DetachAsync(this, page, context, token);

                if (result.IsFailure)
                {
                    return result;
                }

                if (token.IsCancellationRequested)
                {
                    return PageFlowError.Cancelled();
                }

                index = _pages.IndexOf(page);

                if (index < 0)
                {
                    return PageFlowError.PageNotFound(page);
                }

                _pages.RemoveAt(index);

                if (IsCurrent(page))
                {
                    CurrentPage = default;
                }

                return Success.Yes;
            }
            finally
            {
                _tracker.EndRemove(page);
            }
        }

        private async UnityTask<Success<PageFlowError>> RemoveCoreAsync(
              int index
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            if ((uint)index >= (uint)_pages.Count)
            {
                return PageFlowError.IndexOutOfRange(index, _pages.Count);
            }

            return await RemoveCoreAsync(_pages[index], context, abort, token);
        }

        private async UnityTask<(Success<PageFlowError> Result, Option<TPage> Hidden)> ShowCoreAsync(
              [NotNull] TPage page
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNullOrUnityObjectInvalid(page);

            var added = false;

            if (_pages.IndexOf(page) < 0)
            {
                var add = await AddCoreAsync(page, context, abort, token);

                if (add.IsFailure)
                {
                    return (add, Option.None);
                }

                added = true;
            }

            (Success<PageFlowError> Result, Option<TPage> Hidden) show;

            try
            {
                show = await ShowListedCoreAsync(page, context, abort, token);
            }
            catch
            {
                if (added && abort.IsRequested)
                {
                    try
                    {
                        var remove = await RemoveAddedPageAsync(
                              page: page
                            , context: context
                            , aborted: true
                            , token: token
                        );

                        if (remove.TryGetFailure(out var removeError))
                        {
                            removeError.Log(_logger);
                        }
                    }
                    catch (Exception inner)
                    {
                        _logger.LogException(inner);
                    }
                }

                throw;
            }

            if (added && show.Result.IsApplied() == false)
            {
                var error = show.Result.GetFailureOrThrow();
                var remove = await RemoveAddedPageAsync(page, context, error.IsAborted, token);

                return (PageFlow.WithSuppressedFailure(error, remove), Option.None);
            }

            return show;
        }

        private async UnityTask<Success<PageFlowError>> RemoveAddedPageAsync(
              TPage page
            , PageContext context
            , bool aborted
            , CancellationToken token
        )
        {
            if (Contains(page) == false || _tracker.IsRemoving(page))
            {
                return Success.Yes;
            }

            try
            {
                if (aborted)
                {
                    return await _flow.ForceDetachInAbortAsync(this, page, context, token);
                }

                return await _flow.ForceDetachAsync(this, page, context, token);
            }
            finally
            {
                var index = _pages.IndexOf(page);

                if (index >= 0)
                {
                    _pages.RemoveAt(index);
                }
            }
        }

        private async UnityTask<Success<PageFlowError>> ShowCoreAsync(
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
                    var show = await ShowCoreAsync(page, context, abort, token);
                    result = show.Result;
                }
            }
            catch
            {
                if (page != null && IsUnheld(page))
                {
                    PageFlow.ReleaseOnThrow(page, release, _logger);
                }

                throw;
            }

            if (result.TryGetFailure(out var error) && page != null && IsUnheld(page))
            {
                result = PageFlow.ReleaseOnFailure(in error, page, release);
            }

            return result;
        }

        private async UnityTask<(Success<PageFlowError> Result, Option<TPage> Hidden)> ShowCoreAsync(
              int index
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            var count = _pages.Count;

            if ((uint)index >= (uint)count)
            {
                return (PageFlowError.IndexOutOfRange(index, count), Option.None);
            }

            return await ShowCoreAsync(_pages[index], context, abort, token);
        }

        private async UnityTask<(Success<PageFlowError> Result, Option<TPage> Hidden)> ShowListedCoreAsync(
              TPage pageToShow
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            if (_tracker.IsRemoving(pageToShow))
            {
                return (PageFlowError.PageBeingRemoved(pageToShow), Option.None);
            }

            if (IsCurrent(pageToShow))
            {
                return (PageFlowError.AlreadyShown(_pages.IndexOf(pageToShow)), Option.None);
            }

            if (abort.IsRequested)
            {
                return (PageFlowError.Aborted(abort.RequestOrigin, abort.RequestMode), Option.None);
            }

            if (token.IsCancellationRequested)
            {
                return (PageFlowError.Cancelled(), Option.None);
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
                return (validation, Option.None);
            }

            if (token.IsCancellationRequested)
            {
                return (PageFlowError.Cancelled(), Option.None);
            }

            if (_pages.IndexOf(pageToShow) < 0)
            {
                return (PageFlowError.PageNotFound(pageToShow), Option.None);
            }

            if (_tracker.IsRemoving(pageToShow))
            {
                return (PageFlowError.PageBeingRemoved(pageToShow), Option.None);
            }

            if (IsCurrent(pageToShow))
            {
                return (PageFlowError.AlreadyShown(_pages.IndexOf(pageToShow)), Option.None);
            }

            _aborts.EnterCall(abort, token);
            IsInTransition = true;

            var previousPage = CurrentPage;
            var pageToHide = previousPage.GetValueOrDefault();
            var hasCalledTransitionAsync = false;

            _tracker.BeginTransition(pageToHide, pageToShow);

            try
            {
                hasCalledTransitionAsync = true;
                var result = await _flow.TransitionAsync(
                      this
                    , PageTransition.Show
                    , pageToHide
                    , pageToShow
                    , context
                    , token
                );

                if (result.TryGetFailure(out var transitionError))
                {
                    if (transitionError.IsAborted)
                    {
                        CurrentPage = previousPage;
                        return (result, Option.None);
                    }

                    abort.BeginRecovering(transitionError);
                }

                var hasTransitionBegun = _flow.HasTransitionBegun;
                var isChangeApplied = result.IsSuccess || hasTransitionBegun;

                CurrentPage = isChangeApplied ? pageToShow : previousPage;
                return (result, Option.SomeIf(isChangeApplied, previousPage));
            }
            catch
            {
                if (abort.IsRequested)
                {
                    CurrentPage = previousPage;
                    throw;
                }

                var hasTransitionBegun = hasCalledTransitionAsync && _flow.HasTransitionBegun;

                CurrentPage = hasTransitionBegun ? pageToShow : previousPage;
                throw;
            }
            finally
            {
                _tracker.EndTransition(pageToHide, pageToShow);
                IsInTransition = false;
            }
        }
    }
}
