namespace EncosyTower.SourceGen.Tests.Helpers;

internal interface IProductionGeneratorContractProvider
{
    string FeaturePath { get; }

    IReadOnlyList<ProductionGeneratorContractCase> Contracts { get; }
}
