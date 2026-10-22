#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.Entities
{
    [Feature("9. EncosyTower: Entities Stats")]
    [RequiresPackage(PackageRegistry.Unity, "com.unity.entities", "1.5.0")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.latios.latiosframework", "0.16.3", isOptional: true)]
    internal readonly struct FeatureEntitiesStats { }
}

#endif
