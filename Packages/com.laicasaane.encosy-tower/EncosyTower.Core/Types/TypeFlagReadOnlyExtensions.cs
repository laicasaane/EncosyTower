#pragma warning disable IDE0060 // Remove unused parameter

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Tasks;

namespace EncosyTower.Types
{
    public static class TypeFlagReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetInstance<T>(this TypeFlag<T>.ReadOnly self, [MaybeNullWhen(false)] out T instance)
            where T : class
            => self.GetLink<T>().TryGetObject(out instance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetInstanceOrThrow<T>(this TypeFlag<T>.ReadOnly self)
            where T : class
            => self.GetLink<T>().GetObjectOrThrow();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> GetInstanceAsync<T>(
              this TypeFlag<T>.ReadOnly self
            , CancellationToken token = default
        )
            where T : class
            => self.GetLink<T>().GetObjectAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this TypeFlag<T>.ReadOnly self, out T value)
            where T : struct
            => self.GetLink<T>().TryGetValue(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueOrThrow<T>(this TypeFlag<T>.ReadOnly self)
            where T : struct
            => self.GetLink<T>().GetValueOrThrow();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> GetValueAsync<T>(this TypeFlag<T>.ReadOnly self, CancellationToken token = default)
            where T : struct
            => self.GetLink<T>().GetValueAsync(token);
    }
}
