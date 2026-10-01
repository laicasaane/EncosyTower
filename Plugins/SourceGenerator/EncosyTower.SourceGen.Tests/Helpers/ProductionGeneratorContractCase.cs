namespace EncosyTower.SourceGen.Tests;

internal readonly record struct ProductionGeneratorExpectedOutput(
    Type GeneratorType,
    string HintName,
    string Source
);

public sealed class ProductionGeneratorContractCase
{
    internal ProductionGeneratorContractCase(
          Type generatorType
        , string fixturePath
        , string fixtureMethod
        , string relevantEditOldValue
        , string relevantEditNewValue
        , params Type[] additionalGeneratorTypes
    )
    {
        GeneratorType = generatorType;
        FixturePath = fixturePath;
        FixtureMethod = fixtureMethod;
        RelevantEditOldValue = relevantEditOldValue;
        RelevantEditNewValue = relevantEditNewValue;
        AdditionalGeneratorTypes = additionalGeneratorTypes;
    }

    internal Type GeneratorType { get; }

    internal string FixturePath { get; }

    internal string FixtureMethod { get; }

    internal string RelevantEditOldValue { get; }

    internal string RelevantEditNewValue { get; }

    internal IReadOnlyList<Type> AdditionalGeneratorTypes { get; }

    public override string ToString()
        => GeneratorType.FullName ?? GeneratorType.Name;
}
