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

    [g__SCM.TypeConverter(typeof(global::TestProject.Counted.CountedTypeConverter))]
    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial class Counted : g__ETTW.IWrap<global::TestProject.Counter>
    {
        public readonly global::TestProject.Counter value;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public Counted(global::TestProject.Counter value)
        {
            this.value = value;
        }

        public int Count
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => this.value.Count;
        }

        public int Step
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => this.value.Step;

        }

        public int this[int index]
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => this.value[index];

        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => this.value.GetHashCode();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => this.value.ToString();

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static explicit operator global::TestProject.Counted(global::TestProject.Counter value)
            => new global::TestProject.Counted(value);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator global::TestProject.Counter(global::TestProject.Counted value)
        {
            if (value is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(value));
            }

            return value.value;
        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        private sealed class CountedTypeConverter : g__SCM.TypeConverter
        {
            private static readonly g__S.Type s_wrapperType = typeof(global::TestProject.Counted);
            private static readonly g__S.Type s_underlyingType = typeof(global::TestProject.Counter);

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
                    if (t == typeof(global::TestProject.Counted)) return (global::TestProject.Counted)value;
                    if (t == typeof(global::TestProject.Counter)) return new global::TestProject.Counted((global::TestProject.Counter)value);
                }
                return base.ConvertFrom(context, culture, value);
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public override object ConvertTo(g__SCM.ITypeDescriptorContext context, g__SG.CultureInfo culture, object value, g__S.Type destinationType)
            {
                if (value is global::TestProject.Counted wrappedValue)
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

    partial class Counted // Internals
    {
        private const string GENERATOR = "EncosyTower.Core.Generators.TypeWraps.TypeWrapGenerator";

    }



}

