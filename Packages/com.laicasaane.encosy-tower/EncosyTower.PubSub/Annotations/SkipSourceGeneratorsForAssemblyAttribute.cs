using System;

namespace EncosyTower.PubSub
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SkipSourceGeneratorsForAssemblyAttribute : Attribute { }
}
