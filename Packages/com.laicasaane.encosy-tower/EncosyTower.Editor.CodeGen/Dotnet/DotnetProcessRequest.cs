#if UNITY_EDITOR

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Defines one owned .NET process invocation and its observable artifacts.
    /// </summary>
    /// <param name="OperationName">The operation name recorded in the process manifest.</param>
    /// <param name="ExecutablePath">The executable to launch.</param>
    /// <param name="WorkingDirectory">The process working directory.</param>
    /// <param name="Arguments">The argument list passed without shell interpretation.</param>
    /// <param name="StandardOutputLogPath">The standard-output log destination.</param>
    /// <param name="StandardErrorLogPath">The standard-error log destination.</param>
    /// <param name="ProcessManifestPath">The ownership manifest used for the process tree.</param>
    internal sealed record class DotnetProcessRequest(
          string OperationName
        , string ExecutablePath
        , string WorkingDirectory
        , string[] Arguments
        , string StandardOutputLogPath
        , string StandardErrorLogPath
        , string ProcessManifestPath
    );
}

#endif
