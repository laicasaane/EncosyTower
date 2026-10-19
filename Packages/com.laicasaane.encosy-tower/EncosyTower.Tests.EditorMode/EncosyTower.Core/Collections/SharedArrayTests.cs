// Adapted from Unity.Collections.Tests/NativeArrayTests.cs.

using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedArrayTests
    {
        [Test]
        public void Constructors_CreateExpectedContent()
        {
            var source = new[] { 1, 2, 3, 4 };
            var segment = new ArraySegment<int>(source, 1, 2);
            var span = source.AsSpan(0, 3);
            ICollection<int> collection = new List<int> { 5, 6, 7 };

            using var fromSize = new SharedArray<int>(3);
            using var fromArray = new SharedArray<int>(source);
            using var fromSegment = new SharedArray<int>(in segment);
            using var fromSpan = new SharedArray<int>(span);
            using var fromCollection = new SharedArray<int>(collection);
            using var fromCollectionWithExtra = new SharedArray<int>(collection, 2);

            Assert.AreEqual(3, fromSize.Length);
            CollectionAssert.AreEqual(new[] { 0, 0, 0 }, fromSize.AsManagedArray());
            CollectionAssert.AreEqual(source, fromArray.AsManagedArray());
            CollectionAssert.AreEqual(new[] { 2, 3 }, fromSegment.AsManagedArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, fromSpan.AsManagedArray());
            CollectionAssert.AreEqual(new[] { 5, 6, 7 }, fromCollection.AsManagedArray());
            CollectionAssert.AreEqual(
                  new[] { 5, 6, 7, 0, 0 }
                , fromCollectionWithExtra.AsManagedArray()
            );
        }

        [Test]
        public void Indexer_ReadsAndWritesValues()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });

            array[1] = 20;

            Assert.AreEqual(1, array[0]);
            Assert.AreEqual(20, array[1]);
            Assert.AreEqual(3, array[2]);
        }

        [Test]
        public void Clear_ZeroesEveryElement()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });

            array.Clear();

            CollectionAssert.AreEqual(new[] { 0, 0, 0 }, array.AsManagedArray());
        }

        [Test]
        public void Length_RemainsFixedWhileManagedAndNativeAliasesMutateElements()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });

            // SAFETY: array remains the live owner while the borrowed native view is used.
            unsafe
            {
                var native = array.AsNative();

                array[0] = 10;
                native[1] = 20;

                Assert.AreEqual(3, array.Length);
                CollectionAssert.AreEqual(new[] { 10, 20, 3 }, array.AsManagedArray());
            }
        }

        [Test]
        public void Views_ReflectContent()
        {
            using var array = new SharedArray<int>(new[] { 10, 20, 30 });

            // SAFETY: array remains the live owner while all borrowed views are inspected.
            unsafe
            {
                var managed = array.AsManagedArray();
                var segment = array.AsArraySegment();
                var span = array.AsSpan();
                var readOnlySpan = array.AsReadOnlySpan();
                var memory = array.AsMemory();
                var readOnlyMemory = array.AsReadOnlyMemory();
                var nativeArray = array.AsNativeArray();
                var nativeSlice = array.AsNativeSlice();

                CollectionAssert.AreEqual(new[] { 10, 20, 30 }, managed);
                CollectionAssert.AreEqual(new[] { 10, 20, 30 }, segment);
                CollectionAssert.AreEqual(new[] { 10, 20, 30 }, span.ToArray());
                CollectionAssert.AreEqual(new[] { 10, 20, 30 }, readOnlySpan.ToArray());
                CollectionAssert.AreEqual(new[] { 10, 20, 30 }, memory.ToArray());
                CollectionAssert.AreEqual(new[] { 10, 20, 30 }, readOnlyMemory.ToArray());
                Assert.AreEqual(3, nativeArray.Length);
                Assert.AreEqual(20, nativeArray[1]);
                Assert.AreEqual(3, nativeSlice.Length);
                Assert.AreEqual(30, nativeSlice[2]);
            }
        }

        [Test]
        public void ImplicitOperators_ReflectContent()
        {
            using var array = new SharedArray<int>(new[] { 10, 20, 30 });

            // SAFETY: array remains the live owner while all borrowed implicit views are inspected.
            unsafe
            {
                int[] managed = array;
                ArraySegment<int> segment = array;
                Span<int> span = array;
                ReadOnlySpan<int> readOnlySpan = array;
                Memory<int> memory = array;
                ReadOnlyMemory<int> readOnlyMemory = array;
                NativeArray<int> nativeArray = array;
                NativeSlice<int> nativeSlice = array;

                Assert.AreEqual(20, managed[1]);
                Assert.AreEqual(20, segment.Array[segment.Offset + 1]);
                Assert.AreEqual(20, span[1]);
                Assert.AreEqual(20, readOnlySpan[1]);
                Assert.AreEqual(20, memory.Span[1]);
                Assert.AreEqual(20, readOnlyMemory.Span[1]);
                Assert.AreEqual(20, nativeArray[1]);
                Assert.AreEqual(20, nativeSlice[1]);
            }
        }

        [Test]
        public void AsNativeArray_WriteIsVisibleThroughIndexer()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });
            // SAFETY: array remains the live owner while the borrowed native array is mutated.
            unsafe
            {
                var nativeArray = array.AsNativeArray();

                nativeArray[1] = 99;

                Assert.AreEqual(99, array[1]);
            }
        }

        [Test]
        public void GetPinnableReference_FixedPointerReadsAndWrites()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });

            // SAFETY: SharedArray pins its managed storage for the lifetime of the fixed scope.
            unsafe
            {
                fixed (int* pointer = array)
                {
                    Assert.AreEqual(2, pointer[1]);
                    pointer[1] = 99;
                }
            }

            Assert.AreEqual(99, array[1]);
        }

        [Test]
        public void Enumerator_VisitsEveryElement()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });
            var values = new List<int>();

            // SAFETY: array remains alive and unmodified for the complete enumeration.
            unsafe
            {
                foreach (var value in array)
                {
                    values.Add(value);
                }
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, values);
        }

        [Test]
        public void Enumerator_MutationDuringIteration_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });
            // SAFETY: array remains alive while the enumerator's version guard is exercised.
            unsafe
            {
                var enumerator = array.GetEnumerator();

                Assert.IsTrue(enumerator.MoveNext());
                array[0] = 10;

                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }
        }

        [Test]
        public void Dispose_CanBeCalledTwice()
        {
            var array = new SharedArray<int>(3);

            Assert.DoesNotThrow(array.Dispose);
            Assert.DoesNotThrow(array.Dispose);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void NativeViewCreatedBeforeDispose_ThrowsOnAccessAfterDispose()
        {
            var array = new SharedArray<int>(new[] { 1, 2, 3 });
            // SAFETY: This test intentionally keeps a borrowed view past owner disposal to verify safety checks.
            unsafe
            {
                var staleView = array.AsNative();

                array.Dispose();

                Assert.Catch(() => staleView[0] = 99);
            }
        }

        [Test]
        public void GenericConstructors_CreateExpectedManagedAndNativeContent()
        {
            var source = new[] { 1, 2, 3, 4 };
            var segment = new ArraySegment<int>(source, 1, 2);
            ReadOnlySpan<int> span = source.AsSpan(0, 3);
            ICollection<int> collection = new List<int> { 5, 6, 7 };
            using var native = new NativeArray<uint>(new uint[] { 8, 9, 10 }, Allocator.Temp);
            var nativeSlice = new NativeSlice<uint>(native, 1, 2);

            using var fromSize = new SharedArray<int, uint>(3);
            using var fromArray = new SharedArray<int, uint>(source);
            using var fromSegment = new SharedArray<int, uint>(in segment);
            using var fromSpan = new SharedArray<int, uint>(span);
            using var fromNative = new SharedArray<int, uint>(in native);
            using var fromNativeSlice = new SharedArray<int, uint>(in nativeSlice);
            using var fromCollection = new SharedArray<int, uint>(collection);
            using var fromCollectionWithExtra = new SharedArray<int, uint>(collection, 2);

            CollectionAssert.AreEqual(new[] { 0, 0, 0 }, fromSize.AsManagedArray());
            CollectionAssert.AreEqual(source, fromArray.AsManagedArray());
            CollectionAssert.AreEqual(new[] { 2, 3 }, fromSegment.AsManagedArray());
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, fromSpan.AsManagedArray());
            CollectionAssert.AreEqual(new[] { 8, 9, 10 }, fromNative.AsManagedArray());
            CollectionAssert.AreEqual(new[] { 9, 10 }, fromNativeSlice.AsManagedArray());
            CollectionAssert.AreEqual(new[] { 5, 6, 7 }, fromCollection.AsManagedArray());
            CollectionAssert.AreEqual(
                  new[] { 5, 6, 7, 0, 0 }
                , fromCollectionWithExtra.AsManagedArray()
            );
        }

        [Test]
        public void Enumerator_DirectConstructorResetDisposeAndInterfacesWork()
        {
            using var array = new SharedArray<int>(new[] { 7, 8 });
            var direct = new SharedArray<int, int>.Enumerator(array);

            Assert.IsTrue(direct.MoveNext());
            Assert.AreEqual(7, direct.Current);
            IEnumerator interfaceEnumerator = direct;
            interfaceEnumerator.Reset();
            Assert.IsTrue(interfaceEnumerator.MoveNext());
            Assert.AreEqual(7, interfaceEnumerator.Current);
            direct.Dispose();

            // SAFETY: array remains alive and unmodified while both interface enumerators are consumed.
            unsafe
            {
                IEnumerable<int> genericEnumerable = array;
                var genericEnumerator = genericEnumerable.GetEnumerator();
                Assert.IsTrue(genericEnumerator.MoveNext());
                genericEnumerator.Dispose();

                IEnumerable enumerable = array;
                var enumerator = enumerable.GetEnumerator();
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(7, enumerator.Current);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Indexer_OutOfRange_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });
            using var empty = new SharedArray<int>(0);

            Assert.Throws<InvalidOperationException>(() => _ = array[-1]);
            Assert.Throws<InvalidOperationException>(() => _ = array[3]);
            Assert.Throws<InvalidOperationException>(() => _ = array[int.MaxValue]);
            Assert.Throws<InvalidOperationException>(() => array[-1] = 9);
            Assert.Throws<InvalidOperationException>(() => array[3] = 9);
            Assert.Throws<InvalidOperationException>(() => _ = empty[0]);
            Assert.Throws<InvalidOperationException>(() => empty[0] = 9);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, array.AsManagedArray());
        }

        [Test]
        public void Constructors_EmptySources_CreateZeroLengthArrays()
        {
            var segment = new ArraySegment<int>(new int[2], 1, 0);
            using var fromSize = new SharedArray<int>(0);
            using var fromArray = new SharedArray<int>(Array.Empty<int>());
            using var fromSegment = new SharedArray<int>(in segment);
            using var fromSpan = new SharedArray<int>(ReadOnlySpan<int>.Empty);
            using var fromCollection = new SharedArray<int>(new List<int>());
            using var fromCollectionWithoutExtra = new SharedArray<int>(new List<int>(), 0);

            Assert.AreEqual(0, fromSize.Length);
            Assert.AreEqual(0, fromArray.Length);
            Assert.AreEqual(0, fromSegment.Length);
            Assert.AreEqual(0, fromSpan.Length);
            Assert.AreEqual(0, fromCollection.Length);
            Assert.AreEqual(0, fromCollectionWithoutExtra.Length);
            Assert.AreEqual(0, fromSize.AsManagedArray().Length);
        }

        [Test]
        public void Constructors_EmptyNativeSources_CreateZeroLengthArrays()
        {
            using var native = new NativeArray<int>(0, Allocator.Temp);
            var nativeSlice = new NativeSlice<int>(native);
            using var fromNative = new SharedArray<int, int>(in native);
            using var fromNativeSlice = new SharedArray<int, int>(in nativeSlice);

            Assert.AreEqual(0, fromNative.Length);
            Assert.AreEqual(0, fromNativeSlice.Length);
        }

        [Test]
        public void AsNative_ZeroLengthArray_HasLengthZero()
        {
            using var array = new SharedArray<int>(0);

            // SAFETY: array remains the live owner while the borrowed view is inspected.
            unsafe
            {
                var native = array.AsNative();

                Assert.AreEqual(0, native.Length);
            }
        }

        [Test]
        public void AsNativeArrayAndAsNativeSlice_ZeroLengthArray_HaveLengthZero()
        {
            using var array = new SharedArray<int>(0);

            // SAFETY: array remains the live owner while the borrowed aliases are inspected.
            unsafe
            {
                var nativeArray = array.AsNativeArray();
                var nativeSlice = array.AsNativeSlice();
                NativeSlice<int> implicitSlice = array;

                Assert.AreEqual(0, nativeArray.Length);
                Assert.AreEqual(0, nativeSlice.Length);
                Assert.AreEqual(0, implicitSlice.Length);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AsNativeSlice_IndexOutsideTheArray_Throws()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });

            // SAFETY: array remains the live owner while the borrowed slices are inspected.
            unsafe
            {
                var slice = array.AsNativeSlice();
                NativeSlice<int> implicitSlice = array;

                Assert.AreEqual(3, slice.Length);
                Assert.Throws<IndexOutOfRangeException>(() => _ = slice[-1]);
                Assert.Throws<IndexOutOfRangeException>(() => _ = slice[3]);
                Assert.Throws<IndexOutOfRangeException>(() => _ = implicitSlice[-1]);
                Assert.Throws<IndexOutOfRangeException>(() => _ = implicitSlice[3]);
            }
        }

        [Test]
        public void AsNativeSlice_WriteThroughTheSlice_IsSeenByTheArray()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });

            // SAFETY: array remains the live owner while the borrowed slices are used.
            unsafe
            {
                var slice = array.AsNativeSlice();
                NativeSlice<int> implicitSlice = array;

                slice[0] = 10;
                implicitSlice[2] = 30;

                Assert.AreEqual(10, array[0]);
                Assert.AreEqual(30, array[2]);
                Assert.AreEqual(30, slice[2]);
                Assert.AreEqual(10, implicitSlice[0]);
            }
        }

        [Test]
        public void AsNativeSlice_ElementSizes_RoundTripBitExact()
        {
            AssertNativeSliceRoundTrip(new byte[] { 0, 255, 128 });
            AssertNativeSliceRoundTrip(new[] { long.MinValue, -1L, long.MaxValue });
            AssertNativeSliceRoundTrip(new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 0, b = 128 } });
            AssertNativeSliceRoundTrip(new[] { new Wide { a = long.MinValue, b = -1, c = long.MaxValue } });
        }

        [Test]
        public void GetPinnableReference_ZeroLengthArray_ProducesANullPointer()
        {
            using var array = new SharedArray<int>(0);

            // SAFETY: A zero-length array pins nothing; the pointer is only compared with null.
            unsafe
            {
                fixed (int* pointer = array)
                {
                    Assert.IsTrue(pointer == null);
                }
            }
        }

        [Test]
        public void GetPinnableReference_PointerAndSpansAlias_SeeEachOthersWrites()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });

            // SAFETY: array stays live and is not disposed while the pointer and the spans are used.
            unsafe
            {
                fixed (int* pointer = array)
                {
                    var span = array.AsSpan();

                    pointer[0] = 7;

                    Assert.AreEqual(7, span[0]);

                    span[2] = 9;

                    Assert.AreEqual(9, pointer[2]);
                    Assert.AreEqual(7, array.AsReadOnlySpan()[0]);
                    Assert.AreEqual(9, array.AsReadOnlySpan()[2]);
                }
            }

            Assert.AreEqual(7, array[0]);
            Assert.AreEqual(9, array[2]);
        }

        [Test]
        public void GetUnsafeBufferPointer_PointerAndIndexerAlias_SeeEachOthersWrites()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });
            using var empty = new SharedArray<int>(0);

            // SAFETY: Both arrays stay live while the pointers are used.
            unsafe
            {
                var pointer = array.GetUnsafeBufferPointer();

                pointer[1] = 20;

                Assert.AreEqual(20, array[1]);

                array[2] = 30;

                Assert.AreEqual(30, pointer[2]);
                Assert.AreEqual(1, pointer[0]);

                var emptyPointer = empty.GetUnsafeBufferPointer();
            }

            Assert.AreEqual(0, empty.Length);
        }

        [Test]
        public void GetEnumerator_ZeroLengthArray_YieldsNothing()
        {
            using var array = new SharedArray<int>(0);
            var count = 0;

            // SAFETY: array outlives the enumerators and is not modified while they run.
            unsafe
            {
                foreach (var item in array)
                {
                    count++;
                }

                foreach (var item in (IEnumerable<int>)array)
                {
                    count++;
                }

                foreach (var item in (IEnumerable)array)
                {
                    count++;
                }
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Dispose_ZeroLengthArray_CanBeCalledTwice()
        {
            var array = new SharedArray<int>(0);

            Assert.DoesNotThrow(array.Dispose);
            Assert.DoesNotThrow(array.Dispose);
        }

        [Test]
        public void AsSpanAndAsReadOnlySpan_ZeroLengthArray_HaveLengthZero()
        {
            using var array = new SharedArray<int>(0);

            // SAFETY: array outlives the spans, which are only inspected here.
            unsafe
            {
                Assert.AreEqual(0, array.AsSpan().Length);
                Assert.AreEqual(0, array.AsReadOnlySpan().Length);
            }
        }

        [Test]
        public void AsSpanAndAsReadOnlySpan_ElementSizes_RoundTripBitExact()
        {
            AssertSpanRoundTrip(new byte[] { 0, 255, 128 });
            AssertSpanRoundTrip(new[] { long.MinValue, -1L, long.MaxValue });
            AssertSpanRoundTrip(new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 0, b = 128 } });
            AssertSpanRoundTrip(new[] { new Wide { a = long.MinValue, b = -1, c = long.MaxValue } });
        }

        [Test]
        public void AsReadOnly_ZeroLengthArray_ReportsNoItems()
        {
            using var array = new SharedArray<int>(0);

            // SAFETY: array outlives the view, which is only inspected here.
            unsafe
            {
                var view = array.AsReadOnly();

                Assert.IsTrue(view.IsCreated);
                Assert.AreEqual(0, view.Length);
                Assert.AreEqual(0, view.Count);
                Assert.AreEqual(0, view.AsReadOnlySpan().Length);
                Assert.AreEqual(0, view.ToArray().Length);
            }
        }

        [Test]
        public void AsReadOnly_ViewSeesLaterWrites()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3 });

            // SAFETY: array outlives the view, which is only used here.
            unsafe
            {
                var view = array.AsReadOnly();
                var span = view.AsReadOnlySpan();

                array[0] = 10;
                array.AsSpan()[2] = 30;

                Assert.AreEqual(10, view[0]);
                Assert.AreEqual(30, view[2]);
                Assert.AreEqual(10, span[0]);
                CollectionAssert.AreEqual(new[] { 10, 2, 30 }, view.ToArray());
            }
        }

        [Test]
        public void ReadOnly_Default_IsNotCreated()
        {
            var view = default(SharedArray<int, int>.ReadOnly);

            Assert.IsFalse(view.IsCreated);
        }

        [Test]
        public void ReadOnly_Empty_ReportsNoItems()
        {
            var view = SharedArray<int, int>.ReadOnly.Empty;
            var count = 0;

            // SAFETY: The empty owner is static and never disposed.
            unsafe
            {
                foreach (var item in view)
                {
                    count++;
                }

                foreach (var item in (IEnumerable<int>)view)
                {
                    count++;
                }

                foreach (var item in (IEnumerable)view)
                {
                    count++;
                }

                Assert.AreEqual(0, view.AsReadOnlySpan().Length);
            }

            Assert.AreEqual(0, count);
            Assert.AreEqual(0, view.Length);
            Assert.AreEqual(0, view.ToArray().Length);
        }

        [Test]
        public void ReadOnlyCopyTo_ZeroLengthArray_AcceptsEmptyDestinations()
        {
            using var array = new SharedArray<int>(0);

            // SAFETY: array outlives the view, which is only used here.
            unsafe
            {
                var view = array.AsReadOnly();

                view.CopyTo(Span<int>.Empty);
                view.CopyTo(Array.Empty<int>(), 0);
                view.CopyTo(0, Span<int>.Empty);

                Assert.IsTrue(view.TryCopyTo(Span<int>.Empty));
                Assert.IsTrue(view.TryCopyTo(0, Span<int>.Empty));
                Assert.IsFalse(view.TryCopyTo(new int[1]));
            }
        }

        [Test]
        public void ReadOnlyCopyTo_RangesOutsideTheViewOrDestination_ThrowOrReturnFalse()
        {
            using var array = new SharedArray<int>(new[] { 1, 2, 3, 4 });

            // SAFETY: array outlives the view, which is only used here.
            unsafe
            {
                var view = array.AsReadOnly();
                var destination = new int[4];

                Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(-1, new int[4]));
                Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(5, new int[0]));
                Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(1, new int[4]));
                Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(0, new int[2], 3));
                Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(0, new int[4], -1));
                Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(new int[3], 1));
                Assert.IsFalse(view.TryCopyTo(-1, destination));
                Assert.IsFalse(view.TryCopyTo(5, new int[0]));
                Assert.IsFalse(view.TryCopyTo(1, destination));
                Assert.IsFalse(view.TryCopyTo(0, new int[2], 3));
                Assert.IsFalse(view.TryCopyTo(0, destination, -1));
                CollectionAssert.AreEqual(new int[4], destination);
            }
        }

        [Test]
        public void ReadOnlyCopyTo_DestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var array = new SharedArray<int>((int[])seed.Clone());

            // SAFETY: array outlives the view and the spans, which are consumed by each copy.
            unsafe
            {
                var view = array.AsReadOnly();

                view.CopyTo(2, array.AsSpan()[..4]);

                var expectedBackward = (int[])seed.Clone();
                Array.Copy(expectedBackward, 2, expectedBackward, 0, 4);
                CollectionAssert.AreEqual(expectedBackward, array.AsManagedArray());

                seed.AsSpan().CopyTo(array.AsSpan());

                Assert.IsTrue(view.TryCopyTo(0, array.AsSpan()[2..]));

                var expectedForward = (int[])seed.Clone();
                Array.Copy(expectedForward, 0, expectedForward, 2, 4);
                CollectionAssert.AreEqual(expectedForward, array.AsManagedArray());
            }
        }

        [Test]
        public void ReadOnlyCopyTo_ElementSizes_RoundTripBitExact()
        {
            AssertReadOnlyCopyRoundTrip(new byte[] { 0, 255, 128 });
            AssertReadOnlyCopyRoundTrip(new[] { long.MinValue, -1L, long.MaxValue });
            AssertReadOnlyCopyRoundTrip(new[] { new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 0, b = 128 } });
            AssertReadOnlyCopyRoundTrip(new[] { new Wide { a = long.MinValue, b = -1, c = long.MaxValue } });
        }

        [Test]
        public void OnBeforeSerialize_ZeroLengthArray_StoresAnEmptyPayload()
        {
            using var array = new SharedArray<int>(0);

            ((ISerializationCallbackReceiver)array).OnBeforeSerialize();

            Assert.That(array._serializedItems, Is.Not.Null);
            Assert.That(array._serializedItems, Is.Empty);
        }

        [Test]
        public void OnBeforeSerialize_DisposedArray_StoresNoPayload()
        {
            var array = new SharedArray<int>(new[] { 1, 2 });

            array.Dispose();

            ((ISerializationCallbackReceiver)array).OnBeforeSerialize();

            Assert.That(array._serializedItems, Is.Null);
        }

        private static void AssertNativeSliceRoundTrip<T>(T[] values)
            where T : unmanaged
        {
            using var array = new SharedArray<T>((T[])values.Clone());

            // SAFETY: array outlives the slice, which is only used here.
            unsafe
            {
                var slice = array.AsNativeSlice();
                NativeSlice<T> implicitSlice = array;
                var copy = new T[values.Length];

                Assert.AreEqual(values.Length, slice.Length);
                Assert.AreEqual(values.Length, implicitSlice.Length);

                slice.CopyTo(copy);

                SpanBytesAssert.AreEqual(values, copy);

                for (var i = 0; i < values.Length; i++)
                {
                    SpanBytesAssert.AreEqual(new[] { values[i] }, new[] { implicitSlice[i] });
                }
            }
        }

        private static void AssertSpanRoundTrip<T>(T[] values)
            where T : unmanaged
        {
            using var array = new SharedArray<T>((T[])values.Clone());

            // SAFETY: array outlives the spans, which are only inspected here.
            unsafe
            {
                var readOnly = array.AsReadOnlySpan();
                var writable = array.AsSpan();

                Assert.AreEqual(values.Length, readOnly.Length);
                Assert.AreEqual(values.Length, writable.Length);
                SpanBytesAssert.AreEqual(values, readOnly);
                SpanBytesAssert.AreEqual(values, writable);
            }
        }

        private static void AssertReadOnlyCopyRoundTrip<T>(T[] values)
            where T : unmanaged
        {
            using var array = new SharedArray<T>((T[])values.Clone());
            var copy = new T[values.Length];

            // SAFETY: array outlives the view, which is only used here.
            unsafe
            {
                var view = array.AsReadOnly();

                view.CopyTo(copy);
                SpanBytesAssert.AreEqual(values, copy);
                SpanBytesAssert.AreEqual(values, view.AsReadOnlySpan());
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
