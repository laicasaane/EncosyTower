using EncosyTower.SourceGen.Helpers.Variants;

namespace EncosyTower.Mvvm.Generators.InternalVariants
{
    [Generator]
    public sealed class InternalVariantGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Mvvm";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        private const string OBSERVABLE_PROPERTY_ATTRIBUTE = $"{NAMESPACE}.ComponentModel.ObservablePropertyAttribute";
        private const string NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE = $"{NAMESPACE}.ComponentModel.NotifyPropertyChangedForAttribute";
        private const string RELAY_COMMAND_ATTRIBUTE = $"{NAMESPACE}.Input.RelayCommandAttribute";
        private const string BINDING_PROPERTY_ATTRIBUTE = $"{NAMESPACE}.ViewBinding.BindingPropertyAttribute";
        private const string BINDING_COMMAND_ATTRIBUTE = $"{NAMESPACE}.ViewBinding.BindingCommandAttribute";
        private const string VARIANT_ATTRIBUTE = "EncosyTower.Variants.VariantAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE))
                .WithTrackingName("MvvmInternalVariantGenerator.Compilation");

            var obsProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      OBSERVABLE_PROPERTY_ATTRIBUTE
                    , static (node, _) => node is VariableDeclaratorSyntax
                        or FieldDeclarationSyntax
                        or PropertyDeclarationSyntax
                    , static (ctx, token) => GetSemanticMatch_ObservableProperty(ctx, token)
                )
                .Where(static x => x.IsValid)
                .WithTrackingName("MvvmInternalVariantGenerator.ObservableProperties");

            var relayCommandProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      RELAY_COMMAND_ATTRIBUTE
                    , static (node, _) => node is MethodDeclarationSyntax m
                        && m.ParameterList.Parameters.Count == 1
                    , static (ctx, token) => GetSemanticMatch_Method(ctx, token)
                )
                .Where(static x => x.IsValid)
                .WithTrackingName("MvvmInternalVariantGenerator.RelayCommands");

            var bindingPropertyProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      BINDING_PROPERTY_ATTRIBUTE
                    , static (node, _) => node is MethodDeclarationSyntax m
                        && m.ParameterList.Parameters.Count == 1
                    , static (ctx, token) => GetSemanticMatch_Method(ctx, token)
                )
                .Where(static x => x.IsValid)
                .WithTrackingName("MvvmInternalVariantGenerator.BindingProperties");

            var bindingCommandProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      BINDING_COMMAND_ATTRIBUTE
                    , static (node, _) => node is MethodDeclarationSyntax m
                        && m.ParameterList.Parameters.Count == 1
                    , static (ctx, token) => GetSemanticMatch_Method(ctx, token)
                )
                .Where(static x => x.IsValid)
                .WithTrackingName("MvvmInternalVariantGenerator.BindingCommands");

            var npcfProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE
                    , static (node, _) => node is VariableDeclaratorSyntax
                        or FieldDeclarationSyntax
                        or PropertyDeclarationSyntax
                    , static (ctx, token) => GetSemanticMatch_NotifyPropertyChangedFor(ctx, token)
                )
                .Where(static x => x.IsValid)
                .WithTrackingName("MvvmInternalVariantGenerator.NotifyPropertyChangedFor");

            var typeNamesToIgnoreProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      VARIANT_ATTRIBUTE
                    , static (node, _) => node is StructDeclarationSyntax
                    , static (ctx, token) => GetTypeNameFromVariantAttribute(ctx, token)
                )
                .Where(static x => string.IsNullOrEmpty(x) == false)
                .WithTrackingName("MvvmInternalVariantGenerator.IgnoredTypeNames");

            var allCandidatesFlat = obsProvider.Collect()
                .Combine(relayCommandProvider.Collect())
                .Combine(bindingPropertyProvider.Collect())
                .Combine(bindingCommandProvider.Collect())
                .Combine(npcfProvider.Collect())
                .Select(static (data, _) => {
                    using var b = ImmutableArrayBuilder<InternalVariantSpec>.Rent();
                    foreach (var x in data.Left.Left.Left.Left)  b.Add(x); // [ObservableProperty]
                    foreach (var x in data.Left.Left.Left.Right) b.Add(x); // [RelayCommand]
                    foreach (var x in data.Left.Left.Right)      b.Add(x); // [BindingProperty]
                    foreach (var x in data.Left.Right)           b.Add(x); // [BindingCommand]
                    foreach (var x in data.Right)                b.Add(x); // [NotifyPropertyChangedFor]
                    return b.ToImmutable().AsEquatableArray();
                })
                .WithTrackingName("MvvmInternalVariantGenerator.CollectedSpecs");

            var typeNamesToIgnore = typeNamesToIgnoreProvider
                .Collect()
                .Select(static (x, _) => x.AsEquatableArray());

            var combined = allCandidatesFlat
                .Combine(typeNamesToIgnore)
                .Combine(compilationProvider)
                .WithTrackingName("MvvmInternalVariantGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left.Left, source.Left.Right);
            });
        }

        private static InternalVariantSpec GetSemanticMatch_ObservableProperty(
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

            return BuildDeclaration(typeSymbol, token);
        }

        private static InternalVariantSpec GetSemanticMatch_Method(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not IMethodSymbol methodSymbol
                || methodSymbol.Parameters.Length != 1
            )
            {
                return default;
            }

            return BuildDeclaration(methodSymbol.Parameters[0].Type, token);
        }

        private static InternalVariantSpec GetSemanticMatch_NotifyPropertyChangedFor(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            INamedTypeSymbol containingType;

            if (context.TargetSymbol is IFieldSymbol fieldSymbol
                && fieldSymbol.ContainingType is INamedTypeSymbol fieldContainingType
            )
            {
                containingType = fieldContainingType;
            }
            else if (context.TargetSymbol is IPropertySymbol propertySymbol
                && propertySymbol.ContainingType is INamedTypeSymbol propContainingType
            )
            {
                containingType = propContainingType;
            }
            else
            {
                return default;
            }

            var implementsIObservableObject = false;

            foreach (var iface in containingType.AllInterfaces)
            {
                token.ThrowIfCancellationRequested();

                if (iface.Name == "IObservableObject"
                    && iface.ContainingNamespace is { Name: "ComponentModel" } ns1
                    && ns1.ContainingNamespace is { Name: "Mvvm" } ns2
                    && ns2.ContainingNamespace is { Name: "EncosyTower" } ns3
                    && ns3.ContainingNamespace.IsGlobalNamespace
                )
                {
                    implementsIObservableObject = true;
                    break;
                }
            }

            if (implementsIObservableObject == false)
            {
                return default;
            }

            foreach (var attribute in context.Attributes)
            {
                token.ThrowIfCancellationRequested();

                if (attribute.ConstructorArguments.Length < 1
                    || attribute.ConstructorArguments[0].Value is not string propertyName
                    || string.IsNullOrEmpty(propertyName)
                )
                {
                    continue;
                }

                foreach (var member in containingType.GetMembers(propertyName))
                {
                    token.ThrowIfCancellationRequested();

                    if (member is not IPropertySymbol targetProperty)
                    {
                        continue;
                    }

                    return BuildDeclaration(targetProperty.Type, token);
                }
            }

            return default;
        }

        private static string GetTypeNameFromVariantAttribute(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Attributes.Length < 1)
            {
                return string.Empty;
            }

            var attributeData = context.Attributes[0];

            if (attributeData.ConstructorArguments.Length < 1
                || attributeData.ConstructorArguments[0].Value is not ITypeSymbol typeArg
            )
            {
                return string.Empty;
            }

            return typeArg.ToFullName();
        }

        private static InternalVariantSpec BuildDeclaration(ITypeSymbol typeSymbol, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (typeSymbol is not INamedTypeSymbol namedType
                || namedType.IsUnboundGenericType
                || (namedType.IsGenericType && namedType.TypeParameters.Length != 0)
            )
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            return InternalVariantSpecFactory.Create(namedType, MvvmIdentifiers.FromType(namedType), token);
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , EquatableArray<InternalVariantSpec> candidates
            , EquatableArray<string> typeNamesToIgnore
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (compilation.IsValid == false || candidates.Count < 1)
            {
                return;
            }

            var seenTypeNames = new HashSet<string>(typeNamesToIgnore, StringComparer.Ordinal);
            using var valueTypeBuilder = ImmutableArrayBuilder<InternalVariantSpec>.Rent();
            using var refTypeBuilder = ImmutableArrayBuilder<InternalVariantSpec>.Rent();
            var assemblyName = compilation.AssemblyName;

            foreach (var candidate in candidates)
            {
                if (seenTypeNames.Add(candidate.fullTypeName) == false)
                {
                    continue;
                }

                MvvmInternalVariantWriteCode.WriteVariantCode(ref context, in candidate, assemblyName);

                if (candidate.isValueType)
                {
                    valueTypeBuilder.Add(candidate);
                }
                else
                {
                    refTypeBuilder.Add(candidate);
                }
            }

            MvvmInternalVariantWriteCode.WriteStaticClass(
                  ref context
                , valueTypeBuilder.ToImmutable()
                , refTypeBuilder.ToImmutable()
                , assemblyName
            );
        }
    }
}
