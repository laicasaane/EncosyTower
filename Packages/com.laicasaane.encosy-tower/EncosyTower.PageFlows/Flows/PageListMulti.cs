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
    /// <summary>A page list that can show any number of its pages.</summary>
    /// <remarks>
    /// <para>
    /// A multi page list keeps no shown or hidden state: showing a shown page, or hiding a hidden one, runs its
    /// transition again. All page flow operations and callbacks run on the Unity main thread; see
    /// <see cref="IPageFlow"/>.
    /// </para>
    /// <para>Adding and removing pages are not abortable.</para>
    /// </remarks>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    public class PageListMulti<TPage> : IPageListMulti<TPage>, IDisposable
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
        public PageListMulti([NotNull] IPageFlowContext context) : this(context, backend: null)
        {
        }

        /// <summary>Creates an empty list.</summary>
        /// <param name="context">The context that supplies the services and settings of the flow.</param>
        /// <param name="backend">The platform backend, or <c>null</c> for no rendering integration.</param>
        /// <remarks>
        /// The list reads <see cref="IPageFlowContext.RejectDuplicatePages"/> once, when it is created.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
        public PageListMulti([NotNull] IPageFlowContext context, IPageFlowBackend backend)
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
        /// <remarks>
        /// A multi page list keeps no shown or hidden state: hiding a hidden page runs its transition again.
        /// </remarks>
        public async UnityTask<Success<PageFlowError>> HideAsync(
              [NotNull] TPage page
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.Abortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                result = await HideCoreAsync(page, context, abort, token);
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
        public async UnityTask<Success<PageFlowError>> HideAsync(
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
                result = await HideCoreAsync(factory, release, context, abort, token);
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
        /// A multi page list keeps no shown or hidden state: hiding a hidden page runs its transition again.
        /// </remarks>
        public async UnityTask<Success<PageFlowError>> HideAsync(
              int index
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.Abortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                result = await HideCoreAsync(index, context, abort, token);
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
        /// <remarks>
        /// A multi page list keeps no shown or hidden state: showing a shown page runs its transition again.
        /// </remarks>
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
                result = await ShowCoreAsync(page, context, abort, token);
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
        /// A multi page list keeps no shown or hidden state: showing a shown page runs its transition again.
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
                result = await ShowCoreAsync(index, context, abort, token);
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

        internal UnityTask<Success<PageFlowError>> HideAllAsync(
              TPage[] pages
            , int count
            , PageContext context
            , CancellationToken token
        )
            => TransitionAllAsync(PageTransition.Hide, pages, count, context, token);

        internal UnityTask<Success<PageFlowError>> ShowAllAsync(
              TPage[] pages
            , int count
            , PageContext context
            , CancellationToken token
        )
            => TransitionAllAsync(PageTransition.Show, pages, count, context, token);

        private async UnityTask<Success<PageFlowError>> TransitionAllAsync(
              PageTransition transition
            , TPage[] pages
            , int count
            , PageContext context
            , CancellationToken token
        )
        {
            var abort = BeginCall(ref context, PageAbortCallKind.Abortable);
            Success<PageFlowError> result = Success.Yes;

            try
            {
                result = await TransitionAllCoreAsync(transition, pages, count, context, abort, token);
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

        private bool Contains(TPage page)
            => _pages.IndexOf(page) >= 0;

        private bool IsUnheld(TPage page)
            => Contains(page) == false && _tracker.IsAdding(page) == false;

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
              [NotNull] TPage page
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNullOrUnityObjectInvalid(page);

            if (_pages.IndexOf(page) < 0)
            {
                return PageFlowError.PageNotFound(page);
            }

            return await HideListedCoreAsync(page, context, abort, token);
        }

        private async UnityTask<Success<PageFlowError>> HideCoreAsync(
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
                    result = await HideCoreAsync(page, context, abort, token);
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

        private async UnityTask<Success<PageFlowError>> HideCoreAsync(
              int index
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            var count = _pages.Count;

            if ((uint)index >= (uint)count)
            {
                return PageFlowError.IndexOutOfRange(index, count);
            }

            return await HideCoreAsync(_pages[index], context, abort, token);
        }

        private async UnityTask<Success<PageFlowError>> HideListedCoreAsync(
              TPage pageToHide
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            if (_tracker.IsRemoving(pageToHide))
            {
                return PageFlowError.PageBeingRemoved(pageToHide);
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

            if (_pages.IndexOf(pageToHide) < 0)
            {
                return PageFlowError.PageNotFound(pageToHide);
            }

            if (_tracker.IsRemoving(pageToHide))
            {
                return PageFlowError.PageBeingRemoved(pageToHide);
            }

            _aborts.EnterCall(abort, token);
            IsInTransition = true;

            _tracker.BeginTransition(pageToHide: pageToHide, pageToShow: default);

            try
            {
                var result = await _flow.TransitionAsync(
                      flow: this
                    , transition: PageTransition.Hide
                    , pageToHide: pageToHide
                    , pageToShow: default
                    , context: context
                    , token: token
                );

                if (result.TryGetFailure(out var transitionError) && transitionError.IsAborted == false)
                {
                    abort.BeginRecovering(transitionError);
                }

                return result;
            }
            finally
            {
                _tracker.EndTransition(pageToHide: pageToHide, pageToShow: default);
                IsInTransition = false;
            }
        }

        private async UnityTask<Success<PageFlowError>> TransitionAllCoreAsync(
              PageTransition transition
            , TPage[] pages
            , int count
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            if (count < 1)
            {
                return PageFlowError.NoPage();
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

            for (var index = 0; index < count; index++)
            {
                var page = pages[index];

                if (_pages.IndexOf(page) < 0)
                {
                    return PageFlowError.PageNotFound(page);
                }

                if (_tracker.IsRemoving(page))
                {
                    return PageFlowError.PageBeingRemoved(page);
                }
            }

            _aborts.EnterCall(abort, token);
            IsInTransition = true;

            _tracker.BeginTransition(pages, count);

            try
            {
                var result = await _flow.TransitionManyAsync(this, transition, pages, count, context, token);

                if (result.TryGetFailure(out var transitionError) && transitionError.IsAborted == false)
                {
                    abort.BeginRecovering(transitionError);
                }

                return result;
            }
            finally
            {
                _tracker.EndTransition(pages, count);
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

                RemovePages(snapshot, count);
                IsInTransition = false;
            }
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

        private async UnityTask<Success<PageFlowError>> ShowCoreAsync(
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
                    return add;
                }

                added = true;
            }

            Success<PageFlowError> result;

            try
            {
                result = await ShowListedCoreAsync(page, context, abort, token);
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

            if (added && result.IsApplied() == false)
            {
                var error = result.GetFailureOrThrow();
                var remove = await RemoveAddedPageAsync(page, context, error.IsAborted, token);

                return PageFlow.WithSuppressedFailure(error, remove);
            }

            return result;
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
                    result = await ShowCoreAsync(page, context, abort, token);
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

        private async UnityTask<Success<PageFlowError>> ShowCoreAsync(
              int index
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            var count = _pages.Count;

            if ((uint)index >= (uint)count)
            {
                return PageFlowError.IndexOutOfRange(index, count);
            }

            return await ShowCoreAsync(_pages[index], context, abort, token);
        }

        private async UnityTask<Success<PageFlowError>> ShowListedCoreAsync(
              TPage pageToShow
            , PageContext context
            , PageAbortState abort
            , CancellationToken token
        )
        {
            if (_tracker.IsRemoving(pageToShow))
            {
                return PageFlowError.PageBeingRemoved(pageToShow);
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

            if (_pages.IndexOf(pageToShow) < 0)
            {
                return PageFlowError.PageNotFound(pageToShow);
            }

            if (_tracker.IsRemoving(pageToShow))
            {
                return PageFlowError.PageBeingRemoved(pageToShow);
            }

            _aborts.EnterCall(abort, token);
            IsInTransition = true;

            _tracker.BeginTransition(pageToHide: default, pageToShow: pageToShow);

            try
            {
                var result = await _flow.TransitionAsync(
                      flow: this
                    , transition: PageTransition.Show
                    , pageToHide: default
                    , pageToShow: pageToShow
                    , context: context
                    , token: token
                );

                if (result.TryGetFailure(out var transitionError) && transitionError.IsAborted == false)
                {
                    abort.BeginRecovering(transitionError);
                }

                return result;
            }
            finally
            {
                _tracker.EndTransition(pageToHide: default, pageToShow: pageToShow);
                IsInTransition = false;
            }
        }
    }
}
