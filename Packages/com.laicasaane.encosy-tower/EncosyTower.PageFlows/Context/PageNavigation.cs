using EncosyTower.AssetKeys;
using EncosyTower.Common;

namespace EncosyTower.PageFlows
{
    /// <summary>The kind of navigation a completed page flow call made.</summary>
    public enum PageNavigationKind
    {
        /// <summary>A single-page stack showed a page.</summary>
        Show,

        /// <summary>A single-page stack hid its page.</summary>
        Hide,

        /// <summary>A multi-page stack pushed a page.</summary>
        Push,

        /// <summary>A multi-page stack popped a page.</summary>
        Pop,

        /// <summary>A list showed the page at an index.</summary>
        ShowAt,

        /// <summary>A list hid the page at an index.</summary>
        HideAt,
    }

    /// <summary>
    /// A page in a navigation: its page instance, asset key and optional index, set only for list flows.
    /// </summary>
    /// <param name="Page">The page instance.</param>
    /// <param name="AssetKey">The key of the page asset.</param>
    /// <param name="Index">The index of the page in a list flow; none for a stack.</param>
    public readonly record struct PageRef(IPage Page, AssetKey AssetKey, Option<int> Index) : IIsValid
    {
        /// <summary>A reference that names no page.</summary>
        public static readonly PageRef None = default;

        /// <summary>True when the reference names a page.</summary>
        public bool IsValid => AssetKey.IsValid;
    }

    /// <summary>Describes a completed page flow call.</summary>
    /// <param name="FlowId">The identifier of the flow that ran the call.</param>
    /// <param name="Kind">The kind of the navigation.</param>
    /// <param name="From">The page the call hid or removed, if any.</param>
    /// <param name="To">The page the call showed, if any.</param>
    /// <param name="Context">The context of the call.</param>
    /// <param name="IssuedBy">The object that issued the call; see <see cref="PageContext.IssuedBy"/>.</param>
    public readonly record struct PageNavigation(
          string FlowId
        , PageNavigationKind Kind
        , PageRef From
        , PageRef To
        , PageContext Context
        , object IssuedBy
    )
    {
        /// <summary>
        /// The object that issued this call, read from PageContext.IssuedBy; null for calls made by the game.
        /// </summary>
        public object IssuedBy { get; init; } = IssuedBy;
    }
}
