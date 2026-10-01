
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

    partial struct ObservableListChange<T> // Case Structs
    {
    }

    partial struct ObservableListChange<T> : global::TestProject.ObservableListChange.IEnumCase, g__SRCS.IUnion, g__ET.IHasValue // Enum Struct
    {
        public readonly global::TestProject.ObservableListChange.EnumCase enumCase;
        public readonly int field_I_System_x002EInt32_1;
        public readonly T field_I_TestProject_x002EObservableListChange_x002BAdd_x00601_x00210_2;
        public readonly int field_I_System_x002EInt32_3;
        public readonly T field_I_TestProject_x002EObservableListChange_x002BRemove_x00601_x00210_4;
        public readonly T field_I_TestProject_x002EObservableListChange_x002BReplace_x00601_x00210_5;
        public readonly T field_I_TestProject_x002EObservableListChange_x002BReplace_x00601_x00210_6;
        public readonly T field_I_TestProject_x002EObservableListChange_x002BMove_x00601_x00210_7;

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableListChange(global::TestProject.ObservableListChange.Add<T> @case) : this()
        {
            this.enumCase = global::TestProject.ObservableListChange.EnumCase.Add;
            this.field_I_System_x002EInt32_1 = @case.Index;
            this.field_I_TestProject_x002EObservableListChange_x002BAdd_x00601_x00210_2 = @case.Value;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableListChange(global::TestProject.ObservableListChange.AddRange @case) : this()
        {
            this.enumCase = global::TestProject.ObservableListChange.EnumCase.AddRange;
            this.field_I_System_x002EInt32_1 = @case.Index;
            this.field_I_System_x002EInt32_3 = @case.Count;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableListChange(global::TestProject.ObservableListChange.Remove<T> @case) : this()
        {
            this.enumCase = global::TestProject.ObservableListChange.EnumCase.Remove;
            this.field_I_System_x002EInt32_1 = @case.Index;
            this.field_I_TestProject_x002EObservableListChange_x002BRemove_x00601_x00210_4 = @case.Value;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableListChange(global::TestProject.ObservableListChange.RemoveRange @case) : this()
        {
            this.enumCase = global::TestProject.ObservableListChange.EnumCase.RemoveRange;
            this.field_I_System_x002EInt32_1 = @case.Index;
            this.field_I_System_x002EInt32_3 = @case.Count;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableListChange(global::TestProject.ObservableListChange.Replace<T> @case) : this()
        {
            this.enumCase = global::TestProject.ObservableListChange.EnumCase.Replace;
            this.field_I_System_x002EInt32_1 = @case.Index;
            this.field_I_TestProject_x002EObservableListChange_x002BReplace_x00601_x00210_5 = @case.OldValue;
            this.field_I_TestProject_x002EObservableListChange_x002BReplace_x00601_x00210_6 = @case.NewValue;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableListChange(global::TestProject.ObservableListChange.ReplaceRange @case) : this()
        {
            this.enumCase = global::TestProject.ObservableListChange.EnumCase.ReplaceRange;
            this.field_I_System_x002EInt32_1 = @case.Index;
            this.field_I_System_x002EInt32_3 = @case.Count;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableListChange(global::TestProject.ObservableListChange.Move<T> @case) : this()
        {
            this.enumCase = global::TestProject.ObservableListChange.EnumCase.Move;
            this.field_I_System_x002EInt32_1 = @case.PreviousIndex;
            this.field_I_System_x002EInt32_3 = @case.Index;
            this.field_I_TestProject_x002EObservableListChange_x002BMove_x00601_x00210_7 = @case.Value;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableListChange(global::TestProject.ObservableListChange.Clear @case) : this()
        {
            this.enumCase = global::TestProject.ObservableListChange.EnumCase.Clear;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableListChange(global::TestProject.ObservableListChange.Reset @case) : this()
        {
            this.enumCase = global::TestProject.ObservableListChange.EnumCase.Reset;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ObservableListChange(global::TestProject.ObservableListChange.ObservableListChange_Undefined @case) : this()
        {
            this.enumCase = global::TestProject.ObservableListChange.EnumCase.Undefined;
        }

        public object Value
        {
            get => this.enumCase switch
            {
                global::TestProject.ObservableListChange.EnumCase.Add => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableListChange.Add<T>>()),
                global::TestProject.ObservableListChange.EnumCase.AddRange => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableListChange.AddRange>()),
                global::TestProject.ObservableListChange.EnumCase.Remove => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableListChange.Remove<T>>()),
                global::TestProject.ObservableListChange.EnumCase.RemoveRange => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableListChange.RemoveRange>()),
                global::TestProject.ObservableListChange.EnumCase.Replace => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableListChange.Replace<T>>()),
                global::TestProject.ObservableListChange.EnumCase.ReplaceRange => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableListChange.ReplaceRange>()),
                global::TestProject.ObservableListChange.EnumCase.Move => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableListChange.Move<T>>()),
                global::TestProject.ObservableListChange.EnumCase.Clear => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableListChange.Clear>()),
                global::TestProject.ObservableListChange.EnumCase.Reset => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableListChange.Reset>()),
                global::TestProject.ObservableListChange.EnumCase.Undefined => GetValueOrDefault(g__ET.GenericT.T<global::TestProject.ObservableListChange.ObservableListChange_Undefined>()),
                _ => null,
            };
        }

        public bool HasValue
        {
            get => this.enumCase switch
            {
                global::TestProject.ObservableListChange.EnumCase.Add => true,
                global::TestProject.ObservableListChange.EnumCase.AddRange => true,
                global::TestProject.ObservableListChange.EnumCase.Remove => true,
                global::TestProject.ObservableListChange.EnumCase.RemoveRange => true,
                global::TestProject.ObservableListChange.EnumCase.Replace => true,
                global::TestProject.ObservableListChange.EnumCase.ReplaceRange => true,
                global::TestProject.ObservableListChange.EnumCase.Move => true,
                global::TestProject.ObservableListChange.EnumCase.Clear => true,
                global::TestProject.ObservableListChange.EnumCase.Reset => true,
                global::TestProject.ObservableListChange.EnumCase.Undefined => true,
                _ => false
            };
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableListChange<T>(global::TestProject.ObservableListChange.Add<T> @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableListChange.Add<T>(in ObservableListChange<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableListChange.Add<T>>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableListChange<T>(global::TestProject.ObservableListChange.AddRange @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableListChange.AddRange(in ObservableListChange<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableListChange.AddRange>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableListChange<T>(global::TestProject.ObservableListChange.Remove<T> @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableListChange.Remove<T>(in ObservableListChange<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableListChange.Remove<T>>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableListChange<T>(global::TestProject.ObservableListChange.RemoveRange @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableListChange.RemoveRange(in ObservableListChange<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableListChange.RemoveRange>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableListChange<T>(global::TestProject.ObservableListChange.Replace<T> @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableListChange.Replace<T>(in ObservableListChange<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableListChange.Replace<T>>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableListChange<T>(global::TestProject.ObservableListChange.ReplaceRange @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableListChange.ReplaceRange(in ObservableListChange<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableListChange.ReplaceRange>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableListChange<T>(global::TestProject.ObservableListChange.Move<T> @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableListChange.Move<T>(in ObservableListChange<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableListChange.Move<T>>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableListChange<T>(global::TestProject.ObservableListChange.Clear @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableListChange.Clear(in ObservableListChange<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableListChange.Clear>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableListChange<T>(global::TestProject.ObservableListChange.Reset @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableListChange.Reset(in ObservableListChange<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableListChange.Reset>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ObservableListChange<T>(global::TestProject.ObservableListChange.ObservableListChange_Undefined @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator global::TestProject.ObservableListChange.ObservableListChange_Undefined(in ObservableListChange<T> @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.ObservableListChange.ObservableListChange_Undefined>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.Add<T> GetValueOrThrow(g__ET.T<global::TestProject.ObservableListChange.Add<T>> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Add);

            return new(
                  Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BAdd_x00601_x00210_2)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.Add<T> GetValueOrDefault(g__ET.T<global::TestProject.ObservableListChange.Add<T>> _ = default, global::TestProject.ObservableListChange.Add<T> @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Add))
            {
                return new(
                      Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BAdd_x00601_x00210_2)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableListChange.Add<T> value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Add))
            {
                value = new(
                      Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BAdd_x00601_x00210_2)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.AddRange GetValueOrThrow(g__ET.T<global::TestProject.ObservableListChange.AddRange> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.AddRange);

            return new(
                  Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                , Count: g__ET.Option.Some(this.field_I_System_x002EInt32_3)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.AddRange GetValueOrDefault(g__ET.T<global::TestProject.ObservableListChange.AddRange> _ = default, global::TestProject.ObservableListChange.AddRange @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.AddRange))
            {
                return new(
                      Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , Count: g__ET.Option.Some(this.field_I_System_x002EInt32_3)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableListChange.AddRange value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.AddRange))
            {
                value = new(
                      Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , Count: g__ET.Option.Some(this.field_I_System_x002EInt32_3)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.Remove<T> GetValueOrThrow(g__ET.T<global::TestProject.ObservableListChange.Remove<T>> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Remove);

            return new(
                  Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BRemove_x00601_x00210_4)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.Remove<T> GetValueOrDefault(g__ET.T<global::TestProject.ObservableListChange.Remove<T>> _ = default, global::TestProject.ObservableListChange.Remove<T> @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Remove))
            {
                return new(
                      Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BRemove_x00601_x00210_4)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableListChange.Remove<T> value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Remove))
            {
                value = new(
                      Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BRemove_x00601_x00210_4)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.RemoveRange GetValueOrThrow(g__ET.T<global::TestProject.ObservableListChange.RemoveRange> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.RemoveRange);

            return new(
                  Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                , Count: g__ET.Option.Some(this.field_I_System_x002EInt32_3)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.RemoveRange GetValueOrDefault(g__ET.T<global::TestProject.ObservableListChange.RemoveRange> _ = default, global::TestProject.ObservableListChange.RemoveRange @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.RemoveRange))
            {
                return new(
                      Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , Count: g__ET.Option.Some(this.field_I_System_x002EInt32_3)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableListChange.RemoveRange value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.RemoveRange))
            {
                value = new(
                      Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , Count: g__ET.Option.Some(this.field_I_System_x002EInt32_3)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.Replace<T> GetValueOrThrow(g__ET.T<global::TestProject.ObservableListChange.Replace<T>> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Replace);

            return new(
                  Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                , OldValue: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BReplace_x00601_x00210_5)
                , NewValue: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BReplace_x00601_x00210_6)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.Replace<T> GetValueOrDefault(g__ET.T<global::TestProject.ObservableListChange.Replace<T>> _ = default, global::TestProject.ObservableListChange.Replace<T> @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Replace))
            {
                return new(
                      Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , OldValue: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BReplace_x00601_x00210_5)
                    , NewValue: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BReplace_x00601_x00210_6)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableListChange.Replace<T> value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Replace))
            {
                value = new(
                      Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , OldValue: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BReplace_x00601_x00210_5)
                    , NewValue: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BReplace_x00601_x00210_6)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.ReplaceRange GetValueOrThrow(g__ET.T<global::TestProject.ObservableListChange.ReplaceRange> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.ReplaceRange);

            return new(
                  Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                , Count: g__ET.Option.Some(this.field_I_System_x002EInt32_3)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.ReplaceRange GetValueOrDefault(g__ET.T<global::TestProject.ObservableListChange.ReplaceRange> _ = default, global::TestProject.ObservableListChange.ReplaceRange @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.ReplaceRange))
            {
                return new(
                      Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , Count: g__ET.Option.Some(this.field_I_System_x002EInt32_3)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableListChange.ReplaceRange value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.ReplaceRange))
            {
                value = new(
                      Index: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , Count: g__ET.Option.Some(this.field_I_System_x002EInt32_3)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.Move<T> GetValueOrThrow(g__ET.T<global::TestProject.ObservableListChange.Move<T>> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Move);

            return new(
                  PreviousIndex: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                , Index: g__ET.Option.Some(this.field_I_System_x002EInt32_3)
                , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BMove_x00601_x00210_7)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.Move<T> GetValueOrDefault(g__ET.T<global::TestProject.ObservableListChange.Move<T>> _ = default, global::TestProject.ObservableListChange.Move<T> @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Move))
            {
                return new(
                      PreviousIndex: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , Index: g__ET.Option.Some(this.field_I_System_x002EInt32_3)
                    , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BMove_x00601_x00210_7)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableListChange.Move<T> value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Move))
            {
                value = new(
                      PreviousIndex: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , Index: g__ET.Option.Some(this.field_I_System_x002EInt32_3)
                    , Value: g__ET.Option.Some(this.field_I_TestProject_x002EObservableListChange_x002BMove_x00601_x00210_7)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.Clear GetValueOrThrow(g__ET.T<global::TestProject.ObservableListChange.Clear> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Clear);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.Clear GetValueOrDefault(g__ET.T<global::TestProject.ObservableListChange.Clear> _ = default, global::TestProject.ObservableListChange.Clear @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Clear))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableListChange.Clear value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Clear))
            {
                value = new();
                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.Reset GetValueOrThrow(g__ET.T<global::TestProject.ObservableListChange.Reset> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Reset);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.Reset GetValueOrDefault(g__ET.T<global::TestProject.ObservableListChange.Reset> _ = default, global::TestProject.ObservableListChange.Reset @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Reset))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableListChange.Reset value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Reset))
            {
                value = new();
                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.ObservableListChange_Undefined GetValueOrThrow(g__ET.T<global::TestProject.ObservableListChange.ObservableListChange_Undefined> _)
        {
            ThrowIfUncastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Undefined);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.ObservableListChange_Undefined GetValueOrDefault(g__ET.T<global::TestProject.ObservableListChange.ObservableListChange_Undefined> _ = default, global::TestProject.ObservableListChange.ObservableListChange_Undefined @default = default)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Undefined))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out global::TestProject.ObservableListChange.ObservableListChange_Undefined value)
        {
            if (IsCastable(this.enumCase, global::TestProject.ObservableListChange.EnumCase.Undefined))
            {
                value = new();
                return true;
            }

            value = default;
            return false;
        }

        public void GetAffectedRange(out int start, out int end, out bool following, out bool structural)
        {
            switch (this.enumCase)
            {
                case global::TestProject.ObservableListChange.EnumCase.Add:
                {
                    global::TestProject.ObservableListChange.Add<T> enum_case = (global::TestProject.ObservableListChange.Add<T>)this;
                    enum_case.GetAffectedRange(out start, out end, out following, out structural);
                    return;
                }

                case global::TestProject.ObservableListChange.EnumCase.AddRange:
                {
                    global::TestProject.ObservableListChange.AddRange enum_case = (global::TestProject.ObservableListChange.AddRange)this;
                    enum_case.GetAffectedRange(out start, out end, out following, out structural);
                    return;
                }

                case global::TestProject.ObservableListChange.EnumCase.Remove:
                {
                    global::TestProject.ObservableListChange.Remove<T> enum_case = (global::TestProject.ObservableListChange.Remove<T>)this;
                    enum_case.GetAffectedRange(out start, out end, out following, out structural);
                    return;
                }

                case global::TestProject.ObservableListChange.EnumCase.RemoveRange:
                {
                    global::TestProject.ObservableListChange.RemoveRange enum_case = (global::TestProject.ObservableListChange.RemoveRange)this;
                    enum_case.GetAffectedRange(out start, out end, out following, out structural);
                    return;
                }

                case global::TestProject.ObservableListChange.EnumCase.Replace:
                {
                    global::TestProject.ObservableListChange.Replace<T> enum_case = (global::TestProject.ObservableListChange.Replace<T>)this;
                    enum_case.GetAffectedRange(out start, out end, out following, out structural);
                    return;
                }

                case global::TestProject.ObservableListChange.EnumCase.ReplaceRange:
                {
                    global::TestProject.ObservableListChange.ReplaceRange enum_case = (global::TestProject.ObservableListChange.ReplaceRange)this;
                    enum_case.GetAffectedRange(out start, out end, out following, out structural);
                    return;
                }

                case global::TestProject.ObservableListChange.EnumCase.Move:
                {
                    global::TestProject.ObservableListChange.Move<T> enum_case = (global::TestProject.ObservableListChange.Move<T>)this;
                    enum_case.GetAffectedRange(out start, out end, out following, out structural);
                    return;
                }

                case global::TestProject.ObservableListChange.EnumCase.Clear:
                {
                    global::TestProject.ObservableListChange.Clear enum_case = (global::TestProject.ObservableListChange.Clear)this;
                    enum_case.GetAffectedRange(out start, out end, out following, out structural);
                    return;
                }

                case global::TestProject.ObservableListChange.EnumCase.Reset:
                {
                    global::TestProject.ObservableListChange.Reset enum_case = (global::TestProject.ObservableListChange.Reset)this;
                    enum_case.GetAffectedRange(out start, out end, out following, out structural);
                    return;
                }

                default:
                {
                    global::TestProject.ObservableListChange.ObservableListChange_Undefined enum_case = (global::TestProject.ObservableListChange.ObservableListChange_Undefined)this;
                    enum_case.GetAffectedRange(out start, out end, out following, out structural);
                    return;
                }
            }
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly global::TestProject.ObservableListChange.EnumCase GetEnumCase()
        {
            return enumCase;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly ObservableListChange<T> ToObservableListChange()
        {
            return this;
        }

        [g__SRCS.MethodImpl(INLINING)]
        private static bool IsCastable(global::TestProject.ObservableListChange.EnumCase a, global::TestProject.ObservableListChange.EnumCase b)
        {
            return a == b;
        }

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS)]
        private static void ThrowIfUncastable(global::TestProject.ObservableListChange.EnumCase source, global::TestProject.ObservableListChange.EnumCase target)
        {
            if (IsCastable(source, target) == false)
            {
                throw new g__S.InvalidCastException(
                    $"Cannot cast 'ObservableListChange' into '{target}' because it currently stores a '{source}'."
                );
            }

        }

    }

    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial struct ObservableListChange<T> // Internals
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }
}
