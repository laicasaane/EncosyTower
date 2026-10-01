using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using Unity.Collections;

namespace EncosyTower.StringIds
{
    partial class StringVault
    {
        /// <safety>The returned view must not outlive this vault or survive backing collection resize.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnly AsReadOnly()
        {
            // SAFETY: This vault owns every collection borrowed by the returned read-only view.
            unsafe
            {
                return new(this);
            }
        }

        public readonly partial struct ReadOnly : IReadOnlyStringVault, IReadOnlyList<UnmanagedString>
        {
            internal readonly SharedArrayMapNative<StringHash, StringId>.ReadOnly _map;
            internal readonly SharedArrayMapNative<UnmanagedString, StringId>.ReadOnly _collisionMap;
            internal readonly SharedListNative<Range>.ReadOnly _unmanagedStringRanges;
            internal readonly SharedListNative<byte>.ReadOnly _unmanagedStringBuffer;
            internal readonly SharedListNative<Option<StringHash>>.ReadOnly _hashes;
            internal readonly NativeArray<int>.ReadOnly _count;

            /// <safety>
            /// The vault must remain alive and must not resize its backing collections while this view is used.
            /// </safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe ReadOnly(StringVault vault)
            {
                // SAFETY: One vault owns all six backing allocations for the complete borrowed-view lifetime.
                unsafe
                {
                    _map = vault._map.AsNative();
                    _collisionMap = vault._collisionMap.AsNative();
                    _unmanagedStringRanges = vault._unmanagedStringRanges.AsNative();
                    _unmanagedStringBuffer = vault._unmanagedStringBuffer.AsNative();
                    _hashes = vault._hashes.AsNative();
                    _count = vault._count.AsNativeArray().AsReadOnly();
                }

                AllowEmptyString = vault.AllowEmptyString;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map.IsCreated
                    && _collisionMap.IsCreated
                    && _unmanagedStringRanges.IsCreated
                    && _unmanagedStringBuffer.IsCreated
                    && _hashes.IsCreated
                    && _count.IsCreated;
            }

            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashes.Count;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _count[0];
            }

            public bool AllowEmptyString { get; }

            public UnmanagedString this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => UnmanagedString
                    .FromBufferAt(_unmanagedStringRanges[index], _unmanagedStringBuffer)
                    .GetValueOrThrow();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Option<StringId> TryGetId(in UnmanagedString str)
                => Option.SomeIf(TryGetId(str, out var result), result);

            public bool TryGetId(in UnmanagedString str, out StringId result)
            {
                if (AllowEmptyString == false && str.IsEmpty)
                {
                    result = default;
                    return false;
                }

                var hash = str.GetHashCode64();
                var registered = _map.TryGetValue(hash, out var id);

                if (registered)
                {
                    TryGetUnmanagedString(id, out var registeredString);

                    if (str == registeredString)
                    {
                        result = id;
                        return true;
                    }

                    if (_collisionMap.TryGetValue(str, out var collidedId))
                    {
                        result = collidedId;
                        return true;
                    }
                }

                result = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Option<UnmanagedString> TryGetUnmanagedString(StringId id)
                => Option.SomeIf(TryGetUnmanagedString(id, out var result), result);

            public bool TryGetUnmanagedString(StringId id, out UnmanagedString result)
            {
                var indexUnsigned = (uint)id.Id;
                var index = (int)indexUnsigned;
                var validIndex = indexUnsigned < (uint)_hashes.Count;

                var resultOpt = validIndex
                    ? UnmanagedString.FromBufferAt(_unmanagedStringRanges[index], _unmanagedStringBuffer)
                    : Option.None;

                result = resultOpt.GetValueOrDefault();
                return resultOpt.HasValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ContainsId(StringId id)
            {
                var indexUnsigned = (uint)id.Id;
                var index = (int)indexUnsigned;
                var validIndex = indexUnsigned < (uint)_hashes.Count;
                return validIndex && _hashes[index].HasValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<UnmanagedString> destination)
                => CopyTo(destination, Count);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<UnmanagedString> destination, int length)
                => CopyTo(0, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, Span<UnmanagedString> destination)
                => CopyTo(sourceStartIndex, destination, Count);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, Span<UnmanagedString> destination, int length)
            {
                // SAFETY: The copy consumes both borrowed spans before this method returns.
                unsafe
                {
                    new UnmanagedStringSpan(
                          _unmanagedStringRanges.AsReadOnlySpan()[1..Count]
                        , _unmanagedStringBuffer.AsReadOnlySpan()
                    ).CopyTo(sourceStartIndex, destination, length);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(Span<UnmanagedString> destination)
                => TryCopyTo(destination, Count);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(Span<UnmanagedString> destination, int length)
                => TryCopyTo(0, destination, length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, Span<UnmanagedString> destination)
                => TryCopyTo(sourceStartIndex, destination, Count);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, Span<UnmanagedString> destination, int length)
            {
                // SAFETY: The copy consumes both borrowed spans before this method returns.
                unsafe
                {
                    return new UnmanagedStringSpan(
                          _unmanagedStringRanges.AsReadOnlySpan()[1..Count]
                        , _unmanagedStringBuffer.AsReadOnlySpan()
                    ).TryCopyTo(sourceStartIndex, destination, length);
                }
            }

            /// <safety>The vault must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe Enumerator GetEnumerator()
            {
                // SAFETY: The enumerator borrows two collections owned by the same live vault.
                unsafe
                {
                    return new(_unmanagedStringRanges, _unmanagedStringBuffer);
                }
            }

            /// <safety>The vault must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe IEnumerator<UnmanagedString> IEnumerable<UnmanagedString>.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows this live vault view.
                unsafe
                {
                    return GetEnumerator();
                }
            }

            /// <safety>The vault must remain alive and unmodified during enumeration.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe IEnumerator IEnumerable.GetEnumerator()
            {
                // SAFETY: The interface enumerator borrows this live vault view.
                unsafe
                {
                    return GetEnumerator();
                }
            }

            /// <safety>The returned alias must not outlive the source vault.</safety>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator ReadOnly(StringVault vault)
            {
                // SAFETY: The caller accepts the source vault lifetime inherited by the alias.
                unsafe
                {
                    return new(vault);
                }
            }
        }
    }
}
