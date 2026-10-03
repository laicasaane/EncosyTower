#pragma warning disable 0219

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



#pragma warning disable

[g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator", "0.1.8-preview.1")]
partial struct Value_EnumTemplate : g__ETEE.IEnumTemplate<Value> { } // Value

[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator", "0.1.8-preview.1")]
[g__ETEESG.GeneratedFromEnumTemplate(typeof(global::TestProject.Value_EnumTemplate))]
public enum Value : byte
{
    /// <seealso cref="global::TestProject.Values"/>
    First = 0,
}

#region    EXTENSIONS
#endregion ==========

partial struct Value_EnumTemplate { }  // ValueExtensions

#region    INTERFACE
#endregion =========

static partial class ValueExtensions { } // IValueExtensions

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::TestProject.Value), typeof(IValueExtensions), typeof(ValueExtensions), typeof(ValueExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator", "0.1.8-preview.1")]
public partial interface IValueExtensions
    : g__ETEE.IEnumExtensions<ValueExtended, global::TestProject.Value, byte>
    , g__ETCon.IToFixedString
    , g__ETCon.IToDisplayFixedString
    , g__ETCon.IToFixedString<g__UC.FixedString32Bytes>
    , g__ETCon.IToDisplayFixedString<g__UC.FixedString32Bytes>
{
}

#region    EXTENDED STRUCT
#endregion ===============

static partial class ValueExtensions { } // ValueExtended

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::TestProject.Value), typeof(IValueExtensions), typeof(ValueExtensions), typeof(ValueExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator", "0.1.8-preview.1")]
[g__SDCA.ExcludeFromCodeCoverage]
[g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
public readonly partial struct ValueExtended : IValueExtensions
    , g__S.IEquatable<ValueExtended>
    , g__S.IComparable<ValueExtended>
{
    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    private readonly global::TestProject.Value _value;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    private readonly byte _underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public ValueExtended(global::TestProject.Value value) : this()
    {
        _value = value;
    }

    public global::TestProject.Value Value
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    public byte UnderlyingValue
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => _underlyingValue;
    }

    public int Length
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => ValueExtensions.Length;
    }

    public bool IsDefined
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => ValueExtensions.IsDefined(_value);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public ValueExtended Create(global::TestProject.Value value) => new ValueExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public ValueExtended CreateFromUnderlyingValue(byte value) => new ValueExtended((global::TestProject.Value)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToStringFast() => ToStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToStringFast(bool emptyIfUndefined) => ValueExtensions.ToStringFast(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayString() => ToDisplayString(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayString(bool emptyIfUndefined) => ValueExtensions.ToDisplayStringFast(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayStringFast() => ToDisplayStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayStringFast(bool emptyIfUndefined) => ValueExtensions.ToDisplayStringFast(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string name, out ValueExtended value) => TryParse(name, out value, false, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string name, out ValueExtended value, bool ignoreCase) => TryParse(name, out value, ignoreCase, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string name, out ValueExtended value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        var result = ValueExtensions.TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);
        value = new ValueExtended(enumValue);
        return result;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(g__S.ReadOnlySpan<char> name, out ValueExtended value) => TryParse(name, out value, false, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(g__S.ReadOnlySpan<char> name, out ValueExtended value, bool ignoreCase) => TryParse(name, out value, ignoreCase, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(g__S.ReadOnlySpan<char> name, out ValueExtended value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        var result = ValueExtensions.TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);
        value = new ValueExtended(enumValue);
        return result;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToFixedString() => ToFixedString(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToFixedString(bool emptyIfUndefined) => ValueExtensions.ToFixedString(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToDisplayFixedString() => ToDisplayFixedString(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToDisplayFixedString(bool emptyIfUndefined) => ValueExtensions.ToDisplayFixedString(_value, emptyIfUndefined);

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
    ) => ValueExtensions.TryFormat(_value, destination, out charsWritten);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(
          g__S.Span<char> destination
        , out int charsWritten
        , g__S.ReadOnlySpan<char> format
        , g__S.IFormatProvider provider = null
    ) => ValueExtensions.TryFormat(_value, destination, out charsWritten, format, provider);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool IsNameDefined(string name) => ValueExtensions.IsNameDefined(name, default(global::TestProject.Value));

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool IsNameDefined(string name, bool allowMatchingMetadataAttribute) => ValueExtensions.IsNameDefined(name, default(global::TestProject.Value), allowMatchingMetadataAttribute);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public int ToIndex() => ValueExtensions.FindIndex(_value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public override string ToString() => ToStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToString(string format, g__S.IFormatProvider formatProvider) => ToStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => _underlyingValue.GetHashCode();

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public int CompareTo(ValueExtended other) => this._underlyingValue.CompareTo(other._underlyingValue);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool Equals(ValueExtended other) => this._underlyingValue == other._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj) => obj is ValueExtended other && this._underlyingValue == other._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static implicit operator ValueExtended(global::TestProject.Value value) => new ValueExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ValueExtended left, ValueExtended right) => left._underlyingValue == right._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ValueExtended left, ValueExtended right) => left._underlyingValue != right._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator <(ValueExtended left, ValueExtended right) => left._underlyingValue < right._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator >(ValueExtended left, ValueExtended right) => left._underlyingValue > right._underlyingValue;

}

#region    EXTENSIONS
#endregion ==========

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::TestProject.Value), typeof(IValueExtensions), typeof(ValueExtensions), typeof(ValueExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator", "0.1.8-preview.1")]
[g__SDCA.ExcludeFromCodeCoverage]
public static partial class ValueExtensions // ValueExtensions
{
    /// <summary>
    /// The number of members in the enum.
    /// This is a non-distinct count of defined names.
    /// </summary>
    public const int Length = 1;

    /// <summary>
    /// Returns the string representation of the <see cref="global::TestProject.Value"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToStringFast(this global::TestProject.Value value)
        => ToStringFast(value, true);

    /// <summary>
    /// Returns the string representation of the <see cref="global::TestProject.Value"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToStringFast(this global::TestProject.Value value, bool emptyIfUndefined)
        => Names.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the string representation of the <see cref="global::TestProject.Value"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToDisplayStringFast(this global::TestProject.Value value)
        => ToDisplayStringFast(value, true);

    /// <summary>
    /// Returns the string representation of the <see cref="global::TestProject.Value"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToDisplayStringFast(this global::TestProject.Value value, bool emptyIfUndefined)
        => DisplayNames.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::TestProject.Value"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToFixedString(this global::TestProject.Value value)
        => ToFixedString(value, true);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::TestProject.Value"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToFixedString(this global::TestProject.Value value, bool emptyIfUndefined)
        => FixedNames.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::TestProject.Value"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToDisplayFixedString(this global::TestProject.Value value)
        => ToDisplayFixedString(value, true);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::TestProject.Value"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToDisplayFixedString(this global::TestProject.Value value, bool emptyIfUndefined)
        => FixedDisplayNames.Get(value, emptyIfUndefined);

    private static g__UC.FixedString32Bytes ToFixedString(byte value)
    {
        var fs = new g__UC.FixedString32Bytes();
        g__UC.FixedStringMethods.Append(ref fs, value);
        return fs;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static ValueExtended AsExtended(this global::TestProject.Value value)
        => new ValueExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static ValueExtended AsValueExtended(this byte value)
        => new ValueExtended((global::TestProject.Value)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static ValueExtended AsValueExtended(this sbyte value)
        => new ValueExtended((global::TestProject.Value)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static ValueExtended AsValueExtended(this short value)
        => new ValueExtended((global::TestProject.Value)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static ValueExtended AsValueExtended(this ushort value)
        => new ValueExtended((global::TestProject.Value)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static ValueExtended AsValueExtended(this int value)
        => new ValueExtended((global::TestProject.Value)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static ValueExtended AsValueExtended(this uint value)
        => new ValueExtended((global::TestProject.Value)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static ValueExtended AsValueExtended(this long value)
        => new ValueExtended((global::TestProject.Value)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static ValueExtended AsValueExtended(this ulong value)
        => new ValueExtended((global::TestProject.Value)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static byte ToUnderlyingValue(this global::TestProject.Value value)
        => (byte)value;

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Value" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Value" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Value" />. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this string name, out global::TestProject.Value value)
        => TryParse(name, out value, false, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Value" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Value" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Value" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this string name, out global::TestProject.Value value, bool ignoreCase)
        => TryParse(name, out value, ignoreCase, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Value" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Value" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Value" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
    /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(this string name, out global::TestProject.Value value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            value = default;
            return false;
        }

        var stringComparison = ignoreCase ? g__S.StringComparison.OrdinalIgnoreCase : g__S.StringComparison.Ordinal;

        switch (name)
        {
            case string s when s.Equals(Names.First, stringComparison):
            {
                value = global::TestProject.Value.First;
                return true;
            }

            case string s when byte.TryParse(name, out var underlyingValue):
            {
                value = (global::TestProject.Value)underlyingValue;
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
    /// an <see cref="global::TestProject.Value" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Value" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Value" />. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this g__S.ReadOnlySpan<char> name, out global::TestProject.Value value)
        => TryParse(name, out value, false, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Value" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Value" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Value" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this g__S.ReadOnlySpan<char> name, out global::TestProject.Value value, bool ignoreCase)
        => TryParse(name, out value, ignoreCase, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Value" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Value" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Value" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
    /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(this g__S.ReadOnlySpan<char> name, out global::TestProject.Value value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        if (name.IsEmpty)
        {
            value = default;
            return false;
        }

        var stringComparison = ignoreCase ? g__S.StringComparison.OrdinalIgnoreCase : g__S.StringComparison.Ordinal;

        switch (name)
        {
            case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.First), stringComparison):
            {
                value = global::TestProject.Value.First;
                return true;
            }

            case g__S.ReadOnlySpan<char> s when byte.TryParse(name, out var underlyingValue):
            {
                value = (global::TestProject.Value)underlyingValue;
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
    public static bool IsDefined(this global::TestProject.Value value)
        => value switch
        {
            global::TestProject.Value.First => true,
            _ => false,
        };

    /// <summary>
    /// Returns a boolean telling whether an enum with the given name exists in the enumeration.
    /// </summary>
    /// <param name="name">The name to check if it's defined</param>
    /// <returns><c>true</c> if a member with the name exists in the enumeration, <c>false</c> otherwise</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool IsNameDefined(this string name, global::TestProject.Value _)
        => IsNameDefined(name, default(global::TestProject.Value), allowMatchingMetadataAttribute: false);

    /// <summary>
    /// Returns a boolean telling whether an enum with the given name exists in the enumeration,
    /// or if a member decorated with a <c>[Display]</c> attribute
    /// with the required name exists.
    /// </summary>
    /// <param name="name">The name to check if it's defined</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value of metadata attributes, otherwise ignores them</param>
    /// <returns><c>true</c> if a member with the name exists in the enumeration, or a member is decorated
    /// with a <c>[Display]</c> attribute with the name, <c>false</c> otherwise</returns>
    public static bool IsNameDefined(this string name, global::TestProject.Value _, bool allowMatchingMetadataAttribute)
    {
        return name switch
        {
            Names.First => true,
            _ => false,
        };
    }

    public static bool TryFormat(
          this global::TestProject.Value value
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
          this global::TestProject.Value value
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
    public static int FindIndex(this global::TestProject.Value value)
        => value switch
        {
            global::TestProject.Value.First => 0,
            _ => -1,
        };

    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class Values
    {
        private static readonly global::TestProject.Value[] s_values = new global::TestProject.Value[]
        {
            global::TestProject.Value.First,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<global::TestProject.Value> AsMemory() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<global::TestProject.Value> AsSpan() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<global::TestProject.Value> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => g__UC.CollectionHelper.CreateNativeArray<global::TestProject.Value>(s_values, allocator);
    }

    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class UnderlyingValues
    {
        private static readonly byte[] s_values = new byte[]
        {
            ToUnderlyingValue(global::TestProject.Value.First),
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<byte> AsMemory() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<byte> AsSpan() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<byte> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => g__UC.CollectionHelper.CreateNativeArray<byte>(s_values, allocator);
    }

}

#region    NAMES
#endregion =====

static partial class ValueExtensions// Names
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class Names
    {
        public const string First = nameof(global::TestProject.Value.First);

        private static readonly string[] s_names = new string[]
        {
            First,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Get(global::TestProject.Value value) => Get(value, true);

        public static string Get(global::TestProject.Value value, bool emptyIfUndefined)
            => value switch
            {
                global::TestProject.Value.First => First,
                _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
            };
    }

}

#region    DISPLAY NAMES
#endregion =============

static partial class ValueExtensions// DisplayNames
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class DisplayNames
    {
        public const string First = Names.First;

        private static readonly string[] s_names = new string[]
        {
            First,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Get(global::TestProject.Value value) => Get(value, true);

        public static string Get(global::TestProject.Value value, bool emptyIfUndefined)
            => value switch
            {
                global::TestProject.Value.First => First,
                _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
            };
    }

}

#region    FIXED NAMES
#endregion ===========

static partial class ValueExtensions// FixedNames
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class FixedNames
    {
        public static g__UC.FixedString32Bytes First
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)Names.First;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

        public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        {
            var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(ValueExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
            names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(First);
            return names;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes Get(global::TestProject.Value value)
            => Get(value, true);

        public static g__UC.FixedString32Bytes Get(global::TestProject.Value value, bool emptyIfUndefined)
            => value switch
            {
                global::TestProject.Value.First => First,
                _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
            };
    }

}

#region    FIXED DISPLAY NAMES
#endregion ===================

static partial class ValueExtensions// FixedDisplayNames
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class FixedDisplayNames
    {
        public static g__UC.FixedString32Bytes First
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)DisplayNames.First;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

        public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        {
            var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(ValueExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
            names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(First);
            return names;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes Get(global::TestProject.Value value)
            => Get(value, true);

        public static g__UC.FixedString32Bytes Get(global::TestProject.Value value, bool emptyIfUndefined)
            => value switch
            {
                global::TestProject.Value.First => First,
                _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
            };
    }

}

#region    EXTENDED STRUCT - ADDITIONAL API
#endregion ================================

partial struct Value_EnumTemplate { } // ValueExtended - Additional API

partial struct ValueExtended
    : g__S.IEquatable<global::TestProject.Values>
{
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(global::TestProject.Values other) => ValueExtensions.Equals(Value, other);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ValueExtended left, global::TestProject.Values right) => ValueExtensions.Equals(left.Value, right);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(global::TestProject.Values left, ValueExtended right) => ValueExtensions.Equals(right.Value, left);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ValueExtended left, global::TestProject.Values right) => !ValueExtensions.Equals(left.Value, right);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(global::TestProject.Values left, ValueExtended right) => !ValueExtensions.Equals(right.Value, left);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly void Convert(out global::TestProject.Values result) => ValueExtensions.Convert(Value, out result);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly bool TryConvert(out global::TestProject.Values result) => ValueExtensions.TryConvert(Value, out result);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly bool TryConvert(out ulong order, out ulong value) => ValueExtensions.TryConvert(Value, out order, out value);

}

#region    EXTENSIONS - ADDITIONAL API
#endregion ===========================

partial struct Value_EnumTemplate { }  // ValueExtensions - Additional API

static partial class ValueExtensions // ValueExtensions - Additional API
{
    public static bool Equals(this Value self, global::TestProject.Values other)
    {
        var valueSelf = (byte)self;
        var valueOther = (byte)other;
        return (valueSelf - 0) == valueOther;
    }

    public static Value ToValue(this global::TestProject.Values self)
    {
        var value = (byte)self;
        return (Value)(byte)(value + 0);
    }

    public static void Convert(this Value self, out global::TestProject.Values result)
    {
        var value = (byte)self;
        result = (global::TestProject.Values)(byte)(value - 0);
    }

    public static bool TryConvert(this Value self, out global::TestProject.Values result)
    {
        switch (self)
        {
            case Value.First:
            {
                result = global::TestProject.Values.First;
                return true;
            }

        }

        result = default;
        return false;
    }

    public static bool TryConvert(this Value self, out ulong order, out ulong value)
    {
        switch (self)
        {
            case Value.First:
            {
                order = 0;
                value = 0;
                return true;
            }

        }

        order = default;
        value = default;
        return false;
    }

}



}

