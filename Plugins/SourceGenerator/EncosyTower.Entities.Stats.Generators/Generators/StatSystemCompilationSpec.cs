namespace EncosyTower.Entities.Stats.Generators
{
    internal readonly record struct StatSystemCompilationSpec(
          CompilationSpec Compilation
        , bool LatiosCore
    )
    {
        public static StatSystemCompilationSpec Create(
              Compilation compilation
            , CancellationToken token
            , string generatorNamespace
            , string skipAttribute
        )
            => new(
                  CompilationSpec.Create(compilation, token, generatorNamespace, skipAttribute)
                , compilation.HasReferencedAssembly("Latios.Core", token)
            );
    }
}
