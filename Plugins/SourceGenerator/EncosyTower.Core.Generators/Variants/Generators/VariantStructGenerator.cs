using EncosyTower.SourceGen.Helpers.Variants;

namespace EncosyTower.Core.Generators.Variants
{
    [Generator]
    public sealed class VariantStructGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Variants";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string VARIANT_ATTRIBUTE = $"{NAMESPACE}.VariantAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE))
                .WithTrackingName("VariantStructGenerator.Compilation");

            var candidateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  VARIANT_ATTRIBUTE
                , static (node, _) => node is StructDeclarationSyntax
                , GetSemanticMatch
            ).WithTrackingName("VariantStructGenerator.Candidates")
                .Where(static x => x.IsValid)
                .WithTrackingName("VariantStructGenerator.ValidSpecs");

            var inputs = candidateProvider.Combine(compilationProvider)
                .WithTrackingName("VariantStructGenerator.Inputs");
            var combined = inputs
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("VariantStructGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        public static VariantSpec GetSemanticMatch(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol structSymbol
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            var attributeData = context.Attributes[0];

            if (attributeData.ConstructorArguments.Length < 1
                || attributeData.ConstructorArguments[0].Value is not ITypeSymbol typeArg
            )
            {
                return default;
            }

            var decl = BuildDeclaration(structSymbol, typeArg, token);

            if (decl.IsValid)
            {
                TypeCreationHelpers.GenerateOpeningAndClosingSource(
                      context.TargetNode
                    , token
                    , out decl.openingSource
                    , out decl.closingSource
                    , printAdditionalUsings: PrintAdditionalUsings
                );
            }

            return decl;
        }

        internal static VariantSpec BuildDeclaration(
              INamedTypeSymbol structSymbol
            , ITypeSymbol typeArg
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (typeArg.ContainsErrorType(token))
            {
                return default;
            }

            var fullTypeName = typeArg.ToFullName();

            if (fullTypeName.ToUnionType().IsNativeUnionType())
            {
                return default;
            }

            var isValueType = typeArg.IsUnmanagedType;
            int? unmanagedSize = null;

            if (isValueType)
            {
                var size = 0;
                var alignment = 1;
                typeArg.GetUnmanagedSizeAndAlignment(ref size, ref alignment, token);
                unmanagedSize = size;
            }

            var structFullName = structSymbol.ToFullName();
            var converterDefault = $"{structFullName}.Converter.Default";
            var fileHintName = structSymbol.ToFileName();

            var structNs = structSymbol.ContainingNamespace;
            var namespaceName = structNs is { IsGlobalNamespace: false } ? structNs.ToDisplayString() : string.Empty;

            return new VariantSpec {
                fullTypeName = fullTypeName,
                typeName = typeArg.ToFullNameNoGlobal(),
                converterDefault = converterDefault,
                unmanagedSize = unmanagedSize,
                isValueType = isValueType,
                hasImplicitFromStructToType = isValueType || typeArg.TypeKind != TypeKind.Interface,
                structName = structSymbol.Name,
                structFullName = structFullName,
                fileHintName = fileHintName,
                namespaceName = namespaceName,
                containingTypes = structSymbol.GetContainingTypes(token),
                isValid = true,
            };
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , VariantSpec declaration
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (declaration.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            declaration.WriteVariantCode(ref context, compilation);
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using g__S = global::System;");
            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using g__ETT = global::EncosyTower.Types;");
            p.PrintLine("using g__ETV = global::EncosyTower.Variants;");
            p.PrintLine("using g__ETVC = global::EncosyTower.Variants.Converters;");
            p.PrintLine("using g__ETVSG = global::EncosyTower.Variants.SourceGen;");
            p.PrintLine("using g__UE = global::UnityEngine;");
            p.PrintLine("using g__UES = global::UnityEngine.Scripting;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
