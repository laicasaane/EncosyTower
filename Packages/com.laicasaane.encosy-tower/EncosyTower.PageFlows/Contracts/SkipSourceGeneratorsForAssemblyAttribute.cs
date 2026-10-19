using System;

namespace EncosyTower.PageFlows
{
    /// <summary>Makes the page flow source generator and analyzer skip the assembly it marks.</summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SkipSourceGeneratorsForAssemblyAttribute : Attribute { }
}
