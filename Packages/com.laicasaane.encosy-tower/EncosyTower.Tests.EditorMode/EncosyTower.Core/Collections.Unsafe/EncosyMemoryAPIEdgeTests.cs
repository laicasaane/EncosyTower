using System;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    public partial class EncosyMemoryAPIEdgeTests
    {
        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void UnmanagedAllocate_AcceptedAllocator_ReturnsWritableStorageAndFreeReleasesIt(Allocator allocator)
        {
            var strategy = new AllocatorStrategy(allocator);

            // SAFETY: The block holds four ints, every access stays below four, and it is freed once in finally.
            unsafe
            {
                void* pointer = null;

                try
                {
                    pointer = EncosyMemoryAPI.Unmanaged.Allocate(
                          UnsafeUtility.SizeOf<int>()
                        , UnsafeUtility.AlignOf<int>()
                        , 4
                        , strategy
                    );

                    var values = (int*)pointer;
                    values[0] = 1;
                    values[3] = 4;

                    Assert.AreNotEqual(IntPtr.Zero, (IntPtr)pointer);
                    Assert.AreEqual(1, values[0]);
                    Assert.AreEqual(4, values[3]);
                }
                finally
                {
                    EncosyMemoryAPI.Unmanaged.Free(pointer, strategy);
                    pointer = null;
                }
            }
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void UnmanagedAllocateGeneric_AcceptedAllocator_ReturnsWritableStorageAndFreeReleasesIt(
            Allocator allocator
        )
        {
            var strategy = new AllocatorStrategy(allocator);

            // SAFETY: The block holds one long and is freed exactly once in finally.
            unsafe
            {
                long* pointer = null;

                try
                {
                    pointer = EncosyMemoryAPI.Unmanaged.Allocate<long>(strategy);
                    *pointer = long.MinValue;

                    Assert.AreEqual(long.MinValue, *pointer);
                }
                finally
                {
                    EncosyMemoryAPI.Unmanaged.Free(pointer, strategy);
                    pointer = null;
                }
            }
        }

        [Test]
        public void UnmanagedAllocate_ZeroCount_ReturnsNullAndFreeIsANoOp()
        {
            var strategy = new AllocatorStrategy(Allocator.Temp);

            // SAFETY: A count of zero allocates nothing, so the returned null pointer is only compared and freed.
            unsafe
            {
                var pointer = EncosyMemoryAPI.Unmanaged.Allocate(
                      UnsafeUtility.SizeOf<int>()
                    , UnsafeUtility.AlignOf<int>()
                    , 0
                    , strategy
                );

                Assert.AreEqual(IntPtr.Zero, (IntPtr)pointer);

                EncosyMemoryAPI.Unmanaged.Free(pointer, strategy);
            }
        }

        [Test]
        public void UnmanagedFree_NullPointer_DoesNothingForEveryStrategy()
        {
            var strategies = new[] {
                default(AllocatorStrategy),
                new AllocatorStrategy(Allocator.Temp),
                new AllocatorStrategy(Allocator.Persistent),
            };

            // SAFETY: Free documents that a null pointer is ignored, so no memory is read or released.
            unsafe
            {
                foreach (var strategy in strategies)
                {
                    EncosyMemoryAPI.Unmanaged.Free(null, strategy);
                    EncosyMemoryAPI.Unmanaged.Free<int>(null, strategy);
                    EncosyMemoryAPI.Unmanaged.Array.Free<int>(null, 0, strategy);
                    EncosyMemoryAPI.Unmanaged.Array.Free<int>(null, 4, strategy);
                }
            }
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void UnmanagedArrayAllocate_AcceptedAllocator_ReturnsWritableStorage(Allocator allocator)
        {
            var strategy = new AllocatorStrategy(allocator);

            // SAFETY: The block holds 1024 ints, every access stays below that count, and it is freed once in finally.
            unsafe
            {
                int* pointer = null;

                try
                {
                    pointer = EncosyMemoryAPI.Unmanaged.Array.Allocate<int>(1024, strategy);

                    for (var i = 0; i < 1024; i++)
                    {
                        pointer[i] = i;
                    }

                    Assert.AreEqual(0, pointer[0]);
                    Assert.AreEqual(1023, pointer[1023]);
                }
                finally
                {
                    EncosyMemoryAPI.Unmanaged.Array.Free(pointer, 1024, strategy);
                    pointer = null;
                }
            }
        }

        [Test]
        public void UnmanagedArrayAllocate_ZeroCount_ReturnsNullAndFreeIsANoOp()
        {
            var strategy = new AllocatorStrategy(Allocator.Temp);

            // SAFETY: A count of zero allocates nothing, so the returned null pointer is only compared and freed.
            unsafe
            {
                var pointer = EncosyMemoryAPI.Unmanaged.Array.Allocate<int>(0, strategy);

                Assert.AreEqual(IntPtr.Zero, (IntPtr)pointer);

                EncosyMemoryAPI.Unmanaged.Array.Free(pointer, 0, strategy);
            }
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void UnmanagedArrayResize_AcceptedAllocator_GrowsShrinksAndKeepsContent(Allocator allocator)
        {
            AssertResizeRoundTrip(1, 2, 3, allocator);
        }

        [Test]
        public void UnmanagedArrayResize_ByteElement_RoundTripBitExact()
        {
            AssertResizeRoundTrip<byte>(0xA5, 0x00, 0xFF, Allocator.Persistent);
        }

        [Test]
        public void UnmanagedArrayResize_ThreeByteStruct_RoundTripBitExact()
        {
            AssertResizeRoundTrip(
                  new Rgb { r = 1, g = 2, b = 3 }
                , new Rgb { r = 0, g = 0, b = 0 }
                , new Rgb { r = 255, g = 254, b = 253 }
                , Allocator.Persistent
            );
        }

        [Test]
        public void UnmanagedArrayResize_LongElement_RoundTripBitExact()
        {
            AssertResizeRoundTrip(long.MinValue, 0L, long.MaxValue, Allocator.Persistent);
        }

        [Test]
        public void UnmanagedArrayResize_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertResizeRoundTrip(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = 0, b = 0, c = 0 }
                , new Wide { a = long.MinValue, b = 7, c = -1 }
                , Allocator.Persistent
            );
        }

        [Test]
        public void UnmanagedArrayResize_RawOverloadWithThreeByteElements_KeepsEveryByte()
        {
            var strategy = new AllocatorStrategy(Allocator.Persistent);

            // SAFETY: The block holds six bytes (two three-byte elements) and is replaced or freed exactly once.
            unsafe
            {
                byte* pointer = null;
                long count = 0;

                try
                {
                    pointer = (byte*)EncosyMemoryAPI.Unmanaged.Array.Resize(null, 0, 2, strategy, 3, 1);
                    count = 2;

                    for (var i = 0; i < 6; i++)
                    {
                        pointer[i] = (byte)(i + 1);
                    }

                    pointer = (byte*)EncosyMemoryAPI.Unmanaged.Array.Resize(pointer, count, 4, strategy, 3, 1);
                    count = 4;

                    for (var i = 0; i < 6; i++)
                    {
                        Assert.AreEqual((byte)(i + 1), pointer[i]);
                    }
                }
                finally
                {
                    EncosyMemoryAPI.Unmanaged.Array.Free(pointer, count * 3, strategy);
                    pointer = null;
                }
            }
        }

        [Test]
        public void UnmanagedArrayResize_ToZeroCount_FreesTheBlockAndReturnsNull()
        {
            var strategy = new AllocatorStrategy(Allocator.Persistent);

            // SAFETY: The block is replaced by the resize, which frees it; only the returned null pointer remains.
            unsafe
            {
                var pointer = EncosyMemoryAPI.Unmanaged.Array.Allocate<int>(4, strategy);
                pointer[0] = 1;

                pointer = EncosyMemoryAPI.Unmanaged.Array.Resize(pointer, 4, 0, strategy);

                Assert.AreEqual(IntPtr.Zero, (IntPtr)pointer);

                pointer = EncosyMemoryAPI.Unmanaged.Array.Resize(pointer, 0, 2, strategy);
                pointer[1] = 5;

                Assert.AreEqual(5, pointer[1]);

                EncosyMemoryAPI.Unmanaged.Array.Free(pointer, 2, strategy);
            }
        }

        [Test]
        public void ArraySet_ZeroCount_WritesNothing()
        {
            var strategy = new AllocatorStrategy(Allocator.Temp);

            // SAFETY: The null pointer is never dereferenced for a zero count; the block holds two ints and is freed
            // once in finally.
            unsafe
            {
                EncosyMemoryAPI.Array.Set<int>(null, 0, 9);

                int* pointer = null;

                try
                {
                    pointer = EncosyMemoryAPI.Unmanaged.Array.Allocate<int>(2, strategy);
                    pointer[0] = 5;
                    pointer[1] = 6;

                    EncosyMemoryAPI.Array.Set(pointer, 0, 9);

                    Assert.AreEqual(5, pointer[0]);
                    Assert.AreEqual(6, pointer[1]);
                }
                finally
                {
                    EncosyMemoryAPI.Unmanaged.Array.Free(pointer, 2, strategy);
                    pointer = null;
                }
            }
        }

        [Test]
        public void ArraySet_DefaultValueOrGivenValue_FillsEveryElementOfEveryElementSize()
        {
            AssertSetFillsEveryElement<byte>(0xA5);
            AssertSetFillsEveryElement(new Rgb { r = 1, g = 2, b = 3 });
            AssertSetFillsEveryElement(long.MinValue);
            AssertSetFillsEveryElement(new Wide { a = 1, b = -2, c = long.MaxValue });
        }

        [Test]
        public void ArrayClear_ZeroCount_ClearsNothing()
        {
            var strategy = new AllocatorStrategy(Allocator.Temp);

            // SAFETY: The block holds two ints and is freed once in finally; a zero count clears nothing.
            unsafe
            {
                int* pointer = null;

                try
                {
                    pointer = EncosyMemoryAPI.Unmanaged.Array.Allocate<int>(2, strategy);
                    pointer[0] = 5;
                    pointer[1] = 6;

                    EncosyMemoryAPI.Array.Clear(pointer, 0);

                    Assert.AreEqual(5, pointer[0]);
                    Assert.AreEqual(6, pointer[1]);

                    EncosyMemoryAPI.Array.Clear(pointer, 2);

                    Assert.AreEqual(0, pointer[0]);
                    Assert.AreEqual(0, pointer[1]);
                }
                finally
                {
                    EncosyMemoryAPI.Unmanaged.Array.Free(pointer, 2, strategy);
                    pointer = null;
                }
            }
        }

        [Test]
        public void ArrayCopy_ZeroCount_CopiesNothing()
        {
            var strategy = new AllocatorStrategy(Allocator.Temp);

            // SAFETY: Both blocks hold two ints and are freed once in finally; a zero count copies nothing.
            unsafe
            {
                int* source = null;
                int* destination = null;

                try
                {
                    source = EncosyMemoryAPI.Unmanaged.Array.Allocate<int>(2, strategy);
                    destination = EncosyMemoryAPI.Unmanaged.Array.Allocate<int>(2, strategy);
                    source[0] = 1;
                    source[1] = 2;
                    destination[0] = 8;
                    destination[1] = 9;

                    EncosyMemoryAPI.Array.Copy(destination, source, 0);

                    Assert.AreEqual(8, destination[0]);
                    Assert.AreEqual(9, destination[1]);
                }
                finally
                {
                    EncosyMemoryAPI.Unmanaged.Array.Free(source, 2, strategy);
                    EncosyMemoryAPI.Unmanaged.Array.Free(destination, 2, strategy);
                    source = null;
                    destination = null;
                }
            }
        }

        [Test]
        public void ArrayCopy_EveryElementSize_CopiesBitExactAndLeavesTheNeighboursAlone()
        {
            AssertCopyIsBitExact<byte>(0xA5, 0xFF);
            AssertCopyIsBitExact(new Rgb { r = 1, g = 2, b = 3 }, new Rgb { r = 255, g = 254, b = 253 });
            AssertCopyIsBitExact(long.MinValue, long.MaxValue);
            AssertCopyIsBitExact(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = long.MinValue, b = 0, c = -1 }
            );
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void CheckByteCountIsReasonable_ExtremeValues_Throw()
        {
            Assert.DoesNotThrow(() => EncosyMemoryAPI.CheckByteCountIsReasonable(1));
            Assert.Throws<InvalidOperationException>(() => EncosyMemoryAPI.CheckByteCountIsReasonable(long.MinValue));
            Assert.Throws<InvalidOperationException>(() => EncosyMemoryAPI.CheckByteCountIsReasonable(long.MaxValue));
        }

        [Test]
        public void DisposeJob_Execute_NullPointer_DoesNothing()
        {
            var job = new EncosyMemoryAPI.DisposeJob {
                allocator = new AllocatorStrategy(Allocator.Persistent),
            };

            Assert.DoesNotThrow(() => job.Execute());
            Assert.DoesNotThrow(() => default(EncosyMemoryAPI.DisposeJob).Execute());
        }

#if UNITY_COLLECTIONS
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [Category("Task045X2Regression")]
        public void ArrayResize_CustomAllocatorDimensionPastInt32_RejectsBeforeUsingStorage(int dimension)
        {
            var allocatorHelper = new AllocatorHelper<RewindableAllocator>(AllocatorManager.Persistent);
            var initialized = false;

            try
            {
                allocatorHelper.Allocator.Initialize(4096);
                initialized = true;
                var oversized = (long)int.MaxValue + 1;
                var size = dimension == 0 ? oversized : 1;
                var newCount = dimension == 1 ? oversized : 0;
                var oldCount = dimension == 2 ? oversized : 0;
                var strategy = new AllocatorStrategy(allocatorHelper.Allocator.Handle);
                var blocksBefore = allocatorHelper.Allocator.BlocksAllocated;

                // SAFETY: Invalid dimensions must throw before allocation, copy, or free of the null old pointer.
                unsafe
                {
                    Assert.Throws<InvalidOperationException>(() => EncosyMemoryAPI.Unmanaged.Array.Resize(
                          oldPointer: null
                        , oldCount: oldCount
                        , newCount: newCount
                        , allocator: strategy
                        , size: size
                        , align: 1
                    ));
                }

                Assert.AreEqual(blocksBefore, allocatorHelper.Allocator.BlocksAllocated);

                // SAFETY: A valid one-byte allocation confirms the same allocator remains usable after rejection.
                unsafe
                {
                    var pointer = EncosyMemoryAPI.Unmanaged.Allocate<byte>(strategy);
                    *pointer = 73;
                    Assert.AreEqual(73, *pointer);
                    EncosyMemoryAPI.Unmanaged.Free(pointer, strategy);
                }
            }
            finally
            {
                if (initialized)
                {
                    allocatorHelper.Allocator.Dispose();
                }

                allocatorHelper.Dispose();
            }
        }
#endif

        [TestCase(false)]
        [TestCase(true)]
        [Category("Task045X2Regression")]
        public void ArrayResize_ByteCountMultiplicationOverflows_RejectsBeforeUsingStorage(bool oldCountOverflows)
        {
            var oldCount = oldCountOverflows ? long.MaxValue : 0;
            var newCount = oldCountOverflows ? 0 : long.MaxValue;

            // SAFETY: Overflow must throw before allocation, copy, or free; no storage is supplied or requested.
            unsafe
            {
                Assert.Throws<OverflowException>(() => EncosyMemoryAPI.Unmanaged.Array.Resize(
                      oldPointer: null
                    , oldCount: oldCount
                    , newCount: newCount
                    , allocator: Allocator.Persistent
                    , size: 2
                    , align: 1
                ));
            }
        }

        private static void AssertResizeRoundTrip<T>(T a, T b, T c, Allocator allocator)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var strategy = new AllocatorStrategy(allocator);

            // SAFETY: pointer and count always describe the live block; each resize replaces and frees the old block
            // and the final block is freed exactly once in finally.
            unsafe
            {
                T* pointer = null;
                long count = 0;

                try
                {
                    pointer = EncosyMemoryAPI.Unmanaged.Array.Resize<T>(null, 0, 2, strategy);
                    count = 2;
                    pointer[0] = a;
                    pointer[1] = b;

                    pointer = EncosyMemoryAPI.Unmanaged.Array.Resize(pointer, count, 3, strategy);
                    count = 3;

                    Assert.AreEqual(a, pointer[0], name);
                    Assert.AreEqual(b, pointer[1], name);

                    pointer[2] = c;
                    pointer = EncosyMemoryAPI.Unmanaged.Array.Resize(pointer, count, 3, strategy);

                    Assert.AreEqual(a, pointer[0], name);
                    Assert.AreEqual(b, pointer[1], name);
                    Assert.AreEqual(c, pointer[2], name);

                    pointer = EncosyMemoryAPI.Unmanaged.Array.Resize(pointer, count, 1, strategy);
                    count = 1;

                    Assert.AreEqual(a, pointer[0], name);
                }
                finally
                {
                    EncosyMemoryAPI.Unmanaged.Array.Free(pointer, count, strategy);
                    pointer = null;
                }
            }
        }

        private static void AssertSetFillsEveryElement<T>(T value)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var strategy = new AllocatorStrategy(Allocator.Temp);

            // SAFETY: The block holds five elements, every access stays below five, and it is freed once in finally.
            unsafe
            {
                T* pointer = null;

                try
                {
                    pointer = EncosyMemoryAPI.Unmanaged.Array.Allocate<T>(5, strategy);

                    EncosyMemoryAPI.Array.Set(pointer, 5, value);

                    for (var i = 0; i < 5; i++)
                    {
                        Assert.AreEqual(value, pointer[i], name);
                    }

                    EncosyMemoryAPI.Array.Set(pointer, 3);

                    for (var i = 0; i < 3; i++)
                    {
                        Assert.AreEqual(default(T), pointer[i], name);
                    }

                    Assert.AreEqual(value, pointer[3], name);
                    Assert.AreEqual(value, pointer[4], name);
                }
                finally
                {
                    EncosyMemoryAPI.Unmanaged.Array.Free(pointer, 5, strategy);
                    pointer = null;
                }
            }
        }

        private static void AssertCopyIsBitExact<T>(T first, T last)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            var strategy = new AllocatorStrategy(Allocator.Temp);

            // SAFETY: Both blocks hold four elements, the copy writes elements 1 and 2 only, and each block is freed
            // once in finally.
            unsafe
            {
                T* source = null;
                T* destination = null;

                try
                {
                    source = EncosyMemoryAPI.Unmanaged.Array.Allocate<T>(4, strategy);
                    destination = EncosyMemoryAPI.Unmanaged.Array.Allocate<T>(4, strategy);
                    EncosyMemoryAPI.Array.Clear(destination, 4);
                    source[0] = last;
                    source[1] = first;
                    source[2] = last;
                    source[3] = first;

                    EncosyMemoryAPI.Array.Copy(destination + 1, source + 1, 2);

                    Assert.AreEqual(default(T), destination[0], name);
                    Assert.AreEqual(first, destination[1], name);
                    Assert.AreEqual(last, destination[2], name);
                    Assert.AreEqual(default(T), destination[3], name);
                }
                finally
                {
                    EncosyMemoryAPI.Unmanaged.Array.Free(source, 4, strategy);
                    EncosyMemoryAPI.Unmanaged.Array.Free(destination, 4, strategy);
                    source = null;
                    destination = null;
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
    }
}
