using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Tests;

internal static class GeneratorTestHelper
{
    private const string INPUT_ASSEMBLY_NAME = "EncosyTower.SourceGen.Tests.Input";
    private const string INPUT_PATH = "Test0.cs";
    private const string STACK_TRACE_HIDDEN_METADATA_NAME = "System.Diagnostics.StackTraceHiddenAttribute";
    private const string STACK_TRACE_HIDDEN_POLYFILL_PATH = "StackTraceHiddenAttribute.cs";
    private const string STACK_TRACE_HIDDEN_POLYFILL_SOURCE = """
        namespace System.Diagnostics
        {
            [System.AttributeUsage(
                  System.AttributeTargets.Class
                | System.AttributeTargets.Method
                | System.AttributeTargets.Constructor
                | System.AttributeTargets.Struct
              , Inherited = false
            )]
            internal sealed class StackTraceHiddenAttribute : System.Attribute { }
        }
        """;
    private const string VARIANT_METADATA_NAME = "EncosyTower.Variants.Variant";
    private const string VARIANT_POLYFILL_PATH = "Variant.cs";
    private const string VARIANT_POLYFILL_SOURCE = """
        namespace EncosyTower.Variants
        {
            public struct Variant { }
        }
        """;
    private const string UNRELATED_PATH = "Unrelated.cs";

    private static readonly CSharpParseOptions s_parseOptions = CSharpParseOptions.Default
        .WithLanguageVersion(LanguageVersion.CSharp10);

    internal static Task VerifyNoOutputAsync<TGenerator>(
          string source = ""
        , CancellationToken token = default
    )
        where TGenerator : IIncrementalGenerator, new()
        => VerifyNoOutputAsync<TGenerator>(new[] { new NamedSource(INPUT_PATH, source) }, token);

