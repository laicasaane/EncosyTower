using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Initialization;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Loading;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Serialization;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows
{
    /// <summary>What a UI Toolkit page flow needs to run: its services, its scope and its settings.</summary>
    /// <remarks>
    /// <para>
    /// A codex owns one and gives a copy to each flow. Each overridable field uses its value when it is overridden;
    /// otherwise it uses <see cref="UitkPageFlowSettings"/>. The fields are read once, in <see cref="Initialize"/>.
    /// </para>
    /// <para>All page flow operations and callbacks run on the Unity main thread; see <see cref="IPageFlow"/>.</para>
    /// </remarks>
    [Serializable]
    public sealed class UitkPageFlowContext : IPageFlowContext, IIsInitialized
    {
        /// <summary>Whether the codex initializes itself in <c>Awake</c>.</summary>
        public bool autoInitializeOnAwake;

        /// <summary>Whether a message that no subscriber received is added to the result of the call.</summary>
        [OverridableDefault(typeof(UitkPageFlowSettings), nameof(UitkPageFlowSettings.warnNoSubscriber))]
        public Overridable<bool> warnNoSubscriber;

        /// <summary>Whether list flows reject adding a page that is already in the list.</summary>
        [OverridableDefault(typeof(UitkPageFlowSettings), nameof(UitkPageFlowSettings.rejectDuplicatePages))]
        public Overridable<bool> rejectDuplicatePages;

        /// <summary>How page assets are loaded.</summary>
        [OverridableDefault(typeof(UitkPageFlowSettings), nameof(UitkPageFlowSettings.loaderStrategy))]
        public Overridable<PageLoaderStrategy> loadStrategy;

        /// <summary>Which logger the flows use.</summary>
        [OverridableDefault(typeof(UitkPageFlowSettings), nameof(UitkPageFlowSettings.logEnvironment))]
        public Overridable<LogEnvironment> logEnvironment;

        [NonSerialized] private bool _warnNoSubscriber;
        [NonSerialized] private bool _rejectDuplicatePages;
        [NonSerialized] private PageLoaderStrategy _loadStrategy;
        [NonSerialized] private LogEnvironment _logEnvironment;
        [NonSerialized] private MessageSubscriber _subscriber;
        [NonSerialized] private MessagePublisher _publisher;
        [NonSerialized] private Processor _processor;

        /// <summary>Gets or sets the component that owns the context: the codex.</summary>
        /// <remarks>The codex sets it when it initializes.</remarks>
        public Component Owner { get; set; }

        /// <summary>Gets whether <see cref="Initialize"/> ran.</summary>
        public bool IsInitialized { get; private set; }

        /// <inheritdoc/>
        /// <remarks>Each flow gets its own scope when it initializes.</remarks>
        public PageFlowScope FlowScope { get; internal set; }

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
            , UitkPageFlowSettings settings = null
        )
        {
            var source = settings.IsValid() ? settings : null;
            var hasSource = source != null;

            _warnNoSubscriber = warnNoSubscriber.GetValueOrDefault(hasSource && source.warnNoSubscriber);
            _rejectDuplicatePages = rejectDuplicatePages.GetValueOrDefault(hasSource && source.rejectDuplicatePages);
            _loadStrategy = loadStrategy.GetValueOrDefault(hasSource ? source.loaderStrategy : default);
            _logEnvironment = logEnvironment.GetValueOrDefault(hasSource ? source.logEnvironment : default);
            IsInitialized = true;

            _subscriber = subscriber ?? GlobalMessenger.Subscriber;
            _publisher = publisher ?? GlobalMessenger.Publisher;
            _processor = processor ?? GlobalProcessor.Instance;
        }

        /// <summary>Creates a copy with the same values and services, and no owner.</summary>
        /// <returns>The copy. The codex gives one to each flow it creates.</returns>
        public UitkPageFlowContext CloneWithoutOwner()
            => new() {
                autoInitializeOnAwake = autoInitializeOnAwake,
                warnNoSubscriber = warnNoSubscriber,
                rejectDuplicatePages = rejectDuplicatePages,
                loadStrategy = loadStrategy,
                logEnvironment = logEnvironment,
                _warnNoSubscriber = _warnNoSubscriber,
                _rejectDuplicatePages = _rejectDuplicatePages,
                _loadStrategy = _loadStrategy,
                _logEnvironment = _logEnvironment,
                _subscriber = _subscriber,
                _publisher = _publisher,
                _processor = _processor,
                IsInitialized = IsInitialized,
                FlowScope = FlowScope,
                FlowScopeCollectionApplier = FlowScopeCollectionApplier,
                OwnershipTracker = OwnershipTracker,
            };
    }
}
