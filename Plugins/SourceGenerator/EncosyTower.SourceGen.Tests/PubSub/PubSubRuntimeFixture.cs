using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using EncosyTower.PubSub.Analyzers;
using EncosyTower.PubSub.Generators;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Tests.PubSub;

internal static class PubSubRuntimeFixture
{
    private const string ASSEMBLY_NAME = "EncosyTower.SourceGen.Tests.PubSub.Input";

    private static readonly CSharpParseOptions s_parseOptions = CSharpParseOptions.Default
        .WithLanguageVersion(LanguageVersion.CSharp10);

    internal const string MarkerSource = """
        global using EncosyTower.CodeGen;

        namespace EncosyTower.CodeGen
        {
            public enum ApiMode
            {
                Sync = 0,
                Async = 1,
                Both = 2,
            }

            public enum StateMode
            {
                Stateless = 0,
                Stateful = 1,
                Both = 2,
            }
        }

        namespace EncosyTower.Common
        {
            public readonly struct GlobalScope { }
        }

        namespace EncosyTower.PubSub
        {
            [System.AttributeUsage(
                  System.AttributeTargets.Class | System.AttributeTargets.Struct
                , AllowMultiple = true
                , Inherited = false
            )]
            public sealed class PubSubAttribute : System.Attribute
            {
                public PubSubAttribute(ApiMode mode)
                {
                    Mode = mode;
                }

                public ApiMode Mode { get; }

                public StateMode State { get; set; } = StateMode.Both;

                public System.Type Scope { get; set; }
            }

            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public sealed class SkipSourceGeneratorsForAssemblyAttribute : System.Attribute { }
        }
        """;

    internal const string RuntimeSource = """
        namespace EncosyTower.Tasks
        {
            public readonly struct UnityTask { }
        }

        namespace EncosyTower.UnityExtensions
        {
            public readonly struct UnityEntityId<T> { }
        }

        namespace UnityEngine
        {
            public abstract class Object { }
        }

        namespace EncosyTower.Logging
        {
            public interface ILogger { }
        }

        namespace EncosyTower.PubSub
        {
            using System;
            using System.Threading;
            using EncosyTower.Tasks;
            using EncosyTower.Logging;

            public interface IMessage { }

            public interface ISubscription { }

            public readonly struct PublishingContext { }

            public readonly struct CachedPublisher<TScope, TMessage> { }

            public sealed class MessagePublisher
            {
                public Publisher<EncosyTower.Common.GlobalScope> Global()
                    => default;

                public readonly struct Publisher<TScope>
                {
                    public CachedPublisher<TScope, TMessage> Cache<TMessage>(Func<TMessage> factory, ILogger logger)
                        => default;

                    public void Publish<TMessage>(TMessage message, PublishingContext context) { }

                    public void Publish<TMessage>(PublishingContext context) { }

                    public UnityTask PublishAsync<TMessage>(TMessage message, PublishingContext context)
                        => default;

                    public UnityTask PublishAsync<TMessage>(PublishingContext context)
                        => default;
                }

                public readonly struct UnityPublisher<TScope>
                {
                    public CachedPublisher<EncosyTower.UnityExtensions.UnityEntityId<TScope>, TMessage>
                        Cache<TMessage>(Func<TMessage> factory, ILogger logger)
                        => default;

                    public void Publish<TMessage>(TMessage message, PublishingContext context) { }

                    public void Publish<TMessage>(PublishingContext context) { }

                    public UnityTask PublishAsync<TMessage>(TMessage message, PublishingContext context)
                        => default;

                    public UnityTask PublishAsync<TMessage>(PublishingContext context)
                        => default;
                }
            }

            public sealed class MessageSubscriber
            {
                public Subscriber<EncosyTower.Common.GlobalScope> Global()
                    => default;

                public readonly struct Subscriber<TScope>
                {
                    public ISubscription Subscribe<TMessage>(Delegate handler, int order, ILogger logger)
                        => default;

                    public void Subscribe<TMessage>(
                          Delegate handler
                        , CancellationToken token
                        , int order
                        , ILogger logger
                    ) { }
                }

                public readonly struct Subscriber<TScope, TState>
                    where TState : class
                {
                    public ISubscription Subscribe<TMessage>(Delegate handler, int order, ILogger logger)
                        => default;

                    public void Subscribe<TMessage>(
                          Delegate handler
                        , CancellationToken token
                        , int order
                        , ILogger logger
                    ) { }
                }

                public readonly struct UnitySubscriber<TScope>
                {
                    public ISubscription Subscribe<TMessage>(Delegate handler, int order, ILogger logger)
                        => default;

                    public void Subscribe<TMessage>(
                          Delegate handler
                        , CancellationToken token
                        , int order
                        , ILogger logger
                    ) { }
                }

                public readonly struct UnitySubscriber<TScope, TState>
                    where TState : class
                {
                    public ISubscription Subscribe<TMessage>(Delegate handler, int order, ILogger logger)
                        => default;

                    public void Subscribe<TMessage>(
                          Delegate handler
                        , CancellationToken token
                        , int order
                        , ILogger logger
                    ) { }
                }
            }

            public static class MessageSubscriberExtensions
            {
                public static MessageSubscriber.Subscriber<TScope, TState> WithState<TScope, TState>(
                      in MessageSubscriber.Subscriber<TScope> subscriber
                    , TState state
                )
                    where TState : class
                    => default;

                public static MessageSubscriber.UnitySubscriber<TScope, TState> WithState<TScope, TState>(
                      in MessageSubscriber.UnitySubscriber<TScope> subscriber
                    , TState state
                )
                    where TState : class
                    => default;
            }

            public static class GlobalMessenger
            {
                public static MessagePublisher Publisher { get; } = new MessagePublisher();

                public static MessageSubscriber Subscriber { get; } = new MessageSubscriber();
            }
        }
        """;

