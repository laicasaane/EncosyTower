#if UNITY_EDITOR

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Reports the identity and exit status of a completed .NET process.
    /// </summary>
    /// <param name="ProcessId">The operating-system process identifier.</param>
    /// <param name="ExitCode">The process exit code.</param>
    internal readonly record struct DotnetProcessResult(int ProcessId, int ExitCode);
}

#endif
