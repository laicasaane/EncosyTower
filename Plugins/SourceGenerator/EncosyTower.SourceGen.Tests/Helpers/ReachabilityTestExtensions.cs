namespace EncosyTower.SourceGen.Tests;

internal static class ReachabilityTestExtensions
{
    internal static bool HasGeneratedSource(this GeneratorDriverRunResult result, string hintPrefix)
        => FindGeneratedSources(result, hintPrefix).Any();

    internal static string GetGeneratedSourceText(this GeneratorDriverRunResult result, string hintPrefix)
        => FindGeneratedSources(result, hintPrefix).Single().SourceText.ToString();

    internal static DiagnosticResult OnTestLine(
          this DiagnosticResult result
        , int line
        , int startColumn
        , int endColumn
    )
        => result.WithSpan(
              path: GeneratorAnalyzerTestHelper.INPUT_PATH
            , startLine: line
            , startColumn: startColumn
            , endLine: line
            , endColumn: endColumn
        );

    private static IEnumerable<GeneratedSourceResult> FindGeneratedSources(
          GeneratorDriverRunResult result
        , string hintPrefix
    )
        => result.Results
            .SelectMany(static run => run.GeneratedSources)
            .Where(source => source.HintName.StartsWith(hintPrefix, StringComparison.Ordinal));
}
