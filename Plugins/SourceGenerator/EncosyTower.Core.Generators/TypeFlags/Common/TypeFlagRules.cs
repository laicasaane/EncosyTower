using System.Globalization;

namespace EncosyTower.Core.TypeFlags
{
    internal enum TypeFlagAccess : byte
    {
        Private,
        Internal,
        Public,
    }

    [Flags]
    internal enum TypeFlagApi : byte
    {
        State = 0,
        Self = 1 << 0,
        Related = 1 << 1,
        Async = 1 << 2,
        Default = Self | Related | Async,
    }

    [Flags]
    internal enum TypeFlagMember : byte
    {
        None = 0,
        TypeFlagField = 1 << 0,
        ReadWriteField = 1 << 1,
        ApiType = 1 << 2,
        ReadWriteType = 1 << 3,
    }

    internal enum TypeFlagMemberPlan : byte
    {
        Emit,
        EmitHiding,
        Conflict,
    }

    internal readonly record struct TypeFlagOptions(TypeFlagAccess WriteAccess, TypeFlagApi Api, bool UseExtensions);

    internal readonly record struct TypeFlagOptionValues(
          long WriteAccess
        , long Api
        , bool HasApi
        , bool UseExtensions
        , bool HasErrorValue
    )
    {
        public bool IsWriteAccessDefined => WriteAccess >= 0 && WriteAccess <= (long)TypeFlagAccess.Public;

        public bool IsApiDefined => (Api & ~(long)TypeFlagApi.Default) == 0;

        public bool IsValid => HasErrorValue == false && IsWriteAccessDefined && (UseExtensions || IsApiDefined);

        public TypeFlagOptions ToOptions()
            => new((TypeFlagAccess)WriteAccess, UseExtensions ? TypeFlagApi.Default : (TypeFlagApi)Api, UseExtensions);
    }

    internal static class TypeFlagRules
    {
        public const string NAMESPACE = "EncosyTower.TypeFlags";
        public const string ATTRIBUTE = "global::EncosyTower.TypeFlags.TypeFlagAttribute";
        public const string ATTRIBUTE_METADATA_NAME = "EncosyTower.TypeFlags.TypeFlagAttribute";
        public const string SKIP_ATTRIBUTE = "global::EncosyTower.TypeFlags.SkipSourceGeneratorsForAssemblyAttribute";
        public const string TYPE_FLAG_METADATA_NAME = "EncosyTower.TypeFlags.TypeFlag`1";
        public const string GENERATED_CODE_ATTRIBUTE = "global::System.CodeDom.Compiler.GeneratedCodeAttribute";
        public const string GENERATOR_METADATA_NAME = "EncosyTower.Core.Generators.TypeFlags.TypeFlagGenerator";
        public const string READ_ONLY_NAME = "ReadOnly";
        public const string TYPE_FLAG_NAME = "TypeFlag";
        public const string READ_WRITE_FIELD_NAME = "s_typeFlag";
        public const string API_TYPE_NAME = "TypeFlagAPI";
        public const string READ_WRITE_TYPE_NAME = "TypeFlagReadWrite";
        public const string WRITE_ACCESS_OPTION = "WriteAccess";
        public const string API_OPTION = "Api";
        public const string USE_EXTENSIONS_OPTION = "UseExtensions";

        public static int CountMarkers(INamedTypeSymbol type, INamedTypeSymbol marker, CancellationToken token)
        {
            var attributes = type.GetAttributes();
            var count = 0;

            foreach (var attribute in attributes)
            {
                token.ThrowIfCancellationRequested();

                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, marker))
                {
                    count++;
                }
            }

