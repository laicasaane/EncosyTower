#pragma warning disable IDE0060 // Remove unused parameter

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Ids;
using EncosyTower.Tasks;
using EncosyTower.Vaults;

namespace EncosyTower.Types
{
    /// <summary>
    /// Stores and reads the global object or value that a <see cref="TypeFlagLink{TOwner, TLinked}"/> keys.
    /// </summary>
    public static class TypeFlagLinkExtensions
    {
        /// <summary>
        /// Stores <paramref name="obj"/> for <typeparamref name="TOwner"/> unless an object of
        /// <typeparamref name="TObject"/> is already stored for it.
        /// </summary>
        /// <typeparam name="TOwner">The type flag owner.</typeparam>
        /// <typeparam name="TObject">The type of the stored object.</typeparam>
        /// <param name="self">The link that keys the entry.</param>
        /// <param name="obj">The object to store.</param>
        /// <returns><see langword="true"/> when <paramref name="obj"/> is stored; <see langword="false"/> when another
        /// object is already stored, which keeps the first one.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAddObject<TOwner, TObject>(this TypeFlagLink<TOwner, TObject> self, [NotNull] TObject obj)
            where TObject : class
            => GlobalObjectVault.TryAdd(GetKey<TOwner, TObject>(), obj);

        /// <summary>
        /// Removes the object of <typeparamref name="TObject"/> stored for <typeparamref name="TOwner"/> when it is
        /// <paramref name="expected"/>.
        /// </summary>
        /// <typeparam name="TOwner">The type flag owner.</typeparam>
        /// <typeparam name="TObject">The type of the stored object.</typeparam>
        /// <param name="self">The link that keys the entry.</param>
        /// <param name="expected">The instance to remove.</param>
        /// <returns><see langword="true"/> only when the stored object is the same instance as
        /// <paramref name="expected"/> and is removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemoveObject<TOwner, TObject>(this TypeFlagLink<TOwner, TObject> self, TObject expected)
            where TObject : class
            => GlobalObjectVault.TryRemove(GetKey<TOwner, TObject>(), expected);

        /// <summary>
        /// Gets the object of <typeparamref name="TObject"/> stored for <typeparamref name="TOwner"/>.
        /// </summary>
        /// <typeparam name="TOwner">The type flag owner.</typeparam>
        /// <typeparam name="TObject">The type of the stored object.</typeparam>
        /// <param name="self">The link that keys the entry.</param>
        /// <param name="obj">The stored object, or <see langword="null"/> when none is stored.</param>
        /// <returns><see langword="true"/> only when a live object is stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetObject<TOwner, TObject>(
              this TypeFlagLink<TOwner, TObject> self
            , [MaybeNullWhen(false)] out TObject obj
        )
            where TObject : class
        {
            if (GlobalObjectVault.TryGet<TObject>(GetKey<TOwner, TObject>(), out var result)
                && result.TryGetValue(out obj)
            )
            {
                return true;
            }

            obj = default;
            return false;
        }

        /// <summary>
        /// Gets the object of <typeparamref name="TObject"/> stored for <typeparamref name="TOwner"/>.
        /// </summary>
        /// <typeparam name="TOwner">The type flag owner.</typeparam>
        /// <typeparam name="TObject">The type of the stored object.</typeparam>
        /// <param name="self">The link that keys the entry.</param>
        /// <returns>The stored object.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when no live object is stored.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TObject GetObjectOrThrow<TOwner, TObject>(this TypeFlagLink<TOwner, TObject> self)
            where TObject : class
        {
            if (self.TryGetObject(out var obj))
            {
                return obj;
            }

            Internals.ThrowHelper.ThrowGlobalObjectNotFound<TOwner, TObject>();
            return default;
        }

        /// <summary>
        /// Waits until an object of <typeparamref name="TObject"/> is stored for <typeparamref name="TOwner"/>, then
        /// gets it.
        /// </summary>
        /// <remarks>
        /// Completes without waiting for a frame when an object is already stored; otherwise checks once per frame.
        /// </remarks>
        /// <typeparam name="TOwner">The type flag owner.</typeparam>
        /// <typeparam name="TObject">The type of the stored object.</typeparam>
        /// <param name="self">The link that keys the entry.</param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>The stored object.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the stored object is no longer live once the wait completes.
        /// </exception>
        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        public static async UnityTask<TObject> GetObjectAsync<TOwner, TObject>(
              this TypeFlagLink<TOwner, TObject> self
            , CancellationToken token = default
        )
            where TObject : class
        {
            await GlobalObjectVault.WaitUntilContains<TObject>(GetKey<TOwner, TObject>(), token);
            return self.GetObjectOrThrow();
        }

