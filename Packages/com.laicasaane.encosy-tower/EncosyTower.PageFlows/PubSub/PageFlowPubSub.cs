using System.Threading;
using EncosyTower.CodeGen;
using EncosyTower.Ids;
using EncosyTower.PubSub;

namespace EncosyTower.PageFlows
{
    public readonly partial record struct PageFlowScope(Id3 Value);

    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct AttachPageMessage(IPageFlow Flow, IPage Page, CancellationToken Token);

    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct DetachPageMessage(IPageFlow Flow, IPage Page, CancellationToken Token);

    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct BeginTransitionMessage(
          IPage Previous
        , IPage Current
        , CancellationToken Token
    );

    [PubSub(ApiMode.Async, State = StateMode.Stateful, Scope = typeof(PageFlowScope))]
    public readonly partial record struct EndTransitionMessage(
          IPage Previous
        , IPage Current
        , CancellationToken Token
    );
}
