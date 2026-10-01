using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.Loaders
{
    public interface ILoadOrErrorAsync<TValue, TError>
    {
        UnityTask<Result<TValue, TError>> LoadOrErrorAsync(CancellationToken token);
    }
}
