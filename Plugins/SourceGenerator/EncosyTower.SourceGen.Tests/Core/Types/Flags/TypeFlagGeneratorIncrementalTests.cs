using EncosyTower.Core.Generators.Types.Flags;
using Microsoft.CodeAnalysis.CSharp;

namespace EncosyTower.SourceGen.Tests.Core.Types.Flags;

[TestClass]
public sealed class TypeFlagGeneratorIncrementalTests
{
    private const string VALID_SPECS = "TypeFlagGenerator.ValidSpecs";
    private const string OUTPUTS = "TypeFlagGenerator.Outputs";

    private const string NESTED_OWNER = """
        using EncosyTower.Types;

        namespace TestProject
        {
            {{EDIT}}
            {
                [TypeFlag]
                public partial class Owner { }
            }
        }
        """;

    private const string NAMESPACED_OWNER = """
        using EncosyTower.Types;

        {{EDIT}}
        {
            [TypeFlag]
            public partial class Owner { }
        }
        """;

    private const string MARKED_OWNER = """
        using EncosyTower.Types;

        namespace TestProject
        {
            {{EDIT}}
            public partial class Owner { }
        }
        """;

    private const string OWNER = """
        using EncosyTower.Types;

        namespace TestProject
        {
            [TypeFlag]
            public partial class Owner { }
        }
        """;

    private const string OTHER = """
        namespace TestProject
        {
            public class Other { }
        }
        """;

    private const string OTHER_EDITED = """
        namespace TestProject
        {
            public class Other
            {
                public int Value;
            }
        }
        """;

    private const string HIERARCHY = """
        using EncosyTower.Types;

        namespace TestProject
        {
            {{MARKER}}
            public partial class Base { }

            [TypeFlag]
            public partial class Derived : Base { }
        }
        """;

    private const string MARKER = "{{MARKER}}";

    public static IEnumerable<object[]> EditCases
    {
        get
        {
            yield return new object[] {
                  "containing type kind"
                , NESTED_OWNER
                , ReusedDriverEditCase.CLASS_OUTER
                , ReusedDriverEditCase.STRUCT_OUTER
            };

            yield return new object[] {
                  "containing type parameter name"
                , NESTED_OWNER
                , "public partial class Outer<T>"
                , "public partial class Outer<TItem>"
            };

            yield return new object[] {
                  "namespace"
                , NAMESPACED_OWNER
                , ReusedDriverEditCase.TEST_NAMESPACE
                , ReusedDriverEditCase.OTHER_NAMESPACE
            };

            yield return new object[] {
                  "write access"
                , MARKED_OWNER
                , "[TypeFlag]"
                , "[TypeFlag(WriteAccess = TypeFlagAccess.Public)]"
            };

            yield return new object[] {
                  "use extensions"
                , MARKED_OWNER
                , "[TypeFlag]"
                , "[TypeFlag(UseExtensions = true)]"
            };
        }
    }

    [TestMethod]
    public async Task IdenticalRerunAndUnrelatedEdit_KeepOutputsCached()
    {
        var owner = new NamedSource("Owner.cs", OWNER);
        var other = new NamedSource("Other.cs", OTHER);
        var first = await TypeFlagTestFixture.RunAsync([owner, other]);
        var rerun = await TypeFlagTestFixture.RunAsync([owner, other], first);

        AssertStable(rerun, VALID_SPECS);
        AssertStable(rerun, OUTPUTS);
        CollectionAssert.AreEqual(GetSources(first), GetSources(rerun));

        var edited = await TypeFlagTestFixture.RunAsync([owner, other with { Source = OTHER_EDITED }], rerun);

        AssertStable(edited, VALID_SPECS);
        AssertStable(edited, OUTPUTS);
        CollectionAssert.AreEqual(GetSources(first), GetSources(edited));
    }

