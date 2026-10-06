using EncosyTower.PageFlows;

namespace EncosyTower.Samples.UitkPages
{
    [PageFlowScopeCollection]
    public partial struct GamePageFlowScopes
    {
        public PageFlowScope Screen { get; private set; }

        public PageFlowScope Popup { get; private set; }

        public PageFlowScope FreeTop { get; private set; }
    }
}
