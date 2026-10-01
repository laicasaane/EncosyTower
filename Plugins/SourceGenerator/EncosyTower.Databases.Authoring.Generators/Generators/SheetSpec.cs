namespace EncosyTower.Databases.Authoring.Generators
{
    public struct SheetSpec : IEquatable<SheetSpec>
    {
        public string sheetName;
        public string idTypeFullName;
        public string idTypeSimpleName;
        public string dataTypeFullName;
        public string dataTypeSimpleName;
        public string tableTypeFullName;
        public EquatableArray<string> nestedDataTypeFullNames;
        public EquatableArray<HorizontalCollectionSpec> horizontalCollections;
        public EquatableArray<string> generatedKeyTypeFullNames;
        public EquatableArray<GeneratedKeyEqualitySpec> generatedKeyEquality;
        public HashValue64 scopeKey;
        public string hintName;

        public readonly bool IsValid
            => string.IsNullOrEmpty(idTypeFullName) == false
            && string.IsNullOrEmpty(dataTypeFullName) == false;

        public readonly bool Equals(SheetSpec other)
            => string.Equals(idTypeFullName, other.idTypeFullName, StringComparison.Ordinal)
            && string.Equals(idTypeSimpleName, other.idTypeSimpleName, StringComparison.Ordinal)
            && string.Equals(dataTypeFullName, other.dataTypeFullName, StringComparison.Ordinal)
            && string.Equals(dataTypeSimpleName, other.dataTypeSimpleName, StringComparison.Ordinal)
            && string.Equals(tableTypeFullName, other.tableTypeFullName, StringComparison.Ordinal)
            && string.Equals(sheetName, other.sheetName, StringComparison.Ordinal)
            && scopeKey == other.scopeKey
            && nestedDataTypeFullNames.Equals(other.nestedDataTypeFullNames)
            && horizontalCollections.Equals(other.horizontalCollections)
            && generatedKeyTypeFullNames.Equals(other.generatedKeyTypeFullNames)
            && generatedKeyEquality.Equals(other.generatedKeyEquality)
            ;

        public readonly override bool Equals(object obj)
            => obj is SheetSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  idTypeFullName
                , idTypeSimpleName
                , dataTypeFullName
                , dataTypeSimpleName
                , tableTypeFullName
                , sheetName
                , scopeKey
                , nestedDataTypeFullNames
            )
            .Add(horizontalCollections)
            .Add(generatedKeyTypeFullNames)
            .Add(generatedKeyEquality);
    }
}
