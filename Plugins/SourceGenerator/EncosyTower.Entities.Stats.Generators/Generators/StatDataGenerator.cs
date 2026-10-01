namespace EncosyTower.Entities.Stats.Generators
{
    [Generator]
    internal sealed class StatDataGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = StatTypeInfo.NAMESPACE;
        private const string SKIP_ATTRIBUTE = StatTypeInfo.SKIP_ATTRIBUTE;
        private const string STAT_DATA_ATTRIBUTE = $"global::{NAMESPACE}.StatDataAttribute";
        private const string STAT_DATA_ATTRIBUTE_METADATA_NAME = $"{NAMESPACE}.StatDataAttribute";
        private const string GENERATOR_NAME = nameof(StatDataGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      STAT_DATA_ATTRIBUTE_METADATA_NAME
                    , static (node, _) => node is StructDeclarationSyntax syntax && syntax.TypeParameterList is null
                    , ExtractSpec
                )
                .WithTrackingName("StatDataGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("StatDataGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("StatDataGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static StatDataSpec ExtractSpec(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not StructDeclarationSyntax syntax || syntax.TypeParameterList is not null)
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

            var semanticModel = context.SemanticModel;
            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = syntax.SyntaxTree;
            var typeIdentifier = structSymbol.ToValidIdentifier();
            var hintName = structSymbol.ToMetadataName();

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var result = new StatDataSpec {
                typeName = structSymbol.Name,
                typeNamespace = structSymbol.ContainingNamespace.ToDisplayString(),
                typeIdentifier = typeIdentifier,
                hintName = hintName,
                openingSource = openingSource,
                closingSource = closingSource,
                singleValue = false,
            };

            var firstArg = attribute.ConstructorArguments[0];

            if (firstArg.Kind == TypedConstantKind.Enum && firstArg.Value is byte enumByte)
            {
                var index = (int)enumByte;

                if (StatTypeInfo.TryGet(index, out var typeInfo) == false)
                {
                    return default;
                }

                result.size = typeInfo.size;
                result.valueTypeNs = typeInfo.namespaceName;
                result.valueType = typeInfo.type;
                result.valueTypeName = typeInfo.typeName;
            }
            else if (firstArg.Kind == TypedConstantKind.Type && firstArg.Value is INamedTypeSymbol enumType)
            {
                if (enumType.TypeKind != TypeKind.Enum)
                {
                    return default;
                }

                var underlyingType = enumType.EnumUnderlyingType.ToFullNameNoGlobal();

                if (StatTypeInfo.TryGetEnumTypeName(underlyingType, out var valueTypeName) == false)
                {
                    return default;
                }

                result.isEnum = true;
                result.valueTypeName = valueTypeName;
                result.valueType = enumType.ToFullName();
                result.underlyingTypeName = underlyingType;
                enumType.EnumUnderlyingType.GetUnmanagedSize(ref result.size, token);
            }
            else
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            foreach (var namedArg in attribute.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

                if (string.Equals(namedArg.Key, "SingleValue", StringComparison.Ordinal)
                    && namedArg.Value.Value is bool singleValue
                )
                {
                    result.singleValue = singleValue;
                }
            }

            return result;

            static void PrintAdditionalUsings(ref Printer p)
            {
                p.PrintEndLine();
                p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
                p.PrintLine("using g__S = global::System;");
                p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
                p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
                p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
                p.PrintLine("using g__SRIS = global::System.Runtime.InteropServices;");
                p.PrintLine("using g__ETES = global::EncosyTower.Entities.Stats;");
                p.PrintLine("using g__ETL = global::EncosyTower.Logging;");
                p.PrintLine("using g__UM = global::Unity.Mathematics;");
                p.PrintEndLine();
                p.PrintLine("using g__UnityDebug = global::UnityEngine.Debug;");
                p.PrintEndLine();
                p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , StatDataSpec candidate
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
                  "EncosyTower.Entities.Stats.Generators.StatDataGenerator"
                , assemblyName
                , candidate.hintName
                , "StatData"
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
