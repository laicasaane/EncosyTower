namespace EncosyTower.Core.Generators.EnumTemplates
{
    internal readonly record struct EnumTemplateCompilationSpec(
          CompilationSpec Compilation
        , bool UnityCollections
    )
    {
        public static EnumTemplateCompilationSpec Create(
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
