namespace EncosyTower.Entities.Stats.Generators
{
    [Generator]
    internal sealed class StatCollectionGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = StatTypeInfo.NAMESPACE;
        private const string SKIP_ATTRIBUTE = StatTypeInfo.SKIP_ATTRIBUTE;
        private const string STAT_DATA = "StatData";
        private const string STAT_COLLECTION_ATTRIBUTE = $"global::{NAMESPACE}.StatCollectionAttribute";
        private const string STAT_COLLECTION_ATTRIBUTE_METADATA = $"{NAMESPACE}.StatCollectionAttribute";
        private const string STAT_SYSTEM_ATTRIBUTE = $"global::{NAMESPACE}.StatSystemAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  STAT_COLLECTION_ATTRIBUTE_METADATA
                , static (node, _) => node is StructDeclarationSyntax { TypeParameterList: null }
                , ExtractSpec
            ).WithTrackingName("StatCollectionGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("StatCollectionGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("StatCollectionGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static StatCollectionSpec ExtractSpec(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not StructDeclarationSyntax syntax)
            {
                return default;
            }

            if (context.TargetSymbol is not INamedTypeSymbol structSymbol)
            {
                return default;
            }

            var attribute = context.Attributes[0];

            if (attribute.ConstructorArguments.Length < 1)
            {
                return default;
            }


            if (attribute.ConstructorArguments[0].Value is not INamedTypeSymbol statSystemTypeSymbol
                || statSystemTypeSymbol.HasAttribute(STAT_SYSTEM_ATTRIBUTE, token) == false
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = syntax.SyntaxTree;
            var typeIdentifier = structSymbol.ToValidIdentifier();
            var hintName = structSymbol.ToMetadataName();
            var statSystemFullTypeName = statSystemTypeSymbol.ToFullName();

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var result = new StatCollectionSpec {
                typeName = structSymbol.Name,
                typeNamespace = structSymbol.ContainingNamespace.ToDisplayString(),
                typeIdentifier = typeIdentifier,
                statSystemFullTypeName = statSystemFullTypeName,
                hintName = hintName,
                openingSource = openingSource,
                closingSource = closingSource,
            };

            var args = attribute.ConstructorArguments;

            if (args.Length > 1 && args[1].Value is uint typeIdOffset)
            {
                result.typeIdOffset = typeIdOffset;
            }

            GetStatDataDefintions(syntax, token, ref result);

            if ((result.typeIdOffset + (ulong)result.statDataCollection.Count) > uint.MaxValue)
            {
                return default;
            }

            return result;

            void PrintAdditionalUsings(ref Printer p)
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
                p.PrintLine("using g__ETCol = global::EncosyTower.Collections;");
                p.PrintLine("using g__ETCon = global::EncosyTower.Conversion;");
                p.PrintLine("using g__ETDVD = global::EncosyTower.Debugging.ValidationDefines;");
                p.PrintLine("using g__ETES = global::EncosyTower.Entities.Stats;");
                p.PrintLine("using g__ETL = global::EncosyTower.Logging;");
                p.PrintLine("using g__UC = global::Unity.Collections;");
                p.PrintLine("using g__UCLU = global::Unity.Collections.LowLevel.Unsafe;");
                p.PrintLine("using g__UECS = global::Unity.Entities;");
                p.PrintLine("using g__UM = global::Unity.Mathematics;");
                p.PrintLine("using g__UE = global::UnityEngine;");
                p.PrintEndLine();
                p.PrintBeginLine("using g__StatSystem = ").Print(statSystemFullTypeName).PrintEndLine(";");
                p.PrintEndLine();
                p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
            }

            static void GetStatDataDefintions(
                  StructDeclarationSyntax parentSyntax
                , CancellationToken token
                , ref StatCollectionSpec statCollection
            )
            {
                token.ThrowIfCancellationRequested();

                using var arrayBuilder = ImmutableArrayBuilder<StatCollectionSpec.StatDataSpec>.Rent();

                foreach (var childNode in parentSyntax.ChildNodes())
                {
                    token.ThrowIfCancellationRequested();

                    var statData = GetStatDataDefinition(childNode, token);

                    if (statData.IsValid == false)
                    {
                        continue;
                    }

                    arrayBuilder.Add(statData);
                }

                statCollection.statDataCollection = arrayBuilder.ToImmutable();
            }

            static StatCollectionSpec.StatDataSpec GetStatDataDefinition(SyntaxNode node, CancellationToken token)
            {
                token.ThrowIfCancellationRequested();

                if (node is not StructDeclarationSyntax syntax
                    || syntax.TypeParameterList is not null
                    || syntax.GetAttribute(NAMESPACE, STAT_DATA, token) is not AttributeSyntax attributeSyntax
                    || attributeSyntax.ArgumentList is not AttributeArgumentListSyntax argumentList
                    || argumentList.Arguments.Count < 1
                )
                {
                    return default;
                }

                var args = argumentList.Arguments;
                var result = new StatCollectionSpec.StatDataSpec {
                    typeName = syntax.Identifier.ValueText,
                    fieldName = syntax.Identifier.ValueText.ToPublicFieldName(),
                    singleValue = false,
                };

                if (args[0].Expression is MemberAccessExpressionSyntax memberAccessExpr
                    && memberAccessExpr.Expression is IdentifierNameSyntax identifierSyntax
                    && string.Equals(identifierSyntax.Identifier.ValueText, "StatVariantType")
                )
                {
                    if (Enum.TryParse(memberAccessExpr.Name.Identifier.Text, false, out StatVariantType variantType) == false)
                    {
                        return default;

                    }

                    var index = (int)variantType;

                    if (StatTypeInfo.TryGet(index, out var typeInfo) == false)
                    {
                        return default;
                    }

                    result.valueTypeNamespace = typeInfo.namespaceName;
                    result.valueType = typeInfo.type;
                }
                else if (args[0].Expression is TypeOfExpressionSyntax typeOfExpr)
                {
                    result.valueType = typeOfExpr.Type.ToFullString();
                }
                else
                {
                    return default;
                }

                token.ThrowIfCancellationRequested();

                for (var i = 1; i < args.Count; i++)
                {
                    token.ThrowIfCancellationRequested();

                    var arg = args[i];

                    if (arg.NameEquals is not NameEqualsSyntax nameEquals
                        || arg.Expression is not LiteralExpressionSyntax literalExpr2
                    )
                    {
                        continue;
                    }

                    switch (nameEquals.Name.Identifier.ValueText)
                    {
                        case "SingleValue":
                        {
                            result.singleValue = (bool)literalExpr2.Token.Value;
                            break;
                        }
                    }
                }

                return result;
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , StatCollectionSpec candidate
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (candidate.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var assemblyName = compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Entities.Stats.Generators.StatCollectionGenerator"
                , assemblyName
                , candidate.hintName
                , "StatCollection"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  candidate.openingSource
                , candidate.WriteCode(context.CancellationToken)
                , candidate.closingSource
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }
    }
}
