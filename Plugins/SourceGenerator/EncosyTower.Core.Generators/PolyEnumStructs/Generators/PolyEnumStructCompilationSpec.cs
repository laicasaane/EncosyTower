namespace EncosyTower.Core.Generators.PolyEnumStructs
{
    internal readonly record struct PolyEnumStructCompilationSpec(
          CompilationSpec Compilation
        , bool EnableNullable
        , bool UnityCollections
    )
    {
        public static PolyEnumStructCompilationSpec Create(
              Compilation compilation
            , CancellationToken token
            , string generatorNamespace
            , string skipAttribute
        )
            => new(
                  CompilationSpec.Create(compilation, token, generatorNamespace, skipAttribute)
                , compilation.Options.NullableContextOptions != NullableContextOptions.Disable
                , compilation.HasReferencedAssembly("Unity.Collections", token)
            );
    }
}
