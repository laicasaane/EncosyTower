using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Helpers.PolyEnumStructs
{
    public enum ContainerResolutionKind
    {
        Invalid,
        Valid,
        GenericContainer,
        CrossAssemblyContainer,
        InvalidContainerKind,
        ParameterMapping,
        ConstraintMismatch,
        MissingInterfaceDependency,
        InterfaceInsideTarget,
        InterfaceDependency,
        DuplicateInterfaceMember,
        DuplicateUndefined,
    }

    public enum CaseOwner
    {
        Target,
        Container,
    }

    public sealed class ContainerResolution
    {
        public ContainerResolutionKind Kind { get; set; }

        public INamedTypeSymbol Target { get; set; }

        public INamedTypeSymbol Container { get; set; }

        public INamedTypeSymbol InvalidType { get; set; }

        public INamedTypeSymbol BaseInterface { get; set; }

        public INamedTypeSymbol GenericInterface { get; set; }

        public ITypeParameterSymbol MissingParameter { get; set; }

        public string MemberName { get; set; }

        public bool UsesSeparateContainer { get; set; }

        public string SupportTypeName { get; set; }

        public string SupportMetadataName { get; set; }

        public List<TargetParameter> TargetParameters { get; } = new();

        public List<CaseResolution> Cases { get; } = new();

        public int[] GenericInterfaceTargetIndices { get; set; } = Array.Empty<int>();
    }

    public readonly record struct TargetParameter(
          int Index
        , ITypeParameterSymbol Symbol
        , string SourceName
        , string Name
    );

    public sealed class CaseResolution
    {
        public INamedTypeSymbol Symbol { get; set; }

        public CaseOwner Owner { get; set; }

        public int[] AuthoredTargetIndices { get; set; } = Array.Empty<int>();

        public int[] TargetOrderedIndices { get; set; } = Array.Empty<int>();

        public bool IsUndefined { get; set; }
    }

    public static class ContainerResolver
    {
        private const string POLY_ENUM_STRUCT_ATTRIBUTE =
            "global::EncosyTower.PolyEnumStructs.PolyEnumStructAttribute";

        private const string ENUM_CASE_IGNORE_ATTRIBUTE =
            "global::EncosyTower.PolyEnumStructs.EnumCaseIgnoreAttribute";

        private const string INTERFACE_NAME = "IEnumCase";
        private const string UNDEFINED_NAME = "Undefined";

        public static ContainerResolution Resolve(
              INamedTypeSymbol target
            , AttributeData attribute
            , Compilation compilation
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (target is null || attribute is null)
            {
                return new ContainerResolution { Kind = ContainerResolutionKind.Invalid };
            }

            target = target.OriginalDefinition;
            var result = new ContainerResolution {
                Kind = ContainerResolutionKind.Valid,
                Target = target,
            };

            FillTargetParameters(target, result.TargetParameters, token);

            var container = GetContainer(attribute, token);
            result.Container = container;
            result.UsesSeparateContainer = container is not null || result.TargetParameters.Count > 0;

            if (container is not null)
            {
                if (IsEffectivelyNonGeneric(container, token) == false)
                {
                    result.Kind = ContainerResolutionKind.GenericContainer;
                    result.InvalidType = container;
                    return result;
                }

                if (SymbolEqualityComparer.Default.Equals(
                      container.ContainingAssembly
                    , target.ContainingAssembly
                ) == false)
                {
                    result.Kind = ContainerResolutionKind.CrossAssemblyContainer;
                    result.InvalidType = container;
                    return result;
                }

                if (container.TypeKind is not (TypeKind.Class or TypeKind.Struct or TypeKind.Interface))
                {
                    result.Kind = ContainerResolutionKind.InvalidContainerKind;
                    result.InvalidType = container;
                    return result;
                }
            }

            if (result.UsesSeparateContainer)
            {
                if (container is null)
                {
                    BuildDefaultContainerIdentity(
                          target
                        , out var supportTypeName
                        , out var supportMetadataName
                        , token
                    );
                    result.SupportTypeName = supportTypeName;
                    result.SupportMetadataName = supportMetadataName;
                }
                else
                {
                    result.SupportTypeName = container.ToFullName();
                    result.SupportMetadataName = container.ToMetadataName();
                }
            }
            else
            {
                result.SupportTypeName = target.ToFullName();
                result.SupportMetadataName = target.ToMetadataName();
            }

            var undefinedCount = 0;

            if (AddCases(target, CaseOwner.Target, result, compilation, ref undefinedCount, token) == false)
            {
                return result;
            }

            if (container is not null
                && SymbolEqualityComparer.Default.Equals(container, target) == false
                && AddCases(container, CaseOwner.Container, result, compilation, ref undefinedCount, token) == false
            )
            {
                return result;
            }

            if (undefinedCount > 1)
            {
                result.Kind = ContainerResolutionKind.DuplicateUndefined;
                return result;
            }

            if (ResolveInterfaces(result, compilation, token) == false)
            {
                return result;
            }

            return result;
        }

        public static string FormatType(
              ITypeSymbol type
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArgumentNames
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var parts = type.ToDisplayParts(SymbolDisplayFormat.FullyQualifiedFormat);
            var builder = new StringBuilder();

            foreach (var part in parts)
            {
                token.ThrowIfCancellationRequested();

                if (part.Symbol is ITypeParameterSymbol parameter
                    && substitutions.TryGetValue(parameter, out var targetIndex)
                )
                {
                    builder.Append(targetArgumentNames[targetIndex]);
                }
                else
                {
                    builder.Append(part.ToString());
                }
            }

            return builder.ToString();
        }

        public static int[] GetTypeDependencies(
              ITypeSymbol type
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var dependencies = new HashSet<int>();
            AddTypeDependencies(type, substitutions, dependencies, token);
            return ToSortedArray(dependencies, token);
        }

        public static Dictionary<ITypeParameterSymbol, int> CreateTargetMap(
              ContainerResolution resolution
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            return CreateTargetMap(resolution, @case: null, token);
        }

        public static Dictionary<ITypeParameterSymbol, int> CreateTargetMap(
              ContainerResolution resolution
            , CaseResolution @case
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var result = new Dictionary<ITypeParameterSymbol, int>(SymbolEqualityComparer.Default);

            foreach (var parameter in resolution.TargetParameters)
            {
                token.ThrowIfCancellationRequested();
                result[parameter.Symbol] = parameter.Index;
            }

            if (@case is not null)
            {
                var localParameters = @case.Symbol.TypeParameters;

                for (var i = 0; i < localParameters.Length; i++)
                {
                    token.ThrowIfCancellationRequested();
                    result[localParameters[i]] = @case.AuthoredTargetIndices[i];
                }
            }

            return result;
        }

        public static string FormatConstraintClause(
              ITypeParameterSymbol parameter
            , string parameterName
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , IReadOnlyList<string> targetArgumentNames
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var constraints = new List<string>();

            if (parameter.HasUnmanagedTypeConstraint)
            {
                constraints.Add("unmanaged");
            }
            else if (parameter.HasValueTypeConstraint)
            {
                constraints.Add("struct");
            }
            else if (parameter.HasReferenceTypeConstraint)
            {
                constraints.Add(parameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
                    ? "class?"
                    : "class");
            }
            else if (parameter.HasNotNullConstraint)
            {
                constraints.Add("notnull");
            }

            foreach (var constraintType in parameter.ConstraintTypes)
            {
                token.ThrowIfCancellationRequested();
                constraints.Add(FormatType(constraintType, substitutions, targetArgumentNames, token));
            }

            if (parameter.HasConstructorConstraint && parameter.HasValueTypeConstraint == false)
            {
                constraints.Add("new()");
            }

            return constraints.Count < 1
                ? string.Empty
                : $"where {parameterName} : {string.Join(", ", constraints)}";
        }

        /// <summary>
        /// Gets the constraint clauses of the type parameters <paramref name="type"/> declares, with every
        /// constraint type fully qualified, one clause per line.
        /// </summary>
        public static string FormatConstraintClauses(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var substitutions = new Dictionary<ITypeParameterSymbol, int>(SymbolEqualityComparer.Default);
            var clauses = new List<string>();

            foreach (var parameter in type.TypeParameters)
            {
                token.ThrowIfCancellationRequested();

                var clause = FormatConstraintClause(
                      parameter
                    , parameter.Name
                    , substitutions
                    , Array.Empty<string>()
                    , token
                );

                if (string.IsNullOrEmpty(clause) == false)
                {
                    clauses.Add(clause);
                }
            }

            return string.Join("\n", clauses);
        }

        public static List<ITypeSymbol> GetEffectiveTypeArguments(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var chain = GetTypeChain(type, token);
            var result = new List<ITypeSymbol>();

            foreach (var current in chain)
            {
                token.ThrowIfCancellationRequested();
                result.AddRange(current.TypeArguments);
            }

            return result;
        }

        private static INamedTypeSymbol GetContainer(AttributeData attribute, CancellationToken token)
        {
            foreach (var argument in attribute.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

                if (string.Equals(argument.Key, "Container", StringComparison.Ordinal)
                    && argument.Value.Value is INamedTypeSymbol container
                )
                {
                    return container.OriginalDefinition;
                }
            }

            return null;
        }

        private static bool AddCases(
              INamedTypeSymbol owner
            , CaseOwner caseOwner
            , ContainerResolution result
            , Compilation compilation
            , ref int undefinedCount
            , CancellationToken token
        )
        {
            var targetName = result.Target.Name;
            var verboseUndefinedName = $"{targetName}_{UNDEFINED_NAME}";

            foreach (var nested in owner.GetTypeMembers())
            {
                token.ThrowIfCancellationRequested();

                if (nested.TypeKind != TypeKind.Struct
                    || nested.HasAttribute(ENUM_CASE_IGNORE_ATTRIBUTE, token)
                )
                {
                    continue;
                }

                int[] authoredIndices;

                if (caseOwner == CaseOwner.Target)
                {
                    if (nested.Arity > 0)
                    {
                        result.Kind = ContainerResolutionKind.ParameterMapping;
                        result.InvalidType = nested;
                        return false;
                    }

                    authoredIndices = Enumerable.Range(0, result.TargetParameters.Count).ToArray();
                }
                else if (TryMapParameters(
                      nested
                    , result.TargetParameters
                    , compilation
                    , out authoredIndices
                    , out var constraintMismatch
                    , token
                ) == false)
                {
                    result.Kind = constraintMismatch
                        ? ContainerResolutionKind.ConstraintMismatch
                        : ContainerResolutionKind.ParameterMapping;
                    result.InvalidType = nested;
                    return false;
                }

                var isUndefined = string.Equals(nested.Name, UNDEFINED_NAME, StringComparison.Ordinal)
                    || string.Equals(nested.Name, verboseUndefinedName, StringComparison.Ordinal);

                if (isUndefined)
                {
                    undefinedCount++;
                    result.InvalidType = nested;
                }

                result.Cases.Add(new CaseResolution {
                    Symbol = nested,
                    Owner = caseOwner,
                    AuthoredTargetIndices = authoredIndices,
                    TargetOrderedIndices = ToSortedArray(authoredIndices, token),
                    IsUndefined = isUndefined,
                });
            }

            return true;
        }

        private static bool ResolveInterfaces(
              ContainerResolution result
            , Compilation compilation
            , CancellationToken token
        )
        {
            var targetInterfaces = result.Target.GetTypeMembers(INTERFACE_NAME);

            if (result.UsesSeparateContainer && targetInterfaces.Length > 0)
            {
                result.Kind = ContainerResolutionKind.InterfaceInsideTarget;
                result.InvalidType = targetInterfaces[0];
                return false;
            }

            var interfaceOwner = result.UsesSeparateContainer
                ? result.Container
                : result.Target;

            if (interfaceOwner is not null)
            {
                foreach (var candidate in interfaceOwner.GetTypeMembers(INTERFACE_NAME))
                {
                    token.ThrowIfCancellationRequested();

                    if (candidate.TypeKind != TypeKind.Interface)
                    {
                        continue;
                    }

                    if (candidate.Arity < 1)
                    {
                        result.BaseInterface ??= candidate;
                        continue;
                    }

                    if (result.GenericInterface is not null)
                    {
                        result.Kind = ContainerResolutionKind.ParameterMapping;
                        result.InvalidType = candidate;
                        return false;
                    }

                    if (TryMapParameters(
                          candidate
                        , result.TargetParameters
                        , compilation
                        , out var mappedIndices
                        , out var constraintMismatch
                        , token
                    ) == false)
                    {
                        result.Kind = constraintMismatch
                            ? ContainerResolutionKind.ConstraintMismatch
                            : ContainerResolutionKind.ParameterMapping;
                        result.InvalidType = candidate;
                        return false;
                    }

                    result.GenericInterface = candidate;
                    result.GenericInterfaceTargetIndices = mappedIndices;
                }
            }

            if (HaveDuplicateMembers(result, token, out var duplicateMember))
            {
                result.Kind = ContainerResolutionKind.DuplicateInterfaceMember;
                result.MemberName = duplicateMember;
                result.InvalidType = result.GenericInterface;
                return false;
            }

            var automaticDependencies = GetAutomaticInterfaceDependencies(result, token, out var memberName);

            if (result.GenericInterface is not null)
            {
                var genericSet = new HashSet<int>(result.GenericInterfaceTargetIndices);

                foreach (var dependency in GetMemberDependencies(result.GenericInterface, result, token))
                {
                    if (genericSet.Contains(dependency))
                    {
                        continue;
                    }

                    result.Kind = ContainerResolutionKind.InterfaceDependency;
                    result.MissingParameter = result.TargetParameters[dependency].Symbol;
                    result.MemberName = memberName;
                    result.InvalidType = result.GenericInterface;
                    return false;
                }

                foreach (var dependency in automaticDependencies)
                {
                    if (genericSet.Contains(dependency))
                    {
                        continue;
                    }

                    result.Kind = ContainerResolutionKind.InterfaceDependency;
                    result.MissingParameter = result.TargetParameters[dependency].Symbol;
                    result.MemberName = memberName;
                    result.InvalidType = result.GenericInterface;
                    return false;
                }
            }
            else
            {
                result.GenericInterfaceTargetIndices = automaticDependencies;
            }

            var required = new HashSet<int>(result.GenericInterfaceTargetIndices);

            foreach (var @case in result.Cases)
            {
                token.ThrowIfCancellationRequested();

                if (@case.Owner != CaseOwner.Container)
                {
                    continue;
                }

                var mapped = new HashSet<int>(@case.TargetOrderedIndices);

                foreach (var dependency in required)
                {
                    if (mapped.Contains(dependency))
                    {
                        continue;
                    }

                    result.Kind = ContainerResolutionKind.MissingInterfaceDependency;
                    result.InvalidType = @case.Symbol;
                    result.MissingParameter = result.TargetParameters[dependency].Symbol;
                    return false;
                }
            }

            return true;
        }

        private static bool HaveDuplicateMembers(
              ContainerResolution result
            , CancellationToken token
            , out string duplicateMember
        )
        {
            duplicateMember = string.Empty;

            if (result.BaseInterface is null || result.GenericInterface is null)
            {
                return false;
            }

            var baseMap = CreateTargetMap(result, token);
            var baseMembers = new HashSet<string>(StringComparer.Ordinal);

            foreach (var member in result.BaseInterface.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (TryGetMemberIdentity(member, baseMap, result, token, out var identity, out _))
                {
                    baseMembers.Add(identity);
                }
            }

            var genericMap = CreateTargetMap(result, token);

            for (var i = 0; i < result.GenericInterface.TypeParameters.Length; i++)
            {
                genericMap[result.GenericInterface.TypeParameters[i]] = result.GenericInterfaceTargetIndices[i];
            }

            foreach (var member in result.GenericInterface.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (TryGetMemberIdentity(member, genericMap, result, token, out var identity, out _)
                    && baseMembers.Contains(identity)
                )
                {
                    duplicateMember = member.Name;
                    return true;
                }
            }

            return false;
        }

        private static int[] GetAutomaticInterfaceDependencies(
              ContainerResolution result
            , CancellationToken token
            , out string memberName
        )
        {
            memberName = string.Empty;
            var authoredIdentities = new HashSet<string>(StringComparer.Ordinal);
            AddAuthoredIdentities(result.BaseInterface, result, authoredIdentities, token);
            AddAuthoredIdentities(result.GenericInterface, result, authoredIdentities, token);
            Dictionary<string, (int Count, HashSet<int> Dependencies, string Name)> common = null;
            var consideredCaseCount = 0;

            foreach (var @case in result.Cases)
            {
                token.ThrowIfCancellationRequested();

                if (@case.IsUndefined)
                {
                    continue;
                }

                consideredCaseCount++;
                var map = CreateTargetMap(result, @case, token);
                var current = new Dictionary<string, (HashSet<int> Dependencies, string Name)>(StringComparer.Ordinal);

                foreach (var member in @case.Symbol.GetMembers())
                {
                    token.ThrowIfCancellationRequested();

                    if (member.IsStatic
                        || member.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)
                        || TryGetMemberIdentity(
                              member
                            , map
                            , result
                            , token
                            , out var identity
                            , out var dependencies
                        ) == false
                        || authoredIdentities.Contains(identity)
                    )
                    {
                        continue;
                    }

                    current[identity] = (dependencies, member.Name);
                }

                if (common is null)
                {
                    common = new Dictionary<string, (int Count, HashSet<int> Dependencies, string Name)>(
                        StringComparer.Ordinal
                    );

                    foreach (var pair in current)
                    {
                        token.ThrowIfCancellationRequested();
                        common.Add(pair.Key, (1, pair.Value.Dependencies, pair.Value.Name));
                    }

                    continue;
                }

                foreach (var pair in current)
                {
                    if (common.TryGetValue(pair.Key, out var value))
                    {
                        common[pair.Key] = (value.Count + 1, value.Dependencies, value.Name);
                    }
                }
            }

            var resultSet = new HashSet<int>();

            if (common is not null)
            {
                foreach (var value in common.Values)
                {
                    if (value.Count != consideredCaseCount)
                    {
                        continue;
                    }

                    memberName = value.Name;
                    resultSet.UnionWith(value.Dependencies);
                }
            }

            return ToSortedArray(resultSet, token);
        }

        private static void AddAuthoredIdentities(
              INamedTypeSymbol interfaceSymbol
            , ContainerResolution result
            , HashSet<string> identities
            , CancellationToken token
        )
        {
            if (interfaceSymbol is null)
            {
                return;
            }

            var map = CreateTargetMap(result, token);

            if (interfaceSymbol.Arity > 0)
            {
                for (var i = 0; i < interfaceSymbol.TypeParameters.Length; i++)
                {
                    map[interfaceSymbol.TypeParameters[i]] = result.GenericInterfaceTargetIndices[i];
                }
            }

            foreach (var member in interfaceSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (TryGetMemberIdentity(member, map, result, token, out var identity, out _))
                {
                    identities.Add(identity);
                }
            }
        }

        private static int[] GetMemberDependencies(
              INamedTypeSymbol interfaceSymbol
            , ContainerResolution result
            , CancellationToken token
        )
        {
            var map = CreateTargetMap(result, token);

            for (var i = 0; i < interfaceSymbol.TypeParameters.Length; i++)
            {
                map[interfaceSymbol.TypeParameters[i]] = result.GenericInterfaceTargetIndices[i];
            }

            var dependencies = new HashSet<int>();

            foreach (var member in interfaceSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (TryGetMemberIdentity(member, map, result, token, out _, out var memberDependencies))
                {
                    dependencies.UnionWith(memberDependencies);
                }
            }

            return ToSortedArray(dependencies, token);
        }

        private static bool TryGetMemberIdentity(
              ISymbol member
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , ContainerResolution result
            , CancellationToken token
            , out string identity
            , out HashSet<int> dependencies
        )
        {
            identity = string.Empty;
            dependencies = new HashSet<int>();
            var targetNames = new string[result.TargetParameters.Count];

            for (var i = 0; i < targetNames.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                targetNames[i] = result.TargetParameters[i].Name;
            }

            if (member is IPropertySymbol property)
            {
                AddTypeDependencies(property.Type, substitutions, dependencies, token);
                var builder = new StringBuilder(property.IsIndexer ? "I|" : "P|");
                builder.Append(property.Name).Append('|');
                builder.Append(FormatType(property.Type, substitutions, targetNames, token)).Append('|');
                builder.Append((int)property.RefKind);

                foreach (var parameter in property.Parameters)
                {
                    AddTypeDependencies(parameter.Type, substitutions, dependencies, token);
                    builder.Append('|').Append((int)parameter.RefKind).Append(':');
                    builder.Append(FormatType(parameter.Type, substitutions, targetNames, token));
                }

                identity = builder.ToString();
                return true;
            }

            if (member is IMethodSymbol method
                && method.MethodKind == MethodKind.Ordinary
                && method.TypeParameters.Length < 1
            )
            {
                AddTypeDependencies(method.ReturnType, substitutions, dependencies, token);
                var builder = new StringBuilder("M|");
                builder.Append(method.Name).Append('|');
                builder.Append(FormatType(method.ReturnType, substitutions, targetNames, token)).Append('|');
                builder.Append((int)method.RefKind);

                foreach (var parameter in method.Parameters)
                {
                    AddTypeDependencies(parameter.Type, substitutions, dependencies, token);
                    builder.Append('|').Append((int)parameter.RefKind).Append(':');
                    builder.Append(FormatType(parameter.Type, substitutions, targetNames, token));
                }

                identity = builder.ToString();
                return true;
            }

            return false;
        }

        private static void AddTypeDependencies(
              ITypeSymbol type
            , IReadOnlyDictionary<ITypeParameterSymbol, int> substitutions
            , HashSet<int> dependencies
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (type is ITypeParameterSymbol parameter)
            {
                if (substitutions.TryGetValue(parameter, out var targetIndex))
                {
                    dependencies.Add(targetIndex);
                }

                return;
            }

            if (type is IArrayTypeSymbol array)
            {
                AddTypeDependencies(array.ElementType, substitutions, dependencies, token);
                return;
            }

            if (type is IPointerTypeSymbol pointer)
            {
                AddTypeDependencies(pointer.PointedAtType, substitutions, dependencies, token);
                return;
            }

            if (type is INamedTypeSymbol named)
            {
                foreach (var argument in named.TypeArguments)
                {
                    AddTypeDependencies(argument, substitutions, dependencies, token);
                }
            }
        }

        private static bool TryMapParameters(
              INamedTypeSymbol mappedType
            , IReadOnlyList<TargetParameter> targetParameters
            , Compilation compilation
            , out int[] authoredIndices
            , out bool constraintMismatch
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            authoredIndices = Array.Empty<int>();
            constraintMismatch = false;
            var localParameters = mappedType.TypeParameters;

            if (localParameters.Length < 1)
            {
                return true;
            }

            var targetNames = new HashSet<string>(StringComparer.Ordinal);
            var duplicateTargetNames = false;

            foreach (var parameter in targetParameters)
            {
                token.ThrowIfCancellationRequested();

                if (targetNames.Add(parameter.SourceName) == false)
                {
                    duplicateTargetNames = true;
                    break;
                }
            }
            var indices = new int[localParameters.Length];

            if (duplicateTargetNames)
            {
                if (localParameters.Length != targetParameters.Count)
                {
                    return false;
                }

                for (var i = 0; i < indices.Length; i++)
                {
                    indices[i] = i;
                }
            }
            else
            {
                var targetByName = new Dictionary<string, TargetParameter>(StringComparer.Ordinal);

                foreach (var parameter in targetParameters)
                {
                    token.ThrowIfCancellationRequested();
                    targetByName.Add(parameter.SourceName, parameter);
                }

                var used = new HashSet<int>();

                for (var i = 0; i < localParameters.Length; i++)
                {
                    if (targetByName.TryGetValue(localParameters[i].Name, out var target) == false
                        || used.Add(target.Index) == false
                    )
                    {
                        return false;
                    }

                    indices[i] = target.Index;
                }
            }

            var substitutions = new Dictionary<ITypeParameterSymbol, ITypeParameterSymbol>(
                SymbolEqualityComparer.Default
            );

            for (var i = 0; i < localParameters.Length; i++)
            {
                substitutions[targetParameters[indices[i]].Symbol] = localParameters[i];
            }

            for (var i = 0; i < localParameters.Length; i++)
            {
                if (HaveEquivalentConstraints(
                      targetParameters[indices[i]].Symbol
                    , localParameters[i]
                    , substitutions
                    , compilation
                    , token
                ) == false)
                {
                    constraintMismatch = true;
                    return false;
                }
            }

            authoredIndices = indices;
            return true;
        }

        private static bool HaveEquivalentConstraints(
              ITypeParameterSymbol target
            , ITypeParameterSymbol mapped
            , IReadOnlyDictionary<ITypeParameterSymbol, ITypeParameterSymbol> substitutions
            , Compilation compilation
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (target.HasConstructorConstraint != mapped.HasConstructorConstraint
                || target.HasNotNullConstraint != mapped.HasNotNullConstraint
                || target.HasReferenceTypeConstraint != mapped.HasReferenceTypeConstraint
                || target.HasUnmanagedTypeConstraint != mapped.HasUnmanagedTypeConstraint
                || target.HasValueTypeConstraint != mapped.HasValueTypeConstraint
                || target.ReferenceTypeConstraintNullableAnnotation != mapped.ReferenceTypeConstraintNullableAnnotation
                || target.ConstraintTypes.Length != mapped.ConstraintTypes.Length
            )
            {
                return false;
            }

            var unmatched = mapped.ConstraintTypes.ToList();

            foreach (var constraint in target.ConstraintTypes)
            {
                token.ThrowIfCancellationRequested();
                var substituted = Substitute(constraint, substitutions, compilation, token);
                var index = -1;

                for (var candidateIndex = 0; candidateIndex < unmatched.Count; candidateIndex++)
                {
                    token.ThrowIfCancellationRequested();

                    if (SymbolEqualityComparer.Default.Equals(unmatched[candidateIndex], substituted))
                    {
                        index = candidateIndex;
                        break;
                    }
                }

                if (index < 0)
                {
                    return false;
                }

                unmatched.RemoveAt(index);
            }

            return true;
        }

        private static ITypeSymbol Substitute(
              ITypeSymbol type
            , IReadOnlyDictionary<ITypeParameterSymbol, ITypeParameterSymbol> substitutions
            , Compilation compilation
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (type is ITypeParameterSymbol parameter && substitutions.TryGetValue(parameter, out var replacement))
            {
                return replacement;
            }

            if (type is IArrayTypeSymbol array)
            {
                return compilation.CreateArrayTypeSymbol(
                      Substitute(array.ElementType, substitutions, compilation, token)
                    , array.Rank
                );
            }

            if (type is INamedTypeSymbol named && named.IsGenericType)
            {
                var arguments = new ITypeSymbol[named.TypeArguments.Length];

                for (var i = 0; i < arguments.Length; i++)
                {
                    token.ThrowIfCancellationRequested();
                    arguments[i] = Substitute(named.TypeArguments[i], substitutions, compilation, token);
                }

                return named.OriginalDefinition.Construct(arguments);
            }

            return type;
        }

        private static void FillTargetParameters(
              INamedTypeSymbol target
            , List<TargetParameter> result
            , CancellationToken token
        )
        {
            var chain = GetTypeChain(target, token);

            var usedNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var type in chain)
            {
                token.ThrowIfCancellationRequested();

                foreach (var parameter in type.OriginalDefinition.TypeParameters)
                {
                    token.ThrowIfCancellationRequested();
                    var name = parameter.Name;

                    if (usedNames.Add(name) == false)
                    {
                        var suffix = result.Count;

                        do
                        {
                            token.ThrowIfCancellationRequested();
                            name = $"{parameter.Name}{suffix}";
                            suffix++;
                        }
                        while (usedNames.Add(name) == false);
                    }

                    result.Add(new TargetParameter(result.Count, parameter, parameter.Name, name));
                }
            }
        }

        private static bool IsEffectivelyNonGeneric(INamedTypeSymbol type, CancellationToken token)
        {
            for (var current = type; current is not null; current = current.ContainingType)
            {
                token.ThrowIfCancellationRequested();

                if (current.Arity > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static List<INamedTypeSymbol> GetTypeChain(INamedTypeSymbol type, CancellationToken token)
        {
            var result = new List<INamedTypeSymbol>();

            for (var current = type; current is not null; current = current.ContainingType)
            {
                token.ThrowIfCancellationRequested();
                result.Add(current);
            }

            result.Reverse();
            return result;
        }

        private static int[] ToSortedArray(IEnumerable<int> values, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var result = new List<int>();

            foreach (var value in values)
            {
                token.ThrowIfCancellationRequested();
                result.Add(value);
            }

            result.Sort();
            token.ThrowIfCancellationRequested();
            return result.ToArray();
        }

        private static void BuildDefaultContainerIdentity(
              INamedTypeSymbol target
            , out string typeName
            , out string metadataName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var chain = GetTypeChain(target, token);
            var names = new List<string>();

            foreach (var type in chain)
            {
                token.ThrowIfCancellationRequested();
                names.Add(type.Name);
            }

            var namespaceName = target.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : target.ContainingNamespace.ToDisplayString();
            var namespacePrefix = string.IsNullOrEmpty(namespaceName) ? "global::" : $"global::{namespaceName}.";
            var metadataPrefix = string.IsNullOrEmpty(namespaceName) ? string.Empty : $"{namespaceName}.";
            typeName = $"{namespacePrefix}{string.Join(".", names)}";
            metadataName = $"{metadataPrefix}{string.Join("+", names)}";
        }
    }
}
