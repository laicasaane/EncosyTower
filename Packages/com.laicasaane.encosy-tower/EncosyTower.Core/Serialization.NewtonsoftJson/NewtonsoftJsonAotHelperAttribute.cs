#if UNITY_NEWTONSOFT_JSON

using System;
using System.Diagnostics.CodeAnalysis;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Serialization.NewtonsoftJson
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class NewtonsoftJsonAotHelperAttribute : Attribute
    {
        public Type BaseType { get; }

        public NewtonsoftJsonAotHelperAttribute([NotNull] Type baseType)
        {
            DebuggingThrowHelper.ThrowIfNull(baseType);
            BaseType = baseType;
        }
    }
}

#endif
