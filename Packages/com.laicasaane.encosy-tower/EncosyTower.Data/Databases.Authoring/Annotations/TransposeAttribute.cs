using System;
using System.Diagnostics;
using EncosyTower.Core;

namespace EncosyTower.Databases.Authoring
{
    [ApiForAuthoring]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR"), Conditional("ENCOSY_INCLUDE_AUTHORING")]
    public sealed class TransposeAttribute : Attribute
    {
    }
}
