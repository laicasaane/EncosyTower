using System;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    // Adapted from Unity.Collections.Tests/NativeReferenceTests.cs against the raw Unsafe header.
    public partial class ReferenceUnsafeTests
    {
        [Test]
        public void Constructor_WithValue_StoresValue()
        {
            using var reference = new ReferenceUnsafe<int>(42, Allocator.Temp);

            Assert.IsTrue(reference.IsCreated);
            Assert.AreEqual(42, reference.Value);
        }

        [Test]
        public void Constructor_Default_ClearsToZero()
        {
            using var reference = new ReferenceUnsafe<int>(Allocator.Temp);

            Assert.IsTrue(reference.IsCreated);
            Assert.AreEqual(0, reference.Value);
        }

        [Test]
        public void DefaultReference_IsNotCreated()
        {
            ReferenceUnsafe<int> reference = default;

            Assert.IsFalse(reference.IsCreated);
        }

        [Test]
        public void Value_SetGet()
        {
            var reference = new ReferenceUnsafe<int>(Allocator.Temp);

            try
            {
                reference.Value = 99;
                Assert.AreEqual(99, reference.Value);
            }
            finally
            {
                reference.Dispose();
            }
        }

        [Test]
        public void ValueAsRef_Mutates()
        {
            var reference = new ReferenceUnsafe<int>(5, Allocator.Temp);

            try
            {
                ref var slot = ref reference.ValueAsRef;
                slot = 123;
                Assert.AreEqual(123, reference.Value);
            }
            finally
            {
                reference.Dispose();
            }
        }

        [Test]
        public void CopyFrom_CopiesValue()
        {
            using var a = new ReferenceUnsafe<int>(1, Allocator.Temp);
            using var b = new ReferenceUnsafe<int>(2, Allocator.Temp);

            a.CopyFrom(b);

            Assert.AreEqual(2, a.Value);
            Assert.AreEqual(2, b.Value);
        }

        [Test]
        public void CopyTo_CopiesValue()
        {
            using var a = new ReferenceUnsafe<int>(1, Allocator.Temp);
            using var b = new ReferenceUnsafe<int>(2, Allocator.Temp);

            a.CopyTo(b);

            Assert.AreEqual(1, a.Value);
            Assert.AreEqual(1, b.Value);
        }

        [Test]
        public void Equals_ByValue()
        {
            using var a = new ReferenceUnsafe<int>(7, Allocator.Temp);
            using var b = new ReferenceUnsafe<int>(7, Allocator.Temp);
            using var c = new ReferenceUnsafe<int>(8, Allocator.Temp);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a == c);
            Assert.IsTrue(a != c);
            Assert.IsTrue(a.Equals((object)b));
            Assert.IsFalse(a.Equals((object)c));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void IndexerLengthSpansPointerReadOnlyAndConversion_AliasOwnedValue()
        {
            using var reference = new ReferenceUnsafe<int>(5, Allocator.Temp);

            Assert.AreEqual(1, reference.Length);
            Assert.AreEqual(5, reference[0]);

            reference[0] = 6;
            var span = reference.AsSpan();
            var readOnlySpan = reference.AsReadOnlySpan();
            var readOnly = reference.AsReadOnly();
            ReferenceUnsafe<int>.ReadOnly converted = reference;

            Assert.AreEqual(6, span[0]);
            Assert.AreEqual(6, readOnlySpan[0]);
            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(6, readOnly.Value);
            Assert.IsTrue(converted.IsCreated);
            Assert.AreEqual(6, converted.Value);

            // SAFETY: The pointer aliases one live int allocation and does not escape this scope.
            unsafe
            {
                var pointer = (int*)reference.GetUnsafePtr();

                Assert.AreNotEqual(IntPtr.Zero, (IntPtr)pointer);

                *pointer = 7;
            }

            Assert.AreEqual(7, reference.Value);
            Assert.IsFalse(default(ReferenceUnsafe<int>.ReadOnly).IsCreated);
        }

        [Test]
        public void SpanCopyFamilies_AllOverloadsCopyOrReturnFalse()
        {
            using var reference = new ReferenceUnsafe<int>(0, Allocator.Temp);

            reference.CopyFrom(new[] { 1 });
            Assert.AreEqual(1, reference.Value);
            reference.CopyFrom(new[] { 2, 3 }, 1);
            Assert.AreEqual(2, reference.Value);
            reference.CopyFrom(0, new[] { 3 });
            Assert.AreEqual(3, reference.Value);
            reference.CopyFrom(0, new[] { 4, 5 }, 1);
            Assert.AreEqual(4, reference.Value);

            Assert.IsTrue(reference.TryCopyFrom(new[] { 5 }));
            Assert.IsTrue(reference.TryCopyFrom(new[] { 6, 7 }, 1));
            Assert.IsTrue(reference.TryCopyFrom(0, new[] { 7 }));
            Assert.IsTrue(reference.TryCopyFrom(0, new[] { 8, 9 }, 1));
            Assert.IsFalse(reference.TryCopyFrom(new[] { 10, 11 }));
            Assert.IsFalse(reference.TryCopyFrom(1, new[] { 10 }));
            Assert.AreEqual(8, reference.Value);

            var all = new int[1];
            var explicitLength = new int[1];
            var sourceIndex = new int[1];
            var explicitWindow = new int[1];

            reference.CopyTo(all);
            reference.CopyTo(explicitLength, 1);
            reference.CopyTo(0, sourceIndex);
            reference.CopyTo(0, explicitWindow, 1);

            Assert.AreEqual(8, all[0]);
            Assert.AreEqual(8, explicitLength[0]);
            Assert.AreEqual(8, sourceIndex[0]);
            Assert.AreEqual(8, explicitWindow[0]);
            Assert.IsTrue(reference.TryCopyTo(new int[1]));
            Assert.IsTrue(reference.TryCopyTo(new int[1], 1));
            Assert.IsTrue(reference.TryCopyTo(0, new int[1]));
            Assert.IsTrue(reference.TryCopyTo(0, new int[1], 1));
            Assert.IsFalse(reference.TryCopyTo(new int[1], 2));
            Assert.IsFalse(reference.TryCopyTo(1, new int[1]));
        }

        [Test]
        public void StaticCopy_CopiesSourceIntoDestination()
        {
            using var destination = new ReferenceUnsafe<int>(1, Allocator.Temp);
            using var source = new ReferenceUnsafe<int>(2, Allocator.Temp);

            ReferenceUnsafe<int>.Copy(destination, source);

            Assert.AreEqual(2, destination.Value);
            Assert.AreEqual(2, source.Value);
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var reference = new ReferenceUnsafe<int>(1, Allocator.Temp);
            Assert.IsTrue(reference.IsCreated);

            reference.Dispose();

            Assert.IsFalse(reference.IsCreated);
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var reference = new ReferenceUnsafe<int>(1, Allocator.TempJob);

            var handle = reference.Dispose(default);

            Assert.IsFalse(reference.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_CreatesAndDisposes(Allocator allocator)
        {
            var withValue = new ReferenceUnsafe<int>(11, allocator);
            var cleared = new ReferenceUnsafe<int>(allocator);

            try
            {
                Assert.IsTrue(withValue.IsCreated);
                Assert.IsTrue(cleared.IsCreated);
                Assert.AreEqual(11, withValue.Value);
                Assert.AreEqual(0, cleared.Value);
            }
            finally
            {
                withValue.Dispose();
                cleared.Dispose();
            }

            Assert.IsFalse(withValue.IsCreated);
            Assert.IsFalse(cleared.IsCreated);
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_JobHandle_AcceptedAllocator_ReleasesTheStorage(Allocator allocator)
        {
            var reference = new ReferenceUnsafe<int>(3, allocator);

            var handle = reference.Dispose(default);

            Assert.IsFalse(reference.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Constructor_DefaultAllocatorStrategy_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () => _ = new ReferenceUnsafe<int>(default(AllocatorStrategy))
            );
            Assert.Throws<InvalidOperationException>(
                () => _ = new ReferenceUnsafe<int>(1, default(AllocatorStrategy))
            );
        }

        [Test]
        public void Dispose_Twice_ThrowsObjectDisposedException()
        {
            var reference = new ReferenceUnsafe<int>(1, Allocator.Persistent);
            reference.Dispose();

            Assert.Throws<ObjectDisposedException>(() => reference.Dispose());
            Assert.IsFalse(reference.IsCreated);
        }

        [Test]
        public void Dispose_DefaultReference_ThrowsObjectDisposedException()
        {
            var reference = default(ReferenceUnsafe<int>);

            Assert.Throws<ObjectDisposedException>(() => reference.Dispose());
            Assert.Catch(() => reference.Dispose(default));
        }

        [Test]
        public void Dispose_JobHandle_AfterDispose_Throws()
        {
            var reference = new ReferenceUnsafe<int>(1, Allocator.Persistent);
            reference.Dispose();

            Assert.Catch(() => reference.Dispose(default));
        }

        [Test]
        public void UncreatedReference_Members_ReportNoStorage()
        {
            var reference = default(ReferenceUnsafe<int>);

            Assert.IsFalse(reference.IsCreated);
            Assert.AreEqual(0, reference.Length);
            Assert.IsFalse(reference.AsReadOnly().IsCreated);

            ReferenceUnsafe<int>.ReadOnly converted = reference;

            Assert.IsFalse(converted.IsCreated);

            // SAFETY: The pointer of an uncreated reference is only compared with null and never dereferenced.
            unsafe
            {
                Assert.AreEqual(IntPtr.Zero, (IntPtr)reference.GetUnsafePtr());
            }
        }

        [Test]
        public void AsSpan_UncreatedReference_LengthMatchesLength()
        {
            var reference = default(ReferenceUnsafe<int>);

            Assert.AreEqual(reference.Length, reference.AsSpan().Length);
            Assert.AreEqual(reference.Length, reference.AsReadOnlySpan().Length);
        }

        [Test]
        public void AsSpan_AfterDispose_LengthMatchesLength()
        {
            var reference = new ReferenceUnsafe<int>(1, Allocator.Persistent);
            reference.Dispose();

            Assert.AreEqual(0, reference.Length);
            Assert.AreEqual(reference.Length, reference.AsSpan().Length);
            Assert.AreEqual(reference.Length, reference.AsReadOnlySpan().Length);
        }

        [Test]
        public void AsSpan_CreatedReference_HasLengthOneAndAliasesTheValue()
        {
            using var reference = new ReferenceUnsafe<int>(5, Allocator.Temp);

            var span = reference.AsSpan();
            var readOnlySpan = reference.AsReadOnlySpan();

            span[0] = 6;

            Assert.AreEqual(1, span.Length);
            Assert.AreEqual(1, readOnlySpan.Length);
            Assert.AreEqual(6, readOnlySpan[0]);
            Assert.AreEqual(6, reference.Value);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Indexer_IndexOtherThanZero_Throws()
        {
            using var reference = new ReferenceUnsafe<int>(5, Allocator.Temp);

            Assert.Throws<IndexOutOfRangeException>(() => _ = reference[1]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = reference[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = reference[int.MaxValue]);
            Assert.Throws<IndexOutOfRangeException>(() => reference[1] = 9);
            Assert.AreEqual(5, reference.Value);
        }

        [Test]
        public void Elements_ByteElement_RoundTripBitExact()
        {
            AssertElementRoundTrip<byte>(0xA5, 0xFF);
        }

        [Test]
        public void Elements_ThreeByteStruct_RoundTripBitExact()
        {
            AssertElementRoundTrip(new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 254, b = 253 });
        }

        [Test]
        public void Elements_LongElement_RoundTripBitExact()
        {
            AssertElementRoundTrip(long.MinValue, long.MaxValue);
        }

        [Test]
        public void Elements_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = long.MinValue, b = 0, c = -1 }
            );
        }

        [Test]
        public void ConvertExistingData_View_AliasesTheMemoryAndDisposeFreesNothing()
        {
            // SAFETY: The block holds one int, outlives the view, and is freed exactly once in finally; the view's
            // Dispose releases nothing, so the block stays valid until then.
            unsafe
            {
                var memory = (int*)UnsafeUtility.Malloc(
                      sizeof(int)
                    , UnsafeUtility.AlignOf<int>()
                    , Allocator.Persistent
                );

                try
                {
                    *memory = 5;

                    var view = ReferenceUnsafe<int>.ConvertExistingData(memory);

                    Assert.IsTrue(view.IsCreated);
                    Assert.AreEqual(1, view.Length);
                    Assert.AreEqual(5, view.Value);

                    view.Value = 6;

                    Assert.AreEqual(6, *memory);

                    *memory = 7;

                    Assert.AreEqual(7, view[0]);
                    Assert.AreEqual(7, view.AsReadOnlySpan()[0]);

                    view.Dispose();

                    Assert.IsFalse(view.IsCreated);
                    Assert.AreEqual(7, *memory);
                }
                finally
                {
                    UnsafeUtility.Free(memory, Allocator.Persistent);
                }
            }
        }

        [Test]
        public void ConvertExistingData_NullPointer_ReportsNoStorage()
        {
            // SAFETY: The null pointer is only stored and compared, and is never dereferenced.
            unsafe
            {
                var view = ReferenceUnsafe<int>.ConvertExistingData(null);

                Assert.IsFalse(view.IsCreated);
                Assert.AreEqual(0, view.Length);
                Assert.Throws<ObjectDisposedException>(() => view.Dispose());
            }
        }

        [Test]
        public void ReadOnly_Constructor_NullAndLivePointer_ReportCreation()
        {
            // SAFETY: The pointer addresses the live local int for the whole block and is only read through Value.
            unsafe
            {
                var value = 9;
                var live = new ReferenceUnsafe<int>.ReadOnly(&value);
                var empty = new ReferenceUnsafe<int>.ReadOnly(null);

                Assert.IsTrue(live.IsCreated);
                Assert.AreEqual(9, live.Value);
                Assert.IsFalse(empty.IsCreated);
            }
        }

        [Test]
        public void CopyFrom_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsValue()
        {
            using var reference = new ReferenceUnsafe<int>(5, Allocator.Temp);

            Assert.Throws<ArgumentOutOfRangeException>(() => reference.CopyFrom(new[] { 1, 2 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => reference.CopyFrom(-1, new[] { 1 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => reference.CopyFrom(1, new[] { 1 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => reference.CopyFrom(0, new[] { 1 }, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => reference.CopyFrom(0, new[] { 1 }, -1));
            Assert.IsFalse(reference.TryCopyFrom(-1, new[] { 1 }));
            Assert.IsFalse(reference.TryCopyFrom(0, new[] { 1 }, 2));
            Assert.IsFalse(reference.TryCopyFrom(0, new[] { 1 }, -1));
            Assert.AreEqual(5, reference.Value);
        }

        [Test]
        public void CopyTo_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            using var reference = new ReferenceUnsafe<int>(5, Allocator.Temp);
            var destination = new[] { 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() => reference.CopyTo(new int[2]));
            Assert.Throws<ArgumentOutOfRangeException>(() => reference.CopyTo(-1, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => reference.CopyTo(1, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => reference.CopyTo(0, destination, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => reference.CopyTo(0, destination, -1));
            Assert.IsFalse(reference.TryCopyTo(new int[2]));
            Assert.IsFalse(reference.TryCopyTo(-1, destination));
            Assert.IsFalse(reference.TryCopyTo(0, destination, 2));
            Assert.IsFalse(reference.TryCopyTo(0, destination, -1));
            CollectionAssert.AreEqual(new[] { 9 }, destination);
        }

        [Test]
        public void CopyFromAndCopyTo_EmptySpans_ChangeNothing()
        {
            using var reference = new ReferenceUnsafe<int>(5, Allocator.Temp);

            reference.CopyFrom(ReadOnlySpan<int>.Empty);
            reference.CopyFrom(1, ReadOnlySpan<int>.Empty);
            reference.CopyTo(Span<int>.Empty);
            reference.CopyTo(1, Span<int>.Empty);

            Assert.IsTrue(reference.TryCopyFrom(ReadOnlySpan<int>.Empty));
            Assert.IsTrue(reference.TryCopyFrom(1, ReadOnlySpan<int>.Empty));
            Assert.IsTrue(reference.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(reference.TryCopyTo(1, Span<int>.Empty));
            Assert.AreEqual(5, reference.Value);
        }

        [Test]
        public void CopyFrom_SourceIsOwnStorage_KeepsTheValue()
        {
            using var reference = new ReferenceUnsafe<int>(5, Allocator.Temp);

            reference.CopyFrom(reference.AsReadOnlySpan());
            reference.CopyFrom(reference);
            reference.CopyTo(reference.AsSpan());
            reference.CopyTo(reference);
            ReferenceUnsafe<int>.Copy(reference, reference);

            Assert.AreEqual(5, reference.Value);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Copy_UncreatedSourceOrDestination_Throws()
        {
            using var created = new ReferenceUnsafe<int>(5, Allocator.Temp);
            var uncreated = default(ReferenceUnsafe<int>);

            Assert.Throws<InvalidOperationException>(() => ReferenceUnsafe<int>.Copy(created, uncreated));
            Assert.Throws<InvalidOperationException>(() => ReferenceUnsafe<int>.Copy(uncreated, created));
            Assert.Throws<InvalidOperationException>(() => created.CopyFrom(uncreated));
            Assert.Throws<InvalidOperationException>(() => created.CopyTo(uncreated));
            Assert.AreEqual(5, created.Value);
        }

        private static void AssertElementRoundTrip<T>(T a, T b)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var reference = new ReferenceUnsafe<T>(a, Allocator.Temp);
            var other = new ReferenceUnsafe<T>(b, Allocator.Temp);

            try
            {
                Assert.AreEqual(a, reference.Value, name);
                Assert.AreEqual(a, reference[0], name);
                Assert.AreEqual(a, reference.AsReadOnlySpan()[0], name);

                reference.AsSpan()[0] = b;

                Assert.AreEqual(b, reference.Value, name);

                reference.ValueAsRef = a;

                Assert.AreEqual(a, reference.Value, name);

                reference[0] = b;

                Assert.AreEqual(b, reference.Value, name);

                reference.CopyFrom(new[] { a });

                Assert.AreEqual(a, reference.Value, name);

                var copy = new T[1];
                reference.CopyTo(copy);

                Assert.AreEqual(a, copy[0], name);

                ReferenceUnsafe<T>.Copy(other, reference);

                Assert.AreEqual(a, other.Value, name);
                Assert.AreEqual(a, reference.Value, name);

                other.Value = b;
                reference.CopyFrom(other);

                Assert.AreEqual(b, reference.Value, name);
            }
            finally
            {
                reference.Dispose();
                other.Dispose();
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
    }
}
