using System;
using System.Collections.Generic;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Core.Collections.Unsafe
{
    public partial class SharedListUnsafeTests
    {
        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Alloc_AcceptedAllocator_CreatesAndFreesTheHeader(Allocator allocator)
        {
            using var storage = new SharedListStorage<int>(4);

            // SAFETY: The header borrows storage, which outlives it; Free runs exactly once in finally.
            unsafe
            {
                var data = SharedListUnsafe<int>.Alloc(
                      storage.buffer
                    , storage.capacity
                    , storage.count
                    , storage.version
                    , allocator
                );

                try
                {
                    data->Add(7);

                    Assert.AreEqual(4, data->Capacity);
                    Assert.AreEqual(1, data->Count);
                    Assert.AreEqual(1, storage.Count);
                    Assert.AreEqual(7, (*data)[0]);
                }
                finally
                {
                    SharedListUnsafe<int>.Free(data, allocator);
                }
            }
        }

        [Test]
        public void Alloc_ZeroCapacity_CreatesAHeaderOfAnEmptyFixedList()
        {
            using var storage = new SharedListStorage<int>(0);

            // SAFETY: The header borrows storage, which outlives it; Free runs exactly once in finally.
            unsafe
            {
                var data = SharedListUnsafe<int>.Alloc(
                      storage.buffer
                    , storage.capacity
                    , storage.count
                    , storage.version
                    , Allocator.Persistent
                );

                try
                {
                    Assert.AreEqual(0, data->Capacity);
                    Assert.AreEqual(0, data->Count);
                    Assert.AreEqual(0, data->ToArray().Length);
                    Assert.AreEqual(0, data->AsReadOnlySpan().Length);
                    Assert.IsTrue(data->TryCopyTo(Span<int>.Empty));
                }
                finally
                {
                    SharedListUnsafe<int>.Free(data, Allocator.Persistent);
                }
            }
        }

        [Test]
        public void Free_NullPointer_DoesNothingForEveryStrategy()
        {
            // SAFETY: Free documents that a null header is ignored, so no memory is read or released.
            unsafe
            {
                SharedListUnsafe<int>.Free(null, default(AllocatorStrategy));
                SharedListUnsafe<int>.Free(null, Allocator.Temp);
                SharedListUnsafe<int>.Free(null, Allocator.Persistent);
            }
        }

        [Test]
        public void VersionClearAndIncrementVersion_EmptyAndFilledList_TrackTheSharedState()
        {
            using var storage = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                Assert.AreEqual(0, header.Version);
                Assert.AreEqual(0, header.ToArray().Length);

                header.IncrementVersion();

                Assert.AreEqual(1, header.Version);
                Assert.AreEqual(1, storage.Version);

                header.Add(1);
                header.Add(2);

                var version = header.Version;

                header.Clear();

                Assert.AreEqual(0, header.Count);
                Assert.AreEqual(0, storage.Count);
                Assert.AreEqual(version + 1, header.Version);

                header.Clear();

                Assert.AreEqual(0, header.Count);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Indexer_IndexOutsideTheCount_Throws()
        {
            using var storage = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                Assert.Throws<InvalidOperationException>(() => _ = header[0]);
                Assert.Throws<InvalidOperationException>(() => header[0] = 1);

                header.Add(1);
                header.Add(2);

                Assert.Throws<InvalidOperationException>(() => _ = header[-1]);
                Assert.Throws<InvalidOperationException>(() => _ = header[2]);
                Assert.Throws<InvalidOperationException>(() => _ = header[int.MaxValue]);
                Assert.Throws<InvalidOperationException>(() => header[2] = 9);
                Assert.Throws<InvalidOperationException>(() => header.ElementAt(2));
                Assert.Throws<InvalidOperationException>(() => header.ElementAt(-1));
                CollectionAssert.AreEqual(new[] { 1, 2 }, header.ToArray());
            }
        }

        [Test]
        public void IndexerSetterAndElementAt_WriteThroughTheSharedBuffer()
        {
            using var storage = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1);
                header.Add(2);

                var version = header.Version;

                header[0] = 10;

                Assert.AreEqual(version + 1, header.Version);

                ref var second = ref header.ElementAt(1);
                second = 20;

                Assert.AreEqual(10, storage.buffer[0]);
                Assert.AreEqual(20, storage.buffer[1]);
                Assert.AreEqual(20, header[1]);

                storage.buffer[1] = 30;

                Assert.AreEqual(30, second);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Add_ZeroCapacityOrFullList_ThrowsAndKeepsContent()
        {
            using var zero = new SharedListStorage<int>(0);
            using var storage = new SharedListStorage<int>(2);
            var zeroHeader = zero.CreateHeader();
            var header = storage.CreateHeader();

            // SAFETY: both storages own the blocks their headers borrow and outlive every call below.
            unsafe
            {
                Assert.Throws<InvalidOperationException>(() => zeroHeader.Add(1));

                header.Add(1);
                header.Add(in storage.buffer[0]);

                Assert.Throws<InvalidOperationException>(() => header.Add(3));
                Assert.Throws<InvalidOperationException>(() =>
                {
                    var item = 4;
                    header.Add(in item);
                });
                Assert.AreEqual(0, zero.Count);
                Assert.AreEqual(2, storage.Count);
                CollectionAssert.AreEqual(new[] { 1, 1 }, header.ToArray());
            }
        }

        [Test]
        public void Insert_EveryPosition_MatchesListOfT()
        {
            const int COUNT = 5;
            using var storage = new SharedListStorage<int>(COUNT + 1);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                for (var index = 0; index <= COUNT; index++)
                {
                    header.Clear();
                    var expected = new List<int>();

                    for (var i = 1; i <= COUNT; i++)
                    {
                        header.Add(i);
                        expected.Add(i);
                    }

                    header.Insert(index, 99);
                    expected.Insert(index, 99);

                    CollectionAssert.AreEqual(expected, header.ToArray(), $"insert {index}");
                }
            }
        }

        [Test]
        public void Insert_InArgumentOverload_InsertsTheValue()
        {
            using var storage = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                var item = 9;

                header.Add(1);
                header.Add(2);
                header.Insert(1, in item);
                header.Insert(0, in storage.buffer[2]);

                CollectionAssert.AreEqual(new[] { 2, 1, 9, 2 }, header.ToArray());
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Insert_IndexOutsideTheCountOrFullList_Throws()
        {
            using var storage = new SharedListStorage<int>(2);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1);

                Assert.Throws<InvalidOperationException>(() => header.Insert(-1, 5));
                Assert.Throws<InvalidOperationException>(() => header.Insert(2, 5));
                Assert.Throws<InvalidOperationException>(() => header.Insert(int.MaxValue, 5));

                header.Add(2);

                Assert.Throws<InvalidOperationException>(() => header.Insert(0, 5));
                Assert.Throws<InvalidOperationException>(() => header.Insert(2, 5));
                CollectionAssert.AreEqual(new[] { 1, 2 }, header.ToArray());
            }
        }

        [Test]
        public void AddRange_SpanOverOwnStorage_AppendsACopy()
        {
            using var storage = new SharedListStorage<int>(6);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.AddRange(new[] { 1, 2, 3 });
                header.AddRange(header.AsReadOnlySpan());

                CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 2, 3 }, header.ToArray());
            }
        }

        [Test]
        public void AddRange_ExactlyFillsTheCapacity_AddsEveryItem()
        {
            using var storage = new SharedListStorage<int>(3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.AddRange(new[] { 1, 2 });
                header.AddRange(new[] { 3, 99 }, 1);

                Assert.AreEqual(3, header.Count);
                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, header.ToArray());
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AddRange_MoreThanTheFreeCapacity_ThrowsAndKeepsContent()
        {
            using var storage = new SharedListStorage<int>(3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.AddRange(new[] { 1, 2 });

                Assert.Throws<InvalidOperationException>(() => header.AddRange(new[] { 3, 4 }));
                Assert.Throws<InvalidOperationException>(() => header.AddRange(new[] { 3, 4, 5 }, 2));
                CollectionAssert.AreEqual(new[] { 1, 2 }, header.ToArray());
            }
        }

        [Test]
        public void AddRange_EmptyItemsOrZeroCount_ChangesNothing()
        {
            using var storage = new SharedListStorage<int>(3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1);
                header.AddRange(ReadOnlySpan<int>.Empty);
                header.AddRange(new[] { 9, 9 }, 0);

                CollectionAssert.AreEqual(new[] { 1 }, header.ToArray());
            }
        }

        [Test]
        public void AddRange_CountOutsideTheItems_ThrowsArgumentOutOfRangeAndKeepsContent()
        {
            using var storage = new SharedListStorage<int>(6);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1);

                Assert.Throws<ArgumentOutOfRangeException>(() => header.AddRange(new[] { 2, 3 }, 3));
                Assert.Throws<ArgumentOutOfRangeException>(() => header.AddRange(new[] { 2, 3 }, -1));
                Assert.AreEqual(1, header.Count);
                CollectionAssert.AreEqual(new[] { 1 }, header.ToArray());
            }
        }

        [Test]
        public void AddRange_NativeSlice_AppendsTheRequestedItems()
        {
            using var storage = new SharedListStorage<int>(6);
            var header = storage.CreateHeader();
            using var array = new NativeArray<int>(new[] { 4, 5, 6 }, Allocator.Persistent);
            var slice = new NativeSlice<int>(array);

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.AddRange(in slice, 2);
                header.AddRange(in slice, 0);
                header.AddRange(in slice);

                CollectionAssert.AreEqual(new[] { 4, 5, 4, 5, 6 }, header.ToArray());
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AddRange_NativeSliceMoreThanTheFreeCapacity_ThrowsAndKeepsContent()
        {
            using var storage = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();
            using var array = new NativeArray<int>(new[] { 4, 5, 6 }, Allocator.Persistent);
            var slice = new NativeSlice<int>(array);

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1);
                header.Add(2);

                Assert.Throws<InvalidOperationException>(() => header.AddRange(in slice));
                Assert.Throws<InvalidOperationException>(() => header.AddRange(in slice, 3));
                CollectionAssert.AreEqual(new[] { 1, 2 }, header.ToArray());
            }
        }

        [Test]
        public void CopyFrom_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsContent()
        {
            using var storage = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();
            var source = new[] { 9, 9 };

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.AddRange(new[] { 1, 2, 3, 4 });

                Assert.Throws<ArgumentOutOfRangeException>(() => header.CopyFrom(-1, source));
                Assert.Throws<ArgumentOutOfRangeException>(() => header.CopyFrom(5, source));
                Assert.Throws<ArgumentOutOfRangeException>(() => header.CopyFrom(3, source));
                Assert.Throws<ArgumentOutOfRangeException>(() => header.CopyFrom(0, source, 3));
                Assert.Throws<ArgumentOutOfRangeException>(() => header.CopyFrom(0, source, -1));
                Assert.IsFalse(header.TryCopyFrom(-1, source));
                Assert.IsFalse(header.TryCopyFrom(3, source));
                Assert.IsFalse(header.TryCopyFrom(0, source, 3));
                Assert.IsFalse(header.TryCopyFrom(0, source, -1));
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, header.ToArray());
            }
        }

        [Test]
        public void CopyTo_InvalidRange_ThrowsArgumentOutOfRangeAndKeepsDestination()
        {
            using var storage = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();
            var destination = new[] { 9, 9, 9, 9 };

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.AddRange(new[] { 1, 2, 3, 4 });

                Assert.Throws<ArgumentOutOfRangeException>(() => header.CopyTo(-1, destination));
                Assert.Throws<ArgumentOutOfRangeException>(() => header.CopyTo(5, destination));
                Assert.Throws<ArgumentOutOfRangeException>(() => header.CopyTo(0, destination, 5));
                Assert.Throws<ArgumentOutOfRangeException>(() => header.CopyTo(0, new int[2], 3));
                Assert.Throws<ArgumentOutOfRangeException>(() => header.CopyTo(0, destination, -1));
                Assert.IsFalse(header.TryCopyTo(-1, destination));
                Assert.IsFalse(header.TryCopyTo(0, destination, 5));
                Assert.IsFalse(header.TryCopyTo(0, new int[2], 3));
                Assert.IsFalse(header.TryCopyTo(0, destination, -1));
                CollectionAssert.AreEqual(new[] { 9, 9, 9, 9 }, destination);
            }
        }

        [Test]
        public void CopyFromAndCopyTo_EmptyListAndEmptySpans_CopyNothing()
        {
            using var storage = new SharedListStorage<int>(3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.CopyFrom(ReadOnlySpan<int>.Empty);
                header.CopyTo(Span<int>.Empty);

                Assert.IsTrue(header.TryCopyFrom(ReadOnlySpan<int>.Empty));
                Assert.IsTrue(header.TryCopyTo(Span<int>.Empty));
                Assert.IsFalse(header.TryCopyFrom(new[] { 1 }));
                Assert.IsFalse(header.TryCopyTo(new int[1]));
            }
        }

        [Test]
        public void CopyFrom_SourceOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var storage = new SharedListStorage<int>(6);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.AddRange(seed);
                header.CopyFrom(1, header.AsReadOnlySpan()[..4]);

                var expectedForward = (int[])seed.Clone();
                Array.Copy(expectedForward, 0, expectedForward, 1, 4);
                CollectionAssert.AreEqual(expectedForward, header.ToArray());

                header.CopyFrom(seed);
                header.CopyFrom(0, header.AsReadOnlySpan()[1..5]);

                var expectedBackward = (int[])seed.Clone();
                Array.Copy(expectedBackward, 1, expectedBackward, 0, 4);
                CollectionAssert.AreEqual(expectedBackward, header.ToArray());
            }
        }

        [Test]
        public void CopyTo_DestinationOverlapsOwnStorage_MatchesArrayCopy()
        {
            var seed = new[] { 1, 2, 3, 4, 5, 6 };
            using var storage = new SharedListStorage<int>(6);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.AddRange(seed);
                header.CopyTo(2, header.AsSpan()[..4]);

                var expectedBackward = (int[])seed.Clone();
                Array.Copy(expectedBackward, 2, expectedBackward, 0, 4);
                CollectionAssert.AreEqual(expectedBackward, header.ToArray());

                header.CopyFrom(seed);
                header.CopyTo(0, header.AsSpan()[2..]);

                var expectedForward = (int[])seed.Clone();
                Array.Copy(expectedForward, 0, expectedForward, 2, 4);
                CollectionAssert.AreEqual(expectedForward, header.ToArray());
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void RemoveAtAndRemoveAtSwapBack_EmptyListOrIndexOutsideTheCount_Throw()
        {
            using var storage = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                Assert.Throws<InvalidOperationException>(() => header.RemoveAt(0));
                Assert.Throws<InvalidOperationException>(() => header.RemoveAtSwapBack(0));

                header.Add(1);
                header.Add(2);

                Assert.Throws<InvalidOperationException>(() => header.RemoveAt(-1));
                Assert.Throws<InvalidOperationException>(() => header.RemoveAt(2));
                Assert.Throws<InvalidOperationException>(() => header.RemoveAtSwapBack(-1));
                Assert.Throws<InvalidOperationException>(() => header.RemoveAtSwapBack(2));
                CollectionAssert.AreEqual(new[] { 1, 2 }, header.ToArray());
            }
        }

        [Test]
        public void RemoveAt_EveryPosition_MatchesListOfT()
        {
            const int COUNT = 5;
            using var storage = new SharedListStorage<int>(COUNT);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                for (var index = 0; index < COUNT; index++)
                {
                    header.Clear();
                    var expected = new List<int>();

                    for (var i = 1; i <= COUNT; i++)
                    {
                        header.Add(i);
                        expected.Add(i);
                    }

                    header.RemoveAt(index);
                    expected.RemoveAt(index);

                    CollectionAssert.AreEqual(expected, header.ToArray(), $"remove {index}");
                }
            }
        }

        [Test]
        public void RemoveAtSwapBack_EveryPosition_MovesTheLastElementIntoTheGap()
        {
            const int COUNT = 5;
            using var storage = new SharedListStorage<int>(COUNT);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                for (var index = 0; index < COUNT; index++)
                {
                    header.Clear();
                    var expected = new List<int>();

                    for (var i = 1; i <= COUNT; i++)
                    {
                        header.Add(i);
                        expected.Add(i);
                    }

                    expected[index] = expected[COUNT - 1];
                    expected.RemoveAt(COUNT - 1);
                    header.RemoveAtSwapBack(index);

                    CollectionAssert.AreEqual(expected, header.ToArray(), $"index {index}");
                }
            }
        }

        [Test]
        public void RemoveRange_EveryValidRange_MatchesListOfT()
        {
            const int COUNT = 6;
            using var storage = new SharedListStorage<int>(COUNT);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                for (var start = 0; start < COUNT; start++)
                {
                    for (var length = 0; length <= COUNT - start; length++)
                    {
                        header.Clear();
                        var expected = new List<int>();

                        for (var i = 1; i <= COUNT; i++)
                        {
                            header.Add(i);
                            expected.Add(i);
                        }

                        header.RemoveRange(start, length);
                        expected.RemoveRange(start, length);

                        CollectionAssert.AreEqual(expected, header.ToArray(), $"start {start} length {length}");
                    }
                }
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void RemoveRange_InvalidRange_ThrowsAndKeepsContent()
        {
            using var storage = new SharedListStorage<int>(4);
            using var empty = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();
            var emptyHeader = empty.CreateHeader();

            // SAFETY: both storages own the blocks their headers borrow and outlive every call below.
            unsafe
            {
                header.AddRange(new[] { 1, 2, 3, 4 });

                Assert.Throws<InvalidOperationException>(() => emptyHeader.RemoveRange(0, 0));
                Assert.Throws<InvalidOperationException>(() => header.RemoveRange(-1, 1));
                Assert.Throws<InvalidOperationException>(() => header.RemoveRange(4, 0));
                Assert.Throws<InvalidOperationException>(() => header.RemoveRange(2, 3));
                Assert.Throws<InvalidOperationException>(() => header.RemoveRange(0, int.MaxValue));
                Assert.Throws<InvalidOperationException>(() => header.RemoveRange(1, int.MaxValue));
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, header.ToArray());
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void RemoveRange_NegativeLength_Throws()
        {
            using var storage = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.AddRange(new[] { 1, 2, 3, 4 });

                Assert.Catch(() => header.RemoveRange(2, -1));
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, header.ToArray());
            }
        }

        [Test]
        public void AsSpanAndAsReadOnlySpan_EmptyAndFilledList_AliasTheBufferUpToTheCount()
        {
            using var storage = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                Assert.AreEqual(0, header.AsSpan().Length);
                Assert.AreEqual(0, header.AsReadOnlySpan().Length);

                header.Add(1);
                header.Add(2);

                var writable = header.AsSpan();
                var readable = header.AsReadOnlySpan();

                writable[0] = 10;
                storage.buffer[1] = 20;

                Assert.AreEqual(2, writable.Length);
                Assert.AreEqual(2, readable.Length);
                Assert.AreEqual(10, readable[0]);
                Assert.AreEqual(10, storage.buffer[0]);
                Assert.AreEqual(20, writable[1]);
            }
        }

        [Test]
        public void AddReplicate_FillsTheRequestedRangeAndAliasesTheBuffer()
        {
            using var storage = new SharedListStorage<int>(6);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1);

                var zeroed = header.AddReplicate(2);
                var filled = header.AddReplicate(7, 2);
                var noInit = header.AddReplicateNoInit(1);
                noInit[0] = 8;
                zeroed[0] = 5;

                Assert.AreEqual(2, zeroed.Length);
                Assert.AreEqual(2, filled.Length);
                Assert.AreEqual(6, header.Count);
                CollectionAssert.AreEqual(new[] { 1, 5, 0, 7, 7, 8 }, header.ToArray());
                Assert.AreEqual(8, storage.buffer[5]);
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AddReplicate_MoreThanTheFreeCapacity_ThrowsAndKeepsContent()
        {
            using var storage = new SharedListStorage<int>(3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1);

                Assert.Throws<InvalidOperationException>(() => header.AddReplicate(3));
                Assert.Throws<InvalidOperationException>(() => header.AddReplicate(7, 3));
                Assert.Throws<InvalidOperationException>(() => header.AddReplicateNoInit(3));
                Assert.AreEqual(1, header.Count);
                CollectionAssert.AreEqual(new[] { 1 }, header.ToArray());
            }
        }

        [Test]
        public void AddReplicate_ExactlyFillsTheList_AddsEveryElement()
        {
            using var storage = new SharedListStorage<int>(3);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.AddReplicate(4, 3);

                Assert.AreEqual(3, header.Count);
                CollectionAssert.AreEqual(new[] { 4, 4, 4 }, header.ToArray());
            }
        }

        [Test]
        public void AddReplicate_NegativeAmount_ThrowsAndKeepsTheCount()
        {
            using var storage = new SharedListStorage<int>(4);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(1);

                Assert.Catch(() => header.AddReplicate(-1));
                Assert.Catch(() => header.AddReplicate(7, -1));
                Assert.AreEqual(1, header.Count);
            }
        }

        [Test]
        public void Elements_ByteElement_RoundTripBitExact()
        {
            AssertElementRoundTrip<byte>(0xA5, 0x00, 0xFF);
        }

        [Test]
        public void Elements_ThreeByteStruct_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Rgb { r = 1, g = 2, b = 3 }
                , new Rgb { r = 0, g = 0, b = 0 }
                , new Rgb { r = 255, g = 254, b = 253 }
            );
        }

        [Test]
        public void Elements_LongElement_RoundTripBitExact()
        {
            AssertElementRoundTrip(long.MinValue, 0L, long.MaxValue);
        }

        [Test]
        public void Elements_StructLargerThanSixteenBytes_RoundTripBitExact()
        {
            AssertElementRoundTrip(
                  new Wide { a = 1, b = -2, c = long.MaxValue }
                , new Wide { a = 0, b = 0, c = 0 }
                , new Wide { a = long.MinValue, b = 7, c = -1 }
            );
        }

        private static void AssertElementRoundTrip<T>(T a, T b, T c)
            where T : unmanaged
        {
            var name = typeof(T).Name;
            using var storage = new SharedListStorage<T>(6);
            var header = storage.CreateHeader();

            // SAFETY: storage owns the blocks the header borrows and outlives every call below.
            unsafe
            {
                header.Add(a);
                header.Add(in b);
                header.AddRange(new[] { c });
                header.Insert(1, c);

                CollectionAssert.AreEqual(new[] { a, c, b, c }, header.ToArray(), name);

                header.RemoveAt(1);

                Assert.AreEqual(a, header[0], name);
                Assert.AreEqual(b, header.ElementAt(1), name);
                Assert.AreEqual(c, header[2], name);

                var copy = new T[3];
                header.CopyTo(copy);

                CollectionAssert.AreEqual(new[] { a, b, c }, copy, name);

                header.CopyFrom(new[] { c, b, a });

                CollectionAssert.AreEqual(new[] { c, b, a }, header.AsReadOnlySpan().ToArray(), name);

                var replicated = header.AddReplicate(a, 2);

                Assert.AreEqual(a, replicated[0], name);
                Assert.AreEqual(a, replicated[1], name);
                Assert.AreEqual(5, header.Count, name);
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
