namespace EncosyTower.Databases.Authoring.Generators
{
    internal readonly record struct DatabaseAuthoringCompilationSpec(
          CompilationSpec Compilation
        , bool DatabaseAuthoring
        , bool BakingSheet
    )
    {
        public static DatabaseAuthoringCompilationSpec Create(Compilation compilation, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (compilation == null)
            {
                return default;
            }

            return new DatabaseAuthoringCompilationSpec(
                  CompilationSpec.Create(compilation, token, Helpers.DATABASES_NAMESPACE, Helpers.SKIP_ATTRIBUTE)
                , compilation.HasReferencedAssembly(Helpers.DATABASES_AUTHORING_NAMESPACE, token)
                , compilation.HasReferencedAssembly("BakingSheet", token)
            );
        }
    }
}
