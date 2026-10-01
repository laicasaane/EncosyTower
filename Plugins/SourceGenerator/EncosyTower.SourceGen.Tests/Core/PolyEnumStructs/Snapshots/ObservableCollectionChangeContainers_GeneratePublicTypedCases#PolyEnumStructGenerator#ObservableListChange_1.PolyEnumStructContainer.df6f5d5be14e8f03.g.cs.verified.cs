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

    public static partial class ObservableListChange
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator";

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        public enum EnumCase : byte
        {
            Undefined = 0,

            Add = 1,

            AddRange = 2,

            Remove = 3,

            RemoveRange = 4,

            Replace = 5,

            ReplaceRange = 6,

            Move = 7,

            Clear = 8,

            Reset = 9,

        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        partial interface IEnumCase
        {
            EnumCase GetEnumCase();
        }

        partial record struct Add<T> : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public Add(
                  g__ET.Option<int> Index = default
                , g__ET.Option<T> Value = default
            ) : this(default(int), default(T))
            {
                this.Index = Index.GetValueOrDefault();
                this.Value = Value.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Add;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableListChange<T> ToObservableListChange()
            {
                return this;
            }
        }

        partial record struct AddRange : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public AddRange(
                  g__ET.Option<int> Index = default
                , g__ET.Option<int> Count = default
            ) : this(default(int), default(int))
            {
                this.Index = Index.GetValueOrDefault();
                this.Count = Count.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.AddRange;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableListChange<T> ToObservableListChange<T>()
            {
                return this;
            }
        }

        partial record struct Remove<T> : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public Remove(
                  g__ET.Option<int> Index = default
                , g__ET.Option<T> Value = default
            ) : this(default(int), default(T))
            {
                this.Index = Index.GetValueOrDefault();
                this.Value = Value.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Remove;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableListChange<T> ToObservableListChange()
            {
                return this;
            }
        }

        partial record struct RemoveRange : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public RemoveRange(
                  g__ET.Option<int> Index = default
                , g__ET.Option<int> Count = default
            ) : this(default(int), default(int))
            {
                this.Index = Index.GetValueOrDefault();
                this.Count = Count.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.RemoveRange;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableListChange<T> ToObservableListChange<T>()
            {
                return this;
            }
        }

        partial record struct Replace<T> : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public Replace(
                  g__ET.Option<int> Index = default
                , g__ET.Option<T> OldValue = default
                , g__ET.Option<T> NewValue = default
            ) : this(default(int), default(T), default(T))
            {
                this.Index = Index.GetValueOrDefault();
                this.OldValue = OldValue.GetValueOrDefault();
                this.NewValue = NewValue.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Replace;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableListChange<T> ToObservableListChange()
            {
                return this;
            }
        }

        partial record struct ReplaceRange : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public ReplaceRange(
                  g__ET.Option<int> Index = default
                , g__ET.Option<int> Count = default
            ) : this(default(int), default(int))
            {
                this.Index = Index.GetValueOrDefault();
                this.Count = Count.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.ReplaceRange;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableListChange<T> ToObservableListChange<T>()
            {
                return this;
            }
        }

        partial record struct Move<T> : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public Move(
                  g__ET.Option<int> PreviousIndex = default
                , g__ET.Option<int> Index = default
                , g__ET.Option<T> Value = default
            ) : this(default(int), default(int), default(T))
            {
                this.PreviousIndex = PreviousIndex.GetValueOrDefault();
                this.Index = Index.GetValueOrDefault();
                this.Value = Value.GetValueOrDefault();
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Move;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableListChange<T> ToObservableListChange()
            {
                return this;
            }
        }

        partial record struct Clear : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Clear;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableListChange<T> ToObservableListChange<T>()
            {
                return this;
            }
        }

        partial record struct Reset : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Reset;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableListChange<T> ToObservableListChange<T>()
            {
                return this;
            }
        }

        partial struct ObservableListChange_Undefined : IEnumCase
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly EnumCase GetEnumCase()
            {
                return EnumCase.Undefined;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly global::TestProject.ObservableListChange<T> ToObservableListChange<T>()
            {
                return this;
            }
        }

        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        internal static partial class EnumCaseAPI
        {
        }
    }
}
