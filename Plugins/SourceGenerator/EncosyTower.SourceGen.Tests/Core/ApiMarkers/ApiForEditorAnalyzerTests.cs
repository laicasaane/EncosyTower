using EncosyTower.Core.Analyzers.ApiMarkers;
using Microsoft.CodeAnalysis.CSharp;
using static EncosyTower.SourceGen.Tests.Core.ApiMarkers.ApiMarkerTestHelper;

namespace EncosyTower.SourceGen.Tests.Core.ApiMarkers;

[TestClass]
public sealed class ApiForEditorAnalyzerTests
{
    [TestMethod]
    public Task Player_ReportsEachDirectMemberUse()
        => VerifyAsync(
              """
              public class Consumer
              {
                  public void Run()
                  {
                      Api.{|#0:Editor|}();
                      Api.Authoring();
                      Api.{|#1:Field|} = Api.{|#2:Property|};
                      Api.{|#3:Changed|} += Api.{|#4:Editor|};
                      _ = new {|#5:Api|}();
                  }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "Api.Editor()")
            , Editor(location: 1, api: "Api.Field")
            , Editor(location: 2, api: "Api.Property")
            , Editor(location: 3, api: "Api.Changed")
            , Editor(location: 4, api: "Api.Editor()")
            , Editor(location: 5, api: "Api.Api()")
        );

    [TestMethod]
    public Task EditorBuild_AllUsesAreAllowed()
        => VerifyAsync(
              """
              public class Consumer : EditorType
              {
                  public void Run()
                  {
                      Api.Editor();
                      Api.Authoring();
                      Api.Field = Api.Property;
                      _ = new Api();
                      _ = typeof(EditorType.Nested);
                  }
              }
              """
            , new[] { "UNITY_EDITOR" }
        );

    [TestMethod]
    public Task AuthoringBuild_AllowsAuthoringButReportsEditorUse()
        => VerifyAsync(
              """
              public class Consumer
              {
                  public void Run()
                  {
                      Api.Authoring();
                      _ = new AuthoringType();
                      Api.{|#0:Editor|}();
                  }
              }
              """
            , new[] { "ENCOSY_INCLUDE_AUTHORING" }
            , Editor(location: 0, api: "Api.Editor()")
        );

    [TestMethod]
    public Task MarkedMembers_ApplyAsymmetricExemptions()
        => VerifyAsync(
              """
              public class Consumer
              {
                  [EncosyTower.Core.ApiForEditor]
                  public void Editor()
                  {
                      Api.Editor();
                      Api.Authoring();
                      void Local() { Api.Editor(); }
                      System.Action closure = () => Api.Editor();
                      Local();
                      closure();
                  }

                  [EncosyTower.Core.ApiForAuthoring]
                  public void Authoring()
                  {
                      Api.Authoring();
                      Api.{|#0:Editor|}();
                  }

                  [EncosyTower.Core.ApiForEditor]
                  public EditorType Property => new EditorType();
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "Api.Editor()")
        );

    [TestMethod]
    public Task MarkedContainingTypes_ExemptNestedMembers()
        => VerifyAsync(
              """
              [EncosyTower.Core.ApiForEditor]
              public partial class EditorConsumer { }

              public partial class EditorConsumer
              {
                  public class Nested
                  {
                      public void Run() { Api.Editor(); Api.Authoring(); }
                  }
              }

              [EncosyTower.Core.ApiForAuthoring]
              public class AuthoringConsumer
              {
                  public class Nested
                  {
                      public void Run() { Api.Authoring(); Api.{|#0:Editor|}(); }
                  }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "Api.Editor()")
        );

    [TestMethod]
    public Task MarkedTypes_ReportReferencesConstructionInheritanceAndGenericArguments()
        => VerifyAsync(
              """
              public class Consumer : {|#0:EditorType|}
              {
                  public {|#1:EditorType|} Echo({|#2:EditorType|} value) => value;

                  public void Run()
                  {
                      _ = typeof({|#3:EditorType|});
                      _ = new {|#4:EditorType|}();
                      EditorType.{|#5:Unmarked|}();
                      _ = new EditorType.{|#6:Nested|}();
                      _ = new System.Collections.Generic.List<{|#7:EditorType|}>();
                  }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "EditorType")
            , Editor(location: 1, api: "EditorType")
            , Editor(location: 2, api: "EditorType")
            , Editor(location: 3, api: "EditorType")
            , Editor(location: 4, api: "EditorType.EditorType()")
            , Editor(location: 5, api: "EditorType.Unmarked()")
            , Editor(location: 6, api: "EditorType.Nested.Nested()")
            , Editor(location: 7, api: "EditorType")
        );

    [TestMethod]
    public Task NameOfAndInactiveEditorCode_AreExcluded()
        => VerifyAsync(
              """
              public class Consumer
              {
                  public string Run()
                  {
              #if UNITY_EDITOR
                      Api.Editor();
              #endif
                      return nameof(Api.Editor) + nameof(EditorType.Nested) + nameof(Api.Authoring);
                  }
              }
              """
            , Array.Empty<string>()
        );

    [TestMethod]
    public Task MethodNamedNameOf_IsNotExcluded()
        => VerifyAsync(
              """
              public class Consumer
              {
                  public void Run() { @nameof(Api.{|#0:Property|}); }
                  private void @nameof(int value) { }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "Api.Property")
        );

    [TestMethod]
    public Task AttributeAndImplicitConstruction_ReportOncePerUse()
        => VerifyAsync(
              """
              [EncosyTower.Core.ApiForEditor]
              public sealed class RestrictedAttribute : System.Attribute { }

              [{|#0:Restricted|}]
              public class Consumer
              {
                  public void Run() { Api value = {|#1:new|}(); }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "RestrictedAttribute.RestrictedAttribute()")
            , Editor(location: 1, api: "Api.Api()")
        );

    [TestMethod]
    public Task IndexersOperatorsAndAccessorMarkers_ReportOnlyUsedAccessors()
        => VerifyAsync(
              """
              public class Value
              {
                  public static int Property
                  {
                      get => 0;
                      [EncosyTower.Core.ApiForEditor] set { }
                  }

                  [EncosyTower.Core.ApiForEditor]
                  public int this[int index] => index;

                  [EncosyTower.Core.ApiForEditor]
                  public static Value operator +(Value left, Value right) => left;
              }

              public class Consumer
              {
                  public void Run(Value value)
                  {
                      _ = Value.Property;
                      Value.{|#0:Property|} = 1;
                      _ = value{|#1:[|}0];
                      _ = value {|#2:+|} value;
                  }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "Value.Property")
            , Editor(location: 1, api: "Value.this[int]")
            , Editor(location: 2, api: "Value.operator +(Value, Value)")
        );

    [TestMethod]
    public Task AliasAndFullyQualifiedReferences_UseSemanticMarkers()
        => VerifyAsync(
              """
              using Alias = {|#0:EditorType|};

              public class Consumer
              {
                  public void Run()
                  {
                      _ = typeof({|#1:Alias|});
                      global::EditorType.{|#2:Unmarked|}();
                  }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "EditorType")
            , Editor(location: 1, api: "EditorType")
            , Editor(location: 2, api: "EditorType.Unmarked()")
        );

    [TestMethod]
    public Task UnrelatedMarkerName_DoesNotRestrictApi()
        => VerifyAsync(
              """
              public sealed class ApiForEditorAttribute : System.Attribute { }
              public class Consumer
              {
                  [ApiForEditor] public static void Plain() { }
                  public void Run() { Plain(); }
              }
              """
            , Array.Empty<string>()
        );

    [TestMethod]
    public Task BothMarkers_ReportOneEditorError()
        => VerifyAsync(
              """
              public class Consumer
              {
                  [EncosyTower.Core.ApiForEditor, EncosyTower.Core.ApiForAuthoring]
                  public static void Both() { }
                  public void Run() { {|#0:Both|}(); }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "Consumer.Both()")
        );

    [TestMethod]
    public async Task MetadataMarkers_RestrictCrossAssemblyUses()
    {
        var references = await TestReferenceHelper.FrameworkReferences.ResolveAsync(
              LanguageNames.CSharp
            , CancellationToken.None
        );

        var dependency = CSharpCompilation.Create(
              assemblyName: "MarkedDependency"
            , syntaxTrees: new[] { CSharpSyntaxTree.ParseText(MARKERS + APIS) }
            , references: references
            , options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var stream = new MemoryStream();
        var result = dependency.Emit(stream);
        Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.AreEqual(expected: 0, actual: result.Diagnostics.Length);

        var test = CreateTest(
              "public class Consumer { public void Run() { Api.{|#0:Editor|}(); Api.Authoring(); } }"
            , Array.Empty<string>()
        );

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromImage(stream.ToArray()));
        test.ExpectedDiagnostics.Add(Editor(location: 0, api: "Api.Editor()"));
        await test.RunAsync();
    }

    [TestMethod]
    public Task MissingMarkers_ProducesNoDiagnostics()
        => CreateTest("public class Plain { public void Run() { System.Console.WriteLine(); } }", []).RunAsync();

    [TestMethod]
    public Task ReducedExtensionAndConditionalAccess_ReportBoundMembers()
        => VerifyAsync(
              """
              public static class Extensions
              {
                  [EncosyTower.Core.ApiForEditor]
                  public static void Use(this string value) { }
              }

              public class Consumer
              {
                  public void Run(string text)
                  {
                      text.{|#0:Use|}();
                      text?.{|#1:Use|}();
                  }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "Extensions.Use(string)")
            , Editor(location: 1, api: "Extensions.Use(string)")
        );

    [TestMethod]
    public Task ConstructorInitializer_ReportsMarkedConstructor()
        => VerifyAsync(
              """
              public class Consumer : Api
              {
                  public Consumer() : {|#0:base|}() { }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "Api.Api()")
        );

    [TestMethod]
    public Task MarkedQualifierOfInheritedApi_RemainsRestricted()
        => VerifyAsync(
              """
              public class Base
              {
                  [EncosyTower.Core.ApiForAuthoring]
                  public static void Use() { }
              }

              [EncosyTower.Core.ApiForEditor]
              public class Derived : Base { }

              public class Consumer
              {
                  public void Run() { {|#0:Derived|}.Use(); }
              }
              """
            , new[] { "ENCOSY_INCLUDE_AUTHORING" }
            , Editor(location: 0, api: "Derived")
        );

    [TestMethod]
    public Task GeneratedCode_IsChecked()
    {
        var test = CreateTest("", Array.Empty<string>());
        test.TestState.Sources.Add(("Markers.cs", MARKERS));
        test.TestState.Sources.Add(("Apis.cs", APIS));

        test.TestState.Sources.Add((
              "Consumer.g.cs"
            , "public class Consumer { public void Run() { Api.{|#0:Editor|}(); } }"
        ));

        test.ExpectedDiagnostics.Add(Editor(location: 0, api: "Api.Editor()"));
        return test.RunAsync();
    }

    [TestMethod]
    public Task ImplicitConversions_ReportMarkedOperator()
        => VerifyAsync(
              """
              public class Value
              {
                  [EncosyTower.Core.ApiForEditor]
                  public static implicit operator Value(int value) => new Value();
              }

              public class Consumer
              {
                  public Value Run()
                  {
                      Value value = {|#0:1|};
                      value = {|#1:2|};
                      Accept(value);
                      Accept({|#2:3|});
                      return {|#3:4|};
                  }

                  private void Accept(Value value) { }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "Value.implicit operator Value(int)")
            , Editor(location: 1, api: "Value.implicit operator Value(int)")
            , Editor(location: 2, api: "Value.implicit operator Value(int)")
            , Editor(location: 3, api: "Value.implicit operator Value(int)")
        );

    [TestMethod]
    public Task EventAccessorMarkers_RestrictOnlyTheSelectedAccessor()
        => VerifyAsync(
              """
              public class Events
              {
                  public static event System.Action Changed
                  {
                      [EncosyTower.Core.ApiForEditor] add { }
                      remove { }
                  }
              }

              public class Consumer
              {
                  public void Run(System.Action handler)
                  {
                      Events.{|#0:Changed|} += handler;
                      Events.Changed -= handler;
                  }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "Events.Changed")
        );

    [TestMethod]
    public Task InferredVariableType_DoesNotDuplicateConstructionDiagnostic()
        => VerifyAsync(
              """
              public class Consumer
              {
                  public object Run()
                  {
                      var value = new {|#0:EditorType|}();
                      return value;
                  }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "EditorType.EditorType()")
        );

    [TestMethod]
    public Task ChainedMemberUses_ReportEachApi()
        => VerifyAsync(
              """
              public class Chain
              {
                  [EncosyTower.Core.ApiForEditor]
                  public Chain Next => this;

                  [EncosyTower.Core.ApiForEditor]
                  public void Use() { }

                  public void Run() { this.{|#0:Next|}.{|#1:Use|}(); }
              }
              """
            , Array.Empty<string>()
            , Editor(location: 0, api: "Chain.Next")
            , Editor(location: 1, api: "Chain.Use()")
        );

    [TestMethod]
    public Task AuthoringOnlyApis_AreIgnored()
        => VerifyAsync(
              """
              public class Consumer : AuthoringType
              {
                  public void Run()
                  {
                      Api.Authoring();
                      var value = new AuthoringType();
                      _ = typeof(AuthoringType);
                      _ = new System.Collections.Generic.List<AuthoringType>();
                  }
              }
              """
            , Array.Empty<string>()
        );

    private static DiagnosticResult Editor(int location, string api)
        => new DiagnosticResult("SG_API_MARKER_0001", DiagnosticSeverity.Error)
            .WithLocation(location).WithMessage($"API '{api}' requires UNITY_EDITOR.");

    private static Task VerifyAsync(string source, string[] symbols, params DiagnosticResult[] expected)
        => ApiMarkerTestHelper.VerifyAsync<ApiForEditorAnalyzer>(source, symbols, expected);

    private static CSharpAnalyzerTest<ApiForEditorAnalyzer, DefaultVerifier> CreateTest(string source, string[] symbols)
        => ApiMarkerTestHelper.CreateTest<ApiForEditorAnalyzer>(source, symbols);
}
