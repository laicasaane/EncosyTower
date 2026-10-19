using System;
using EncosyTower.Collections;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Collections
{
    // Adapted from Unity.Collections.Tests/NativeReferenceTests.cs against the Native forwarder.
    public partial class ReferenceNativeTests
    {
        [Test]
        public void Constructor_WithValue_StoresValue()
        {
            using var reference = new ReferenceNative<int>(42, Allocator.Temp);

            Assert.IsTrue(reference.IsCreated);
            Assert.AreEqual(42, reference.Value);
        }

        [Test]
        public void Constructor_Default_ClearsToZero()
        {
            using var reference = new ReferenceNative<int>(Allocator.Temp);

            Assert.IsTrue(reference.IsCreated);
            Assert.AreEqual(0, reference.Value);
        }

        [Test]
        public void DefaultReference_IsNotCreated()
        {
            ReferenceNative<int> reference = default;

            Assert.IsFalse(reference.IsCreated);
        }

        [Test]
        public void Value_SetGet()
        {
            var reference = new ReferenceNative<int>(Allocator.Temp);

            try
            {
                reference.Value = 99;
                Assert.AreEqual(99, reference.Value);
            }
            finally
            {
                reference.Dispose();
            }
        }

        [Test]
        public void CopyFrom_CopiesValue()
        {
            using var a = new ReferenceNative<int>(1, Allocator.Temp);
            using var b = new ReferenceNative<int>(2, Allocator.Temp);

            a.CopyFrom(b);

            Assert.AreEqual(2, a.Value);
            Assert.AreEqual(2, b.Value);
        }

        [Test]
        public void CopyTo_CopiesValue()
        {
            using var a = new ReferenceNative<int>(1, Allocator.Temp);
            using var b = new ReferenceNative<int>(2, Allocator.Temp);

            a.CopyTo(b);

            Assert.AreEqual(1, a.Value);
            Assert.AreEqual(1, b.Value);
        }

        [Test]
        public void Equals_ByValue()
        {
            using var a = new ReferenceNative<int>(7, Allocator.Temp);
            using var b = new ReferenceNative<int>(7, Allocator.Temp);
            using var c = new ReferenceNative<int>(8, Allocator.Temp);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a == c);
            Assert.IsTrue(a != c);
            Assert.IsTrue(a.Equals((object)b));
            Assert.IsFalse(a.Equals((object)c));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void StaticCopy_CopiesSourceIntoDestination()
        {
            using var destination = new ReferenceNative<int>(1, Allocator.Temp);
            using var source = new ReferenceNative<int>(2, Allocator.Temp);

            ReferenceNative<int>.Copy(destination, source);

            Assert.AreEqual(2, destination.Value);
            Assert.AreEqual(2, source.Value);
        }

        [Test]
        public void AsReadOnly_ReflectsValue()
        {
            using var reference = new ReferenceNative<int>(55, Allocator.Temp);

            var readOnly = reference.AsReadOnly();
            ReferenceNative<int>.ReadOnly converted = reference;

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(55, readOnly.Value);
            Assert.IsTrue(converted.IsCreated);
            Assert.AreEqual(55, converted.Value);
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var reference = new ReferenceNative<int>(1, Allocator.Temp);
            Assert.IsTrue(reference.IsCreated);

            reference.Dispose();

            Assert.IsFalse(reference.IsCreated);
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var reference = new ReferenceNative<int>(1, Allocator.TempJob);

            var handle = reference.Dispose(default);

            Assert.IsFalse(reference.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void UseAfterDispose_Throws()
        {
            var reference = new ReferenceNative<int>(1, Allocator.Temp);
            reference.Dispose();

            Assert.Catch(() => _ = reference.Value);
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Constructor_AcceptedAllocator_StoresValueAndDisposes(Allocator allocator)
        {
            var withValue = new ReferenceNative<int>(42, allocator);
            var cleared = new ReferenceNative<int>(allocator);
            var uninitialized = new ReferenceNative<int>(allocator, NativeArrayOptions.UninitializedMemory);

            try
            {
                Assert.IsTrue(withValue.IsCreated);
                Assert.IsTrue(cleared.IsCreated);
                Assert.IsTrue(uninitialized.IsCreated);
                Assert.AreEqual(42, withValue.Value);
                Assert.AreEqual(0, cleared.Value);

                uninitialized.Value = 7;

                Assert.AreEqual(7, uninitialized.Value);
            }
            finally
            {
                withValue.Dispose();
                cleared.Dispose();
                uninitialized.Dispose();
            }

            Assert.IsFalse(withValue.IsCreated);
            Assert.IsFalse(cleared.IsCreated);
            Assert.IsFalse(uninitialized.IsCreated);
        }

        [Test]
        public void Value_ElementShapes_RoundTripBitExact()
        {
            var bytes = new ReferenceNative<byte>(0xA5, Allocator.Temp);
            var small = new ReferenceNative<Rgb>(new Rgb { r = 1, g = 2, b = 3 }, Allocator.Temp);
            var longs = new ReferenceNative<long>(long.MinValue, Allocator.Temp);
            var wide = new ReferenceNative<Wide>(new Wide { a = 1, b = -2, c = long.MaxValue }, Allocator.Temp);

            try
            {
                Assert.AreEqual(0xA5, bytes.Value);
                Assert.AreEqual(1, small.Value.r);
                Assert.AreEqual(2, small.Value.g);
                Assert.AreEqual(3, small.Value.b);
                Assert.AreEqual(long.MinValue, longs.Value);
                Assert.AreEqual(1L, wide.Value.a);
                Assert.AreEqual(-2L, wide.Value.b);
                Assert.AreEqual(long.MaxValue, wide.Value.c);

                bytes.Value = 0xFF;
                small.Value = new Rgb { r = 255, g = 254, b = 253 };
                longs.Value = long.MaxValue;
                wide.Value = new Wide { a = long.MinValue, b = 0, c = -1 };

                Assert.AreEqual(0xFF, bytes.Value);
                Assert.AreEqual(255, small.Value.r);
                Assert.AreEqual(253, small.Value.b);
                Assert.AreEqual(long.MaxValue, longs.Value);
                Assert.AreEqual(long.MinValue, wide.Value.a);
                Assert.AreEqual(-1L, wide.Value.c);
            }
            finally
            {
                bytes.Dispose();
                small.Dispose();
                longs.Dispose();
                wide.Dispose();
            }
        }

        [Test]
        public void Copy_ElementShapes_CopyBitExactAndStayIndependent()
        {
            var source = new ReferenceNative<Wide>(new Wide { a = 1, b = -2, c = long.MaxValue }, Allocator.Temp);
            var destination = new ReferenceNative<Wide>(Allocator.Temp);
            var smallSource = new ReferenceNative<Rgb>(new Rgb { r = 9, g = 8, b = 7 }, Allocator.Temp);
            var smallDestination = new ReferenceNative<Rgb>(Allocator.Temp);

            try
            {
                ReferenceNative<Wide>.Copy(destination, source);
                smallDestination.CopyFrom(smallSource);

                Assert.AreEqual(1L, destination.Value.a);
                Assert.AreEqual(-2L, destination.Value.b);
                Assert.AreEqual(long.MaxValue, destination.Value.c);
                Assert.AreEqual(9, smallDestination.Value.r);
                Assert.AreEqual(7, smallDestination.Value.b);

                source.Value = new Wide { a = 5, b = 5, c = 5 };

                Assert.AreEqual(1L, destination.Value.a);

                smallDestination.CopyTo(smallSource);

                Assert.AreEqual(9, smallSource.Value.r);
            }
            finally
            {
                source.Dispose();
                destination.Dispose();
                smallSource.Dispose();
                smallDestination.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Copy_DisposedOperand_Throws()
        {
            var live = new ReferenceNative<int>(1, Allocator.Persistent);
            var disposedSource = new ReferenceNative<int>(2, Allocator.Persistent);
            var disposedDestination = new ReferenceNative<int>(3, Allocator.Persistent);

            try
            {
                disposedSource.Dispose();
                disposedDestination.Dispose();

                Assert.Catch(() => ReferenceNative<int>.Copy(live, disposedSource));
                Assert.Catch(() => ReferenceNative<int>.Copy(disposedDestination, live));
                Assert.Catch(() => live.CopyFrom(disposedSource));
                Assert.Catch(() => live.CopyTo(disposedDestination));
                Assert.AreEqual(1, live.Value);
            }
            finally
            {
                live.Dispose();
            }
        }

        [Test]
        public void AsReadOnly_DefaultReference_Throws()
        {
            ReferenceNative<int> notCreated = default;

            Assert.Throws<ArgumentException>(() => notCreated.AsReadOnly());
            Assert.Throws<ArgumentException>(() => _ = (ReferenceNative<int>.ReadOnly)notCreated);
            Assert.IsFalse(default(ReferenceNative<int>.ReadOnly).IsCreated);
        }

        [Test]
        public void ReadOnly_Constructor_FromDefaultData_Throws()
        {
            var data = default(ReferenceUnsafe<int>.ReadOnly);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var safety = default(AtomicSafetyHandle);

            Assert.Throws<ArgumentException>(() => _ = new ReferenceNative<int>.ReadOnly(in data, ref safety));
#else
            Assert.Throws<ArgumentException>(() => _ = new ReferenceNative<int>.ReadOnly(in data));
#endif
        }

        [Test]
        public void ReadOnly_ElementShapes_ViewTheOwnerValue()
        {
            var small = new ReferenceNative<Rgb>(new Rgb { r = 1, g = 2, b = 3 }, Allocator.Temp);
            var wide = new ReferenceNative<Wide>(new Wide { a = 1, b = -2, c = long.MaxValue }, Allocator.Temp);

            try
            {
                var smallView = small.AsReadOnly();
                ReferenceNative<Wide>.ReadOnly wideView = wide;

                Assert.AreEqual(3, smallView.Value.b);
                Assert.AreEqual(long.MaxValue, wideView.Value.c);

                small.Value = new Rgb { r = 10, g = 20, b = 30 };
                wide.Value = new Wide { a = 0, b = 0, c = 7 };

                Assert.AreEqual(20, smallView.Value.g);
                Assert.AreEqual(7L, wideView.Value.c);
            }
            finally
            {
                small.Dispose();
                wide.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Members_AfterOwnerDispose_Throw()
        {
            var reference = new ReferenceNative<int>(1, Allocator.Persistent);
            var other = new ReferenceNative<int>(2, Allocator.Persistent);
            var readOnly = reference.AsReadOnly();

            try
            {
                reference.Dispose();

                Assert.Catch(() => _ = reference.Value);
                Assert.Catch(() => reference.Value = 5);
                Assert.Catch(() => _ = reference.Equals(other));
                Assert.Catch(() => _ = reference.Equals((object)other));
                Assert.Catch(() => _ = reference.GetHashCode());
                Assert.Catch(() => _ = reference == other);
                Assert.Catch(() => _ = reference != other);
                Assert.Catch(() => reference.AsReadOnly());
                Assert.Catch(() => _ = readOnly.Value);
                Assert.IsFalse(reference.IsCreated);
            }
            finally
            {
                other.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Dispose_Twice_Throws()
        {
            var reference = new ReferenceNative<int>(1, Allocator.Persistent);

            reference.Dispose();

            Assert.IsFalse(reference.IsCreated);
            Assert.Catch(() => reference.Dispose());
        }

        [Test]
        public void Dispose_DefaultReference_IsNoOp()
        {
            ReferenceNative<int> reference = default;

            Assert.DoesNotThrow(() => reference.Dispose());

            var handle = reference.Dispose(default);

            Assert.DoesNotThrow(() => handle.Complete());
            Assert.IsFalse(reference.IsCreated);
        }

        [Test]
        public void DisposeJob_PersistentAllocator_CompletesAndMarksNotCreated()
        {
            var reference = new ReferenceNative<int>(1, Allocator.Persistent);

            var handle = reference.Dispose(default);

            Assert.IsFalse(reference.IsCreated);
            Assert.DoesNotThrow(() => handle.Complete());
        }

        [Test]
        public void NativeDispose_AllocatedData_ReleasesIt()
        {
            var dispose = new ReferenceNativeDispose<int> {
                m_Data = new ReferenceUnsafe<int>(5, Allocator.Persistent),
            };

            Assert.IsTrue(dispose.m_Data.IsCreated);

            dispose.Dispose();

            Assert.IsFalse(dispose.m_Data.IsCreated);
        }

        [Test]
        public void NativeDispose_DefaultData_ThrowsObjectDisposed()
        {
            var dispose = default(ReferenceNativeDispose<int>);

            Assert.Throws<ObjectDisposedException>(() => dispose.Dispose());
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
