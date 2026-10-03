namespace EncosyTower.Entities.Stats.Generators
{
    internal partial struct StatCollectionSpec : IEquatable<StatCollectionSpec>
    {
        public string typeName;
        public string typeNamespace;
        public string typeIdentifier;
        public string statSystemFullTypeName;
        public string hintName;
        public string openingSource;
        public string closingSource;
        public string typeFullName;
        public string extensionsOpeningSource;
        public string extensionsClosingSource;
        public EquatableArray<StatDataSpec> statDataCollection;
        public EquatableArray<ContainingTypeSpec> containingTypes;
        public uint typeIdOffset;
        public Accessibility typeAccessibility;
        public ExtensionsPlacement extensionsPlacement;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeNamespace) == false
            && string.IsNullOrEmpty(statSystemFullTypeName) == false
            && statDataCollection.IsEmpty == false
            ;

        public readonly bool HasNamespaceExtensions
            => extensionsPlacement is ExtensionsPlacement.NamespacePublic or ExtensionsPlacement.NamespaceInternal;

        public readonly bool Equals(StatCollectionSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeNamespace, other.typeNamespace, StringComparison.Ordinal)
            && string.Equals(statSystemFullTypeName, other.statSystemFullTypeName, StringComparison.Ordinal)
            && statDataCollection.Equals(other.statDataCollection)
            && containingTypes.Equals(other.containingTypes)
            && typeIdOffset == other.typeIdOffset
            && string.Equals(typeFullName, other.typeFullName, StringComparison.Ordinal)
            && typeAccessibility == other.typeAccessibility
            && extensionsPlacement == other.extensionsPlacement
            ;

        public readonly override bool Equals(object obj)
            => obj is StatCollectionSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(typeName, typeNamespace, statSystemFullTypeName, statDataCollection)
            .Add(containingTypes)
            .Add(typeIdOffset)
            .Add(typeFullName)
            .Add(typeAccessibility)
            .Add(extensionsPlacement)
            ;

        internal partial struct StatDataSpec : IEquatable<StatDataSpec>
        {
            public string typeName;
            public string fieldName;
            public string valueTypeNamespace;
            public string valueType;
            public string valueTypeFullName;
            public bool singleValue;

            public readonly bool IsValid
                => string.IsNullOrEmpty(typeName) == false
                && string.IsNullOrEmpty(fieldName) == false
                && string.IsNullOrEmpty(valueType) == false
                ;

            public readonly override bool Equals(object obj)
                => obj is StatDataSpec other && Equals(other);

            public readonly bool Equals(StatDataSpec other)
                => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
                && string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
                && string.Equals(valueTypeNamespace, other.valueTypeNamespace, StringComparison.Ordinal)
                && string.Equals(valueType, other.valueType, StringComparison.Ordinal)
                && string.Equals(valueTypeFullName, other.valueTypeFullName, StringComparison.Ordinal)
                && singleValue == other.singleValue
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(typeName, fieldName, valueTypeNamespace, valueType, singleValue)
                .Add(valueTypeFullName);
        }

        internal enum ExtensionsPlacement : byte
        {
            BesideTopLevelPublic,
            BesideTopLevelInternal,
            NamespacePublic,
            NamespaceInternal,
            BesideNestedType,
        }
    }
}
