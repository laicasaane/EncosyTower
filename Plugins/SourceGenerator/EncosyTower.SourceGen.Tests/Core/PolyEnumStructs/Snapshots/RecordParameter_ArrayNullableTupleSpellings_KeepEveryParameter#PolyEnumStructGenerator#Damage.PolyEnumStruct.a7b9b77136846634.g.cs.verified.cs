#pragma warning disable 0219

using System;
using System.Collections.Generic;
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

    partial struct Damage // EnumCase
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        public enum EnumCase : byte
        {
            /// <inheritdoc cref="Damage.Damage_Undefined"/>
            /// <seealso cref="Damage.Damage_Undefined"/>
            Undefined = 0,

            /// <inheritdoc cref="Damage.Hit"/>
            /// <seealso cref="Damage.Hit"/>
            Hit = 1,

            /// <inheritdoc cref="Damage.Miss"/>
            /// <seealso cref="Damage.Miss"/>
            Miss = 2,

        }

    }

#region    INTERFACE ENUM CASE
#endregion ===================

    partial struct Damage // IEnumCase
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        public partial interface IEnumCase
        {
            EnumCase GetEnumCase();

            Damage ToDamage();
        }

    }

#region    CASE STRUCTS
#endregion ============

    partial struct Damage // Case Structs
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct Damage_Undefined : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Undefined;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Damage ToDamage()
            {
                return this;
            }
        }

        partial record struct Hit : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public Hit(
                  g__ET.Option<int[]> Values = default
                , g__ET.Option<int?> Maybe = default
                , g__ET.Option<(int, int)> Pair = default
            ) : this(default(int[]), default(int?), default((int, int)))
            {
                this.Values = Values.GetValueOrDefault();
                this.Maybe = Maybe.GetValueOrDefault();
                this.Pair = Pair.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Hit;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Damage ToDamage()
            {
                return this;
            }
        }

        partial struct Miss : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Miss;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Damage ToDamage()
            {
                return this;
            }
        }

    }

#region    ENUM STRUCT
#endregion ===========

    partial struct Damage : Damage.IEnumCase, g__SRCS.IUnion, g__ET.IHasValue // Enum Struct
    {
        public EnumCase enumCase;
        public int[] field_I_System_x002EInt32_x005B_x005D_1;
        public int? field_I_System_x002ENullable_x00601_x003CSystem_x002EInt32_x003E_2;
        public (int, int) field_I_System_x002EValueTuple_x00602_x003CSystem_x002EInt32_x002CSystem_x002EInt32_x003E_3;

        [g__SRCS.MethodImpl(INLINING)]
        public Damage(in Hit @case) : this()
        {
            this.enumCase = EnumCase.Hit;
            this.field_I_System_x002EInt32_x005B_x005D_1 = @case.Values;
            this.field_I_System_x002ENullable_x00601_x003CSystem_x002EInt32_x003E_2 = @case.Maybe;
            this.field_I_System_x002EValueTuple_x00602_x003CSystem_x002EInt32_x002CSystem_x002EInt32_x003E_3 = @case.Pair;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public Damage(Miss @case) : this()
        {
            this.enumCase = EnumCase.Miss;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public Damage(Damage_Undefined @case) : this()
        {
            this.enumCase = EnumCase.Undefined;
        }

        public object Value
        {
            get => this.enumCase switch
            {
                EnumCase.Hit => GetValueOrDefault(g__ET.GenericT.T<Hit>()),
                EnumCase.Miss => GetValueOrDefault(g__ET.GenericT.T<Miss>()),
                EnumCase.Undefined => GetValueOrDefault(g__ET.GenericT.T<Damage_Undefined>()),
                _ => null,
            };
        }

        public bool HasValue
        {
            get => this.enumCase switch
            {
                EnumCase.Hit => true,
                EnumCase.Miss => true,
                EnumCase.Undefined => true,
                _ => false
            };
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Damage(in Hit @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator Hit(in Damage @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<Hit>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Damage(Miss @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator Miss(in Damage @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<Miss>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Damage(Damage_Undefined @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator Damage_Undefined(in Damage @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<Damage_Undefined>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Hit GetValueOrThrow(g__ET.T<Hit> _)
        {
            ThrowIfUncastable(this.enumCase, EnumCase.Hit);

            return new(
                  Values: g__ET.Option.Some(this.field_I_System_x002EInt32_x005B_x005D_1)
                , Maybe: g__ET.Option.Some(this.field_I_System_x002ENullable_x00601_x003CSystem_x002EInt32_x003E_2)
                , Pair: g__ET.Option.Some(this.field_I_System_x002EValueTuple_x00602_x003CSystem_x002EInt32_x002CSystem_x002EInt32_x003E_3)
            );
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Hit GetValueOrDefault(g__ET.T<Hit> _ = default, in Hit @default = default)
        {
            if (IsCastable(this.enumCase, EnumCase.Hit))
            {
                return new(
                      Values: g__ET.Option.Some(this.field_I_System_x002EInt32_x005B_x005D_1)
                    , Maybe: g__ET.Option.Some(this.field_I_System_x002ENullable_x00601_x003CSystem_x002EInt32_x003E_2)
                    , Pair: g__ET.Option.Some(this.field_I_System_x002EValueTuple_x00602_x003CSystem_x002EInt32_x002CSystem_x002EInt32_x003E_3)
                );
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out Hit value)
        {
            if (IsCastable(this.enumCase, EnumCase.Hit))
            {
                value = new(
                      Values: g__ET.Option.Some(this.field_I_System_x002EInt32_x005B_x005D_1)
                    , Maybe: g__ET.Option.Some(this.field_I_System_x002ENullable_x00601_x003CSystem_x002EInt32_x003E_2)
                    , Pair: g__ET.Option.Some(this.field_I_System_x002EValueTuple_x00602_x003CSystem_x002EInt32_x002CSystem_x002EInt32_x003E_3)
                );

                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Miss GetValueOrThrow(g__ET.T<Miss> _)
        {
            ThrowIfUncastable(this.enumCase, EnumCase.Miss);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Miss GetValueOrDefault(g__ET.T<Miss> _ = default, Miss @default = default)
        {
            if (IsCastable(this.enumCase, EnumCase.Miss))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out Miss value)
        {
            if (IsCastable(this.enumCase, EnumCase.Miss))
            {
                value = new();
                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Damage_Undefined GetValueOrThrow(g__ET.T<Damage_Undefined> _)
        {
            ThrowIfUncastable(this.enumCase, EnumCase.Undefined);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Damage_Undefined GetValueOrDefault(g__ET.T<Damage_Undefined> _ = default, Damage_Undefined @default = default)
        {
            if (IsCastable(this.enumCase, EnumCase.Undefined))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out Damage_Undefined value)
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
        public readonly Damage ToDamage()
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
                    $"Cannot cast 'Damage' into '{target}' because it currently stores a '{source}'."
                );
            }

        }

    }

#region    ENUM CASE API
#endregion =============

    partial struct Damage // Enum Case API
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        private static partial class EnumCaseAPI
        {
        }
    }

#region    INTERNALS
#endregion =========

    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial struct Damage // Internals
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }



}

