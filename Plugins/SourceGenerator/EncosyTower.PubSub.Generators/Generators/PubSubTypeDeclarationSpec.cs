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

        public PubSubTypeDeclarationSpec(
              string openingSource
            , string closingSource
            , string typeName
            , string typeKeyword
            , string fullTypeName
            , string metadataName
            , bool isReferenceType
        )
        {
            OpeningSource = openingSource;
            ClosingSource = closingSource;
            TypeName = typeName;
            TypeKeyword = typeKeyword;
            FullTypeName = fullTypeName;
            MetadataName = metadataName;
            IsReferenceType = isReferenceType;
        }

        public bool IsValid => string.IsNullOrEmpty(TypeName) == false;

        public static bool operator ==(PubSubTypeDeclarationSpec left, PubSubTypeDeclarationSpec right)
            => left.Equals(right);

        public static bool operator !=(PubSubTypeDeclarationSpec left, PubSubTypeDeclarationSpec right)
            => left.Equals(right) == false;

        public readonly bool Equals(PubSubTypeDeclarationSpec other)
            => string.Equals(OpeningSource, other.OpeningSource, StringComparison.Ordinal)
            && string.Equals(ClosingSource, other.ClosingSource, StringComparison.Ordinal)
            && string.Equals(TypeName, other.TypeName, StringComparison.Ordinal)
            && string.Equals(TypeKeyword, other.TypeKeyword, StringComparison.Ordinal)
            && string.Equals(FullTypeName, other.FullTypeName, StringComparison.Ordinal)
            && string.Equals(MetadataName, other.MetadataName, StringComparison.Ordinal)
            && IsReferenceType == other.IsReferenceType
            ;

        public readonly override bool Equals(object obj)
            => obj is PubSubTypeDeclarationSpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash = hash.Add(OpeningSource);
            hash = hash.Add(ClosingSource);
            hash = hash.Add(TypeName);
            hash = hash.Add(TypeKeyword);
            hash = hash.Add(FullTypeName);
            hash = hash.Add(MetadataName);
            hash = hash.Add(IsReferenceType);
            return hash.ToHashCode();
        }
    }
}
