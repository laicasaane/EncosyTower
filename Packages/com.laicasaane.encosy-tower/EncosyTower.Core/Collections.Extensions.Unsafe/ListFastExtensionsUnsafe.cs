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
    public static class ListFastExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> GetListUnsafe<T>(this ListFast<T>.ReadOnly list)
        {
            ThrowHelper.ThrowInvalidOperationException_ReadOnlyCollectionNotCreated(list.IsCreated);
            return list._list.List;
        }
    }
}
