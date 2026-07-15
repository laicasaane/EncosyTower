#if !(UNITY_EDITOR || DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Buffers
{
    public static class BufferUnsafeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShiftLeft<T>(this ref BufferUnsafe<T> self, int index, int count)
            where T : unmanaged
        {
            ThrowHelper.ThrowIfBufferShiftIndexIsOutOfRange((uint)index < (uint)self.Capacity);
            ThrowHelper.ThrowIfBufferShiftCountIsOutOfRange((uint)count < (uint)self.Capacity);

            if (count == index)
                return;

            ThrowHelper.ThrowIfBufferShiftCountIsBelowIndex(count > index);

            MemoryCopyUnsafe(ref self, index + 1, index, count - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShiftRight<T>(this ref BufferUnsafe<T> self, int index, int count)
            where T : unmanaged
        {
            ThrowHelper.ThrowIfBufferShiftIndexIsOutOfRange((uint)index < (uint)self.Capacity);
            ThrowHelper.ThrowIfBufferShiftCountIsOutOfRange((uint)count < (uint)self.Capacity);

            if (count == index)
                return;

            ThrowHelper.ThrowIfBufferShiftCountIsBelowIndex(count > index);

            MemoryCopyUnsafe(ref self, index, index + 1, count - index);
        }

        private static void MemoryCopyUnsafe<T>(
              ref BufferUnsafe<T> self
            , int sourceIndex
            , int destinationIndex
            , int length
        )
            where T : unmanaged
        {
            // SAFETY: Callers validate source/destination ranges before copying within the live buffer.
            unsafe
            {
                var sizeOf = UnsafeUtility.SizeOf<T>();
                var ptr = (IntPtr)self.GetUnsafePtr();

                Buffer.MemoryCopy(
                      (void*)(ptr + sourceIndex * sizeOf)
                    , (void*)(ptr + destinationIndex * sizeOf)
                    , (long)(self.Capacity - destinationIndex) * sizeOf
                    , (long)length * sizeOf
                );
            }
        }
    }
}
