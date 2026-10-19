using EncosyTower.Common;

namespace EncosyTower.PageFlows
{
    /// <summary>The result of a page stack pop and the page the pop removed from the stack.</summary>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    /// <param name="Result">The result of the pop.</param>
    /// <param name="Popped">The page the pop removed, if any.</param>
    public readonly record struct PagePopResult<TPage>(Success<PageFlowError> Result, Option<TPage> Popped)
        where TPage : class, IPage;
}
