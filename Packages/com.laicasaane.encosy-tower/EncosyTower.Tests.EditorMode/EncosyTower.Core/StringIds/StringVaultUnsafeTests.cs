using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EncosyTower.Ids;
using EncosyTower.StringIds;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Tests.Core.StringIds
{
    // Adapted from Unity.Collections.Tests/NativeHashMapTests.cs (id/lookup patterns)
    // against the raw StringVaultUnsafe header.
    public partial class StringVaultUnsafeTests
    {
        private const string TABLE_NAME = "level_table_asset";
        private const string UNICODE_NAME = "\u00dcn\u00efc\u00f6d\u00e9 \u2713";

        [Test]
        public void GetOrMakeId_SameString_ReturnsSameId()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);

            var id1 = vault.GetOrMakeId((UnmanagedString)"hello");
            var id2 = vault.GetOrMakeId((UnmanagedString)"hello");

            Assert.AreEqual(id1, id2);
        }

        [Test]
        public void GetOrMakeId_DifferentStrings_ReturnsDifferentIds()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);

            var idA = vault.GetOrMakeId((UnmanagedString)"apple");
            var idB = vault.GetOrMakeId((UnmanagedString)"banana");

            Assert.AreNotEqual(idA, idB);
        }

        [Test]
        public void TryGetId_Existing_MatchesGetOrMakeId()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)"world");

            Assert.IsTrue(vault.TryGetId((UnmanagedString)"world", out var found));
            Assert.AreEqual(id, found);
        }

        [Test]
        public void TryGetId_Missing_ReturnsFalse()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"present");

            Assert.IsFalse(vault.TryGetId((UnmanagedString)"absent", out _));
        }

        [Test]
        public void TryGetUnmanagedString_RoundTrips()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)"round-trip");

            Assert.IsTrue(vault.TryGetUnmanagedString(id, out var back));
            Assert.AreEqual("round-trip", back.ToString());
        }

        [Test]
        public void ContainsId_ReflectsRegistration()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)"registered");

            Assert.IsTrue(vault.ContainsId(id));
        }

        [Test]
        public void Clear_ResetsRegistry()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"temp");
            var oldId = vault.GetOrMakeId((UnmanagedString)"temp2");

            vault.Clear();

            Assert.IsFalse(vault.TryGetId((UnmanagedString)"temp", out _));
            Assert.IsFalse(vault.ContainsId(oldId));

            vault.GetOrMakeId((UnmanagedString)"fresh");
            Assert.IsTrue(vault.TryGetId((UnmanagedString)"fresh", out _));
            Assert.IsFalse(vault.ContainsId(oldId));
        }

        [Test]
        public void RoundTrip_ManyStrings_GrowsAndResolves()
        {
            const int N = 40;
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);

            var ids = new StringId[N];

            for (var i = 0; i < N; i++)
            {
                ids[i] = vault.GetOrMakeId((UnmanagedString)$"str{i}");
            }

            for (var i = 0; i < N; i++)
            {
                Assert.IsTrue(vault.TryGetUnmanagedString(ids[i], out var back), $"id {i}");
                Assert.AreEqual($"str{i}", back.ToString());

                Assert.IsTrue(vault.TryGetId((UnmanagedString)$"str{i}", out var id), $"str{i}");
                Assert.AreEqual(ids[i], id);
            }
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var vault = new StringVaultUnsafe(4, Allocator.Persistent);
            vault.GetOrMakeId((UnmanagedString)"value");

            vault.Dispose(default(JobHandle)).Complete();

            Assert.IsFalse(vault.IsCreated);
        }

        [Test]
        public void GetOrMakeId_SameTextFromEverySource_ReturnsOneId()
        {
            var builder = new StringBuilder("level_");
            builder.Append("table_asset");
            var runtimeText = builder.ToString();

            using var vault = new StringVaultUnsafe(4, Allocator.Temp);

            var fromLiteral = vault.GetOrMakeId((UnmanagedString)TABLE_NAME);
            StackOverwriter.Fill(value: 0xCD);

            var fromRuntime = vault.GetOrMakeId((UnmanagedString)runtimeText);
            StackOverwriter.Fill(value: 0x00);

            UnmanagedString fixedSource = (FixedString512Bytes)TABLE_NAME;
            var fromFixed = vault.GetOrMakeId(fixedSource);
            StackOverwriter.Fill(value: 0xCD);

            Assert.IsTrue(vault.TryGetUnmanagedString(fromLiteral, out var storedUnmanaged));
            var fromStored = vault.GetOrMakeId(storedUnmanaged);
            StackOverwriter.Fill(value: 0x00);

            Assert.AreEqual(fromLiteral, fromRuntime);
            Assert.AreEqual(fromLiteral, fromFixed);
            Assert.AreEqual(fromLiteral, fromStored);

            Assert.IsTrue(vault.TryGetId((UnmanagedString)TABLE_NAME, out var found));
            Assert.AreEqual(fromLiteral, found);

            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(TABLE_NAME), storedUnmanaged.AsReadOnlySpan().ToArray());
        }

        [Test]
        public void GetOrMakeId_TableNames_RoundTripEveryName()
        {
            var names = new[]
            {
                "hero_table_asset",
                "enemy_table_asset",
                "string_table_asset",
                TABLE_NAME,
                UNICODE_NAME,
            };

            var count = names.Length;
            var ids = new StringId[count];

            using var vault = new StringVaultUnsafe(4, Allocator.Temp);

            for (var i = 0; i < count; i++)
            {
                ids[i] = vault.GetOrMakeId((UnmanagedString)names[i]);
                StackOverwriter.Fill(value: 0xCD);
            }

            for (var i = 0; i < count; i++)
            {
                for (var j = i + 1; j < count; j++)
                {
                    Assert.AreNotEqual(ids[i], ids[j], $"{names[i]} / {names[j]}");
                }
            }

            for (var i = 0; i < count; i++)
            {
                var name = names[i];
                var unmanagedName = (UnmanagedString)name;

                Assert.IsTrue(vault.TryGetId(unmanagedName, out var found), name);
                Assert.AreEqual(ids[i], found, name);

                StackOverwriter.Fill(value: 0x00);

                Assert.AreEqual(ids[i], vault.GetOrMakeId(unmanagedName), name);

                Assert.IsTrue(vault.TryGetUnmanagedString(ids[i], out var stored), name);
                CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(name), stored.AsReadOnlySpan().ToArray(), name);
            }
        }

        [Test]
        public void Constructor_ZeroOrNegativeCapacity_CreatesVaultWithOnlyTheInvalidSlot()
        {
            using var zero = new StringVaultUnsafe(0, Allocator.Temp);
            using var negative = new StringVaultUnsafe(-5, Allocator.Temp);

            Assert.IsTrue(zero.IsCreated);
            Assert.AreEqual(1, zero.Count);
            Assert.GreaterOrEqual(zero.Capacity, 1);

            Assert.IsTrue(negative.IsCreated);
            Assert.AreEqual(1, negative.Count);
            Assert.GreaterOrEqual(negative.Capacity, 1);
        }

        [Test]
        public void Constructor_ZeroCapacity_GrowsOnFirstRegistration()
        {
            using var vault = new StringVaultUnsafe(0, Allocator.Temp);

            var id = vault.GetOrMakeId((UnmanagedString)"first");

            Assert.AreEqual(2, vault.Count);
            Assert.IsTrue(vault.TryGetUnmanagedString(id, out var stored));
            Assert.AreEqual("first", stored.ToString());
        }

        [Test]
        public void Constructor_CopySource_IsIndependentDeepCopy()
        {
            using var source = new StringVaultUnsafe(4, Allocator.Temp);
            var idA = source.GetOrMakeId((UnmanagedString)"alpha");
            var idB = source.GetOrMakeId((UnmanagedString)"beta");

            using var copy = new StringVaultUnsafe(source, Allocator.Temp);
            var idC = copy.GetOrMakeId((UnmanagedString)"gamma");

            Assert.AreEqual(3, source.Count);
            Assert.AreEqual(4, copy.Count);
            Assert.IsTrue(copy.TryGetId((UnmanagedString)"alpha", out var foundA));
            Assert.AreEqual(idA, foundA);
            Assert.IsTrue(copy.TryGetId((UnmanagedString)"beta", out var foundB));
            Assert.AreEqual(idB, foundB);
            Assert.IsFalse(source.TryGetId((UnmanagedString)"gamma", out _));
            Assert.IsTrue(copy.ContainsId(idC));
            Assert.IsFalse(source.ContainsId(idC));
        }

        [Test]
        public void Constructor_CopySourceNotCreated_ThrowsArgumentException()
        {
            var notCreated = default(StringVaultUnsafe);

            Assert.Catch<ArgumentException>(() => _ = new StringVaultUnsafe(notCreated, Allocator.Temp));
        }

        [Test]
        public void Indexer_InvalidSlot_IsEmpty()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);

            Assert.IsTrue(vault[0].IsEmpty);
        }

        [TestCase("A\U0001F600B")]
        [TestCase("\u00fc\u00ef\u00f6")]
        [TestCase("\u2713\u2713\u2713")]
        public void Indexer_NonAsciiText_ReturnsTheRegisteredUtf8Bytes(string text)
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)text);

            var stored = vault[(int)(uint)id.Id];

            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), stored.AsReadOnlySpan().ToArray());
        }

        [Test]
        public void Indexer_NegativeOrHugeIndex_ThrowsIndexOutOfRange()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"a");

            Assert.Throws<IndexOutOfRangeException>(() => _ = vault[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = vault[int.MaxValue]);
        }

        [Test]
        public void Alloc_ThenFree_ReleasesTheHeader()
        {
            // SAFETY: The header returned by Alloc stays live until Free, which runs exactly once in finally.
            unsafe
            {
                var data = StringVaultUnsafe.Alloc(4, Allocator.Persistent);

                try
                {
                    Assert.IsTrue(data->IsCreated);
                    Assert.AreEqual(1, data->Count);
                    Assert.IsFalse(data->AllowEmptyString);
                }
                finally
                {
                    StringVaultUnsafe.Free(data);
                }
            }
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Alloc_AcceptedAllocator_CreatesAndFreesTheHeader(Allocator allocator)
        {
            // SAFETY: The header returned by Alloc stays live until Free, which runs exactly once in finally.
            unsafe
            {
                var data = StringVaultUnsafe.Alloc(2, allocator);

                try
                {
                    var id = data->GetOrMakeId((UnmanagedString)"value");

                    Assert.IsTrue(data->IsCreated);
                    Assert.IsTrue(data->ContainsId(id));
                }
                finally
                {
                    StringVaultUnsafe.Free(data);
                }
            }
        }

        [Test]
        public void Alloc_CapacityOne_GrowsPastFullAndKeepsEveryString()
        {
            // SAFETY: The header returned by Alloc stays live until Free, which runs exactly once in finally.
            unsafe
            {
                var data = StringVaultUnsafe.Alloc(1, Allocator.Persistent);

                try
                {
                    var ids = new StringId[6];

                    for (var i = 0; i < ids.Length; i++)
                    {
                        ids[i] = data->GetOrMakeId((UnmanagedString)$"s{i}");
                    }

                    for (var i = 0; i < ids.Length; i++)
                    {
                        Assert.IsTrue(data->TryGetUnmanagedString(ids[i], out var stored), $"s{i}");
                        Assert.AreEqual($"s{i}", stored.ToString());
                    }
                }
                finally
                {
                    StringVaultUnsafe.Free(data);
                }
            }
        }

        [Test]
        public void Alloc_CopySource_IsIndependentOfTheSource()
        {
            using var source = new StringVaultUnsafe(4, Allocator.Temp);
            var id = source.GetOrMakeId((UnmanagedString)"shared");

            // SAFETY: The header returned by Alloc stays live until Free, which runs exactly once in finally.
            unsafe
            {
                var data = StringVaultUnsafe.Alloc(source, Allocator.Persistent);

                try
                {
                    data->GetOrMakeId((UnmanagedString)"only-in-copy");

                    Assert.AreEqual(2, source.Count);
                    Assert.AreEqual(3, data->Count);
                    Assert.IsTrue(data->TryGetId((UnmanagedString)"shared", out var found));
                    Assert.AreEqual(id, found);
                }
                finally
                {
                    StringVaultUnsafe.Free(data);
                }
            }
        }

        [Test]
        public void Free_NullPointer_DoesNothing()
        {
            // SAFETY: Free documents that a null header is ignored, so no memory is read or released.
            unsafe
            {
                Assert.DoesNotThrow(() => StringVaultUnsafe.Free(null));
            }
        }

        [Test]
        public void Dispose_Twice_IsNoOp()
        {
            var vault = new StringVaultUnsafe(4, Allocator.Persistent);
            vault.GetOrMakeId((UnmanagedString)"value");

            vault.Dispose();

            Assert.IsFalse(vault.IsCreated);
            Assert.DoesNotThrow(() => vault.Dispose());
            Assert.IsFalse(vault.IsCreated);
        }

        [Test]
        public void Dispose_Job_AfterDispose_ReturnsTheInputHandleUnchanged()
        {
            var vault = new StringVaultUnsafe(4, Allocator.Persistent);
            vault.Dispose();

            var handle = vault.Dispose(default(JobHandle));
            handle.Complete();

            Assert.IsFalse(vault.IsCreated);
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_AcceptedAllocator_MarksNotCreated(Allocator allocator)
        {
            var vault = new StringVaultUnsafe(4, allocator);
            vault.GetOrMakeId((UnmanagedString)"value");

            vault.Dispose();

            Assert.IsFalse(vault.IsCreated);
            Assert.AreEqual(0, vault.Count);
        }

        [Test]
        public void Clear_EmptyVault_KeepsOnlyTheInvalidSlot()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);

            vault.Clear();
            vault.Clear();

            Assert.AreEqual(1, vault.Count);
            Assert.IsTrue(vault.IsCreated);
        }

        [Test]
        public void ContainsId_DefaultAndUnregisteredId_ReturnsFalse()
        {
            using var vault = new StringVaultUnsafe(16, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"a");

            var unregistered = new StringId(new Id(vault.Count + 3));
            var beyondCapacity = new StringId(new Id(uint.MaxValue));

            Assert.IsFalse(vault.ContainsId(default));
            Assert.IsFalse(vault.ContainsId(unregistered));
            Assert.IsFalse(vault.ContainsId(beyondCapacity));
        }

        [Test]
        public void GetOrMakeId_EmptyString_ReturnsInvalidIdWithoutRegistering()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            var countBefore = vault.Count;

            var id = vault.GetOrMakeId(default(UnmanagedString));

            Assert.IsFalse(id.IsValid);
            Assert.AreEqual(countBefore, vault.Count);
            Assert.IsFalse(vault.TryGetId(default(UnmanagedString), out _));
            Assert.IsFalse(vault.TryGetId(default(UnmanagedString)).HasValue);
        }

        [TestCase("A\U0001F600B")]
        [TestCase("\u00fc\u00ef\u00f6")]
        [TestCase("\u2713\u2713\u2713")]
        public void TryGetUnmanagedString_NonAsciiText_RoundTripsUtf8Bytes(string text)
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)text);

            StackOverwriter.Fill(value: 0xCD);

            Assert.IsTrue(vault.TryGetId((UnmanagedString)text, out var found));
            Assert.AreEqual(id, found);
            Assert.IsTrue(vault.TryGetUnmanagedString(id, out var stored));
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), stored.AsReadOnlySpan().ToArray());
            Assert.AreEqual(text, vault.TryGetUnmanagedString(id).GetValueOrThrow().ToString());
        }

        [Test]
        public void GetOrMakeId_ExactCapacityString_RoundTripsEveryByte()
        {
            var text = new string('a', UnmanagedString.UTF8MaxLengthInBytes);

            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)text);

            Assert.IsTrue(vault.TryGetUnmanagedString(id, out var stored));
            Assert.AreEqual(UnmanagedString.UTF8MaxLengthInBytes, stored.Length);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), stored.AsReadOnlySpan().ToArray());
        }

        [Test]
        public void TryGetUnmanagedString_NeverRegisteredId_ReturnsFalse()
        {
            using var vault = new StringVaultUnsafe(16, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"a");
            vault.GetOrMakeId((UnmanagedString)"b");

            var neverRegistered = new StringId(new Id(vault.Count + 3));

            Assert.IsFalse(vault.ContainsId(neverRegistered));
            Assert.IsFalse(vault.TryGetUnmanagedString(neverRegistered, out _));
            Assert.IsFalse(vault.TryGetUnmanagedString(neverRegistered).HasValue);
        }

        [Test]
        public void TryGetUnmanagedString_DefaultId_ReturnsFalse()
        {
            using var vault = new StringVaultUnsafe(16, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"a");

            Assert.IsFalse(vault.ContainsId(default));
            Assert.IsFalse(vault.TryGetUnmanagedString(id: default, result: out var result));
            Assert.IsTrue(result.IsEmpty);
        }

        [Test]
        public void TryGetUnmanagedString_IdBeyondCapacity_ReturnsFalseWithEmptyString()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"a");

            var beyondCapacity = new StringId(new Id(uint.MaxValue));

            Assert.IsFalse(vault.TryGetUnmanagedString(beyondCapacity, out var result));
            Assert.IsTrue(result.IsEmpty);
        }

        [Test]
        public void CopyTo_DefaultLength_CopiesEveryRegisteredString()
        {
            using var vault = CreateVault(count: 8);
            var expected = UnmanagedStringVaultCopyAssert.Collect(vault);

            UnmanagedStringVaultCopyAssert.CopyToDefaultLength(vault, expected);
        }

        [Test]
        public void TryCopyTo_DefaultLength_ReturnsTrueAndCopiesEveryRegisteredString()
        {
            using var vault = CreateVault(count: 8);
            var expected = UnmanagedStringVaultCopyAssert.Collect(vault);

            UnmanagedStringVaultCopyAssert.TryCopyToDefaultLength(vault, expected);
        }

        [Test]
        public void CopyTo_SourceStartAndLength_CopiesTheMatchingSlice()
        {
            using var vault = CreateVault(count: 8);
            var expected = UnmanagedStringVaultCopyAssert.Collect(vault);

            UnmanagedStringVaultCopyAssert.CopyToSourceStartAndLength(vault, expected);
        }

        [Test]
        public void TryCopyTo_SourceStartAndLength_ReturnsTrueAndCopiesTheMatchingSlice()
        {
            using var vault = CreateVault(count: 8);
            var expected = UnmanagedStringVaultCopyAssert.Collect(vault);

            UnmanagedStringVaultCopyAssert.TryCopyToSourceStartAndLength(vault, expected);
        }

        [Test]
        public void TryCopyTo_StartPastDestinationLength_ReturnsTrue()
        {
            using var vault = CreateVault(count: 8);
            var expected = UnmanagedStringVaultCopyAssert.Collect(vault);

            UnmanagedStringVaultCopyAssert.TryCopyToStartPastDestinationLength(vault, expected);
        }

        [Test]
        public void TryCopyTo_LengthPastDestination_ReturnsFalse()
        {
            using var vault = CreateVault(count: 8);

            UnmanagedStringVaultCopyAssert.TryCopyToLengthPastDestination(vault);
        }

        [Test]
        public void TryCopyTo_InvalidRange_ReturnsFalse()
        {
            using var vault = CreateVault(count: 3);

            UnmanagedStringVaultCopyAssert.TryCopyToInvalidRange(vault, vault.Count);
        }

        [Test]
        public void CopyTo_InvalidRange_ThrowsArgumentOutOfRange()
        {
            using var vault = CreateVault(count: 3);

            UnmanagedStringVaultCopyAssert.CopyToInvalidRangeThrows(vault, vault.Count);
            UnmanagedStringVaultCopyAssert.CopyToLengthPastDestinationThrows(vault);
        }

        [Test]
        public void CopyTo_EmptyVault_CopiesTheInvalidSlot()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);

            UnmanagedStringVaultCopyAssert.CopyToEmptyVault(vault);
        }

        [Test]
        public void TryCopyTo_EmptyVault_ReturnsTrueAndCopiesTheInvalidSlot()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);

            UnmanagedStringVaultCopyAssert.TryCopyToEmptyVault(vault);
        }

        [Test]
        public void IncreaseCapacityBy_ZeroOrNegativeAmount_ThrowsArgumentOutOfRange()
        {
            var vault = new StringVaultUnsafe(4, Allocator.Temp);

            try
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => vault.IncreaseCapacityBy(0));
                Assert.Throws<ArgumentOutOfRangeException>(() => vault.IncreaseCapacityBy(-1));
            }
            finally
            {
                vault.Dispose();
            }
        }

        [Test]
        public void IncreaseCapacityBy_PositiveAmount_GrowsAndKeepsEveryString()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            var idA = vault.GetOrMakeId((UnmanagedString)"alpha");
            var idB = vault.GetOrMakeId((UnmanagedString)"beta");
            var capacityBefore = vault.Capacity;

            var result = vault.IncreaseCapacityBy(10);

            Assert.GreaterOrEqual(vault.Capacity, capacityBefore + 10);
            Assert.GreaterOrEqual(result, vault.Capacity);
            Assert.IsTrue(vault.TryGetUnmanagedString(idA, out var storedA));
            Assert.AreEqual("alpha", storedA.ToString());
            Assert.IsTrue(vault.TryGetUnmanagedString(idB, out var storedB));
            Assert.AreEqual("beta", storedB.ToString());

            var idC = vault.GetOrMakeId((UnmanagedString)"gamma");

            Assert.AreEqual(4, vault.Count);
            Assert.IsTrue(vault.ContainsId(idC));
        }

        [Test]
        public void IncreaseCapacityTo_SmallerOrZero_KeepsCapacityAndContent()
        {
            using var vault = new StringVaultUnsafe(8, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)"alpha");
            var capacityBefore = vault.Capacity;

            vault.IncreaseCapacityTo(0);
            vault.IncreaseCapacityTo(1);

            Assert.AreEqual(capacityBefore, vault.Capacity);
            Assert.IsTrue(vault.ContainsId(id));
        }

        [Test]
        public void IncreaseCapacityTo_Larger_GrowsAndKeepsEveryString()
        {
            using var vault = new StringVaultUnsafe(2, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)"alpha");

            vault.IncreaseCapacityTo(64);

            Assert.GreaterOrEqual(vault.Capacity, 64);
            Assert.IsTrue(vault.TryGetUnmanagedString(id, out var stored));
            Assert.AreEqual("alpha", stored.ToString());
        }

        [Test]
        public void GetEnumerator_EmptyVault_YieldsOnlyTheInvalidSlot()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);

            var items = new List<UnmanagedString>();

            foreach (var item in vault)
            {
                items.Add(item);
            }

            Assert.AreEqual(1, items.Count);
            Assert.IsTrue(items[0].IsEmpty);
        }

        [Test]
        public void GetEnumerator_Dispose_IsRepeatableNoOp()
        {
            using var vault = new StringVaultUnsafe(4, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"a");

            var enumerator = vault.GetEnumerator();
            var moved = enumerator.MoveNext();

            enumerator.Dispose();
            enumerator.Dispose();

            Assert.IsTrue(moved);
            Assert.IsTrue(vault.IsCreated);
        }

        [Test]
        public void GetEnumerator_InterfaceEnumerators_YieldTheSameSequenceAsTheStructEnumerator()
        {
            using var vault = CreateVault(count: 3);

            var expected = new List<UnmanagedString>();

            foreach (var item in vault)
            {
                expected.Add(item);
            }

            var generic = UnmanagedStringVaultCopyAssert.Collect(vault);
            var nonGenericCount = 0;
            var nonGeneric = ((IEnumerable)vault).GetEnumerator();

            while (nonGeneric.MoveNext())
            {
                nonGenericCount++;
            }

            CollectionAssert.AreEqual(expected, generic);
            Assert.AreEqual(expected.Count, nonGenericCount);
            Assert.AreEqual(vault.Count, expected.Count);
        }

        private static StringVaultUnsafe CreateVault(int count)
        {
            var vault = new StringVaultUnsafe(16, Allocator.Temp);

            for (var i = 0; i < count; i++)
            {
                vault.GetOrMakeId((UnmanagedString)$"s{i}");
            }

            return vault;
        }
    }
}
