using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.Vaults
{
    using UnityObject = UnityEngine.Object;

    partial class ObjectVault<TId>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UnityTask WaitUntilContains(TId id, CancellationToken token = default)
        {
            var map = _map;

            while (map.ContainsKey(id) == false)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await UnityTask.NextFrameAsync(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UnityTask<Option<T>> TryGetAsync<T>(TId id, UnityObject context, CancellationToken token)
        {
            await WaitUntilContains(id, token);

            if (token.IsCancellationRequested)
            {
                return Option.None;
            }

            return TryCast<T>(id, _map[id], context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UnityTask<Option<object>> TryGetAsync(TId id, UnityObject context, CancellationToken token)
        {
            await WaitUntilContains(id, token);

            if (token.IsCancellationRequested)
            {
                return Option.None;
            }

            return TryCast(id, _map[id], context);
        }
    }
}
