using EncosyTower.Persistence.Generators;

namespace EncosyTower.SourceGen.Tests.Persistence.Persistences;

[TestClass]
public class PersistenceGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PersistenceGenerator>();

    [TestMethod]
    public Task GenericPersistence_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PersistenceGenerator>("""
            using EncosyTower.Persistences;

            namespace TestProject;

            [Persistence]
            internal static partial class SavePersistence<T> { }
            """);

    [TestMethod]
    public Task GenericAccessor_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PersistenceGenerator>("""
            using EncosyTower.Persistences;

            namespace TestProject;

            internal static class SavePersistence { }

            [PersistAccessor(typeof(SavePersistence))]
            internal sealed class SaveAccessor<T> : IPersistAccessor { }
            """);

    [TestMethod]
    public Task PersistenceWithoutAccessor_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PersistenceGenerator>(PERSISTENCE_SOURCE);

    [TestMethod]
    public Task PersistenceWithAccessor_GeneratesVaultAndAccessor()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<PersistenceGenerator>(
              PERSISTENCE_WITH_ACCESSOR_SOURCE
            , new[] {
                ExpectedGeneratedSource.Create<PersistenceGenerator>(
                    "SavePersistence.Persistence.9b029cf59d65a6bb.g.cs"
                ),
            }
            , verifyDebuggingAliasContract: true
        );

    [TestMethod]
    public Task MultipleAccessors_UseUnityTaskBridge()
        => GeneratorTestHelper.VerifyGeneratedSourceFragmentsAsync<PersistenceGenerator>(
              PERSISTENCE_WITH_MULTIPLE_ACCESSORS_SOURCE
            , new[] {
                  "using g__ETT = global::EncosyTower.Tasks;"
                , "g__SB.ArrayPool<g__ETT.UnityTask>"
                , "protected sealed override g__ETT.UnityTask<bool> OnTryLoadAsync("
                , "return g__ETT.UnityTask.FromResult(true);"
                , ": g__ETT.UnityTask.CompletedTask;"
                , "await g__ETT.UnityTask.WhenAll(tasks);"
              }
            , new[] {
                  "g__UnityTask"
                , "UnityTasks"
              }
        );

    [TestMethod]
    public async Task UniTaskReference_AddRemove_DoesNotInvalidateOutput()
    {
        var fakeUniTaskReference = await GeneratorTestHelper.CompileToReferenceAsync(
              "namespace Cysharp.Threading.Tasks { public readonly struct UniTask { } }"
            , "UniTask"
        );
        await GeneratorTestHelper.VerifyReferenceAddRemoveInvarianceAsync<PersistenceGenerator>(
              PERSISTENCE_WITH_ACCESSOR_SOURCE
            , fakeUniTaskReference
            , "PersistenceGenerator.Outputs"
        );
    }

    [TestMethod]
    public Task GeneratedDebuggingAlias_WithGenericTypeParameterName_Compiles()
        => GeneratorTestHelper.VerifyDebuggingAliasCollisionAsync();

    [TestMethod]
    public Task PersistAndPersistence_GenerateCleanCombinedOutput()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<PersistGenerator>(
              COMBINED_SOURCE
            , new[] {
                ExpectedGeneratedSource.Create<PersistGenerator>(
                    "SaveData.Persist.1642900d3d199011.g.cs"
                ),
                ExpectedGeneratedSource.Create<PersistenceGenerator>(
                    "SavePersistence.Persistence.9b029cf59d65a6bb.g.cs"
                ),
            }
            , new IIncrementalGenerator[] { new PersistenceGenerator() }
        );

    [TestMethod]
    public Task AccessorWithoutPersistence_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PersistenceGenerator>("""
            using EncosyTower.Persistences;

            namespace TestProject;

            internal static class SavePersistence { }

            [Persist]
            internal sealed class SaveData : IPersist
            {
                public string Id { get; set; } = "";
                public int Version { get; set; }
            }

            [PersistAccessor(typeof(SavePersistence))]
            internal sealed class SaveAccessor : IPersistAccessor
            {
                internal SaveAccessor(PersistStoreDefault<SaveData> store) { }
            }
            """);

    private const string PERSISTENCE_SOURCE = """
        using EncosyTower.Persistences;

        namespace TestProject;

        [Persistence]
        internal static partial class SavePersistence { }
        """;

    private const string PERSISTENCE_WITH_ACCESSOR_SOURCE = """
        using System;
        using EncosyTower.Persistences;

        namespace TestProject;

        [Persistence]
        internal static partial class SavePersistence
        {
            internal partial class PersistDirectory
            {
                private static partial PersistStoreArgs GetStoreArgs<TData, TStore>(Func<TData> createDataFunc)
                    where TData : IPersist
                    where TStore : PersistStoreBase<TData>
                    => default;
            }
        }

        internal sealed class SaveData : IPersist
        {
            public string Id { get; set; } = "";
            public int Version { get; set; }
        }

        [PersistAccessor(typeof(SavePersistence))]
        internal sealed class SaveAccessor : IPersistAccessor
        {
            internal SaveAccessor(PersistStoreDefault<SaveData> store) { }
        }
        """;

    private const string PERSISTENCE_WITH_MULTIPLE_ACCESSORS_SOURCE = """
        using System;
        using EncosyTower.Persistences;

        namespace TestProject;

        [Persistence]
        internal static partial class SavePersistence
        {
            internal partial class PersistDirectory
            {
                private static partial PersistStoreArgs GetStoreArgs<TData, TStore>(Func<TData> createDataFunc)
                    where TData : IPersist
                    where TStore : PersistStoreBase<TData>
                    => default;
            }
        }

        internal sealed class SaveData : IPersist
        {
            public string Id { get; set; } = "";
            public int Version { get; set; }
        }

        internal sealed class SettingsData : IPersist
        {
            public string Id { get; set; } = "";
            public int Version { get; set; }
        }

        [PersistAccessor(typeof(SavePersistence))]
        internal sealed class SaveAccessor : IPersistAccessor
        {
            internal SaveAccessor(PersistStoreDefault<SaveData> store) { }
        }

        [PersistAccessor(typeof(SavePersistence))]
        internal sealed class SettingsAccessor : IPersistAccessor
        {
            internal SettingsAccessor(PersistStoreDefault<SettingsData> store) { }
        }
        """;

    private const string COMBINED_SOURCE = """
        using System;
        using EncosyTower.Persistences;

        namespace TestProject;

        [Persistence]
        internal static partial class SavePersistence
        {
            internal partial class PersistDirectory
            {
                private static partial PersistStoreArgs GetStoreArgs<TData, TStore>(Func<TData> createDataFunc)
                    where TData : IPersist
                    where TStore : PersistStoreBase<TData>
                    => default;
            }
        }

        [Persist]
        internal partial class SaveData { }

        [PersistAccessor(typeof(SavePersistence))]
        internal sealed class SaveAccessor : IPersistAccessor
        {
            internal SaveAccessor(PersistStoreDefault<SaveData> store) { }
        }
        """;
}
