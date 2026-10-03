#pragma warning disable 0219

using EncosyTower.PolyEnumStructs;

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

#region    ENUM CASE
#endregion =========

    partial struct Holder // EnumCase
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        public enum EnumCase : byte
        {
            /// <inheritdoc cref="Holder.Holder_Undefined"/>
            /// <seealso cref="Holder.Holder_Undefined"/>
            Undefined = 0,

            /// <inheritdoc cref="Holder.Mixed"/>
            /// <seealso cref="Holder.Mixed"/>
            Mixed = 1,

            /// <inheritdoc cref="Holder.Small"/>
            /// <seealso cref="Holder.Small"/>
            Small = 2,

        }

    }

#region    INTERFACE ENUM CASE
#endregion ===================

    partial struct Holder // IEnumCase
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        public partial interface IEnumCase
        {
            EnumCase GetEnumCase();

            Holder ToHolder();
        }

    }

#region    CASE STRUCTS
#endregion ============

    partial struct Holder // Case Structs
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct Holder_Undefined : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Undefined;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Holder ToHolder()
            {
                return this;
            }
        }

        partial struct Mixed : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public Mixed(
                  g__ET.Option<int> visible = default
            ) : this()
            {
                this.visible = visible.GetValueOrDefault();
            }

            [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
            [g__SRCS.MethodImpl(INLINING)]
            internal Mixed(
                  g__ET.T<Mixed> g__tag
                , g__ET.Option<int> visible
                , int _hidden
                , global::System.Action Changed
                , long Total
            ) : this(visible: visible)
            {
                this._hidden = _hidden;
                this.Changed = Changed;
                this.Total = Total;
            }

            [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
            [g__SRCS.MethodImpl(INLINING)]
            internal readonly void CopyStorage(
                  g__ET.T<Mixed> g__tag
                , out int _hidden
                , out global::System.Action Changed
                , out long Total
            )
            {
                _hidden = this._hidden;
                Changed = this.Changed;
                Total = this.Total;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Mixed;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Holder ToHolder()
            {
                return this;
            }
        }

        partial struct Small : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public Small(
                  g__ET.Option<int> visible = default
            )
            {
                this.visible = visible.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Small;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Holder ToHolder()
            {
                return this;
            }
        }

    }

#region    ENUM STRUCT
#endregion ===========

    partial struct Holder : Holder.IEnumCase, g__SRCS.IUnion, g__ET.IHasValue // Enum Struct
    {
        public EnumCase enumCase;
        public int field_I_System_x002EInt32_1;
        public int field_I_System_x002EInt32_2;
        public global::System.Action field_I_System_x002EAction_3;
        public long field_I_System_x002EInt64_4;

        [g__SRCS.MethodImpl(INLINING)]
        public Holder(in Mixed @case) : this()
        {
            this.enumCase = EnumCase.Mixed;
            this.field_I_System_x002EInt32_1 = @case.visible;
            @case.CopyStorage(g__ET.GenericT.T<Mixed>(), _hidden: out this.field_I_System_x002EInt32_2, Changed: out this.field_I_System_x002EAction_3, Total: out this.field_I_System_x002EInt64_4);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public Holder(Small @case) : this()
        {
            this.enumCase = EnumCase.Small;
            this.field_I_System_x002EInt32_1 = @case.visible;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public Holder(Holder_Undefined @case) : this()
        {
            this.enumCase = EnumCase.Undefined;
        }

        public object Value
        {
            get => this.enumCase switch
            {
                EnumCase.Mixed => GetValueOrDefault(g__ET.GenericT.T<Mixed>()),
                EnumCase.Small => GetValueOrDefault(g__ET.GenericT.T<Small>()),
                EnumCase.Undefined => GetValueOrDefault(g__ET.GenericT.T<Holder_Undefined>()),
                _ => null,
            };
        }

        public bool HasValue
        {
            get => this.enumCase switch
            {
                EnumCase.Mixed => true,
                EnumCase.Small => true,
                EnumCase.Undefined => true,
                _ => false
            };
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Holder(in Mixed @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator Mixed(in Holder @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<Mixed>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Holder(Small @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator Small(in Holder @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<Small>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Holder(Holder_Undefined @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator Holder_Undefined(in Holder @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<Holder_Undefined>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Mixed GetValueOrThrow(g__ET.T<Mixed> _)
        {
            ThrowIfUncastable(this.enumCase, EnumCase.Mixed);

            return new(
                  g__ET.GenericT.T<Mixed>()
                , visible: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                , _hidden: this.field_I_System_x002EInt32_2
                , Changed: this.field_I_System_x002EAction_3
                , Total: this.field_I_System_x002EInt64_4
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Mixed GetValueOrDefault(g__ET.T<Mixed> _ = default, in Mixed @default = default)
        {
            if (IsCastable(this.enumCase, EnumCase.Mixed))
            {
                return new(
                      g__ET.GenericT.T<Mixed>()
                    , visible: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , _hidden: this.field_I_System_x002EInt32_2
                    , Changed: this.field_I_System_x002EAction_3
                    , Total: this.field_I_System_x002EInt64_4
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out Mixed value)
        {
            if (IsCastable(this.enumCase, EnumCase.Mixed))
            {
                value = new(
                      g__ET.GenericT.T<Mixed>()
                    , visible: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                    , _hidden: this.field_I_System_x002EInt32_2
                    , Changed: this.field_I_System_x002EAction_3
                    , Total: this.field_I_System_x002EInt64_4
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Small GetValueOrThrow(g__ET.T<Small> _)
        {
            ThrowIfUncastable(this.enumCase, EnumCase.Small);

            return new(
                  visible: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Small GetValueOrDefault(g__ET.T<Small> _ = default, Small @default = default)
        {
            if (IsCastable(this.enumCase, EnumCase.Small))
            {
                return new(
                      visible: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out Small value)
        {
            if (IsCastable(this.enumCase, EnumCase.Small))
            {
                value = new(
                      visible: g__ET.Option.Some(this.field_I_System_x002EInt32_1)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Holder_Undefined GetValueOrThrow(g__ET.T<Holder_Undefined> _)
        {
            ThrowIfUncastable(this.enumCase, EnumCase.Undefined);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Holder_Undefined GetValueOrDefault(g__ET.T<Holder_Undefined> _ = default, Holder_Undefined @default = default)
        {
            if (IsCastable(this.enumCase, EnumCase.Undefined))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out Holder_Undefined value)
        {
            if (IsCastable(this.enumCase, EnumCase.Undefined))
            {
                value = new();
                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly EnumCase GetEnumCase()
        {
            return enumCase;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Holder ToHolder()
        {
            return this;
        }

        [g__SRCS.MethodImpl(INLINING)]
        private static bool IsCastable(EnumCase a, EnumCase b)
        {
            return a == b;
        }

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS)]
        private static void ThrowIfUncastable(EnumCase source, EnumCase target)
        {
            if (IsCastable(source, target) == false)
            {
                throw new g__S.InvalidCastException(
                    $"Cannot cast 'Holder' into '{target}' because it currently stores a '{source}'."
                );
            }

        }

    }

#region    ENUM CASE API
#endregion =============

    partial struct Holder // Enum Case API
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        private static partial class EnumCaseAPI
        {
        }
    }

#region    INTERNALS
#endregion =========

    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial struct Holder // Internals
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }



}

