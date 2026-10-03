using System;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen
{
    public static class GeneratorHelpers
    {
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.CodeGen.SkipSourceGeneratorsForAssemblyAttribute";
        private const string ALLOW_ATTRIBUTE = "global::EncosyTower.CodeGen.AllowSourceGeneratorsForAssemblyAttribute";

        public const string FIELD_PREFIX_UNDERSCORE = "_";
        public const string FIELD_PREFIX_M_UNDERSCORE = "m_";

        public static bool IsValidCompilation(
              this Compilation compilation
            , CancellationToken token
            , string generatorNamespace
            , string skipAttribute
        )
        {
            token.ThrowIfCancellationRequested();

            var assembly = compilation.Assembly;
            var skipAllSourceGen = assembly.HasAttribute(SKIP_ATTRIBUTE, token);
            var skipThisSourceGen = CanSkipThisSourceGen(assembly, skipAttribute, token);
            var isAllowed = IsThisSourceGenAllowed(assembly, generatorNamespace, token);

            return (skipAllSourceGen && isAllowed)
                || (skipAllSourceGen == false && skipThisSourceGen == false);

            static bool CanSkipThisSourceGen(IAssemblySymbol assembly, string skipAttribute, CancellationToken token)
            {
                return string.IsNullOrWhiteSpace(skipAttribute) == false
                    && assembly.HasAttribute(skipAttribute, token);
            }

            static bool IsThisSourceGenAllowed(
                  IAssemblySymbol assembly
                , string generatorNamespace
                , CancellationToken token
            )
            {
                if (string.IsNullOrWhiteSpace(generatorNamespace)
                    || assembly.TryGetAttribute(ALLOW_ATTRIBUTE, out var allowAttrib, token) == false
                )
                {
                    return false;
                }

                var args = allowAttrib.ConstructorArguments;

                if (args.Length < 1 || args[0].Kind != TypedConstantKind.Array)
                {
                    return false;
                }

                var values = args[0].Values;

                foreach (var value in values)
                {
                    token.ThrowIfCancellationRequested();

                    if (value.Value is string ns && string.Equals(ns, generatorNamespace, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static string ToPropertyName(this IFieldSymbol field)
        {
            return ToPropertyName(field.Name);
        }

        public static string ToPropertyName(this string fieldName)
        {
            var nameSpan = fieldName.AsSpan();
            var prefix = FIELD_PREFIX_UNDERSCORE.AsSpan();

            if (nameSpan.StartsWith(prefix))
            {
                return MakeFirstCharUpperCase(nameSpan.Slice(1));
            }

            prefix = FIELD_PREFIX_M_UNDERSCORE.AsSpan();

            if (nameSpan.StartsWith(prefix))
            {
                return MakeFirstCharUpperCase(nameSpan.Slice(2));
            }

            return MakeFirstCharUpperCase(nameSpan);
        }

        public static string ToPublicFieldName(this IPropertySymbol property)
        {
            return MakeFirstCharLowerCase(property.Name.AsSpan());
        }

        public static string ToPublicFieldName(this string propertyName)
        {
            return MakeFirstCharLowerCase(propertyName.AsSpan());
        }

        public static string ToPrivateFieldName(this IPropertySymbol property)
        {
            return $"{FIELD_PREFIX_UNDERSCORE}{MakeFirstCharLowerCase(property.Name.AsSpan())}";
        }

        public static string ToPrivateFieldName(this string propertyName)
        {
            return $"{FIELD_PREFIX_UNDERSCORE}{MakeFirstCharLowerCase(propertyName.AsSpan())}";
        }

        public static string ToArgumentName(this string propertyName)
        {
            return $"{MakeFirstCharLowerCase(propertyName.AsSpan())}";
        }

        public static string MakeFirstCharUpperCase(this ReadOnlySpan<char> value)
        {
            return $"{char.ToUpper(value[0])}{value.Slice(1).ToString()}";
        }

        public static string MakeFirstCharLowerCase(this ReadOnlySpan<char> value)
        {
            return $"{char.ToLower(value[0])}{value.Slice(1).ToString()}";
        }

        public static string GetKeyword(this Accessibility accessibility)
            => accessibility switch {
                Accessibility.Public => "public",
                Accessibility.Private => "private",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.ProtectedAndInternal => "private protected",
                _ => ""
            };

        public static string GetFixedStringTypeName(int maxByteCount)
            => maxByteCount switch {
                <= 32 - 3 => "FixedString32Bytes",
                <= 64 - 3 => "FixedString64Bytes",
                <= 128 - 3 => "FixedString128Bytes",
                <= 512 - 3 => "FixedString512Bytes",
                _ => "FixedString4096Bytes",
            };

        public static string GetPrintFixedStringTypeName(int maxByteCount)
            => maxByteCount switch {
                <= 32 - 3 => "g__UC.FixedString32Bytes",
                <= 64 - 3 => "g__UC.FixedString64Bytes",
                <= 128 - 3 => "g__UC.FixedString128Bytes",
                <= 512 - 3 => "g__UC.FixedString512Bytes",
                _ => "g__UC.FixedString4096Bytes",
            };

        public static string GetEnumUnderlyingTypeFromMemberCount(ulong memberCount)
            => GetEnumUnderlyingTypeFromSize(DetermineEnumSizeFromMemberCount(memberCount));

        public static string GetEnumUnderlyingTypeFromSize(int size)
            => size switch {
                <= 1 => "byte",
                <= 2 => "ushort",
                <= 4 => "uint",
                _ => "ulong",
            };

        public static int DetermineEnumSizeFromMemberCount(long memberCount)
            => memberCount switch {
                <= byte.MaxValue => 1,
                <= ushort.MaxValue => 2,
                <= uint.MaxValue => 4,
                _ => 8,
            };

        public static int DetermineEnumSizeFromMemberCount(ulong memberCount)
            => memberCount switch {
                <= byte.MaxValue => 1,
                <= ushort.MaxValue => 2,
                <= uint.MaxValue => 4,
                _ => 8,
            };
    }
}
