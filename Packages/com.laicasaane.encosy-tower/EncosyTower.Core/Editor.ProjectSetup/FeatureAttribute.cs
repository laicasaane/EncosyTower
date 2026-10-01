#if UNITY_EDITOR

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Editor.ProjectSetup
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class FeatureAttribute : Attribute
    {
        public string Name { get; }

        public FeatureAttribute([NotNull] string name)
        {
            DebuggingThrowHelper.ThrowIfNull(name);
            Name = name;
        }
    }
}

#endif
