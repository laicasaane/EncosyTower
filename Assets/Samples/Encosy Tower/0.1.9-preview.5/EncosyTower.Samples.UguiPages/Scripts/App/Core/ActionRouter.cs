using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.PageFlows;
using EncosyTower.PubSub;
using EncosyTower.Samples.UguiPages.Messages;
using EncosyTower.Tasks;

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>
    /// The only entry point for what the pages ask for: it receives <see cref="AppActionMessage"/> and hands
    /// each <see cref="AppAction"/> to the module that owns it.
    /// </summary>
    internal sealed class ActionRouter
    {
        private readonly CodexContext _ctx;
        private readonly AppController _book;
        private readonly ScreenController _screens;
        private readonly DimensionController _dimensions;
        private readonly DialogController _dialogs;
        private readonly PopupController _popups;
        private readonly IncantationController _incantation;
        private readonly List<ISubscription> _subscriptions = new();

        public ActionRouter(
              CodexContext ctx
            , AppController book
            , ScreenController screens
            , DimensionController dimensions
            , DialogController dialogs
            , PopupController popups
            , IncantationController incantation
        )
        {
            _ctx = ctx;
            _book = book;
            _screens = screens;
            _dimensions = dimensions;
            _dialogs = dialogs;
            _popups = popups;
            _incantation = incantation;
        }

        public void Start()
        {
            var screen = _ctx.Codex.FlowContext.Subscriber
                .Scope(_ctx.Scopes.Screen)
                .WithState(this)
                .WithSubscriptions(_subscriptions);

            AppActionMessage.Subscribe(in screen, HandleAction);
        }

        public void Dispose()
        {
            _subscriptions.Unsubscribe();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HandleAction(ActionRouter owner, AppActionMessage message)
        {
            owner.Route(message);
        }

        private void Route(AppActionMessage message)
        {
            if (_ctx.IsReady == false)
            {
                return;
            }

            switch (message.Action)
            {
                case AppAction.NewGame: _book.NewGameAsync().Forget(); break;
                case AppAction.Continue: _book.ContinueAsync().Forget(); break;
                case AppAction.Exit: _book.ExitAsync().Forget(); break;

                case AppAction.Travel: _screens.RequestTravel(message.Location); break;
                case AppAction.Back: _screens.BackAsync().Forget(); break;
                case AppAction.Forward: _screens.ForwardAsync().Forget(); break;

                case AppAction.SelectTab: _dimensions.SelectTabAsync(message.Index).Forget(); break;
                case AppAction.ToggleCard: _dimensions.ToggleCardAsync(message.Index, toggle: true).Forget(); break;
                case AppAction.CloseCard: _dimensions.ToggleCardAsync(message.Index, toggle: false).Forget(); break;

                case AppAction.More: _dialogs.OpenActionsAsync().Forget(); break;
                case AppAction.ReadEcho: _dialogs.OpenEchoAsync().Forget(); break;
                case AppAction.ReadAppendix: _dialogs.OpenAppendixAsync().Forget(); break;
                case AppAction.OpenMap: _dialogs.OpenMapAsync().Forget(); break;
                case AppAction.CloseMap: _dialogs.CloseMapAsync().Forget(); break;
                case AppAction.MapTravel: _dialogs.MapTravelAsync(message.Location).Forget(); break;
                case AppAction.OpenSettings: _dialogs.OpenSettingsAsync().Forget(); break;
                case AppAction.Reopen: _dialogs.ReopenAsync().Forget(); break;

                case AppAction.Done: _popups.CloseDialogAsync(offerReopen: true).Forget(); break;
                case AppAction.Cancel: _popups.CancelOpening(); break;
                case AppAction.KeepReading: _popups.KeepReading(); break;
                case AppAction.StopClosing: _popups.AbortPopup(PageAbortMode.Reverse); break;

                case AppAction.CastIncantation: _incantation.CastAsync().Forget(); break;
                case AppAction.ReadName: _incantation.ReadNameAsync().Forget(); break;
                case AppAction.WriteInTheBook: _incantation.WriteInTheBookAsync().Forget(); break;
            }
        }
    }
}
