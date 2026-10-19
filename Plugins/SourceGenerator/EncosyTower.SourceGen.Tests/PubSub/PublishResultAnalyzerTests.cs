using EncosyTower.PubSub.Analyzers;
using EncosyTower.PubSub.Generators;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.PubSub;

[TestClass]
public sealed class PublishResultAnalyzerTests
{
    private const string PUBLISH_ASYNC = "MessagePublisher.Publisher<TScope>.PublishAsync";
    private const string GENERATED_PUBLISH = "M.Async.Publish";
    private const string SKIP_ATTRIBUTE = "[assembly: EncosyTower.PubSub.SkipSourceGeneratorsForAssembly]";

    [TestMethod]
    public void DescriptorContract_IsExact()
    {
        var descriptors = new PublishResultAnalyzer().SupportedDiagnostics;

        Assert.AreEqual(1, descriptors.Length);

        var descriptor = descriptors[0];

        Assert.AreSame(PublishResultAnalyzer.PublishResultDiscarded, descriptor);
        Assert.AreEqual("SG_PUBSUB_0006", descriptor.Id);
        Assert.AreEqual("PubSub publish result is discarded", descriptor.Title.ToString());
        Assert.AreEqual("PubSub", descriptor.Category);
        Assert.AreEqual(DiagnosticSeverity.Warning, descriptor.DefaultSeverity);
        Assert.IsTrue(descriptor.IsEnabledByDefault);
        Assert.IsTrue(string.IsNullOrEmpty(descriptor.HelpLinkUri));
        Assert.AreEqual(0, descriptor.CustomTags.Count());
    }

    [TestMethod]
    public Task DiscardedPublishAsync_ReportsOnTheCall()
        => RunAsync(
              CreateSource("await publisher.PublishAsync(m);")
            , new DiagnosticResult(PublishResultAnalyzer.PublishResultDiscarded)
                .OnTestLine(line: 15, startColumn: 15, endColumn: 40)
                .WithArguments(PUBLISH_ASYNC)
        );

    [TestMethod]
    public Task DiscardedGeneratedAsyncPublish_ReportsOnTheCall()
        => RunAsync(
              CreateSource("await M.Async.Publish(in publisher, m);")
            , new DiagnosticResult(PublishResultAnalyzer.PublishResultDiscarded)
                .OnTestLine(line: 15, startColumn: 15, endColumn: 47)
                .WithArguments(GENERATED_PUBLISH)
        );

    [TestMethod]
    public Task AssignedResult_ProducesNoDiagnostics()
        => RunAsync(CreateSource("var result = await publisher.PublishAsync(m);"));

    [TestMethod]
    public Task ExplicitDiscard_ProducesNoDiagnostics()
        => RunAsync(CreateSource("_ = await publisher.PublishAsync(m);"));

    [TestMethod]
    public Task NotAwaitedCall_ProducesNoDiagnostics()
        => RunAsync(CreateSource("publisher.PublishAsync(m);", returnType: "void"));

    [TestMethod]
    public Task UnrelatedAwait_ProducesNoDiagnostics()
        => RunAsync(CreateSource("await Task.Delay(1);"));

    [TestMethod]
    public Task AssemblySkipMarker_SuppressesTheDiagnostic()
        => RunAsync(CreateSource("await publisher.PublishAsync(m);", assemblyAttribute: SKIP_ATTRIBUTE));

    private static string CreateSource(
          string statement
        , string returnType = "async Task"
        , string assemblyAttribute = ""
    )
        => $$"""
            using System.Threading.Tasks;
            using EncosyTower.Common;
            using EncosyTower.PubSub;
            {{assemblyAttribute}}

            namespace TestProject;

            [PubSub(ApiMode.Both)]
            public partial struct M { }

            public static class Usage
            {
                public static {{returnType}} Run(MessagePublisher.Publisher<GlobalScope> publisher, M m)
                {
                    {{statement}}
                }
            }
            """;

    private static Task RunAsync(string source, params DiagnosticResult[] expected)
        => GeneratorAnalyzerTestHelper.VerifyAsync(
              [
                  new NamedSource("PubSubRuntime.cs", PubSubRuntimeFixture.PublishResultSource),
                  new NamedSource(GeneratorAnalyzerTestHelper.INPUT_PATH, source),
              ]
            , [new PubSubMessageGenerator()]
            , [new PublishResultAnalyzer()]
            , expected
        );
}
