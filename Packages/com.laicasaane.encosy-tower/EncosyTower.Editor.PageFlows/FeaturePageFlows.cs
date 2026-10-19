#if UNITY_EDITOR

using EncosyTower.Editor.ProjectSetup;

namespace EncosyTower.Editor.PageFlows
{
    /// <summary>
    /// Registers the Page Flows entry in Project Setup.
    /// </summary>
    /// <remarks>
    /// The entry lists UniTask from OpenUPM as an optional package.
    /// </remarks>
    [Feature("8. EncosyTower: Page Flows")]
    [RequiresPackage(PackageRegistry.OpenUpm, "com.cysharp.unitask", isOptional: true)]
    internal readonly struct FeaturePageFlows { }
}

#endif
