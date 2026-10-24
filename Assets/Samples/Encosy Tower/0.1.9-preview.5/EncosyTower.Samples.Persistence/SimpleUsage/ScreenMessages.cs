using EncosyTower.CodeGen;
using EncosyTower.PubSub;

namespace EncosyTower.Samples.Persistence.SimpleUsage;

internal readonly record struct ScreenScope();

[PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(ScreenScope))]
internal readonly partial record struct ShowMainMenuScreenMsg();

[PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = typeof(ScreenScope))]
internal readonly partial record struct ShowLobbyScreenMsg();
