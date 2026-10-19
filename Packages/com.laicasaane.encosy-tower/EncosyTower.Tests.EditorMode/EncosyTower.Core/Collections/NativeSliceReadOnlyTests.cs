// Adapted from Unity.Collections.Tests/NativeArrayTests.cs slice cases.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EncosyTower.Collections;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class NativeSliceReadOnlyTests
    {
        [Test]
        public void Constructors_CreateExpectedWindows()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var nativeSlice = new NativeSlice<int>(array, 1, 3);
            var readOnlyArray = array.AsReadOnly();
            var empty = new NativeSliceReadOnly<int>();

            var fromSlice = new NativeSliceReadOnly<int>(nativeSlice);
            var fromSliceStart = new NativeSliceReadOnly<int>(nativeSlice, 1);
            var fromSliceRange = new NativeSliceReadOnly<int>(nativeSlice, 1, 1);
            var fromArray = new NativeSliceReadOnly<int>(array);
            var fromArrayStart = new NativeSliceReadOnly<int>(array, 2);
            var fromArrayRange = new NativeSliceReadOnly<int>(array, 1, 2);
            var fromReadOnly = new NativeSliceReadOnly<int>(readOnlyArray);
            var fromReadOnlyStart = new NativeSliceReadOnly<int>(readOnlyArray, 2);
            var fromReadOnlyRange = new NativeSliceReadOnly<int>(readOnlyArray, 1, 2);

            Assert.AreEqual(0, empty.Length);
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, fromSlice.ToArray());
            CollectionAssert.AreEqual(new[] { 3, 4 }, fromSliceStart.ToArray());
            CollectionAssert.AreEqual(new[] { 3 }, fromSliceRange.ToArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, fromArray.ToArray());
            CollectionAssert.AreEqual(new[] { 3, 4, 5 }, fromArrayStart.ToArray());
            CollectionAssert.AreEqual(new[] { 2, 3 }, fromArrayRange.ToArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, fromReadOnly.ToArray());
            CollectionAssert.AreEqual(new[] { 3, 4, 5 }, fromReadOnlyStart.ToArray());
            CollectionAssert.AreEqual(new[] { 2, 3 }, fromReadOnlyRange.ToArray());
        }

        [Test]
        public void SubSliceConstructors_CreateExpectedWindows()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var source = new NativeSliceReadOnly<int>(array);

            var copy = new NativeSliceReadOnly<int>(source);
            var fromStart = new NativeSliceReadOnly<int>(source, 2);
            var fromRange = new NativeSliceReadOnly<int>(source, 1, 3);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, copy.ToArray());
            CollectionAssert.AreEqual(new[] { 3, 4, 5 }, fromStart.ToArray());
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, fromRange.ToArray());
        }

        [Test]
        public void IndexerLengthAndStride_ReflectSource()
        {
            using var array = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Temp);
            var slice = new NativeSliceReadOnly<int>(array);

            Assert.AreEqual(3, slice.Length);
            Assert.AreEqual(UnsafeUtility.SizeOf<int>(), slice.Stride);
            Assert.AreEqual(10, slice[0]);
            Assert.AreEqual(30, slice[2]);
        }

        [Test]
        public void ImplicitConversions_CreateExpectedViews()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var nativeSlice = new NativeSlice<int>(array, 1, 2);
            var readOnlyArray = array.AsReadOnly();

            NativeSliceReadOnly<int> fromSlice = nativeSlice;
            NativeSliceReadOnly<int> fromArray = array;
            NativeSliceReadOnly<int> fromReadOnly = readOnlyArray;

            CollectionAssert.AreEqual(new[] { 2, 3 }, fromSlice.ToArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, fromArray.ToArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, fromReadOnly.ToArray());
        }

        [Test]
        public void SliceConvert_ReinterpretsContiguousBytes()
        {
            using var array = new NativeArray<int>(new[] { 0x01020304 }, Allocator.Temp);
            var slice = new NativeSliceReadOnly<int>(array);

            var bytes = slice.SliceConvert<byte>();

            Assert.AreEqual(UnsafeUtility.SizeOf<int>(), bytes.Length);
            CollectionAssert.AreEqual(BitConverter.GetBytes(array[0]), bytes.ToArray());
        }

        [Test]
        public void SliceWithStride_SelectsFieldFromEveryElement()
        {
            using var array = new NativeArray<Pair>(
                  new[] {
                      new Pair { First = 1, Second = 2 },
                      new Pair { First = 3, Second = 4 },
                  }
                , Allocator.Temp
            );
            var slice = new NativeSliceReadOnly<Pair>(array);

            var secondValues = slice.SliceWithStride<int>(UnsafeUtility.SizeOf<int>());
            var firstValues = slice.SliceWithStride<int>();

            CollectionAssert.AreEqual(new[] { 2, 4 }, secondValues.ToArray());
            CollectionAssert.AreEqual(new[] { 1, 3 }, firstValues.ToArray());
        }

        [Test]
        public void CopyToAndToArray_CopyEveryValue()
        {
            using var source = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            using var nativeDestination = new NativeArray<int>(3, Allocator.Temp);
            var managedDestination = new int[3];
            var slice = new NativeSliceReadOnly<int>(source);

            slice.CopyTo(nativeDestination);
            slice.CopyTo(managedDestination);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, nativeDestination.ToArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, managedDestination);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, slice.ToArray());
        }

        [Test]
        public void Enumerator_VisitsEveryValue()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var slice = new NativeSliceReadOnly<int>(array);
            var values = new List<int>();

            foreach (var value in slice)
            {
                values.Add(value);
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, values);
        }

        [Test]
        public void AsSpanEqualityHashAndOperators_UseViewIdentityAndExposeContent()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var slice = new NativeSliceReadOnly<int>(array);
            var alias = new NativeSliceReadOnly<int>(slice);
            var different = new NativeSliceReadOnly<int>(slice, 1);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, slice.AsSpan().ToArray());
            Assert.IsTrue(slice.Equals(alias));
            Assert.IsTrue(slice.Equals((object)alias));
            Assert.AreEqual(slice.GetHashCode(), alias.GetHashCode());
            Assert.IsTrue(slice == alias);
            Assert.IsFalse(slice != alias);
            Assert.IsFalse(slice == different);
            Assert.IsTrue(slice != different);
        }

        [Test]
        public void Enumerator_DirectConstructionResetDisposeAndInterfacesWork()
        {
            using var array = new NativeArray<int>(new[] { 7, 8 }, Allocator.Temp);
            var slice = new NativeSliceReadOnly<int>(array);
            var direct = new NativeSliceReadOnly<int>.Enumerator(ref slice);

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

            var fromOwner = slice.GetEnumerator();
            Assert.IsTrue(fromOwner.MoveNext());
            fromOwner.Dispose();

            IEnumerable<int> genericEnumerable = slice;
            var genericEnumerator = genericEnumerable.GetEnumerator();
            Assert.IsTrue(genericEnumerator.MoveNext());
            genericEnumerator.Dispose();

            IEnumerable enumerable = slice;
            var enumerator = enumerable.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(7, enumerator.Current);
        }

        private struct Pair
        {
            public int First;
            public int Second;
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Indexer_OutOfRange_Throws()
        {
            using var array = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Temp);
            using var pairs = new NativeArray<Pair>(
                  new[] { new Pair { First = 1, Second = 2 }, new Pair { First = 3, Second = 4 } }
                , Allocator.Temp
            );
            var slice = new NativeSliceReadOnly<int>(array);
            var subSlice = new NativeSliceReadOnly<int>(array, 1, 2);
            var strided = new NativeSliceReadOnly<Pair>(pairs).SliceWithStride<int>();

            Assert.Throws<IndexOutOfRangeException>(() => _ = slice[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = slice[3]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = subSlice[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = subSlice[2]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = strided[2]);
        }

        [Test]
        public void Indexer_ElementShapes_RoundTripBitExact()
        {
            using var small = new NativeArray<Rgb>(
                  new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 254, b = 253 } }
                , Allocator.Temp
            );
            using var longs = new NativeArray<long>(new[] { long.MinValue, 0L, long.MaxValue }, Allocator.Temp);
            using var wide = new NativeArray<Wide>(
                  new[] { new Wide { a = 1, b = -2, c = long.MaxValue } }
                , Allocator.Temp
            );

            var smallSlice = new NativeSliceReadOnly<Rgb>(small);
            var longSlice = new NativeSliceReadOnly<long>(longs);
            var wideSlice = new NativeSliceReadOnly<Wide>(wide);

            Assert.AreEqual(UnsafeUtility.SizeOf<Rgb>(), smallSlice.Stride);
            Assert.AreEqual(3, smallSlice.Stride);
            Assert.AreEqual(1, smallSlice[0].r);
            Assert.AreEqual(3, smallSlice[0].b);
            Assert.AreEqual(255, smallSlice[1].r);
            Assert.AreEqual(253, smallSlice[1].b);
            Assert.AreEqual(long.MinValue, longSlice[0]);
            Assert.AreEqual(long.MaxValue, longSlice[2]);
            Assert.AreEqual(UnsafeUtility.SizeOf<Wide>(), wideSlice.Stride);
            Assert.AreEqual(-2L, wideSlice[0].b);
            Assert.AreEqual(long.MaxValue, wideSlice[0].c);
        }

        [Test]
        public void AsSpan_StridedSlice_MatchesIndexer()
        {
            using var array = new NativeArray<Pair>(
                  new[] {
                      new Pair { First = 1, Second = 2 },
                      new Pair { First = 3, Second = 4 },
                      new Pair { First = 5, Second = 6 },
                  }
                , Allocator.Temp
            );
            var whole = new NativeSliceReadOnly<Pair>(array);
            var firsts = whole.SliceWithStride<int>();
            var seconds = whole.SliceWithStride<int>(UnsafeUtility.SizeOf<int>());

            AssertSpanMatchesIndexer(firsts, new[] { 1, 3, 5 });
            AssertSpanMatchesIndexer(seconds, new[] { 2, 4, 6 });
        }

        [Test]
        public void AsSpan_StridedSlice_SnapshotDoesNotObserveLaterSourceWrites()
        {
            using var array = new NativeArray<Pair>(
                  new[] { new Pair { First = 1, Second = 2 }, new Pair { First = 3, Second = 4 } }
                , Allocator.Temp
            );
            var seconds = new NativeSliceReadOnly<Pair>(array).SliceWithStride<int>(UnsafeUtility.SizeOf<int>());
            var snapshot = seconds.AsSpan();

            var writable = array;
            writable[0] = new Pair { First = 10, Second = 20 };
            writable[1] = new Pair { First = 30, Second = 40 };

            CollectionAssert.AreEqual(new[] { 2, 4 }, snapshot.ToArray());
            Assert.AreEqual(20, seconds[0]);
            Assert.AreEqual(40, seconds[1]);
        }

        [Test]
        public void AsSpan_EmptySlice_IsEmpty()
        {
            using var array = new NativeArray<int>(0, Allocator.Temp);
            var slice = new NativeSliceReadOnly<int>(array);

            Assert.AreEqual(0, slice.Length);
            Assert.AreEqual(0, slice.AsSpan().Length);
        }

        [Test]
        public void AsSpan_SubSliceAndWriteToSource_ViewsTheSourceMemory()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            try
            {
                var full = new NativeSliceReadOnly<int>(array);
                var tail = new NativeSliceReadOnly<int>(array, 2);
                var fullSpan = full.AsSpan();
                var tailSpan = tail.AsSpan();

                array[3] = 40;

                Assert.AreEqual(4, fullSpan.Length);
                Assert.AreEqual(2, tailSpan.Length);
                Assert.AreEqual(40, fullSpan[3]);
                Assert.AreEqual(3, tailSpan[0]);
                Assert.AreEqual(40, tailSpan[1]);
                Assert.AreEqual(40, tail[1]);
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        public void AsSpan_ThreeByteStruct_RoundTripBitExact()
        {
            using var array = new NativeArray<Rgb>(
                  new[] { new Rgb { r = 7, g = 8, b = 9 }, new Rgb { r = 10, g = 11, b = 12 } }
                , Allocator.Temp
            );
            var span = new NativeSliceReadOnly<Rgb>(array).AsSpan();

            Assert.AreEqual(2, span.Length);
            Assert.AreEqual(8, span[0].g);
            Assert.AreEqual(10, span[1].r);
            Assert.AreEqual(12, span[1].b);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void CopyTo_AfterSourceDispose_Throws()
        {
            // The managed-array overload pins its destination before it reads the slice, so the throw leaves one
            // pinned GCHandle behind (suspected defect D-07). It holds no native memory.
            var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var destination = new NativeArray<int>(3, Allocator.Persistent);

            try
            {
                var slice = new NativeSliceReadOnly<int>(array);

                array.Dispose();

                Assert.Catch(() => slice.CopyTo(new int[3]));
                Assert.Catch(() => slice.CopyTo(destination));
            }
            finally
            {
                destination.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Members_AfterSourceDispose_Throw()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var slice = new NativeSliceReadOnly<int>(array);
            var enumerator = slice.GetEnumerator();

            array.Dispose();

            Assert.Catch(() => _ = slice[0]);
            Assert.Catch(() => slice.ToArray());
            Assert.Catch(() => slice.AsSpan());
            Assert.Catch(() => _ = enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.Catch(() => _ = enumerator.Current);
            Assert.DoesNotThrow(() => enumerator.Reset());
            Assert.DoesNotThrow(() => enumerator.Dispose());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void CopyTo_LengthMismatch_Throws()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            using var shortNative = new NativeArray<int>(2, Allocator.Temp);
            var slice = new NativeSliceReadOnly<int>(array);
            var shortManaged = new int[2];

            Assert.Throws<ArgumentException>(() => slice.CopyTo(shortManaged));
            Assert.Throws<ArgumentException>(() => slice.CopyTo(new int[4]));
            Assert.Throws<ArgumentException>(() => slice.CopyTo(shortNative));
            CollectionAssert.AreEqual(new[] { 0, 0 }, shortManaged);
            CollectionAssert.AreEqual(new[] { 0, 0 }, shortNative.ToArray());
        }

        [Test]
        public void CopyToAndToArray_StridedSlice_CopyTheSelectedElements()
        {
            using var array = new NativeArray<Pair>(
                  new[] {
                      new Pair { First = 1, Second = 2 },
                      new Pair { First = 3, Second = 4 },
                      new Pair { First = 5, Second = 6 },
                  }
                , Allocator.Temp
            );
            using var nativeDestination = new NativeArray<int>(3, Allocator.Temp);
            var seconds = new NativeSliceReadOnly<Pair>(array).SliceWithStride<int>(UnsafeUtility.SizeOf<int>());
            var managedDestination = new int[3];

            seconds.CopyTo(managedDestination);
            seconds.CopyTo(nativeDestination);

            CollectionAssert.AreEqual(new[] { 2, 4, 6 }, managedDestination);
            CollectionAssert.AreEqual(new[] { 2, 4, 6 }, nativeDestination.ToArray());
            CollectionAssert.AreEqual(new[] { 2, 4, 6 }, seconds.ToArray());
        }

        [Test]
        public void CopyToAndToArray_EmptySlice_CopyNothing()
        {
            using var array = new NativeArray<int>(0, Allocator.Temp);
            using var nativeDestination = new NativeArray<int>(0, Allocator.Temp);
            var slice = new NativeSliceReadOnly<int>(array);

            slice.CopyTo(new int[0]);
            slice.CopyTo(nativeDestination);

            Assert.AreEqual(0, slice.ToArray().Length);
        }

        [Test]
        public void CopyToAndToArray_ElementShapes_RoundTripBitExact()
        {
            using var small = new NativeArray<Rgb>(
                  new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 4, g = 5, b = 6 } }
                , Allocator.Temp
            );
            using var wide = new NativeArray<Wide>(
                  new[] { new Wide { a = long.MinValue, b = 0, c = long.MaxValue } }
                , Allocator.Temp
            );

            var smallCopy = new NativeSliceReadOnly<Rgb>(small).ToArray();
            var wideCopy = new NativeSliceReadOnly<Wide>(wide).ToArray();

            Assert.AreEqual(2, smallCopy.Length);
            Assert.AreEqual(2, smallCopy[0].g);
            Assert.AreEqual(4, smallCopy[1].r);
            Assert.AreEqual(6, smallCopy[1].b);
            Assert.AreEqual(long.MinValue, wideCopy[0].a);
            Assert.AreEqual(long.MaxValue, wideCopy[0].c);
        }

        [Test]
        public void Enumerator_EmptySlice_VisitsNothing()
        {
            using var array = new NativeArray<int>(0, Allocator.Temp);
            var slice = new NativeSliceReadOnly<int>(array);
            var direct = new NativeSliceReadOnly<int>.Enumerator(ref slice);
            var fromOwner = slice.GetEnumerator();
            var generic = ((IEnumerable<int>)slice).GetEnumerator();
            var nonGeneric = ((IEnumerable)slice).GetEnumerator();

            Assert.IsFalse(direct.MoveNext());
            Assert.IsFalse(fromOwner.MoveNext());
            Assert.IsFalse(generic.MoveNext());
            Assert.IsFalse(nonGeneric.MoveNext());
        }

        [Test]
        public void Enumerator_StridedSlice_VisitsTheSelectedElements()
        {
            using var array = new NativeArray<Pair>(
                  new[] { new Pair { First = 1, Second = 2 }, new Pair { First = 3, Second = 4 } }
                , Allocator.Temp
            );
            var seconds = new NativeSliceReadOnly<Pair>(array).SliceWithStride<int>(UnsafeUtility.SizeOf<int>());
            var visited = new List<int>();

            foreach (var value in seconds)
            {
                visited.Add(value);
            }

            CollectionAssert.AreEqual(new[] { 2, 4 }, visited);
        }

        [Test]
        public void Enumerator_DisposeAndResetOnAnExhaustedEnumerator_AreRepeatable()
        {
            using var array = new NativeArray<int>(new[] { 7 }, Allocator.Temp);
            var slice = new NativeSliceReadOnly<int>(array);
            var enumerator = slice.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Dispose();
            enumerator.Dispose();
            enumerator.Reset();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(7, enumerator.Current);
        }

        [Test]
        public void Equals_DifferentRangeStrideOrType_IsFalse()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            using var pairs = new NativeArray<Pair>(
                  new[] { new Pair { First = 1, Second = 2 }, new Pair { First = 3, Second = 4 } }
                , Allocator.Temp
            );
            var full = new NativeSliceReadOnly<int>(array);
            var head = new NativeSliceReadOnly<int>(array, 0, 2);
            var tail = new NativeSliceReadOnly<int>(array, 2);
            var whole = new NativeSliceReadOnly<Pair>(pairs);
            var firsts = whole.SliceWithStride<int>();
            var firstsAgain = whole.SliceWithStride<int>(0);
            var seconds = whole.SliceWithStride<int>(UnsafeUtility.SizeOf<int>());

            Assert.IsFalse(full.Equals(head));
            Assert.IsFalse(full == tail);
            Assert.IsTrue(full != head);
            Assert.IsFalse(full.Equals((object)head));
            Assert.IsFalse(full.Equals((object)null));
            Assert.IsFalse(full.Equals((object)5));
            Assert.IsTrue(firsts == firstsAgain);
            Assert.AreEqual(firsts.GetHashCode(), firstsAgain.GetHashCode());
            Assert.IsFalse(firsts == seconds);
            Assert.IsTrue(default(NativeSliceReadOnly<int>) == default(NativeSliceReadOnly<int>));
            Assert.IsFalse(default(NativeSliceReadOnly<int>) == full);
        }

        [Test]
        public void Equals_AfterSourceDispose_StillComparesTheViewIdentity()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Persistent);
            var slice = new NativeSliceReadOnly<int>(array);
            var alias = new NativeSliceReadOnly<int>(slice);
            var different = new NativeSliceReadOnly<int>(slice, 1);

            array.Dispose();

            Assert.IsTrue(slice.Equals(alias));
            Assert.IsTrue(slice == alias);
            Assert.IsFalse(slice == different);
            Assert.AreEqual(slice.GetHashCode(), alias.GetHashCode());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ArrayConstructors_InvalidRange_Throw()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var readOnlyArray = array.AsReadOnly();

            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NativeSliceReadOnly<int>(array, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NativeSliceReadOnly<int>(array, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NativeSliceReadOnly<int>(array, 4));
            Assert.Throws<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(array, 1, 3));
            Assert.Throws<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(array, 4, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NativeSliceReadOnly<int>(readOnlyArray, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NativeSliceReadOnly<int>(readOnlyArray, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NativeSliceReadOnly<int>(readOnlyArray, 4));
            Assert.Throws<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(readOnlyArray, 1, 3));
            Assert.Throws<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(readOnlyArray, 4, 0));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void SliceConstructors_InvalidRange_Throw()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var nativeSlice = new NativeSlice<int>(array);
            var source = new NativeSliceReadOnly<int>(array);

            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NativeSliceReadOnly<int>(nativeSlice, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NativeSliceReadOnly<int>(nativeSlice, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NativeSliceReadOnly<int>(nativeSlice, 4));
            Assert.Throws<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(nativeSlice, 1, 3));
            Assert.Throws<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(nativeSlice, 4, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NativeSliceReadOnly<int>(source, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NativeSliceReadOnly<int>(source, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NativeSliceReadOnly<int>(source, 4));
            Assert.Throws<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(source, 1, 3));
            Assert.Throws<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(source, 4, 0));
        }

        [Test]
        public void Constructors_BoundaryRanges_CreateEmptyAndFullViews()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var nativeSlice = new NativeSlice<int>(array);
            var source = new NativeSliceReadOnly<int>(array);

            Assert.AreEqual(0, new NativeSliceReadOnly<int>(array, 3, 0).Length);
            Assert.AreEqual(0, new NativeSliceReadOnly<int>(array, 3).Length);
            Assert.AreEqual(0, new NativeSliceReadOnly<int>(nativeSlice, 3, 0).Length);
            Assert.AreEqual(0, new NativeSliceReadOnly<int>(source, 3, 0).Length);
            Assert.AreEqual(0, new NativeSliceReadOnly<int>(source, 3).Length);
            Assert.AreEqual(3, new NativeSliceReadOnly<int>(source, 0, 3).Length);
            Assert.AreEqual(1, new NativeSliceReadOnly<int>(source, 2, 1).Length);
            Assert.AreEqual(3, new NativeSliceReadOnly<int>(source, 2, 1)[0]);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void SubSliceConstructors_LengthOverflow_Throw()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var nativeSlice = new NativeSlice<int>(array);
            var source = new NativeSliceReadOnly<int>(array);

            Assert.Catch<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(source, 1, int.MaxValue));
            Assert.Catch<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(source, int.MaxValue, int.MaxValue));
            Assert.Catch<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(source, 2, int.MaxValue - 1));
            Assert.Catch<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(nativeSlice, 1, int.MaxValue));
            Assert.Catch<ArgumentException>(
                () => _ = new NativeSliceReadOnly<int>(nativeSlice, int.MaxValue, int.MaxValue)
            );
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ArrayConstructors_LengthOverflow_Throw()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var readOnlyArray = array.AsReadOnly();

            Assert.Catch<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(array, 1, int.MaxValue));
            Assert.Catch<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(array, int.MaxValue, int.MaxValue));
            Assert.Catch<ArgumentException>(() => _ = new NativeSliceReadOnly<int>(readOnlyArray, 1, int.MaxValue));
            Assert.Catch<ArgumentException>(
                () => _ = new NativeSliceReadOnly<int>(readOnlyArray, int.MaxValue, int.MaxValue)
            );
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        [Test]
        public unsafe void BorrowedHandleConstructor_ViewsTheBufferAndFollowsTheHandle()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Persistent);

            try
            {
                var handle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(array);
                var pointer = NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(array);
                var slice = new NativeSliceReadOnly<int>(pointer, sizeof(int), 3, handle);
                var empty = new NativeSliceReadOnly<int>(pointer, sizeof(int), 0, handle);

                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, slice.ToArray());
                Assert.AreEqual(0, empty.Length);
                Assert.AreEqual(0, empty.ToArray().Length);

                array.Dispose();

                Assert.Catch(() => _ = slice[0]);
            }
            finally
            {
                if (array.IsCreated)
                {
                    array.Dispose();
                }
            }
        }
