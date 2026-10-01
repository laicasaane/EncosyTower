using System.Threading;
using EncosyTower.Tasks;

namespace EncosyTower.Loaders
{
    public interface ILoadAsync<T>
    {
        UnityTask<T> LoadAsync(CancellationToken token);
    }
}
