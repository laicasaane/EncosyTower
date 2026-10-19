using EncosyTower.Buffers;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Buffers
{
    public partial class AllocatorStrategyTests
    {
        [Test]
        public void Default_IsInvalid()
        {
            AllocatorStrategy strategy = default;

            Assert.IsFalse(strategy.IsValid);
            Assert.IsFalse(strategy.TryGetAllocator(out var allocator));
            Assert.AreEqual(default(Allocator), allocator);
            Assert.AreEqual(Allocator.Invalid, strategy.ToAllocator());

#if UNITY_COLLECTIONS
            Assert.IsFalse(strategy.TryGetAllocatorHandle(out var handle));
            Assert.AreEqual(default(AllocatorManager.AllocatorHandle), handle);
#endif
        }

        [Test]
        public void Allocator_ConstructorAndImplicitConversion_RoundTrip()
        {
            var constructed = new AllocatorStrategy(Allocator.Temp);
            AllocatorStrategy converted = Allocator.Temp;

            Assert.IsTrue(constructed.IsValid);
            Assert.IsTrue(constructed.TryGetAllocator(out var constructedAllocator));
            Assert.AreEqual(Allocator.Temp, constructedAllocator);
            Assert.AreEqual(Allocator.Temp, constructed.ToAllocator());

            Assert.IsTrue(converted.IsValid);
            Assert.IsTrue(converted.TryGetAllocator(out var convertedAllocator));
            Assert.AreEqual(Allocator.Temp, convertedAllocator);
        }

#if UNITY_COLLECTIONS
        [Test]
        public void AllocatorHandle_ConstructorAndImplicitConversion_RoundTrip()
        {
            AllocatorManager.AllocatorHandle source = AllocatorManager.Persistent;
            var constructed = new AllocatorStrategy(source);
            AllocatorStrategy converted = source;

            Assert.IsTrue(constructed.IsValid);
            Assert.IsTrue(constructed.TryGetAllocatorHandle(out var constructedHandle));
            Assert.AreEqual(source, constructedHandle);
            Assert.IsFalse(constructed.TryGetAllocator(out _));
            Assert.AreEqual(source.ToAllocator, constructed.ToAllocator());

            Assert.IsTrue(converted.TryGetAllocatorHandle(out var convertedHandle));
            Assert.AreEqual(source, convertedHandle);
        }
#endif

        [Test]
        public void RawAllocateAndFree_ReturnAlignedWritableStorage()
        {
            var strategy = new AllocatorStrategy(Allocator.Temp);

            unsafe
            {
                void* pointer = null;

                try
                {
                    pointer = strategy.Allocate(sizeof(int), UnsafeUtility.AlignOf<int>(), 4);

                    Assert.AreNotEqual(System.IntPtr.Zero, (System.IntPtr)pointer);
                    Assert.AreEqual(0, (long)pointer % UnsafeUtility.AlignOf<int>());

                    var values = (int*)pointer;
                    values[0] = 11;
                    values[3] = 44;
                    Assert.AreEqual(11, values[0]);
                    Assert.AreEqual(44, values[3]);
                }
                finally
                {
                    strategy.Free(pointer);
                    pointer = null;
                }
            }
        }

        [Test]
        public void GenericAllocateAndFree_ReturnWritableStorage()
        {
            var strategy = new AllocatorStrategy(Allocator.Temp);

            unsafe
            {
                int* pointer = null;

                try
                {
                    pointer = strategy.Allocate<int>();
                    Assert.AreNotEqual(System.IntPtr.Zero, (System.IntPtr)pointer);

                    *pointer = 73;
                    Assert.AreEqual(73, *pointer);
                }
                finally
                {
                    strategy.Free(pointer);
                    pointer = null;
                }
            }
        }

        [Test]
        public void ArrayAllocateAndFree_ReturnWritableStorage()
        {
            var strategy = new AllocatorStrategy(Allocator.Temp);

            unsafe
            {
                int* pointer = null;

                try
                {
                    pointer = strategy.AllocateArray<int>(4);
                    Assert.AreNotEqual(System.IntPtr.Zero, (System.IntPtr)pointer);

                    pointer[0] = 13;
                    pointer[3] = 46;
                    Assert.AreEqual(13, pointer[0]);
                    Assert.AreEqual(46, pointer[3]);
                }
                finally
                {
                    strategy.FreeArray(pointer, 4);
                    pointer = null;
                }
            }
        }

        [Test]
        public void ZeroLengthArrayAllocation_ReturnsNullAndCanBeFreed()
        {
            var strategy = new AllocatorStrategy(Allocator.Temp);

            unsafe
            {
                var pointer = strategy.AllocateArray<int>(0);

                Assert.AreEqual(System.IntPtr.Zero, (System.IntPtr)pointer);
                strategy.FreeArray(pointer, 0);
            }
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void ImplicitConversion_AcceptedAllocator_RoundTrips(Allocator allocator)
        {
            AllocatorStrategy strategy = allocator;

            Assert.IsTrue(strategy.IsValid);
            Assert.IsTrue(strategy.TryGetAllocator(out var result));
            Assert.AreEqual(allocator, result);
            Assert.AreEqual(allocator, strategy.ToAllocator());
        }

#if UNITY_COLLECTIONS
        [Test]
        public void ImplicitConversion_AcceptedAllocatorHandle_RoundTripsThroughTheHandle()
        {
            AllocatorManager.AllocatorHandle[] handles = {
                AllocatorManager.Temp,
                AllocatorManager.TempJob,
                AllocatorManager.Persistent,
            };

            foreach (var source in handles)
            {
                AllocatorStrategy strategy = source;

                Assert.IsTrue(strategy.IsValid);
                Assert.IsTrue(strategy.TryGetAllocatorHandle(out var handle));
                Assert.AreEqual(source, handle);
                Assert.IsFalse(strategy.TryGetAllocator(out _));
                Assert.AreEqual(source.ToAllocator, strategy.ToAllocator());
            }
        }
#endif

        [Test]
        public void Free_NullPointer_DoesNothingForEveryStrategy()
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
                    Assert.DoesNotThrow(() => strategy.Free(null));
                    Assert.DoesNotThrow(() => strategy.Free<int>(null));
                    Assert.DoesNotThrow(() => strategy.FreeArray<int>(null, 0));
                    Assert.DoesNotThrow(() => strategy.FreeArray<int>(null, 4));
                }
            }
        }

        [Test]
        public void Allocate_ZeroCount_ReturnsNull()
        {
            var strategy = new AllocatorStrategy(Allocator.Temp);

            // SAFETY: A zero count allocates nothing, so the returned null pointer is only compared.
            unsafe
            {
                var pointer = strategy.Allocate(sizeof(int), UnsafeUtility.AlignOf<int>(), 0);

                Assert.AreEqual(System.IntPtr.Zero, (System.IntPtr)pointer);

                strategy.Free(pointer);
            }
        }

        [Test]
        public void Allocate_SingleElementOfEveryUnmanagedType_IsAlignedAndBitExact()
        {
            AssertSingleElementRoundTrip(byte.MaxValue);
            AssertSingleElementRoundTrip(new Rgb { r = 1, g = 2, b = 3 });
            AssertSingleElementRoundTrip(long.MinValue);
            AssertSingleElementRoundTrip(new Wide { a = 1, b = -2, c = long.MaxValue });
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4096)]
        public void AllocateArray_Count_EveryElementIsWritableAndTheLastSlotKeepsItsValue(int count)
        {
            var strategy = new AllocatorStrategy(Allocator.Persistent);

            // SAFETY: The array holds count ints, every access stays below count, and it is freed once in finally.
            unsafe
            {
                int* pointer = null;

                try
                {
                    pointer = strategy.AllocateArray<int>(count);

                    Assert.AreNotEqual(System.IntPtr.Zero, (System.IntPtr)pointer);

                    for (var i = 0; i < count; i++)
                    {
                        pointer[i] = i * 3;
                    }

                    for (var i = 0; i < count; i++)
                    {
                        Assert.AreEqual(i * 3, pointer[i]);
                    }
                }
                finally
                {
                    strategy.FreeArray(pointer, count);
                    pointer = null;
                }
            }
        }

