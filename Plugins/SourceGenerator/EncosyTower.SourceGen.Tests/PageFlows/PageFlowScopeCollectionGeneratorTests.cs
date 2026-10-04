using EncosyTower.PageFlows.Generators;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Tests.PageFlows;

[TestClass]
public sealed class PageFlowScopeCollectionGeneratorTests
{
    private const string BASIC_SOURCE = """
        using EncosyTower.PageFlows;

        namespace TestProject;

        [PageFlowScopeCollection]
        public partial struct GamePageFlowScopes
        {
            public PageFlowScope Screen { get; private set; }

            public PageFlowScope Popup { get; private set; }
        }
        """;

    [TestMethod]
    public async Task Basic_MatchesSnapshotAndHintName()
    {
        var run = await PageFlowsRuntimeFixture.RunAsync(BASIC_SOURCE);
        var source = run.Sources.Single();
        var expectedHintName = SourceGenHelpers.BuildSemanticHintName(
              PageFlowScopeCollectionSourceGenContract.GENERATOR_METADATA_NAME
            , "EncosyTower.SourceGen.Tests.PageFlows.Input"
            , "TestProject.GamePageFlowScopes"
            , PageFlowScopeCollectionSourceGenContract.OUTPUT_ROLE
            , string.Empty
        );

        var mismatch = await GeneratedSourceSnapshot.VerifyAsync(
              source.SourceText.ToString()
            , Path.Combine(GetSnapshotDirectory(), "PageFlowScopeCollectionGeneratorTests.Basic.verified.cs")
        );

        Assert.AreEqual(expectedHintName, source.HintName);
        Assert.IsNull(mismatch, mismatch);
        PageFlowsRuntimeFixture.AssertNoOutputErrors(run);
    }

    [TestMethod]
    public async Task GeneratedMembers_SetScopesAndListIdentifiers()
    {
        var run = await PageFlowsRuntimeFixture.RunAsync(BASIC_SOURCE + """

            public static class Probe
            {
                public static string Run()
                {
                    var scopes = new GamePageFlowScopes();
                    var setScreen = scopes.TrySetScope("Screen", new PageFlowScope(7));
                    var setUnknown = scopes.TrySetScope("Nope", new PageFlowScope(9));
                    var identifiers = string.Join(",", scopes.ScopeIdentifiers.ToArray());
                    return $"{setScreen}|{setUnknown}|{scopes.Screen.Value}|{scopes.Popup.Value}|{identifiers}";
                }
            }
            """);

        PageFlowsRuntimeFixture.AssertNoOutputErrors(run);

        var result = PageFlowsRuntimeFixture.InvokeStatic(run, "TestProject.Probe", "Run");

        Assert.AreEqual("True|False|7|0|Screen,Popup", result);
    }

    [TestMethod]
    public async Task NestedGenericKeywordAndSetterKinds_Compile()
    {
        var run = await PageFlowsRuntimeFixture.RunAsync("""
            using EncosyTower.PageFlows;

            namespace TestProject;

            public partial class Outer
            {
                [PageFlowScopeCollection]
                public partial struct NestedScopes
                {
                    public PageFlowScope Public { get; set; }

                    public PageFlowScope Private { get; private set; }
                }
            }

            [PageFlowScopeCollection]
            public partial struct GenericScopes<T>
            {
                public PageFlowScope Screen { get; private set; }
            }

            [PageFlowScopeCollection]
            public partial struct KeywordScopes
            {
                public PageFlowScope @class { get; private set; }
            }

            public static class Probe
            {
                public static string Run()
                {
                    var keyword = new KeywordScopes();
                    var set = keyword.TrySetScope("class", new PageFlowScope(3));
                    var generic = new GenericScopes<int>();
                    var setGeneric = generic.TrySetScope("Screen", new PageFlowScope(4));
                    var nested = new Outer.NestedScopes();
                    var setNested = nested.TrySetScope("Private", new PageFlowScope(5));
                    return $"{set}:{keyword.@class.Value}|{setGeneric}:{generic.Screen.Value}"
                        + $"|{setNested}:{nested.Private.Value}";
                }
            }
            """);

        PageFlowsRuntimeFixture.AssertNoOutputErrors(run);
        Assert.AreEqual(3, run.Sources.Count);
        Assert.AreEqual(3, run.Sources.Select(static source => source.HintName).Distinct().Count());
        Assert.AreEqual("True:3|True:4|True:5", PageFlowsRuntimeFixture.InvokeStatic(run, "TestProject.Probe", "Run"));
    }

