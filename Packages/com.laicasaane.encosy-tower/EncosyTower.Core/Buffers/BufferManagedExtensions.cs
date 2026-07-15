#if !(UNITY_EDITOR || DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;

namespace EncosyTower.Buffers
{
    public static class BufferManagedExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShiftLeft<T>(this BufferManaged<T> self, int index, int count)
        {
            ThrowHelper.ThrowIfBufferShiftIndexIsOutOfRange((uint)index < (uint)self.Capacity);
            ThrowHelper.ThrowIfBufferShiftCountIsOutOfRange((uint)count < (uint)self.Capacity);

            if (count == index) return;

            ThrowHelper.ThrowIfBufferShiftCountIsBelowIndex(count > index);

            var managedArray = self.AsManagedArray();
            Array.Copy(managedArray, index + 1, managedArray, index, count - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShiftRight<T>(this BufferManaged<T> self, int index, int count)
        {
            ThrowHelper.ThrowIfBufferShiftIndexIsOutOfRange((uint)index < (uint)self.Capacity);
            ThrowHelper.ThrowIfBufferShiftCountIsOutOfRange((uint)count < (uint)self.Capacity);

            if (count == index) return;

            ThrowHelper.ThrowIfBufferShiftCountIsBelowIndex(count > index);

            var managedArray = self.AsManagedArray();
            Array.Copy(managedArray, index, managedArray, index + 1, count - index);
        }
    }
}