    internal static async Task VerifyNoOutputAsync<TGenerator>(
          IReadOnlyList<NamedSource> sources
        , CancellationToken token = default
    )
        where TGenerator : IIncrementalGenerator, new()
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var compilation = CreateCompilation(sources, INPUT_ASSEMBLY_NAME, references, token);
        var generators = new IIncrementalGenerator[] { new TGenerator() };
        var run = RunDriver(CreateDriver(generators), compilation, token);
        var actualSources = GetActualSources(run.Result, generators);
        Assert.AreEqual(0, actualSources.Count, "Expected the generator to produce no sources.");
    }

    internal static Task VerifyGeneratedSourceFragmentsAsync<TGenerator>(
          string source
        , IReadOnlyList<string> expectedFragments
        , IReadOnlyList<string> unexpectedFragments
        , CancellationToken token = default
    )
        where TGenerator : IIncrementalGenerator, new()
        => VerifyGeneratedSourceFragmentsAsync<TGenerator>(
              source
            , expectedFragments
            , unexpectedFragments
            , Array.Empty<IIncrementalGenerator>()
            , token
        );

    internal static async Task VerifyGeneratedSourceFragmentsAsync<TGenerator>(
          string source
        , IReadOnlyList<string> expectedFragments
        , IReadOnlyList<string> unexpectedFragments
        , IReadOnlyList<IIncrementalGenerator> additionalGenerators
        , CancellationToken token = default
    )
        where TGenerator : IIncrementalGenerator, new()
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var compilation = CreateCompilation(source, INPUT_ASSEMBLY_NAME, references);
        var generators = new List<IIncrementalGenerator> { new TGenerator() };

        generators.AddRange(additionalGenerators);

        var run = RunDriver(CreateDriver(generators), compilation, token);
        var allSources = GetActualSources(run.Result, generators);
        var actualSources = allSources
            .Where(static pair => pair.Key.GeneratorType == typeof(TGenerator))
            .ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.AreEqual(1, actualSources.Count, "Expected exactly one generated source.");
        var generatedSource = actualSources.Single().Value.Source;

        foreach (var expectedFragment in expectedFragments)
        {
            Assert.IsTrue(
                  generatedSource.Contains(expectedFragment, StringComparison.Ordinal)
                , $"Generated source does not contain expected fragment:\n{expectedFragment}"
            );
        }

        foreach (var unexpectedFragment in unexpectedFragments)
        {
            Assert.IsFalse(
                  generatedSource.Contains(unexpectedFragment, StringComparison.Ordinal)
                , $"Generated source contains unexpected fragment:\n{unexpectedFragment}"
            );
        }
    }

    internal static async Task VerifyGeneratedSourceSetFragmentsAsync<TGenerator>(
          string source
        , int expectedSourceCount
        , IReadOnlyList<string> expectedFragments
        , IReadOnlyList<string> unexpectedFragments
        , IReadOnlyList<IIncrementalGenerator>? additionalGenerators = null
        , CancellationToken token = default
        , IEnumerable<MetadataReference>? additionalReferences = null
        , IReadOnlyList<string>? expectedOrderedFragments = null
        , IReadOnlyList<string>? preprocessorSymbols = null
        , string? generatedOutputDirectory = null
    )
        where TGenerator : IIncrementalGenerator, new()
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);

        if (additionalReferences is not null)
        {
            references = references.AddRange(additionalReferences);
        }

        var compilation = CreateCompilation(source, INPUT_ASSEMBLY_NAME, references);
        var parseOptions = preprocessorSymbols is null
            ? s_parseOptions
            : s_parseOptions.WithPreprocessorSymbols(preprocessorSymbols);

        if (preprocessorSymbols is not null)
        {
            var parsedTrees = compilation.SyntaxTrees
                .Select(tree => tree.WithRootAndOptions(tree.GetRoot(), parseOptions))
                .ToArray();
            compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(parsedTrees);
        }

        var generators = new List<IIncrementalGenerator> { new TGenerator() };

        if (additionalGenerators is not null)
        {
            generators.AddRange(additionalGenerators);
        }

        var run = RunDriver(
              CreateDriver(generators, parseOptions), compilation, token, includeGeneratedSourcesOnFailure: true
        );
        var allSources = GetActualSources(run.Result, generators);
        var actualSources = allSources
            .Where(static pair => pair.Key.GeneratorType == typeof(TGenerator))
            .OrderBy(static pair => pair.Key.HintName, StringComparer.Ordinal)
            .ToArray();
        Assert.AreEqual(expectedSourceCount, actualSources.Length, "Generated source count differs.");
        var generatedSource = string.Join("\n", actualSources.Select(static pair => pair.Value.Source));

        foreach (var expectedFragment in expectedFragments)
        {
            Assert.IsTrue(
                  generatedSource.Contains(expectedFragment, StringComparison.Ordinal)
                , $"Generated sources do not contain expected fragment:\n{expectedFragment}"
            );
        }

        foreach (var unexpectedFragment in unexpectedFragments)
        {
            Assert.IsFalse(
                  generatedSource.Contains(unexpectedFragment, StringComparison.Ordinal)
                , $"Generated sources contain unexpected fragment:\n{unexpectedFragment}"
            );
        }

        if (expectedOrderedFragments is not null)
        {
            var previous = -1;

            foreach (var fragment in expectedOrderedFragments)
            {
                var index = generatedSource.IndexOf(fragment, previous + 1, StringComparison.Ordinal);
                Assert.IsTrue(index > previous, $"Generated fragment is missing or out of order:\n{fragment}");
                previous = index;
            }
        }

        if (generatedOutputDirectory is not null)
        {
            Directory.CreateDirectory(generatedOutputDirectory);

            foreach (var sourceFile in actualSources)
            {
                File.WriteAllText(
                      Path.Combine(generatedOutputDirectory, sourceFile.Key.HintName)
                    , sourceFile.Value.Source
                );
            }
        }
    }

    internal static async Task VerifyDescriptorSurvivesMemberRemovalAsync<TGenerator>(
          string sourceWithMember
        , string sourceWithoutMember
        , string sourceWithoutBinder
        , string descriptorFragment
        , string memberFragment
    )
        where TGenerator : IIncrementalGenerator, new()
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(CancellationToken.None);
        var generators = new IIncrementalGenerator[] { new TGenerator() };
        var initialCompilation = CreateCompilation(sourceWithMember, INPUT_ASSEMBLY_NAME, references);
        var initial = RunDriver(CreateDriver(generators), initialCompilation, CancellationToken.None);
        var initialSources = GetActualSources(initial.Result, generators);
        Assert.AreEqual(2, initialSources.Count);
        var initialText = string.Join("\n", initialSources.Select(static value => value.Value.Source));
        StringAssert.Contains(initialText, descriptorFragment);
        StringAssert.Contains(initialText, memberFragment);

        var emptyCompilation = CreateTransitionCompilation(
              initial.InputCompilation
            , new[] { new NamedSource(INPUT_PATH, sourceWithoutMember) }
            , INPUT_ASSEMBLY_NAME
            , references
            , CancellationToken.None
        );
        var empty = RunDriver(initial.Driver, emptyCompilation, CancellationToken.None);
        var emptySources = GetActualSources(empty.Result, generators);
        Assert.AreEqual(2, emptySources.Count);
        var emptyText = string.Join("\n", emptySources.Select(static value => value.Value.Source));
        StringAssert.Contains(emptyText, descriptorFragment);
        Assert.IsFalse(emptyText.Contains(memberFragment, StringComparison.Ordinal));

        var removedCompilation = CreateTransitionCompilation(
              empty.InputCompilation
            , new[] { new NamedSource(INPUT_PATH, sourceWithoutBinder) }
            , INPUT_ASSEMBLY_NAME
            , references
            , CancellationToken.None
        );
        var removed = RunDriver(empty.Driver, removedCompilation, CancellationToken.None);
        Assert.AreEqual(0, GetActualSources(removed.Result, generators).Count);
    }

    internal static async Task VerifyCandidateCountsAsync<TGenerator>(
          IReadOnlyList<NamedSource> sources
        , IReadOnlyDictionary<string, int> expectedCandidateCounts
        , CancellationToken token = default
    )
        where TGenerator : IIncrementalGenerator, new()
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var compilation = CreateCompilation(sources, INPUT_ASSEMBLY_NAME, references, token);
        var generators = new IIncrementalGenerator[] { new TGenerator() };
        var run = RunDriver(CreateDriver(generators), compilation, token);
        Assert.AreEqual(1, run.Result.Results.Length);
        var generatorResult = run.Result.Results[0];
        Assert.IsTrue(
            generatorResult.GeneratedSources.Length > 0,
            "The structural fixture must produce generated source."
        );

        foreach (var pair in expectedCandidateCounts)
        {
            Assert.IsTrue(
                TryGetOwnedTrackingSteps(generatorResult, pair.Key, out var steps),
                $"Tracking name '{pair.Key}' was not recorded."
            );
            var outputs = steps.SelectMany(static step => step.Outputs).ToArray();
            Assert.AreEqual(pair.Value, outputs.Length, $"Tracked candidate count differs for '{pair.Key}'.");

            foreach (var output in outputs)
            {
                Assert.AreEqual(
                      IncrementalStepRunReason.New
                    , output.Reason
                    , $"Unexpected initial reason for '{pair.Key}'."
                );
            }
        }
    }

    internal static async Task VerifyProductionContractAsync(
          ProductionGeneratorContractCase contract
        , string validSource
        , string relevantSource
        , IReadOnlyList<ProductionGeneratorExpectedOutput> expectedValidOutputs
        , CancellationToken token = default
    )
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var generators = new List<IIncrementalGenerator> {
            CreateGenerator(contract.GeneratorType),
        };
        generators.AddRange(contract.AdditionalGeneratorTypes.Select(CreateGenerator));
        var markerSources = new[] {
            new NamedSource("MarkerOnly.cs", "internal sealed class MarkerOnly { }"),
        };
        var validSources = new[] { new NamedSource("Valid.cs", validSource) };
        var markerRun = RunDriver(
              CreateDriver(generators)
            , CreateCompilation(markerSources, INPUT_ASSEMBLY_NAME, references, token)
            , token
        );
        VerifyNoContractOutputs(markerRun.Result, generators);
        var validRun = RunDriver(
              markerRun.Driver
            , CreateTransitionCompilation(
                  markerRun.InputCompilation
                , validSources
                , INPUT_ASSEMBLY_NAME
                , references
                , token
            )
            , token
        );
        VerifyProductionContractOutputs(validRun.Result, generators, expectedValidOutputs);
        VerifyOwnedTrackingNames(validRun.Result, GetOwnedTrackingNames(contract.GeneratorType), generatorIndex: 0);
        var identicalRun = RunDriver(validRun.Driver, validRun.InputCompilation, token);
        VerifyEquivalentContractOutputs(validRun.Result, identicalRun.Result, generators);
        VerifyStableOwnedOutputSteps(
              identicalRun.Result
            , GetOwnedOutputTrackingNames(contract.GeneratorType)
            , generatorIndex: 0
        );
        var unrelatedSources = new[] {
            new NamedSource("Valid.cs", validSource),
            new NamedSource("Unrelated.cs", "internal sealed class UnrelatedEdit { }"),
        };
        var unrelatedRun = RunDriver(
              identicalRun.Driver
            , CreateTransitionCompilation(
                  identicalRun.InputCompilation
                , unrelatedSources
                , INPUT_ASSEMBLY_NAME
                , references
                , token
            )
            , token
        );
        VerifyEquivalentContractOutputs(validRun.Result, unrelatedRun.Result, generators);
        VerifyStableOwnedOutputSteps(
              unrelatedRun.Result
            , GetOwnedOutputTrackingNames(contract.GeneratorType)
            , generatorIndex: 0
        );
        var movedSources = new[] {
            new NamedSource("Moved/Valid.cs", validSource),
            new NamedSource("Unrelated.cs", "internal sealed class UnrelatedEdit { }"),
        };
        var movedRun = RunDriver(
              unrelatedRun.Driver
            , CreateTransitionCompilation(
                  unrelatedRun.InputCompilation
                , movedSources
                , INPUT_ASSEMBLY_NAME
                , references
                , token
            )
            , token
        );
        if (Helpers.ProductionGeneratorContractTestHelper.IsSourceLocationSensitive(contract))
        {
            VerifySourceLocationOutputChanged(validRun.Result, movedRun.Result, generators);
        }
        else
        {
            VerifyEquivalentContractOutputs(validRun.Result, movedRun.Result, generators);
        }
        var precedingEditSource = Environment.NewLine + validSource;
        var precedingSources = new[] {
            new NamedSource("Moved/Valid.cs", precedingEditSource),
            new NamedSource("Unrelated.cs", "internal sealed class UnrelatedEdit { }"),
        };
        var precedingRun = RunDriver(
              movedRun.Driver
            , CreateTransitionCompilation(
                  movedRun.InputCompilation
                , precedingSources
                , INPUT_ASSEMBLY_NAME
                , references
                , token
            )
            , token
        );
        if (Helpers.ProductionGeneratorContractTestHelper.IsSourceLocationSensitive(contract))
        {
            VerifySourceLocationOutputChanged(movedRun.Result, precedingRun.Result, generators);
        }
        else
        {
            VerifyEquivalentContractOutputs(validRun.Result, precedingRun.Result, generators);
        }
        var orderSources = new[] {
            new NamedSource("Unrelated.cs", "internal sealed class UnrelatedEdit { }"),
            new NamedSource("Moved/Valid.cs", precedingEditSource),
        };
        var orderRun = RunDriver(
              precedingRun.Driver
            , CreateTransitionCompilation(
                  precedingRun.InputCompilation
                , orderSources
                , INPUT_ASSEMBLY_NAME
                , references
                , token
            )
            , token
        );
        if (Helpers.ProductionGeneratorContractTestHelper.IsSourceLocationSensitive(contract))
        {
            VerifyEquivalentContractOutputs(precedingRun.Result, orderRun.Result, generators);
        }
        else
        {
            VerifyEquivalentContractOutputs(validRun.Result, orderRun.Result, generators);
        }
        var relevantRun = RunDriver(
              orderRun.Driver
            , CreateTransitionCompilation(
                  orderRun.InputCompilation
                , new[] { new NamedSource("RelevantEdit.cs", relevantSource) }
                , INPUT_ASSEMBLY_NAME
                , references
                , token
            )
            , token
        );
        VerifyRelevantContractOutputsChanged(
              validRun.Result
            , relevantRun.Result
            , generators
        );
        var removalRun = RunDriver(
              relevantRun.Driver
            , CreateTransitionCompilation(
                  relevantRun.InputCompilation
                , markerSources
                , INPUT_ASSEMBLY_NAME
                , references
                , token
            )
            , token
        );
        VerifyNoContractOutputs(removalRun.Result, generators);
        var freshRun = RunDriver(CreateDriver(generators), validRun.InputCompilation, token);
        VerifyProductionContractOutputs(freshRun.Result, generators, expectedValidOutputs);
    }

    internal static async Task VerifyGeneratedSourcesAsync<TGenerator>(
          string source
        , IReadOnlyList<ExpectedGeneratedSource> expectedSources
        , IReadOnlyList<IIncrementalGenerator>? additionalGenerators = null
        , IEnumerable<MetadataReference>? additionalReferences = null
        , bool verifyDebuggingAliasContract = false
        , CancellationToken token = default
    )
        where TGenerator : IIncrementalGenerator, new()
    {
        ValidateExpectedSources(expectedSources);
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);

        if (additionalReferences is not null)
        {
            references = references.AddRange(additionalReferences);
        }

        var compilation = CreateCompilation(source, INPUT_ASSEMBLY_NAME, references);
        var generators = new List<IIncrementalGenerator> { new TGenerator() };

        if (additionalGenerators is not null)
        {
            generators.AddRange(additionalGenerators);
        }

        var driver = CreateDriver(generators);
        var firstRun = RunDriver(driver, compilation, token);
        await VerifyExpectedSourcesAsync(
              firstRun.Result
            , generators
            , expectedSources
            , verifyDebuggingAliasContract
            , token
        );

        var sameInputRun = RunDriver(firstRun.Driver, compilation, token);
        VerifyIncrementality(sameInputRun.Result);
        VerifySecondRunSources(firstRun.Result, sameInputRun.Result, generators);

        var unrelatedTree = CSharpSyntaxTree.ParseText(
              "namespace Unrelated { internal sealed class Marker { } }"
            , s_parseOptions
            , UNRELATED_PATH
            , encoding: null
            , cancellationToken: token
        );
        var secondCompilation = compilation.Clone().AddSyntaxTrees(unrelatedTree);
        var secondRun = RunDriver(sameInputRun.Driver, secondCompilation, token);
        VerifyIncrementality(secondRun.Result);
        VerifySecondRunSources(firstRun.Result, secondRun.Result, generators);

        var removalCompilation = secondCompilation.RemoveSyntaxTrees(compilation.SyntaxTrees);
        var removalRun = RunDriver(secondRun.Driver, removalCompilation, token);
        VerifyRemovedSources(firstRun.Result, removalRun.Result, generators);

        var freshRun = RunDriver(CreateDriver(generators), compilation, token);
        VerifySecondRunSources(firstRun.Result, freshRun.Result, generators);
    }

    internal static async Task VerifyContractAsync<TGenerator>(
          GeneratorContractCase contract
        , IEnumerable<MetadataReference>? additionalReferences = null
        , CancellationToken token = default
    )
        where TGenerator : IIncrementalGenerator, new()
    {
        ValidateContract(contract);
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);

        if (additionalReferences is not null)
        {
            references = references.AddRange(additionalReferences);
        }

        var generators = new IIncrementalGenerator[] { new TGenerator() };
        var markerOnlyRun = RunContractTransition(
              CreateDriver(generators)
            , contract.MarkerOnly
            , references
            , generators
            , previousRun: null
            , contract
            , token
            , verifyOwnedTrackingNames: false
        );
        var validRun = RunContractTransition(
              markerOnlyRun.Driver
            , contract.Valid
            , references
            , generators
            , markerOnlyRun
            , contract
            , token
        );
        var identicalRun = RunContractTransition(
              validRun.Driver
            , contract.Valid
            , references
            , generators
            , validRun
            , contract
            , token
            , expectedChangedHintNames: Array.Empty<string>()
            , expectStableOutputSteps: true
            , inputCompilation: validRun.InputCompilation
        );
        var unrelatedRun = RunContractTransition(
              identicalRun.Driver
            , contract.UnrelatedEdit
            , references
            , generators
            , identicalRun
            , contract
            , token
        );
        var fileMoveRun = RunContractTransition(
              unrelatedRun.Driver
            , contract.FileMove
            , references
            , generators
            , unrelatedRun
            , contract
            , token
        );
        var precedingLineRun = RunContractTransition(
              fileMoveRun.Driver
            , contract.PrecedingLineEdit
            , references
            , generators
            , fileMoveRun
            , contract
            , token
        );
        var orderRun = RunContractTransition(
              precedingLineRun.Driver
            , contract.OrderEdit
            , references
            , generators
            , precedingLineRun
            , contract
            , token
        );
        var relevantRun = RunContractTransition(
              orderRun.Driver
            , contract.RelevantEdit
            , references
            , generators
            , orderRun
            , contract
            , token
        );
        RunContractTransition(
              relevantRun.Driver
            , contract.Removal
            , references
            , generators
            , relevantRun
            , contract
            , token
        );

        var freshRun = RunDriver(
              CreateDriver(generators)
            , CreateCompilation(contract.Valid.Sources, INPUT_ASSEMBLY_NAME, references, token)
            , token
        );
        VerifyContractOutputs(freshRun.Result, generators, contract.Valid.Outputs);
        VerifySecondRunSources(validRun.Result, freshRun.Result, generators);
    }

    internal static async Task VerifyFaultFixtureAsync(CancellationToken token = default)
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var compilation = CreateCompilation(
              new[] { new NamedSource(INPUT_PATH, "internal sealed class Input { }") }
            , INPUT_ASSEMBLY_NAME
            , references
            , token
        );
        var driver = CreateDriver(new IIncrementalGenerator[] { new ThrowingIncrementalGenerator(), });
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var driverDiagnostics, token);
        var result = driver.GetRunResult();
        Assert.AreEqual(1, result.Results.Length);
        var generatorResult = result.Results[0];
        Assert.IsInstanceOfType<InvalidOperationException>(generatorResult.Exception);
        Assert.AreEqual(ThrowingIncrementalGenerator.EXCEPTION_MESSAGE, generatorResult.Exception.Message);
        Assert.AreEqual(0, generatorResult.GeneratedSources.Length);
        Assert.IsFalse(
            driverDiagnostics
                .Concat(generatorResult.Diagnostics)
                .Any(static diagnostic =>
                    diagnostic.Id.StartsWith("SG_", StringComparison.Ordinal)
                ),
            "The fault fixture produced a component fallback diagnostic."
        );
    }

    internal static async Task VerifyCancellationFixtureAsync(CancellationToken token = default)
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        using var cancellationSource = new CancellationTokenSource();
        var generator = new DeterministicCancellationIncrementalGenerator(cancellationSource);
        var generators = new IIncrementalGenerator[] { generator };
        var firstCompilation = CreateCompilation(
              new[] {
                  new NamedSource(INPUT_PATH, "internal sealed class GenerateBeforeCancellation { }"),
              }
            , INPUT_ASSEMBLY_NAME
            , references
            , token
        );
        var firstRun = RunDriver(CreateDriver(generators), firstCompilation, token);
        Assert.AreEqual(1, GetActualSources(firstRun.Result, generators).Count);
        var cancellationCompilation = CreateCompilation(
              new[] {
                  new NamedSource(INPUT_PATH, "internal sealed class CancelGeneration { }"),
              }
            , INPUT_ASSEMBLY_NAME
            , references
            , token
        );

        try
        {
            firstRun.Driver.RunGeneratorsAndUpdateCompilation(
                  cancellationCompilation
                , out _
                , out _
                , cancellationSource.Token
            );
            Assert.Fail("The deterministic cancellation fixture did not cancel.");
        }
        catch (OperationCanceledException exception)
        {
            Assert.AreEqual(cancellationSource.Token, exception.CancellationToken);
        }

        var recoveryCompilation = CreateCompilation(
              new[] { new NamedSource(INPUT_PATH, "internal sealed class NoTarget { }") }
            , INPUT_ASSEMBLY_NAME
            , references
            , CancellationToken.None
        );
        var recoveryRun = RunDriver(firstRun.Driver, recoveryCompilation, CancellationToken.None);
        Assert.AreEqual(
            0,
            GetActualSources(recoveryRun.Result, generators).Count,
            "Cancellation left a stale generated source."
        );
    }

    internal static async Task VerifyDebuggingAliasCollisionAsync(CancellationToken token = default)
    {
        const string SOURCE = """
            using g__ETDBG = global::EncosyTower.Debugging;

            namespace TestProject;

            internal static class Container<ETDBG>
            {
                internal static void Guard(object value)
                    => g__ETDBG.ThrowHelper.ThrowIfNull(value);
            }
            """;

        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var compilation = CreateCompilation(SOURCE, "EncosyTower.SourceGen.Tests.DebuggingAliasCollision", references);
        AssertCleanCompilation(compilation, token);
    }

    internal static Task VerifyStackTraceHiddenPolyfillAsync(
          IEnumerable<MetadataReference> references
        , bool expectPolyfill
        , CancellationToken token = default
    )
    {
        var referenceArray = references.ToImmutableArray();
        var compilation = CreateCompilation(
              Array.Empty<NamedSource>()
            , "EncosyTower.SourceGen.Tests.StackTraceHiddenPolyfill"
            , referenceArray
            , token
        );
        Assert.AreEqual(
              expectPolyfill
            , compilation.SyntaxTrees.Any(static tree =>
                string.Equals(tree.FilePath, STACK_TRACE_HIDDEN_POLYFILL_PATH, StringComparison.Ordinal)
              )
        );
        Assert.IsTrue(
            expectPolyfill
                ? compilation.GetTypeByMetadataName(STACK_TRACE_HIDDEN_METADATA_NAME) is not null
                : HasStackTraceHiddenAttribute(compilation, referenceArray)
        );
        AssertCleanCompilation(compilation, token);
        return Task.CompletedTask;
    }

    internal static async Task VerifyStackTraceHiddenConflictIsStrictAsync(CancellationToken token = default)
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var compilation = CreateCompilation(
              new[] {
                  new NamedSource(
                        INPUT_PATH
                      , """
                        namespace System.Diagnostics
                        {
                            public sealed class StackTraceHiddenAttribute : System.Attribute { }
                        }

                        [System.Diagnostics.StackTraceHidden]
                        internal sealed class ConflictTarget { }
                        """
                  ),
              }
            , "EncosyTower.SourceGen.Tests.StackTraceHiddenConflict"
            , references
            , token
        );
        var exception = Assert.ThrowsException<AssertFailedException>(() => AssertCleanCompilation(compilation, token));
        StringAssert.Contains(exception.Message, "CS0436");
        StringAssert.Contains(exception.Message, "StackTraceHiddenAttribute");
    }

    internal static async Task VerifyReferenceAddRemoveInvarianceAsync<TGenerator>(
          string source
        , MetadataReference reference
        , string outputTrackingName
        , CancellationToken token = default
    )
        where TGenerator : IIncrementalGenerator, new()
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var compilation = CreateCompilation(source, INPUT_ASSEMBLY_NAME, references);
        var generators = new IIncrementalGenerator[] { new TGenerator() };
        var firstRun = RunDriver(CreateDriver(generators), compilation, token);
        var addedCompilation = compilation.AddReferences(reference);
        var addedRun = RunDriver(firstRun.Driver, addedCompilation, token);
        VerifySecondRunSources(firstRun.Result, addedRun.Result, generators);
        VerifyStableOwnedOutputSteps(addedRun.Result, new[] { outputTrackingName });
        var removedCompilation = addedCompilation.RemoveReferences(reference);
        var removedRun = RunDriver(addedRun.Driver, removedCompilation, token);
        VerifySecondRunSources(firstRun.Result, removedRun.Result, generators);
        VerifyStableOwnedOutputSteps(removedRun.Result, new[] { outputTrackingName });
    }

    internal static async Task VerifyObservablePropertyMarkerOutputTransitionAsync(
        CancellationToken token = default
    )
    {
        const string MODEL = """
            namespace TestProject
            {
                [EncosyTower.Mvvm.ComponentModel.ObservableObject]
                public partial class Model
                {
                    [EncosyTower.Mvvm.ComponentModel.ObservableProperty]
                    {{MARKER}}
                    private int _value;
                }
            }
            """;
        const string COMMAND = """
            namespace TestProject
            {
                public partial class Model
                {
                    [EncosyTower.Mvvm.Input.RelayCommand]
                    private void Execute() { }
                }
            }
            """;
        const string UNRELATED = "namespace Unrelated { internal sealed class Edit { } }";
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var generators = new IIncrementalGenerator[] {
            new EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator(),
            new EncosyTower.Mvvm.Generators.RelayCommands.RelayCommandGenerator(),
        };
        var baselineSources = CreateSources(marker: string.Empty);
        var baseline = RunDriver(
              CreateDriver(generators)
            , CreateCompilation(baselineSources, INPUT_ASSEMBLY_NAME, references, token)
            , token
        );
        var markerAdded = RunDriver(
              baseline.Driver
            , CreateTransitionCompilation(
                  baseline.InputCompilation
                , CreateSources("[EncosyTower.Mvvm.ComponentModel.TwoWay]")
                , INPUT_ASSEMBLY_NAME
                , references
                , token
            )
            , token
        );
        AssertObservableMarkerTransition(baseline.Result, markerAdded.Result);
        AssertStableOutputSteps(markerAdded.Result.Results[1], "RelayCommandGenerator.CommandOutputs");

        var markerRemoved = RunDriver(
              markerAdded.Driver
            , CreateTransitionCompilation(
                  markerAdded.InputCompilation
                , baselineSources
                , INPUT_ASSEMBLY_NAME
                , references
                , token
            )
            , token
        );
        AssertSourceSetsEqual(baseline.Result, markerRemoved.Result, generators);

        var unrelatedSources = baselineSources.Append(new NamedSource("Unrelated.cs", UNRELATED)).ToArray();
        var unrelated = RunDriver(
              markerRemoved.Driver
            , CreateTransitionCompilation(
                  markerRemoved.InputCompilation
                , unrelatedSources
                , INPUT_ASSEMBLY_NAME
                , references
                , token
            )
            , token
        );
        AssertSourceSetsEqual(baseline.Result, unrelated.Result, generators);
        AssertStableOutputSteps(unrelated.Result.Results[1], "RelayCommandGenerator.CommandOutputs");
        return;

        static NamedSource[] CreateSources(string marker)
            => new[] {
                new NamedSource("Model.cs", MODEL.Replace("{{MARKER}}", marker, StringComparison.Ordinal)),
                new NamedSource("Commands.cs", COMMAND),
            };
    }

    internal static async Task<MetadataReference> CompileToReferenceAsync(
          string source
        , string assemblyName
        , IReadOnlyList<IIncrementalGenerator>? generators = null
        , IEnumerable<MetadataReference>? additionalReferences = null
        , CancellationToken token = default
    )
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);

        if (additionalReferences is not null)
        {
            references = references.AddRange(additionalReferences);
        }

        var compilation = CreateCompilation(source, assemblyName, references);
        Compilation outputCompilation = compilation;

        if (generators is { Count: > 0 })
        {
            var run = RunDriver(CreateDriver(generators), compilation, token);
            outputCompilation = run.OutputCompilation;
        }

        AssertCleanCompilation(outputCompilation, token);
        using var stream = new MemoryStream();
        var emitResult = outputCompilation.Emit(stream, cancellationToken: token);

        if (emitResult.Success == false)
        {
            Assert.Fail($"Reference compilation '{assemblyName}' failed:\n{string.Join("\n", emitResult.Diagnostics)}");
        }

        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    internal static CSharpCompilation CreateCompilation(
          string source
        , string assemblyName
        , IEnumerable<MetadataReference> references
    )
        => CreateCompilation(
              new[] { new NamedSource(INPUT_PATH, source) }
            , assemblyName
            , references
            , CancellationToken.None
        );

    private static CSharpCompilation CreateCompilation(
          IReadOnlyList<NamedSource> sources
        , string assemblyName
        , IEnumerable<MetadataReference> references
        , CancellationToken token
    )
    {
        var referenceArray = references.ToImmutableArray();
        var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(
              SourceText.From(source.Source, Encoding.UTF8)
            , s_parseOptions
            , source.Path
            , token
        )).ToList();
        var compilation = CSharpCompilation.Create(
              assemblyName
            , syntaxTrees
            , referenceArray
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
        );

        if (HasStackTraceHiddenAttribute(compilation, referenceArray) == false)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                  SourceText.From(STACK_TRACE_HIDDEN_POLYFILL_SOURCE, Encoding.UTF8)
                , s_parseOptions
                , STACK_TRACE_HIDDEN_POLYFILL_PATH
                , token
            ));
            compilation = compilation.AddSyntaxTrees(syntaxTrees[^1]);
        }

        if (compilation.GetTypeByMetadataName(VARIANT_METADATA_NAME) is null)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                  SourceText.From(VARIANT_POLYFILL_SOURCE, Encoding.UTF8)
                , s_parseOptions
                , VARIANT_POLYFILL_PATH
                , token
            ));
            compilation = compilation.AddSyntaxTrees(syntaxTrees[^1]);
        }

        return compilation;
    }

    private static bool HasStackTraceHiddenAttribute(
          Compilation compilation
        , ImmutableArray<MetadataReference> references
    )
        => references.Any(reference => compilation.GetAssemblyOrModuleSymbol(reference) switch
        {
            IAssemblySymbol assembly => assembly.GetTypeByMetadataName(STACK_TRACE_HIDDEN_METADATA_NAME) is not null,
            IModuleSymbol module => module.ContainingAssembly.GetTypeByMetadataName(
                STACK_TRACE_HIDDEN_METADATA_NAME
            ) is not null,
            _ => false,
        });

    private static CSharpCompilation CreateTransitionCompilation(
          Compilation? previousCompilation
        , IReadOnlyList<NamedSource> sources
        , string assemblyName
        , IEnumerable<MetadataReference> references
        , CancellationToken token
    )
    {
        var previousTrees = previousCompilation?.SyntaxTrees.ToDictionary(
              static tree => tree.FilePath
            , StringComparer.Ordinal
        ) ?? new Dictionary<string, SyntaxTree>(StringComparer.Ordinal);
        var syntaxTrees = new List<SyntaxTree>(sources.Count);

        foreach (var source in sources)
        {
            var sourceText = SourceText.From(source.Source, Encoding.UTF8);

            if (previousTrees.TryGetValue(source.Path, out var previousTree)
                && previousTree.GetText(token).ContentEquals(sourceText)
            )
            {
                syntaxTrees.Add(previousTree);
                continue;
            }

            syntaxTrees.Add(CSharpSyntaxTree.ParseText(sourceText, s_parseOptions, source.Path, token));
        }

        AddInfrastructureTree(STACK_TRACE_HIDDEN_POLYFILL_PATH);
        AddInfrastructureTree(VARIANT_POLYFILL_PATH);

        return CSharpCompilation.Create(
              assemblyName
            , syntaxTrees
            , references
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
        );

        void AddInfrastructureTree(string path)
        {
            if (previousTrees.TryGetValue(path, out var tree))
            {
                syntaxTrees.Add(tree);
            }
        }
    }

    internal static GeneratorDriver CreateDriver(
          IReadOnlyList<IIncrementalGenerator> generators
        , CSharpParseOptions? parseOptions = null
    )
        => CSharpGeneratorDriver.Create(
              generators: generators.Select(static generator => generator.AsSourceGenerator()).ToArray()
            , parseOptions: parseOptions ?? s_parseOptions
            , driverOptions: new GeneratorDriverOptions(disabledOutputs: default, trackIncrementalGeneratorSteps: true)
        );

    internal static DriverRun RunDriver(
          GeneratorDriver driver
        , Compilation compilation
        , CancellationToken token
        , bool includeGeneratedSourcesOnFailure = false
    )
    {
        driver = driver.RunGeneratorsAndUpdateCompilation(
              compilation
            , out var outputCompilation
            , out var diagnostics
            , token
        );
        var result = driver.GetRunResult();

        if (diagnostics.IsDefaultOrEmpty == false)
        {
            Assert.Fail(
                "Generator driver diagnostics:\n"
                    + string.Join("\n", diagnostics.Select(static value => value.ToString()))
            );
        }

        foreach (var generatorResult in result.Results)
        {
            if (generatorResult.Exception is not null)
            {
                Assert.Fail($"Generator threw an exception:\n{generatorResult.Exception}");
            }

            if (generatorResult.Diagnostics.IsDefaultOrEmpty == false)
            {
                Assert.Fail(
                    "Generator diagnostics:\n"
                        + string.Join("\n", generatorResult.Diagnostics.Select(static value => value.ToString()))
                );
            }
        }

        if (includeGeneratedSourcesOnFailure)
        {
            AssertCleanCompilation(outputCompilation, result, token);
        }
        else
        {
            AssertCleanCompilation(outputCompilation, token);
        }

        return new DriverRun(driver, compilation, outputCompilation, result);
    }

    private static DriverRun RunContractTransition(
          GeneratorDriver driver
        , GeneratorTransitionExpectation expectation
        , IEnumerable<MetadataReference> references
        , IReadOnlyList<IIncrementalGenerator> generators
        , DriverRun? previousRun
        , GeneratorContractCase contract
        , CancellationToken token
        , IReadOnlyList<string>? expectedChangedHintNames = null
        , bool? expectStableOutputSteps = null
        , bool verifyOwnedTrackingNames = true
        , Compilation? inputCompilation = null
    )
    {
        var compilation = inputCompilation ?? CreateTransitionCompilation(
              previousRun?.InputCompilation
            , expectation.Sources
            , INPUT_ASSEMBLY_NAME
            , references
            , token
        );
        var run = RunDriver(driver, compilation, token);
        VerifyContractOutputs(run.Result, generators, expectation.Outputs);

        if (previousRun is { } previous)
        {
            VerifyChangedOutputs(
                  previous.Result
                , run.Result
                , generators
                , expectedChangedHintNames ?? expectation.ChangedHintNames
            );
        }

        if (verifyOwnedTrackingNames)
        {
            VerifyOwnedTrackingNames(run.Result, contract.OwnedTrackingNames);
        }

        if (expectStableOutputSteps ?? expectation.ExpectStableOutputSteps)
        {
            VerifyStableOwnedOutputSteps(run.Result, contract.OwnedOutputTrackingNames);
        }

        return run;
    }

    private static void VerifyContractOutputs(
          GeneratorDriverRunResult result
        , IReadOnlyList<IIncrementalGenerator> generators
        , IReadOnlyList<GeneratorExpectedOutput> expectedOutputs
    )
    {
        var actualSources = GetActualSources(result, generators);
        var generatorType = generators[0].GetType();
        var expectedKeys = expectedOutputs
            .Select(output => (generatorType, output.HintName))
            .ToHashSet();
        var actualKeys = actualSources.Keys.ToHashSet();

        if (expectedKeys.SetEquals(actualKeys) == false)
        {
            Assert.Fail(
                $"Generated output keys differ.\nExpected: {FormatKeys(expectedKeys)}\nActual: {FormatKeys(actualKeys)}"
            );
        }

        foreach (var expected in expectedOutputs)
        {
            var actual = actualSources[(generatorType, expected.HintName)];
            CollectionAssert.AreEqual(
                  Encoding.UTF8.GetBytes(expected.Source)
                , Encoding.UTF8.GetBytes(actual.Source)
                , $"Generated source differs for '{expected.HintName}'."
            );
        }
    }

    private static void VerifyChangedOutputs(
          GeneratorDriverRunResult previousResult
        , GeneratorDriverRunResult currentResult
        , IReadOnlyList<IIncrementalGenerator> generators
        , IReadOnlyList<string> expectedChangedHintNames
    )
    {
        var previousSources = GetActualSources(previousResult, generators)
            .ToDictionary(static pair => pair.Key.HintName, static pair => pair.Value.Source);
        var currentSources = GetActualSources(currentResult, generators)
            .ToDictionary(static pair => pair.Key.HintName, static pair => pair.Value.Source);
        var changedHintNames = previousSources.Keys
            .Concat(currentSources.Keys)
            .Distinct(StringComparer.Ordinal)
            .Where(hintName =>
                previousSources.TryGetValue(hintName, out var previousSource) == false
                || currentSources.TryGetValue(hintName, out var currentSource) == false
                || string.Equals(previousSource, currentSource, StringComparison.Ordinal) == false
            )
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        var expected = expectedChangedHintNames
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        CollectionAssert.AreEqual(expected, changedHintNames, "Changed generated outputs differ.");
    }

    private static void AssertObservableMarkerTransition(
          GeneratorDriverRunResult baseline
        , GeneratorDriverRunResult markerAdded
    )
    {
        var baselineSources = GetSourcesByHint(baseline.Results[0]);
        var markedSources = GetSourcesByHint(markerAdded.Results[0]);
        AssertSourceChanged(baselineSources, markedSources, ".ObservableModel.", "Set_Value_WithoutNotify");
        AssertSourceChanged(baselineSources, markedSources, ".ObservablePropertyBag.", "Set_Value_WithoutNotify");
        AssertSourceChanged(baselineSources, markedSources, ".MvvmEditorMetadata.", "true");
        AssertSourceUnchanged(baselineSources, markedSources, ".MvvmKnownNames.");
        AssertSourceUnchanged(baselineSources, markedSources, ".MvvmPropertyBagRegistry.");
    }

    private static Dictionary<string, string> GetSourcesByHint(GeneratorRunResult result)
        => result.GeneratedSources.ToDictionary(
              static source => source.HintName
            , static source => source.SourceText.ToString()
            , StringComparer.Ordinal
        );

    private static void AssertSourceChanged(
          IReadOnlyDictionary<string, string> baseline
        , IReadOnlyDictionary<string, string> marked
        , string hintFragment
        , string requiredMarkedFragment
    )
    {
        var hint = FindHint(baseline, hintFragment);
        Assert.IsTrue(marked.TryGetValue(hint, out var markedSource));
        Assert.AreNotEqual(baseline[hint], markedSource, $"'{hint}' did not react to [TwoWay].");
        StringAssert.Contains(markedSource, requiredMarkedFragment);
    }

    private static void AssertSourceUnchanged(
          IReadOnlyDictionary<string, string> baseline
        , IReadOnlyDictionary<string, string> marked
        , string hintFragment
    )
    {
        var hint = FindHint(baseline, hintFragment);
        Assert.IsTrue(marked.TryGetValue(hint, out var markedSource));
        Assert.AreEqual(baseline[hint], markedSource, $"'{hint}' changed without a [TwoWay] dependency.");
    }

    private static string FindHint(IReadOnlyDictionary<string, string> sources, string fragment)
    {
        var hints = sources.Keys.Where(hint => hint.Contains(fragment, StringComparison.Ordinal)).ToArray();
        Assert.AreEqual(1, hints.Length, $"Expected one generated hint containing '{fragment}'.");
        return hints[0];
    }

    private static void AssertSourceSetsEqual(
          GeneratorDriverRunResult expected
        , GeneratorDriverRunResult actual
        , IReadOnlyList<IIncrementalGenerator> generators
    )
    {
        var expectedSources = GetActualSources(expected, generators);
        var actualSources = GetActualSources(actual, generators);
        CollectionAssert.AreEquivalent(expectedSources.Keys.ToArray(), actualSources.Keys.ToArray());

        foreach (var pair in expectedSources)
        {
            Assert.AreEqual(pair.Value.Source, actualSources[pair.Key].Source, pair.Key.HintName);
        }
    }

    private static void AssertStableOutputSteps(GeneratorRunResult result, string trackingName)
    {
        Assert.IsTrue(TryGetOwnedTrackingSteps(result, trackingName, out var steps));
        var reasons = steps.SelectMany(static step => step.Outputs).Select(static output => output.Reason).ToArray();
        Assert.IsTrue(reasons.Length > 0, $"'{trackingName}' recorded no outputs.");
        Assert.IsTrue(reasons.All(static reason => reason is IncrementalStepRunReason.Cached
            or IncrementalStepRunReason.Unchanged),
            $"'{trackingName}' reasons: {string.Join(", ", reasons)}");
    }

    private static void VerifyOwnedTrackingNames(
          GeneratorDriverRunResult result
        , IReadOnlyList<string> ownedTrackingNames
        , int generatorIndex = 0
    )
    {
        Assert.IsTrue(generatorIndex >= 0 && generatorIndex < result.Results.Length);
        var generatorResult = result.Results[generatorIndex];
        var actualNames = generatorResult.TrackedSteps.Keys
            .Concat(generatorResult.TrackedOutputSteps.Keys)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var trackingName in ownedTrackingNames)
        {
            Assert.IsTrue(
                actualNames.Contains(trackingName),
                $"Owned tracking name '{trackingName}' was not recorded."
            );
        }
    }

    private static void VerifyStableOwnedOutputSteps(
          GeneratorDriverRunResult result
        , IReadOnlyList<string> ownedOutputTrackingNames
        , int generatorIndex = 0
    )
    {
        Assert.IsTrue(generatorIndex >= 0 && generatorIndex < result.Results.Length);
        var generatorResult = result.Results[generatorIndex];

        foreach (var trackingName in ownedOutputTrackingNames)
        {
            Assert.IsTrue(
                TryGetOwnedTrackingSteps(generatorResult, trackingName, out var steps),
                $"Owned output tracking name '{trackingName}' was not recorded."
            );
            var outputs = steps.SelectMany(static step => step.Outputs).ToArray();
            Assert.IsTrue(outputs.Length > 0, $"Owned output step '{trackingName}' recorded no outputs.");

            foreach (var output in outputs)
            {
                Assert.IsTrue(
                    output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
                    $"Unexpected incremental reason for '{trackingName}': {output.Reason}."
                );
            }
        }
    }

    private static bool TryGetOwnedTrackingSteps(
          GeneratorRunResult result
        , string trackingName
        , out ImmutableArray<IncrementalGeneratorRunStep> steps
    )
    {
        if (result.TrackedOutputSteps.TryGetValue(trackingName, out steps))
        {
            return true;
        }

        return result.TrackedSteps.TryGetValue(trackingName, out steps);
    }

    private static IIncrementalGenerator CreateGenerator(Type generatorType)
    {
        Assert.IsTrue(
            typeof(IIncrementalGenerator).IsAssignableFrom(generatorType),
            $"{generatorType.FullName} is not an incremental generator."
        );
        var generator = Activator.CreateInstance(generatorType) as IIncrementalGenerator;
        Assert.IsNotNull(generator, $"Could not create {generatorType.FullName}.");
        return generator;
    }

    private static string[] GetOwnedTrackingNames(Type generatorType)
    {
        if (string.Equals(generatorType.Name, "ObservablePropertyGenerator", StringComparison.Ordinal))
        {
            return new[] {
                "ObservablePropertyGenerator.Compilation",
                "ObservablePropertyGenerator.Candidates",
                "ObservablePropertyGenerator.ValidModels",
                "ObservablePropertyGenerator.ModelInputs",
                "ObservablePropertyGenerator.ModelOutputs",
                "ObservablePropertyGenerator.PropertyBagInputs",
                "ObservablePropertyGenerator.PropertyNameInputs",
                "ObservablePropertyGenerator.PropertyBagRegistrationInputs",
            };
        }

        if (string.Equals(generatorType.Name, "RelayCommandGenerator", StringComparison.Ordinal))
        {
            return new[] {
                "RelayCommandGenerator.Compilation",
                "RelayCommandGenerator.Candidates",
                "RelayCommandGenerator.ValidMethods",
                "RelayCommandGenerator.CommandInputs",
                "RelayCommandGenerator.CommandOutputs",
                "RelayCommandGenerator.CommandNameInputs",
            };
        }

        if (string.Equals(
              generatorType.FullName
            , "EncosyTower.Mvvm.Generators.InternalVariants.InternalVariantGenerator"
            , StringComparison.Ordinal
        ))
        {
            return new[] {
                "InternalVariantGenerator.Compilation",
                "InternalVariantGenerator.ObservableCandidates",
                "InternalVariantGenerator.ObservableOccurrences",
                "InternalVariantGenerator.Occurrences",
                "InternalVariantGenerator.VariantSupport",
                "InternalVariantGenerator.WithCompilation",
                "InternalVariantGenerator.TypeOutputs",
                "InternalVariantGenerator.Registration",
            };
        }

        if (string.Equals(generatorType.Name, "InternalStringAdapterGenerator", StringComparison.Ordinal))
        {
            return new[] {
                "InternalStringAdapterGenerator.Compilation",
                "InternalStringAdapterGenerator.ObservableCandidates",
                "InternalStringAdapterGenerator.ObservableOccurrences",
                "InternalStringAdapterGenerator.WithBinderOccurrences",
                "InternalStringAdapterGenerator.VariantSupport",
                "InternalStringAdapterGenerator.StringConversionTypes",
                "InternalStringAdapterGenerator.WithCompilation",
                "InternalStringAdapterGenerator.TypeOutputs",
            };
        }

        return new[] {
            $"{generatorType.Name}.Candidates",
            $"{generatorType.Name}.ValidSpecs",
            $"{generatorType.Name}.Outputs",
        };
    }

    private static string[] GetOwnedOutputTrackingNames(Type generatorType)
    {
        if (string.Equals(generatorType.Name, "ObservablePropertyGenerator", StringComparison.Ordinal))
        {
            return new[] {
                "ObservablePropertyGenerator.ModelOutputs",
                "ObservablePropertyGenerator.PropertyBagInputs",
                "ObservablePropertyGenerator.PropertyNameInputs",
                "ObservablePropertyGenerator.PropertyBagRegistrationInputs",
            };
        }

        if (string.Equals(generatorType.Name, "RelayCommandGenerator", StringComparison.Ordinal))
        {
            return new[] {
                "RelayCommandGenerator.CommandOutputs",
                "RelayCommandGenerator.CommandNameInputs",
            };
        }

        if (string.Equals(
              generatorType.FullName
            , "EncosyTower.Mvvm.Generators.InternalVariants.InternalVariantGenerator"
            , StringComparison.Ordinal
        ))
        {
            return new[] { "InternalVariantGenerator.TypeOutputs", "InternalVariantGenerator.Registration" };
        }

        if (string.Equals(generatorType.Name, "InternalStringAdapterGenerator", StringComparison.Ordinal))
        {
            return new[] { "InternalStringAdapterGenerator.TypeOutputs" };
        }

        return new[] { $"{generatorType.Name}.Outputs" };
    }

    private static void VerifyNoContractOutputs(
          GeneratorDriverRunResult result
        , IReadOnlyList<IIncrementalGenerator> generators
    )
    {
        var actual = GetActualSources(result, generators);
        Assert.AreEqual(0, actual.Count, "Expected no owned generated outputs.");
    }

    private static void VerifyProductionContractOutputs(
          GeneratorDriverRunResult result
        , IReadOnlyList<IIncrementalGenerator> generators
        , IReadOnlyList<ProductionGeneratorExpectedOutput> expectedOutputs
    )
    {
        var actual = GetActualSources(result, generators);
        var expectedKeys = expectedOutputs
            .Select(static output => (output.GeneratorType, output.HintName))
            .ToHashSet();
        Assert.IsTrue(
            expectedKeys.SetEquals(actual.Keys),
            $"Generated output keys differ.\nExpected: {FormatKeys(expectedKeys)}\nActual: {FormatKeys(actual.Keys)}"
        );

        foreach (var expected in expectedOutputs)
        {
            CollectionAssert.AreEqual(
                  Encoding.UTF8.GetBytes(expected.Source)
                , Encoding.UTF8.GetBytes(actual[(expected.GeneratorType, expected.HintName)].Source)
                , $"Generated source differs for '{expected.GeneratorType.Name}#{expected.HintName}'."
            );
        }
    }

    private static void VerifyEquivalentContractOutputs(
          GeneratorDriverRunResult expectedResult
        , GeneratorDriverRunResult actualResult
        , IReadOnlyList<IIncrementalGenerator> generators
    )
    {
        var expected = GetActualSources(expectedResult, generators);
        var actual = GetActualSources(actualResult, generators);
        Assert.IsTrue(expected.Keys.ToHashSet().SetEquals(actual.Keys));

        foreach (var pair in expected)
        {
            Assert.AreEqual(pair.Value.Path, actual[pair.Key].Path);
            Assert.AreEqual(pair.Value.Source, actual[pair.Key].Source);
        }
    }

    private static void VerifyEveryContractOutputChanged(
          GeneratorDriverRunResult previousResult
        , GeneratorDriverRunResult currentResult
        , IReadOnlyList<IIncrementalGenerator> generators
    )
    {
        var previous = GetActualSources(previousResult, generators);
        var current = GetActualSources(currentResult, generators);
        var keys = previous.Keys.Concat(current.Keys).ToHashSet();
        Assert.IsTrue(keys.Count > 0, "The relevant edit must own generated output.");

        foreach (var key in keys)
        {
            var unchanged = previous.TryGetValue(key, out var previousOutput)
                && current.TryGetValue(key, out var currentOutput)
                && string.Equals(previousOutput.Source, currentOutput.Source, StringComparison.Ordinal);
            Assert.IsFalse(
                unchanged,
                $"Relevant semantic edit left an owned output unchanged: {key.GeneratorType.Name}#{key.HintName}."
            );
        }
    }

    private static void VerifyRelevantContractOutputsChanged(
          GeneratorDriverRunResult previousResult
        , GeneratorDriverRunResult currentResult
        , IReadOnlyList<IIncrementalGenerator> generators
    )
    {
        var previous = GetActualSources(previousResult, generators);
        var current = GetActualSources(currentResult, generators);
        var keys = previous.Keys.Concat(current.Keys).ToHashSet();
        var changedCount = 0;

        foreach (var key in keys)
        {
            var unchanged = previous.TryGetValue(key, out var previousOutput)
                && current.TryGetValue(key, out var currentOutput)
                && string.Equals(previousOutput.Source, currentOutput.Source, StringComparison.Ordinal);

            if (unchanged)
            {
                Assert.IsTrue(
                      key.HintName.Contains(".MvvmPropertyNames.", StringComparison.Ordinal)
                    || key.HintName.Contains(".MvvmCommandNames.", StringComparison.Ordinal)
                    , $"Relevant semantic edit left an owned output unchanged: " +
                        $"{key.GeneratorType.Name}#{key.HintName}."
                );
            }
            else
            {
                changedCount++;
            }
        }

        Assert.IsTrue(changedCount > 0, "The relevant edit must change observable generated output.");
    }

    private static void VerifySourceLocationOutputChanged(
          GeneratorDriverRunResult previousResult
        , GeneratorDriverRunResult currentResult
        , IReadOnlyList<IIncrementalGenerator> generators
    )
    {
        var previous = GetActualSources(previousResult, generators);
        var current = GetActualSources(currentResult, generators);
        var changed = false;

        foreach (var key in previous.Keys.Concat(current.Keys).ToHashSet())
        {
            if (key.HintName.Contains(".ObservableEditorMetadata.", StringComparison.Ordinal) == false)
            {
                continue;
            }

            changed = previous.TryGetValue(key, out var before) == false
                || current.TryGetValue(key, out var after) == false
                || string.Equals(before.Source, after.Source, StringComparison.Ordinal) == false;
        }

        Assert.IsTrue(changed, "The source-location edit must change the MVVM Editor metadata aggregate.");
    }

    private static void ValidateContract(GeneratorContractCase contract)
    {
        Assert.IsTrue(contract.OwnedTrackingNames.Count > 0, "Owned tracking names are required.");
        Assert.IsTrue(contract.OwnedOutputTrackingNames.Count > 0, "Owned output tracking names are required.");
        Assert.AreEqual(0, contract.MarkerOnly.Outputs.Count, "Marker-only input must produce no output.");

        var trackingNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var trackingName in contract.OwnedTrackingNames)
        {
            Assert.IsTrue(trackingNames.Add(trackingName), $"Duplicate owned tracking name: {trackingName}.");
        }

        foreach (var outputTrackingName in contract.OwnedOutputTrackingNames)
        {
            Assert.IsTrue(
                trackingNames.Contains(outputTrackingName),
                $"Output tracking name '{outputTrackingName}' is not an owned tracking name."
            );
        }

        ValidateTransition(contract.MarkerOnly);
        ValidateTransition(contract.Valid);
        ValidateTransition(contract.UnrelatedEdit);
        ValidateTransition(contract.FileMove);
        ValidateTransition(contract.PrecedingLineEdit);
        ValidateTransition(contract.OrderEdit);
        ValidateTransition(contract.RelevantEdit);
        ValidateTransition(contract.Removal);
    }

    private static void ValidateTransition(GeneratorTransitionExpectation expectation)
    {
        var paths = new HashSet<string>(StringComparer.Ordinal);

        foreach (var source in expectation.Sources)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(source.Path), "Input paths must be explicit.");
            Assert.IsTrue(paths.Add(source.Path), $"Duplicate input path: {source.Path}.");
        }

        var hintNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var output in expectation.Outputs)
        {
            Assert.IsTrue(hintNames.Add(output.HintName), $"Duplicate expected hint name: {output.HintName}.");
        }
    }

    private static void AssertCleanCompilation(Compilation compilation, CancellationToken token)
    {
        var diagnostics = compilation.GetDiagnostics(token)
            .Where(static diagnostic => diagnostic.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Error)
            .ToArray();

        if (diagnostics.Length > 0)
        {
            Assert.Fail(
                "Output compilation diagnostics:\n"
                    + string.Join("\n", diagnostics.Select(static value => value.ToString()))
            );
        }
    }

    private static void AssertCleanCompilation(
          Compilation compilation
        , GeneratorDriverRunResult result
        , CancellationToken token
    )
    {
        var diagnostics = compilation.GetDiagnostics(token)
            .Where(static diagnostic => diagnostic.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Error)
            .ToArray();

        if (diagnostics.Length < 1)
        {
            return;
        }

        var sources = result.Results
            .SelectMany(static generator => generator.GeneratedSources)
            .Select(static source => $"HINT: {source.HintName}\n{source.SourceText}");
        Assert.Fail(
            "Output compilation diagnostics:\n"
                + string.Join("\n", diagnostics.Select(static value => value.ToString()))
                + "\n\nGenerated sources:\n"
                + string.Join("\n\n", sources)
        );
    }

    private static async Task VerifyExpectedSourcesAsync(
          GeneratorDriverRunResult result
        , IReadOnlyList<IIncrementalGenerator> generators
        , IReadOnlyList<ExpectedGeneratedSource> expectedSources
        , bool verifyDebuggingAliasContract
        , CancellationToken token
    )
    {
        var actualSources = GetActualSources(result, generators);
        var expectedKeys = expectedSources.Select(static expected => (
            expected.GeneratorType,
            expected.HintName
        )).ToHashSet();
        var actualKeys = actualSources.Keys.ToHashSet();

        if (expectedKeys.SetEquals(actualKeys) == false)
        {
            Assert.Fail(
                $"Generated output keys differ.\nExpected: {FormatKeys(expectedKeys)}\nActual: {FormatKeys(actualKeys)}"
            );
        }

        var snapshotFailures = new List<string>();

        foreach (var expected in expectedSources)
        {
            var actual = actualSources[(expected.GeneratorType, expected.HintName)];

            if (verifyDebuggingAliasContract)
            {
                VerifyDebuggingAliasContract(actual.Source);
            }

            var generatorType = expected.GeneratorType;
            var expectedPath = NormalizePath(
                $"{generatorType.Assembly.GetName().Name}/{generatorType.FullName}/{expected.HintName}"
            );
            Assert.AreEqual(expectedPath, NormalizePath(actual.Path));
            var snapshotFailure = await GeneratedSourceSnapshot.VerifyAsync(
                  actual.Source
                , expected.VerifiedPath
                , token
            );

            if (snapshotFailure is not null)
            {
                snapshotFailures.Add(snapshotFailure);
            }
        }

        if (snapshotFailures.Count > 0)
        {
            Assert.Fail(string.Join("\n\n", snapshotFailures));
        }
    }

    private static void VerifyDebuggingAliasContract(string source)
    {
        const string ALIAS = "using g__ETDBG = global::EncosyTower.Debugging;";

        Assert.AreEqual(1, CountOrdinal(source, ALIAS));
        Assert.IsTrue(source.Contains("g__ETDBG.ThrowHelper", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("using ETDBG =", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("using EncosyTower.Debugging;", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("using ThrowHelper =", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("g__g__", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("global::EncosyTower.Debugging.ThrowHelper", StringComparison.Ordinal));
    }

    private static int CountOrdinal(string source, string value)
    {
        var count = 0;
        var startIndex = 0;

        while ((startIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal)) >= 0)
        {
            count++;
            startIndex += value.Length;
        }

        return count;
    }

    private static void VerifyIncrementality(GeneratorDriverRunResult result)
    {
        foreach (var generatorResult in result.Results)
        {
            Assert.IsTrue(
                generatorResult.TrackedOutputSteps.Count > 0,
                "No tracked output steps were recorded for a positive generator run."
            );
            VerifyIncrementalSteps(generatorResult.TrackedOutputSteps);
        }
    }

    private static void VerifyIncrementalSteps(
        IEnumerable<KeyValuePair<string, ImmutableArray<IncrementalGeneratorRunStep>>> trackedSteps
    )
    {
        foreach (var pair in trackedSteps)
        {
            foreach (var output in pair.Value.SelectMany(static step => step.Outputs))
            {
                Assert.IsTrue(
                    output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
                    $"Unexpected incremental reason for '{pair.Key}': {output.Reason}."
                );
            }
        }
    }

    private static void VerifySecondRunSources(
          GeneratorDriverRunResult firstResult
        , GeneratorDriverRunResult secondResult
        , IReadOnlyList<IIncrementalGenerator> generators
    )
    {
        var firstSources = GetActualSources(firstResult, generators);
        var secondSources = GetActualSources(secondResult, generators);
        CollectionAssert.AreEquivalent(firstSources.Keys.ToArray(), secondSources.Keys.ToArray());

        foreach (var pair in firstSources)
        {
            var second = secondSources[pair.Key];
            Assert.AreEqual(NormalizePath(pair.Value.Path), NormalizePath(second.Path));
            Assert.AreEqual(pair.Value.Source, second.Source);
        }
    }

    private static void VerifyRemovedSources(
          GeneratorDriverRunResult firstResult
        , GeneratorDriverRunResult removalResult
        , IReadOnlyList<IIncrementalGenerator> generators
    )
    {
        var firstSources = GetActualSources(firstResult, generators);
        var removalSources = GetActualSources(removalResult, generators);

        foreach (var key in firstSources.Keys)
        {
            Assert.IsFalse(
                removalSources.ContainsKey(key),
                $"Removed candidate left stale generated output: {key.GeneratorType.FullName}, {key.HintName}."
            );
        }
    }

    private static Dictionary<(Type GeneratorType, string HintName), ActualGeneratedSource> GetActualSources(
          GeneratorDriverRunResult result
        , IReadOnlyList<IIncrementalGenerator> generators
    )
    {
        Assert.AreEqual(generators.Count, result.Results.Length);
        var sources = new Dictionary<(Type GeneratorType, string HintName), ActualGeneratedSource>();

        for (var generatorIndex = 0; generatorIndex < result.Results.Length; generatorIndex++)
        {
            var generatorType = generators[generatorIndex].GetType();

            foreach (var generatedSource in result.Results[generatorIndex].GeneratedSources)
            {
                var key = (generatorType, generatedSource.HintName);

                if (sources.TryAdd(
                        key,
                        new ActualGeneratedSource(
                              generatedSource.SyntaxTree.FilePath
                            , generatedSource.SourceText.ToString()
                        )
                    ) == false
                )
                {
                    Assert.Fail($"Duplicate generated output: {generatorType.FullName}, {generatedSource.HintName}");
                }
            }
        }

        return sources;
    }

    private static void ValidateExpectedSources(IReadOnlyList<ExpectedGeneratedSource> expectedSources)
    {
        var keys = new HashSet<(Type GeneratorType, string HintName)>();
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var expected in expectedSources)
        {
            Assert.IsTrue(
                keys.Add((expected.GeneratorType, expected.HintName)),
                $"Duplicate expected output: {expected.GeneratorType.FullName}, {expected.HintName}"
            );
            Assert.IsTrue(paths.Add(expected.VerifiedPath), $"Duplicate snapshot path: {expected.VerifiedPath}");
        }
    }

    private static string NormalizePath(string path)
        => path.Replace('\\', '/');

    private static string FormatKeys(IEnumerable<(Type GeneratorType, string HintName)> keys)
        => string.Join(", ", keys.Select(static key => $"({key.GeneratorType.FullName}, {key.HintName})"));

    internal readonly record struct DriverRun(
          GeneratorDriver Driver
        , Compilation InputCompilation
        , Compilation OutputCompilation
        , GeneratorDriverRunResult Result
    );

    private readonly record struct ActualGeneratedSource(string Path, string Source);
}
