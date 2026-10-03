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

    [g__SCM.TypeConverter(typeof(global::TestProject.Pair.PairTypeConverter))]
    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial record struct Pair : g__ETTW.IWrap<(int, int)>
        , g__S.IEquatable<global::TestProject.Pair>
        , g__S.IEquatable<(int, int)>
        , g__S.IComparable
        , g__S.IComparable<global::TestProject.Pair>
        , g__S.IComparable<(int, int)>
    {
        public int Item1
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => this.Value.Item1;
        }

        public int Item2
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => this.Value.Item2;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public int CompareTo((int, int) other)
            => this.Value.CompareTo(other);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool Equals((int, int) other)
            => this.Value.Equals(other);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(global::TestProject.Pair other)
            => this.Value.CompareTo(other.Value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(object obj)
            => obj switch
            {
                Pair other => CompareTo(other),
                global::System.ValueTuple<int, int> other => this.Value.CompareTo(other),
                _ => 1,
            };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(global::TestProject.Pair other)
            => this.Value.Equals(other.Value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => this.Value.GetHashCode();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => this.Value.ToString();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator global::TestProject.Pair((int, int) value)
            => new global::TestProject.Pair(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator (int, int)(global::TestProject.Pair value)
            => value.Value;

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        private sealed class PairTypeConverter : g__SCM.TypeConverter
        {
            private static readonly g__S.Type s_wrapperType = typeof(global::TestProject.Pair);
            private static readonly g__S.Type s_underlyingType = typeof((int, int));

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
                    if (t == typeof(global::TestProject.Pair)) return (global::TestProject.Pair)value;
                    if (t == typeof((int, int))) return new global::TestProject.Pair(((int, int))value);
                }
                return base.ConvertFrom(context, culture, value);
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public override object ConvertTo(g__SCM.ITypeDescriptorContext context, g__SG.CultureInfo culture, object value, g__S.Type destinationType)
            {
                if (value is global::TestProject.Pair wrappedValue)
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

    partial record struct Pair // Internals
    {
        private const string GENERATOR = "EncosyTower.Core.Generators.TypeWraps.TypeWrapGenerator";

    }



}

