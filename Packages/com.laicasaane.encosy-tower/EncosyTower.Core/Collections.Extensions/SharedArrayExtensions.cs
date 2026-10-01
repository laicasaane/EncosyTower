using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Extensions
{
    public static class SharedArrayExtensions
    {
        public static bool Contains<T, TNative, TComparer>(
              [NotNull] this SharedArray<T, TNative> self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            ReadOnlySpan<T> items;

            // SAFETY: self remains alive and unmodified while the borrowed span is consumed.
            unsafe
            {
                items = self.AsReadOnlySpan();
            }
            var length = items.Length;

            for (var index = 0; index < length; index++)
            {
                ref readonly var item2 = ref items[index];

                if (comparer.Equals(item, item2))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Contains<T, TNative, TComparer>(
              [NotNull] this SharedArray<T, TNative> self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            ReadOnlySpan<T> items;

            // SAFETY: self remains alive and unmodified while the borrowed span is consumed.
            unsafe
            {
                items = self.AsReadOnlySpan();
            }
            var length = items.Length;

            for (var index = 0; index < length; index++)
            {
                ref readonly var item2 = ref items[index];

                if (comparer.Equals(item, item2))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              [NotNull] this SharedArray<T, TNative> self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return BinarySearch(self, 0, self.Length, item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              [NotNull] this SharedArray<T, TNative> self
            , int index
            , int count
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArray);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArray);
            ThrowHelper.ThrowIfRangeIsWithinArray(
                  self.Length - index >= count
                , ThrowHelper.CollectionType.SharedArray
            );

            int result;

            // SAFETY: self remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                result = MemoryExtensions.BinarySearch(self.AsReadOnlySpan().Slice(index, count), item, comparer);
            }
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              [NotNull] this SharedArray<T, TNative> self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return BinarySearch(self, 0, self.Length, in item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              [NotNull] this SharedArray<T, TNative> self
            , int index
            , int count
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArray);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArray);
            ThrowHelper.ThrowIfRangeIsWithinArray(
                  self.Length - index >= count
                , ThrowHelper.CollectionType.SharedArray
            );

            int result;

            // SAFETY: self remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                result = MemoryExtensions.BinarySearch(self.AsReadOnlySpan().Slice(index, count), item, comparer);
            }
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this SharedArray<T, TNative> self, T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return IndexOf(self, item, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this SharedArray<T, TNative> self, T item, int index)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return IndexOf(self, item, index, self.Length - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(
              [NotNull] this SharedArray<T, TNative> self
            , T item
            , int index
            , int count
        )
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArray);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArray);
            ThrowHelper.ThrowIfSectionIsWithinArray(
                  index + count <= self.Length
                , ThrowHelper.CollectionType.SharedArray
            );

            int result;

            // SAFETY: self remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            }
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative, TComparer>(
              [NotNull] this SharedArray<T, TNative> self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            // SAFETY: self remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                return EncosyMemoryExtensions.IndexOf(self.AsReadOnlySpan(), item, comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative, TComparer>(
              [NotNull] this SharedArray<T, TNative> self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            // SAFETY: self remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                return EncosyMemoryExtensions.IndexOf(self.AsReadOnlySpan(), in item, comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T, TNative, TComparer>(
              [NotNull] this SharedArray<T, TNative> self
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            Sort(self, 0, self.Length, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T, TNative, TComparer>(
              [NotNull] this SharedArray<T, TNative> self
            , int index
            , int count
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArray);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArray);
            ThrowHelper.ThrowIfOffsetLengthIsValid(self.Length - index >= count);

            self._version++;
            // SAFETY: self remains alive and is mutated only through the borrowed span during this call.
            unsafe
            {
                ArraySortHelper<T, TComparer>.Sort(self.AsSpan().Slice(index, count), comparer);
            }
        }

    }
}
