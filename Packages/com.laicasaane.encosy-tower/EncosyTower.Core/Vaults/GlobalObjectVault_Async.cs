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

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntilContains<T>(Id<T> id, CancellationToken token = default)
            => s_vaultIdT.WaitUntilContains(ToId2(id), token);

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<T>> TryGetAsync<T>(
              Id<T> id
            , UnityObject context = null
            , CancellationToken token = default
        )
            => s_vaultIdT.TryGetAsync<T>(ToId2(id), context, token);

        #region    ID2
        #endregion ===

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntilContains<T>(Id2 id, CancellationToken token = default)
            => s_vaultId2.WaitUntilContains(id, token);

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<T>> TryGetAsync<T>(
              Id2 id
            , UnityObject context = null
            , CancellationToken token = default
        )
            => s_vaultId2.TryGetAsync<T>(id, context, token);

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<object>> TryGetAsync(
              Id2 id
            , UnityObject context = null
            , CancellationToken token = default
        )
            => s_vaultId2.TryGetAsync(id, context, token);

        #region    STRINGID<T>
        #endregion ===========

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntilContains<T>(StringId<T> id, CancellationToken token = default)
            => s_vaultStringId.WaitUntilContains(ToMetaStringId(id), token);

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<T>> TryGetAsync<T>(
              StringId<T> id
            , UnityObject context = null
            , CancellationToken token = default
        )
            => s_vaultStringId.TryGetAsync<T>(ToMetaStringId(id), context, token);
    }
}
