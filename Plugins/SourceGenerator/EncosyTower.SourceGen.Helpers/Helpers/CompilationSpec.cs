using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen
{
    public readonly record struct CompilationSpec(string AssemblyName, bool IsValid)
    {
        public static CompilationSpec Create(
              Compilation compilation
            , CancellationToken token
            , string generatorNamespace
            , string skipAttribute
        )
            => new(compilation.Assembly.Name, compilation.IsValidCompilation(token, generatorNamespace, skipAttribute));
    }
}
