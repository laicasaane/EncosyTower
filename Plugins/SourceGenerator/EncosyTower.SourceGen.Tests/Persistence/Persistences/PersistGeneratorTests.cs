using System.Runtime.CompilerServices;
using EncosyTower.Persistence.Generators;

namespace EncosyTower.SourceGen.Tests.Persistence.Persistences;

[TestClass]
public class PersistGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PersistGenerator>();

    [TestMethod]
    public Task PartialClass_GeneratesPersistMembers()
        => VerifyGeneratedAsync("""
            [Persist]
            public partial class PlayerData { }
            """, "PlayerData.Persist.ac287e605579fe1f.g.cs");

    [TestMethod]
    public Task PartialStruct_GeneratesPersistMembers()
        => VerifyGeneratedAsync("""
            [Persist]
            public partial struct PlayerData { }
            """, "PlayerData.Persist.ac287e605579fe1f.g.cs");

    [TestMethod]
    public Task PartialRecordClass_GeneratesPersistMembers()
        => VerifyGeneratedAsync("""
            [Persist]
            public partial record class PlayerData { }
            """, "PlayerData.Persist.ac287e605579fe1f.g.cs");

    [TestMethod]
    public Task PartialRecordStruct_GeneratesPersistMembers()
        => VerifyGeneratedAsync("""
            [Persist]
            public partial record struct PlayerData { }
            """, "PlayerData.Persist.ac287e605579fe1f.g.cs");

    [TestMethod]
    public Task ExistingId_GeneratesMissingVersion()
        => VerifyGeneratedAsync("""
            [Persist]
            public partial class PlayerData
            {
                public string Id { get; set; } = "";
            }
            """, "PlayerData.Persist.ac287e605579fe1f.g.cs");

    [TestMethod]
    public Task DirectIdWithAbstractBaseVersion_GeneratesVersionOverride()
        => VerifyGeneratedAsync("""
            public abstract class BaseData
            {
                public string Id { get; set; } = "";
                public abstract int Version { get; set; }
            }

            [Persist]
            public partial class PlayerData : BaseData
            {
                public new string Id { get; set; } = "";
            }
            """, "PlayerData.Persist.ac287e605579fe1f.g.cs");

    [TestMethod]
    public Task ConcreteBaseMembers_GeneratesInterfaceOnly()
        => VerifyGeneratedAsync("""
            public class BaseData
            {
                public string Id { get; set; } = "";
                public int Version { get; set; }
            }

            [Persist]
            public partial class PlayerData : BaseData { }
            """, "PlayerData.Persist.ac287e605579fe1f.g.cs");

    [TestMethod]
    public Task AbstractBaseMembers_GeneratesOverrides()
        => VerifyGeneratedAsync("""
            public abstract class BaseData
            {
                public abstract string Id { get; set; }
                public abstract int Version { get; set; }
            }

            [Persist]
            public partial class PlayerData : BaseData { }
            """, "PlayerData.Persist.ac287e605579fe1f.g.cs");

    [TestMethod]
    public Task BaseIdOnly_GeneratesMissingVersion()
        => VerifyGeneratedAsync("""
            public class BaseData
            {
                public string Id { get; set; } = "";
            }

            [Persist]
            public partial class PlayerData : BaseData { }
            """, "PlayerData.Persist.ac287e605579fe1f.g.cs");

    [TestMethod]
    public Task ExistingInterfaceAndMembers_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PersistGenerator>(Wrap("""
            [Persist]
            public partial class PlayerData : IPersist
            {
                public string Id { get; set; } = "";
                public int Version { get; set; }
            }
            """));

    [TestMethod]
    public Task AbstractTarget_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PersistGenerator>(Wrap("""
            [Persist]
            public abstract partial class PlayerData { }
            """));

    [TestMethod]
    public Task NestedTarget_GeneratesPersistMembers()
        => VerifyGeneratedAsync("""
            public partial class Outer
            {
                [Persist]
                public partial class PlayerData { }
            }
            """, "PlayerData.Persist.cf6a84db26967217.g.cs");

    [TestMethod]
    public Task GenericContainingTypeParameterMatchingAlias_GeneratesPersistMembers()
        => VerifyGeneratedAsync("""
            public partial class Outer<SCDC>
            {
                [Persist]
                public partial class PlayerData { }
            }
            """, "PlayerData.Persist.2fcae2e2e2ba9a22.g.cs");

    [DataTestMethod]
    [DataRow("GeneratedBaseMembers")]
    [DataRow("HandWrittenBaseMembers")]
    [DataRow("FieldBackedBaseMembers")]
    public Task PersistBase_DerivedReusesBaseMembers(string baseMembersCase)
    {
        var baseMembers = baseMembersCase switch {
            "HandWrittenBaseMembers" => "public string Id { get; set; } = \"\";\n    public int Version { get; set; }",
            "FieldBackedBaseMembers" => "protected string _id = \"\";\n    protected int _version = 1;",
            _ => string.Empty,
        };

        return GeneratorTestHelper.VerifyGeneratedSourcesAsync<PersistGenerator>(
              Wrap($$"""
                  [Persist]
                  public partial class BaseData
                  {
                      {{baseMembers}}
                  }

                  [Persist]
                  public partial class PlayerData : BaseData { }
                  """)
            , new[] {
                ExpectedGeneratedSource.Create<PersistGenerator>("PlayerData.Persist.ac287e605579fe1f.g.cs"),
                ExpectedGeneratedSource.Create<PersistGenerator>(
                      "BaseData.Persist.4ac35334e9e0fe02.g.cs"
                    , caseName: baseMembersCase
                ),
            }
        );
    }

    [TestMethod]
    public Task PersistMarkerAddedToBase_RemovesDerivedMembers()
        => GeneratorTestHelper.VerifyCrossFileEditAsync(
              new IIncrementalGenerator[] { new PersistGenerator() }
            , new[] {
                new NamedSource("BaseData.cs", Wrap("public partial class BaseData { }")),
                new NamedSource("PlayerData.cs", Wrap("[Persist] public partial class PlayerData : BaseData { }")),
            }
            , new[] {
                new NamedSource("BaseData.cs", Wrap("[Persist] public partial class BaseData { }")),
                new NamedSource("PlayerData.cs", Wrap("[Persist] public partial class PlayerData : BaseData { }")),
            }
            , new[] {
                "BaseData.Persist.4ac35334e9e0fe02.g.cs",
                "PlayerData.Persist.ac287e605579fe1f.g.cs",
            }
        );

    private static Task VerifyGeneratedAsync(
          string declaration
        , string hintName
        , [CallerMemberName] string testMethod = ""
        , [CallerFilePath] string testFile = ""
    )
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<PersistGenerator>(
              Wrap(declaration)
            , new[] {
                ExpectedGeneratedSource.Create<PersistGenerator>(hintName, testMethod: testMethod, testFile: testFile),
            }
        );

    private static string Wrap(string declaration)
        => $$"""
            using EncosyTower.Persistences;

            namespace TestProject;

            {{declaration}}
            """;
}
