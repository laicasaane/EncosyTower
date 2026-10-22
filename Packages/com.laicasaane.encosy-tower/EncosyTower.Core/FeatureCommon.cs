#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Common
{
    [Feature("0. EncosyTower: Common")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.burst", "1.8.31")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.collections", "2.7.1")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.mathematics", "1.3.3")]

    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask", isOptional: true)]
    internal readonly struct FeatureCommon { }
}

#endif
