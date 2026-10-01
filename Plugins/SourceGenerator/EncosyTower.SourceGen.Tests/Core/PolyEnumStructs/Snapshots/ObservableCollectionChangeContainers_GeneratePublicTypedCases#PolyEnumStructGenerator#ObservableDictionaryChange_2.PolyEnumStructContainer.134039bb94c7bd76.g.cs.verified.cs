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

    public static partial class ObservableDictionaryChange
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")]
        public enum EnumCase : byte
        {
            Undefined = 0,

            Add = 1,

            Remove = 2,

            Replace = 3,

            Clear = 4,

        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")]
        partial interface IEnumCase
        {
            EnumCase GetEnumCase();
        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")]
        partial interface IEnumCase<TKey, TValue> : IEnumCase
        {
        }

        partial record struct Add<TKey, TValue> : IEnumCase, IEnumCase<TKey, TValue>
        {
            [g__SRCS.MethodImpl(INLINING)]
            public Add(
                  g__ET.Option<TKey> Key = default
                , g__ET.Option<TValue> Value = default
            ) : this(default(TKey), default(TValue))
            {
                this.Key = Key.GetValueOrDefault();
                this.Value = Value.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Add;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableDictionaryChange<TKey, TValue> ToObservableDictionaryChange()
            {
                return this;
            }
        }

        partial record struct Remove<TKey, TValue> : IEnumCase, IEnumCase<TKey, TValue>
        {
            [g__SRCS.MethodImpl(INLINING)]
            public Remove(
                  g__ET.Option<TKey> Key = default
                , g__ET.Option<TValue> Value = default
            ) : this(default(TKey), default(TValue))
            {
                this.Key = Key.GetValueOrDefault();
                this.Value = Value.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Remove;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableDictionaryChange<TKey, TValue> ToObservableDictionaryChange()
            {
                return this;
            }
        }

        partial record struct Replace<TKey, TValue> : IEnumCase, IEnumCase<TKey, TValue>
        {
            [g__SRCS.MethodImpl(INLINING)]
            public Replace(
                  g__ET.Option<TKey> Key = default
                , g__ET.Option<TValue> OldValue = default
                , g__ET.Option<TValue> NewValue = default
            ) : this(default(TKey), default(TValue), default(TValue))
            {
                this.Key = Key.GetValueOrDefault();
                this.OldValue = OldValue.GetValueOrDefault();
                this.NewValue = NewValue.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Replace;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableDictionaryChange<TKey, TValue> ToObservableDictionaryChange()
            {
                return this;
            }
        }

        partial record struct Clear<TKey, TValue> : IEnumCase, IEnumCase<TKey, TValue>
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Clear;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableDictionaryChange<TKey, TValue> ToObservableDictionaryChange()
            {
                return this;
            }
        }

        partial struct ObservableDictionaryChange_Undefined<TKey, TValue> : IEnumCase, IEnumCase<TKey, TValue>
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Undefined;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableDictionaryChange<TKey, TValue> ToObservableDictionaryChange()
            {
                return this;
            }
        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        internal static partial class EnumCaseAPI
        {
        }
    }
}
