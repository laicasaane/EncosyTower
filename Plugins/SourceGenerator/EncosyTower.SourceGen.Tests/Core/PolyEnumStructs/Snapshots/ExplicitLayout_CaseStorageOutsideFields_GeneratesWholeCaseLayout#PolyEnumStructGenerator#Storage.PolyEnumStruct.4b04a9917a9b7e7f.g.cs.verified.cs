#pragma warning disable 0219

using System.Runtime.InteropServices;
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

    partial struct Storage // EnumCase
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        public enum EnumCase : byte
        {
            /// <inheritdoc cref="Storage.Storage_Undefined"/>
            /// <seealso cref="Storage.Storage_Undefined"/>
            Undefined = 0,

            /// <inheritdoc cref="Storage.Mixed"/>
            /// <seealso cref="Storage.Mixed"/>
            Mixed = 1,

            /// <inheritdoc cref="Storage.Secret"/>
            /// <seealso cref="Storage.Secret"/>
            Secret = 2,

            /// <inheritdoc cref="Storage.Nothing"/>
            /// <seealso cref="Storage.Nothing"/>
            Nothing = 3,

        }

    }

#region    INTERFACE ENUM CASE
#endregion ===================

    partial struct Storage // IEnumCase
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        public partial interface IEnumCase
        {
            EnumCase GetEnumCase();

            Storage ToStorage();
        }

    }

#region    CASE STRUCTS
#endregion ============

    partial struct Storage // Case Structs
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct Storage_Undefined : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Undefined;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Storage ToStorage()
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

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Mixed;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Storage ToStorage()
            {
                return this;
            }
        }

        partial struct Secret : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Secret;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Storage ToStorage()
            {
                return this;
            }
        }

        partial struct Nothing : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Nothing;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Storage ToStorage()
            {
                return this;
            }
        }

    }

#region    ENUM STRUCT
#endregion ===========

    partial struct Storage : Storage.IEnumCase, g__SRCS.IUnion, g__ET.IHasValue // Enum Struct
    {
        // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
        [g__SRIS.FieldOffset(0)] public Mixed case_Mixed;
        // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
        [g__SRIS.FieldOffset(0)] public Secret case_Secret;
        // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
        [g__SRIS.FieldOffset(0)] public Nothing case_Nothing;
        // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
        [g__SRIS.FieldOffset(0)] public Storage_Undefined case_Undefined;
        // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
        [g__SRIS.FieldOffset(8)] public EnumCase enumCase;

        [g__SRCS.MethodImpl(INLINING)]
        public Storage(Mixed @case) : this()
        {
            this.enumCase = EnumCase.Mixed;
            this.case_Mixed = @case;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public Storage(Secret @case) : this()
        {
            this.enumCase = EnumCase.Secret;
            this.case_Secret = @case;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public Storage(Nothing @case) : this()
        {
            this.enumCase = EnumCase.Nothing;
            this.case_Nothing = @case;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public Storage(Storage_Undefined @case) : this()
        {
            this.enumCase = EnumCase.Undefined;
            this.case_Undefined = @case;
        }

        public object Value
        {
            get => this.enumCase switch
            {
                EnumCase.Mixed => GetValueOrDefault(g__ET.GenericT.T<Mixed>()),
                EnumCase.Secret => GetValueOrDefault(g__ET.GenericT.T<Secret>()),
                EnumCase.Nothing => GetValueOrDefault(g__ET.GenericT.T<Nothing>()),
                EnumCase.Undefined => GetValueOrDefault(g__ET.GenericT.T<Storage_Undefined>()),
                _ => null,
            };
        }

        public bool HasValue
        {
            get => this.enumCase switch
            {
                EnumCase.Mixed => true,
                EnumCase.Secret => true,
                EnumCase.Nothing => true,
                EnumCase.Undefined => true,
                _ => false
            };
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Storage(Mixed @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator Mixed(in Storage @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<Mixed>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Storage(Secret @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator Secret(in Storage @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<Secret>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Storage(Nothing @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator Nothing(in Storage @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<Nothing>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Storage(Storage_Undefined @case)
        {
            return new(@case);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static explicit operator Storage_Undefined(in Storage @enum)
        {
            return @enum.GetValueOrThrow(g__ET.GenericT.T<Storage_Undefined>());
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Mixed GetValueOrThrow(g__ET.T<Mixed> _)
        {
            ThrowIfUncastable(this.enumCase, EnumCase.Mixed);

            return this.case_Mixed;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Mixed GetValueOrDefault(g__ET.T<Mixed> _ = default, Mixed @default = default)
        {
            if (IsCastable(this.enumCase, EnumCase.Mixed))
            {
                return this.case_Mixed;
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out Mixed value)
        {
            if (IsCastable(this.enumCase, EnumCase.Mixed))
            {
                value = this.case_Mixed;
                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Secret GetValueOrThrow(g__ET.T<Secret> _)
        {
            ThrowIfUncastable(this.enumCase, EnumCase.Secret);

            return this.case_Secret;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Secret GetValueOrDefault(g__ET.T<Secret> _ = default, Secret @default = default)
        {
            if (IsCastable(this.enumCase, EnumCase.Secret))
            {
                return this.case_Secret;
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out Secret value)
        {
            if (IsCastable(this.enumCase, EnumCase.Secret))
            {
                value = this.case_Secret;
                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Nothing GetValueOrThrow(g__ET.T<Nothing> _)
        {
            ThrowIfUncastable(this.enumCase, EnumCase.Nothing);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Nothing GetValueOrDefault(g__ET.T<Nothing> _ = default, Nothing @default = default)
        {
            if (IsCastable(this.enumCase, EnumCase.Nothing))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out Nothing value)
        {
            if (IsCastable(this.enumCase, EnumCase.Nothing))
            {
                value = new();
                return true;
            }

            value = default;
            return false;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Storage_Undefined GetValueOrThrow(g__ET.T<Storage_Undefined> _)
        {
            ThrowIfUncastable(this.enumCase, EnumCase.Undefined);

            return new();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly Storage_Undefined GetValueOrDefault(g__ET.T<Storage_Undefined> _ = default, Storage_Undefined @default = default)
        {
            if (IsCastable(this.enumCase, EnumCase.Undefined))
            {
                return new();
            }

            return @default;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly bool TryGetValue(out Storage_Undefined value)
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
        public readonly Storage ToStorage()
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
                    $"Cannot cast 'Storage' into '{target}' because it currently stores a '{source}'."
                );
            }

        }

    }

#region    ENUM CASE API
#endregion =============

    partial struct Storage // Enum Case API
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        private static partial class EnumCaseAPI
        {
        }
    }

#region    INTERNALS
#endregion =========

    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial struct Storage // Internals
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }



}

