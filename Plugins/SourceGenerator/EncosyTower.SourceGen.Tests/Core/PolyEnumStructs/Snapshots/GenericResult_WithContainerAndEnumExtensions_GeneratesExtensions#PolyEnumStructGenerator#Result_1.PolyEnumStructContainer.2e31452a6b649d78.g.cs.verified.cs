#pragma warning disable 0219

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

    public static partial class ResultCases
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        public enum EnumCase : byte
        {
            Undefined = 0,

            Success = 1,

        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        public partial interface IEnumCase
        {
            EnumCase GetEnumCase();
        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        public partial interface IEnumCase<T> : IEnumCase
        {
            T Value { get; }

        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct Result_Undefined<T> : IEnumCase, IEnumCase<T>
        {
            public readonly T Value
            {
                [g__SRCS.MethodImpl(INLINING)]
                get
                {
                    return default;
                }

            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Undefined;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.Result<T> ToResult()
            {
                return this;
            }
        }

        partial record struct Success<T> : IEnumCase, IEnumCase<T>
        {
            [g__SRCS.MethodImpl(INLINING)]
            public Success(
                  g__ET.Option<T> Value = default
            ) : this(default(T))
            {
                this.Value = Value.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Success;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.Result<T> ToResult()
            {
                return this;
            }
        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        internal static partial class EnumCaseAPI
        {
        }
    }

    partial struct ResultCases_EnumCaseExtended // EnumCaseExtended
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }

    static partial class ResultCases_EnumCaseExtensions // EnumCaseExtensions
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

    }

#region ENUM CASE - ENUM EXTENSIONS
#endregion ========================

#region    INTERFACE
#endregion =========

    static partial class ResultCases_EnumCaseExtensions { } // IResultCases_EnumCaseExtensions

    [g__ETEESG.GeneratedEnumExtensionsFor(typeof(ResultCases.EnumCase), typeof(IResultCases_EnumCaseExtensions), typeof(ResultCases_EnumCaseExtensions), typeof(ResultCases_EnumCaseExtended))]
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator", "0.1.8-preview.1")]
    public partial interface IResultCases_EnumCaseExtensions
        : g__ETEE.IEnumExtensions<ResultCases_EnumCaseExtended, ResultCases.EnumCase, byte>
        , g__ETCon.IToFixedString
        , g__ETCon.IToDisplayFixedString
        , g__ETCon.IToFixedString<g__UC.FixedString32Bytes>
        , g__ETCon.IToDisplayFixedString<g__UC.FixedString32Bytes>
    {
    }

#region    EXTENDED STRUCT
#endregion ===============

    static partial class ResultCases_EnumCaseExtensions { } // ResultCases_EnumCaseExtended

    [g__ETEESG.GeneratedEnumExtensionsFor(typeof(ResultCases.EnumCase), typeof(IResultCases_EnumCaseExtensions), typeof(ResultCases_EnumCaseExtensions), typeof(ResultCases_EnumCaseExtended))]
    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
    public readonly partial struct ResultCases_EnumCaseExtended : IResultCases_EnumCaseExtensions
        , g__S.IEquatable<ResultCases_EnumCaseExtended>
        , g__S.IComparable<ResultCases_EnumCaseExtended>
    {
        // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
        [g__SRIS.FieldOffset(0)]
        private readonly ResultCases.EnumCase _value;

        // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
        [g__SRIS.FieldOffset(0)]
        private readonly byte _underlyingValue;

        [g__SRCS.MethodImpl(INLINING)]
        public ResultCases_EnumCaseExtended(ResultCases.EnumCase value) : this()
        {
            _value = value;
        }

        public ResultCases.EnumCase Value
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
            get => ResultCases_EnumCaseExtensions.Length;
        }

        public bool IsDefined
        {
            [g__SRCS.MethodImpl(INLINING)]
            get => ResultCases_EnumCaseExtensions.IsDefined(_value);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public ResultCases_EnumCaseExtended Create(ResultCases.EnumCase value) => new ResultCases_EnumCaseExtended(value);

        [g__SRCS.MethodImpl(INLINING)]
        public ResultCases_EnumCaseExtended CreateFromUnderlyingValue(byte value) => new ResultCases_EnumCaseExtended((ResultCases.EnumCase)value);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToStringFast() => ToStringFast(true);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToStringFast(bool emptyIfUndefined) => ResultCases_EnumCaseExtensions.ToStringFast(_value, emptyIfUndefined);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToDisplayString() => ToDisplayString(true);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToDisplayString(bool emptyIfUndefined) => ResultCases_EnumCaseExtensions.ToDisplayStringFast(_value, emptyIfUndefined);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToDisplayStringFast() => ToDisplayStringFast(true);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToDisplayStringFast(bool emptyIfUndefined) => ResultCases_EnumCaseExtensions.ToDisplayStringFast(_value, emptyIfUndefined);

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryParse(string name, out ResultCases_EnumCaseExtended value) => TryParse(name, out value, false, false);

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryParse(string name, out ResultCases_EnumCaseExtended value, bool ignoreCase) => TryParse(name, out value, ignoreCase, false);

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryParse(string name, out ResultCases_EnumCaseExtended value, bool ignoreCase, bool allowMatchingMetadataAttribute)
        {
            var result = ResultCases_EnumCaseExtensions.TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);
            value = new ResultCases_EnumCaseExtended(enumValue);
            return result;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryParse(g__S.ReadOnlySpan<char> name, out ResultCases_EnumCaseExtended value) => TryParse(name, out value, false, false);

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryParse(g__S.ReadOnlySpan<char> name, out ResultCases_EnumCaseExtended value, bool ignoreCase) => TryParse(name, out value, ignoreCase, false);

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryParse(g__S.ReadOnlySpan<char> name, out ResultCases_EnumCaseExtended value, bool ignoreCase, bool allowMatchingMetadataAttribute)
        {
            var result = ResultCases_EnumCaseExtensions.TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);
            value = new ResultCases_EnumCaseExtended(enumValue);
            return result;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public g__UC.FixedString32Bytes ToFixedString() => ToFixedString(true);

        [g__SRCS.MethodImpl(INLINING)]
        public g__UC.FixedString32Bytes ToFixedString(bool emptyIfUndefined) => ResultCases_EnumCaseExtensions.ToFixedString(_value, emptyIfUndefined);

        [g__SRCS.MethodImpl(INLINING)]
        public g__UC.FixedString32Bytes ToDisplayFixedString() => ToDisplayFixedString(true);

        [g__SRCS.MethodImpl(INLINING)]
        public g__UC.FixedString32Bytes ToDisplayFixedString(bool emptyIfUndefined) => ResultCases_EnumCaseExtensions.ToDisplayFixedString(_value, emptyIfUndefined);

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
        ) => ResultCases_EnumCaseExtensions.TryFormat(_value, destination, out charsWritten);

        [g__SRCS.MethodImpl(INLINING)]
        public bool TryFormat(
              g__S.Span<char> destination
            , out int charsWritten
            , g__S.ReadOnlySpan<char> format
            , g__S.IFormatProvider provider = null
        ) => ResultCases_EnumCaseExtensions.TryFormat(_value, destination, out charsWritten, format, provider);

        [g__SRCS.MethodImpl(INLINING)]
        public bool IsNameDefined(string name) => ResultCases_EnumCaseExtensions.IsNameDefined(name, default(ResultCases.EnumCase));

        [g__SRCS.MethodImpl(INLINING)]
        public bool IsNameDefined(string name, bool allowMatchingMetadataAttribute) => ResultCases_EnumCaseExtensions.IsNameDefined(name, default(ResultCases.EnumCase), allowMatchingMetadataAttribute);

        [g__SRCS.MethodImpl(INLINING)]
        public int ToIndex() => ResultCases_EnumCaseExtensions.FindIndex(_value);

        [g__SRCS.MethodImpl(INLINING)]
        public override string ToString() => ToStringFast(true);

        [g__SRCS.MethodImpl(INLINING)]
        public string ToString(string format, g__S.IFormatProvider formatProvider) => ToStringFast(true);

        [g__SRCS.MethodImpl(INLINING)]
        public override int GetHashCode() => _underlyingValue.GetHashCode();

        [g__SRCS.MethodImpl(INLINING)]
        public int CompareTo(ResultCases_EnumCaseExtended other) => this._underlyingValue.CompareTo(other._underlyingValue);

        [g__SRCS.MethodImpl(INLINING)]
        public bool Equals(ResultCases_EnumCaseExtended other) => this._underlyingValue == other._underlyingValue;

        [g__SRCS.MethodImpl(INLINING)]
        public override bool Equals(object obj) => obj is ResultCases_EnumCaseExtended other && this._underlyingValue == other._underlyingValue;

        [g__SRCS.MethodImpl(INLINING)]
        public static implicit operator ResultCases_EnumCaseExtended(ResultCases.EnumCase value) => new ResultCases_EnumCaseExtended(value);

        [g__SRCS.MethodImpl(INLINING)]
        public static bool operator ==(ResultCases_EnumCaseExtended left, ResultCases_EnumCaseExtended right) => left._underlyingValue == right._underlyingValue;

        [g__SRCS.MethodImpl(INLINING)]
        public static bool operator !=(ResultCases_EnumCaseExtended left, ResultCases_EnumCaseExtended right) => left._underlyingValue != right._underlyingValue;

        [g__SRCS.MethodImpl(INLINING)]
        public static bool operator <(ResultCases_EnumCaseExtended left, ResultCases_EnumCaseExtended right) => left._underlyingValue < right._underlyingValue;

        [g__SRCS.MethodImpl(INLINING)]
        public static bool operator >(ResultCases_EnumCaseExtended left, ResultCases_EnumCaseExtended right) => left._underlyingValue > right._underlyingValue;

    }

#region    EXTENSIONS
#endregion ==========

    [g__ETEESG.GeneratedEnumExtensionsFor(typeof(ResultCases.EnumCase), typeof(IResultCases_EnumCaseExtensions), typeof(ResultCases_EnumCaseExtensions), typeof(ResultCases_EnumCaseExtended))]
    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class ResultCases_EnumCaseExtensions // ResultCases_EnumCaseExtensions
    {
        /// <summary>
        /// The number of members in the enum.
        /// This is a non-distinct count of defined names.
        /// </summary>
        public const int Length = 2;

        /// <summary>
        /// Returns the string representation of the <see cref="ResultCases.EnumCase"/> value.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <returns>The string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static string ToStringFast(this ResultCases.EnumCase value)
            => ToStringFast(value, true);

        /// <summary>
        /// Returns the string representation of the <see cref="ResultCases.EnumCase"/> value.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
        /// <returns>The string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static string ToStringFast(this ResultCases.EnumCase value, bool emptyIfUndefined)
            => Names.Get(value, emptyIfUndefined);

        /// <summary>
        /// Returns the string representation of the <see cref="ResultCases.EnumCase"/> value.
        /// If the attribute is decorated with a <c>[Display]</c> attribute, then
        /// uses the provided value. Otherwise uses the name of the member, equivalent to
        /// calling <c>ToString()</c> on <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <returns>The string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static string ToDisplayStringFast(this ResultCases.EnumCase value)
            => ToDisplayStringFast(value, true);

        /// <summary>
        /// Returns the string representation of the <see cref="ResultCases.EnumCase"/> value.
        /// If the attribute is decorated with a <c>[Display]</c> attribute, then
        /// uses the provided value. Otherwise uses the name of the member, equivalent to
        /// calling <c>ToString()</c> on <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
        /// <returns>The string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static string ToDisplayStringFast(this ResultCases.EnumCase value, bool emptyIfUndefined)
            => DisplayNames.Get(value, emptyIfUndefined);

        /// <summary>
        /// Returns the fixed string representation of the <see cref="ResultCases.EnumCase"/> value.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <returns>The fixed string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static g__UC.FixedString32Bytes ToFixedString(this ResultCases.EnumCase value)
            => ToFixedString(value, true);

        /// <summary>
        /// Returns the fixed string representation of the <see cref="ResultCases.EnumCase"/> value.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
        /// <returns>The fixed string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static g__UC.FixedString32Bytes ToFixedString(this ResultCases.EnumCase value, bool emptyIfUndefined)
            => FixedNames.Get(value, emptyIfUndefined);

        /// <summary>
        /// Returns the fixed string representation of the <see cref="ResultCases.EnumCase"/> value.
        /// If the attribute is decorated with a <c>[Display]</c> attribute, then
        /// uses the provided value. Otherwise uses the name of the member, equivalent to
        /// calling <c>ToString()</c> on <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <returns>The fixed string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static g__UC.FixedString32Bytes ToDisplayFixedString(this ResultCases.EnumCase value)
            => ToDisplayFixedString(value, true);

        /// <summary>
        /// Returns the fixed string representation of the <see cref="ResultCases.EnumCase"/> value.
        /// If the attribute is decorated with a <c>[Display]</c> attribute, then
        /// uses the provided value. Otherwise uses the name of the member, equivalent to
        /// calling <c>ToString()</c> on <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to retrieve the string value for</param>
        /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
        /// <returns>The fixed string representation of the value</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static g__UC.FixedString32Bytes ToDisplayFixedString(this ResultCases.EnumCase value, bool emptyIfUndefined)
            => FixedDisplayNames.Get(value, emptyIfUndefined);

        private static g__UC.FixedString32Bytes ToFixedString(byte value)
        {
            var fs = new g__UC.FixedString32Bytes();
            g__UC.FixedStringMethods.Append(ref fs, value);
            return fs;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static ResultCases_EnumCaseExtended AsExtended(this ResultCases.EnumCase value)
            => new ResultCases_EnumCaseExtended(value);

        [g__SRCS.MethodImpl(INLINING)]
        public static ResultCases_EnumCaseExtended AsResultCases_EnumCaseExtended(this byte value)
            => new ResultCases_EnumCaseExtended((ResultCases.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static ResultCases_EnumCaseExtended AsResultCases_EnumCaseExtended(this sbyte value)
            => new ResultCases_EnumCaseExtended((ResultCases.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static ResultCases_EnumCaseExtended AsResultCases_EnumCaseExtended(this short value)
            => new ResultCases_EnumCaseExtended((ResultCases.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static ResultCases_EnumCaseExtended AsResultCases_EnumCaseExtended(this ushort value)
            => new ResultCases_EnumCaseExtended((ResultCases.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static ResultCases_EnumCaseExtended AsResultCases_EnumCaseExtended(this int value)
            => new ResultCases_EnumCaseExtended((ResultCases.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static ResultCases_EnumCaseExtended AsResultCases_EnumCaseExtended(this uint value)
            => new ResultCases_EnumCaseExtended((ResultCases.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static ResultCases_EnumCaseExtended AsResultCases_EnumCaseExtended(this long value)
            => new ResultCases_EnumCaseExtended((ResultCases.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static ResultCases_EnumCaseExtended AsResultCases_EnumCaseExtended(this ulong value)
            => new ResultCases_EnumCaseExtended((ResultCases.EnumCase)(byte)value);

        [g__SRCS.MethodImpl(INLINING)]
        public static byte ToUnderlyingValue(this ResultCases.EnumCase value)
            => (byte)value;

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref="ResultCases.EnumCase" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name="value">When this method returns, contains an object of type 
        /// <see cref="ResultCases.EnumCase" /> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref="ResultCases.EnumCase" />. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static bool TryParse(this string name, out ResultCases.EnumCase value)
            => TryParse(name, out value, false, false);

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref="ResultCases.EnumCase" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name="value">When this method returns, contains an object of type 
        /// <see cref="ResultCases.EnumCase" /> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref="ResultCases.EnumCase" />. This parameter is passed uninitialized.</param>
        /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static bool TryParse(this string name, out ResultCases.EnumCase value, bool ignoreCase)
            => TryParse(name, out value, ignoreCase, false);

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref="ResultCases.EnumCase" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name="value">When this method returns, contains an object of type 
        /// <see cref="ResultCases.EnumCase" /> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref="ResultCases.EnumCase" />. This parameter is passed uninitialized.</param>
        /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
        /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
        /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(this string name, out ResultCases.EnumCase value, bool ignoreCase, bool allowMatchingMetadataAttribute)
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
                    value = ResultCases.EnumCase.Undefined;
                    return true;
                }

                case string s when s.Equals(Names.Success, stringComparison):
                {
                    value = ResultCases.EnumCase.Success;
                    return true;
                }

                case string s when byte.TryParse(name, out var underlyingValue):
                {
                    value = (ResultCases.EnumCase)underlyingValue;
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
        /// an <see cref="ResultCases.EnumCase" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name="value">When this method returns, contains an object of type 
        /// <see cref="ResultCases.EnumCase" /> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref="ResultCases.EnumCase" />. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static bool TryParse(this g__S.ReadOnlySpan<char> name, out ResultCases.EnumCase value)
            => TryParse(name, out value, false, false);

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref="ResultCases.EnumCase" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name="value">When this method returns, contains an object of type 
        /// <see cref="ResultCases.EnumCase" /> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref="ResultCases.EnumCase" />. This parameter is passed uninitialized.</param>
        /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static bool TryParse(this g__S.ReadOnlySpan<char> name, out ResultCases.EnumCase value, bool ignoreCase)
            => TryParse(name, out value, ignoreCase, false);

        /// <summary>
        /// Converts the string representation of the name or numeric value of
        /// an <see cref="ResultCases.EnumCase" /> to the equivalent instance.
        /// The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
        /// <param name="value">When this method returns, contains an object of type 
        /// <see cref="ResultCases.EnumCase" /> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds.
        /// If the parse operation fails, contains the default value of the underlying type
        /// of <see cref="ResultCases.EnumCase" />. This parameter is passed uninitialized.</param>
        /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
        /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
        /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(this g__S.ReadOnlySpan<char> name, out ResultCases.EnumCase value, bool ignoreCase, bool allowMatchingMetadataAttribute)
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
                    value = ResultCases.EnumCase.Undefined;
                    return true;
                }

                case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.Success), stringComparison):
                {
                    value = ResultCases.EnumCase.Success;
                    return true;
                }

                case g__S.ReadOnlySpan<char> s when byte.TryParse(name, out var underlyingValue):
                {
                    value = (ResultCases.EnumCase)underlyingValue;
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
        public static bool IsDefined(this ResultCases.EnumCase value)
            => value switch
            {
                ResultCases.EnumCase.Undefined => true,
                ResultCases.EnumCase.Success => true,
                _ => false,
            };

        /// <summary>
        /// Returns a boolean telling whether an enum with the given name exists in the enumeration.
        /// </summary>
        /// <param name="name">The name to check if it's defined</param>
        /// <returns><c>true</c> if a member with the name exists in the enumeration, <c>false</c> otherwise</returns>
        [g__SRCS.MethodImpl(INLINING)]
        public static bool IsNameDefined(this string name, ResultCases.EnumCase _)
            => IsNameDefined(name, default(ResultCases.EnumCase), allowMatchingMetadataAttribute: false);

        /// <summary>
        /// Returns a boolean telling whether an enum with the given name exists in the enumeration,
        /// or if a member decorated with a <c>[Display]</c> attribute
        /// with the required name exists.
        /// </summary>
        /// <param name="name">The name to check if it's defined</param>
        /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value of metadata attributes, otherwise ignores them</param>
        /// <returns><c>true</c> if a member with the name exists in the enumeration, or a member is decorated
        /// with a <c>[Display]</c> attribute with the name, <c>false</c> otherwise</returns>
        public static bool IsNameDefined(this string name, ResultCases.EnumCase _, bool allowMatchingMetadataAttribute)
        {
            return name switch
            {
                Names.Undefined => true,
                Names.Success => true,
                _ => false,
            };
        }

        public static bool TryFormat(
              this ResultCases.EnumCase value
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
              this ResultCases.EnumCase value
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
        public static int FindIndex(this ResultCases.EnumCase value)
            => value switch
            {
                ResultCases.EnumCase.Undefined => 0,
                ResultCases.EnumCase.Success => 1,
                _ => -1,
            };

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class Values
        {
            private static readonly ResultCases.EnumCase[] s_values = new ResultCases.EnumCase[]
            {
                ResultCases.EnumCase.Undefined,
                ResultCases.EnumCase.Success,
            };

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlyMemory<ResultCases.EnumCase> AsMemory() => s_values;

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlySpan<ResultCases.EnumCase> AsSpan() => s_values;

            [g__SRCS.MethodImpl(INLINING)]
            public static g__UC.NativeArray<ResultCases.EnumCase> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
                => g__UC.CollectionHelper.CreateNativeArray<ResultCases.EnumCase>(s_values, allocator);
        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class UnderlyingValues
        {
            private static readonly byte[] s_values = new byte[]
            {
                ToUnderlyingValue(ResultCases.EnumCase.Undefined),
                ToUnderlyingValue(ResultCases.EnumCase.Success),
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

    static partial class ResultCases_EnumCaseExtensions// Names
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class Names
        {
            public const string Undefined = nameof(ResultCases.EnumCase.Undefined);

            public const string Success = nameof(ResultCases.EnumCase.Success);

            private static readonly string[] s_names = new string[]
            {
                Undefined,
                Success,
            };

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

            [g__SRCS.MethodImpl(INLINING)]
            public static string Get(ResultCases.EnumCase value) => Get(value, true);

            public static string Get(ResultCases.EnumCase value, bool emptyIfUndefined)
                => value switch
                {
                    ResultCases.EnumCase.Undefined => Undefined,
                    ResultCases.EnumCase.Success => Success,
                    _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
                };
        }

    }

#region    DISPLAY NAMES
#endregion =============

    static partial class ResultCases_EnumCaseExtensions// DisplayNames
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class DisplayNames
        {
            public const string Undefined = "Undefined";

            public const string Success = "Success";

            private static readonly string[] s_names = new string[]
            {
                Undefined,
                Success,
            };

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

            [g__SRCS.MethodImpl(INLINING)]
            public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

            [g__SRCS.MethodImpl(INLINING)]
            public static string Get(ResultCases.EnumCase value) => Get(value, true);

            public static string Get(ResultCases.EnumCase value, bool emptyIfUndefined)
                => value switch
                {
                    ResultCases.EnumCase.Undefined => Undefined,
                    ResultCases.EnumCase.Success => Success,
                    _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
                };
        }

    }

#region    FIXED NAMES
#endregion ===========

    static partial class ResultCases_EnumCaseExtensions// FixedNames
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

            public static g__UC.FixedString32Bytes Success
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => (g__UC.FixedString32Bytes)Names.Success;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
                => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

            public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
                where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
            {
                var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(ResultCases_EnumCaseExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
                names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Undefined);
                names[1] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Success);
                return names;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static g__UC.FixedString32Bytes Get(ResultCases.EnumCase value)
                => Get(value, true);

            public static g__UC.FixedString32Bytes Get(ResultCases.EnumCase value, bool emptyIfUndefined)
                => value switch
                {
                    ResultCases.EnumCase.Undefined => Undefined,
                    ResultCases.EnumCase.Success => Success,
                    _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
                };
        }

    }

#region    FIXED DISPLAY NAMES
#endregion ===================

    static partial class ResultCases_EnumCaseExtensions// FixedDisplayNames
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

            public static g__UC.FixedString32Bytes Success
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => (g__UC.FixedString32Bytes)DisplayNames.Success;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
                => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

            public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
                where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
            {
                var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(ResultCases_EnumCaseExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
                names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Undefined);
                names[1] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Success);
                return names;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static g__UC.FixedString32Bytes Get(ResultCases.EnumCase value)
                => Get(value, true);

            public static g__UC.FixedString32Bytes Get(ResultCases.EnumCase value, bool emptyIfUndefined)
                => value switch
                {
                    ResultCases.EnumCase.Undefined => Undefined,
                    ResultCases.EnumCase.Success => Success,
                    _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
                };
        }

    }

}
