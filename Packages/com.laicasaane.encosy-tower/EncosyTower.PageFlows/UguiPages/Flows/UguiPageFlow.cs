#if UNITY_UGUI

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.AssetKeys;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Initialization;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Flows;
using EncosyTower.PageFlows.Internals;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UguiPages.Internals;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.StringIds;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.UI;

using ThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.UguiPages
{
    /// <summary>The base component of a uGUI page flow: it owns the page pools and the services of the flow.</summary>
    /// <remarks>
    /// <para>
    /// A <see cref="UguiPageCodex"/> creates one flow for each flow definition. A flow can also be created with
    /// <see cref="Create{T}"/>, or added to a GameObject and initialized with <see cref="Initialize"/>.
    /// </para>
    /// <para>
    /// A flow loads the asset of a page when it first needs it, keeps the instances in a pool, and returns each page it
    /// removes to the pool, or destroys it when <see cref="PageContext.ReturnOperation"/> says so. All page flow
    /// operations and callbacks run on the Unity main thread; see <see cref="IPageFlow"/>.
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UguiPageFlow : MonoBehaviour, IIsInitialized, IPageFlowBackend
    {
        [SerializeField] internal UguiPageFlowContext _context = new();

        private readonly Dictionary<StringId, UguiPagePool> _pageIdToPool = new();
        private readonly List<UguiPageIdentifier> _pageIds = new();
        private readonly List<ISubscription> _subscriptions = new();
        private readonly List<ProcessRegistry> _processRegistries = new();

        /// <summary>Gets whether <see cref="Initialize"/> ran.</summary>
        public bool IsInitialized { get; private set; }

        /// <summary>The identifier of this flow: its codex identifier, or the GameObject name.</summary>
        public string Identifier { get; private set; }

        /// <summary>Gets or sets the function that supplies the publisher when the flow initializes.</summary>
        /// <value>Set it before <see cref="Initialize"/>. <c>null</c> uses the global publisher.</value>
        public Func<MessagePublisher> GetPublisher { get; set; }

        /// <summary>Gets or sets the function that supplies the subscriber when the flow initializes.</summary>
        /// <value>Set it before <see cref="Initialize"/>. <c>null</c> uses the global subscriber.</value>
        public Func<MessageSubscriber> GetSubscriber { get; set; }

        /// <summary>Gets or sets the function that supplies the processor when the flow initializes.</summary>
        /// <value>Set it before <see cref="Initialize"/>. <c>null</c> uses the global processor.</value>
        public Func<Processor> GetProcessor { get; set; }

        /// <summary>Gets the context of the flow.</summary>
        public UguiPageFlowContext Context => _context;

        /// <summary>Gets the rect transform of the flow root.</summary>
        public RectTransform RectTransform { get; private set; }

        /// <summary>Gets the canvas of the flow root.</summary>
        public Canvas Canvas { get; private set; }

        /// <summary>Gets the canvas group of the flow root.</summary>
        public CanvasGroup CanvasGroup { get; private set; }

        /// <summary>Gets the pool root: the parent of the pages that wait in the pool.</summary>
        /// <remarks>The pool root has a hidden canvas group that does not block raycasts.</remarks>
        public RectTransform PoolTransform { get; private set; }

        /// <summary>
        /// Creates the flow root under <paramref name="parent"/> and initializes it with the same parent,
        /// so the pool root is placed under <paramref name="parent"/> too.
        /// </summary>
        /// <remarks>
        /// If the caller later moves the flow root, it must move <see cref="PoolTransform"/> along with it,
        /// so the pool root keeps the same parent and rect as the flow root.
        /// </remarks>
        /// <typeparam name="T">The type of the flow.</typeparam>
        /// <param name="name">The name of the GameObject, which is also the identifier of the flow.</param>
        /// <param name="parent">The parent of the flow root and of the pool root.</param>
        /// <param name="context">The context of the flow.</param>
        /// <returns>The flow.</returns>
        public static T Create<T>(string name, RectTransform parent, UguiPageFlowContext context)
            where T : UguiPageFlow
        {
            var root = new GameObject(name, typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasGroup));

            var rectTransform = root.GetOrAddComponent<RectTransform>();
            rectTransform.FillParent(parent);

            var flow = root.GetOrAddComponent<T>();
            flow.Initialize(parent, context, name);

            return flow;
        }

        /// <summary>
        /// Initializes the flow and creates its pool root: under <paramref name="parent"/> when it is valid,
        /// otherwise under the rect of this flow.
        /// </summary>
        /// <remarks>
        /// A rented page waits under the pool root, and is laid out there during
        /// <see cref="IPageOnCreateAsync.OnCreateAsync"/>, until its show transition starts.
        /// A caller that moves the flow root must move <see cref="PoolTransform"/> along with it
        /// and keep the pool root's rect matching the flow root's rect.
        /// </remarks>
        /// <param name="parent">
        /// The parent of the pool root. When it is not valid, the pool root goes under this flow.
        /// </param>
        /// <param name="context">The context of the flow. <c>null</c> uses the serialized context.</param>
        /// <param name="identifier">The identifier of the flow. Empty uses the name of the GameObject.</param>
        /// <exception cref="System.InvalidOperationException">The flow is already initialized.</exception>
        public void Initialize(
              RectTransform parent = default
            , UguiPageFlowContext context = default
            , string identifier = null
        )
        {
            ThrowIfInitialized(IsInitialized, this);

            RectTransform = this.GetOrAddComponent<RectTransform>();
            Canvas = this.GetOrAddComponent<Canvas>();
            CanvasGroup = this.GetOrAddComponent<CanvasGroup>();

            CreateRootForPooling(parent.IsValid() ? parent : RectTransform);

            context ??= _context;
            _context = context;

            Identifier = string.IsNullOrEmpty(identifier) ? gameObject.name : identifier;
            context.Owner = this;

            if (context.IsInitialized == false)
            {
                var settings = UguiPageFlowSettings.Instance;
                var subscriber = GetSubscriber?.Invoke();
                var publisher = GetPublisher?.Invoke();
                var processor = GetProcessor?.Invoke();

                context.Initialize(subscriber, publisher, processor, settings);
            }

            var initContext = new InitializationContext(
                  context.Subscriber.Scope(context.FlowScope).WithSubscriptions(_subscriptions)
                , context.ProcessHub.WithRegistries(_processRegistries)
            );

            SubscribeMessages(initContext);
            OnInitialize(initContext);

            IsInitialized = true;
        }

        /// <summary>
        /// Called once when the flow initializes, after it subscribed to the messages every flow handles.
        /// </summary>
        /// <param name="context">The subscriber and the process hub of the flow, scoped to the flow.</param>
        /// <remarks>Subscribe the messages and register the requests of the flow type.</remarks>
        protected abstract void OnInitialize(in InitializationContext context);

        /// <summary>Initializes the flow when <c>autoInitializeOnAwake</c> of its context is on.</summary>
        protected void Awake()
        {
            if (_context.autoInitializeOnAwake)
            {
                var rectTransform = GetComponent<RectTransform>();
                Initialize(rectTransform, _context);
            }
        }

        /// <summary>Unsubscribes the flow, calls <see cref="OnDispose"/> and disposes the pools.</summary>
        /// <remarks>Disposing a pool returns its active pages to it and releases the page assets.</remarks>
        protected void OnDestroy()
        {
            _subscriptions.AsListFast().Unsubscribe();
            _processRegistries.AsListFast().Unregister();
            _pageIds.Clear();

            OnDispose();

            foreach (var pool in _pageIdToPool.Values)
            {
                pool.Dispose();
            }

            _pageIdToPool.Clear();

            var poolTransform = PoolTransform;

            if (poolTransform.IsValid() && poolTransform.IsChildOf(transform) == false)
            {
                Destroy(poolTransform.gameObject);
            }
        }

        /// <summary>Called when the flow is destroyed, before its pools are disposed.</summary>
        protected abstract void OnDispose();

        /// <summary>
        /// Preloads an amount of page instances and keeps them in the pool.
        /// </summary>
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
        /// <exception cref="System.InvalidOperationException">The flow is not initialized.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityTask<Success<PageFlowError>> PrepoolPageAsync(
              AssetKey pageAssetKey
            , int amount
            , CancellationToken token
        )
        {
            var pageKey = MakePageKey(pageAssetKey);
            return PrepoolPageAsync(pageKey, amount, token);
        }

        /// <summary>
        /// Keeps a certain amount of pages in the pool and destroys the others.
        /// </summary>
        /// <param name="pageAssetKey">The key of the page asset.</param>
        /// <param name="amountToKeep">The number of pooled instances to keep.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="pageAssetKey"/> is not valid (default or empty).
        /// </exception>
        /// <exception cref="System.InvalidOperationException">The flow is not initialized.</exception>
        public void TrimPool(AssetKey pageAssetKey, int amountToKeep)
        {
            var pageKey = MakePageKey(pageAssetKey);
            TrimPool(pageKey, amountToKeep);
        }

        /// <summary>Creates the key of a page asset.</summary>
        /// <param name="pageAssetKey">The key of the page asset.</param>
        /// <returns>The key, with its string id.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="pageAssetKey"/> is not valid (default or empty).
        /// </exception>
        /// <exception cref="System.InvalidOperationException">The flow is not initialized.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected PageKey MakePageKey(AssetKey pageAssetKey)
        {
            ThrowHelper.ThrowIfNullOrEmpty(pageAssetKey.Value, nameof(pageAssetKey));

            ThrowIfNotInitialized(IsInitialized, this);

            var id = Context.MakeStringId(pageAssetKey.Value);
            return new(pageAssetKey, id);
        }

        /// <summary>
        /// Preloads an amount of page instances and keeps them in the pool.
        /// </summary>
        /// <param name="pageKey">The key of the page asset.</param>
        /// <param name="amount">The number of pooled instances to have ready; at least 1.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// Success when the pool holds the amount; otherwise a failure; see <see cref="PageFlowError"/>.
        /// </returns>
        protected async UnityTask<Success<PageFlowError>> PrepoolPageAsync(
              PageKey pageKey
            , int amount
            , CancellationToken token
        )
        {
            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            if (amount < 1)
            {
                return PageFlowError.InvalidPrepoolAmount(amount);
            }

            var pool = GetPoolFor(pageKey);

            if (pool.PoolingCount >= amount)
            {
                return Success.Yes;
            }

            var poolResult = await InitializePoolAsync(pool, pageKey, token);

            if (poolResult.IsFailure)
            {
                return poolResult;
            }

            var differentAmount = amount - pool.PoolingCount;

            if (differentAmount < 1)
            {
                return Success.Yes;
            }

            pool.Prepool(differentAmount);
            return Success.Yes;
        }

        /// <summary>
        /// Keeps a certain amount of pages in the pool and destroys the others.
        /// </summary>
        /// <param name="pageKey">The key of the page asset.</param>
        /// <param name="amountToKeep">The number of pooled instances to keep.</param>
        protected void TrimPool(PageKey pageKey, int amountToKeep)
        {
            if (_pageIdToPool.TryGetValue(pageKey.Id, out var pool) == false)
            {
                return;
            }

            var amountToDestroy = pool.PoolingCount - Mathf.Clamp(amountToKeep, 0, pool.PoolingCount);

            if (amountToDestroy < 1)
            {
                return;
            }

            pool.Destroy(amountToDestroy);
        }

        /// <summary>Rents a page of an asset from its pool and calls its <c>OnCreateAsync</c> hook.</summary>
        /// <param name="pageKey">The key of the page asset.</param>
        /// <param name="context">The context of the caller.</param>
        /// <param name="flowContext">The context of the flow call, with the abort handle the flow gave it.</param>
        /// <param name="token">Cancels the call.</param>
        /// <returns>
        /// The identifier of the rented page; otherwise the <see cref="PageFlowError"/> that says why the page could
        /// not be rented.
        /// </returns>
        /// <remarks>
        /// The flow loads the asset on the first rent. A page that is rented and then rejected, or that fails, goes
        /// back to the pool. The rented page waits under the pool root until its show starts.
        /// </remarks>
        protected async UnityTask<Result<UguiPageIdentifier, PageFlowError>> RentPageAsync(
              PageKey pageKey
            , PageContext context
            , PageContext flowContext
            , CancellationToken token
        )
        {
            if (token.IsCancellationRequested)
            {
                return PageFlowError.Cancelled();
            }

            var pool = GetPoolFor(pageKey);

            var poolResult = await InitializePoolAsync(pool, pageKey, token);

            if (poolResult.TryGetFailure(out var poolError))
            {
                return poolError;
            }

            var rentResult = pool.Rent(pageKey.Id, pageKey.Value, this.GetLogger(Context.LogEnvironment));

            if (rentResult.TryGetValue(out var identifier) == false)
            {
                return rentResult.GetErrorOrDefault();
            }

            if (identifier.Page is not IPageOnCreateAsync onCreate)
            {
                return identifier;
            }

            bool isPageCreationAccepted;

            try
            {
                isPageCreationAccepted = await onCreate.OnCreateAsync(
                      PageFlow.WithPageOrigin(flowContext, identifier.Page)
                    , token
                );
            }
            catch (Exception ex)
            {
                ReturnPageToPoolOrLog(identifier, context);

                if (ex is OperationCanceledException)
                {
                    throw;
                }

                return PageFlowError.Exception(ex);
            }

            if (isPageCreationAccepted == false)
            {
                ReturnPageToPoolOrLog(identifier, context);
                return PageFlowError.CreateRejected(pageKey.Value);
            }

            if (token.IsCancellationRequested)
            {
                ReturnPageToPoolOrLog(identifier, context);
                return PageFlowError.Cancelled();
            }

            return identifier;
        }

        /// <summary>Publishes the failure of a call, if it has one.</summary>
        /// <param name="result">The result of the call.</param>
        /// <param name="context">The context of the call.</param>
        /// <remarks>
        /// An abort is published as a <see cref="EncosyTower.PageFlows.PubSub.PageAbortedMessage"/> that carries the
        /// failure. Any other failure is published as a
        /// <see cref="EncosyTower.PageFlows.PubSub.PageFlowFailedMessage"/>, except a cancel or an unheard message
        /// that carries no other failure.
        /// </remarks>
        protected void PublishFailureOrAbort(in Success<PageFlowError> result, PageContext context)
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

        /// <summary>Creates the reference of a page for a navigation.</summary>
        /// <param name="page">The page.</param>
        /// <param name="index">The index of the page in a list; none for a stack.</param>
        /// <returns>The reference, or <see cref="PageRef.None"/> when <paramref name="page"/> is <c>null</c>.</returns>
        protected PageRef MakePageRef(IUguiPage page, Option<int> index)
        {
            if (page == null)
            {
                return PageRef.None;
            }

            return TryGetPageIdentifier(page, out var identifier)
                ? new PageRef(page, identifier.AssetKey, index)
                : new PageRef(Page: page, AssetKey: default, Index: index);
        }

        /// <summary>Publishes the navigation of a call that was applied.</summary>
        /// <param name="kind">The kind of the navigation.</param>
        /// <param name="from">The page the call hid or removed.</param>
        /// <param name="to">The page the call showed.</param>
        /// <param name="context">The context of the call.</param>
        /// <param name="result">The result of the call.</param>
        /// <returns>
        /// <paramref name="result"/>, with a <c>SubscriberNotFound</c> case added when the context warns about messages
        /// that no subscriber received, and with a failed publish added: as the failure, with the Completed state,
        /// when result succeeded; otherwise as a suppressed error.
        /// </returns>
        /// <remarks>
        /// A derived flow that does not call PublishFailureOrAbort after this method loses the navigated failure.
        /// </remarks>
        protected async UnityTask<Success<PageFlowError>> PublishNavigatedAsync(
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

        /// <summary>Reports the result of a call to its receipt.</summary>
        /// <param name="result">The result of the call.</param>
        /// <param name="context">The context of the call. Nothing is reported when it has no receipt.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ReportCall(in Success<PageFlowError> result, PageContext context)
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

        /// <summary>Gets the identifier of a page that a pool of this flow created.</summary>
        /// <param name="page">The page.</param>
        /// <param name="identifier">The identifier of the page.</param>
        /// <returns><c>true</c> when the page is a component with an identifier; otherwise <c>false</c>.</returns>
        protected bool TryGetPageIdentifier(IUguiPage page, out UguiPageIdentifier identifier)
            => GetPageIdentifier(page).TryGetValue(out identifier);

        internal void EnsureInitialized()
            => ThrowIfNotInitialized(IsInitialized, this);

        void IPageFlowBackend.ActivatePageRendering(IPage page)
        {
            if (page is Component component
                && component.TryGetComponent<UguiPageIdentifier>(out var identifier)
                && _pageIdToPool.TryGetValue(identifier.KeyId, out var pool)
            )
            {
                pool.MoveToActive(identifier);
            }
        }

        internal void ActivateForShow(UguiPageIdentifier identifier)
        {
            var gameObject = identifier.GameObject;

            if (gameObject.activeSelf == false && _pageIdToPool.TryGetValue(identifier.KeyId, out var pool))
            {
                pool.MoveToPool(identifier);
            }

            gameObject.SetActive(true);
        }

        internal void ReturnPageToPoolOrLog(IUguiPage page, PageContext context)
        {
            if (ReturnPageToPool(page, context).TryGetFailure(out var error))
            {
                error.Log(Context.Logger);
            }
        }

        internal void ReturnPageToPoolOrLog(UguiPageIdentifier identifier, PageContext context)
        {
            if (ReturnPageToPool(identifier, context).TryGetFailure(out var error))
            {
                error.Log(Context.Logger);
            }
        }

        /// <summary>Takes a page back from the flow.</summary>
        /// <param name="page">The page to return.</param>
        /// <param name="context">
        /// The context of the call. <see cref="PageContext.ReturnOperation"/> says what happens.
        /// </param>
        /// <returns>
        /// Success when the page went back to its pool or was destroyed; otherwise a failure; see
        /// <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>The page gets its <see cref="IPageOnReturnToPool.OnReturnToPool"/> hook first.</remarks>
        protected Success<PageFlowError> ReturnPageToPool(IUguiPage page, PageContext context)
        {
            var identifierResult = GetPageIdentifier(page);

            if (identifierResult.TryGetValue(out var identifier) == false)
            {
                return identifierResult.GetErrorOrDefault();
            }

            return ReturnPageToPool(identifier, context);
        }

        /// <summary>Takes a page back from the flow.</summary>
        /// <param name="identifier">The identifier of the page to return.</param>
        /// <param name="context">
        /// The context of the call. <see cref="PageContext.ReturnOperation"/> says what happens.
        /// </param>
        /// <returns>
        /// Success when the page went back to its pool or was destroyed; otherwise a failure; see
        /// <see cref="PageFlowError"/>.
        /// </returns>
        /// <remarks>The page gets its <see cref="IPageOnReturnToPool.OnReturnToPool"/> hook first.</remarks>
        protected Success<PageFlowError> ReturnPageToPool(UguiPageIdentifier identifier, PageContext context)
        {
            if (identifier.IsInvalid() || identifier.Page is null)
            {
                // A destroyed identifier still holds its page reference; only a missing one has none.
                return PageFlowError.PageDestroyed(identifier is null ? null : identifier.Page);
            }

            if (identifier.Page is IPageOnReturnToPool onReturn)
            {
                onReturn.OnReturnToPool(context);
            }

            if (identifier.GameObjectId.IsValid == false || identifier.GameObject.IsInvalid())
            {
                return PageFlowError.PageDestroyed(identifier.Page);
            }

            if (context.ReturnOperation == PageReturnOperation.Destroy)
            {
                Destroy(identifier.gameObject);
                return Success.Yes;
            }

            if (_pageIdToPool.TryGetValue(identifier.KeyId, out var pool) == false)
            {
                return PageFlowError.PoolNotFound(identifier.AssetKey);
            }

            pool.Return(identifier);
            return Success.Yes;
        }

        private Result<UguiPageIdentifier, PageFlowError> GetPageIdentifier(IUguiPage page)
        {
            if (page is not Component component)
            {
                return PageFlowError.PageNotComponent(page);
            }

            if (component.TryGetComponent<UguiPageIdentifier>(out var identifier) == false)
            {
                return PageFlowError.PageIdentifierMissing(page);
            }

            return identifier;
        }

        private void CreateRootForPooling(RectTransform parent)
        {
            var poolGO = new GameObject(
                  $"[Pool] {name}"
                , typeof(Canvas)
                , typeof(CanvasGroup)
                , typeof(LayoutElement)
            ) {
                layer = 2, // Ignore Raycast
            };

            PoolTransform = poolGO.GetOrAddComponent<RectTransform>();
            PoolTransform.SetParent(parent, false);
            PoolTransform.FillParent(parent);

            var poolCanvasGroup = poolGO.GetComponent<CanvasGroup>();
            poolCanvasGroup.alpha = 0f;
            poolCanvasGroup.blocksRaycasts = false;
            poolCanvasGroup.interactable = false;

            var layoutElement = poolGO.GetComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
        }

        private async UnityTask<Success<PageFlowError>> InitializePoolAsync(
              UguiPagePool pool
            , PageKey pageKey
            , CancellationToken token
        )
        {
            if (pool.IsInitialized)
            {
                return Success.Yes;
            }

            var load = await Context.LoadAssetAsync(pageKey.Value, token, this);

            // The flow was destroyed while loading (OnDestroy already disposed the pool), or the call was cancelled.
            if (this.IsInvalid() || token.IsCancellationRequested)
            {
                if (load.TryGetValue(out var unusedLease))
                {
                    unusedLease.Release();
                }

                return PageFlowError.Cancelled();
            }

            if (load.TryGetValue(out var lease) == false)
            {
                return PageFlowError.AssetLoadFailed(pageKey.Value, load.GetErrorOrDefault());
            }

            pool.Initialize(lease);
            return Success.Yes;
        }

        private UguiPagePool GetPoolFor(PageKey pageKey)
        {
            if (_pageIdToPool.TryGetValue(pageKey.Id, out var pool) == false)
            {
                _pageIdToPool[pageKey.Id] = pool = new UguiPagePool(
                      PoolTransform
                    , RectTransform
                    , _pageIds
                    , _context.PoolRentingStrategy
                    , _context.PoolReturningStrategy
                );
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
              UguiPageFlow flow
            , PrepoolPageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (PrepoolPageMessage)msg;
            var result = await flow.PrepoolPageAsync(message.AssetKey, message.Amount, context.Token);

            flow.PublishFailureOrAbort(in result, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Handle(UguiPageFlow flow, TrimPoolMessage msg)
            => flow.TrimPool(msg.AssetKey, msg.AmountToKeep);

        [HideInCallstack, StackTraceHidden]
        private static void ThrowIfNotInitialized([DoesNotReturnIf(false)] bool isInitialized, UguiPageFlow context)
        {
            if (isInitialized == false)
            {
                throw CreateException(context);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(UguiPageFlow context)
                => new($"The page flow must be initialized via '{context.GetType()}.Initialize'. " +
                    $"Or the property 'Auto Initialize On Awake' must be checked on the Inspector window.");
        }

        [HideInCallstack, StackTraceHidden]
        private static void ThrowIfInitialized([DoesNotReturnIf(true)] bool isInitialized, UguiPageFlow context)
        {
            if (isInitialized)
            {
                throw CreateException(context);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(UguiPageFlow context)
                => new($"The page flow '{context.Identifier}' is already initialized; " +
                    $"'{context.GetType()}.Initialize' runs only once.");
        }

        /// <summary>What a flow type needs to subscribe its messages and register its requests.</summary>
        /// <param name="Subscriber">The subscriber, scoped to the flow.</param>
        /// <param name="ProcessHub">The process hub, scoped to the flow.</param>
        protected readonly record struct InitializationContext(
              MessageSubscriber.Subscriber<PageFlowScope> Subscriber
            , Processor.Hub<PageFlowScope> ProcessHub
        );

        /// <summary>The key of a page asset and its string id.</summary>
        /// <param name="Value">The key.</param>
        /// <param name="Id">The string id of the key.</param>
        protected readonly record struct PageKey(AssetKey Value, StringId Id);
    }
}

#endif
