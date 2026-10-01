using System;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen
{
    public static class CompilationExtensions
    {
        public static bool HasReferencedAssembly(
              this Compilation compilation
            , string assemblyName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            foreach (var reference in compilation.ReferencedAssemblyNames)
            {
                token.ThrowIfCancellationRequested();

                if (string.Equals(reference.Name, assemblyName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasReferencedAssemblyPrefix(
              this Compilation compilation
            , string assemblyNamePrefix
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            foreach (var reference in compilation.ReferencedAssemblyNames)
            {
                token.ThrowIfCancellationRequested();

                if (reference.Name.StartsWith(assemblyNamePrefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
