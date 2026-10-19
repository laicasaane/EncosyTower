using EncosyTower.Common;

namespace EncosyTower.PageFlows.Codex
{
    /// <summary>The result of matching the flow definitions of a codex against its scope collection.</summary>
    /// <param name="FirstEmptyIndex">The index of the first definition with a null or empty identifier, or -1.</param>
    /// <param name="DuplicateIdentifiers">The identifiers that more than one definition uses.</param>
    /// <param name="DefinitionsWithoutScope">The definition identifiers that no scope property has.</param>
    /// <param name="ScopesWithoutDefinition">The scope property identifiers that no definition has.</param>
    public readonly record struct PageFlowCodexValidation(
          int FirstEmptyIndex
        , string[] DuplicateIdentifiers
        , string[] DefinitionsWithoutScope
        , string[] ScopesWithoutDefinition
    ) : IIsValid
    {
        /// <summary>Gets whether the definitions and the scope properties match one to one.</summary>
        public bool IsValid
            => FirstEmptyIndex < 0
            && DuplicateIdentifiers.Length == 0
            && DefinitionsWithoutScope.Length == 0
            && ScopesWithoutDefinition.Length == 0
            ;
    }
}
