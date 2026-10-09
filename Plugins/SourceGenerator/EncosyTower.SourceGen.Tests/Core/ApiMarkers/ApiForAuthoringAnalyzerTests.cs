using EncosyTower.Core.Analyzers.ApiMarkers;
using Microsoft.CodeAnalysis.CSharp;
using static EncosyTower.SourceGen.Tests.Core.ApiMarkers.ApiMarkerTestHelper;

namespace EncosyTower.SourceGen.Tests.Core.ApiMarkers;

[TestClass]
public sealed class ApiForAuthoringAnalyzerTests
{
    [TestMethod]
    public Task Player_ReportsEachDirectMemberUse()
        => VerifyAsync(
              """
              public class Consumer
              {
                  public void Run()
                  {
                      Api.Editor();
                      Api.{|#0:Authoring|}();
                      Api.Field = Api.Property;
                      Api.Changed += Api.Editor;
                      _ = new Api();
                  }
              }
              """
            , Array.Empty<string>()
            , Authoring(location: 0, api: "Api.Authoring()")
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
    public Task AuthoringBuild_AllowsAuthoringUses()
        => VerifyAsync(
              """
              public class Consumer
              {
                  public void Run()
                  {
                      Api.Authoring();
                      _ = new AuthoringType();
                      Api.Editor();
                  }
              }
              """
            , new[] { "ENCOSY_INCLUDE_AUTHORING" }
        );

    [TestMethod]
    public Task MarkedMembers_AllowEditorAndAuthoringExemptions()
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
                      Api.Editor();
                  }

                  [EncosyTower.Core.ApiForEditor]
                  public EditorType Property => new EditorType();
              }
              """
            , Array.Empty<string>()
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
                      public void Run() { Api.Authoring(); Api.Editor(); }
                  }
              }
              """
            , Array.Empty<string>()
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
              "public class Consumer { public void Run() { Api.Editor(); Api.{|#0:Authoring|}(); } }"
            , Array.Empty<string>()
        );

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromImage(stream.ToArray()));
        test.ExpectedDiagnostics.Add(Authoring(location: 0, api: "Api.Authoring()"));
        await test.RunAsync();
    }

    [TestMethod]
    public Task MissingMarkers_ProducesNoDiagnostics()
        => CreateTest("public class Plain { public void Run() { System.Console.WriteLine(); } }", []).RunAsync();

    [TestMethod]
    public Task BothMarkers_ReportOneAuthoringError()
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
            , Authoring(location: 0, api: "Consumer.Both()")
        );

    [TestMethod]
    public Task UnrelatedMarkerName_DoesNotRestrictApi()
        => VerifyAsync(
              """
              public sealed class ApiForAuthoringAttribute : System.Attribute { }
              public class Consumer
              {
                  [ApiForAuthoring] public static void Plain() { }
                  public void Run() { Plain(); }
              }
              """
            , Array.Empty<string>()
        );

    [TestMethod]
    public Task MarkedTypes_ReportReferencesConstructionInheritanceAndGenericArguments()
        => VerifyAsync(
              """
              public class Consumer : {|#0:AuthoringType|}
              {
                  public {|#1:AuthoringType|} Echo({|#2:AuthoringType|} value) => value;

                  public void Run()
                  {
                      _ = typeof({|#3:AuthoringType|});
                      _ = new {|#4:AuthoringType|}();
                      AuthoringType.{|#5:Unmarked|}();
                      _ = new AuthoringType.{|#6:Nested|}();
                      _ = new System.Collections.Generic.List<{|#7:AuthoringType|}>();
                  }
              }
              """
            , Array.Empty<string>()
            , Authoring(location: 0, api: "AuthoringType")
            , Authoring(location: 1, api: "AuthoringType")
            , Authoring(location: 2, api: "AuthoringType")
            , Authoring(location: 3, api: "AuthoringType")
            , Authoring(location: 4, api: "AuthoringType.AuthoringType()")
            , Authoring(location: 5, api: "AuthoringType.Unmarked()")
            , Authoring(location: 6, api: "AuthoringType.Nested.Nested()")
            , Authoring(location: 7, api: "AuthoringType")
        );

    [TestMethod]
    public Task EventAccessorMarkers_RestrictOnlyTheSelectedAccessor()
        => VerifyAsync(
              """
              public class Events
              {
                  public static event System.Action Changed
                  {
                      [EncosyTower.Core.ApiForAuthoring] add { }
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
            , Authoring(location: 0, api: "Events.Changed")
        );

    [TestMethod]
    public Task InferredVariableType_DoesNotDuplicateConstructionDiagnostic()
        => VerifyAsync(
              """
              public class Consumer
              {
                  public object Run()
                  {
                      var value = new {|#0:AuthoringType|}();
                      return value;
                  }
              }
              """
            , Array.Empty<string>()
            , Authoring(location: 0, api: "AuthoringType.AuthoringType()")
        );

    [TestMethod]
    public Task EditorOnlyApis_AreIgnored()
        => VerifyAsync(
              """
              public class Consumer : EditorType
              {
                  public void Run()
                  {
                      Api.Editor();
                      Api.Field = Api.Property;
                      Api.Changed += Api.Editor;
                      _ = new Api();
                      var value = new EditorType();
                      _ = typeof(EditorType.Nested);
                      _ = new System.Collections.Generic.List<EditorType>();
                  }
              }
              """
            , Array.Empty<string>()
        );

    private static DiagnosticResult Authoring(int location, string api)
        => new DiagnosticResult("SG_API_MARKER_0002", DiagnosticSeverity.Error)
            .WithLocation(location).WithMessage($"API '{api}' requires UNITY_EDITOR or ENCOSY_INCLUDE_AUTHORING.");

    private static Task VerifyAsync(string source, string[] symbols, params DiagnosticResult[] expected)
        => ApiMarkerTestHelper.VerifyAsync<ApiForAuthoringAnalyzer>(source, symbols, expected);

    private static CSharpAnalyzerTest<ApiForAuthoringAnalyzer, DefaultVerifier> CreateTest(
          string source
        , string[] symbols
    )
        => ApiMarkerTestHelper.CreateTest<ApiForAuthoringAnalyzer>(source, symbols);
}
