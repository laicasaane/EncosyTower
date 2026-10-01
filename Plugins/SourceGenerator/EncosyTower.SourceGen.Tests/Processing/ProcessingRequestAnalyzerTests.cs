using EncosyTower.Processing.Analyzers;
using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Processing;

[TestClass]
public sealed class ProcessingRequestAnalyzerTests
{
    [TestMethod]
    public void DescriptorContract_IsExactAndOrdered()
    {
        var analyzer = new ProcessingRequestAnalyzer();
        var descriptors = analyzer.SupportedDiagnostics;

        CollectionAssert.AreEqual(
              new[] {
                  "SG_PROCESSING_0001",
                  "SG_PROCESSING_0002",
                  "SG_PROCESSING_0003",
                  "SG_PROCESSING_0004",
                  "SG_PROCESSING_0005",
                  "SG_PROCESSING_0006",
                  "SG_PROCESSING_0007",
              }
            , descriptors.Select(static descriptor => descriptor.Id).ToArray()
        );
        CollectionAssert.AreEqual(
              new[] {
                  DiagnosticSeverity.Error,
                  DiagnosticSeverity.Error,
                  DiagnosticSeverity.Error,
                  DiagnosticSeverity.Error,
                  DiagnosticSeverity.Warning,
                  DiagnosticSeverity.Error,
                  DiagnosticSeverity.Error,
              }
            , descriptors.Select(static descriptor => descriptor.DefaultSeverity).ToArray()
        );

        foreach (var descriptor in descriptors)
        {
            Assert.AreEqual("Processing", descriptor.Category);
            Assert.IsTrue(descriptor.IsEnabledByDefault);
            Assert.IsTrue(string.IsNullOrEmpty(descriptor.HelpLinkUri));
            Assert.AreEqual(0, descriptor.CustomTags.Count());
        }

        Assert.AreEqual("Processing scope is invalid", ProcessingRequestAnalyzer.InvalidScope.Title.ToString());
        Assert.AreEqual(
            "Processing scope is repeated",
            ProcessingRequestAnalyzer.RepeatedScope.Title.ToString()
        );
        Assert.AreEqual(
            "Processing request result types conflict",
            ProcessingRequestAnalyzer.ResultTypeConflict.Title.ToString()
        );
        Assert.AreEqual(
            "Processing request declaration is unsupported",
            ProcessingRequestAnalyzer.UnsupportedDeclaration.Title.ToString()
        );
        Assert.AreEqual(
            "Async request interface is redundant",
            ProcessingRequestAnalyzer.RedundantAsyncRequestInterface.Title.ToString()
        );
        Assert.AreEqual(
            "Processing API mode is invalid",
            ProcessingRequestAnalyzer.InvalidApiMode.Title.ToString()
        );
        Assert.AreEqual(
            "Processing state mode is invalid",
            ProcessingRequestAnalyzer.InvalidStateMode.Title.ToString()
        );
    }

    [TestMethod]
    public Task AllowedScopes_ProduceNoDiagnostics()
        => RunAsync("""
            using EncosyTower.Common;
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both)]
            public partial class GlobalRequest { }

            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(GlobalScope))]
            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(AbstractScope))]
            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(EnumScope))]
            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(NoDefaultConstructorScope))]
            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(Processor))]
            [Processing(ApiMode.Async, State = StateMode.Both, Scope = typeof(ViewScope))]
            public partial class RequestWithScope { }
            """);

