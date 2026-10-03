namespace EncosyTower.PubSub.Generators
{
    internal readonly struct PubSubTypeDeclarationSpec : IEquatable<PubSubTypeDeclarationSpec>
    {
        public readonly string OpeningSource;
        public readonly string ClosingSource;
        public readonly string TypeName;
        public readonly string TypeKeyword;
        public readonly string FullTypeName;
        public readonly string MetadataName;
        public readonly bool IsReferenceType;
        public readonly EquatableArray<ContainingTypeSpec> ContainingTypes;
        public readonly string AssemblyName;

        public PubSubTypeDeclarationSpec(
              string openingSource
            , string closingSource
            , string typeName
            , string typeKeyword
            , string fullTypeName
            , string metadataName
            , bool isReferenceType
            , EquatableArray<ContainingTypeSpec> containingTypes = default
            , string assemblyName = null
        )
        {
            OpeningSource = openingSource;
            ClosingSource = closingSource;
            TypeName = typeName;
            TypeKeyword = typeKeyword;
            FullTypeName = fullTypeName;
            MetadataName = metadataName;
            IsReferenceType = isReferenceType;
            ContainingTypes = containingTypes;
            AssemblyName = assemblyName;
        }

        public bool IsValid => string.IsNullOrEmpty(TypeName) == false;

        public static bool operator ==(PubSubTypeDeclarationSpec left, PubSubTypeDeclarationSpec right)
            => left.Equals(right);

        public static bool operator !=(PubSubTypeDeclarationSpec left, PubSubTypeDeclarationSpec right)
            => left.Equals(right) == false;

        public readonly bool Equals(PubSubTypeDeclarationSpec other)
            => string.Equals(TypeName, other.TypeName, StringComparison.Ordinal)
            && string.Equals(TypeKeyword, other.TypeKeyword, StringComparison.Ordinal)
            && string.Equals(FullTypeName, other.FullTypeName, StringComparison.Ordinal)
            && string.Equals(MetadataName, other.MetadataName, StringComparison.Ordinal)
            && IsReferenceType == other.IsReferenceType
            && ContainingTypes.Equals(other.ContainingTypes)
            && string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal)
            ;

        public readonly override bool Equals(object obj)
            => obj is PubSubTypeDeclarationSpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash = hash.Add(TypeName);
            hash = hash.Add(TypeKeyword);
            hash = hash.Add(FullTypeName);
            hash = hash.Add(MetadataName);
            hash = hash.Add(IsReferenceType);
            hash = hash.Add(ContainingTypes);
            hash = hash.Add(AssemblyName);
            return hash.ToHashCode();
        }
    }
}
