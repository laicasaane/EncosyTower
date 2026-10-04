using System.Collections.Generic;
using EncosyTower.CodeGen;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Processing;

namespace EncosyTower.PageFlows.UitkPages
{
    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetPageIndexRequest(IUitkPage Page) : IRequest<int>;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetCurrentPageRequest() : IRequest<Option<IUitkPage>>;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetPageListRequest() : IRequest<ListFast<IUitkPage>.ReadOnly>;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetPageCollectionRequest() : IRequest<IReadOnlyCollection<IUitkPage>>;
}
