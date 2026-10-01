using EncosyTower.Core.PolyEnumFactories;
using EncosyTower.SourceGen.Helpers.PolyEnumStructs;

namespace EncosyTower.Core.Generators.PolyEnumFactories
{
    [Generator]
    internal sealed class PolyEnumFactoryGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.PolyEnumStructs";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string POLY_ENUM_FACTORY_FOR_ATTRIBUTE_METADATA = $"{NAMESPACE}.PolyEnumFactoryForAttribute";
        private const string POLY_ENUM_STRUCT_ATTRIBUTE = $"global::{NAMESPACE}.PolyEnumStructAttribute";
        private const string ENUM_CASE_IGNORE_ATTRIBUTE = $"global::{NAMESPACE}.EnumCaseIgnoreAttribute";
        private const string UNDEFINED_NAME = "Undefined";
        private const string DEFAULT_FIELD_NAME_PREFIX = "_enumStruct_";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  POLY_ENUM_FACTORY_FOR_ATTRIBUTE_METADATA
                , static (node, _) => IsCandidateNode(node)
                , ExtractSpec
            ).WithTrackingName("PolyEnumFactoryGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("PolyEnumFactoryGenerator.ValidSpecs");

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("PolyEnumFactoryGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static bool IsCandidateNode(SyntaxNode node)
            => node is TypeDeclarationSyntax;

        private static PolyEnumFactorySpec ExtractSpec(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not TypeDeclarationSyntax wrapperSyntax
                || context.TargetSymbol is not INamedTypeSymbol wrapperSymbol
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            var attribute = context.Attributes[0];

            if (attribute.ConstructorArguments.Length < 1
                || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol rawEnumStructSymbol
                || rawEnumStructSymbol.OriginalDefinition.HasAttribute(POLY_ENUM_STRUCT_ATTRIBUTE, token) == false
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var polyEnumAttribute = rawEnumStructSymbol.OriginalDefinition.GetAttribute(
                  POLY_ENUM_STRUCT_ATTRIBUTE
                , token
            );
            var containerResolution = ContainerResolver.Resolve(
                  rawEnumStructSymbol.OriginalDefinition
                , polyEnumAttribute
                , semanticModel.Compilation
                , token
            );

            if (containerResolution.Kind != ContainerResolutionKind.Valid)
            {
                return default;
            }

            var resolution = FactoryTargetResolver.Resolve(
                  rawEnumStructSymbol
                , wrapperSymbol
                , semanticModel.Compilation
                , token
            );

            if (resolution.Kind != ResolutionKind.Valid)
            {
                return default;
            }

            var enumStructSymbol = resolution.Target;

            var canAccessMembers = wrapperSymbol.CanAccessMembersOf(enumStructSymbol, token);

            if (canAccessMembers == CanAccessMembersResult.NoAccess)
            {
                return default;
            }

            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = wrapperSyntax.SyntaxTree;
            var hintName = wrapperSymbol.ToMetadataName();

            var result = new PolyEnumFactorySpec {
                wrapperTypeName = wrapperSymbol.Name,
                wrapperSelfName = wrapperSyntax.Identifier.Text + wrapperSyntax.TypeParameterList,
                wrapperConstraints = wrapperSyntax.ConstraintClauses.ToString(),
                wrapperTypeNamespace = wrapperSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                wrapperKindKeyword = GetWrapperKindKeyword(wrapperSyntax),
                wrapperPreModifiers = GetWrapperPreModifiers(wrapperSyntax, token),
                wrapperAccessibility = GetAccessibilityKeyword(wrapperSymbol.DeclaredAccessibility),
                enumStructTypeName = SelectEnumStructTypeName(enumStructSymbol, canAccessMembers),
                enumStructNamespace = enumStructSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                enumStructIsReadOnly = enumStructSymbol.IsReadOnly,
                enumStructSize = 0,
                hintName = hintName,
                parentIsNamespace = wrapperSyntax.Parent is BaseNamespaceDeclarationSyntax,
                isStruct = IsStruct(wrapperSyntax),
            };
            result.supportTypeName = containerResolution.SupportTypeName;
            result.enumCaseTypeName = $"{result.supportTypeName}.EnumCase";
            result.separateTypeContainer = ContainerResolver.GetEffectiveTypeArguments(wrapperSymbol, token)
                .Any(static argument => argument.TypeKind == TypeKind.TypeParameter);

            if (result.separateTypeContainer)
            {
                result.typeContainer = FactoryOutputScopeSpecFactory.CreateCompanion(wrapperSymbol, token);
                result.wrapperOutputScope = FactoryOutputScopeSpecFactory.CreateSource(wrapperSymbol, token);
            }
            else
            {
                TypeCreationHelpers.GenerateOpeningAndClosingSource(
                      wrapperSyntax
                    , token
                    , out result.openingSource
                    , out result.closingSource
                    , printAdditionalUsings: PrintAdditionalUsings
                );
            }

            ResolveBackingField(wrapperSyntax, wrapperSymbol, enumStructSymbol, ref result, token);
            AggregateCases(
                  enumStructSymbol
                , containerResolution
                , canAccessMembers
                , ref result
                , token
            );
            ResolveExplicitUndefinedMethod(ref result, token);

            return result;
        }

        private static void ResolveBackingField(
              TypeDeclarationSyntax wrapperSyntax
            , INamedTypeSymbol wrapperSymbol
            , INamedTypeSymbol enumStructSymbol
            , ref PolyEnumFactorySpec result
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (wrapperSyntax is RecordDeclarationSyntax recordSyntax
                && recordSyntax.ParameterList is ParameterListSyntax paramListSyntax
                && paramListSyntax.Parameters.Count > 0
            )
            {
                var param = paramListSyntax.Parameters[0];

                if (param.Type is TypeSyntax type && type.IsTypeNameCandidate(result.enumStructTypeName, token))
                {
                    result.emitBackingField = false;
                    result.fieldName = param.Identifier.Text;
                    return;
                }
            }

            var comparer = SymbolEqualityComparer.Default;

            foreach (var ctor in wrapperSymbol.InstanceConstructors)
            {
                token.ThrowIfCancellationRequested();

                if (ctor.Parameters.Length < 1)
                {
                    continue;
                }

                var paramType = ctor.Parameters[0].Type;

                if (comparer.Equals(paramType, enumStructSymbol))
                {
                    result.emitBackingField = false;
                    result.fieldName = GetUserDefinedBackingField(wrapperSymbol, enumStructSymbol, token);
                    return;
                }
            }

            result.emitBackingField = true;

            token.ThrowIfCancellationRequested();

            var baseName = $"{DEFAULT_FIELD_NAME_PREFIX}{enumStructSymbol.Name}";
            var fieldName = baseName;
            var suffix = 1;

            while (HasInstanceField(wrapperSymbol, fieldName, token))
            {
                token.ThrowIfCancellationRequested();

                fieldName = $"{baseName}{suffix}";
                suffix++;
            }

            result.fieldName = fieldName;
            return;

            static string GetUserDefinedBackingField(
                  INamedTypeSymbol wrapperSymbol
                , INamedTypeSymbol enumStructSymbol
                , CancellationToken token
            )
            {
                var comparer = SymbolEqualityComparer.Default;

                foreach (var member in wrapperSymbol.GetMembers())
                {
                    token.ThrowIfCancellationRequested();

                    if (member is IFieldSymbol field
                        && field.IsStatic == false
                        && comparer.Equals(field.Type, enumStructSymbol)
                    )
                    {
                        return field.Name;
                    }
                }

                return string.Empty;
            }
        }

        private static bool HasInstanceField(INamedTypeSymbol typeSymbol, string name, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var member in typeSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is IFieldSymbol field
                    && field.IsStatic == false
                    && string.Equals(field.Name, name, StringComparison.Ordinal)
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static void AggregateCases(
              INamedTypeSymbol enumStructSymbol
            , ContainerResolution resolution
            , CanAccessMembersResult access
            , ref PolyEnumFactorySpec result
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            using var casesBuilder = ImmutableArrayBuilder<PolyEnumFactorySpec.CaseSpec>.Rent();

            var verboseUndefined = $"{enumStructSymbol.Name}_Undefined";
            var targetArguments = ContainerResolver.GetEffectiveTypeArguments(enumStructSymbol, token)
                .Select(static argument => argument.ToFullName())
                .ToArray();

            foreach (var @case in resolution.Cases)
            {
                token.ThrowIfCancellationRequested();
                var nested = @case.Symbol;

                if (nested.TypeKind != TypeKind.Struct)
                {
                    continue;
                }

                if (nested.HasAttribute(ENUM_CASE_IGNORE_ATTRIBUTE, token))
                {
                    continue;
                }

                var substitutions = ContainerResolver.CreateTargetMap(resolution, @case, token);
                var qualifiedName = BuildQualifiedCaseName(
                      enumStructSymbol
                    , @case
                    , result.supportTypeName
                    , access
                    , targetArguments
                    , token
                );
                var caseSpec = BuildCaseSpec(
                      enumStructSymbol
                    , nested
                    , access
                    , verboseUndefined
                    , qualifiedName
                    , substitutions
                    , targetArguments
                    , token
                );

                if (caseSpec.IsValid == false)
                {
                    continue;
                }

                casesBuilder.Add(caseSpec);
            }

            result.cases = casesBuilder.ToImmutable().AsEquatableArray();
        }

        private static PolyEnumFactorySpec.CaseSpec BuildCaseSpec(
              INamedTypeSymbol enumStructSymbol
            , INamedTypeSymbol caseSymbol
            , CanAccessMembersResult access
            , string verboseUndefined
            , string qualifiedName
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArguments
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var isUndefined = string.Equals(caseSymbol.Name, UNDEFINED_NAME, StringComparison.Ordinal)
                || string.Equals(caseSymbol.Name, verboseUndefined, StringComparison.Ordinal);

            var spec = new PolyEnumFactorySpec.CaseSpec {
                name = caseSymbol.Name,
                identifier = caseSymbol.Name,
                qualifiedName = qualifiedName,
                isUndefined = isUndefined,
                isReadOnly = caseSymbol.IsReadOnly,
                size = 0,
                strategy = PolyEnumFactorySpec.ConstructionStrategy.Default,
                ctors = default,
                initMembers = default,
            };

            if (TryBuildCtorList(caseSymbol, substitutions, targetArguments, token, out var ctors))
            {
                spec.strategy = PolyEnumFactorySpec.ConstructionStrategy.Ctors;
                spec.ctors = ctors;

                if (TryBuildMemberInitList(
                      caseSymbol
                    , substitutions
                    , targetArguments
                    , token
                    , out var settableMembers
                ))
                {
                    var maxCtorParamCount = 0;

                    for (var i = 0; i < ctors.Count; i++)
                    {
                        token.ThrowIfCancellationRequested();

                        var count = ctors[i].parameters.Count;

                        if (count > maxCtorParamCount)
                        {
                            maxCtorParamCount = count;
                        }
                    }

                    if (maxCtorParamCount < settableMembers.Count)
                    {
                        spec.initMembers = settableMembers;
                        spec.emitMemberInitOverload = true;
                    }
                }

                return spec;
            }

            if (TryBuildGeneratedFieldCtor(
                  caseSymbol
                , substitutions
                , targetArguments
                , token
                , out var generatedCtor
            ))
            {
                spec.strategy = PolyEnumFactorySpec.ConstructionStrategy.Ctors;
                spec.ctors = ImmutableArray.Create(generatedCtor).AsEquatableArray();
                return spec;
            }

            if (TryBuildMemberInitList(caseSymbol, substitutions, targetArguments, token, out var members))
            {
                spec.strategy = PolyEnumFactorySpec.ConstructionStrategy.MemberInit;
                spec.initMembers = members;
                return spec;
            }

            spec.strategy = PolyEnumFactorySpec.ConstructionStrategy.Default;
            return spec;
        }

        private static bool TryBuildGeneratedFieldCtor(
              INamedTypeSymbol caseSymbol
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArguments
            , CancellationToken token
            , out PolyEnumFactorySpec.CtorSpec result
        )
        {
            using var builder = ImmutableArrayBuilder<PolyEnumFactorySpec.ParamSpec>.Rent();

            foreach (var member in caseSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IFieldSymbol field
                    || field.IsStatic
                    || field.IsConst
                    || field.IsImplicitlyDeclared
                )
                {
                    continue;
                }

                var fieldType = ContainerResolver.FormatType(
                      field.Type
                    , substitutions
                    , targetArguments
                    , token
                );
                builder.Add(new PolyEnumFactorySpec.ParamSpec {
                    name = field.Name,
                    typeFullyQualifiedName = $"global::EncosyTower.Common.Option<{fieldType}>",
                    refKind = RefKind.None,
                    isParams = false,
                    hasExplicitDefaultValue = true,
                    defaultValueLiteral = "default",
                });
            }

            if (builder.Count == 0)
            {
                result = default;
                return false;
            }

            result = new PolyEnumFactorySpec.CtorSpec {
                parameters = builder.ToImmutable().AsEquatableArray(),
                isParameterless = false,
            };
            return true;
        }

        private static bool TryBuildCtorList(
              INamedTypeSymbol caseSymbol
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArguments
            , CancellationToken token
            , out EquatableArray<PolyEnumFactorySpec.CtorSpec> result
        )
        {
            using var builder = ImmutableArrayBuilder<PolyEnumFactorySpec.CtorSpec>.Rent();

            foreach (var ctor in caseSymbol.InstanceConstructors)
            {
                token.ThrowIfCancellationRequested();

                if (IsSupportedAccessibility(ctor.DeclaredAccessibility) == false)
                {
                    continue;
                }

                if (ctor.IsImplicitlyDeclared)
                {
                    if (ctor.Parameters.Length < 1)
                    {
                        continue;
                    }
                }

                if (HasOutParameter(ctor, token))
                {
                    continue;
                }

                var paramArray = BuildParamSpecs(ctor, substitutions, targetArguments, token);

                builder.Add(new PolyEnumFactorySpec.CtorSpec {
                    parameters = paramArray,
                    isParameterless = ctor.Parameters.Length == 0,
                });
            }

            if (builder.Count < 1)
            {
                result = default;
                return false;
            }

            result = builder.ToImmutable().AsEquatableArray();
            return true;
        }

        private static bool HasOutParameter(IMethodSymbol ctor, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var p in ctor.Parameters)
            {
                token.ThrowIfCancellationRequested();

                if (p.RefKind == RefKind.Out)
                {
                    return true;
                }
            }

            return false;
        }

        private static EquatableArray<PolyEnumFactorySpec.ParamSpec> BuildParamSpecs(
              IMethodSymbol ctor
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArguments
            , CancellationToken token
        )
        {
            using var builder = ImmutableArrayBuilder<PolyEnumFactorySpec.ParamSpec>.Rent();

            foreach (var p in ctor.Parameters)
            {
                token.ThrowIfCancellationRequested();

                var hasDefault = p.HasExplicitDefaultValue && p.RefKind == RefKind.None;
                var literal = hasDefault ? FormatDefaultValue(p) : null;
                var paramName = NameCasing.Camel.ConvertName(p.Name);

                builder.Add(new PolyEnumFactorySpec.ParamSpec {
                    name = paramName,
                    typeFullyQualifiedName = ContainerResolver.FormatType(
                          p.Type
                        , substitutions
                        , targetArguments
                        , token
                    ),
                    refKind = p.RefKind,
                    isParams = p.IsParams,
                    hasExplicitDefaultValue = hasDefault,
                    defaultValueLiteral = literal,
                });
            }

            return builder.ToImmutable().AsEquatableArray();
        }

        private static bool TryBuildMemberInitList(
              INamedTypeSymbol caseSymbol
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArguments
            , CancellationToken token
            , out EquatableArray<PolyEnumFactorySpec.MemberSpec> result
        )
        {
            using var builder = ImmutableArrayBuilder<PolyEnumFactorySpec.MemberSpec>.Rent();
            var seenNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var member in caseSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member.IsStatic || member.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (IsSupportedAccessibility(member.DeclaredAccessibility) == false)
                {
                    continue;
                }

                if (member is IFieldSymbol field)
                {
                    if (field.IsConst || field.IsReadOnly)
                    {
                        continue;
                    }

                    if (seenNames.Add(field.Name) == false)
                    {
                        continue;
                    }

                    builder.Add(new PolyEnumFactorySpec.MemberSpec {
                        name = field.Name,
                        parameterName = NameCasing.Camel.ConvertName(field.Name),
                        typeFullyQualifiedName = ContainerResolver.FormatType(
                              field.Type
                            , substitutions
                            , targetArguments
                            , token
                        ),
                        isProperty = false,
                    });
                }
                else if (member is IPropertySymbol property)
                {
                    if (property.IsIndexer)
                    {
                        continue;
                    }

                    var setter = property.SetMethod;

                    if (setter is null || IsSupportedAccessibility(setter.DeclaredAccessibility) == false)
                    {
                        continue;
                    }

                    if (seenNames.Add(property.Name) == false)
                    {
                        continue;
                    }

                    builder.Add(new PolyEnumFactorySpec.MemberSpec {
                        name = property.Name,
                        parameterName = NameCasing.Camel.ConvertName(property.Name),
                        typeFullyQualifiedName = ContainerResolver.FormatType(
                              property.Type
                            , substitutions
                            , targetArguments
                            , token
                        ),
                        isProperty = true,
                    });
                }
            }

            if (builder.Count < 1)
            {
                result = default;
                return false;
            }

            result = builder.ToImmutable().AsEquatableArray();
            return true;
        }

        private static string BuildQualifiedCaseName(
              INamedTypeSymbol enumStructSymbol
            , CaseResolution @case
            , string supportTypeName
            , CanAccessMembersResult access
            , IReadOnlyList<string> targetArguments
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (@case.Owner == CaseOwner.Container)
            {
                if (@case.AuthoredTargetIndices.Length < 1)
                {
                    return $"{supportTypeName}.{@case.Symbol.Name}";
                }

                var arguments = new List<string>();

                foreach (var index in @case.AuthoredTargetIndices)
                {
                    token.ThrowIfCancellationRequested();
                    arguments.Add(targetArguments[index]);
                }

                return $"{supportTypeName}.{@case.Symbol.Name}<{string.Join(", ", arguments)}>";
            }

            var nested = enumStructSymbol.GetTypeMembers(@case.Symbol.Name, @case.Symbol.Arity)
                .FirstOrDefault();
            return nested is null
                ? string.Empty
                : SelectCaseStructTypeName(enumStructSymbol, nested, access);
        }

        private static void ResolveExplicitUndefinedMethod(ref PolyEnumFactorySpec result, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var cases = result.cases;
            var hasParameterlessUndefined = false;

            for (var i = 0; i < cases.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                var c = cases[i];

                if (c.isUndefined == false)
                {
                    continue;
                }

                switch (c.strategy)
                {
                    case PolyEnumFactorySpec.ConstructionStrategy.Default:
                        hasParameterlessUndefined = true;
                        break;

                    case PolyEnumFactorySpec.ConstructionStrategy.Ctors:
                    {
                        var ctors = c.ctors;
                        for (var j = 0; j < ctors.Count; j++)
                        {
                            if (ctors[j].isParameterless)
                            {
                                hasParameterlessUndefined = true;
                                break;
                            }
                        }

                        break;
                    }
                }

                if (hasParameterlessUndefined)
                {
                    break;
                }
            }

            result.emitExplicitUndefinedMethod = hasParameterlessUndefined == false;
        }

        private static bool IsStruct(TypeDeclarationSyntax typeSyntax)
        {
            return typeSyntax is RecordDeclarationSyntax record
                ? record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)
                : typeSyntax is StructDeclarationSyntax;
        }

        private static bool IsSupportedAccessibility(Accessibility accessibility)
        {
            return accessibility >= Accessibility.Internal;
        }

        private static string GetWrapperKindKeyword(TypeDeclarationSyntax typeSyntax)
        {
            if (typeSyntax is RecordDeclarationSyntax record)
            {
                return record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) ? "record struct" : "record class";
            }

            if (typeSyntax is StructDeclarationSyntax)
            {
                return "struct";
            }

            if (typeSyntax is ClassDeclarationSyntax)
            {
                return "class";
            }

            return string.Empty;
        }

        private static string GetWrapperPreModifiers(TypeDeclarationSyntax typeSyntax, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var sb = new StringBuilder();

            foreach (var modifier in typeSyntax.Modifiers)
            {
                token.ThrowIfCancellationRequested();

                if (modifier.IsKind(SyntaxKind.PartialKeyword))
                {
                    continue;
                }

                if (IsAccessibilityModifier(modifier))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(modifier.ValueText);
            }

            return sb.ToString();
        }

        private static string SelectEnumStructTypeName(INamedTypeSymbol type, CanAccessMembersResult access)
        {
            switch (access)
            {
                case CanAccessMembersResult.EnclosingType:
                case CanAccessMembersResult.SameAssemblyInheritance:
                case CanAccessMembersResult.CrossAssemblyInheritance:
                    return type.Name;

                default:
                    return type.ToFullName();
            }
        }

        private static string SelectCaseStructTypeName(
              INamedTypeSymbol enumStructType
            , INamedTypeSymbol caseStructType
            , CanAccessMembersResult access
        )
        {
            switch (access)
            {
                case CanAccessMembersResult.EnclosingType:
                case CanAccessMembersResult.SameAssemblyInheritance:
                case CanAccessMembersResult.CrossAssemblyInheritance:
                    return $"{enumStructType.Name}.{caseStructType.Name}";

                default:
                    return caseStructType.ToFullName();
            }
        }

        private static bool IsAccessibilityModifier(SyntaxToken token)
            => token.IsKind(SyntaxKind.PublicKeyword)
            || token.IsKind(SyntaxKind.PrivateKeyword)
            || token.IsKind(SyntaxKind.ProtectedKeyword)
            || token.IsKind(SyntaxKind.InternalKeyword)
            ;

        private static string GetAccessibilityKeyword(Accessibility accessibility)
        {
            return accessibility switch {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                Accessibility.Private => "private",
                Accessibility.Protected => "protected",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.ProtectedAndInternal => "private protected",
                _ => string.Empty,
            };
        }

        private static string FormatDefaultValue(IParameterSymbol parameter)
        {
            var value = parameter.ExplicitDefaultValue;
            var type = parameter.Type;

            if (value is null)
            {
                if (type.IsReferenceType || type.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    return "null";
                }

                return $"default({type.ToFullName()})";
            }

            if (type.TypeKind == TypeKind.Enum && type is INamedTypeSymbol enumType)
            {
                return $"({enumType.ToFullName()})({FormatPrimitive(value)})";
            }

            return FormatPrimitive(value);
        }

        private static string FormatPrimitive(object value)
        {
            return value switch {
                null => "null",
                bool b => b ? "true" : "false",
                string s => $"\"{StringExtensions.EscapeStringLiteral(s)}\"",
                char c => FormatCharLiteral(c),
                float f => $"{f.ToString("R", System.Globalization.CultureInfo.InvariantCulture)}f",
                double d => $"{d.ToString("R", System.Globalization.CultureInfo.InvariantCulture)}d",
                decimal m => $"{m.ToString(System.Globalization.CultureInfo.InvariantCulture)}m",
                long l => $"{l.ToString(System.Globalization.CultureInfo.InvariantCulture)}L",
                ulong ul => $"{ul.ToString(System.Globalization.CultureInfo.InvariantCulture)}UL",
                uint ui => $"{ui.ToString(System.Globalization.CultureInfo.InvariantCulture)}U",
                int i => i.ToString(System.Globalization.CultureInfo.InvariantCulture),
                short sh => $"(short){sh.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                ushort ush => $"(ushort){ush.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                byte by => $"(byte){by.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                sbyte sb => $"(sbyte){sb.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                _ => value.ToString(),
            };
        }

        private static string FormatCharLiteral(char c)
        {
            switch (c)
            {
                case '\\': return "'\\\\'";
                case '\'': return "'\\''";
                case '\0': return "'\\0'";
                case '\a': return "'\\a'";
                case '\b': return "'\\b'";
                case '\f': return "'\\f'";
                case '\n': return "'\\n'";
                case '\r': return "'\\r'";
                case '\t': return "'\\t'";
                case '\v': return "'\\v'";
                default:
                    if (char.IsControl(c) || c > 127)
                    {
                        return $"'\\u{((int)c).ToString("x4", System.Globalization.CultureInfo.InvariantCulture)}'";
                    }

                    return $"'{c}'";
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , PolyEnumFactorySpec candidate
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
                  "EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator"
                , assemblyName
                , candidate.hintName
                , "PolyEnumFactory"
                , string.Empty
            );

            Microsoft.CodeAnalysis.Text.SourceText generatedSource;

            if (candidate.separateTypeContainer)
            {
                var wrapperPrinter = new Printer(0, 1024 * 64, context.CancellationToken);
                candidate.wrapperOutputScope.WriteOpening(
                      ref wrapperPrinter
                    , PrintAdditionalUsings
                    , context.CancellationToken
                );
                wrapperPrinter.Print(candidate.WriteCode(compilation, context.CancellationToken));
                wrapperPrinter.PrintEndLine();
                candidate.wrapperOutputScope.WriteClosing(ref wrapperPrinter, context.CancellationToken);
                generatedSource = Microsoft.CodeAnalysis.Text.SourceText.From(
                      wrapperPrinter.Result.TrimEnd('\n') + "\n"
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

            if (candidate.separateTypeContainer == false)
            {
                return;
            }

            var typeHintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator"
                , assemblyName
                , candidate.hintName
                , "PolyEnumFactoryContainer"
                , candidate.typeContainer.MetadataName
            );
            var typePrinter = new Printer(0, 1024 * 32, context.CancellationToken);
            candidate.typeContainer.WriteOpening(
                  ref typePrinter
                , PrintAdditionalUsings
                , context.CancellationToken
            );
            candidate.WriteTypeCode(ref typePrinter, context.CancellationToken);
            candidate.typeContainer.WriteClosing(ref typePrinter, context.CancellationToken);
            var typeSource = Microsoft.CodeAnalysis.Text.SourceText.From(typePrinter.Result, Encoding.UTF8)
                .WithIgnoreUnassignedVariableWarning();
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(typeHintName, typeSource);
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__ETCol = global::EncosyTower.Collections;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
