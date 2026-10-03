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




#pragma warning disable

#region    ENUM CASE
#endregion =========

    partial struct Choice // EnumCase
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
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
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
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
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
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
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        private static partial class EnumCaseAPI
        {
        }
    }

#region    INTERNALS
#endregion =========

    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial struct Choice // Internals
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }

    partial struct Choice_EnumCaseExtended // EnumCaseExtended
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }

    static partial class Choice_EnumCaseExtensions // EnumCaseExtensions
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }

#region ENUM CASE - ENUM EXTENSIONS
#endregion ========================

#region    INTERFACE
#endregion =========

    static partial class Choice_EnumCaseExtensions { } // IChoice_EnumCaseExtensions

    [g__ETEESG.GeneratedEnumExtensionsFor(typeof(Choice.EnumCase), typeof(IChoice_EnumCaseExtensions), typeof(Choice_EnumCaseExtensions), typeof(Choice_EnumCaseExtended))]
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator", "0.1.8-preview.1")]
    public partial interface IChoice_EnumCaseExtensions
        : g__ETEE.IEnumExtensions<Choice_EnumCaseExtended, Choice.EnumCase, byte>
        , g__ETCon.IToFixedString
        , g__ETCon.IToDisplayFixedString
        , g__ETCon.IToFixedString<g__UC.FixedString32Bytes>
        , g__ETCon.IToDisplayFixedString<g__UC.FixedString32Bytes>
    {
    }

