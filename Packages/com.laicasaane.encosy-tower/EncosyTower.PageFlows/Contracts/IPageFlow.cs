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
    /// The type implements this interface must also define writable properties
    /// whose return type is <see cref="PageFlowScope"/>.
    /// </summary>
    /// <remarks>
    /// Because the system uses reflection mechanism to retrieve property information from this type,
    /// the type and all of its properties should be annotated with <c>[UnityEngine.Scripting.Preserve]</c>
    /// so their code will not be stripped away at build time.
    /// </remarks>
    /// <seealso cref="UnityEngine.Scripting.PreserveAttribute"/>
    /// <example>
    /// <code>
    /// [UnityEngine.Scripting.Preserve]
    /// public struct GameFlowScopes : IPageFlowScopeCollection
    /// {
    ///     [UnityEngine.Scripting.Preserve]
    ///     public PageFlowScope Screen { get; set; }
    ///
    ///     [UnityEngine.Scripting.Preserve]
    ///     public PageFlowScope Popup { get; set; }
    ///
    ///     [UnityEngine.Scripting.Preserve]
    ///     public PageFlowScope FreeTop { get; set; }
    /// }
    /// </code>
    /// </example>
    public interface IPageFlowScopeCollection
    {
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
