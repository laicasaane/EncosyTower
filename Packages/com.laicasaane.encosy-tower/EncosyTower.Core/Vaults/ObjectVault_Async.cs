using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.Vaults
{
    using UnityObject = UnityEngine.Object;

    partial class ObjectVault<TId>
    {
        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        public async UnityTask WaitUntilContains(TId id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var map = _map;

            while (map.ContainsKey(id) == false)
            {
                await UnityTask.NextFrameAsync(token);
                token.ThrowIfCancellationRequested();
            }
        }

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        public async UnityTask<Option<T>> TryGetAsync<T>(TId id, UnityObject context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var map = _map;
            object obj;

            while (map.TryGetValue(id, out obj) == false)
            {
                await UnityTask.NextFrameAsync(token);
                token.ThrowIfCancellationRequested();
            }

            return TryCast<T>(id, obj, context);
        }

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        public async UnityTask<Option<object>> TryGetAsync(TId id, UnityObject context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var map = _map;
            object obj;

            while (map.TryGetValue(id, out obj) == false)
            {
                await UnityTask.NextFrameAsync(token);
                token.ThrowIfCancellationRequested();
            }

            return TryCast(id, obj, context);
        }
    }
}
