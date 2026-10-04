#pragma warning disable IDE0060 // Remove unused parameter

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Tasks;

namespace EncosyTower.TypeFlags
{
    /// <summary>
    /// Reads the instance or value of a type flag owner through its read-only flag view.
    /// </summary>
    public static class TypeFlagReadOnlyExtensions
    {
        /// <inheritdoc cref="TypeFlagExtensions.TryGetInstance{T}"/>
        /// <param name="self">The read-only flag of <typeparamref name="T"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetInstance<T>(this TypeFlag<T>.ReadOnly self, [MaybeNullWhen(false)] out T instance)
            where T : class
            => self.GetLink<T>().TryGetObject(out instance);

        /// <inheritdoc cref="TypeFlagExtensions.GetInstanceOrThrow{T}"/>
        /// <param name="self">The read-only flag of <typeparamref name="T"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetInstanceOrThrow<T>(this TypeFlag<T>.ReadOnly self)
            where T : class
            => self.GetLink<T>().GetObjectOrThrow();

        /// <inheritdoc cref="TypeFlagExtensions.GetInstanceAsync{T}"/>
        /// <param name="self">The read-only flag of <typeparamref name="T"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> GetInstanceAsync<T>(
              this TypeFlag<T>.ReadOnly self
            , CancellationToken token = default
        )
            where T : class
            => self.GetLink<T>().GetObjectAsync(token);

        /// <inheritdoc cref="TypeFlagExtensions.TryGetValue{T}"/>
        /// <param name="self">The read-only flag of <typeparamref name="T"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this TypeFlag<T>.ReadOnly self, out T value)
            where T : struct
            => self.GetLink<T>().TryGetValue(out value);

        /// <inheritdoc cref="TypeFlagExtensions.GetValueOrThrow{T}"/>
        /// <param name="self">The read-only flag of <typeparamref name="T"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueOrThrow<T>(this TypeFlag<T>.ReadOnly self)
            where T : struct
            => self.GetLink<T>().GetValueOrThrow();

        /// <inheritdoc cref="TypeFlagExtensions.GetValueAsync{T}"/>
        /// <param name="self">The read-only flag of <typeparamref name="T"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> GetValueAsync<T>(this TypeFlag<T>.ReadOnly self, CancellationToken token = default)
            where T : struct
            => self.GetLink<T>().GetValueAsync(token);
    }
}
