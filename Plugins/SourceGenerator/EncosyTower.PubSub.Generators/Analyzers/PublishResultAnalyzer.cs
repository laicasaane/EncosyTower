using Microsoft.CodeAnalysis.Operations;

namespace EncosyTower.PubSub.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed partial class PublishResultAnalyzer : DiagnosticAnalyzer
    {
        private const string UNITY_TASK_T = "EncosyTower.Tasks.UnityTask`1";
        private const string SUCCESS_T = "EncosyTower.Common.Success`1";
        private const string PUBLISH_ERROR = "EncosyTower.PubSub.PublishError";
        private const string SKIP_ATTRIBUTE =
            "global::EncosyTower.PubSub.SkipSourceGeneratorsForAssemblyAttribute";

        private static readonly SymbolDisplayFormat s_ownerFormat = new(
              typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes
            , genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            var token = context.CancellationToken;
            var compilation = context.Compilation;

            if (compilation.Assembly.HasAttribute(SKIP_ATTRIBUTE, token))
            {
                return;
            }

            var unityTask = compilation.GetTypeByMetadataName(UNITY_TASK_T);
            var success = compilation.GetTypeByMetadataName(SUCCESS_T);
            var publishError = compilation.GetTypeByMetadataName(PUBLISH_ERROR);

            if (unityTask == null || success == null || publishError == null)
            {
                return;
            }

            context.RegisterOperationAction(AnalyzeStatement, OperationKind.ExpressionStatement);

            void AnalyzeStatement(OperationAnalysisContext operationContext)
            {
                AnalyzeExpressionStatement(operationContext, unityTask, success, publishError);
            }
        }

        private static void AnalyzeExpressionStatement(
              OperationAnalysisContext context
            , INamedTypeSymbol unityTask
            , INamedTypeSymbol success
            , INamedTypeSymbol publishError
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (context.Operation is not IExpressionStatementOperation statement
                || statement.Operation is not IAwaitOperation awaitOperation
                || awaitOperation.Operation is not IInvocationOperation invocation
                || IsPublishResultTask(invocation.Type, unityTask, success, publishError) == false
            )
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                  PublishResultDiscarded
                , invocation.Syntax.GetLocation()
                , GetDisplayName(invocation.TargetMethod)
            ));
        }

        private static bool IsPublishResultTask(
              ITypeSymbol type
            , INamedTypeSymbol unityTask
            , INamedTypeSymbol success
            , INamedTypeSymbol publishError
        )
        {
            if (type is not INamedTypeSymbol task
                || SymbolEqualityComparer.Default.Equals(task.OriginalDefinition, unityTask) == false
                || task.TypeArguments.Length != 1
                || task.TypeArguments[0] is not INamedTypeSymbol result
                || SymbolEqualityComparer.Default.Equals(result.OriginalDefinition, success) == false
                || result.TypeArguments.Length != 1
            )
            {
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(result.TypeArguments[0], publishError);
        }

        private static string GetDisplayName(IMethodSymbol method)
        {
            var definition = method.OriginalDefinition;
            var owner = definition.ContainingType.ToDisplayString(s_ownerFormat);

            return owner + "." + definition.Name;
        }
    }
}
