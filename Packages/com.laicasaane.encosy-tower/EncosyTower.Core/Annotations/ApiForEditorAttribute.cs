using System;

namespace EncosyTower.Core
{
    /// <summary>
    /// The API marked with this attribute should take effect only in Editor context
    /// even though it is defined in non-Editor assemblies (such as EncosyTower.Core).
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class ApiForEditorAttribute : Attribute
    {
    }
}
