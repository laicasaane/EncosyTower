#if UNITY_EDITOR

using System;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Reports a code generation failure with a stable diagnostic code.
    /// </summary>
    internal sealed class CodeGenRunException : Exception
    {
        /// <summary>
        /// Initializes an exception with a diagnostic code and message.
        /// </summary>
        public CodeGenRunException(string code, string message) : base(message)
        {
            Code = code;
        }

        /// <summary>
        /// Initializes an exception with a diagnostic code, message, and underlying failure.
        /// </summary>
        public CodeGenRunException(string code, string message, Exception innerException)
            : base(message, innerException)
        {
            Code = code;
        }

        /// <summary>
        /// Gets the stable code that identifies the failure category.
        /// </summary>
        public string Code { get; }
    }
}

#endif
