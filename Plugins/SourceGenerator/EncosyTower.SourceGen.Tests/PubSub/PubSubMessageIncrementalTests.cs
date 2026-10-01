using System.Collections.Immutable;
using System.Text;
using EncosyTower.PubSub.Generators;

namespace EncosyTower.SourceGen.Tests.PubSub;

[TestClass]
public sealed class PubSubMessageIncrementalTests
{
    [TestMethod]
    public async Task UnrelatedEdit_KeepsMessageAndScopeOutputsStable()
    {
        var message = new NamedSource("Message.cs", """
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, State = StateMode.Both)]
            public partial class Message { }
            """);
        var first = await PubSubRuntimeFixture.RunAsync([
            message,
            new NamedSource("Unrelated.cs", "internal sealed class UnrelatedA { }"),
        ]);
        var second = await PubSubRuntimeFixture.RunAsync([
            message,
            new NamedSource("Unrelated.cs", "internal sealed class UnrelatedB { }"),
        ], first);

        AssertStableReasons(second, "PubSubMessageGenerator.SharedMessages");
        AssertStableReasons(second, "PubSubMessageGenerator.MessageOutputs");
        AssertStableReasons(second, "PubSubMessageGenerator.ScopeOutputs");
        CollectionAssert.AreEqual(GetSources(first), GetSources(second));
    }

