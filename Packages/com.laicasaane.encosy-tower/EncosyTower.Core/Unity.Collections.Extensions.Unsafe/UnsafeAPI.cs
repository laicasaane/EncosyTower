using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Collections.Unsafe
{
    public static class UnsafeAPI
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreTypesEqualSize<T, U>()
            where T : unmanaged
            where U : unmanaged
            => UnsafeUtility.SizeOf<T>() == UnsafeUtility.SizeOf<U>();

        /// <safety>The pointer must be null or identify an address that may be inspected as an integer value.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool IsPointerAligned<T>(void* pointer)
            where T : unmanaged
        {
            // SAFETY: The caller provides an opaque address; this block compares it without dereferencing it.
            unsafe
            {
                return pointer == null || (ulong)pointer % (uint)UnsafeUtility.AlignOf<T>() == 0;
            }
        }
    }
}