#if UNITY_COLLECTIONS
        [Test]
        public void TryGetCustomAllocatorHandle_BuiltInOrDefaultStrategy_ReturnsFalse()
        {
            Assert.IsFalse(default(AllocatorStrategy).TryGetCustomAllocatorHandle(out _));
            Assert.IsFalse(new AllocatorStrategy(Allocator.Temp).TryGetCustomAllocatorHandle(out _));
            Assert.IsFalse(new AllocatorStrategy(Allocator.TempJob).TryGetCustomAllocatorHandle(out _));
            Assert.IsFalse(new AllocatorStrategy(Allocator.Persistent).TryGetCustomAllocatorHandle(out _));
            Assert.IsFalse(new AllocatorStrategy(AllocatorManager.Persistent).TryGetCustomAllocatorHandle(out _));
        }
#endif

        private static void AssertSingleElementRoundTrip<T>(T value)
            where T : unmanaged
        {
            var strategy = new AllocatorStrategy(Allocator.Temp);

            // SAFETY: The block holds one T and is freed exactly once in finally.
            unsafe
            {
                T* pointer = null;

                try
                {
                    pointer = strategy.Allocate<T>();

                    Assert.AreNotEqual(System.IntPtr.Zero, (System.IntPtr)pointer, typeof(T).Name);
                    Assert.AreEqual(0, (long)pointer % UnsafeUtility.AlignOf<T>(), typeof(T).Name);

                    *pointer = value;

                    Assert.AreEqual(value, *pointer, typeof(T).Name);
                }
                finally
                {
                    strategy.Free(pointer);
                    pointer = null;
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
