using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Helpers.Variants
{
    internal static class InternalVariantSpecFactory
    {
        internal static InternalVariantSpec Create(INamedTypeSymbol typeArg, CancellationToken token)
            => Create(typeArg, typeArg.ToValidIdentifier(), token);

        internal static InternalVariantSpec Create(
              INamedTypeSymbol typeArg
            , string generatedIdentifier
            , CancellationToken token
        )
            => Create((ITypeSymbol)typeArg, generatedIdentifier, token);

        internal static InternalVariantSpec Create(
              ITypeSymbol typeArg
            , string generatedIdentifier
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var fullTypeName = typeArg.ToFullName();

            if (fullTypeName.ToUnionType().IsNativeUnionType())
            {
                return default;
            }

            var isValueType = typeArg.IsUnmanagedType;
            int? unmanagedSize = null;

            if (isValueType && typeArg is INamedTypeSymbol named)
            {
                var size = 0;
                var alignment = 1;
                named.GetUnmanagedSizeAndAlignment(ref size, ref alignment, token);
                unmanagedSize = size;
            }

            return new InternalVariantSpec {
                fullTypeName = fullTypeName,
                typeIdentity = typeArg.ToMetadataIdentity(),
                simpleTypeName = typeArg.ToFullNameNoGlobal(),
                generatedIdentifier = generatedIdentifier,
                structName = $"Variant__{generatedIdentifier}",
                converterDefault = $"Variant__{generatedIdentifier}.Converter.Default",
                fileHintName = typeArg.ToFileName(),
                unmanagedSize = unmanagedSize,
                isValueType = isValueType,
                hasImplicitFromStructToType = isValueType || typeArg.TypeKind != TypeKind.Interface,
                isValid = true,
            };
        }
    }
}