#endif

        [Test]
        public void ImplicitConversions_ElementShapes_ViewTheSourceBitExact()
        {
            using var small = new NativeArray<Rgb>(
                  new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 254, b = 253 } }
                , Allocator.Temp
            );
            using var wide = new NativeArray<Wide>(
                  new[] { new Wide { a = long.MinValue, b = 7, c = -1 } }
                , Allocator.Temp
            );

            NativeSliceReadOnly<Rgb> fromArray = small;
            NativeSliceReadOnly<Rgb> fromSlice = new NativeSlice<Rgb>(small, 1, 1);
            NativeSliceReadOnly<Wide> fromReadOnly = wide.AsReadOnly();

            Assert.AreEqual(2, fromArray.Length);
            Assert.AreEqual(3, fromArray.Stride);
            Assert.AreEqual(2, fromArray[0].g);
            Assert.AreEqual(1, fromSlice.Length);
            Assert.AreEqual(255, fromSlice[0].r);
            Assert.AreEqual(253, fromSlice[0].b);
            Assert.AreEqual(1, fromReadOnly.Length);
            Assert.AreEqual(long.MinValue, fromReadOnly[0].a);
            Assert.AreEqual(-1L, fromReadOnly[0].c);
        }

        [Test]
        public void ImplicitConversions_EmptySources_CreateEmptyViews()
        {
            using var array = new NativeArray<int>(0, Allocator.Temp);
            var nativeSlice = new NativeSlice<int>(array);

            NativeSliceReadOnly<int> fromArray = array;
            NativeSliceReadOnly<int> fromSlice = nativeSlice;
            NativeSliceReadOnly<int> fromReadOnly = array.AsReadOnly();

            Assert.AreEqual(0, fromArray.Length);
            Assert.AreEqual(0, fromSlice.Length);
            Assert.AreEqual(0, fromReadOnly.Length);
        }

        [Test]
        public void SliceConvert_IntToLong_MatchesMemoryMarshalCast()
        {
            var source = new[] { 1, 2, 3, -4 };
            using var array = new NativeArray<int>(source, Allocator.Temp);
            var converted = new NativeSliceReadOnly<int>(array).SliceConvert<long>();
            var expected = MemoryMarshal.Cast<int, long>(source.AsSpan()).ToArray();

            Assert.AreEqual(2, converted.Length);
            Assert.AreEqual(UnsafeUtility.SizeOf<long>(), converted.Stride);
            CollectionAssert.AreEqual(expected, converted.ToArray());
        }

        [Test]
        public void SliceConvert_ThreeByteStructToBytes_ViewsEveryByte()
        {
            using var array = new NativeArray<Rgb>(
                  new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 4, g = 5, b = 6 } }
                , Allocator.Temp
            );
            var bytes = new NativeSliceReadOnly<Rgb>(array).SliceConvert<byte>();

            Assert.AreEqual(6, bytes.Length);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6 }, bytes.ToArray());
        }

        [Test]
        public void SliceConvert_ZeroLengthSlice_ReturnsAnEmptyView()
        {
            using var array = new NativeArray<int>(0, Allocator.Temp);

            var converted = new NativeSliceReadOnly<int>(array).SliceConvert<byte>();

            Assert.AreEqual(0, converted.Length);
        }

        [Test]
        public void SliceConvert_WriteToSource_IsSeenByTheView()
        {
            var array = new NativeArray<int>(new[] { 0, 0 }, Allocator.Temp);

            try
            {
                var bytes = new NativeSliceReadOnly<int>(array).SliceConvert<byte>();
                var longs = new NativeSliceReadOnly<int>(array).SliceConvert<long>();

                array[0] = 0x0A0B0C0D;
                array[1] = 1;

                CollectionAssert.AreEqual(BitConverter.GetBytes(0x0A0B0C0D), bytes.ToArray().AsSpan(0, 4).ToArray());
                Assert.AreEqual(1, longs.Length);
                Assert.AreEqual(0x0A0B0C0DL | (1L << 32), longs[0]);
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void SliceConvert_LengthNotMultipleOfTargetSize_Throws()
        {
            using var array = new NativeArray<byte>(new byte[] { 1, 2, 3 }, Allocator.Temp);
            var slice = new NativeSliceReadOnly<byte>(array);

            Assert.Throws<InvalidOperationException>(() => slice.SliceConvert<int>());
            Assert.AreEqual(3, slice.SliceConvert<byte>().Length);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void SliceConvert_StridedSlice_Throws()
        {
            using var array = new NativeArray<Pair>(
                  new[] { new Pair { First = 1, Second = 2 }, new Pair { First = 3, Second = 4 } }
                , Allocator.Temp
            );
            var firsts = new NativeSliceReadOnly<Pair>(array).SliceWithStride<int>();

            Assert.Throws<InvalidOperationException>(() => firsts.SliceConvert<byte>());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void SliceConvert_AfterSourceDispose_ReadsThroughTheViewThrow()
        {
            var array = new NativeArray<int>(new[] { 1, 2 }, Allocator.Persistent);
            var converted = new NativeSliceReadOnly<int>(array).SliceConvert<byte>();

            array.Dispose();

            Assert.Catch(() => _ = converted[0]);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void SliceWithStride_InvalidOffsetOrSize_Throws()
        {
            using var pairs = new NativeArray<Pair>(
                  new[] { new Pair { First = 1, Second = 2 }, new Pair { First = 3, Second = 4 } }
                , Allocator.Temp
            );
            using var small = new NativeArray<Rgb>(new[] { new Rgb { r = 1, g = 2, b = 3 } }, Allocator.Temp);
            var pairSlice = new NativeSliceReadOnly<Pair>(pairs);
            var smallSlice = new NativeSliceReadOnly<Rgb>(small);

            Assert.Throws<ArgumentOutOfRangeException>(() => pairSlice.SliceWithStride<int>(-1));
            Assert.Throws<ArgumentException>(() => pairSlice.SliceWithStride<int>(5));
            Assert.Throws<ArgumentException>(() => pairSlice.SliceWithStride<long>(4));
            Assert.Throws<ArgumentException>(() => pairSlice.SliceWithStride<int>(8));
            Assert.Throws<ArgumentException>(() => smallSlice.SliceWithStride<byte>(3));
            Assert.Throws<ArgumentException>(() => smallSlice.SliceWithStride<short>(2));
            Assert.AreEqual(1, smallSlice.SliceWithStride<byte>(2).Length);
        }

        [Test]
        public void SliceWithStride_ElementShapes_SelectTheFieldBitExact()
        {
            using var small = new NativeArray<Rgb>(
                  new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 4, g = 5, b = 6 } }
                , Allocator.Temp
            );
            using var wide = new NativeArray<Wide>(
                  new[] { new Wide { a = 1, b = 2, c = 3 }, new Wide { a = 4, b = long.MinValue, c = long.MaxValue } }
                , Allocator.Temp
            );
            var smallSlice = new NativeSliceReadOnly<Rgb>(small);
            var wideSlice = new NativeSliceReadOnly<Wide>(wide);

            var greens = smallSlice.SliceWithStride<byte>(1);
            var blues = smallSlice.SliceWithStride<byte>(2);
            var wideB = wideSlice.SliceWithStride<long>(8);
            var wideC = wideSlice.SliceWithStride<long>(16);

            CollectionAssert.AreEqual(new byte[] { 2, 5 }, greens.ToArray());
            CollectionAssert.AreEqual(new byte[] { 3, 6 }, blues.ToArray());
            Assert.AreEqual(3, greens.Stride);
            CollectionAssert.AreEqual(new[] { 2L, long.MinValue }, wideB.ToArray());
            CollectionAssert.AreEqual(new[] { 3L, long.MaxValue }, wideC.ToArray());
        }

        [Test]
        public void SliceWithStride_ZeroLengthSlice_ReturnsAnEmptyView()
        {
            using var array = new NativeArray<Pair>(0, Allocator.Temp);

            var firsts = new NativeSliceReadOnly<Pair>(array).SliceWithStride<int>();

            Assert.AreEqual(0, firsts.Length);
            Assert.AreEqual(0, firsts.ToArray().Length);
        }

        [Test]
        public void SliceWithStride_WriteToSource_IsSeenByTheView()
        {
            var array = new NativeArray<Pair>(
                  new[] { new Pair { First = 1, Second = 2 }, new Pair { First = 3, Second = 4 } }
                , Allocator.Temp
            );

            try
            {
                var seconds = new NativeSliceReadOnly<Pair>(array).SliceWithStride<int>(UnsafeUtility.SizeOf<int>());

                array[1] = new Pair { First = 30, Second = 40 };

                CollectionAssert.AreEqual(new[] { 2, 40 }, seconds.ToArray());
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        public void SliceWithStride_OfAStridedView_AddsTheOffsets()
        {
            using var array = new NativeArray<Pair>(
                  new[] { new Pair { First = 0x0102, Second = 0x0304 }, new Pair { First = 0x0506, Second = 0x0708 } }
                , Allocator.Temp
            );
            var seconds = new NativeSliceReadOnly<Pair>(array).SliceWithStride<int>(UnsafeUtility.SizeOf<int>());

            var secondBytes = seconds.SliceWithStride<byte>(1);

            Assert.AreEqual(2, secondBytes.Length);
            Assert.AreEqual(BitConverter.GetBytes(0x0304)[1], secondBytes[0]);
            Assert.AreEqual(BitConverter.GetBytes(0x0708)[1], secondBytes[1]);
        }

        [Test]
        public void DebugView_ItemsMatchTheSliceContent()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            using var empty = new NativeArray<int>(0, Allocator.Temp);

            var view = new NativeSliceReadOnlyDebugView<int>(new NativeSliceReadOnly<int>(array, 1));
            var emptyView = new NativeSliceReadOnlyDebugView<int>(new NativeSliceReadOnly<int>(empty));

            CollectionAssert.AreEqual(new[] { 2, 3 }, view.Items);
            Assert.AreEqual(0, emptyView.Items.Length);
        }

        [Test]
        public void ReadOnlyArrayConstructors_EmptyRangeAtTheEnd_CreateEmptyViews()
        {
            // The NativeArray, NativeSlice and NativeSliceReadOnly constructors accept the empty range at the end
            // (start == Length); the managed equivalent (Span<T>.Slice(Length, 0)) does too.
            using var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var readOnlyArray = array.AsReadOnly();

            Assert.AreEqual(0, new NativeSliceReadOnly<int>(readOnlyArray, 3, 0).Length);
            Assert.AreEqual(0, new NativeSliceReadOnly<int>(readOnlyArray, 3).Length);
        }

        private static void AssertSpanMatchesIndexer(NativeSliceReadOnly<int> slice, int[] expected)
        {
            var span = slice.AsSpan();

            Assert.AreEqual(slice.Length, span.Length);

            for (var i = 0; i < slice.Length; i++)
            {
                Assert.AreEqual(expected[i], slice[i], $"indexer {i}");
                Assert.AreEqual(slice[i], span[i], $"span {i}");
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
