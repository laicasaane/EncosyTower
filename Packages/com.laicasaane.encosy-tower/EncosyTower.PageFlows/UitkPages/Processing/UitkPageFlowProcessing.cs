using System.Collections.Generic;
using EncosyTower.CodeGen;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.Processing;

namespace EncosyTower.PageFlows.UitkPages.Processing
{
    /// <summary>Asks a page list for the index of a page.</summary>
    /// <remarks>
    /// Every UI Toolkit page list of the scope answers with the index, or <c>-1</c> when the list does not hold the
    /// page.
    /// </remarks>
    /// <param name="Page">The page to find.</param>
    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetPageIndexRequest(IUitkPage Page) : IRequest<int>;

    /// <summary>Asks a page flow for its current page.</summary>
    /// <remarks>Every UI Toolkit page stack and single-page list of the scope answers.</remarks>
    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetCurrentPageRequest() : IRequest<Option<IUitkPage>>;

    /// <summary>Asks a page list for its pages by index.</summary>
    /// <remarks>Every UI Toolkit page list of the scope answers.</remarks>
    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetPageListRequest() : IRequest<ListFast<IUitkPage>.ReadOnly>;

    /// <summary>Asks a page flow for its pages as a collection.</summary>
    /// <remarks>Every UI Toolkit multi-page stack and page list of the scope answers.</remarks>
    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetPageCollectionRequest() : IRequest<IReadOnlyCollection<IUitkPage>>;
}
