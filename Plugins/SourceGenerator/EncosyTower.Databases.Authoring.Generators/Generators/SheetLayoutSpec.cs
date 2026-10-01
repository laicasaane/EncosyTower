namespace EncosyTower.Databases.Authoring.Generators
{
    internal struct SheetLayoutSpec : IEquatable<SheetLayoutSpec>
    {
        public HashValue64 scopeKey;
        public EquatableArray<ScopedConverterSpec> converters;
        public EquatableArray<HorizontalCollectionSpec> horizontalCollections;
        public EquatableArray<string> generatedKeyTypeFullNames;

        public readonly bool Equals(SheetLayoutSpec other)
            => scopeKey == other.scopeKey
            && converters.Equals(other.converters)
            && horizontalCollections.Equals(other.horizontalCollections)
            && generatedKeyTypeFullNames.Equals(other.generatedKeyTypeFullNames)
            ;

        public readonly override bool Equals(object obj)
            => obj is SheetLayoutSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(scopeKey, converters, horizontalCollections, generatedKeyTypeFullNames);
    }
}
