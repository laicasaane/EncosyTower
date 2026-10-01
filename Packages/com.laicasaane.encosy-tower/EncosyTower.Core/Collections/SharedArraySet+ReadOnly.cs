using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    partial class SharedArraySet<T>
    {
        /// <safety>The returned alias must not outlive this set.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnly AsReadOnly()
        {
            // SAFETY: This set remains the designated owner of the returned read-only alias.
            unsafe
            {
                return new(this);
            }
        }

        [DebuggerTypeProxy(typeof(SharedArraySetDebugProxy<>))]
        public readonly partial struct ReadOnly : IIsCreated, IReadOnlyCollection<T>, IHasCount
            , ICopyToSpan<T>, ITryCopyToSpan<T>
        {
            private static readonly SharedArraySet<T> s_emptyOwner = new();
            private static readonly ReadOnly s_empty = new(s_emptyOwner);

            internal readonly SharedArraySet<T> _set;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly([NotNull] SharedArraySet<T> set)
            {
                DebuggingThrowHelper.ThrowIfNull(set);
                _set = set;
            }

            public static ReadOnly Empty
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => s_empty;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _set != null;
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _set.Capacity;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _set.Count;
            }

            public ReadOnlyMemory<T> Items
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _set.Items;
            }

            /// <safety>The returned alias must not outlive the source set.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnly(SharedArraySet<T> set)
            {
                // SAFETY: A non-null set remains the designated owner of the returned alias.
                unsafe
                {
                    return set is not null ? set.AsReadOnly() : Empty;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<T> destination)
                => _set.CopyTo(destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<T> destination, int length)
                => _set.CopyTo(destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, Span<T> destination)
                => _set.CopyTo(sourceStartIndex, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, Span<T> destination, int length)
                => _set.CopyTo(sourceStartIndex, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(Span<T> destination)
                => _set.TryCopyTo(destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(Span<T> destination, int length)
                => _set.TryCopyTo(destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, Span<T> destination)
                => _set.TryCopyTo(sourceStartIndex, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
                => _set.TryCopyTo(sourceStartIndex, destination, length);

            /// <safety>The source set must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe SharedArraySetEnumerator<T> GetEnumerator()
            {
                // SAFETY: The enumerator borrows the live source set.
                unsafe
                {
                    return _set.GetEnumerator();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(T value)
                => _set.Contains(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(in T value)
                => _set.Contains(in value);

            /// <safety>The set must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows the live source set.
                unsafe
                {
                    return GetEnumerator();
                }
            }

            /// <safety>The set must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe IEnumerator IEnumerable.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows the live source set.
                unsafe
                {
                    return GetEnumerator();
                }
            }
        }
    }
}
