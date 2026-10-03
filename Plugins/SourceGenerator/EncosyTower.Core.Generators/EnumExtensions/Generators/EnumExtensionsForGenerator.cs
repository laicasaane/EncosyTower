namespace EncosyTower.Core.Generators.EnumExtensions
{
    [Generator]
    public sealed class EnumExtensionsForGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.EnumExtensions";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string ENUM_EXTENSIONS_FOR_ATTRIBUTE = $"global::{NAMESPACE}.EnumExtensionsForAttribute";
        private const string ENUM_EXTENSIONS_FOR_ATTRIBUTE_METADATA = $"{NAMESPACE}.EnumExtensionsForAttribute";
        public const string FLAGS_ATTRIBUTE = "global::System.FlagsAttribute";
        public const string GENERATOR_NAME = nameof(EnumExtensionsGenerator);

        private const string GENERATED_CODE = "[g__SCDC.GeneratedCode("
            + "\"EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsForGenerator\", "
            + $"\"{SourceGenVersion.VALUE}\")]";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => EnumExtensionCompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ENUM_EXTENSIONS_FOR_ATTRIBUTE_METADATA
                    , static (node, _) => node is ClassDeclarationSyntax cls
                        && cls.HasModifier(SyntaxKind.StaticKeyword)
                    , ExtractCandidate
                )
                .WithTrackingName("EnumExtensionsForGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("EnumExtensionsForGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.Compilation.IsValid)
                .WithTrackingName("EnumExtensionsForGenerator.Outputs");

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

            if (context.TargetSymbol is not INamedTypeSymbol classSymbol
                || classSymbol.IsStatic == false
            )
            {
                return default;
            }

            if (context.Attributes.Length < 1)
            {
                return default;
            }

            var attribData = context.Attributes[0];

            if (attribData.ConstructorArguments.Length < 1)
            {
                return default;
            }


            if (attribData.ConstructorArguments[0].Value is not INamedTypeSymbol enumSymbol
                || enumSymbol.TypeKind != TypeKind.Enum
                || enumSymbol.ContainsErrorType(token)
            )
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

            var containingTypes = classSymbol.GetContainingTypes(token);
            var ns = classSymbol.ContainingNamespace;
            var namespaceName = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : string.Empty;
            var candidate = EnumExtensionSpec.Extract(
                  enumSymbol
                , syntax.Parent is BaseNamespaceDeclarationSyntax or CompilationUnitSyntax
                , classSymbol.Name
                , classSymbol.DeclaredAccessibility
                , namespaceName
                , containingTypes
                , token
            );

            candidate.openingSource = openingSource;
            candidate.closingSource = closingSource;
            candidate.fileHintName = classSymbol.ToMetadataName();

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

            var declaration = new EnumExtensionsDeclaration(candidate, compilation.UnityCollections) {
                GeneratedCode = GENERATED_CODE,
                InterfaceGeneratedCode = GENERATED_CODE,
            };
            var assemblyName = compilation.Compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsForGenerator"
                , assemblyName
                , candidate.fileHintName
                , "EnumExtensionsFor"
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
