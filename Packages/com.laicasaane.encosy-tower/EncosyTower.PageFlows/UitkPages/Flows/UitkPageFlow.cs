using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.AssetKeys;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.Initialization;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.Flows;
using EncosyTower.PageFlows.Internals;
using EncosyTower.PageFlows.Loading;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages.Codex;
using EncosyTower.PageFlows.UitkPages.Internals;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using EncosyTower.Types;
using UnityEngine;
using UnityEngine.UIElements;

using ThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.UitkPages
{
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// The base class of a UI Toolkit page flow: it owns the page pools and the elements of the flow.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="UitkPageCodex"/> creates one flow for each flow definition with <see cref="TryCreate"/> and
    /// initializes it. A flow adds an element, <see cref="FlowElement"/>, to the panel. It holds the elements of its
    /// pages, and a <see cref="Blocker"/> that swallows pointer input while a page transitions.
    /// </para>
    /// <para>
    /// A flow loads the asset of a page when it first needs it, keeps the instances in a pool, and returns each
    /// page it removes to the pool, or destroys it when <see cref="PageContext.ReturnOperation"/> says so. After a
    /// show, the flow moves focus as <see cref="PageFocusMode"/> of the page says. All page flow operations and
    /// callbacks run on the Unity main thread; see <see cref="IPageFlow"/>.
    /// </para>
    /// </remarks>
    public abstract class UitkPageFlow : IIsInitialized, IDisposable, IPageFlowBackend
    {
        /// <summary>
        /// The USS class of the flow element. The element also has the class <c>FLOW_USS_CLASS_NAME--identifier</c>.
        /// </summary>
        public const string FLOW_USS_CLASS_NAME = "uitk-page-flow";

        /// <summary>The USS class of the blocker element.</summary>
        public const string BLOCKER_USS_CLASS_NAME = FLOW_USS_CLASS_NAME + "__blocker";

        internal const string ADDRESSABLES_NOT_INSTALLED_MESSAGE
            = "Addressables is not installed. UI Toolkit Page Loader will use Resources instead.";

        private static int s_flowSerial;

        private readonly Dictionary<AssetKey, UitkPagePool> _pools = new();
        private readonly Dictionary<IUitkPage, UitkPageSlot> _activeSlots = new();
        private readonly List<ISubscription> _subscriptions = new();
        private readonly List<ProcessRegistry> _processRegistries = new();
        private readonly Stack<Focusable> _focusHistory = new();

        private Transform _behaviourParent;
        private int _blockingCount;

        /// <summary>Gets whether <see cref="Initialize"/> ran and the flow is not disposed.</summary>
        public bool IsInitialized { get; private set; }

        /// <summary>Gets the identifier of this flow: the identifier of its definition.</summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Gets the name of the panel element that holds the flow element, or empty for the codex root.
        /// </summary>
        public string ContainerName { get; private set; }

        /// <summary>Gets the codex that created the flow.</summary>
        public UitkPageCodex Codex { get; private set; }

        /// <summary>Gets the context of the flow.</summary>
        public UitkPageFlowContext Context { get; private set; }

        /// <summary>Gets the element that holds the pages of the flow.</summary>
        public VisualElement FlowElement { get; private set; }

        /// <summary>Gets the element that swallows pointer input while a page transitions.</summary>
        public VisualElement Blocker { get; private set; }

        /// <summary>Gets whether the flow is running a transition.</summary>
        public abstract bool IsInTransition { get; }

        /// <summary>Creates a flow of a kind.</summary>
        /// <param name="kind">The kind of the flow.</param>
        /// <param name="flow">The new flow, not initialized.</param>
        /// <returns><c>true</c> when the kind has a flow type; otherwise <c>false</c>.</returns>
        public static bool TryCreate(PageFlowKind kind, out UitkPageFlow flow)
        {
            flow = kind switch {
                PageFlowKind.PageStackSingle => new UitkPageStackSingle(),
                PageFlowKind.PageStackMulti => new UitkPageStackMulti(),
                PageFlowKind.PageListSingle => new UitkPageListSingle(),
                PageFlowKind.PageListMulti => new UitkPageListMulti(),
                _ => null,
            };

            return flow != null;
        }

        /// <summary>Initializes the flow: gives it a scope and its elements, and subscribes its messages.</summary>
        /// <param name="identifier">The identifier of the flow.</param>
        /// <param name="containerName">The name of the panel element that holds the flow element, or empty.</param>
        /// <param name="codex">The codex that owns the flow.</param>
        /// <param name="context">The context of the flow.</param>
        /// <param name="behaviourParent">The parent of the page behaviours the flow creates.</param>
        /// <exception cref="System.InvalidOperationException">The flow is already initialized.</exception>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="identifier"/>, <paramref name="codex"/> or <paramref name="context"/> is <c>null</c> or
        /// destroyed.
        /// </exception>
        public void Initialize(
              string identifier
            , string containerName
            , [NotNull] UitkPageCodex codex
            , [NotNull] UitkPageFlowContext context
            , Transform behaviourParent
        )
        {
            ThrowIfInitialized(IsInitialized, this);

            ThrowHelper.ThrowIfNull(identifier);
            ThrowHelper.ThrowIfNullOrUnityObjectInvalid(codex);
            ThrowHelper.ThrowIfNull(context);

            Identifier = identifier;
            ContainerName = containerName;
            Codex = codex;
            Context = context;
            _behaviourParent = behaviourParent;

            context.Owner = codex;
            context.FlowScope = new PageFlowScope(new Id3(
                  (Id<UitkPageFlow>)Type<UitkPageFlow>.Id
                , Interlocked.Increment(ref s_flowSerial)
                , 0
            ));

            FlowElement = CreateFlowElement(identifier);
            Blocker = CreateBlocker();
            FlowElement.Add(Blocker);

            var initContext = new InitializationContext(
                  context.Subscriber.Scope(context.FlowScope).WithSubscriptions(_subscriptions)
                , context.ProcessHub.WithRegistries(_processRegistries)
            );

            SubscribeMessages(initContext);
            OnInitialize(initContext);
            IsInitialized = true;
        }

        /// <summary>Unsubscribes the flow, disposes its pools and removes its element from the panel.</summary>
        /// <remarks>Disposing a pool returns its active pages to it and releases the page assets.</remarks>
        public void Dispose()
        {
            _subscriptions.AsListFast().Unsubscribe();
            _processRegistries.AsListFast().Unregister();

            OnDispose();

            foreach (var pool in _pools.Values)
            {
                pool.Dispose();
            }

            _pools.Clear();
            _activeSlots.Clear();
            _focusHistory.Clear();
            FlowElement?.RemoveFromHierarchy();
            IsInitialized = false;
        }

        /// <summary>Preloads instances of a page and keeps them in the pool.</summary>
        /// <param name="pageAssetKey">The key of the page asset.</param>
        /// <param name="amount">The number of pooled instances to have ready; at least 1.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the pool holds the amount; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>
        /// The flow creates only the missing instances. It loads the asset first when it is not loaded.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="pageAssetKey"/> is not valid (default or empty).
        /// </exception>
        public async UnityTask<Success<PageFlowError>> PrepoolPageAsync(
              AssetKey pageAssetKey
            , int amount
            , CancellationToken token
        )
        {
            ThrowHelper.ThrowIfNullOrEmpty(pageAssetKey.Value, nameof(pageAssetKey));

            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            if (amount < 1)
            {
                return PageFlowError.InvalidPrepoolAmount(amount);
            }

            var poolResult = await GetInitializedPoolAsync(pageAssetKey, token);

            if (poolResult.TryGetValue(out var pool) == false)
            {
                return poolResult.GetErrorOrDefault();
            }

            if (IsInitialized == false || token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            var differentAmount = amount - pool.PoolingCount;

            if (differentAmount > 0)
            {
                return pool.Prepool(differentAmount);
            }

            return Success.Yes;
        }

        /// <summary>Keeps a certain amount of instances of a page in the pool, and destroys the others.</summary>
        /// <param name="pageAssetKey">The key of the page asset.</param>
        /// <param name="amountToKeep">The number of pooled instances to keep. A negative value keeps none.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="pageAssetKey"/> is not valid (default or empty).
        /// </exception>
        public void TrimPool(AssetKey pageAssetKey, int amountToKeep)
        {
            ThrowHelper.ThrowIfNullOrEmpty(pageAssetKey.Value, nameof(pageAssetKey));

            if (_pools.TryGetValue(pageAssetKey, out var pool))
            {
                pool.Trim(Mathf.Max(amountToKeep, 0));
            }
        }

        /// <summary>Gets the number of pooled instances of a page.</summary>
        /// <param name="pageAssetKey">The key of the page asset.</param>
        /// <param name="count">The number of pooled instances.</param>
        /// <returns><c>true</c> when the flow has a pool for the asset; otherwise <c>false</c>.</returns>
        public bool TryGetPooledCount(AssetKey pageAssetKey, out int count)
        {
            if (_pools.TryGetValue(pageAssetKey, out var pool))
            {
                count = pool.PoolingCount;
                return true;
            }

            count = 0;
            return false;
        }

        /// <summary>
        /// Called once when the flow initializes, after it subscribed to the messages every flow handles.
        /// </summary>
        /// <param name="context">The subscriber and the process hub of the flow, scoped to the flow.</param>
        /// <remarks>Subscribe the messages and register the requests of the flow type.</remarks>
        protected abstract void OnInitialize(in InitializationContext context);

        /// <summary>Called when the flow is disposed, before its pools are disposed.</summary>
        protected abstract void OnDispose();

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack]
        internal static void ErrorIfAddressablesNotInstalled(
              PageLoaderStrategy strategy
            , bool isAddressablesAvailable
            , EncosyTower.Logging.ILogger logger
        )
        {
            if (strategy == PageLoaderStrategy.Addressables && isAddressablesAvailable == false)
            {
                logger.LogError(ADDRESSABLES_NOT_INSTALLED_MESSAGE);
            }
        }

        internal bool TryGetSlot(IUitkPage page, out UitkPageSlot slot)
        {
            if (page == null)
            {
                slot = null;
                return false;
            }

            return _activeSlots.TryGetValue(page, out slot);
        }

        internal async UnityTask<Result<UitkPageSlot, PageFlowError>> RentPageAsync(
              AssetKey pageAssetKey
            , PageContext context
            , PageContext flowContext
            , CancellationToken token
        )
        {
            ThrowHelper.ThrowIfNullOrEmpty(pageAssetKey.Value, nameof(pageAssetKey));
            ThrowIfNotInitialized(IsInitialized, this);

            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            var poolResult = await GetInitializedPoolAsync(pageAssetKey, token);

            if (poolResult.TryGetValue(out var pool) == false)
            {
                return poolResult.GetErrorOrDefault();
            }

            if (token.IsCancellationRequested || IsInitialized == false)
            {
                return PageFlowError.Cancelled();
            }

            var rentResult = pool.Rent();

            if (rentResult.TryGetValue(out var slot) == false)
            {
                return rentResult.GetErrorOrDefault();
            }

            _activeSlots[slot.Page] = slot;

            if (slot.Page is not IPageOnCreateAsync onCreate)
            {
                return slot;
            }

            bool isPageCreationAccepted;

            try
            {
                isPageCreationAccepted = await onCreate.OnCreateAsync(
                      PageFlow.WithPageOrigin(flowContext, slot.Page)
                    , token
                );
            }
            catch (Exception ex)
            {
                ReturnPageOrLog(slot.Page, context);

                if (ex is OperationCanceledException)
                {
                    throw;
                }

                return PageFlowError.Exception(ex);
            }

            if (IsInitialized == false)
            {
                return PageFlowError.Cancelled();
            }

            if (isPageCreationAccepted == false)
            {
                ReturnPageOrLog(slot.Page, context);
                return PageFlowError.CreateRejected(pageAssetKey);
            }

            if (token.IsCancellationRequested)
            {
                ReturnPageOrLog(slot.Page, context);
                return PageFlowError.Cancelled();
            }

            return slot;
        }

        internal void EnsureInitialized()
            => ThrowIfNotInitialized(IsInitialized, this);

        internal void PublishFailureOrAbort(in Success<PageFlowError> result, PageContext context)
            => PublishFailureOrAbort(result: result, context: context, unansweredError: default);

        internal void PublishFailureOrAbort(
              in Success<PageFlowError> result
            , PageContext context
            , Option<PageFlowError> unansweredError
        )
        {
            if (result.TryGetFailure(out var error) == false)
            {
                return;
            }

            var publisher = Context.Publisher.Scope(Context.FlowScope);

            if (error.TryGetAbort(out var origin, out var mode))
            {
                PageFlow.PublishAndForget(
                      publisher: publisher
                    , message: new PageAbortedMessage(origin, mode, error, context)
                    , logger: Context.Logger
                    , unansweredError: default
                );

                return;
            }

            if ((error.IsCancelled || error.IsSubscriberNotFound) && error.HasSuppressedFailure() == false)
            {
                return;
            }

            PageFlow.PublishAndForget(
                  publisher: publisher
                , message: new PageFlowFailedMessage(error, context)
                , logger: Context.Logger
                , unansweredError: unansweredError
            );
        }

        internal PageRef MakePageRef(IUitkPage page, Option<int> index)
        {
            if (page == null)
            {
                return PageRef.None;
            }

            return TryGetSlot(page, out var slot)
                ? new PageRef(page, slot.AssetKey, index)
                : new PageRef(Page: page, AssetKey: default, Index: index);
        }

        internal async UnityTask<Success<PageFlowError>> PublishNavigatedAsync(
              PageNavigationKind kind
            , PageRef from
            , PageRef to
            , PageContext context
            , Success<PageFlowError> result
        )
        {
            var navigation = new PageNavigation(Identifier, kind, from, to, context, context.IssuedBy);
            var message = new PageNavigatedMessage(navigation);
            var publisher = Context.Publisher.Scope(Context.FlowScope);

            var publishingContext = PublishingContext.Default(logger: Context.Logger, token: CancellationToken.None);
            PageFlowError error;

            try
            {
                var published = await PageNavigatedMessage.Async.Publish(in publisher, message, publishingContext);

                if (published.TryGetFailure(out var publishError) == false)
                {
                    return result;
                }

                if (publishError.IsSubscriberNotFound)
                {
                    return Context.WarnNoSubscriber
                        ? PageFlow.MergeSoft(result, PageFlowError.SubscriberNotFound(typeof(PageNavigatedMessage)))
                        : result;
                }

                error = PageFlowError.FromPublish(publishError);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                error = PageFlowError.Exception(ex);
            }

            return PageFlow.MergeFailure(result, error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ReportCall(in Success<PageFlowError> result, PageContext context)
            => context.Receipt?.Report(Identifier, result);

        /// <summary>Reports an exception of a call to its receipt.</summary>
        /// <param name="ex">The exception. A cancellation is reported as the <c>Cancelled</c> case.</param>
        /// <param name="context">The context of the call. Nothing is reported when it has no receipt.</param>
        protected void ReportException(Exception ex, PageContext context)
        {
            Success<PageFlowError> failure = ex is OperationCanceledException
                ? PageFlowError.Cancelled()
                : PageFlowError.Exception(ex);

            ReportCall(failure, context);
        }

        // B4 (D85): reports a call that threw. The receipt gets the Cancelled or Exception case with the errors the
        // flow set aside while the cancellation won. A thrown cancel that carries a failure is also published; with no
        // receipt, a PageFlowFailedMessage that nobody hears logs those errors (D28). A thrown exception is not
        // published (as today); with no receipt its set-aside errors are logged. A receipt that was already reported
        // counts as no receipt, so the errors are not lost.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void ReportThrownCall(Exception ex, PageContext context, PageAbortState abort)
        {
            var isCancel = ex is OperationCanceledException;
            var failure = isCancel ? PageFlowError.Cancelled() : PageFlowError.Exception(ex);
            var setAside = abort?.SetAsideErrors;
            var count = setAside?.Count ?? 0;

            if (count < 1)
            {
                ReportCall(failure, context);
                return;
            }

            var receipt = context.Receipt;
            var wasReported = receipt != null && receipt.IsReported;
            failure = PageFlow.WithSuppressedErrors(failure, setAside);
            ReportCall(failure, context);
            var hasReceipt = receipt != null && wasReported == false;

            if (isCancel)
            {
                Option<PageFlowError> unanswered = default;

                if (hasReceipt == false)
                {
                    unanswered = PageFlow.JoinErrors(setAside);
                }

                PublishFailureOrAbort(failure, context, unanswered);
                return;
            }

            if (hasReceipt == false)
            {
                for (var i = 0; i < count; i++)
                {
                    setAside[i].Log(Context.Logger);
                }
            }
        }

        internal void ReturnPageOrLog(IUitkPage page, PageContext context)
        {
            if (ReturnPage(page, context).TryGetFailure(out var error))
            {
                error.Log(Context.Logger);
            }
        }

        internal Success<PageFlowError> ReturnPage(IUitkPage page, PageContext context)
        {
            if (page == null || _activeSlots.Remove(page, out var slot) == false)
            {
                return PageFlowError.PageNotActive(page);
            }

            BlurIfFocusedInside(slot);

            if (page is IPageOnReturnToPool onReturn)
            {
                onReturn.OnReturnToPool(context);
            }

            if (_pools.TryGetValue(slot.AssetKey, out var pool) == false)
            {
                return PageFlowError.PoolNotFound(slot.AssetKey);
            }

            if (context.ReturnOperation == PageReturnOperation.Destroy)
            {
                pool.Destroy(slot);
                return Success.Yes;
            }

            pool.Return(slot);
            return Success.Yes;
        }

        void IPageFlowBackend.ActivatePageRendering(IPage page)
        {
            if (page is IUitkPage uitkPage && _activeSlots.TryGetValue(uitkPage, out var slot))
            {
                ShowSlot(slot);
            }
        }

        internal void ShowSlot(UitkPageSlot slot)
        {
            var container = slot.Container;
            container.style.display = DisplayStyle.Flex;

            if (container.parent == FlowElement)
            {
                container.PlaceBehind(Blocker);
            }
        }

        internal void HideSlot(UitkPageSlot slot)
        {
            BlurIfFocusedInside(slot);
            slot.Container.style.display = DisplayStyle.None;
        }

        internal void BeginBlocking()
        {
            _blockingCount++;
            Blocker.style.display = DisplayStyle.Flex;
        }

        internal void EndBlocking()
        {
            _blockingCount = Mathf.Max(_blockingCount - 1, 0);

            if (_blockingCount < 1)
            {
                Blocker.style.display = DisplayStyle.None;
            }
        }

        internal void RecordFocus()
        {
            _focusHistory.Push(FlowElement.panel?.focusController?.focusedElement);
        }

        internal void RestoreFocus()
        {
            if (_focusHistory.TryPop(out var focusable) && focusable is VisualElement element && IsDisplayed(element))
            {
                element.Focus();
            }
        }

        internal static void FillParent(IStyle style)
        {
            style.position = Position.Absolute;
            style.left = 0f;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
        }

        internal static void ApplyFocus(UitkPageSlot slot)
        {
            if (slot.Page is not IUitkPageHasFocus page)
            {
                return;
            }

            switch (page.FocusMode)
            {
                case PageFocusMode.FirstFocusable:
                {
                    var target = slot.Container.Query<VisualElement>()
                        .Where(static element => element.focusable && element.canGrabFocus)
                        .First();

                    target?.Focus();
                    break;
                }

                case PageFocusMode.Named:
                {
                    if (string.IsNullOrEmpty(page.FocusElementName) == false)
                    {
                        slot.Container.Q(page.FocusElementName)?.Focus();
                    }

                    break;
                }
            }
        }

        private static void BlurIfFocusedInside(UitkPageSlot slot)
        {
            var container = slot.Container;

            if (container.panel?.focusController?.focusedElement is VisualElement focused
                && (focused == container || container.Contains(focused))
            )
            {
                focused.Blur();
            }
        }

        private static bool IsDisplayed(VisualElement element)
        {
            if (element.panel == null)
            {
                return false;
            }

            for (var current = element; current != null; current = current.hierarchy.parent)
            {
                if (current.resolvedStyle.display == DisplayStyle.None)
                {
                    return false;
                }
            }

            return true;
        }

        private static VisualElement CreateFlowElement(string identifier)
        {
            var element = new VisualElement {
                name = identifier,
                pickingMode = PickingMode.Ignore,
            };

            element.AddToClassList(FLOW_USS_CLASS_NAME);
            element.AddToClassList($"{FLOW_USS_CLASS_NAME}--{identifier}");
            FillParent(element.style);
            return element;
        }

        private static VisualElement CreateBlocker()
        {
            var element = new VisualElement { pickingMode = PickingMode.Position };
            element.AddToClassList(BLOCKER_USS_CLASS_NAME);
            FillParent(element.style);
            element.style.display = DisplayStyle.None;
            return element;
        }

        private async UnityTask<Result<UitkPagePool, PageFlowError>> GetInitializedPoolAsync(
              AssetKey pageAssetKey
            , CancellationToken token
        )
        {
            if (_pools.TryGetValue(pageAssetKey, out var pool) == false)
            {
                pool = new UitkPagePool(pageAssetKey, FlowElement, Blocker, _behaviourParent);
                _pools[pageAssetKey] = pool;
            }

            if (pool.IsInitialized)
            {
                return pool;
            }

            ErrorIfAddressablesNotInstalled(
                  Context.LoadStrategy
                , PageAssetLoader.IsAddressablesAvailable
                , Context.Logger
            );

            var load = await PageAssetLoader.LoadAsync<UnityObject>(Context.LoadStrategy, pageAssetKey, token);

            if (IsInitialized == false)
            {
                pool.Dispose();

                if (load.TryGetValue(out var unusedLease))
                {
                    unusedLease.Release();
                }

                return PageFlowError.Cancelled();
            }

            if (token.IsCancellationRequested)
            {
                if (load.TryGetValue(out var unusedLease))
                {
                    unusedLease.Release();
                }

                return PageFlowError.Cancelled();
            }

            if (load.TryGetValue(out var lease) == false)
            {
                return PageFlowError.AssetLoadFailed(pageAssetKey, load.GetErrorOrDefault());
            }

            var initialization = pool.Initialize(lease);

            if (initialization.TryGetFailure(out var reason))
            {
                // The pool has released the lease.
                return PageFlowError.AssetLoadFailed(pageAssetKey, reason);
            }

            return pool;
        }

        private void SubscribeMessages(in InitializationContext context)
        {
            var subscriber = context.Subscriber.WithState(this);
            PrepoolPageMessage.Async.Subscribe(in subscriber, HandleAsync);
            TrimPoolMessage.Subscribe(in subscriber, Handle);
        }

        private static async UnityTask HandleAsync(
              UitkPageFlow flow
            , PrepoolPageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (PrepoolPageMessage)msg;
            var result = await flow.PrepoolPageAsync(message.AssetKey, message.Amount, context.Token);

            flow.PublishFailureOrAbort(in result, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Handle(UitkPageFlow flow, TrimPoolMessage msg)
            => flow.TrimPool(msg.AssetKey, msg.AmountToKeep);

        [HideInCallstack, StackTraceHidden]
        private static void ThrowIfNotInitialized([DoesNotReturnIf(false)] bool isInitialized, UitkPageFlow context)
        {
            if (isInitialized == false)
            {
                throw CreateException(context);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(UitkPageFlow context)
                => new($"The page flow '{context.Identifier}' must be initialized by its {nameof(UitkPageCodex)}.");
        }

        [HideInCallstack, StackTraceHidden]
        private static void ThrowIfInitialized([DoesNotReturnIf(true)] bool isInitialized, UitkPageFlow context)
        {
            if (isInitialized)
            {
                throw CreateException(context);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(UitkPageFlow context)
                => new($"The page flow '{context.Identifier}' is already initialized; " +
                    $"its {nameof(UitkPageCodex)} initializes it once.");
        }

        /// <summary>What a flow type needs to subscribe its messages and register its requests.</summary>
        /// <param name="Subscriber">The subscriber, scoped to the flow.</param>
        /// <param name="ProcessHub">The process hub, scoped to the flow.</param>
        protected readonly record struct InitializationContext(
              MessageSubscriber.Subscriber<PageFlowScope> Subscriber
            , Processor.Hub<PageFlowScope> ProcessHub
        );
    }
}
