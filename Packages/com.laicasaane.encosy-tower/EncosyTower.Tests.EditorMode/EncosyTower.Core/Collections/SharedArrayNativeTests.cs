using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedArrayNativeTests
    {
        [Test]
        public void CreatedStateLengthIndexerAndElementReferenceReflectOwner()
        {
            SharedArrayNative<int> uncreated = default;
            using var owner = new SharedArray<int>(new[] { 1, 2, 3 });

            // SAFETY: owner remains live while the borrowed view and element reference are used.
            unsafe
            {
                var array = owner.AsNative();

                ref var item = ref array.ElementAt(1);
                item = 20;

                Assert.IsFalse(uncreated.IsCreated);
                Assert.IsTrue(array.IsCreated);
                Assert.AreEqual(3, array.Length);
                Assert.AreEqual(20, owner[1]);
            }
        }

        [Test]
        public void CopyClearSpansAndNativeSliceOperateOnBorrowedStorage()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3, 4 });

            // SAFETY: owner remains live while all borrowed spans, slices, and the native view are consumed.
            unsafe
            {
                var array = owner.AsNative();

                array.CopyFrom(1, new[] { 8, 9 }, 2);
                CollectionAssert.AreEqual(new[] { 1, 8, 9, 4 }, array.AsReadOnlySpan().ToArray());

                var destination = new int[2];
                array.CopyTo(1, destination, 2);
                CollectionAssert.AreEqual(new[] { 8, 9 }, destination);

                array.AsSpan()[0] = 7;
                var nativeSlice = array.AsNativeSlice();
                nativeSlice[3] = 6;
                CollectionAssert.AreEqual(new[] { 7, 8, 9, 6 }, array.ToArray());

                array.Clear();
                CollectionAssert.AreEqual(new[] { 0, 0, 0, 0 }, owner.AsManagedArray());
            }
        }

        [Test]
        public void Reinterpret_EqualSizeAliasesStorageAndDifferentSizeThrows()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3 });

            // SAFETY: owner remains live while equal-size aliases and the rejection path are exercised.
            unsafe
            {
                var array = owner.AsNative();
                var reinterpreted = array.Reinterpret<uint>();

                reinterpreted[0] = 42;

                Assert.AreEqual(42, owner[0]);
                Assert.Throws<InvalidOperationException>(() => array.Reinterpret<ushort>());
            }
        }

        [Test]
        public void Enumeration_VisitsFixedLengthAndSupportsInterfaces()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3 });
            var values = new List<int>();

            // SAFETY: owner remains live and unmodified while every borrowed enumerator is consumed.
            unsafe
            {
                var array = owner.AsNative();

                foreach (var value in array)
                {
                    values.Add(value);
                }

                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, values);

                IEnumerable<int> generic = array;
                using var genericEnumerator = generic.GetEnumerator();
                Assert.IsTrue(genericEnumerator.MoveNext());
                Assert.AreEqual(1, genericEnumerator.Current);

                IEnumerable nongeneric = array;
                var enumerator = nongeneric.GetEnumerator();
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(1, enumerator.Current);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ViewCreatedBeforeOwnerDispose_ThrowsOnAccess()
        {
            var owner = new SharedArray<int>(new[] { 1, 2, 3 });

            // SAFETY: This test intentionally keeps a borrowed view past owner disposal to verify safety checks.
            unsafe
            {
                var stale = owner.AsNative();

                owner.Dispose();

                Assert.Catch(() => _ = stale[0]);
            }
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
        [Test]
        public unsafe void Constructor_BorrowedBuffer_ViewsTheBufferAndFollowsTheHandle()
        {
            var managed = new[] { 1, 2, 3 };
            var safety = AtomicSafetyHandle.Create();
            var released = false;

            try
            {
                fixed (int* pointer = managed)
                {
                    var view = new SharedArrayNative<int>(pointer, 3, safety);
                    var empty = new SharedArrayNative<int>(pointer, 0, safety);
                    var nullBuffer = new SharedArrayNative<int>(null, 0, safety);

                    Assert.IsTrue(view.IsCreated);
                    Assert.IsTrue(empty.IsCreated);
                    Assert.IsTrue(nullBuffer.IsCreated);
                    Assert.AreEqual(3, view.Length);
                    Assert.AreEqual(0, empty.Length);
                    Assert.AreEqual(0, nullBuffer.Length);
                    CollectionAssert.AreEqual(new[] { 1, 2, 3 }, view.ToArray());
                    Assert.AreEqual(0, empty.ToArray().Length);
                    Assert.AreEqual(0, nullBuffer.AsSpan().Length);
                    Assert.AreEqual(0, nullBuffer.AsReadOnlySpan().Length);
                    Assert.DoesNotThrow(() => nullBuffer.Clear());

                    view[1] = 20;

                    Assert.AreEqual(20, managed[1]);

                    AtomicSafetyHandle.Release(safety);
                    released = true;

                    Assert.Catch(() => _ = view[0]);
                    Assert.Catch(() => _ = empty.ToArray());
                }
            }
            finally
            {
                if (released == false)
                {
                    AtomicSafetyHandle.Release(safety);
                }
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ZeroLengthView_IndexerAndElementAtThrowAndEnumerationIsEmpty()
        {
            var safety = AtomicSafetyHandle.Create();

            try
            {
                var view = new SharedArrayNative<int>(null, 0, safety);

                Assert.Throws<InvalidOperationException>(() => _ = view[0]);
                Assert.Throws<InvalidOperationException>(() => view[0] = 1);
                Assert.Throws<InvalidOperationException>(() => view.ElementAt(0));
                Assert.IsFalse(view.GetEnumerator().MoveNext());
                Assert.IsFalse(((IEnumerable<int>)view).GetEnumerator().MoveNext());
                Assert.IsFalse(((IEnumerable)view).GetEnumerator().MoveNext());
                Assert.IsTrue(view.TryCopyTo(Span<int>.Empty));
                Assert.IsTrue(view.TryCopyFrom(ReadOnlySpan<int>.Empty));
                Assert.IsFalse(view.TryCopyTo(new int[1]));
            }
            finally
            {
                AtomicSafetyHandle.Release(safety);
            }
        }

        [Test]
        public unsafe void ReadOnlyConstructor_BorrowedBuffer_ViewsTheBufferAndFollowsTheHandle()
        {
            var managed = new[] { 1, 2, 3 };
            var safety = AtomicSafetyHandle.Create();
            var released = false;

            try
            {
                fixed (int* pointer = managed)
                {
                    var view = new SharedArrayNative<int>.ReadOnly(pointer, 3, safety);
                    var nullBuffer = new SharedArrayNative<int>.ReadOnly(null, 0, safety);

                    Assert.IsTrue(view.IsCreated);
                    Assert.IsTrue(nullBuffer.IsCreated);
                    Assert.AreEqual(3, view.Length);
                    Assert.AreEqual(3, view.Count);
                    Assert.AreEqual(0, nullBuffer.Length);
                    Assert.AreEqual(2, view[1]);
                    Assert.AreEqual(0, nullBuffer.ToArray().Length);
                    Assert.IsFalse(nullBuffer.GetEnumerator().MoveNext());

                    AtomicSafetyHandle.Release(safety);
                    released = true;

                    Assert.Catch(() => _ = view[0]);
                }
            }
            finally
            {
                if (released == false)
                {
                    AtomicSafetyHandle.Release(safety);
                }
            }
        }
#endif

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void IndexerAndElementAt_OutOfRange_Throw()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3 });
            var array = owner.AsNative();

            Assert.Throws<InvalidOperationException>(() => _ = array[-1]);
            Assert.Throws<InvalidOperationException>(() => _ = array[3]);
            Assert.Throws<InvalidOperationException>(() => _ = array[int.MaxValue]);
            Assert.Throws<InvalidOperationException>(() => array[3] = 9);
            Assert.Throws<InvalidOperationException>(() => array.ElementAt(3));
            Assert.Throws<InvalidOperationException>(() => array.ElementAt(-1));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, owner.AsManagedArray());
        }

        [Test]
        public unsafe void IndexerSetterAndElementAt_WriteThroughToTheOwner()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3 });
            var array = owner.AsNative();

            array[0] = 10;

            ref var element = ref array.ElementAt(2);
            element = 30;

            var span = array.AsSpan();

            span[1] = 20;

            CollectionAssert.AreEqual(new[] { 10, 20, 30 }, owner.AsManagedArray());
            Assert.AreEqual(20, array[1]);
        }

        [Test]
        public unsafe void CopyFrom_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsContent()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3, 4 });
            var array = owner.AsNative();
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
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, owner.AsManagedArray());
        }

        [Test]
        public unsafe void CopyTo_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3, 4 });
            var array = owner.AsNative();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyTo(-1, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyTo(5, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyTo(0, destination, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.CopyTo(0, new int[2], 3));
            Assert.IsFalse(array.TryCopyTo(-1, destination));
            Assert.IsFalse(array.TryCopyTo(0, destination, 5));
            Assert.IsFalse(array.TryCopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public unsafe void CopyFrom_SourceOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var owner = new SharedArray<int>((int[])seed.Clone());
            var array = owner.AsNative();

            array.CopyFrom(1, array.AsReadOnlySpan()[..4]);

            var expectedForward = (int[])seed.Clone();
            Array.Copy(expectedForward, 0, expectedForward, 1, 4);
            CollectionAssert.AreEqual(expectedForward, owner.AsManagedArray());

            array.CopyFrom(seed);
            array.CopyFrom(0, array.AsReadOnlySpan()[1..5]);

            var expectedBackward = (int[])seed.Clone();
            Array.Copy(expectedBackward, 1, expectedBackward, 0, 4);
            CollectionAssert.AreEqual(expectedBackward, owner.AsManagedArray());
        }

        [Test]
        public unsafe void CopyTo_DestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var owner = new SharedArray<int>((int[])seed.Clone());
            var array = owner.AsNative();

            array.CopyTo(2, array.AsSpan()[..4]);

            var expectedBackward = (int[])seed.Clone();
            Array.Copy(expectedBackward, 2, expectedBackward, 0, 4);
            CollectionAssert.AreEqual(expectedBackward, owner.AsManagedArray());

            array.CopyFrom(seed);
            array.CopyTo(0, array.AsSpan()[2..]);

            var expectedForward = (int[])seed.Clone();
            Array.Copy(expectedForward, 0, expectedForward, 2, 4);
            CollectionAssert.AreEqual(expectedForward, owner.AsManagedArray());
        }

        [Test]
        public unsafe void Clear_ElementShapes_ZeroEveryByte()
        {
            using var small = new SharedArray<Rgb>(
                new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 4, g = 5, b = 6 } }
            );
            using var wide = new SharedArray<Wide>(
                new[] { new Wide { a = 1, b = -2, c = long.MaxValue } }
            );
            var smallView = small.AsNative();
            var wideView = wide.AsNative();

            smallView.Clear();
            wideView.Clear();

            Assert.AreEqual(0, small.AsManagedArray()[0].r);
            Assert.AreEqual(0, small.AsManagedArray()[1].b);
            Assert.AreEqual(0L, wide.AsManagedArray()[0].a);
            Assert.AreEqual(0L, wide.AsManagedArray()[0].c);
        }

        [Test]
        public unsafe void Spans_ElementShapes_RoundTripBitExactAndAliasTheOwner()
        {
            using var small = new SharedArray<Rgb>(new[] { new Rgb { r = 1, g = 2, b = 3 } });
            using var wide = new SharedArray<Wide>(
                new[] { new Wide { a = long.MinValue, b = 0, c = long.MaxValue } }
            );
            var smallView = small.AsNative();
            var wideView = wide.AsNative();

            Assert.AreEqual(2, smallView.AsReadOnlySpan()[0].g);
            Assert.AreEqual(long.MinValue, wideView.AsReadOnlySpan()[0].a);
            Assert.AreEqual(long.MaxValue, wideView.ToArray()[0].c);

            smallView.AsSpan()[0] = new Rgb { r = 9, g = 8, b = 7 };
            wideView[0] = new Wide { a = 1, b = 2, c = 3 };

            Assert.AreEqual(7, small.AsManagedArray()[0].b);
            Assert.AreEqual(3L, wide.AsManagedArray()[0].c);
        }

        [Test]
        public unsafe void AsNativeSlice_AliasesTheOwnerAndSeesWritesInBothDirections()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3, 4 });
            var array = owner.AsNative();

            var slice = array.AsNativeSlice();

            Assert.AreEqual(4, slice.Length);

            slice[0] = 10;
            array[3] = 40;

            Assert.AreEqual(10, owner[0]);
            Assert.AreEqual(40, slice[3]);
            CollectionAssert.AreEqual(new[] { 10, 2, 3, 40 }, slice.ToArray());
        }

        [Test]
        public unsafe void AsNativeSlice_ElementShapes_RoundTripBitExact()
        {
            using var small = new SharedArray<Rgb>(
                new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 4, g = 5, b = 6 } }
            );
            var slice = small.AsNative().AsNativeSlice();

            Assert.AreEqual(2, slice.Length);
            Assert.AreEqual(3, slice[0].b);
            Assert.AreEqual(4, slice[1].r);
        }

        [Test]
        public unsafe void Reinterpret_WriteThroughTheViewAndWideElements_IsSeenByTheOwner()
        {
            using var floats = new SharedArray<int>(new[] { 0, 0 });
            using var wides = new SharedArray<Wide>(new Wide[2]);
            var floatView = floats.AsNative().Reinterpret<float>();
            var wideView = wides.AsNative().Reinterpret<WideOther>();

            floatView[1] = 1.5f;
            wideView[1] = new WideOther { x = 1, y = -2, z = long.MaxValue };

            Assert.AreEqual(BitConverter.SingleToInt32Bits(1.5f), floats[1]);
            Assert.AreEqual(1L, wides[1].a);
            Assert.AreEqual(-2L, wides[1].b);
            Assert.AreEqual(long.MaxValue, wides[1].c);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Reinterpret_DifferentSize_Throws()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2 });
            var array = owner.AsNative();

            Assert.Throws<InvalidOperationException>(() => array.Reinterpret<byte>());
            Assert.Throws<InvalidOperationException>(() => array.Reinterpret<long>());
            Assert.Throws<InvalidOperationException>(() => array.Reinterpret<Rgb>());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Enumerator_CurrentOutsideTheArray_Throws()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2 });
            var array = owner.AsNative();
            var enumerator = array.GetEnumerator();

            Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsTrue(enumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);

            enumerator.Reset();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Members_AfterOwnerDispose_Throw()
        {
            var owner = new SharedArray<int>(new[] { 1, 2, 3 });
            var array = owner.AsNative();
            var enumerator = array.GetEnumerator();

            owner.Dispose();

            Assert.Catch(() => _ = array[0]);
            Assert.Catch(() => array[0] = 1);
            Assert.Catch(() => array.ElementAt(0));
            Assert.Catch(() => array.CopyFrom(new[] { 1 }));
            Assert.Catch(() => array.CopyFrom(0, new[] { 1 }, 1));
            Assert.Catch(() => array.TryCopyFrom(new[] { 1 }));
            Assert.Catch(() => array.TryCopyFrom(0, new[] { 1 }, 1));
            Assert.Catch(() => array.CopyTo(new int[3]));
            Assert.Catch(() => array.CopyTo(0, new int[3], 1));
            Assert.Catch(() => array.TryCopyTo(new int[3]));
            Assert.Catch(() => array.TryCopyTo(0, new int[3], 1));
            Assert.Catch(() => array.Clear());
            Assert.Catch(() => array.ToArray());
            Assert.Catch(() => array.AsSpan());
            Assert.Catch(() => array.AsReadOnlySpan());
            Assert.Catch(() => array.AsNativeSlice());
            Assert.Catch(() => array.Reinterpret<uint>());
            Assert.Catch(() => _ = enumerator.Current);
            Assert.AreEqual(3, array.Length);
        }

        [Test]
        public unsafe void ReadOnly_AsNativeFromTheManagedReadOnly_ViewsTheOwner()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3 });

            var readOnly = owner.AsReadOnly().AsNative();
            var viaNative = owner.AsNative().AsReadOnly();

            owner[1] = 20;

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(3, readOnly.Length);
            Assert.AreEqual(20, readOnly[1]);
            Assert.AreEqual(20, viaNative[1]);
            Assert.IsFalse(default(SharedArrayNative<int>.ReadOnly).IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ReadOnly_IndexerOutOfRange_Throws()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3 });
            var readOnly = owner.AsNative().AsReadOnly();

            Assert.Throws<InvalidOperationException>(() => _ = readOnly[-1]);
            Assert.Throws<InvalidOperationException>(() => _ = readOnly[3]);
            Assert.Throws<InvalidOperationException>(() => _ = readOnly[int.MaxValue]);
        }

        [Test]
        public unsafe void ReadOnly_CopyToInvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3, 4 });
            var readOnly = owner.AsNative().AsReadOnly();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(-1, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(5, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(0, destination, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(new int[2], 0));
            Assert.IsFalse(readOnly.TryCopyTo(-1, destination));
            Assert.IsFalse(readOnly.TryCopyTo(0, destination, 5));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public unsafe void ReadOnly_CopyToDestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var owner = new SharedArray<int>((int[])seed.Clone());
            var array = owner.AsNative();
            var readOnly = array.AsReadOnly();

            readOnly.CopyTo(2, array.AsSpan()[..4]);

            var expected = (int[])seed.Clone();
            Array.Copy(expected, 2, expected, 0, 4);
            CollectionAssert.AreEqual(expected, owner.AsManagedArray());
        }

        [Test]
        public unsafe void ReadOnly_ElementShapesAndReinterpret_RoundTripBitExact()
        {
            using var small = new SharedArray<Rgb>(new[] { new Rgb { r = 1, g = 2, b = 3 } });
            using var wide = new SharedArray<Wide>(
                new[] { new Wide { a = long.MinValue, b = 0, c = long.MaxValue } }
            );
            using var ints = new SharedArray<int>(new[] { -1, 0x3F800000 });
            var smallView = small.AsNative().AsReadOnly();
            var wideView = wide.AsNative().AsReadOnly();
            var floatView = ints.AsNative().AsReadOnly().Reinterpret<float>();

            Assert.AreEqual(3, smallView[0].b);
            Assert.AreEqual(2, smallView.AsReadOnlySpan()[0].g);
            Assert.AreEqual(long.MinValue, wideView[0].a);
            Assert.AreEqual(long.MaxValue, wideView.ToArray()[0].c);
            Assert.AreEqual(1f, floatView[1]);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ReadOnly_Reinterpret_DifferentSize_Throws()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2 });
            var readOnly = owner.AsNative().AsReadOnly();

            Assert.Throws<InvalidOperationException>(() => readOnly.Reinterpret<byte>());
            Assert.Throws<InvalidOperationException>(() => readOnly.Reinterpret<long>());
        }

        [Test]
        public unsafe void ReadOnly_EnumeratorForms_VisitEveryElement()
        {
            using var owner = new SharedArray<int>(new[] { 1, 2, 3 });
            var readOnly = owner.AsNative().AsReadOnly();
            var visited = new List<int>();

            foreach (var value in readOnly)
            {
                visited.Add(value);
            }

            var generic = ((IEnumerable<int>)readOnly).GetEnumerator();
            var nonGeneric = ((IEnumerable)readOnly).GetEnumerator();

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, visited);
            Assert.IsTrue(generic.MoveNext());
            Assert.AreEqual(1, generic.Current);
            Assert.IsTrue(nonGeneric.MoveNext());
            Assert.AreEqual(1, nonGeneric.Current);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ReadOnly_Members_AfterOwnerDispose_Throw()
        {
            var owner = new SharedArray<int>(new[] { 1, 2, 3 });
            var readOnly = owner.AsNative().AsReadOnly();
            var enumerator = readOnly.GetEnumerator();

            owner.Dispose();

            Assert.Catch(() => _ = readOnly[0]);
            Assert.Catch(() => readOnly.AsReadOnlySpan());
            Assert.Catch(() => readOnly.ToArray());
            Assert.Catch(() => readOnly.CopyTo(new int[3]));
            Assert.Catch(() => readOnly.CopyTo(new int[3], 0));
            Assert.Catch(() => readOnly.TryCopyTo(new int[3]));
            Assert.Catch(() => readOnly.Reinterpret<uint>());
            Assert.Catch(() => _ = enumerator.Current);
            Assert.Catch(() => _ = (ReadOnlySpan<int>)readOnly);
            Assert.AreEqual(3, readOnly.Length);
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
