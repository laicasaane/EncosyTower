#if UNITY_UGUI

using System.Collections.Generic;
using EncosyTower.CodeGen;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Processing;

namespace EncosyTower.PageFlows.UguiPages
{
    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetPageIndexRequest(IUguiPage Page) : IRequest<int>;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetCurrentPageRequest() : IRequest<Option<IUguiPage>>;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetPageListRequest() : IRequest<ListFast<IUguiPage>.ReadOnly>;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetPageCollectionRequest() : IRequest<IReadOnlyCollection<IUguiPage>>;
}

#endif
