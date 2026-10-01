using EncosyTower.PubSub.Analyzers;
using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.PubSub;

[TestClass]
public sealed class PubSubMessageAnalyzerTests
{
    [TestMethod]
    public void DescriptorContract_IsExactAndOrdered()
    {
        var analyzer = new PubSubMessageAnalyzer();
        var descriptors = analyzer.SupportedDiagnostics;

        CollectionAssert.AreEqual(
              new[] {
                  "SG_PUBSUB_0002",
                  "SG_PUBSUB_0003",
                  "SG_PUBSUB_0005",
                  "SG_PUBSUB_0012",
                  "SG_PUBSUB_0013",
              }
            , descriptors.Select(static descriptor => descriptor.Id).ToArray()
        );
        CollectionAssert.AreEqual(
              new[] {
                  DiagnosticSeverity.Error,
                  DiagnosticSeverity.Error,
                  DiagnosticSeverity.Error,
                  DiagnosticSeverity.Error,
                  DiagnosticSeverity.Error,
              }
            , descriptors.Select(static descriptor => descriptor.DefaultSeverity).ToArray()
        );

        foreach (var descriptor in descriptors)
        {
            Assert.AreEqual("PubSub", descriptor.Category);
            Assert.IsTrue(descriptor.IsEnabledByDefault);
            Assert.IsTrue(string.IsNullOrEmpty(descriptor.HelpLinkUri));
            Assert.AreEqual(0, descriptor.CustomTags.Count());
        }

        Assert.AreEqual("PubSub scope is invalid", PubSubMessageAnalyzer.InvalidScope.Title.ToString());
        Assert.AreEqual("PubSub scope is repeated", PubSubMessageAnalyzer.RepeatedScope.Title.ToString());
        Assert.AreEqual(
            "PubSub message declaration is unsupported",
            PubSubMessageAnalyzer.UnsupportedDeclaration.Title.ToString()
        );
        Assert.AreEqual("PubSub API mode is invalid", PubSubMessageAnalyzer.InvalidApiMode.Title.ToString());
        Assert.AreEqual("PubSub state mode is invalid", PubSubMessageAnalyzer.InvalidStateMode.Title.ToString());
    }

