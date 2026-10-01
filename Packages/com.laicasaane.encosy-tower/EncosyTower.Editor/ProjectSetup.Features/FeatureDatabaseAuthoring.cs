#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Databases.Authoring
{
    [Feature("5. EncosyTower: Databases Authoring")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.editorcoroutines")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.laicasaane.bakingsheet")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask")]
    internal readonly struct FeatureData { }
}

#endif
