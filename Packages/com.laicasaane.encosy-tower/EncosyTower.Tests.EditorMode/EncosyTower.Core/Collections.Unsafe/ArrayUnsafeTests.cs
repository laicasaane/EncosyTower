// Adapted from Unity.Collections.Tests/NativeArrayTests.cs.

using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class ArrayUnsafeTests
    {
        [Test]
        public void Constructor_DefaultClearsMemory()
        {
            using var array = new ArrayUnsafe<int>(4, Allocator.Temp);

            Assert.IsTrue(array.IsCreated);
            Assert.AreEqual(4, array.Length);

            for (var i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(0, array[i]);
            }
        }

        [Test]
        public void Constructor_UninitializedMemoryCanBeWritten()
        {
            using var array = new ArrayUnsafe<int>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            array[0] = 10;
            array[1] = 20;
            array[2] = 30;

            CollectionAssert.AreEqual(new[] { 10, 20, 30 }, array.ToArray());
        }

        [Test]
        public void Constructor_AllocatorHandleCreatesWritableArray()
        {
            using var array = new ArrayUnsafe<int>(2, AllocatorManager.Persistent);

            array[0] = 10;
            array[1] = 20;

            CollectionAssert.AreEqual(new[] { 10, 20 }, array.ToArray());
        }

        [Test]
        public void Constructor_ZeroLengthCreatesDisposableArray()
        {
            var array = new ArrayUnsafe<int>(0, Allocator.Temp);

            Assert.IsTrue(array.IsCreated);
            Assert.AreEqual(0, array.Length);
            Assert.AreEqual(0, array.AsSpan().Length);

            array.Dispose();

            Assert.IsFalse(array.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Constructor_NonOwningAllocatorStrategiesThrow()
        {
            Assert.Throws<ArgumentException>(
                () => _ = new ArrayUnsafe<int>(1, default(AllocatorStrategy))
            );
            Assert.Throws<ArgumentException>(
                () => _ = new ArrayUnsafe<int>(1, Allocator.None)
            );
            Assert.Throws<ArgumentException>(
                () => _ = new ArrayUnsafe<int>(1, Allocator.Invalid)
            );
        }

        [Test]
        public void Indexer_ReadsAndWritesValues()
        {
            using var array = new ArrayUnsafe<int>(3, Allocator.Temp);

            array[0] = 10;
            array[2] = 30;

            Assert.AreEqual(10, array[0]);
            Assert.AreEqual(30, array[2]);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Indexer_OutOfRange_Throws()
        {
            using var array = new ArrayUnsafe<int>(3, Allocator.Temp);

            Assert.Catch(() => _ = array[-1]);
            Assert.Catch(() => array[3] = 10);
        }

        [Test]
        public void CopyFromAndTryCopyFrom_SpanFamiliesCopyOrReturnFalse()
        {
            using var array = new ArrayUnsafe<int>(6, Allocator.Temp);

            array.CopyFrom(new[] { 1, 2, 3 });
            array.CopyFrom(new[] { 4, 5, 6 }, 2);
            array.CopyFrom(3, new[] { 7, 8 });
            array.CopyFrom(5, new[] { 9, 10 }, 1);

            CollectionAssert.AreEqual(new[] { 4, 5, 3, 7, 8, 9 }, array.ToArray());
            Assert.IsTrue(array.TryCopyFrom(new[] { 11, 12 }));
            Assert.IsTrue(array.TryCopyFrom(new[] { 13, 14 }, 1));
            Assert.IsTrue(array.TryCopyFrom(2, new[] { 15, 16 }));
            Assert.IsTrue(array.TryCopyFrom(4, new[] { 17, 18 }, 2));
            Assert.IsFalse(array.TryCopyFrom(5, new[] { 19, 20 }));
        }

        [Test]
        public void CopyToAndTryCopyTo_SpanFamiliesCopyOrReturnFalse()
        {
            using var array = new ArrayUnsafe<int>(6, Allocator.Temp);
            array.CopyFrom(new[] { 1, 2, 3, 4, 5, 6 });

            var all = new int[6];
            var firstThree = new int[3];
            var fromIndex = new int[2];
            var range = new int[2];

            array.CopyTo(all);
            array.CopyTo(firstThree, 3);
            array.CopyTo(2, fromIndex);
            array.CopyTo(4, range, 2);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, all);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, firstThree);
            CollectionAssert.AreEqual(new[] { 3, 4 }, fromIndex);
            CollectionAssert.AreEqual(new[] { 5, 6 }, range);
            Assert.IsTrue(array.TryCopyTo(new int[6]));
            Assert.IsTrue(array.TryCopyTo(new int[3], 3));
            Assert.IsTrue(array.TryCopyTo(2, new int[2]));
            Assert.IsTrue(array.TryCopyTo(4, new int[2], 2));
            Assert.IsFalse(array.TryCopyTo(new int[1], 2));
        }

        [Test]
        public void ToArrayAndSpans_ReflectContentAndWrites()
        {
            using var array = new ArrayUnsafe<int>(3, Allocator.Temp);
            array.CopyFrom(new[] { 1, 2, 3 });

            var span = array.AsSpan();
            var readOnlySpan = array.AsReadOnlySpan();

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, array.ToArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, readOnlySpan.ToArray());

            span[1] = 20;

            Assert.AreEqual(20, array[1]);
        }

        [Test]
        public void Reinterpret_SharesMemory()
        {
            using var array = new ArrayUnsafe<int>(2, Allocator.Temp);
            array[0] = 42;

            var reinterpreted = array.Reinterpret<uint>();

            Assert.AreEqual(array.Length, reinterpreted.Length);
            Assert.AreEqual(42u, reinterpreted[0]);

            reinterpreted[1] = 99u;

            Assert.AreEqual(99, array[1]);
            reinterpreted.Dispose();
        }

        [Test]
        public void EqualsAndGetHashCode_UseBufferIdentity()
        {
            var array = new ArrayUnsafe<int>(2, Allocator.Temp);
            var alias = array;
            using var other = new ArrayUnsafe<int>(2, Allocator.Temp);

            Assert.IsTrue(array.Equals(alias));
            Assert.IsTrue(array == alias);
            Assert.IsFalse(array != alias);
            Assert.AreEqual(array.GetHashCode(), alias.GetHashCode());
            Assert.IsFalse(array.Equals(other));
            Assert.IsTrue(array.Equals((object)alias));
            Assert.IsFalse(array.Equals((object)other));
            Assert.IsTrue(array != other);

            array.Dispose();
        }

        [Test]
        public void Enumerator_VisitsEveryElement()
        {
            using var array = new ArrayUnsafe<int>(3, Allocator.Temp);
            array.CopyFrom(new[] { 1, 2, 3 });
            var values = new int[3];
            var index = 0;

            foreach (var value in array)
            {
                values[index++] = value;
            }

            Assert.AreEqual(3, index);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, values);
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var array = new ArrayUnsafe<int>(3, Allocator.Temp);

            array.Dispose();

            Assert.IsFalse(array.IsCreated);
        }

        [Test]
        public void DoubleDispose_Throws()
        {
            var array = new ArrayUnsafe<int>(3, Allocator.Temp);
            array.Dispose();

            Assert.Throws<ObjectDisposedException>(() => array.Dispose());
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var array = new ArrayUnsafe<int>(3, AllocatorManager.Persistent);

            array.Dispose(default(JobHandle)).Complete();

            Assert.IsFalse(array.IsCreated);
        }

        [Test]
        public void GetUnsafePtr_MutatesOwnedStorage()
        {
            using var array = new ArrayUnsafe<int>(2, Allocator.Temp);

            // SAFETY: The pointer is bounded by the live two-element allocation and does not escape.
            unsafe
            {
                var pointer = (int*)array.GetUnsafePtr();

                Assert.AreNotEqual(IntPtr.Zero, (IntPtr)pointer);

                pointer[0] = 10;
                pointer[1] = 20;
            }

            CollectionAssert.AreEqual(new[] { 10, 20 }, array.ToArray());
        }

        [Test]
        public void Enumerator_DirectConstructionResetDisposeAndInterfacesWork()
        {
            var array = new ArrayUnsafe<int>(2, Allocator.Temp);

            try
            {
                array.CopyFrom(new[] { 7, 8 });
                var alias = array;
                var direct = new ArrayUnsafe<int>.Enumerator(ref alias);

                Assert.IsTrue(direct.MoveNext());
                Assert.AreEqual(7, direct.Current);
                Assert.IsTrue(direct.MoveNext());
                Assert.AreEqual(8, direct.Current);
                Assert.IsFalse(direct.MoveNext());
                direct.Reset();
                Assert.IsTrue(direct.MoveNext());
                IEnumerator interfaceEnumerator = direct;
                Assert.AreEqual(7, interfaceEnumerator.Current);
                direct.Dispose();

                var ownerEnumerator = array.GetEnumerator();
                Assert.IsTrue(ownerEnumerator.MoveNext());
                ownerEnumerator.Dispose();
            }
            finally
            {
                array.Dispose();
            }
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_CreatesAndDisposes(Allocator allocator)
        {
            var array = new ArrayUnsafe<int>(3, allocator);

            try
            {
                array[2] = 7;

                Assert.IsTrue(array.IsCreated);
                Assert.AreEqual(3, array.Length);
                Assert.AreEqual(7, array[2]);
            }
            finally
            {
                array.Dispose();
            }

            Assert.IsFalse(array.IsCreated);
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_JobHandle_AcceptedAllocator_ReleasesTheStorage(Allocator allocator)
        {
            var array = new ArrayUnsafe<int>(3, allocator);

            var handle = array.Dispose(default);

            Assert.IsFalse(array.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void Dispose_UncreatedArray_ThrowsObjectDisposedException()
        {
            var array = default(ArrayUnsafe<int>);

            Assert.Throws<ObjectDisposedException>(() => array.Dispose());
            Assert.Catch(() => array.Dispose(default));
        }

        [Test]
        public void UncreatedArray_Members_ReportAnEmptyArray()
        {
            var array = default(ArrayUnsafe<int>);

            Assert.IsFalse(array.IsCreated);
            Assert.AreEqual(0, array.Length);
            Assert.AreEqual(0, array.AsSpan().Length);
            Assert.AreEqual(0, array.AsReadOnlySpan().Length);
            Assert.AreEqual(0, array.ToArray().Length);
            Assert.IsTrue(array.TryCopyTo(Span<int>.Empty));
            Assert.IsFalse(array.TryCopyTo(new int[1]));
            Assert.IsTrue(array.TryCopyFrom(ReadOnlySpan<int>.Empty));
            Assert.IsFalse(array.TryCopyFrom(new[] { 1 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyTo(new int[1]));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyFrom(new[] { 1 }));

            // SAFETY: The pointer of an uncreated array is only compared with null and never dereferenced.
            unsafe
            {
                Assert.AreEqual(IntPtr.Zero, (IntPtr)array.GetUnsafePtr());
            }
        }

        [Test]
        public void ZeroLengthArray_Members_ReportAnEmptyArrayAndEnumerateNothing()
        {
            var array = new ArrayUnsafe<int>(0, Allocator.Temp);

            try
            {
                var structEnumerator = array.GetEnumerator();
                var genericEnumerator = ((IEnumerable<int>)array).GetEnumerator();
                var nonGenericEnumerator = ((IEnumerable)array).GetEnumerator();

                Assert.IsTrue(array.IsCreated);
                Assert.AreEqual(0, array.ToArray().Length);
                Assert.AreEqual(0, array.AsReadOnlySpan().Length);
                Assert.IsFalse(structEnumerator.MoveNext());
                Assert.IsFalse(genericEnumerator.MoveNext());
                Assert.IsFalse(nonGenericEnumerator.MoveNext());
                Assert.IsTrue(array.TryCopyTo(Span<int>.Empty));
                Assert.IsTrue(array.TryCopyFrom(ReadOnlySpan<int>.Empty));
                Assert.AreEqual(0, array.Reinterpret<uint>().Length);
            }
            finally
            {
                array.Dispose();
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
            using var array = new ArrayUnsafe<int>(4, Allocator.Temp);
            array.CopyFrom(new[] { 1, 2, 3, 4 });
            var source = new[] { 9, 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyFrom(-1, source));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyFrom(5, source));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyFrom(3, source));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyFrom(0, source, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyFrom(0, source, -1));
            Assert.IsFalse(array.TryCopyFrom(-1, source));
            Assert.IsFalse(array.TryCopyFrom(3, source));
            Assert.IsFalse(array.TryCopyFrom(0, source, 3));
            Assert.IsFalse(array.TryCopyFrom(0, source, -1));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, array.ToArray());
        }

        [Test]
        public void CopyTo_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            using var array = new ArrayUnsafe<int>(4, Allocator.Temp);
            array.CopyFrom(new[] { 1, 2, 3, 4 });
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyTo(-1, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyTo(5, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyTo(0, destination, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyTo(0, new int[2], 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyTo(0, destination, -1));
            Assert.IsFalse(array.TryCopyTo(-1, destination));
            Assert.IsFalse(array.TryCopyTo(0, destination, 5));
            Assert.IsFalse(array.TryCopyTo(0, new int[2], 3));
            Assert.IsFalse(array.TryCopyTo(0, destination, -1));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public void CopyFromAndCopyTo_EmptySpans_ChangeNothing()
        {
            using var array = new ArrayUnsafe<int>(3, Allocator.Temp);
            array.CopyFrom(new[] { 1, 2, 3 });

            array.CopyFrom(ReadOnlySpan<int>.Empty);
            array.CopyFrom(3, ReadOnlySpan<int>.Empty);
            array.CopyTo(Span<int>.Empty);
            array.CopyTo(3, Span<int>.Empty);

            Assert.IsTrue(array.TryCopyFrom(ReadOnlySpan<int>.Empty));
            Assert.IsTrue(array.TryCopyFrom(3, ReadOnlySpan<int>.Empty));
            Assert.IsTrue(array.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(array.TryCopyTo(3, Span<int>.Empty));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, array.ToArray());
        }

        [Test]
        public void CopyFrom_SourceOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var array = new ArrayUnsafe<int>(6, Allocator.Temp);
            array.CopyFrom(seed);

            array.CopyFrom(1, array.AsReadOnlySpan()[..4]);

            var expectedForward = (int[])seed.Clone();
            Array.Copy(expectedForward, 0, expectedForward, 1, 4);
            CollectionAssert.AreEqual(expectedForward, array.ToArray());

            array.CopyFrom(seed);
            array.CopyFrom(0, array.AsReadOnlySpan()[1..5]);

            var expectedBackward = (int[])seed.Clone();
            Array.Copy(expectedBackward, 1, expectedBackward, 0, 4);
            CollectionAssert.AreEqual(expectedBackward, array.ToArray());
        }

        [Test]
        public void CopyTo_DestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var array = new ArrayUnsafe<int>(6, Allocator.Temp);
            array.CopyFrom(seed);

            array.CopyTo(2, array.AsSpan()[..4]);

            var expectedBackward = (int[])seed.Clone();
            Array.Copy(expectedBackward, 2, expectedBackward, 0, 4);
            CollectionAssert.AreEqual(expectedBackward, array.ToArray());

            array.CopyFrom(seed);
            array.CopyTo(0, array.AsSpan()[2..]);

            var expectedForward = (int[])seed.Clone();
            Array.Copy(expectedForward, 0, expectedForward, 2, 4);
            CollectionAssert.AreEqual(expectedForward, array.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Reinterpret_DifferentSize_Throws()
        {
            using var array = new ArrayUnsafe<int>(4, Allocator.Temp);

            Assert.Throws<InvalidOperationException>(() => array.Reinterpret<ushort>());
            Assert.Throws<InvalidOperationException>(() => array.Reinterpret<long>());
            Assert.Throws<InvalidOperationException>(() => array.Reinterpret<byte>());
        }

        [Test]
        public void Reinterpret_UncreatedArray_ReturnsAnEmptyView()
        {
            var array = default(ArrayUnsafe<int>);

            var view = array.Reinterpret<uint>();

            Assert.AreEqual(0, view.Length);
            Assert.AreEqual(0, view.AsSpan().Length);
        }

        [Test]
        public void Reinterpret_WriteThroughTheView_IsSeenByTheOwnerAndTheReverse()
        {
            using var floats = new ArrayUnsafe<int>(3, Allocator.Temp);
            using var wides = new ArrayUnsafe<Wide>(2, Allocator.Temp);

            var floatView = floats.Reinterpret<float>();
            var wideView = wides.Reinterpret<WideOther>();

            floatView[1] = 1.5f;
            wides[1] = new Wide { a = 1, b = -2, c = long.MaxValue };

            Assert.AreEqual(BitConverter.SingleToInt32Bits(1.5f), floats[1]);
            Assert.AreEqual(1L, wideView[1].x);
            Assert.AreEqual(-2L, wideView[1].y);
            Assert.AreEqual(long.MaxValue, wideView[1].z);

            floats[2] = BitConverter.SingleToInt32Bits(-2.25f);
            wideView[0] = new WideOther { x = long.MinValue, y = 0, z = -1 };

            Assert.AreEqual(-2.25f, floatView[2]);
            Assert.AreEqual(long.MinValue, wides[0].a);
            Assert.AreEqual(-1L, wides[0].c);
        }

        [Test]
        public void Reinterpret_DisposeView_LeavesTheOwnerUsable()
        {
            var array = new ArrayUnsafe<int>(2, Allocator.Persistent);

            try
            {
                array[0] = 5;

                var view = array.Reinterpret<uint>();
                view.Dispose();

                Assert.IsFalse(view.IsCreated);
                Assert.IsTrue(array.IsCreated);
                Assert.AreEqual(5, array[0]);

                array[1] = 6;

                CollectionAssert.AreEqual(new[] { 5, 6 }, array.ToArray());
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        public void ConvertExistingData_View_AliasesTheMemoryAndDisposeFreesNothing()
        {
            // SAFETY: The block holds three ints, outlives the view, and is freed exactly once in finally; the view's
            // Dispose releases nothing, so the block stays valid until then.
            unsafe
            {
                var memory = (int*)UnsafeUtility.Malloc(
                      sizeof(int) * 3
                    , UnsafeUtility.AlignOf<int>()
                    , Allocator.Persistent
                );

                try
                {
                    memory[0] = 1;
                    memory[1] = 2;
                    memory[2] = 3;

                    var view = ArrayUnsafe<int>.ConvertExistingData(memory, 3);

                    Assert.IsTrue(view.IsCreated);
                    Assert.AreEqual(3, view.Length);
                    CollectionAssert.AreEqual(new[] { 1, 2, 3 }, view.ToArray());

                    view[1] = 20;
                    memory[2] = 30;

                    Assert.AreEqual(20, memory[1]);
                    Assert.AreEqual(30, view[2]);
                    Assert.AreEqual(30, view.AsReadOnlySpan()[2]);

                    view.Dispose();

                    Assert.IsFalse(view.IsCreated);
                    Assert.AreEqual(30, memory[2]);
                }
                finally
                {
                    UnsafeUtility.Free(memory, Allocator.Persistent);
                }
            }
        }

        [Test]
        public void ConvertExistingData_ZeroLengthOrNullPointer_ReportsCreationByLengthAndPointer()
        {
            // SAFETY: The null pointer is only stored and compared, and is never dereferenced.
            unsafe
            {
                var empty = ArrayUnsafe<int>.ConvertExistingData(null, 0);
                var missing = ArrayUnsafe<int>.ConvertExistingData(null, 3);

                Assert.IsTrue(empty.IsCreated);
                Assert.AreEqual(0, empty.AsSpan().Length);
                Assert.DoesNotThrow(() => empty.Dispose());
                Assert.IsFalse(missing.IsCreated);
                Assert.Throws<ObjectDisposedException>(() => missing.Dispose());
            }
        }

        [Test]
        public void Enumerator_ZeroLengthArray_MovesNextOnlyToFalse()
        {
            var array = new ArrayUnsafe<int>(0, Allocator.Temp);

            try
            {
                var alias = array;
                var direct = new ArrayUnsafe<int>.Enumerator(ref alias);

                Assert.IsFalse(direct.MoveNext());

                direct.Reset();

                Assert.IsFalse(direct.MoveNext());
                direct.Dispose();
            }
            finally
            {
                array.Dispose();
            }
        }

        private static void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var array = new ArrayUnsafe<T>(3, Allocator.Temp);

            try
            {
                array[0] = a;
                array.AsSpan()[1] = b;
                array[2] = c;

                var span = array.AsReadOnlySpan();

                Assert.AreEqual(3, span.Length, name);
                Assert.AreEqual(a, span[0], name);
                Assert.AreEqual(b, span[1], name);
                Assert.AreEqual(c, span[2], name);
                CollectionAssert.AreEqual(new[] { a, b, c }, array.ToArray(), name);

                array.CopyFrom(new[] { c, b, a });

                Assert.AreEqual(c, array[0], name);
                Assert.AreEqual(b, array[1], name);
                Assert.AreEqual(a, array[2], name);

                var copy = new T[3];
                array.CopyTo(copy);

                CollectionAssert.AreEqual(new[] { c, b, a }, copy, name);
            }
            finally
            {
                array.Dispose();
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
