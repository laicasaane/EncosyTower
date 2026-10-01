#pragma warning disable 0219

using System;
using EncosyTower.EnumExtensions;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SC = global::System.Collections;
using g__SCG = global::System.Collections.Generic;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SRIS = global::System.Runtime.InteropServices;
using g__ETCol = global::EncosyTower.Collections;
using g__ETCon = global::EncosyTower.Conversion;
using g__ETEE = global::EncosyTower.EnumExtensions;
using g__ETEESG = global::EncosyTower.EnumExtensions.SourceGen;
using g__UC = global::Unity.Collections;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{


#region    INTERFACE
#endregion =========

static partial class DayOfWeekExtensions { } // IDayOfWeekExtensions

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::System.DayOfWeek), typeof(IDayOfWeekExtensions), typeof(DayOfWeekExtensions), typeof(DayOfWeekExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.8-preview.1")]
public partial interface IDayOfWeekExtensions
    : g__ETEE.IEnumExtensions<DayOfWeekExtended, global::System.DayOfWeek, int>
    , g__ETCon.IToFixedString
    , g__ETCon.IToDisplayFixedString
    , g__ETCon.IToFixedString<g__UC.FixedString32Bytes>
    , g__ETCon.IToDisplayFixedString<g__UC.FixedString32Bytes>
{
}

#region    EXTENDED STRUCT
#endregion ===============

static partial class DayOfWeekExtensions { } // DayOfWeekExtended

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::System.DayOfWeek), typeof(IDayOfWeekExtensions), typeof(DayOfWeekExtensions), typeof(DayOfWeekExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.8-preview.1")]
[g__SDCA.ExcludeFromCodeCoverage]
[g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
public readonly partial struct DayOfWeekExtended : IDayOfWeekExtensions
    , g__S.IEquatable<DayOfWeekExtended>
    , g__S.IComparable<DayOfWeekExtended>
{
    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    private readonly global::System.DayOfWeek _value;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    private readonly int _underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public DayOfWeekExtended(global::System.DayOfWeek value) : this()
    {
        _value = value;
    }

    public global::System.DayOfWeek Value
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    public int UnderlyingValue
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => _underlyingValue;
    }

    public int Length
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => DayOfWeekExtensions.Length;
    }

    public bool IsDefined
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => DayOfWeekExtensions.IsDefined(_value);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public DayOfWeekExtended Create(global::System.DayOfWeek value) => new DayOfWeekExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public DayOfWeekExtended CreateFromUnderlyingValue(int value) => new DayOfWeekExtended((global::System.DayOfWeek)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToStringFast() => ToStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToStringFast(bool emptyIfUndefined) => DayOfWeekExtensions.ToStringFast(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayString() => ToDisplayString(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayString(bool emptyIfUndefined) => DayOfWeekExtensions.ToDisplayStringFast(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayStringFast() => ToDisplayStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayStringFast(bool emptyIfUndefined) => DayOfWeekExtensions.ToDisplayStringFast(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string name, out DayOfWeekExtended value) => TryParse(name, out value, false, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string name, out DayOfWeekExtended value, bool ignoreCase) => TryParse(name, out value, ignoreCase, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string name, out DayOfWeekExtended value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        var result = DayOfWeekExtensions.TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);
        value = new DayOfWeekExtended(enumValue);
        return result;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(g__S.ReadOnlySpan<char> name, out DayOfWeekExtended value) => TryParse(name, out value, false, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(g__S.ReadOnlySpan<char> name, out DayOfWeekExtended value, bool ignoreCase) => TryParse(name, out value, ignoreCase, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(g__S.ReadOnlySpan<char> name, out DayOfWeekExtended value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        var result = DayOfWeekExtensions.TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);
        value = new DayOfWeekExtended(enumValue);
        return result;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToFixedString() => ToFixedString(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToFixedString(bool emptyIfUndefined) => DayOfWeekExtensions.ToFixedString(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToDisplayFixedString() => ToDisplayFixedString(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToDisplayFixedString(bool emptyIfUndefined) => DayOfWeekExtensions.ToDisplayFixedString(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public TFixedString ToFixedString<TFixedString>()
        where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToFixedString());

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public TFixedString ToDisplayFixedString<TFixedString>()
        where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToDisplayFixedString());

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(
          g__S.Span<char> destination
        , out int charsWritten
    ) => DayOfWeekExtensions.TryFormat(_value, destination, out charsWritten);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(
          g__S.Span<char> destination
        , out int charsWritten
        , g__S.ReadOnlySpan<char> format
        , g__S.IFormatProvider provider = null
    ) => DayOfWeekExtensions.TryFormat(_value, destination, out charsWritten, format, provider);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool IsNameDefined(string name) => DayOfWeekExtensions.IsNameDefined(name, default(global::System.DayOfWeek));

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool IsNameDefined(string name, bool allowMatchingMetadataAttribute) => DayOfWeekExtensions.IsNameDefined(name, default(global::System.DayOfWeek), allowMatchingMetadataAttribute);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public int ToIndex() => DayOfWeekExtensions.FindIndex(_value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public override string ToString() => ToStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToString(string format, g__S.IFormatProvider formatProvider) => ToStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => _underlyingValue.GetHashCode();

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public int CompareTo(DayOfWeekExtended other) => this._underlyingValue.CompareTo(other._underlyingValue);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool Equals(DayOfWeekExtended other) => this._underlyingValue == other._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj) => obj is DayOfWeekExtended other && this._underlyingValue == other._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static implicit operator DayOfWeekExtended(global::System.DayOfWeek value) => new DayOfWeekExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(DayOfWeekExtended left, DayOfWeekExtended right) => left._underlyingValue == right._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(DayOfWeekExtended left, DayOfWeekExtended right) => left._underlyingValue != right._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator <(DayOfWeekExtended left, DayOfWeekExtended right) => left._underlyingValue < right._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator >(DayOfWeekExtended left, DayOfWeekExtended right) => left._underlyingValue > right._underlyingValue;

}

#region    EXTENSIONS
#endregion ==========

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::System.DayOfWeek), typeof(IDayOfWeekExtensions), typeof(DayOfWeekExtensions), typeof(DayOfWeekExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.8-preview.1")]
[g__SDCA.ExcludeFromCodeCoverage]
public static partial class DayOfWeekExtensions // DayOfWeekExtensions
{
    /// <summary>
    /// The number of members in the enum.
    /// This is a non-distinct count of defined names.
    /// </summary>
    public const int Length = 7;

    /// <summary>
    /// Returns the string representation of the <see cref="global::System.DayOfWeek"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToStringFast(this global::System.DayOfWeek value)
        => ToStringFast(value, true);

    /// <summary>
    /// Returns the string representation of the <see cref="global::System.DayOfWeek"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToStringFast(this global::System.DayOfWeek value, bool emptyIfUndefined)
        => Names.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the string representation of the <see cref="global::System.DayOfWeek"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToDisplayStringFast(this global::System.DayOfWeek value)
        => ToDisplayStringFast(value, true);

    /// <summary>
    /// Returns the string representation of the <see cref="global::System.DayOfWeek"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToDisplayStringFast(this global::System.DayOfWeek value, bool emptyIfUndefined)
        => DisplayNames.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::System.DayOfWeek"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToFixedString(this global::System.DayOfWeek value)
        => ToFixedString(value, true);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::System.DayOfWeek"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToFixedString(this global::System.DayOfWeek value, bool emptyIfUndefined)
        => FixedNames.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::System.DayOfWeek"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToDisplayFixedString(this global::System.DayOfWeek value)
        => ToDisplayFixedString(value, true);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::System.DayOfWeek"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToDisplayFixedString(this global::System.DayOfWeek value, bool emptyIfUndefined)
        => FixedDisplayNames.Get(value, emptyIfUndefined);

    private static g__UC.FixedString32Bytes ToFixedString(int value)
    {
        var fs = new g__UC.FixedString32Bytes();
        g__UC.FixedStringMethods.Append(ref fs, value);
        return fs;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static DayOfWeekExtended AsExtended(this global::System.DayOfWeek value)
        => new DayOfWeekExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static DayOfWeekExtended AsDayOfWeekExtended(this byte value)
        => new DayOfWeekExtended((global::System.DayOfWeek)(int)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static DayOfWeekExtended AsDayOfWeekExtended(this sbyte value)
        => new DayOfWeekExtended((global::System.DayOfWeek)(int)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static DayOfWeekExtended AsDayOfWeekExtended(this short value)
        => new DayOfWeekExtended((global::System.DayOfWeek)(int)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static DayOfWeekExtended AsDayOfWeekExtended(this ushort value)
        => new DayOfWeekExtended((global::System.DayOfWeek)(int)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static DayOfWeekExtended AsDayOfWeekExtended(this int value)
        => new DayOfWeekExtended((global::System.DayOfWeek)(int)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static DayOfWeekExtended AsDayOfWeekExtended(this uint value)
        => new DayOfWeekExtended((global::System.DayOfWeek)(int)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static DayOfWeekExtended AsDayOfWeekExtended(this long value)
        => new DayOfWeekExtended((global::System.DayOfWeek)(int)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static DayOfWeekExtended AsDayOfWeekExtended(this ulong value)
        => new DayOfWeekExtended((global::System.DayOfWeek)(int)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static int ToUnderlyingValue(this global::System.DayOfWeek value)
        => (int)value;

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::System.DayOfWeek" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::System.DayOfWeek" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::System.DayOfWeek" />. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this string name, out global::System.DayOfWeek value)
        => TryParse(name, out value, false, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::System.DayOfWeek" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::System.DayOfWeek" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::System.DayOfWeek" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this string name, out global::System.DayOfWeek value, bool ignoreCase)
        => TryParse(name, out value, ignoreCase, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::System.DayOfWeek" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::System.DayOfWeek" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::System.DayOfWeek" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
    /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(this string name, out global::System.DayOfWeek value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            value = default;
            return false;
        }

        var stringComparison = ignoreCase ? g__S.StringComparison.OrdinalIgnoreCase : g__S.StringComparison.Ordinal;

        switch (name)
        {
            case string s when s.Equals(Names.Sunday, stringComparison):
            {
                value = global::System.DayOfWeek.Sunday;
                return true;
            }

            case string s when s.Equals(Names.Monday, stringComparison):
            {
                value = global::System.DayOfWeek.Monday;
                return true;
            }

            case string s when s.Equals(Names.Tuesday, stringComparison):
            {
                value = global::System.DayOfWeek.Tuesday;
                return true;
            }

            case string s when s.Equals(Names.Wednesday, stringComparison):
            {
                value = global::System.DayOfWeek.Wednesday;
                return true;
            }

            case string s when s.Equals(Names.Thursday, stringComparison):
            {
                value = global::System.DayOfWeek.Thursday;
                return true;
            }

            case string s when s.Equals(Names.Friday, stringComparison):
            {
                value = global::System.DayOfWeek.Friday;
                return true;
            }

            case string s when s.Equals(Names.Saturday, stringComparison):
            {
                value = global::System.DayOfWeek.Saturday;
                return true;
            }

            case string s when int.TryParse(name, out var underlyingValue):
            {
                value = (global::System.DayOfWeek)underlyingValue;
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
    /// an <see cref="global::System.DayOfWeek" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::System.DayOfWeek" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::System.DayOfWeek" />. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this g__S.ReadOnlySpan<char> name, out global::System.DayOfWeek value)
        => TryParse(name, out value, false, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::System.DayOfWeek" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::System.DayOfWeek" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::System.DayOfWeek" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this g__S.ReadOnlySpan<char> name, out global::System.DayOfWeek value, bool ignoreCase)
        => TryParse(name, out value, ignoreCase, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::System.DayOfWeek" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::System.DayOfWeek" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::System.DayOfWeek" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
    /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(this g__S.ReadOnlySpan<char> name, out global::System.DayOfWeek value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        if (name.IsEmpty)
        {
            value = default;
            return false;
        }

        var stringComparison = ignoreCase ? g__S.StringComparison.OrdinalIgnoreCase : g__S.StringComparison.Ordinal;

        switch (name)
        {
            case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.Sunday), stringComparison):
            {
                value = global::System.DayOfWeek.Sunday;
                return true;
            }

            case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.Monday), stringComparison):
            {
                value = global::System.DayOfWeek.Monday;
                return true;
            }

            case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.Tuesday), stringComparison):
            {
                value = global::System.DayOfWeek.Tuesday;
                return true;
            }

            case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.Wednesday), stringComparison):
            {
                value = global::System.DayOfWeek.Wednesday;
                return true;
            }

            case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.Thursday), stringComparison):
            {
                value = global::System.DayOfWeek.Thursday;
                return true;
            }

            case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.Friday), stringComparison):
            {
                value = global::System.DayOfWeek.Friday;
                return true;
            }

            case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.Saturday), stringComparison):
            {
                value = global::System.DayOfWeek.Saturday;
                return true;
            }

            case g__S.ReadOnlySpan<char> s when int.TryParse(name, out var underlyingValue):
            {
                value = (global::System.DayOfWeek)underlyingValue;
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
    public static bool IsDefined(this global::System.DayOfWeek value)
        => value switch
        {
            global::System.DayOfWeek.Sunday => true,
            global::System.DayOfWeek.Monday => true,
            global::System.DayOfWeek.Tuesday => true,
            global::System.DayOfWeek.Wednesday => true,
            global::System.DayOfWeek.Thursday => true,
            global::System.DayOfWeek.Friday => true,
            global::System.DayOfWeek.Saturday => true,
            _ => false,
        };

    /// <summary>
    /// Returns a boolean telling whether an enum with the given name exists in the enumeration.
    /// </summary>
    /// <param name="name">The name to check if it's defined</param>
    /// <returns><c>true</c> if a member with the name exists in the enumeration, <c>false</c> otherwise</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool IsNameDefined(this string name, global::System.DayOfWeek _)
        => IsNameDefined(name, default(global::System.DayOfWeek), allowMatchingMetadataAttribute: false);

    /// <summary>
    /// Returns a boolean telling whether an enum with the given name exists in the enumeration,
    /// or if a member decorated with a <c>[Display]</c> attribute
    /// with the required name exists.
    /// </summary>
    /// <param name="name">The name to check if it's defined</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value of metadata attributes, otherwise ignores them</param>
    /// <returns><c>true</c> if a member with the name exists in the enumeration, or a member is decorated
    /// with a <c>[Display]</c> attribute with the name, <c>false</c> otherwise</returns>
    public static bool IsNameDefined(this string name, global::System.DayOfWeek _, bool allowMatchingMetadataAttribute)
    {
        return name switch
        {
            Names.Sunday => true,
            Names.Monday => true,
            Names.Tuesday => true,
            Names.Wednesday => true,
            Names.Thursday => true,
            Names.Friday => true,
            Names.Saturday => true,
            _ => false,
        };
    }

    public static bool TryFormat(
          this global::System.DayOfWeek value
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
          this global::System.DayOfWeek value
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
    public static int FindIndex(this global::System.DayOfWeek value)
        => value switch
        {
            global::System.DayOfWeek.Sunday => 0,
            global::System.DayOfWeek.Monday => 1,
            global::System.DayOfWeek.Tuesday => 2,
            global::System.DayOfWeek.Wednesday => 3,
            global::System.DayOfWeek.Thursday => 4,
            global::System.DayOfWeek.Friday => 5,
            global::System.DayOfWeek.Saturday => 6,
            _ => -1,
        };

    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class Values
    {
        private static readonly global::System.DayOfWeek[] s_values = new global::System.DayOfWeek[]
        {
            global::System.DayOfWeek.Sunday,
            global::System.DayOfWeek.Monday,
            global::System.DayOfWeek.Tuesday,
            global::System.DayOfWeek.Wednesday,
            global::System.DayOfWeek.Thursday,
            global::System.DayOfWeek.Friday,
            global::System.DayOfWeek.Saturday,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<global::System.DayOfWeek> AsMemory() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<global::System.DayOfWeek> AsSpan() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<global::System.DayOfWeek> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => g__UC.CollectionHelper.CreateNativeArray<global::System.DayOfWeek>(s_values, allocator);
    }

    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class UnderlyingValues
    {
        private static readonly int[] s_values = new int[]
        {
            ToUnderlyingValue(global::System.DayOfWeek.Sunday),
            ToUnderlyingValue(global::System.DayOfWeek.Monday),
            ToUnderlyingValue(global::System.DayOfWeek.Tuesday),
            ToUnderlyingValue(global::System.DayOfWeek.Wednesday),
            ToUnderlyingValue(global::System.DayOfWeek.Thursday),
            ToUnderlyingValue(global::System.DayOfWeek.Friday),
            ToUnderlyingValue(global::System.DayOfWeek.Saturday),
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<int> AsMemory() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<int> AsSpan() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<int> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => g__UC.CollectionHelper.CreateNativeArray<int>(s_values, allocator);
    }

}

#region    NAMES
#endregion =====

static partial class DayOfWeekExtensions// Names
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class Names
    {
        public const string Sunday = nameof(global::System.DayOfWeek.Sunday);

        public const string Monday = nameof(global::System.DayOfWeek.Monday);

        public const string Tuesday = nameof(global::System.DayOfWeek.Tuesday);

        public const string Wednesday = nameof(global::System.DayOfWeek.Wednesday);

        public const string Thursday = nameof(global::System.DayOfWeek.Thursday);

        public const string Friday = nameof(global::System.DayOfWeek.Friday);

        public const string Saturday = nameof(global::System.DayOfWeek.Saturday);

        private static readonly string[] s_names = new string[]
        {
            Sunday,
            Monday,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Get(global::System.DayOfWeek value) => Get(value, true);

        public static string Get(global::System.DayOfWeek value, bool emptyIfUndefined)
            => value switch
            {
                global::System.DayOfWeek.Sunday => Sunday,
                global::System.DayOfWeek.Monday => Monday,
                global::System.DayOfWeek.Tuesday => Tuesday,
                global::System.DayOfWeek.Wednesday => Wednesday,
                global::System.DayOfWeek.Thursday => Thursday,
                global::System.DayOfWeek.Friday => Friday,
                global::System.DayOfWeek.Saturday => Saturday,
                _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
            };
    }

}

#region    DISPLAY NAMES
#endregion =============

static partial class DayOfWeekExtensions// DisplayNames
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class DisplayNames
    {
        public const string Sunday = Names.Sunday;

        public const string Monday = Names.Monday;

        public const string Tuesday = Names.Tuesday;

        public const string Wednesday = Names.Wednesday;

        public const string Thursday = Names.Thursday;

        public const string Friday = Names.Friday;

        public const string Saturday = Names.Saturday;

        private static readonly string[] s_names = new string[]
        {
            Sunday,
            Monday,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Get(global::System.DayOfWeek value) => Get(value, true);

        public static string Get(global::System.DayOfWeek value, bool emptyIfUndefined)
            => value switch
            {
                global::System.DayOfWeek.Sunday => Sunday,
                global::System.DayOfWeek.Monday => Monday,
                global::System.DayOfWeek.Tuesday => Tuesday,
                global::System.DayOfWeek.Wednesday => Wednesday,
                global::System.DayOfWeek.Thursday => Thursday,
                global::System.DayOfWeek.Friday => Friday,
                global::System.DayOfWeek.Saturday => Saturday,
                _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
            };
    }

}

#region    FIXED NAMES
#endregion ===========

static partial class DayOfWeekExtensions// FixedNames
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class FixedNames
    {
        public static g__UC.FixedString32Bytes Sunday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)Names.Sunday;
        }

        public static g__UC.FixedString32Bytes Monday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)Names.Monday;
        }

        public static g__UC.FixedString32Bytes Tuesday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)Names.Tuesday;
        }

        public static g__UC.FixedString32Bytes Wednesday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)Names.Wednesday;
        }

        public static g__UC.FixedString32Bytes Thursday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)Names.Thursday;
        }

        public static g__UC.FixedString32Bytes Friday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)Names.Friday;
        }

        public static g__UC.FixedString32Bytes Saturday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)Names.Saturday;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

        public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        {
            var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(DayOfWeekExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
            names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Sunday);
            names[1] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Monday);
            names[2] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Tuesday);
            names[3] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Wednesday);
            names[4] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Thursday);
            names[5] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Friday);
            names[6] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Saturday);
            return names;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes Get(global::System.DayOfWeek value)
            => Get(value, true);

        public static g__UC.FixedString32Bytes Get(global::System.DayOfWeek value, bool emptyIfUndefined)
            => value switch
            {
                global::System.DayOfWeek.Sunday => Sunday,
                global::System.DayOfWeek.Monday => Monday,
                global::System.DayOfWeek.Tuesday => Tuesday,
                global::System.DayOfWeek.Wednesday => Wednesday,
                global::System.DayOfWeek.Thursday => Thursday,
                global::System.DayOfWeek.Friday => Friday,
                global::System.DayOfWeek.Saturday => Saturday,
                _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
            };
    }

}

