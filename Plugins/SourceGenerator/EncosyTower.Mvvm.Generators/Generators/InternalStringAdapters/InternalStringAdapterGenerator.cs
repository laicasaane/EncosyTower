namespace EncosyTower.Mvvm.Generators.InternalStringAdapters
{
    [Generator]
    public sealed partial class InternalStringAdapterGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Mvvm";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        private const string OBSERVABLE_PROPERTY_ATTRIBUTE_FULL = $"{NAMESPACE}.ComponentModel.ObservablePropertyAttribute";
        private const string NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE_FULL = $"{NAMESPACE}.ComponentModel.NotifyPropertyChangedForAttribute";
        private const string RELAY_COMMAND_ATTRIBUTE_FULL = $"{NAMESPACE}.Input.RelayCommandAttribute";
        private const string BINDING_PROPERTY_ATTRIBUTE_FULL = $"{NAMESPACE}.ViewBinding.BindingPropertyAttribute";
        private const string ADAPTER_ATTRIBUTE_FULL = $"{NAMESPACE}.ViewBinding.AdapterAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var obsCandidatesProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      OBSERVABLE_PROPERTY_ATTRIBUTE_FULL
                    , static (node, _) => node is VariableDeclaratorSyntax
                        or FieldDeclarationSyntax
                        or PropertyDeclarationSyntax
                    , static (ctx, token) => GetCandidate_ObservableProperty(ctx, token)
                )
                .Where(static x => x.IsValid)
                .WithTrackingName("InternalStringAdapterGenerator.ObservableProperties");

            var methodRelayCommandCandidatesProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      RELAY_COMMAND_ATTRIBUTE_FULL
                    , static (node, _) => node is MethodDeclarationSyntax m
                        && m.ParameterList.Parameters.Count == 1
                    , static (ctx, token) => GetCandidate_Method(ctx, token)
                )
                .Where(static x => x.IsValid)
                .WithTrackingName("InternalStringAdapterGenerator.RelayCommands");

            var methodBindingCandidatesProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      BINDING_PROPERTY_ATTRIBUTE_FULL
                    , static (node, _) => node is MethodDeclarationSyntax m
                        && m.ParameterList.Parameters.Count == 1
                    , static (ctx, token) => GetCandidate_Method(ctx, token)
                )
                .Where(static x => x.IsValid)
                .WithTrackingName("InternalStringAdapterGenerator.BindingProperties");

            var npcfCandidatesProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE_FULL
                    , static (node, _) => node is VariableDeclaratorSyntax
                        or FieldDeclarationSyntax
                        or PropertyDeclarationSyntax
                    , static (ctx, token) => GetCandidate_NotifyPropertyChangedFor(ctx, token)
                )
                .Where(static x => x.IsValid)
                .WithTrackingName("InternalStringAdapterGenerator.NotifyPropertyChangedFor");

            var existingAdaptersProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ADAPTER_ATTRIBUTE_FULL
                    , static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax
                    , static (ctx, token) => GetExistingAdapterSourceTypeName(ctx, token)
                )
                .Where(static t => t != null)
                .WithTrackingName("InternalStringAdapterGenerator.ExistingAdapters");

            var allCandidatesFlat = obsCandidatesProvider.Collect()
                .Combine(methodRelayCommandCandidatesProvider.Collect())
                .Combine(methodBindingCandidatesProvider.Collect())
                .Combine(npcfCandidatesProvider.Collect())
                .Select(static (data, _) => {
                    using var b = ImmutableArrayBuilder<StringAdapterSpec>.Rent();
                    foreach (var x in data.Left.Left.Left)  b.Add(x); // [ObservableProperty]
                    foreach (var x in data.Left.Left.Right) b.Add(x); // relay-command method
                    foreach (var x in data.Left.Right)      b.Add(x); // binding method
                    foreach (var x in data.Right)           b.Add(x); // [NotifyPropertyChangedFor]
                    return b.ToImmutable().AsEquatableArray();
                })
                .WithTrackingName("InternalStringAdapterGenerator.CollectedSpecs");

            var existingAdapters = existingAdaptersProvider
                .Collect()
                .Select(static (x, _) => x.AsEquatableArray());

            var combined = allCandidatesFlat
                .Combine(compilationProvider)
                .Combine(existingAdapters)
                .WithTrackingName("InternalStringAdapterGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Left.Right, source.Left.Left, source.Right);
            });
        }

        private static StringAdapterSpec GetCandidate_ObservableProperty(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            ITypeSymbol typeSymbol;

            if (context.TargetSymbol is IFieldSymbol fieldSymbol)
            {
                typeSymbol = fieldSymbol.Type;
            }
            else if (context.TargetSymbol is IPropertySymbol propertySymbol)
            {
                typeSymbol = propertySymbol.Type;
            }
            else
            {
                return default;
            }

            return MakeCandidate(typeSymbol);
        }

        private static StringAdapterSpec GetCandidate_Method(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not IMethodSymbol methodSymbol
                || methodSymbol.Parameters.Length != 1
                || methodSymbol.Parameters[0].Type is not ITypeSymbol typeSymbol
            )
            {
                return default;
            }

            return MakeCandidate(typeSymbol);
        }

        private static StringAdapterSpec GetCandidate_NotifyPropertyChangedFor(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            ITypeSymbol typeSymbol;
            INamedTypeSymbol containingType;

            if (context.TargetSymbol is IFieldSymbol fieldSymbol
                && fieldSymbol.ContainingType is INamedTypeSymbol fieldContainingType
            )
            {
                typeSymbol = fieldSymbol.Type;
                containingType = fieldContainingType;
            }
            else if (context.TargetSymbol is IPropertySymbol propertySymbol
                && propertySymbol.ContainingType is INamedTypeSymbol propertyContainingType
            )
            {
                typeSymbol = propertySymbol.Type;
                containingType = propertyContainingType;
            }
            else
            {
                return default;
            }

            foreach (var iface in containingType.AllInterfaces)
            {
                if (iface.Name == "IObservableObject"
                    && iface.ContainingNamespace is { Name: "ComponentModel" } ns1
                    && ns1.ContainingNamespace is { Name: "Mvvm" } ns2
                    && ns2.ContainingNamespace is { Name: "EncosyTower" } ns3
                    && ns3.ContainingNamespace.IsGlobalNamespace
                )
                {
                    return MakeCandidate(typeSymbol);
                }
            }

            return default;
        }

        private static string GetExistingAdapterSourceTypeName(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol declaredSymbol)
            {
                return null;
            }

            var implementsIAdapter = false;

            foreach (var iface in declaredSymbol.AllInterfaces)
            {
                token.ThrowIfCancellationRequested();

                if (iface.Name == "IAdapter"
                    && iface.ContainingNamespace is { Name: "ViewBinding" } ns1
                    && ns1.ContainingNamespace is { Name: "Mvvm" } ns2
                    && ns2.ContainingNamespace is { Name: "EncosyTower" } ns3
                    && ns3.ContainingNamespace.IsGlobalNamespace
                )
                {
                    implementsIAdapter = true;
                    break;
                }
            }

            if (implementsIAdapter == false)
            {
                return null;
            }

            foreach (var attribute in context.Attributes)
            {
                token.ThrowIfCancellationRequested();

                if (attribute.ConstructorArguments.Length != 2)
                {
                    continue;
                }

                if (attribute.ConstructorArguments[1].Value is not ITypeSymbol toArg
                    || toArg.SpecialType != SpecialType.System_String
                )
                {
                    continue;
                }

                if (attribute.ConstructorArguments[0].Value is INamedTypeSymbol fromArg)
                {
                    return fromArg.ToFullName();
                }
            }

            return null;
        }

        private static StringAdapterSpec MakeCandidate(ITypeSymbol typeSymbol)
        {
            var ns = typeSymbol.ContainingNamespace;

            return new StringAdapterSpec {
                fullTypeName = typeSymbol.ToFullName(),
                simpleName = typeSymbol.Name,
                namespaceName = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : string.Empty,
                identifierName = MvvmIdentifiers.FromSimpleType(typeSymbol),
                labelName = typeSymbol.ToFullNameNoGlobal(),
            };
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , EquatableArray<StringAdapterSpec> candidates
            , EquatableArray<string> existingAdapterTypeNames
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (compilation.IsValid == false || candidates.Count < 1)
            {
                return;
            }

            var assemblyName = compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Mvvm.Generators.InternalStringAdapters.InternalStringAdapterGenerator"
                , assemblyName
                , assemblyName
                , "InternalStringAdapters"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  PrintAdditionalUsings(context.CancellationToken)
                , WriteAdapter(candidates, existingAdapterTypeNames, assemblyName, context.CancellationToken)
                , string.Empty
                , context.CancellationToken
            );

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }
    }
}