    [TestMethod]
    public async Task AsyncScopeEdit_ChangesOnlyEditedScopeOutput()
    {
        var first = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, State = StateMode.Both)]
            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            public partial class Message { }
            """);
        var second = await PubSubRuntimeFixture.RunAsync([
            new NamedSource("Message.cs", """
                using EncosyTower.PubSub;

                namespace TestProject;

                [PubSub(ApiMode.Both, State = StateMode.Both)]
                [PubSub(ApiMode.Both, State = StateMode.Both, Scope = typeof(IOrderScope))]
                public partial class Message { }
                """),
        ], first);

        Assert.AreEqual(GetMessageSource(first), GetMessageSource(second));
        Assert.AreEqual(GetScopeSource(first, "g__ET.GlobalScope"), GetScopeSource(second, "g__ET.GlobalScope"));
        Assert.AreNotEqual(
              GetScopeSource(first, "global::TestProject.IOrderScope")
            , GetScopeSource(second, "global::TestProject.IOrderScope")
        );
        AssertStableReasons(second, "PubSubMessageGenerator.MessageOutputs");
        AssertReasonsInclude(second, "PubSubMessageGenerator.ScopeOutputs", IncrementalStepRunReason.Modified);
        AssertReasonsInclude(second, "PubSubMessageGenerator.ScopeOutputs", IncrementalStepRunReason.Unchanged);
    }

    [TestMethod]
    public async Task StateEdit_ChangesOnlyEditedScopeOutput()
    {
        var first = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.CodeGen;
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both, State = StateMode.Stateless, Scope = typeof(IOrderScope))]
            public partial class Message { }
            """);
        var second = await PubSubRuntimeFixture.RunAsync([
            new NamedSource("Message.cs", """
                using EncosyTower.CodeGen;
                using EncosyTower.PubSub;

                namespace TestProject;

                [PubSub(ApiMode.Both, State = StateMode.Stateful, Scope = typeof(IOrderScope))]
                public partial class Message { }
                """),
        ], first);

        AssertStableReasons(second, "PubSubMessageGenerator.MessageOutputs");
        AssertReasonsInclude(second, "PubSubMessageGenerator.ScopeOutputs", IncrementalStepRunReason.Modified);
        Assert.AreNotEqual(GetCombinedSubscribeCount(first), GetCombinedSubscribeCount(second));
    }

    private static int GetCombinedSubscribeCount(PubSubRun run)
        => run.Sources.SelectMany(static source => source.SyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
            .Count(static method => method.Identifier.ValueText == "Subscribe");

    [TestMethod]
    public async Task MarkerOrder_PreservesEveryOccurrenceInSourceOrderWithUniqueHints()
    {
        var run = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            [PubSub(ApiMode.Sync, State = StateMode.Both)]
            [PubSub(ApiMode.Both, State = StateMode.Both, Scope = typeof(AbstractScope))]
            public partial class Message { }
            """);
        var scopes = GetScopeSources(run);

        Assert.AreEqual(3, scopes.Length);
        StringAssert.Contains(scopes[0].SourceText.ToString(), "global::TestProject.IOrderScope");
        StringAssert.Contains(scopes[1].SourceText.ToString(), "g__ET.GlobalScope");
        StringAssert.Contains(scopes[2].SourceText.ToString(), "global::TestProject.AbstractScope");
        Assert.AreEqual(3, scopes.Select(static source => source.HintName).Distinct().Count());
    }

    [TestMethod]
    public async Task ReusedDriverRemovalAndFreshRuns_AreDeterministic()
    {
        var message = new NamedSource("Message.cs", """
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Sync, State = StateMode.Both)]
            public partial struct Message { }
            """);
        var first = await PubSubRuntimeFixture.RunAsync([message]);
        var same = await PubSubRuntimeFixture.RunAsync([message], first);
        var fresh = await PubSubRuntimeFixture.RunAsync([message]);
        var removed = await PubSubRuntimeFixture.RunAsync(Array.Empty<NamedSource>(), same);

        CollectionAssert.AreEqual(GetSources(first), GetSources(same));
        CollectionAssert.AreEqual(GetSources(first), GetSources(fresh));
        AssertStableReasons(same, "PubSubMessageGenerator.MessageOutputs");
        AssertStableReasons(same, "PubSubMessageGenerator.ScopeOutputs");
        Assert.AreEqual(0, removed.Sources.Count);
    }

    [TestMethod]
    public async Task ThousandTargets_ProduceTwoUniqueCompilableOutputsEach()
    {
        const int COUNT = 1_000;
        var source = new StringBuilder("using EncosyTower.PubSub; namespace Scale;");

        for (var i = 0; i < COUNT; i++)
        {
            source.Append("[PubSub(ApiMode.Sync, State = StateMode.Both, Scope = typeof(TestProject.IOrderScope))]")
                .Append("public partial struct Message").Append(i).Append(" { }");
        }

        var run = await PubSubRuntimeFixture.RunAsync(source.ToString());

        Assert.AreEqual(COUNT * 2, run.Sources.Count);
        Assert.AreEqual(COUNT * 2, run.Sources.Select(static item => item.HintName).Distinct().Count());
        Assert.AreEqual(COUNT, GetMessageSources(run).Length);
        Assert.AreEqual(COUNT, GetScopeSources(run).Length);
        PubSubRuntimeFixture.AssertNoOutputErrors(run);
        AssertTrackedOutputCount(run, "PubSubMessageGenerator.MessageOutputs", COUNT);
        AssertTrackedOutputCount(run, "PubSubMessageGenerator.ScopeOutputs", COUNT);
    }

    [TestMethod]
    public void Specs_UseEveryOutputFactInEqualityAndHashing()
    {
        var declaration = new PubSubTypeDeclarationSpec(
              "opening"
            , "closing"
            , "Message"
            , "class"
            , "global::Test.Message"
            , "Test.Message"
            , true
        );
        var scope = new PubSubScopeSpec(
              declaration
            , "global::Test.Scope"
            , "scope.g.cs"
            , true
            , true
            , true
            , true
            , true
            , false
            , false
        );
        var scopes = ImmutableArray.Create(scope).AsEquatableArray();
        var message = new PubSubMessageSpec(declaration, scopes, "message.g.cs", true, true, true);
        var equalDeclaration = new PubSubTypeDeclarationSpec(
              "opening"
            , "closing"
            , "Message"
            , "class"
            , "global::Test.Message"
            , "Test.Message"
            , true
        );
        var equalScope = new PubSubScopeSpec(
              equalDeclaration
            , "global::Test.Scope"
            , "scope.g.cs"
            , true
            , true
            , true
            , true
            , true
            , false
            , false
        );
        var equalMessage = new PubSubMessageSpec(
              equalDeclaration
            , ImmutableArray.Create(equalScope).AsEquatableArray()
            , "message.g.cs"
            , true
            , true
            , true
        );
        var transitiveDeclaration = new PubSubTypeDeclarationSpec(
              "opening"
            , "closing"
            , "Message"
            , "class"
            , "global::Test.Message"
            , "Test.Message"
            , true
        );
        var transitiveScope = new PubSubScopeSpec(
              transitiveDeclaration
            , "global::Test.Scope"
            , "scope.g.cs"
            , true
            , true
            , true
            , true
            , true
            , false
            , false
        );
        var transitiveMessage = new PubSubMessageSpec(
              transitiveDeclaration
            , ImmutableArray.Create(transitiveScope).AsEquatableArray()
            , "message.g.cs"
            , true
            , true
            , true
        );

        AssertEqualityLaws(declaration, equalDeclaration, transitiveDeclaration);
        AssertEqualityLaws(scope, equalScope, transitiveScope);
        AssertEqualityLaws(message, equalMessage, transitiveMessage);
        AssertEqualityLaws(default(PubSubTypeDeclarationSpec), default, default);
        AssertEqualityLaws(default(PubSubScopeSpec), default, default);
        AssertEqualityLaws(default(PubSubMessageSpec), default, default);
        AssertEqualityLaws(
              default(PubSubMessageSpec)
            , new PubSubMessageSpec(
                  default
                , ImmutableArray<PubSubScopeSpec>.Empty.AsEquatableArray()
                , null!
                , false
                , false
                , false
              )
            , default
        );
        AssertEqualityLaws(
              new PubSubTypeDeclarationSpec(null!, null!, null!, null!, null!, null!, false)
            , new PubSubTypeDeclarationSpec(null!, null!, null!, null!, null!, null!, false)
            , new PubSubTypeDeclarationSpec(null!, null!, null!, null!, null!, null!, false)
        );

        AssertDistinct(declaration, new PubSubTypeDeclarationSpec(
            "changed", "closing", "Message", "class", "global::Test.Message", "Test.Message", true
        ));
        AssertDistinct(declaration, new PubSubTypeDeclarationSpec(
            "opening", "changed", "Message", "class", "global::Test.Message", "Test.Message", true
        ));
        AssertDistinct(declaration, new PubSubTypeDeclarationSpec(
            "opening", "closing", "Changed", "class", "global::Test.Message", "Test.Message", true
        ));
        AssertDistinct(declaration, new PubSubTypeDeclarationSpec(
            "opening", "closing", "Message", "struct", "global::Test.Message", "Test.Message", true
        ));
        AssertDistinct(declaration, new PubSubTypeDeclarationSpec(
            "opening", "closing", "Message", "class", "global::Test.Changed", "Test.Message", true
        ));
        AssertDistinct(declaration, new PubSubTypeDeclarationSpec(
            "opening", "closing", "Message", "class", "global::Test.Message", "Test.Changed", true
        ));
        AssertDistinct(declaration, new PubSubTypeDeclarationSpec(
            "opening", "closing", "Message", "class", "global::Test.Message", "Test.Message", false
        ));
        AssertDistinct(scope, new PubSubScopeSpec(
            default, "global::Test.Scope", "scope.g.cs", true, true, true, true, true, false, false
        ));
        AssertDistinct(scope, new PubSubScopeSpec(
            declaration, "global::Test.Changed", "scope.g.cs", true, true, true, true, true, false, false
        ));
        AssertDistinct(scope, new PubSubScopeSpec(
            declaration, "global::Test.Scope", "changed.g.cs", true, true, true, true, true, false, false
        ));
        AssertDistinct(scope, new PubSubScopeSpec(
            declaration, "global::Test.Scope", "scope.g.cs", false, true, true, true, true, false, false
        ));
        AssertDistinct(scope, new PubSubScopeSpec(
            declaration, "global::Test.Scope", "scope.g.cs", true, false, true, true, true, false, false
        ));
        AssertDistinct(scope, new PubSubScopeSpec(
            declaration, "global::Test.Scope", "scope.g.cs", true, true, false, true, true, false, false
        ));
        AssertDistinct(scope, new PubSubScopeSpec(
            declaration, "global::Test.Scope", "scope.g.cs", true, true, true, false, true, false, false
        ));
        AssertDistinct(scope, new PubSubScopeSpec(
            declaration, "global::Test.Scope", "scope.g.cs", true, true, true, true, false, false, false
        ));
        AssertDistinct(scope, new PubSubScopeSpec(
            declaration, "global::Test.Scope", "scope.g.cs", true, true, true, true, true, true, false
        ));
        AssertDistinct(scope, new PubSubScopeSpec(
            declaration, "global::Test.Scope", "scope.g.cs", true, true, true, true, true, false, true
        ));
        AssertDistinct(message, new PubSubMessageSpec(default, scopes, "message.g.cs", true, true, true));
        AssertDistinct(message, new PubSubMessageSpec(declaration, default, "message.g.cs", true, true, true));
        AssertDistinct(message, new PubSubMessageSpec(declaration, scopes, "changed.g.cs", true, true, true));
        AssertDistinct(message, new PubSubMessageSpec(declaration, scopes, "message.g.cs", false, true, true));
        AssertDistinct(message, new PubSubMessageSpec(declaration, scopes, "message.g.cs", true, false, true));
        AssertDistinct(message, new PubSubMessageSpec(declaration, scopes, "message.g.cs", true, true, false));

        Assert.AreEqual(new CollisionValue(1).GetHashCode(), new CollisionValue(2).GetHashCode());
        AssertDistinct(new CollisionValue(1), new CollisionValue(2));
    }

    private static GeneratedSourceResult[] GetMessageSources(PubSubRun run)
        => run.Sources.Where(static source => source.HintName.Contains(
              ".PubSubMessage."
            , StringComparison.Ordinal
        )).ToArray();

    private static GeneratedSourceResult[] GetScopeSources(PubSubRun run)
        => run.Sources.Where(static source => source.HintName.Contains(
              ".PubSubScope."
            , StringComparison.Ordinal
        )).ToArray();

    private static string GetMessageSource(PubSubRun run)
        => GetMessageSources(run).Single().SourceText.ToString();

    private static string GetScopeSource(PubSubRun run, string scopeType)
        => GetScopeSources(run).Single(source => source.SourceText.ToString().Contains(
              scopeType
            , StringComparison.Ordinal
        )).SourceText.ToString();

    private static string[] GetSources(PubSubRun run)
        => run.Sources
            .OrderBy(static source => source.HintName, StringComparer.Ordinal)
            .Select(static source => source.HintName + "\n" + source.SourceText)
            .ToArray();

    private static void AssertTrackedOutputCount(PubSubRun run, string trackingName, int count)
    {
        Assert.IsTrue(run.Result.TrackedSteps.TryGetValue(trackingName, out var steps));
        Assert.AreEqual(count, steps.SelectMany(static step => step.Outputs).Count());
    }

    private static void AssertReasonsInclude(
        PubSubRun run,
        string trackingName,
        IncrementalStepRunReason expected
    )
    {
        Assert.IsTrue(run.Result.TrackedSteps.TryGetValue(trackingName, out var steps));
        Assert.IsTrue(steps.SelectMany(static step => step.Outputs).Any(output => output.Reason == expected));
    }

    private static void AssertStableReasons(PubSubRun run, string trackingName)
    {
        Assert.IsTrue(run.Result.TrackedSteps.TryGetValue(trackingName, out var steps));
        var outputs = steps.SelectMany(static step => step.Outputs).ToArray();
        Assert.IsTrue(outputs.Length > 0);

        foreach (var output in outputs)
        {
            Assert.IsTrue(output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged);
        }
    }

    private static void AssertDistinct<T>(T left, T right)
        where T : struct, IEquatable<T>
        => Assert.IsFalse(left.Equals(right));

    private static void AssertEqualityLaws<T>(T first, T second, T third)
        where T : struct, IEquatable<T>
    {
        Assert.IsTrue(first.Equals(first));
        Assert.IsTrue(first.Equals(second));
        Assert.IsTrue(second.Equals(first));
        Assert.IsTrue(second.Equals(third));
        Assert.IsTrue(first.Equals(third));
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        Assert.AreEqual(second.GetHashCode(), third.GetHashCode());
    }

    private readonly struct CollisionValue : IEquatable<CollisionValue>
    {
        private readonly int _value;

        public CollisionValue(int value)
        {
            _value = value;
        }

        public bool Equals(CollisionValue other)
            => _value == other._value;

        public override bool Equals(object? obj)
            => obj is CollisionValue other && Equals(other);

        public override int GetHashCode()
            => 0;
    }
}
