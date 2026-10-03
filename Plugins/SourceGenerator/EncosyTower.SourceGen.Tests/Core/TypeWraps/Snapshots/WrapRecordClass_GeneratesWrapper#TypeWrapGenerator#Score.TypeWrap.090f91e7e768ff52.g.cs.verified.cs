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

    [g__SCM.TypeConverter(typeof(global::TestProject.Score.ScoreTypeConverter))]
    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial record class Score : g__ETTW.IWrap<int>
        , g__S.IEquatable<global::TestProject.Score>
        , g__S.IEquatable<int>
        , g__S.IComparable
        , g__S.IComparable<global::TestProject.Score>
        , g__S.IComparable<int>
    {
        public static readonly global::TestProject.Score MaxValue = new global::TestProject.Score(int.MaxValue);

        public static readonly global::TestProject.Score MinValue = new global::TestProject.Score(int.MinValue);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int CompareTo(int value)
            => this.Value.CompareTo(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool Equals(int obj)
            => this.Value.Equals(obj);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public global::System.TypeCode GetTypeCode()
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
        public string ToString(global::System.IFormatProvider provider)
            => this.Value.ToString(provider);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string ToString(string format)
            => this.Value.ToString(format);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, global::System.IFormatProvider provider)
            => this.Value.ToString(format, provider);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool TryFormat(global::System.Span<char> destination, out int charsWritten, global::System.ReadOnlySpan<char> format, global::System.IFormatProvider provider)
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
        public virtual int CompareTo(global::TestProject.Score other)
        {
            if (global::System.Object.ReferenceEquals(this, other))
            {
                return 0;
            }

            if (other is null)
            {
                return 1;
            }

            return this.Value.CompareTo(other.Value);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int CompareTo(object obj)
            => obj switch
            {
                Score other => CompareTo(other),
                int other => this.Value.CompareTo(other),
                _ => 1,
            };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static explicit operator global::TestProject.Score(int value)
            => new global::TestProject.Score(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(global::TestProject.Score value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            return value.Value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator +(global::TestProject.Score value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            return new global::TestProject.Score((int)(+(value.Value)));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator -(global::TestProject.Score value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            return new global::TestProject.Score((int)(-(value.Value)));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator ~(global::TestProject.Score value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            return new global::TestProject.Score((int)(~(value.Value)));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator ++(global::TestProject.Score value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            var tempValue = value.Value;
            tempValue ++;
            return new global::TestProject.Score((int)(tempValue));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator --(global::TestProject.Score value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            var tempValue = value.Value;
            tempValue --;
            return new global::TestProject.Score((int)(tempValue));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator +(global::TestProject.Score left, global::TestProject.Score right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Score((int)(left.Value + right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator -(global::TestProject.Score left, global::TestProject.Score right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Score((int)(left.Value - right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator *(global::TestProject.Score left, global::TestProject.Score right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Score((int)(left.Value * right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator /(global::TestProject.Score left, global::TestProject.Score right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Score((int)(left.Value / right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator %(global::TestProject.Score left, global::TestProject.Score right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Score((int)(left.Value % right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator &(global::TestProject.Score left, global::TestProject.Score right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Score((int)(left.Value & right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator |(global::TestProject.Score left, global::TestProject.Score right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Score((int)(left.Value | right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator ^(global::TestProject.Score left, global::TestProject.Score right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Score((int)(left.Value ^ right.Value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator <<(global::TestProject.Score left, int right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            return new global::TestProject.Score((int)(left.Value << right));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Score operator >>(global::TestProject.Score left, int right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            return new global::TestProject.Score((int)(left.Value >> right));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator >(global::TestProject.Score left, global::TestProject.Score right)
        {
            if (global::System.Object.ReferenceEquals(left, right))
            {
                return false;
            }

            if (left is null || right is null)
            {
                return right is null;
            }

            return left.Value > right.Value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator <(global::TestProject.Score left, global::TestProject.Score right)
        {
            if (global::System.Object.ReferenceEquals(left, right))
            {
                return false;
            }

            if (left is null || right is null)
            {
                return left is null;
            }

            return left.Value < right.Value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(global::TestProject.Score left, global::TestProject.Score right)
        {
            if (global::System.Object.ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return right is null;
            }

            return left.Value >= right.Value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(global::TestProject.Score left, global::TestProject.Score right)
        {
            if (global::System.Object.ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return left is null;
            }

            return left.Value <= right.Value;
        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        private sealed class ScoreTypeConverter : g__SCM.TypeConverter
        {
            private static readonly g__S.Type s_wrapperType = typeof(global::TestProject.Score);
            private static readonly g__S.Type s_underlyingType = typeof(int);

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
                    if (t == typeof(global::TestProject.Score)) return (global::TestProject.Score)value;
                    if (t == typeof(int)) return new global::TestProject.Score((int)value);
                }
                return base.ConvertFrom(context, culture, value);
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public override object ConvertTo(g__SCM.ITypeDescriptorContext context, g__SG.CultureInfo culture, object value, g__S.Type destinationType)
            {
                if (value is global::TestProject.Score wrappedValue)
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

    partial record class Score // Internals
    {
        private const string GENERATOR = "EncosyTower.Core.Generators.TypeWraps.TypeWrapGenerator";

    }



}

