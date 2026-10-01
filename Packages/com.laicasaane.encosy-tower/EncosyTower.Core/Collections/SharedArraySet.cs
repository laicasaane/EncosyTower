// https://github.com/sebas77/Svelto.Common/blob/master/DataStructures/Dictionaries/SveltoDictionary.cs

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
    /// <summary>
    /// A set whose storage can be shared with a <see cref="SharedArraySetNative{T}"/>
    /// without copying.
    /// </summary>
    /// <remarks>
    /// Not thread-safe.
    /// </remarks>
    [Serializable, DebuggerTypeProxy(typeof(SharedArraySetDebugProxy<>))]
    public partial class SharedArraySet<T> : IDisposable
        , ICollection<T>, IReadOnlyCollection<T>
        , IClearable, IIncreaseCapacity, IHasCount
        , ICopyToSpan<T>, ITryCopyToSpan<T>
        where T : unmanaged, IEquatable<T>
    {
        [NonSerialized] internal SharedArrayMap<T, T, T> _map;

        public SharedArraySet() : this(0)
        {
        }

        public SharedArraySet(int capacity)
        {
            _map = new(capacity);
        }

        public SharedArraySet([NotNull] SharedArraySet<T> source)
        {
            DebuggingThrowHelper.ThrowIfNull(source);
            _map = new(source._map);
        }

        public SharedArraySet(ReadOnly source) : this(GetSet(source))
        {
        }

        public SharedArraySet(in SharedArraySetNative<T> source)
        {
            DebuggingThrowHelper.ThrowIfNotCreated(source);

            // SAFETY: source is live and the map constructor copies its contents synchronously.
            unsafe
            {
                var map = source.AsMap();
                _map = new(in map);
            }
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map.Capacity;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map.Count;
        }

        public ReadOnlyMemory<T> Items
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map.Values;
        }

        bool ICollection<T>.IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        public void Dispose()
        {
            _map?.Dispose();
            _map = null;
        }

        /// <remarks>
        /// The set must not be modified while it is being enumerated.
        /// </remarks>
        /// <safety>This set must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe SharedArraySetEnumerator<T> GetEnumerator()
        {
            // SAFETY: The enumerator borrows this set and preserves the backing map version checks.
            unsafe
            {
                return new(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T value)
            => _map.TryAdd(value, in value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(in T value)
            => _map.TryAdd(value, in value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
            => _map.Clear();

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
            => new CopyToSpan<T>(Items.Span).CopyTo(sourceStartIndex, destination, length);

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
            => new CopyToSpan<T>(Items.Span).TryCopyTo(sourceStartIndex, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T value)
            => _map.ContainsKey(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T value)
            => _map.ContainsKey(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
            => Items.Span.CopyTo(array.AsSpan(arrayIndex));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int size)
            => _map.EnsureCapacity(size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IncreaseCapacityBy(int amount)
            => _map.IncreaseCapacityBy(amount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IncreaseCapacityTo(int size)
            => _map.IncreaseCapacityTo(size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T value)
            => _map.Remove(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T value)
            => _map.Remove(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
            => _map.Trim();

        public void Intersect([NotNull] SharedArraySet<T> otherSet)
        {
            DebuggingThrowHelper.ThrowIfNull(otherSet);
            _map.Intersect(otherSet._map);
        }

        public void Exclude([NotNull] SharedArraySet<T> otherSet)
        {
            DebuggingThrowHelper.ThrowIfNull(otherSet);
            _map.Exclude(otherSet._map);
        }

        public void Union([NotNull] SharedArraySet<T> otherSet)
        {
            DebuggingThrowHelper.ThrowIfNull(otherSet);
            _map.Union(otherSet._map);
        }

        /// <safety>This set must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            // SAFETY: The interface enumerator borrows this live set.
            unsafe
            {
                return GetEnumerator();
            }
        }

        /// <safety>This set must remain alive and unmodified during enumeration.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe IEnumerator IEnumerable.GetEnumerator()
        {
            // SAFETY: The interface enumerator borrows this live set.
            unsafe
            {
                return GetEnumerator();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICollection<T>.Add(T item)
            => Add(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SharedArraySet<T> GetSet(ReadOnly source)
        {
            DebuggingThrowHelper.ThrowIfNotCreated(source);
            return source._set;
        }
    }

    public struct SharedArraySetEnumerator<T> : IEnumerator<T>, IIsValid
        where T : unmanaged, IEquatable<T>
    {
        private SharedArrayMap<T, T, T>.KeyEnumerator _enumerator;

        /// <safety>The set must remain alive and unmodified during enumeration.</safety>
        public unsafe SharedArraySetEnumerator([NotNull] SharedArraySet<T> set) : this()
        {
            DebuggingThrowHelper.ThrowIfNull(set);

            // SAFETY: The enumerator borrows the live backing map and preserves its version checks.
            unsafe
            {
                _enumerator = set._map.Keys.GetEnumerator();
            }
        }

        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _enumerator.IsValid;
        }

        public readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _enumerator.Current;
        }

        readonly object IEnumerator.Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Current;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
            => _enumerator.MoveNext();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
            => _enumerator.Reset();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
            => _enumerator.Dispose();
    }

    internal sealed class SharedArraySetDebugProxy<T>
        where T : unmanaged, IEquatable<T>
    {
        private readonly SharedArraySet<T> _set;

        public SharedArraySetDebugProxy([NotNull] SharedArraySet<T> set)
        {
            DebuggingThrowHelper.ThrowIfNull(set);
            _set = set;
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (uint)_set.Count;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _set.Items.ToArray();
        }
    }
}
