namespace EncosyTower.Core.Generators.UnionIds
{
    internal readonly record struct UnionIdCompilationSpec(
          CompilationSpec Compilation
        , bool EnableNullable
        , bool Odin
        , bool Unity
        , bool UnityCollections
    )
    {
        public static UnionIdCompilationSpec Create(
              Compilation compilation
            , CancellationToken token
            , string generatorNamespace
            , string skipAttribute
        )
            => new(
                  CompilationSpec.Create(compilation, token, generatorNamespace, skipAttribute)
                , compilation.Options.NullableContextOptions != NullableContextOptions.Disable
                , compilation.HasReferencedAssembly("Sirenix.OdinInspector.Attributes", token)
                , compilation.HasReferencedAssemblyPrefix("UnityEngine", token)
                , compilation.HasReferencedAssembly("Unity.Collections", token)
            );
    }
}