    [TestMethod]
    public Task AllowedScopes_ProduceNoDiagnostics()
        => RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Sync, State = StateMode.Both)]
            public partial class GlobalMessage { }

            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(AbstractScope))]
            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(EnumScope))]
            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(NoDefaultConstructorScope))]
            [PubSub(ApiMode.Async, State = StateMode.Both, Scope = typeof(ViewScope))]
            public partial struct MessageWithScopes { }
            """);

    [TestMethod]
    public Task ExplicitNullScope_ReportsInvalidScope()
        => RunAsync(
              """
              using EncosyTower.PubSub;

              namespace TestProject;

              [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = {|#0:null|})]
              public partial class Message { }
              """
            , new DiagnosticResult(PubSubMessageAnalyzer.InvalidScope)
                .WithLocation(0)
                .WithArguments("global::TestProject.Message", "null")
        );

    [TestMethod]
    public Task InvalidStaticAndOpenScopes_ReportEachOccurrence()
        => RunAsync(
              """
              using EncosyTower.PubSub;

              namespace TestProject;

              [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = {|#0:typeof(StaticScope)|})]
              [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = {|#1:typeof(OpenScope<>)|})]
              public partial class Message { }
              """
            , new DiagnosticResult(PubSubMessageAnalyzer.InvalidScope)
                .WithLocation(0)
                .WithArguments("global::TestProject.Message", "global::TestProject.StaticScope")
            , new DiagnosticResult(PubSubMessageAnalyzer.InvalidScope)
                .WithLocation(1)
                .WithArguments("global::TestProject.Message", "global::TestProject.OpenScope<>")
        );

    [TestMethod]
    public Task MissingPartialAndInaccessibleScope_ProduceNoDiagnostics()
        => RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            internal sealed class HiddenScope { }

            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(HiddenScope))]
            public class Message { }
            """);

    [TestMethod]
    public Task DuplicateGlobalScope_ReportsEveryOccurrence()
        => RunAsync(
              """
              using EncosyTower.Common;
              using EncosyTower.PubSub;

              namespace TestProject;

              [{|#0:PubSub|}(ApiMode.Sync)]
              [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = {|#1:typeof(GlobalScope)|})]
              public partial class Message { }
              """
            , new DiagnosticResult(PubSubMessageAnalyzer.RepeatedScope)
                .WithLocation(0)
                .WithArguments("global::EncosyTower.Common.GlobalScope", "global::TestProject.Message")
            , new DiagnosticResult(PubSubMessageAnalyzer.RepeatedScope)
                .WithLocation(1)
                .WithArguments("global::EncosyTower.Common.GlobalScope", "global::TestProject.Message")
        );

    [TestMethod]
    public Task RefStruct_ReportsUnsupportedDeclaration()
        => RunAsync(
              """
              using EncosyTower.PubSub;

              namespace TestProject;

              [PubSub(ApiMode.Sync, State = StateMode.Both)]
              public ref partial struct {|#0:Message|} { }
              """
            , new DiagnosticResult(PubSubMessageAnalyzer.UnsupportedDeclaration)
                .WithLocation(0)
                .WithArguments("global::TestProject.Message")
        );

    [TestMethod]
    public Task InvalidMode_ReportsAtArgumentWithoutScopeCascade()
        => RunAsync(
              """
              using EncosyTower.PubSub;

              namespace TestProject;

              [PubSub({|#0:(ApiMode)99|}, State = StateMode.Both, Scope = null)]
              [PubSub(ApiMode.Sync, State = StateMode.Both)]
              public partial class Message { }
              """
            , new DiagnosticResult(PubSubMessageAnalyzer.InvalidApiMode)
                .WithLocation(0)
                .WithArguments("global::TestProject.Message", 99)
        );

    [TestMethod]
    public Task InvalidState_ReportsAndNormalizesWithoutSuppressingScopeCheck()
        => RunAsync(
              """
              using EncosyTower.CodeGen;
              using EncosyTower.PubSub;

              namespace TestProject;

              [PubSub(ApiMode.Sync, State = {|#0:(StateMode)99|}, Scope = {|#1:typeof(StaticScope)|})]
              public partial class Message { }
              """
            , new DiagnosticResult(PubSubMessageAnalyzer.InvalidStateMode)
                .WithLocation(0)
                .WithArguments("global::TestProject.Message", 99)
            , new DiagnosticResult(PubSubMessageAnalyzer.InvalidScope)
                .WithLocation(1)
                .WithArguments("global::TestProject.Message", "global::TestProject.StaticScope")
        );

    [TestMethod]
    public Task DifferentStateModes_OnSameScope_StillReportsRepeatedScope()
        => RunAsync(
              """
              using EncosyTower.CodeGen;
              using EncosyTower.Common;
              using EncosyTower.PubSub;

              namespace TestProject;

              [{|#0:PubSub|}(ApiMode.Sync, State = StateMode.Stateless)]
              [PubSub(ApiMode.Sync, State = StateMode.Stateful, Scope = {|#1:typeof(GlobalScope)|})]
              public partial class Message { }
              """
            , new DiagnosticResult(PubSubMessageAnalyzer.RepeatedScope)
                .WithLocation(0)
                .WithArguments("global::EncosyTower.Common.GlobalScope", "global::TestProject.Message")
            , new DiagnosticResult(PubSubMessageAnalyzer.RepeatedScope)
                .WithLocation(1)
                .WithArguments("global::EncosyTower.Common.GlobalScope", "global::TestProject.Message")
        );

    [TestMethod]
    public Task AssemblySkipMarker_SuppressesAllDiagnostics()
        => RunAsync("""
            using EncosyTower.PubSub;

            [assembly: EncosyTower.PubSub.SkipSourceGeneratorsForAssembly]

            namespace TestProject;

            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = null)]
            public static class Message { }
            """);

    private static Task RunAsync(string source, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<PubSubMessageAnalyzer>(
              [new NamedSource("Message.cs", source)]
            , expected
            , featureLocalStubSources: [
                new NamedSource("PubSubAttribute.cs", PubSubRuntimeFixture.MarkerSource),
                new NamedSource("PubSubRuntime.cs", PubSubRuntimeFixture.RuntimeSource),
                new NamedSource("PubSubScopes.cs", PubSubRuntimeFixture.ScopeSource),
            ]
        );

}

internal sealed class PubSubDiagnosticContractProvider : IDiagnosticContractProvider
{
    private const string OWNER = "EncosyTower.PubSub.Analyzers.PubSubMessageAnalyzer";

    public string FeaturePath => "PubSub";

    public IReadOnlyList<Type> ComponentTypes { get; } = [typeof(PubSubMessageAnalyzer)];

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = [
        new(
              OWNER
            , "SG_PUBSUB_0002"
            , "PubSub scope is invalid"
            , "Message '{0}' uses scope '{1}', which cannot be used as a PubSub scope."
            , "PubSub"
            , DiagnosticSeverity.Error
            , true
            , "PubSub scopes must be closed, non-static, non-ref-like types that can be used by the "
                + "selected PubSub publisher."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PUBSUB_0003"
            , "PubSub scope is repeated"
            , "Scope '{0}' is declared more than once on message '{1}'. Keep exactly one PubSub attribute "
                + "for this scope."
            , "PubSub"
            , DiagnosticSeverity.Error
            , true
            , "Each semantic PubSub scope may be declared only once per message."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PUBSUB_0005"
            , "PubSub message declaration is unsupported"
            , "Type '{0}' cannot generate PubSub members."
            , "PubSub"
            , DiagnosticSeverity.Error
            , true
            , "PubSub generation supports non-static, non-ref-like, non-file-local classes, structs, "
                + "record classes, and record structs."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PUBSUB_0012"
            , "PubSub API mode is invalid"
            , "Message '{0}' uses API mode value '{1}', which is not a defined EncosyTower.CodeGen.ApiMode."
            , "PubSub"
            , DiagnosticSeverity.Error
            , true
            , "PubSub API mode must be Sync, Async, or Both."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PUBSUB_0013"
            , "PubSub state mode is invalid"
            , "Message '{0}' uses state mode value '{1}', which is not a defined EncosyTower.CodeGen.StateMode."
            , "PubSub"
            , DiagnosticSeverity.Error
            , true
            , "PubSub state mode must be Stateless, Stateful, or Both."
            , ""
            , Array.Empty<string>()
        ),
    ];

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions { get; }
        = Array.Empty<SuppressionDescriptorContract>();
}
