// Adapted from Unity.Collections.Tests/NativeReferenceTests.cs.

using System;
using EncosyTower.Collections;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace EncosyTower.Tests.Core.Collections
{
    public partial class SharedReferenceTests
    {
        [Test]
        public void Constructors_StoreDefaultAndProvidedValues()
        {
            using var defaultReference = new SharedReference<int>();
            using var valueReference = new SharedReference<int>(42);

            // SAFETY: Both local references remain live while their borrowed value references are read.
            unsafe
            {
                Assert.AreEqual(1, defaultReference.Length);
                Assert.AreEqual(0, defaultReference.ValueRO);
                Assert.AreEqual(1, valueReference.Length);
                Assert.AreEqual(42, valueReference.ValueRO);
            }
        }

        [Test]
        public void ValueRW_WriteIsVisibleThroughValueRO()
        {
            using var reference = new SharedReference<int>(1);

            // SAFETY: reference remains live while both borrowed value references are used.
            unsafe
            {
                reference.ValueRW = 99;

                Assert.AreEqual(99, reference.ValueRO);
            }
        }

        [Test]
        public void Views_ReflectContentAndSpanWrite()
        {
            using var reference = new SharedReference<int>(42);

            // SAFETY: reference remains live while all borrowed views and references are used.
            unsafe
            {
                var span = reference.AsSpan();
                var readOnlySpan = reference.AsReadOnlySpan();
                var managed = reference.AsManagedArray();
                var memory = reference.AsMemory();
                var readOnlyMemory = reference.AsReadOnlyMemory();
                var nativeSpan = reference.AsSpanNative();
                var readOnlyNativeSpan = reference.AsReadOnlySpanNative();
                var nativeArray = reference.AsNativeArray();
                var nativeSlice = reference.AsNativeSlice();

                Assert.AreEqual(1, span.Length);
                Assert.AreEqual(42, span[0]);
                Assert.AreEqual(1, readOnlySpan.Length);
                Assert.AreEqual(42, readOnlySpan[0]);
                Assert.AreEqual(42, managed[0]);
                Assert.AreEqual(42, memory.Span[0]);
                Assert.AreEqual(42, readOnlyMemory.Span[0]);
                Assert.AreEqual(1, nativeSpan.Length);
                Assert.AreEqual(42, nativeSpan[0]);
                Assert.AreEqual(1, readOnlyNativeSpan.Length);
                Assert.AreEqual(42, readOnlyNativeSpan[0]);
                Assert.AreEqual(1, nativeArray.Length);
                Assert.AreEqual(42, nativeArray[0]);
                Assert.AreEqual(1, nativeSlice.Length);
                Assert.AreEqual(42, nativeSlice[0]);

                span[0] = 99;

                Assert.AreEqual(99, reference.ValueRO);
            }
        }

        [Test]
        public void ImplicitAndExplicitOperators_ReflectContent()
        {
            using var reference = new SharedReference<int, uint>(42);

            // SAFETY: reference remains live while all borrowed implicit views are inspected.
            unsafe
            {
                Span<int> span = reference;
                ReadOnlySpan<int> readOnlySpan = reference;
                Memory<int> memory = reference;
                ReadOnlyMemory<int> readOnlyMemory = reference;
                Span<uint> nativeSpan = reference;
                ReadOnlySpan<uint> readOnlyNativeSpan = reference;
                NativeArray<uint> nativeArray = reference;
                NativeSlice<uint> nativeSlice = reference;
                var managed = (int[])reference;

                Assert.AreEqual(42, span[0]);
                Assert.AreEqual(42, readOnlySpan[0]);
                Assert.AreEqual(42, memory.Span[0]);
                Assert.AreEqual(42, readOnlyMemory.Span[0]);
                Assert.AreEqual(42u, nativeSpan[0]);
                Assert.AreEqual(42u, readOnlyNativeSpan[0]);
                Assert.AreEqual(42u, nativeArray[0]);
                Assert.AreEqual(42u, nativeSlice[0]);
                Assert.AreEqual(42, managed[0]);
            }
        }

        [Test]
        public void Dispose_CanBeCalledTwice()
        {
            var reference = new SharedReference<int>(42);

            Assert.DoesNotThrow(reference.Dispose);
            Assert.DoesNotThrow(reference.Dispose);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ValueRO_AfterDispose_Throws()
        {
            var reference = new SharedReference<int>(42);
            reference.Dispose();

            // SAFETY: This test intentionally reads a borrowed reference after owner disposal to verify checks.
            unsafe
            {
                Assert.Catch(() => _ = reference.ValueRO);
            }
        }

        [Test]
        public void ValueRW_DefaultReference_StartsAtDefaultAndBumpsTheVersion()
        {
            using var reference = new SharedReference<int>();
            var version = reference._version;

            // SAFETY: reference remains live while the borrowed value references are used.
            unsafe
            {
                Assert.AreEqual(0, reference.ValueRO);
                Assert.AreEqual(version, reference._version);

                ref var value = ref reference.ValueRW;

                Assert.AreEqual(0, value);
                Assert.AreNotEqual(version, reference._version);

                value = 5;

                Assert.AreEqual(5, reference.ValueRO);
            }
        }

        [Test]
        public void AccessAfterDispose_ThrowsNotCreated()
        {
            var reference = new SharedReference<int>(42);

            reference.Dispose();

            Assert.Throws<ArgumentException>(() => _ = reference.Length);
            Assert.Throws<ArgumentException>(() => _ = reference.AsManagedArray());
            Assert.Throws<ArgumentException>(() => _ = reference.AsMemory());
            Assert.Throws<ArgumentException>(() => _ = reference.AsReadOnlyMemory());

            // SAFETY: The lambdas only provoke the creation check; no borrowed value escapes.
            unsafe
            {
                Assert.Throws<ArgumentException>(() => { reference.AsSpan(); });
                Assert.Throws<ArgumentException>(() => { reference.AsReadOnlySpan(); });
                Assert.Throws<ArgumentException>(() => { reference.AsSpanNative(); });
                Assert.Throws<ArgumentException>(() => { reference.AsReadOnlySpanNative(); });
                Assert.Throws<ArgumentException>(() => { reference.AsNativeArray(); });
                Assert.Throws<ArgumentException>(() => { reference.AsNativeSlice(); });
                Assert.Throws<ArgumentException>(() => { _ = reference.ValueRW; });
                Assert.Throws<ArgumentException>(() => { _ = reference.ValueRO; });
            }
        }

        [Test]
        public void Dispose_AfterWritingThroughTheViews_CanBeCalledTwice()
        {
            var reference = new SharedReference<long>(1L);

            // SAFETY: reference remains live while the borrowed span is used.
            unsafe
            {
                reference.AsSpan()[0] = long.MaxValue;
            }

            Assert.DoesNotThrow(reference.Dispose);
            Assert.DoesNotThrow(reference.Dispose);
        }

        [Test]
        public void Spans_ElementSizes_RoundTripBitExact()
        {
            AssertSpansRoundTrip((byte)255);
            AssertSpansRoundTrip(long.MinValue);
            AssertSpansRoundTrip(new Rgb { r = 255, g = 0, b = 128 });
            AssertSpansRoundTrip(new Wide { a = long.MinValue, b = -1, c = long.MaxValue });
        }

        [Test]
        public void NativeSpans_NativeTypeOfTheSameSize_ReadAndWriteTheSameBits()
        {
            using var reference = new SharedReference<float, int>(1.5f);

            // SAFETY: reference remains live while the borrowed spans are used.
            unsafe
            {
                Assert.AreEqual(BitConverter.SingleToInt32Bits(1.5f), reference.AsSpanNative()[0]);
                Assert.AreEqual(BitConverter.SingleToInt32Bits(1.5f), reference.AsReadOnlySpanNative()[0]);

                reference.AsSpanNative()[0] = BitConverter.SingleToInt32Bits(-2.25f);

                Assert.AreEqual(-2.25f, reference.ValueRO);
                Assert.AreEqual(1, reference.AsSpanNative().Length);
                Assert.AreEqual(1, reference.AsReadOnlySpanNative().Length);
            }
        }

        [Test]
        public void NativeSlice_DefaultReference_HasOneDefaultElement()
        {
            using var reference = new SharedReference<int>();

            // SAFETY: reference remains live while the borrowed slices are inspected.
            unsafe
            {
                var slice = reference.AsNativeSlice();
                NativeSlice<int> implicitSlice = reference;

                Assert.AreEqual(1, slice.Length);
                Assert.AreEqual(1, implicitSlice.Length);
                Assert.AreEqual(0, slice[0]);
                Assert.AreEqual(0, implicitSlice[0]);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void NativeSlice_IndexOutsideTheReference_Throws()
        {
            using var reference = new SharedReference<int>(42);

            // SAFETY: reference remains live while the borrowed slices are inspected.
            unsafe
            {
                var slice = reference.AsNativeSlice();
                NativeSlice<int> implicitSlice = reference;

                Assert.Throws<IndexOutOfRangeException>(() => _ = slice[-1]);
                Assert.Throws<IndexOutOfRangeException>(() => _ = slice[1]);
                Assert.Throws<IndexOutOfRangeException>(() => _ = implicitSlice[-1]);
                Assert.Throws<IndexOutOfRangeException>(() => _ = implicitSlice[1]);
            }
        }

        [Test]
        public void NativeSlice_WriteThroughTheSlice_IsSeenByTheOwner()
        {
            using var reference = new SharedReference<int>(1);

            // SAFETY: reference remains live while the borrowed slices are used.
            unsafe
            {
                var slice = reference.AsNativeSlice();
                NativeSlice<int> implicitSlice = reference;

                slice[0] = 10;

                Assert.AreEqual(10, reference.ValueRO);
                Assert.AreEqual(10, implicitSlice[0]);

                implicitSlice[0] = 20;

                Assert.AreEqual(20, reference.ValueRO);
                Assert.AreEqual(20, slice[0]);
            }
        }

        [Test]
        public void NativeSlice_ElementSizes_RoundTripBitExact()
        {
            AssertNativeSliceRoundTrip((byte)255);
            AssertNativeSliceRoundTrip(long.MinValue);
            AssertNativeSliceRoundTrip(new Rgb { r = 255, g = 0, b = 128 });
            AssertNativeSliceRoundTrip(new Wide { a = long.MinValue, b = -1, c = long.MaxValue });
        }

        [Test]
        public void GetUnsafeBufferPointer_PointerAndValueAlias_SeeEachOthersWrites()
        {
            using var reference = new SharedReference<int>();

            // SAFETY: reference stays live while the pointer and the borrowed reference are used.
            unsafe
            {
                var pointer = reference.GetUnsafeBufferPointer();

                Assert.AreEqual(0, pointer[0]);

                pointer[0] = 7;

                Assert.AreEqual(7, reference.ValueRO);

                reference.ValueRW = 9;

                Assert.AreEqual(9, pointer[0]);
            }
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
        [Test]
        public void GetSafetyHandle_LiveReference_ReturnsALiveHandleAndDisposedReferenceThrows()
        {
            var reference = new SharedReference<int>();

            Assert.IsFalse(AtomicSafetyHandle.IsDefaultValue(reference.GetSafetyHandle()));

            reference.Dispose();

            Assert.Throws<ArgumentException>(() => reference.GetSafetyHandle());
        }
#endif

        [Test]
        public void OnBeforeSerialize_DefaultReference_StoresTheDefaultValue()
        {
            using var reference = new SharedReference<int>();

            reference._serializedValue = 99;

            ((ISerializationCallbackReceiver)reference).OnBeforeSerialize();

            Assert.AreEqual(0, reference._serializedValue);
        }

        [Test]
        public void OnBeforeSerialize_DisposedReference_StoresTheDefaultValue()
        {
            var reference = new SharedReference<int>(42);

            reference.Dispose();

            reference._serializedValue = 99;

            ((ISerializationCallbackReceiver)reference).OnBeforeSerialize();

            Assert.AreEqual(0, reference._serializedValue);
        }

        private static void AssertSpansRoundTrip<T>(T value)
            where T : unmanaged
        {
            using var reference = new SharedReference<T>(value);
            var expected = new[] { value };

            // SAFETY: reference outlives the spans, which are only inspected here.
            unsafe
            {
                var span = reference.AsSpan();
                var readOnlySpan = reference.AsReadOnlySpan();
                var nativeSpan = reference.AsSpanNative();
                var readOnlyNativeSpan = reference.AsReadOnlySpanNative();

                Assert.AreEqual(1, span.Length);
                Assert.AreEqual(1, readOnlySpan.Length);
                Assert.AreEqual(1, nativeSpan.Length);
                Assert.AreEqual(1, readOnlyNativeSpan.Length);
                SpanBytesAssert.AreEqual(expected, span);
                SpanBytesAssert.AreEqual(expected, readOnlySpan);
                SpanBytesAssert.AreEqual(expected, nativeSpan);
                SpanBytesAssert.AreEqual(expected, readOnlyNativeSpan);
            }
        }

        private static void AssertNativeSliceRoundTrip<T>(T value)
            where T : unmanaged
        {
            using var reference = new SharedReference<T>(value);

            // SAFETY: reference outlives the slices, which are only inspected here.
            unsafe
            {
                var slice = reference.AsNativeSlice();
                NativeSlice<T> implicitSlice = reference;

                Assert.AreEqual(1, slice.Length);
                Assert.AreEqual(1, implicitSlice.Length);
                SpanBytesAssert.AreEqual(new[] { value }, new[] { slice[0] });
                SpanBytesAssert.AreEqual(new[] { value }, new[] { implicitSlice[0] });
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
