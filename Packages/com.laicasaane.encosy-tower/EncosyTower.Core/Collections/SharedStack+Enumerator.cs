using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    public partial class SharedStack<T, TNative>
        where T : unmanaged
        where TNative : unmanaged
    {
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly ReadOnly _stack;
            private readonly int _version;
            private int _index;
            private T _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ReadOnly stack)
            {
                DebuggingThrowHelper.ThrowIfNotCreated(stack);
                _stack = stack;
                _version = stack._stack.VersionRO;
                _index = 0;
                _current = default;
            }

            public bool MoveNext()
            {
                ThrowHelper.ThrowIfCollectionWasModified(_version == _stack._stack.VersionRO);

                if ((uint)_index < (uint)_stack.Count)
                {
                    _current = _stack.ToArray()[_index++];
                    return true;
                }
                _current = default;
                return false;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }

            object IEnumerator.Current => Current;

            public void Reset()
            {
                ThrowHelper.ThrowIfCollectionWasModified(_version == _stack._stack.VersionRO);

                _index = 0;
                _current = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Dispose()
            {
            }
        }
    }
}
