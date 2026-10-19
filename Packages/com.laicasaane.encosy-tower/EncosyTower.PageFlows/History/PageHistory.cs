using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
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
    /// <summary>
    /// Records page navigations and replays them as Back and Forward across the flows it covers. At most one
    /// navigation history covers a flow.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The history records each navigation that a covered flow publishes as a
    /// <see cref="EncosyTower.PageFlows.PubSub.PageNavigatedMessage"/>, as its recording strategy decides. It ignores
    /// the navigations it replays itself. A replay runs one at a time. All page flow operations and callbacks run on
    /// the Unity main thread; see <see cref="IPageFlow"/>.
    /// </para>
    /// <para>
    /// The history reads a flow by <see cref="IPageFlowRecordable"/> and replays an entry with the
    /// <see cref="IPageHistoryNavigator"/> that <see cref="NavigatorFactory"/> creates for the flow.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using var history = new PageHistory();
    ///
    /// history.Attach(codex);
    ///
    /// if (history.CanGoBack)
    /// {
    ///     var back = await history.GoBackAsync(token);
    ///
    ///     if (back.TryGetFailure(out var error)) logger.LogError(error.ToMessage());
    /// }
    /// </code>
    /// </example>
    public sealed class PageHistory : IDisposable, IClearable
    {
        private static readonly ConditionalWeakTable<IPageFlowRecordable, PageHistory> s_coverage = new();

        private readonly PageHistoryRecord _record = new();
        private readonly Dictionary<string, IPageFlowRecordable> _flows = new(StringComparer.Ordinal);
        private readonly Dictionary<string, IPageHistoryNavigator> _navigators = new(StringComparer.Ordinal);
        private readonly List<ISubscription> _subscriptions = new();
        private readonly ILogger _logger;

        private bool _isReplaying;

        /// <summary>Creates a history that is not attached to any flow.</summary>
        /// <param name="recordingStrategy">
        /// Decides what each navigation does to the history. <c>null</c> uses a
        /// <see cref="PageBackStackHistoryStrategy"/>.
        /// </param>
        /// <param name="logger">The logger the history uses. <c>null</c> uses the default logger.</param>
        public PageHistory(IPageHistoryRecordingStrategy recordingStrategy = null, ILogger logger = null)
        {
            RecordingStrategy = recordingStrategy ?? new PageBackStackHistoryStrategy();
            _logger = logger ?? DevLogger.Default;
        }

        /// <summary>Gets the strategy that decides what each navigation does to the history.</summary>
        public IPageHistoryRecordingStrategy RecordingStrategy { get; }

        /// <summary>Gets the recorded entries, oldest first.</summary>
        public ListFast<PageHistoryEntry>.ReadOnly Entries => _record.Entries;

        /// <summary>Gets the index of the entry that Back would undo, or <c>-1</c> when there is none.</summary>
        public int CurrentIndex => _record.CurrentIndex;

        /// <summary>Gets whether there is an entry to go back from.</summary>
        public bool CanGoBack => CurrentIndex >= 0;

        /// <summary>Gets whether there is an entry after the current one.</summary>
        public bool CanGoForward => CurrentIndex < Entries.Count - 1;

        /// <summary>Gets or sets the number of entries the history keeps.</summary>
        /// <value>The default is 100. A value below 1 sets 1. Lowering it drops the oldest entries at once.</value>
        public int MaxEntries
        {
            get => _record.MaxEntries;
            set => _record.MaxEntries = value;
        }

        /// <summary>Gets whether the history covers at least one flow.</summary>
        public bool IsAttached => _flows.Count > 0;

        /// <summary>Gets or sets what a replayed call does when its flow is in transition.</summary>
        /// <value>The default is <see cref="PageAsyncOperation.Sequential"/>.</value>
        public PageAsyncOperation ReplayOperation { get; set; } = PageAsyncOperation.Sequential;

        /// <summary>Gets the function that creates the navigator which replays a flow.</summary>
        /// <value>
        /// The default is <see cref="PageHistoryNavigators.Create(IPageFlowRecordable)"/>. A replay throws an
        /// <see cref="System.InvalidOperationException"/> when the function returns <c>null</c>.
        /// </value>
        public Func<IPageFlowRecordable, IPageHistoryNavigator> NavigatorFactory { get; init; }
            = PageHistoryNavigators.Create;

        /// <summary>Covers one flow with the history.</summary>
        /// <param name="flow">The flow to cover.</param>
        /// <returns>
        /// Success when the history covers the flow; otherwise a failure; see <see cref="PageHistoryError"/>.
        /// </returns>
        /// <remarks>At most one navigation history covers a flow.</remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="flow"/> is <c>null</c>.</exception>
        public Success<PageHistoryError> Attach([NotNull] IPageFlowRecordable flow)
        {
            ThrowHelper.ThrowIfNull(flow);
            return Attach(new[] { flow });
        }

        /// <summary>Covers flows with the history.</summary>
        /// <param name="flows">The flows to cover.</param>
        /// <returns>
        /// Success when the history covers the flows; otherwise a failure; see <see cref="PageHistoryError"/>.
        /// </returns>
        /// <remarks>
        /// The history ignores a flow whose <see cref="IPageFlowRecordable.FlowId"/> appears again later in
        /// <paramref name="flows"/>, and logs a warning. At most one navigation history covers a flow.
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

                if (s_coverage.TryGetValue(flow, out var owner) && owner != this)
                {
                    return (PageHistoryError)new PageHistoryError.FlowAlreadyCovered(flow.FlowId);
                }
            }

            for (var i = 0; i < acceptedCount; i++)
            {
                var flow = uniqueFlows[i];
                s_coverage.Remove(flow);
                s_coverage.Add(flow, this);
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
        /// <summary>Covers every flow of a UGUI page codex with the history.</summary>
        /// <param name="codex">The codex whose flows to cover.</param>
        /// <returns>
        /// Success when the history covers the flows; otherwise a failure; see <see cref="PageHistoryError"/>.
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

        /// <summary>Covers every flow of a UI Toolkit page codex with the history.</summary>
        /// <param name="codex">The codex whose flows to cover.</param>
        /// <returns>
        /// Success when the history covers the flows; otherwise a failure; see <see cref="PageHistoryError"/>.
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

        /// <summary>Stops covering the flows and drops their navigators. The entries stay.</summary>
        public void Detach()
        {
            _subscriptions.AsListFast().Unsubscribe();

            foreach (var flow in _flows.Values)
            {
                if (s_coverage.TryGetValue(flow, out var owner) && owner == this)
                {
                    s_coverage.Remove(flow);
                }
            }

            _flows.Clear();
            _navigators.Clear();
        }

        /// <summary>Removes every entry and resets the current index.</summary>
        public void Clear()
            => _record.Clear();

        /// <summary>Detaches the history.</summary>
        public void Dispose()
            => Detach();

        /// <summary>Undoes the current entry by replaying it backwards on its flow.</summary>
        /// <param name="token">Cancels the replay.</param>
        /// <returns>
        /// Success when the replay was applied; otherwise a failure; see <see cref="PageHistoryError"/>.
        /// </returns>
        /// <remarks>
        /// The current index moves whenever the change was applied (success, or a failure whose transition state is
        /// <see cref="PageTransitionState.Completed"/>), even when an error is returned; a change that was not applied,
        /// or was aborted, keeps the index. Do not retry on any error, or the history steps twice.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// <see cref="NavigatorFactory"/> returns <c>null</c> for the flow of the entry.
        /// </exception>
        public async UnityTask<Success<PageHistoryError>> GoBackAsync(CancellationToken token)
        {
            if (IsAttached == false)
            {
                return (PageHistoryError)new PageHistoryError.NotAttached();
            }

            if (_isReplaying)
            {
                return (PageHistoryError)new PageHistoryError.Busy();
            }

            if (CurrentIndex < 0)
            {
                return (PageHistoryError)new PageHistoryError.NothingToGoBack();
            }

            var entry = Entries[CurrentIndex];

            if (TryGetOrCreateNavigator(entry.FlowId, out var navigator) == false)
            {
                return (PageHistoryError)new PageHistoryError.FlowNotFound(entry.FlowId);
            }

            var replayEntry = entry with { Context = entry.Context with { IssuedBy = this } };
            var version = _record.Version;
            var target = CurrentIndex - 1;
            Success<PageFlowError> result;

            _isReplaying = true;

            try
            {
                result = await navigator.BackAsync(replayEntry, ReplayOperation, token);
            }
            finally
            {
                _isReplaying = false;
            }

            return FinishReplay(result, version, target);
        }

        /// <summary>Repeats the entry after the current one by replaying it forwards on its flow.</summary>
        /// <param name="token">Cancels the replay.</param>
        /// <returns>
        /// Success when the replay was applied; otherwise a failure; see <see cref="PageHistoryError"/>.
        /// </returns>
        /// <remarks>
        /// The current index moves whenever the change was applied (success, or a failure whose transition state is
        /// <see cref="PageTransitionState.Completed"/>), even when an error is returned; a change that was not applied,
        /// or was aborted, keeps the index. Do not retry on any error, or the history steps twice.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// <see cref="NavigatorFactory"/> returns <c>null</c> for the flow of the entry.
        /// </exception>
        public async UnityTask<Success<PageHistoryError>> GoForwardAsync(CancellationToken token)
        {
            if (IsAttached == false)
            {
                return (PageHistoryError)new PageHistoryError.NotAttached();
            }

            if (_isReplaying)
            {
                return (PageHistoryError)new PageHistoryError.Busy();
            }

            if (CurrentIndex >= Entries.Count - 1)
            {
                return (PageHistoryError)new PageHistoryError.NothingToGoForward();
            }

            var entry = Entries[CurrentIndex + 1];

            if (TryGetOrCreateNavigator(entry.FlowId, out var navigator) == false)
            {
                return (PageHistoryError)new PageHistoryError.FlowNotFound(entry.FlowId);
            }

            var replayEntry = entry with { Context = entry.Context with { IssuedBy = this } };
            var version = _record.Version;
            var target = CurrentIndex + 1;
            Success<PageFlowError> result;

            _isReplaying = true;

            try
            {
                result = await navigator.ForwardAsync(replayEntry, ReplayOperation, token);
            }
            finally
            {
                _isReplaying = false;
            }

            return FinishReplay(result, version, target);
        }

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

        private bool TryGetOrCreateNavigator(string flowId, out IPageHistoryNavigator navigator)
        {
            if (flowId == null || _flows.TryGetValue(flowId, out var flow) == false || flow.IsAlive == false)
            {
                navigator = null;
                return false;
            }

            if (_navigators.TryGetValue(flowId, out navigator) == false)
            {
                navigator = NavigatorFactory(flow);

                if (navigator == null)
                {
                    ThrowNavigatorIsNull(flowId);
                }

                _navigators[flowId] = navigator;
            }

            return true;
        }

        private Success<PageHistoryError> FinishReplay(in Success<PageFlowError> result, int version, int target)
        {
            if (result.IsApplied())
            {
                if (_record.Version == version)
                {
                    _record.MoveTo(target);
                }
                else
                {
                    _logger.LogWarning("The history changed during the replay; the current index was not moved.");
                }
            }

            if (result.TryGetFailure(out var error) && PageHistoryReplay.IsLearnOnly(result) == false)
            {
                return ToReplayError(error);
            }

            return Success.Yes;
        }

        private static PageHistoryError ToReplayError(in PageFlowError error)
        {
            if (error.IsAborted)
            {
                return new PageHistoryError.ReplayAborted(error);
            }

            return new PageHistoryError.ReplayFailed(error);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [UnityEngine.HideInCallstack, StackTraceHidden, DoesNotReturn]
        private static void ThrowNavigatorIsNull(string flowId)
            => throw new InvalidOperationException($"The navigator factory returned null for the flow '{flowId}'.");

        private void OnNavigated(in PageNavigation navigation)
        {
            if (ReferenceEquals(navigation.IssuedBy, this))
            {
                return;
            }

            PageHistoryChange change;

            try
            {
                change = RecordingStrategy.OnNavigated(navigation, Entries.AsReadOnlySpan(), CurrentIndex);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogException(ex);
                return;
            }

            _record.Apply(change, shouldApplyBackAndForwardChanges: true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(
              PageHistory history
            , PageNavigatedMessage.Async msg
            , PublishingContext _
        )
        {
            history.OnNavigated(((PageNavigatedMessage)msg).Navigation);
            return UnityTask.CompletedTask;
        }
    }
}
