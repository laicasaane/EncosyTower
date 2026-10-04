#if UNITY_UGUI

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.Initialization;
using EncosyTower.Logging;
using EncosyTower.Pooling;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Serialization;
using EncosyTower.StringIds;
using EncosyTower.Tasks;
using EncosyTower.Types;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace EncosyTower.PageFlows.UguiPages
{
    [Serializable]
    [MovedFrom(
          true
        , sourceNamespace: "EncosyTower.PageFlows.MonoPages"
        , sourceAssembly: "EncosyTower.PageFlows.MonoPages"
        , sourceClassName: "MonoPageFlowContext"
    )]
    public sealed class UguiPageFlowContext : IPageFlowContext, IIsInitialized
    {
        public bool autoInitializeOnAwake;

        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.warnNoSubscriber))]
        public Overridable<bool> warnNoSubscriber;

        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.loaderStrategy))]
        public Overridable<PageLoaderStrategy> loadStrategy;

        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.poolRentingStrategy))]
        public Overridable<RentingStrategy> poolRentingStrategy;

        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.poolReturningStrategy))]
        public Overridable<ReturningStrategy> poolReturningStrategy;

        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.messageScope))]
        public Overridable<UguiMessageScope> messageScope;

        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.logEnvironment))]
        public Overridable<LogEnvironment> logEnvironment;

        private bool _warnNoSubscriber;
        private PageLoaderStrategy _loadStrategy;
        private RentingStrategy _poolRentingStrategy;
        private ReturningStrategy _poolReturningStrategy;
        private UguiMessageScope _messageScope;
        private LogEnvironment _logEnvironment;
        private MessageSubscriber _subscriber;
        private MessagePublisher _publisher;
        private Processor _processor;
        private ArrayPool<UnityTask> _taskArrayPool;

        public Component Owner { get; set; }

        public bool IsInitialized { get; private set; }

        public ArrayPool<UnityTask> TaskArrayPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _taskArrayPool;
        }

        public PageFlowScope FlowScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MessageScope == UguiMessageScope.Component
                ? GetScope((Id<UguiPageFlow>)Type<UguiPageFlow>.Id, Owner.GetEntityId())
                : GetScope((Id<GameObject>)Type<GameObject>.Id, Owner.gameObject.GetEntityId());
        }

        public Option<IPageFlowScopeCollectionApplier> FlowScopeCollectionApplier { get; set; }

        public MessageSubscriber Subscriber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _subscriber;
        }

        public MessagePublisher Publisher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _publisher;
        }

        public Processor.Hub<PageFlowScope> ProcessHub
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _processor.Scope(FlowScope);
        }

        public bool WarnNoSubscriber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _warnNoSubscriber : warnNoSubscriber.GetValueOrDefault();
        }

        public PageLoaderStrategy LoadStrategy
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _loadStrategy : loadStrategy.GetValueOrDefault();
        }

        public RentingStrategy PoolRentingStrategy
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _poolRentingStrategy : poolRentingStrategy.GetValueOrDefault();
        }

        public ReturningStrategy PoolReturningStrategy
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _poolReturningStrategy : poolReturningStrategy.GetValueOrDefault();
        }

        public UguiMessageScope MessageScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _messageScope : messageScope.GetValueOrDefault();
        }

        public LogEnvironment LogEnvironment
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _logEnvironment : logEnvironment.GetValueOrDefault();
        }

        public Logging.ILogger Logger
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => LogEnvironment == LogEnvironment.Runtime ? Logging.Logger.Default : DevLogger.Default;
        }

        public void Initialize(
              MessageSubscriber subscriber = null
            , MessagePublisher publisher = null
            , Processor processor = null
            , UguiPageFlowSettings settings = null
            , ArrayPool<UnityTask> taskArrayPool = null
        )
        {
            var source = settings.IsValid() ? settings : null;
            var hasSource = source != null;

            _warnNoSubscriber = warnNoSubscriber.GetValueOrDefault(hasSource && source.warnNoSubscriber);
            _loadStrategy = loadStrategy.GetValueOrDefault(hasSource ? source.loaderStrategy : default);
            _poolRentingStrategy = poolRentingStrategy.GetValueOrDefault(
                hasSource ? source.poolRentingStrategy : default
            );
            _poolReturningStrategy = poolReturningStrategy.GetValueOrDefault(
                hasSource ? source.poolReturningStrategy : default
            );
            _messageScope = messageScope.GetValueOrDefault(hasSource ? source.messageScope : default);
            _logEnvironment = logEnvironment.GetValueOrDefault(hasSource ? source.logEnvironment : default);
            IsInitialized = true;

            _subscriber = subscriber ?? GlobalMessenger.Subscriber;
            _publisher = publisher ?? GlobalMessenger.Publisher;
            _processor = processor ?? GlobalProcessor.Instance;
            _taskArrayPool = taskArrayPool ?? ArrayPool<UnityTask>.Shared;
        }

        public UguiPageFlowContext CloneWithoutOwner()
            => new() {
                autoInitializeOnAwake = autoInitializeOnAwake,
                warnNoSubscriber = warnNoSubscriber,
                loadStrategy = loadStrategy,
                poolRentingStrategy = poolRentingStrategy,
                poolReturningStrategy = poolReturningStrategy,
                messageScope = messageScope,
                logEnvironment = logEnvironment,
                _warnNoSubscriber = _warnNoSubscriber,
                _loadStrategy = _loadStrategy,
                _poolRentingStrategy = _poolRentingStrategy,
                _poolReturningStrategy = _poolReturningStrategy,
                _messageScope = _messageScope,
                _logEnvironment = _logEnvironment,
                _subscriber = _subscriber,
                _publisher = _publisher,
                _processor = _processor,
                _taskArrayPool = _taskArrayPool,
                IsInitialized = IsInitialized,
                FlowScopeCollectionApplier = FlowScopeCollectionApplier,
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringId MakeStringId(string str)
            => StringToId.Get(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(StringId id)
            => IdToString.GetManaged(id);

        public UnityTask<Option<GameObject>> LoadAssetAsync(
              string assetKey
            , CancellationToken token
            , [NotNull] UnityEngine.Object logContext
        )
        {
            Debugging.ThrowHelper.ThrowIfUnityObjectInvalid(logContext);

            if (LoadStrategy == PageLoaderStrategy.Addressables && PageAssetLoader.IsAddressablesAvailable == false)
            {
                ErrorIfAddressablesNotInstalled(this, logContext);
            }

            return PageAssetLoader.LoadAsync<GameObject>(LoadStrategy, assetKey, token);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack]
        private static void ErrorIfAddressablesNotInstalled(
              UguiPageFlowContext context
            , UnityEngine.Object logContext
        )
        {
            logContext.GetLogger(context.LogEnvironment).LogError(
                "Addressables is not installed. uGUI Page Loader will use Resources instead."
            );
        }

        private static PageFlowScope GetScope(Id id, EntityId entityId)
        {
            var id2 = new Id2EntityIdUnion(entityId).id2;
            return new(new(id, id2.X, id2.Y));
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Id2EntityIdUnion
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public EntityId entityId;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public Id2 id2;

            public Id2EntityIdUnion(EntityId entityId) : this()
            {
                this.entityId = entityId;
            }
        }
    }
}

#endif
