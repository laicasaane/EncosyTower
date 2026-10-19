using System;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.State;

namespace EncosyTower.Samples.UitkPages.Attachments
{
    /// <summary>The three flows attached to one screen: the navigation bar, the tabs and the cards.</summary>
    internal sealed class AttachmentSet : IDisposable
    {
        private readonly AppState _state;

        public AttachmentSet(
              AppState state
            , UitkPageStackSingle navigationBarFlow
            , UitkPageListSingle tabsFlow
            , UitkPageListMulti cardsFlow
        )
        {
            _state = state;
            NavigationBarFlow = navigationBarFlow;
            TabsFlow = tabsFlow;
            CardsFlow = cardsFlow;

            _state.Changed += OnStateChanged;
        }

        public event Action Changed;

        public AppState State => _state;

        public UitkPageStackSingle NavigationBarFlow { get; }

        public UitkPageListSingle TabsFlow { get; }

        public UitkPageListMulti CardsFlow { get; }

        public PageFlowScope NavigationBarScope => NavigationBarFlow.Context.FlowScope;

        public PageFlowScope TabsScope => TabsFlow.Context.FlowScope;

        public PageFlowScope CardsScope => CardsFlow.Context.FlowScope;

        public NavigationBarPage Bar { get; set; }

        public BookLocation Location { get; set; }

        public bool InUse { get; set; }

        public bool IsPopulated { get; set; }

        public void Notify()
        {
            Changed?.Invoke();
        }

        public void Dispose()
        {
            _state.Changed -= OnStateChanged;

            NavigationBarFlow.Dispose();
            TabsFlow.Dispose();
            CardsFlow.Dispose();

            Bar = null;
            InUse = false;
        }

        private void OnStateChanged()
        {
            if (InUse)
            {
                Notify();
            }
        }
    }
}
