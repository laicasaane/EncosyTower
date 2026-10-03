using System.Collections.Generic;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.Vaults
{
    partial class ValueVault<TId, TValue>
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
        public async UnityTask WaitUntil(TId id, TValue other, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var map = _map;

            while (map.TryGetValue(id, out var value) == false
                || EqualityComparer<TValue>.Default.Equals(value, other) == false
            )
            {
                await UnityTask.NextFrameAsync(token);
                token.ThrowIfCancellationRequested();
            }
        }

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        public async UnityTask<Option<TValue>> TryGetAsync(TId id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var map = _map;
            TValue value;

            while (map.TryGetValue(id, out value) == false)
            {
                await UnityTask.NextFrameAsync(token);
                token.ThrowIfCancellationRequested();
            }

            return Option.Some(value);
        }
    }
}
