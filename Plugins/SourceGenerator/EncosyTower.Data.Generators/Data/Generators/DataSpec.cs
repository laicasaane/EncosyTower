using EncosyTower.SourceGen.Data.Helpers;

namespace EncosyTower.Data.Generators.Data
{
    public partial struct DataSpec : IEquatable<DataSpec>
    {

        public string typeName;
        public string readOnlyTypeName;
        public string typeIdentifier;
        public string typeValidIdentifier;
        public string baseTypeName;
        public string accessibilityKeyword;
        public string hintName;
        public string openingSource;
        public string closingSource;
        public string idPropertyTypeName;
        public DataFieldPolicy fieldPolicy;
        public bool isMutable;
        public bool isValueType;
        public bool withoutPropertySetters;
        public bool withReadOnlyView;
        public bool isSealed;
        public bool hasSerializableAttribute;
        public bool hasGeneratePropertyBagAttribute;
        public bool hasGetHashCodeMethod;
        public bool hasEqualsMethod;
        public bool hasIEquatableMethod;
        public bool withoutId;
        public EquatableArray<OrderData> orders;
        public EquatableArray<FieldRefData> fieldRefs;
        public EquatableArray<PropRefData> propRefs;
        public EquatableArray<string> overrideEquals;
        public EquatableArray<ContainingTypeSpec> containingTypes;

        public readonly bool HasBaseType => string.IsNullOrEmpty(baseTypeName) == false;

        public readonly bool HasIdProperty => string.IsNullOrEmpty(idPropertyTypeName) == false;

        public readonly bool IsValid => string.IsNullOrEmpty(typeName) == false;

        public readonly bool Equals(DataSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(readOnlyTypeName, other.readOnlyTypeName, StringComparison.Ordinal)
            && string.Equals(typeIdentifier, other.typeIdentifier, StringComparison.Ordinal)
            && string.Equals(typeValidIdentifier, other.typeValidIdentifier, StringComparison.Ordinal)
            && string.Equals(baseTypeName, other.baseTypeName, StringComparison.Ordinal)
            && string.Equals(accessibilityKeyword, other.accessibilityKeyword, StringComparison.Ordinal)
            && string.Equals(idPropertyTypeName, other.idPropertyTypeName, StringComparison.Ordinal)
            && fieldPolicy == other.fieldPolicy
            && isMutable == other.isMutable
            && isValueType == other.isValueType
            && withoutPropertySetters == other.withoutPropertySetters
            && withReadOnlyView == other.withReadOnlyView
            && isSealed == other.isSealed
            && hasSerializableAttribute == other.hasSerializableAttribute
            && hasGeneratePropertyBagAttribute == other.hasGeneratePropertyBagAttribute
            && hasGetHashCodeMethod == other.hasGetHashCodeMethod
            && hasEqualsMethod == other.hasEqualsMethod
            && hasIEquatableMethod == other.hasIEquatableMethod
            && withoutId == other.withoutId
            && orders.Equals(other.orders)
            && fieldRefs.Equals(other.fieldRefs)
            && propRefs.Equals(other.propRefs)
            && overrideEquals.Equals(other.overrideEquals)
            && containingTypes.Equals(other.containingTypes);

        public readonly override bool Equals(object obj)
            => obj is DataSpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash = hash.Add(typeName);
            hash = hash.Add(readOnlyTypeName);
            hash = hash.Add(typeIdentifier);
            hash = hash.Add(typeValidIdentifier);
            hash = hash.Add(baseTypeName);
            hash = hash.Add(accessibilityKeyword);
            hash = hash.Add(idPropertyTypeName);
            hash = hash.Add(fieldPolicy);
            hash = hash.Add(isMutable);
            hash = hash.Add(isValueType);
            hash = hash.Add(withoutPropertySetters);
            hash = hash.Add(withReadOnlyView);
            hash = hash.Add(isSealed);
            hash = hash.Add(hasSerializableAttribute);
            hash = hash.Add(hasGeneratePropertyBagAttribute);
            hash = hash.Add(hasGetHashCodeMethod);
            hash = hash.Add(hasEqualsMethod);
            hash = hash.Add(hasIEquatableMethod);
            hash = hash.Add(withoutId);
            hash = hash.Add(orders);
            hash = hash.Add(fieldRefs);
            hash = hash.Add(propRefs);
            hash = hash.Add(overrideEquals);
            hash = hash.Add(containingTypes);
            return hash.ToHashCode();
        }
    }
}
