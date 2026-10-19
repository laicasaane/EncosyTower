
namespace EncosyTower.PageFlows.Codex
{
    /// <summary>The kind of a page flow that a codex creates.</summary>
    public enum PageFlowKind
    {
        /// <summary>A stack that holds at most one page; see <see cref="IPageStackSingle{TPage}"/>.</summary>
        PageStackSingle,

        /// <summary>A stack that keeps every pushed page; see <see cref="IPageStackMulti{TPage}"/>.</summary>
        PageStackMulti,

        /// <summary>A list that shows at most one of its pages; see <see cref="IPageListSingle{TPage}"/>.</summary>
        PageListSingle,

        /// <summary>A list that can show any number of its pages; see <see cref="IPageListMulti{TPage}"/>.</summary>
        PageListMulti,
    }
}
