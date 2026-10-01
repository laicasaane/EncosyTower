namespace EncosyTower.Databases.Authoring.Generators
{
    public struct HorizontalCollectionSpec : IEquatable<HorizontalCollectionSpec>
    {
        public string targetTypeFullName;
        public EquatableArray<string> propertyNames;

        public readonly bool Equals(HorizontalCollectionSpec other)
            => string.Equals(targetTypeFullName, other.targetTypeFullName, StringComparison.Ordinal)
            && propertyNames.Equals(other.propertyNames)
            ;

        public readonly override bool Equals(object obj)
            => obj is HorizontalCollectionSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(targetTypeFullName, propertyNames);
    }
}
