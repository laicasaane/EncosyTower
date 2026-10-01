#pragma warning disable 0219

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ETA = global::EncosyTower.Annotations;
using g__ETMVB = global::EncosyTower.Mvvm.ViewBinding;
using g__ETV = global::EncosyTower.Variants;
using g__ETVC = global::EncosyTower.Variants.Converters;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace



#pragma warning disable

namespace EncosyTower.Mvvm.ViewBinding.__InternalStringAdapters.EncosyTower_SourceGen_Tests_Input
{
    [g__ETMVB.Adapter(sourceType: typeof(global::System.Collections.Generic.List<int>), destType: typeof(string), order: 1)]
    [g__ETA.Label("System.Collections.Generic.List<int> ⇒ String", "Generated/System.Collections.Generic")]
    [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.InternalStringAdapters.InternalStringAdapterGenerator", "0.1.7-preview.3")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public sealed class ListᐸintᐳToStringAdapter : g__ETMVB.IAdapter
    {
        private readonly g__ETVC.CachedVariantConverter<global::System.Collections.Generic.List<int>> _converter = g__ETVC.CachedVariantConverter<global::System.Collections.Generic.List<int>>.Default;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public g__ETV.Variant Convert(in g__ETV.Variant variant)
        {
            return this._converter.ToString(variant);
        }

    }

}