    [TestMethod]
    public async Task SwappedFileOrder_KeepsHintsAndText()
    {
        var owner = new NamedSource("Owner.cs", OWNER);
        var other = new NamedSource("Other.cs", OTHER);
        var first = await TypeFlagTestFixture.RunAsync([owner, other]);
        var swapped = await TypeFlagTestFixture.RunAsync([other, owner], first);

        CollectionAssert.AreEqual(GetSources(first), GetSources(swapped));
    }

    [TestMethod]
    public async Task IgnoredApiEdit_KeepsOutputsCached()
    {
        var first = await TypeFlagTestFixture.RunAsync([
            new NamedSource("Owner.cs", MARKED_OWNER.Replace(
                  ReusedDriverEditCase.EDIT
                , "[TypeFlag(UseExtensions = true, Api = TypeFlagApi.State)]"
                , StringComparison.Ordinal
            )),
        ]);

        var edited = await TypeFlagTestFixture.RunAsync(
              [new NamedSource("Owner.cs", MARKED_OWNER.Replace(
                    ReusedDriverEditCase.EDIT
                  , "[TypeFlag(UseExtensions = true, Api = TypeFlagApi.Self)]"
                  , StringComparison.Ordinal
              ))]
            , first
        );

        AssertStable(edited, OUTPUTS);
        CollectionAssert.AreEqual(GetSources(first), GetSources(edited));
    }

    [TestMethod]
    public async Task EditSequence_MatchesFreshRun()
    {
        var first = await TypeFlagTestFixture.RunAsync([new NamedSource("Owner.cs", OWNER)]);
        var oldHint = TypeFlagGeneratorTests.GetHintName("TestProject.Owner");

        Assert.IsTrue(HasHint(first, oldHint));

        var renamed = await TypeFlagTestFixture.RunAsync(
              [new NamedSource("Owner.cs", OWNER.Replace("class Owner", "class Renamed", StringComparison.Ordinal))]
            , first
        );

        Assert.IsFalse(HasHint(renamed, oldHint));
        Assert.IsTrue(HasHint(renamed, TypeFlagGeneratorTests.GetHintName("TestProject.Renamed")));

        var unmarked = await TypeFlagTestFixture.RunAsync(
              [new NamedSource("Owner.cs", OWNER.Replace("[TypeFlag]", string.Empty, StringComparison.Ordinal))]
            , renamed
        );

        Assert.AreEqual(0, unmarked.Result.GeneratedSources.Length);

        var remarked = await TypeFlagTestFixture.RunAsync([new NamedSource("Owner.cs", OWNER)], unmarked);

        Assert.IsTrue(HasHint(remarked, oldHint));

        var taken = await TypeFlagTestFixture.RunAsync(
              [new NamedSource(
                    "Owner.cs"
                  , OWNER.Replace(
                        "public partial class Owner { }"
                      , "public partial class Owner { public struct TypeFlagAPI { } }"
                      , StringComparison.Ordinal
                  )
              )]
            , remarked
        );

        Assert.AreEqual(0, taken.Result.GeneratedSources.Length);

        var unmarkedBase = await TypeFlagTestFixture.RunAsync(
              [new NamedSource("Owner.cs", HIERARCHY.Replace(MARKER, string.Empty, StringComparison.Ordinal))]
            , taken
        );

        var derivedHint = TypeFlagGeneratorTests.GetHintName("TestProject.Derived");

        StringAssert.Contains(GetSource(unmarkedBase, derivedHint), "public static readonly TypeFlagAPI TypeFlag");
        StringAssert.Contains(GetSource(unmarkedBase, derivedHint), "public readonly struct TypeFlagAPI");

        var finalSources = new[] {
            new NamedSource("Owner.cs", HIERARCHY.Replace(MARKER, "[TypeFlag]", StringComparison.Ordinal)),
        };

        var markedBase = await TypeFlagTestFixture.RunAsync(finalSources, unmarkedBase);

        StringAssert.Contains(GetSource(markedBase, derivedHint), "public static new readonly TypeFlagAPI TypeFlag");
        StringAssert.Contains(GetSource(markedBase, derivedHint), "public new readonly struct TypeFlagAPI");

        var fresh = await TypeFlagTestFixture.RunAsync(finalSources);

        CollectionAssert.AreEqual(GetSources(fresh), GetSources(markedBase));
        TypeFlagTestFixture.AssertCompilerDiagnostics(markedBase);
    }

