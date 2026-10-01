namespace EncosyTower.Core.Generators.EnumExtensions
{
    [Generator]
    public sealed class EnumExtensionsGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.EnumExtensions";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string ENUM_EXTENSIONS_ATTRIBUTE = $"global::{NAMESPACE}.EnumExtensionsAttribute";
        private const string ENUM_EXTENSIONS_ATTRIBUTE_METADATA = $"{NAMESPACE}.EnumExtensionsAttribute";
        public const string FLAGS_ATTRIBUTE = "global::System.FlagsAttribute";
        public const string GENERATOR_NAME = nameof(EnumExtensionsGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => EnumExtensionCompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ENUM_EXTENSIONS_ATTRIBUTE_METADATA
                    , static (node, _) => node is EnumDeclarationSyntax
                    , ExtractCandidate
                )
                .WithTrackingName("EnumExtensionsGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("EnumExtensionsGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.Compilation.IsValid)
                .WithTrackingName("EnumExtensionsGenerator.Outputs");

            context.RegisterSourceOutput(combined, (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static EnumExtensionSpec ExtractCandidate(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol enumSymbol)
            {
                return default;
            }

            var syntax = context.TargetNode;

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var containingTypes = enumSymbol.GetContainingTypes(token);
            var ns = enumSymbol.ContainingNamespace;
            var namespaceName = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : string.Empty;

            var candidate = EnumExtensionSpec.Extract(
                  enumSymbol
                , syntax.Parent is BaseNamespaceDeclarationSyntax
                , EnumExtensionsDeclaration.GetNameExtensionsClass(enumSymbol.Name)
                , enumSymbol.DeclaredAccessibility
                , namespaceName
                , containingTypes
                , token
            );

            candidate.openingSource = openingSource;
            candidate.closingSource = closingSource;

            return candidate;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , EnumExtensionCompilationSpec compilation
            , EnumExtensionSpec candidate
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (candidate.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var declaration = new EnumExtensionsDeclaration(candidate, compilation.UnityCollections);
            var assemblyName = compilation.Compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator"
                , assemblyName
                , candidate.fullyQualifiedName
                , "EnumExtensions"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  candidate.openingSource
                , declaration.WriteCode(context.CancellationToken)
                , candidate.closingSource
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using g__S = global::System;");
            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SC = global::System.Collections;");
            p.PrintLine("using g__SCG = global::System.Collections.Generic;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using g__ETCol = global::EncosyTower.Collections;");
            p.PrintLine("using g__ETCon = global::EncosyTower.Conversion;");
            p.PrintLine("using g__ETEE = global::EncosyTower.EnumExtensions;");
            p.PrintLine("using g__ETEESG = global::EncosyTower.EnumExtensions.SourceGen;");
            p.PrintLine("using g__UC = global::Unity.Collections;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
