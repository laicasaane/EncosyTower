using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Extensions
{
    public static class SharedArrayReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T, TNative>(this SharedArray<T, TNative>.ReadOnly self, T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ReadOnlySpan<T> items;

            // SAFETY: The underlying owner remains alive and unmodified while the borrowed span is consumed.
            unsafe
            {
                items = self.AsReadOnlySpan();
            }
            var length = items.Length;
            return length > 0 && MemoryExtensions.IndexOf(items, item) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T, TNative>(this SharedArray<T, TNative>.ReadOnly self, in T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ReadOnlySpan<T> items;

            // SAFETY: The underlying owner remains alive and unmodified while the borrowed span is consumed.
            unsafe
            {
                items = self.AsReadOnlySpan();
            }
            var length = items.Length;
            return length > 0 && MemoryExtensions.IndexOf(items, item) >= 0;
        }

        public static bool Contains<T, TNative, TComparer>(
              this SharedArray<T, TNative>.ReadOnly self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ReadOnlySpan<T> items;

            // SAFETY: The underlying owner remains alive and unmodified while the borrowed span is consumed.
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
              this SharedArray<T, TNative>.ReadOnly self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ReadOnlySpan<T> items;

            // SAFETY: The underlying owner remains alive and unmodified while the borrowed span is consumed.
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
              this SharedArray<T, TNative>.ReadOnly self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return BinarySearch(self, 0, self.Length, item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              this SharedArray<T, TNative>.ReadOnly self
            , int index
            , int count
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayReadOnly);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayReadOnly);
            ThrowHelper.ThrowIfRangeIsWithinArray(
                  self.Length - index >= count
                , ThrowHelper.CollectionType.SharedArrayReadOnly
            );

            int result;

            // SAFETY: The underlying owner remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                result = MemoryExtensions.BinarySearch(self.AsReadOnlySpan().Slice(index, count), item, comparer);
            }
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              this SharedArray<T, TNative>.ReadOnly self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return BinarySearch(self, 0, self.Length, in item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              this SharedArray<T, TNative>.ReadOnly self
            , int index
            , int count
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayReadOnly);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayReadOnly);
            ThrowHelper.ThrowIfRangeIsWithinArray(
                  self.Length - index >= count
                , ThrowHelper.CollectionType.SharedArrayReadOnly
            );

            int result;

            // SAFETY: The underlying owner remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                result = MemoryExtensions.BinarySearch(self.AsReadOnlySpan().Slice(index, count), item, comparer);
            }
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(this SharedArray<T, TNative>.ReadOnly self, T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return IndexOf(self, item, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(this SharedArray<T, TNative>.ReadOnly self, T item, int index)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return IndexOf(self, item, index, self.Length - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(this SharedArray<T, TNative>.ReadOnly self, T item, int index, int count)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayReadOnly);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayReadOnly);
            ThrowHelper.ThrowIfSectionIsWithinArray(
                  index + count <= self.Length
                , ThrowHelper.CollectionType.SharedArrayReadOnly
            );

            int result;

            // SAFETY: The underlying owner remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            }
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(this SharedArray<T, TNative>.ReadOnly self, in T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return IndexOf(self, in item, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(this SharedArray<T, TNative>.ReadOnly self, in T item, int index)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            return IndexOf(self, in item, index, self.Length - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(
              this SharedArray<T, TNative>.ReadOnly self
            , in T item
            , int index
            , int count
        )
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            ThrowHelper.ThrowIfIndexIsNonNegative(index >= 0, ThrowHelper.CollectionType.SharedArrayReadOnly);
            ThrowHelper.ThrowIfCountIsNonNegative(count >= 0, ThrowHelper.CollectionType.SharedArrayReadOnly);
            ThrowHelper.ThrowIfSectionIsWithinArray(
                  index + count <= self.Length
                , ThrowHelper.CollectionType.SharedArrayReadOnly
            );

            int result;

            // SAFETY: The underlying owner remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            }
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative, TComparer>(
              this SharedArray<T, TNative>.ReadOnly self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            // SAFETY: The underlying owner remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                return EncosyMemoryExtensions.IndexOf(self.AsReadOnlySpan(), item, comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative, TComparer>(
              this SharedArray<T, TNative>.ReadOnly self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            // SAFETY: The underlying owner remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                return EncosyMemoryExtensions.IndexOf(self.AsReadOnlySpan(), in item, comparer);
            }
        }

    }
}