#region    EXTENDED STRUCT
#endregion ===============

    static partial class Choice_EnumCaseExtensions { } // Choice_EnumCaseExtended

    [g__ETEESG.GeneratedEnumExtensionsFor(typeof(Choice.EnumCase), typeof(IChoice_EnumCaseExtensions), typeof(Choice_EnumCaseExtensions), typeof(Choice_EnumCaseExtended))]
    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
    public readonly partial struct Choice_EnumCaseExtended : IChoice_EnumCaseExtensions
        , g__S.IEquatable<Choice_EnumCaseExtended>
        , g__S.IComparable<Choice_EnumCaseExtended>
    {
        // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
        [g__SRIS.FieldOffset(0)]
        private readonly Choice.EnumCase _value;

        // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
        [g__SRIS.FieldOffset(0)]
        private readonly byte _underlyingValue;

        [g__SRCS.MethodImpl(INLINING)]
        public Choice_EnumCaseExtended(Choice.EnumCase value) : this()
        {
            _value = value;
        }

        public Choice.EnumCase Value
        {
            [g__SRCS.MethodImpl(INLINING)]
            get => _value;
        }

        public byte UnderlyingValue
        {
            [g__SRCS.MethodImpl(INLINING)]
            get => _underlyingValue;
        }

        public int Length
        {
            [g__SRCS.MethodImpl(INLINING)]
            get => Choice_EnumCaseExtensions.Length;
        }

        public bool IsDefined
        {
            [g__SRCS.MethodImpl(INLINING)]
            get => Choice_EnumCaseExtensions.IsDefined(_value);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public Choice_EnumCaseExtended Create(Choice.EnumCase value) => new Choice_EnumCaseExtended(value);

        [g__SRCS.MethodImpl(INLINING)]
        public Choice_EnumCaseExtended CreateFromUnderlyingValue(byte value) => new Choice_EnumCaseExtended((Choice.EnumCase)value);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToStringFast() => ToStringFast(true);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToStringFast(bool emptyIfUndefined) => Choice_EnumCaseExtensions.ToStringFast(_value, emptyIfUndefined);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToDisplayString() => ToDisplayString(true);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToDisplayString(bool emptyIfUndefined) => Choice_EnumCaseExtensions.ToDisplayStringFast(_value, emptyIfUndefined);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToDisplayStringFast() => ToDisplayStringFast(true);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToDisplayStringFast(bool emptyIfUndefined) => Choice_EnumCaseExtensions.ToDisplayStringFast(_value, emptyIfUndefined);

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryParse(string name, out Choice_EnumCaseExtended value) => TryParse(name, out value, false, false);

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryParse(string name, out Choice_EnumCaseExtended value, bool ignoreCase) => TryParse(name, out value, ignoreCase, false);

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryParse(string name, out Choice_EnumCaseExtended value, bool ignoreCase, bool allowMatchingMetadataAttribute)
        {
            var result = Choice_EnumCaseExtensions.TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);
            value = new Choice_EnumCaseExtended(enumValue);
            return result;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryParse(g__S.ReadOnlySpan<char> name, out Choice_EnumCaseExtended value) => TryParse(name, out value, false, false);

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryParse(g__S.ReadOnlySpan<char> name, out Choice_EnumCaseExtended value, bool ignoreCase) => TryParse(name, out value, ignoreCase, false);

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryParse(g__S.ReadOnlySpan<char> name, out Choice_EnumCaseExtended value, bool ignoreCase, bool allowMatchingMetadataAttribute)
        {
            var result = Choice_EnumCaseExtensions.TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);
            value = new Choice_EnumCaseExtended(enumValue);
            return result;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public g__UC.FixedString32Bytes ToFixedString() => ToFixedString(true);

        [g__SRCS.MethodImpl(INLINING)]
        public g__UC.FixedString32Bytes ToFixedString(bool emptyIfUndefined) => Choice_EnumCaseExtensions.ToFixedString(_value, emptyIfUndefined);

        [g__SRCS.MethodImpl(INLINING)]
        public g__UC.FixedString32Bytes ToDisplayFixedString() => ToDisplayFixedString(true);

        [g__SRCS.MethodImpl(INLINING)]
        public g__UC.FixedString32Bytes ToDisplayFixedString(bool emptyIfUndefined) => Choice_EnumCaseExtensions.ToDisplayFixedString(_value, emptyIfUndefined);

        [g__SRCS.MethodImpl(INLINING)]
        public TFixedString ToFixedString<TFixedString>()
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
            => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToFixedString());

        [g__SRCS.MethodImpl(INLINING)]
        public TFixedString ToDisplayFixedString<TFixedString>()
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
            => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToDisplayFixedString());

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryFormat(
              g__S.Span<char> destination
            , out int charsWritten
        ) => Choice_EnumCaseExtensions.TryFormat(_value, destination, out charsWritten);

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryFormat(
              g__S.Span<char> destination
            , out int charsWritten
            , g__S.ReadOnlySpan<char> format
            , g__S.IFormatProvider provider = null
        ) => Choice_EnumCaseExtensions.TryFormat(_value, destination, out charsWritten, format, provider);

        [g__SRCS.MethodImpl(INLINING)]
        public bool IsNameDefined(string name) => Choice_EnumCaseExtensions.IsNameDefined(name, default(Choice.EnumCase));

        [g__SRCS.MethodImpl(INLINING)]
        public bool IsNameDefined(string name, bool allowMatchingMetadataAttribute) => Choice_EnumCaseExtensions.IsNameDefined(name, default(Choice.EnumCase), allowMatchingMetadataAttribute);

        [g__SRCS.MethodImpl(INLINING)]
        public int ToIndex() => Choice_EnumCaseExtensions.FindIndex(_value);

        [g__SRCS.MethodImpl(INLINING)]
        public override string ToString() => ToStringFast(true);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToString(string format, g__S.IFormatProvider formatProvider) => ToStringFast(true);

        [g__SRCS.MethodImpl(INLINING)]
        public override int GetHashCode() => _underlyingValue.GetHashCode();

        [g__SRCS.MethodImpl(INLINING)]
        public int CompareTo(Choice_EnumCaseExtended other) => this._underlyingValue.CompareTo(other._underlyingValue);

        [g__SRCS.MethodImpl(INLINING)]
        public bool Equals(Choice_EnumCaseExtended other) => this._underlyingValue == other._underlyingValue;

        [g__SRCS.MethodImpl(INLINING)]
        public override bool Equals(object obj) => obj is Choice_EnumCaseExtended other && this._underlyingValue == other._underlyingValue;

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator Choice_EnumCaseExtended(Choice.EnumCase value) => new Choice_EnumCaseExtended(value);

        [g__SRCS.MethodImpl(INLINING)]
        public static bool operator ==(Choice_EnumCaseExtended left, Choice_EnumCaseExtended right) => left._underlyingValue == right._underlyingValue;

        [g__SRCS.MethodImpl(INLINING)]
        public static bool operator !=(Choice_EnumCaseExtended left, Choice_EnumCaseExtended right) => left._underlyingValue != right._underlyingValue;

        [g__SRCS.MethodImpl(INLINING)]
        public static bool operator <(Choice_EnumCaseExtended left, Choice_EnumCaseExtended right) => left._underlyingValue < right._underlyingValue;

        [g__SRCS.MethodImpl(INLINING)]
        public static bool operator >(Choice_EnumCaseExtended left, Choice_EnumCaseExtended right) => left._underlyingValue > right._underlyingValue;

    }

