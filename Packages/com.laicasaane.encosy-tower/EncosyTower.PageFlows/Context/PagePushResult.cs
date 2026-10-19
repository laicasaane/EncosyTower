using EncosyTower.Common;

namespace EncosyTower.PageFlows
{
    /// <summary>The result of a single page stack push and the page the push removed from the stack.</summary>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    /// <param name="Result">The result of the push.</param>
    /// <param name="Displaced">The page the push removed, if any.</param>
    public readonly record struct PagePushResult<TPage>(Success<PageFlowError> Result, Option<TPage> Displaced)
        where TPage : class, IPage;
}
