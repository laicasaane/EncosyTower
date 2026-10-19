using System;
using EncosyTower.Buffers;
using EncosyTower.Tests.Core.Collections;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Buffers
{
    // Adapted from Unity.Collections.Tests/UnsafeListTests.cs (fixed-capacity subset).
    public partial class BufferUnsafeTests
    {
        [Test]
        public void Constructor_CreatesValidBuffer()
        {
            using var buffer = new BufferUnsafe<int>(8, Allocator.Temp);

            Assert.IsTrue(buffer.IsCreated);
            Assert.AreEqual(8, buffer.Capacity);
        }

        [Test]
        public void DefaultBuffer_IsNotCreated()
        {
            BufferUnsafe<int> buffer = default;

            Assert.IsFalse(buffer.IsCreated);
        }

        [Test]
        public void Constructor_ClearsToZero()
        {
            using var buffer = new BufferUnsafe<int>(4, Allocator.Temp);

            for (var i = 0; i < buffer.Capacity; i++)
            {
                Assert.AreEqual(0, buffer[i]);
            }
        }

        [Test]
        public void Indexer_GetSet()
        {
            using var buffer = new BufferUnsafe<int>(4, Allocator.Temp);

            buffer[0] = 10;
            buffer[3] = 40;

            Assert.AreEqual(10, buffer[0]);
            Assert.AreEqual(40, buffer[3]);
        }

        [Test]
        public void Resize_Grow_PreservesContent()
        {
            using var buffer = new BufferUnsafe<int>(4, Allocator.Temp);
            buffer[0] = 1;
            buffer[1] = 2;
            buffer[2] = 3;
            buffer[3] = 4;

            buffer.Resize(16, copyContent: true);

            Assert.GreaterOrEqual(buffer.Capacity, 16);
            Assert.AreEqual(1, buffer[0]);
            Assert.AreEqual(2, buffer[1]);
            Assert.AreEqual(3, buffer[2]);
            Assert.AreEqual(4, buffer[3]);
        }

        [Test]
        public void Clear_ZeroesContent()
        {
            using var buffer = new BufferUnsafe<int>(4, Allocator.Temp);
            buffer[0] = 1;
            buffer[1] = 2;

            buffer.Clear();

            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(0, buffer[1]);
        }

        [Test]
        public void CopyFrom_And_CopyTo_Span()
        {
            using var buffer = new BufferUnsafe<int>(4, Allocator.Temp);

            buffer.CopyFrom(new[] { 5, 6, 7, 8 });

            var dst = new int[4];
            buffer.CopyTo(dst);

            CollectionAssert.AreEqual(new[] { 5, 6, 7, 8 }, dst);
        }

        [Test]
        public void AsSpan_ReflectsContent()
        {
            using var buffer = new BufferUnsafe<int>(4, Allocator.Temp);
            buffer[0] = 11;
            buffer[1] = 22;

            var span = buffer.AsSpan();

            Assert.AreEqual(4, span.Length);
            Assert.AreEqual(11, span[0]);
            Assert.AreEqual(22, span[1]);
        }

        [Test]
        public void Reinterpret_ReusesMemory()
        {
            using var buffer = new BufferUnsafe<int>(4, Allocator.Temp);
            buffer[0] = 7;

            var reinterpreted = buffer.Reinterpret<uint>();

            Assert.AreEqual(buffer.Capacity, reinterpreted.Capacity);
            Assert.AreEqual(7u, reinterpreted[0]);
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var buffer = new BufferUnsafe<int>(4, Allocator.Temp);
            Assert.IsTrue(buffer.IsCreated);

            buffer.Dispose();

            Assert.IsFalse(buffer.IsCreated);
        }

        [Test]
        public unsafe void PointerConstructor_UsesAndOwnsSuppliedAllocation()
        {
            var strategy = new AllocatorStrategy(Allocator.Temp);
            int* pointer = strategy.AllocateArray<int>(3);
            BufferUnsafe<int> buffer = default;

            try
            {
                buffer = new BufferUnsafe<int>(pointer, 3, strategy);
                pointer = null;

                buffer[0] = 11;
                buffer[2] = 33;
                Assert.AreEqual(11, buffer[0]);
                Assert.AreEqual(33, buffer[2]);
            }
            finally
            {
                if (buffer.IsCreated)
                {
                    buffer.Dispose();
                }

                strategy.FreeArray(pointer, 3);
                pointer = null;
            }
        }

        [Test]
        public void Alloc_CreatesDefaultBufferAndClearsStorage()
        {
            BufferUnsafe<int> buffer = default;

            try
            {
                buffer.Alloc(3, new AllocatorStrategy(Allocator.Temp), memClear: true);

                Assert.IsTrue(buffer.IsCreated);
                Assert.AreEqual(3, buffer.Capacity);
                CollectionAssert.AreEqual(new[] { 0, 0, 0 }, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void Resize_RemainingOverloadsPreserveOrDiscardContent()
        {
            using var buffer = new BufferUnsafe<int>(3, Allocator.Temp);
            buffer.CopyFrom(new[] { 1, 2, 3 });

            buffer.Resize(5);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 0, 0 }, buffer.AsSpan().ToArray());

            buffer.Resize(4, copyContent: false, memClear: true);
            CollectionAssert.AreEqual(new[] { 0, 0, 0, 0 }, buffer.AsSpan().ToArray());
        }

        [Test]
        public void FastClear_DoesNotWriteUnmanagedStorage()
        {
            using var buffer = new BufferUnsafe<int>(3, Allocator.Temp);
            buffer.CopyFrom(new[] { 1, 2, 3 });

            buffer.FastClear();

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, buffer.AsSpan().ToArray());
        }

        [Test]
        public void CopyFromAndTryCopyFrom_AllOverloadsUseRequestedWindow()
        {
            using var buffer = new BufferUnsafe<int>(6, Allocator.Temp);
            var source = new[] { 1, 2, 3, 4 };

            buffer.CopyFrom(source);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 0, 0 }, buffer.AsSpan().ToArray());
            buffer.Clear();
            buffer.CopyFrom(source, 2);
            CollectionAssert.AreEqual(new[] { 1, 2, 0, 0, 0, 0 }, buffer.AsSpan().ToArray());
            buffer.Clear();
            buffer.CopyFrom(1, source);
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 0 }, buffer.AsSpan().ToArray());
            buffer.Clear();
            buffer.CopyFrom(2, source, 3);
            CollectionAssert.AreEqual(new[] { 0, 0, 1, 2, 3, 0 }, buffer.AsSpan().ToArray());

            Assert.IsTrue(buffer.TryCopyFrom(source));
            Assert.IsTrue(buffer.TryCopyFrom(source, 2));
            Assert.IsTrue(buffer.TryCopyFrom(1, source));
            Assert.IsTrue(buffer.TryCopyFrom(2, source, 3));

            var snapshot = buffer.AsSpan().ToArray();
            Assert.IsFalse(buffer.TryCopyFrom(5, source));
            Assert.IsFalse(buffer.TryCopyFrom(5, source, 2));
            CollectionAssert.AreEqual(snapshot, buffer.AsSpan().ToArray());
        }

        [Test]
        public void CopyToAndTryCopyTo_AllOverloadsUseRequestedWindow()
        {
            using var buffer = new BufferUnsafe<int>(4, Allocator.Temp);
            buffer.CopyFrom(new[] { 1, 2, 3, 4 });
            var destination = new int[4];

            buffer.CopyTo(destination);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, destination);
            Array.Clear(destination, 0, destination.Length);
            buffer.CopyTo(destination, 2);
            CollectionAssert.AreEqual(new[] { 1, 2, 0, 0 }, destination);
            destination = new int[3];
            buffer.CopyTo(1, destination);
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, destination);
            destination = new int[4];
            buffer.CopyTo(1, destination, 2);
            CollectionAssert.AreEqual(new[] { 2, 3, 0, 0 }, destination);
            Assert.IsTrue(buffer.TryCopyTo(destination));
            Assert.IsTrue(buffer.TryCopyTo(destination, 2));
            Assert.IsTrue(buffer.TryCopyTo(1, new int[3]));
            Assert.IsTrue(buffer.TryCopyTo(1, destination, 2));

            destination = new[] { 9, 9, 9, 9 };
            Assert.IsFalse(buffer.TryCopyTo(3, destination));
            Assert.IsFalse(buffer.TryCopyTo(3, destination, 2));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public unsafe void UnsafePointerAndReadOnlyViews_AliasOwnerStorage()
        {
            using var buffer = new BufferUnsafe<int>(4, Allocator.Temp);
            buffer.CopyFrom(new[] { 1, 2, 3, 4 });

            var pointer = (int*)buffer.GetUnsafePtr();
            var span = buffer.AsReadOnlySpan();
            var readOnly = buffer.AsReadOnly();
            BufferUnsafe<int>.ReadOnly converted = buffer;

            pointer[0] = 10;
            Assert.AreEqual(10, buffer[0]);
            Assert.AreEqual(10, span[0]);
            Assert.AreEqual(10, readOnly[0]);
            Assert.AreEqual(10, converted[0]);
        }

        [Test]
        public unsafe void ReadOnly_AllCopyPointerViewAndReinterpretMembersWork()
        {
            using var buffer = new BufferUnsafe<int>(4, Allocator.Temp);
            buffer.CopyFrom(new[] { 1, 2, 3, 4 });
            var readOnly = buffer.AsReadOnly();

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(4, readOnly.Capacity);
            Assert.AreEqual(2, readOnly[1]);

            var destination = new int[4];
            readOnly.CopyTo(destination);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, destination);
            Array.Clear(destination, 0, destination.Length);
            readOnly.CopyTo(destination, 2);
            CollectionAssert.AreEqual(new[] { 1, 2, 0, 0 }, destination);
            destination = new int[3];
            readOnly.CopyTo(1, destination);
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, destination);
            destination = new int[4];
            readOnly.CopyTo(1, destination, 2);
            CollectionAssert.AreEqual(new[] { 2, 3, 0, 0 }, destination);
            Assert.IsTrue(readOnly.TryCopyTo(destination));
            Assert.IsTrue(readOnly.TryCopyTo(destination, 2));
            Assert.IsTrue(readOnly.TryCopyTo(1, new int[3]));
            Assert.IsTrue(readOnly.TryCopyTo(1, destination, 2));
            Assert.IsFalse(readOnly.TryCopyTo(3, destination));
            Assert.IsFalse(readOnly.TryCopyTo(3, destination, 2));

            Assert.AreEqual((System.IntPtr)buffer.GetUnsafePtr(), (System.IntPtr)readOnly.GetUnsafeReadOnlyPtr());
            Assert.AreEqual(4, readOnly.AsReadOnlySpan().Length);
            Assert.AreEqual(1u, readOnly.Reinterpret<uint>()[0]);
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var buffer = new BufferUnsafe<int>(4, Allocator.TempJob);
            buffer[0] = 1;

            var handle = buffer.Dispose(default);

            Assert.IsFalse(buffer.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void DoubleDispose_IsNoOp()
        {
            var buffer = new BufferUnsafe<int>(4, Allocator.Temp);
            buffer.Dispose();

            Assert.DoesNotThrow(() => buffer.Dispose());
            Assert.IsFalse(buffer.IsCreated);
            Assert.AreEqual(0, buffer.Capacity);
            Assert.AreEqual(0, buffer.AsSpan().Length);
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_CreatesAndDisposes(Allocator allocator)
        {
            var buffer = new BufferUnsafe<int>(4, allocator);

            try
            {
                Assert.IsTrue(buffer.IsCreated);
                Assert.AreEqual(4, buffer.Capacity);
            }
            finally
            {
                buffer.Dispose();
            }

            Assert.IsFalse(buffer.IsCreated);
            Assert.AreEqual(0, buffer.Capacity);
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_JobHandle_AcceptedAllocator_ReleasesTheStorage(Allocator allocator)
        {
            var buffer = new BufferUnsafe<int>(4, allocator);
            buffer[0] = 1;

            var handle = buffer.Dispose(default);

            Assert.IsFalse(buffer.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Constructor_DefaultAllocatorStrategy_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _ = new BufferUnsafe<int>(4, default(AllocatorStrategy)));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Alloc_AlreadyAllocated_Throws()
        {
            var buffer = new BufferUnsafe<int>(2, Allocator.Temp);

            try
            {
                Assert.Throws<InvalidOperationException>(() => buffer.Alloc(4, Allocator.Temp));
                Assert.AreEqual(2, buffer.Capacity);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void Alloc_AfterDispose_AllocatesAgain()
        {
            var buffer = new BufferUnsafe<int>(2, Allocator.Temp);
            buffer.Dispose();

            try
            {
                buffer.Alloc(3, Allocator.Temp);

                Assert.IsTrue(buffer.IsCreated);
                CollectionAssert.AreEqual(new[] { 0, 0, 0 }, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void Alloc_ZeroCapacity_CreatesAnEmptyBufferThatCanGrow()
        {
            var buffer = default(BufferUnsafe<int>);

            try
            {
                buffer.Alloc(0, Allocator.Temp);

                Assert.IsTrue(buffer.IsCreated);
                Assert.AreEqual(0, buffer.Capacity);
                Assert.AreEqual(0, buffer.AsSpan().Length);
                Assert.AreEqual(0, buffer.AsReadOnlySpan().Length);

                buffer.Resize(3);

                Assert.AreEqual(3, buffer.Capacity);
                CollectionAssert.AreEqual(new[] { 0, 0, 0 }, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Alloc_NegativeCapacity_Throws()
        {
            var buffer = default(BufferUnsafe<int>);

            try
            {
                Assert.Catch(() => buffer.Alloc(-1, Allocator.Persistent));
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Constructor_NegativeCapacity_Throws()
        {
            Assert.Catch(() => _ = new BufferUnsafe<int>(-1, Allocator.Persistent));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Resize_NegativeCapacity_Throws()
        {
            var buffer = new BufferUnsafe<int>(4, Allocator.Persistent);

            try
            {
                buffer.CopyFrom(new[] { 1, 2, 3, 4 });

                Assert.Catch(() => buffer.Resize(-5));
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Resize_UncreatedBuffer_Throws()
        {
            var buffer = default(BufferUnsafe<int>);

            Assert.Throws<InvalidOperationException>(() => buffer.Resize(4));
            Assert.Throws<InvalidOperationException>(() => buffer.Resize(4, copyContent: false));
            Assert.Throws<InvalidOperationException>(() => buffer.Resize(4, copyContent: false, memClear: false));
        }

        [Test]
        public void Resize_SameCapacity_KeepsContent()
        {
            var buffer = new BufferUnsafe<int>(3, Allocator.Temp);

            try
            {
                buffer.CopyFrom(new[] { 1, 2, 3 });

                buffer.Resize(3);
                buffer.Resize(3, copyContent: false, memClear: true);

                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void Resize_ToZeroThenGrow_KeepsTheBufferUsable()
        {
            var buffer = new BufferUnsafe<int>(3, Allocator.Temp);

            try
            {
                buffer.CopyFrom(new[] { 1, 2, 3 });

                buffer.Resize(0);

                Assert.IsTrue(buffer.IsCreated);
                Assert.AreEqual(0, buffer.Capacity);
                Assert.AreEqual(0, buffer.AsSpan().Length);

                buffer.Resize(2);

                Assert.AreEqual(2, buffer.Capacity);
                CollectionAssert.AreEqual(new[] { 0, 0 }, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void Resize_Shrink_KeepsTheLeadingElements()
        {
            var buffer = new BufferUnsafe<int>(5, Allocator.Temp);

            try
            {
                buffer.CopyFrom(new[] { 1, 2, 3, 4, 5 });

                buffer.Resize(2);

                Assert.AreEqual(2, buffer.Capacity);
                CollectionAssert.AreEqual(new[] { 1, 2 }, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void Elements_ByteElement_RoundTripBitExact()
        {
            AssertElementRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public void Elements_ThreeByteStruct_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Rgb { r = 1, g = 2, b = 3 }
                , new Rgb { r = 0, g = 0, b = 0 }
                , new Rgb { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public void Elements_LongElement_RoundTripBitExact()
        {
            AssertElementRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public void Elements_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = 0, b = 0, c = 0 }
                , new Wide { a = long.MinValue, b = 7, c = -1 }
            );
        }

        [Test]
        public void UncreatedBuffer_Members_ReturnEmptyViewsWithoutThrowing()
        {
            var buffer = default(BufferUnsafe<int>);

            Assert.DoesNotThrow(() => buffer.FastClear());
            Assert.DoesNotThrow(() => buffer.Clear());
            Assert.AreEqual(0, buffer.Capacity);
            Assert.AreEqual(0, buffer.AsSpan().Length);
            Assert.AreEqual(0, buffer.AsReadOnlySpan().Length);

            // SAFETY: The pointer of an uncreated buffer is only compared with null and never dereferenced.
            unsafe
            {
                Assert.AreEqual(System.IntPtr.Zero, (System.IntPtr)buffer.GetUnsafePtr());
            }
        }

        [Test]
        public void AsReadOnly_UncreatedBuffer_Throws()
        {
            var buffer = default(BufferUnsafe<int>);

            Assert.Throws<ArgumentException>(() => buffer.AsReadOnly());
            Assert.Throws<ArgumentException>(() => _ = (BufferUnsafe<int>.ReadOnly)buffer);
        }

        [Test]
        public void AsReadOnly_ZeroCapacity_ReportsNoElements()
        {
            var buffer = default(BufferUnsafe<int>);

            try
            {
                buffer.Alloc(0, Allocator.Temp);

                var readOnly = buffer.AsReadOnly();

                Assert.AreEqual(0, readOnly.Capacity);
                Assert.AreEqual(0, readOnly.AsReadOnlySpan().Length);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void CopyFrom_EmptySource_LeavesTheBufferUnchanged()
        {
            var buffer = new BufferUnsafe<int>(3, Allocator.Temp);

            try
            {
                buffer.CopyFrom(new[] { 1, 2, 3 });

                buffer.CopyFrom(ReadOnlySpan<int>.Empty);
                buffer.CopyFrom(2, ReadOnlySpan<int>.Empty);
                buffer.CopyFrom(3, ReadOnlySpan<int>.Empty);

                Assert.IsTrue(buffer.TryCopyFrom(ReadOnlySpan<int>.Empty));
                Assert.IsTrue(buffer.TryCopyFrom(3, ReadOnlySpan<int>.Empty));
                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void CopyFrom_UncreatedBuffer_AcceptsEmptyAndRejectsNonEmptySource()
        {
            var buffer = default(BufferUnsafe<int>);

            Assert.DoesNotThrow(() => buffer.CopyFrom(ReadOnlySpan<int>.Empty));
            Assert.IsTrue(buffer.TryCopyFrom(ReadOnlySpan<int>.Empty));
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(new[] { 1 }));
            Assert.IsFalse(buffer.TryCopyFrom(new[] { 1 }));
        }

        [Test]
        public void CopyFrom_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsContent()
        {
            var buffer = new BufferUnsafe<int>(4, Allocator.Temp);

            try
            {
                buffer.CopyFrom(new[] { 1, 2, 3, 4 });
                var source = new[] { 9, 9 };

                Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(-1, source));
                Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(5, source));
                Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(3, source));
                Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(0, source, 3));
                Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(0, source, -1));
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void TryCopyFrom_InvalidRange_ReturnsFalseAndKeepsContent()
        {
            var buffer = new BufferUnsafe<int>(4, Allocator.Temp);

            try
            {
                buffer.CopyFrom(new[] { 1, 2, 3, 4 });
                var source = new[] { 9, 9 };

                Assert.IsFalse(buffer.TryCopyFrom(-1, source));
                Assert.IsFalse(buffer.TryCopyFrom(3, source));
                Assert.IsFalse(buffer.TryCopyFrom(0, source, 3));
                Assert.IsFalse(buffer.TryCopyFrom(0, source, -1));
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void CopyFrom_SourceOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            var buffer = new BufferUnsafe<int>(6, Allocator.Temp);

            try
            {
                buffer.CopyFrom(seed);
                buffer.CopyFrom(1, buffer.AsReadOnlySpan()[..4]);

                var expectedForward = (int[])seed.Clone();
                Array.Copy(expectedForward, 0, expectedForward, 1, 4);
                CollectionAssert.AreEqual(expectedForward, buffer.AsSpan().ToArray());

                buffer.CopyFrom(seed);
                buffer.CopyFrom(0, buffer.AsReadOnlySpan()[1..5]);

                var expectedBackward = (int[])seed.Clone();
                Array.Copy(expectedBackward, 1, expectedBackward, 0, 4);
                CollectionAssert.AreEqual(expectedBackward, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void CopyTo_EmptyDestination_CopiesNothing()
        {
            var buffer = new BufferUnsafe<int>(3, Allocator.Temp);

            try
            {
                buffer.CopyFrom(new[] { 1, 2, 3 });

                buffer.CopyTo(Span<int>.Empty);
                buffer.CopyTo(3, Span<int>.Empty);

                Assert.IsTrue(buffer.TryCopyTo(Span<int>.Empty));
                Assert.IsTrue(buffer.TryCopyTo(3, Span<int>.Empty));
                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void CopyTo_UncreatedBuffer_AcceptsEmptyAndRejectsNonEmptyDestination()
        {
            var buffer = default(BufferUnsafe<int>);

            Assert.DoesNotThrow(() => buffer.CopyTo(Span<int>.Empty));
            Assert.IsTrue(buffer.TryCopyTo(Span<int>.Empty));
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(new int[1]));
            Assert.IsFalse(buffer.TryCopyTo(new int[1]));
        }

        [Test]
        public void CopyTo_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            var buffer = new BufferUnsafe<int>(4, Allocator.Temp);

            try
            {
                buffer.CopyFrom(new[] { 1, 2, 3, 4 });
                var destination = new[] { 9, 9, 9, 9 };

                Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(-1, destination));
                Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(5, destination));
                Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(0, destination, 5));
                Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(0, new int[2], 3));
                Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(0, destination, -1));
                CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void TryCopyTo_InvalidRange_ReturnsFalseAndKeepsDestination()
        {
            var buffer = new BufferUnsafe<int>(4, Allocator.Temp);

            try
            {
                buffer.CopyFrom(new[] { 1, 2, 3, 4 });
                var destination = new[] { 9, 9, 9, 9 };

                Assert.IsFalse(buffer.TryCopyTo(-1, destination));
                Assert.IsFalse(buffer.TryCopyTo(0, destination, 5));
                Assert.IsFalse(buffer.TryCopyTo(0, new int[2], 3));
                Assert.IsFalse(buffer.TryCopyTo(0, destination, -1));
                CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void CopyTo_DestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            var buffer = new BufferUnsafe<int>(6, Allocator.Temp);

            try
            {
                buffer.CopyFrom(seed);
                buffer.CopyTo(2, buffer.AsSpan()[..4]);

                var expectedBackward = (int[])seed.Clone();
                Array.Copy(expectedBackward, 2, expectedBackward, 0, 4);
                CollectionAssert.AreEqual(expectedBackward, buffer.AsSpan().ToArray());

                buffer.CopyFrom(seed);
                buffer.CopyTo(0, buffer.AsSpan()[2..]);

                var expectedForward = (int[])seed.Clone();
                Array.Copy(expectedForward, 0, expectedForward, 2, 4);
                CollectionAssert.AreEqual(expectedForward, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void ReadOnly_Elements_RoundTripBitExactForEveryElementSize()
        {
            AssertReadOnlyElementRoundTrip<byte>(0xA5, 0xFF);
            AssertReadOnlyElementRoundTrip(new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 254, b = 253 });
            AssertReadOnlyElementRoundTrip(long.MinValue, long.MaxValue);
            AssertReadOnlyElementRoundTrip(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = long.MinValue, b = 0, c = -1 }
            );
        }

        [Test]
        public void ReadOnly_CopyTo_InvalidRange_ThrowsAndTryCopyToReturnsFalse()
        {
            var buffer = new BufferUnsafe<int>(4, Allocator.Temp);

            try
            {
                buffer.CopyFrom(new[] { 1, 2, 3, 4 });
                var readOnly = buffer.AsReadOnly();
                var destination = new[] { 9, 9, 9, 9 };

                Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(-1, destination));
                Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(5, destination));
                Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(0, destination, 5));
                Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(0, new int[2], 3));
                Assert.IsFalse(readOnly.TryCopyTo(-1, destination));
                Assert.IsFalse(readOnly.TryCopyTo(0, destination, 5));
                Assert.IsFalse(readOnly.TryCopyTo(0, new int[2], 3));
                Assert.IsFalse(readOnly.TryCopyTo(0, destination, -1));
                CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void ReadOnly_CopyTo_EmptyDestination_CopiesNothing()
        {
            var buffer = new BufferUnsafe<int>(3, Allocator.Temp);

            try
            {
                buffer.CopyFrom(new[] { 1, 2, 3 });
                var readOnly = buffer.AsReadOnly();

                readOnly.CopyTo(Span<int>.Empty);
                readOnly.CopyTo(3, Span<int>.Empty);

                Assert.IsTrue(readOnly.TryCopyTo(Span<int>.Empty));
                Assert.IsTrue(readOnly.TryCopyTo(3, Span<int>.Empty));
                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void ReadOnly_CopyTo_DestinationOverlapsOwnerStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            var buffer = new BufferUnsafe<int>(6, Allocator.Temp);

            try
            {
                buffer.CopyFrom(seed);
                var readOnly = buffer.AsReadOnly();

                readOnly.CopyTo(0, buffer.AsSpan()[2..]);

                var expected = (int[])seed.Clone();
                Array.Copy(expected, 0, expected, 2, 4);
                CollectionAssert.AreEqual(expected, buffer.AsSpan().ToArray());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void ReadOnly_Default_ReportsNoStorage()
        {
            var readOnly = default(BufferUnsafe<int>.ReadOnly);

            Assert.IsFalse(readOnly.IsCreated);
            Assert.AreEqual(0, readOnly.Capacity);
            Assert.AreEqual(0, readOnly.AsReadOnlySpan().Length);
            Assert.IsTrue(readOnly.TryCopyTo(Span<int>.Empty));
            Assert.IsFalse(readOnly.TryCopyTo(new int[1]));

            // SAFETY: The pointer of a default view is only compared with null and never dereferenced.
            unsafe
            {
                Assert.AreEqual(System.IntPtr.Zero, (System.IntPtr)readOnly.GetUnsafeReadOnlyPtr());
            }
        }

        [Test]
        public void ReadOnly_GetUnsafeReadOnlyPtr_AliasesTheOwnerStorage()
        {
            var buffer = new BufferUnsafe<int>(3, Allocator.Temp);

            try
            {
                var readOnly = buffer.AsReadOnly();

                // SAFETY: Both pointers address the live three-element owner storage and every access stays below 3.
                unsafe
                {
                    var writable = (int*)buffer.GetUnsafePtr();
                    var readable = (int*)readOnly.GetUnsafeReadOnlyPtr();

                    Assert.AreEqual((System.IntPtr)writable, (System.IntPtr)readable);

                    writable[2] = 77;

                    Assert.AreEqual(77, readable[2]);
                    Assert.AreEqual(77, readOnly[2]);
                    Assert.AreEqual(77, readOnly.AsReadOnlySpan()[2]);
                }
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Reinterpret_DifferentSize_Throws()
        {
            var buffer = new BufferUnsafe<int>(4, Allocator.Temp);

            try
            {
                Assert.Throws<InvalidOperationException>(() => buffer.Reinterpret<ushort>());
                Assert.Throws<InvalidOperationException>(() => buffer.Reinterpret<long>());
                Assert.Throws<InvalidOperationException>(() => buffer.AsReadOnly().Reinterpret<byte>());
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void Reinterpret_UncreatedBuffer_ReturnsAnUncreatedView()
        {
            var buffer = default(BufferUnsafe<int>);

            var view = buffer.Reinterpret<uint>();

            Assert.IsFalse(view.IsCreated);
            Assert.AreEqual(0, view.Capacity);
            Assert.AreEqual(0, view.AsSpan().Length);
        }

        [Test]
        public void Reinterpret_WriteThroughTheView_IsSeenByTheOwnerAndTheReverse()
        {
            var buffer = new BufferUnsafe<int>(3, Allocator.Temp);

            try
            {
                var view = buffer.Reinterpret<float>();
                var readOnlyView = buffer.AsReadOnly().Reinterpret<float>();

                view[1] = 1.5f;

                Assert.AreEqual(BitConverter.SingleToInt32Bits(1.5f), buffer[1]);
                Assert.AreEqual(1.5f, readOnlyView[1]);

                buffer[2] = BitConverter.SingleToInt32Bits(-2.25f);

                Assert.AreEqual(-2.25f, view[2]);
                Assert.AreEqual(-2.25f, view.AsReadOnlySpan()[2]);
                Assert.AreEqual(3, view.Capacity);
                Assert.AreEqual(3, readOnlyView.Capacity);
            }
            finally
            {
                // The view is never disposed: it shares the owner's allocation, so only the owner releases it.
                buffer.Dispose();
            }
        }

        [Test]
        public void Reinterpret_LongToDouble_RoundTripsBitExact()
        {
            var buffer = new BufferUnsafe<long>(2, Allocator.Temp);

            try
            {
                var view = buffer.Reinterpret<double>();

                view[0] = double.MaxValue;
                buffer[1] = BitConverter.DoubleToInt64Bits(double.Epsilon);

                Assert.AreEqual(BitConverter.DoubleToInt64Bits(double.MaxValue), buffer[0]);
                Assert.AreEqual(double.Epsilon, view[1]);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void Reinterpret_StructLargerThanSixteenBytes_AliasesEveryField()
        {
            var buffer = new BufferUnsafe<Wide>(2, Allocator.Temp);

            try
            {
                var view = buffer.Reinterpret<WideOther>();

                buffer[1] = new Wide { a = 1, b = -2, c = long.MaxValue };

                Assert.AreEqual(1L, view[1].x);
                Assert.AreEqual(-2L, view[1].y);
                Assert.AreEqual(long.MaxValue, view[1].z);

                view[0] = new WideOther { x = long.MinValue, y = 0, z = -1 };

                Assert.AreEqual(long.MinValue, buffer[0].a);
                Assert.AreEqual(0L, buffer[0].b);
                Assert.AreEqual(-1L, buffer[0].c);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void Reinterpret_ZeroCapacityBuffer_ReturnsAnEmptyView()
        {
            var buffer = default(BufferUnsafe<int>);

            try
            {
                buffer.Alloc(0, Allocator.Temp);

                var view = buffer.Reinterpret<uint>();
                var readOnlyView = buffer.AsReadOnly().Reinterpret<uint>();

                Assert.AreEqual(0, view.Capacity);
                Assert.AreEqual(0, view.AsSpan().Length);
                Assert.AreEqual(0, readOnlyView.Capacity);
                Assert.AreEqual(0, readOnlyView.AsReadOnlySpan().Length);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        [Category("Task045X2Regression")]
        public void Reinterpret_DisposeBorrowedView_LeavesOwnerWritableAndResizable(bool scheduled)
        {
            var owner = new BufferUnsafe<int>(2, Allocator.Persistent);
            var view = owner.Reinterpret<uint>();

            try
            {
                owner[0] = 17;
                owner[1] = 29;

                if (scheduled)
                {
                    view.Dispose(default(Unity.Jobs.JobHandle)).Complete();
                }
                else
                {
                    view.Dispose();
                }

                Assert.IsFalse(view.IsCreated);
                Assert.AreEqual(0, view.Capacity);
                CollectionAssert.AreEqual(new[] { 17, 29 }, owner.AsReadOnlySpan().ToArray());
                owner[1] = 31;
                owner.Resize(4);
                CollectionAssert.AreEqual(new[] { 17, 31, 0, 0 }, owner.AsReadOnlySpan().ToArray());
            }
            finally
            {
                view.Dispose();
                owner.Dispose();
            }
        }

        [Test]
        [Category("Task045X2Regression")]
        public void Reinterpret_ChangeBorrowedCapacity_RejectsWithoutChangingOwner()
        {
            var owner = new BufferUnsafe<int>(2, Allocator.Persistent);
            var view = owner.Reinterpret<uint>();

            try
            {
                owner[0] = 17;
                owner[1] = 29;
                Assert.DoesNotThrow(() => view.Resize(2));
                Assert.Throws<InvalidOperationException>(() => view.Resize(3));
                Assert.Throws<InvalidOperationException>(() => view.Resize(1));
                Assert.AreEqual(2, view.Capacity);
                Assert.AreEqual(2, owner.Capacity);
                CollectionAssert.AreEqual(new[] { 17, 29 }, owner.AsReadOnlySpan().ToArray());
                view[1] = 31;
                Assert.AreEqual(31, owner[1]);
            }
            finally
            {
                view.Dispose();
                owner.Dispose();
            }
        }

        private static void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var buffer = new BufferUnsafe<T>(3, Allocator.Temp);

            try
            {
                buffer[0] = a;
                buffer.AsSpan()[1] = b;
                buffer[2] = c;

                var span = buffer.AsReadOnlySpan();

                Assert.AreEqual(3, span.Length, name);
                Assert.AreEqual(a, span[0], name);
                Assert.AreEqual(b, span[1], name);
                Assert.AreEqual(c, span[2], name);

                var copy = new T[3];
                buffer.CopyTo(copy);

                CollectionAssert.AreEqual(new[] { a, b, c }, copy, name);

                buffer.Clear();
                buffer.CopyFrom(new[] { c, b, a });

                Assert.AreEqual(c, buffer[0], name);
                Assert.AreEqual(b, buffer[1], name);
                Assert.AreEqual(a, buffer[2], name);

                buffer.Resize(5);

                Assert.AreEqual(c, buffer[0], name);
                Assert.AreEqual(b, buffer[1], name);
                Assert.AreEqual(a, buffer[2], name);
                Assert.AreEqual(default(T), buffer[3], name);
                Assert.AreEqual(default(T), buffer[4], name);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        private static void AssertReadOnlyElementRoundTrip<T>(T first, T last)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var buffer = new BufferUnsafe<T>(3, Allocator.Temp);

            try
            {
                buffer[0] = first;
                buffer[2] = last;

                var readOnly = buffer.AsReadOnly();
                var span = readOnly.AsReadOnlySpan();

                Assert.AreEqual(first, readOnly[0], name);
                Assert.AreEqual(default(T), readOnly[1], name);
                Assert.AreEqual(last, readOnly[2], name);
                Assert.AreEqual(first, span[0], name);
                Assert.AreEqual(last, span[2], name);

                var copy = new T[3];
                readOnly.CopyTo(copy);

                CollectionAssert.AreEqual(new[] { first, default, last }, copy, name);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        private struct Rgb
        {
            public byte r;
            public byte g;
            public byte b;
        }

        private struct Wide
        {
            public long a;
            public long b;
            public long c;
        }

        private struct WideOther
        {
            public long x;
            public long y;
            public long z;
        }
    }
}
