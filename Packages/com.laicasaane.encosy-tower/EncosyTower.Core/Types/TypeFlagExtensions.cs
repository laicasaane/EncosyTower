#pragma warning disable IDE0060 // Remove unused parameter

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Tasks;

namespace EncosyTower.Types
{
    public static class TypeFlagExtensions
    {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetInstance<T>(this TypeFlag<T> self, [MaybeNullWhen(false)] out T instance)
            where T : class
            => self.GetLink<T>().TryGetObject(out instance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetInstanceOrThrow<T>(this TypeFlag<T> self)
            where T : class
            => self.GetLink<T>().GetObjectOrThrow();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> GetInstanceAsync<T>(this TypeFlag<T> self, CancellationToken token = default)
            where T : class
            => self.GetLink<T>().GetObjectAsync(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue<T>(this TypeFlag<T> self, T value)
            where T : struct
            => self.GetLink<T>().SetValue(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemoveValue<T>(this TypeFlag<T> self, out T value)
            where T : struct
            => self.GetLink<T>().TryRemoveValue(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this TypeFlag<T> self, out T value)
            where T : struct
            => self.GetLink<T>().TryGetValue(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueOrThrow<T>(this TypeFlag<T> self)
            where T : struct
            => self.GetLink<T>().GetValueOrThrow();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<T> GetValueAsync<T>(this TypeFlag<T> self, CancellationToken token = default)
            where T : struct
            => self.GetLink<T>().GetValueAsync(token);
    }
}
