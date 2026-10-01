#if UNITY_EDITOR

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Describes a diagnostic emitted while discovering or running a code generator.
    /// </summary>
    /// <param name="Severity">The diagnostic severity.</param>
    /// <param name="Code">The stable diagnostic identifier.</param>
    /// <param name="Message">The human-readable diagnostic message.</param>
    /// <param name="FilePath">The related source path, or an empty string.</param>
    /// <param name="Line">The one-based source line, or zero when unavailable.</param>
    /// <param name="Column">The one-based source column, or zero when unavailable.</param>
    internal readonly record struct CodeGenDiagnostic(
          CodeGenDiagnosticSeverity Severity
        , string Code
        , string Message
        , string FilePath
        , int Line
        , int Column
    );
}

#endif
