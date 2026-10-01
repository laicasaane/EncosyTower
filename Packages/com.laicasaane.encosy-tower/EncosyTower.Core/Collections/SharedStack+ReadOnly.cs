using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    public partial class SharedStack<T, TNative>
        where T : unmanaged
        where TNative : unmanaged
    {
        public readonly struct ReadOnly : IReadOnlyCollection<T>, IHasCapacity, IHasCount, IIsCreated, ITryCopyToSpan<T>
        {
            internal readonly SharedStack<T, TNative> _stack;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ReadOnly(SharedStack<T, TNative> stack)
            {
                DebuggingThrowHelper.ThrowIfNull(stack);
                _stack = stack;
            }

            public readonly bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _stack != null && _stack.IsCreated;
            }

            public readonly int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _stack?.Count ?? 0;
            }

            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _stack?.Capacity ?? 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly T Peek()
                => _stack.Peek();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryPeek(out T value)
                => _stack.TryPeek(out value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly T[] ToArray()
                => _stack.ToArray();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(Span<T> destination)
                => _stack.CopyTo(destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(Span<T> destination, int length)
                => _stack.CopyTo(destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(int sourceStartIndex, Span<T> destination)
                => _stack.CopyTo(sourceStartIndex, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void CopyTo(int sourceStartIndex, Span<T> destination, int length)
                => _stack.CopyTo(sourceStartIndex, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(Span<T> destination)
                => _stack.TryCopyTo(destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(Span<T> destination, int length)
                => _stack.TryCopyTo(destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination)
                => _stack.TryCopyTo(sourceStartIndex, destination);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly bool TryCopyTo(int sourceStartIndex, Span<T> destination, int length)
                => _stack.TryCopyTo(sourceStartIndex, destination, length);

            /// <safety>The stack must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly unsafe Enumerator GetEnumerator()
            {
                // SAFETY: The enumerator borrows this stack view.
                unsafe
                {
                    return new(this);
                }
            }

            /// <safety>The stack must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows this stack view.
                unsafe
                {
                    return GetEnumerator();
                }
            }

            /// <safety>The stack must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe IEnumerator IEnumerable.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows this stack view.
                unsafe
                {
                    return GetEnumerator();
                }
            }

            /// <safety>The returned native alias must not outlive the stack or survive resize.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly unsafe SharedStackNative<TNative>.ReadOnly AsNative()
            {
                // SAFETY: The managed stack owns both aliases for the complete borrowed lifetime.
                unsafe
                {
                    return _stack.AsNative().AsReadOnly();
                }
            }

            /// <safety>The returned alias must not outlive the source stack.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnly(SharedStack<T, TNative> stack)
            {
                DebuggingThrowHelper.ThrowIfNull(stack);

                // SAFETY: The non-null stack remains the designated owner of the returned alias.
                unsafe
                {
                    return new(stack);
                }
            }
        }
    }
}
