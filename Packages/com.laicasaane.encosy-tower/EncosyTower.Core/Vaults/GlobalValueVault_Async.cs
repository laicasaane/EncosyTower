using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.StringIds;
using EncosyTower.Tasks;

namespace EncosyTower.Vaults
{
    public static partial class GlobalValueVault<TValue>
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
        public static UnityTask WaitUntil<T>(Id<T> id, TValue value, CancellationToken token = default)
            => s_vaultIdT.WaitUntil(ToId2(id), value, token);

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<TValue>> TryGetAsync<T>(Id<T> id, CancellationToken token = default)
            => s_vaultIdT.TryGetAsync(ToId2(id), token);

        #region    ID2
        #endregion ===

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntilContains(Id2 id, CancellationToken token = default)
            => s_vaultId2.WaitUntilContains(id, token);

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask WaitUntil(Id2 id, TValue value, CancellationToken token = default)
            => s_vaultId2.WaitUntil(id, value, token);

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<TValue>> TryGetAsync(Id2 id, CancellationToken token = default)
            => s_vaultId2.TryGetAsync(id, token);

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
        public static UnityTask WaitUntil<T>(StringId<T> id, TValue value, CancellationToken token = default)
            => s_vaultStringId.WaitUntil(ToMetaStringId(id), value, token);

        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<TValue>> TryGetAsync<T>(StringId<T> id, CancellationToken token = default)
            => s_vaultStringId.TryGetAsync(ToMetaStringId(id), token);
    }
}
