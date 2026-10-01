using System;

namespace EncosyTower.Processing
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SkipSourceGeneratorsForAssemblyAttribute : Attribute { }
}
