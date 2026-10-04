#if UNITY_UGUI

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Initialization;
using EncosyTower.Logging;
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
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UguiPageFlow : MonoBehaviour, IIsInitialized
    {
        [SerializeField] internal UguiPageFlowContext _context = new();

        private readonly Dictionary<StringId, UguiPagePool> _pageIdToPool = new();
        private readonly List<UguiPageIdentifier> _pageIds = new();
        private readonly List<ISubscription> _subscriptions = new();
        private readonly List<ProcessRegistry> _processRegistries = new();

        public bool IsInitialized { get; private set; }

        public Func<MessagePublisher> GetPublisher { get; set; }

        public Func<MessageSubscriber> GetSubscriber { get; set; }

        public Func<Processor> GetProcessor { get; set; }

        public Func<ArrayPool<UnityTask>> GetTaskArrayPool { get; set; }

        public UguiPageFlowContext Context => _context;

        public RectTransform RectTransform { get; private set; }

        public Canvas Canvas { get; private set; }

        public CanvasGroup CanvasGroup { get; private set; }

        public RectTransform PoolTransform { get; private set; }

        public static T Create<T>(string name, RectTransform parent, UguiPageFlowContext context)
            where T : UguiPageFlow
        {
            var root = new GameObject(name, typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasGroup));

            var rectTransform = root.GetOrAddComponent<RectTransform>();
            rectTransform.FillParent(parent);

            var flow = root.GetOrAddComponent<T>();
            flow.Initialize(parent, context);

            return flow;
        }

        public void Initialize(RectTransform parent = default, UguiPageFlowContext context = default)
        {
            RectTransform = this.GetOrAddComponent<RectTransform>();
            Canvas = this.GetOrAddComponent<Canvas>();
            CanvasGroup = this.GetOrAddComponent<CanvasGroup>();

            if (parent.IsValid())
                CreateRootForPooling(parent);
            else
                CreateRootForPooling(GetComponent<RectTransform>());

            context ??= _context;
            _context = context;

            context.Owner = this;

            if (context.IsInitialized == false)
            {
                var settings = UguiPageFlowSettings.Instance;
                var subscriber = GetSubscriber?.Invoke();
                var publisher = GetPublisher?.Invoke();
                var processor = GetProcessor?.Invoke();
                var taskArrayPool = GetTaskArrayPool?.Invoke();

                context.Initialize(subscriber, publisher, processor, settings, taskArrayPool);
            }

            var initContext = new InitializationContext(
                  context.Subscriber.Scope(context.FlowScope).WithSubscriptions(_subscriptions)
                , context.ProcessHub.WithRegistries(_processRegistries)
            );

            SubscribeMessages(initContext);
            OnInitialize(initContext);

            IsInitialized = true;
        }

        protected abstract void OnInitialize(in InitializationContext context);

        protected void Awake()
        {
            if (_context.autoInitializeOnAwake)
            {
                var rectTransform = GetComponent<RectTransform>();
                Initialize(rectTransform, _context);
            }
        }

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
        }

        protected abstract void OnDispose();

        /// <summary>
        /// Preload an amount of view instances and keep them in the pool.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityTask PrepoolPageAsync(string pageAssetKey, int amount, CancellationToken token)
        {
            var pageKey = MakePageKey(pageAssetKey);
            return PrepoolPageAsync(pageKey, Mathf.Max(amount, 0), token);
        }

        /// <summary>
        /// Only keep a certain amount of pages in the pool, destroy the others.
        /// </summary>
        public void TrimPool(string pageAssetKey, int amountToKeep)
        {
            var pageKey = MakePageKey(pageAssetKey);
            TrimPool(pageKey, amountToKeep);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected PageKey MakePageKey([NotNull] string pageAssetKey)
        {
            ThrowHelper.ThrowIfNull(pageAssetKey);

            ThrowIfNotInitialized(IsInitialized, this);

            var id = Context.MakeStringId(pageAssetKey);
            return new(pageAssetKey, id);
        }

        /// <summary>
        /// Preload an amount of view instances and keep them in the pool.
        /// </summary>
        protected async UnityTask PrepoolPageAsync(PageKey pageKey, int amount, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (amount < 1)
            {
                WarningIfAmountLesserThanOne(this);
                return;
            }

            var pool = GetPoolFor(pageKey);

            if (pool.PoolingCount >= amount)
            {
                return;
            }

            if (pool.IsInitialized == false)
            {
                var sourceOpt = await Context.LoadAssetAsync(pageKey.Value, token, this);

                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (sourceOpt.TryGetValue(out var source) == false
                    || source.IsInvalid()
                )
                {
                    ErrorIfCannotLoadAsset(pageKey, this);
                    return;
                }

                pool.Initialize(source);
            }

            var differentAmount = amount - pool.PoolingCount;

            if (differentAmount < 1)
            {
                return;
            }

            pool.Prepool(differentAmount);
        }

        /// <summary>
        /// Only keep a certain amount of pages in the pool, destroy the others.
        /// </summary>
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

        protected async UnityTask<Option<UguiPageIdentifier>> RentPageAsync(
              PageKey pageKey
            , PageContext context
            , CancellationToken token
        )
        {
            if (token.IsCancellationRequested)
            {
                return Option.None;
            }

            var pool = GetPoolFor(pageKey);

            if (pool.IsInitialized == false)
            {
                var sourceOpt = await Context.LoadAssetAsync(pageKey.Value, token, this);

                if (token.IsCancellationRequested)
                {
                    return Option.None;
                }

                if (sourceOpt.TryGetValue(out var source) == false
                    || source.IsInvalid()
                )
                {
                    ErrorIfCannotLoadAsset(pageKey, this);
                    return Option.None;
                }

                pool.Initialize(source);
            }

            var identifierOpt = pool.Rent(pageKey.Id, pageKey.Value, this.GetLogger(Context.LogEnvironment));

            if (identifierOpt.TryGetValue(out var identifier) && identifier.Page is IPageOnCreateAsync onCreate)
            {
                var creationResult = await onCreate.OnCreateAsync(context, token);

                if (creationResult == false)
                {
                    WarningIfPageCreationFailed(pageKey, this);
                }

                if (creationResult == false || token.IsCancellationRequested)
                {
                    ReturnPageToPool(identifier, context);
                    return Option.None;
                }
            }

            return identifierOpt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Option<UguiPageIdentifier> GetPageIdentifierAt(int index)
        {
            return (uint)index < (uint)_pageIds.Count ? _pageIds[index] : Option.None;
        }

        protected bool ReturnPageToPool(IUguiPage page, PageContext context)
        {
            if (page is not Component component)
            {
                ErrorIfPageIsNotComponent(page, this);
                return false;
            }

            if (component.TryGetComponent<UguiPageIdentifier>(out var identifier) == false)
            {
                ErrorIfPageMissingIdentifier(page, this);
                return false;
            }

            return ReturnPageToPool(identifier, context);
        }

        protected bool ReturnPageToPool(UguiPageIdentifier identifier, PageContext context)
        {
            if (identifier.IsInvalid() || identifier.Page is null)
            {
                return false;
            }

            if (identifier.Page is IPageOnReturnToPool onReturn)
            {
                onReturn.OnReturnToPool(context);
            }

            if (identifier.GameObjectId.IsValid == false || identifier.GameObject.IsInvalid())
            {
                ErrorIfPageIsDestroyedOrNotInited(identifier.Page, this);
                return false;
            }

            if (context.ReturnOperation == PageReturnOperation.Destroy)
            {
                Destroy(identifier.gameObject);
                return false;
            }

            if (_pageIdToPool.TryGetValue(identifier.KeyId, out var pool) == false)
            {
                ErrorIfCannotReturnToPool(identifier.AssetKey, this);
                return false;
            }

            pool.Return(identifier);
            return true;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              UguiPageFlow flow
            , PrepoolPageMessage.Async msg
            , PublishingContext context
        )
        {
            var message = (PrepoolPageMessage)msg;
            return flow.PrepoolPageAsync(message.AssetKey, message.Amount, context.Token);
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void WarningIfAmountLesserThanOne(UguiPageFlow context)
        {
            context.GetLogger(context.Context.LogEnvironment).LogWarning(
                $"The amount of preloaded instances should be greater than 0."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfCannotReturnToPool(string key, UguiPageFlow context)
        {
            context.GetLogger(context.Context.LogEnvironment).LogError(
                $"Cannot return the page on the instance originated from '{key}' to pool. " +
                $"The pool may not be created properly."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfPageIsNotComponent(IUguiPage page, UguiPageFlow context)
        {
            context.GetLogger(context.Context.LogEnvironment).LogError(
                $"The page '{page.GetType()}' is not derived from 'UnityEngine.Component'."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfPageMissingIdentifier(IUguiPage page, UguiPageFlow context)
        {
            context.GetLogger(context.Context.LogEnvironment).LogError(
                $"Cannot found any {nameof(UguiPageIdentifier)} component on the page '{page.GetType()}'. " +
                $"The page might not be created correctly."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfPageIsDestroyedOrNotInited(IUguiPage page, UguiPageFlow context)
        {
            context.GetLogger(context.Context.LogEnvironment).LogError(
                $"The page '{page.GetType()}' might have already been destroyed, " +
                $"or not properly initialized via pooling mechanism of UguiPageFlow."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfCannotLoadAsset(PageKey pageKey, UguiPageFlow context)
        {
            context.GetLogger(context.Context.LogEnvironment).LogError(
                $"Cannot load asset by the key '{pageKey.Value}'"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void WarningIfPageCreationFailed(PageKey pageKey, UguiPageFlow context)
        {
            context.GetLogger(context.Context.LogEnvironment).LogWarning(
                $"The result of '{nameof(IPageOnCreateAsync)}.{nameof(IPageOnCreateAsync.OnCreateAsync)}' " +
                $"for page loaded by the key '{pageKey.Value}' was a failure."
            );
        }

        protected readonly record struct InitializationContext(
              MessageSubscriber.Subscriber<PageFlowScope> Subscriber
            , Processor.Hub<PageFlowScope> ProcessHub
        );

        protected readonly record struct PageKey(string Value, StringId Id);
    }
}

#endif
