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


#region    INTERFACE
#endregion =========

static partial class FruitExtensions { } // IFruitExtensions

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::TestProject.Fruit), typeof(IFruitExtensions), typeof(FruitExtensions), typeof(FruitExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.7-preview.3")]
public partial interface IFruitExtensions
    : g__ETEE.IEnumExtensions<FruitExtended, global::TestProject.Fruit, byte>
    , g__ETCon.IToFixedString
    , g__ETCon.IToDisplayFixedString
    , g__ETCon.IToFixedString<g__UC.FixedString32Bytes>
    , g__ETCon.IToDisplayFixedString<g__UC.FixedString32Bytes>
{
}

#region    EXTENDED STRUCT
#endregion ===============

static partial class FruitExtensions { } // FruitExtended

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::TestProject.Fruit), typeof(IFruitExtensions), typeof(FruitExtensions), typeof(FruitExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.7-preview.3")]
[g__SDCA.ExcludeFromCodeCoverage]
[g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
public readonly partial struct FruitExtended : IFruitExtensions
    , g__S.IEquatable<FruitExtended>
    , g__S.IComparable<FruitExtended>
{
    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    private readonly global::TestProject.Fruit _value;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    private readonly byte _underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public FruitExtended(global::TestProject.Fruit value) : this()
    {
        _value = value;
    }

    public global::TestProject.Fruit Value
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
        get => FruitExtensions.Length;
    }

    public bool IsDefined
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => FruitExtensions.IsDefined(_value);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public FruitExtended Create(global::TestProject.Fruit value) => new FruitExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public FruitExtended CreateFromUnderlyingValue(byte value) => new FruitExtended((global::TestProject.Fruit)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToStringFast() => ToStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToStringFast(bool emptyIfUndefined) => FruitExtensions.ToStringFast(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayString() => ToDisplayString(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayString(bool emptyIfUndefined) => FruitExtensions.ToDisplayStringFast(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayStringFast() => ToDisplayStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayStringFast(bool emptyIfUndefined) => FruitExtensions.ToDisplayStringFast(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string name, out FruitExtended value) => TryParse(name, out value, false, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string name, out FruitExtended value, bool ignoreCase) => TryParse(name, out value, ignoreCase, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string name, out FruitExtended value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        var result = FruitExtensions.TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);
        value = new FruitExtended(enumValue);
        return result;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(g__S.ReadOnlySpan<char> name, out FruitExtended value) => TryParse(name, out value, false, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(g__S.ReadOnlySpan<char> name, out FruitExtended value, bool ignoreCase) => TryParse(name, out value, ignoreCase, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(g__S.ReadOnlySpan<char> name, out FruitExtended value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        var result = FruitExtensions.TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);
        value = new FruitExtended(enumValue);
        return result;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToFixedString() => ToFixedString(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToFixedString(bool emptyIfUndefined) => FruitExtensions.ToFixedString(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToDisplayFixedString() => ToDisplayFixedString(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToDisplayFixedString(bool emptyIfUndefined) => FruitExtensions.ToDisplayFixedString(_value, emptyIfUndefined);

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
    ) => FruitExtensions.TryFormat(_value, destination, out charsWritten);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(
          g__S.Span<char> destination
        , out int charsWritten
        , g__S.ReadOnlySpan<char> format
        , g__S.IFormatProvider provider = null
    ) => FruitExtensions.TryFormat(_value, destination, out charsWritten, format, provider);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool IsNameDefined(string name) => FruitExtensions.IsNameDefined(name, default(global::TestProject.Fruit));

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool IsNameDefined(string name, bool allowMatchingMetadataAttribute) => FruitExtensions.IsNameDefined(name, default(global::TestProject.Fruit), allowMatchingMetadataAttribute);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public int ToIndex() => FruitExtensions.FindIndex(_value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public override string ToString() => ToStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToString(string format, g__S.IFormatProvider formatProvider) => ToStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => _underlyingValue.GetHashCode();

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public int CompareTo(FruitExtended other) => this._underlyingValue.CompareTo(other._underlyingValue);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool Equals(FruitExtended other) => this._underlyingValue == other._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj) => obj is FruitExtended other && this._underlyingValue == other._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static implicit operator FruitExtended(global::TestProject.Fruit value) => new FruitExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(FruitExtended left, FruitExtended right) => left._underlyingValue == right._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(FruitExtended left, FruitExtended right) => left._underlyingValue != right._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator <(FruitExtended left, FruitExtended right) => left._underlyingValue < right._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator >(FruitExtended left, FruitExtended right) => left._underlyingValue > right._underlyingValue;

}

#region    EXTENSIONS
#endregion ==========

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::TestProject.Fruit), typeof(IFruitExtensions), typeof(FruitExtensions), typeof(FruitExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.7-preview.3")]
[g__SDCA.ExcludeFromCodeCoverage]
public static partial class FruitExtensions // FruitExtensions
{
    /// <summary>
    /// The number of members in the enum.
    /// This is a non-distinct count of defined names.
    /// </summary>
    public const int Length = 1;

    /// <summary>
    /// Returns the string representation of the <see cref="global::TestProject.Fruit"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToStringFast(this global::TestProject.Fruit value)
        => ToStringFast(value, true);

    /// <summary>
    /// Returns the string representation of the <see cref="global::TestProject.Fruit"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToStringFast(this global::TestProject.Fruit value, bool emptyIfUndefined)
        => Names.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the string representation of the <see cref="global::TestProject.Fruit"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToDisplayStringFast(this global::TestProject.Fruit value)
        => ToDisplayStringFast(value, true);

    /// <summary>
    /// Returns the string representation of the <see cref="global::TestProject.Fruit"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToDisplayStringFast(this global::TestProject.Fruit value, bool emptyIfUndefined)
        => DisplayNames.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::TestProject.Fruit"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToFixedString(this global::TestProject.Fruit value)
        => ToFixedString(value, true);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::TestProject.Fruit"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToFixedString(this global::TestProject.Fruit value, bool emptyIfUndefined)
        => FixedNames.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::TestProject.Fruit"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToDisplayFixedString(this global::TestProject.Fruit value)
        => ToDisplayFixedString(value, true);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::TestProject.Fruit"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToDisplayFixedString(this global::TestProject.Fruit value, bool emptyIfUndefined)
        => FixedDisplayNames.Get(value, emptyIfUndefined);

    private static g__UC.FixedString32Bytes ToFixedString(byte value)
    {
        var fs = new g__UC.FixedString32Bytes();
        g__UC.FixedStringMethods.Append(ref fs, value);
        return fs;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static FruitExtended AsExtended(this global::TestProject.Fruit value)
        => new FruitExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static FruitExtended AsFruitExtended(this byte value)
        => new FruitExtended((global::TestProject.Fruit)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static FruitExtended AsFruitExtended(this sbyte value)
        => new FruitExtended((global::TestProject.Fruit)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static FruitExtended AsFruitExtended(this short value)
        => new FruitExtended((global::TestProject.Fruit)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static FruitExtended AsFruitExtended(this ushort value)
        => new FruitExtended((global::TestProject.Fruit)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static FruitExtended AsFruitExtended(this int value)
        => new FruitExtended((global::TestProject.Fruit)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static FruitExtended AsFruitExtended(this uint value)
        => new FruitExtended((global::TestProject.Fruit)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static FruitExtended AsFruitExtended(this long value)
        => new FruitExtended((global::TestProject.Fruit)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static FruitExtended AsFruitExtended(this ulong value)
        => new FruitExtended((global::TestProject.Fruit)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static byte ToUnderlyingValue(this global::TestProject.Fruit value)
        => (byte)value;

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Fruit" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Fruit" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Fruit" />. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this string name, out global::TestProject.Fruit value)
        => TryParse(name, out value, false, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Fruit" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Fruit" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Fruit" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this string name, out global::TestProject.Fruit value, bool ignoreCase)
        => TryParse(name, out value, ignoreCase, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Fruit" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Fruit" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Fruit" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
    /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(this string name, out global::TestProject.Fruit value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            value = default;
            return false;
        }

        var stringComparison = ignoreCase ? g__S.StringComparison.OrdinalIgnoreCase : g__S.StringComparison.Ordinal;

        switch (name)
        {
            case string s when s.Equals(Names.Apple, stringComparison):
            {
                value = global::TestProject.Fruit.Apple;
                return true;
            }

            case string s when byte.TryParse(name, out var underlyingValue):
            {
                value = (global::TestProject.Fruit)underlyingValue;
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
    /// an <see cref="global::TestProject.Fruit" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Fruit" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Fruit" />. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this g__S.ReadOnlySpan<char> name, out global::TestProject.Fruit value)
        => TryParse(name, out value, false, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Fruit" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Fruit" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Fruit" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this g__S.ReadOnlySpan<char> name, out global::TestProject.Fruit value, bool ignoreCase)
        => TryParse(name, out value, ignoreCase, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Fruit" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Fruit" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Fruit" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
    /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(this g__S.ReadOnlySpan<char> name, out global::TestProject.Fruit value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        if (name.IsEmpty)
        {
            value = default;
            return false;
        }

        var stringComparison = ignoreCase ? g__S.StringComparison.OrdinalIgnoreCase : g__S.StringComparison.Ordinal;

        switch (name)
        {
            case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.Apple), stringComparison):
            {
                value = global::TestProject.Fruit.Apple;
                return true;
            }

            case g__S.ReadOnlySpan<char> s when byte.TryParse(name, out var underlyingValue):
            {
                value = (global::TestProject.Fruit)underlyingValue;
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
    public static bool IsDefined(this global::TestProject.Fruit value)
        => value switch
        {
            global::TestProject.Fruit.Apple => true,
            _ => false,
        };

    /// <summary>
    /// Returns a boolean telling whether an enum with the given name exists in the enumeration.
    /// </summary>
    /// <param name="name">The name to check if it's defined</param>
    /// <returns><c>true</c> if a member with the name exists in the enumeration, <c>false</c> otherwise</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool IsNameDefined(this string name, global::TestProject.Fruit _)
        => IsNameDefined(name, default(global::TestProject.Fruit), allowMatchingMetadataAttribute: false);

    /// <summary>
    /// Returns a boolean telling whether an enum with the given name exists in the enumeration,
    /// or if a member decorated with a <c>[Display]</c> attribute
    /// with the required name exists.
    /// </summary>
    /// <param name="name">The name to check if it's defined</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value of metadata attributes, otherwise ignores them</param>
    /// <returns><c>true</c> if a member with the name exists in the enumeration, or a member is decorated
    /// with a <c>[Display]</c> attribute with the name, <c>false</c> otherwise</returns>
    public static bool IsNameDefined(this string name, global::TestProject.Fruit _, bool allowMatchingMetadataAttribute)
    {
        return name switch
        {
            Names.Apple => true,
            _ => false,
        };
    }

    public static bool TryFormat(
          this global::TestProject.Fruit value
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
          this global::TestProject.Fruit value
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
    public static int FindIndex(this global::TestProject.Fruit value)
        => value switch
        {
            global::TestProject.Fruit.Apple => 0,
            _ => -1,
        };

    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.7-preview.3")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class Values
    {
        private static readonly global::TestProject.Fruit[] s_values = new global::TestProject.Fruit[]
        {
            global::TestProject.Fruit.Apple,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<global::TestProject.Fruit> AsMemory() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<global::TestProject.Fruit> AsSpan() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<global::TestProject.Fruit> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => g__UC.CollectionHelper.CreateNativeArray<global::TestProject.Fruit>(s_values, allocator);
    }

    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.7-preview.3")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class UnderlyingValues
    {
        private static readonly byte[] s_values = new byte[]
        {
            ToUnderlyingValue(global::TestProject.Fruit.Apple),
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

static partial class FruitExtensions// Names
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.7-preview.3")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class Names
    {
        public const string Apple = nameof(global::TestProject.Fruit.Apple);

        private static readonly string[] s_names = new string[]
        {
            Apple,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Get(global::TestProject.Fruit value) => Get(value, true);

        public static string Get(global::TestProject.Fruit value, bool emptyIfUndefined)
            => value switch
            {
                global::TestProject.Fruit.Apple => Apple,
                _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
            };
    }

}

#region    DISPLAY NAMES
#endregion =============

static partial class FruitExtensions// DisplayNames
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.7-preview.3")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class DisplayNames
    {
        public const string Apple = Names.Apple;

        private static readonly string[] s_names = new string[]
        {
            Apple,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Get(global::TestProject.Fruit value) => Get(value, true);

        public static string Get(global::TestProject.Fruit value, bool emptyIfUndefined)
            => value switch
            {
                global::TestProject.Fruit.Apple => Apple,
                _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
            };
    }

}

#region    FIXED NAMES
#endregion ===========

static partial class FruitExtensions// FixedNames
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.7-preview.3")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class FixedNames
    {
        public static g__UC.FixedString32Bytes Apple
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)Names.Apple;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

        public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        {
            var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(FruitExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
            names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Apple);
            return names;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes Get(global::TestProject.Fruit value)
            => Get(value, true);

        public static g__UC.FixedString32Bytes Get(global::TestProject.Fruit value, bool emptyIfUndefined)
            => value switch
            {
                global::TestProject.Fruit.Apple => Apple,
                _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
            };
    }

}

#region    FIXED DISPLAY NAMES
#endregion ===================

static partial class FruitExtensions// FixedDisplayNames
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator", "0.1.7-preview.3")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class FixedDisplayNames
    {
        public static g__UC.FixedString32Bytes Apple
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)DisplayNames.Apple;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

        public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        {
            var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(FruitExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
            names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Apple);
            return names;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes Get(global::TestProject.Fruit value)
            => Get(value, true);

        public static g__UC.FixedString32Bytes Get(global::TestProject.Fruit value, bool emptyIfUndefined)
            => value switch
            {
                global::TestProject.Fruit.Apple => Apple,
                _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
            };
    }

}



}

