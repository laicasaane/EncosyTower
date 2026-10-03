using EncosyTower.Core.PolyEnumStructs;
using EncosyTower.SourceGen.Helpers.PolyEnumStructs;

namespace EncosyTower.Core.Generators.PolyEnumStructs
{
    [Generator]
    internal sealed class PolyEnumStructGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.PolyEnumStructs";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string POLY_ENUM_STRUCT_ATTRIBUTE_METADATA = $"{NAMESPACE}.PolyEnumStructAttribute";
        private const string ENUM_CASE_VALUE_ATTRIBUTE = $"global::{NAMESPACE}.EnumCaseValueAttribute";
        private const string ENUM_CASE_IGNORE_ATTRIBUTE = $"global::{NAMESPACE}.EnumCaseIgnoreAttribute";
        private const string UNDEFINED_NAME = "Undefined";
        private const string INTERFACE_NAME = "IEnumCase";
        private const string READ_ONLY_ATTRIBUTE = "global::System.ComponentModel.ReadOnlyAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => PolyEnumStructCompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  POLY_ENUM_STRUCT_ATTRIBUTE_METADATA
                , static (node, _) => node is StructDeclarationSyntax
                , ExtractSpec
            ).WithTrackingName("PolyEnumStructGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("PolyEnumStructGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.Compilation.IsValid)
                .WithTrackingName("PolyEnumStructGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static PolyEnumStructSpec ExtractSpec(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not StructDeclarationSyntax structSyntax
                || context.TargetSymbol is not INamedTypeSymbol structSymbol
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            var attribute = context.Attributes[0];
            var semanticModel = context.SemanticModel;
            var resolution = ContainerResolver.Resolve(
                  structSymbol
                , attribute
                , semanticModel.Compilation
                , token
            );

            if (resolution.Kind != ContainerResolutionKind.Valid)
            {
                return default;
            }

            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = structSyntax.SyntaxTree;
            var typeIdentifier = structSymbol.ToValidIdentifier();
            var hintName = structSymbol.ToMetadataName();

            var isExplicitLayout = CaseLayoutRules.IsExplicitLayout(structSymbol, token);

            if (isExplicitLayout && HasCaseFieldWithUnknownSize(resolution, token))
            {
                return default;
            }

            if (isExplicitLayout && HasCaseStorageWithManagedReference(resolution, token))
            {
                return default;
            }

            var result = new PolyEnumStructSpec {
                typeName = structSymbol.Name,
                typeSelfName = structSyntax.Identifier.Text + structSyntax.TypeParameterList,
                typeFullName = structSymbol.ToFullName(),
                typeConstraints = ContainerResolver.FormatConstraintClauses(structSymbol, token),
                enumExtensionsAttributeOwner = GetUnboundContainingTypeName(structSymbol.ContainingType, token),
                typeNamespace = structSymbol.ContainingNamespace.ToDisplayString(),
                typeIdentifier = typeIdentifier,
                hintName = hintName,
                parentIsNamespace = structSyntax.Parent is BaseNamespaceDeclarationSyntax or CompilationUnitSyntax,
                isReadOnly = structSymbol.IsReadOnly,
                isExplicitLayout = isExplicitLayout,
                typeAccessibility = structSymbol.DeclaredAccessibility,
                separateContainer = resolution.UsesSeparateContainer,
            };

            if (resolution.UsesSeparateContainer)
            {
                result.targetOutputScope = SupportContainerSpecFactory.CreateCustom(structSymbol, token);
            }
            else
            {
                TypeCreationHelpers.GenerateOpeningAndClosingSource(
                      structSyntax
                    , token
                    , out result.openingSource
                    , out result.closingSource
                    , printAdditionalUsings: PrintAdditionalUsings
                );

                result.containingTypes = TypeCreationHelpers.GetContainingTypeSpecs(structSyntax, token);
            }

            FillTargetParameters(ref result, resolution, token);

            if (resolution.UsesSeparateContainer)
            {
                var supportContainer = resolution.Container is null
                    ? SupportContainerSpecFactory.CreateDefault(structSymbol, token)
                    : SupportContainerSpecFactory.CreateCustom(resolution.Container, token);

                if (supportContainer.IsValid(token) == false)
                {
                    return default;
                }

                result.supportContainer = supportContainer;
                result.parentIsNamespace = supportContainer.Parents.Count < 1;
                result.enumExtensionsAttributeOwner = string.Empty;
                result.typeAccessibility = ParseAccessibility(supportContainer.Declaration.Accessibility);
            }

            foreach (var arg in attribute.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

                if (arg.Key == "SortFieldsBySize" && arg.Value.Value is bool sortFieldsBySize)
                {
                    result.sortFieldsBySize = sortFieldsBySize;
                }
                else if (arg.Key == "AutoEquatable" && arg.Value.Value is bool autoEquatable)
                {
                    result.autoEquatable = autoEquatable;
                }
                else if (arg.Key == "WithEnumExtensions" && arg.Value.Value is bool withEnumExtensions)
                {
                    result.withEnumExtensions = withEnumExtensions;
                }
            }

            if (structSymbol.TypeParameters.Length > 0
                && resolution.Container is null
                && result.withEnumExtensions
            )
            {
                return default;
            }

            AggregateInterfaceAndStructs(ref result, resolution, token);

            return result;
        }

        private static string GetUnboundContainingTypeName(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (type is null)
            {
                return string.Empty;
            }

            var prefix = type.ContainingType is null
                ? GetGlobalNamespacePrefix(type.ContainingNamespace)
                : GetUnboundContainingTypeName(type.ContainingType, token);

            var arity = type.Arity;
            var typeArguments = arity < 1 ? string.Empty : $"<{new string(',', arity - 1)}>";
            var separator = string.Equals(prefix, "global::", StringComparison.Ordinal) ? string.Empty : ".";

            return $"{prefix}{separator}{type.Name}{typeArguments}";

            static string GetGlobalNamespacePrefix(INamespaceSymbol containingNamespace)
                => containingNamespace.IsGlobalNamespace
                    ? "global::"
                    : $"global::{containingNamespace.ToDisplayString()}";
        }

        private static Accessibility ParseAccessibility(string value)
            => value switch {
                "public" => Accessibility.Public,
                "internal" => Accessibility.Internal,
                "private" => Accessibility.Private,
                "protected" => Accessibility.Protected,
                "protected internal" => Accessibility.ProtectedOrInternal,
                "private protected" => Accessibility.ProtectedAndInternal,
                _ => Accessibility.NotApplicable,
            };

        private static void GenerateOutput(
              SourceProductionContext context
            , PolyEnumStructCompilationSpec compilation
            , PolyEnumStructSpec candidate
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
                  "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator"
                , assemblyName
                , candidate.hintName
                , "PolyEnumStruct"
                , string.Empty
            );

            Microsoft.CodeAnalysis.Text.SourceText generatedSource;

            if (candidate.separateContainer)
            {
                var targetPrinter = new Printer(0, 1024 * 512, context.CancellationToken);
                candidate.targetOutputScope.WriteOpening(
                      ref targetPrinter
                    , PrintAdditionalUsings
                    , context.CancellationToken
                );
                targetPrinter.Print(candidate.WriteCode(compilation, context.CancellationToken));
                targetPrinter.PrintEndLine();
                candidate.targetOutputScope.WriteClosing(ref targetPrinter, context.CancellationToken);
                generatedSource = Microsoft.CodeAnalysis.Text.SourceText.From(
                      targetPrinter.Result.TrimEnd('\n') + "\n"
                    , Encoding.UTF8
                );
            }
            else
            {
                generatedSource = TypeCreationHelpers.GenerateSourceText(
                      candidate.openingSource
                    , candidate.WriteCode(compilation, context.CancellationToken)
                    , candidate.closingSource
                    , context.CancellationToken
                );
            }

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);

            if (candidate.separateContainer == false)
            {
                return;
            }

            var containerHintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator"
                , assemblyName
                , candidate.hintName
                , "PolyEnumStructContainer"
                , candidate.supportContainer.MetadataName
            );
            var containerPrinter = new Printer(0, 1024 * 512, context.CancellationToken);
            candidate.supportContainer.WriteOpening(
                  ref containerPrinter
                , PrintAdditionalUsings
                , context.CancellationToken
            );
            candidate.WriteContainerCode(ref containerPrinter, compilation, context.CancellationToken);
            candidate.supportContainer.WriteClosing(ref containerPrinter, context.CancellationToken);
            var containerSource = Microsoft.CodeAnalysis.Text.SourceText.From(containerPrinter.Result, Encoding.UTF8)
                .WithIgnoreUnassignedVariableWarning();
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(containerHintName, containerSource);
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
            p.PrintLine("using g__SD = global::System.Diagnostics;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using g__ET = global::EncosyTower.Common;");
            p.PrintLine("using g__ETCol = global::EncosyTower.Collections;");
            p.PrintLine("using g__ETCon = global::EncosyTower.Conversion;");
            p.PrintLine("using g__ETDVD = global::EncosyTower.Debugging.ValidationDefines;");
            p.PrintLine("using g__ETEE = global::EncosyTower.EnumExtensions;");
            p.PrintLine("using g__ETEESG = global::EncosyTower.EnumExtensions.SourceGen;");
            p.PrintLine("using g__UE = global::UnityEngine;");
            p.PrintLine("using g__UC = global::Unity.Collections;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        private static void AggregateInterfaceAndStructs(
              ref PolyEnumStructSpec polyEnumStruct
            , ContainerResolution resolution
            , CancellationToken token
        )
        {
            using var structsBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.StructSpec>.Rent();

            var structName = resolution.Target.Name;
            var verboseUndefinedName = $"{structName}_{UNDEFINED_NAME}";

            PolyEnumStructSpec.StructSpec undefinedStruct = default;

            foreach (var @case in resolution.Cases)
            {
                token.ThrowIfCancellationRequested();

                var @struct = GetStruct(@case, resolution, polyEnumStruct, token);

                if (@struct.IsValid == false)
                {
                    continue;
                }

                if (@case.IsUndefined)
                {
                    undefinedStruct = @struct;
                    polyEnumStruct.definedUndefinedStruct = TryGetUndefinedCase(
                          @case.Symbol.Name
                        , UNDEFINED_NAME
                        , verboseUndefinedName
                        , out var undefinedKind
                    )
                        ? undefinedKind
                        : PolyEnumStructSpec.DefinedUndefinedStruct.Default;
                }
                else
                {
                    structsBuilder.Add(@struct);
                }
            }

            polyEnumStruct.interfaceDef = resolution.BaseInterface is null
                ? default
                : GetInterface(resolution.BaseInterface, resolution, genericInterface: false, token);
            polyEnumStruct.genericInterfaceDef = resolution.GenericInterface is null
                ? default
                : GetInterface(resolution.GenericInterface, resolution, genericInterface: true, token);

            if (undefinedStruct.IsValid)
            {
                structsBuilder.Add(undefinedStruct with {
                    isUndefined = true,
                    implicitlyDeclared = false,
                });
            }
            else if (polyEnumStruct.definedUndefinedStruct == PolyEnumStructSpec.DefinedUndefinedStruct.None)
            {
                var undefinedDeclaration = BuildImplicitUndefinedDeclaration(polyEnumStruct, verboseUndefinedName);
                structsBuilder.Add(new PolyEnumStructSpec.StructSpec {
                    name = polyEnumStruct.separateContainer
                        ? $"{polyEnumStruct.supportContainer.TypeName}.{undefinedDeclaration}"
                        : verboseUndefinedName,
                    declarationName = undefinedDeclaration,
                    declarationConstraints = BuildIncludedTypeConstraints(
                          polyEnumStruct
                        , polyEnumStruct.genericInterfaceTargetIndices
                    ),
                    targetTypeName = polyEnumStruct.typeFullName,
                    toMethodTypeParameters = BuildMissingTypeParameters(
                          polyEnumStruct
                        , polyEnumStruct.genericInterfaceTargetIndices
                    ),
                    toMethodConstraints = BuildMissingTypeConstraints(
                          polyEnumStruct
                        , polyEnumStruct.genericInterfaceTargetIndices
                    ),
                    identifier = UNDEFINED_NAME,
                    targetIndices = polyEnumStruct.genericInterfaceTargetIndices,
                    isUndefined = true,
                    implicitlyDeclared = true,
                    ownerIsContainer = polyEnumStruct.separateContainer,
                });
            }

            polyEnumStruct.structs = structsBuilder.ToImmutable();
            EnsureInterface(ref polyEnumStruct.interfaceDef);
            EnsureGenericInterface(ref polyEnumStruct.genericInterfaceDef, polyEnumStruct);
        }

        private static bool HasCaseFieldWithUnknownSize(ContainerResolution resolution, CancellationToken token)
        {
            var cases = resolution.Cases;
            var caseCount = cases.Count;

            for (var i = 0; i < caseCount; i++)
            {
                var members = cases[i].Symbol.GetMembers();
                var memberCount = members.Length;

                for (var k = 0; k < memberCount; k++)
                {
                    token.ThrowIfCancellationRequested();

                    if (members[k] is IFieldSymbol field
                        && (CaseLayoutRules.HasUnknownSize(field, token) || StoresErrorType(field, token))
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool StoresErrorType(IFieldSymbol field, CancellationToken token)
        {
            if (field.IsStatic || field.IsConst)
            {
                return false;
            }

            return CaseLayoutRules.TryFindStoredType(
                  field.Type
                , static (type, _) => type.TypeKind == TypeKind.Error
                , token
                , out _
            );
        }

        private static bool HasCaseStorageWithManagedReference(ContainerResolution resolution, CancellationToken token)
        {
            var cases = resolution.Cases;
            var caseCount = cases.Count;

            for (var i = 0; i < caseCount; i++)
            {
                var members = cases[i].Symbol.GetMembers();
                var memberCount = members.Length;

                for (var k = 0; k < memberCount; k++)
                {
                    token.ThrowIfCancellationRequested();

                    var member = members[k];

                    if (member is IFieldSymbol { IsStatic: false, IsConst: false } field
                        && CaseLayoutRules.CanHoldManagedReference(field.Type)
                    )
                    {
                        return true;
                    }

                    if (member is IEventSymbol { IsStatic: false } eventSymbol
                        && CaseLayoutRules.IsFieldLikeEvent(eventSymbol)
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryGetUndefinedCase(
              string caseName
            , string undefinedName
            , string verboseUndefinedName
            , out PolyEnumStructSpec.DefinedUndefinedStruct result
        )
        {
            if (string.Equals(caseName, undefinedName, StringComparison.Ordinal))
            {
                result = PolyEnumStructSpec.DefinedUndefinedStruct.Default;
                return true;
            }

            if (string.Equals(caseName, verboseUndefinedName, StringComparison.Ordinal))
            {
                result = PolyEnumStructSpec.DefinedUndefinedStruct.Verbose;
                return true;
            }

            result = default;
            return false;
        }

        private static PolyEnumStructSpec.InterfaceSpec GetInterface(
              INamedTypeSymbol symbol
            , ContainerResolution resolution
            , bool genericInterface
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var syntax = symbol.DeclaringSyntaxReferences
                .Select(reference => reference.GetSyntax(token))
                .OfType<InterfaceDeclarationSyntax>()
                .FirstOrDefault();

            var result = new PolyEnumStructSpec.InterfaceSpec {
                name = INTERFACE_NAME,
                declarationName = syntax is null
                    ? symbol.Name
                    : syntax.Identifier.Text + syntax.TypeParameterList,
                constraints = ContainerResolver.FormatConstraintClauses(symbol, token),
                definedInterface = true,
            };

            var substitutions = ContainerResolver.CreateTargetMap(resolution, token);

            if (genericInterface)
            {
                for (var i = 0; i < symbol.TypeParameters.Length; i++)
                {
                    substitutions[symbol.TypeParameters[i]] = resolution.GenericInterfaceTargetIndices[i];
                }
            }

            var targetNames = resolution.TargetParameters.Select(static value => value.Name).ToArray();
            AggregateInterfaceMembers(
                  symbol
                , substitutions
                , targetNames
                , genericInterface
                , ref result
                , token
            );

            return result;
        }

        private static void AggregateInterfaceMembers(
              INamedTypeSymbol symbol
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetNames
            , bool genericInterface
            , ref PolyEnumStructSpec.InterfaceSpec interfaceDef
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            using var propertiesBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.PropertyDeclaration>.Rent();
            using var indexersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.IndexerDeclaration>.Rent();
            using var methodsBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.MethodDeclaration>.Rent();

            foreach (var member in symbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (TryGetInterfaceMember(
                      member
                    , substitutions
                    , targetNames
                    , genericInterface
                    , propertiesBuilder
                    , indexersBuilder
                    , methodsBuilder
                    , token
                ) == false)
                {
                    propertiesBuilder.Clear();
                    indexersBuilder.Clear();
                    methodsBuilder.Clear();
                    break;
                }
            }

            interfaceDef.properties = propertiesBuilder.ToImmutable();
            interfaceDef.indexers = indexersBuilder.ToImmutable();
            interfaceDef.methods = methodsBuilder.ToImmutable();
        }

        private static void EnsureInterface(ref PolyEnumStructSpec.InterfaceSpec result)
        {
            if (result.IsValid)
            {
                return;
            }

            using var propertiesBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.PropertyDeclaration>.Rent();
            using var indexersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.IndexerDeclaration>.Rent();
            using var methodsBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.MethodDeclaration>.Rent();

            result = new PolyEnumStructSpec.InterfaceSpec {
                name = INTERFACE_NAME,
                declarationName = INTERFACE_NAME,
                properties = propertiesBuilder.ToImmutable(),
                indexers = indexersBuilder.ToImmutable(),
                methods = methodsBuilder.ToImmutable(),
                definedInterface = false,
            };
        }

        private static void EnsureGenericInterface(
              ref PolyEnumStructSpec.InterfaceSpec result
            , in PolyEnumStructSpec polyEnumStruct
        )
        {
            if (result.IsValid || polyEnumStruct.genericInterfaceTargetIndices.Count < 1)
            {
                return;
            }

            var names = new List<string>();
            var constraints = new List<string>();

            foreach (var index in polyEnumStruct.genericInterfaceTargetIndices.AsReadOnlySpan())
            {
                var parameter = polyEnumStruct.targetParameters[index];
                names.Add(parameter.name);

                if (string.IsNullOrEmpty(parameter.constraint) == false)
                {
                    constraints.Add(parameter.constraint);
                }
            }

            result = new PolyEnumStructSpec.InterfaceSpec {
                name = INTERFACE_NAME,
                declarationName = $"{INTERFACE_NAME}<{string.Join(", ", names)}>",
                constraints = string.Join("\n", constraints),
                properties = default,
                indexers = default,
                methods = default,
                definedInterface = false,
            };
        }

        private static PolyEnumStructSpec.StructSpec GetStruct(
              CaseResolution @case
            , ContainerResolution resolution
            , in PolyEnumStructSpec polyEnumStruct
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var symbol = @case.Symbol;

            if (symbol.HasAttribute(ENUM_CASE_IGNORE_ATTRIBUTE, token))
            {
                return default;
            }

            var substitutions = ContainerResolver.CreateTargetMap(resolution, @case, token);
            var targetArgumentNames = resolution.TargetParameters.Select(static value => value.Name).ToArray();

            for (var i = 0; i < symbol.TypeParameters.Length; i++)
            {
                targetArgumentNames[@case.AuthoredTargetIndices[i]] = symbol.TypeParameters[i].Name;
            }

            var declarationName = symbol.Name;

            if (symbol.TypeParameters.Length > 0)
            {
                declarationName += $"<{string.Join(", ", symbol.TypeParameters.Select(static value => value.Name))}>";
            }

            var constructedName = @case.Owner == CaseOwner.Container
                ? $"{polyEnumStruct.supportContainer.TypeName}.{declarationName}"
                : symbol.Name;
            var missingIndices = Enumerable.Range(0, resolution.TargetParameters.Count)
                .Where(index => @case.TargetOrderedIndices.Contains(index) == false)
                .ToArray();
            var result = new PolyEnumStructSpec.StructSpec {
                name = constructedName,
                declarationName = declarationName,
                targetTypeName = ContainerResolver.FormatType(
                      resolution.Target
                    , substitutions
                    , targetArgumentNames
                    , token
                ),
                toMethodTypeParameters = BuildTypeParameterList(missingIndices, targetArgumentNames),
                toMethodConstraints = BuildTypeParameterConstraints(
                      missingIndices
                    , resolution
                    , substitutions
                    , targetArgumentNames
                    , token
                ),
                displayName = symbol.GetDisplayNameOrDefault(symbol.Name, token),
                identifier = @case.IsUndefined ? UNDEFINED_NAME : symbol.Name,
                targetIndices = @case.TargetOrderedIndices.ToImmutableArray().AsEquatableArray(),
                isUndefined = @case.IsUndefined,
                isReadOnly = symbol.IsReadOnly,
                isRecord = symbol.IsRecord,
                ownerIsContainer = @case.Owner == CaseOwner.Container,
            };

            AggregateConstructions(ref result, symbol, substitutions, targetArgumentNames, token);
            AggregatePrimaryParameters(ref result, symbol, substitutions, targetArgumentNames, token);

            AggregateStructMembers(
                  ref result
                , symbol
                , substitutions
                , targetArgumentNames
                , polyEnumStruct.isExplicitLayout == false
                , token
            );

            return result;
        }

        private static void AggregateConstructions(
              ref PolyEnumStructSpec.StructSpec structDef
            , ITypeSymbol symbol
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArgumentNames
            , CancellationToken token
        )
        {
            using var arrayBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.ConstructionSpec>.Rent();

            foreach (var attribute in symbol.GetAttributes(ENUM_CASE_VALUE_ATTRIBUTE, token))
            {
                token.ThrowIfCancellationRequested();

                if (attribute.ConstructorArguments.Length < 1)
                {
                    continue;
                }

                var arg = attribute.ConstructorArguments[0];

                if (arg.IsNull == false
                    && arg.Kind is TypedConstantKind.Primitive or TypedConstantKind.Enum
                    && arg.Type is INamedTypeSymbol
                )
                {
                    arrayBuilder.Add(new PolyEnumStructSpec.ConstructionSpec {
                        type = GetType(arg.Type, substitutions, targetArgumentNames, token),
                        value = GetCreationValue(arg.Type, arg.Value, token),
                    });
                }
            }

            structDef.constructions = arrayBuilder.ToImmutable();
        }

        private static void AggregatePrimaryParameters(
              ref PolyEnumStructSpec.StructSpec structDef
            , INamedTypeSymbol symbol
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArgumentNames
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.ParameterSpec>.Rent();

            if (TryGetPrimaryConstructor(symbol, token, out var constructor))
            {
                var isReadOnly = symbol.IsReadOnly;

                foreach (var parameter in constructor.Parameters)
                {
                    token.ThrowIfCancellationRequested();

                    var fieldSize = 0;
                    parameter.Type.GetUnmanagedSize(ref fieldSize, token);

                    parametersBuilder.Add(new PolyEnumStructSpec.ParameterSpec {
                        refKind = parameter.RefKind,
                        field = new PolyEnumStructSpec.FieldSpec {
                            name = parameter.Name,
                            returnType = GetType(parameter.Type, substitutions, targetArgumentNames, token),
                            size = fieldSize,
                            implicityDeclared = true,
                            isReadOnly = isReadOnly,
                        },
                    });
                }
            }

            structDef.parameters = parametersBuilder.ToImmutable();
        }

        private static bool TryGetPrimaryConstructor(
              INamedTypeSymbol symbol
            , CancellationToken token
            , out IMethodSymbol result
        )
        {
            foreach (var constructor in symbol.InstanceConstructors)
            {
                token.ThrowIfCancellationRequested();

                foreach (var reference in constructor.DeclaringSyntaxReferences)
                {
                    if (reference.GetSyntax(token) is RecordDeclarationSyntax)
                    {
                        result = constructor;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        private static void AggregateStructMembers(
              ref PolyEnumStructSpec.StructSpec structDef
            , ITypeSymbol symbol
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArgumentNames
            , bool collectHiddenFields
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            using var fieldsBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.FieldSpec>.Rent();
            using var hiddenFieldsBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.FieldSpec>.Rent();
            using var propertiesBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.PropertyDeclaration>.Rent();
            using var indexersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.IndexerDeclaration>.Rent();
            using var methodsBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.MethodDeclaration>.Rent();

            var structSize = 0;
            var structAlignment = 1;
            var hasUnlistedStorage = false;
            var isReadOnly = symbol.IsReadOnly;

            foreach (var member in symbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member.IsStatic)
                {
                    continue;
                }

                var accessibility = member.DeclaredAccessibility;

                if (member is IFieldSymbol fieldSymbol)
                {
                    if (fieldSymbol.IsConst)
                    {
                        continue;
                    }

                    var fieldSize = 0;
                    var fieldAlignment = 1;
                    fieldSymbol.GetUnmanagedSizeAndAlignment(ref fieldSize, ref fieldAlignment, token);
                    AppendStorage(ref structSize, ref structAlignment, fieldSize, fieldAlignment);

                    var isListed = fieldSymbol.IsImplicitlyDeclared == false
                        && accessibility is Accessibility.Public or Accessibility.Internal;

                    if (isListed == false)
                    {
                        hasUnlistedStorage = true;

                        if (collectHiddenFields
                            && TryGetHiddenFieldName(fieldSymbol, structDef.parameters, out var hiddenName)
                        )
                        {
                            hiddenFieldsBuilder.Add(new PolyEnumStructSpec.FieldSpec {
                                name = hiddenName,
                                returnType = GetType(fieldSymbol.Type, substitutions, targetArgumentNames, token),
                                size = fieldSize,
                                implicityDeclared = fieldSymbol.IsImplicitlyDeclared,
                                isReadOnly = fieldSymbol.IsReadOnly,
                            });
                        }

                        continue;
                    }

                    fieldsBuilder.Add(new PolyEnumStructSpec.FieldSpec {
                        name = fieldSymbol.Name,
                        returnType = GetType(fieldSymbol.Type, substitutions, targetArgumentNames, token),
                        size = fieldSize,
                        implicityDeclared = fieldSymbol.IsImplicitlyDeclared,
                        isReadOnly = fieldSymbol.IsReadOnly,
                    });

                    continue;
                }

                if (member is IEventSymbol eventSymbol && CaseLayoutRules.IsFieldLikeEvent(eventSymbol))
                {
                    AppendStorage(ref structSize, ref structAlignment, sizeof(ulong), sizeof(ulong));
                    hasUnlistedStorage = true;

                    if (collectHiddenFields)
                    {
                        hiddenFieldsBuilder.Add(new PolyEnumStructSpec.FieldSpec {
                            name = eventSymbol.Name,
                            returnType = GetType(eventSymbol.Type, substitutions, targetArgumentNames, token),
                            size = sizeof(ulong),
                            implicityDeclared = true,
                            isReadOnly = false,
                        });
                    }

                    continue;
                }

                if (member.IsImplicitlyDeclared == false
                    && accessibility is Accessibility.Public or Accessibility.Internal
                )
                {
                    GetStructMember(
                          member
                        , isReadOnly
                        , substitutions
                        , targetArgumentNames
                        , token
                        , propertiesBuilder
                        , indexersBuilder
                        , methodsBuilder
                    );
                }
            }

            var tailRemainder = structAlignment > 0 ? structSize % structAlignment : 0;

            if (tailRemainder != 0)
            {
                structSize += structAlignment - tailRemainder;
            }

            structDef.fields = fieldsBuilder.ToImmutable();
            structDef.properties = propertiesBuilder.ToImmutable();
            structDef.indexers = indexersBuilder.ToImmutable();
            structDef.methods = methodsBuilder.ToImmutable();
            structDef.hiddenFields = hiddenFieldsBuilder.ToImmutable();
            structDef.size = structSize;
            structDef.hasUnlistedStorage = hasUnlistedStorage;

            return;

            static void AppendStorage(ref int structSize, ref int structAlignment, int size, int alignment)
            {
                var remainder = structSize % alignment;

                if (remainder != 0)
                {
                    structSize += alignment - remainder;
                }

                structSize += size;
                structAlignment = Math.Max(structAlignment, alignment);
            }

            static bool TryGetHiddenFieldName(
                  IFieldSymbol field
                , EquatableArray<PolyEnumStructSpec.ParameterSpec> parameters
                , out string name
            )
            {
                name = null;

                if (field.IsFixedSizeBuffer || field.Type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer)
                {
                    return false;
                }

                if (field.IsImplicitlyDeclared == false)
                {
                    name = field.Name;
                    return true;
                }

                if (field.AssociatedSymbol is not IPropertySymbol property
                    || property.ExplicitInterfaceImplementations.Length > 0
                    || IsPrimaryParameter(property.Name, parameters)
                )
                {
                    return false;
                }

                name = property.Name;
                return true;
            }

            static bool IsPrimaryParameter(string name, EquatableArray<PolyEnumStructSpec.ParameterSpec> parameters)
            {
                var count = parameters.Count;

                for (var i = 0; i < count; i++)
                {
                    if (string.Equals(parameters[i].field.name, name, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static bool TryGetInterfaceMember(
              ISymbol member
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArgumentNames
            , bool genericInterface
            , ImmutableArrayBuilder<PolyEnumStructSpec.PropertyDeclaration> propertiesBuilder
            , ImmutableArrayBuilder<PolyEnumStructSpec.IndexerDeclaration> indexersBuilder
            , ImmutableArrayBuilder<PolyEnumStructSpec.MethodDeclaration> methodsBuilder
            , CancellationToken token
        )
        {
            if (member is IPropertySymbol propertySymbol)
            {
                var getterIsDim = false;
                var setterIsDim = false;

                foreach (var syntaxRef in propertySymbol.DeclaringSyntaxReferences)
                {
                    token.ThrowIfCancellationRequested();

                    if (syntaxRef.GetSyntax(token) is not BasePropertyDeclarationSyntax baseSyntax)
                    {
                        continue;
                    }

                    if (baseSyntax.AccessorList is AccessorListSyntax accessorListSyntax
                        && accessorListSyntax.Accessors.Count > 0
                    )
                    {
                        foreach (var accessor in accessorListSyntax.Accessors)
                        {
                            token.ThrowIfCancellationRequested();

                            if (accessor.Body is not null || accessor.ExpressionBody is not null)
                            {
                                if (accessor.Keyword.Text == "get")
                                {
                                    getterIsDim = true;
                                }
                                else if (accessor.Keyword.Text == "set")
                                {
                                    setterIsDim = true;
                                }
                            }
                        }
                    }
                    else if (baseSyntax is PropertyDeclarationSyntax propertySyntax)
                    {
                        if (propertySyntax.ExpressionBody is not null)
                        {
                            getterIsDim = true;
                        }
                    }
                    else if (baseSyntax is IndexerDeclarationSyntax indexerSyntax)
                    {
                        if (indexerSyntax.ExpressionBody is not null)
                        {
                            getterIsDim = true;
                        }
                    }
                }

                var isReadOnly = false;

                if (propertySymbol.TryGetAttribute(READ_ONLY_ATTRIBUTE, out var attrib, token)
                    && attrib.ConstructorArguments.Length == 1
                )
                {
                    isReadOnly = (bool)attrib.ConstructorArguments[0].Value;
                }

                if (propertySymbol.IsIndexer)
                {
                    var indexerDef = new PolyEnumStructSpec.IndexerDeclaration {
                        returnType = GetType(propertySymbol.Type, substitutions, targetArgumentNames, token),
                        getter = GetPropertyMethod(propertySymbol.GetMethod, token, isGetter: true, getterIsDim),
                        setter = GetPropertyMethod(propertySymbol.SetMethod, token, isGetter: false, setterIsDim),
                        refKind = propertySymbol.RefKind,
                        genericInterface = genericInterface,
                    };

                    using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.SlimParameterSpec>.Rent();

                    foreach (var parameter in propertySymbol.Parameters)
                    {
                        parametersBuilder.Add(new PolyEnumStructSpec.SlimParameterSpec {
                            name = parameter.Name,
                            type = GetType(parameter.Type, substitutions, targetArgumentNames, token),
                            refKind = parameter.RefKind,
                        });
                    }

                    indexerDef.parameters = parametersBuilder.ToImmutable();

                    if (isReadOnly)
                    {
                        indexerDef.getter.isReadOnly = isReadOnly;
                        indexerDef.setter = default;
                    }

                    indexersBuilder.Add(indexerDef);
                }
                else
                {
                    var propertyDef = new PolyEnumStructSpec.PropertyDeclaration {
                        name = propertySymbol.Name,
                        returnType = GetType(propertySymbol.Type, substitutions, targetArgumentNames, token),
                        refKind = propertySymbol.RefKind,
                        getter = GetPropertyMethod(propertySymbol.GetMethod, token, isGetter: true, getterIsDim),
                        setter = GetPropertyMethod(propertySymbol.SetMethod, token, isGetter: false, setterIsDim),
                        genericInterface = genericInterface,
                    };

                    if (isReadOnly)
                    {
                        propertyDef.getter.isReadOnly = isReadOnly;
                        propertyDef.setter = default;
                    }

                    propertiesBuilder.Add(propertyDef);
                }
            }
            else if (member is IMethodSymbol methodSymbol && methodSymbol.MethodKind == MethodKind.Ordinary)
            {
                if (methodSymbol.TypeParameters.Length > 0)
                {
                    return false;
                }

                var isDim = false;

                foreach (var syntaxRef in methodSymbol.DeclaringSyntaxReferences)
                {
                    token.ThrowIfCancellationRequested();

                    if (syntaxRef.GetSyntax(token) is MethodDeclarationSyntax methodSyntax)
                    {
                        isDim = methodSyntax.Body is not null || methodSyntax.ExpressionBody is not null;
                    }
                }

                var methodDef = new PolyEnumStructSpec.MethodDeclaration {
                    name = methodSymbol.Name,
                    returnType = GetType(methodSymbol.ReturnType, substitutions, targetArgumentNames, token),
                    refKind = methodSymbol.RefKind,
                    returnsVoid = methodSymbol.ReturnsVoid,
                    isReadOnly = methodSymbol.IsReadOnly,
                    isDim = isDim,
                    genericInterface = genericInterface,
                };

                using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.SlimParameterSpec>.Rent();

                foreach (var parameter in methodSymbol.Parameters)
                {
                    token.ThrowIfCancellationRequested();

                    parametersBuilder.Add(new PolyEnumStructSpec.SlimParameterSpec {
                        name = parameter.Name,
                        type = GetType(parameter.Type, substitutions, targetArgumentNames, token),
                        refKind = parameter.RefKind,
                    });
                }

                methodDef.parameters = parametersBuilder.ToImmutable();
                methodsBuilder.Add(methodDef);
            }

            return true;
        }

        private static void GetStructMember(
              ISymbol member
            , bool isReadOnly
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArgumentNames
            , CancellationToken token
            , ImmutableArrayBuilder<PolyEnumStructSpec.PropertyDeclaration> propertiesBuilder
            , ImmutableArrayBuilder<PolyEnumStructSpec.IndexerDeclaration> indexersBuilder
            , ImmutableArrayBuilder<PolyEnumStructSpec.MethodDeclaration> methodsBuilder
        )
        {
            token.ThrowIfCancellationRequested();

            if (member is IPropertySymbol propertySymbol)
            {
                if (propertySymbol.ExplicitInterfaceImplementations.Length > 0)
                {
                    return;
                }

                if (propertySymbol.IsIndexer)
                {
                    var indexerDecl = new PolyEnumStructSpec.IndexerDeclaration {
                        returnType = GetType(propertySymbol.Type, substitutions, targetArgumentNames, token),
                        refKind = propertySymbol.RefKind,
                        getter = GetPropertyMethod(propertySymbol.GetMethod, token, isGetter: true, false),
                    };

                    if (isReadOnly == false && propertySymbol.IsReadOnly == false)
                    {
                        indexerDecl.setter = GetPropertyMethod(propertySymbol.SetMethod, token, isGetter: false, false);
                    }

                    using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.SlimParameterSpec>.Rent();

                    foreach (var parameter in propertySymbol.Parameters)
                    {
                        token.ThrowIfCancellationRequested();

                        parametersBuilder.Add(new PolyEnumStructSpec.SlimParameterSpec {
                            name = parameter.Name,
                            type = GetType(parameter.Type, substitutions, targetArgumentNames, token),
                            refKind = parameter.RefKind,
                        });
                    }

                    indexerDecl.parameters = parametersBuilder.ToImmutable();
                    indexerDecl.genericInterface = HasDependencies(indexerDecl);
                    indexersBuilder.Add(indexerDecl);
                }
                else
                {
                    var propDecl = new PolyEnumStructSpec.PropertyDeclaration {
                        name = propertySymbol.Name,
                        returnType = GetType(propertySymbol.Type, substitutions, targetArgumentNames, token),
                        refKind = propertySymbol.RefKind,
                        getter = GetPropertyMethod(propertySymbol.GetMethod, token, isGetter: true, false),
                    };

                    if (isReadOnly == false && propertySymbol.IsReadOnly == false)
                    {
                        propDecl.setter = GetPropertyMethod(propertySymbol.SetMethod, token, isGetter: false, false);
                    }

                    propDecl.genericInterface = HasDependencies(propDecl);
                    propertiesBuilder.Add(propDecl);
                }
            }
            else if (member is IMethodSymbol methodSymbol && methodSymbol.MethodKind == MethodKind.Ordinary)
            {
                if (methodSymbol.ExplicitInterfaceImplementations.Length > 0
                    || methodSymbol.TypeParameters.Length > 0
                )
                {
                    return;
                }

                var methodDef = new PolyEnumStructSpec.MethodDeclaration {
                    name = methodSymbol.Name,
                    returnType = GetType(methodSymbol.ReturnType, substitutions, targetArgumentNames, token),
                    refKind = methodSymbol.RefKind,
                    returnsVoid = methodSymbol.ReturnsVoid,
                    isReadOnly = methodSymbol.IsReadOnly,
                };

                using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.SlimParameterSpec>.Rent();

                foreach (var parameter in methodSymbol.Parameters)
                {
                    token.ThrowIfCancellationRequested();

                    parametersBuilder.Add(new PolyEnumStructSpec.SlimParameterSpec {
                        name = parameter.Name,
                        type = GetType(parameter.Type, substitutions, targetArgumentNames, token),
                        refKind = parameter.RefKind,
                    });
                }

                methodDef.parameters = parametersBuilder.ToImmutable();
                methodDef.genericInterface = HasDependencies(methodDef);
                methodsBuilder.Add(methodDef);
            }
        }

        private static PolyEnumStructSpec.TypeSpec GetType(
              ITypeSymbol typeSymbol
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArgumentNames
            , CancellationToken token
        )
        {
            return new PolyEnumStructSpec.TypeSpec {
                name = ContainerResolver.FormatType(
                      typeSymbol
                    , substitutions
                    , targetArgumentNames
                    , token
                ),
                identifier = typeSymbol.ToValidIdentifier(),
                dependencies = ContainerResolver.GetTypeDependencies(
                      typeSymbol
                    , substitutions
                    , token
                ).ToImmutableArray().AsEquatableArray(),
                isEnum = typeSymbol.IsEnumType(),
            };
        }

        private static void FillTargetParameters(
              ref PolyEnumStructSpec result
            , ContainerResolution resolution
            , CancellationToken token
        )
        {
            var substitutions = ContainerResolver.CreateTargetMap(resolution, token);
            var targetNames = resolution.TargetParameters.Select(static value => value.Name).ToArray();
            using var builder = ImmutableArrayBuilder<PolyEnumStructSpec.TypeParameterSpec>.Rent();

            foreach (var parameter in resolution.TargetParameters)
            {
                token.ThrowIfCancellationRequested();
                builder.Add(new PolyEnumStructSpec.TypeParameterSpec {
                    name = parameter.Name,
                    constraint = ContainerResolver.FormatConstraintClause(
                          parameter.Symbol
                        , parameter.Name
                        , substitutions
                        , targetNames
                        , token
                    ),
                });
            }

            result.targetParameters = builder.ToImmutable();
            result.genericInterfaceTargetIndices = resolution.GenericInterfaceTargetIndices
                .ToImmutableArray()
                .AsEquatableArray();
        }

        private static string BuildImplicitUndefinedDeclaration(
              in PolyEnumStructSpec polyEnumStruct
            , string name
        )
        {
            if (polyEnumStruct.genericInterfaceTargetIndices.Count < 1)
            {
                return name;
            }

            var parameterNames = new List<string>();

            foreach (var index in polyEnumStruct.genericInterfaceTargetIndices.AsReadOnlySpan())
            {
                parameterNames.Add(polyEnumStruct.targetParameters[index].name);
            }

            return $"{name}<{string.Join(", ", parameterNames)}>";
        }

        private static string BuildMissingTypeParameters(
              in PolyEnumStructSpec polyEnumStruct
            , EquatableArray<int> includedIndices
        )
        {
            var included = new HashSet<int>(includedIndices.AsReadOnlySpan().ToArray());
            var missing = new List<string>();

            for (var index = 0; index < polyEnumStruct.targetParameters.Count; index++)
            {
                if (included.Contains(index) == false)
                {
                    missing.Add(polyEnumStruct.targetParameters[index].name);
                }
            }

            return missing.Count < 1 ? string.Empty : $"<{string.Join(", ", missing)}>";
        }

        private static string BuildMissingTypeConstraints(
              in PolyEnumStructSpec polyEnumStruct
            , EquatableArray<int> includedIndices
        )
        {
            var included = new HashSet<int>(includedIndices.AsReadOnlySpan().ToArray());
            var constraints = new List<string>();

            for (var index = 0; index < polyEnumStruct.targetParameters.Count; index++)
            {
                var constraint = polyEnumStruct.targetParameters[index].constraint;

                if (included.Contains(index) == false && string.IsNullOrEmpty(constraint) == false)
                {
                    constraints.Add(constraint);
                }
            }

            return string.Join("\n", constraints);
        }

        private static string BuildIncludedTypeConstraints(
              in PolyEnumStructSpec polyEnumStruct
            , EquatableArray<int> includedIndices
        )
        {
            var constraints = new List<string>();

            foreach (var index in includedIndices.AsReadOnlySpan())
            {
                var constraint = polyEnumStruct.targetParameters[index].constraint;

                if (string.IsNullOrEmpty(constraint) == false)
                {
                    constraints.Add(constraint);
                }
            }

            return string.Join("\n", constraints);
        }

        private static string BuildTypeParameterList(int[] indices, IReadOnlyList<string> targetArgumentNames)
            => indices.Length < 1
                ? string.Empty
                : $"<{string.Join(", ", indices.Select(index => targetArgumentNames[index]))}>";

        private static string BuildTypeParameterConstraints(
              int[] indices
            , ContainerResolution resolution
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArgumentNames
            , CancellationToken token
        )
        {
            var constraints = new List<string>();

            foreach (var index in indices)
            {
                token.ThrowIfCancellationRequested();
                var parameter = resolution.TargetParameters[index];
                var constraint = ContainerResolver.FormatConstraintClause(
                      parameter.Symbol
                    , targetArgumentNames[index]
                    , substitutions
                    , targetArgumentNames
                    , token
                );

                if (string.IsNullOrEmpty(constraint) == false)
                {
                    constraints.Add(constraint);
                }
            }

            return string.Join("\n", constraints);
        }

        private static bool HasDependencies(in PolyEnumStructSpec.PropertyDeclaration declaration)
            => declaration.returnType.dependencies.Count > 0;

        private static bool HasDependencies(in PolyEnumStructSpec.IndexerDeclaration declaration)
        {
            if (declaration.returnType.dependencies.Count > 0)
            {
                return true;
            }

            foreach (var parameter in declaration.parameters.AsReadOnlySpan())
            {
                if (parameter.type.dependencies.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasDependencies(in PolyEnumStructSpec.MethodDeclaration declaration)
        {
            if (declaration.returnType.dependencies.Count > 0)
            {
                return true;
            }

            foreach (var parameter in declaration.parameters.AsReadOnlySpan())
            {
                if (parameter.type.dependencies.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetCreationValue(ITypeSymbol typeSymbol, object value, CancellationToken token)
        {
            if (typeSymbol.TypeKind == TypeKind.Enum)
            {
                return typeSymbol.GetEnumMemberName(value, token);
            }

            if (typeSymbol.SpecialType == SpecialType.System_String)
            {
                return $"\"{value}\"";
            }

            return value.ToString();
        }

        private static PolyEnumStructSpec.PropertyMethodDeclaration GetPropertyMethod(
              IMethodSymbol methodSymbol
            , CancellationToken token
            , bool isGetter
            , bool isDim
        )
        {
            if (methodSymbol is null)
            {
                return default;
            }

            var isReadOnly = methodSymbol.IsReadOnly;

            if (methodSymbol.TryGetAttribute(READ_ONLY_ATTRIBUTE, out var attrib, token)
                && attrib.ConstructorArguments.Length == 1
            )
            {
                isReadOnly = (bool)attrib.ConstructorArguments[0].Value;
            }

            return new PolyEnumStructSpec.PropertyMethodDeclaration {
                isValid = true,
                refKind = methodSymbol.RefKind,
                isGetter = isGetter,
                isReadOnly = isReadOnly,
                isDim = isDim,
            };
        }
    }
}
