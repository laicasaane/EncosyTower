#if UNITY_COLLECTIONS

using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    public class NativeListExtensionsUnsafeTests
    {
        [Test]
        public void ElementRefs_AliasOriginalList()
        {
            var list = new NativeList<int>(3, AllocatorManager.Persistent);

            try
            {
                list.Add(1);
                list.Add(2);
                list.Add(3);

                ref var writable = ref EncosyNativeListExtensionsUnsafe.ElementAsUnsafeRefRW(
                      list
                    , 1
                );
                writable = 20;
                ref readonly var readOnly = ref EncosyNativeListExtensionsUnsafe
                    .ElementAsUnsafeRefRO(
                          list
                        , 1
                    );

                Assert.AreEqual(20, list[1]);
                Assert.AreEqual(20, readOnly);
            }
            finally
            {
                list.Dispose();
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
        public void ElementRefs_AfterGrowth_AliasTheNewStorage()
        {
            var list = new NativeList<int>(1, AllocatorManager.Persistent);

            try
            {
                list.Add(1);

                for (var i = 2; i <= 40; i++)
                {
                    list.Add(i);
                }

                EncosyNativeListExtensionsUnsafe.ElementAsUnsafeRefRW(list, 0) = 99;
                EncosyNativeListExtensionsUnsafe.ElementAsUnsafeRefRW(list, list.Length - 1) = 77;

                Assert.AreEqual(99, list[0]);
                Assert.AreEqual(77, list[list.Length - 1]);
                Assert.AreEqual(99, EncosyNativeListExtensionsUnsafe.ElementAsUnsafeRefRO(list, 0));
                Assert.AreEqual(2, list[1]);
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ElementRefs_AfterDispose_Throw()
        {
            var list = new NativeList<int>(3, AllocatorManager.Persistent);
            list.Add(1);
            list.Dispose();

            Assert.Catch(() => _ = EncosyNativeListExtensionsUnsafe.ElementAsUnsafeRefRW(list, 0));
            Assert.Catch(() => _ = EncosyNativeListExtensionsUnsafe.ElementAsUnsafeRefRO(list, 0));
        }

        private static void AssertElementRefsRoundTrip<T>(T first, T second)
            where T : unmanaged
        {
            var list = new NativeList<T>(3, AllocatorManager.Persistent);

            try
            {
                list.Resize(3, NativeArrayOptions.ClearMemory);

                EncosyNativeListExtensionsUnsafe.ElementAsUnsafeRefRW(list, 0) = first;
                EncosyNativeListExtensionsUnsafe.ElementAsUnsafeRefRW(list, 2) = second;
                ref readonly var readOnly = ref EncosyNativeListExtensionsUnsafe.ElementAsUnsafeRefRO(list, 2);

                Assert.AreEqual(first, list[0]);
                Assert.AreEqual(default(T), list[1]);
                Assert.AreEqual(second, list[2]);
                Assert.AreEqual(second, readOnly);
            }
            finally
            {
                list.Dispose();
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

#endif