    [TestMethod]
    public Task MissingPartialAndInaccessibleScope_ProduceNoDiagnostics()
        => RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            internal sealed class HiddenScope { }

            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(HiddenScope))]
            public class Request { }
            """);

    [TestMethod]
    public Task ExplicitNullScope_ReportsInvalidScope()
        => RunAsync(
              """
              using EncosyTower.Processing;

              namespace TestProject;

              [Processing(ApiMode.Sync, State = StateMode.Both, Scope = {|#0:null|})]
              public partial class Request { }
              """
            , new DiagnosticResult(ProcessingRequestAnalyzer.InvalidScope)
                .WithLocation(0)
                .WithArguments("global::TestProject.Request", "null")
        );

    [TestMethod]
    public Task InvalidStaticAndOpenScopes_ReportEachOccurrence()
        => RunAsync(
              """
              using EncosyTower.Processing;

              namespace TestProject;

              [Processing(ApiMode.Sync, State = StateMode.Both, Scope = {|#0:typeof(StaticScope)|})]
              [Processing(ApiMode.Sync, State = StateMode.Both, Scope = {|#1:typeof(OpenScope<>)|})]
              public partial class Request { }
              """
            , new DiagnosticResult(ProcessingRequestAnalyzer.InvalidScope)
                .WithLocation(0)
                .WithArguments("global::TestProject.Request", "global::TestProject.StaticScope")
            , new DiagnosticResult(ProcessingRequestAnalyzer.InvalidScope)
                .WithLocation(1)
                .WithArguments("global::TestProject.Request", "global::TestProject.OpenScope<>")
        );

    [TestMethod]
    public Task DuplicateGlobalScope_ReportsEveryOccurrence()
        => RunAsync(
              """
              using EncosyTower.Common;
              using EncosyTower.Processing;

              namespace TestProject;

              [{|#0:Processing|}(ApiMode.Sync)]
              [Processing(ApiMode.Sync, State = StateMode.Both, Scope = {|#1:typeof(GlobalScope)|})]
              public partial class Request { }
              """
            , new DiagnosticResult(ProcessingRequestAnalyzer.RepeatedScope)
                .WithLocation(0)
                .WithArguments(
                    "global::EncosyTower.Common.GlobalScope",
                    "global::TestProject.Request"
                )
            , new DiagnosticResult(ProcessingRequestAnalyzer.RepeatedScope)
                .WithLocation(1)
                .WithArguments(
                    "global::EncosyTower.Common.GlobalScope",
                    "global::TestProject.Request"
                )
        );

    [TestMethod]
    public Task ResultConflict_ReportsAtRequestIdentifier()
        => RunAsync(
              """
              using EncosyTower.Processing;

              namespace TestProject;

              [Processing(ApiMode.Sync, State = StateMode.Both)]
              public partial class {|#0:Request|} : IRequest<int>, IRequest<string> { }
              """
            , new DiagnosticResult(ProcessingRequestAnalyzer.ResultTypeConflict)
                .WithLocation(0)
                .WithArguments(
                    "global::TestProject.Request",
                    "int, string"
                )
        );

    [TestMethod]
    public Task RefStruct_ReportsUnsupportedDeclaration()
        => RunAsync(
              """
              using EncosyTower.Processing;

              namespace TestProject;

              [Processing(ApiMode.Sync, State = StateMode.Both)]
              public ref partial struct {|#0:Request|} { }
              """
            , new DiagnosticResult(ProcessingRequestAnalyzer.UnsupportedDeclaration)
                .WithLocation(0)
                .WithArguments("global::TestProject.Request")
        );

    [TestMethod]
    public Task ExistingAsyncInterface_ReportsWarningOnly()
        => RunAsync(
              """
              using EncosyTower.Processing;

              namespace TestProject;

              [Processing(ApiMode.Async, State = StateMode.Both)]
              public partial class {|#0:Request|} : IAsyncRequest { }
              """
            , new DiagnosticResult(ProcessingRequestAnalyzer.RedundantAsyncRequestInterface)
                .WithLocation(0)
                .WithArguments(
                    "global::TestProject.Request",
                    "global::EncosyTower.Processing.IAsyncRequest"
                )
        );

    [TestMethod]
    public Task SyncMode_DoesNotReportRedundantAsyncInterface()
        => RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both)]
            public partial class Request : IAsyncRequest { }
            """);

    [TestMethod]
    public Task InvalidMode_ReportsAtArgumentWithoutScopeCascade()
        => RunAsync(
              """
              using EncosyTower.Processing;

              namespace TestProject;

              [Processing({|#0:(ApiMode)99|}, State = StateMode.Both, Scope = null)]
              [Processing(ApiMode.Sync, State = StateMode.Both)]
              public partial class Request { }
              """
            , new DiagnosticResult(ProcessingRequestAnalyzer.InvalidApiMode)
                .WithLocation(0)
                .WithArguments("global::TestProject.Request", 99)
        );

    [TestMethod]
    public Task InvalidState_ReportsAndNormalizesWithoutSuppressingScopeCheck()
        => RunAsync(
              """
              using EncosyTower.CodeGen;
              using EncosyTower.Processing;

              namespace TestProject;

              [Processing(ApiMode.Sync, State = {|#0:(StateMode)99|}, Scope = {|#1:typeof(StaticScope)|})]
              public partial class Request { }
              """
            , new DiagnosticResult(ProcessingRequestAnalyzer.InvalidStateMode)
                .WithLocation(0)
                .WithArguments("global::TestProject.Request", 99)
            , new DiagnosticResult(ProcessingRequestAnalyzer.InvalidScope)
                .WithLocation(1)
                .WithArguments("global::TestProject.Request", "global::TestProject.StaticScope")
        );

    [TestMethod]
    public Task DifferentStateModes_OnSameScope_StillReportsRepeatedScope()
        => RunAsync(
              """
              using EncosyTower.CodeGen;
              using EncosyTower.Common;
              using EncosyTower.Processing;

              namespace TestProject;

              [{|#0:Processing|}(ApiMode.Sync, State = StateMode.Stateless)]
              [Processing(ApiMode.Sync, State = StateMode.Stateful, Scope = {|#1:typeof(GlobalScope)|})]
              public partial class Request { }
              """
            , new DiagnosticResult(ProcessingRequestAnalyzer.RepeatedScope)
                .WithLocation(0)
                .WithArguments(
                    "global::EncosyTower.Common.GlobalScope",
                    "global::TestProject.Request"
                )
            , new DiagnosticResult(ProcessingRequestAnalyzer.RepeatedScope)
                .WithLocation(1)
                .WithArguments(
                    "global::EncosyTower.Common.GlobalScope",
                    "global::TestProject.Request"
                )
        );

    [TestMethod]
    public Task AssemblySkipMarker_SuppressesInvalidScopeDiagnostic()
        => RunAsync("""
            using EncosyTower.Processing;

            [assembly: EncosyTower.Processing.SkipSourceGeneratorsForAssembly]

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = null)]
            public partial class Request { }
            """);

    private static Task RunAsync(string source, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<ProcessingRequestAnalyzer>(
              [new NamedSource("Request.cs", source)]
            , expected
            , featureLocalStubSources: [
                new NamedSource("ProcessingAttribute.cs", ProcessingRuntimeFixture.MarkerSource),
                new NamedSource("ProcessingRuntime.cs", ProcessingRuntimeFixture.RuntimeSource),
                new NamedSource("ProcessingScopes.cs", ProcessingRuntimeFixture.ScopeSource),
            ]
        );

}

internal sealed class ProcessingDiagnosticContractProvider : IDiagnosticContractProvider
{
    private const string OWNER =
        "EncosyTower.Processing.Analyzers.ProcessingRequestAnalyzer";

    public string FeaturePath => "Processing";

    public IReadOnlyList<Type> ComponentTypes { get; } = [typeof(ProcessingRequestAnalyzer)];

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = [
        new(
              OWNER
            , "SG_PROCESSING_0001"
            , "Processing scope is invalid"
            , "Request '{0}' uses scope '{1}', which cannot be used as a Processing scope."
            , "Processing"
            , DiagnosticSeverity.Error
            , true
            , "Processing scopes must be closed, non-static, non-ref-like types that can be used by the "
                + "selected Processing hub."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PROCESSING_0002"
            , "Processing scope is repeated"
            , "Scope '{0}' is declared more than once on request '{1}'. Keep exactly one Processing "
                + "attribute for this scope."
            , "Processing"
            , DiagnosticSeverity.Error
            , true
            , "Each semantic Processing scope may be declared only once per request."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PROCESSING_0003"
            , "Processing request result types conflict"
            , "Request '{0}' declares conflicting Processing result types: {1}."
            , "Processing"
            , DiagnosticSeverity.Error
            , true
            , "A Processing request may expose only one normalized result type through "
                + "global::EncosyTower.Processing.IRequest<TResult>."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PROCESSING_0004"
            , "Processing request declaration is unsupported"
            , "Type '{0}' cannot generate Processing members."
            , "Processing"
            , DiagnosticSeverity.Error
            , true
            , "Processing generation supports non-static, non-ref-like, non-file-local classes, structs, "
                + "record classes, and record structs."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PROCESSING_0005"
            , "Async request interface is redundant"
            , "Request '{0}' implements redundant async request interface '{1}'. Remove the interface and use "
                + "ApiMode.Async or ApiMode.Both."
            , "Processing"
            , DiagnosticSeverity.Warning
            , true
            , "ProcessingAttribute with ApiMode.Async or ApiMode.Both creates the async request interface and "
                + "derives its result from global::EncosyTower.Processing.IRequest<TResult>."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PROCESSING_0006"
            , "Processing API mode is invalid"
            , "Request '{0}' uses API mode value '{1}', which is not a defined EncosyTower.CodeGen.ApiMode."
            , "Processing"
            , DiagnosticSeverity.Error
            , true
            , "Processing API mode must be Sync, Async, or Both."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PROCESSING_0007"
            , "Processing state mode is invalid"
            , "Request '{0}' uses state mode value '{1}', which is not a defined EncosyTower.CodeGen.StateMode."
            , "Processing"
            , DiagnosticSeverity.Error
            , true
            , "Processing state mode must be Stateless, Stateful, or Both."
            , ""
            , Array.Empty<string>()
        ),
    ];

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions { get; }
        = Array.Empty<SuppressionDescriptorContract>();
}
