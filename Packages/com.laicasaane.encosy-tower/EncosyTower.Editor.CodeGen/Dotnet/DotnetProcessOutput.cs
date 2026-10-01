#if UNITY_EDITOR

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Carries one line captured from a .NET process output stream.
    /// </summary>
    /// <param name="IsStandardError">Whether the line came from standard error.</param>
    /// <param name="Text">The captured output text.</param>
    internal readonly record struct DotnetProcessOutput(bool IsStandardError, string Text);
}

#endif
