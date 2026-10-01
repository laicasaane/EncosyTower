#if UNITY_EDITOR

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Core;

namespace EncosyTower.CodeGen
{
    [ApiForEditor]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class CodeGeneratorAttribute : Attribute
    {
        public CodeGeneratorAttribute([CallerFilePath] string filePath = "")
        {
            FilePath = filePath;
        }

        public string FilePath { get; }
    }
}

#endif
