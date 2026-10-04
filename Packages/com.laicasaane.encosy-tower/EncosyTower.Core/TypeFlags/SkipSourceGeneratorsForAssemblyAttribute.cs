using System;

namespace EncosyTower.TypeFlags
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SkipSourceGeneratorsForAssemblyAttribute : Attribute { }
}
