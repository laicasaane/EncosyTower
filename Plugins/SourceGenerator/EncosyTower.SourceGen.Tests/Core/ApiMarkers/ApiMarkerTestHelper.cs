using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.ApiMarkers;

internal static class ApiMarkerTestHelper
{
    internal const string MARKERS = """
        namespace EncosyTower.Core
        {
            [System.AttributeUsage(System.AttributeTargets.All)]
            public sealed class ApiForEditorAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.All)]
            public sealed class ApiForAuthoringAttribute : System.Attribute { }
        }
        """;

    internal const string APIS = """
        public class Api
        {
            [EncosyTower.Core.ApiForEditor]
            public static void Editor() { }

            [EncosyTower.Core.ApiForAuthoring]
            public static void Authoring() { }

            [EncosyTower.Core.ApiForEditor]
            public static int Field;

            [EncosyTower.Core.ApiForEditor]
            public static int Property { get; set; }

            [EncosyTower.Core.ApiForEditor]
            public static event System.Action Changed { add { } remove { } }

            [EncosyTower.Core.ApiForEditor]
            public Api() { }
        }

        [EncosyTower.Core.ApiForEditor]
        public class EditorType
        {
            public static void Unmarked() { }
            public class Nested { }
        }

        [EncosyTower.Core.ApiForAuthoring]
        public class AuthoringType
        {
            public static void Unmarked() { }
            public class Nested { }
        }
        """;

    internal static Task VerifyAsync<TAnalyzer>(string source, string[] symbols, params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = CreateTest<TAnalyzer>(source, symbols);
        test.TestState.Sources.Add(("Markers.cs", MARKERS));
        test.TestState.Sources.Add(("Apis.cs", APIS));
        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }

    internal static CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> CreateTest<TAnalyzer>(
          string source
        , string[] symbols
    )
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> {
            TestCode = source,
            CompilerDiagnostics = CompilerDiagnostics.All,
            ReferenceAssemblies = TestReferenceHelper.FrameworkReferences,
        };

        test.SolutionTransforms.Add(Configure);
        return test;

        Solution Configure(Solution solution, ProjectId projectId)
            => solution.WithProjectParseOptions(
                  projectId
                , new CSharpParseOptions(LanguageVersion.CSharp10).WithPreprocessorSymbols(symbols)
            );
    }
}
