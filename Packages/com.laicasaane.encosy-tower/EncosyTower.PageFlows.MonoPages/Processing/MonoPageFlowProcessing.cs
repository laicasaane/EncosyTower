using System.Collections.Generic;
using EncosyTower.CodeGen;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Processing;

namespace EncosyTower.PageFlows.MonoPages
{
    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct IsInTransitionRequest() : IRequest<bool>;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetPageIndexRequest(IMonoPage Page) : IRequest<int>;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetCurrentPageRequest() : IRequest<Option<IMonoPage>>;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetPageListRequest() : IRequest<ListFast<IMonoPage>.ReadOnly>;

    [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct GetPageCollectionRequest() : IRequest<IReadOnlyCollection<IMonoPage>>;
}
