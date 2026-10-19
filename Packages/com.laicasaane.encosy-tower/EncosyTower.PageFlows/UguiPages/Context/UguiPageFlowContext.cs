#if UNITY_UGUI

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.Initialization;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Loading;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Pooling;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Serialization;
using EncosyTower.StringIds;
using EncosyTower.Tasks;
using EncosyTower.Types;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows
{
    /// <summary>What a uGUI page flow needs to run: its services, its scope and its settings.</summary>
    /// <remarks>
    /// <para>
    /// A codex or a flow owns one. Each overridable field uses its value when it is overridden; otherwise it uses
    /// <see cref="UguiPageFlowSettings"/>. The fields are read once, in <see cref="Initialize"/>.
    /// </para>
    /// <para>All page flow operations and callbacks run on the Unity main thread; see <see cref="IPageFlow"/>.</para>
    /// </remarks>
    [Serializable]
    public sealed class UguiPageFlowContext : IPageFlowContext, IIsInitialized
    {
        /// <summary>Whether the owner initializes itself in <c>Awake</c>.</summary>
        public bool autoInitializeOnAwake;

        /// <summary>Whether a message that no subscriber received is added to the result of the call.</summary>
        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.warnNoSubscriber))]
        public Overridable<bool> warnNoSubscriber;

        /// <summary>Whether list flows reject adding a page that is already in the list.</summary>
        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.rejectDuplicatePages))]
        public Overridable<bool> rejectDuplicatePages;

        /// <summary>How page assets are loaded.</summary>
        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.loaderStrategy))]
        public Overridable<PageLoaderStrategy> loadStrategy;

        /// <summary>How the page pools rent an instance.</summary>
        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.poolRentingStrategy))]
        public Overridable<RentingStrategy> poolRentingStrategy;

        /// <summary>How the page pools take an instance back.</summary>
        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.poolReturningStrategy))]
        public Overridable<ReturningStrategy> poolReturningStrategy;

        /// <summary>What the scope of a flow identifies.</summary>
        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.messageScope))]
        public Overridable<UguiMessageScope> messageScope;

        /// <summary>Which logger the flows use.</summary>
        [OverridableDefault(typeof(UguiPageFlowSettings), nameof(UguiPageFlowSettings.logEnvironment))]
        public Overridable<LogEnvironment> logEnvironment;

        [NonSerialized] private bool _warnNoSubscriber;
        [NonSerialized] private bool _rejectDuplicatePages;
        [NonSerialized] private PageLoaderStrategy _loadStrategy;
        [NonSerialized] private RentingStrategy _poolRentingStrategy;
        [NonSerialized] private ReturningStrategy _poolReturningStrategy;
        [NonSerialized] private UguiMessageScope _messageScope;
        [NonSerialized] private LogEnvironment _logEnvironment;
        [NonSerialized] private MessageSubscriber _subscriber;
        [NonSerialized] private MessagePublisher _publisher;
        [NonSerialized] private Processor _processor;

        /// <summary>Gets or sets the component that owns the context: a codex or a flow.</summary>
        /// <remarks>The owner sets it when it initializes. <see cref="FlowScope"/> needs it.</remarks>
        public Component Owner { get; set; }

        /// <summary>Gets whether <see cref="Initialize"/> ran.</summary>
        public bool IsInitialized { get; private set; }

        /// <inheritdoc/>
        /// <remarks>
        /// Built from the owner: its GameObject or the owner component itself, as <see cref="MessageScope"/> says.
        /// </remarks>
        public PageFlowScope FlowScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MessageScope == UguiMessageScope.Component
                ? GetScope((Id<UguiPageFlow>)Type<UguiPageFlow>.Id, Owner.GetEntityId())
                : GetScope((Id<GameObject>)Type<GameObject>.Id, Owner.gameObject.GetEntityId());
        }

        /// <inheritdoc/>
        /// <remarks>The codex sets it before it creates its flows.</remarks>
        public Option<IPageFlowScopeCollectionApplier> FlowScopeCollectionApplier { get; set; }

        /// <inheritdoc/>
        public Option<IPageOwnershipTracker> OwnershipTracker { get; set; }

        /// <inheritdoc/>
        public MessageSubscriber Subscriber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _subscriber;
        }

        /// <inheritdoc/>
        public MessagePublisher Publisher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _publisher;
        }

        /// <summary>Gets the process hub of the scope of the flow.</summary>
        public Processor.Hub<PageFlowScope> ProcessHub
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _processor.Scope(FlowScope);
        }

        /// <inheritdoc/>
        public bool WarnNoSubscriber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _warnNoSubscriber : warnNoSubscriber.GetValueOrDefault();
        }

        /// <inheritdoc/>
        public bool RejectDuplicatePages
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _rejectDuplicatePages : rejectDuplicatePages.GetValueOrDefault();
        }

        /// <summary>Gets how page assets are loaded.</summary>
        public PageLoaderStrategy LoadStrategy
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _loadStrategy : loadStrategy.GetValueOrDefault();
        }

        /// <summary>Gets how the page pools rent an instance.</summary>
        public RentingStrategy PoolRentingStrategy
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _poolRentingStrategy : poolRentingStrategy.GetValueOrDefault();
        }

        /// <summary>Gets how the page pools take an instance back.</summary>
        public ReturningStrategy PoolReturningStrategy
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _poolReturningStrategy : poolReturningStrategy.GetValueOrDefault();
        }

        /// <summary>Gets what the scope of a flow identifies.</summary>
        public UguiMessageScope MessageScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _messageScope : messageScope.GetValueOrDefault();
        }

        /// <summary>Gets which logger the flows use.</summary>
        public LogEnvironment LogEnvironment
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsInitialized ? _logEnvironment : logEnvironment.GetValueOrDefault();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The runtime logger for <see cref="EncosyTower.Logging.LogEnvironment.Runtime"/>; otherwise the
        /// development logger.
        /// </remarks>
        public Logging.ILogger Logger
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => LogEnvironment == LogEnvironment.Runtime ? Logging.Logger.Default : DevLogger.Default;
        }

        /// <summary>Resolves the settings and the services, and marks the context initialized.</summary>
        /// <param name="subscriber">The subscriber. <c>null</c> uses the global subscriber.</param>
        /// <param name="publisher">The publisher. <c>null</c> uses the global publisher.</param>
        /// <param name="processor">The processor. <c>null</c> uses the global processor.</param>
        /// <param name="settings">
        /// The settings for the values that are not overridden. <c>null</c> uses the defaults.
        /// </param>
        public void Initialize(
              MessageSubscriber subscriber = null
            , MessagePublisher publisher = null
            , Processor processor = null
            , UguiPageFlowSettings settings = null
        )
        {
            var source = settings.IsValid() ? settings : null;
            var hasSource = source != null;

            _warnNoSubscriber = warnNoSubscriber.GetValueOrDefault(hasSource && source.warnNoSubscriber);
            _rejectDuplicatePages = rejectDuplicatePages.GetValueOrDefault(hasSource && source.rejectDuplicatePages);
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
        }

        /// <summary>Creates a copy with the same values and services, and no owner.</summary>
        /// <returns>The copy. The codex gives one to each flow it creates.</returns>
        public UguiPageFlowContext CloneWithoutOwner()
            => new() {
                autoInitializeOnAwake = autoInitializeOnAwake,
                warnNoSubscriber = warnNoSubscriber,
                rejectDuplicatePages = rejectDuplicatePages,
                loadStrategy = loadStrategy,
                poolRentingStrategy = poolRentingStrategy,
                poolReturningStrategy = poolReturningStrategy,
                messageScope = messageScope,
                logEnvironment = logEnvironment,
                _warnNoSubscriber = _warnNoSubscriber,
                _rejectDuplicatePages = _rejectDuplicatePages,
                _loadStrategy = _loadStrategy,
                _poolRentingStrategy = _poolRentingStrategy,
                _poolReturningStrategy = _poolReturningStrategy,
                _messageScope = _messageScope,
                _logEnvironment = _logEnvironment,
                _subscriber = _subscriber,
                _publisher = _publisher,
                _processor = _processor,
                IsInitialized = IsInitialized,
                FlowScopeCollectionApplier = FlowScopeCollectionApplier,
                OwnershipTracker = OwnershipTracker,
            };

        /// <summary>Gets the string id of a string.</summary>
        /// <param name="str">The string.</param>
        /// <returns>The string id.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringId MakeStringId(string str)
            => StringToId.Get(str);

        /// <summary>Gets the string of a string id.</summary>
        /// <param name="id">The string id.</param>
        /// <returns>The string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(StringId id)
            => IdToString.GetManaged(id);

        /// <summary>Loads a page asset with <see cref="LoadStrategy"/>.</summary>
        /// <param name="assetKey">The key of the asset.</param>
        /// <param name="token">Cancels the load.</param>
        /// <param name="logContext">The object that owns the log message, if the load logs.</param>
        /// <returns>
        /// The lease of the loaded asset; otherwise the <see cref="PageAssetLoadError"/> that says why the load failed.
        /// </returns>
        /// <remarks>
        /// When the strategy is Addressables and the package is not installed, the method logs an error and loads with
        /// Resources.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="logContext"/> is <c>null</c> or destroyed.
        /// </exception>
        public UnityTask<Result<PageAssetHandle<GameObject>, PageAssetLoadError>> LoadAssetAsync(
              AssetKey assetKey
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
