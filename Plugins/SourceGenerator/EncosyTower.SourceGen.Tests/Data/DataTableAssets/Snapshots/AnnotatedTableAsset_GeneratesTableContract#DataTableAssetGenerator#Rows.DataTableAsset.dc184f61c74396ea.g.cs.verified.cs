#pragma warning disable 0219

using EncosyTower.Data;
using EncosyTower.Databases;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SRIS = global::System.Runtime.InteropServices;
using g__ETDB = EncosyTower.Databases;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

partial class Rows
{
    [g__SCDC.GeneratedCode("EncosyTower.Data.Generators.DataTableAssets.DataTableAssetGenerator", "0.1.8-preview.1")]
    public const string NAME = nameof(Rows);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.DataTableAssets.DataTableAssetGenerator", "0.1.8-preview.1")]
    protected sealed override int GetId(in global::TestProject.Row entry)
    {
        return entry.Id;
    }

}


}

