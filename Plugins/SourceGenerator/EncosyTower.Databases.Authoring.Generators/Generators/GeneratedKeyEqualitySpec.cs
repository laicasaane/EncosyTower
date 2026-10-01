namespace EncosyTower.Databases.Authoring.Generators
{
    public enum GeneratedKeyEqualityMode
    {
        Generate,
        Preserve,
        Invalid,
    }

    public struct GeneratedKeyEqualitySpec : IEquatable<GeneratedKeyEqualitySpec>
    {
        public string typeFullName;
        public GeneratedKeyEqualityMode mode;

        public readonly bool Equals(GeneratedKeyEqualitySpec other)
            => string.Equals(typeFullName, other.typeFullName, StringComparison.Ordinal)
            && mode == other.mode;

        public readonly override bool Equals(object obj)
            => obj is GeneratedKeyEqualitySpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(typeFullName, mode);
    }
}
