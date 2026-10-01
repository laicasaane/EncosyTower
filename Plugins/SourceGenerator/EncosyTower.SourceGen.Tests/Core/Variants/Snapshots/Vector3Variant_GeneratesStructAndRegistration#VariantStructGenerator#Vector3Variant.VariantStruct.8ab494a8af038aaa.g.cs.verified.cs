#pragma warning disable 0219

using EncosyTower.Variants;

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


namespace TestProject
{



#pragma warning disable

[g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.Variants.VariantStructGenerator", "0.1.8-preview.1")]
[g__SDCA.ExcludeFromCodeCoverage]
[g__UES.Preserve]
partial struct Vector3Variant : g__ETV.IVariant<global::UnityEngine.Vector3>
{
    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(g__ETV.VariantBase.META_OFFSET)]
    [g__UES.Preserve]
    public readonly g__ETV.Variant<global::UnityEngine.Vector3> Variant;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(g__ETV.VariantBase.DATA_OFFSET)]
    [g__UES.Preserve]
    public readonly global::UnityEngine.Vector3 Value;

    [g__UES.Preserve]
    public Vector3Variant(global::UnityEngine.Vector3 value)
    {
        this.Variant = new g__ETV.Variant(g__ETV.VariantTypeKind.ValueType, g__ETV.Variant<global::UnityEngine.Vector3>.TypeId);
        this.Value = value;
    }

    [g__UES.Preserve]
    public Vector3Variant(in g__ETV.Variant<global::UnityEngine.Vector3> variant) : this()
    {
        this.Variant = variant;
    }

    [g__UES.Preserve]
    public Vector3Variant(in g__ETV.Variant variant) : this()
    {
        ValidateTypeId(variant);
        this.Variant = variant;
    }

    [g__UES.Preserve]
    private static void ValidateTypeId(in g__ETV.Variant variant)
    {
        if (variant.TypeId != g__ETV.Variant<global::UnityEngine.Vector3>.TypeId)
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
            $"Cannot cast {type} to {typeof(global::UnityEngine.Vector3)}"
        );
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    [g__UES.Preserve]
    public static implicit operator Vector3Variant(global::UnityEngine.Vector3 value) => new Vector3Variant(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    [g__UES.Preserve]
    public static implicit operator g__ETV.Variant(in Vector3Variant value) => value.Variant;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    [g__UES.Preserve]
    public static implicit operator g__ETV.Variant<global::UnityEngine.Vector3>(in Vector3Variant value) => value.Variant;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    [g__UES.Preserve]
    public static implicit operator Vector3Variant(in g__ETV.Variant<global::UnityEngine.Vector3> value) => new Vector3Variant(value);

    [g__UES.Preserve]
    public sealed class Converter : g__ETVC.IVariantConverter<global::UnityEngine.Vector3>
    {
        [g__UES.Preserve]
        public static readonly Converter Default = new Converter();

        [g__UES.Preserve]
        private Converter() { }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        [g__UES.Preserve]
        public g__ETV.Variant ToVariant(global::UnityEngine.Vector3 value) => new Vector3Variant(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        [g__UES.Preserve]
        public g__ETV.Variant<global::UnityEngine.Vector3> ToVariantT(global::UnityEngine.Vector3 value) => new Vector3Variant(value).Variant;

        [g__UES.Preserve]
        public global::UnityEngine.Vector3 GetValue(in g__ETV.Variant variant)
        {
            if (variant.TypeId != g__ETV.Variant<global::UnityEngine.Vector3>.TypeId)
            {
                ThrowIfInvalidCast();
            }

            var temp = new Vector3Variant(variant);
            return temp.Value;
        }

        [g__UES.Preserve]
        public bool TryGetValue(in g__ETV.Variant variant, out global::UnityEngine.Vector3 result)
        {
            if (variant.TypeId == g__ETV.Variant<global::UnityEngine.Vector3>.TypeId)
            {
                var temp = new Vector3Variant(variant);
                result = temp.Value;
                return true;
            }

            result = default;
            return false;
        }

        [g__UES.Preserve]
        public bool TrySetValueTo(in g__ETV.Variant variant, ref global::UnityEngine.Vector3 result)
        {
            if (variant.TypeId == g__ETV.Variant<global::UnityEngine.Vector3>.TypeId)
            {
                var temp = new Vector3Variant(variant);
                result = temp.Value;
                return true;
            }

            return false;
        }

        [g__UES.Preserve]
        public string ToString(in g__ETV.Variant variant)
        {
            if (variant.TypeId == g__ETV.Variant<global::UnityEngine.Vector3>.TypeId)
            {
                var temp = new Vector3Variant(variant);
                return temp.Value.ToString();
            }

            return g__ETT.TypeIdExtensions.ToType(variant.TypeId).ToString();
        }

        [g__SDCA.DoesNotReturn]
        private static void ThrowIfInvalidCast()
        {
            throw new g__S.InvalidCastException
            (
                $"Cannot get value of {typeof(global::UnityEngine.Vector3)} from the input variant."
            );
        }

    }

}


}

