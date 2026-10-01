using System.Collections.Immutable;
using EncosyTower.Processing.Generators;

namespace EncosyTower.SourceGen.Tests.Processing;

[TestClass]
public sealed class ProcessingRequestIncrementalTests
{
    [TestMethod]
    public async Task UnrelatedEdit_KeepsRequestAndScopeOutputsUnchanged()
    {
        var request = new NamedSource("Request.cs", """
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            public partial class Request { }
            """);
        var first = await ProcessingRequestFixture.RunAsync([
            request,
            new NamedSource("Unrelated.cs", "internal sealed class UnrelatedA { }"),
        ]);
        var second = await ProcessingRequestFixture.RunAsync([
            request,
            new NamedSource("Unrelated.cs", "internal sealed class UnrelatedB { }"),
        ], first);

        AssertStableReasons(second, "ProcessingRequestGenerator.RequestOutputs");
        AssertStableReasons(second, "ProcessingRequestGenerator.ScopeOutputs");
    }

    [TestMethod]
    public async Task AsyncContributionEdit_ChangesCommonAndEditedScopeOutputs()
    {
        var first = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Sync, State = StateMode.Both, Scope = typeof(IOrderScope))]
            public partial class Request { }
            """);
        var second = await ProcessingRequestFixture.RunAsync([
            new NamedSource("Request.cs", """
                using EncosyTower.Processing;

                namespace TestProject;

                [Processing(ApiMode.Both, State = StateMode.Both, Scope = typeof(IOrderScope))]
                public partial class Request { }
                """),
        ], first);

        AssertReasons(second, "ProcessingRequestGenerator.RequestOutputs", IncrementalStepRunReason.Modified);
        AssertReasons(second, "ProcessingRequestGenerator.ScopeOutputs", IncrementalStepRunReason.Modified);
        StringAssert.Contains(second.CombinedSource, "public readonly partial struct Async");
    }

    [TestMethod]
    public async Task StateEdit_ChangesOnlyEditedScopeOutput()
    {
        var first = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.CodeGen;
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Both, State = StateMode.Stateless, Scope = typeof(IOrderScope))]
            public partial class Request { }
            """);
        var second = await ProcessingRequestFixture.RunAsync([
            new NamedSource("Request.cs", """
                using EncosyTower.CodeGen;
                using EncosyTower.Processing;

                namespace TestProject;

                [Processing(ApiMode.Both, State = StateMode.Stateful, Scope = typeof(IOrderScope))]
                public partial class Request { }
                """),
        ], first);

        AssertStableReasons(second, "ProcessingRequestGenerator.RequestOutputs");
        AssertReasons(second, "ProcessingRequestGenerator.ScopeOutputs", IncrementalStepRunReason.Modified);
        Assert.AreNotEqual(GetCombinedRegisterCount(first), GetCombinedRegisterCount(second));
    }

    private static int GetCombinedRegisterCount(ProcessingRun run)
        => run.Sources.SelectMany(static source => source.SyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
            .Count(static method => method.Identifier.ValueText == "Register");

    [TestMethod]
    public void Specs_UseEveryOutputFactInEqualityAndHashing()
    {
        var declaration = CreateDeclaration();
        var equalDeclaration = CreateDeclaration();
        var transitiveDeclaration = CreateDeclaration();
        var scopeSpec = CreateScope(declaration);
        var equalScope = CreateScope(equalDeclaration);
        var transitiveScope = CreateScope(transitiveDeclaration);
        var request = CreateRequest(declaration, scopeSpec);
        var equalRequest = CreateRequest(equalDeclaration, equalScope);
        var transitiveRequest = CreateRequest(transitiveDeclaration, transitiveScope);

        AssertEqualityLaws(declaration, equalDeclaration, transitiveDeclaration);
        AssertEqualityLaws(scopeSpec, equalScope, transitiveScope);
        AssertEqualityLaws(request, equalRequest, transitiveRequest);
        AssertEqualityLaws(
              default(ProcessingTypeDeclarationSpec)
            , default
            , default
        );
        AssertEqualityLaws(default(ProcessingScopeSpec), default, default);
        AssertEqualityLaws(default(ProcessingRequestSpec), default, default);
        AssertEqualityLaws(
              default(ProcessingRequestSpec)
            , new ProcessingRequestSpec(
                  default
                , ImmutableArray<ProcessingScopeSpec>.Empty.AsEquatableArray()
                , null!
                , null!
                , false
                , false
                , false
              )
            , default
        );
        AssertEqualityLaws(
              new ProcessingTypeDeclarationSpec(null!, null!, null!, null!, null!, null!, false)
            , new ProcessingTypeDeclarationSpec(null!, null!, null!, null!, null!, null!, false)
            , new ProcessingTypeDeclarationSpec(null!, null!, null!, null!, null!, null!, false)
        );

        AssertDistinct(declaration, new ProcessingTypeDeclarationSpec(
            "changed", "closing", "Request", "class", "global::Test.Request", "Test.Request", true
        ));
        AssertDistinct(declaration, new ProcessingTypeDeclarationSpec(
            "opening", "changed", "Request", "class", "global::Test.Request", "Test.Request", true
        ));
        AssertDistinct(declaration, new ProcessingTypeDeclarationSpec(
            "opening", "closing", "Changed", "class", "global::Test.Request", "Test.Request", true
        ));
        AssertDistinct(declaration, new ProcessingTypeDeclarationSpec(
            "opening", "closing", "Request", "struct", "global::Test.Request", "Test.Request", true
        ));
        AssertDistinct(declaration, new ProcessingTypeDeclarationSpec(
            "opening", "closing", "Request", "class", "global::Test.Changed", "Test.Request", true
        ));
        AssertDistinct(declaration, new ProcessingTypeDeclarationSpec(
            "opening", "closing", "Request", "class", "global::Test.Request", "Test.Changed", true
        ));
        AssertDistinct(declaration, new ProcessingTypeDeclarationSpec(
            "opening", "closing", "Request", "class", "global::Test.Request", "Test.Request", false
        ));
        AssertDistinct(scopeSpec, new ProcessingScopeSpec(
              default
            , "global::System.Int32"
            , "global::Test.Scope"
            , "scope.g.cs"
            , true
            , true
            , true
            , true
            , true
            , false
            , false
        ));
        AssertDistinct(scopeSpec, new ProcessingScopeSpec(
              declaration
            , "global::System.String"
            , "global::Test.Scope"
            , "scope.g.cs"
            , true
            , true
            , true
            , true
            , true
            , false
            , false
        ));
        AssertDistinct(scopeSpec, new ProcessingScopeSpec(
              declaration
            , "global::System.Int32"
            , "global::Test.Changed"
            , "scope.g.cs"
            , true
            , true
            , true
            , true
            , true
            , false
            , false
        ));
        AssertDistinct(scopeSpec, new ProcessingScopeSpec(
              declaration
            , "global::System.Int32"
            , "global::Test.Scope"
            , "changed.g.cs"
            , true
            , true
            , true
            , true
            , true
            , false
            , false
        ));
        AssertDistinct(scopeSpec, new ProcessingScopeSpec(
            declaration, "global::System.Int32", "global::Test.Scope", "scope.g.cs"
            , false, true, true, true, true, false, false
        ));
        AssertDistinct(scopeSpec, new ProcessingScopeSpec(
            declaration, "global::System.Int32", "global::Test.Scope", "scope.g.cs"
            , true, false, true, true, true, false, false
        ));
        AssertDistinct(scopeSpec, new ProcessingScopeSpec(
            declaration, "global::System.Int32", "global::Test.Scope", "scope.g.cs"
            , true, true, false, true, true, false, false
        ));
        AssertDistinct(scopeSpec, new ProcessingScopeSpec(
            declaration, "global::System.Int32", "global::Test.Scope", "scope.g.cs"
            , true, true, true, false, true, false, false
        ));
        AssertDistinct(scopeSpec, new ProcessingScopeSpec(
            declaration, "global::System.Int32", "global::Test.Scope", "scope.g.cs"
            , true, true, true, true, false, false, false
        ));
        AssertDistinct(scopeSpec, new ProcessingScopeSpec(
            declaration, "global::System.Int32", "global::Test.Scope", "scope.g.cs"
            , true, true, true, true, true, true, false
        ));
        AssertDistinct(scopeSpec, new ProcessingScopeSpec(
            declaration, "global::System.Int32", "global::Test.Scope", "scope.g.cs"
            , true, true, true, true, true, false, true
        ));

        var scopes = ImmutableArray.Create(scopeSpec).AsEquatableArray();
        AssertDistinct(request, new ProcessingRequestSpec(
              default
            , scopes
            , "global::System.Int32"
            , "request.g.cs"
            , true
            , false
            , true
        ));
        AssertDistinct(request, new ProcessingRequestSpec(
            declaration, scopes, "global::System.String", "request.g.cs", true, false, true
        ));
        AssertDistinct(request, new ProcessingRequestSpec(
            declaration, scopes, "global::System.Int32", "changed.g.cs", true, false, true
        ));
        AssertDistinct(request, new ProcessingRequestSpec(
            declaration, default, "global::System.Int32", "request.g.cs", true, false, true
        ));
        AssertDistinct(request, new ProcessingRequestSpec(
            declaration, scopes, "global::System.Int32", "request.g.cs", false, false, true
        ));
        AssertDistinct(request, new ProcessingRequestSpec(
            declaration, scopes, "global::System.Int32", "request.g.cs", true, true, true
        ));
        AssertDistinct(request, new ProcessingRequestSpec(
            declaration, scopes, "global::System.Int32", "request.g.cs", true, false, false
        ));

        Assert.AreEqual(new CollisionValue(1).GetHashCode(), new CollisionValue(2).GetHashCode());
        AssertDistinct(new CollisionValue(1), new CollisionValue(2));
    }

    private static ProcessingTypeDeclarationSpec CreateDeclaration()
        => new(
              "opening"
            , "closing"
            , "Request"
            , "class"
            , "global::Test.Request"
            , "Test.Request"
            , true
        );

    private static ProcessingScopeSpec CreateScope(ProcessingTypeDeclarationSpec declaration)
        => new(
              declaration
            , "global::System.Int32"
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

    private static ProcessingRequestSpec CreateRequest(
          ProcessingTypeDeclarationSpec declaration
        , ProcessingScopeSpec scope
    )
        => new(
              declaration
            , ImmutableArray.Create(scope).AsEquatableArray()
            , "global::System.Int32"
            , "request.g.cs"
            , true
            , false
            , true
        );

    private static void AssertReasons(
        ProcessingRun run,
        string trackingName,
        IncrementalStepRunReason expected
    )
    {
        Assert.IsTrue(run.Result.TrackedSteps.TryGetValue(trackingName, out var steps));
        var outputs = steps.SelectMany(static step => step.Outputs).ToArray();
        Assert.IsTrue(outputs.Length > 0);

        foreach (var output in outputs)
        {
            Assert.AreEqual(expected, output.Reason);
        }
    }

    private static void AssertStableReasons(ProcessingRun run, string trackingName)
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
