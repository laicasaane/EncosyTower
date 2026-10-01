namespace EncosyTower.Entities.Stats.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class StatSystemDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.Entities.Stats";
        private const string STAT_SYSTEM_ATTRIBUTE = $"global::{NAMESPACE}.StatSystemAttribute";

        public static readonly DiagnosticDescriptor MustNotBeGeneric = new(
              id: "SG_STAT_SYSTEM_0001"
            , title: "[StatSystem] cannot be applied to a generic type"
            , messageFormat: "\"{0}\" is a generic type. [StatSystem] can only be applied to non-generic types."
            , category: "StatSystemGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[StatSystem] can only be applied to non-generic types."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(MustNotBeGeneric);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol typeSymbol
                || typeSymbol.HasAttribute(STAT_SYSTEM_ATTRIBUTE, token) == false
            )
            {
                return;
            }

            if (typeSymbol.IsGenericType)
            {
                if (typeSymbol.TryGetSourceLocation(out var location) == false)
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(MustNotBeGeneric, location, typeSymbol.Name));
            }
        }
    }
}
