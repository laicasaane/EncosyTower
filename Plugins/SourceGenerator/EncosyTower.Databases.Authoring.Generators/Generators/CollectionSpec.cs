using EncosyTower.SourceGen.Data.Helpers;

namespace EncosyTower.Databases.Authoring.Generators
{
    public struct CollectionSpec : IEquatable<CollectionSpec>
    {
        public CollectionKind kind;
        public TypeSpec keyType;
        public TypeSpec elementType;
        public CollectionSpecArray argumentCollections;

        public readonly bool Equals(CollectionSpec other)
            => kind == other.kind
            && keyType.Equals(other.keyType)
            && elementType.Equals(other.elementType)
            && argumentCollections.Equals(other.argumentCollections)
            ;

        public readonly override bool Equals(object obj)
            => obj is CollectionSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(kind, keyType, elementType, argumentCollections);
    }
}
