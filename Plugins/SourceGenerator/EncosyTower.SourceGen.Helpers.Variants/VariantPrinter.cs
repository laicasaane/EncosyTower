using System;
using System.Globalization;

namespace EncosyTower.SourceGen.Helpers.Variants
{
    public readonly partial struct VariantPrinter
    {
        private readonly bool _useTypeConstants;

        private VariantPrinter(bool useTypeConstants)
            => _useTypeConstants = useTypeConstants;

        private IFormatProvider FormatProvider
            => _useTypeConstants ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;

        public const string AGGRESSIVE_INLINING = "[g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]";
        public const string EXCLUDE_COVERAGE = "[g__SDCA.ExcludeFromCodeCoverage]";
        public const string LOG_REGISTRIES = "ENCOSY_LOG_VARIANTS_REGISTRIES";
        public const string STRUCT_LAYOUT = "[g__SRIS.StructLayout(g__SRIS.InteropServices.LayoutKind.Explicit)]";
        public const string UNSAFE_EVOLUTION_FIELD_MARKER =
            "// TODO(unsafe-evolution): mark this overlapping field safe/unsafe "
            + "when the new syntax is available.";
        public const string META_OFFSET = "[g__SRIS.FieldOffset(g__ETV.VariantBase.META_OFFSET)]";
        public const string DATA_OFFSET = "[g__SRIS.FieldOffset(g__ETV.VariantBase.DATA_OFFSET)]";
        public const string VARIANT_TYPE = "g__ETV.Variant";
        public const string VARIANT_DATA_TYPE = "g__ETV.VariantData";
        public const string VARIANT_TYPE_KIND = "g__ETV.VariantTypeKind";
        public const string VARIANT_CONVERTER_TYPE = "g__ETVC.VariantConverter";
        public const string DOES_NOT_RETURN = "[g__SDCA.DoesNotReturn]";
        public const string RUNTIME_INITIALIZE_ON_LOAD_METHOD =
            "[g__UE.RuntimeInitializeOnLoadMethod(g__UE.RuntimeInitializeLoadType.BeforeSceneLoad)]";
        public const string PRESERVE = "[g__UES.Preserve]";

        public void WriteRegister(
              ref Printer p
            , string typeName
            , string simpleTypeName
            , string converterDefault
            , int? unmanagedSize
        )
        {
            if (unmanagedSize.HasValue)
            {
                p.PrintBeginLine("if (").Print(VARIANT_CONVERTER_TYPE).Print(".CanStore<")
                    .Print(typeName)
                    .PrintEndLine(">())");
                p.OpenScope();
                {
                    WriteRegisterCall(ref p, typeName, simpleTypeName, converterDefault);
                }
                p.CloseScope();
            }
            else
            {
                WriteRegisterCall(ref p, typeName, simpleTypeName, converterDefault);
            }

            p.PrintEndLine();
        }

        private static void WriteRegisterCall(
              ref Printer p
            , string typeName
            , string simpleTypeName
            , string converterDefault
        )
        {
            p.OpenScope($"Register<{typeName}>({converterDefault}");
            {
                p.Print("#if UNITY_EDITOR && ").Print(LOG_REGISTRIES).PrintEndLine();
                p.PrintBeginLine(", \"").Print(simpleTypeName).PrintEndLine("\"");
                p.Print("#endif").PrintEndLine();
            }
            p.CloseScope(");");
        }

        public void WriteRegisterMethod(ref Printer p)
        {
            p.Print("#if !UNITY_EDITOR || !").Print(LOG_REGISTRIES).PrintEndLine();
            p.PrintLine(AGGRESSIVE_INLINING);
            p.Print("#endif").PrintEndLine();
            p.PrintLine(PRESERVE);
            p.PrintLine("private static void Register<T>(");
            p.IncreasedIndent();
            {
                p.PrintBeginLine().Print(Printer.INDENT.Substring(0, 2))
                    .PrintEndLine("g__ETVC.IVariantConverter<T> converter");
                p.Print("#if UNITY_EDITOR && ").Print(LOG_REGISTRIES).PrintEndLine();
                p.PrintLine(", string typeName");
                p.Print("#endif").PrintEndLine();
            }
            p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                p.Print("#if UNITY_EDITOR && ").Print(LOG_REGISTRIES).PrintEndLine();
                p.PrintLine("var result =");
                p.Print("#endif").PrintEndLine();

                p.PrintLine("g__ETVC.VariantConverter.TryRegister<T>(converter);");
                p.PrintEndLine();

                p.Print("#if UNITY_EDITOR && ").Print(LOG_REGISTRIES).PrintEndLine();

                p.PrintLine("if (result)");
                p.OpenScope();
                {
                    p.PrintBeginLine("g__UE.Debug.Log(\"Register variant for ")
                        .Print("{typeName}").PrintEndLine("\");");
                }
                p.CloseScope();
                p.PrintLine("else");
                p.OpenScope();
                {
                    p.PrintBeginLine("g__UE.Debug.LogError(\"Cannot register variant for ")
                        .Print("{typeName}").PrintEndLine("\");");
                }
                p.CloseScope();

                p.Print("#endif").PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        public void WriteVariantBody(
              ref Printer p
            , bool isValueType
            , bool hasImplicitFromStructToType
            , string typeName
            , string structName
            , string variantName
            , bool useMvvmThrowHelper = false
            , bool useTypeConstants = false
        )
        {
            var writer = useTypeConstants ? new VariantPrinter(true) : this;
            p.OpenScope();
            {
                if (isValueType)
                {
                    writer.WriteFieldsForValueType(ref p, typeName, variantName, useMvvmThrowHelper);
                    writer.WriteConstructorForValueType(ref p, typeName, structName, variantName);
                }
                else
                {
                    writer.WriteFieldsForRefType(ref p, typeName, variantName, useMvvmThrowHelper);
                    writer.WriteConstructorForRefType(ref p, typeName, structName, variantName);
                }

                writer.WriteOtherConstructors(ref p, structName, variantName);

                if (useMvvmThrowHelper && isValueType == false)
                {
                    writer.WriteRefValueProperty(ref p, typeName, variantName, useMvvmThrowHelper);
                }

                if (useMvvmThrowHelper)
                {
                    writer.WriteImplicitConversions(
                          ref p
                        , typeName
                        , structName
                        , variantName
                        , hasImplicitFromStructToType
                        , useMvvmThrowHelper
                    );

                    writer.WriteValidateTypeIdMethod(ref p, typeName, variantName, useMvvmThrowHelper);
                }
                else
                {
                    writer.WriteValidateTypeIdMethod(ref p, typeName, variantName, useMvvmThrowHelper);

                    writer.WriteImplicitConversions(
                          ref p
                        , typeName
                        , structName
                        , variantName
                        , hasImplicitFromStructToType
                        , useMvvmThrowHelper
                    );
                }

                writer.WriteConverterClass(ref p, typeName, structName, variantName, isValueType, useMvvmThrowHelper);
            }
            p.CloseScope();
        }

        private void WriteFieldsForValueType(
              ref Printer p
            , string typeName
            , string variantName
            , bool useMvvmThrowHelper
        )
        {
            p.PrintLineIf(useMvvmThrowHelper == false, UNSAFE_EVOLUTION_FIELD_MARKER);
            p.PrintLine(META_OFFSET).PrintLine(PRESERVE);
            p.PrintLine(string.Format(FormatProvider, FORMAT_PUBLIC_READONLY_0_VARIANT, variantName));
            p.PrintEndLine();

            p.PrintLineIf(useMvvmThrowHelper == false, UNSAFE_EVOLUTION_FIELD_MARKER);
            p.PrintLine(DATA_OFFSET).PrintLine(PRESERVE);
            p.PrintLine(string.Format(FormatProvider, FORMAT_PUBLIC_READONLY_0_VALUE, typeName));
            p.PrintEndLine();
        }

        private void WriteFieldsForRefType(
              ref Printer p
            , string typeName
            , string variantName
            , bool useMvvmThrowHelper
        )
        {
            p.PrintLine(PRESERVE);
            p.PrintLine(string.Format(FormatProvider, FORMAT_PUBLIC_READONLY_0_VARIANT, variantName));
            p.PrintEndLine();

            if (useMvvmThrowHelper == false)
            {
                WriteRefValueProperty(ref p, typeName, variantName, useMvvmThrowHelper);
            }
        }

        private void WriteRefValueProperty(
              ref Printer p
            , string typeName
            , string variantName
            , bool useMvvmThrowHelper
        )
        {
            p.PrintLine(PRESERVE);
            p.PrintLine(string.Format(
                  FormatProvider
                , VALUE_PROPERTY_DECLARATION_FORMAT
                , typeName
            ));
            p.OpenScope();
            {
                p.PrintLine(GET_ACCESSOR);
                p.OpenScope();
                {
                    if (useMvvmThrowHelper)
                    {
                        p.PrintLine(
                            IF_THIS_VARIANT_VALUE_TRY_GET_VALUE
                            + string.Format(FormatProvider, FORMAT_OBJ_IS_0_OBJECT_T, typeName)
                        );
                        p.OpenScope();
                        {
                            p.PrintLine(RETURN_OBJECT_T);
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                        p.PrintLine(RETURN_DEFAULT);
                    }
                    else
                    {
                        p.PrintLine(
                            string.Format(
                                  FormatProvider
                                , FORMAT_IF_THIS_VARIANT_VALUE_TRY_GET_VALUE
                                , typeName
                            )
                            + RETURN_OBJECT_T
                        );
                        p.PrintLine(RETURN_DEFAULT);
                    }
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructorForValueType(ref Printer p, string typeName, string structName, string variantName)
        {
            p.PrintLine(PRESERVE);
            p.PrintLine(string.Format(FormatProvider, FORMAT_PUBLIC_0_1_VALUE, structName, typeName));
            p.OpenScope();
            {
                p.PrintLine(string.Format(
                      FormatProvider
                    , FORMAT_THIS_VARIANT_NEW_0_1_VALUE_TYPE
                    , VARIANT_TYPE
                    , VARIANT_TYPE_KIND
                    , variantName
                ));
                p.PrintLine(THIS_VALUE_VALUE);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteConstructorForRefType(ref Printer p, string typeName, string structName, string variantName)
        {
            p.PrintLine(PRESERVE);
            p.PrintLine(string.Format(FormatProvider, FORMAT_PUBLIC_0_1_VALUE, structName, typeName));
            p.OpenScope();
            {
                p.PrintLine(string.Format(
                      FormatProvider
                    , FORMAT_THIS_VARIANT_NEW_0_1_TYPE_ID
                    , VARIANT_TYPE
                    , variantName
                ));
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteOtherConstructors(ref Printer p, string structName, string variantName)
        {
            p.PrintLine(PRESERVE);
            p.PrintLine(string.Format(
                  FormatProvider
                , VARIANT_CONSTRUCTOR_DECLARATION_FORMAT
                , structName
                , variantName
            ));
            p.OpenScope();
            {
                p.PrintLine(THIS_VARIANT_VARIANT);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(PRESERVE);
            p.PrintLine(string.Format(
                  FormatProvider
                , VARIANT_CONSTRUCTOR_DECLARATION_FORMAT
                , structName
                , VARIANT_TYPE
            ));
            p.OpenScope();
            {
                p.PrintLine(VALIDATE_TYPE_ID_VARIANT);
                p.PrintLine(THIS_VARIANT_VARIANT);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteValidateTypeIdMethod(
              ref Printer p
            , string typeName
            , string variantName
            , bool useMvvmThrowHelper
        )
        {
            p.PrintLine(PRESERVE);
            p.PrintLine(string.Format(
                  FormatProvider
                , FORMAT_PRIVATE_STATIC_VOID_VALIDATE_TYPE_ID_IN
                , VARIANT_TYPE
            ));
            p.OpenScope();
            {
                p.PrintLine(string.Format(
                      FormatProvider
                    , FORMAT_IF_VARIANT_TYPE_ID_0_TYPE_ID
                    , variantName
                ));
                p.OpenScope();
                {
                    p.PrintLineIf(
                          useMvvmThrowHelper
                        , string.Format(
                              FormatProvider
                            , FORMAT_G_ETMINT_THROW_HELPER_THROW_VARIANT_INVALID_CAST
                            , typeName
                        )
                        , THROW_IF_INVALID_CAST_VARIANT
                    );
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            if (useMvvmThrowHelper == false)
            {
                p.PrintLine(DOES_NOT_RETURN);
                p.PrintLine(string.Format(
                      FormatProvider
                    , FORMAT_PRIVATE_STATIC_VOID_THROW_IF_INVALID_CAST
                    , VARIANT_TYPE
                ));
                p.OpenScope();
                {
                    p.PrintLine(VAR_TYPE_G_ETT_TYPE_ID_EXTENSIONS);
                    p.PrintEndLine();

                    p.PrintLine(THROW_NEW_G_S_INVALID_CAST_EXCEPTION);
                    p.OpenScope(EXCEPTION_ARGUMENTS_OPEN);
                    {
                        p.PrintLine(string.Format(
                              FormatProvider
                            , FORMAT_CANNOT_CAST_TYPE_TO_TYPEOF_0
                            , typeName
                        ));
                    }
                    p.CloseScope(EXCEPTION_STATEMENT_CLOSE);
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private void WriteImplicitConversions(
              ref Printer p
            , string typeName
            , string structName
            , string variantName
            , bool hasImplicitFromStructToType
            , bool useMvvmThrowHelper
        )
        {
            if (hasImplicitFromStructToType)
            {
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                WriteConversion(
                      ref p
                    , string.Format(
                          FormatProvider
                        , FORMAT_PUBLIC_STATIC_IMPLICIT_OPERATOR_0_1_VALUE
                        , structName
                        , typeName
                    )
                    , string.Format(
                          FormatProvider
                        , NEW_VARIANT_FROM_VALUE_FORMAT
                        , structName
                    )
                    , useMvvmThrowHelper
                );
                p.PrintEndLine();
            }

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
            WriteConversion(
                  ref p
                , string.Format(
                      FormatProvider
                    , IMPLICIT_FROM_VARIANT_DECLARATION_FORMAT
                    , VARIANT_TYPE
                    , structName
                )
                , VALUE_VARIANT
                , useMvvmThrowHelper
            );
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
            WriteConversion(
                  ref p
                , string.Format(
                      FormatProvider
                    , IMPLICIT_FROM_VARIANT_DECLARATION_FORMAT
                    , variantName
                    , structName
                )
                , VALUE_VARIANT
                , useMvvmThrowHelper
            );
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
            WriteConversion(
                  ref p
                , string.Format(
                      FormatProvider
                    , IMPLICIT_FROM_VARIANT_DECLARATION_FORMAT
                    , structName
                    , variantName
                )
                , string.Format(
                      FormatProvider
                    , NEW_VARIANT_FROM_VALUE_FORMAT
                    , structName
                )
                , useMvvmThrowHelper
            );
            p.PrintEndLine();
        }

        private void WriteConversion(
              ref Printer printer
            , string signature
            , string expression
            , bool useMvvmThrowHelper
        )
        {
            if (useMvvmThrowHelper)
            {
                printer.PrintLine(signature);
                printer.IncreasedIndent();
                {
                    printer.PrintLine(string.Format(
                          FormatProvider
                        , CONVERSION_EXPRESSION_FORMAT
                        , expression
                    ));
                }
                printer.DecreasedIndent();
            }
            else
            {
                printer.PrintLine(string.Format(
                      FormatProvider
                    , CONVERSION_ARM_FORMAT
                    , signature
                    , expression
                ));
            }
        }

        private void WriteConverterClass(
              ref Printer p
            , string typeName
            , string structName
            , string variantName
            , bool isValueType
            , bool useMvvmThrowHelper
        )
        {
            p.PrintLine(PRESERVE);
            p.PrintBeginLine()
                .Print(PUBLIC_SEALED_CLASS_CONVERTER)
                .Print(string.Format(FormatProvider, FORMAT_G_ETVC_IVARIANT_CONVERTER_0, typeName))
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine(PRESERVE);
                p.PrintLine(useMvvmThrowHelper
                    ? PUBLIC_STATIC_READONLY_CONVERTER_DEFAULT_NEW
                    : PUBLIC_STATIC_READONLY_CONVERTER_DEFAULT_NEW_CONVERTER);
                p.PrintEndLine();

                p.PrintLine(PRESERVE);
                if (useMvvmThrowHelper)
                {
                    p.PrintLine(PRIVATE_CONVERTER);
                    p.OpenScope();
                    {
                    }
                    p.CloseScope();
                }
                else
                {
                    p.PrintLine(PRIVATE_CONVERTER_2);
                }
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                WriteConversion(
                      ref p
                    , string.Format(
                          FormatProvider
                        , FORMAT_PUBLIC_0_TO_VARIANT_1_VALUE
                        , VARIANT_TYPE
                        , typeName
                    )
                    , string.Format(
                          FormatProvider
                        , NEW_VARIANT_FROM_VALUE_FORMAT
                        , structName
                    )
                    , useMvvmThrowHelper
                );
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(PRESERVE);
                WriteConversion(
                      ref p
                    , string.Format(
                          FormatProvider
                        , FORMAT_PUBLIC_0_TO_VARIANT_T_1_VALUE
                        , variantName
                        , typeName
                    )
                    , string.Format(FormatProvider, FORMAT_NEW_0_VALUE_VARIANT, structName)
                    , useMvvmThrowHelper
                );
                p.PrintEndLine();

                p.PrintLine(PRESERVE);
                p.PrintLine(string.Format(
                      FormatProvider
                    , GET_VALUE_DECLARATION_FORMAT
                    , typeName
                    , VARIANT_TYPE
                ));
                p.OpenScope();
                {
                    p.PrintLine(string.Format(
                          FormatProvider
                        , FORMAT_IF_VARIANT_TYPE_ID_0_TYPE_ID
                        , variantName
                    ));
                    p.OpenScope();
                    {
                        p.PrintLineIf(
                              useMvvmThrowHelper
                            , string.Format(
                                  FormatProvider
                                , FORMAT_G_ETMINT_THROW_HELPER_THROW_VARIANT_VALUE_UNAVAILABLE
                                , typeName
                            )
                            , THROW_IF_INVALID_CAST
                        );
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(string.Format(FormatProvider, FORMAT_VAR_TEMP_NEW_0_VARIANT, structName));
                    p.PrintLine(RETURN_TEMP_VALUE);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(PRESERVE);
                p.PrintLine(string.Format(
                      FormatProvider
                    , TRY_GET_VALUE_DECLARATION_FORMAT
                    , VARIANT_TYPE
                    , typeName
                ));
                p.OpenScope();
                {
                    p.PrintLine(string.Format(
                          FormatProvider
                        , FORMAT_IF_VARIANT_TYPE_ID_0_TYPE_ID_2
                        , variantName
                    ));
                    p.OpenScope();
                    {
                        p.PrintLine(string.Format(
                              FormatProvider
                            , FORMAT_VAR_TEMP_NEW_0_VARIANT
                            , structName
                        ));
                        p.PrintLine(RESULT_TEMP_VALUE);
                        p.PrintLine(RETURN_TRUE);
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(RESULT_DEFAULT);
                    p.PrintLine(RETURN_FALSE);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(PRESERVE);
                p.PrintLine(string.Format(
                      FormatProvider
                    , FORMAT_PUBLIC_BOOL_TRY_SET_VALUE_TO_IN
                    , VARIANT_TYPE
                    , typeName
                ));
                p.OpenScope();
                {
                    p.PrintLine(string.Format(
                          FormatProvider
                        , FORMAT_IF_VARIANT_TYPE_ID_0_TYPE_ID_2
                        , variantName
                    ));
                    p.OpenScope();
                    {
                        p.PrintLine(string.Format(
                              FormatProvider
                            , FORMAT_VAR_TEMP_NEW_0_VARIANT
                            , structName
                        ));
                        p.PrintLine(RESULT_TEMP_VALUE);
                        p.PrintLine(RETURN_TRUE);
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(RETURN_FALSE);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(PRESERVE);
                p.PrintLine(string.Format(
                      FormatProvider
                    , TO_STRING_DECLARATION_FORMAT
                    , VARIANT_TYPE
                ));
                p.OpenScope();
                {
                    p.PrintLine(string.Format(
                          FormatProvider
                        , FORMAT_IF_VARIANT_TYPE_ID_0_TYPE_ID_2
                        , variantName
                    ));
                    p.OpenScope();
                    {
                        p.PrintLine(string.Format(
                              FormatProvider
                            , FORMAT_VAR_TEMP_NEW_0_VARIANT
                            , structName
                        ));
                        if (useMvvmThrowHelper && isValueType == false)
                        {
                            p.PrintLine(RETURN_TEMP_VALUE_TO_STRING_STRING_EMPTY);
                        }
                        else
                        {
                            p.PrintLine(RETURN_TEMP_VALUE_TO_STRING);
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(RETURN_G_ETT_TYPE_ID_EXTENSIONS_TO);
                }
                p.CloseScope();
                p.PrintEndLine();

                if (useMvvmThrowHelper == false)
                {
                    p.PrintLine(DOES_NOT_RETURN);
                    p.PrintLine(PRIVATE_STATIC_VOID_THROW_IF_INVALID_CAST);
                    p.OpenScope();
                    {
                        p.PrintLine(THROW_NEW_G_S_INVALID_CAST_EXCEPTION);
                        p.OpenScope(EXCEPTION_ARGUMENTS_OPEN);
                        {
                            p.PrintLine(string.Format(
                                  FormatProvider
                                , FORMAT_CANNOT_GET_VALUE_OF_TYPEOF_0_FROM
                                , typeName
                            ));
                        }
                        p.CloseScope(EXCEPTION_STATEMENT_CLOSE);
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
            p.CloseScope();

            if (useMvvmThrowHelper == false)
            {
                p.PrintEndLine();
            }
        }
    }
}
