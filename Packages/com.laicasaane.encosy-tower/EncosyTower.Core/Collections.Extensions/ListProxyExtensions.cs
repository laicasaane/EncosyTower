using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Buffers;
using EncosyTower.Debugging;

namespace EncosyTower.Collections.Extensions
{
    public static class ListProxyExtensions
    {
        public static bool Contains<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , T item
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where T : IEquatable<T>
        {
            var items = self.AsReadOnlySpan();
            var length = items.Length;

            for (var index = 0; index < length; index++)
            {
                ref readonly var item2 = ref items[index];

                if (item.Equals(item2))
                    return true;
            }

            return false;
        }

        public static bool Contains<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , in T item
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where T : IEquatable<T>
        {
            var items = self.AsReadOnlySpan();
            var length = items.Length;

            for (var index = 0; index < length; index++)
            {
                ref readonly var item2 = ref items[index];

                if (item.Equals(item2))
                    return true;
            }

            return false;
        }

        public static bool Contains<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , T item
            , TComparer comparer
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where TComparer : IEqualityComparer<T>
        {
            var items = self.AsReadOnlySpan();
            var length = items.Length;

            for (var index = 0; index < length; index++)
            {
                ref readonly var item2 = ref items[index];

                if (comparer.Equals(item, item2))
                    return true;
            }

            return false;
        }

        public static bool Contains<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , in T item
            , TComparer comparer
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where TComparer : IEqualityComparer<T>
        {
            var items = self.AsReadOnlySpan();
            var length = items.Length;

            for (var index = 0; index < length; index++)
            {
                ref readonly var item2 = ref items[index];

                if (comparer.Equals(item, item2))
                    return true;
            }

            return false;
        }

        public static bool Remove<TProvider, TBuffer, T>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , T item
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where T : IEquatable<T>
        {
            self._version++;

            var index = IndexOf(ref self, item);

            if ((uint)index >= (uint)self._count)
                return false;

            if (index < --self._count)
            {
                self.CopyBuffer(index + 1, index, self._count - index);
            }

            var shouldClear = false;
            ListProxy<TProvider, TBuffer, T>.ShouldClear(ref shouldClear);

            if (shouldClear)
            {
                self.ClearBuffer(self._count, 1);
            }

            return true;
        }

        public static bool Remove<TProvider, TBuffer, T>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , in T item
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where T : IEquatable<T>
        {
            self._version++;

            var index = IndexOf(ref self, in item);

            if ((uint)index >= (uint)self._count)
                return false;

            if (index < --self._count)
            {
                self.CopyBuffer(index + 1, index, self._count - index);
            }

            var shouldClear = false;
            ListProxy<TProvider, TBuffer, T>.ShouldClear(ref shouldClear);

            if (shouldClear)
            {
                self.ClearBuffer(self._count, 1);
            }

            return true;
        }

        public static bool Remove<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , T item
            , TComparer comparer
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where TComparer : IEqualityComparer<T>
        {
            self._version++;

            var index = IndexOf(ref self, item, comparer);

            if ((uint)index >= (uint)self._count)
                return false;

            if (index < --self._count)
            {
                self.CopyBuffer(index + 1, index, self._count - index);
            }

            var shouldClear = false;
            ListProxy<TProvider, TBuffer, T>.ShouldClear(ref shouldClear);

            if (shouldClear)
            {
                self.ClearBuffer(self._count, 1);
            }

            return true;
        }

        public static bool Remove<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , in T item
            , TComparer comparer
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where TComparer : IEqualityComparer<T>
        {
            self._version++;

            var index = IndexOf(ref self, in item, comparer);

            if ((uint)index >= (uint)self._count)
                return false;

            if (index < --self._count)
            {
                self.CopyBuffer(index + 1, index, self._count - index);
            }

            var shouldClear = false;
            ListProxy<TProvider, TBuffer, T>.ShouldClear(ref shouldClear);

            if (shouldClear)
            {
                self.ClearBuffer(self._count, 1);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , T item
            , TComparer comparer
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where TComparer : IComparer<T>
        {
            return BinarySearch(ref self, 0, self._count, item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , int index
            , int count
            , T item
            , TComparer comparer
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where TComparer : IComparer<T>
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(
                  self._count - index >= count
                , "index and count do not denote a valid range in the StatelessList<TProvider, TBuffer, T"
            );

            return MemoryExtensions.BinarySearch(self.AsReadOnlySpan(), item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , in T item
            , TComparer comparer
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where TComparer : IComparer<T>
        {
            return BinarySearch(ref self, 0, self._count, in item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , int index
            , int count
            , in T item
            , TComparer comparer
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where TComparer : IComparer<T>
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(
                  self._count - index >= count
                , "index and count do not denote a valid range in the StatelessList<TProvider, TBuffer, T>"
            );

            return MemoryExtensions.BinarySearch(self.AsReadOnlySpan(), item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<TProvider, TBuffer, T>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , T item
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where T : IEquatable<T>
        {
            return IndexOf(ref self, item, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<TProvider, TBuffer, T>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , T item
            , int index
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where T : IEquatable<T>
        {
            return IndexOf(ref self, item, index, self._count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<TProvider, TBuffer, T>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , T item
            , int index
            , int count
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where T : IEquatable<T>
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(
                  index + count <= self._count
                , "index and count do not specify a valid section in the StatelessList<TProvider, TBuffer, T>"
            );

            var result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<TProvider, TBuffer, T>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , in T item
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where T : IEquatable<T>
        {
            return IndexOf(ref self, in item, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<TProvider, TBuffer, T>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , in T item
            , int index
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where T : IEquatable<T>
        {
            return IndexOf(ref self, in item, index, self._count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<TProvider, TBuffer, T>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , in T item
            , int index
            , int count
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where T : IEquatable<T>
        {
            Checks.IsTrue(index >= 0, "index is less than 0");
            Checks.IsTrue(count >= 0, "count is less than 0");
            Checks.IsTrue(
                  index + count <= self._count
                , "index and count do not specify a valid section in the StatelessList<TProvider, TBuffer, T>"
            );

            var result = MemoryExtensions.IndexOf(self.AsReadOnlySpan().Slice(index, count), item);
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , T item
            , TComparer comparer
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where TComparer : IEqualityComparer<T>
        {
            return EncosyMemoryExtensions.IndexOf(self.AsReadOnlySpan(), item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , in T item
            , TComparer comparer
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where TComparer : IEqualityComparer<T>
        {
            return EncosyMemoryExtensions.IndexOf(self.AsReadOnlySpan(), in item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , TComparer comparer
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where TComparer : IComparer<T>
        {
            Sort(ref self, 0, self._count, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<TProvider, TBuffer, T, TComparer>(
              this ref ListProxy<TProvider, TBuffer, T> self
            , int index
            , int count
            , TComparer comparer
        )
            where TProvider : IBufferProvider<TBuffer, T>
            where TBuffer : IBuffer<T>
            where TComparer : IComparer<T>
        {
            Checks.IsTrue(index >= 0, "'index' must be non-negative number");
            Checks.IsTrue(count >= 0, "'count' must be non-negative number");
            Checks.IsTrue(self._count - index >= count, "Invalid offset length");

            self._version++;
            ArraySortHelper<T, TComparer>.Sort(self.AsSpan().Slice(index, count), comparer);
        }
    }
}
