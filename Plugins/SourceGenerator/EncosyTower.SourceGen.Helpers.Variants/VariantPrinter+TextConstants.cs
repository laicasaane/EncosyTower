namespace EncosyTower.SourceGen.Helpers.Variants
{
    public readonly partial struct VariantPrinter
    {
        private const string FORMAT_PUBLIC_READONLY_0_VARIANT = "public readonly {0} Variant;";
        private const string FORMAT_PUBLIC_READONLY_0_VALUE = "public readonly {0} Value;";
        private const string VALUE_PROPERTY_DECLARATION_FORMAT = "public {0} Value";
        private const string GET_ACCESSOR = "get";
        private const string IF_THIS_VARIANT_VALUE_TRY_GET_VALUE =
            "if (this.Variant.Value.TryGetValue(out object obj) ";
        private const string FORMAT_OBJ_IS_0_OBJECT_T = "&& obj is {0} objectT)";
        private const string RETURN_OBJECT_T = "return objectT;";
        private const string RETURN_DEFAULT = "return default;";
        private const string FORMAT_IF_THIS_VARIANT_VALUE_TRY_GET_VALUE =
            "if (this.Variant.Value.TryGetValue(out object obj) && obj is {0} objectT) ";
        private const string FORMAT_PUBLIC_0_1_VALUE = "public {0}({1} value)";
        private const string FORMAT_THIS_VARIANT_NEW_0_1_VALUE_TYPE =
            "this.Variant = new {0}({1}.ValueType, {2}.TypeId);";
        private const string THIS_VALUE_VALUE = "this.Value = value;";
        private const string FORMAT_THIS_VARIANT_NEW_0_1_TYPE_ID = "this.Variant = new {0}({1}.TypeId, (object)value);";
        private const string VARIANT_CONSTRUCTOR_DECLARATION_FORMAT = "public {0}(in {1} variant) : this()";
        private const string THIS_VARIANT_VARIANT = "this.Variant = variant;";
        private const string VALIDATE_TYPE_ID_VARIANT = "ValidateTypeId(variant);";
        private const string FORMAT_PRIVATE_STATIC_VOID_VALIDATE_TYPE_ID_IN =
            "private static void ValidateTypeId(in {0} variant)";
        private const string FORMAT_IF_VARIANT_TYPE_ID_0_TYPE_ID = "if (variant.TypeId != {0}.TypeId)";
        private const string FORMAT_G_ETMINT_THROW_HELPER_THROW_VARIANT_INVALID_CAST =
            "g__ETMINT.ThrowHelper.ThrowVariantInvalidCast(variant, typeof({0}));";
        private const string THROW_IF_INVALID_CAST_VARIANT = "ThrowIfInvalidCast(variant);";
        private const string FORMAT_PRIVATE_STATIC_VOID_THROW_IF_INVALID_CAST =
            "private static void ThrowIfInvalidCast(in {0} variant)";
        private const string VAR_TYPE_G_ETT_TYPE_ID_EXTENSIONS =
            "var type = g__ETT.TypeIdExtensions.ToType(variant.TypeId);";
        private const string THROW_NEW_G_S_INVALID_CAST_EXCEPTION = "throw new g__S.InvalidCastException";
        private const string EXCEPTION_ARGUMENTS_OPEN = "(";
        private const string FORMAT_CANNOT_CAST_TYPE_TO_TYPEOF_0 = "$\"Cannot cast {{type}} to {{typeof({0})}}\"";
        private const string EXCEPTION_STATEMENT_CLOSE = ");";
        private const string FORMAT_PUBLIC_STATIC_IMPLICIT_OPERATOR_0_1_VALUE =
            "public static implicit operator {0}({1} value)";
        private const string NEW_VARIANT_FROM_VALUE_FORMAT = "new {0}(value)";
        private const string IMPLICIT_FROM_VARIANT_DECLARATION_FORMAT =
            "public static implicit operator {0}(in {1} value)";
        private const string VALUE_VARIANT = "value.Variant";
        private const string CONVERSION_EXPRESSION_FORMAT = "=> {0};";
        private const string CONVERSION_ARM_FORMAT = "{0} => {1};";
        private const string PUBLIC_SEALED_CLASS_CONVERTER = "public sealed class Converter";
        private const string FORMAT_G_ETVC_IVARIANT_CONVERTER_0 = " : g__ETVC.IVariantConverter<{0}>";
        private const string PUBLIC_STATIC_READONLY_CONVERTER_DEFAULT_NEW =
            "public static readonly Converter Default = new();";
        private const string PUBLIC_STATIC_READONLY_CONVERTER_DEFAULT_NEW_CONVERTER =
            "public static readonly Converter Default = new Converter();";
        private const string PRIVATE_CONVERTER = "private Converter()";
        private const string PRIVATE_CONVERTER_2 = "private Converter() { }";
        private const string FORMAT_PUBLIC_0_TO_VARIANT_1_VALUE = "public {0} ToVariant({1} value)";
        private const string FORMAT_PUBLIC_0_TO_VARIANT_T_1_VALUE = "public {0} ToVariantT({1} value)";
        private const string FORMAT_NEW_0_VALUE_VARIANT = "new {0}(value).Variant";
        private const string GET_VALUE_DECLARATION_FORMAT = "public {0} GetValue(in {1} variant)";
        private const string FORMAT_G_ETMINT_THROW_HELPER_THROW_VARIANT_VALUE_UNAVAILABLE =
            "g__ETMINT.ThrowHelper.ThrowVariantValueUnavailable(typeof({0}));";
        private const string THROW_IF_INVALID_CAST = "ThrowIfInvalidCast();";
        private const string FORMAT_VAR_TEMP_NEW_0_VARIANT = "var temp = new {0}(variant);";
        private const string RETURN_TEMP_VALUE = "return temp.Value;";
        private const string TRY_GET_VALUE_DECLARATION_FORMAT =
            "public bool TryGetValue(in {0} variant, out {1} result)";
        private const string FORMAT_IF_VARIANT_TYPE_ID_0_TYPE_ID_2 = "if (variant.TypeId == {0}.TypeId)";
        private const string RESULT_TEMP_VALUE = "result = temp.Value;";
        private const string RETURN_TRUE = "return true;";
        private const string RESULT_DEFAULT = "result = default;";
        private const string RETURN_FALSE = "return false;";
        private const string FORMAT_PUBLIC_BOOL_TRY_SET_VALUE_TO_IN =
            "public bool TrySetValueTo(in {0} variant, ref {1} result)";
        private const string TO_STRING_DECLARATION_FORMAT = "public string ToString(in {0} variant)";
        private const string RETURN_TEMP_VALUE_TO_STRING_STRING_EMPTY =
            "return temp.Value?.ToString() ?? string.Empty;";
        private const string RETURN_TEMP_VALUE_TO_STRING = "return temp.Value.ToString();";
        private const string RETURN_G_ETT_TYPE_ID_EXTENSIONS_TO =
            "return g__ETT.TypeIdExtensions.ToType(variant.TypeId).ToString();";
        private const string PRIVATE_STATIC_VOID_THROW_IF_INVALID_CAST =
            "private static void ThrowIfInvalidCast()";
        private const string FORMAT_CANNOT_GET_VALUE_OF_TYPEOF_0_FROM =
            "$\"Cannot get value of {{typeof({0})}} from the input variant.\"";
    }
}
