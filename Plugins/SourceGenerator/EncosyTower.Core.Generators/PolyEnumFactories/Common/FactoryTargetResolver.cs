namespace EncosyTower.Core.PolyEnumFactories
{
    internal enum ResolutionKind
    {
        Invalid,
        Valid,
        ArityMismatch,
        ConstraintMismatch,
    }

    internal readonly record struct FactoryTargetResolution(
          ResolutionKind Kind
        , INamedTypeSymbol Target
    );

    internal static class FactoryTargetResolver
    {
        public static FactoryTargetResolution Resolve(
              INamedTypeSymbol target
            , INamedTypeSymbol factory
            , Compilation compilation
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (IsSameOwnerNestedTarget(target, factory, token))
            {
                var nested = factory.GetTypeMembers(target.Name, target.Arity);
                return nested.Length == 1
                    ? new(ResolutionKind.Valid, nested[0])
                    : default;
            }

            if (IsFullyClosed(target, token))
            {
                return new(ResolutionKind.Valid, target);
            }

            if (IsFullyOpen(target, token) == false)
            {
                return default;
            }

            var targetParameters = GetEffectiveTypeParameters(target, token);
            var factoryParameters = GetEffectiveTypeParameters(factory, token);

            if (targetParameters.Count != factoryParameters.Count)
            {
                return new(ResolutionKind.ArityMismatch, null);
            }

            if (HaveEquivalentConstraints(targetParameters, factoryParameters, compilation, token) == false)
            {
                return new(ResolutionKind.ConstraintMismatch, null);
            }

            var resolved = ConstructTarget(target, factoryParameters, token);
            return resolved is null
                ? default
                : new(ResolutionKind.Valid, resolved);
        }

        public static int GetEffectiveArity(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return GetEffectiveTypeParameters(type, token).Count;
        }

        private static bool IsSameOwnerNestedTarget(
              INamedTypeSymbol target
            , INamedTypeSymbol factory
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            return target.Arity == 0
                && target.ContainingType is INamedTypeSymbol containing
                && SymbolEqualityComparer.Default.Equals(containing.OriginalDefinition, factory.OriginalDefinition);
        }

        private static bool IsFullyClosed(INamedTypeSymbol target, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            for (var current = target; current is not null; current = current.ContainingType)
            {
                token.ThrowIfCancellationRequested();

                if (current.IsUnboundGenericType)
                {
                    return false;
                }

                foreach (var argument in current.TypeArguments)
                {
                    token.ThrowIfCancellationRequested();

                    if (argument.TypeKind == TypeKind.TypeParameter)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsFullyOpen(INamedTypeSymbol target, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var foundGeneric = false;

            for (var current = target; current is not null; current = current.ContainingType)
            {
                token.ThrowIfCancellationRequested();

                if (current.Arity < 1)
                {
                    continue;
                }

                foundGeneric = true;

                if (current.IsUnboundGenericType == false)
                {
                    return false;
                }
            }

            return foundGeneric;
        }

        private static List<ITypeParameterSymbol> GetEffectiveTypeParameters(
              INamedTypeSymbol type
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var chain = GetTypeChain(type, token);
            var result = new List<ITypeParameterSymbol>();

            foreach (var current in chain)
            {
                token.ThrowIfCancellationRequested();
                result.AddRange(current.OriginalDefinition.TypeParameters);
            }

            return result;
        }

        private static INamedTypeSymbol ConstructTarget(
              INamedTypeSymbol target
            , IReadOnlyList<ITypeParameterSymbol> arguments
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var chain = GetTypeChain(target, token);
            INamedTypeSymbol constructed = null;
            var argumentIndex = 0;

            foreach (var definition in chain)
            {
                token.ThrowIfCancellationRequested();
                var original = definition.OriginalDefinition;
                INamedTypeSymbol current;

                if (constructed is null)
                {
                    current = original;
                }
                else
                {
                    current = constructed.GetTypeMembers(original.Name, original.Arity)
                        .FirstOrDefault(candidate => {
                            token.ThrowIfCancellationRequested();
                            return SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, original);
                        });
                }

                if (current is null)
                {
                    return null;
                }

                if (current.Arity > 0)
                {
                    var localArguments = new ITypeSymbol[current.Arity];

                    for (var i = 0; i < localArguments.Length; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        localArguments[i] = arguments[argumentIndex++];
                    }

                    current = current.Construct(localArguments);
                }

                constructed = current;
            }

            return constructed;
        }

        private static List<INamedTypeSymbol> GetTypeChain(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var result = new List<INamedTypeSymbol>();

            for (var current = type; current is not null; current = current.ContainingType)
            {
                token.ThrowIfCancellationRequested();
                result.Add(current);
            }

            result.Reverse();
            return result;
        }

        private static bool HaveEquivalentConstraints(
              IReadOnlyList<ITypeParameterSymbol> targets
            , IReadOnlyList<ITypeParameterSymbol> factories
            , Compilation compilation
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var hasConstraintTypes = false;

            for (var i = 0; i < targets.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                var target = targets[i];
                var factory = factories[i];

                if (target.HasConstructorConstraint != factory.HasConstructorConstraint
                    || target.HasNotNullConstraint != factory.HasNotNullConstraint
                    || target.HasReferenceTypeConstraint != factory.HasReferenceTypeConstraint
                    || target.HasUnmanagedTypeConstraint != factory.HasUnmanagedTypeConstraint
                    || target.HasValueTypeConstraint != factory.HasValueTypeConstraint
                    || target.ReferenceTypeConstraintNullableAnnotation
                        != factory.ReferenceTypeConstraintNullableAnnotation
                    || target.ConstraintTypes.Length != factory.ConstraintTypes.Length
                )
                {
                    return false;
                }

                hasConstraintTypes |= target.ConstraintTypes.Length > 0;
            }

            if (hasConstraintTypes == false)
            {
                return true;
            }

            var substitutions = new Dictionary<ITypeParameterSymbol, ITypeParameterSymbol>(
                  targets.Count
                , SymbolEqualityComparer.Default
            );

            for (var i = 0; i < targets.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                substitutions.Add(targets[i], factories[i]);
            }

            for (var i = 0; i < targets.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                var target = targets[i];

                if (target.ConstraintTypes.Length < 1)
                {
                    continue;
                }

                if (HaveEquivalentConstraintTypes(
                      target.ConstraintTypes
                    , target.ConstraintNullableAnnotations
                    , factories[i].ConstraintTypes
                    , factories[i].ConstraintNullableAnnotations
                    , substitutions
                    , compilation
                    , token
                ) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HaveEquivalentConstraintTypes(
              ImmutableArray<ITypeSymbol> targets
            , ImmutableArray<NullableAnnotation> targetAnnotations
            , ImmutableArray<ITypeSymbol> factories
            , ImmutableArray<NullableAnnotation> factoryAnnotations
            , IReadOnlyDictionary<ITypeParameterSymbol, ITypeParameterSymbol> substitutions
            , Compilation compilation
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (targets.Length != factories.Length)
            {
                return false;
            }

            var unmatched = new List<(ITypeSymbol Type, NullableAnnotation Annotation)>(factories.Length);

            for (var i = 0; i < factories.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                unmatched.Add((factories[i], factoryAnnotations[i]));
            }

            for (var targetIndex = 0; targetIndex < targets.Length; targetIndex++)
            {
                token.ThrowIfCancellationRequested();
                var target = targets[targetIndex];
                var substituted = Substitute(target, substitutions, compilation, token);
                var targetAnnotation = targetAnnotations[targetIndex];
                var index = unmatched.FindIndex(candidate => {
                    token.ThrowIfCancellationRequested();
                    return candidate.Annotation == targetAnnotation
                        && SymbolEqualityComparer.Default.Equals(candidate.Type, substituted);
                });

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
                var arguments = named.TypeArguments
                    .Select(argument => {
                        token.ThrowIfCancellationRequested();
                        return Substitute(argument, substitutions, compilation, token);
                    })
                    .ToArray();

                return named.OriginalDefinition.Construct(arguments);
            }

            return type;
        }
    }
}