            return count;
        }

        public static AttributeData GetMarker(INamedTypeSymbol type, INamedTypeSymbol marker, CancellationToken token)
        {
            var attributes = type.GetAttributes();

            foreach (var attribute in attributes)
            {
                token.ThrowIfCancellationRequested();

                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, marker))
                {
                    return attribute;
                }
            }

            return null;
        }

        public static TypeFlagOptionValues ReadOptions(AttributeData marker, CancellationToken token)
        {
            var writeAccess = 0L;
            var api = (long)TypeFlagApi.Default;
            var hasApi = false;
            var useExtensions = false;
            var hasErrorValue = false;

            foreach (var argument in marker.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

                var constant = argument.Value;
                var value = constant.Kind == TypedConstantKind.Error ? null : constant.Value;

                if (value == null)
                {
                    hasErrorValue = true;
                    continue;
                }

                switch (argument.Key)
                {
                    case WRITE_ACCESS_OPTION:
                    {
                        writeAccess = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                        break;
                    }

                    case API_OPTION:
                    {
                        api = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                        hasApi = true;
                        break;
                    }

                    case USE_EXTENSIONS_OPTION:
                    {
                        useExtensions = value is true;
                        break;
                    }
                }
            }

            return new TypeFlagOptionValues(writeAccess, api, hasApi, useExtensions, hasErrorValue);
        }

        public static TypeFlagMember GetMembers(in TypeFlagOptions options)
        {
            if (options.UseExtensions)
            {
                return options.WriteAccess == TypeFlagAccess.Public
                    ? TypeFlagMember.TypeFlagField
                    : TypeFlagMember.TypeFlagField | TypeFlagMember.ReadWriteField;
            }

            if (options.WriteAccess == TypeFlagAccess.Private)
            {
                return TypeFlagMember.TypeFlagField
                    | TypeFlagMember.ReadWriteField
                    | TypeFlagMember.ApiType
                    | TypeFlagMember.ReadWriteType;
            }

            return TypeFlagMember.TypeFlagField | TypeFlagMember.ApiType;
        }

        public static string GetMemberName(TypeFlagMember member)
            => member switch {
                TypeFlagMember.TypeFlagField => TYPE_FLAG_NAME,
                TypeFlagMember.ReadWriteField => READ_WRITE_FIELD_NAME,
                TypeFlagMember.ApiType => API_TYPE_NAME,
                TypeFlagMember.ReadWriteType => READ_WRITE_TYPE_NAME,
                _ => string.Empty,
            };

        public static bool TryGetUnsupportedKind(INamedTypeSymbol owner, out string kind)
        {
            if (owner.IsStatic)
            {
                kind = "a static class";
                return true;
            }

            if (owner.IsRefLikeType)
            {
                kind = "a ref struct";
                return true;
            }

            kind = null;
            return false;
        }

        public static bool HasUnresolvedBase(INamedTypeSymbol owner, CancellationToken token)
        {
            for (var baseType = owner.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                token.ThrowIfCancellationRequested();

                if (baseType.TypeKind == TypeKind.Error)
                {
                    return true;
                }
            }

            return false;
        }

        public static TypeFlagMemberPlan PlanMember(
              INamedTypeSymbol owner
            , TypeFlagMember member
            , INamedTypeSymbol marker
            , INamedTypeSymbol typeFlag
            , Compilation compilation
            , CancellationToken token
            , out ISymbol conflict
        )
        {
            var name = GetMemberName(member);

            if (string.Equals(owner.Name, name, StringComparison.Ordinal))
            {
                conflict = owner;
                return TypeFlagMemberPlan.Conflict;
            }

            if (TryFindOwnMember(owner, name, token, out conflict))
            {
                return TypeFlagMemberPlan.Conflict;
            }

            for (var baseType = owner.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                token.ThrowIfCancellationRequested();

                if (TryPlanBaseMembers(owner, baseType, name, typeFlag, compilation, token, out var plan, out conflict))
                {
                    return plan;
                }

                if (WillGenerateVisibleMember(owner, baseType, member, marker, typeFlag, compilation, token))
                {
                    return TypeFlagMemberPlan.EmitHiding;
                }
            }

            conflict = null;
            return TypeFlagMemberPlan.Emit;
        }

        public static bool TryPlanOwner(
              INamedTypeSymbol owner
            , in TypeFlagOptions options
            , INamedTypeSymbol marker
            , INamedTypeSymbol typeFlag
            , Compilation compilation
            , CancellationToken token
            , out TypeFlagMember hiddenMembers
        )
        {
            var members = GetMembers(options);
            hiddenMembers = TypeFlagMember.None;

            for (var bit = 1; bit <= 8; bit <<= 1)
            {
                token.ThrowIfCancellationRequested();

                var member = (TypeFlagMember)bit;

                if ((members & member) == 0)
                {
                    continue;
                }

                var plan = PlanMember(owner, member, marker, typeFlag, compilation, token, out _);

                if (plan == TypeFlagMemberPlan.Conflict)
                {
                    hiddenMembers = TypeFlagMember.None;
                    return false;
                }

                if (plan == TypeFlagMemberPlan.EmitHiding)
                {
                    hiddenMembers |= member;
                }
            }

            return true;
        }

        private static bool TryFindOwnMember(
              INamedTypeSymbol owner
            , string name
            , CancellationToken token
            , out ISymbol conflict
        )
        {
            var members = owner.GetMembers(name);

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (IsGeneratedByThisGenerator(member, token) == false)
                {
                    conflict = member;
                    return true;
                }
            }

            conflict = null;
            return false;
        }

        private static bool TryPlanBaseMembers(
              INamedTypeSymbol owner
            , INamedTypeSymbol baseType
            , string name
            , INamedTypeSymbol typeFlag
            , Compilation compilation
            , CancellationToken token
            , out TypeFlagMemberPlan plan
            , out ISymbol conflict
        )
        {
            var members = baseType.GetMembers(name);
            var found = false;

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (compilation.IsSymbolAccessibleWithin(member, owner) == false)
                {
                    continue;
                }

                if (IsHideable(member, name, typeFlag, token) == false)
                {
                    plan = TypeFlagMemberPlan.Conflict;
                    conflict = member;
                    return true;
                }

                found = true;
            }

            plan = found ? TypeFlagMemberPlan.EmitHiding : TypeFlagMemberPlan.Emit;
            conflict = null;
            return found;
        }

        private static bool WillGenerateVisibleMember(
              INamedTypeSymbol owner
            , INamedTypeSymbol baseType
            , TypeFlagMember member
            , INamedTypeSymbol marker
            , INamedTypeSymbol typeFlag
            , Compilation compilation
            , CancellationToken token
        )
        {
            var baseDefinition = baseType.OriginalDefinition;

            if (baseDefinition.DeclaringSyntaxReferences.Length < 1
                || CountMarkers(baseDefinition, marker, token) != 1
                || baseDefinition.TypeKind is not (TypeKind.Class or TypeKind.Struct)
                || TryGetUnsupportedKind(baseDefinition, out _)
            )
            {
                return false;
            }

            var values = ReadOptions(GetMarker(baseDefinition, marker, token), token);

            if (values.IsValid == false || HasUnresolvedBase(baseDefinition, token))
            {
                return false;
            }

            var baseOptions = values.ToOptions();

            if ((GetMembers(baseOptions) & member) == 0
                || IsVisible(owner, baseDefinition, member, baseOptions, token) == false
            )
            {
                return false;
            }

            return TryPlanOwner(baseDefinition, baseOptions, marker, typeFlag, compilation, token, out _);
        }

        private static bool IsVisible(
              INamedTypeSymbol owner
            , INamedTypeSymbol baseDefinition
            , TypeFlagMember member
            , in TypeFlagOptions baseOptions
            , CancellationToken token
        )
        {
            switch (member)
            {
                case TypeFlagMember.TypeFlagField:
                case TypeFlagMember.ApiType:
                {
                    return true;
                }

                case TypeFlagMember.ReadWriteField:
                {
                    return (baseOptions.UseExtensions && baseOptions.WriteAccess == TypeFlagAccess.Internal)
                        || IsNestedWithin(owner, baseDefinition, token);
                }

                default:
                {
                    return IsNestedWithin(owner, baseDefinition, token);
                }
            }
        }

        private static bool IsNestedWithin(INamedTypeSymbol owner, INamedTypeSymbol container, CancellationToken token)
        {
            for (var type = owner.ContainingType; type != null; type = type.ContainingType)
            {
                token.ThrowIfCancellationRequested();

                if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, container))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsHideable(ISymbol member, string name, INamedTypeSymbol typeFlag, CancellationToken token)
        {
            if (IsGeneratedByThisGenerator(member, token))
            {
                return true;
            }

            if (string.Equals(name, TYPE_FLAG_NAME, StringComparison.Ordinal) == false
                && string.Equals(name, READ_WRITE_FIELD_NAME, StringComparison.Ordinal) == false
            )
            {
                return false;
            }

            if (member.IsStatic == false)
            {
                return false;
            }

            var memberType = member switch {
                IFieldSymbol field => field.Type,
                IPropertySymbol { IsIndexer: false } property => property.Type,
                _ => null,
            };

            if (memberType is not INamedTypeSymbol namedType)
            {
                return false;
            }

            if (SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, typeFlag))
            {
                return true;
            }

            return string.Equals(namedType.Name, READ_ONLY_NAME, StringComparison.Ordinal)
                && namedType.ContainingType != null
                && SymbolEqualityComparer.Default.Equals(namedType.ContainingType.OriginalDefinition, typeFlag);
        }

        private static bool IsGeneratedByThisGenerator(ISymbol member, CancellationToken token)
        {
            var attributes = member.GetAttributes();

            foreach (var attribute in attributes)
            {
                token.ThrowIfCancellationRequested();

                if (attribute.AttributeClass.HasFullName(GENERATED_CODE_ATTRIBUTE, token)
                    && attribute.ConstructorArguments.Length > 0
                    && attribute.ConstructorArguments[0].Value is string tool
                    && string.Equals(tool, GENERATOR_METADATA_NAME, StringComparison.Ordinal)
                )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
