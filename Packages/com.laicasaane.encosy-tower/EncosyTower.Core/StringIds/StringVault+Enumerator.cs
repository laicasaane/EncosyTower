using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;

using CollectionsThrowHelper = EncosyTower.Collections.ThrowHelper;

namespace EncosyTower.StringIds
{
    partial class StringVault
    {
        public struct Enumerator : IEnumerator<UnmanagedString>
        {
            private readonly SharedListNative<Range>.ReadOnly _ranges;
            private readonly SharedListNative<byte>.ReadOnly _buffer;
            private readonly int _count;

            private Range _current;
            private readonly int _version;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(
                  SharedListNative<Range>.ReadOnly ranges
                , SharedListNative<byte>.ReadOnly buffer
                , int count
            )
            {
                _ranges = ranges;
                _buffer = buffer;
                _count = count;
                _version = ranges.Version;
                _current = default;
                _index = 0;
            }

            public bool MoveNext()
            {
                var ranges = _ranges;

                if (_version == ranges.Version && ((uint)_index < (uint)_count))
                {
                    _current = ranges[_index];
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                CollectionsThrowHelper.ThrowIfCollectionWasModified(_version == _ranges.Version);

                _index = _count + 1;
                _current = default;
                return false;
            }

            public readonly UnmanagedString Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => UnmanagedString.FromBufferAt(_current, _buffer).GetValueOrThrow();
            }

            public void Reset()
            {
                CollectionsThrowHelper.ThrowIfCollectionWasModified(_version == _ranges.Version);

                _index = 0;
                _current = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Dispose()
            {
            }

            readonly object IEnumerator.Current
            {
                get
                {
                    CollectionsThrowHelper.ThrowIfEnumeratorOperationIsInvalid(_index != 0 && _index != _count + 1);

                    return Current;
                }
            }
        }
    }
}