    [TestMethod]
    [DynamicData(nameof(EditCases), DynamicDataSourceType.Property)]
    public async Task ReusedDriverEdit_RegeneratesOutput(string name, string source, string before, string after)
    {
        var original = new NamedSource("Owner.cs", source.Replace(ReusedDriverEditCase.EDIT, before));
        var edited = new NamedSource("Owner.cs", source.Replace(ReusedDriverEditCase.EDIT, after));
        var first = await TypeFlagTestFixture.RunAsync([original]);
        var reused = await TypeFlagTestFixture.RunAsync([edited], first);
        var fresh = await TypeFlagTestFixture.RunAsync([edited]);

        CollectionAssert.AreEqual(GetSources(fresh), GetSources(reused), name);
        TypeFlagTestFixture.AssertCompilerDiagnostics(reused);
        AssertAllModified(reused, OUTPUTS);
    }

    [TestMethod]
    public async Task CancelledDriverRun_Throws()
    {
        var run = await TypeFlagTestFixture.RunAsync([new NamedSource("Owner.cs", OWNER)]);
        var driver = CSharpGeneratorDriver.Create(new TypeFlagGenerator());
        var token = new CancellationToken(canceled: true);

        AssertThrowsCanceled(RunDriver);

        void RunDriver()
        {
            driver.RunGenerators(run.InputCompilation, token);
        }
    }

    [TestMethod]
    public void CancelledWriter_Throws()
    {
        var token = new CancellationToken(canceled: true);

        AssertThrowsCanceled(Write);

        void Write()
        {
            TypeFlagSourceWriter.Write(spec: default, token: token);
        }
    }

    private static string[] GetSources(TypeFlagRun run)
        => run.Result.GeneratedSources
            .OrderBy(static source => source.HintName, StringComparer.Ordinal)
            .Select(static source => source.HintName + "\n" + source.SourceText)
            .ToArray();

    private static bool HasHint(TypeFlagRun run, string hintName)
        => run.Result.GeneratedSources.Any(
              source => string.Equals(source.HintName, hintName, StringComparison.Ordinal)
        );

    private static string GetSource(TypeFlagRun run, string hintName)
        => run.Result.GeneratedSources
            .Single(source => string.Equals(source.HintName, hintName, StringComparison.Ordinal))
            .SourceText
            .ToString();

    private static void AssertStable(TypeFlagRun run, string trackingName)
    {
        Assert.IsTrue(run.Result.TrackedSteps.TryGetValue(trackingName, out var steps), trackingName);

        var outputs = steps.SelectMany(static step => step.Outputs).ToArray();

        Assert.IsTrue(outputs.Length > 0, trackingName);

        foreach (var output in outputs)
        {
            Assert.IsTrue(
                  output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged
                , $"{trackingName}: {output.Reason}"
            );
        }
    }

    private static void AssertAllModified(TypeFlagRun run, string trackingName)
    {
        Assert.IsTrue(run.Result.TrackedSteps.TryGetValue(trackingName, out var steps), trackingName);

        var outputs = steps.SelectMany(static step => step.Outputs).ToArray();

        Assert.IsTrue(outputs.Length > 0, trackingName);

        foreach (var output in outputs)
        {
            Assert.AreEqual(IncrementalStepRunReason.Modified, output.Reason, trackingName);
        }
    }

    private static void AssertThrowsCanceled(Action action)
    {
        try
        {
            action();
        }
        catch (OperationCanceledException)
        {
            return;
        }

        Assert.Fail($"Expected {nameof(OperationCanceledException)}.");
    }
}
