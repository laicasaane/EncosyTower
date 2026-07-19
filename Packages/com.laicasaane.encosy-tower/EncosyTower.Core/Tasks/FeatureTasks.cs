#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.Tasks
{
    [Feature("7. EncosyTower: Tasks")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask", isOptional: true)]
    internal readonly struct FeatureTasks { }
}

#endif
