namespace EncosyTower.Core.Generators.EnumExtensions
{
    internal readonly record struct EnumExtensionCompilationSpec(
          CompilationSpec Compilation
        , bool UnityCollections
    )
    {
        public static EnumExtensionCompilationSpec Create(
              Compilation compilation
            , CancellationToken token
            , string generatorNamespace
            , string skipAttribute
        )
            => new(
                  CompilationSpec.Create(compilation, token, generatorNamespace, skipAttribute)
                , compilation.HasReferencedAssembly("Unity.Collections", token)
            );
    }
}
