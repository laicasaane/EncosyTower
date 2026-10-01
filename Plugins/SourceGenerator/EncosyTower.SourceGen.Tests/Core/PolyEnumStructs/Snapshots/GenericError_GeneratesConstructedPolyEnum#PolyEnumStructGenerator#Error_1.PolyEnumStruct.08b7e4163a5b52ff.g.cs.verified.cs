
#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SC = global::System.Collections;
using g__SCG = global::System.Collections.Generic;
using g__SD = global::System.Diagnostics;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SRIS = global::System.Runtime.InteropServices;
using g__ET = global::EncosyTower.Common;
using g__ETCol = global::EncosyTower.Collections;
using g__ETCon = global::EncosyTower.Conversion;
using g__ETDVD = global::EncosyTower.Debugging.ValidationDefines;
using g__ETEE = global::EncosyTower.EnumExtensions;
using g__ETEESG = global::EncosyTower.EnumExtensions.SourceGen;
using g__UE = global::UnityEngine;
using g__UC = global::Unity.Collections;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace

namespace TestProject
{

#pragma warning disable

    partial struct Error<T> // Case Structs
    where T : unmanaged
    {
        partial record struct Invalid : global::TestProject.Error.IEnumCase, global::TestProject.Error.IEnumCase<T>
        {
            [g__SRCS.MethodImpl(INLINING)]
            public Invalid(
                  g__ET.Option<T> Data = default
            ) : this(default(T))
            {
                this.Data = Data.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.Error.EnumCase GetEnumCase()
            {
                return global::TestProject.Error.EnumCase.Invalid;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.Error<T> ToError()
            {
                return this;
            }
        }

    }

    partial struct Error<T> : global::TestProject.Error.IEnumCase, global::TestProject.Error.IEnumCase<T>, g__SRCS.IUnion, g__ET.IHasValue // Enum Struct
    where T : unmanaged
    {
        public global::TestProject.Error.EnumCase enumCase;
        public T field_I_TestProject_x002EError_x00601_x00210_1;

        [g__SRCS.MethodImpl(INLINING)]
        public Error(Invalid @case) : this()
        {
            this.enumCase = global::TestProject.Error.EnumCase.Invalid;
            this.field_I_TestProject_x002EError_x00601_x00210_1 = @case.Data;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public Error(global::TestProject.Error.Error_Undefined<T> @case) : this()
        {
            this.enumCase = global::TestProject.Error.EnumCase.Undefined;
        }

        public object Value
        {
            get => this.enumCase switch
            {
                global::TestProject.Error.EnumCase.Invalid => GetValueOrDefault(g__ET.GenericT.T<Invalid>()),
                global::TestProject.Error.EnumCase.Undefined => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.Error.Error_Undefined<T>>()),
                _ => null,
            };
        }

        public bool HasValue
        {
            get => this.enumCase switch
            {
                global::TestProject.Error.EnumCase.Invalid => true,
                global::TestProject.Error.EnumCase.Undefined => true,
                _ => false
            };
        }

        public T Data
        {
            readonly get
            {
                switch (this.enumCase)
                {
                    case global::TestProject.Error.EnumCase.Invalid:
                    {
                        Invalid enum_case = (Invalid)this;
                        var result_for_enum_case = enum_case.Data;
                        return result_for_enum_case;
                    }

                    default:
                    {
                        global::TestProject.Error.Error_Undefined<T> enum_case = (global::TestProject.Error.Error_Undefined<T>)this;
                        var result_for_enum_case = enum_case.Data;
                        return result_for_enum_case;
                    }
                }
            }

            set
            {
                switch (this.enumCase)
                {
                    case global::TestProject.Error.EnumCase.Invalid:
                    {
                        Invalid enum_case = (Invalid)this;
                        enum_case.Data = value;
                        return;
                    }

                    default:
                    {
                        global::TestProject.Error.Error_Undefined<T> enum_case = (global::TestProject.Error.Error_Undefined<T>)this;
                        enum_case.Data = value;
                        return;
                    }
                }
            }
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Error<T>(Invalid @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator Invalid(Error<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<Invalid>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Error<T>(global::TestProject.Error.Error_Undefined<T> @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.Error.Error_Undefined<T>(Error<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.Error.Error_Undefined<T>>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Invalid GetValueOrThrow(g__ET.T<Invalid> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.Error.EnumCase.Invalid);

            return new(
                  Data: g__ET.Option.Some(this.field_I_TestProject_x002EError_x00601_x00210_1)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Invalid GetValueOrDefault(g__ET.T<Invalid> _ = default, Invalid @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.Error.EnumCase.Invalid))
            {
                return new(
                      Data: g__ET.Option.Some(this.field_I_TestProject_x002EError_x00601_x00210_1)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out Invalid value)
        {
            if (IsCastable(this.enumCase, global::TestProject.Error.EnumCase.Invalid))
            {
                value = new(
                      Data: g__ET.Option.Some(this.field_I_TestProject_x002EError_x00601_x00210_1)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.Error.Error_Undefined<T> GetValueOrThrow(g__ET.T<global::TestProject.Error.Error_Undefined<T>> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.Error.EnumCase.Undefined);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.Error.Error_Undefined<T> GetValueOrDefault(g__ET.T<global::TestProject.Error.Error_Undefined<T>> _ = default, global::TestProject.Error.Error_Undefined<T> @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.Error.EnumCase.Undefined))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.Error.Error_Undefined<T> value)
        {
            if (IsCastable(this.enumCase, global::TestProject.Error.EnumCase.Undefined))
            {
                value = new();
                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.Error.EnumCase GetEnumCase()
        {
            return enumCase;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Error<T> ToError()
        {
            return this;
        }

        [g__SRCS.MethodImpl(INLINING)]
        private static bool IsCastable(global::TestProject.Error.EnumCase a, global::TestProject.Error.EnumCase b)
        {
            return a == b;
        }

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS)]
        private static void ThrowIfUncastable(global::TestProject.Error.EnumCase source, global::TestProject.Error.EnumCase target)
        {
            if (IsCastable(source, target) == false)
            {
                throw new g__S.InvalidCastException(
                    $"Cannot cast 'Error' into '{target}' because it currently stores a '{source}'."
                );
            }

        }

    }

    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial struct Error<T> // Internals
    where T : unmanaged
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }
}
