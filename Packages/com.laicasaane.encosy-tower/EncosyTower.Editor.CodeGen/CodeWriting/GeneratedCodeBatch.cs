#if UNITY_EDITOR

using EncosyTower.CodeGen;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Carries all outputs and diagnostics produced by one backend run.
    /// </summary>
    /// <param name="GeneratedCodes">The generated files to validate and apply.</param>
    /// <param name="Diagnostics">The diagnostics emitted during the run.</param>
    /// <param name="AllCandidatesSkipped">
    /// Whether discovery found candidates but none could be executed.
    /// </param>
    internal readonly record struct GeneratedCodeBatch(
          GeneratedCode[] GeneratedCodes
        , CodeGenDiagnostic[] Diagnostics
        , bool AllCandidatesSkipped
    );
}

#endif
