#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.PageFlows
{
    [Feature("8. EncosyTower: Page Flows")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask", isOptional: true)]
    internal readonly struct FeaturePageFlows { }
}

#endif
