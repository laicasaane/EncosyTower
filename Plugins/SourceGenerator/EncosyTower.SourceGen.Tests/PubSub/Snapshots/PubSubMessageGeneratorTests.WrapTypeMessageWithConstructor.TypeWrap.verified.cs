#pragma warning disable 0219

using EncosyTower.PubSub;
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

    [g__SCM.TypeConverter(typeof(global::TestProject.Points.PointsTypeConverter))]
    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial class Points : g__ETTW.IWrap<int>
        , g__S.IEquatable<global::TestProject.Points>
        , g__S.IEquatable<int>
        , g__S.IComparable
        , g__S.IComparable<global::TestProject.Points>
        , g__S.IComparable<int>
    {
        public readonly int value;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public Points(int value)
        {
            this.value = value;
        }

        public static readonly global::TestProject.Points MaxValue = new global::TestProject.Points(int.MaxValue);

        public static readonly global::TestProject.Points MinValue = new global::TestProject.Points(int.MinValue);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int CompareTo(int value)
            => this.value.CompareTo(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool Equals(int obj)
            => this.value.Equals(obj);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public global::System.TypeCode GetTypeCode()
            => this.value.GetTypeCode();

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
            => this.value.ToString(provider);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string ToString(string format)
            => this.value.ToString(format);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, global::System.IFormatProvider provider)
            => this.value.ToString(format, provider);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool TryFormat(global::System.Span<char> destination, out int charsWritten, global::System.ReadOnlySpan<char> format, global::System.IFormatProvider provider)
            => this.value.TryFormat(destination, out charsWritten, format, provider);

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
        public virtual int CompareTo(global::TestProject.Points other)
        {
            if (global::System.Object.ReferenceEquals(this, other))
            {
                return 0;
            }

            if (other is null)
            {
                return 1;
            }

            return this.value.CompareTo(other.value);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int CompareTo(object obj)
            => obj switch
            {
                Points other => CompareTo(other),
                int other => this.value.CompareTo(other),
                _ => 1,
            };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public virtual bool Equals(global::TestProject.Points other)
        {
            if (global::System.Object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            return this.value == other.value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj switch
            {
                Points other => Equals(other),
                int other => this.value.Equals(other),
                _ => false,
            };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => this.value.GetHashCode();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => this.value.ToString();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static explicit operator global::TestProject.Points(int value)
            => new global::TestProject.Points(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(global::TestProject.Points value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            return value.value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator +(global::TestProject.Points value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            return new global::TestProject.Points((int)(+(value.value)));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator -(global::TestProject.Points value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            return new global::TestProject.Points((int)(-(value.value)));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator ~(global::TestProject.Points value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            return new global::TestProject.Points((int)(~(value.value)));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator ++(global::TestProject.Points value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            var tempValue = value.value;
            tempValue ++;
            return new global::TestProject.Points((int)(tempValue));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator --(global::TestProject.Points value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            var tempValue = value.value;
            tempValue --;
            return new global::TestProject.Points((int)(tempValue));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator +(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Points((int)(left.value + right.value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator -(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Points((int)(left.value - right.value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator *(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Points((int)(left.value * right.value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator /(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Points((int)(left.value / right.value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator %(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Points((int)(left.value % right.value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator &(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Points((int)(left.value & right.value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator |(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Points((int)(left.value | right.value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator ^(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(right));
            }

            return new global::TestProject.Points((int)(left.value ^ right.value));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator <<(global::TestProject.Points left, int right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            return new global::TestProject.Points((int)(left.value << right));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static global::TestProject.Points operator >>(global::TestProject.Points left, int right)
        {
            if (left is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(left));
            }

            return new global::TestProject.Points((int)(left.value >> right));
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (global::System.Object.ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.value == right.value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (global::System.Object.ReferenceEquals(left, right))
            {
                return false;
            }

            if (left is null || right is null)
            {
                return true;
            }

            return left.value != right.value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator >(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (global::System.Object.ReferenceEquals(left, right))
            {
                return false;
            }

            if (left is null || right is null)
            {
                return right is null;
            }

            return left.value > right.value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator <(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (global::System.Object.ReferenceEquals(left, right))
            {
                return false;
            }

            if (left is null || right is null)
            {
                return left is null;
            }

            return left.value < right.value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (global::System.Object.ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return right is null;
            }

            return left.value >= right.value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(global::TestProject.Points left, global::TestProject.Points right)
        {
            if (global::System.Object.ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return left is null;
            }

            return left.value <= right.value;
        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        private sealed class PointsTypeConverter : g__SCM.TypeConverter
        {
            private static readonly g__S.Type s_wrapperType = typeof(global::TestProject.Points);
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
                    if (t == typeof(global::TestProject.Points)) return (global::TestProject.Points)value;
                    if (t == typeof(int)) return new global::TestProject.Points((int)value);
                }
                return base.ConvertFrom(context, culture, value);
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public override object ConvertTo(g__SCM.ITypeDescriptorContext context, g__SG.CultureInfo culture, object value, g__S.Type destinationType)
            {
                if (value is global::TestProject.Points wrappedValue)
                {
                    if (destinationType == s_wrapperType) return wrappedValue;
                    if (destinationType == s_underlyingType) return wrappedValue.value;
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

    }
#region INTERNALS
#endregion ======

    partial class Points // Internals
    {
        private const string GENERATOR = "EncosyTower.Core.Generators.TypeWraps.TypeWrapGenerator";

    }



}