        /// <summary>
        /// Stores <paramref name="value"/> for <typeparamref name="TOwner"/>, replacing any stored value of
        /// <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TOwner">The type flag owner.</typeparam>
        /// <typeparam name="TValue">The type of the stored value.</typeparam>
        /// <param name="self">The link that keys the entry.</param>
        /// <param name="value">The value to store.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue<TOwner, TValue>(this TypeFlagLink<TOwner, TValue> self, TValue value)
            where TValue : struct
            => GlobalValueVault<TValue>.TrySet(GetKey<TOwner, TValue>(), value);

        /// <summary>
        /// Removes the value of <typeparamref name="TValue"/> stored for <typeparamref name="TOwner"/>.
        /// </summary>
        /// <typeparam name="TOwner">The type flag owner.</typeparam>
        /// <typeparam name="TValue">The type of the stored value.</typeparam>
        /// <param name="self">The link that keys the entry.</param>
        /// <param name="value">The removed value, or <see langword="default"/> when none is stored.</param>
        /// <returns><see langword="true"/> only when a value is stored and removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemoveValue<TOwner, TValue>(this TypeFlagLink<TOwner, TValue> self, out TValue value)
            where TValue : struct
            => GlobalValueVault<TValue>.TryRemove(GetKey<TOwner, TValue>(), out value);

        /// <summary>
        /// Gets the value of <typeparamref name="TValue"/> stored for <typeparamref name="TOwner"/>.
        /// </summary>
        /// <typeparam name="TOwner">The type flag owner.</typeparam>
        /// <typeparam name="TValue">The type of the stored value.</typeparam>
        /// <param name="self">The link that keys the entry.</param>
        /// <param name="value">The stored value, or <see langword="default"/> when none is stored.</param>
        /// <returns><see langword="true"/> only when a value is stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<TOwner, TValue>(this TypeFlagLink<TOwner, TValue> self, out TValue value)
            where TValue : struct
            => GlobalValueVault<TValue>.TryGet(GetKey<TOwner, TValue>(), out value);

        /// <summary>
        /// Gets the value of <typeparamref name="TValue"/> stored for <typeparamref name="TOwner"/>.
        /// </summary>
        /// <typeparam name="TOwner">The type flag owner.</typeparam>
        /// <typeparam name="TValue">The type of the stored value.</typeparam>
        /// <param name="self">The link that keys the entry.</param>
        /// <returns>The stored value.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when no value is stored.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue GetValueOrThrow<TOwner, TValue>(this TypeFlagLink<TOwner, TValue> self)
            where TValue : struct
        {
            if (self.TryGetValue(out var value))
            {
                return value;
            }

            Internals.ThrowHelper.ThrowGlobalValueNotFound<TOwner, TValue>();
            return default;
        }

        /// <summary>
        /// Waits until a value of <typeparamref name="TValue"/> is stored for <typeparamref name="TOwner"/>, then gets
        /// it.
        /// </summary>
        /// <remarks>
        /// Completes without waiting for a frame when a value is already stored; otherwise checks once per frame.
        /// </remarks>
        /// <typeparam name="TOwner">The type flag owner.</typeparam>
        /// <typeparam name="TValue">The type of the stored value.</typeparam>
        /// <param name="self">The link that keys the entry.</param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>The stored value.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the value is removed again before the wait completes.
        /// </exception>
        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        public static async UnityTask<TValue> GetValueAsync<TOwner, TValue>(
              this TypeFlagLink<TOwner, TValue> self
            , CancellationToken token = default
        )
            where TValue : struct
        {
            await GlobalValueVault<TValue>.WaitUntilContains(GetKey<TOwner, TValue>(), token);
            return self.GetValueOrThrow();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Id2 GetKey<TOwner, TLinked>()
            => Type<TOwner>.Id.ToId2((Id<TLinked>)Type<TLinked>.Id);
    }
}
