using System.Runtime.CompilerServices;

namespace EncosyTower.Variants.Converters
{
    internal sealed class VariantConverterObject : IVariantConverter<object>
    {
        public static readonly VariantConverterObject Default = new();

        private VariantConverterObject() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant ToVariant(object value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Variant<object> ToVariantT(object value)
            => new Variant(value);

        public object GetValue(in Variant variant)
        {
            var validCast = variant.TryGetValue(out object result);
            ThrowHelper.ThrowIfObjectInvalidCast(validCast);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in Variant variant, out object result)
            => variant.TryGetValue(out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetValueTo(in Variant variant, ref object dest)
            => variant.TrySetValueTo(ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(in Variant variant)
            => variant.Object?.ToString() ?? string.Empty;

    }
}
