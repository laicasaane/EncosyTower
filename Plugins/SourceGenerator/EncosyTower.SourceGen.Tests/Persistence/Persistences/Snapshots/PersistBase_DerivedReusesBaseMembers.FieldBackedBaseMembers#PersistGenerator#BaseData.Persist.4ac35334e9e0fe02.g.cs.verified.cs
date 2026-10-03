#pragma warning disable 0219

using EncosyTower.Persistences;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ETUV = global::EncosyTower.Persistences;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

    [g__SCDC.GeneratedCode("EncosyTower.Persistence.Generators.PersistGenerator", "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial class BaseData : g__ETUV.IPersist
    {
        public string Id
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => this._id;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            set => this._id = value;
        }

        public int Version
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => this._version;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            set => this._version = value;
        }
    }


}

