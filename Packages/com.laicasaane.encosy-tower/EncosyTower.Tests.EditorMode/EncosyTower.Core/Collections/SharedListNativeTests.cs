using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Collections
{
    // Adapted from Unity.Collections.Tests/NativeListTests.cs against the SharedListNative view.
    // The view is a fixed-capacity window onto the owner's shared buffer/count/version.
    public partial class SharedListNativeTests
    {
        [Test]
        public void AsNative_ReflectsOwnerContent()
        {
            using var list = new SharedList<int>(8);
            list.Add(1);
            list.Add(2);
            list.Add(3);

            SharedListNative<int> view;

            // SAFETY: list remains the live owner of the borrowed native view.
            unsafe
            {
                view = list.AsNative();
            }

            Assert.IsTrue(view.IsCreated);
            Assert.AreEqual(3, view.Count);
            Assert.AreEqual(1, view[0]);
            Assert.AreEqual(2, view[1]);
            Assert.AreEqual(3, view[2]);
        }

        [Test]
        public void DefaultAndImplicitConversion_ReportCreationStateAndOwnerValues()
        {
            SharedListNative<int> empty = default;
            using var list = new SharedList<int, int>(new[] { 1, 2 });
            SharedListNative<int> view;

            // SAFETY: list remains the live owner of the borrowed implicit native view.
            unsafe
            {
                view = list;
            }

            Assert.IsFalse(empty.IsCreated);
            Assert.IsTrue(view.IsCreated);
            Assert.AreEqual(2, view.Count);
            Assert.GreaterOrEqual(view.Capacity, 2);
            CollectionAssert.AreEqual(new[] { 1, 2 }, view.ToArray());
        }

        [Test]
        public void GenericOwnerAsNative_UsesNativeElementTypeAndAliasesOwnerStorage()
        {
            using var list = new SharedList<int, uint>(new[] { 1, 2 });
            SharedListNative<uint> view;

            // SAFETY: list remains the live owner of the borrowed native-element view.
            unsafe
            {
                view = list.AsNative();
            }

            Assert.AreEqual(1u, view[0]);

            view[1] = 20u;

            Assert.AreEqual(20, list[1]);
        }

        [Test]
        public void ViewAdd_WithinCapacity_OwnerObservesChange()
        {
            using var list = new SharedList<int>(8);
            list.Add(1);

            SharedListNative<int> view;

            // SAFETY: list remains the live owner of the borrowed native view.
            unsafe
            {
                view = list.AsNative();
            }
            view.Add(2);
            view.Add(3);

            Assert.AreEqual(3, view.Count);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
        }

        [Test]
        public void ValueAndInOverloads_AddAndInsertMutateOwner()
        {
            using var list = new SharedList<int>(8);
            SharedListNative<int> view;

            // SAFETY: list remains the live owner of the borrowed native view.
            unsafe
            {
                view = list.AsNative();
            }
            var two = 2;
            var four = 4;

            view.Add(1);
            view.Add(in two);
            view.Insert(2, 3);
            view.Insert(3, in four);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public void AddRange_AllOverloadsAppendRequestedValues()
        {
            using var list = new SharedList<int>(20);
            using var native = new NativeArray<int>(new[] { 5, 6, 7 }, Allocator.Temp);
            using var sliceOwner = new NativeArray<int>(new[] { 0, 8, 9, 10 }, Allocator.Temp);
            var slice = new NativeSlice<int>(sliceOwner, 1, 3);
            SharedListNative<int> view;

            // SAFETY: list remains the live owner of the borrowed native view.
            unsafe
            {
                view = list.AsNative();
            }

            view.AddRange(new[] { 1, 2 });
            view.AddRange(new[] { 3, 4, 99 }, 2);
            view.AddRange(native);
            view.AddRange(native, 2);
            view.AddRange(in slice);
            view.AddRange(in slice, 2);

            CollectionAssert.AreEqual(
                new[] { 1, 2, 3, 4, 5, 6, 7, 5, 6, 8, 9, 10, 8, 9 },
                list.ToArray()
            );
        }

        [Test]
        public void ViewIndexerSet_OwnerObservesChange()
        {
            using var list = new SharedList<int>(8);
            list.Add(1);
            list.Add(2);

            SharedListNative<int> view;

            // SAFETY: list remains the live owner of the borrowed native view.
            unsafe
            {
                view = list.AsNative();
            }
            view[1] = 99;

            Assert.AreEqual(99, list[1]);
        }

        [Test]
        public void ElementAtCopyFromAndTryCopyFrom_MutateOwnerOrReturnFalse()
        {
            using var list = new SharedList<int>(new[] { 0, 0, 0, 0, 0, 0 });
            SharedListNative<int> view;

            // SAFETY: list remains the live owner of the borrowed native view and element reference.
            unsafe
            {
                view = list.AsNative();

                ref var first = ref view.ElementAt(0);
                first = 1;
            }
            view.CopyFrom(new[] { 2, 3 });
            view.CopyFrom(new[] { 4, 5, 99 }, 2);
            view.CopyFrom(2, new[] { 6, 7 });
            view.CopyFrom(4, new[] { 8, 9, 99 }, 2);

            CollectionAssert.AreEqual(new[] { 4, 5, 6, 7, 8, 9 }, list.ToArray());

            Assert.IsTrue(view.TryCopyFrom(new[] { 10, 11 }));
            Assert.IsTrue(view.TryCopyFrom(new[] { 12, 13, 99 }, 2));
            Assert.IsTrue(view.TryCopyFrom(2, new[] { 14, 15 }));
            Assert.IsTrue(view.TryCopyFrom(4, new[] { 16, 17, 99 }, 2));
            Assert.IsFalse(view.TryCopyFrom(5, new[] { 18, 19 }));

            CollectionAssert.AreEqual(new[] { 12, 13, 14, 15, 16, 17 }, list.ToArray());
        }

        [Test]
        public void CopyToAndTryCopyTo_SpanAndArrayOverloadsCopyOrReturnFalse()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });
            SharedListNative<int> view;

            // SAFETY: list remains the live owner of the borrowed native view.
            unsafe
            {
                view = list.AsNative();
            }
            var array = new int[6];
            var full = new int[4];
            var partial = new int[2];
            var offset = new int[2];
            var explicitLength = new int[3];

            view.CopyTo(array, 1);
            view.CopyTo(full);
            view.CopyTo(partial.AsSpan(), 2);
            view.CopyTo(1, offset);
            view.CopyTo(1, explicitLength, 2);

            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 0 }, array);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, full);
            CollectionAssert.AreEqual(new[] { 1, 2 }, partial);
            CollectionAssert.AreEqual(new[] { 2, 3 }, offset);
            CollectionAssert.AreEqual(new[] { 2, 3, 0 }, explicitLength);

            Assert.IsTrue(view.TryCopyTo(full));
            Assert.IsTrue(view.TryCopyTo(partial, 2));
            Assert.IsTrue(view.TryCopyTo(1, offset));
            Assert.IsTrue(view.TryCopyTo(1, explicitLength, 2));
            Assert.IsFalse(view.TryCopyTo(3, partial));
        }

        [Test]
        public void CopyTo_NativeArrayAndSliceOverloadsCopyRequestedRanges()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });
            using var full = new NativeArray<int>(4, Allocator.Temp);
            using var offset = new NativeArray<int>(3, Allocator.Temp);
            using var partial = new NativeArray<int>(3, Allocator.Temp);
            using var sliceOwner = new NativeArray<int>(5, Allocator.Temp);
            var fullSlice = new NativeSlice<int>(sliceOwner, 1, 3);
            var partialSlice = new NativeSlice<int>(sliceOwner, 2, 2);
            SharedListNative<int> view;

            // SAFETY: list remains the live owner of the borrowed native view.
            unsafe
            {
                view = list.AsNative();
            }

            view.CopyTo(full);
            view.CopyTo(1, offset);
            view.CopyTo(1, partial, 2);
            view.CopyTo(1, in fullSlice);
            view.CopyTo(2, in partialSlice, 2);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, full.ToArray());
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, offset.ToArray());
            CollectionAssert.AreEqual(new[] { 2, 3, 0 }, partial.ToArray());
            CollectionAssert.AreEqual(new[] { 0, 2, 3, 4, 0 }, sliceOwner.ToArray());
        }

        [Test]
        public void RemoveClearReplicateAndViewsMutateSharedStorage()
        {
            using var list = new SharedList<int>(12);

            // SAFETY: list remains live while each borrowed view is consumed before owner disposal.
            unsafe
            {
                var view = list.AsNative();

                var defaults = view.AddReplicate(2);
                CollectionAssert.AreEqual(new[] { 0, 0 }, defaults.ToArray());

                var values = view.AddReplicate(7, 3);
                CollectionAssert.AreEqual(new[] { 7, 7, 7 }, values.ToArray());

                var noInit = view.AddReplicateNoInit(2);
                noInit[0] = 8;
                noInit[1] = 9;

                Assert.AreEqual(9, view[^1]);
                view.RemoveAt(view.Count - 1);

                view.RemoveAt(0);
                view.RemoveRange(0, 2);
                view.RemoveAtSwapBack(0);

                var span = view.AsSpan();
                var readOnlySpan = view.AsReadOnlySpan();
                var slice = view.AsNativeSlice();
                span[0] = 10;
                slice[1] = 11;

                Assert.AreEqual(10, readOnlySpan[0]);
                CollectionAssert.AreEqual(new[] { 10, 11 }, list.ToArray());

                view.Clear();

                Assert.AreEqual(0, list.Count);
            }
        }

        [Test]
        public void ReinterpretAndReadOnlySurface_AliasOwnerAndCopyAllRanges()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });

            // SAFETY: list remains live and stable while all native/read-only aliases are used.
            unsafe
            {
                var view = list.AsNative();
                var reinterpreted = view.Reinterpret<uint>();
                SharedListNative<int>.ReadOnly readOnly = view;
                ReadOnlySpan<int> implicitSpan = readOnly;
                var array = new int[6];
                var full = new int[4];
                var partial = new int[2];
                var offset = new int[2];
                var explicitLength = new int[3];

                reinterpreted[0] = 10u;

                Assert.AreEqual(10, list[0]);
                Assert.IsTrue(readOnly.IsCreated);
                Assert.AreEqual(4, readOnly.Count);
                Assert.GreaterOrEqual(readOnly.Capacity, 4);
                Assert.IsTrue(readOnly.IsReadOnly);
                Assert.AreEqual(3, readOnly[2]);
                CollectionAssert.AreEqual(new[] { 10, 2, 3, 4 }, implicitSpan.ToArray());
                CollectionAssert.AreEqual(new[] { 10, 2, 3, 4 }, readOnly.AsReadOnlySpan().ToArray());
                CollectionAssert.AreEqual(new[] { 10, 2, 3, 4 }, readOnly.ToArray());

                readOnly.CopyTo(array, 1);
                readOnly.CopyTo(full);
                readOnly.CopyTo(partial.AsSpan(), 2);
                readOnly.CopyTo(1, offset);
                readOnly.CopyTo(1, explicitLength, 2);

                CollectionAssert.AreEqual(new[] { 0, 10, 2, 3, 4, 0 }, array);
                CollectionAssert.AreEqual(new[] { 10, 2, 3, 4 }, full);
                CollectionAssert.AreEqual(new[] { 10, 2 }, partial);
                CollectionAssert.AreEqual(new[] { 2, 3 }, offset);
                CollectionAssert.AreEqual(new[] { 2, 3, 0 }, explicitLength);

                Assert.IsTrue(readOnly.TryCopyTo(full));
                Assert.IsTrue(readOnly.TryCopyTo(partial, 2));
                Assert.IsTrue(readOnly.TryCopyTo(1, offset));
                Assert.IsTrue(readOnly.TryCopyTo(1, explicitLength, 2));
                Assert.IsFalse(readOnly.TryCopyTo(3, partial));

                var reinterpretedReadOnly = readOnly.Reinterpret<uint>();
                Assert.AreEqual(10u, reinterpretedReadOnly[0]);
            }
        }

        [Test]
        public void OwnerAndReadOnlyEnumerators_ConstructMoveResetAndDisposeDirectly()
        {
            using var list = new SharedList<int>(new[] { 1, 2 });

            // SAFETY: list remains live and unmodified while both borrowed enumerators are consumed.
            unsafe
            {
                var view = list.AsNative();
                var ownerEnumerator = view.GetEnumerator();

                Assert.IsTrue(ownerEnumerator.MoveNext());
                Assert.AreEqual(1, ownerEnumerator.Current);
                ownerEnumerator.Reset();
                Assert.IsTrue(ownerEnumerator.MoveNext());
                ownerEnumerator.Dispose();

                var readOnly = view.AsReadOnly();
                var readOnlyEnumerator = readOnly.GetEnumerator();

                Assert.IsTrue(readOnlyEnumerator.MoveNext());
                Assert.AreEqual(1, readOnlyEnumerator.Current);
                Assert.IsTrue(readOnlyEnumerator.MoveNext());
                Assert.AreEqual(2, readOnlyEnumerator.Current);
                Assert.IsFalse(readOnlyEnumerator.MoveNext());
                readOnlyEnumerator.Reset();
                Assert.IsTrue(readOnlyEnumerator.MoveNext());
                readOnlyEnumerator.Dispose();

                var directEnumerator = new SharedListNative<int>.Enumerator(readOnly);
                Assert.IsTrue(directEnumerator.MoveNext());
                directEnumerator.Dispose();
            }
        }

        [Test]
        public void CurrentViewCreatedAfterOwnerGrowth_WritesOwnerStorage()
        {
            using var list = new SharedList<int>(1);
            list.Add(1);
            SharedListNative<int> stale;

            // SAFETY: stale is read only before the owner grows and invalidates it.
            unsafe
            {
                stale = list.AsNative();
            }
            var staleCapacity = stale.Capacity;

            list.IncreaseCapacityBy(4);

            SharedListNative<int> current;

            // SAFETY: list remains live and stable after growth while the refreshed view is used.
            unsafe
            {
                current = list.AsNative();
            }
            current.Add(2);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(2, list[1]);
            Assert.Greater(current.Capacity, staleCapacity);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void NativeView_CannotGrowOwnerStorage()
        {
            using var list = new SharedList<int>(2);
            list.AddRange(new[] { 1, 2 });
            SharedListNative<int> view;

            // SAFETY: list remains live while the fixed-capacity borrowed view is exercised.
            unsafe
            {
                view = list.AsNative();
            }
            var capacity = list.Capacity;

            Assert.Throws<InvalidOperationException>(() => view.Add(3));
            Assert.AreEqual(capacity, list.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void ViewCreatedBeforeOwnerResize_ThrowsOnWrite()
        {
            // A view holds a copy of the owner buffer's safety handle at AsNative() time.
            // Growing the owner past capacity re-pins the buffer and releases that handle,
            // so a write through the stale view must throw.
            using var list = new SharedList<int>(2);
            list.Add(1);

            SharedListNative<int> staleView;

            // SAFETY: This test intentionally retains the borrowed view across owner resize.
            unsafe
            {
                staleView = list.AsNative();
            }

            for (var i = 0; i < 64; i++)
            {
                list.Add(i); // forces AllocateMore -> re-pin -> old handle released
            }

            Assert.Catch(() => staleView.Add(123));
        }

        [Test]
        public void RemoveRange_ZeroLengthBoundaries_PinContract()
        {
            // Pins the family-wide contract: RemoveRange validates startIndex < Count before
            // looking at length, so a zero-length remove at Count (including on an empty list)
            // throws, unlike BCL List<T>.RemoveRange. Zero-length removes at a valid index
            // are no-ops.
            using var emptyOwner = new SharedList<int>(4);
            SharedListNative<int> emptyView;

            // SAFETY: emptyOwner remains live while the borrowed view checks the boundary contract.
            unsafe
            {
                emptyView = emptyOwner.AsNative();
            }

            Assert.Throws<InvalidOperationException>(() => emptyView.RemoveRange(0, 0));

            using var owner = new SharedList<int>(new[] { 1, 2, 3 });
            SharedListNative<int> view;

            // SAFETY: owner remains live while the borrowed view checks the boundary contract.
            unsafe
            {
                view = owner.AsNative();
            }

            Assert.Throws<InvalidOperationException>(() => view.RemoveRange(3, 0));

            view.RemoveRange(0, 0);
            view.RemoveRange(2, 0);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, view.ToArray());
            Assert.AreEqual(3, owner.Count);
        }

        [Test]
        public unsafe void Constructor_BorrowedHeader_ViewsTheHeaderAndMarkChangedBumpsTheVersion()
        {
            using var storage = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            var safety = AtomicSafetyHandle.Create();
            var view = new SharedListNative<int>(&header, safety);
#else
            var view = new SharedListNative<int>(&header);
#endif

            try
            {
                Assert.IsTrue(view.IsCreated);
                Assert.AreEqual(4, view.Capacity);
                Assert.AreEqual(0, view.Count);

                view.Add(5);

                Assert.AreEqual(1, storage.Count);
                Assert.AreEqual(5, storage.buffer[0]);

                var version = storage.Version;

                view.MarkChanged();

                Assert.AreEqual(version + 1, storage.Version);
            }
            finally
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                AtomicSafetyHandle.Release(safety);
#endif
            }
        }

        [Test]
        public unsafe void Constructor_NullHeader_IsNotCreated()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            var safety = AtomicSafetyHandle.Create();

            try
            {
                var view = new SharedListNative<int>(null, safety);
                var readOnly = new SharedListNative<int>.ReadOnly(null, safety);

                Assert.IsFalse(view.IsCreated);
                Assert.IsFalse(readOnly.IsCreated);
            }
            finally
            {
                AtomicSafetyHandle.Release(safety);
            }
#else
            var view = new SharedListNative<int>(null);
            var readOnly = new SharedListNative<int>.ReadOnly(null);

            Assert.IsFalse(view.IsCreated);
            Assert.IsFalse(readOnly.IsCreated);
#endif
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Indexer_OutOfRange_Throws()
        {
            using var empty = new SharedList<int>(4);
            using var list = new SharedList<int>(new[] { 1, 2 });
            var emptyView = empty.AsNative();
            var view = list.AsNative();

            Assert.Throws<InvalidOperationException>(() => _ = emptyView[0]);
            Assert.Throws<InvalidOperationException>(() => emptyView.ElementAt(0));
            Assert.Throws<InvalidOperationException>(() => _ = view[-1]);
            Assert.Throws<InvalidOperationException>(() => _ = view[2]);
            Assert.Throws<InvalidOperationException>(() => _ = view[int.MaxValue]);
            Assert.Throws<InvalidOperationException>(() => view[2] = 9);
            Assert.Throws<InvalidOperationException>(() => view.ElementAt(2));
            Assert.Throws<InvalidOperationException>(() => view.ElementAt(-1));
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public unsafe void ElementAt_ReferenceAliasesTheOwnerAndTheSpan()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3 });
            var view = list.AsNative();

            ref var element = ref view.ElementAt(2);
            var span = view.AsSpan();

            element = 30;

            Assert.AreEqual(30, list[2]);
            Assert.AreEqual(30, span[2]);

            span[1] = 20;

            Assert.AreEqual(20, view.ElementAt(1));
        }

        [Test]
        public unsafe void Add_ExactlyFillsTheCapacity_StoresEveryItem()
        {
            using var list = new SharedList<int>(3);
            var view = list.AsNative();
            var third = 3;

            view.Add(1);
            view.Add(2);
            view.Add(in third);

            Assert.AreEqual(3, view.Count);
            Assert.AreEqual(3, view.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void FullView_Mutations_ThrowAndKeepTheContent()
        {
            using var list = new SharedList<int>(2);
            using var native = new NativeArray<int>(new[] { 7 }, Allocator.Temp);
            var slice = new NativeSlice<int>(native);
            list.AddRange(new[] { 1, 2 });
            var view = list.AsNative();
            var value = 9;

            Assert.Throws<InvalidOperationException>(() => view.Add(3));
            Assert.Throws<InvalidOperationException>(() => view.Add(in value));
            Assert.Throws<InvalidOperationException>(() => view.Insert(0, 3));
            Assert.Throws<InvalidOperationException>(() => view.Insert(0, in value));
            Assert.Throws<InvalidOperationException>(() => view.AddRange(new[] { 3 }));
            Assert.Throws<InvalidOperationException>(() => view.AddRange(native));
            Assert.Throws<InvalidOperationException>(() => view.AddRange(in slice));
            Assert.Throws<InvalidOperationException>(() => view.AddReplicate(1));
            Assert.Throws<InvalidOperationException>(() => view.AddReplicate(5, 1));
            Assert.Throws<InvalidOperationException>(() => view.AddReplicateNoInit(1));
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public unsafe void Insert_EveryPosition_MatchesListOfT()
        {
            for (var index = 0; index <= 5; index++)
            {
                using var list = new SharedList<int>(7);
                list.AddRange(new[] { 1, 2, 3, 4, 5 });
                var view = list.AsNative();
                var expected = new List<int> { 1, 2, 3, 4, 5 };
                var inValue = 77;

                view.Insert(index, 99);
                expected.Insert(index, 99);

                CollectionAssert.AreEqual(expected, list.ToArray(), $"insert {index}");

                view.Insert(index, in inValue);
                expected.Insert(index, inValue);

                CollectionAssert.AreEqual(expected, list.ToArray(), $"insert in {index}");
            }
        }

        [Test]
        public unsafe void Insert_IntoEmptyList_AddsTheOnlyElement()
        {
            using var list = new SharedList<int>(2);
            var view = list.AsNative();

            view.Insert(0, 7);

            CollectionAssert.AreEqual(new[] { 7 }, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Insert_IndexOutsideTheCount_Throws()
        {
            using var list = new SharedList<int>(4);
            list.AddRange(new[] { 1, 2 });
            var view = list.AsNative();
            var value = 9;

            Assert.Throws<InvalidOperationException>(() => view.Insert(-1, 9));
            Assert.Throws<InvalidOperationException>(() => view.Insert(3, 9));
            Assert.Throws<InvalidOperationException>(() => view.Insert(int.MaxValue, 9));
            Assert.Throws<InvalidOperationException>(() => view.Insert(3, in value));
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public unsafe void AddRange_SpanOverOwnStorage_AppendsACopy()
        {
            using var list = new SharedList<int>(8);
            list.AddRange(new[] { 1, 2, 3, 4 });
            var view = list.AsNative();

            view.AddRange(view.AsReadOnlySpan());

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public unsafe void AddRange_ExactlyFillsTheCapacity_AddsEveryItem()
        {
            using var list = new SharedList<int>(4);
            list.Add(1);
            var view = list.AsNative();

            view.AddRange(new[] { 2, 3, 4 });

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public unsafe void AddRange_EmptyItemsOrZeroCount_ChangesNothing()
        {
            using var list = new SharedList<int>(4);
            using var native = new NativeArray<int>(new[] { 5, 6 }, Allocator.Temp);
            var slice = new NativeSlice<int>(native);
            list.AddRange(new[] { 1, 2 });
            var view = list.AsNative();

            view.AddRange(ReadOnlySpan<int>.Empty);
            view.AddRange(new[] { 9, 9 }, 0);
            view.AddRange(native, 0);
            view.AddRange(in slice, 0);

            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public unsafe void AddRange_CountOutsideTheItems_ThrowsAndKeepsContent()
        {
            using var list = new SharedList<int>(8);
            using var native = new NativeArray<int>(new[] { 5, 6 }, Allocator.Temp);
            var slice = new NativeSlice<int>(native);
            list.AddRange(new[] { 1, 2 });
            var view = list.AsNative();

            Assert.Throws<ArgumentOutOfRangeException>(() => view.AddRange(new[] { 2, 3 }, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => view.AddRange(new[] { 2, 3 }, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => view.AddRange(native, 3));
            Assert.Catch(() => view.AddRange(in slice, 3));
            CollectionAssert.AreEqual(new[] { 1, 2 }, list.ToArray());
        }

        [Test]
        public unsafe void AddRange_NativeSources_AppendTheRequestedItems()
        {
            using var list = new SharedList<int>(8);
            using var native = new NativeArray<int>(new[] { 5, 6, 7 }, Allocator.Temp);
            var slice = new NativeSlice<int>(native, 1, 2);
            var view = list.AsNative();

            view.AddRange(native);
            view.AddRange(native, 1);
            view.AddRange(in slice);
            view.AddRange(in slice, 1);

            CollectionAssert.AreEqual(new[] { 5, 6, 7, 5, 6, 7, 6 }, list.ToArray());
        }

        [Test]
        public unsafe void CopyFrom_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsContent()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });
            var view = list.AsNative();
            var source = new[] { 9, 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyFrom(-1, source));
            Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyFrom(5, source));
            Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyFrom(3, source));
            Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyFrom(0, source, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyFrom(0, source, -1));
            Assert.IsFalse(view.TryCopyFrom(-1, source));
            Assert.IsFalse(view.TryCopyFrom(3, source));
            Assert.IsFalse(view.TryCopyFrom(0, source, 3));
            Assert.IsFalse(view.TryCopyFrom(0, source, -1));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public unsafe void CopyTo_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });
            var view = list.AsNative();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(-1, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(5, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(0, destination, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(0, new int[2], 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(new int[2], 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => view.CopyTo(destination, 1));
            Assert.IsFalse(view.TryCopyTo(-1, destination));
            Assert.IsFalse(view.TryCopyTo(0, destination, 5));
            Assert.IsFalse(view.TryCopyTo(0, new int[2], 3));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public unsafe void CopyTo_NativeDestinationsOutsideTheList_Throw()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });
            using var small = new NativeArray<int>(2, Allocator.Temp);
            using var sliceOwner = new NativeArray<int>(2, Allocator.Temp);
            var slice = new NativeSlice<int>(sliceOwner);
            var view = list.AsNative();

            Assert.Catch(() => view.CopyTo(0, small, 3));
            Assert.Catch(() => view.CopyTo(3, small));
            Assert.Catch(() => view.CopyTo(3, in slice, 2));
            Assert.Catch(() => view.CopyTo(0, in slice, 3));
            CollectionAssert.AreEqual(new[] { 0, 0 }, small.ToArray());
            CollectionAssert.AreEqual(new[] { 0, 0 }, sliceOwner.ToArray());
        }

        [Test]
        public unsafe void CopyFromAndCopyTo_EmptyListAndEmptySpans_CopyNothing()
        {
            using var list = new SharedList<int>(4);
            var view = list.AsNative();

            view.CopyTo(Span<int>.Empty);
            view.CopyFrom(ReadOnlySpan<int>.Empty);

            Assert.IsTrue(view.TryCopyTo(Span<int>.Empty));
            Assert.IsTrue(view.TryCopyFrom(ReadOnlySpan<int>.Empty));
            Assert.IsFalse(view.TryCopyTo(new int[1]));
            Assert.IsFalse(view.TryCopyFrom(new[] { 1 }));
        }

        [Test]
        public unsafe void CopyFrom_SourceOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var list = new SharedList<int>((int[])seed.Clone());
            var view = list.AsNative();

            view.CopyFrom(1, view.AsReadOnlySpan()[..4]);

            var expectedForward = (int[])seed.Clone();
            Array.Copy(expectedForward, 0, expectedForward, 1, 4);
            CollectionAssert.AreEqual(expectedForward, list.ToArray());

            view.CopyFrom(seed);
            view.CopyFrom(0, view.AsReadOnlySpan()[1..5]);

            var expectedBackward = (int[])seed.Clone();
            Array.Copy(expectedBackward, 1, expectedBackward, 0, 4);
            CollectionAssert.AreEqual(expectedBackward, list.ToArray());
        }

        [Test]
        public unsafe void CopyTo_DestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var list = new SharedList<int>((int[])seed.Clone());
            var view = list.AsNative();

            view.CopyTo(2, view.AsSpan()[..4]);

            var expectedBackward = (int[])seed.Clone();
            Array.Copy(expectedBackward, 2, expectedBackward, 0, 4);
            CollectionAssert.AreEqual(expectedBackward, list.ToArray());

            view.CopyFrom(seed);
            view.CopyTo(0, view.AsSpan()[2..]);

            var expectedForward = (int[])seed.Clone();
            Array.Copy(expectedForward, 0, expectedForward, 2, 4);
            CollectionAssert.AreEqual(expectedForward, list.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void RemoveAtAndRemoveAtSwapBack_OutOfRange_Throw()
        {
            using var empty = new SharedList<int>(4);
            using var list = new SharedList<int>(new[] { 1, 2, 3 });
            var emptyView = empty.AsNative();
            var view = list.AsNative();

            Assert.Throws<InvalidOperationException>(() => emptyView.RemoveAt(0));
            Assert.Throws<InvalidOperationException>(() => emptyView.RemoveAtSwapBack(0));
            Assert.Throws<InvalidOperationException>(() => view.RemoveAt(-1));
            Assert.Throws<InvalidOperationException>(() => view.RemoveAt(3));
            Assert.Throws<InvalidOperationException>(() => view.RemoveAtSwapBack(-1));
            Assert.Throws<InvalidOperationException>(() => view.RemoveAtSwapBack(3));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.ToArray());
        }

        [Test]
        public unsafe void RemoveAtAndRemoveAtSwapBack_EveryIndex_MatchListOfT()
        {
            const int COUNT = 5;

            for (var index = 0; index < COUNT; index++)
            {
                using var removeList = new SharedList<int>(new[] { 1, 2, 3, 4, 5 });
                using var swapList = new SharedList<int>(new[] { 1, 2, 3, 4, 5 });
                var removeView = removeList.AsNative();
                var swapView = swapList.AsNative();
                var expectedRemove = new List<int> { 1, 2, 3, 4, 5 };
                var expectedSwap = new List<int> { 1, 2, 3, 4, 5 };

                removeView.RemoveAt(index);
                expectedRemove.RemoveAt(index);
                expectedSwap[index] = expectedSwap[COUNT - 1];
                expectedSwap.RemoveAt(COUNT - 1);
                swapView.RemoveAtSwapBack(index);

                CollectionAssert.AreEqual(expectedRemove, removeList.ToArray(), $"remove {index}");
                CollectionAssert.AreEqual(expectedSwap, swapList.ToArray(), $"swap back {index}");
            }
        }

        [Test]
        public unsafe void RemoveRange_EveryValidRange_MatchesListOfT()
        {
            const int COUNT = 6;

            for (var start = 0; start < COUNT; start++)
            {
                for (var length = 0; length <= COUNT - start; length++)
                {
                    using var list = new SharedList<int>(new[] { 1, 2, 3, 4, 5, 6 });
                    var view = list.AsNative();
                    var expected = new List<int> { 1, 2, 3, 4, 5, 6 };

                    view.RemoveRange(start, length);
                    expected.RemoveRange(start, length);

                    CollectionAssert.AreEqual(expected, list.ToArray(), $"start {start} length {length}");
                }
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void RemoveRange_RangeOutsideTheList_Throws()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });
            var view = list.AsNative();

            Assert.Throws<InvalidOperationException>(() => view.RemoveRange(-1, 1));
            Assert.Throws<InvalidOperationException>(() => view.RemoveRange(4, 0));
            Assert.Throws<InvalidOperationException>(() => view.RemoveRange(2, 3));
            Assert.Throws<InvalidOperationException>(() => view.RemoveRange(0, int.MaxValue));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());
        }

        [Test]
        public unsafe void SpansSliceAndArray_EmptyList_AreEmpty()
        {
            using var list = new SharedList<int>(4);
            var view = list.AsNative();

            Assert.AreEqual(0, view.AsSpan().Length);
            Assert.AreEqual(0, view.AsReadOnlySpan().Length);
            Assert.AreEqual(0, view.AsNativeSlice().Length);
            Assert.AreEqual(0, view.ToArray().Length);
        }

        [Test]
        public unsafe void AsNativeSlice_CountBelowCapacity_LengthIsCountAndAliasesTheOwner()
        {
            using var list = new SharedList<int>(8);
            list.AddRange(new[] { 1, 2, 3 });
            var view = list.AsNative();

            var slice = view.AsNativeSlice();

            Assert.AreEqual(3, slice.Length);

            slice[1] = 20;
            list[2] = 30;

            Assert.AreEqual(20, list[1]);
            Assert.AreEqual(30, slice[2]);
            CollectionAssert.AreEqual(new[] { 1, 20, 30 }, slice.ToArray());
        }

        [Test]
        public unsafe void AddReplicate_FillsTheRequestedRangeAndAliasesTheOwner()
        {
            using var list = new SharedList<int>(6);
            list.Add(1);
            var view = list.AsNative();

            var zeroed = view.AddReplicate(2);
            var filled = view.AddReplicate(7, 2);
            var noInit = view.AddReplicateNoInit(1);
            noInit[0] = 8;
            zeroed[0] = 5;

            Assert.AreEqual(2, zeroed.Length);
            Assert.AreEqual(2, filled.Length);
            CollectionAssert.AreEqual(new[] { 1, 5, 0, 7, 7, 8 }, list.ToArray());
            Assert.AreEqual(8, list[5]);
        }

        [Test]
        public unsafe void AddReplicate_NegativeAmount_ThrowsAndKeepsTheCount()
        {
            using var list = new SharedList<int>(4);
            list.Add(1);
            var view = list.AsNative();

            Assert.Catch(() => view.AddReplicate(-1));
            Assert.Catch(() => view.AddReplicate(7, -1));
            Assert.Catch(() => view.AddReplicateNoInit(-1));
            Assert.AreEqual(1, list.Count);
        }

        [Test]
        public unsafe void Reinterpret_SameSizeView_WritesThroughToTheOwnerAndEmptyListIsEmpty()
        {
            using var empty = new SharedList<int>(4);
            using var floats = new SharedList<int>(new[] { 0, 0, 0 });
            using var wides = new SharedList<Wide>(new Wide[2]);
            var emptyView = empty.AsNative().Reinterpret<uint>();
            var floatView = floats.AsNative().Reinterpret<float>();
            var wideView = wides.AsNative().Reinterpret<WideOther>();

            floatView[1] = 1.5f;
            wides[1] = new Wide { a = 1, b = -2, c = long.MaxValue };

            Assert.IsTrue(emptyView.IsCreated);
            Assert.AreEqual(0, emptyView.Count);
            Assert.AreEqual(BitConverter.SingleToInt32Bits(1.5f), floats[1]);
            Assert.AreEqual(1L, wideView[1].x);
            Assert.AreEqual(-2L, wideView[1].y);
            Assert.AreEqual(long.MaxValue, wideView[1].z);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Reinterpret_DifferentSize_Throws()
        {
            using var list = new SharedList<int>(new[] { 1, 2 });
            var view = list.AsNative();

            Assert.Throws<InvalidOperationException>(() => view.Reinterpret<byte>());
            Assert.Throws<InvalidOperationException>(() => view.Reinterpret<long>());
            Assert.Throws<InvalidOperationException>(() => view.Reinterpret<Rgb>());
        }

        [Test]
        public unsafe void Elements_ByteElement_RoundTripBitExact()
        {
            AssertElementRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public unsafe void Elements_ThreeByteStruct_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Rgb { r = 1, g = 2, b = 3 }
                , new Rgb { r = 0, g = 0, b = 0 }
                , new Rgb { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public unsafe void Elements_LongElement_RoundTripBitExact()
        {
            AssertElementRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public unsafe void Elements_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = 0, b = 0, c = 0 }
                , new Wide { a = long.MinValue, b = 7, c = -1 }
            );
        }

        [Test]
        public unsafe void GetEnumerator_EmptyList_YieldsNothingForEveryEnumeratorForm()
        {
            using var list = new SharedList<int>(4);
            var view = list.AsNative();

            var structEnumerator = view.GetEnumerator();
            var genericEnumerator = ((IEnumerable<int>)view).GetEnumerator();
            var nonGenericEnumerator = ((IEnumerable)view).GetEnumerator();

            Assert.IsFalse(structEnumerator.MoveNext());
            Assert.IsFalse(genericEnumerator.MoveNext());
            Assert.IsFalse(nonGenericEnumerator.MoveNext());
        }

        [Test]
        public unsafe void Enumerator_ModifiedList_MoveNextAndResetThrow()
        {
            using var list = new SharedList<int>(8);
            list.AddRange(new[] { 1, 2, 3 });
            var view = list.AsNative();
            var enumerator = view.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());

            view.Add(4);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
        }

        [Test]
        public unsafe void Enumerator_FromDefaultReadOnly_Throws()
        {
            SharedListNative<int>.ReadOnly notCreated = default;

            Assert.Throws<ArgumentException>(() => _ = new SharedListNative<int>.Enumerator(notCreated));
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void Members_AfterOwnerDispose_Throw()
        {
            var list = new SharedList<int>(new[] { 1, 2, 3 });
            var native = new NativeArray<int>(new[] { 4, 5 }, Allocator.Persistent);

            try
            {
                var view = list.AsNative();
                var enumerator = view.GetEnumerator();
                var slice = new NativeSlice<int>(native, 0, 2);
                var value = 9;

                list.Dispose();

                Assert.Catch(() => _ = view.Count);
                Assert.Catch(() => _ = view.Capacity);
                Assert.Catch(() => _ = view[0]);
                Assert.Catch(() => view[0] = 1);
                Assert.Catch(() => view.ElementAt(0));
                Assert.Catch(() => view.Add(1));
                Assert.Catch(() => view.Add(in value));
                Assert.Catch(() => view.Insert(0, 1));
                Assert.Catch(() => view.Insert(0, in value));
                Assert.Catch(() => view.AddRange(new[] { 1 }));
                Assert.Catch(() => view.AddRange(new[] { 1 }, 1));
                Assert.Catch(() => view.AddRange(native));
                Assert.Catch(() => view.AddRange(native, 1));
                Assert.Catch(() => view.AddRange(in slice));
                Assert.Catch(() => view.AddRange(in slice, 1));
                Assert.Catch(() => view.CopyFrom(new[] { 1 }));
                Assert.Catch(() => view.CopyFrom(0, new[] { 1 }, 1));
                Assert.Catch(() => view.TryCopyFrom(new[] { 1 }));
                Assert.Catch(() => view.TryCopyFrom(0, new[] { 1 }, 1));
                Assert.Catch(() => view.CopyTo(new int[3], 0));
                Assert.Catch(() => view.CopyTo(new int[3]));
                Assert.Catch(() => view.CopyTo(0, new int[3], 1));
                Assert.Catch(() => view.TryCopyTo(new int[3]));
                Assert.Catch(() => view.TryCopyTo(0, new int[3], 1));
                Assert.Catch(() => view.CopyTo(native));
                Assert.Catch(() => view.CopyTo(0, native, 1));
                Assert.Catch(() => view.CopyTo(0, in slice));
                Assert.Catch(() => view.CopyTo(0, in slice, 1));
                Assert.Catch(() => view.Clear());
                Assert.Catch(() => view.RemoveAt(0));
                Assert.Catch(() => view.RemoveRange(0, 1));
                Assert.Catch(() => view.RemoveAtSwapBack(0));
                Assert.Catch(() => view.ToArray());
                Assert.Catch(() => view.AsSpan());
                Assert.Catch(() => view.AsReadOnlySpan());
                Assert.Catch(() => view.AsNativeSlice());
                Assert.Catch(() => view.AddReplicate(1));
                Assert.Catch(() => view.AddReplicate(1, 1));
                Assert.Catch(() => view.AddReplicateNoInit(1));
                Assert.Catch(() => view.Reinterpret<uint>());
                Assert.Catch(() => view.GetEnumerator());
                Assert.Catch(() => view.MarkChanged());
                Assert.Catch(() => enumerator.MoveNext());
            }
            finally
            {
                native.Dispose();
            }
        }

        [Test]
        public unsafe void ReadOnly_EmptyList_ReturnsEmptyResults()
        {
            using var list = new SharedList<int>(4);
            var readOnly = list.AsNative().AsReadOnly();
            var enumerator = readOnly.GetEnumerator();

            Assert.IsTrue(readOnly.IsCreated);
            Assert.IsFalse(enumerator.MoveNext());
            Assert.AreEqual(0, readOnly.Count);
            Assert.AreEqual(4, readOnly.Capacity);
            Assert.AreEqual(0, readOnly.AsReadOnlySpan().Length);
            Assert.AreEqual(0, readOnly.ToArray().Length);
            Assert.IsTrue(readOnly.TryCopyTo(Span<int>.Empty));
            Assert.IsFalse(readOnly.TryCopyTo(new int[1]));
        }

        [Test]
        public unsafe void ReadOnly_ViewsOwnerWritesAndVersion()
        {
            using var list = new SharedList<int>(4);
            list.Add(1);
            var view = list.AsNative();
            var readOnly = view.AsReadOnly();
            var version = readOnly.Version;

            view.Add(2);

            Assert.AreEqual(2, readOnly.Count);
            Assert.AreEqual(2, readOnly[1]);
            Assert.AreNotEqual(version, readOnly.Version);

            ReadOnlySpan<int> span = readOnly;

            CollectionAssert.AreEqual(new[] { 1, 2 }, span.ToArray());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ReadOnly_IndexerOutOfRange_Throws()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3 });
            var readOnly = list.AsNative().AsReadOnly();

            Assert.Throws<InvalidOperationException>(() => _ = readOnly[-1]);
            Assert.Throws<InvalidOperationException>(() => _ = readOnly[3]);
            Assert.Throws<InvalidOperationException>(() => _ = readOnly[int.MaxValue]);
        }

        [Test]
        public unsafe void ReadOnly_CopyToInvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            using var list = new SharedList<int>(new[] { 1, 2, 3, 4 });
            var readOnly = list.AsNative().AsReadOnly();
            var destination = new[] { 9, 9, 9, 9 };

            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(-1, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(5, destination));
            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(0, destination, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => readOnly.CopyTo(new int[2], 0));
            Assert.IsFalse(readOnly.TryCopyTo(-1, destination));
            Assert.IsFalse(readOnly.TryCopyTo(0, destination, 5));
            CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
        }

        [Test]
        public unsafe void ReadOnly_CopyToDestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var list = new SharedList<int>((int[])seed.Clone());
            var view = list.AsNative();
            var readOnly = view.AsReadOnly();

            readOnly.CopyTo(2, view.AsSpan()[..4]);

            var expected = (int[])seed.Clone();
            Array.Copy(expected, 2, expected, 0, 4);
            CollectionAssert.AreEqual(expected, list.ToArray());
        }

        [Test]
        public unsafe void ReadOnly_ElementShapes_ReinterpretAndRoundTripBitExact()
        {
            using var small = new SharedList<Rgb>(new[] { new Rgb { r = 1, g = 2, b = 3 } });
            using var wide = new SharedList<Wide>(
                new[] { new Wide { a = long.MinValue, b = 0, c = long.MaxValue } }
            );
            using var ints = new SharedList<int>(new[] { -1, 0x3F800000 });
            var smallView = small.AsNative().AsReadOnly();
            var wideView = wide.AsNative().AsReadOnly();
            var floatView = ints.AsNative().AsReadOnly().Reinterpret<float>();

            Assert.AreEqual(3, smallView[0].b);
            Assert.AreEqual(2, smallView.AsReadOnlySpan()[0].g);
            Assert.AreEqual(long.MinValue, wideView[0].a);
            Assert.AreEqual(long.MaxValue, wideView.ToArray()[0].c);
            Assert.AreEqual(1f, floatView[1]);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ReadOnly_Reinterpret_DifferentSize_Throws()
        {
            using var list = new SharedList<int>(new[] { 1, 2 });
            var readOnly = list.AsNative().AsReadOnly();

            Assert.Throws<InvalidOperationException>(() => readOnly.Reinterpret<byte>());
            Assert.Throws<InvalidOperationException>(() => readOnly.Reinterpret<long>());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public unsafe void ReadOnly_Members_AfterOwnerDispose_Throw()
        {
            var list = new SharedList<int>(new[] { 1, 2, 3 });
            var readOnly = list.AsNative().AsReadOnly();
            var enumerator = readOnly.GetEnumerator();

            list.Dispose();

            Assert.Catch(() => _ = readOnly.Count);
            Assert.Catch(() => _ = readOnly.Capacity);
            Assert.Catch(() => _ = readOnly.Version);
            Assert.Catch(() => _ = readOnly[0]);
            Assert.Catch(() => readOnly.AsReadOnlySpan());
            Assert.Catch(() => readOnly.ToArray());
            Assert.Catch(() => readOnly.CopyTo(new int[3]));
            Assert.Catch(() => readOnly.CopyTo(new int[3], 0));
            Assert.Catch(() => readOnly.TryCopyTo(new int[3]));
            Assert.Catch(() => readOnly.Reinterpret<uint>());
            Assert.Catch(() => readOnly.GetEnumerator());
            Assert.Catch(() => enumerator.MoveNext());
        }

        private static unsafe void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            using var list = new SharedList<T>(8);
            var view = list.AsNative();

            view.Add(a);
            view.Add(in b);
            view.AddRange(new[] { c });
            view.Insert(1, c);

            CollectionAssert.AreEqual(new[] { a, c, b, c }, list.ToArray(), name);

            view.RemoveAt(1);

            Assert.AreEqual(a, view[0], name);
            Assert.AreEqual(b, view.ElementAt(1), name);
            Assert.AreEqual(c, view[2], name);

            var copy = new T[3];
            view.CopyTo(copy, 0);

            CollectionAssert.AreEqual(new[] { a, b, c }, copy, name);

            view.CopyFrom(new[] { c, b, a });

            CollectionAssert.AreEqual(new[] { c, b, a }, view.AsReadOnlySpan().ToArray(), name);

            var replicated = view.AddReplicate(a, 2);

            Assert.AreEqual(a, replicated[0], name);
            Assert.AreEqual(a, replicated[1], name);
            Assert.AreEqual(5, view.Count, name);
            Assert.AreEqual(a, view.AsReadOnly()[4], name);
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

        private struct WideOther
        {
            public long x;
            public long y;
            public long z;
        }
    }
}
