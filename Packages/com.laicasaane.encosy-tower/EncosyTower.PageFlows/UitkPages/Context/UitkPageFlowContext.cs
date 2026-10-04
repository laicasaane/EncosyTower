using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Initialization;
using EncosyTower.Logging;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Serialization;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.UitkPages
{
    [Serializable]
    public sealed class UitkPageFlowContext : IPageFlowContext, IIsInitialized
    {
        public bool autoInitializeOnAwake;

        [OverridableDefault(typeof(UitkPageFlowSettings), nameof(UitkPageFlowSettings.warnNoSubscriber))]
        public Overridable<bool> warnNoSubscriber;

        [OverridableDefault(typeof(UitkPageFlowSettings), nameof(UitkPageFlowSettings.loaderStrategy))]
        public Overridable<PageLoaderStrategy> loadStrategy;

        [OverridableDefault(typeof(UitkPageFlowSettings), nameof(UitkPageFlowSettings.logEnvironment))]
        public Overridable<LogEnvironment> logEnvironment;

        private bool _warnNoSubscriber;
        private PageLoaderStrategy _loadStrategy;
        private LogEnvironment _logEnvironment;
        private MessageSubscriber _subscriber;
        private MessagePublisher _publisher;
        private Processor _processor;
        private ArrayPool<UnityTask> _taskArrayPool;

        public Component Owner { get; set; }

        public bool IsInitialized { get; private set; }

        public PageFlowScope FlowScope { get; internal set; }

        public Option<IPageFlowScopeCollectionApplier> FlowScopeCollectionApplier { get; set; }

        public ArrayPool<UnityTask> TaskArrayPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _taskArrayPool;
        }

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
            , UitkPageFlowSettings settings = null
            , ArrayPool<UnityTask> taskArrayPool = null
        )
        {
            var source = settings.IsValid() ? settings : null;
            var hasSource = source != null;

            _warnNoSubscriber = warnNoSubscriber.GetValueOrDefault(hasSource && source.warnNoSubscriber);
            _loadStrategy = loadStrategy.GetValueOrDefault(hasSource ? source.loaderStrategy : default);
            _logEnvironment = logEnvironment.GetValueOrDefault(hasSource ? source.logEnvironment : default);
            IsInitialized = true;

            _subscriber = subscriber ?? GlobalMessenger.Subscriber;
            _publisher = publisher ?? GlobalMessenger.Publisher;
            _processor = processor ?? GlobalProcessor.Instance;
            _taskArrayPool = taskArrayPool ?? ArrayPool<UnityTask>.Shared;
        }

        public UitkPageFlowContext CloneWithoutOwner()
            => new() {
                autoInitializeOnAwake = autoInitializeOnAwake,
                warnNoSubscriber = warnNoSubscriber,
                loadStrategy = loadStrategy,
                logEnvironment = logEnvironment,
                _warnNoSubscriber = _warnNoSubscriber,
                _loadStrategy = _loadStrategy,
                _logEnvironment = _logEnvironment,
                _subscriber = _subscriber,
                _publisher = _publisher,
                _processor = _processor,
                _taskArrayPool = _taskArrayPool,
                IsInitialized = IsInitialized,
                FlowScope = FlowScope,
                FlowScopeCollectionApplier = FlowScopeCollectionApplier,
            };
    }
}
