using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Extensions
{
    public static class SharedArrayNativeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this in SharedArrayNative<T> self, T item)
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
        public static bool Contains<T>(this in SharedArrayNative<T> self, in T item)
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
              this in SharedArrayNative<T> self
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
              this in SharedArrayNative<T> self
            , int index
            , int count
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayNative);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayNative);
            ThrowHelper.ThrowIfRangeIsWithinArray(
                  self.Length - index >= count
                , ThrowHelper.CollectionType.SharedArrayNative
            );

            var result = self.AsNativeSlice().Slice(index, count).BinarySearch(item, comparer);
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparer>(
              this in SharedArrayNative<T> self
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
            this in SharedArrayNative<T> self
            , int index
            , int count
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayNative);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayNative);
            ThrowHelper.ThrowIfRangeIsWithinArray(
                  self.Length - index >= count
                , ThrowHelper.CollectionType.SharedArrayNative
            );

            var result = self.AsNativeSlice().Slice(index, count).BinarySearch(item, comparer);
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this in SharedArrayNative<T> self, T item)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return IndexOf(self, item, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this in SharedArrayNative<T> self, T item, int index)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return IndexOf(self, item, index, self.Length - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this in SharedArrayNative<T> self, T item, int index, int count)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayNative);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayNative);
            ThrowHelper.ThrowIfSectionIsWithinArray(
                  index + count <= self.Length
                , ThrowHelper.CollectionType.SharedArrayNative
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
        public static int IndexOf<T>(this in SharedArrayNative<T> self, in T item)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return IndexOf(self, in item, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this in SharedArrayNative<T> self, in T item, int index)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return IndexOf(self, in item, index, self.Length - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this in SharedArrayNative<T> self, in T item, int index, int count)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayNative);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayNative);
            ThrowHelper.ThrowIfSectionIsWithinArray(
                  index + count <= self.Length
                , ThrowHelper.CollectionType.SharedArrayNative
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
        public static int IndexOf<T, TComparer>(this in SharedArrayNative<T> self, T item, TComparer comparer)
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
              this in SharedArrayNative<T> self
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T, TComparer>(this in SharedArrayNative<T> self, TComparer comparer)
            where T : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            Sort(self, 0, self.Length, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T, TComparer>(
              this in SharedArrayNative<T> self
            , int index
            , int count
            , TComparer comparer
        )
            where T : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayNative);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayNative);
            ThrowHelper.ThrowIfOffsetLengthIsValid(self.Length - index >= count);

            if (count > 1)
            {
                self.AsNativeSlice().Slice(index, count).Sort(comparer);
            }
        }

    }
}
