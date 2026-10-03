namespace EncosyTower.Core.Generators.NewtonsoftAotHelpers
{
    internal struct NewtonsoftAotHelperSpec : IEquatable<NewtonsoftAotHelperSpec>
    {
        public string openingSource;
        public string closingSource;
        public string typeName;
        public string hintName;
        public string baseTypeFullName;
        public string namespaceName;
        public EquatableArray<AotTypeSpec> typeCandidates;
        public EquatableArray<string> containingTypes;
        public bool isStatic;
        public bool isRecord;
        public bool isValid;
        public TypeKind typeKind;

        public readonly bool IsValid => isValid;

        public readonly bool Equals(NewtonsoftAotHelperSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(baseTypeFullName, other.baseTypeFullName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && typeCandidates.Equals(other.typeCandidates)
            && containingTypes.Equals(other.containingTypes)
            && isStatic == other.isStatic
            && isRecord == other.isRecord
            && isValid == other.isValid
            && typeKind == other.typeKind
            ;

        public readonly override bool Equals(object obj)
            => obj is NewtonsoftAotHelperSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(typeName, baseTypeFullName, namespaceName)
            .Add(typeCandidates.GetHashCode())
            .Add(containingTypes.GetHashCode())
            .Add(isStatic)
            .Add(isRecord)
            .Add(isValid)
            .Add(typeKind);
    }
}
