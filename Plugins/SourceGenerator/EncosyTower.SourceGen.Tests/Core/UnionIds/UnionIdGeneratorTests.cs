using EncosyTower.Core.Generators.UnionIds;

namespace EncosyTower.SourceGen.Tests.Core.UnionIds;

[TestClass]
public class UnionIdGeneratorTests
{
    private const string NESTED_UNION_ID_SOURCE = """
        using System;
        using EncosyTower.UnionIds;

        namespace TestProject;

        public enum Kind : byte
        {
            None,
        }

        public readonly struct Code : IEquatable<Code>
        {
            public readonly int Value;

            public bool Equals(Code other) => Value == other.Value;

            public override bool Equals(object obj) => obj is Code other && Equals(other);

            public override int GetHashCode() => Value;

            public override string ToString() => Value.ToString();
        }

        public partial class Outer
        {
            [UnionId]
            [UnionIdKind(typeof(Kind), 0)]
            [UnionIdKind(typeof(Code), 1)]
            public readonly partial struct Id
            {
                private static partial bool TryParse_Code(
                      ReadOnlySpan<char> str
                    , out Code value
                    , bool ignoreCase
                    , bool allowMatchingMetadataAttribute
                )
                {
                    value = default;
                    return false;
                }

                private static partial void Append_Code(
                      ref Unity.Collections.FixedString32Bytes fs
                    , Code value
                    , bool isDisplay
                )
                { }
            }
        }

        internal static class Usage
        {
            public static string KindName(Outer.Id id)
                => Outer.Id_IdKindExtensions.ToStringFast(id.Kind);
        }
        """;

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

    [TestMethod]
    public Task NestedUnionId_GeneratesUnionId()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<UnionIdGenerator>(
              NESTED_UNION_ID_SOURCE
            , new[] {
                ExpectedGeneratedSource.Create<UnionIdGenerator>("Id.UnionId.10e94c22284eb5a1.g.cs"),
            }
        );

    [TestMethod]
    public Task PrivateNestedUnionId_CallsKindHelperStatically()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<UnionIdGenerator>(
              """
              using EncosyTower.UnionIds;

              namespace TestProject;

              public partial struct Outer
              {
                  public enum Kind : byte
                  {
                      None,
                      Some,
                  }

                  [UnionId]
                  [UnionIdKind(typeof(Kind), 0)]
                  private readonly partial struct Id { }
              }
              """
            , expectedSourceCount: 1
            , expectedFragments: new[] {
                "private static partial class Id_IdKindExtensions",
                "TryFormat(Id_IdKindExtensions.ToFixedString(Kind), destination, out var kindCharsWritten)",
                "g__UC.FixedStringMethods.Append(ref fs, Id_IdKindExtensions.ToFixedString(Kind, false));",
            }
            , unexpectedFragments: new[] {
                "Kind.ToFixedString(",
                "Kind.ToUnderlyingValue(",
            }
        );

    [TestMethod]
    public async Task NestedUnionIdWithoutUnityCollections_Compiles()
    {
        var references = (await TestReferenceHelper.ResolveCompilationReferencesAsync())
            .RemoveAll(static reference => string.Equals(
                  Path.GetFileNameWithoutExtension(reference.Display)
                , "Unity.Collections"
                , StringComparison.OrdinalIgnoreCase
            ));
        var compilation = GeneratorTestHelper.CreateCompilation(
              """
              using EncosyTower.UnionIds;

              namespace TestProject;

              public enum Kind : byte
              {
                  None,
              }

              public partial class Outer
              {
                  [UnionId]
                  [UnionIdKind(typeof(Kind), 0)]
                  public readonly partial struct Id { }
              }
              """
            , "EncosyTower.SourceGen.Tests.Input"
            , references
        );
        var generators = new IIncrementalGenerator[] { new UnionIdGenerator() };
        var run = GeneratorTestHelper.RunDriver(GeneratorTestHelper.CreateDriver(generators), compilation, default);
        var source = run.Result.Results.Single().GeneratedSources.Single().SourceText.ToString();

        StringAssert.Contains(source, "g__S.MemoryExtensions.AsSpan(Id_IdKindExtensions.ToStringFast(Kind));");
    }

    [TestMethod]
    public Task GlobalNamespaceUnionId_GeneratesKindExtensionMethods()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<UnionIdGenerator>(
              """
              using EncosyTower.UnionIds;

              public enum Kind : byte
              {
                  None,
              }

              [UnionId]
              [UnionIdKind(typeof(Kind), 0)]
              public readonly partial struct Id { }

              internal static class Usage
              {
                  public static string KindName(Id id)
                      => id.Kind.ToStringFast();
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<UnionIdGenerator>("global__Id.UnionId.b440d6bd379cb8dd.g.cs"),
            }
        );
}
