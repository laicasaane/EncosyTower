namespace EncosyTower.Core.Analyzers.ApiMarkers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class ApiForEditorAnalyzer : DiagnosticAnalyzer
    {
        internal const string MARKER = "EncosyTower.Core.ApiForEditorAttribute";

        public static readonly DiagnosticDescriptor EditorApi = new(
              id: "SG_API_MARKER_0001"
            , title: "Editor API used outside an editor build"
            , messageFormat: "API '{0}' requires UNITY_EDITOR."
            , category: "ApiMarkerAnalyzer"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Use editor APIs only in editor builds or from an [ApiForEditor] member or containing type."
        );

        private static readonly string[] s_allowedSymbols = { "UNITY_EDITOR" };
        private static readonly string[] s_exemptionMarkers = { MARKER };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(EditorApi);

        private static void AnalyzeName(SyntaxNodeAnalysisContext context)
            => ApiMarkerAnalyzerAPI.AnalyzeName(context, MARKER, s_allowedSymbols, s_exemptionMarkers, EditorApi);

        private static void AnalyzeUse(SyntaxNodeAnalysisContext context)
            => ApiMarkerAnalyzerAPI.AnalyzeUse(context, MARKER, s_allowedSymbols, s_exemptionMarkers, EditorApi);

        private static void AnalyzeConversion(SyntaxNodeAnalysisContext context)
            => ApiMarkerAnalyzerAPI.AnalyzeConversion(context, MARKER, s_allowedSymbols, s_exemptionMarkers, EditorApi);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                  GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
            );

            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeName, ApiMarkerAnalyzerAPI.NameKinds);
            context.RegisterSyntaxNodeAction(AnalyzeUse, ApiMarkerAnalyzerAPI.UseKinds);
            context.RegisterSyntaxNodeAction(AnalyzeConversion, ApiMarkerAnalyzerAPI.ConversionKinds);
        }
    }
}