    [TestMethod]
    public async Task IneligibleMembers_AreExcludedFromIdentifiers()
    {
        var run = await PageFlowsRuntimeFixture.RunAsync("""
            using EncosyTower.PageFlows;

            namespace TestProject;

            public interface IHasScope
            {
                PageFlowScope Explicit { get; set; }
            }

            [PageFlowScopeCollection]
            public partial struct Scopes : IHasScope
            {
                public PageFlowScope Field;

                public static PageFlowScope Static { get; set; }

                public PageFlowScope GetOnly { get; }

                public PageFlowScope InitOnly { get; init; }

                public PageFlowScope this[int index] { get => default; set { } }

                PageFlowScope IHasScope.Explicit { get; set; }

                public int Other { get; set; }

                public PageFlowScope Eligible { get; set; }
            }
            """);

        PageFlowsRuntimeFixture.AssertNoOutputErrors(run);
        CollectionAssert.AreEqual(new[] { "Eligible" }, GetIdentifierArguments(run.Sources.Single()));
    }

    [TestMethod]
    [DataRow("public partial struct Scopes", DisplayName = "MissingAttribute")]
    [DataRow("[PageFlowScopeCollection] public struct Scopes", DisplayName = "NonPartialStruct")]
    [DataRow("[PageFlowScopeCollection] public readonly partial struct Scopes", DisplayName = "ReadOnlyStruct")]
    [DataRow("[PageFlowScopeCollection] public partial struct Empty", DisplayName = "NoEligibleProperty")]
    public async Task InvalidDeclaration_ProducesNoOutput(string declaration)
    {
        var body = declaration.EndsWith("Empty", StringComparison.Ordinal)
            ? "{ public PageFlowScope GetOnly { get; } }"
            : "{ public PageFlowScope Screen { get; set; } }";

        var run = await PageFlowsRuntimeFixture.RunAsync($$"""
            using EncosyTower.PageFlows;

            namespace TestProject;

            {{declaration}}
            {{body}}
            """);

        Assert.AreEqual(0, run.Sources.Count);
    }

    [TestMethod]
    public async Task NonPartialContainingType_ProducesNoOutput()
    {
        var run = await PageFlowsRuntimeFixture.RunAsync("""
            using EncosyTower.PageFlows;

            namespace TestProject;

            public class Outer
            {
                [PageFlowScopeCollection]
                public partial struct Scopes
                {
                    public PageFlowScope Screen { get; set; }
                }
            }
            """);

        Assert.AreEqual(0, run.Sources.Count);
    }

    [TestMethod]
    public async Task SkipAttributeOnAssembly_ProducesNoOutput()
    {
        var run = await PageFlowsRuntimeFixture.RunAsync("""
            using EncosyTower.PageFlows;

            [assembly: SkipSourceGeneratorsForAssembly]

            namespace TestProject;

            [PageFlowScopeCollection]
            public partial struct Scopes
            {
                public PageFlowScope Screen { get; set; }
            }
            """);

        Assert.AreEqual(0, run.Sources.Count);
    }

    [TestMethod]
    public async Task AttributeOnSecondPartialDeclaration_ProducesOneOutput()
    {
        var run = await PageFlowsRuntimeFixture.RunAsync(
              [
                  new NamedSource("A.cs", """
                      namespace TestProject;

                      public partial struct Scopes
                      {
                          public EncosyTower.PageFlows.PageFlowScope Screen { get; set; }
                      }
                      """),
                  new NamedSource("B.cs", """
                      namespace TestProject;

                      [EncosyTower.PageFlows.PageFlowScopeCollection]
                      public partial struct Scopes
                      {
                          public EncosyTower.PageFlows.PageFlowScope Popup { get; set; }
                      }
                      """),
              ]
            , previous: null
        );

        PageFlowsRuntimeFixture.AssertNoOutputErrors(run);
        CollectionAssert.AreEqual(new[] { "Screen", "Popup" }, GetIdentifierArguments(run.Sources.Single()));
    }

    private static string[] GetIdentifierArguments(GeneratedSourceResult source)
        => source.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Single(static declarator => declarator.Identifier.ValueText == "s_scopeIdentifiers")
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(static invocation => invocation.ArgumentList.Arguments.Single().ToString())
            .ToArray();

    private static string GetSnapshotDirectory()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory != null;
            directory = directory.Parent
        )
        {
            if (File.Exists(Path.Combine(directory.FullName, "EncosyTower.SourceGen.slnx")))
            {
                return Path.Combine(directory.FullName, "EncosyTower.SourceGen.Tests", "PageFlows", "Snapshots");
            }
        }

        throw new DirectoryNotFoundException("Could not locate EncosyTower.SourceGen.slnx.");
    }
}
