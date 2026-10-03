namespace EncosyTower.Core.PolyEnumFactories
{
    internal enum WrapperShape
    {
        /// <summary>
        /// The generator can complete the wrapper.
        /// </summary>
        Supported,

        /// <summary>
        /// A positional record declares the poly-enum struct after its first positional parameter.
        /// </summary>
        LaterRecordParameter,

        /// <summary>
        /// No constructor takes the poly-enum struct as its only required argument, and a constructor of the wrapper
        /// keeps the generator from adding one.
        /// </summary>
        BlockingConstructor,

        /// <summary>
        /// A constructor takes the poly-enum struct first, but the wrapper declares no instance field or
        /// auto-property of the poly-enum struct type for the generated members to read.
        /// </summary>
        MissingStorage,
    }

    /// <summary>
    /// Wrapper rules shared by the PolyEnumFactory generator and analyzer. Reports nothing.
    /// </summary>
    internal static class FactoryWrapperRules
    {
        /// <summary>
        /// Decides whether the generator can complete <paramref name="wrapper"/>.
        /// </summary>
        /// <param name="recordParameter">
        /// The first positional parameter of type <paramref name="enumStruct"/>, whichever partial part declares the
        /// parameter list, or <see langword="null"/>.
        /// </param>
        /// <param name="blockingConstructor">
        /// The constructor that keeps the generator from completing the wrapper when the result is
        /// <see cref="WrapperShape.BlockingConstructor"/> or <see cref="WrapperShape.MissingStorage"/>,
        /// otherwise <see langword="null"/>.
        /// </param>
        /// <param name="storageField">
        /// The instance field the generated members read when the wrapper stores the value itself,
        /// otherwise <see langword="null"/>.
        /// </param>
        public static WrapperShape GetWrapperShape(
              INamedTypeSymbol wrapper
            , INamedTypeSymbol enumStruct
            , CancellationToken token
            , out IParameterSymbol recordParameter
            , out IMethodSymbol blockingConstructor
            , out IFieldSymbol storageField
        )
        {
            token.ThrowIfCancellationRequested();
            recordParameter = null;
            blockingConstructor = null;
            storageField = null;

            var comparer = SymbolEqualityComparer.Default;
            var isPositionalRecord = TryGetPrimaryConstructor(wrapper, token, out var primaryConstructor);

            if (isPositionalRecord)
            {
                var parameters = primaryConstructor.Parameters;

                for (var i = 0; i < parameters.Length; i++)
                {
                    token.ThrowIfCancellationRequested();

                    if (comparer.Equals(parameters[i].Type, enumStruct))
                    {
                        recordParameter = parameters[i];

                        if (i > 0)
                        {
                            return WrapperShape.LaterRecordParameter;
                        }

                        break;
                    }
                }
            }

            var storageConstructor = GetStorageConstructor(wrapper, enumStruct, token);

            if (storageConstructor is not null)
            {
                // The generator adds no constructor and creates the wrapper with `new Wrapper(value)`.
                if (HasSingleArgumentConstructor(wrapper, enumStruct, token) == false)
                {
                    blockingConstructor = storageConstructor;
                    return WrapperShape.BlockingConstructor;
                }

                if (recordParameter is not null)
                {
                    return WrapperShape.Supported;
                }

                // The generated members read the value from the wrapper's own storage.
                storageField = GetStorageField(wrapper, enumStruct, token);

                if (storageField is null)
                {
                    blockingConstructor = storageConstructor;
                    return WrapperShape.MissingStorage;
                }

                return WrapperShape.Supported;
            }

            // The generator adds a field and a private constructor that takes the value.
            if (isPositionalRecord)
            {
                blockingConstructor = primaryConstructor;
                return WrapperShape.BlockingConstructor;
            }

            if (wrapper.TypeKind == TypeKind.Struct)
            {
                foreach (var constructor in wrapper.InstanceConstructors)
                {
                    token.ThrowIfCancellationRequested();

                    if (constructor.IsImplicitlyDeclared == false
                        && CallsAnotherConstructor(constructor, token) == false
                    )
                    {
                        blockingConstructor = constructor;
                        return WrapperShape.BlockingConstructor;
                    }
                }
            }

            return WrapperShape.Supported;
        }

        /// <summary>
        /// Gets the first instance constructor whose first parameter has the poly-enum struct type; the generator
        /// then expects the wrapper to store the value itself.
        /// </summary>
        private static IMethodSymbol GetStorageConstructor(
              INamedTypeSymbol wrapper
            , INamedTypeSymbol enumStruct
            , CancellationToken token
        )
        {
            var comparer = SymbolEqualityComparer.Default;

            foreach (var constructor in wrapper.InstanceConstructors)
            {
                token.ThrowIfCancellationRequested();

                if (constructor.Parameters.Length > 0
                    && comparer.Equals(constructor.Parameters[0].Type, enumStruct)
                )
                {
                    return constructor;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the first instance field of the poly-enum struct type, a compiler-generated auto-property backing
        /// field included.
        /// </summary>
        private static IFieldSymbol GetStorageField(
              INamedTypeSymbol wrapper
            , INamedTypeSymbol enumStruct
            , CancellationToken token
        )
        {
            var comparer = SymbolEqualityComparer.Default;

            foreach (var member in wrapper.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is IFieldSymbol field
                    && field.IsStatic == false
                    && comparer.Equals(field.Type, enumStruct)
                )
                {
                    return field;
                }
            }

            return null;
        }

        /// <summary>
        /// Whether an instance constructor takes the poly-enum struct as its only required argument: its first
        /// parameter, passed by value or <see langword="in"/>, has the poly-enum struct type, <see cref="object"/>
        /// or <see cref="ValueType"/>, and every other parameter is optional or a parameter array.
        /// </summary>
        private static bool HasSingleArgumentConstructor(
              INamedTypeSymbol wrapper
            , INamedTypeSymbol enumStruct
            , CancellationToken token
        )
        {
            var comparer = SymbolEqualityComparer.Default;

            foreach (var constructor in wrapper.InstanceConstructors)
            {
                token.ThrowIfCancellationRequested();

                var parameters = constructor.Parameters;

                if (parameters.Length < 1
                    || parameters[0].RefKind == RefKind.Ref
                    || parameters[0].RefKind == RefKind.Out
                )
                {
                    continue;
                }

                var firstType = parameters[0].Type;

                if (comparer.Equals(firstType, enumStruct) == false
                    && firstType.SpecialType != SpecialType.System_Object
                    && firstType.SpecialType != SpecialType.System_ValueType
                )
                {
                    continue;
                }

                var othersOptional = true;

                for (var i = 1; i < parameters.Length; i++)
                {
                    if (parameters[i].IsOptional == false && parameters[i].IsParams == false)
                    {
                        othersOptional = false;
                        break;
                    }
                }

                if (othersOptional)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CallsAnotherConstructor(IMethodSymbol constructor, CancellationToken token)
        {
            foreach (var reference in constructor.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax(token) is ConstructorDeclarationSyntax syntax
                    && syntax.Initializer is not null
                    && syntax.Initializer.IsKind(SyntaxKind.ThisConstructorInitializer)
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetPrimaryConstructor(
              INamedTypeSymbol type
            , CancellationToken token
            , out IMethodSymbol result
        )
        {
            foreach (var constructor in type.InstanceConstructors)
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
    }
}
