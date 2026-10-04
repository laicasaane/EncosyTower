using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
    public interface IPageFlow
    {
        bool IsInTransition { get; }
    }

    /// <summary>
    /// A collection of <see cref="PageFlowScope"/> values, one for each page flow.
    /// </summary>
    /// <remarks>
    /// Declare a <c>partial struct</c> with <see cref="PageFlowScopeCollectionAttribute"/>;
    /// the source generator implements this interface.
    /// </remarks>
    /// <example>
    /// <code>
    /// [PageFlowScopeCollection]
    /// public partial struct GamePageFlowScopes
    /// {
    ///     public PageFlowScope Screen { get; private set; }
    ///
    ///     public PageFlowScope Popup { get; private set; }
    ///
    ///     public PageFlowScope FreeTop { get; private set; }
    /// }
    /// </code>
    /// </example>
    public interface IPageFlowScopeCollection
    {
        ReadOnlyMemory<string> ScopeIdentifiers { get; }

        bool TrySetScope(string identifier, PageFlowScope scope);
    }

    public interface ISinglePageStack<TPage> : IPageFlow
        , IPageStackStrategy<TPage>
        , IHasCurrentPage<TPage>
        where TPage : class, IPage
    {
    }

    public interface IMultiPageStack<TPage> : IPageFlow
        , IPageStackStrategy<TPage>
        , IHasCurrentPage<TPage>
        , IHasPageCollection<TPage>
        where TPage : class, IPage
    {
        UnityTask<bool> RemoveAllAsync(PageContext context, CancellationToken token);
    }

    public interface ISinglePageList<TPage> : IPageFlow
        , IPageListStrategy<TPage>
        , IHasCurrentPage<TPage>
        , IHasPages<TPage>
        , IHasPageCollection<TPage>
        where TPage : class, IPage
    {
        UnityTask<bool> HideAsync(PageContext context, CancellationToken token);
    }

    public interface IMultiPageList<TPage> : IPageFlow
        , IPageListStrategy<TPage>
        , IHasPages<TPage>
        where TPage : class, IPage
    {
        UnityTask<bool> HideAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        UnityTask<bool> HideAsync(
              [NotNull] Func<CancellationToken, UnityTask<TPage>> factory
            , PageContext context
            , CancellationToken token
        );

        UnityTask<bool> HideAsync(int index, PageContext context, CancellationToken token);
    }

    public interface IHasCurrentPage<TPage>
        where TPage : class, IPage
    {
        Option<TPage> CurrentPage { get; }
    }

    public interface IHasPageCollection<TPage>
        where TPage : class, IPage
    {
        IReadOnlyCollection<TPage> PageCollection { get; }
    }

    public interface IHasPages<TPage>
        where TPage : class, IPage
    {
        ListFast<TPage>.ReadOnly Pages { get; }
    }
}
