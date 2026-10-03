namespace EncosyTower.SourceGen
{
    /// <summary>
    /// One containing type declaration exactly as
    /// <see cref="TypeCreationHelpers.GenerateOpeningAndClosingSource"/> prints it:
    /// <c>partial {Keyword} {Name}{TypeParameters} {Constraints}</c>.
    /// </summary>
    public readonly record struct ContainingTypeSpec(
          string Keyword
        , string Name
        , string TypeParameters
        , string Constraints
    );
}
