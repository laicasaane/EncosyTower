using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.StringIds;
using EncosyTower.Tasks;

namespace EncosyTower.Vaults
{
    using UnityObject = UnityEngine.Object;

    public static partial class GlobalObjectVault
    {
        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntilContains<T>(Id<T> id, CancellationToken token = default)
            => s_vaultIdT.WaitUntilContains(ToId2(id), token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<T>> TryGetAsync<T>(
              Id<T> id
            , UnityObject context = null
            , CancellationToken token = default
        )
            => s_vaultIdT.TryGetAsync<T>(ToId2(id), context, token);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntilContains<T>(Id2 id, CancellationToken token = default)
            => s_vaultId2.WaitUntilContains(id, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<T>> TryGetAsync<T>(
              Id2 id
            , UnityObject context = null
            , CancellationToken token = default
        )
            => s_vaultId2.TryGetAsync<T>(id, context, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<object>> TryGetAsync(
              Id2 id
            , UnityObject context = null
            , CancellationToken token = default
        )
            => s_vaultId2.TryGetAsync(id, context, token);

        #region    STRINGID<T>
        #endregion ===========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntilContains<T>(StringId<T> id, CancellationToken token = default)
            => s_vaultStringId.WaitUntilContains(ToMetaStringId(id), token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<T>> TryGetAsync<T>(
              StringId<T> id
            , UnityObject context = null
            , CancellationToken token = default
        )
            => s_vaultStringId.TryGetAsync<T>(ToMetaStringId(id), context, token);
    }
}
