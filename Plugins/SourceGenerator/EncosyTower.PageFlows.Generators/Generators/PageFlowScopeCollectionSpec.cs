namespace EncosyTower.PageFlows.Generators
{
    internal readonly partial struct PageFlowScopeCollectionSpec : IEquatable<PageFlowScopeCollectionSpec>
    {
        public readonly string OpeningSource;
        public readonly string ClosingSource;
        public readonly string TypeName;
        public readonly string MetadataName;
        public readonly string AssemblyName;
        public readonly string HintName;
        public readonly EquatableArray<string> PropertyNames;

        public PageFlowScopeCollectionSpec(
              string openingSource
            , string closingSource
            , string typeName
            , string metadataName
            , string assemblyName
            , string hintName
            , EquatableArray<string> propertyNames
        )
        {
            OpeningSource = openingSource;
            ClosingSource = closingSource;
            TypeName = typeName;
            MetadataName = metadataName;
            AssemblyName = assemblyName;
            HintName = hintName;
            PropertyNames = propertyNames;
        }

        public bool IsValid => string.IsNullOrEmpty(TypeName) == false && PropertyNames.Count > 0;

        public static bool operator ==(PageFlowScopeCollectionSpec left, PageFlowScopeCollectionSpec right)
            => left.Equals(right);

        public static bool operator !=(PageFlowScopeCollectionSpec left, PageFlowScopeCollectionSpec right)
            => left.Equals(right) == false;

        public readonly bool Equals(PageFlowScopeCollectionSpec other)
            => string.Equals(OpeningSource, other.OpeningSource, StringComparison.Ordinal)
            && string.Equals(ClosingSource, other.ClosingSource, StringComparison.Ordinal)
            && string.Equals(TypeName, other.TypeName, StringComparison.Ordinal)
            && string.Equals(MetadataName, other.MetadataName, StringComparison.Ordinal)
            && string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal)
            && PropertyNames.Equals(other.PropertyNames)
            ;

        public readonly override bool Equals(object obj)
            => obj is PageFlowScopeCollectionSpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash = hash.Add(OpeningSource);
            hash = hash.Add(ClosingSource);
            hash = hash.Add(TypeName);
            hash = hash.Add(MetadataName);
            hash = hash.Add(AssemblyName);
            hash = hash.Add(PropertyNames);
            return hash.ToHashCode();
        }
    }
}
