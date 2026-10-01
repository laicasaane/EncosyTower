using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    partial class SharedArray<T, TNative>
    {
        /// <safety>The returned view must not outlive this array.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnly AsReadOnly()
        {
            // SAFETY: This array remains the designated owner of the returned read-only view.
            unsafe
            {
                return new(this);
            }
        }

        public readonly partial struct ReadOnly : IReadOnlyList<T>, IToArray<T>, IReadOnlyIndexer<T>
            , IAsReadOnlySpan<T>, ICopyToSpan<T>, ITryCopyToSpan<T>, IHasLength, IIsCreated
        {
            private static readonly SharedArray<T, TNative> s_emptyOwner = new();
            private static readonly ReadOnly s_empty = new(s_emptyOwner);

            internal readonly SharedArray<T, TNative> _array;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnly(SharedArray<T, TNative> array)
            {
                DebuggingThrowHelper.ThrowIfNull(array);
                array.CheckRead();
                _array = array;
            }

            public static ReadOnly Empty
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => s_empty;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _array is not null && _array._buffer.IsCreated;
            }

            public int Length
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _array.Length;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Length;
            }

            public bool IsReadOnly => true;

            public T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _array[index];
            }

            /// <safety>The returned alias must not outlive the source array.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnly(SharedArray<T, TNative> array)
            {
                // SAFETY: A non-null array remains the designated owner of the returned alias.
                unsafe
                {
                    return array is not null ? array.AsReadOnly() : Empty;
                }
            }

            /// <safety>The returned span must not outlive the source array.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnlySpan<T>(in ReadOnly array)
            {
                DebuggingThrowHelper.ThrowIfNotCreated(array);

                // SAFETY: The caller accepts the borrowed span lifetime.
                unsafe
                {
                    return array.AsReadOnlySpan();
                }
            }

            /// <safety>The source array must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe SharedArray<T, TNative>.Enumerator GetEnumerator()
            {
                DebuggingThrowHelper.ThrowIfNotCreated(this);

                // SAFETY: The enumerator borrows the live source array.
                unsafe
                {
                    return _array.GetEnumerator();
                }
            }

            /// <safety>The returned span must not outlive the source array.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe ReadOnlySpan<T> AsReadOnlySpan()
            {
                DebuggingThrowHelper.ThrowIfNotCreated(this);

                // SAFETY: The returned span borrows the live source array.
                unsafe
                {
                    return _array.AsReadOnlySpan();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(T[] destination, int destinationIndex)
                => CopyTo(destination.AsSpan().Slice(destinationIndex, Length));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<T> destination)
                => CopyTo(0, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<T> destination, int length)
                => CopyTo(0, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, Span<T> destination)
                => CopyTo(sourceStartIndex, destination, destination.Length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, Span<T> destination, int length)
            {
                // SAFETY: The copy consumes the borrowed span before this method returns.
                unsafe
                {
                    new CopyToSpan<T>(AsReadOnlySpan()).CopyTo(sourceStartIndex, destination, length);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(Span<T> destination)
                => TryCopyTo(0, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(Span<T> destination, int length)
                => TryCopyTo(0, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, Span<T> destination)
                => TryCopyTo(sourceStartIndex, destination, destination.Length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
            {
                // SAFETY: The copy consumes the borrowed span before this method returns.
                unsafe
                {
                    return new CopyToSpan<T>(AsReadOnlySpan()).TryCopyTo(sourceStartIndex, destination, length);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T[] ToArray()
            {
                // SAFETY: ToArray copies the borrowed span before this method returns.
                unsafe
                {
                    return AsReadOnlySpan().ToArray();
                }
            }

            /// <safety>The source array must remain alive and unmodified during enumeration.</safety>
            unsafe IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows the live source array.
                unsafe
                {
                    return GetEnumerator();
                }
            }

            /// <safety>The source array must remain alive and unmodified during enumeration.</safety>
            unsafe IEnumerator IEnumerable.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows the live source array.
                unsafe
                {
                    return GetEnumerator();
                }
            }
        }
    }
}
