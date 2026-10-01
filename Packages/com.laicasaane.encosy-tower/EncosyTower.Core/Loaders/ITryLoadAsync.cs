using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.Loaders
{
    public interface ITryLoadAsync<T>
    {
        UnityTask<Option<T>> TryLoadAsync(CancellationToken token);
    }
}
