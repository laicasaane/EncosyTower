// Adapted from Unity.Collections.Tests/NativeArrayTests.cs.

using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using NUnit.Framework;
using Unity.Collections;

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
    }
}
