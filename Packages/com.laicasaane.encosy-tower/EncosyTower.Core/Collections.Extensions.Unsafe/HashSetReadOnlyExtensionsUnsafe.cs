#if !(UNITY_EDITOR || DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;

namespace EncosyTower.Collections.Extensions.Unsafe
{
    public static class HashSetReadOnlyExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSet<T> GetHashSetUnsafe<T>(this HashSetReadOnly<T> set)
        {
            ThrowHelper.ThrowInvalidOperationException_ReadOnlyCollectionNotCreated(set.IsCreated);
            return set._set.Set;
        }
    }
}
