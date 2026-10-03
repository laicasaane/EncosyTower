using EncosyTower.Core.Generators.TypeWraps;
using EncosyTower.PubSub.Generators;
using EncosyTower.SourceGen.Tests.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Tests.PubSub;

[TestClass]
public sealed class PubSubMessageGeneratorTests
{
    [TestMethod]
    public async Task ApiMode_GatesGeneratedInterfacesAndMethods()
    {
        var sync = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Sync, State = StateMode.Both)]
            public partial class SyncMessage { }
            """);
        var asyncOnly = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Async, State = StateMode.Both)]
            public partial class AsyncMessage { }
            """);
        var both = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, State = StateMode.Both)]
            public partial class BothMessage { }
            """);
        var invalid = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub((ApiMode)99, State = StateMode.Both)]
            public partial class InvalidMessage { }
            """);

        PubSubRuntimeFixture.AssertNoOutputErrors(sync);
        PubSubRuntimeFixture.AssertNoOutputErrors(asyncOnly);
        PubSubRuntimeFixture.AssertNoOutputErrors(both);
        PubSubRuntimeFixture.AssertNoOutputErrors(invalid);
        Assert.AreEqual(46, GetScopeSources(sync).SelectMany(GetMethods).Count());
        Assert.AreEqual(44, GetScopeSources(asyncOnly).SelectMany(GetMethods).Count());
        Assert.AreEqual(90, GetScopeSources(both).SelectMany(GetMethods).Count());
        Assert.AreEqual(0, GetScopeSources(invalid).SelectMany(GetMethods).Count());
        Assert.IsTrue(GetScopeSources(asyncOnly).SelectMany(GetMethods).All(static method => method.Ancestors()
            .OfType<StructDeclarationSyntax>()
            .Any(static declaration => declaration.Identifier.ValueText == "Async")));
        Assert.IsFalse(
            GetMessageSource(asyncOnly).SourceText.ToString()
                .Contains("partial class AsyncMessage : g__ETPS.IMessage", StringComparison.Ordinal)
        );
        Assert.IsFalse(
            GetMessageSource(invalid).SourceText.ToString()
                .Contains(" : g__ETPS.IMessage", StringComparison.Ordinal)
        );
        StringAssert.Contains(GetMessageSource(sync).SourceText.ToString(), " : g__ETPS.IMessage");
        StringAssert.Contains(
              GetMessageSource(asyncOnly).SourceText.ToString()
            , "public readonly partial struct Async : g__ETPS.IMessage"
        );
        StringAssert.Contains(GetMessageSource(both).SourceText.ToString(), " : g__ETPS.IMessage");
        StringAssert.Contains(
              GetMessageSource(both).SourceText.ToString()
            , "public readonly partial struct Async : g__ETPS.IMessage"
        );
    }

    [TestMethod]
    public async Task StateMode_GatesGeneratedSubscribeCounterpartsOnly()
    {
        var stateless = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.CodeGen;
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, State = StateMode.Stateless, Scope = typeof(IOrderScope))]
            public partial class StatelessMessage { }
            """);
        var stateful = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.CodeGen;
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, State = StateMode.Stateful, Scope = typeof(IOrderScope))]
            public partial class StatefulMessage { }
            """);
        var both = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.CodeGen;
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, State = StateMode.Both, Scope = typeof(IOrderScope))]
            public partial class BothStateMessage { }
            """);
        var omitted = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, Scope = typeof(IOrderScope))]
            public partial class OmittedStateMessage { }
            """);
        var invalid = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.CodeGen;
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, State = (StateMode)99, Scope = typeof(IOrderScope))]
            public partial class InvalidStateMessage { }
            """);

        PubSubRuntimeFixture.AssertNoOutputErrors(stateless);
        PubSubRuntimeFixture.AssertNoOutputErrors(stateful);
        PubSubRuntimeFixture.AssertNoOutputErrors(both);
        PubSubRuntimeFixture.AssertNoOutputErrors(omitted);
        PubSubRuntimeFixture.AssertNoOutputErrors(invalid);
        Assert.AreEqual(16, CountSubscribe(stateless));
        Assert.AreEqual(32, CountSubscribe(stateful));
        Assert.AreEqual(48, CountSubscribe(both));
        Assert.AreEqual(48, CountSubscribe(omitted));
        Assert.AreEqual(48, CountSubscribe(invalid));
        Assert.AreEqual(5, CountCacheAndPublish(stateless));
        Assert.AreEqual(5, CountCacheAndPublish(stateful));
        Assert.AreEqual(5, CountCacheAndPublish(both));
    }

    private static int CountSubscribe(PubSubRun run)
        => GetScopeSources(run).SelectMany(GetMethods)
            .Count(static method => method.Identifier.ValueText == "Subscribe");

    private static int CountCacheAndPublish(PubSubRun run)
        => GetScopeSources(run).SelectMany(GetMethods)
            .Count(static method => method.Identifier.ValueText is "Cache" or "Publish");

    [TestMethod]
    public async Task RepeatedAttributes_SelectModesIndependentlyAndIgnoreInvalidOccurrence()
    {
        var run = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            [PubSub(ApiMode.Async, State = StateMode.Both, Scope = typeof(AbstractScope))]
            [PubSub((ApiMode)99, State = StateMode.Both, Scope = typeof(ViewScope))]
            public partial class Message { }
            """);

        AssertHintRoles(run, messageCount: 1, scopeCount: 2);
        PubSubRuntimeFixture.AssertNoOutputErrors(run);
        var sync = GetScopeSource(run, "global::TestProject.IOrderScope");
        var asyncOnly = GetScopeSource(run, "global::TestProject.AbstractScope");
        Assert.AreEqual(27, GetMethods(sync).Length);
        Assert.AreEqual(26, GetMethods(asyncOnly).Length);
        Assert.IsFalse(sync.SourceText.ToString().Contains("struct Async", StringComparison.Ordinal));
        StringAssert.Contains(asyncOnly.SourceText.ToString(), "struct Async");
        Assert.IsFalse(run.CombinedSource.Contains("global::TestProject.ViewScope", StringComparison.Ordinal));
        StringAssert.Contains(GetMessageSource(run).SourceText.ToString(), " : g__ETPS.IMessage");
        StringAssert.Contains(
              GetMessageSource(run).SourceText.ToString()
            , "public readonly partial struct Async : g__ETPS.IMessage"
        );
    }

    [TestMethod]
    public async Task MessageAndPerScopeSnapshots_AreExact()
    {
        var run = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both)]
            public partial class Message { }
            """);
        var snapshotDirectory = Path.Combine(
              FindSourceGeneratorRoot()
            , "EncosyTower.SourceGen.Tests"
            , "PubSub"
            , "Snapshots"
        );
        var messageMismatch = await GeneratedSourceSnapshot.VerifyAsync(
              GetMessageSource(run).SourceText.ToString()
            , Path.Combine(snapshotDirectory, "PubSubMessageGeneratorTests.Message.verified.cs")
        );
        var scopeMismatch = await GeneratedSourceSnapshot.VerifyAsync(
              GetScopeSources(run).Single().SourceText.ToString()
            , Path.Combine(snapshotDirectory, "PubSubMessageGeneratorTests.Scope.verified.cs")
        );

        Assert.IsNull(messageMismatch, messageMismatch);
        Assert.IsNull(scopeMismatch, scopeMismatch);
    }

    [TestMethod]
    public async Task GlobalNormalAndUnityScopes_EmitExactPerScopeFamiliesAndCompile()
    {
        var run = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, State = StateMode.Both)]
            [PubSub(ApiMode.Both, State = StateMode.Both, Scope = typeof(IOrderScope))]
            [PubSub(ApiMode.Both, State = StateMode.Both, Scope = typeof(ViewScope))]
            public partial class Message { }
            """);

        AssertHintRoles(run, messageCount: 1, scopeCount: 3);
        AssertGeneratorSucceeded(run);
        var message = GetMessageSource(run);
        var global = GetScopeSource(run, "g__ET.GlobalScope");
        var normal = GetScopeSource(run, "global::TestProject.IOrderScope");
        var unity = GetScopeSource(run, "global::TestProject.ViewScope");

        Assert.AreEqual(0, GetMethods(message).Length);
        Assert.AreEqual(
              2
            , message.SyntaxTree.GetRoot().DescendantNodes().OfType<ConversionOperatorDeclarationSyntax>().Count()
        );
        AssertMethodCounts(global, total: 90, cache: 2, publish: 8, subscribe: 80);
        AssertMethodCounts(normal, total: 53, cache: 1, publish: 4, subscribe: 48);
        AssertMethodCounts(unity, total: 53, cache: 1, publish: 4, subscribe: 48);
        StringAssert.Contains(message.SourceText.ToString(), "partial class Message : g__ETPS.IMessage");
        StringAssert.Contains(message.SourceText.ToString(), "public readonly partial struct Async : g__ETPS.IMessage");
        StringAssert.Contains(
              normal.SourceText.ToString()
            , "MessagePublisher.Publisher<global::TestProject.IOrderScope>"
        );
        StringAssert.Contains(
              unity.SourceText.ToString()
            , "MessagePublisher.UnityPublisher<global::TestProject.ViewScope>"
        );
        StringAssert.Contains(global.SourceText.ToString(), ".GlobalMessenger.Publisher.Global();");
        PubSubRuntimeFixture.AssertNoOutputErrors(run);
    }

    [TestMethod]
    public async Task DefaultAndExplicitGlobalScopes_KeepDistinctRenderingAndHints()
    {
        var run = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.Common;
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Sync, State = StateMode.Both)]
            public partial struct DefaultGlobalMessage { }

            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(GlobalScope))]
            public partial struct ExplicitGlobalMessage { }
            """);

        AssertHintRoles(run, messageCount: 2, scopeCount: 2);
        AssertGeneratorSucceeded(run);
        StringAssert.Contains(run.CombinedSource, "Publisher<g__ET.GlobalScope>");
        StringAssert.Contains(run.CombinedSource, "Publisher<global::EncosyTower.Common.GlobalScope>");
        PubSubRuntimeFixture.AssertNoOutputErrors(run);
    }

    [TestMethod]
    public async Task IneligibleParameterlessPublish_OmitsOnlyExistingOverloads()
    {
        var run = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, State = StateMode.Both)]
            public abstract partial class Message { }
            """);
        var scope = GetScopeSources(run).Single();

        AssertHintRoles(run, messageCount: 1, scopeCount: 1);
        AssertGeneratorSucceeded(run);
        AssertMethodCounts(scope, total: 86, cache: 2, publish: 4, subscribe: 80);
        PubSubRuntimeFixture.AssertNoOutputErrors(run);
    }

    [TestMethod]
    public async Task FixedAliases_AreExactForNonCollidingNestedGenericDeclaration()
    {
        var run = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            public partial record class Outer<TOuter>
                where TOuter : class, new()
            {
                [PubSub(ApiMode.Sync, State = StateMode.Both)]
                public partial record struct Message<TValue>
                    where TValue : struct
                {
                }
            }
            """);

        AssertHintRoles(run, messageCount: 1, scopeCount: 1);
        AssertGeneratorSucceeded(run);
        var source = run.CombinedSource;
        Assert.AreEqual(2, CountText(source, "using g__S = global::System;"));
        Assert.AreEqual(2, CountText(source, "using g__ETPS = global::EncosyTower.PubSub;"));
        Assert.IsFalse(source.Contains("using g__S_", StringComparison.Ordinal));
        StringAssert.Contains(source, "partial record class Outer<TOuter>");
        StringAssert.Contains(source, "where TOuter : class, new()");
        StringAssert.Contains(source, "partial record struct Message<TValue>");
        PubSubRuntimeFixture.AssertNoOutputErrors(run);
    }

    [TestMethod]
    public async Task DuplicateScopes_EmitEverySaltedOutputAndCompilerCollisions()
    {
        var run = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            public partial class Message { }
            """);
        var scopes = GetScopeSources(run);

        AssertHintRoles(run, messageCount: 1, scopeCount: 2);
        AssertGeneratorSucceeded(run);
        Assert.AreNotEqual(scopes[0].HintName, scopes[1].HintName);
        Assert.AreEqual(scopes[0].SourceText.ToString(), scopes[1].SourceText.ToString());
        Assert.IsTrue(run.OutputCompilation.GetDiagnostics().Any(static diagnostic =>
            diagnostic.Severity == DiagnosticSeverity.Error
            && diagnostic.Id is "CS0111" or "CS0121"
        ));
    }

    [TestMethod]
    public async Task MissingPartialAndInaccessibleScope_EmitAndRemainCompilerOwned()
    {
        var missingPartial = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Sync, State = StateMode.Both)]
            public class Message { }
            """);
        var inaccessibleScope = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            public static partial class Container
            {
                private sealed class Scope { }

                [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(Scope))]
                public partial class Message { }
            }
            """);

        AssertHintRoles(missingPartial, messageCount: 1, scopeCount: 1);
        AssertHintRoles(inaccessibleScope, messageCount: 1, scopeCount: 1);
        AssertGeneratorSucceeded(missingPartial);
        AssertGeneratorSucceeded(inaccessibleScope);
        AssertCompilerError(missingPartial, "CS0260");
        Assert.IsTrue(inaccessibleScope.OutputCompilation.GetDiagnostics().Any(static diagnostic =>
            diagnostic.Severity == DiagnosticSeverity.Error
            && diagnostic.Id is "CS0050" or "CS0051" or "CS0052" or "CS0053"
        ));
    }

    [TestMethod]
    public async Task InvalidScope_SkipsOnlyScopeOutputAndCannotEnableAsyncStorage()
    {
        var run = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Async, State = StateMode.Both, Scope = null)]
            public partial class Message { }
            """);

        AssertHintRoles(run, messageCount: 1, scopeCount: 0);
        AssertGeneratorSucceeded(run);
        Assert.IsFalse(run.CombinedSource.Contains("struct Async", StringComparison.Ordinal));
        PubSubRuntimeFixture.AssertNoOutputErrors(run);
    }

    [TestMethod]
    public async Task UnmarkedSkippedAndUnsupportedInputs_ProduceNoOutput()
    {
        var unmarked = await PubSubRuntimeFixture.RunAsync("namespace TestProject; public class Message { }");
        var skipped = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            [assembly: SkipSourceGeneratorsForAssembly]

            namespace TestProject;

            [PubSub(ApiMode.Sync, State = StateMode.Both)]
            public partial class Message { }
            """);
        var unsupported = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Sync, State = StateMode.Both)]
            public static class Message { }
            """);

        Assert.AreEqual(0, unmarked.Sources.Count);
        Assert.AreEqual(0, skipped.Sources.Count);
        Assert.AreEqual(0, unsupported.Sources.Count);
        AssertGeneratorSucceeded(unmarked);
        AssertGeneratorSucceeded(skipped);
        AssertGeneratorSucceeded(unsupported);
    }

    [TestMethod]
    public async Task PartialMarkers_UseOneMessageAndOneOutputPerValidOccurrence()
    {
        var run = await PubSubRuntimeFixture.RunAsync([
            new NamedSource("Z.Part.cs", """
                using EncosyTower.PubSub;

                namespace TestProject;

                [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
                public partial class Message { }
                """),
            new NamedSource("A.Part.cs", """
                using EncosyTower.PubSub;

                namespace TestProject;

                [PubSub(ApiMode.Both, State = StateMode.Both, Scope = typeof(AbstractScope))]
                public partial class Message { }
                """),
        ]);

        AssertHintRoles(run, messageCount: 1, scopeCount: 2);
        AssertGeneratorSucceeded(run);
        Assert.AreEqual(1, CountText(GetMessageSource(run).SourceText.ToString(), "private readonly "));
        PubSubRuntimeFixture.AssertNoOutputErrors(run);
    }

    [TestMethod]
    public async Task GeneratedSurface_UsesExactRoutesAnnotationsAndDirectForwarding()
    {
        var run = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, State = StateMode.Both, Scope = typeof(IOrderScope))]
            public partial struct Message { }
            """);
        var source = run.CombinedSource;

        AssertHintRoles(run, messageCount: 1, scopeCount: 1);
        AssertGeneratorSucceeded(run);
        StringAssert.Contains(source, "in g__ETPS.MessagePublisher.Publisher<global::TestProject.IOrderScope>");
        StringAssert.Contains(source, "in g__ETPS.MessageSubscriber.Subscriber<global::TestProject.IOrderScope>");
        StringAssert.Contains(source, "MessageSubscriberExtensions.WithState(in subscriber, state)");
        StringAssert.Contains(source, "[g__SDCA.NotNull]");
        StringAssert.Contains(source, "g__ST.CancellationToken unsubscribeToken");
        StringAssert.Contains(source, "where TState : class");
        StringAssert.Contains(source, "return publisher.PublishAsync(message, context);");
        Assert.IsFalse(source.Contains("CustomMessenger", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("ThrowHelper", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("PubSubMessageAttribute", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("MessagePublisher publisher", StringComparison.Ordinal));
        Assert.IsFalse(source.Contains("MessageSubscriber subscriber", StringComparison.Ordinal));
        PubSubRuntimeFixture.AssertNoOutputErrors(run);
    }

    [TestMethod]
    public async Task Outputs_HaveDeterministicDistinctHintsLfAndOneFinalNewline()
    {
        var source = """
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, State = StateMode.Both)]
            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            public partial class Message { }
            """;
        var first = await PubSubRuntimeFixture.RunAsync(source);
        var second = await PubSubRuntimeFixture.RunAsync(source);

        AssertHintRoles(first, messageCount: 1, scopeCount: 2);
        AssertGeneratorSucceeded(first);
        CollectionAssert.AreEqual(GetOrderedSources(first), GetOrderedSources(second));

        foreach (var generated in first.Sources)
        {
            var text = generated.SourceText.ToString();
            Assert.IsFalse(text.Contains('\r'));
            Assert.IsTrue(text.EndsWith("\n", StringComparison.Ordinal));
            Assert.IsFalse(text.EndsWith("\n\n", StringComparison.Ordinal));
        }
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task WrapTypeClassMessage_PublishesParameterlessOnlyWithDeclaredConstructor(
          bool declaresParameterlessConstructor
    )
    {
        var constructor = declaresParameterlessConstructor ? "public Points() : this(0) { }" : string.Empty;
        var caseName = declaresParameterlessConstructor ? "WrapTypeMessageWithConstructor" : "WrapTypeMessage";
        var run = await PubSubRuntimeFixture.RunAsync(
              sources: [
                  new NamedSource("Message.cs", $$"""
                      using EncosyTower.PubSub;
                      using EncosyTower.TypeWraps;

                      namespace TestProject;

                      [WrapType(typeof(int), "value")]
                      [PubSub(ApiMode.Both, State = StateMode.Both)]
                      public partial class Points
                      {
                          {{constructor}}
                      }
                      """),
              ]
            , previous: null
            , additionalGenerators: [new TypeWrapGenerator()]
        );
        var scope = GetScopeSources(run).Single();
        var typeWrapSources = run.AdditionalResults.Single().GeneratedSources;

        AssertHintRoles(run, messageCount: 1, scopeCount: 1);
        AssertGeneratorSucceeded(run);
        PubSubRuntimeFixture.AssertNoOutputErrors(run);
        Assert.AreEqual(1, typeWrapSources.Length);
        Assert.AreEqual("Points.TypeWrap.b758ebbfb7b8ec68.g.cs", typeWrapSources[0].HintName);

        AssertMethodCounts(
              scope
            , total: declaresParameterlessConstructor ? 90 : 86
            , cache: 2
            , publish: declaresParameterlessConstructor ? 8 : 4
            , subscribe: 80
        );

        var snapshotDirectory = Path.Combine(
              FindSourceGeneratorRoot()
            , "EncosyTower.SourceGen.Tests"
            , "PubSub"
            , "Snapshots"
        );

        var messageMismatch = await GeneratedSourceSnapshot.VerifyAsync(
              GetMessageSource(run).SourceText.ToString()
            , Path.Combine(snapshotDirectory, "PubSubMessageGeneratorTests.WrapTypeMessage.Message.verified.cs")
        );

        var scopeMismatch = await GeneratedSourceSnapshot.VerifyAsync(
              scope.SourceText.ToString()
            , Path.Combine(snapshotDirectory, $"PubSubMessageGeneratorTests.{caseName}.Scope.verified.cs")
        );

        var typeWrapMismatch = await GeneratedSourceSnapshot.VerifyAsync(
              typeWrapSources[0].SourceText.ToString()
            , Path.Combine(snapshotDirectory, $"PubSubMessageGeneratorTests.{caseName}.TypeWrap.verified.cs")
        );

        Assert.IsNull(messageMismatch, messageMismatch);
        Assert.IsNull(scopeMismatch, scopeMismatch);
        Assert.IsNull(typeWrapMismatch, typeWrapMismatch);
    }

    private static GeneratedSourceResult GetMessageSource(PubSubRun run)
        => run.Sources.Single(static source => source.HintName.Contains(
              ".PubSubMessage."
            , StringComparison.Ordinal
        ));

    private static GeneratedSourceResult[] GetScopeSources(PubSubRun run)
        => run.Sources.Where(static source => source.HintName.Contains(
              ".PubSubScope."
            , StringComparison.Ordinal
        )).ToArray();

    private static GeneratedSourceResult GetScopeSource(PubSubRun run, string scopeType)
        => GetScopeSources(run).Single(source => source.SourceText.ToString().Contains(
              scopeType
            , StringComparison.Ordinal
        ));

    private static MethodDeclarationSyntax[] GetMethods(GeneratedSourceResult source)
        => source.SyntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToArray();

    private static string[] GetOrderedSources(PubSubRun run)
        => run.Sources
            .OrderBy(static source => source.HintName, StringComparer.Ordinal)
            .Select(static source => source.HintName + "\n" + source.SourceText)
            .ToArray();

    private static void AssertMethodCounts(
        GeneratedSourceResult source,
        int total,
        int cache,
        int publish,
        int subscribe
    )
    {
        var methods = GetMethods(source);
        Assert.AreEqual(total, methods.Length);
        Assert.AreEqual(cache, methods.Count(static method => method.Identifier.ValueText == "Cache"));
        Assert.AreEqual(publish, methods.Count(static method => method.Identifier.ValueText == "Publish"));
        Assert.AreEqual(subscribe, methods.Count(static method => method.Identifier.ValueText == "Subscribe"));
    }

    private static void AssertHintRoles(PubSubRun run, int messageCount, int scopeCount)
    {
        Assert.AreEqual(messageCount, run.Sources.Count(static source => source.HintName.Contains(
              ".PubSubMessage."
            , StringComparison.Ordinal
        )));
        Assert.AreEqual(scopeCount, GetScopeSources(run).Length);
        Assert.AreEqual(run.Sources.Count, run.Sources.Select(static source => source.HintName).Distinct().Count());
    }

    private static void AssertGeneratorSucceeded(PubSubRun run)
    {
        Assert.IsNull(run.Result.Exception);
        Assert.AreEqual(0, run.Result.Diagnostics.Length);
    }

    private static void AssertCompilerError(PubSubRun run, string id)
        => Assert.IsTrue(run.OutputCompilation.GetDiagnostics().Any(diagnostic =>
            diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id == id
        ));

    private static string FindSourceGeneratorRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (File.Exists(Path.Combine(directory.FullName, "EncosyTower.SourceGen.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate EncosyTower.SourceGen.slnx.");
    }

    private static int CountText(string source, string value)
        => source.Split(value, StringSplitOptions.None).Length - 1;
}

internal sealed class PubSubProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    public string FeaturePath => "PubSub";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts { get; } = [
        new(
              typeof(PubSubMessageGenerator)
            , "PubSub/PubSubMessageGeneratorTests.cs"
            , "GlobalNormalAndUnityScopes_EmitExactPerScopeFamiliesAndCompile"
            , "public partial class Message"
            , "public partial class RenamedMessage"
        ),
    ];
}
