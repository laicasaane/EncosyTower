using EncosyTower.CodeGen;
using EncosyTower.PubSub;
using EncosyTower.Samples.Persistence.Shared;

namespace EncosyTower.Samples.Persistence.Persistences;

public readonly record struct PlayerAccessorScope
{
}

[PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(PlayerAccessorScope))]
public readonly partial record struct OnItemAmountUpdatedMsg(ItemId Id, Changed<int> Value);
