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
    }
}
