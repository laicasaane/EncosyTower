#pragma warning disable IDE0060 // Remove unused parameter

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Ids;
using EncosyTower.Tasks;
using EncosyTower.Vaults;

namespace EncosyTower.Types
{
    public static class TypeFlagLinkExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAddObject<TOwner, TObject>(this TypeFlagLink<TOwner, TObject> self, [NotNull] TObject obj)
            where TObject : class
            => GlobalObjectVault.TryAdd(GetKey<TOwner, TObject>(), obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemoveObject<TOwner, TObject>(this TypeFlagLink<TOwner, TObject> self, TObject expected)
            where TObject : class
            => GlobalObjectVault.TryRemove(GetKey<TOwner, TObject>(), expected);

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

        public static async UnityTask<TObject> GetObjectAsync<TOwner, TObject>(
              this TypeFlagLink<TOwner, TObject> self
            , CancellationToken token = default
        )
            where TObject : class
        {
            await GlobalObjectVault.WaitUntilContains<TObject>(GetKey<TOwner, TObject>(), token);
            return self.GetObjectOrThrow();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue<TOwner, TValue>(this TypeFlagLink<TOwner, TValue> self, TValue value)
            where TValue : struct
            => GlobalValueVault<TValue>.TrySet(GetKey<TOwner, TValue>(), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemoveValue<TOwner, TValue>(this TypeFlagLink<TOwner, TValue> self, out TValue value)
            where TValue : struct
            => GlobalValueVault<TValue>.TryRemove(GetKey<TOwner, TValue>(), out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<TOwner, TValue>(this TypeFlagLink<TOwner, TValue> self, out TValue value)
            where TValue : struct
            => GlobalValueVault<TValue>.TryGet(GetKey<TOwner, TValue>(), out value);

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
