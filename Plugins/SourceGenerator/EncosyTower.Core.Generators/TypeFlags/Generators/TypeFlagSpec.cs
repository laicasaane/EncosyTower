using EncosyTower.Core.TypeFlags;

namespace EncosyTower.Core.Generators.TypeFlags
{
    internal readonly partial struct TypeFlagSpec : IEquatable<TypeFlagSpec>
    {
        public readonly string AssemblyName;
        public readonly string MetadataName;
        public readonly EquatableArray<Declaration> Declarations;
        public readonly bool IsValueType;
        public readonly TypeFlagOptions Options;
        public readonly TypeFlagMember HiddenMembers;
        public readonly string NamespaceName;
        public readonly EquatableArray<string> ContainingTypeHeaders;
        public readonly string DeclarationHeader;
        public readonly string FullTypeName;
        public readonly string HintName;

        public TypeFlagSpec(
              string assemblyName
            , string metadataName
            , EquatableArray<Declaration> declarations
            , bool isValueType
            , TypeFlagOptions options
            , TypeFlagMember hiddenMembers
            , string namespaceName
            , EquatableArray<string> containingTypeHeaders
            , string declarationHeader
            , string fullTypeName
            , string hintName
        )
        {
            AssemblyName = assemblyName;
            MetadataName = metadataName;
            Declarations = declarations;
            IsValueType = isValueType;
            Options = options;
            HiddenMembers = hiddenMembers;
            NamespaceName = namespaceName;
            ContainingTypeHeaders = containingTypeHeaders;
            DeclarationHeader = declarationHeader;
            FullTypeName = fullTypeName;
            HintName = hintName;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => string.IsNullOrEmpty(MetadataName) == false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TypeFlagSpec left, TypeFlagSpec right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TypeFlagSpec left, TypeFlagSpec right)
            => left.Equals(right) == false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(TypeFlagSpec other)
            => string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal)
            && string.Equals(MetadataName, other.MetadataName, StringComparison.Ordinal)
            && Declarations.Equals(other.Declarations)
            && IsValueType == other.IsValueType
            && Options == other.Options
            && HiddenMembers == other.HiddenMembers
            ;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is TypeFlagSpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = HashValue.Combine(AssemblyName, MetadataName, IsValueType, Options, HiddenMembers);

            foreach (var declaration in Declarations)
            {
                hash = hash.Add(declaration);
            }

            return hash.ToHashCode();
        }

        public readonly record struct Declaration(string Keyword, string TypeParameterNames);
    }
}
