namespace EncosyTower.SourceGen.Tests.PageFlows;

[TestClass]
public sealed class PageFlowScopeCollectionIncrementalTests
{
    private const string TRACKING_NAME = "PageFlowScopeCollectionGenerator.Specs";

    private const string SCREEN_SOURCE = """
        using EncosyTower.PageFlows;

        namespace TestProject;

        [PageFlowScopeCollection]
        public partial struct ScreenScopes
        {
            public PageFlowScope Screen { get; set; }
        }
        """;

    private const string POPUP_SOURCE = """
        using EncosyTower.PageFlows;

        namespace TestProject;

        [PageFlowScopeCollection]
        public partial struct PopupScopes
        {
            public PageFlowScope Popup { get; set; }
        }
        """;

    [TestMethod]
    public async Task UnrelatedEdit_KeepsSpecsCachedAndOutputIdentical()
    {
        var first = await PageFlowsRuntimeFixture.RunAsync(
              [
                  new NamedSource("Screen.cs", SCREEN_SOURCE),
                  new NamedSource("Unrelated.cs", "internal sealed class UnrelatedA { }"),
              ]
            , previous: null
        );

        var second = await PageFlowsRuntimeFixture.RunAsync(
              [
                  new NamedSource("Screen.cs", SCREEN_SOURCE),
                  new NamedSource("Unrelated.cs", "internal sealed class UnrelatedB { }"),
              ]
            , first
        );

        AssertReasons(second, IncrementalStepRunReason.Cached, IncrementalStepRunReason.Unchanged);
        CollectionAssert.AreEqual(GetSources(first), GetSources(second));
    }

    [TestMethod]
    public async Task AddedProperty_RegeneratesOnlyThatTypeOutput()
    {
        var first = await PageFlowsRuntimeFixture.RunAsync(
              [new NamedSource("Screen.cs", SCREEN_SOURCE), new NamedSource("Popup.cs", POPUP_SOURCE)]
            , previous: null
        );

        var editedPopup = POPUP_SOURCE.Replace(
              "public PageFlowScope Popup { get; set; }"
            , "public PageFlowScope Popup { get; set; }\n\n    public PageFlowScope Toast { get; set; }"
            , StringComparison.Ordinal
        );

        var second = await PageFlowsRuntimeFixture.RunAsync(
              [new NamedSource("Screen.cs", SCREEN_SOURCE), new NamedSource("Popup.cs", editedPopup)]
            , first
        );

        var firstSources = GetSourcesByHint(first);
        var secondSources = GetSourcesByHint(second);
        var screenHint = FindHint(firstSources, "ScreenScopes");
        var popupHint = FindHint(firstSources, "PopupScopes");

        Assert.AreEqual(firstSources[screenHint], secondSources[screenHint]);
        Assert.AreNotEqual(firstSources[popupHint], secondSources[popupHint]);
        StringAssert.Contains(secondSources[popupHint], "nameof(Toast)");
        AssertReasonsInclude(second, IncrementalStepRunReason.Modified);
        AssertReasonsInclude(second, IncrementalStepRunReason.Cached);
        PageFlowsRuntimeFixture.AssertNoOutputErrors(second);
    }

    [TestMethod]
    public async Task RemovedAttribute_RemovesOutput()
    {
        var first = await PageFlowsRuntimeFixture.RunAsync(
              [new NamedSource("Screen.cs", SCREEN_SOURCE)]
            , previous: null
        );

        var withoutAttribute = SCREEN_SOURCE.Replace(
              "[PageFlowScopeCollection]"
            , string.Empty
            , StringComparison.Ordinal
        );

        var second = await PageFlowsRuntimeFixture.RunAsync([new NamedSource("Screen.cs", withoutAttribute)], first);

        Assert.AreEqual(1, first.Sources.Count);
        Assert.AreEqual(0, second.Sources.Count);
    }

    private static string[] GetSources(PageFlowsRun run)
        => run.Sources
            .OrderBy(static source => source.HintName, StringComparer.Ordinal)
            .Select(static source => source.HintName + "\n" + source.SourceText)
            .ToArray();

    private static Dictionary<string, string> GetSourcesByHint(PageFlowsRun run)
        => run.Sources.ToDictionary(
              static source => source.HintName
            , static source => source.SourceText.ToString()
            , StringComparer.Ordinal
        );

    private static string FindHint(Dictionary<string, string> sources, string typeName)
        => sources.Keys.Single(hint => hint.Contains(typeName, StringComparison.Ordinal));

    private static void AssertReasons(PageFlowsRun run, params IncrementalStepRunReason[] allowed)
    {
        Assert.IsTrue(run.Result.TrackedSteps.TryGetValue(TRACKING_NAME, out var steps));

        var outputs = steps.SelectMany(static step => step.Outputs).ToArray();

        Assert.IsTrue(outputs.Length > 0);

        foreach (var output in outputs)
        {
            CollectionAssert.Contains(allowed, output.Reason);
        }
    }

    private static void AssertReasonsInclude(PageFlowsRun run, IncrementalStepRunReason expected)
    {
        Assert.IsTrue(run.Result.TrackedSteps.TryGetValue(TRACKING_NAME, out var steps));
        Assert.IsTrue(steps.SelectMany(static step => step.Outputs).Any(output => output.Reason == expected));
    }
}