    internal const string ScopeSource = """
        namespace TestProject
        {
            public interface IOrderScope { }

            public abstract class AbstractScope { }

            public enum EnumScope { Value }

            public sealed class NoDefaultConstructorScope
            {
                public NoDefaultConstructorScope(int value) { }
            }

            public sealed class ViewScope : UnityEngine.Object { }

            public static class StaticScope { }

            public sealed class OpenScope<T> { }
        }
        """;

    internal static Task<PubSubRun> RunAsync(
          IReadOnlyList<NamedSource> sources
        , [AllowNull] PubSubRun previous
        , CancellationToken token = default
    )
        => RunAsync(sources, previous, Array.Empty<IIncrementalGenerator>(), token);

    internal static async Task<PubSubRun> RunAsync(
          IReadOnlyList<NamedSource> sources
        , [AllowNull] PubSubRun previous
        , IReadOnlyList<IIncrementalGenerator> additionalGenerators
        , CancellationToken token = default
    )
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var allSources = new List<NamedSource>(sources.Count + 3) {
            new("PubSubAttribute.cs", MarkerSource),
            new("PubSubRuntime.cs", RuntimeSource),
            new("PubSubScopes.cs", ScopeSource),
        };
        allSources.AddRange(sources);
        var compilation = CreateCompilation(allSources, references, previous?.InputCompilation, token);
        var generators = new List<ISourceGenerator>(1 + additionalGenerators.Count) {
            new PubSubMessageGenerator().AsSourceGenerator(),
        };

        generators.AddRange(additionalGenerators.Select(static generator => generator.AsSourceGenerator()));

        var driver = previous?.Driver ?? CSharpGeneratorDriver.Create(
              generators
            , parseOptions: s_parseOptions
            , driverOptions: new GeneratorDriverOptions(
                  disabledOutputs: default
                , trackIncrementalGeneratorSteps: true
            )
        );
        driver = driver.RunGeneratorsAndUpdateCompilation(
              compilation
            , out var outputCompilation
            , out var diagnostics
            , token
        );
        var result = driver.GetRunResult();

