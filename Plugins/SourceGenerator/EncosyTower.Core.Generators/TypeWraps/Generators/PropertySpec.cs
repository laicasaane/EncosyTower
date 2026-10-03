namespace EncosyTower.Core.Generators.TypeWraps
{
    public struct PropertySpec : IEquatable<PropertySpec>
    {
        public string name;
        public string typeName;
        public string parameters;
        public string arguments;
        public int explicitInterfaceImplementationsLength;
        public RefKind refKind;
        public bool sameType;
        public bool isPublic;
        public bool isReadOnly;
        public bool isStatic;
        public bool isIndexer;
        public bool hasGetter;
        public bool getterSetterCanBeReadOnly;
        public bool getterCanBeReadOnly;
        public bool isGetterRef;
        public bool isGetterRefRO;
        public bool isGetterRO;
        public bool hasSetter;
        public bool isSetterRO;
        public bool withoutSetter;
        public bool isUnsafe;

        public readonly bool IsValid => string.IsNullOrEmpty(name) == false && string.IsNullOrEmpty(typeName) == false;

        public static PropertySpec Create(
              IPropertySymbol property
            , INamedTypeSymbol fieldTypeSymbol
            , bool wrapperIsStruct
            , bool wrapperIsReadOnly
            , bool storageMembersAreReadOnly
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var p = new Printer(0, 1024 * 16, token);
            var name = property.ToDisplayString(SymbolExtensions.QualifiedMemberNameWithGlobalPrefix);

            var parameters = string.Empty;
            var arguments = string.Empty;

            if (property.IsIndexer || property.Parameters.Length > 0)
            {
                p.Clear();
                p.PrintIf(property.IsIndexer, "this", name).Print("[");

                var propParams = property.Parameters;
                var last = propParams.Length - 1;

                for (var i = 0; i <= last; i++)
                {
                    var param = propParams[i];

                    p.Print($"{param.Type.ToFullName()} {param.Name}");
                    p.PrintIf(i < last, ", ");
                }

                p.Print("]");

                parameters = p.Result;

                p.Clear();

                for (var i = 0; i <= last; i++)
                {
                    var param = propParams[i];

                    p.Print(param.Name);
                    p.PrintIf(i < last, ", ");
                }

                arguments = p.Result;
            }

            bool hasGetter = default;
            bool isGetterRef = default;
            bool isGetterRefRO = default;
            bool isGetterRO = default;

            if (property.GetMethod is IMethodSymbol getter)
            {
                hasGetter = true;
                isGetterRef = property.RefKind != RefKind.Ref && getter.RefKind == RefKind.Ref;
                isGetterRefRO = property.RefKind != RefKind.RefReadOnly && getter.RefKind == RefKind.RefReadOnly;
                isGetterRO = getter.IsReadOnly;
            }

            bool hasSetter = default;
            bool isSetterRO = default;
            bool withoutSetter;

            if (property.SetMethod is IMethodSymbol setter)
            {
                hasSetter = true;
                isSetterRO = setter.IsReadOnly;

                withoutSetter = setter.IsInitOnly
                    || storageMembersAreReadOnly
                    || (fieldTypeSymbol.IsReadOnly == false && wrapperIsStruct && wrapperIsReadOnly);
            }
            else
            {
                withoutSetter = true;
            }

            var isUnsafe = property.Type is IPointerTypeSymbol;

            if (isUnsafe == false)
            {
                foreach (var param in property.Parameters)
                {
                    token.ThrowIfCancellationRequested();

                    if (param.Type is IPointerTypeSymbol)
                    {
                        isUnsafe = true;
                        break;
                    }
                }
            }

            return new PropertySpec {
                name = name,
                typeName = property.Type.ToFullName(),
                sameType = SymbolEqualityComparer.Default.Equals(property.Type, fieldTypeSymbol),
                parameters = parameters,
                arguments = arguments,
                isPublic = property.DeclaredAccessibility == Accessibility.Public,
                isReadOnly = property.IsReadOnly,
                isStatic = property.IsStatic,
                isIndexer = property.IsIndexer,
                hasGetter = hasGetter,
                getterSetterCanBeReadOnly = wrapperIsStruct && withoutSetter == false && property.IsStatic,
                getterCanBeReadOnly = withoutSetter && isGetterRO,
                refKind = property.RefKind,
                explicitInterfaceImplementationsLength = property.ExplicitInterfaceImplementations.Length,
                isGetterRef = isGetterRef,
                isGetterRefRO = isGetterRefRO,
                isGetterRO = isGetterRO,
                hasSetter = hasSetter,
                isSetterRO = isSetterRO,
                isUnsafe = isUnsafe,
                withoutSetter = withoutSetter,
            };
        }

        public readonly bool Equals(PropertySpec other)
            => string.Equals(name, other.name, StringComparison.Ordinal)
            && string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(parameters, other.parameters, StringComparison.Ordinal)
            && string.Equals(arguments, other.arguments, StringComparison.Ordinal)
            && explicitInterfaceImplementationsLength == other.explicitInterfaceImplementationsLength
            && refKind == other.refKind
            && sameType == other.sameType
            && isPublic == other.isPublic
            && isReadOnly == other.isReadOnly
            && isStatic == other.isStatic
            && isIndexer == other.isIndexer
            && hasGetter == other.hasGetter
            && getterSetterCanBeReadOnly == other.getterSetterCanBeReadOnly
            && getterCanBeReadOnly == other.getterCanBeReadOnly
            && isGetterRef == other.isGetterRef
            && isGetterRefRO == other.isGetterRefRO
            && isGetterRO == other.isGetterRO
            && hasSetter == other.hasSetter
            && isSetterRO == other.isSetterRO
            && withoutSetter == other.withoutSetter
            && isUnsafe == other.isUnsafe
            ;

        public readonly override bool Equals(object obj)
            => obj is PropertySpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  name
                , typeName
                , parameters
                , arguments
                , explicitInterfaceImplementationsLength
                , refKind
                , sameType
                , isPublic
            )
            .Add(isReadOnly)
            .Add(isStatic)
            .Add(isIndexer)
            .Add(hasGetter)
            .Add(getterSetterCanBeReadOnly)
            .Add(getterCanBeReadOnly)
            .Add(isGetterRef)
            .Add(isGetterRefRO)
            .Add(isGetterRO)
            .Add(hasSetter)
            .Add(isSetterRO)
            .Add(withoutSetter)
            .Add(isUnsafe)
            ;
    }
}
