
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

    partial struct Result<T> // Case Structs
    {
    }

    partial struct Result<T> : global::TestProject.ResultCases.IEnumCase, global::TestProject.ResultCases.IEnumCase<T>, g__SRCS.IUnion, g__ET.IHasValue // Enum Struct
    {
        public global::TestProject.ResultCases.EnumCase enumCase;
        public readonly T field_I_TestProject_x002EResultCases_x002BSuccess_x00601_x00210_1;

        [g__SRCS.MethodImpl(INLINING)]
        public Result(global::TestProject.ResultCases.Success<T> @case) : this()
        {
            this.enumCase = global::TestProject.ResultCases.EnumCase.Success;
            this.field_I_TestProject_x002EResultCases_x002BSuccess_x00601_x00210_1 = @case.Value;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public Result(global::TestProject.ResultCases.Result_Undefined<T> @case) : this()
        {
            this.enumCase = global::TestProject.ResultCases.EnumCase.Undefined;
        }

        object g__SRCS.IUnion.Value
        {
            get => this.enumCase switch
            {
                global::TestProject.ResultCases.EnumCase.Success => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ResultCases.Success<T>>()),
                global::TestProject.ResultCases.EnumCase.Undefined => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ResultCases.Result_Undefined<T>>()),
                _ => null,
            };
        }

        public bool HasValue
        {
            get => this.enumCase switch
            {
                global::TestProject.ResultCases.EnumCase.Success => true,
                global::TestProject.ResultCases.EnumCase.Undefined => true,
                _ => false
            };
        }

        public readonly T Value
        {
            get
            {
                switch (this.enumCase)
                {
                    case global::TestProject.ResultCases.EnumCase.Success:
                    {
                        global::TestProject.ResultCases.Success<T> enum_case = (global::TestProject.ResultCases.Success<T>)this;
                        var result_for_enum_case = enum_case.Value;
                        return result_for_enum_case;
                    }

                    default:
                    {
                        global::TestProject.ResultCases.Result_Undefined<T> enum_case = (global::TestProject.ResultCases.Result_Undefined<T>)this;
                        var result_for_enum_case = enum_case.Value;
                        return result_for_enum_case;
                    }
                }
            }

        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Result<T>(global::TestProject.ResultCases.Success<T> @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ResultCases.Success<T>(Result<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ResultCases.Success<T>>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Result<T>(global::TestProject.ResultCases.Result_Undefined<T> @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ResultCases.Result_Undefined<T>(Result<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ResultCases.Result_Undefined<T>>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ResultCases.Success<T> GetValueOrThrow(g__ET.T<global::TestProject.ResultCases.Success<T>> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ResultCases.EnumCase.Success);

            return new(
                  Value: g__ET.Option.Some(this.field_I_TestProject_x002EResultCases_x002BSuccess_x00601_x00210_1)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ResultCases.Success<T> GetValueOrDefault(g__ET.T<global::TestProject.ResultCases.Success<T>> _ = default, global::TestProject.ResultCases.Success<T> @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ResultCases.EnumCase.Success))
            {
                return new(
                      Value: g__ET.Option.Some(this.field_I_TestProject_x002EResultCases_x002BSuccess_x00601_x00210_1)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ResultCases.Success<T> value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ResultCases.EnumCase.Success))
            {
                value = new(
                      Value: g__ET.Option.Some(this.field_I_TestProject_x002EResultCases_x002BSuccess_x00601_x00210_1)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ResultCases.Result_Undefined<T> GetValueOrThrow(g__ET.T<global::TestProject.ResultCases.Result_Undefined<T>> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ResultCases.EnumCase.Undefined);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ResultCases.Result_Undefined<T> GetValueOrDefault(g__ET.T<global::TestProject.ResultCases.Result_Undefined<T>> _ = default, global::TestProject.ResultCases.Result_Undefined<T> @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ResultCases.EnumCase.Undefined))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ResultCases.Result_Undefined<T> value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ResultCases.EnumCase.Undefined))
            {
                value = new();
                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ResultCases.EnumCase GetEnumCase()
        {
            return enumCase;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Result<T> ToResult()
        {
            return this;
        }

        [g__SRCS.MethodImpl(INLINING)]
        private static bool IsCastable(global::TestProject.ResultCases.EnumCase a, global::TestProject.ResultCases.EnumCase b)
        {
            return a == b;
        }

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS)]
        private static void ThrowIfUncastable(global::TestProject.ResultCases.EnumCase source, global::TestProject.ResultCases.EnumCase target)
        {
            if (IsCastable(source, target) == false)
            {
                throw new g__S.InvalidCastException(
                    $"Cannot cast 'Result' into '{target}' because it currently stores a '{source}'."
                );
            }

        }

    }

    [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
    partial struct Result<T> // Internals
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }
}
