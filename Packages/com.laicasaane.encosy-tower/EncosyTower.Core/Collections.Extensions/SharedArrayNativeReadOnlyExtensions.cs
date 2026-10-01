using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Extensions
{
    public static class SharedArrayNativeReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this in SharedArrayNative<T>.ReadOnly self, T item)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ReadOnlySpan<T> items;

            // SAFETY: The native view's owner remains alive and unmodified while the borrowed span is consumed.
            unsafe
            {
                items = self.AsReadOnlySpan();
            }
            var length = items.Length;
            return length > 0 && MemoryExtensions.IndexOf(items, item) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this in SharedArrayNative<T>.ReadOnly self, in T item)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ReadOnlySpan<T> items;

            // SAFETY: The native view's owner remains alive and unmodified while the borrowed span is consumed.
            unsafe
            {
                items = self.AsReadOnlySpan();
            }
            var length = items.Length;
            return length > 0 && MemoryExtensions.IndexOf(items, item) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparer>(
              this in SharedArrayNative<T>.ReadOnly self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return BinarySearch(self, 0, self.Length, item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparer>(
              this in SharedArrayNative<T>.ReadOnly self
            , int index
            , int count
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayNativeReadOnly);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayNativeReadOnly);
            ThrowHelper.ThrowIfRangeIsWithinArray(
                  self.Length - index >= count
                , ThrowHelper.CollectionType.SharedArrayNativeReadOnly
            );

            int result;

            // SAFETY: The native view's owner remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                result = MemoryExtensions.BinarySearch(self.AsReadOnlySpan().Slice(index, count), item, comparer);
            }
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparer>(
              this in SharedArrayNative<T>.ReadOnly self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return BinarySearch(self, 0, self.Length, in item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparer>(
              this in SharedArrayNative<T>.ReadOnly self
            , int index
            , int count
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayNativeReadOnly);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayNativeReadOnly);
            ThrowHelper.ThrowIfRangeIsWithinArray(
                  self.Length - index >= count
                , ThrowHelper.CollectionType.SharedArrayNativeReadOnly
            );

            int result;

            // SAFETY: The native view's owner remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                result = MemoryExtensions.BinarySearch(self.AsReadOnlySpan().Slice(index, count), item, comparer);
            }
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this in SharedArrayNative<T>.ReadOnly self, T item)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return IndexOf(self, item, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this in SharedArrayNative<T>.ReadOnly self, T item, int index)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return IndexOf(self, item, index, self.Length - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this in SharedArrayNative<T>.ReadOnly self, T item, int index, int count)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayNativeReadOnly);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayNativeReadOnly);
            ThrowHelper.ThrowIfSectionIsWithinArray(
                  index + count <= self.Length
                , ThrowHelper.CollectionType.SharedArrayNativeReadOnly
            );

            int result;

            // SAFETY: The native view's owner remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            }
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this in SharedArrayNative<T>.ReadOnly self, in T item)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return IndexOf(self, in item, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this in SharedArrayNative<T>.ReadOnly self, in T item, int index)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return IndexOf(self, in item, index, self.Length - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this in SharedArrayNative<T>.ReadOnly self, in T item, int index, int count)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayNativeReadOnly);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayNativeReadOnly);
            ThrowHelper.ThrowIfSectionIsWithinArray(
                  index + count <= self.Length
                , ThrowHelper.CollectionType.SharedArrayNativeReadOnly
            );

            int result;

            // SAFETY: The native view's owner remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            }
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TComparer>(this in SharedArrayNative<T>.ReadOnly self, T item, TComparer comparer)
            where T : unmanaged
            where TComparer : unmanaged, IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            // SAFETY: The native view's owner remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                return EncosyMemoryExtensions.IndexOf(self.AsReadOnlySpan(), item, comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TComparer>(
              this in SharedArrayNative<T>.ReadOnly self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TComparer : unmanaged, IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            // SAFETY: The native view's owner remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                return EncosyMemoryExtensions.IndexOf(self.AsReadOnlySpan(), in item, comparer);
            }
        }

    }
}