#region    FIXED DISPLAY NAMES
#endregion ===================

static partial class DayOfWeekExtensions// FixedDisplayNames
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class FixedDisplayNames
    {
        public static g__UC.FixedString32Bytes Sunday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)DisplayNames.Sunday;
        }

        public static g__UC.FixedString32Bytes Monday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)DisplayNames.Monday;
        }

        public static g__UC.FixedString32Bytes Tuesday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)DisplayNames.Tuesday;
        }

        public static g__UC.FixedString32Bytes Wednesday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)DisplayNames.Wednesday;
        }

        public static g__UC.FixedString32Bytes Thursday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)DisplayNames.Thursday;
        }

        public static g__UC.FixedString32Bytes Friday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)DisplayNames.Friday;
        }

        public static g__UC.FixedString32Bytes Saturday
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)DisplayNames.Saturday;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

        public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        {
            var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(DayOfWeekExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
            names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Sunday);
            names[1] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Monday);
            names[2] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Tuesday);
            names[3] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Wednesday);
            names[4] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Thursday);
            names[5] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Friday);
            names[6] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Saturday);
            return names;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes Get(global::System.DayOfWeek value)
            => Get(value, true);

        public static g__UC.FixedString32Bytes Get(global::System.DayOfWeek value, bool emptyIfUndefined)
            => value switch
            {
                global::System.DayOfWeek.Sunday => Sunday,
                global::System.DayOfWeek.Monday => Monday,
                global::System.DayOfWeek.Tuesday => Tuesday,
                global::System.DayOfWeek.Wednesday => Wednesday,
                global::System.DayOfWeek.Thursday => Thursday,
                global::System.DayOfWeek.Friday => Friday,
                global::System.DayOfWeek.Saturday => Saturday,
                _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
            };
    }

}



}

