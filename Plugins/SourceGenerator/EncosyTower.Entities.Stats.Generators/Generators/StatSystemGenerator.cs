namespace EncosyTower.Entities.Stats.Generators
{
    [Generator]
    internal sealed class StatSystemGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = StatTypeInfo.NAMESPACE;
        private const string SKIP_ATTRIBUTE = StatTypeInfo.SKIP_ATTRIBUTE;
        private const string STAT_SYSTEM_ATTRIBUTE = $"global::{NAMESPACE}.StatSystemAttribute";
        private const string STAT_SYSTEM_ATTRIBUTE_METADATA_NAME = $"{NAMESPACE}.StatSystemAttribute";
        private const string GENERATOR_NAME = nameof(StatSystemGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => StatSystemCompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      STAT_SYSTEM_ATTRIBUTE_METADATA_NAME
                    , static (node, _) => node is TypeDeclarationSyntax syntax && syntax.TypeParameterList is null
                    , ExtractSpec
                )
                .WithTrackingName("StatSystemGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("StatSystemGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.Compilation.IsValid)
                .WithTrackingName("StatSystemGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static StatSystemSpec ExtractSpec(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not TypeDeclarationSyntax syntax || syntax.TypeParameterList is not null)
            {
                return default;
            }

            if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            {
                return default;
            }

            var attribute = context.Attributes[0];

            if (attribute.ConstructorArguments.Length < 1)
            {
                return default;
            }

            var args = attribute.ConstructorArguments;

            if (args[0].Value is not byte maxDataSize)
            {
                return default;
            }

            int maxUserDataSize;

            if (args.Length > 1 && args[1].Value is byte userDataSize)
            {
                if (userDataSize > 2)
                {
                    maxUserDataSize = 4;
                }
                else if (userDataSize > 1)
                {
                    maxUserDataSize = 2;
                }
                else
                {
                    maxUserDataSize = 1;
                }
            }
            else
            {
                maxUserDataSize = 1;
            }

            var syntaxTree = syntax.SyntaxTree;
            var typeIdentifier = typeSymbol.ToValidIdentifier();
            var fileTypeName = typeSymbol.ToFileName();
            var hintName = typeSymbol.ToMetadataName();

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            return new StatSystemSpec {
                typeName = typeSymbol.Name,
                typeNamespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                syntaxKeyword = syntax.Keyword.ValueText,
                typeIdentifier = typeIdentifier,
                hintName = hintName,
                openingSource = openingSource,
                closingSource = closingSource,
                maxDataSize = Math.Max((int)maxDataSize, 1),
                maxUserDataSize = maxUserDataSize,
                isStatic = typeSymbol.IsStatic,
            };

            static void PrintAdditionalUsings(ref Printer p)
            {
                p.PrintEndLine();
                p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
                p.PrintLine("using g__S = global::System;");
                p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
                p.PrintLine("using g__SD = global::System.Diagnostics;");
                p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
                p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
                p.PrintLine("using g__SRIS = global::System.Runtime.InteropServices;");
                p.PrintLine("using g__ET = global::EncosyTower.Common;");
                p.PrintLine("using g__ETDVD = global::EncosyTower.Debugging.ValidationDefines;");
                p.PrintLine("using g__ETES = global::EncosyTower.Entities.Stats;");
                p.PrintLine("using g__UE = UnityEngine;");
                p.PrintLine("using g__UB = Unity.Burst;");
                p.PrintLine("using g__UC = global::Unity.Collections;");
                p.PrintLine("using g__UCLU = global::Unity.Collections.LowLevel.Unsafe;");
                p.PrintLine("using g__UECS = global::Unity.Entities;");
                p.PrintLine("using g__UM = global::Unity.Mathematics;");
                p.PrintLine("using g__UJ = Unity.Jobs;");
                p.PrintEndLine();
                p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , StatSystemCompilationSpec compilation
            , StatSystemSpec candidate
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (candidate.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var assemblyName = compilation.Compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Entities.Stats.Generators.StatSystemGenerator"
                , assemblyName
                , candidate.hintName
                , "StatSystem"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  candidate.openingSource
                , candidate.WriteCode(compilation.LatiosCore, context.CancellationToken)
                , candidate.closingSource
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }
    }
}
