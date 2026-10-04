using EncosyTower.PageFlows;

namespace Samples.PageFlows
{
    [PageFlowScopeCollection]
    public partial struct GamePageFlowScopes
    {
        public PageFlowScope Screen { get; private set; }

        public PageFlowScope Popup { get; private set; }

        public PageFlowScope FreeTop { get; private set; }
    }

    public partial class GameMenu
    {
        [PageFlowScopeCollection]
        public partial struct MenuPageFlowScopes
        {
            public PageFlowScope Tab { get; private set; }

            public PageFlowScope Dialog { get; set; }
        }
    }
}
