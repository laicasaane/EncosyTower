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

    partial struct Choice // EnumCase
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")]
        public enum EnumCase : byte
        {
            /// <inheritdoc cref="Choice.Choice_Undefined"/>
            /// <seealso cref="Choice.Choice_Undefined"/>
            Undefined = 0,

            /// <inheritdoc cref="Choice.A"/>
            /// <seealso cref="Choice.A"/>
            A = 1,

        }

    }

#region    INTERFACE ENUM CASE
#endregion ===================

    partial struct Choice // IEnumCase
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")]
        public partial interface IEnumCase
        {
            EnumCase GetEnumCase();

            Choice ToChoice();
        }

    }

#region    CASE STRUCTS
#endregion ============

    partial struct Choice // Case Structs
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct Choice_Undefined : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Undefined;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Choice ToChoice()
            {
                return this;
            }
        }

        partial struct A : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.A;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Choice ToChoice()
            {
                return this;
            }
        }

    }

#region    ENUM STRUCT
#endregion ===========

    partial struct Choice : Choice.IEnumCase, g__SRCS.IUnion, g__ET.IHasValue // Enum Struct
    {
        public EnumCase enumCase;

        [g__SRCS.MethodImpl(INLINING)]
        public Choice(A @case) : this()
        {
            this.enumCase = EnumCase.A;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public Choice(Choice_Undefined @case) : this()
        {
            this.enumCase = EnumCase.Undefined;
        }

        public object Value
        {
            get => this.enumCase switch
            {
                EnumCase.A => GetValueOrDefault(g__ET.GenericT.T<A>()),
                EnumCase.Undefined => GetValueOrDefault(g__ET.GenericT.T<Choice_Undefined>()),
                _ => null,
            };
        }

        public bool HasValue
        {
            get => this.enumCase switch
            {
                EnumCase.A => true,
                EnumCase.Undefined => true,
                _ => false
            };
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Choice(A @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator A(Choice @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<A>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Choice(Choice_Undefined @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator Choice_Undefined(Choice @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<Choice_Undefined>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly A GetValueOrThrow(g__ET.T<A> _)
        {
            ThrowIfUncastable(this.enumCase, EnumCase.A);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly A GetValueOrDefault(g__ET.T<A> _ = default, A @default = default)
        {
            if (IsCastable(this.enumCase, EnumCase.A))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out A value)
        {
            if (IsCastable(this.enumCase, EnumCase.A))
            {
                value = new();
                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Choice_Undefined GetValueOrThrow(g__ET.T<Choice_Undefined> _)
        {
            ThrowIfUncastable(this.enumCase, EnumCase.Undefined);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Choice_Undefined GetValueOrDefault(g__ET.T<Choice_Undefined> _ = default, Choice_Undefined @default = default)
        {
            if (IsCastable(this.enumCase, EnumCase.Undefined))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out Choice_Undefined value)
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
        public readonly Choice ToChoice()
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
                    $"Cannot cast 'Choice' into '{target}' because it currently stores a '{source}'."
                );
            }

        }

    }

#region    ENUM CASE API
#endregion =============

    partial struct Choice // Enum Case API
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        private static partial class EnumCaseAPI
        {
        }
    }

#region    INTERNALS
#endregion =========

    [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
    partial struct Choice // Internals
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }



}

