namespace EncosyTower.SourceGen.Tests.Common;

[TestClass]
public sealed class GeneratorTestHelperTests
{
    private const string ALPHA_OUTPUT = """
        namespace GeneratorContractFixture;

        internal static class ContractAlphaGenerated
        {
            internal const string Value = "ContractAlpha";
        }
        """;

    private const string BETA_OUTPUT = """
        namespace GeneratorContractFixture;

        internal static class ContractBetaGenerated
        {
            internal const string Value = "ContractBeta";
        }
        """;

    private const string GAMMA_OUTPUT = """
        namespace GeneratorContractFixture;

        internal static class ContractGammaGenerated
        {
            internal const string Value = "ContractGamma";
        }
        """;

    [TestMethod]
    public Task ContractMatrix_SupportsEveryRequiredTransition()
        => GeneratorTestHelper.VerifyContractAsync<ContractIncrementalGenerator>(CreateContractCase());

    [TestMethod]
    public Task NoOutput_UsesNamedSourcesAndRawDriver()
        => GeneratorTestHelper.VerifyNoOutputAsync<ContractIncrementalGenerator>(
            new[] {
                new NamedSource("MarkerOnly.cs", "internal sealed class MarkerOnly { }"),
                new NamedSource("Unrelated.cs", "internal sealed class Unrelated { }"),
            }
        );

    [TestMethod]
    public Task AnalyzerHelper_AcceptsNamedSources()
        => AnalyzerTestHelper.VerifyAsync<NoOpDiagnosticAnalyzer>(
            new[] {
                new NamedSource("First.cs", "internal sealed class First { }"),
                new NamedSource("Second.cs", "internal sealed class Second { }"),
            }
        );

    [TestMethod]
    public Task FaultFixture_ExposesExceptionWithoutFallbackOutput()
        => GeneratorTestHelper.VerifyFaultFixtureAsync();

    [TestMethod]
    public Task CancellationFixture_PropagatesMatchingCancellationWithoutStaleOutput()
        => GeneratorTestHelper.VerifyCancellationFixtureAsync();

    [TestMethod]
    public async Task StackTraceHiddenPolyfill_FrameworkDefinesType_DoesNotInjectPolyfill()
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync();

        await GeneratorTestHelper.VerifyStackTraceHiddenPolyfillAsync(references, expectPolyfill: false);
    }

    [TestMethod]
    public async Task StackTraceHiddenPolyfill_LegacyReferencesInjectDefinition()
    {
        var references = await ReferenceAssemblies.NetStandard.NetStandard20.ResolveAsync(
              LanguageNames.CSharp
            , CancellationToken.None
        );

        await GeneratorTestHelper.VerifyStackTraceHiddenPolyfillAsync(references, expectPolyfill: true);
    }

    [TestMethod]
    public Task StackTraceHiddenConflict_FailsThroughStrictCompilationSurface()
        => GeneratorTestHelper.VerifyStackTraceHiddenConflictIsStrictAsync();

    private static GeneratorContractCase CreateContractCase()
    {
        var alphaAndBeta = new[] {
            new GeneratorExpectedOutput("ContractAlpha.g.cs", ALPHA_OUTPUT),
            new GeneratorExpectedOutput("ContractBeta.g.cs", BETA_OUTPUT),
        };
        var alphaAndGamma = new[] {
            new GeneratorExpectedOutput("ContractAlpha.g.cs", ALPHA_OUTPUT),
            new GeneratorExpectedOutput("ContractGamma.g.cs", GAMMA_OUTPUT),
        };

        return new GeneratorContractCase(
              markerOnly: new GeneratorTransitionExpectation(
                  new[] {
                      new NamedSource("MarkerOnly.cs", "internal sealed class MarkerOnly { }"),
                  }
                , Array.Empty<GeneratorExpectedOutput>()
              )
            , valid: new GeneratorTransitionExpectation(
                  new[] {
                      new NamedSource(
                          "Valid.cs",
                          """
                          internal sealed class ContractAlpha { }
                          internal sealed class ContractBeta { }
                          """
                      ),
                  }
                , alphaAndBeta
                , new[] { "ContractAlpha.g.cs", "ContractBeta.g.cs" }
              )
            , unrelatedEdit: new GeneratorTransitionExpectation(
                  new[] {
                      new NamedSource(
                          "Valid.cs",
                          """
                          internal sealed class ContractAlpha { }
                          internal sealed class ContractBeta { }
                          """
                      ),
                      new NamedSource("Unrelated.cs", "internal sealed class UnrelatedEdit { }"),
                  }
                , alphaAndBeta
                , expectStableOutputSteps: true
              )
            , fileMove: new GeneratorTransitionExpectation(
                  new[] {
                      new NamedSource(
                          "Moved/Valid.cs",
                          """
                          internal sealed class ContractAlpha { }
                          internal sealed class ContractBeta { }
                          """
                      ),
                      new NamedSource("Unrelated.cs", "internal sealed class UnrelatedEdit { }"),
                  }
                , alphaAndBeta
              )
            , precedingLineEdit: new GeneratorTransitionExpectation(
                  new[] {
                      new NamedSource(
                          "Moved/Valid.cs",
                          """
                          // A preceding non-semantic edit.
                          internal sealed class ContractAlpha { }
                          internal sealed class ContractBeta { }
                          """
                      ),
                      new NamedSource("Unrelated.cs", "internal sealed class UnrelatedEdit { }"),
                  }
                , alphaAndBeta
              )
            , orderEdit: new GeneratorTransitionExpectation(
                  new[] {
                      new NamedSource("Beta.cs", "internal sealed class ContractBeta { }"),
                      new NamedSource("Alpha.cs", "internal sealed class ContractAlpha { }"),
                  }
                , alphaAndBeta
              )
            , relevantEdit: new GeneratorTransitionExpectation(
                  new[] {
                      new NamedSource("Alpha.cs", "internal sealed class ContractAlpha { }"),
                      new NamedSource("Gamma.cs", "internal sealed class ContractGamma { }"),
                  }
                , alphaAndGamma
                , new[] { "ContractBeta.g.cs", "ContractGamma.g.cs" }
              )
            , removal: new GeneratorTransitionExpectation(
                  new[] {
                      new NamedSource("MarkerOnly.cs", "internal sealed class MarkerOnly { }"),
                  }
                , Array.Empty<GeneratorExpectedOutput>()
                , new[] { "ContractAlpha.g.cs", "ContractGamma.g.cs" }
              )
            , ownedTrackingNames: new[] {
                  ContractIncrementalGenerator.CANDIDATES_TRACKING_NAME
                , ContractIncrementalGenerator.VALID_SPECS_TRACKING_NAME
                , ContractIncrementalGenerator.OUTPUTS_TRACKING_NAME
              }
            , ownedOutputTrackingNames: new[] {
                ContractIncrementalGenerator.OUTPUTS_TRACKING_NAME,
            }
        );
    }
}
