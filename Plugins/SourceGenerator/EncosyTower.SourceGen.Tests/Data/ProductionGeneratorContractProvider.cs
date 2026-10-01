using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Data;

internal sealed class ProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    private const string TEST_PROJECT_NAMESPACE = "namespace TestProject;";
    private const string RELEVANT_PROJECT_NAMESPACE = "namespace RelevantProject;";

    private static readonly ProductionGeneratorContractCase[] s_contracts = {
        new(
              typeof(global::EncosyTower.Data.Generators.Data.DataGenerator)
            , "Data/DataGeneratorTests.cs"
            , "DataProperty_GeneratesDataContract"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
        ),
    };

    public string FeaturePath => "Data";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts => s_contracts;
}
