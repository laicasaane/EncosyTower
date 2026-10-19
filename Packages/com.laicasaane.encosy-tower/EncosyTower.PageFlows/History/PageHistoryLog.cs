using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Internals;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UguiPages.Codex;
using EncosyTower.PageFlows.UitkPages.Codex;
using EncosyTower.PubSub;
using EncosyTower.Tasks;

#if UNITY_UGUI
#endif

using ThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.History
{
    /// <summary>Records page navigations for inspection; never navigates.</summary>
    /// <remarks>
    /// Records every navigation, including the Back and Forward replays a <see cref="PageHistory"/> issues. Any
    /// number of logs can cover a flow.
    /// </remarks>
    public sealed class PageHistoryLog : IDisposable, IClearable
    {
        private readonly PageHistoryRecord _record = new();
        private readonly Dictionary<string, IPageFlowRecordable> _flows = new(StringComparer.Ordinal);
        private readonly List<ISubscription> _subscriptions = new();
        private readonly ILogger _logger;

        /// <summary>Creates a log that is not attached to any flow.</summary>
        /// <param name="recordingStrategy">
        /// Decides what each navigation does to the log. <c>null</c> uses a <see cref="PageCallLogHistoryStrategy"/>.
        /// </param>
        /// <param name="logger">The logger the log uses. <c>null</c> uses the default logger.</param>
        public PageHistoryLog(IPageHistoryRecordingStrategy recordingStrategy = null, ILogger logger = null)
        {
            RecordingStrategy = recordingStrategy ?? new PageCallLogHistoryStrategy();
            _logger = logger ?? DevLogger.Default;
        }

        /// <summary>Gets the strategy that decides what each navigation does to the log.</summary>
        public IPageHistoryRecordingStrategy RecordingStrategy { get; }

        /// <summary>Gets the recorded entries, oldest first.</summary>
        public ListFast<PageHistoryEntry>.ReadOnly Entries => _record.Entries;

        /// <summary>Gets or sets the number of entries the log keeps.</summary>
        /// <value>The default is 100. A value below 1 sets 1. Lowering it drops the oldest entries at once.</value>
        public int MaxEntries
        {
            get => _record.MaxEntries;
            set => _record.MaxEntries = value;
        }

        /// <summary>Gets whether the log covers at least one flow.</summary>
        public bool IsAttached => _flows.Count > 0;

        /// <summary>Records the navigations of one flow.</summary>
        /// <param name="flow">The flow to record.</param>
        /// <returns>
        /// Success when the log covers the flow; otherwise a failure; see <see cref="PageHistoryError"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="flow"/> is <c>null</c>.</exception>
        public Success<PageHistoryError> Attach([NotNull] IPageFlowRecordable flow)
        {
            ThrowHelper.ThrowIfNull(flow);
            return Attach(new[] { flow });
        }

        /// <summary>Records the navigations of flows.</summary>
        /// <param name="flows">The flows to record.</param>
        /// <returns>
        /// Success when the log covers the flows; otherwise a failure; see <see cref="PageHistoryError"/>.
        /// </returns>
        /// <remarks>
        /// The log ignores a flow whose <see cref="IPageFlowRecordable.FlowId"/> appears again later in
        /// <paramref name="flows"/>, and logs a warning.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// An element of <paramref name="flows"/> is <c>null</c>.
        /// </exception>
        public Success<PageHistoryError> Attach(ReadOnlySpan<IPageFlowRecordable> flows)
        {
            var count = flows.Length;

            for (var i = 0; i < count; i++)
            {
                ThrowHelper.ThrowIfNull(flows[i], nameof(flows));
            }

            if (IsAttached)
            {
                return (PageHistoryError)new PageHistoryError.AlreadyAttached();
            }

            var uniqueFlows = CollectFlowsWithUniqueIds(flows);
            var acceptedCount = uniqueFlows.Count;

            if (acceptedCount < 1)
            {
                return (PageHistoryError)new PageHistoryError.NoFlowToAttach();
            }

            for (var i = 0; i < acceptedCount; i++)
            {
                var flow = uniqueFlows[i];
                _flows[flow.FlowId] = flow;

                var subscriber = flow.Subscriber
                    .Scope(flow.FlowScope)
                    .WithSubscriptions(_subscriptions)
                    .WithState(this);

                PageNavigatedMessage.Async.Subscribe(in subscriber, HandleAsync);
            }

            return Success.Yes;
        }

#if UNITY_UGUI
        /// <summary>Records the navigations of every flow of a UGUI page codex.</summary>
        /// <param name="codex">The codex whose flows to record.</param>
        /// <returns>
        /// Success when the log covers the flows; otherwise a failure; see <see cref="PageHistoryError"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="codex"/> is <c>null</c>.</exception>
        public Success<PageHistoryError> Attach([NotNull] UguiPageCodex codex)
        {
            ThrowHelper.ThrowIfNull(codex);

            var count = codex.FlowCount;
            var flows = new List<IPageFlowRecordable>(count);

            for (var i = 0; i < count; i++)
            {
                if (codex.GetFlowAt(i).TryGetValue(out var flow) && flow is IPageFlowRecordable historyFlow)
                {
                    flows.Add(historyFlow);
                }
            }

            return Attach(flows.AsReadOnlySpan());
        }
#endif

        /// <summary>Records the navigations of every flow of a UI Toolkit page codex.</summary>
        /// <param name="codex">The codex whose flows to record.</param>
        /// <returns>
        /// Success when the log covers the flows; otherwise a failure; see <see cref="PageHistoryError"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="codex"/> is <c>null</c>.</exception>
        public Success<PageHistoryError> Attach([NotNull] UitkPageCodex codex)
        {
            ThrowHelper.ThrowIfNull(codex);

            var count = codex.FlowCount;
            var flows = new List<IPageFlowRecordable>(count);

            for (var i = 0; i < count; i++)
            {
                if (codex.GetFlowAt(i).TryGetValue(out var flow) && flow is IPageFlowRecordable historyFlow)
                {
                    flows.Add(historyFlow);
                }
            }

            return Attach(flows.AsReadOnlySpan());
        }

        /// <summary>Stops recording the flows. The entries stay.</summary>
        public void Detach()
        {
            _subscriptions.AsListFast().Unsubscribe();

            _flows.Clear();
        }

        /// <summary>Removes every entry.</summary>
        public void Clear()
            => _record.Clear();

        /// <summary>Detaches the log.</summary>
        public void Dispose()
            => Detach();

        private List<IPageFlowRecordable> CollectFlowsWithUniqueIds(ReadOnlySpan<IPageFlowRecordable> flows)
        {
            var count = flows.Length;
            var uniqueFlows = new List<IPageFlowRecordable>(count);
            var flowIds = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < count; i++)
            {
                var flow = flows[i];

                if (flowIds.Add(flow.FlowId) == false)
                {
                    _logger.LogWarning(
                        $"The flow id '{flow.FlowId}' appears more than once; the later flow is ignored."
                    );

                    continue;
                }

                uniqueFlows.Add(flow);
            }

            return uniqueFlows;
        }

        private void OnNavigated(in PageNavigation navigation)
        {
            PageHistoryChange change;

            try
            {
                change = RecordingStrategy.OnNavigated(navigation, Entries.AsReadOnlySpan(), Entries.Count - 1);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogException(ex);
                return;
            }

            _record.Apply(change, shouldApplyBackAndForwardChanges: false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              PageHistoryLog log
            , PageNavigatedMessage.Async msg
            , PublishingContext _
        )
        {
            log.OnNavigated(((PageNavigatedMessage)msg).Navigation);
            return UnityTask.CompletedTask;
        }
    }
}
