namespace EncosyTower.Entities.Generators.Entities.TypeHandles
{
    [Generator]
    internal sealed class TypeHandleGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.Entities";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string HANDLE_ATTRIBUTE_METADATA_NAME = $"{NAMESPACE}.TypeHandleAttribute";

        private const string I_BUFFER_ELEMENT_DATA = "global::Unity.Entities.IBufferElementData";
        private const string I_COMPONENT_DATA = "global::Unity.Entities.IComponentData";
        private const string I_SHARED_COMPONENT_DATA = "global::Unity.Entities.ISharedComponentData";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      HANDLE_ATTRIBUTE_METADATA_NAME
                    , static (node, _) => node is StructDeclarationSyntax syntax && syntax.TypeParameterList is null
                    , GetSemanticMatch
                )
                .WithTrackingName("TypeHandleGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("TypeHandleGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("TypeHandleGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (context, source) => {
                GenerateOutput(context, source.Right, source.Left);
            });
        }

        private static TypeHandleSpec GetSemanticMatch(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not StructDeclarationSyntax syntax
                || context.TargetSymbol is not INamedTypeSymbol structSymbol
                || syntax.TypeParameterList is not null
            )
            {
                return default;
            }

            using var typeRefsBuilder = ImmutableArrayBuilder<TypeRefSpec>.Rent();
            var typeHash = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var attribute in context.Attributes)
            {
                token.ThrowIfCancellationRequested();

                var args = attribute.ConstructorArguments;

                if (args.Length < 1 || args[0].Value is not INamedTypeSymbol type)
                {
                    continue;
                }

                if (type.IsUnboundGenericType
                    || type.IsUnmanagedType == false
                    || type.ContainsErrorType(token)
                )
                {
                    continue;
                }

                var kind = GetHandleKind(type, token);

                if (kind == TypeKind.None)
                {
                    continue;
                }

                if (typeHash.Add(type))
                {
                    typeRefsBuilder.Add(new TypeRefSpec {
                        typeName = type.ToFullName(),
                        typeIdentifier = type.ToValidIdentifier(),
                        typeShortName = type.Name,
                        isReadOnly = args.Length > 1 && args[1].Value is true,
                        kind = kind,
                    });
                }
            }

            var typeRefs = typeRefsBuilder.ToImmutable().AsEquatableArray();

            if (typeRefs.Count == 0)
            {
                return default;
            }

            var syntaxTree = syntax.SyntaxTree;
            var fileTypeName = structSymbol.ToFileName();
            var hintName = structSymbol.ToMetadataName();

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            return new TypeHandleSpec {
                structName = structSymbol.Name,
                structFullName = structSymbol.ToFullName(),
                hintName = hintName,
                openingSource = openingSource,
                closingSource = closingSource,
                typeRefs = typeRefs,
                containingTypes = TypeCreationHelpers.GetContainingTypeSpecs(syntax, token),
            };
        }

        private static TypeKind GetHandleKind(INamedTypeSymbol structSymbol, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var iface in structSymbol.AllInterfaces)
            {
                token.ThrowIfCancellationRequested();

                if (iface.HasFullName(I_BUFFER_ELEMENT_DATA))
                    return TypeKind.Buffer;

                if (iface.HasFullName(I_COMPONENT_DATA))
                    return TypeKind.Component;

                if (iface.HasFullName(I_SHARED_COMPONENT_DATA))
                    return TypeKind.SharedComponent;
            }

            return TypeKind.None;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , TypeHandleSpec candidate
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
                  "EncosyTower.Entities.Generators.Entities.TypeHandles.TypeHandleGenerator"
                , assemblyName
                , candidate.hintName
                , "EntityTypeHandle"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  candidate.openingSource
                , TypeHandleCodeWriter.WriteCode(candidate, context.CancellationToken)
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
            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__ET = global::EncosyTower.Common;");
            p.PrintLine("using g__ETETH = global::EncosyTower.Entities.TypeHandles;");
            p.PrintLine("using g__UC = global::Unity.Collections;");
            p.PrintLine("using g__UE = global::Unity.Entities;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
