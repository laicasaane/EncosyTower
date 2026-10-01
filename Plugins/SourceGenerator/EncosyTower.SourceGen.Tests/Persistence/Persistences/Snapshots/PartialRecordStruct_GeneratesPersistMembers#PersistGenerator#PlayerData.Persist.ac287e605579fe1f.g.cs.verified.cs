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

    [g__SCDC.GeneratedCode("EncosyTower.Persistence.Generators.PersistGenerator", "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
    partial record struct PlayerData : g__ETUV.IPersist
    {
        public string Id { get; set; }

        public int Version { get; set; }
    }


}

