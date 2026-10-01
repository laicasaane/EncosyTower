using System.Runtime.CompilerServices;
using EncosyTower.Types;

namespace EncosyTower.Variants.Converters
{
    internal sealed class VariantConverterUndefined<T> : IVariantConverter<T>
    {
        public static readonly VariantConverterUndefined<T> Default = new();

        private VariantConverterUndefined() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant ToVariant(T value)
            => new(VariantTypeKind.Undefined, (TypeId)Type<T>.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant<T> ToVariantT(T value)
            => new Variant(VariantTypeKind.Undefined, (TypeId)Type<T>.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(in Variant variant)
        {
            ThrowHelper.ThrowIfUndefinedInvalidCast<T>(false);
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Variant variant, out T result)
        {
            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Variant variant, ref T dest)
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Variant variant)
        {
            return variant.ToString();
        }

    }
}
