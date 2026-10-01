using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Tasks;

namespace EncosyTower.PageFlows
{
    public interface IPageStackStrategy<TPage>
        where TPage : class, IPage
    {
        UnityTask<bool> PopAsync(PageContext context, CancellationToken token);

        UnityTask<bool> PushAsync([NotNull] TPage page, PageContext context, CancellationToken token);

        UnityTask<bool> PushAsync(
              [NotNull] Func<CancellationToken, UnityTask<TPage>> factory
            , PageContext context
            , CancellationToken token
        );
    }
}
