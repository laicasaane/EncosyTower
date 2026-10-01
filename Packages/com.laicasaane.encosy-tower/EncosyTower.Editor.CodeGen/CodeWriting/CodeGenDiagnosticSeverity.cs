#if UNITY_EDITOR

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Specifies how a code generation diagnostic is reported.
    /// </summary>
    internal enum CodeGenDiagnosticSeverity
    {
        /// <summary>
        /// Reports informational context.
        /// </summary>
        Info = 0,

        /// <summary>
        /// Reports a recoverable concern.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Reports a generator or contract failure.
        /// </summary>
        Error = 2,
    }
}

#endif
