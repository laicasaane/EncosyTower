using System;
using EncosyTower.Buffers;
using EncosyTower.Tests.Core.Collections;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Buffers
{
    public partial class BufferSharedTests
    {
        [Test]
        public void Constructors_CreateDefaultZeroLengthAndAdoptExactArray()
        {
            BufferShared<int> uncreated = default;
            var zero = new BufferShared<int>(0);
            var source = new[] { 1, 2, 3 };
            var adopted = new BufferShared<int>(source);

            try
            {
                Assert.IsFalse(uncreated.IsCreated);
                Assert.IsTrue(zero.IsCreated);
                Assert.AreEqual(0, zero.Capacity);
                Assert.AreSame(source, adopted.AsManagedArray());

                source[1] = 20;

                // SAFETY: adopted owns the pinned array throughout this bounded index read.
                unsafe
                {
                    Assert.AreEqual(20, adopted[1]);
                }
            }
            finally
            {
                // SAFETY: These local buffers are the designated owners and have no surviving borrowers.
                unsafe
                {
                    zero.Dispose();
                    adopted.Dispose();
                }
            }
        }

        [Test]
        public void Alloc_CreatesManagedStorageAndIgnoresNativeAllocator()
        {
            var buffer = default(BufferShared<int>);

            try
            {
                buffer.Alloc(3, new AllocatorStrategy(Allocator.Temp), memClear: false);

                Assert.IsTrue(buffer.IsCreated);
                Assert.AreEqual(3, buffer.Capacity);
                // SAFETY: buffer owns the pinned storage while the span is copied immediately.
                unsafe
                {
                    CollectionAssert.AreEqual(new[] { 0, 0, 0 }, buffer.AsSpan().ToArray());
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void Resize_UsesPinnedPointersForCopyAndDiscardModes()
        {
            var buffer = new BufferShared<int>(new[] { 1, 2, 3 });

            try
            {
                // SAFETY: buffer is the sole owner; each span is consumed before the next resize.
                unsafe
                {
                    buffer.Resize(5, copyContent: true, memClear: false);
                    CollectionAssert.AreEqual(new[] { 1, 2, 3, 0, 0 }, buffer.AsSpan().ToArray());

                    buffer.Resize(2, copyContent: false, memClear: false);
                    CollectionAssert.AreEqual(new[] { 0, 0 }, buffer.AsSpan().ToArray());
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void ClearCopiesAndSpans_UsePinnedStorage()
        {
            var buffer = new BufferShared<int>(new[] { 1, 2, 3, 4 });

            try
            {
                // SAFETY: buffer owns the pinned storage and every borrowed span is consumed synchronously.
                unsafe
                {
                    buffer.CopyFrom(1, new[] { 8, 9 }, 2);
                    CollectionAssert.AreEqual(new[] { 1, 8, 9, 4 }, buffer.AsReadOnlySpan().ToArray());

                    var destination = new int[2];
                    buffer.CopyTo(1, destination, 2);
                    CollectionAssert.AreEqual(new[] { 8, 9 }, destination);

                    buffer.AsSpan()[0] = 7;
                    Assert.AreEqual(7, buffer.AsReadOnlySpan()[0]);

                    buffer.Clear();
                    CollectionAssert.AreEqual(new[] { 0, 0, 0, 0 }, buffer.AsReadOnlySpan().ToArray());
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void ManagedViews_AliasAdoptedArray()
        {
            var source = new[] { 1, 2, 3 };
            var buffer = new BufferShared<int>(source);

            try
            {
                var segment = buffer.AsArraySegment();
                var memory = buffer.AsMemory();
                var readOnlyMemory = buffer.AsReadOnlyMemory();

                segment.Array[1] = 20;
                memory.Span[2] = 30;

                Assert.AreSame(source, buffer.AsManagedArray());
                Assert.AreEqual(20, source[1]);
                Assert.AreEqual(30, readOnlyMemory.Span[2]);
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void NativeAliases_ExposePinnedStorageForBothGenericForms()
        {
            var direct = new BufferShared<int>(new[] { 1, 2, 3 });
            var aliased = new BufferShared<int, uint>(new[] { 4, 5, 6 });

            try
            {
                // SAFETY: Both local buffers own their pinned storage for every alias and index access below.
                unsafe
                {
                    var directNative = direct.AsNativeArray();
                    var aliasedNative = aliased.AsNativeArray();

                    directNative[1] = 20;
                    aliasedNative[2] = 30;

                    Assert.AreEqual(20, direct[1]);
                    Assert.AreEqual(30, aliased[2]);
                }
            }
            finally
            {
                // SAFETY: Both local buffers are designated owners and no aliases survive this test.
                unsafe
                {
                    direct.Dispose();
                    aliased.Dispose();
                }
            }
        }

        [Test]
        public void Reinterpret_EqualSizeAliasesStorageAndDifferentSizeThrows()
        {
            var buffer = new BufferShared<int>(new[] { 1, 2, 3 });
            var nativeBuffer = new BufferShared<int, uint>(new[] { 4, 5, 6 });

            try
            {
                // SAFETY: Both owners remain live; every equal-size alias is used before owner disposal.
                unsafe
                {
                    BufferShared<int, uint> alias = buffer.Reinterpret<uint>();
                    BufferShared<int, float> nativeAlias = nativeBuffer.Reinterpret<float>();
                    var nativeView = alias.AsNativeArray();
                    var nativeAliasView = nativeAlias.AsNativeArray();
                    nativeView[0] = 42;
                    nativeAliasView[1] = BitConverter.Int32BitsToSingle(9);

                    Assert.AreEqual(42, buffer[0]);
                    Assert.AreEqual(9, nativeBuffer[1]);
                    Assert.AreEqual(42u, buffer.AsReadOnly().Reinterpret<uint>().AsNativeSliceReadOnly()[0]);
                    Assert.Throws<InvalidOperationException>(() => buffer.Reinterpret<ushort>());
                }
            }
            finally
            {
                // SAFETY: Both local buffers are designated owners and no aliases survive this test.
                unsafe
                {
                    buffer.Dispose();
                    nativeBuffer.Dispose();
                }
            }
        }

        [Test]
        public void Dispose_SameValueTwiceClearsState()
        {
            var buffer = new BufferShared<int>(3);

            // SAFETY: buffer is the designated owner and this test intentionally repeats owner disposal.
            unsafe
            {
                Assert.DoesNotThrow(() => buffer.Dispose());
                Assert.DoesNotThrow(() => buffer.Dispose());
            }
            Assert.IsFalse(buffer.IsCreated);
            Assert.AreEqual(0, buffer.Capacity);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void OwnerAndReadOnlyIndexer_OutOfRangeThrow()
        {
            var buffer = new BufferShared<int>(1);

            try
            {
                // SAFETY: buffer remains live while its read-only alias and both checked indexers are exercised.
                unsafe
                {
                    var readOnly = buffer.AsReadOnly();

                    Assert.Throws<IndexOutOfRangeException>(() => _ = buffer[1]);
                    Assert.Throws<IndexOutOfRangeException>(() => _ = readOnly[1]);
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void UncreatedBuffer_Members_ThrowArgumentException()
        {
            var buffer = default(BufferShared<int>);

            Assert.IsFalse(buffer.IsCreated);
            Assert.AreEqual(0, buffer.Capacity);

            // SAFETY: An uncreated buffer pins no storage; every call below is rejected before any pointer is read.
            unsafe
            {
                Assert.Throws<ArgumentException>(() => _ = buffer[0]);
                Assert.Throws<ArgumentException>(() => buffer.Clear());
                Assert.Throws<ArgumentException>(() => buffer.AsSpan());
                Assert.Throws<ArgumentException>(() => buffer.AsReadOnlySpan());
                Assert.Throws<ArgumentException>(() => buffer.AsMemory());
                Assert.Throws<ArgumentException>(() => buffer.AsReadOnlyMemory());
                Assert.Throws<ArgumentException>(() => buffer.AsArraySegment());
                Assert.Throws<ArgumentException>(() => buffer.AsNativeArray());
                Assert.Throws<ArgumentException>(() => buffer.AsNativeSlice());
                Assert.Throws<ArgumentException>(() => buffer.AsNativeSliceReadOnly());
                Assert.Throws<ArgumentException>(() => buffer.AsReadOnly());
                Assert.Throws<ArgumentException>(() => buffer.Reinterpret<uint>());
                Assert.Throws<ArgumentException>(() => buffer.GetUnsafeBufferPointer());
                Assert.Throws<ArgumentException>(() => buffer.CopyFrom(new[] { 1 }));
                Assert.Throws<ArgumentException>(() => buffer.CopyTo(new int[1]));
                Assert.Throws<ArgumentException>(() => buffer.TryCopyFrom(new[] { 1 }));
                Assert.Throws<ArgumentException>(() => buffer.TryCopyTo(new int[1]));
            }
        }

        [Test]
        public void UncreatedAliasBuffer_Members_ThrowArgumentException()
        {
            var buffer = default(BufferShared<int, uint>);

            Assert.IsFalse(buffer.IsCreated);
            Assert.AreEqual(0, buffer.Capacity);

            // SAFETY: An uncreated buffer pins no storage; every call below is rejected before any pointer is read.
            unsafe
            {
                Assert.Throws<ArgumentException>(() => _ = buffer[0]);
                Assert.Throws<ArgumentException>(() => buffer.Clear());
                Assert.Throws<ArgumentException>(() => buffer.AsSpan());
                Assert.Throws<ArgumentException>(() => buffer.AsReadOnlySpan());
                Assert.Throws<ArgumentException>(() => buffer.AsMemory());
                Assert.Throws<ArgumentException>(() => buffer.AsReadOnlyMemory());
                Assert.Throws<ArgumentException>(() => buffer.AsArraySegment());
                Assert.Throws<ArgumentException>(() => buffer.AsNativeArray());
                Assert.Throws<ArgumentException>(() => buffer.AsNativeSlice());
                Assert.Throws<ArgumentException>(() => buffer.AsNativeSliceReadOnly());
                Assert.Throws<ArgumentException>(() => buffer.AsReadOnly());
                Assert.Throws<ArgumentException>(() => buffer.Reinterpret<float>());
                Assert.Throws<ArgumentException>(() => buffer.GetUnsafeBufferPointer());
            }
        }

        [Test]
        public void UncreatedBuffer_DisposeAndFastClear_DoNothing()
        {
            var buffer = default(BufferShared<int>);

            // SAFETY: Dispose of an uncreated buffer releases nothing.
            unsafe
            {
                Assert.DoesNotThrow(() => buffer.FastClear());
                Assert.DoesNotThrow(() => buffer.Dispose());
            }

            Assert.IsFalse(buffer.IsCreated);
        }

        [Test]
        public void UncreatedReadOnly_Members_ThrowArgumentException()
        {
            var readOnly = default(BufferShared<int>.ReadOnly);

            Assert.IsFalse(readOnly.IsCreated);
            Assert.AreEqual(0, readOnly.Capacity);

            // SAFETY: An uncreated view pins no storage; every call below is rejected before any pointer is read.
            unsafe
            {
                Assert.Throws<ArgumentException>(() => _ = readOnly[0]);
                Assert.Throws<ArgumentException>(() => readOnly.AsReadOnlySpan());
                Assert.Throws<ArgumentException>(() => readOnly.AsReadOnlyMemory());
                Assert.Throws<ArgumentException>(() => readOnly.AsNativeSliceReadOnly());
                Assert.Throws<ArgumentException>(() => readOnly.Reinterpret<uint>());
                Assert.Throws<ArgumentException>(() => readOnly.CopyTo(new int[1]));
                Assert.Throws<ArgumentException>(() => readOnly.TryCopyTo(new int[1]));
            }
        }

        [Test]
        public void AsReadOnly_DisposedBuffer_ThrowsArgumentException()
        {
            var buffer = new BufferShared<int>(2);

            // SAFETY: buffer is the designated owner and has no surviving borrower.
            unsafe
            {
                buffer.Dispose();

                Assert.Throws<ArgumentException>(() => buffer.AsReadOnly());
                Assert.Throws<ArgumentException>(() => buffer.AsSpan());
                Assert.Throws<ArgumentException>(() => _ = buffer[0]);
            }
        }

        [Test]
        public void ZeroCapacity_Buffer_ReportsNoElementsAndCanGrow()
        {
            var buffer = new BufferShared<int>(0);

            try
            {
                // SAFETY: buffer owns the pinned empty storage; each view is consumed before the resize.
                unsafe
                {
                    Assert.IsTrue(buffer.IsCreated);
                    Assert.AreEqual(0, buffer.Capacity);
                    Assert.AreEqual(0, buffer.AsSpan().Length);
                    Assert.AreEqual(0, buffer.AsReadOnlySpan().Length);
                    Assert.AreEqual(0, buffer.AsMemory().Length);
                    Assert.AreEqual(0, buffer.AsReadOnlyMemory().Length);
                    Assert.AreEqual(0, buffer.AsArraySegment().Count);
                    Assert.AreEqual(0, buffer.AsReadOnly().Capacity);
                    Assert.AreEqual(0, buffer.AsReadOnly().AsReadOnlySpan().Length);
                    Assert.AreEqual(0, buffer.AsReadOnly().AsReadOnlyMemory().Length);
                    Assert.DoesNotThrow(() => buffer.Clear());

                    buffer.CopyFrom(ReadOnlySpan<int>.Empty);
                    buffer.CopyTo(Span<int>.Empty);

                    Assert.IsTrue(buffer.TryCopyFrom(ReadOnlySpan<int>.Empty));
                    Assert.IsTrue(buffer.TryCopyTo(Span<int>.Empty));
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(new[] { 1 }));
                    Assert.IsFalse(buffer.TryCopyFrom(new[] { 1 }));
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(new int[1]));
                    Assert.IsFalse(buffer.TryCopyTo(new int[1]));

                    buffer.Resize(3);

                    Assert.AreEqual(3, buffer.Capacity);
                    CollectionAssert.AreEqual(new[] { 0, 0, 0 }, buffer.AsSpan().ToArray());
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Alloc_AlreadyAllocated_Throws()
        {
            var buffer = new BufferShared<int>(2);

            try
            {
                Assert.Throws<InvalidOperationException>(() => buffer.Alloc(4));
                Assert.Throws<InvalidOperationException>(() => buffer.Alloc(4, new AllocatorStrategy(Allocator.Temp)));
                Assert.AreEqual(2, buffer.Capacity);
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Alloc_NegativeSize_ThrowsAndLeavesTheBufferUncreated()
        {
            var buffer = default(BufferShared<int>);

            Assert.Throws<InvalidOperationException>(() => buffer.Alloc(-1));
            Assert.Throws<InvalidOperationException>(() => _ = new BufferShared<int>(-1));
            Assert.Throws<InvalidOperationException>(() => _ = new BufferShared<int, uint>(-1));
            Assert.IsFalse(buffer.IsCreated);
        }

        [Test]
        public void Alloc_AfterDispose_AllocatesAgain()
        {
            var buffer = new BufferShared<int>(2);

            // SAFETY: buffer is the designated owner and has no surviving borrower when it is disposed.
            unsafe
            {
                buffer.Dispose();
            }

            Assert.IsFalse(buffer.IsCreated);

            try
            {
                buffer.Alloc(3);

                Assert.IsTrue(buffer.IsCreated);
                Assert.AreEqual(3, buffer.Capacity);
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Alloc_AcceptedAllocator_CreatesManagedStorageAndDisposes(Allocator allocator)
        {
            var buffer = default(BufferShared<int>);

            try
            {
                buffer.Alloc(3, new AllocatorStrategy(allocator), memClear: true);

                Assert.IsTrue(buffer.IsCreated);
                Assert.AreEqual(3, buffer.Capacity);
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }

            Assert.IsFalse(buffer.IsCreated);
            Assert.AreEqual(0, buffer.Capacity);
        }

        [Test]
        public void Alloc_DefaultAllocatorStrategy_CreatesManagedStorage()
        {
            var buffer = default(BufferShared<int>);

            try
            {
                buffer.Alloc(2, default, memClear: true);

                Assert.IsTrue(buffer.IsCreated);
                Assert.AreEqual(2, buffer.Capacity);
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void Alloc_SizeOverload_CreatesZeroedStorageAndZeroSizeIsEmpty()
        {
            var buffer = default(BufferShared<int>);
            var empty = default(BufferShared<int>);

            try
            {
                buffer.Alloc(3);
                empty.Alloc(0);

                Assert.AreEqual(0, empty.Capacity);
                Assert.IsTrue(empty.IsCreated);

                // SAFETY: buffer owns the pinned storage and the span is consumed immediately.
                unsafe
                {
                    CollectionAssert.AreEqual(new[] { 0, 0, 0 }, buffer.AsSpan().ToArray());
                }
            }
            finally
            {
                // SAFETY: both buffers are designated owners and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                    empty.Dispose();
                }
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Resize_UncreatedBuffer_Throws()
        {
            var buffer = default(BufferShared<int>);

            // SAFETY: An uncreated buffer has no storage; Resize rejects it before any copy.
            unsafe
            {
                Assert.Throws<InvalidOperationException>(() => buffer.Resize(4));
                Assert.Throws<InvalidOperationException>(() => buffer.Resize(4, copyContent: false));
                Assert.Throws<InvalidOperationException>(() => buffer.Resize(4, copyContent: false, memClear: false));
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Resize_NegativeSize_ThrowsAndKeepsContent()
        {
            var buffer = new BufferShared<int>(new[] { 1, 2, 3 });

            try
            {
                // SAFETY: buffer is the sole owner; Resize rejects the negative size before any copy.
                unsafe
                {
                    Assert.Throws<InvalidOperationException>(() => buffer.Resize(-1));
                    Assert.AreEqual(3, buffer.Capacity);
                    CollectionAssert.AreEqual(new[] { 1, 2, 3 }, buffer.AsReadOnlySpan().ToArray());
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void Resize_FullBufferGrowsAndShrinks_KeepsTheLeadingElements()
        {
            var buffer = new BufferShared<int>(new[] { 1, 2, 3, 4 });

            try
            {
                // SAFETY: buffer is the sole owner; every span is consumed before the next resize.
                unsafe
                {
                    buffer.Resize(4);
                    CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, buffer.AsSpan().ToArray());

                    buffer.Resize(5);
                    CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 0 }, buffer.AsSpan().ToArray());

                    buffer.Resize(2);
                    CollectionAssert.AreEqual(new[] { 1, 2 }, buffer.AsSpan().ToArray());

                    buffer.Resize(0);
                    Assert.IsTrue(buffer.IsCreated);
                    Assert.AreEqual(0, buffer.AsSpan().Length);

                    buffer.Resize(3);
                    CollectionAssert.AreEqual(new[] { 0, 0, 0 }, buffer.AsSpan().ToArray());
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void Resize_AliasForm_KeepsContentAndTheNativeAlias()
        {
            var buffer = new BufferShared<int, uint>(new[] { 1, -1 });

            try
            {
                // SAFETY: buffer is the sole owner; each alias is created after the resize and consumed immediately.
                unsafe
                {
                    buffer.Resize(3, copyContent: true);

                    var native = buffer.AsNativeArray();

                    Assert.AreEqual(3, native.Length);
                    Assert.AreEqual(1u, native[0]);
                    Assert.AreEqual(uint.MaxValue, native[1]);
                    Assert.AreEqual(0u, native[2]);

                    buffer.Resize(1, copyContent: false);

                    Assert.AreEqual(0, buffer[0]);
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no alias survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
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
        public void CopyFrom_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsContent()
        {
            var buffer = new BufferShared<int>(new[] { 1, 2, 3, 4 });

            try
            {
                var source = new[] { 9, 9 };

                // SAFETY: buffer is the sole owner and stays alive for every call below.
                unsafe
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(-1, source));
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(5, source));
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(3, source));
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(0, source, 3));
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(0, source, -1));
                    Assert.IsFalse(buffer.TryCopyFrom(-1, source));
                    Assert.IsFalse(buffer.TryCopyFrom(3, source));
                    Assert.IsFalse(buffer.TryCopyFrom(0, source, 3));
                    Assert.IsFalse(buffer.TryCopyFrom(0, source, -1));
                    CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, buffer.AsReadOnlySpan().ToArray());
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void CopyTo_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            var buffer = new BufferShared<int>(new[] { 1, 2, 3, 4 });

            try
            {
                var destination = new[] { 9, 9, 9, 9 };

                // SAFETY: buffer is the sole owner and stays alive for every call below.
                unsafe
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(-1, destination));
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(5, destination));
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(0, destination, 5));
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(0, new int[2], 3));
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(0, destination, -1));
                    Assert.IsFalse(buffer.TryCopyTo(-1, destination));
                    Assert.IsFalse(buffer.TryCopyTo(0, destination, 5));
                    Assert.IsFalse(buffer.TryCopyTo(0, new int[2], 3));
                    Assert.IsFalse(buffer.TryCopyTo(0, destination, -1));
                    CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void AliasForm_CopyMembers_InvalidRangeThrowsOrReturnsFalse()
        {
            var buffer = new BufferShared<int, uint>(new[] { 1, 2, 3, 4 });

            try
            {
                var other = new[] { 9, 9 };

                // SAFETY: buffer is the sole owner and stays alive for every call below.
                unsafe
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyFrom(3, other));
                    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CopyTo(0, other, 3));
                    Assert.IsFalse(buffer.TryCopyFrom(3, other));
                    Assert.IsFalse(buffer.TryCopyTo(0, other, 3));
                    CollectionAssert.AreEqual(new[] { 9, 9 }, other);
                    CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, buffer.AsReadOnlySpan().ToArray());
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void CopyFrom_SourceOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            var buffer = new BufferShared<int>((int[])seed.Clone());

            try
            {
                // SAFETY: buffer is the sole owner; each span is consumed by the copy that receives it.
                unsafe
                {
                    buffer.CopyFrom(1, buffer.AsReadOnlySpan()[..4]);

                    var expectedForward = (int[])seed.Clone();
                    Array.Copy(expectedForward, 0, expectedForward, 1, 4);
                    CollectionAssert.AreEqual(expectedForward, buffer.AsReadOnlySpan().ToArray());

                    buffer.CopyFrom(seed);
                    buffer.CopyFrom(0, buffer.AsReadOnlySpan()[1..5]);

                    var expectedBackward = (int[])seed.Clone();
                    Array.Copy(expectedBackward, 1, expectedBackward, 0, 4);
                    CollectionAssert.AreEqual(expectedBackward, buffer.AsReadOnlySpan().ToArray());
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void CopyTo_DestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            var buffer = new BufferShared<int>((int[])seed.Clone());

            try
            {
                // SAFETY: buffer is the sole owner; each span is consumed by the copy that receives it.
                unsafe
                {
                    buffer.CopyTo(2, buffer.AsSpan()[..4]);

                    var expectedBackward = (int[])seed.Clone();
                    Array.Copy(expectedBackward, 2, expectedBackward, 0, 4);
                    CollectionAssert.AreEqual(expectedBackward, buffer.AsReadOnlySpan().ToArray());

                    buffer.CopyFrom(seed);
                    buffer.CopyTo(0, buffer.AsSpan()[2..]);

                    var expectedForward = (int[])seed.Clone();
                    Array.Copy(expectedForward, 0, expectedForward, 2, 4);
                    CollectionAssert.AreEqual(expectedForward, buffer.AsReadOnlySpan().ToArray());
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void Views_SpanMemoryPointerAndNativeAliases_SeeEveryWrite()
        {
            var buffer = new BufferShared<int>(3);

            try
            {
                // SAFETY: buffer is the sole owner and never resizes while the views and the pointer are in use.
                unsafe
                {
                    var writable = buffer.AsSpan();
                    var readable = buffer.AsReadOnlySpan();
                    var memory = buffer.AsMemory();
                    var readOnlyMemory = buffer.AsReadOnlyMemory();
                    var native = buffer.AsNativeArray();
                    var slice = buffer.AsNativeSlice();
                    var readOnlySlice = buffer.AsNativeSliceReadOnly();
                    var pointer = buffer.GetUnsafeBufferPointer();

                    writable[0] = 10;
                    memory.Span[1] = 20;
                    pointer[2] = 30;

                    Assert.AreEqual(10, readable[0]);
                    Assert.AreEqual(10, readOnlyMemory.Span[0]);
                    Assert.AreEqual(10, native[0]);
                    Assert.AreEqual(20, slice[1]);
                    Assert.AreEqual(20, readOnlySlice[1]);
                    Assert.AreEqual(30, writable[2]);
                    Assert.AreEqual(30, buffer[2]);

                    slice[0] = 11;
                    native[1] = 21;

                    Assert.AreEqual(11, buffer[0]);
                    Assert.AreEqual(21, buffer[1]);
                    Assert.AreEqual(21, pointer[1]);
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AsNativeSlice_IndexPastLength_ThrowsIndexOutOfRange()
        {
            var buffer = new BufferShared<int>(2);

            try
            {
                // SAFETY: buffer is the sole owner and stays alive while the slice is indexed.
                unsafe
                {
                    var slice = buffer.AsNativeSlice();

                    Assert.Throws<IndexOutOfRangeException>(() => _ = slice[2]);
                    Assert.Throws<IndexOutOfRangeException>(() => _ = slice[-1]);
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void AliasForm_NativeViews_ReadTheBitsOfTheManagedElements()
        {
            var buffer = new BufferShared<int, uint>(new[] { 1, -1, int.MinValue });

            try
            {
                // SAFETY: buffer is the sole owner and never resizes while the aliases are in use.
                unsafe
                {
                    var native = buffer.AsNativeArray();
                    var readOnlySlice = buffer.AsNativeSliceReadOnly();

                    Assert.AreEqual(1u, native[0]);
                    Assert.AreEqual(uint.MaxValue, native[1]);
                    Assert.AreEqual(2147483648u, readOnlySlice[2]);

                    native[0] = 7u;

                    Assert.AreEqual(7, buffer[0]);
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no alias survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void GetUnsafeBufferPointer_AliasesTheSpanForBothForms()
        {
            var single = new BufferShared<int>(new[] { 1, 2, 3 });
            var alias = new BufferShared<int, uint>(new[] { 1, 2, 3 });

            try
            {
                // SAFETY: both buffers are sole owners, never resize, and every pointer access stays below 3.
                unsafe
                {
                    var singlePointer = single.GetUnsafeBufferPointer();
                    var aliasPointer = alias.GetUnsafeBufferPointer();

                    Assert.AreEqual(1, singlePointer[0]);
                    Assert.AreEqual(3, singlePointer[2]);
                    Assert.AreEqual(2u, aliasPointer[1]);

                    singlePointer[1] = 20;
                    aliasPointer[2] = 30u;

                    Assert.AreEqual(20, single.AsReadOnlySpan()[1]);
                    Assert.AreEqual(30, alias.AsReadOnlySpan()[2]);
                }
            }
            finally
            {
                // SAFETY: both buffers are designated owners and no borrower survives this test.
                unsafe
                {
                    single.Dispose();
                    alias.Dispose();
                }
            }
        }

        [Test]
        public void AsArraySegment_CoversTheWholeManagedArray()
        {
            var source = new[] { 1, 2, 3 };
            var single = new BufferShared<int>(source);
            var alias = new BufferShared<int, uint>(new[] { 4, 5 });

            try
            {
                var segment = single.AsArraySegment();
                var aliasSegment = alias.AsArraySegment();

                Assert.AreSame(source, segment.Array);
                Assert.AreEqual(0, segment.Offset);
                Assert.AreEqual(3, segment.Count);
                Assert.AreEqual(0, aliasSegment.Offset);
                Assert.AreEqual(2, aliasSegment.Count);
            }
            finally
            {
                // SAFETY: both buffers are designated owners and no borrower survives this test.
                unsafe
                {
                    single.Dispose();
                    alias.Dispose();
                }
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
            var buffer = new BufferShared<int>(new[] { 1, 2, 3, 4 });

            try
            {
                // SAFETY: buffer is the sole owner and stays alive while its read-only view is used.
                unsafe
                {
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
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void ReadOnly_CopyTo_EmptyDestinationAndOverlap_MatchArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            var buffer = new BufferShared<int>((int[])seed.Clone());

            try
            {
                // SAFETY: buffer is the sole owner and stays alive while its read-only view is used.
                unsafe
                {
                    var readOnly = buffer.AsReadOnly();

                    readOnly.CopyTo(Span<int>.Empty);
                    readOnly.CopyTo(6, Span<int>.Empty);

                    Assert.IsTrue(readOnly.TryCopyTo(Span<int>.Empty));
                    Assert.IsTrue(readOnly.TryCopyTo(6, Span<int>.Empty));
                    CollectionAssert.AreEqual(seed, buffer.AsReadOnlySpan().ToArray());

                    readOnly.CopyTo(0, buffer.AsSpan()[2..]);

                    var expected = (int[])seed.Clone();
                    Array.Copy(expected, 0, expected, 2, 4);
                    CollectionAssert.AreEqual(expected, buffer.AsReadOnlySpan().ToArray());
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void ReadOnly_Views_SeeOwnerWritesForBothForms()
        {
            var single = new BufferShared<int>(new[] { 1, 2, 3 });
            var alias = new BufferShared<int, uint>(new[] { 4, 5, 6 });

            try
            {
                // SAFETY: both buffers are sole owners and never resize while the read-only views are in use.
                unsafe
                {
                    var singleView = single.AsReadOnly();
                    var aliasView = alias.AsReadOnly();

                    single[0] = 10;
                    alias[1] = 50;

                    Assert.AreEqual(10, singleView[0]);
                    Assert.AreEqual(10, singleView.AsReadOnlySpan()[0]);
                    Assert.AreEqual(10, singleView.AsReadOnlyMemory().Span[0]);
                    Assert.AreEqual(10, singleView.AsNativeSliceReadOnly()[0]);
                    Assert.AreEqual(50, aliasView[1]);
                    Assert.AreEqual(50u, aliasView.AsNativeSliceReadOnly()[1]);
                    Assert.AreEqual(3, singleView.Capacity);
                    Assert.IsTrue(singleView.IsCreated);
                }
            }
            finally
            {
                // SAFETY: both buffers are designated owners and no borrower survives this test.
                unsafe
                {
                    single.Dispose();
                    alias.Dispose();
                }
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Reinterpret_DifferentSize_Throws()
        {
            var single = new BufferShared<int>(new[] { 1, 2 });
            var alias = new BufferShared<int, uint>(new[] { 1, 2 });

            try
            {
                // SAFETY: both buffers are sole owners; the size check rejects every call before an alias is built.
                unsafe
                {
                    Assert.Throws<InvalidOperationException>(() => single.Reinterpret<long>());
                    Assert.Throws<InvalidOperationException>(() => single.AsReadOnly().Reinterpret<ushort>());
                    Assert.Throws<InvalidOperationException>(() => alias.Reinterpret<long>());
                    Assert.Throws<InvalidOperationException>(() => alias.Reinterpret<byte>());
                    Assert.Throws<InvalidOperationException>(() => alias.AsReadOnly().Reinterpret<long>());
                }
            }
            finally
            {
                // SAFETY: both buffers are designated owners and no borrower survives this test.
                unsafe
                {
                    single.Dispose();
                    alias.Dispose();
                }
            }
        }

        [Test]
        public void Reinterpret_WriteThroughTheView_IsSeenByTheOwnerAndTheReverse()
        {
            var buffer = new BufferShared<int>(new[] { 0, 0, 0 });

            try
            {
                // SAFETY: buffer is the sole owner and never resizes; the views are not disposed because they share
                // the owner's pinned storage and only the owner releases it.
                unsafe
                {
                    var view = buffer.Reinterpret<float>();
                    var readOnlyView = buffer.AsReadOnly().Reinterpret<float>();
                    var native = view.AsNativeArray();

                    native[1] = 1.5f;

                    Assert.AreEqual(BitConverter.SingleToInt32Bits(1.5f), buffer[1]);
                    Assert.AreEqual(1.5f, readOnlyView.AsNativeSliceReadOnly()[1]);

                    buffer[2] = BitConverter.SingleToInt32Bits(-2.25f);

                    Assert.AreEqual(-2.25f, native[2]);
                    Assert.AreEqual(-2.25f, view.AsNativeSliceReadOnly()[2]);
                    Assert.AreEqual(3, view.Capacity);
                    Assert.AreEqual(3, readOnlyView.Capacity);
                    Assert.IsTrue(view.IsCreated);
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no alias survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void Reinterpret_StructLargerThanSixteenBytes_AliasesEveryField()
        {
            var buffer = new BufferShared<Wide>(2);

            try
            {
                // SAFETY: buffer is the sole owner and never resizes; the view shares its pinned storage.
                unsafe
                {
                    var native = buffer.Reinterpret<WideOther>().AsNativeArray();

                    buffer[1] = new Wide { a = 1, b = -2, c = long.MaxValue };

                    Assert.AreEqual(1L, native[1].x);
                    Assert.AreEqual(-2L, native[1].y);
                    Assert.AreEqual(long.MaxValue, native[1].z);

                    native[0] = new WideOther { x = long.MinValue, y = 0, z = -1 };

                    Assert.AreEqual(long.MinValue, buffer[0].a);
                    Assert.AreEqual(-1L, buffer[0].c);
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no alias survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        public void Reinterpret_ZeroCapacityBuffer_ReturnsAnEmptyView()
        {
            var buffer = new BufferShared<int>(0);

            try
            {
                // SAFETY: buffer is the sole owner and never resizes; the views share its pinned empty storage.
                unsafe
                {
                    var view = buffer.Reinterpret<uint>();
                    var readOnlyView = buffer.AsReadOnly().Reinterpret<uint>();

                    Assert.AreEqual(0, view.Capacity);
                    Assert.AreEqual(0, view.AsReadOnlySpan().Length);
                    Assert.AreEqual(0, readOnlyView.Capacity);
                    Assert.AreEqual(0, readOnlyView.AsReadOnlySpan().Length);
                }
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no alias survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [Test]
        [Category("Task045X2Regression")]
        public void Reinterpret_TwoParameterBorrowedView_DisposePreservesOwnerPinAndRejectsCapacityChange()
        {
            var source = new[] { 17, 29 };
            var owner = new BufferShared<int, int>(source);

            // SAFETY: The owner remains pinned until finally; only borrowed local aliases are disposed first.
            unsafe
            {
                try
                {
                    var view = owner.Reinterpret<uint>();
                    Assert.DoesNotThrow(() => view.Resize(2));
                    Assert.Throws<InvalidOperationException>(() => view.Resize(3));
                    Assert.Throws<InvalidOperationException>(() => view.Resize(1));
                    Assert.AreSame(source, view.AsManagedArray());
                    CollectionAssert.AreEqual(new[] { 17, 29 }, view.AsMemory().ToArray());
                    view.Dispose();
                    Assert.IsFalse(view.IsCreated);
                    Assert.AreEqual(0, view.Capacity);
                    Assert.AreSame(source, owner.AsManagedArray());
                    owner.AsMemory().Span[1] = 31;
                    CollectionAssert.AreEqual(new[] { 17, 31 }, owner.AsMemory().ToArray());
                    owner.Resize(3);
                    CollectionAssert.AreEqual(new[] { 17, 31, 0 }, owner.AsMemory().ToArray());
                }
                finally
                {
                    owner.Dispose();
                }
            }
        }

        [Test]
        [Category("Task045X2Regression")]
        public void Reinterpret_OneParameterForwardedView_DisposePreservesOwnerPinAndRejectsCapacityChange()
        {
            var source = new[] { 17, 29 };
            var owner = new BufferShared<int>(source);

            // SAFETY: The forwarding wrapper preserves the borrowed ownership flag; the sole owner is disposed last.
            unsafe
            {
                try
                {
                    BufferShared<int> view = owner.Reinterpret<int>();
                    Assert.DoesNotThrow(() => view.Resize(2));
                    Assert.Throws<InvalidOperationException>(() => view.Resize(3));
                    Assert.Throws<InvalidOperationException>(() => view.Resize(1));
                    Assert.AreSame(source, view.AsManagedArray());
                    CollectionAssert.AreEqual(new[] { 17, 29 }, view.AsMemory().ToArray());
                    view.Dispose();
                    Assert.IsFalse(view.IsCreated);
                    Assert.AreEqual(0, view.Capacity);
                    Assert.AreSame(source, owner.AsManagedArray());
                    owner.AsMemory().Span[1] = 31;
                    CollectionAssert.AreEqual(new[] { 17, 31 }, owner.AsMemory().ToArray());
                    owner.Resize(3);
                    CollectionAssert.AreEqual(new[] { 17, 31, 0 }, owner.AsMemory().ToArray());
                }
                finally
                {
                    owner.Dispose();
                }
            }
        }

        private static void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var buffer = new BufferShared<T>(3);

            try
            {
                // SAFETY: buffer is the sole owner; every span, slice and copy below is consumed before a resize.
                unsafe
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
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        private static void AssertReadOnlyElementRoundTrip<T>(T first, T last)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var buffer = new BufferShared<T>(3);

            try
            {
                // SAFETY: buffer is the sole owner and never resizes while its read-only view is used.
                unsafe
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
            }
            finally
            {
                // SAFETY: buffer is the designated owner and no borrower survives this test.
                unsafe
                {
                    buffer.Dispose();
                }
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
