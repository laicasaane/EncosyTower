#pragma warning disable 0219

using EncosyTower.Data;
using EncosyTower.Databases;
using EncosyTower.Databases.Authoring;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCG = global::System.Collections.Generic;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SRIS = global::System.Runtime.InteropServices;
using g__ET = global::EncosyTower.Common;
using g__ETCE = global::EncosyTower.Collections.Extensions;
using g__ETD = global::EncosyTower.Data;
using g__ETDA = global::EncosyTower.Data.Authoring;
using g__ETDSG = global::EncosyTower.Data.SourceGen;
using g__ETI = global::EncosyTower.Initialization;
using g__UE = global::UnityEngine;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

    [g__S.Serializable]
    partial struct Loc : g__ETD.IData, g__ETD.IReadOnlyData<Loc>, g__ETD.IDataWithId<int>, g__ETD.IDataWithReadOnlyView<Loc>, g__S.IEquatable<Loc>
    {
        [g__UE.SerializeField]
        [g__ETDSG.GeneratedFieldFromProperty(nameof(Id))][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private int _id;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private readonly int Get_Id()
        {
            return (this._id);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private void Set_Id(int value)
        {
            this._id = (value);
        }

        [g__UE.SerializeField]
        [g__ETDSG.GeneratedFieldFromProperty(nameof(Points))][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private int[] _points;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private readonly int[] Get_Points()
        {
            return (this._points);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private void Set_Points(int[] value)
        {
            this._points = (int[])(value);
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public override readonly int GetHashCode()
        {
            var hash = GetHashCodeInternal();
            return hash.ToHashCode();
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private readonly g__ET.HashValue GetHashCodeInternal()
        {
            var hash = new g__ET.HashValue();
            hash.Add((_id));
            hash.AddEach(g__ETCE.EncosyArrayExtensions.AsReadOnlySpan(_points));
            return hash;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public override readonly bool Equals(object obj)
        {
            return obj is Loc other && Equals(other);
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public readonly bool Equals(Loc other)
        {
            return
                   (this._id == other._id)
                && g__S.MemoryExtensions.SequenceEqual(g__ETCE.EncosyArrayExtensions.AsReadOnlySpan(this._points), g__ETCE.EncosyArrayExtensions.AsReadOnlySpan(other._points))
            ;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public readonly Loc AsReadOnly()
        {
            return this;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public static bool operator ==(in Loc left, in Loc right)
        {
            return left.Equals(right);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public static bool operator !=(in Loc left, in Loc right)
        {
            return !left.Equals(right);
        }

        [g__S.Obsolete("This method is not intended to be used directly by user code.")]
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        internal static class I_TestProject_x002ELoc_ValueSetter
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static void Set_Id(ref Loc @ref, int value_Id)
                => @ref._id = value_Id;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static void Set_Points(ref Loc @ref, int[] value_Points)
                => @ref._points = value_Points;

        }

    }



}

