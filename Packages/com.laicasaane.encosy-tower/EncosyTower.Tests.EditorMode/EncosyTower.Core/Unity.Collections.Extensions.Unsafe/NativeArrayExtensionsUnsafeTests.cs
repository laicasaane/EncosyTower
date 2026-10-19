using System;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    public class NativeArrayExtensionsUnsafeTests
    {
        [Test]
        public void ElementRefs_AliasOriginalArray()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            try
            {
                ref var writable = ref EncosyNativeArrayExtensionsUnsafe.ElementAsUnsafeRefRW(array, 1);
                writable = 20;
                ref readonly var readOnly = ref EncosyNativeArrayExtensionsUnsafe.ElementAsUnsafeRefRO(array, 1);

                Assert.AreEqual(20, array[1]);
                Assert.AreEqual(20, readOnly);
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        public void MemoryCopyWithinArray_CopiesRequestedNonOverlappingRange()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3, 0, 0, 0 }, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(array, 0, 3, 3);

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 2, 3 }, array.ToArray());
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        public void MemoryCopyBetweenArrays_CheckedAndUncheckedFormsCopyRequestedRange()
        {
            var source = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var destination = new NativeArray<int>(4, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(source, 1, destination, 0, 2);

                CollectionAssert.AreEqual(new[] { 2, 3, 0, 0 }, destination.ToArray());

                destination[0] = 0;
                destination[1] = 0;

                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafeWithoutChecks(source, 0, destination, 2, 2);

                CollectionAssert.AreEqual(new[] { 0, 0, 1, 2 }, destination.ToArray());
            }
            finally
            {
                destination.Dispose();
                source.Dispose();
            }
        }

        [Test]
        public void ElementRefs_ByteElement_RoundTripBitExact()
        {
            AssertElementRefsRoundTrip<byte>(first: 0xA5, second: 0xFF);
        }

        [Test]
        public void ElementRefs_ThreeByteStruct_RoundTripBitExact()
        {
            AssertElementRefsRoundTrip(
                  first: new Rgb { r = 1, g = 2, b = 3 }
                , second: new Rgb { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public void ElementRefs_LongElement_RoundTripBitExact()
        {
            AssertElementRefsRoundTrip(first: long.MinValue, second: long.MaxValue);
        }

        [Test]
        public void ElementRefs_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertElementRefsRoundTrip(
                  first: new Wide { a = 1, b = -2, c = long.MaxValue }
                , second: new Wide { a = long.MinValue, b = 0, c = -1 }
            );
        }

        [Test]
        public void ElementRefs_LastElement_AliasesTheLastSlot()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.ElementAsUnsafeRefRW(array, array.Length - 1) = 30;

                CollectionAssert.AreEqual(new[] { 1, 2, 30 }, array.ToArray());
                Assert.AreEqual(30, EncosyNativeArrayExtensionsUnsafe.ElementAsUnsafeRefRO(array, array.Length - 1));
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ElementRefs_AfterDispose_Throw()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            array.Dispose();

            Assert.Catch(() => _ = EncosyNativeArrayExtensionsUnsafe.ElementAsUnsafeRefRW(array, 0));
            Assert.Catch(() => _ = EncosyNativeArrayExtensionsUnsafe.ElementAsUnsafeRefRO(array, 0));
        }

        [Test]
        public void MemoryCopyWithinArray_ZeroLength_LeavesTheArrayUnchanged()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(array, 0, 1, 0);
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(array, 2, 2, 0);

                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, array.ToArray());
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        public void MemoryCopyWithinArray_ForwardOverlap_MatchesArrayCopy()
        {
            var source = new[] { 1, 2, 3, 4, 5, 6 };
            var expected = (int[])source.Clone();
            Array.Copy(source, 0, expected, 1, 4);
            var array = new NativeArray<int>(source, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(array, 0, 1, 4);

                CollectionAssert.AreEqual(expected, array.ToArray());
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        public void MemoryCopyWithinArray_BackwardOverlap_MatchesArrayCopy()
        {
            var source = new[] { 1, 2, 3, 4, 5, 6 };
            var expected = (int[])source.Clone();
            Array.Copy(source, 2, expected, 0, 4);
            var array = new NativeArray<int>(source, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(array, 2, 0, 4);

                CollectionAssert.AreEqual(expected, array.ToArray());
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        public void MemoryCopyWithinArray_ExactlyFillsTheTail_CopiesEveryRequestedElement()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3, 0, 0, 0 }, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(array, 0, 3, 3);

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 2, 3 }, array.ToArray());
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        public void MemoryCopyWithinArray_LengthPastTheDestinationTail_ThrowsAndKeepsTheArray()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3, 4, 5, 6 }, Allocator.Temp);

            try
            {
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(array, 0, 3, 4)
                );

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, array.ToArray());
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        public void MemoryCopyWithinArray_ElementTypes_CopyBitExact()
        {
            AssertMemoryCopyWithinArrayBitExact<byte>(a: 0xA5, b: 0xFF, c: 0x01);
            AssertMemoryCopyWithinArrayBitExact(
                  a: new Rgb { r = 1, g = 2, b = 3 }
                , b: new Rgb { r = 255, g = 254, b = 253 }
                , c: new Rgb { r = 7, g = 0, b = 9 }
            );
            AssertMemoryCopyWithinArrayBitExact(a: long.MinValue, b: long.MaxValue, c: -1L);
            AssertMemoryCopyWithinArrayBitExact(
                  a: new Wide { a = 1, b = -2, c = long.MaxValue }
                , b: new Wide { a = long.MinValue, b = 0, c = -1 }
                , c: new Wide { a = 5, b = 6, c = 7 }
            );
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void MemoryCopyWithinArray_AfterDispose_Throws()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            array.Dispose();

            Assert.Catch(() => EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(array, 0, 1, 1));
        }

        [Test]
        public void MemoryCopyBetweenArrays_ZeroLength_LeavesBothArraysUnchanged()
        {
            var source = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var destination = new NativeArray<int>(new[] { 7, 8, 9 }, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(source, 0, destination, 0, 0);
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(source, 3, destination, 3, 0);
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafeWithoutChecks(source, 0, destination, 0, 0);
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafeWithoutChecks(source, 3, destination, 3, 0);

                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, source.ToArray());
                CollectionAssert.AreEqual(new[] { 7, 8, 9 }, destination.ToArray());
            }
            finally
            {
                destination.Dispose();
                source.Dispose();
            }
        }

        [Test]
        public void MemoryCopyBetweenArrays_WholeArrays_CopiesEveryElement()
        {
            var source = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var destination = new NativeArray<int>(4, Allocator.Temp);
            var uncheckedDestination = new NativeArray<int>(4, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(source, 0, destination, 0, 4);
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafeWithoutChecks(source, 0, uncheckedDestination, 0, 4);

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, destination.ToArray());
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, uncheckedDestination.ToArray());
            }
            finally
            {
                uncheckedDestination.Dispose();
                destination.Dispose();
                source.Dispose();
            }
        }

        [Test]
        public void MemoryCopyBetweenArrays_AdjacentRangesInOneArray_MatchesArrayCopy()
        {
            var values = new[] { 1, 2, 3, 0, 0, 0 };
            var expected = (int[])values.Clone();
            Array.Copy(values, 0, expected, 3, 3);
            var array = new NativeArray<int>(values, Allocator.Temp);
            var other = new NativeArray<int>(values, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(array, 0, array, 3, 3);
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafeWithoutChecks(other, 0, other, 3, 3);

                CollectionAssert.AreEqual(expected, array.ToArray());
                CollectionAssert.AreEqual(expected, other.ToArray());
            }
            finally
            {
                other.Dispose();
                array.Dispose();
            }
        }

        [Test]
        public void MemoryCopyBetweenArrays_SameArrayOverlap_MatchesArrayCopy()
        {
            const int LENGTH = 64;

            var values = new int[LENGTH];

            for (var i = 0; i < LENGTH; i++)
            {
                values[i] = i + 1;
            }

            var expected = (int[])values.Clone();
            Array.Copy(values, 0, expected, 1, LENGTH - 1);
            var array = new NativeArray<int>(values, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(array, 0, array, 1, LENGTH - 1);

                CollectionAssert.AreEqual(expected, array.ToArray());
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        public void MemoryCopyBetweenArrays_ElementTypes_CopyBitExact()
        {
            AssertMemoryCopyBetweenArraysBitExact<byte>(a: 0xA5, b: 0xFF, c: 0x01);
            AssertMemoryCopyBetweenArraysBitExact(
                  a: new Rgb { r = 1, g = 2, b = 3 }
                , b: new Rgb { r = 255, g = 254, b = 253 }
                , c: new Rgb { r = 7, g = 0, b = 9 }
            );
            AssertMemoryCopyBetweenArraysBitExact(a: long.MinValue, b: long.MaxValue, c: -1L);
            AssertMemoryCopyBetweenArraysBitExact(
                  a: new Wide { a = 1, b = -2, c = long.MaxValue }
                , b: new Wide { a = long.MinValue, b = 0, c = -1 }
                , c: new Wide { a = 5, b = 6, c = 7 }
            );
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void MemoryCopyBetweenArrays_AfterDispose_Throws()
        {
            var source = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var destination = new NativeArray<int>(3, Allocator.Temp);
            var disposedSource = new NativeArray<int>(3, Allocator.Temp);
            disposedSource.Dispose();
            var disposedDestination = new NativeArray<int>(3, Allocator.Temp);
            disposedDestination.Dispose();

            try
            {
                Assert.Catch(
                    () => EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(disposedSource, 0, destination, 0, 1)
                );
                Assert.Catch(
                    () => EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(source, 0, disposedDestination, 0, 1)
                );
            }
            finally
            {
                destination.Dispose();
                source.Dispose();
            }
        }

        private static void AssertElementRefsRoundTrip<T>(T first, T second)
            where T : struct
        {
            var array = new NativeArray<T>(3, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.ElementAsUnsafeRefRW(array, 0) = first;
                EncosyNativeArrayExtensionsUnsafe.ElementAsUnsafeRefRW(array, 2) = second;
                ref readonly var readOnly = ref EncosyNativeArrayExtensionsUnsafe.ElementAsUnsafeRefRO(array, 2);

                Assert.AreEqual(first, array[0]);
                Assert.AreEqual(default(T), array[1]);
                Assert.AreEqual(second, array[2]);
                Assert.AreEqual(second, readOnly);
            }
            finally
            {
                array.Dispose();
            }
        }

        private static void AssertMemoryCopyWithinArrayBitExact<T>(T a, T b, T c)
            where T : struct
        {
            var array = new NativeArray<T>(5, Allocator.Temp);
            array[0] = a;
            array[1] = b;
            array[2] = c;

            try
            {
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(array, 0, 2, 3);

                Assert.AreEqual(a, array[0]);
                Assert.AreEqual(b, array[1]);
                Assert.AreEqual(a, array[2]);
                Assert.AreEqual(b, array[3]);
                Assert.AreEqual(c, array[4]);
            }
            finally
            {
                array.Dispose();
            }
        }

        private static void AssertMemoryCopyBetweenArraysBitExact<T>(T a, T b, T c)
            where T : struct
        {
            var source = new NativeArray<T>(3, Allocator.Temp);
            source[0] = a;
            source[1] = b;
            source[2] = c;
            var destination = new NativeArray<T>(5, Allocator.Temp);
            var uncheckedDestination = new NativeArray<T>(5, Allocator.Temp);

            try
            {
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe(source, 0, destination, 1, 3);
                EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafeWithoutChecks(source, 0, uncheckedDestination, 1, 3);

                Assert.AreEqual(default(T), destination[0]);
                Assert.AreEqual(a, destination[1]);
                Assert.AreEqual(b, destination[2]);
                Assert.AreEqual(c, destination[3]);
                Assert.AreEqual(default(T), destination[4]);

                Assert.AreEqual(default(T), uncheckedDestination[0]);
                Assert.AreEqual(a, uncheckedDestination[1]);
                Assert.AreEqual(b, uncheckedDestination[2]);
                Assert.AreEqual(c, uncheckedDestination[3]);
                Assert.AreEqual(default(T), uncheckedDestination[4]);
            }
            finally
            {
                uncheckedDestination.Dispose();
                destination.Dispose();
                source.Dispose();
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
