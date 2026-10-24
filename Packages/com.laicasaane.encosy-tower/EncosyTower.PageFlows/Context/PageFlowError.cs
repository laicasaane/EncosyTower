using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Loading;
using EncosyTower.PolyEnumStructs;
using EncosyTower.PubSub;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;
using HideInCallstack = UnityEngine.HideInCallstackAttribute;

namespace EncosyTower.PageFlows
{
    /// <summary>Describes why a page flow operation failed.</summary>
    /// <remarks>
    /// <para>
    /// A flow never logs a failure it returns: exceptions thrown by page hooks and handlers come back as an
    /// <c>Exception</c> result, and errors suppressed while cleaning up after it are in
    /// <see cref="SuppressedErrors"/>. Failures returned by the subscribers of the flow's messages come back the
    /// same way. The caller owns logging: call <see cref="Log(ILogger)"/> for each failed result it reacts to.
    /// When a cancellation wins over earlier failures, the call throws. A wrapper then reports the <c>Cancelled</c>
    /// case to the report with those failures as suppressed errors and publishes it as a
    /// <see cref="EncosyTower.PageFlows.PubSub.PageFlowFailedMessage"/>; a direct call of a core flow logs them when
    /// the call ends, because it has no other channel.
    /// </para>
    /// <para>The failure is one of these cases:</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Case</term>
    /// <description>When it occurs</description>
    /// </listheader>
    /// <item>
    /// <term><c>Undefined</c></term>
    /// <description>
    /// The default value. A flow returns it when a failed publish has no cause it can name.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>NoPage</c></term>
    /// <description>
    /// The flow has no page for the operation: a pop or a hide on an empty flow, or a remove-all with nothing to
    /// remove.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>PageNotFound</c></term>
    /// <description>
    /// The page is not in the flow.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>IndexOutOfRange</c></term>
    /// <description>
    /// The index is not in the range of the pages of the flow.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>AlreadyShown</c></term>
    /// <description>
    /// A single-page list was asked to show the page at an index when that page is already its current page.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>CannotHideDefaultPage</c></term>
    /// <description>
    /// A single-page list was asked to hide the page at <c>PageContext.DefaultIndex</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>InTransition</c></term>
    /// <description>
    /// The flow was in transition and the call has <c>PageAsyncOperation.Drop</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>Cancelled</c></term>
    /// <description>
    /// The cancellation token of the call was cancelled. Read <c>TransitionState</c> to see whether the screen changed.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>AttachRejected</c></term>
    /// <description>
    /// <c>OnAttachToFlowAsync</c> of the page returned <c>false</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>DetachRejected</c></term>
    /// <description>
    /// <c>OnDetachFromFlowAsync</c> of the page returned <c>false</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>CreateRejected</c></term>
    /// <description>
    /// <c>OnCreateAsync</c> of the page returned <c>false</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>AssetLoadFailed</c></term>
    /// <description>
    /// The page asset could not be loaded, or the pool cannot use it. The case holds the <c>PageAssetLoadError</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>AssetNotSinglePage</c></term>
    /// <description>
    /// The page asset does not hold exactly one page.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>PageBehaviourMissing</c></term>
    /// <description>
    /// A UI Toolkit page asset that is a GameObject has no valid <c>UitkPageBehaviour</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>ReplayUnanswered</c></term>
    /// <description>
    /// A history replayed a call, and no page flow of the flow identifier reported a result.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>SubscriberStateUnavailable</c></term>
    /// <description>
    /// The state object of a subscriber of a published message is not alive anymore.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>SubscriberNotFound</c></term>
    /// <description>
    /// A message the flow published during the call had no subscriber, and the flow warns about it.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>ReplayNotSupported</c></term>
    /// <description>
    /// A navigator cannot replay an entry of that kind on that flow.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>Exception</c></term>
    /// <description>
    /// A page hook, a message handler, a transition or a factory threw an exception. The case holds it.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>PageOwnedByAnotherFlow</c></term>
    /// <description>
    /// The page is attached to another flow of the same ownership tracker.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>PageNotComponent</c></term>
    /// <description>
    /// A UGUI page is not a <c>UnityEngine.Component</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>PageIdentifierMissing</c></term>
    /// <description>
    /// A UGUI page has no <c>UguiPageIdentifier</c>, so no page flow pool created it.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>PageDestroyed</c></term>
    /// <description>
    /// The GameObject of a UGUI page is destroyed, or no page flow pool created it.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>PageNotActive</c></term>
    /// <description>
    /// The page is not an active page of the flow.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>PoolNotFound</c></term>
    /// <description>
    /// The flow has no pool for the asset key of the page.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>InvalidPrepoolAmount</c></term>
    /// <description>
    /// The amount of a prepool request is less than 1.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>Aborted</c></term>
    /// <description>
    /// The call was aborted. The case holds who asked and the mode.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>Withdrawn</c></term>
    /// <description>
    /// An abort was requested for the call before it started, while it waited for the flow. The case holds who asked.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>PageAlreadyAdded</c></term>
    /// <description>
    /// A list that rejects duplicate pages already holds the page. The case holds the level of the setting.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>PageInTransition</c></term>
    /// <description>
    /// The page is part of the transition the flow is running.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>PageBeingRemoved</c></term>
    /// <description>
    /// The flow is already removing the page.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await flow.ShowPageAsync(assetKey, context, token);
    ///
    /// if (result.TryGetFailure(out var error)) error.Log(logger);
    /// </code>
    /// </example>
    [PolyEnumFactoryFor(typeof(Error))]
    public readonly partial struct PageFlowError
    {
        internal readonly Error _error;
        private readonly PageTransitionState _transitionState;
        private readonly PageAbortMode _abortMode;
        private readonly PageFlowError[] _suppressed;

        private PageFlowError(in Error error) : this(error, default, default, null)
        {
        }

        private PageFlowError(
              in Error error
            , PageTransitionState state
            , PageAbortMode abortMode
            , PageFlowError[] suppressed
        )
        {
            _error = error;
            _transitionState = state;
            _abortMode = abortMode;
            _suppressed = suppressed;
        }

        /// <summary>How far the screen got: not started, completed, or undone.</summary>
        public PageTransitionState TransitionState => _transitionState;

        /// <summary>Gets the failures kept with this one while the flow cleaned up after it.</summary>
        public System.ReadOnlySpan<PageFlowError> SuppressedErrors => _suppressed;

        /// <summary>True when this failure keeps at least one suppressed error.</summary>
        public bool HasSuppressedErrors => _suppressed != null;

        /// <summary>True when the cancellation token of the call was cancelled.</summary>
        public bool IsCancelled => _error.IsCancelled;

        /// <summary>True when the operation was aborted or withdrawn.</summary>
        public bool IsAborted => _error.IsAborted;

        /// <summary>True when an abort was requested for the call before it started.</summary>
        public bool IsWithdrawn => _error.IsWithdrawn;

        /// <summary>
        /// True when a message the flow published during the operation had no subscriber.
        /// The operation itself went on; check <c>TransitionState</c> to see whether it was applied.
        /// </summary>
        public bool IsSubscriberNotFound => _error.IsSubscriberNotFound;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static PageFlowError FromPublish(in PublishError error)
        {
            var result = FromPublishCase(error);
            AppendPublishSuppressed(ref result, error.SuppressedErrors);
            return result;
        }

        private static PageFlowError FromPublishCase(in PublishError error)
        {
            if (error.TryGetException(out var exception))
            {
                return Exception(exception);
            }

            if (error.TryGetStateType(out var stateType))
            {
                return SubscriberStateUnavailable(stateType);
            }

            return Undefined();
        }

        private static void AppendPublishSuppressed(
              ref PageFlowError result
            , System.ReadOnlySpan<PublishError> suppressed
        )
        {
            var count = suppressed.Length;

            for (var i = 0; i < count; i++)
            {
                ref readonly var error = ref suppressed[i];

                if (error.IsSubscriberNotFound == false)
                {
                    result = result.WithSuppressed(FromPublishCase(error));
                }

                AppendPublishSuppressed(ref result, error.SuppressedErrors);
            }
        }

        /// <summary>Gets who aborted the operation and with which mode.</summary>
        /// <param name="origin">Who asked for the abort.</param>
        /// <param name="mode">The mode of the abort.</param>
        /// <returns><c>true</c> when the operation was aborted or withdrawn; otherwise <c>false</c>.</returns>
        public bool TryGetAbort(out PageAbortOrigin origin, out PageAbortMode mode)
        {
            if (_error.TryGetValue(out Error.Aborted aborted))
            {
                origin = aborted.Origin;
                mode = aborted.Mode;
                return true;
            }

            if (_error.TryGetValue(out Error.Withdrawn withdrawn))
            {
                origin = withdrawn.Origin;
                mode = _abortMode;
                return true;
            }

            origin = default;
            mode = default;
            return false;
        }

        /// <summary>Creates a copy of this failure with another transition state.</summary>
        /// <param name="state">The transition state to set.</param>
        /// <returns>The copy.</returns>
        public PageFlowError WithTransitionState(PageTransitionState state)
            => new(_error, state, _abortMode, _suppressed);

        /// <summary>Gets a message that describes the failure.</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ToMessage()
            => _error.ToMessage();

        /// <inheritdoc cref="ToMessage"/>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override string ToString()
            => ToMessage();

        /// <summary>Logs the failure, then each of its suppressed errors.</summary>
        /// <param name="logger">The logger to log with.</param>
        /// <remarks>
        /// A <c>SubscriberNotFound</c> failure is logged as a warning, an <c>Exception</c> failure as an exception,
        /// and every other failure as an error.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="logger"/> is <c>null</c>.</exception>
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.NoInlining)]
        public void Log([NotNull] ILogger logger)
        {
            DebuggingThrowHelper.ThrowIfNull(logger);
            _error.Log(logger);
            LogSuppressedCore(logger);
        }

        /// <summary>Logs each suppressed error of this failure, not the failure itself.</summary>
        /// <param name="logger">The logger to log with.</param>
        /// <remarks>
        /// Use it where the outcome is expected, for example an abort the caller asked for, and only the failures met
        /// on
        /// the way need logging. Each suppressed error is logged as <see cref="Log(ILogger)"/> logs it, with its own
        /// suppressed errors.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException"><paramref name="logger"/> is <c>null</c>.</exception>
        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.NoInlining)]
        public void LogSuppressed([NotNull] ILogger logger)
        {
            DebuggingThrowHelper.ThrowIfNull(logger);
            LogSuppressedCore(logger);
        }

        // True when a suppressed error, or one nested in it, is a failure on its own: not a cancel and not an unheard
        // message (D54). Such a failure is published even under a Cancelled or SubscriberNotFound primary error (G2).
        internal bool HasSuppressedFailure()
        {
            var suppressed = _suppressed;

            if (suppressed == null)
            {
                return false;
            }

            var count = suppressed.Length;

            for (var i = 0; i < count; i++)
            {
                ref readonly var error = ref suppressed[i];

                if ((error.IsCancelled == false && error.IsSubscriberNotFound == false) || error.HasSuppressedFailure())
                {
                    return true;
                }
            }

            return false;
        }

        internal PageFlowError WithAbortMode(PageAbortMode mode)
            => new(_error, _transitionState, mode, _suppressed);

        internal PageFlowError WithSuppressed(in PageFlowError error)
        {
            var current = _suppressed;

            if (current == null)
            {
                return new(_error, _transitionState, _abortMode, new[] { error });
            }

            var count = current.Length;
            var suppressed = new PageFlowError[count + 1];

            System.Array.Copy(current, suppressed, count);
            suppressed[count] = error;

            return new(_error, _transitionState, _abortMode, suppressed);
        }

        [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.NoInlining)]
        private void LogSuppressedCore(ILogger logger)
        {
            var suppressed = _suppressed;

            if (suppressed == null)
            {
                return;
            }

            var count = suppressed.Length;

            for (var i = 0; i < count; i++)
            {
                suppressed[i].Log(logger);
            }
        }

        [PolyEnumStruct]
        internal readonly partial struct Error
        {
            partial interface IEnumCase
            {
                bool IsCancelled => false;

                bool IsAborted => false;

                bool IsWithdrawn => false;

                bool IsSubscriberNotFound => false;

                string ToMessage();

                void Log(ILogger logger)
                    => logger.LogError(ToMessage());
            }

            /// <summary>The default value. A flow returns it when a failed publish has no cause it can name.</summary>
            public readonly partial struct Undefined
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => "An unknown page flow error occurred.";
            }

            /// <summary>The flow has no page for the operation.</summary>
            public readonly partial struct NoPage
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => "The flow has no page for this operation.";
            }

            /// <summary>The page is not in the flow.</summary>
            /// <param name="Page">The page that is not in the flow.</param>
            public readonly partial record struct PageNotFound(IPage Page)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page '{Page}' is not in the flow.";
            }

            /// <summary>The index is not in the range of the pages of the flow.</summary>
            /// <param name="Index">The index.</param>
            /// <param name="Count">The number of pages the flow has.</param>
            public readonly partial record struct IndexOutOfRange(int Index, int Count)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"Index '{Index}' is out of range; the flow has {Count} pages.";
            }

            /// <summary>
            /// Fails when the page at an index is already the current page of a single-page list.
            /// </summary>
            /// <param name="Index">The index of the page.</param>
            public readonly partial record struct AlreadyShown(int Index)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page at index '{Index}' is already showing.";
            }

            /// <summary>A single-page list was asked to hide its default page.</summary>
            /// <param name="Index">The index of the default page.</param>
            public readonly partial record struct CannotHideDefaultPage(int Index)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page at index '{Index}' is the default page and cannot be hidden.";
            }

            /// <summary>The flow was in transition and the call has <c>PageAsyncOperation.Drop</c>.</summary>
            /// <param name="FlowType">The type of the flow.</param>
            public readonly partial record struct InTransition(System.Type FlowType)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"{FlowType} is currently in transition.";
            }

            /// <summary>The cancellation token of the call was cancelled.</summary>
            public readonly partial struct Cancelled
            {
                public bool IsCancelled => true;

                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => "The operation was cancelled before the flow changed.";
            }

            /// <summary><c>OnAttachToFlowAsync</c> of the page returned <c>false</c>.</summary>
            /// <param name="Page">The page that rejected.</param>
            public readonly partial record struct AttachRejected(IPage Page)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page '{Page}' rejected attaching to the flow.";
            }

            /// <summary><c>OnDetachFromFlowAsync</c> of the page returned <c>false</c>.</summary>
            /// <param name="Page">The page that rejected.</param>
            public readonly partial record struct DetachRejected(IPage Page)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page '{Page}' rejected detaching from the flow.";
            }

            /// <summary><c>OnCreateAsync</c> of the page returned <c>false</c>.</summary>
            /// <param name="AssetKey">The key of the page asset.</param>
            public readonly partial record struct CreateRejected(AssetKey AssetKey)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page created from asset key '{AssetKey}' reported that its creation failed.";
            }

            /// <summary>The page asset could not be loaded, or the pool cannot use it.</summary>
            /// <param name="AssetKey">The key of the page asset.</param>
            /// <param name="Reason">Why the asset failed.</param>
            public readonly partial record struct AssetLoadFailed(AssetKey AssetKey, PageAssetLoadError Reason)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"Cannot load the page asset with key '{AssetKey}': {Reason.ToMessage()}";
            }

            /// <summary>The page asset does not hold exactly one page.</summary>
            /// <param name="AssetKey">The key of the page asset.</param>
            /// <param name="PageCount">The number of pages the asset holds.</param>
            public readonly partial record struct AssetNotSinglePage(AssetKey AssetKey, int PageCount)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The asset '{AssetKey}' must hold exactly one page but holds {PageCount}.";
            }

            /// <summary>A UI Toolkit page asset that is a GameObject has no valid <c>UitkPageBehaviour</c>.</summary>
            /// <param name="AssetKey">The key of the page asset.</param>
            public readonly partial record struct PageBehaviourMissing(AssetKey AssetKey)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page of asset '{AssetKey}' has no valid page behaviour.";
            }

            /// <summary>A history replayed a call, and no page flow reported a result for it.</summary>
            /// <param name="FlowId">The identifier of the flow the call was for.</param>
            public readonly partial record struct ReplayUnanswered(string FlowId)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"No page flow answered the replayed call on the flow '{FlowId}'.";
            }

            /// <summary>The state object of a subscriber of a published message is not alive anymore.</summary>
            /// <param name="StateType">The type of the state object.</param>
            public readonly partial record struct SubscriberStateUnavailable(System.Type StateType)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The subscriber state of type `{StateType}` is not alive anymore.";
            }

            /// <summary>A message the flow published during an operation had no subscriber.</summary>
            /// <param name="MessageType">The type of the message nobody received.</param>
            public readonly partial record struct SubscriberNotFound(System.Type MessageType)
            {
                public bool IsSubscriberNotFound => true;

                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"No subscriber received the message of type `{MessageType}`.";

                public void Log(ILogger logger)
                    => logger.LogWarning(ToMessage());
            }

            /// <summary>A navigator cannot replay an entry of that kind on that flow.</summary>
            /// <param name="NavigatorName">The type name of the navigator.</param>
            /// <param name="Kind">The kind of the entry.</param>
            /// <param name="FlowId">The identifier of the flow.</param>
            public readonly partial record struct ReplayNotSupported(
                  string NavigatorName
                , PageNavigationKind Kind
                , string FlowId
            )
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The navigator '{NavigatorName}' cannot replay a {Kind} entry on the flow '{FlowId}'.";
            }

            /// <summary>A page hook, a message handler, a transition or a factory threw an exception.</summary>
            /// <param name="Ex">The exception.</param>
            public readonly partial record struct Exception(System.Exception Ex)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"An exception occurred in the page flow: {Ex.Message}";

                public void Log(ILogger logger)
                    => logger.LogException(new System.InvalidOperationException(ToMessage(), Ex));
            }

            /// <summary>The page is attached to another flow of the same ownership tracker.</summary>
            /// <param name="Page">The page.</param>
            public readonly partial record struct PageOwnedByAnotherFlow(IPage Page)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page '{Page}' is already attached to another flow.";
            }

            /// <summary>A UGUI page is not a <c>UnityEngine.Component</c>.</summary>
            /// <param name="Page">The page.</param>
            public readonly partial record struct PageNotComponent(IPage Page)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page '{Page}' is not a UnityEngine.Component.";
            }

            /// <summary>A UGUI page has no <c>UguiPageIdentifier</c>, so no page flow pool created it.</summary>
            /// <param name="Page">The page.</param>
            public readonly partial record struct PageIdentifierMissing(IPage Page)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page '{Page}' has no UguiPageIdentifier; it was not created by a page flow pool.";
            }

            /// <summary>The GameObject of a UGUI page is destroyed, or no page flow pool created it.</summary>
            /// <param name="Page">The page.</param>
            public readonly partial record struct PageDestroyed(IPage Page)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page '{Page}' has been destroyed or was not created by a page flow pool.";
            }

            /// <summary>The page is not an active page of the flow.</summary>
            /// <param name="Page">The page.</param>
            public readonly partial record struct PageNotActive(IPage Page)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page '{Page}' is not an active page of this flow.";
            }

            /// <summary>The flow has no pool for the asset key of the page.</summary>
            /// <param name="AssetKey">The key of the page asset.</param>
            public readonly partial record struct PoolNotFound(AssetKey AssetKey)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"Cannot find the pool for asset key '{AssetKey}'.";
            }

            /// <summary>The amount of a prepool request is less than 1.</summary>
            /// <param name="Amount">The amount.</param>
            public readonly partial record struct InvalidPrepoolAmount(int Amount)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The prepool amount '{Amount}' must be greater than 0.";
            }

            /// <summary>The call was aborted.</summary>
            /// <param name="Origin">Who asked for the abort.</param>
            /// <param name="Mode">The mode of the abort.</param>
            public readonly partial record struct Aborted(PageAbortOrigin Origin, PageAbortMode Mode)
            {
                public bool IsAborted => true;

                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The operation was aborted by {Origin.GetEnumCase()} in {Mode} mode.";
            }

            /// <summary>An abort was requested for the call before it started, while it waited for the flow.</summary>
            /// <param name="Origin">Who asked for the abort.</param>
            public readonly partial record struct Withdrawn(PageAbortOrigin Origin)
            {
                public bool IsAborted => true;

                public bool IsWithdrawn => true;

                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The operation was withdrawn by {Origin.GetEnumCase()} before it started.";
            }

            /// <summary>A list that rejects duplicate pages already holds the page.</summary>
            /// <param name="Page">The page.</param>
            /// <param name="Level">The level of the setting that rejects the page.</param>
            public readonly partial record struct PageAlreadyAdded(IPage Page, PageSettingLevel Level)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page '{Page}' is already in the flow; duplicate pages are rejected at the {Level} level.";
            }

            /// <summary>The page is part of the transition the flow is running.</summary>
            /// <param name="Page">The page.</param>
            public readonly partial record struct PageInTransition(IPage Page)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page '{Page}' is part of the running transition.";
            }

            /// <summary>The flow is already removing the page.</summary>
            /// <param name="Page">The page.</param>
            public readonly partial record struct PageBeingRemoved(IPage Page)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The page '{Page}' is already being removed from the flow.";
            }
        }
    }
}
