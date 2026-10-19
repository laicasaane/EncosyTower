using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EncosyTower.Ids;
using EncosyTower.StringIds;
using EncosyTower.Tests.Core.Collections;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Tests.Core.StringIds
{
    // Adapted from Unity.Collections.Tests/NativeHashMapTests.cs (id/lookup patterns)
    // against the StringVaultNative forwarder.
    public partial class StringVaultNativeTests
    {
        private const string TABLE_NAME = "level_table_asset";
        private const string UNICODE_NAME = "\u00dcn\u00efc\u00f6d\u00e9 \u2713";

        [Test]
        public void GetOrMakeId_SameString_ReturnsSameId()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);

            var id1 = vault.GetOrMakeId((UnmanagedString)"hello");
            var id2 = vault.GetOrMakeId((UnmanagedString)"hello");

            Assert.AreEqual(id1, id2);
        }

        [Test]
        public void GetOrMakeId_DifferentStrings_ReturnsDifferentIds()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);

            var idA = vault.GetOrMakeId((UnmanagedString)"apple");
            var idB = vault.GetOrMakeId((UnmanagedString)"banana");

            Assert.AreNotEqual(idA, idB);
        }

        [Test]
        public void TryGetId_Existing_MatchesGetOrMakeId()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)"world");

            Assert.IsTrue(vault.TryGetId((UnmanagedString)"world", out var found));
            Assert.AreEqual(id, found);
        }

        [Test]
        public void TryGetId_Missing_ReturnsFalse()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"present");

            Assert.IsFalse(vault.TryGetId((UnmanagedString)"absent", out _));
        }

        [Test]
        public void TryGetUnmanagedString_RoundTrips()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)"round-trip");

            Assert.IsTrue(vault.TryGetUnmanagedString(id, out var back));
            Assert.AreEqual("round-trip", back.ToString());
        }

        [Test]
        public void ContainsId_ReflectsRegistration()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)"registered");

            Assert.IsTrue(vault.ContainsId(id));
        }

        [Test]
        public void Clear_ResetsRegistry()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
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
            using var vault = new StringVaultNative(4, Allocator.Temp);

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
        public void Seeding_Constructor_RegistersAll()
        {
            var seed = new[]
            {
                (UnmanagedString)"alpha",
                (UnmanagedString)"beta",
                (UnmanagedString)"gamma",
            };

            using var vault = new StringVaultNative(seed, Allocator.Temp);

            foreach (var str in seed)
            {
                Assert.IsTrue(vault.TryGetId(str, out _), str.ToString());
            }
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var vault = new StringVaultNative(4, Allocator.Temp);
            Assert.IsTrue(vault.IsCreated);

            vault.Dispose();

            Assert.IsFalse(vault.IsCreated);
        }

        [Test]
        public void DisposeJob_CompletesAndMarksNotCreated()
        {
            var vault = new StringVaultNative(4, Allocator.Persistent);
            vault.GetOrMakeId((UnmanagedString)"value");

            vault.Dispose(default(JobHandle)).Complete();

            Assert.IsFalse(vault.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void UseAfterDispose_Throws()
        {
            var vault = new StringVaultNative(4, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"x");
            vault.Dispose();

            Assert.Catch(() => _ = vault.Count);
        }

        [Test]
        public void GetOrMakeId_SameTextFromEverySource_ReturnsOneId()
        {
            var builder = new StringBuilder("level_");
            builder.Append("table_asset");
            var runtimeText = builder.ToString();

            using var vault = new StringVaultNative(4, Allocator.Temp);

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
            var seed = new UnmanagedString[count];

            using var vault = new StringVaultNative(4, Allocator.Temp);

            for (var i = 0; i < count; i++)
            {
                seed[i] = names[i];
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

            using var seededVault = new StringVaultNative(seed, Allocator.Temp);

            for (var i = 0; i < count; i++)
            {
                var name = names[i];
                var unmanagedName = (UnmanagedString)name;

                Assert.IsTrue(seededVault.TryGetId(unmanagedName, out var found), name);
                Assert.AreEqual(found, seededVault.GetOrMakeId(unmanagedName), name);

                StackOverwriter.Fill(value: 0xCD);

                Assert.IsTrue(seededVault.TryGetUnmanagedString(found, out var stored), name);
                CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(name), stored.AsReadOnlySpan().ToArray(), name);
            }
        }

        [Test]
        public void Constructor_ZeroOrNegativeCapacity_CreatesVaultWithOnlyTheInvalidSlot()
        {
            using var zero = new StringVaultNative(0, Allocator.Temp);
            using var negative = new StringVaultNative(-5, Allocator.Temp);

            Assert.IsTrue(zero.IsCreated);
            Assert.AreEqual(1, zero.Count);
            Assert.GreaterOrEqual(zero.Capacity, 1);

            Assert.IsTrue(negative.IsCreated);
            Assert.AreEqual(1, negative.Count);
            Assert.GreaterOrEqual(negative.Capacity, 1);
        }

        [Test]
        public void Constructor_EmptySeed_CreatesVaultWithOnlyTheInvalidSlot()
        {
            using var vault = new StringVaultNative(ReadOnlySpan<UnmanagedString>.Empty, Allocator.Temp);

            Assert.IsTrue(vault.IsCreated);
            Assert.AreEqual(1, vault.Count);
        }

        [Test]
        public void Constructor_AllowEmptyString_IsReported()
        {
            using var allowing = new StringVaultNative(4, Allocator.Temp, allowEmptyString: true);
            using var rejecting = new StringVaultNative(4, Allocator.Temp, allowEmptyString: false);

            Assert.IsTrue(allowing.AllowEmptyString);
            Assert.IsFalse(rejecting.AllowEmptyString);
        }

        [Test]
        public void Indexer_InvalidSlot_IsEmpty()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);

            Assert.IsTrue(vault[0].IsEmpty);
        }

        [TestCase("A\U0001F600B")]
        [TestCase("\u00fc\u00ef\u00f6")]
        [TestCase("\u2713\u2713\u2713")]
        public void Indexer_NonAsciiText_ReturnsTheRegisteredUtf8Bytes(string text)
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)text);

            var stored = vault[(int)(uint)id.Id];

            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), stored.AsReadOnlySpan().ToArray());
        }

        [Test]
        public void Indexer_NegativeOrHugeIndex_ThrowsIndexOutOfRange()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"a");

            Assert.Throws<IndexOutOfRangeException>(() => _ = vault[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = vault[int.MaxValue]);
        }

        [Test]
        public void ContainsId_DefaultAndUnregisteredId_ReturnsFalse()
        {
            using var vault = new StringVaultNative(16, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"a");

            var unregistered = new StringId(new Id(vault.Count + 3));
            var beyondCapacity = new StringId(new Id(uint.MaxValue));

            Assert.IsFalse(vault.ContainsId(default));
            Assert.IsFalse(vault.ContainsId(unregistered));
            Assert.IsFalse(vault.ContainsId(beyondCapacity));
        }

        [Test]
        public void Clear_EmptyVault_KeepsOnlyTheInvalidSlot()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);

            vault.Clear();
            vault.Clear();

            Assert.AreEqual(1, vault.Count);
            Assert.IsTrue(vault.IsCreated);
        }

        [Test]
        public void GetOrMakeId_EmptyString_ReturnsInvalidIdWithoutRegistering()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
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
            using var vault = new StringVaultNative(4, Allocator.Temp);
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

            using var vault = new StringVaultNative(4, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)text);

            Assert.IsTrue(vault.TryGetUnmanagedString(id, out var stored));
            Assert.AreEqual(UnmanagedString.UTF8MaxLengthInBytes, stored.Length);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), stored.AsReadOnlySpan().ToArray());
        }

        [Test]
        public void TryGetUnmanagedString_NeverRegisteredId_ReturnsFalse()
        {
            using var vault = new StringVaultNative(16, Allocator.Temp);
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
            using var vault = new StringVaultNative(16, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"a");

            Assert.IsFalse(vault.ContainsId(default));
            Assert.IsFalse(vault.TryGetUnmanagedString(id: default, result: out var result));
            Assert.IsTrue(result.IsEmpty);
        }

        [Test]
        public void TryGetUnmanagedString_IdBeyondCapacity_ReturnsFalseWithEmptyString()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
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
            using var vault = new StringVaultNative(4, Allocator.Temp);

            UnmanagedStringVaultCopyAssert.CopyToEmptyVault(vault);
        }

        [Test]
        public void TryCopyTo_EmptyVault_ReturnsTrueAndCopiesTheInvalidSlot()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);

            UnmanagedStringVaultCopyAssert.TryCopyToEmptyVault(vault);
        }

        [Test]
        public void IncreaseCapacityBy_ZeroOrNegativeAmount_ThrowsArgumentOutOfRange()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);

            Assert.Throws<ArgumentOutOfRangeException>(() => vault.IncreaseCapacityBy(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => vault.IncreaseCapacityBy(-1));
        }

        [Test]
        public void IncreaseCapacityBy_PositiveAmount_GrowsAndKeepsEveryString()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
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
            using var vault = new StringVaultNative(8, Allocator.Temp);
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
            using var vault = new StringVaultNative(2, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)"alpha");

            vault.IncreaseCapacityTo(64);

            Assert.GreaterOrEqual(vault.Capacity, 64);
            Assert.IsTrue(vault.TryGetUnmanagedString(id, out var stored));
            Assert.AreEqual("alpha", stored.ToString());
        }

        [Test]
        public void Dispose_Twice_IsNoOp()
        {
            var vault = new StringVaultNative(4, Allocator.Persistent);
            vault.GetOrMakeId((UnmanagedString)"value");

            vault.Dispose();

            Assert.IsFalse(vault.IsCreated);
            Assert.DoesNotThrow(() => vault.Dispose());
            Assert.IsFalse(vault.IsCreated);
        }

        [TestCase(Allocator.Temp)]
        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void Dispose_AcceptedAllocator_MarksNotCreated(Allocator allocator)
        {
            var vault = new StringVaultNative(4, allocator);
            vault.GetOrMakeId((UnmanagedString)"value");

            vault.Dispose();

            Assert.IsFalse(vault.IsCreated);
        }

        [TestCase(Allocator.TempJob)]
        [TestCase(Allocator.Persistent)]
        public void DisposeJob_AcceptedAllocator_CompletesAndMarksNotCreated(Allocator allocator)
        {
            var vault = new StringVaultNative(4, allocator);
            vault.GetOrMakeId((UnmanagedString)"value");

            vault.Dispose(default(JobHandle)).Complete();

            Assert.IsFalse(vault.IsCreated);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void Members_AfterDispose_Throw()
        {
            var vault = new StringVaultNative(4, Allocator.Temp);
            var text = (UnmanagedString)"x";
            var id = vault.GetOrMakeId(text);
            var destination = new UnmanagedString[4];

            vault.Dispose();

            Assert.Catch(() => _ = vault.Capacity);
            Assert.Catch(() => _ = vault.AllowEmptyString);
            Assert.Catch(() => _ = vault[0]);
            Assert.Catch(() => vault.Clear());
            Assert.Catch(() => vault.GetOrMakeId(text));
            Assert.Catch(() => vault.TryGetId(text));
            Assert.Catch(() => vault.TryGetId(text, out _));
            Assert.Catch(() => vault.TryGetUnmanagedString(id));
            Assert.Catch(() => vault.TryGetUnmanagedString(id, out _));
            Assert.Catch(() => vault.ContainsId(id));
            Assert.Catch(() => vault.CopyTo(destination));
            Assert.Catch(() => vault.CopyTo(destination, 1));
            Assert.Catch(() => vault.CopyTo(0, destination));
            Assert.Catch(() => vault.CopyTo(0, destination, 1));
            Assert.Catch(() => vault.TryCopyTo(destination));
            Assert.Catch(() => vault.TryCopyTo(destination, 1));
            Assert.Catch(() => vault.TryCopyTo(0, destination));
            Assert.Catch(() => vault.TryCopyTo(0, destination, 1));
            Assert.Catch(() => vault.IncreaseCapacityBy(1));
            Assert.Catch(() => vault.IncreaseCapacityTo(1));
            Assert.Catch(() => vault.GetEnumerator());
            Assert.Catch(() => vault.AsReadOnly().ContainsId(id));
        }

        [Test]
        public void GetEnumerator_EmptyVault_YieldsOnlyTheInvalidSlot()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);

            var items = new List<UnmanagedString>();

            foreach (var item in vault)
            {
                items.Add(item);
            }

            Assert.AreEqual(1, items.Count);
            Assert.IsTrue(items[0].IsEmpty);
        }

        [Test]
        public void GetEnumerator_Reset_RestartsTheSequence()
        {
            using var vault = CreateVault(count: 2);

            var enumerator = vault.GetEnumerator();
            var first = 0;

            while (enumerator.MoveNext())
            {
                first++;
            }

            enumerator.Reset();
            var second = 0;

            while (enumerator.MoveNext())
            {
                second++;
            }

            Assert.AreEqual(vault.Count, first);
            Assert.AreEqual(first, second);
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void GetEnumerator_MoveNextAfterOwnerDispose_Throws()
        {
            // Persistent: a Temp vault shares the temp safety handle, which copies (enumerator,
            // read-only view) cannot see being invalidated by the owner's Dispose.
            var vault = new StringVaultNative(4, Allocator.Persistent);
            vault.GetOrMakeId((UnmanagedString)"x");
            var enumerator = vault.GetEnumerator();

            vault.Dispose();

            Assert.Catch(() => enumerator.MoveNext());
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

        [Test]
        public void AsReadOnly_EmptyVault_ExposesOnlyTheInvalidSlot()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);

            var view = vault.AsReadOnly();

            Assert.IsTrue(view.IsCreated);
            Assert.AreEqual(1, view.Count);
            Assert.AreEqual(vault.Capacity, view.Capacity);
            Assert.IsFalse(view.AllowEmptyString);
            Assert.IsTrue(view[0].IsEmpty);
        }

        [Test]
        public void AsReadOnly_ViewsTheOwner_SeesLaterRegistrations()
        {
            using var vault = new StringVaultNative(16, Allocator.Temp);
            var view = vault.AsReadOnly();

            var id = vault.GetOrMakeId((UnmanagedString)"later");

            Assert.AreEqual(vault.Count, view.Count);
            Assert.IsTrue(view.ContainsId(id));
            Assert.IsTrue(view.TryGetId((UnmanagedString)"later", out var found));
            Assert.AreEqual(id, found);
        }

        [Test]
        public void AsReadOnly_ReadOnlyIndexerInvalidIndex_ThrowsIndexOutOfRange()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
            var view = vault.AsReadOnly();

            Assert.Throws<IndexOutOfRangeException>(() => _ = view[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = view[int.MaxValue]);
        }

        [TestCase("A\U0001F600B")]
        [TestCase("\u00fc\u00ef\u00f6")]
        [TestCase("\u2713\u2713\u2713")]
        public void AsReadOnly_NonAsciiText_RoundTripsUtf8BytesThroughEveryLookup(string text)
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
            var id = vault.GetOrMakeId((UnmanagedString)text);
            var view = vault.AsReadOnly();

            Assert.IsTrue(view.TryGetId((UnmanagedString)text, out var found));
            Assert.AreEqual(id, found);
            Assert.IsTrue(view.TryGetUnmanagedString(id, out var stored));
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), stored.AsReadOnlySpan().ToArray());
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), view[(int)(uint)id.Id].AsReadOnlySpan().ToArray());
        }

        [Test]
        public void AsReadOnly_UnregisteredIds_ReportFalse()
        {
            using var vault = new StringVaultNative(16, Allocator.Temp);
            vault.GetOrMakeId((UnmanagedString)"a");
            var view = vault.AsReadOnly();

            var neverRegistered = new StringId(new Id(vault.Count + 3));
            var beyondCapacity = new StringId(new Id(uint.MaxValue));

            Assert.IsFalse(view.ContainsId(default));
            Assert.IsFalse(view.ContainsId(neverRegistered));
            Assert.IsFalse(view.ContainsId(beyondCapacity));
            Assert.IsFalse(view.TryGetUnmanagedString(neverRegistered, out _));
            Assert.IsFalse(view.TryGetUnmanagedString(beyondCapacity, out _));
            Assert.IsFalse(view.TryGetUnmanagedString(neverRegistered).HasValue);
            Assert.IsFalse(view.TryGetId(default(UnmanagedString), out _));
            Assert.IsFalse(view.TryGetId(default(UnmanagedString)).HasValue);
        }

        [Test]
        public void AsReadOnly_EmptyVault_EnumeratesOnlyTheInvalidSlot()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);
            var view = vault.AsReadOnly();

            var items = new List<UnmanagedString>();

            foreach (var item in view)
            {
                items.Add(item);
            }

            Assert.AreEqual(1, items.Count);
            Assert.IsTrue(items[0].IsEmpty);
        }

        [Test]
        public void AsReadOnly_InterfaceEnumerators_YieldTheSameSequenceAsTheStructEnumerator()
        {
            using var vault = CreateVault(count: 3);
            var view = vault.AsReadOnly();

            var expected = new List<UnmanagedString>();

            foreach (var item in view)
            {
                expected.Add(item);
            }

            var generic = UnmanagedStringVaultCopyAssert.Collect(view);
            var nonGenericCount = 0;
            var nonGeneric = ((IEnumerable)view).GetEnumerator();

            while (nonGeneric.MoveNext())
            {
                nonGenericCount++;
            }

            CollectionAssert.AreEqual(expected, generic);
            Assert.AreEqual(expected.Count, nonGenericCount);
            Assert.AreEqual(vault.Count, expected.Count);
        }

        [Test]
        public void CopyTo_ReadOnlyDefaultLength_CopiesEveryRegisteredString()
        {
            using var vault = CreateVault(count: 8);
            var view = vault.AsReadOnly();
            var expected = UnmanagedStringVaultCopyAssert.Collect(view);

            UnmanagedStringVaultCopyAssert.CopyToDefaultLength(view, expected);
        }

        [Test]
        public void TryCopyTo_ReadOnlyDefaultLength_ReturnsTrueAndCopiesEveryRegisteredString()
        {
            using var vault = CreateVault(count: 8);
            var view = vault.AsReadOnly();
            var expected = UnmanagedStringVaultCopyAssert.Collect(view);

            UnmanagedStringVaultCopyAssert.TryCopyToDefaultLength(view, expected);
        }

        [Test]
        public void CopyTo_ReadOnlySourceStartAndLength_CopiesTheMatchingSlice()
        {
            using var vault = CreateVault(count: 8);
            var view = vault.AsReadOnly();
            var expected = UnmanagedStringVaultCopyAssert.Collect(view);

            UnmanagedStringVaultCopyAssert.CopyToSourceStartAndLength(view, expected);
        }

        [Test]
        public void TryCopyTo_ReadOnlySourceStartAndLength_ReturnsTrueAndCopiesTheMatchingSlice()
        {
            using var vault = CreateVault(count: 8);
            var view = vault.AsReadOnly();
            var expected = UnmanagedStringVaultCopyAssert.Collect(view);

            UnmanagedStringVaultCopyAssert.TryCopyToSourceStartAndLength(view, expected);
        }

        [Test]
        public void TryCopyTo_ReadOnlyStartPastDestinationLength_ReturnsTrue()
        {
            using var vault = CreateVault(count: 8);
            var view = vault.AsReadOnly();
            var expected = UnmanagedStringVaultCopyAssert.Collect(view);

            UnmanagedStringVaultCopyAssert.TryCopyToStartPastDestinationLength(view, expected);
        }

        [Test]
        public void TryCopyTo_ReadOnlyLengthPastDestination_ReturnsFalse()
        {
            using var vault = CreateVault(count: 8);

            UnmanagedStringVaultCopyAssert.TryCopyToLengthPastDestination(vault.AsReadOnly());
        }

        [Test]
        public void TryCopyTo_ReadOnlyInvalidRange_ReturnsFalse()
        {
            using var vault = CreateVault(count: 3);

            UnmanagedStringVaultCopyAssert.TryCopyToInvalidRange(vault.AsReadOnly(), vault.Count);
        }

        [Test]
        public void CopyTo_ReadOnlyInvalidRange_ThrowsArgumentOutOfRange()
        {
            using var vault = CreateVault(count: 3);
            var view = vault.AsReadOnly();

            UnmanagedStringVaultCopyAssert.CopyToInvalidRangeThrows(view, vault.Count);
            UnmanagedStringVaultCopyAssert.CopyToLengthPastDestinationThrows(view);
        }

        [Test]
        public void CopyTo_ReadOnlyEmptyVault_CopiesTheInvalidSlot()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);

            UnmanagedStringVaultCopyAssert.CopyToEmptyVault(vault.AsReadOnly());
        }

        [Test]
        public void TryCopyTo_ReadOnlyEmptyVault_ReturnsTrueAndCopiesTheInvalidSlot()
        {
            using var vault = new StringVaultNative(4, Allocator.Temp);

            UnmanagedStringVaultCopyAssert.TryCopyToEmptyVault(vault.AsReadOnly());
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void AsReadOnly_AfterOwnerDispose_MembersThrow()
        {
            // Persistent: a Temp vault shares the temp safety handle, which copies (enumerator,
            // read-only view) cannot see being invalidated by the owner's Dispose.
            var vault = new StringVaultNative(4, Allocator.Persistent);
            var text = (UnmanagedString)"x";
            var id = vault.GetOrMakeId(text);
            var view = vault.AsReadOnly();
            var destination = new UnmanagedString[4];

            vault.Dispose();

            Assert.Catch(() => _ = view.Capacity);
            Assert.Catch(() => _ = view.Count);
            Assert.Catch(() => _ = view.AllowEmptyString);
            Assert.Catch(() => _ = view[0]);
            Assert.Catch(() => view.TryGetId(text));
            Assert.Catch(() => view.TryGetId(text, out _));
            Assert.Catch(() => view.TryGetUnmanagedString(id));
            Assert.Catch(() => view.TryGetUnmanagedString(id, out _));
            Assert.Catch(() => view.ContainsId(id));
            Assert.Catch(() => view.CopyTo(destination));
            Assert.Catch(() => view.CopyTo(destination, 1));
            Assert.Catch(() => view.CopyTo(0, destination));
            Assert.Catch(() => view.CopyTo(0, destination, 1));
            Assert.Catch(() => view.TryCopyTo(destination));
            Assert.Catch(() => view.TryCopyTo(destination, 1));
            Assert.Catch(() => view.TryCopyTo(0, destination));
            Assert.Catch(() => view.TryCopyTo(0, destination, 1));
            Assert.Catch(() => view.GetEnumerator());
        }

        private static StringVaultNative CreateVault(int count)
        {
            var vault = new StringVaultNative(16, Allocator.Temp);

            for (var i = 0; i < count; i++)
            {
                vault.GetOrMakeId((UnmanagedString)$"s{i}");
            }

            return vault;
        }
    }
}
