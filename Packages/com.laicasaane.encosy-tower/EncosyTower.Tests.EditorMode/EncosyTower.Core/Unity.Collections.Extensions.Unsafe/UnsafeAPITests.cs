using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    public class UnsafeAPITests
    {
        [Test]
        public void AreTypesEqualSize_SameSizeTypes_ReturnsTrue()
        {
            Assert.IsTrue(UnsafeAPI.AreTypesEqualSize<int, uint>());
            Assert.IsTrue(UnsafeAPI.AreTypesEqualSize<int, float>());
            Assert.IsTrue(UnsafeAPI.AreTypesEqualSize<long, ulong>());
            Assert.IsTrue(UnsafeAPI.AreTypesEqualSize<byte, sbyte>());
            Assert.IsTrue(UnsafeAPI.AreTypesEqualSize<Rgb, ThreeBytes>());
            Assert.IsTrue(UnsafeAPI.AreTypesEqualSize<Wide, WideOther>());
        }

        [Test]
        public void AreTypesEqualSize_SameType_ReturnsTrue()
        {
            Assert.IsTrue(UnsafeAPI.AreTypesEqualSize<byte, byte>());
            Assert.IsTrue(UnsafeAPI.AreTypesEqualSize<Rgb, Rgb>());
            Assert.IsTrue(UnsafeAPI.AreTypesEqualSize<Wide, Wide>());
        }

        [Test]
        public void AreTypesEqualSize_DifferentSizeTypes_ReturnsFalse()
        {
            Assert.IsFalse(UnsafeAPI.AreTypesEqualSize<int, long>());
            Assert.IsFalse(UnsafeAPI.AreTypesEqualSize<byte, int>());
            Assert.IsFalse(UnsafeAPI.AreTypesEqualSize<Rgb, int>());
            Assert.IsFalse(UnsafeAPI.AreTypesEqualSize<Rgb, byte>());
            Assert.IsFalse(UnsafeAPI.AreTypesEqualSize<Wide, long>());
        }

        [Test]
        public void IsPointerAligned_NullPointer_ReturnsTrueForEveryType()
        {
            // SAFETY: The null pointer is only compared as an integer value and is never dereferenced.
            unsafe
            {
                Assert.IsTrue(UnsafeAPI.IsPointerAligned<byte>(null));
                Assert.IsTrue(UnsafeAPI.IsPointerAligned<int>(null));
                Assert.IsTrue(UnsafeAPI.IsPointerAligned<long>(null));
                Assert.IsTrue(UnsafeAPI.IsPointerAligned<Rgb>(null));
                Assert.IsTrue(UnsafeAPI.IsPointerAligned<Wide>(null));
            }
        }

        [Test]
        public void IsPointerAligned_AddressesAroundTheAlignment_MatchTheAlignmentOfEveryType()
        {
            AssertAlignmentAroundAddress<byte>();
            AssertAlignmentAroundAddress<short>();
            AssertAlignmentAroundAddress<int>();
            AssertAlignmentAroundAddress<long>();
            AssertAlignmentAroundAddress<Rgb>();
            AssertAlignmentAroundAddress<Wide>();
        }

        [Test]
        public void IsPointerAligned_AllocatedMemory_ReturnsTrueForTheAllocationStart()
        {
            AssertAllocatedMemoryIsAligned<int>();
            AssertAllocatedMemoryIsAligned<long>();
            AssertAllocatedMemoryIsAligned<Wide>();
        }

        private static void AssertAlignmentAroundAddress<T>()
            where T : unmanaged
        {
            var alignment = UnsafeUtility.AlignOf<T>();

            // SAFETY: The addresses are only compared as integer values and are never dereferenced.
            unsafe
            {
                var aligned = (void*)(alignment * 16);

                Assert.IsTrue(UnsafeAPI.IsPointerAligned<T>(aligned), typeof(T).Name);

                if (alignment > 1)
                {
                    var below = (void*)((alignment * 16) - 1);
                    var above = (void*)((alignment * 16) + 1);

                    Assert.IsFalse(UnsafeAPI.IsPointerAligned<T>(below), typeof(T).Name);
                    Assert.IsFalse(UnsafeAPI.IsPointerAligned<T>(above), typeof(T).Name);
                }
            }
        }

        private static void AssertAllocatedMemoryIsAligned<T>()
            where T : unmanaged
        {
            // SAFETY: The block is allocated with the alignment of T, only its address is inspected, and it is freed
            // exactly once in finally.
            unsafe
            {
                var size = UnsafeUtility.SizeOf<T>();
                var memory = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<T>(), Allocator.Temp);

                try
                {
                    Assert.IsTrue(UnsafeAPI.IsPointerAligned<T>(memory), typeof(T).Name);
                }
                finally
                {
                    UnsafeUtility.Free(memory, Allocator.Temp);
                }
            }
        }

        private struct Rgb
        {
            public byte r;
            public byte g;
            public byte b;
        }

        private struct ThreeBytes
        {
            public byte first;
            public byte second;
            public byte third;
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
