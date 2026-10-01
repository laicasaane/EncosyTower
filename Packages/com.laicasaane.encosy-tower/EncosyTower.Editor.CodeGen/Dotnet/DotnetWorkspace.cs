#if UNITY_EDITOR

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Describes a prepared temporary workspace and the process that executes it.
    /// </summary>
    /// <param name="RootPath">The fixed workspace root.</param>
    /// <param name="SelectedSdkVersion">The pinned stable .NET SDK version.</param>
    /// <param name="PrepareRequest">The request that starts the generated Runner.</param>
    /// <param name="ResultManifestPath">
    /// The aggregate result manifest to read after execution.
    /// </param>
    /// <param name="ProcessManifestPath">The process-tree ownership manifest.</param>
    internal sealed record class DotnetWorkspace(
          string RootPath
        , string SelectedSdkVersion
        , DotnetProcessRequest PrepareRequest
        , string ResultManifestPath
        , string ProcessManifestPath
    );
}

#endif
