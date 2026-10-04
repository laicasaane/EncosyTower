namespace EncosyTower.PageFlows
{
    public readonly record struct PageFlowCodexValidation(
          int FirstEmptyIndex
        , string[] DuplicateIdentifiers
        , string[] DefinitionsWithoutScope
        , string[] ScopesWithoutDefinition
    )
    {
        public bool IsValid
            => FirstEmptyIndex < 0
            && DuplicateIdentifiers.Length == 0
            && DefinitionsWithoutScope.Length == 0
            && ScopesWithoutDefinition.Length == 0
            ;
    }
}
