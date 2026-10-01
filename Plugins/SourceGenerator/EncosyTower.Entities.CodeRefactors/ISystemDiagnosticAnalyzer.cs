using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.Entities.CodeRefactors
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class ISystemDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ISYSTEM = "SG_ISYSTEM_0001";

        public const string ATTRIBUTE_METADATA = "EncosyTower.Entities.ISystemAttribute";

        private const string ISYSTEM_FQN = "global::Unity.Entities.ISystem";
        private const string SYSTEMSTATE_FQN = "global::Unity.Entities.SystemState";

        private static readonly DiagnosticDescriptor s_diagnostic = new(
              id: DIAGNOSTIC_ISYSTEM
            , title: "ISystem type can be completed"
            , messageFormat: "Type '{0}' is annotated with [ISystem] and may need ISystem members"
            , category: nameof(ISystemDiagnosticAnalyzer)
            , DiagnosticSeverity.Hidden
            , isEnabledByDefault: true
            , description: "Type annotated with [ISystem] can be completed with lifecycle members."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(s_diagnostic);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol typeSymbol)
            {
                return;
            }

            if (typeSymbol.TypeKind is not TypeKind.Class and not TypeKind.Struct)
            {
                return;
            }

            if (typeSymbol.HasAttribute(ATTRIBUTE_METADATA, token) == false)
            {
                return;
            }

            if (HasCompleteContract(typeSymbol, token))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(s_diagnostic, typeSymbol.Locations[0], typeSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }

        private static bool HasCompleteContract(INamedTypeSymbol typeSymbol, CancellationToken token)
        {
            var hasPartial = false;

            foreach (var syntaxReference in typeSymbol.DeclaringSyntaxReferences)
            {
                token.ThrowIfCancellationRequested();

                if (syntaxReference.GetSyntax(token) is TypeDeclarationSyntax typeDeclaration
                    && typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                )
                {
                    hasPartial = true;
                    break;
                }
            }

            return hasPartial
                && typeSymbol.ImplementsInterface(ISYSTEM_FQN, true)
                && HasCompatibleLifecycleMember(typeSymbol, "OnCreate", token)
                && HasCompatibleLifecycleMember(typeSymbol, "OnUpdate", token)
                && HasCompatibleLifecycleMember(typeSymbol, "OnDestroy", token);
        }

        private static bool HasCompatibleLifecycleMember(
              INamedTypeSymbol typeSymbol
            , string methodName
            , CancellationToken token
        )
        {
            foreach (var member in typeSymbol.GetMembers(methodName))
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method
                    || method.ReturnsVoid == false
                    || method.Parameters.Length != 1
                )
                {
                    continue;
                }

                var parameter = method.Parameters[0];

                if (parameter.RefKind == RefKind.Ref
                    && parameter.Type.IsType(SYSTEMSTATE_FQN)
                )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
