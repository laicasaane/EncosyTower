namespace EncosyTower.Entities.Generators.Entities.Lookups
{
    [Generator]
    internal sealed class LookupGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.Entities";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string LOOKUP_ATTRIBUTE_METADATA_NAME = $"{NAMESPACE}.LookupAttribute";
        private const string GENERATOR_NAME = nameof(LookupGenerator);
        private const string I_BUFFER_LOOKUPS = $"{NAMESPACE}.IBufferLookups";
        private const string I_COMPONENT_LOOKUPS = $"{NAMESPACE}.IComponentLookups";
        private const string I_ENABLEABLE_BUFFER_LOOKUPS = $"{NAMESPACE}.IEnableableBufferLookups";
        private const string I_ENABLEABLE_COMPONENT_LOOKUPS = $"{NAMESPACE}.IEnableableComponentLookups";
        private const string I_PHYSICS_BUFFER_LOOKUPS = $"{NAMESPACE}.IPhysicsBufferLookups";
        private const string I_PHYSICS_COMPONENT_LOOKUPS = $"{NAMESPACE}.IPhysicsComponentLookups";
        private const string I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUPS = $"{NAMESPACE}.IPhysicsEnableableComponentLookups";

        private const string I_BUFFER_LOOKUP_RO = "g__ETEL.IBufferLookupRO";
        private const string I_BUFFER_LOOKUP_RW = "g__ETEL.IBufferLookupRW";
        private const string I_COMPONENT_LOOKUP_RO = "g__ETEL.IComponentLookupRO";
        private const string I_COMPONENT_LOOKUP_RW = "g__ETEL.IComponentLookupRW";
        private const string I_ENABLEABLE_BUFFER_LOOKUP_RO = "g__ETEL.IEnableableBufferLookupRO";
        private const string I_ENABLEABLE_BUFFER_LOOKUP_RW = "g__ETEL.IEnableableBufferLookupRW";
        private const string I_ENABLEABLE_COMPONENT_LOOKUP_RO = "g__ETEL.IEnableableComponentLookupRO";
        private const string I_ENABLEABLE_COMPONENT_LOOKUP_RW = "g__ETEL.IEnableableComponentLookupRW";
        private const string I_PHYSICS_BUFFER_LOOKUP_RO = "g__ETEL.IPhysicsBufferLookupRO";
        private const string I_PHYSICS_BUFFER_LOOKUP_RW = "g__ETEL.IPhysicsBufferLookupRW";
        private const string I_PHYSICS_COMPONENT_LOOKUP_RO = "g__ETEL.IPhysicsComponentLookupRO";
        private const string I_PHYSICS_COMPONENT_LOOKUP_RW = "g__ETEL.IPhysicsComponentLookupRW";
        private const string I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUP_RO = "g__ETEL.IPhysicsEnableableComponentLookupRO";
        private const string I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUP_RW = "g__ETEL.IPhysicsEnableableComponentLookupRW";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      LOOKUP_ATTRIBUTE_METADATA_NAME
                    , static (node, _) => node is StructDeclarationSyntax syntax && syntax.TypeParameterList is null
                    , GetSemanticMatch
                )
                .WithTrackingName("LookupGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("LookupGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("LookupGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (context, source) => {
                GenerateOutput(context, source.Right, source.Left);
            });
        }

        private static LookupSpec GetSemanticMatch(GeneratorAttributeSyntaxContext context, CancellationToken token)
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

            var kind = GetLookupKind(structSymbol, token);

            if (kind == LookupKind.None)
            {
                return default;
            }

            GetLookupInterfaces(kind, out var interfaceLookupRO, out var interfaceLookupRW);

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

                var isReadOnly = args.Length > 1 && args[1].Value is true;

                if (typeHash.Add(type))
                {
                    typeRefsBuilder.Add(new TypeRefSpec {
                        typeName = type.ToFullName(),
                        typeIdentifier = type.ToValidIdentifier(),
                        typeShortName = type.Name,
                        isReadOnly = isReadOnly,
                    });
                }
            }

            var typeRefs = typeRefsBuilder.ToImmutable().AsEquatableArray();

            if (typeRefs.Count == 0)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = syntax.SyntaxTree;
            var hintName = structSymbol.ToMetadataName();

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: kind >= LookupKind.PhysicsBuffer
                    ? PrintAdditionalUsingsWithLatios
                    : PrintAdditionalUsings
            );

            return new LookupSpec {
                structName = structSymbol.Name,
                structFullName = structSymbol.ToFullName(),
                hintName = hintName,
                openingSource = openingSource,
                closingSource = closingSource,
                interfaceLookupRO = interfaceLookupRO,
                interfaceLookupRW = interfaceLookupRW,
                kind = kind,
                typeRefs = typeRefs,
                containingTypes = TypeCreationHelpers.GetContainingTypeSpecs(syntax, token),
            };
        }

        private static LookupKind GetLookupKind(INamedTypeSymbol structSymbol, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var iface in structSymbol.Interfaces)
            {
                token.ThrowIfCancellationRequested();

                if (iface.HasFullName(I_BUFFER_LOOKUPS, token))
                    return LookupKind.Buffer;

                if (iface.HasFullName(I_COMPONENT_LOOKUPS, token))
                    return LookupKind.Component;

                if (iface.HasFullName(I_ENABLEABLE_BUFFER_LOOKUPS, token))
                    return LookupKind.EnableableBuffer;

                if (iface.HasFullName(I_ENABLEABLE_COMPONENT_LOOKUPS, token))
                    return LookupKind.EnableableComponent;

                if (iface.HasFullName(I_PHYSICS_BUFFER_LOOKUPS, token))
                    return LookupKind.PhysicsBuffer;

                if (iface.HasFullName(I_PHYSICS_COMPONENT_LOOKUPS, token))
                    return LookupKind.PhysicsComponent;

                if (iface.HasFullName(I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUPS, token))
                    return LookupKind.PhysicsEnableableComponent;
            }

            return LookupKind.None;
        }

        private static void GetLookupInterfaces(LookupKind kind, out string ro, out string rw)
        {
            switch (kind)
            {
                case LookupKind.Buffer:
                    ro = I_BUFFER_LOOKUP_RO;
                    rw = I_BUFFER_LOOKUP_RW;
                    return;

                case LookupKind.Component:
                    ro = I_COMPONENT_LOOKUP_RO;
                    rw = I_COMPONENT_LOOKUP_RW;
                    return;

                case LookupKind.EnableableBuffer:
                    ro = I_ENABLEABLE_BUFFER_LOOKUP_RO;
                    rw = I_ENABLEABLE_BUFFER_LOOKUP_RW;
                    return;

                case LookupKind.EnableableComponent:
                    ro = I_ENABLEABLE_COMPONENT_LOOKUP_RO;
                    rw = I_ENABLEABLE_COMPONENT_LOOKUP_RW;
                    return;

                case LookupKind.PhysicsBuffer:
                    ro = I_PHYSICS_BUFFER_LOOKUP_RO;
                    rw = I_PHYSICS_BUFFER_LOOKUP_RW;
                    return;

                case LookupKind.PhysicsComponent:
                    ro = I_PHYSICS_COMPONENT_LOOKUP_RO;
                    rw = I_PHYSICS_COMPONENT_LOOKUP_RW;
                    return;

                case LookupKind.PhysicsEnableableComponent:
                    ro = I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUP_RO;
                    rw = I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUP_RW;
                    return;

                default:
                    ro = string.Empty;
                    rw = string.Empty;
                    return;
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , LookupSpec candidate
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
                  "EncosyTower.Entities.Generators.Entities.Lookups.LookupGenerator"
                , assemblyName
                , candidate.hintName
                , "EntityLookup"
                , string.Empty
            );

            var writer = LookupCodeWriter.GetWriter(candidate.kind);

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  candidate.openingSource
                , writer.Write(candidate, context.CancellationToken)
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
            p.PrintLine("using g__ETEL = global::EncosyTower.Entities.Lookups;");
            p.PrintLine("using g__UC = global::Unity.Collections;");
            p.PrintLine("using g__UE = global::Unity.Entities;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        private static void PrintAdditionalUsingsWithLatios(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__ET = global::EncosyTower.Common;");
            p.PrintLine("using g__ETEL = global::EncosyTower.Entities.Lookups;");
            p.PrintLine("using g__LP = global::Latios.Psyshock;");
            p.PrintLine("using g__UC = global::Unity.Collections;");
            p.PrintLine("using g__UE = global::Unity.Entities;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
