using EncosyTower.Core.Generators.UnionIds;

namespace EncosyTower.SourceGen.Tests.Core.UnionIds;

[TestClass]
public class UnionIdGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<UnionIdGenerator>();

    [TestMethod]
    public Task UnionIdWithKind_GeneratesUnionId()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<UnionIdGenerator>(
              """
              using EncosyTower.UnionIds;

              namespace TestProject;

              public enum Kind : byte
              {
                  None,
              }

              [UnionId]
              [UnionIdKind(typeof(Kind), 0)]
              public readonly partial struct Id { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<UnionIdGenerator>("Id.UnionId.ed6754814f13fee9.g.cs"),
            }
            , verifyDebuggingAliasContract: true
        );

    [TestMethod]
    public Task CustomKindName_PreservesAuthoredNormalization()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<UnionIdGenerator>(
              """
              using EncosyTower.UnionIds;

              namespace TestProject;

              public enum Kind : byte
              {
                  None,
              }

              [UnionId]
              [UnionIdKind(typeof(Kind), 0, "A-B")]
              public readonly partial struct Id { }
              """
            , expectedSourceCount: 1
            , expectedFragments: new[] {
                "A__B = 0,",
            }
            , unexpectedFragments: new[] {
                "I_A_x002DB",
            }
        );

    [TestMethod]
    public Task KeywordKind_EscapesOnlyWholeIdentifiers()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<UnionIdGenerator>(
              """
              using EncosyTower.UnionIds;

              namespace TestProject;

              public enum @class : byte
              {
                  None,
              }

              [UnionId]
              [UnionIdKind(typeof(@class), 0)]
              public readonly partial struct Id { }
              """
            , expectedSourceCount: 1
            , expectedFragments: new[] {
                "public readonly global::TestProject.@class Id_class;",
                "Kind = IdKind.@class;",
                "@class = 0,",
            }
            , unexpectedFragments: new[] {
                "Id_@class",
            }
        );

    [TestMethod]
    public Task GeneratedDebuggingAlias_WithGenericTypeParameterName_Compiles()
        => GeneratorTestHelper.VerifyDebuggingAliasCollisionAsync();
}
