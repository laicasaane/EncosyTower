using System;
using EncosyTower.Buffers;
using EncosyTower.Tests.Core.Collections;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Buffers
{
    // Adapted from Unity.Collections.Tests/NativeListTests.cs (fixed-capacity subset)
    // against the Native forwarder.
    public partial class BufferNativeTests
    {
        [Test]
        public void Constructor_CreatesValidBuffer()
        {
            using var buffer = new BufferNative<int>(8, Allocator.Temp);

            Assert.IsTrue(buffer.IsCreated);
            Assert.AreEqual(8, buffer.Capacity);
        }

        [Test]
        public void DefaultBuffer_IsNotCreated()
        {
            BufferNative<int> buffer = default;

            Assert.IsFalse(buffer.IsCreated);
        }

        [Test]
        public void Constructor_ClearsToZero()
        {
            using var buffer = new BufferNative<int>(4, Allocator.Temp);

            for (var i = 0; i < buffer.Capacity; i++)
            {
                Assert.AreEqual(0, buffer[i]);
            }
        }

        [Test]
        public void Indexer_GetSet()
        {
            using var buffer = new BufferNative<int>(4, Allocator.Temp);

            buffer[0] = 10;
            buffer[3] = 40;

            Assert.AreEqual(10, buffer[0]);
            Assert.AreEqual(40, buffer[3]);
        }

        [Test]
        public void Resize_Grow_PreservesContent()
        {
            using var buffer = new BufferNative<int>(4, Allocator.Temp);
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
            using var buffer = new BufferNative<int>(4, Allocator.Temp);
            buffer[0] = 1;
            buffer[1] = 2;

            buffer.Clear();

            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(0, buffer[1]);
        }

        [Test]
        public void CopyFrom_And_CopyTo_Span()
        {
            using var buffer = new BufferNative<int>(4, Allocator.Temp);

            buffer.CopyFrom(new[] { 5, 6, 7, 8 });

            var dst = new int[4];
            buffer.CopyTo(dst);

            CollectionAssert.AreEqual(new[] { 5, 6, 7, 8 }, dst);
        }

        [Test]
        public void AsSpan_ReflectsContent()
        {
            using var buffer = new BufferNative<int>(4, Allocator.Temp);
            buffer[0] = 11;
            buffer[1] = 22;

            var span = buffer.AsSpan();

            Assert.AreEqual(4, span.Length);
            Assert.AreEqual(11, span[0]);
            Assert.AreEqual(22, span[1]);
        }

        [Test]
        public void AsNativeSlice_ReflectsContent()
        {
            using var buffer = new BufferNative<int>(4, Allocator.Temp);
            buffer[0] = 11;
            buffer[1] = 22;

            var slice = buffer.AsNativeSlice();

            Assert.AreEqual(4, slice.Length);
            Assert.AreEqual(11, slice[0]);
            Assert.AreEqual(22, slice[1]);
        }

        [Test]
        public void Reinterpret_ReusesMemory()
        {
            using var buffer = new BufferNative<int>(4, Allocator.Temp);
            buffer[0] = 7;

            var reinterpreted = buffer.Reinterpret<uint>();

            Assert.AreEqual(buffer.Capacity, reinterpreted.Capacity);
            Assert.AreEqual(7u, reinterpreted[0]);
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var buffer = new BufferNative<int>(4, Allocator.Temp);
            Assert.IsTrue(buffer.IsCreated);

            buffer.Dispose();

            Assert.IsFalse(buffer.IsCreated);
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var buffer = new BufferNative<int>(4, Allocator.TempJob);
            buffer[0] = 1;

            var handle = buffer.Dispose(default);

            Assert.IsFalse(buffer.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void DoubleDispose_Throws()
        {
            // Matches Unity NativeArray/NativeList semantics: a second Dispose with
            // safety checks enabled throws (the handle was released, not zeroed).
            var buffer = new BufferNative<int>(4, Allocator.Temp);
            buffer.Dispose();

            Assert.Throws<ObjectDisposedException>(() => buffer.Dispose());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void UseAfterDispose_Throws()
        {
            var buffer = new BufferNative<int>(4, Allocator.Temp);
            buffer[0] = 1;
            buffer.Dispose();

            Assert.Catch(() => _ = buffer[0]);
        }

        [Test]
        public void Alloc_CreatesDefaultBufferAndClearsStorage()
        {
            BufferNative<int> buffer = default;

            try
            {
                buffer.Alloc(3, new AllocatorStrategy(Allocator.Temp), memClear: true);

                Assert.IsTrue(buffer.IsCreated);
                Assert.AreEqual(3, buffer.Capacity);
                CollectionAssert.AreEqual(new[] { 0, 0, 0 }, buffer.AsSpan().ToArray());
            }
            finally
            {
                if (buffer.IsCreated)
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void Resize_RemainingOverloadsPreserveOrDiscardContent()
        {
            using var buffer = new BufferNative<int>(3, Allocator.Temp);
            buffer.CopyFrom(new[] { 1, 2, 3 });

            buffer.Resize(5);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 0, 0 }, buffer.AsSpan().ToArray());

            buffer.Resize(4, copyContent: false, memClear: true);
            CollectionAssert.AreEqual(new[] { 0, 0, 0, 0 }, buffer.AsSpan().ToArray());
        }

        [Test]
        public void FastClear_DoesNotWriteUnmanagedStorage()
        {
            using var buffer = new BufferNative<int>(3, Allocator.Temp);
            buffer.CopyFrom(new[] { 1, 2, 3 });

            buffer.FastClear();

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, buffer.AsSpan().ToArray());
        }

        [Test]
        public void CopyFromAndTryCopyFrom_AllOverloadsUseRequestedWindow()
        {
            using var buffer = new BufferNative<int>(6, Allocator.Temp);
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
            using var buffer = new BufferNative<int>(4, Allocator.Temp);
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
        public void ReadOnlyViews_ReflectOwnerWrites()
        {
            using var buffer = new BufferNative<int>(4, Allocator.Temp);
            buffer.CopyFrom(new[] { 1, 2, 3, 4 });

            var slice = buffer.AsNativeSliceReadOnly();
            var span = buffer.AsReadOnlySpan();
            var readOnly = buffer.AsReadOnly();
            BufferNative<int>.ReadOnly converted = buffer;

            buffer[0] = 10;
            Assert.AreEqual(10, slice[0]);
            Assert.AreEqual(10, span[0]);
            Assert.AreEqual(10, readOnly[0]);
            Assert.AreEqual(10, converted[0]);
        }

        [Test]
        public void ReadOnly_AllCopyViewAndReinterpretMembersWork()
        {
            using var buffer = new BufferNative<int>(4, Allocator.Temp);
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

            Assert.AreEqual(4, readOnly.AsNativeSliceReadOnly().Length);
            Assert.AreEqual(4, readOnly.AsReadOnlySpan().Length);
            Assert.AreEqual(1u, readOnly.Reinterpret<uint>()[0]);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ReadOnly_AfterOwnerDispose_RejectsAccess()
        {
            var buffer = new BufferNative<int>(1, Allocator.Temp);
            var readOnly = buffer.AsReadOnly();
            buffer.Dispose();

            Assert.Catch(() => _ = readOnly[0]);
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_CreatesAndDisposes(Allocator allocator)
        {
            var buffer = new BufferNative<int>(4, allocator);

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
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_JobHandle_AcceptedAllocator_ReleasesTheStorage(Allocator allocator)
        {
            var buffer = new BufferNative<int>(4, allocator);
            buffer[0] = 1;

            var handle = buffer.Dispose(default);

            Assert.IsFalse(buffer.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void Dispose_DefaultBuffer_IsNoOp()
        {
            var buffer = default(BufferNative<int>);

            Assert.DoesNotThrow(() => buffer.Dispose());
            Assert.IsFalse(buffer.IsCreated);
        }

        [Test]
        public void Dispose_JobHandle_DefaultBuffer_ReturnsWithoutScheduling()
        {
            var buffer = default(BufferNative<int>);

            var handle = buffer.Dispose(default);

            Assert.DoesNotThrow(() => handle.Complete());
            Assert.IsFalse(buffer.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Constructor_DefaultAllocatorStrategy_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _ = new BufferNative<int>(4, default(AllocatorStrategy)));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Alloc_DefaultAllocatorStrategy_Throws()
        {
            var buffer = default(BufferNative<int>);

            Assert.Throws<InvalidOperationException>(() => buffer.Alloc(4, default(AllocatorStrategy)));
            Assert.IsFalse(buffer.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Alloc_AlreadyAllocated_Throws()
        {
            var buffer = new BufferNative<int>(2, Allocator.Persistent);

            try
            {
                Assert.Throws<InvalidOperationException>(() => buffer.Alloc(4, Allocator.Persistent));
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
            var buffer = new BufferNative<int>(2, Allocator.Persistent);
            buffer.Dispose();

            try
            {
                buffer.Alloc(3, Allocator.Persistent);

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
            var buffer = new BufferNative<int>(0, Allocator.Persistent);

            try
            {
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
        public void Resize_UncreatedBuffer_Throws()
        {
            var buffer = default(BufferNative<int>);

            Assert.Throws<InvalidOperationException>(() => buffer.Resize(4));
            Assert.Throws<InvalidOperationException>(() => buffer.Resize(4, copyContent: false));
            Assert.Throws<InvalidOperationException>(() => buffer.Resize(4, copyContent: false, memClear: false));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Resize_AfterDispose_Throws()
        {
            var buffer = new BufferNative<int>(4, Allocator.Persistent);
            buffer.Dispose();

            Assert.Throws<InvalidOperationException>(() => buffer.Resize(8));
        }

        [Test]
        public void Resize_SameCapacity_KeepsContent()
        {
            var buffer = new BufferNative<int>(3, Allocator.Persistent);

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
            var buffer = new BufferNative<int>(3, Allocator.Persistent);

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
            var buffer = new BufferNative<int>(5, Allocator.Persistent);

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
        public void AsReadOnly_UncreatedOrDisposedBuffer_Throws()
        {
            var uncreated = default(BufferNative<int>);
            var disposed = new BufferNative<int>(2, Allocator.Persistent);
            disposed.Dispose();

            Assert.Throws<ArgumentException>(() => uncreated.AsReadOnly());
            Assert.Throws<ArgumentException>(() => _ = (BufferNative<int>.ReadOnly)uncreated);
            Assert.Throws<ArgumentException>(() => disposed.AsReadOnly());
        }

        [Test]
        public void FastClear_UncreatedBuffer_DoesNothing()
        {
            var buffer = default(BufferNative<int>);

            Assert.DoesNotThrow(() => buffer.FastClear());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AfterDispose_MembersThatCheckTheSafetyHandle_Throw()
        {
            var buffer = new BufferNative<int>(4, Allocator.Persistent);
            buffer.Dispose();

            Assert.Catch(() => _ = buffer.Capacity);
            Assert.Catch(() => _ = buffer.AsSpan());
            Assert.Catch(() => _ = buffer.AsReadOnlySpan());
            Assert.Catch(() => buffer.Clear());
            Assert.Catch(() => buffer.CopyFrom(new[] { 1 }));
            Assert.Catch(() => buffer.TryCopyFrom(new[] { 1 }));
            Assert.Catch(() => buffer.CopyTo(new int[1]));
            Assert.Catch(() => buffer.TryCopyTo(new int[1]));
        }

        [Test]
        public void CopyFrom_EmptySource_LeavesTheBufferUnchanged()
        {
            var buffer = new BufferNative<int>(3, Allocator.Persistent);

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
        public void CopyFrom_ZeroCapacityBuffer_AcceptsEmptyAndRejectsNonEmptySource()
        {
            var buffer = new BufferNative<int>(0, Allocator.Persistent);

            try
            {
                Assert.DoesNotThrow(() => buffer.CopyFrom(ReadOnlySpan<int>.Empty));
                Assert.IsTrue(buffer.TryCopyFrom(ReadOnlySpan<int>.Empty));
                Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(new[] { 1 }));
                Assert.IsFalse(buffer.TryCopyFrom(new[] { 1 }));
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void CopyFrom_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsContent()
        {
            var buffer = new BufferNative<int>(4, Allocator.Persistent);

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
            var buffer = new BufferNative<int>(4, Allocator.Persistent);

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
            var buffer = new BufferNative<int>(6, Allocator.Persistent);

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
            var buffer = new BufferNative<int>(3, Allocator.Persistent);

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
        public void CopyTo_ZeroCapacityBuffer_AcceptsEmptyAndRejectsNonEmptyDestination()
        {
            var buffer = new BufferNative<int>(0, Allocator.Persistent);

            try
            {
                Assert.DoesNotThrow(() => buffer.CopyTo(Span<int>.Empty));
                Assert.IsTrue(buffer.TryCopyTo(Span<int>.Empty));
                Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(new int[1]));
                Assert.IsFalse(buffer.TryCopyTo(new int[1]));
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void CopyTo_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            var buffer = new BufferNative<int>(4, Allocator.Persistent);

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
            var buffer = new BufferNative<int>(4, Allocator.Persistent);

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
            var buffer = new BufferNative<int>(6, Allocator.Persistent);

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
        public void AsNativeArray_AliasesTheOwnerStorageInBothDirections()
        {
            var buffer = new BufferNative<int>(3, Allocator.Persistent);

            try
            {
                var array = buffer.AsNativeArray();

                array[1] = 5;
                buffer[2] = 7;

                Assert.AreEqual(3, array.Length);
                Assert.AreEqual(5, buffer[1]);
                Assert.AreEqual(7, array[2]);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void AsNativeSlice_AliasesTheOwnerStorageInBothDirections()
        {
            var buffer = new BufferNative<int>(3, Allocator.Persistent);

            try
            {
                var slice = buffer.AsNativeSlice();
                var readOnlySlice = buffer.AsNativeSliceReadOnly();

                slice[1] = 5;
                buffer[2] = 7;

                Assert.AreEqual(3, slice.Length);
                Assert.AreEqual(3, readOnlySlice.Length);
                Assert.AreEqual(5, buffer[1]);
                Assert.AreEqual(5, readOnlySlice[1]);
                Assert.AreEqual(7, slice[2]);
                Assert.AreEqual(7, readOnlySlice[2]);
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
            var buffer = new BufferNative<int>(4, Allocator.Persistent);

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
            var buffer = new BufferNative<int>(3, Allocator.Persistent);

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
            var buffer = new BufferNative<int>(6, Allocator.Persistent);

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
        public void ReadOnly_ZeroCapacity_ReportsNoElements()
        {
            var buffer = new BufferNative<int>(0, Allocator.Persistent);

            try
            {
                var readOnly = buffer.AsReadOnly();

                Assert.IsTrue(readOnly.IsCreated);
                Assert.AreEqual(0, readOnly.Capacity);
                Assert.AreEqual(0, readOnly.AsReadOnlySpan().Length);
                Assert.IsTrue(readOnly.TryCopyTo(Span<int>.Empty));
                Assert.IsFalse(readOnly.TryCopyTo(new int[1]));
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void ReadOnly_AsNativeArray_AliasesTheOwnerStorage()
        {
            var buffer = new BufferNative<int>(3, Allocator.Persistent);

            try
            {
                var readOnly = buffer.AsReadOnly();
                var array = readOnly.AsNativeArray();

                buffer[1] = 5;

                Assert.AreEqual(3, array.Length);
                Assert.AreEqual(5, array[1]);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void ReadOnly_Default_ReportsNoStorage()
        {
            var readOnly = default(BufferNative<int>.ReadOnly);

            Assert.IsFalse(readOnly.IsCreated);
            Assert.AreEqual(0, readOnly.Capacity);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ReadOnly_AfterOwnerDispose_MembersThatCheckTheSafetyHandleThrow()
        {
            var buffer = new BufferNative<int>(4, Allocator.Persistent);
            var readOnly = buffer.AsReadOnly();
            buffer.Dispose();

            Assert.Catch(() => _ = readOnly.AsReadOnlySpan());
            Assert.Catch(() => readOnly.CopyTo(new int[1]));
            Assert.Catch(() => readOnly.TryCopyTo(new int[1]));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Reinterpret_DifferentSize_Throws()
        {
            var buffer = new BufferNative<int>(4, Allocator.Persistent);

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
        public void Reinterpret_WriteThroughTheView_IsSeenByTheOwnerAndTheReverse()
        {
            var buffer = new BufferNative<int>(3, Allocator.Persistent);

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
                // The view is never disposed: it shares the owner's header, so only the owner releases it.
                buffer.Dispose();
            }
        }

        [Test]
        public void Reinterpret_StructLargerThanSixteenBytes_AliasesEveryField()
        {
            var buffer = new BufferNative<Wide>(2, Allocator.Persistent);

            try
            {
                var view = buffer.Reinterpret<WideOther>();

                buffer[1] = new Wide { a = 1, b = -2, c = long.MaxValue };

                Assert.AreEqual(1L, view[1].x);
                Assert.AreEqual(-2L, view[1].y);
                Assert.AreEqual(long.MaxValue, view[1].z);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        [Test]
        public void Reinterpret_ZeroCapacityBuffer_ReturnsAnEmptyView()
        {
            var buffer = new BufferNative<int>(0, Allocator.Persistent);

            try
            {
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

        [Test]
        [TestRequiresCollectionChecks]
        public void Reinterpret_AfterOwnerDispose_ViewRejectsAccess()
        {
            var buffer = new BufferNative<int>(4, Allocator.Persistent);
            var view = buffer.Reinterpret<uint>();
            buffer.Dispose();

            Assert.Catch(() => _ = view[0]);
            Assert.Catch(() => _ = view.AsReadOnlySpan());
        }

        [Test]
        public void DisposeJob_DefaultData_DoesNothing()
        {
            var job = default(BufferNativeDisposeJob);

            Assert.DoesNotThrow(() => job.Execute());
        }

        [Test]
        public void BufferNativeDispose_NullHeader_DoesNothing()
        {
            // SAFETY: A null header is documented to be ignored, so no memory is read or released.
            unsafe
            {
                Assert.DoesNotThrow(() => BufferNativeDispose.Dispose<int>(null));
            }
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void BufferNativeDispose_Header_ReleasesTheStorageAndTheHeader(Allocator allocator)
        {
            var strategy = new AllocatorStrategy(allocator);

            // SAFETY: The header is allocated by the same strategy, owns one live buffer, and is released exactly once
            // by BufferNativeDispose.Dispose, so no pointer is used afterwards.
            unsafe
            {
                var data = strategy.Allocate<BufferUnsafe<int>>();
                *data = new BufferUnsafe<int>(4, strategy);
                data->CopyFrom(new[] { 1, 2, 3, 4 });

                BufferNativeDispose.Dispose(data);
            }
        }

        private static void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var buffer = new BufferNative<T>(3, Allocator.Persistent);

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

                var slice = buffer.AsNativeSlice();

                Assert.AreEqual(a, slice[0], name);
                Assert.AreEqual(b, slice[1], name);
                Assert.AreEqual(c, slice[2], name);
                Assert.AreEqual(b, buffer.AsNativeSliceReadOnly()[1], name);

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
            var buffer = new BufferNative<T>(3, Allocator.Persistent);

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
                Assert.AreEqual(last, readOnly.AsNativeSliceReadOnly()[2], name);

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
