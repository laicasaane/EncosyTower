#pragma warning disable 0219

using EncosyTower.Data;

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
    partial class Row : g__ETD.IData, g__ETD.IReadOnlyData<Row>, g__ETD.IDataWithId<int>, g__ETD.IDataWithReadOnlyView<Row>, g__S.IEquatable<Row>
    {
        [global::UnityEngine.SerializeField()]
        [g__ETDSG.GeneratedFieldFromProperty(nameof(Id))][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private int _id;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private int Get_Id()
        {
            return (this._id);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private void Set_Id(int value)
        {
            this._id = (value);
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public override int GetHashCode()
        {
            var hash = GetHashCodeInternal();
            return hash.ToHashCode();
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        protected virtual g__ET.HashValue GetHashCodeInternal()
        {
            var hash = new g__ET.HashValue();
            hash.Add((_id));
            return hash;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public override bool Equals(object obj)
        {
            return obj is Row other && Equals(other);
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public virtual bool Equals(Row other)
        {
            if (ReferenceEquals(other, null)) return false;

            if (ReferenceEquals(this, other)) return true;
            return EqualsInternal(other);
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        protected bool EqualsInternal(Row other)
        {
            return
                   (this._id == other._id)
            ;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public Row AsReadOnly()
        {
            return this;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public static bool operator ==(Row left, Row right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public static bool operator !=(Row left, Row right)
        {
            if (ReferenceEquals(left, null))
            {
                return !ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }

        [g__S.Obsolete("This method is not intended to be used directly by user code.")]
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        internal static class I_TestProject_x002ERow_ValueSetter
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static void Set_Id(Row @ref, int value_Id)
                => @ref._id = value_Id;

        }

    }



}

