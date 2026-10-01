using System.Collections.Generic;
using System.Runtime.CompilerServices;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Extensions.Unsafe
{
    public static class ListFastExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetBufferUnsafe<T>(this ListFast<T> list, out T[] buffer, out int count)
        {
            DebuggingThrowHelper.ThrowIfNotCreated(list);
            buffer = list._buffer;
            count = list._count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> GetListUnsafe<T>(this ListFast<T>.ReadOnly list)
        {
            DebuggingThrowHelper.ThrowIfNotCreated(list);
            ThrowHelper.ThrowInvalidOperationException_ReadOnlyCollectionNotCreated(list.IsCreated);
            return list._list.List;
        }
    }
}