        Assert.AreEqual(
              0
            , diagnostics.Length
            , string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString()))
        );
        Assert.AreEqual(1 + additionalGenerators.Count, result.Results.Length);

        foreach (var generatorResult in result.Results)
        {
            Assert.IsNull(generatorResult.Exception);
            Assert.AreEqual(0, generatorResult.Diagnostics.Length);
        }

        return new PubSubRun(
              driver
            , compilation
            , outputCompilation
            , result.Results[0]
            , result.Results.RemoveAt(0)
        );
    }

    internal static Task<PubSubRun> RunAsync(string source, CancellationToken token = default)
        => RunAsync([new NamedSource("Message.cs", source)], previous: null, token);

    internal static Task<PubSubRun> RunAsync(
        IReadOnlyList<NamedSource> sources,
        CancellationToken token = default
    )
        => RunAsync(sources, previous: null, token);

    internal static async Task<PubSubRun> RunWithRuntimeAsync(
        IReadOnlyList<NamedSource> sources,
        string runtimeSource,
        CancellationToken token = default
    )
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var allSources = new List<NamedSource>(sources.Count + 3) {
            new("PubSubAttribute.cs", MarkerSource),
            new("PubSubRuntime.cs", runtimeSource),
            new("PubSubScopes.cs", ScopeSource),
        };
        allSources.AddRange(sources);
        var compilation = CreateCompilation(allSources, references, previousCompilation: null, token);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
              new[] { new PubSubMessageGenerator().AsSourceGenerator() }
            , parseOptions: s_parseOptions
            , driverOptions: new GeneratorDriverOptions(
                  disabledOutputs: default
                , trackIncrementalGeneratorSteps: true
            )
        );
        driver = driver.RunGeneratorsAndUpdateCompilation(
              compilation
            , out var outputCompilation
            , out var diagnostics
            , token
        );
        var result = driver.GetRunResult();

        Assert.AreEqual(0, diagnostics.Length);
        Assert.AreEqual(1, result.Results.Length);
        Assert.IsNull(result.Results[0].Exception);
        Assert.AreEqual(0, result.Results[0].Diagnostics.Length);
        return new PubSubRun(
              driver
            , compilation
            , outputCompilation
            , result.Results[0]
            , ImmutableArray<GeneratorRunResult>.Empty
        );
    }

    internal static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(
        IReadOnlyList<NamedSource> sources,
        CancellationToken token = default
    )
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var allSources = new List<NamedSource>(sources.Count + 3) {
            new("PubSubAttribute.cs", MarkerSource),
            new("PubSubRuntime.cs", RuntimeSource),
            new("PubSubScopes.cs", ScopeSource),
        };
        allSources.AddRange(sources);
        var compilation = CreateCompilation(allSources, references, previousCompilation: null, token);
        return await compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new PubSubMessageAnalyzer())
        ).GetAnalyzerDiagnosticsAsync(token);
    }

    internal static void AssertNoOutputErrors(PubSubRun run)
    {
        var errors = run.OutputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(
              0
            , errors.Length
            , string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString()))
        );
    }

    private static CSharpCompilation CreateCompilation(
        IReadOnlyList<NamedSource> sources,
        ImmutableArray<MetadataReference> references,
        [AllowNull] Compilation previousCompilation,
        CancellationToken token
    )
    {
        var previousTrees = previousCompilation?.SyntaxTrees.ToDictionary(
              static tree => tree.FilePath
            , StringComparer.Ordinal
        ) ?? new Dictionary<string, SyntaxTree>(StringComparer.Ordinal);
        var trees = new List<SyntaxTree>(sources.Count);

        foreach (var source in sources)
        {
            var text = SourceText.From(source.Source, Encoding.UTF8);

            if (previousTrees.TryGetValue(source.Path, out var previousTree)
                && previousTree.GetText(token).ContentEquals(text)
            )
            {
                trees.Add(previousTree);
            }
            else
            {
                trees.Add(CSharpSyntaxTree.ParseText(text, s_parseOptions, source.Path, token));
            }
        }

        return CSharpCompilation.Create(
              ASSEMBLY_NAME
            , trees
            , references
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
        );
    }
}

internal sealed record PubSubRun(
    GeneratorDriver Driver,
    Compilation InputCompilation,
    Compilation OutputCompilation,
    GeneratorRunResult Result,
    ImmutableArray<GeneratorRunResult> AdditionalResults
)
{
    internal IReadOnlyList<GeneratedSourceResult> Sources => Result.GeneratedSources;

    internal string CombinedSource
        => string.Join(
            Environment.NewLine,
            Sources.OrderBy(static source => source.HintName, StringComparer.Ordinal)
                .Select(static source => source.SourceText.ToString())
        );
}
