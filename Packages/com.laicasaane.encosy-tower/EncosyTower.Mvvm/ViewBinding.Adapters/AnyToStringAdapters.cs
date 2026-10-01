using System;
using EncosyTower.Annotations;
using EncosyTower.Variants;
using UnityEngine.Scripting.APIUpdating;

namespace EncosyTower.Mvvm.ViewBinding.Adapters
{
    [Serializable]
    [MovedFrom(true, sourceNamespace: null, sourceAssembly: "EncosyTower.Core", sourceClassName: null)]
    [Label("String ⇒ String", "Default")]
    [Adapter(sourceType: typeof(string), destType: typeof(string), order: 0)]
    public sealed class StringToStringAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out string result))
            {
                return result;
            }

            return variant;
        }
    }

    [Serializable]
    [MovedFrom(true, sourceNamespace: null, sourceAssembly: "EncosyTower.Core", sourceClassName: null)]
    [Label("Object ⇒ String", "Default")]
    [Adapter(sourceType: typeof(object), destType: typeof(string), order: 0)]
    public sealed class ObjectToStringAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out object result))
            {
                return result.ToString();
            }

            return variant;
        }
    }
}