#region    EXTENSIONS
#endregion ==========

    [g__ETEESG.GeneratedEnumExtensionsFor(typeof(Choice.EnumCase), typeof(IChoice_EnumCaseExtensions), typeof(Choice_EnumCaseExtensions), typeof(Choice_EnumCaseExtended))]
    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class Choice_EnumCaseExtensions // Choice_EnumCaseExtensions
    {
        /// <summary>
        /// The number of members in the enum.
        /// This is a non-distinct count of defined names.
        /// </summary>
        public const int Length = 2;

        /// <summary>
        /// Returns the string representation of the <see cref="Choice.EnumCase"/> value.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <returns>The string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static string ToStringFast(this Choice.EnumCase value)
            => ToStringFast(value, true);

        /// <summary>
        /// Returns the string representation of the <see cref="Choice.EnumCase"/> value.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
        /// <returns>The string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static string ToStringFast(this Choice.EnumCase value, bool emptyIfUndefined)
            => Names.Get(value, emptyIfUndefined);

        /// <summary>
        /// Returns the string representation of the <see cref="Choice.EnumCase"/> value.
        /// If the attribute is decorated with a <c>[Display]</c> attribute, then
        /// uses the provided value. Otherwise uses the name of the member, equivalent to
        /// calling <c>ToString()</c> on <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <returns>The string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static string ToDisplayStringFast(this Choice.EnumCase value)
            => ToDisplayStringFast(value, true);

        /// <summary>
        /// Returns the string representation of the <see cref="Choice.EnumCase"/> value.
        /// If the attribute is decorated with a <c>[Display]</c> attribute, then
        /// uses the provided value. Otherwise uses the name of the member, equivalent to
        /// calling <c>ToString()</c> on <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
        /// <returns>The string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static string ToDisplayStringFast(this Choice.EnumCase value, bool emptyIfUndefined)
            => DisplayNames.Get(value, emptyIfUndefined);

        /// <summary>
        /// Returns the fixed string representation of the <see cref="Choice.EnumCase"/> value.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <returns>The fixed string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static g__UC.FixedString32Bytes ToFixedString(this Choice.EnumCase value)
            => ToFixedString(value, true);

        /// <summary>
        /// Returns the fixed string representation of the <see cref="Choice.EnumCase"/> value.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
        /// <returns>The fixed string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static g__UC.FixedString32Bytes ToFixedString(this Choice.EnumCase value, bool emptyIfUndefined)
            => FixedNames.Get(value, emptyIfUndefined);

        /// <summary>
        /// Returns the fixed string representation of the <see cref="Choice.EnumCase"/> value.
        /// If the attribute is decorated with a <c>[Display]</c> attribute, then
        /// uses the provided value. Otherwise uses the name of the member, equivalent to
        /// calling <c>ToString()</c> on <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <returns>The fixed string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static g__UC.FixedString32Bytes ToDisplayFixedString(this Choice.EnumCase value)
            => ToDisplayFixedString(value, true);

        /// <summary>
        /// Returns the fixed string representation of the <see cref="Choice.EnumCase"/> value.
        /// If the attribute is decorated with a <c>[Display]</c> attribute, then
        /// uses the provided value. Otherwise uses the name of the member, equivalent to
        /// calling <c>ToString()</c> on <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
        /// <returns>The fixed string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static g__UC.FixedString32Bytes ToDisplayFixedString(this Choice.EnumCase value, bool emptyIfUndefined)
            => FixedDisplayNames.Get(value, emptyIfUndefined);

        private static g__UC.FixedString32Bytes ToFixedString(byte value)
        {
            var fs = new g__UC.FixedString32Bytes();
            g__UC.FixedStringMethods.Append(ref fs, value);
            return fs;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static Choice_EnumCaseExtended AsExtended(this Choice.EnumCase value)
            => new Choice_EnumCaseExtended(value);

        [g__SRCS.MethodImpl(INLINING)]
        public static Choice_EnumCaseExtended AsChoice_EnumCaseExtended(this byte value)
            => new Choice_EnumCaseExtended((Choice.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static Choice_EnumCaseExtended AsChoice_EnumCaseExtended(this sbyte value)
            => new Choice_EnumCaseExtended((Choice.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static Choice_EnumCaseExtended AsChoice_EnumCaseExtended(this short value)
            => new Choice_EnumCaseExtended((Choice.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static Choice_EnumCaseExtended AsChoice_EnumCaseExtended(this ushort value)
            => new Choice_EnumCaseExtended((Choice.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static Choice_EnumCaseExtended AsChoice_EnumCaseExtended(this int value)
            => new Choice_EnumCaseExtended((Choice.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static Choice_EnumCaseExtended AsChoice_EnumCaseExtended(this uint value)
            => new Choice_EnumCaseExtended((Choice.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static Choice_EnumCaseExtended AsChoice_EnumCaseExtended(this long value)
            => new Choice_EnumCaseExtended((Choice.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static Choice_EnumCaseExtended AsChoice_EnumCaseExtended(this ulong value)
            => new Choice_EnumCaseExtended((Choice.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static byte ToUnderlyingValue(this Choice.EnumCase value)
            => (byte)value;

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref="Choice.EnumCase" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name="value">When this method returns, contains an object of type 
        /// <see cref="Choice.EnumCase" /> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref="Choice.EnumCase" />. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static bool TryParse(this string name, out Choice.EnumCase value)
            => TryParse(name, out value, false, false);

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref="Choice.EnumCase" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name="value">When this method returns, contains an object of type 
        /// <see cref="Choice.EnumCase" /> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref="Choice.EnumCase" />. This parameter is passed uninitialized.</param>
        /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static bool TryParse(this string name, out Choice.EnumCase value, bool ignoreCase)
            => TryParse(name, out value, ignoreCase, false);

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref="Choice.EnumCase" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name="value">When this method returns, contains an object of type 
        /// <see cref="Choice.EnumCase" /> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref="Choice.EnumCase" />. This parameter is passed uninitialized.</param>
        /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
        /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
        /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(this string name, out Choice.EnumCase value, bool ignoreCase, bool allowMatchingMetadataAttribute)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                value = default;
                return false;
            }

            var stringComparison = ignoreCase ? g__S.StringComparison.OrdinalIgnoreCase : g__S.StringComparison.Ordinal;

            switch (name)
            {
                case string s when s.Equals(Names.Undefined, stringComparison):
                {
                    value = Choice.EnumCase.Undefined;
                    return true;
                }

                case string s when s.Equals(Names.A, stringComparison):
                {
                    value = Choice.EnumCase.A;
                    return true;
                }

                case string s when byte.TryParse(name, out var underlyingValue):
                {
                    value = (Choice.EnumCase)underlyingValue;
                    return true;
                }

                default:
                {
                    value = default;
                    return false;
                }
            }
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref="Choice.EnumCase" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name="value">When this method returns, contains an object of type 
        /// <see cref="Choice.EnumCase" /> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref="Choice.EnumCase" />. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static bool TryParse(this g__S.ReadOnlySpan<char> name, out Choice.EnumCase value)
            => TryParse(name, out value, false, false);

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref="Choice.EnumCase" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name="value">When this method returns, contains an object of type 
        /// <see cref="Choice.EnumCase" /> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref="Choice.EnumCase" />. This parameter is passed uninitialized.</param>
        /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static bool TryParse(this g__S.ReadOnlySpan<char> name, out Choice.EnumCase value, bool ignoreCase)
            => TryParse(name, out value, ignoreCase, false);

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref="Choice.EnumCase" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name="value">When this method returns, contains an object of type 
        /// <see cref="Choice.EnumCase" /> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref="Choice.EnumCase" />. This parameter is passed uninitialized.</param>
        /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
        /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
        /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(this g__S.ReadOnlySpan<char> name, out Choice.EnumCase value, bool ignoreCase, bool allowMatchingMetadataAttribute)
        {
            if (name.IsEmpty)
            {
                value = default;
                return false;
            }

            var stringComparison = ignoreCase ? g__S.StringComparison.OrdinalIgnoreCase : g__S.StringComparison.Ordinal;

            switch (name)
            {
                case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.Undefined), stringComparison):
                {
                    value = Choice.EnumCase.Undefined;
                    return true;
                }

                case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.A), stringComparison):
                {
                    value = Choice.EnumCase.A;
                    return true;
                }

                case g__S.ReadOnlySpan<char> s when byte.TryParse(name, out var underlyingValue):
                {
                    value = (Choice.EnumCase)underlyingValue;
                    return true;
                }

                default:
                {
                    value = default;
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns a boolean telling whether the given enum value exists in the enumeration.
        /// </summary>
        /// <param name="value">The value to check if it's defined</param>
        /// <returns><c>true</c> if the value exists in the enumeration, <c>false</c> otherwise</returns>
        public static bool IsDefined(this Choice.EnumCase value)
            => value switch
            {
                Choice.EnumCase.Undefined => true,
                Choice.EnumCase.A => true,
                _ => false,
            };

        /// <summary>
        /// Returns a boolean telling whether an enum with the given name exists in the enumeration.
        /// </summary>
        /// <param name="name">The name to check if it's defined</param>
        /// <returns><c>true</c> if a member with the name exists in the enumeration, <c>false</c> otherwise</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static bool IsNameDefined(this string name, Choice.EnumCase _)
            => IsNameDefined(name, default(Choice.EnumCase), allowMatchingMetadataAttribute: false);

        /// <summary>
        /// Returns a boolean telling whether an enum with the given name exists in the enumeration,
        /// or if a member decorated with a <c>[Display]</c> attribute
        /// with the required name exists.
        /// </summary>
        /// <param name="name">The name to check if it's defined</param>
        /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value of metadata attributes, otherwise ignores them</param>
        /// <returns><c>true</c> if a member with the name exists in the enumeration, or a member is decorated
        /// with a <c>[Display]</c> attribute with the name, <c>false</c> otherwise</returns>
        public static bool IsNameDefined(this string name, Choice.EnumCase _, bool allowMatchingMetadataAttribute)
        {
            return name switch
            {
                Names.Undefined => true,
                Names.A => true,
                _ => false,
            };
        }

        public static bool TryFormat(
              this Choice.EnumCase value
            , g__S.Span<char> destination
            , out int charsWritten
        )
        {
            if (IsDefined(value))
            {
                return g__ETCol.EncosyFixedStringExtensions.TryFormat(ToFixedString(value), destination, out charsWritten);
            }

            return g__ETCol.EncosyFixedStringExtensions.TryFormat(g__ETCol.EncosyFixedStringExtensions.ToFixedString(ToUnderlyingValue(value)), destination, out charsWritten);
        }

        public static bool TryFormat(
              this Choice.EnumCase value
            , g__S.Span<char> destination
            , out int charsWritten
            , g__S.ReadOnlySpan<char> format
            , g__S.IFormatProvider provider = null
        )
        {
            if (IsDefined(value))
            {
                return g__ETCol.EncosyFixedStringExtensions.TryFormat(ToFixedString(value), destination, out charsWritten);
            }

            if (ToUnderlyingValue(value).TryFormat(destination, out var chars, format, provider))
            {
                charsWritten = chars;
                 return true;
            }

            charsWritten = 0;
             return false;
        }

        /// <summary>
        /// Finds the index for a given enum value in the enumeration.
        /// </summary>
        /// <param name="value">The value to find the index for</param>
        /// <returns>The zero-based index if the enum value exists in the enumeration, otherwise -1.</returns>
        public static int FindIndex(this Choice.EnumCase value)
            => value switch
            {
                Choice.EnumCase.Undefined => 0,
                Choice.EnumCase.A => 1,
                _ => -1,
            };

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class Values
        {
            private static readonly Choice.EnumCase[] s_values = new Choice.EnumCase[]
            {
                Choice.EnumCase.Undefined,
                Choice.EnumCase.A,
            };

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlyMemory<Choice.EnumCase> AsMemory() => s_values;

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlySpan<Choice.EnumCase> AsSpan() => s_values;

            [g__SRCS.MethodImpl(INLINING)]
            public static g__UC.NativeArray<Choice.EnumCase> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
                => g__UC.CollectionHelper.CreateNativeArray<Choice.EnumCase>(s_values, allocator);
        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class UnderlyingValues
        {
            private static readonly byte[] s_values = new byte[]
            {
                ToUnderlyingValue(Choice.EnumCase.Undefined),
                ToUnderlyingValue(Choice.EnumCase.A),
            };

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlyMemory<byte> AsMemory() => s_values;

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlySpan<byte> AsSpan() => s_values;

            [g__SRCS.MethodImpl(INLINING)]
            public static g__UC.NativeArray<byte> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
                => g__UC.CollectionHelper.CreateNativeArray<byte>(s_values, allocator);
        }

    }

#region    NAMES
#endregion =====

    static partial class Choice_EnumCaseExtensions// Names
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class Names
        {
            public const string Undefined = nameof(Choice.EnumCase.Undefined);

            public const string A = nameof(Choice.EnumCase.A);

            private static readonly string[] s_names = new string[]
            {
                Undefined,
                A,
            };

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

            [g__SRCS.MethodImpl(INLINING)]
            public static string Get(Choice.EnumCase value) => Get(value, true);

            public static string Get(Choice.EnumCase value, bool emptyIfUndefined)
                => value switch
                {
                    Choice.EnumCase.Undefined => Undefined,
                    Choice.EnumCase.A => A,
                    _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
                };
        }

    }

#region    DISPLAY NAMES
#endregion =============

    static partial class Choice_EnumCaseExtensions// DisplayNames
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class DisplayNames
        {
            public const string Undefined = "Undefined";

            public const string A = "A";

            private static readonly string[] s_names = new string[]
            {
                Undefined,
                A,
            };

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

            [g__SRCS.MethodImpl(INLINING)]
            public static string Get(Choice.EnumCase value) => Get(value, true);

            public static string Get(Choice.EnumCase value, bool emptyIfUndefined)
                => value switch
                {
                    Choice.EnumCase.Undefined => Undefined,
                    Choice.EnumCase.A => A,
                    _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
                };
        }

    }

#region    FIXED NAMES
#endregion ===========

    static partial class Choice_EnumCaseExtensions// FixedNames
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class FixedNames
        {
            public static g__UC.FixedString32Bytes Undefined
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => (g__UC.FixedString32Bytes)Names.Undefined;
            }

            public static g__UC.FixedString32Bytes A
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => (g__UC.FixedString32Bytes)Names.A;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
                => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

            public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
                where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
            {
                var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(Choice_EnumCaseExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
                names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Undefined);
                names[1] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(A);
                return names;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static g__UC.FixedString32Bytes Get(Choice.EnumCase value)
                => Get(value, true);

            public static g__UC.FixedString32Bytes Get(Choice.EnumCase value, bool emptyIfUndefined)
                => value switch
                {
                    Choice.EnumCase.Undefined => Undefined,
                    Choice.EnumCase.A => A,
                    _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
                };
        }

    }

#region    FIXED DISPLAY NAMES
#endregion ===================

    static partial class Choice_EnumCaseExtensions// FixedDisplayNames
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class FixedDisplayNames
        {
            public static g__UC.FixedString32Bytes Undefined
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => (g__UC.FixedString32Bytes)DisplayNames.Undefined;
            }

            public static g__UC.FixedString32Bytes A
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => (g__UC.FixedString32Bytes)DisplayNames.A;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
                => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

            public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
                where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
            {
                var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(Choice_EnumCaseExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
                names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Undefined);
                names[1] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(A);
                return names;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static g__UC.FixedString32Bytes Get(Choice.EnumCase value)
                => Get(value, true);

            public static g__UC.FixedString32Bytes Get(Choice.EnumCase value, bool emptyIfUndefined)
                => value switch
                {
                    Choice.EnumCase.Undefined => Undefined,
                    Choice.EnumCase.A => A,
                    _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
                };
        }

    }



