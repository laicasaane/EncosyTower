using EncosyTower.AssetKeys;
using EncosyTower.CodeGen;
using EncosyTower.PubSub;

namespace EncosyTower.PageFlows.PubSub
{
    /// <summary>Asks a page flow to keep a number of pages of an asset ready in its pool.</summary>
    /// <remarks>
    /// <para>
    /// Handled by every UGUI and UI Toolkit page flow of the scope. The flow loads the asset and creates pages
    /// until its pool holds <paramref name="Amount"/> pages; it fails with the <c>InvalidPrepoolAmount</c> case of
    /// <see cref="PageFlowError"/> when <paramref name="Amount"/> is less than 1.
    /// </para>
    /// <para>
    /// Publish it with <c>PrepoolPageMessage.Async.Publish</c>; handle it with
    /// <c>PrepoolPageMessage.Async.Subscribe</c>.
    /// </para>
    /// </remarks>
    /// <param name="AssetKey">The key of the page asset.</param>
    /// <param name="Amount">The number of pooled pages to have ready.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct PrepoolPageMessage(AssetKey AssetKey, int Amount);

    /// <summary>Asks a page flow to destroy pooled pages of an asset beyond a number.</summary>
    /// <remarks>
    /// <para>
    /// Handled by every UGUI and UI Toolkit page flow of the scope. Pages that are not pooled are not touched.
    /// </para>
    /// <para>Publish it with <c>TrimPoolMessage.Publish</c>; handle it with <c>TrimPoolMessage.Subscribe</c>.</para>
    /// </remarks>
    /// <param name="AssetKey">The key of the page asset.</param>
    /// <param name="AmountToKeep">The number of pooled pages to keep.</param>
    [PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct TrimPoolMessage(AssetKey AssetKey, int AmountToKeep);

    /// <summary>Asks a page list to create the page of an asset and add it, without showing it.</summary>
    /// <remarks>
    /// <para>
    /// Handled by every UGUI and UI Toolkit page list of the scope. The result is reported to
    /// <see cref="PageContext.Report"/>.
    /// </para>
    /// <para>
    /// Publish it with <c>AddPageMessage.Async.Publish</c>; handle it with <c>AddPageMessage.Async.Subscribe</c>.
    /// </para>
    /// </remarks>
    /// <param name="AssetKey">The key of the page asset.</param>
    /// <param name="Context">The context of the call.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct AddPageMessage(AssetKey AssetKey, PageContext Context);

    /// <summary>Asks a page stack to create the page of an asset, push it and show it.</summary>
    /// <remarks>
    /// <para>
    /// Handled by every UGUI and UI Toolkit page stack of the scope. The result is reported to
    /// <see cref="PageContext.Report"/>.
    /// </para>
    /// <para>
    /// Publish it with <c>ShowPageMessage.Async.Publish</c>; handle it with <c>ShowPageMessage.Async.Subscribe</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var report = new PageCallReport();
    /// var context = new PageContext { AsyncOperation = PageAsyncOperation.Sequential, Report = report };
    /// var publisher = codex.FlowContext.Publisher.Scope(scopes.Popup);
    /// var publishing = PublishingContext.Default(logger: logger, token: token);
    ///
    /// await ShowPageMessage.Async.Publish(in publisher, new ShowPageMessage(assetKey, context), publishing);
    ///
    /// if (report.IsReported &amp;&amp; report.Result.TryGetFailure(out var error)) error.Log(logger);
    ///
    /// // HideActivePageMessage works the same way. A direct call on a flow returns the result:
    /// var result = await stack.HideActivePageAsync(context with { Report = null }, token);
    /// </code>
    /// </example>
    /// <param name="AssetKey">The key of the page asset.</param>
    /// <param name="Context">The context of the call.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct ShowPageMessage(AssetKey AssetKey, PageContext Context);

    /// <summary>Asks a page list to show the page at an index.</summary>
    /// <remarks>
    /// <para>
    /// Handled by every UGUI and UI Toolkit page list of the scope. The result is reported to
    /// <see cref="PageContext.Report"/>.
    /// </para>
    /// <para>
    /// Publish it with <c>ShowPageAtIndexMessage.Async.Publish</c>; handle it with
    /// <c>ShowPageAtIndexMessage.Async.Subscribe</c>.
    /// </para>
    /// </remarks>
    /// <param name="Index">The index of the page to show.</param>
    /// <param name="Context">The context of the call.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct ShowPageAtIndexMessage(int Index, PageContext Context);

    /// <summary>Asks a multi-page list to hide the page at an index.</summary>
    /// <remarks>
    /// <para>
    /// Handled by every UGUI and UI Toolkit multi-page list of the scope. The result is reported to
    /// <see cref="PageContext.Report"/>.
    /// </para>
    /// <para>
    /// Publish it with <c>HidePageAtIndexMessage.Async.Publish</c>; handle it with
    /// <c>HidePageAtIndexMessage.Async.Subscribe</c>.
    /// </para>
    /// </remarks>
    /// <param name="Index">The index of the page to hide.</param>
    /// <param name="Context">The context of the call.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct HidePageAtIndexMessage(int Index, PageContext Context);

    /// <summary>Asks a page stack to pop its current page, or a single-page list to hide its current page.</summary>
    /// <remarks>
    /// <para>
    /// Handled by every UGUI and UI Toolkit page stack and single-page list of the scope. The result is reported
    /// to <see cref="PageContext.Report"/>.
    /// </para>
    /// <para>
    /// Publish it with <c>HideActivePageMessage.Async.Publish</c>; handle it with
    /// <c>HideActivePageMessage.Async.Subscribe</c>.
    /// </para>
    /// </remarks>
    /// <param name="Context">The context of the call.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct HideActivePageMessage(PageContext Context);

    /// <summary>
    /// Asks a multi-page stack to hide all its pages at the same time, or a multi-page list to hide all its
    /// shown pages at the same time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Handled by every UGUI and UI Toolkit multi-page stack and multi-page list of the scope. The result is
    /// reported to <see cref="PageContext.Report"/>.
    /// </para>
    /// <para>
    /// Publish it with <c>HideAllPagesMessage.Async.Publish</c>; handle it with
    /// <c>HideAllPagesMessage.Async.Subscribe</c>.
    /// </para>
    /// </remarks>
    /// <param name="Context">The context of the call.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct HideAllPagesMessage(PageContext Context);

    /// <summary>Asks a multi-page list to show all its hidden pages at the same time.</summary>
    /// <remarks>
    /// <para>
    /// Handled by every UGUI and UI Toolkit multi-page list of the scope. The result is reported to
    /// <see cref="PageContext.Report"/>.
    /// </para>
    /// <para>
    /// Publish it with <c>ShowAllPagesMessage.Async.Publish</c>; handle it with
    /// <c>ShowAllPagesMessage.Async.Subscribe</c>.
    /// </para>
    /// </remarks>
    /// <param name="Context">The context of the call.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct ShowAllPagesMessage(PageContext Context);

    /// <summary>
    /// Published by a wrapper when an operation failed, other than by an abort. A cancel or an unheard message is
    /// published only when it carries another failure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A flow never logs a failure it publishes here. A subscriber should log the failure with
    /// <c>Error.Log(ILogger)</c>.
    /// </para>
    /// <para>Handle it with <c>PageFlowFailedMessage.Subscribe</c>.</para>
    /// </remarks>
    /// <param name="Error">The failure.</param>
    /// <param name="Context">The context of the failed call.</param>
    [PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct PageFlowFailedMessage(PageFlowError Error, PageContext Context);

    /// <summary>Asks the flow of this scope to abort the show or hide it is running.</summary>
    /// <remarks>
    /// <para>
    /// Handled by every UGUI and UI Toolkit page flow of the scope with
    /// <see cref="IPageFlow.AbortCurrent(PageAbortMode)"/>; the result of that call is not reported back.
    /// </para>
    /// <para>Publish it with <c>AbortPageMessage.Publish</c>; handle it with <c>AbortPageMessage.Subscribe</c>.</para>
    /// </remarks>
    /// <param name="Mode">How the screen goes back.</param>
    [PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct AbortPageMessage(PageAbortMode Mode);

    /// <summary>Published by a wrapper when an operation ended aborted or withdrawn.</summary>
    /// <remarks>
    /// <para>
    /// <c>Error</c> is the <c>Aborted</c> or <c>Withdrawn</c> failure with its suppressed errors: the failures the flow
    /// met while it undid the call. A flow never logs them. A subscriber that treats the abort as expected logs only
    /// those, with <c>Error.LogSuppressed(ILogger)</c>.
    /// </para>
    /// <para>Handle it with <c>PageAbortedMessage.Subscribe</c>.</para>
    /// </remarks>
    /// <param name="Origin">Who asked for the abort.</param>
    /// <param name="Mode">How the screen went back.</param>
    /// <param name="Error">The abort failure, with its suppressed errors.</param>
    /// <param name="Context">The context of the aborted call.</param>
    [PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct PageAbortedMessage(
          PageAbortOrigin Origin
        , PageAbortMode Mode
        , PageFlowError Error
        , PageContext Context
    );

    /// <summary>
    /// Published by a wrapper after a show or hide whose change was applied. Never for aborted, withdrawn or
    /// not-started calls.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="EncosyTower.PageFlows.History.PageHistory"/> and a
    /// <see cref="EncosyTower.PageFlows.History.PageHistoryLog"/> handle it to record the
    /// navigation.
    /// </para>
    /// <para>Handle it with <c>PageNavigatedMessage.Async.Subscribe</c>.</para>
    /// </remarks>
    /// <param name="Navigation">The navigation that completed.</param>
    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct PageNavigatedMessage(PageNavigation Navigation);
}
