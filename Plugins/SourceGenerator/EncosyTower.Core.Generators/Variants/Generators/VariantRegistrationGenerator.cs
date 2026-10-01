namespace EncosyTower.Core.Generators.Variants
{
    [Generator]
    public sealed class VariantRegistrationGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Variants";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string VARIANT_ATTRIBUTE = $"{NAMESPACE}.VariantAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE))
                .WithTrackingName("VariantRegistrationGenerator.Compilation");

            var candidateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  VARIANT_ATTRIBUTE
                , static (node, _) => node is StructDeclarationSyntax
                , GetSemanticMatch
            ).WithTrackingName("VariantRegistrationGenerator.Candidates")
                .Where(static x => x.IsValid)
                .WithTrackingName("VariantRegistrationGenerator.ValidSpecs");

            var collected = candidateProvider.Collect()
                .WithTrackingName("VariantRegistrationGenerator.CollectedSpecs");
            var inputs = collected.Combine(compilationProvider)
                .WithTrackingName("VariantRegistrationGenerator.Inputs");
            var combined = inputs
                .WithTrackingName("VariantRegistrationGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                sourceProductionContext.CancellationToken.ThrowIfCancellationRequested();

                if (source.Right.IsValid == false)
                {
                    return;
                }

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

            var decl = VariantStructGenerator.BuildDeclaration(structSymbol, typeArg, token);

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

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , ImmutableArray<VariantSpec> candidates
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (candidates.Length < 1)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            using var uniqueBuilder = ImmutableArrayBuilder<VariantSpec>.Rent();
            using var redundants = ImmutableArrayBuilder<VariantSpec>.Rent();
            var seenTypeNames = new HashSet<string>(StringComparer.Ordinal);

            var orderedCandidates = candidates
                .OrderBy(static candidate => candidate.fullTypeName, StringComparer.Ordinal)
                .ThenBy(static candidate => candidate.structFullName, StringComparer.Ordinal)
                .ToArray();
            context.CancellationToken.ThrowIfCancellationRequested();

            foreach (var candidate in orderedCandidates)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (seenTypeNames.Add(candidate.fullTypeName))
                {
                    uniqueBuilder.Add(candidate);
                }
                else
                {
                    redundants.Add(candidate);
                }
            }

            VariantSpecWriteCode.WriteStaticRegistrationClass(ref context, uniqueBuilder.ToImmutable(), compilation);

            foreach (var redundant in redundants.ToImmutable())
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                VariantSpecWriteCode.WriteRedundantTypeMarker(ref context, in redundant, compilation);
            }
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
