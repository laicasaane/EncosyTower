using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using EncosyTower.Encryption;
using EncosyTower.IO;
using EncosyTower.Logging;
using EncosyTower.Persistences;
using EncosyTower.StringIds;
using NUnit.Framework;

namespace EncosyTower.Tests.Persistence
{
    public sealed class PersistStoreDefaultTests
    {
        private const string FILE_EXTENSION = "txt";
        private const string KEY = "persist-store-default-tests";
        private const string STORE_ID = "player-one";

        [Test]
        public async Task InitializeDirtySaveAndLoad_RoundTripsThroughLocalSource()
        {
            var temporaryDirectory = CreateTemporaryDirectory();

            try
            {
                using var context = CreateContext(temporaryDirectory);
                var store = context.Store;

                store.Id = STORE_ID;
                store.Initialize();

                Assert.IsTrue(store.IsInitialized);
                Assert.AreEqual(
                    Path.Combine(temporaryDirectory, STORE_ID, $"{KEY}.{FILE_EXTENSION}"),
                    store.FilePath
                );
                Assert.IsFalse(store.IsDataDirty);

                store.CreateData();
                var saved = store.Data;
                saved.Version = 7;
                saved.Name = "Ada";
                saved.Score = 42;
                store.MarkDirty();

                Assert.IsTrue(store.IsDataDirty);

                await store.SaveAsync(SaveDestination.Local);

                Assert.IsFalse(store.IsDataDirty);
                Assert.IsTrue(File.Exists(store.FilePath));

                store.SetData(new TestData {
                    Id = "replacement",
                    Version = -1,
                    Name = "Changed",
                    Score = -1,
                });
                store.MarkDirty(false);

                await store.LoadAsync(SourcePriority.OnlyLocal);

                Assert.AreNotSame(saved, store.Data);
                Assert.AreEqual(STORE_ID, store.Data.Id);
                Assert.AreEqual(7, store.Data.Version);
                Assert.AreEqual("Ada", store.Data.Name);
                Assert.AreEqual(42, store.Data.Score);
                Assert.IsFalse(store.IsDataDirty);
            }
            finally
            {
                DeleteTemporaryDirectory(temporaryDirectory);
            }
        }

        [Test]
        public void TryCloneData_ReturnsIndependentEquivalentReference()
        {
            var temporaryDirectory = CreateTemporaryDirectory();

            try
            {
                using var context = CreateContext(temporaryDirectory);
                var store = context.Store;
                var source = new TestData {
                    Id = STORE_ID,
                    Version = 3,
                    Name = "Grace",
                    Score = 99,
                };

                store.Id = STORE_ID;
                store.Initialize();
                store.SetData(source);

                var cloneOption = store.TryCloneData(SourcePriority.OnlyLocal);

                Assert.IsTrue(cloneOption.TryGetValue(out var clone));
                Assert.AreNotSame(source, clone);
                Assert.AreEqual(source.Id, clone.Id);
                Assert.AreEqual(source.Version, clone.Version);
                Assert.AreEqual(source.Name, clone.Name);
                Assert.AreEqual(source.Score, clone.Score);

                clone.Name = "Independent";
                clone.Score = 1;

                Assert.AreEqual("Grace", source.Name);
                Assert.AreEqual(99, source.Score);
            }
            finally
            {
                DeleteTemporaryDirectory(temporaryDirectory);
            }
        }

        private static TestContext CreateContext(string temporaryDirectory)
        {
            var vault = new StringVault(4);
            var logger = new StringBuilderLogger();
            var encryption = new TestEncryption(logger);
            var sourceArgs = new PersistSourceLocal<TestData>.Args(
                  new RootPath(temporaryDirectory)
                , Serialize
                , Deserialize
                , FILE_EXTENSION
            );
            var storeArgs = new PersistStoreDefault<TestData>.Args(static () => new TestData(), sourceArgs);
            var store = new PersistStoreDefault<TestData>(
                  vault.GetOrMakeId(KEY)
                , vault
                , encryption
                , logger
                , ignoreEncryption: true
                , storeArgs
            );

            return new TestContext(store, vault, encryption);
        }

        private static bool Serialize(TestData data, out string text)
        {
            text = string.Join(
                  "|"
                , data.Id
                , data.Version.ToString(CultureInfo.InvariantCulture)
                , data.Name
                , data.Score.ToString(CultureInfo.InvariantCulture)
            );
            return true;
        }

        private static bool Deserialize(string text, out TestData data)
        {
            var parts = text.Split('|');

            if (parts.Length == 4
                && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var version)
                && int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var score))
            {
                data = new TestData {
                    Id = parts[0],
                    Version = version,
                    Name = parts[2],
                    Score = score,
                };
                return true;
            }

            data = null;
            return false;
        }

        private static string CreateTemporaryDirectory()
        {
            var root = GetTemporaryRoot();
            return Path.GetFullPath(Path.Combine(root, Guid.NewGuid().ToString("N")));
        }

        private static string GetTemporaryRoot()
            => Path.GetFullPath(Path.Combine(
                  Path.GetTempPath()
                , "EncosyTower.Tests"
                , nameof(PersistStoreDefaultTests)
            ));

        private static void DeleteTemporaryDirectory(string temporaryDirectory)
        {
            var root = GetTemporaryRoot();
            var resolved = Path.GetFullPath(temporaryDirectory);
            var rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (resolved.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase) == false)
            {
                Assert.Fail($"Refusing to delete test directory outside '{root}'.");
            }

            if (Directory.Exists(resolved))
            {
                Directory.Delete(resolved, recursive: true);
            }
        }

        private sealed class TestContext : IDisposable
        {
            private readonly StringVault _vault;
            private readonly EncryptionBase _encryption;

            public TestContext(
                  PersistStoreDefault<TestData> store
                , StringVault vault
                , EncryptionBase encryption
            )
            {
                Store = store;
                _vault = vault;
                _encryption = encryption;
            }

            public PersistStoreDefault<TestData> Store { get; }

            public void Dispose()
            {
                _encryption.Dispose();
                _vault.Dispose();
            }
        }

        private sealed class TestEncryption : EncryptionBase
        {
            public TestEncryption(ILogger logger) : base(logger)
            {
            }

            public override string Encrypt(string plain)
                => plain ?? string.Empty;

            public override string Decrypt(string encrypted)
                => encrypted ?? string.Empty;
        }

        private sealed class TestData : IPersist
        {
            public string Id { get; set; }

            public int Version { get; set; }

            public string Name { get; set; }

            public int Score { get; set; }
        }
    }
}
