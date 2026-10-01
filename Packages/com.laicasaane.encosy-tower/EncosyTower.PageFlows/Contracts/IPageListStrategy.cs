using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
    public interface IPageListStrategy<TPage>
        where TPage : class, IPage
    {
        int IndexOf(TPage page);

        UnityTask<bool> AddAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        UnityTask<bool> AddAsync(
              [NotNull] Func<CancellationToken, UnityTask<TPage>> factory
            , PageContext context
            , CancellationToken token
        );

        UnityTask<bool> RemoveAllAsync(PageContext context, CancellationToken token);

        UnityTask<bool> RemoveAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        UnityTask<bool> RemoveAsync(int index, PageContext context, CancellationToken token);

        UnityTask<bool> ShowAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        UnityTask<bool> ShowAsync(
              [NotNull] Func<CancellationToken, UnityTask<TPage>> factory
            , PageContext context
            , CancellationToken token
        );

        UnityTask<bool> ShowAsync(int index, PageContext context, CancellationToken token);
    }
}
