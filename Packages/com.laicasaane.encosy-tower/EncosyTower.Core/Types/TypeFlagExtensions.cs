#pragma warning disable IDE0060 // Remove unused parameter

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Tasks;

namespace EncosyTower.Types
{
    /// <summary>
    /// Stores and reads the instance or value of a type flag owner, keyed by the owner itself.
    /// </summary>
    public static class TypeFlagExtensions
    {
        /// <summary>
        /// Stores <paramref name="instance"/> as the instance of <typeparamref name="T"/>, then enables the flag of
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type flag owner.</typeparam>
        /// <param name="self">The flag of <typeparamref name="T"/>.</param>
        /// <param name="instance">The instance to store.</param>
        /// <returns><see langword="true"/> when <paramref name="instance"/> is stored; <see langword="false"/> when
        /// another instance is already stored, which changes nothing.</returns>
        public static bool TryRegister<T>(this TypeFlag<T> self, [NotNull] T instance)
            where T : class
        {
            if (self.GetLink<T>().TryAddObject(instance) == false)
            {
                return false;
            }

            self.Enable();
            return true;
        }

        /// <summary>
        /// Removes the stored instance of <typeparamref name="T"/> when it is <paramref name="instance"/>, then
        /// disables the flag of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type flag owner.</typeparam>
        /// <param name="self">The flag of <typeparamref name="T"/>.</param>
        /// <param name="instance">The instance to remove.</param>
        /// <returns><see langword="true"/> only when the stored instance is <paramref name="instance"/>; otherwise
        /// nothing changes.</returns>
        public static bool TryUnregister<T>(this TypeFlag<T> self, T instance)
            where T : class
        {
            if (self.GetLink<T>().TryRemoveObject(instance) == false)
            {
                return false;
            }

            self.Disable();
            return true;
        }

        /// <summary>
        /// Gets the stored instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type flag owner.</typeparam>
        /// <param name="self">The flag of <typeparamref name="T"/>.</param>
        /// <param name="instance">The stored instance, or <see langword="null"/> when none is stored.</param>
        /// <returns><see langword="true"/> only when a live instance is stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetInstance<T>(this TypeFlag<T> self, [MaybeNullWhen(false)] out T instance)
            where T : class
            => self.GetLink<T>().TryGetObject(out instance);

        /// <summary>
        /// Gets the stored instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type flag owner.</typeparam>
        /// <param name="self">The flag of <typeparamref name="T"/>.</param>
        /// <returns>The stored instance.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when no live instance is stored.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetInstanceOrThrow<T>(this TypeFlag<T> self)
            where T : class
            => self.GetLink<T>().GetObjectOrThrow();

        /// <summary>
        /// Waits until an instance of <typeparamref name="T"/> is stored, then gets it.
        /// </summary>
        /// <remarks>
        /// Completes without waiting for a frame when an instance is already stored; otherwise checks once per frame.
        /// </remarks>
        /// <typeparam name="T">The type flag owner.</typeparam>
        /// <param name="self">The flag of <typeparamref name="T"/>.</param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>The stored instance.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the stored instance is no longer live once the wait completes.
        /// </exception>
        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> GetInstanceAsync<T>(this TypeFlag<T> self, CancellationToken token = default)
            where T : class
            => self.GetLink<T>().GetObjectAsync(token);

        /// <summary>
        /// Stores <paramref name="value"/> as the value of <typeparamref name="T"/>, replacing any stored value.
        /// </summary>
        /// <typeparam name="T">The type flag owner.</typeparam>
        /// <param name="self">The flag of <typeparamref name="T"/>.</param>
        /// <param name="value">The value to store.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue<T>(this TypeFlag<T> self, T value)
            where T : struct
            => self.GetLink<T>().SetValue(value);

        /// <summary>
        /// Removes the stored value of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type flag owner.</typeparam>
        /// <param name="self">The flag of <typeparamref name="T"/>.</param>
        /// <param name="value">The removed value, or <see langword="default"/> when none is stored.</param>
        /// <returns><see langword="true"/> only when a value is stored and removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemoveValue<T>(this TypeFlag<T> self, out T value)
            where T : struct
            => self.GetLink<T>().TryRemoveValue(out value);

        /// <summary>
        /// Gets the stored value of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type flag owner.</typeparam>
        /// <param name="self">The flag of <typeparamref name="T"/>.</param>
        /// <param name="value">The stored value, or <see langword="default"/> when none is stored.</param>
        /// <returns><see langword="true"/> only when a value is stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this TypeFlag<T> self, out T value)
            where T : struct
            => self.GetLink<T>().TryGetValue(out value);

        /// <summary>
        /// Gets the stored value of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type flag owner.</typeparam>
        /// <param name="self">The flag of <typeparamref name="T"/>.</param>
        /// <returns>The stored value.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when no value is stored.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueOrThrow<T>(this TypeFlag<T> self)
            where T : struct
            => self.GetLink<T>().GetValueOrThrow();

        /// <summary>
        /// Waits until a value of <typeparamref name="T"/> is stored, then gets it.
        /// </summary>
        /// <remarks>
        /// Completes without waiting for a frame when a value is already stored; otherwise checks once per frame.
        /// </remarks>
        /// <typeparam name="T">The type flag owner.</typeparam>
        /// <param name="self">The flag of <typeparamref name="T"/>.</param>
        /// <param name="token">The token that cancels the wait.</param>
        /// <returns>The stored value.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the value is removed again before the wait completes.
        /// </exception>
        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> GetValueAsync<T>(this TypeFlag<T> self, CancellationToken token = default)
            where T : struct
            => self.GetLink<T>().GetValueAsync(token);
    }
}
