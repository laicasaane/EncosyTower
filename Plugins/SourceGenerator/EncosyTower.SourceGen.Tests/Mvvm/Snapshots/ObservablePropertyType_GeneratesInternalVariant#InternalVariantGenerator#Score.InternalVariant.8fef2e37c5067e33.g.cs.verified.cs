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

namespace EncosyTower.Mvvm.__InternalVariants__.EncosyTower_SourceGen_Tests_Input
{
    static partial class InternalVariants
    {
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.InternalVariants.InternalVariantGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__UES.Preserve]
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        private partial struct Variant__TestProject_Score : g__ETV.IVariant<global::TestProject.Score>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(g__ETV.VariantBase.META_OFFSET)]
            [g__UES.Preserve]
            public readonly g__ETV.Variant<global::TestProject.Score> Variant;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(g__ETV.VariantBase.DATA_OFFSET)]
            [g__UES.Preserve]
            public readonly global::TestProject.Score Value;

            [g__UES.Preserve]
            public Variant__TestProject_Score(global::TestProject.Score value)
            {
                this.Variant = new g__ETV.Variant(g__ETV.VariantTypeKind.ValueType, g__ETV.Variant<global::TestProject.Score>.TypeId);
                this.Value = value;
            }

            [g__UES.Preserve]
            public Variant__TestProject_Score(in g__ETV.Variant<global::TestProject.Score> variant) : this()
            {
                this.Variant = variant;
            }

            [g__UES.Preserve]
            public Variant__TestProject_Score(in g__ETV.Variant variant) : this()
            {
                ValidateTypeId(variant);
                this.Variant = variant;
            }

            [g__UES.Preserve]
            private static void ValidateTypeId(in g__ETV.Variant variant)
            {
                if (variant.TypeId != g__ETV.Variant<global::TestProject.Score>.TypeId)
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
                    $"Cannot cast {type} to {typeof(global::TestProject.Score)}"
                );
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            [g__UES.Preserve]
            public static implicit operator Variant__TestProject_Score(global::TestProject.Score value) => new Variant__TestProject_Score(value);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            [g__UES.Preserve]
            public static implicit operator g__ETV.Variant(in Variant__TestProject_Score value) => value.Variant;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            [g__UES.Preserve]
            public static implicit operator g__ETV.Variant<global::TestProject.Score>(in Variant__TestProject_Score value) => value.Variant;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            [g__UES.Preserve]
            public static implicit operator Variant__TestProject_Score(in g__ETV.Variant<global::TestProject.Score> value) => new Variant__TestProject_Score(value);

            [g__UES.Preserve]
            public sealed class Converter : g__ETVC.IVariantConverter<global::TestProject.Score>
            {
                [g__UES.Preserve]
                public static readonly Converter Default = new Converter();

                [g__UES.Preserve]
                private Converter() { }

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                [g__UES.Preserve]
                public g__ETV.Variant ToVariant(global::TestProject.Score value) => new Variant__TestProject_Score(value);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                [g__UES.Preserve]
                public g__ETV.Variant<global::TestProject.Score> ToVariantT(global::TestProject.Score value) => new Variant__TestProject_Score(value).Variant;

                [g__UES.Preserve]
                public global::TestProject.Score GetValue(in g__ETV.Variant variant)
                {
                    if (variant.TypeId != g__ETV.Variant<global::TestProject.Score>.TypeId)
                    {
                        ThrowIfInvalidCast();
                    }

                    var temp = new Variant__TestProject_Score(variant);
                    return temp.Value;
                }

                [g__UES.Preserve]
                public bool TryGetValue(in g__ETV.Variant variant, out global::TestProject.Score result)
                {
                    if (variant.TypeId == g__ETV.Variant<global::TestProject.Score>.TypeId)
                    {
                        var temp = new Variant__TestProject_Score(variant);
                        result = temp.Value;
                        return true;
                    }

                    result = default;
                    return false;
                }

                [g__UES.Preserve]
                public bool TrySetValueTo(in g__ETV.Variant variant, ref global::TestProject.Score result)
                {
                    if (variant.TypeId == g__ETV.Variant<global::TestProject.Score>.TypeId)
                    {
                        var temp = new Variant__TestProject_Score(variant);
                        result = temp.Value;
                        return true;
                    }

                    return false;
                }

                [g__UES.Preserve]
                public string ToString(in g__ETV.Variant variant)
                {
                    if (variant.TypeId == g__ETV.Variant<global::TestProject.Score>.TypeId)
                    {
                        var temp = new Variant__TestProject_Score(variant);
                        return temp.Value.ToString();
                    }

                    return g__ETT.TypeIdExtensions.ToType(variant.TypeId).ToString();
                }

                [g__SDCA.DoesNotReturn]
                private static void ThrowIfInvalidCast()
                {
                    throw new g__S.InvalidCastException
                    (
                        $"Cannot get value of {typeof(global::TestProject.Score)} from the input variant."
                    );
                }

            }

        }
    }
}


