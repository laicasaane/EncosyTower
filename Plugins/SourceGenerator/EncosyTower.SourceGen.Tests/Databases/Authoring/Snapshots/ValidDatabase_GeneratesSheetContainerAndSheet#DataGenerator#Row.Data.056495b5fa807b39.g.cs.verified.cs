#pragma warning disable 0219

using EncosyTower.Collections;
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
    partial struct Row : g__ETD.IData, g__ETD.IReadOnlyData<Row>, g__ETD.IDataWithId<int>, g__ETD.IDataWithReadOnlyView<Row>, g__S.IEquatable<Row>
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
        [g__ETDSG.GeneratedFieldFromProperty(nameof(Items))][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private global::System.Collections.Generic.List<int> _items;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private readonly global::EncosyTower.Collections.ListFast<int>.ReadOnly Get_Items()
        {
            return (this._items);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private void Set_Items(global::System.Collections.Generic.List<int> value)
        {
            this._items = (global::System.Collections.Generic.List<int>)(value);
        }

        [g__UE.SerializeField]
        [g__ETDSG.GeneratedFieldFromProperty(nameof(Tags))][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private global::System.Collections.Generic.HashSet<int> _tags;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private readonly global::EncosyTower.Collections.HashSetReadOnly<int> Get_Tags()
        {
            return (this._tags);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private void Set_Tags(global::System.Collections.Generic.HashSet<int> value)
        {
            this._tags = (global::System.Collections.Generic.HashSet<int>)(value);
        }

        [g__UE.SerializeField]
        [g__ETDSG.GeneratedFieldFromProperty(nameof(Names))][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private global::System.Collections.Generic.Dictionary<int, string> _names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private readonly global::EncosyTower.Collections.DictionaryReadOnly<int, string> Get_Names()
        {
            return (this._names);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        private void Set_Names(global::System.Collections.Generic.Dictionary<int, string> value)
        {
            this._names = (global::System.Collections.Generic.Dictionary<int, string>)(value);
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
            hash.AddEach(g__ETCE.EncosyListExtensions.AsReadOnlySpan(_items));
            hash.AddEach((_tags));
            hash.AddEach((_names));
            return hash;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public override readonly bool Equals(object obj)
        {
            return obj is Row other && Equals(other);
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public readonly bool Equals(Row other)
        {
            return
                   (this._id == other._id)
                && g__S.MemoryExtensions.SequenceEqual(g__ETCE.EncosyListExtensions.AsReadOnlySpan(this._items), g__ETCE.EncosyListExtensions.AsReadOnlySpan(other._items))
                && g__ETCE.EncosyHashSetExtenions.Overlaps(this._tags, other._tags)
                && g__ETCE.EncosyDictionaryExtensions.Overlaps(this._names, other._names)
            ;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public readonly Row AsReadOnly()
        {
            return this;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public static bool operator ==(in Row left, in Row right)
        {
            return left.Equals(right);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        public static bool operator !=(in Row left, in Row right)
        {
            return !left.Equals(right);
        }

        [g__S.Obsolete("This method is not intended to be used directly by user code.")]
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Data.DataGenerator", "0.1.8-preview.1")]
        internal static class I_TestProject_x002ERow_ValueSetter
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static void Set_Id(ref Row @ref, int value_Id)
                => @ref._id = value_Id;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static void Set_Items(ref Row @ref, global::System.Collections.Generic.List<int> value_Items)
                => @ref._items = value_Items;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static void Set_Tags(ref Row @ref, global::System.Collections.Generic.HashSet<int> value_Tags)
                => @ref._tags = value_Tags;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static void Set_Names(ref Row @ref, global::System.Collections.Generic.Dictionary<int, string> value_Names)
                => @ref._names = value_Names;

        }

    }



}

