
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

    partial struct ObservableDictionaryChange<TKey, TValue> // Case Structs
    {
    }

    partial struct ObservableDictionaryChange<TKey, TValue> : global::TestProject.ObservableDictionaryChange.IEnumCase, global::TestProject.ObservableDictionaryChange.IEnumCase<TKey, TValue>, g__SRCS.IUnion, g__ET.IHasValue // Enum Struct
    {
        public readonly global::TestProject.ObservableDictionaryChange.EnumCase enumCase;
        public readonly TKey field_I_TestProject_x002EObservableDictionaryChange_x002BAdd_x00602_x00210_1;
        public readonly TValue field_I_TestProject_x002EObservableDictionaryChange_x002BAdd_x00602_x00211_2;
        public readonly TKey field_I_TestProject_x002EObservableDictionaryChange_x002BRemove_x00602_x00210_3;
        public readonly TValue field_I_TestProject_x002EObservableDictionaryChange_x002BRemove_x00602_x00211_4;
        public readonly TKey field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00210_5;
        public readonly TValue field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00211_6;
        public readonly TValue field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00211_7;

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableDictionaryChange(global::TestProject.ObservableDictionaryChange.Add<TKey, TValue> @case) : this()
        {
            this.enumCase = global::TestProject.ObservableDictionaryChange.EnumCase.Add;
            this.field_I_TestProject_x002EObservableDictionaryChange_x002BAdd_x00602_x00210_1 = @case.Key;
            this.field_I_TestProject_x002EObservableDictionaryChange_x002BAdd_x00602_x00211_2 = @case.Value;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableDictionaryChange(global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue> @case) : this()
        {
            this.enumCase = global::TestProject.ObservableDictionaryChange.EnumCase.Remove;
            this.field_I_TestProject_x002EObservableDictionaryChange_x002BRemove_x00602_x00210_3 = @case.Key;
            this.field_I_TestProject_x002EObservableDictionaryChange_x002BRemove_x00602_x00211_4 = @case.Value;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableDictionaryChange(global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue> @case) : this()
        {
            this.enumCase = global::TestProject.ObservableDictionaryChange.EnumCase.Replace;
            this.field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00210_5 = @case.Key;
            this.field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00211_6 = @case.OldValue;
            this.field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00211_7 = @case.NewValue;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableDictionaryChange(global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue> @case) : this()
        {
            this.enumCase = global::TestProject.ObservableDictionaryChange.EnumCase.Clear;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableDictionaryChange(global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue> @case) : this()
        {
            this.enumCase = global::TestProject.ObservableDictionaryChange.EnumCase.Undefined;
        }

        public object Value
        {
            get => this.enumCase switch
            {
                global::TestProject.ObservableDictionaryChange.EnumCase.Add => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableDictionaryChange.Add<TKey, TValue>>()),
                global::TestProject.ObservableDictionaryChange.EnumCase.Remove => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue>>()),
                global::TestProject.ObservableDictionaryChange.EnumCase.Replace => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue>>()),
                global::TestProject.ObservableDictionaryChange.EnumCase.Clear => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue>>()),
                global::TestProject.ObservableDictionaryChange.EnumCase.Undefined => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue>>()),
                _ => null,
            };
        }

        public bool HasValue
        {
            get => this.enumCase switch
            {
                global::TestProject.ObservableDictionaryChange.EnumCase.Add => true,
                global::TestProject.ObservableDictionaryChange.EnumCase.Remove => true,
                global::TestProject.ObservableDictionaryChange.EnumCase.Replace => true,
                global::TestProject.ObservableDictionaryChange.EnumCase.Clear => true,
                global::TestProject.ObservableDictionaryChange.EnumCase.Undefined => true,
                _ => false
            };
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableDictionaryChange<TKey, TValue>(global::TestProject.ObservableDictionaryChange.Add<TKey, TValue> @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableDictionaryChange.Add<TKey, TValue>(ObservableDictionaryChange<TKey, TValue> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableDictionaryChange.Add<TKey, TValue>>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableDictionaryChange<TKey, TValue>(global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue> @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue>(ObservableDictionaryChange<TKey, TValue> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue>>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableDictionaryChange<TKey, TValue>(global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue> @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue>(ObservableDictionaryChange<TKey, TValue> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue>>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableDictionaryChange<TKey, TValue>(global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue> @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue>(ObservableDictionaryChange<TKey, TValue> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue>>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableDictionaryChange<TKey, TValue>(global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue> @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue>(ObservableDictionaryChange<TKey, TValue> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue>>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableDictionaryChange.Add<TKey, TValue> GetValueOrThrow(g__ET.T<global::TestProject.ObservableDictionaryChange.Add<TKey, TValue>> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Add);

            return new(
                  Key: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BAdd_x00602_x00210_1)
                , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BAdd_x00602_x00211_2)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableDictionaryChange.Add<TKey, TValue> GetValueOrDefault(g__ET.T<global::TestProject.ObservableDictionaryChange.Add<TKey, TValue>> _ = default, global::TestProject.ObservableDictionaryChange.Add<TKey, TValue> @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Add))
            {
                return new(
                      Key: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BAdd_x00602_x00210_1)
                    , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BAdd_x00602_x00211_2)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableDictionaryChange.Add<TKey, TValue> value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Add))
            {
                value = new(
                      Key: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BAdd_x00602_x00210_1)
                    , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BAdd_x00602_x00211_2)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue> GetValueOrThrow(g__ET.T<global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue>> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Remove);

            return new(
                  Key: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BRemove_x00602_x00210_3)
                , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BRemove_x00602_x00211_4)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue> GetValueOrDefault(g__ET.T<global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue>> _ = default, global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue> @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Remove))
            {
                return new(
                      Key: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BRemove_x00602_x00210_3)
                    , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BRemove_x00602_x00211_4)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue> value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Remove))
            {
                value = new(
                      Key: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BRemove_x00602_x00210_3)
                    , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BRemove_x00602_x00211_4)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue> GetValueOrThrow(g__ET.T<global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue>> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Replace);

            return new(
                  Key: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00210_5)
                , OldValue: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00211_6)
                , NewValue: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00211_7)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue> GetValueOrDefault(g__ET.T<global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue>> _ = default, global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue> @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Replace))
            {
                return new(
                      Key: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00210_5)
                    , OldValue: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00211_6)
                    , NewValue: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00211_7)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue> value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Replace))
            {
                value = new(
                      Key: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00210_5)
                    , OldValue: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00211_6)
                    , NewValue: g__ET.Option.Some(this.field_I_TestProject_x002EObservableDictionaryChange_x002BReplace_x00602_x00211_7)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue> GetValueOrThrow(g__ET.T<global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue>> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Clear);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue> GetValueOrDefault(g__ET.T<global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue>> _ = default, global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue> @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Clear))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue> value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Clear))
            {
                value = new();
                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue> GetValueOrThrow(g__ET.T<global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue>> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Undefined);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue> GetValueOrDefault(g__ET.T<global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue>> _ = default, global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue> @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Undefined))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue> value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableDictionaryChange.EnumCase.Undefined))
            {
                value = new();
                return true;
            }

            value = default;
            return false;
        }

        public bool AffectsKey(in TKey selectedKey, global::TestProject.IObservableDictionary<TKey, TValue> source)
        {
            switch (this.enumCase)
            {
                case global::TestProject.ObservableDictionaryChange.EnumCase.Add:
                {
                    global::TestProject.ObservableDictionaryChange.Add<TKey, TValue> enum_case = (global::TestProject.ObservableDictionaryChange.Add<TKey, TValue>)this;
                    var result_for_enum_case = enum_case.AffectsKey(in selectedKey, source);
                    return result_for_enum_case;
                }

                case global::TestProject.ObservableDictionaryChange.EnumCase.Remove:
                {
                    global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue> enum_case = (global::TestProject.ObservableDictionaryChange.Remove<TKey, TValue>)this;
                    var result_for_enum_case = enum_case.AffectsKey(in selectedKey, source);
                    return result_for_enum_case;
                }

                case global::TestProject.ObservableDictionaryChange.EnumCase.Replace:
                {
                    global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue> enum_case = (global::TestProject.ObservableDictionaryChange.Replace<TKey, TValue>)this;
                    var result_for_enum_case = enum_case.AffectsKey(in selectedKey, source);
                    return result_for_enum_case;
                }

                case global::TestProject.ObservableDictionaryChange.EnumCase.Clear:
                {
                    global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue> enum_case = (global::TestProject.ObservableDictionaryChange.Clear<TKey, TValue>)this;
                    var result_for_enum_case = enum_case.AffectsKey(in selectedKey, source);
                    return result_for_enum_case;
                }

                default:
                {
                    global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue> enum_case = (global::TestProject.ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue>)this;
                    var result_for_enum_case = enum_case.AffectsKey(in selectedKey, source);
                    return result_for_enum_case;
                }
            }
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableDictionaryChange.EnumCase GetEnumCase()
        {
            return enumCase;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly ObservableDictionaryChange<TKey, TValue> ToObservableDictionaryChange()
        {
            return this;
        }

        [g__SRCS.MethodImpl(INLINING)]
        private static bool IsCastable(global::TestProject.ObservableDictionaryChange.EnumCase a, global::TestProject.ObservableDictionaryChange.EnumCase b)
        {
            return a == b;
        }

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS)]
        private static void ThrowIfUncastable(global::TestProject.ObservableDictionaryChange.EnumCase source, global::TestProject.ObservableDictionaryChange.EnumCase target)
        {
            if (IsCastable(source, target) == false)
            {
                throw new g__S.InvalidCastException(
                    $"Cannot cast 'ObservableDictionaryChange' into '{target}' because it currently stores a '{source}'."
                );
            }

        }

    }

    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial struct ObservableDictionaryChange<TKey, TValue> // Internals
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }
}
