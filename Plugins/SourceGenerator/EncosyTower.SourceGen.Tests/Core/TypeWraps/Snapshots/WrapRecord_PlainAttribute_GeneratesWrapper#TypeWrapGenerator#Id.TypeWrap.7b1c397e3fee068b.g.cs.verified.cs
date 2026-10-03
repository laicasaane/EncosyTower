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

    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial record struct Id : g__ETTW.IWrap<int>
        , g__S.IEquatable<global::TestProject.Id>
        , g__S.IEquatable<int>
        , g__S.IComparable
        , g__S.IComparable<global::TestProject.Id>
        , g__S.IComparable<int>
    {
        public static readonly global::TestProject.Id MaxValue = new global::TestProject.Id(int.MaxValue);

        public static readonly global::TestProject.Id MinValue = new global::TestProject.Id(int.MinValue);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(int value)
            => this.Value.CompareTo(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(int obj)
            => this.Value.Equals(obj);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly global::System.TypeCode GetTypeCode()
            => this.Value.GetTypeCode();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Parse(global::System.ReadOnlySpan<char> s, global::System.Globalization.NumberStyles style, global::System.IFormatProvider provider)
            => int.Parse(s, style, provider);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Parse(string s)
            => int.Parse(s);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Parse(string s, global::System.Globalization.NumberStyles style)
            => int.Parse(s, style);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Parse(string s, global::System.Globalization.NumberStyles style, global::System.IFormatProvider provider)
            => int.Parse(s, style, provider);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static int Parse(string s, global::System.IFormatProvider provider)
            => int.Parse(s, provider);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(global::System.IFormatProvider provider)
            => this.Value.ToString(provider);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string format)
            => this.Value.ToString(format);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string format, global::System.IFormatProvider provider)
            => this.Value.ToString(format, provider);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly bool TryFormat(global::System.Span<char> destination, out int charsWritten, global::System.ReadOnlySpan<char> format, global::System.IFormatProvider provider)
            => this.Value.TryFormat(destination, out charsWritten, format, provider);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(global::System.ReadOnlySpan<char> s, global::System.Globalization.NumberStyles style, global::System.IFormatProvider provider, out int result)
            => int.TryParse(s, style, provider, out result);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(global::System.ReadOnlySpan<char> s, out int result)
            => int.TryParse(s, out result);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool TryParse([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] string s, global::System.Globalization.NumberStyles style, global::System.IFormatProvider provider, out int result)
            => int.TryParse(s, style, provider, out result);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool TryParse([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] string s, out int result)
            => int.TryParse(s, out result);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(global::TestProject.Id other)
            => this.Value.CompareTo(other.Value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(object obj)
            => obj switch
            {
                Id other => CompareTo(other),
                int other => this.Value.CompareTo(other),
                _ => 1,
            };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(global::TestProject.Id other)
            => this.Value == other.Value;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => this.Value.GetHashCode();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => this.Value.ToString();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator global::TestProject.Id(int value)
            => new global::TestProject.Id(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(global::TestProject.Id value)
            => value.Value;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator +(global::TestProject.Id value)
        {
            return new global::TestProject.Id((int)(+(value.Value)));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator -(global::TestProject.Id value)
        {
            return new global::TestProject.Id((int)(-(value.Value)));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator ~(global::TestProject.Id value)
        {
            return new global::TestProject.Id((int)(~(value.Value)));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator ++(global::TestProject.Id value)
        {
            var tempValue = value.Value;
            tempValue ++;
            return new global::TestProject.Id((int)(tempValue));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator --(global::TestProject.Id value)
        {
            var tempValue = value.Value;
            tempValue --;
            return new global::TestProject.Id((int)(tempValue));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator +(global::TestProject.Id left, global::TestProject.Id right)
        {
            return new global::TestProject.Id((int)(left.Value + right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator -(global::TestProject.Id left, global::TestProject.Id right)
        {
            return new global::TestProject.Id((int)(left.Value - right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator *(global::TestProject.Id left, global::TestProject.Id right)
        {
            return new global::TestProject.Id((int)(left.Value * right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator /(global::TestProject.Id left, global::TestProject.Id right)
        {
            return new global::TestProject.Id((int)(left.Value / right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator %(global::TestProject.Id left, global::TestProject.Id right)
        {
            return new global::TestProject.Id((int)(left.Value % right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator &(global::TestProject.Id left, global::TestProject.Id right)
        {
            return new global::TestProject.Id((int)(left.Value & right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator |(global::TestProject.Id left, global::TestProject.Id right)
        {
            return new global::TestProject.Id((int)(left.Value | right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator ^(global::TestProject.Id left, global::TestProject.Id right)
        {
            return new global::TestProject.Id((int)(left.Value ^ right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator <<(global::TestProject.Id left, int right)
        {
            return new global::TestProject.Id((int)(left.Value << right));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Id operator >>(global::TestProject.Id left, int right)
        {
            return new global::TestProject.Id((int)(left.Value >> right));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator >(global::TestProject.Id left, global::TestProject.Id right)
        {
            return left.Value > right.Value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator <(global::TestProject.Id left, global::TestProject.Id right)
        {
            return left.Value < right.Value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(global::TestProject.Id left, global::TestProject.Id right)
        {
            return left.Value >= right.Value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(global::TestProject.Id left, global::TestProject.Id right)
        {
            return left.Value <= right.Value;
        }

    }
#region INTERNALS
#endregion ======

    partial record struct Id // Internals
    {
        private const string GENERATOR = "EncosyTower.Core.Generators.TypeWraps.TypeWrapGenerator";

    }



}

