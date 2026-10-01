using EncosyTower.Processing.Generators;
using EncosyTower.SourceGen.Tests.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Tests.Processing;

[TestClass]
public sealed class ProcessingRequestGeneratorTests
{
    [TestMethod]
    public async Task ApiMode_GatesGeneratedInterfacesAndMethods()
    {
        var sync = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both)]
            public partial class SyncRequest { }
            """);
        var asyncOnly = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Async, State = StateMode.Both)]
            public partial class AsyncRequest { }
            """);
        var both = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Both, State = StateMode.Both)]
            public partial class BothRequest { }
            """);
        var invalid = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing((ApiMode)99, State = StateMode.Both)]
            public partial class InvalidRequest { }
            """);

        ProcessingRequestFixture.AssertNoOutputErrors(sync);
        ProcessingRequestFixture.AssertNoOutputErrors(asyncOnly);
        ProcessingRequestFixture.AssertNoOutputErrors(both);
        ProcessingRequestFixture.AssertNoOutputErrors(invalid);
        Assert.AreEqual(14, GetGeneratedMethods(sync).Length);
        Assert.AreEqual(14, GetGeneratedMethods(asyncOnly).Length);
        Assert.AreEqual(28, GetGeneratedMethods(both).Length);
        Assert.AreEqual(0, GetGeneratedMethods(invalid).Length);
        Assert.IsTrue(GetGeneratedMethods(asyncOnly).All(static method => method.Ancestors()
            .OfType<StructDeclarationSyntax>()
            .Any(static declaration => declaration.Identifier.ValueText == "Async")));
        Assert.IsFalse(asyncOnly.CombinedSource.Contains(" : g__ETP.IRequest\n", StringComparison.Ordinal));
        Assert.IsFalse(invalid.CombinedSource.Contains(" : g__ETP.IRequest", StringComparison.Ordinal));
        Assert.IsFalse(invalid.CombinedSource.Contains(" : g__ETP.IAsyncRequest", StringComparison.Ordinal));
        StringAssert.Contains(sync.CombinedSource, " : g__ETP.IRequest");
        StringAssert.Contains(asyncOnly.CombinedSource, " : g__ETP.IAsyncRequest");
        StringAssert.Contains(both.CombinedSource, " : g__ETP.IRequest");
        StringAssert.Contains(both.CombinedSource, " : g__ETP.IAsyncRequest");
    }

    [TestMethod]
    public async Task StateMode_GatesGeneratedRegisterCounterpartsOnly()
    {
        var stateless = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.CodeGen;
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Both, State = StateMode.Stateless, Scope = typeof(IOrderScope))]
            public partial class StatelessRequest { }
            """);
        var stateful = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.CodeGen;
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Both, State = StateMode.Stateful, Scope = typeof(IOrderScope))]
            public partial class StatefulRequest { }
            """);
        var both = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.CodeGen;
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Both, State = StateMode.Both, Scope = typeof(IOrderScope))]
            public partial class BothStateRequest { }
            """);
        var omitted = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Both, Scope = typeof(IOrderScope))]
            public partial class OmittedStateRequest { }
            """);
        var invalid = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.CodeGen;
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Both, State = (StateMode)99, Scope = typeof(IOrderScope))]
            public partial class InvalidStateRequest { }
            """);

        ProcessingRequestFixture.AssertNoOutputErrors(stateless);
        ProcessingRequestFixture.AssertNoOutputErrors(stateful);
        ProcessingRequestFixture.AssertNoOutputErrors(both);
        ProcessingRequestFixture.AssertNoOutputErrors(omitted);
        ProcessingRequestFixture.AssertNoOutputErrors(invalid);
        Assert.AreEqual(4, CountRegister(stateless));
        Assert.AreEqual(8, CountRegister(stateful));
        Assert.AreEqual(12, CountRegister(both));
        Assert.AreEqual(12, CountRegister(omitted));
        Assert.AreEqual(12, CountRegister(invalid));
        Assert.AreEqual(4, CountProcessFamily(stateless));
        Assert.AreEqual(4, CountProcessFamily(stateful));
        Assert.AreEqual(4, CountProcessFamily(both));
    }

    private static int CountRegister(ProcessingRun run)
        => GetGeneratedMethods(run).Count(static method => method.Identifier.ValueText == "Register");

    private static int CountProcessFamily(ProcessingRun run)
        => GetGeneratedMethods(run).Count(static method =>
            method.Identifier.ValueText is "Process" or "TryProcess");

    [TestMethod]
    public async Task RepeatedAttributes_SelectModesIndependentlyAndIgnoreInvalidOccurrence()
    {
        var run = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            [Processing(ApiMode.Async, State = StateMode.Both, Scope = typeof(AbstractScope))]
            [Processing((ApiMode)99, State = StateMode.Both, Scope = typeof(ViewScope))]
            public partial class Request { }
            """);

        AssertHintRoles(run, requestCount: 1, scopeCount: 2);
        ProcessingRequestFixture.AssertNoOutputErrors(run);
        var sync = GetScopeSources(run).Single(static source => source.SourceText.ToString()
            .Contains("global::TestProject.IOrderScope", StringComparison.Ordinal));
        var asyncOnly = GetScopeSources(run).Single(static source => source.SourceText.ToString()
            .Contains("global::TestProject.AbstractScope", StringComparison.Ordinal));
        Assert.AreEqual(
              8
            , sync.SyntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Count()
        );
        Assert.AreEqual(
              8
            , asyncOnly.SyntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Count()
        );
        Assert.IsFalse(sync.SourceText.ToString().Contains("struct Async", StringComparison.Ordinal));
        StringAssert.Contains(asyncOnly.SourceText.ToString(), "struct Async");
        Assert.IsFalse(run.CombinedSource.Contains("global::TestProject.ViewScope", StringComparison.Ordinal));
        StringAssert.Contains(run.CombinedSource, " : g__ETP.IRequest");
        StringAssert.Contains(run.CombinedSource, " : g__ETP.IAsyncRequest");
    }

    [TestMethod]
    public async Task GlobalVoidRequest_GeneratesFourteenSynchronousMethodsAndCompiles()
    {
        var run = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both)]
            public readonly partial record struct Ping { }
            """);

        AssertHintRoles(run, requestCount: 1, scopeCount: 1);
        ProcessingRequestFixture.AssertNoOutputErrors(run);
        var methods = GetGeneratedMethods(run);
        Assert.AreEqual(14, methods.Length);
        Assert.AreEqual(8, methods.Count(HasHubParameter));
        Assert.AreEqual(6, methods.Count(static method => HasHubParameter(method) == false));
        Assert.AreEqual(10, methods.Count(static method => method.Identifier.ValueText == "Register"));
        Assert.AreEqual(2, methods.Count(static method => method.Identifier.ValueText == "Process"));
        Assert.AreEqual(2, methods.Count(static method => method.Identifier.ValueText == "TryProcess"));
        StringAssert.Contains(run.CombinedSource, "g__ETP.GlobalProcessor.Instance.Global()");
        StringAssert.Contains(run.CombinedSource, "in g__ETP.Processor.Hub<g__ETC.GlobalScope> hub");
        Assert.IsFalse(
            run.CombinedSource.Contains(
                  "Hub<global::EncosyTower.Common.GlobalScope>"
                , StringComparison.Ordinal
            )
        );
    }

    [TestMethod]
    public async Task DefaultAndExplicitGlobalScopes_UseDistinctTypeRenderingAndCompile()
    {
        var run = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Common;
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both)]
            public readonly partial record struct DefaultGlobalRequest;

            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(GlobalScope))]
            public readonly partial record struct ExplicitGlobalRequest;
            """);

        AssertHintRoles(run, requestCount: 2, scopeCount: 2);
        ProcessingRequestFixture.AssertNoOutputErrors(run);
        StringAssert.Contains(run.CombinedSource, "Hub<g__ETC.GlobalScope>");
        StringAssert.Contains(run.CombinedSource, "Hub<global::EncosyTower.Common.GlobalScope>");
    }

    [TestMethod]
    public async Task TypedResultAsyncRequest_GeneratesSyncAndAsyncHubFamiliesAndCompiles()
    {
        var run = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Both, State = StateMode.Both, Scope = typeof(IOrderScope))]
            public readonly partial record struct Query : IRequest<int> { }

            public static class Consumer
            {
                public static void Use(Processor processor, IOrderScope scope, Query query)
                {
                    var globalHub = GlobalProcessor.Instance.Scope(scope);
                    _ = Query.Process(in globalHub, query);
                    var ownedHub = processor.Scope(scope);
                    _ = Query.TryProcess(in ownedHub, query);
                }
            }
            """);

        AssertHintRoles(run, requestCount: 1, scopeCount: 1);
        ProcessingRequestFixture.AssertNoOutputErrors(run);
        var methods = GetGeneratedMethods(run);
        Assert.AreEqual(16, methods.Length);
        Assert.AreEqual(16, methods.Count(HasHubParameter));
        StringAssert.Contains(run.CombinedSource, "g__S.Func<");
        StringAssert.Contains(run.CombinedSource, "g__ETT.UnityTask<int>");
        StringAssert.Contains(run.CombinedSource, "g__ETC.Option<int>");
        StringAssert.Contains(run.CombinedSource, "ProcessAsync<Async, int>");
        StringAssert.Contains(run.CombinedSource, "TryProcessAsync<Async, int>");
        StringAssert.Contains(
              run.CombinedSource
            , "[g__SCA.NotNull] g__S.Func<TState, Query, int> process"
        );
        StringAssert.Contains(
              run.CombinedSource
            , "[g__SCA.NotNull] g__S.Func<TState, Async, g__ETT.UnityTask<int>> process"
        );
        Assert.IsFalse(run.CombinedSource.Contains("g__S.Func<\n", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task UnityScope_UsesOnlyUnityHubAndCompiles()
    {
        var run = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Both, State = StateMode.Both, Scope = typeof(ViewScope))]
            public sealed partial class ShowView { }
            """);

        ProcessingRequestFixture.AssertNoOutputErrors(run);
        var scopeSource = GetScopeSources(run).Single();
        var methods = scopeSource.SyntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToArray();
        Assert.AreEqual(16, methods.Length);
        Assert.AreEqual(16, methods.Count(HasHubParameter));
        StringAssert.Contains(scopeSource.SourceText.ToString(), "g__ETP.Processor.UnityHub<");
        Assert.IsFalse(
            scopeSource.SourceText.ToString().Contains("g__ETP.Processor.Hub<", StringComparison.Ordinal)
        );
        Assert.IsFalse(
            scopeSource.SourceText.ToString().Contains("GlobalProcessor.Instance.Global", StringComparison.Ordinal)
        );
    }

    [TestMethod]
    public async Task InvalidScope_SkipsOnlyScopeOutputAndSharedAsyncStorage()
    {
        var run = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Async, State = StateMode.Both, Scope = null)]
            public partial class InvalidRequest { }
            """);

        AssertHintRoles(run, requestCount: 1, scopeCount: 0);
        ProcessingRequestFixture.AssertNoOutputErrors(run);
        Assert.IsFalse(run.CombinedSource.Contains("struct Async", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ValidAndInvalidScopes_KeepCommonAndValidScopeOnly()
    {
        var run = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Async, State = StateMode.Both, Scope = null)]
            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            public partial class MixedRequest { }
            """);

        AssertHintRoles(run, requestCount: 1, scopeCount: 1);
        ProcessingRequestFixture.AssertNoOutputErrors(run);
        Assert.IsFalse(run.CombinedSource.Contains("struct Async", StringComparison.Ordinal));
        StringAssert.Contains(run.CombinedSource, "global::TestProject.IOrderScope");
    }

    [TestMethod]
    public async Task AssemblySkipMarker_SuppressesOutputWhileUnmarkedControlGenerates()
    {
        var marked = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            [assembly: EncosyTower.Processing.SkipSourceGeneratorsForAssembly]

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both)]
            public partial class Request { }
            """);
        var unmarked = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both)]
            public partial class Request { }
            """);

        Assert.AreEqual(0, marked.Sources.Count);
        Assert.IsNull(marked.Result.Exception);
        Assert.AreEqual(0, marked.Result.Diagnostics.Length);
        AssertHintRoles(unmarked, requestCount: 1, scopeCount: 1);
        ProcessingRequestFixture.AssertNoOutputErrors(unmarked);
    }

    [TestMethod]
    public async Task MissingPartial_EmitsOutputsAndLeavesExactErrorToCompiler()
    {
        var run = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both)]
            public class Request { }
            """);

        AssertHintRoles(run, requestCount: 1, scopeCount: 1);
        var errors = GetOutputErrors(run);
        Assert.AreEqual(1, errors.Length);
        Assert.AreEqual("CS0260", errors[0].Id);
    }

    [TestMethod]
    public async Task InaccessibleScope_EmitsOutputsAndLeavesExactErrorsToCompiler()
    {
        var run = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            internal sealed class HiddenScope { }

            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(HiddenScope))]
            public partial class Request { }
            """);

        AssertHintRoles(run, requestCount: 1, scopeCount: 1);
        var errors = GetOutputErrors(run);
        Assert.AreEqual(8, errors.Length);
        Assert.IsTrue(errors.All(static diagnostic => diagnostic.Id == "CS0051"));
    }

    [TestMethod]
    public async Task DuplicateScopes_EmitIndependentSaltedOutputsAndCompilerCollisions()
    {
        var run = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            public partial class DuplicateRequest { }
            """);

        AssertHintRoles(run, requestCount: 1, scopeCount: 2);
        var scopeHints = GetScopeSources(run).Select(static source => source.HintName).ToArray();
        Assert.AreNotEqual(scopeHints[0], scopeHints[1]);
        Assert.IsTrue(
            run.OutputCompilation.GetDiagnostics().Any(static diagnostic =>
                diagnostic.Severity == DiagnosticSeverity.Error
                && diagnostic.Id is "CS0111" or "CS0121"
            )
        );
    }

    [TestMethod]
    public async Task PartialMarkers_UseOneCanonicalRequestAndOneOutputPerScope()
    {
        var run = await ProcessingRequestFixture.RunAsync([
            new NamedSource("Z.Part.cs", """
                using EncosyTower.Processing;

                namespace TestProject;

                [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
                public partial class PartialRequest { }
                """),
            new NamedSource("A.Part.cs", """
                using EncosyTower.Processing;

                namespace TestProject;

                [Processing(ApiMode.Both, State = StateMode.Both, Scope = typeof(AbstractScope))]
                public partial class PartialRequest { }
                """),
        ]);

        AssertHintRoles(run, requestCount: 1, scopeCount: 2);
        ProcessingRequestFixture.AssertNoOutputErrors(run);
        Assert.AreEqual(1, CountText(run.CombinedSource, "private readonly global::TestProject.PartialRequest?"));
    }

    [TestMethod]
    public async Task RepresentativeOutput_ContainsOnlyApprovedDelegationSurface()
    {
        var run = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Both, State = StateMode.Both, Scope = typeof(IOrderScope))]
            public partial class Request { }
            """);
        var source = run.CombinedSource;

        ProcessingRequestFixture.AssertNoOutputErrors(run);
        StringAssert.Contains(source, "g__ETP.ProcessorExtensions.WithState(in hub, state)");
        Assert.AreEqual(4, CountText(source, "ProcessorExtensions.WithState(in hub, state)"));
        Assert.IsFalse(source.Contains("CustomProcessor", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("ProcessHub", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("ProcessHubExtensions", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("ThrowHelper", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("new TScope", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("IEquatable<TScope>", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("hub.Scope", StringComparison.Ordinal));
    }

    private static MethodDeclarationSyntax[] GetGeneratedMethods(ProcessingRun run)
        => run.Sources.SelectMany(static source => source.SyntaxTree.GetRoot()
                .DescendantNodes().OfType<MethodDeclarationSyntax>())
            .ToArray();

    private static GeneratedSourceResult[] GetScopeSources(ProcessingRun run)
        => run.Sources.Where(static source => source.HintName.Contains(
              ".ProcessingScope."
            , StringComparison.Ordinal
        )).ToArray();

    private static bool HasHubParameter(MethodDeclarationSyntax method)
        => method.ParameterList.Parameters.Any(static parameter =>
            parameter.Type?.ToString().Contains(".Hub<", StringComparison.Ordinal) == true
            || parameter.Type?.ToString().Contains(".UnityHub<", StringComparison.Ordinal) == true
        );

    private static Diagnostic[] GetOutputErrors(ProcessingRun run)
        => run.OutputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

    private static void AssertHintRoles(ProcessingRun run, int requestCount, int scopeCount)
    {
        Assert.AreEqual(
            requestCount,
            run.Sources.Count(static source => source.HintName.Contains(
                  ".ProcessingRequest."
                , StringComparison.Ordinal
            ))
        );
        Assert.AreEqual(scopeCount, GetScopeSources(run).Length);
    }

    private static int CountText(string source, string value)
        => source.Split(value, StringSplitOptions.None).Length - 1;
}

internal sealed class ProcessingProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    public string FeaturePath => "Processing";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts { get; } = [
        new(
              typeof(ProcessingRequestGenerator)
            , "Processing/ProcessingRequestGeneratorTests.cs"
            , "GlobalVoidRequest_GeneratesFourteenSynchronousMethodsAndCompiles"
            , "public readonly partial record struct Ping"
            , "public readonly partial record struct Pong"
        ),
    ];
}
