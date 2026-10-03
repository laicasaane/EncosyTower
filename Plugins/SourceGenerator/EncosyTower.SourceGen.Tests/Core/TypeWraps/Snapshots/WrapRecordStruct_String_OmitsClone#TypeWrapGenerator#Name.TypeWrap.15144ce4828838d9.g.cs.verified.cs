#pragma warning disable 0219

using EncosyTower.TypeWraps;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SCM = System.ComponentModel;
using g__SC = global::System.Collections;
using g__SCG = global::System.Collections.Generic;
using g__SD = global::System.Diagnostics;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SG = global::System.Globalization;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SRIS = global::System.Runtime.InteropServices;
using g__ET = global::EncosyTower.Common;
using g__ETTW = global::EncosyTower.TypeWraps;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

    [g__SCM.TypeConverter(typeof(global::TestProject.Name.NameTypeConverter))]
    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial record struct Name : g__ETTW.IWrap<string>
        , g__S.IEquatable<global::TestProject.Name>
        , g__S.IEquatable<string>
        , g__S.IComparable
        , g__S.IComparable<global::TestProject.Name>
        , g__S.IComparable<string>
    {
        public static readonly global::TestProject.Name Empty = new global::TestProject.Name(string.Empty);

        public readonly char this[int index]
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => this.Value[index];

        }

        public readonly int Length
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => this.Value.Length;

        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Compare(string strA, int indexA, string strB, int indexB, int length)
            => string.Compare(strA, indexA, strB, indexB, length);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase)
            => string.Compare(strA, indexA, strB, indexB, length, ignoreCase);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase, global::System.Globalization.CultureInfo culture)
            => string.Compare(strA, indexA, strB, indexB, length, ignoreCase, culture);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Compare(string strA, int indexA, string strB, int indexB, int length, global::System.Globalization.CultureInfo culture, global::System.Globalization.CompareOptions options)
            => string.Compare(strA, indexA, strB, indexB, length, culture, options);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Compare(string strA, int indexA, string strB, int indexB, int length, global::System.StringComparison comparisonType)
            => string.Compare(strA, indexA, strB, indexB, length, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Compare(string strA, string strB)
            => string.Compare(strA, strB);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Compare(string strA, string strB, bool ignoreCase)
            => string.Compare(strA, strB, ignoreCase);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Compare(string strA, string strB, bool ignoreCase, global::System.Globalization.CultureInfo culture)
            => string.Compare(strA, strB, ignoreCase, culture);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Compare(string strA, string strB, global::System.Globalization.CultureInfo culture, global::System.Globalization.CompareOptions options)
            => string.Compare(strA, strB, culture, options);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Compare(string strA, string strB, global::System.StringComparison comparisonType)
            => string.Compare(strA, strB, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int CompareOrdinal(string strA, int indexA, string strB, int indexB, int length)
            => string.CompareOrdinal(strA, indexA, strB, indexB, length);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int CompareOrdinal(string strA, string strB)
            => string.CompareOrdinal(strA, strB);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int CompareTo(string strB)
            => this.Value.CompareTo(strB);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat(global::System.Collections.Generic.IEnumerable<string> values)
            => string.Concat(values);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat(object arg0)
            => string.Concat(arg0);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat(object arg0, object arg1)
            => string.Concat(arg0, arg1);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat(object arg0, object arg1, object arg2)
            => string.Concat(arg0, arg1, arg2);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat(object[] args)
            => string.Concat(args);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat(global::System.ReadOnlySpan<char> str0, global::System.ReadOnlySpan<char> str1)
            => string.Concat(str0, str1);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat(global::System.ReadOnlySpan<char> str0, global::System.ReadOnlySpan<char> str1, global::System.ReadOnlySpan<char> str2)
            => string.Concat(str0, str1, str2);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat(global::System.ReadOnlySpan<char> str0, global::System.ReadOnlySpan<char> str1, global::System.ReadOnlySpan<char> str2, global::System.ReadOnlySpan<char> str3)
            => string.Concat(str0, str1, str2, str3);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat(string str0, string str1)
            => string.Concat(str0, str1);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat(string str0, string str1, string str2)
            => string.Concat(str0, str1, str2);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat(string str0, string str1, string str2, string str3)
            => string.Concat(str0, str1, str2, str3);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat(string[] values)
            => string.Concat(values);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Concat<T>(global::System.Collections.Generic.IEnumerable<T> values)
            => string.Concat<T>(values);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool Contains(char value)
            => this.Value.Contains(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool Contains(char value, global::System.StringComparison comparisonType)
            => this.Value.Contains(value, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool Contains(string value)
            => this.Value.Contains(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool Contains(string value, global::System.StringComparison comparisonType)
            => this.Value.Contains(value, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
            => this.Value.CopyTo(sourceIndex, destination, destinationIndex, count);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Create<TState>(int length, TState state, global::System.Buffers.SpanAction<char, TState> action)
            => string.Create<TState>(length, state, action);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(char value)
            => this.Value.EndsWith(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(string value)
            => this.Value.EndsWith(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(string value, bool ignoreCase, global::System.Globalization.CultureInfo culture)
            => this.Value.EndsWith(value, ignoreCase, culture);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(string value, global::System.StringComparison comparisonType)
            => this.Value.EndsWith(value, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public global::System.Text.StringRuneEnumerator EnumerateRunes()
            => this.Value.EnumerateRunes();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool Equals(string value)
            => this.Value.Equals(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool Equals(string a, string b)
            => string.Equals(a, b);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool Equals(string a, string b, global::System.StringComparison comparisonType)
            => string.Equals(a, b, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool Equals(string value, global::System.StringComparison comparisonType)
            => this.Value.Equals(value, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Format(global::System.IFormatProvider provider, string format, object arg0)
            => string.Format(provider, format, arg0);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Format(global::System.IFormatProvider provider, string format, object arg0, object arg1)
            => string.Format(provider, format, arg0, arg1);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Format(global::System.IFormatProvider provider, string format, object arg0, object arg1, object arg2)
            => string.Format(provider, format, arg0, arg1, arg2);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Format(global::System.IFormatProvider provider, string format, object[] args)
            => string.Format(provider, format, args);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Format(string format, object arg0)
            => string.Format(format, arg0);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Format(string format, object arg0, object arg1)
            => string.Format(format, arg0, arg1);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Format(string format, object arg0, object arg1, object arg2)
            => string.Format(format, arg0, arg1, arg2);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Format(string format, object[] args)
            => string.Format(format, args);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public global::System.CharEnumerator GetEnumerator()
            => this.Value.GetEnumerator();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(global::System.ReadOnlySpan<char> value)
            => string.GetHashCode(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(global::System.ReadOnlySpan<char> value, global::System.StringComparison comparisonType)
            => string.GetHashCode(value, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(global::System.StringComparison comparisonType)
            => this.Value.GetHashCode(comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public ref readonly char GetPinnableReference()
            => ref this.Value.GetPinnableReference();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public global::System.TypeCode GetTypeCode()
            => this.Value.GetTypeCode();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char value)
            => this.Value.IndexOf(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char value, int startIndex)
            => this.Value.IndexOf(value, startIndex);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char value, int startIndex, int count)
            => this.Value.IndexOf(value, startIndex, count);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char value, global::System.StringComparison comparisonType)
            => this.Value.IndexOf(value, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOf(string value)
            => this.Value.IndexOf(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOf(string value, int startIndex)
            => this.Value.IndexOf(value, startIndex);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOf(string value, int startIndex, int count)
            => this.Value.IndexOf(value, startIndex, count);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOf(string value, int startIndex, int count, global::System.StringComparison comparisonType)
            => this.Value.IndexOf(value, startIndex, count, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOf(string value, int startIndex, global::System.StringComparison comparisonType)
            => this.Value.IndexOf(value, startIndex, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOf(string value, global::System.StringComparison comparisonType)
            => this.Value.IndexOf(value, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOfAny(char[] anyOf)
            => this.Value.IndexOfAny(anyOf);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOfAny(char[] anyOf, int startIndex)
            => this.Value.IndexOfAny(anyOf, startIndex);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int IndexOfAny(char[] anyOf, int startIndex, int count)
            => this.Value.IndexOfAny(anyOf, startIndex, count);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Insert(int startIndex, string value)
            => this.Value.Insert(startIndex, value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Intern(string str)
            => string.Intern(str);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string IsInterned(string str)
            => string.IsInterned(str);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool IsNormalized()
            => this.Value.IsNormalized();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool IsNormalized(global::System.Text.NormalizationForm normalizationForm)
            => this.Value.IsNormalized(normalizationForm);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(false)] string value)
            => string.IsNullOrEmpty(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(false)] string value)
            => string.IsNullOrWhiteSpace(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Join(char separator, object[] values)
            => string.Join(separator, values);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Join(char separator, string[] value)
            => string.Join(separator, value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Join(char separator, string[] value, int startIndex, int count)
            => string.Join(separator, value, startIndex, count);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Join(string separator, global::System.Collections.Generic.IEnumerable<string> values)
            => string.Join(separator, values);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Join(string separator, object[] values)
            => string.Join(separator, values);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Join(string separator, string[] value)
            => string.Join(separator, value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Join(string separator, string[] value, int startIndex, int count)
            => string.Join(separator, value, startIndex, count);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Join<T>(char separator, global::System.Collections.Generic.IEnumerable<T> values)
            => string.Join<T>(separator, values);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Join<T>(string separator, global::System.Collections.Generic.IEnumerable<T> values)
            => string.Join<T>(separator, values);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(char value)
            => this.Value.LastIndexOf(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(char value, int startIndex)
            => this.Value.LastIndexOf(value, startIndex);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(char value, int startIndex, int count)
            => this.Value.LastIndexOf(value, startIndex, count);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(string value)
            => this.Value.LastIndexOf(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(string value, int startIndex)
            => this.Value.LastIndexOf(value, startIndex);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(string value, int startIndex, int count)
            => this.Value.LastIndexOf(value, startIndex, count);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(string value, int startIndex, int count, global::System.StringComparison comparisonType)
            => this.Value.LastIndexOf(value, startIndex, count, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(string value, int startIndex, global::System.StringComparison comparisonType)
            => this.Value.LastIndexOf(value, startIndex, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(string value, global::System.StringComparison comparisonType)
            => this.Value.LastIndexOf(value, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int LastIndexOfAny(char[] anyOf)
            => this.Value.LastIndexOfAny(anyOf);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int LastIndexOfAny(char[] anyOf, int startIndex)
            => this.Value.LastIndexOfAny(anyOf, startIndex);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int LastIndexOfAny(char[] anyOf, int startIndex, int count)
            => this.Value.LastIndexOfAny(anyOf, startIndex, count);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Normalize()
            => this.Value.Normalize();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Normalize(global::System.Text.NormalizationForm normalizationForm)
            => this.Value.Normalize(normalizationForm);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string PadLeft(int totalWidth)
            => this.Value.PadLeft(totalWidth);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string PadLeft(int totalWidth, char paddingChar)
            => this.Value.PadLeft(totalWidth, paddingChar);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string PadRight(int totalWidth)
            => this.Value.PadRight(totalWidth);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string PadRight(int totalWidth, char paddingChar)
            => this.Value.PadRight(totalWidth, paddingChar);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Remove(int startIndex)
            => this.Value.Remove(startIndex);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Remove(int startIndex, int count)
            => this.Value.Remove(startIndex, count);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Replace(char oldChar, char newChar)
            => this.Value.Replace(oldChar, newChar);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Replace(string oldValue, string newValue)
            => this.Value.Replace(oldValue, newValue);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Replace(string oldValue, string newValue, bool ignoreCase, global::System.Globalization.CultureInfo culture)
            => this.Value.Replace(oldValue, newValue, ignoreCase, culture);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Replace(string oldValue, string newValue, global::System.StringComparison comparisonType)
            => this.Value.Replace(oldValue, newValue, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string[] Split(char separator, int count, global::System.StringSplitOptions options)
            => this.Value.Split(separator, count, options);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string[] Split(char separator, global::System.StringSplitOptions options)
            => this.Value.Split(separator, options);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string[] Split(char[] separator)
            => this.Value.Split(separator);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string[] Split(char[] separator, int count)
            => this.Value.Split(separator, count);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string[] Split(char[] separator, int count, global::System.StringSplitOptions options)
            => this.Value.Split(separator, count, options);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string[] Split(char[] separator, global::System.StringSplitOptions options)
            => this.Value.Split(separator, options);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string[] Split(string separator, int count, global::System.StringSplitOptions options)
            => this.Value.Split(separator, count, options);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string[] Split(string separator, global::System.StringSplitOptions options)
            => this.Value.Split(separator, options);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string[] Split(string[] separator, int count, global::System.StringSplitOptions options)
            => this.Value.Split(separator, count, options);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string[] Split(string[] separator, global::System.StringSplitOptions options)
            => this.Value.Split(separator, options);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(char value)
            => this.Value.StartsWith(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(string value)
            => this.Value.StartsWith(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(string value, bool ignoreCase, global::System.Globalization.CultureInfo culture)
            => this.Value.StartsWith(value, ignoreCase, culture);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(string value, global::System.StringComparison comparisonType)
            => this.Value.StartsWith(value, comparisonType);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Substring(int startIndex)
            => this.Value.Substring(startIndex);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Substring(int startIndex, int length)
            => this.Value.Substring(startIndex, length);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public char[] ToCharArray()
            => this.Value.ToCharArray();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public char[] ToCharArray(int startIndex, int length)
            => this.Value.ToCharArray(startIndex, length);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string ToLower()
            => this.Value.ToLower();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string ToLower(global::System.Globalization.CultureInfo culture)
            => this.Value.ToLower(culture);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string ToLowerInvariant()
            => this.Value.ToLowerInvariant();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string ToString(global::System.IFormatProvider provider)
            => this.Value.ToString(provider);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string ToUpper()
            => this.Value.ToUpper();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string ToUpper(global::System.Globalization.CultureInfo culture)
            => this.Value.ToUpper(culture);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string ToUpperInvariant()
            => this.Value.ToUpperInvariant();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Trim()
            => this.Value.Trim();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Trim(char trimChar)
            => this.Value.Trim(trimChar);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string Trim(char[] trimChars)
            => this.Value.Trim(trimChars);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string TrimEnd()
            => this.Value.TrimEnd();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string TrimEnd(char trimChar)
            => this.Value.TrimEnd(trimChar);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string TrimEnd(char[] trimChars)
            => this.Value.TrimEnd(trimChars);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string TrimStart()
            => this.Value.TrimStart();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string TrimStart(char trimChar)
            => this.Value.TrimStart(trimChar);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string TrimStart(char[] trimChars)
            => this.Value.TrimStart(trimChars);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(global::TestProject.Name other)
            => this.Value.CompareTo(other.Value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(object obj)
            => obj switch
            {
                Name other => CompareTo(other),
                string other => this.Value.CompareTo(other),
                _ => 1,
            };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(global::TestProject.Name other)
            => this.Value == other.Value;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => this.Value.GetHashCode();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => this.Value.ToString();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator global::TestProject.Name(string value)
            => new global::TestProject.Name(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(global::TestProject.Name value)
            => value.Value;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Name operator +(global::TestProject.Name left, global::TestProject.Name right)
        {
            return new global::TestProject.Name((string)(left.Value + right.Value));
        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        private sealed class NameTypeConverter : g__SCM.TypeConverter
        {
            private static readonly g__S.Type s_wrapperType = typeof(global::TestProject.Name);
            private static readonly g__S.Type s_underlyingType = typeof(string);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public override bool CanConvertFrom(g__SCM.ITypeDescriptorContext context, g__S.Type sourceType)
            {
                if (sourceType == s_wrapperType || sourceType == s_underlyingType) return true;
                return base.CanConvertFrom(context, sourceType);
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public override bool CanConvertTo(g__SCM.ITypeDescriptorContext context, g__S.Type destinationType)
            {
                if (destinationType == s_wrapperType || destinationType == s_underlyingType) return true;
                return base.CanConvertTo(context, destinationType);
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public override object ConvertFrom(g__SCM.ITypeDescriptorContext context, g__SG.CultureInfo culture, object value)
            {
                if (value != null)
                {
                    var t = value.GetType();
                    if (t == typeof(global::TestProject.Name)) return (global::TestProject.Name)value;
                    if (t == typeof(string)) return new global::TestProject.Name((string)value);
                }
                return base.ConvertFrom(context, culture, value);
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public override object ConvertTo(g__SCM.ITypeDescriptorContext context, g__SG.CultureInfo culture, object value, g__S.Type destinationType)
            {
                if (value is global::TestProject.Name wrappedValue)
                {
                    if (destinationType == s_wrapperType) return wrappedValue;
                    if (destinationType == s_underlyingType) return wrappedValue.Value;
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

    }
#region INTERNALS
#endregion ======

    partial record struct Name // Internals
    {
        private const string GENERATOR = "EncosyTower.Core.Generators.TypeWraps.TypeWrapGenerator";

    }



}

