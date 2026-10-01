using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Extensions
{
    public static class SharedListExtensions
    {
        public static bool Contains<T, TNative, TComparer>(
              [NotNull] this SharedList<T, TNative> self
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
              [NotNull] this SharedList<T, TNative> self
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

        public static bool Remove<T, TNative, TComparer>(
              [NotNull] this SharedList<T, TNative> self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            self.VersionRW++;

            var index = IndexOf(self, item, comparer);

            if ((uint)index >= (uint)self.CountRO)
            {
                return false;
            }

            if (index < --self.CountRW)
            {
                var buffer = self._buffer.AsManagedArray();
                Array.Copy(buffer, index + 1, buffer, index, self.CountRO - index);
            }

            return true;
        }

        public static bool Remove<T, TNative, TComparer>(
              [NotNull] this SharedList<T, TNative> self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            self.VersionRW++;

            var index = IndexOf(self, in item, comparer);

            if ((uint)index >= (uint)self.CountRO)
            {
                return false;
            }

            if (index < --self.CountRW)
            {
                var buffer = self._buffer.AsManagedArray();
                Array.Copy(buffer, index + 1, buffer, index, self.CountRO - index);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              [NotNull] this SharedList<T, TNative> self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return BinarySearch(self, 0, self.CountRO, item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              [NotNull] this SharedList<T, TNative> self
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
            ThrowIfIndexIsNonNegative(index >= 0);
            ThrowIfCountIsNonNegative(count >= 0);
            ThrowIfRangeIsWithinList(self.CountRO - index >= count);

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
              [NotNull] this SharedList<T, TNative> self
            , in T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return BinarySearch(self, 0, self.CountRO, in item, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TNative, TComparer>(
              [NotNull] this SharedList<T, TNative> self
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
            ThrowIfIndexIsNonNegative(index >= 0);
            ThrowIfCountIsNonNegative(count >= 0);
            ThrowIfRangeIsWithinList(self.CountRO - index >= count);

            int result;

            // SAFETY: self remains alive and unmodified while the borrowed span is searched.
            unsafe
            {
                result = MemoryExtensions.BinarySearch(self.AsReadOnlySpan().Slice(index, count), item, comparer);
            }
            return result < 0 ? result : result + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this SharedList<T, TNative> self, T item)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return IndexOf(self, item, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>([NotNull] this SharedList<T, TNative> self, T item, int index)
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return IndexOf(self, item, index, self.CountRO - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T, TNative>(
              [NotNull] this SharedList<T, TNative> self
            , T item
            , int index
            , int count
        )
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            ThrowIfIndexIsNonNegative(index >= 0);
            ThrowIfCountIsNonNegative(count >= 0);
            ThrowIfSectionIsWithinList(index + count <= self.CountRO);

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
              [NotNull] this SharedList<T, TNative> self
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
              [NotNull] this SharedList<T, TNative> self
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
              [NotNull] this SharedList<T, TNative> self
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            Sort(self, 0, self.CountRO, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T, TNative, TComparer>(
              [NotNull] this SharedList<T, TNative> self
            , int index
            , int count
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
            where TComparer : IComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            ThrowIfIndexIsNonNegative(index >= 0);
            ThrowIfCountIsNonNegative(count >= 0);
            ThrowIfOffsetLengthIsValid(self.CountRO - index >= count);

            self.VersionRW++;
            // SAFETY: self remains alive and is mutated only through the borrowed span during this call.
            unsafe
            {
                ArraySortHelper<T, TComparer>.Sort(self.AsSpan().Slice(index, count), comparer);
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        private static void ThrowIfIndexIsNonNegative([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Index must be non-negative.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        private static void ThrowIfCountIsNonNegative([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Count must be non-negative.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        private static void ThrowIfRangeIsWithinList([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Index and count do not denote a valid range in SharedList<T, TNative>.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        private static void ThrowIfSectionIsWithinList([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Index and count do not specify a valid section in SharedList<T, TNative>.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        private static void ThrowIfOffsetLengthIsValid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Offset and length do not specify a valid range.");
        }
    }
}
