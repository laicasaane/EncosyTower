namespace EncosyTower.Core.Analyzers.ApiMarkers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class ApiForAuthoringAnalyzer : DiagnosticAnalyzer
    {
        internal const string MARKER = "EncosyTower.Core.ApiForAuthoringAttribute";

        public static readonly DiagnosticDescriptor AuthoringApi = new(
              id: "SG_API_MARKER_0002"
            , title: "Authoring API used outside an authoring build"
            , messageFormat: "API '{0}' requires UNITY_EDITOR or ENCOSY_INCLUDE_AUTHORING."
            , category: "ApiMarkerAnalyzer"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Use authoring APIs only in allowed builds or from an authoring or editor marked API."
        );

        private static readonly string[] s_allowedSymbols = { "UNITY_EDITOR", "ENCOSY_INCLUDE_AUTHORING" };

        private static readonly string[] s_exemptionMarkers = { MARKER, ApiForEditorAnalyzer.MARKER };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(AuthoringApi);

        private static void AnalyzeName(SyntaxNodeAnalysisContext context)
            => ApiMarkerAnalyzerAPI.AnalyzeName(context, MARKER, s_allowedSymbols, s_exemptionMarkers, AuthoringApi);

        private static void AnalyzeUse(SyntaxNodeAnalysisContext context)
            => ApiMarkerAnalyzerAPI.AnalyzeUse(context, MARKER, s_allowedSymbols, s_exemptionMarkers, AuthoringApi);

        private static void AnalyzeConversion(SyntaxNodeAnalysisContext context)
            => ApiMarkerAnalyzerAPI.AnalyzeConversion(
                  context
                , MARKER
                , s_allowedSymbols
                , s_exemptionMarkers
                , AuthoringApi
            );

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
