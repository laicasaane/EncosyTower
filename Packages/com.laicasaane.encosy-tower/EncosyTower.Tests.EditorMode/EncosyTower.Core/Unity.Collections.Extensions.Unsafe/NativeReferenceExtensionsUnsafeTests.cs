#if UNITY_COLLECTIONS

using System;
using EncosyTower.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    public class NativeReferenceExtensionsUnsafeTests
    {
        [Test]
        public void ValueRefsAndSpans_AliasOriginalReference()
        {
            var reference = new NativeReference<int>(1, AllocatorManager.Persistent);

            try
            {
                ref var writable = ref EncosyNativeReferenceExtensionsUnsafe.ValueAsUnsafeRefRW(
                    reference
                );
                writable = 10;
                ref readonly var readOnly = ref EncosyNativeReferenceExtensionsUnsafe
                    .ValueAsUnsafeRefRO(reference);

                Assert.AreEqual(10, reference.Value);
                Assert.AreEqual(10, readOnly);

                Span<int> span = EncosyNativeReferenceExtensionsUnsafe.AsSpan(reference);
                span[0] = 20;
                var readOnlySpan = EncosyNativeReferenceExtensionsUnsafe.AsReadOnlySpan(
                    reference
                );

                Assert.AreEqual(20, reference.Value);
                Assert.AreEqual(20, readOnlySpan[0]);
            }
            finally
            {
                reference.Dispose();
            }
        }

        [Test]
        public void ValueRefsAndSpans_ByteElement_RoundTripBitExact()
        {
            AssertValueRefsAndSpansRoundTrip<byte>(initial: 0x01, written: 0xFF, spanWritten: 0xA5);
        }

        [Test]
        public void ValueRefsAndSpans_ThreeByteStruct_RoundTripBitExact()
        {
            AssertValueRefsAndSpansRoundTrip(
                  initial: new Rgb { r = 9, g = 8, b = 7 }
                , written: new Rgb { r = 255, g = 254, b = 253 }
                , spanWritten: new Rgb { r = 1, g = 2, b = 3 }
            );
        }

        [Test]
        public void ValueRefsAndSpans_LongElement_RoundTripBitExact()
        {
            AssertValueRefsAndSpansRoundTrip(initial: 1L, written: long.MinValue, spanWritten: long.MaxValue);
        }

        [Test]
        public void ValueRefsAndSpans_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertValueRefsAndSpansRoundTrip(
                  initial: new Wide { a = 7, b = 8, c = 9 }
                , written: new Wide { a = 1, b = -2, c = long.MaxValue }
                , spanWritten: new Wide { a = long.MinValue, b = 0, c = -1 }
            );
        }

        [Test]
        public void Spans_CreatedReference_HaveExactlyOneElement()
        {
            var reference = new NativeReference<int>(5, AllocatorManager.Persistent);

            try
            {
                var span = EncosyNativeReferenceExtensionsUnsafe.AsSpan(reference);
                var readOnlySpan = EncosyNativeReferenceExtensionsUnsafe.AsReadOnlySpan(reference);

                Assert.AreEqual(1, span.Length);
                Assert.AreEqual(1, readOnlySpan.Length);
                Assert.AreEqual(5, span[0]);
                Assert.AreEqual(5, readOnlySpan[0]);
            }
            finally
            {
                reference.Dispose();
            }
        }

        [Test]
        public void Spans_WriteThroughValue_AreSeenByBothSpans()
        {
            var reference = new NativeReference<int>(5, AllocatorManager.Persistent);

            try
            {
                var span = EncosyNativeReferenceExtensionsUnsafe.AsSpan(reference);
                var readOnlySpan = EncosyNativeReferenceExtensionsUnsafe.AsReadOnlySpan(reference);

                reference.Value = 6;

                Assert.AreEqual(6, span[0]);
                Assert.AreEqual(6, readOnlySpan[0]);
            }
            finally
            {
                reference.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ValueRefsAndSpans_AfterDispose_Throw()
        {
            var reference = new NativeReference<int>(1, AllocatorManager.Persistent);
            reference.Dispose();

            Assert.Catch(() => _ = EncosyNativeReferenceExtensionsUnsafe.ValueAsUnsafeRefRW(reference));
            Assert.Catch(() => _ = EncosyNativeReferenceExtensionsUnsafe.ValueAsUnsafeRefRO(reference));
            Assert.Catch(() => EncosyNativeReferenceExtensionsUnsafe.AsSpan(reference));
            Assert.Catch(() => EncosyNativeReferenceExtensionsUnsafe.AsReadOnlySpan(reference));
        }

        private static void AssertValueRefsAndSpansRoundTrip<T>(T initial, T written, T spanWritten)
            where T : unmanaged
        {
            var reference = new NativeReference<T>(initial, AllocatorManager.Persistent);

            try
            {
                Assert.AreEqual(initial, EncosyNativeReferenceExtensionsUnsafe.ValueAsUnsafeRefRO(reference));

                EncosyNativeReferenceExtensionsUnsafe.ValueAsUnsafeRefRW(reference) = written;

                Assert.AreEqual(written, reference.Value);
                Assert.AreEqual(written, EncosyNativeReferenceExtensionsUnsafe.ValueAsUnsafeRefRO(reference));

                var span = EncosyNativeReferenceExtensionsUnsafe.AsSpan(reference);
                span[0] = spanWritten;

                Assert.AreEqual(spanWritten, reference.Value);
                Assert.AreEqual(spanWritten, EncosyNativeReferenceExtensionsUnsafe.AsReadOnlySpan(reference)[0]);
            }
            finally
            {
                reference.Dispose();
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
