#pragma warning disable 0219

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SRIS = global::System.Runtime.InteropServices;
using g__ETT = global::EncosyTower.Types;
using g__ETV = global::EncosyTower.Variants;
using g__ETVC = global::EncosyTower.Variants.Converters;
using g__ETVSG = global::EncosyTower.Variants.SourceGen;
using g__UE = global::UnityEngine;
using g__UES = global::UnityEngine.Scripting;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace



#pragma warning disable

namespace EncosyTower.Variants.__InternalVariants__.I_EncosyTower_x002ESourceGen_x002ETests_x002EInput
{
    static partial class InternalVariants
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.Variants.InternalVariantGenerator", "0.1.7-preview.3")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__UES.Preserve]
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        private partial struct Variant__I_UnityEngine_x002EVector2 : g__ETV.IVariant<global::UnityEngine.Vector2>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(g__ETV.VariantBase.META_OFFSET)]
            [g__UES.Preserve]
            public readonly g__ETV.Variant<global::UnityEngine.Vector2> Variant;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(g__ETV.VariantBase.DATA_OFFSET)]
            [g__UES.Preserve]
            public readonly global::UnityEngine.Vector2 Value;

            [g__UES.Preserve]
            public Variant__I_UnityEngine_x002EVector2(global::UnityEngine.Vector2 value)
            {
                this.Variant = new g__ETV.Variant(g__ETV.VariantTypeKind.ValueType, g__ETV.Variant<global::UnityEngine.Vector2>.TypeId);
                this.Value = value;
            }

            [g__UES.Preserve]
            public Variant__I_UnityEngine_x002EVector2(in g__ETV.Variant<global::UnityEngine.Vector2> variant) : this()
            {
                this.Variant = variant;
            }

            [g__UES.Preserve]
            public Variant__I_UnityEngine_x002EVector2(in g__ETV.Variant variant) : this()
            {
                ValidateTypeId(variant);
                this.Variant = variant;
            }

            [g__UES.Preserve]
            private static void ValidateTypeId(in g__ETV.Variant variant)
            {
                if (variant.TypeId != g__ETV.Variant<global::UnityEngine.Vector2>.TypeId)
                {
                    ThrowIfInvalidCast(variant);
                }
            }

            [g__SDCA.DoesNotReturn]
            private static void ThrowIfInvalidCast(in g__ETV.Variant variant)
            {
                var type = g__ETT.TypeIdExtensions.ToType(variant.TypeId);

                throw new g__S.InvalidCastException
                (
                    $"Cannot cast {type} to {typeof(global::UnityEngine.Vector2)}"
                );
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            [g__UES.Preserve]
            public static implicit operator Variant__I_UnityEngine_x002EVector2(global::UnityEngine.Vector2 value) => new Variant__I_UnityEngine_x002EVector2(value);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            [g__UES.Preserve]
            public static implicit operator g__ETV.Variant(in Variant__I_UnityEngine_x002EVector2 value) => value.Variant;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            [g__UES.Preserve]
            public static implicit operator g__ETV.Variant<global::UnityEngine.Vector2>(in Variant__I_UnityEngine_x002EVector2 value) => value.Variant;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            [g__UES.Preserve]
            public static implicit operator Variant__I_UnityEngine_x002EVector2(in g__ETV.Variant<global::UnityEngine.Vector2> value) => new Variant__I_UnityEngine_x002EVector2(value);

            [g__UES.Preserve]
            public sealed class Converter : g__ETVC.IVariantConverter<global::UnityEngine.Vector2>
            {
                [g__UES.Preserve]
                public static readonly Converter Default = new Converter();

                [g__UES.Preserve]
                private Converter() { }

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                [g__UES.Preserve]
                public g__ETV.Variant ToVariant(global::UnityEngine.Vector2 value) => new Variant__I_UnityEngine_x002EVector2(value);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                [g__UES.Preserve]
                public g__ETV.Variant<global::UnityEngine.Vector2> ToVariantT(global::UnityEngine.Vector2 value) => new Variant__I_UnityEngine_x002EVector2(value).Variant;

                [g__UES.Preserve]
                public global::UnityEngine.Vector2 GetValue(in g__ETV.Variant variant)
                {
                    if (variant.TypeId != g__ETV.Variant<global::UnityEngine.Vector2>.TypeId)
                    {
                        ThrowIfInvalidCast();
                    }

                    var temp = new Variant__I_UnityEngine_x002EVector2(variant);
                    return temp.Value;
                }

                [g__UES.Preserve]
                public bool TryGetValue(in g__ETV.Variant variant, out global::UnityEngine.Vector2 result)
                {
                    if (variant.TypeId == g__ETV.Variant<global::UnityEngine.Vector2>.TypeId)
                    {
                        var temp = new Variant__I_UnityEngine_x002EVector2(variant);
                        result = temp.Value;
                        return true;
                    }

                    result = default;
                    return false;
                }

                [g__UES.Preserve]
                public bool TrySetValueTo(in g__ETV.Variant variant, ref global::UnityEngine.Vector2 result)
                {
                    if (variant.TypeId == g__ETV.Variant<global::UnityEngine.Vector2>.TypeId)
                    {
                        var temp = new Variant__I_UnityEngine_x002EVector2(variant);
                        result = temp.Value;
                        return true;
                    }

                    return false;
                }

                [g__UES.Preserve]
                public string ToString(in g__ETV.Variant variant)
                {
                    if (variant.TypeId == g__ETV.Variant<global::UnityEngine.Vector2>.TypeId)
                    {
                        var temp = new Variant__I_UnityEngine_x002EVector2(variant);
                        return temp.Value.ToString();
                    }

                    return g__ETT.TypeIdExtensions.ToType(variant.TypeId).ToString();
                }

                [g__SDCA.DoesNotReturn]
                private static void ThrowIfInvalidCast()
                {
                    throw new g__S.InvalidCastException
                    (
                        $"Cannot get value of {typeof(global::UnityEngine.Vector2)} from the input variant."
                    );
                }

            }

        }
    }
}


