using System;
using EncosyTower.Collections;
using EncosyTower.Collections.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Testing
{
    /// <summary>
    /// Owns the native blocks that a <see cref="SharedListUnsafe{T}"/> header borrows, so a test can build a header
    /// without a shared collection. The blocks are released by <see cref="Dispose"/>.
    /// </summary>
    internal sealed unsafe class SharedListStorage<T> : IDisposable
        where T : unmanaged
    {
        public readonly T* buffer;
        public readonly int capacity;
        public readonly int* count;
        public readonly int* version;

        public SharedListStorage(int capacity)
        {
            this.capacity = capacity;
            buffer = SharedUnsafeStorage.AllocateCleared<T>(capacity);
            count = SharedUnsafeStorage.AllocateCleared<int>(1);
            version = SharedUnsafeStorage.AllocateCleared<int>(1);
        }

        public int Count
        {
            get => *count;
            set => *count = value;
        }

        public int Version
        {
            get => *version;
            set => *version = value;
        }

        public SharedListUnsafe<T> CreateHeader()
            => new() {
                _buffer = buffer,
                _capacity = capacity,
                _count = count,
                _version = version,
            };

        public void Dispose()
        {
            SharedUnsafeStorage.Free(buffer);
            SharedUnsafeStorage.Free(count);
            SharedUnsafeStorage.Free(version);
        }
    }

    /// <summary>
    /// Owns the native blocks that a <see cref="SharedQueueUnsafe{T}"/> header borrows.
    /// </summary>
    internal sealed unsafe class SharedQueueStorage<T> : IDisposable
        where T : unmanaged
    {
        public readonly T* buffer;
        public readonly int capacity;
        public readonly int* head;
        public readonly int* tail;
        public readonly int* count;
        public readonly int* version;

        public SharedQueueStorage(int capacity)
        {
            this.capacity = capacity;
            buffer = SharedUnsafeStorage.AllocateCleared<T>(capacity);
            head = SharedUnsafeStorage.AllocateCleared<int>(1);
            tail = SharedUnsafeStorage.AllocateCleared<int>(1);
            count = SharedUnsafeStorage.AllocateCleared<int>(1);
            version = SharedUnsafeStorage.AllocateCleared<int>(1);
        }

        public int Head
        {
            get => *head;
            set => *head = value;
        }

        public int Tail
        {
            get => *tail;
            set => *tail = value;
        }

        public int Count
        {
            get => *count;
            set => *count = value;
        }

        public int Version
        {
            get => *version;
            set => *version = value;
        }

        public SharedQueueUnsafe<T> CreateHeader()
            => new() {
                _buffer = buffer,
                _capacity = capacity,
                _head = head,
                _tail = tail,
                _count = count,
                _version = version,
            };

        public void Dispose()
        {
            SharedUnsafeStorage.Free(buffer);
            SharedUnsafeStorage.Free(head);
            SharedUnsafeStorage.Free(tail);
            SharedUnsafeStorage.Free(count);
            SharedUnsafeStorage.Free(version);
        }
    }

    /// <summary>
    /// Owns the native blocks that a <see cref="SharedStackUnsafe{T}"/> header borrows.
    /// </summary>
    internal sealed unsafe class SharedStackStorage<T> : IDisposable
        where T : unmanaged
    {
        public readonly T* buffer;
        public readonly int capacity;
        public readonly int* count;
        public readonly int* version;

        public SharedStackStorage(int capacity)
        {
            this.capacity = capacity;
            buffer = SharedUnsafeStorage.AllocateCleared<T>(capacity);
            count = SharedUnsafeStorage.AllocateCleared<int>(1);
            version = SharedUnsafeStorage.AllocateCleared<int>(1);
        }

        public int Count
        {
            get => *count;
            set => *count = value;
        }

        public int Version
        {
            get => *version;
            set => *version = value;
        }

        public SharedStackUnsafe<T> CreateHeader()
            => new() {
                _buffer = buffer,
                _capacity = capacity,
                _count = count,
                _version = version,
            };

        public void Dispose()
        {
            SharedUnsafeStorage.Free(buffer);
            SharedUnsafeStorage.Free(count);
            SharedUnsafeStorage.Free(version);
        }
    }

    /// <summary>
    /// Owns the native blocks that a <see cref="SharedArrayMapUnsafe{TKey, TValue}"/> header borrows. The bucket
    /// multiplier starts as the owner of a non-empty map would set it.
    /// </summary>
    internal sealed unsafe class SharedArrayMapStorage<TKey, TValue> : IDisposable
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        public readonly ArrayMapNode<TKey>* valuesInfo;
        public readonly int capacity;
        public readonly TValue* values;
        public readonly int* buckets;
        public readonly int bucketCapacity;
        public readonly ulong* fastModBucketsMultiplier;
        public readonly uint* collisions;
        public readonly int* freeValueCellIndex;
        public readonly int* version;

        public SharedArrayMapStorage(int capacity, int bucketCapacity)
        {
            this.capacity = capacity;
            this.bucketCapacity = bucketCapacity;
            valuesInfo = SharedUnsafeStorage.AllocateCleared<ArrayMapNode<TKey>>(capacity);
            values = SharedUnsafeStorage.AllocateCleared<TValue>(capacity);
            buckets = SharedUnsafeStorage.AllocateCleared<int>(bucketCapacity);
            fastModBucketsMultiplier = SharedUnsafeStorage.AllocateCleared<ulong>(1);
            collisions = SharedUnsafeStorage.AllocateCleared<uint>(1);
            freeValueCellIndex = SharedUnsafeStorage.AllocateCleared<int>(1);
            version = SharedUnsafeStorage.AllocateCleared<int>(1);

            if (bucketCapacity > 0)
            {
                *fastModBucketsMultiplier = EncosyTower.Common.HashHelpers.GetFastModMultiplier((uint)bucketCapacity);
            }
        }

        public int Count
        {
            get => *freeValueCellIndex;
            set => *freeValueCellIndex = value;
        }

        public int Version
        {
            get => *version;
            set => *version = value;
        }

        public SharedArrayMapUnsafe<TKey, TValue> CreateHeader()
            => new() {
                _valuesInfo = valuesInfo,
                _valuesInfoCapacity = capacity,
                _values = values,
                _valuesCapacity = capacity,
                _buckets = buckets,
                _bucketsCapacity = bucketCapacity,
                _fastModBucketsMultiplier = fastModBucketsMultiplier,
                _collisions = collisions,
                _freeValueCellIndex = freeValueCellIndex,
                _version = version,
            };

        public void Dispose()
        {
            SharedUnsafeStorage.Free(valuesInfo);
            SharedUnsafeStorage.Free(values);
            SharedUnsafeStorage.Free(buckets);
            SharedUnsafeStorage.Free(fastModBucketsMultiplier);
            SharedUnsafeStorage.Free(collisions);
            SharedUnsafeStorage.Free(freeValueCellIndex);
            SharedUnsafeStorage.Free(version);
        }
    }

    internal static unsafe class SharedUnsafeStorage
    {
        public static T* AllocateCleared<T>(int count)
            where T : unmanaged
        {
            if (count <= 0)
            {
                return null;
            }

            var size = (long)UnsafeUtility.SizeOf<T>() * count;
            var pointer = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);

            UnsafeUtility.MemClear(pointer, size);

            return (T*)pointer;
        }

        public static void Free<T>(T* pointer)
            where T : unmanaged
        {
            if (pointer != null)
            {
                UnsafeUtility.Free(pointer, Allocator.Persistent);
            }
        }
    }
}
