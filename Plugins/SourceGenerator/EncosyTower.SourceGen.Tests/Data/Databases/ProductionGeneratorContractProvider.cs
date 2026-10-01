using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Data.Databases;

internal sealed class ProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    private const string TEST_PROJECT_NAMESPACE = "namespace TestProject;";
    private const string RELEVANT_PROJECT_NAMESPACE = "namespace RelevantProject;";

    private static readonly ProductionGeneratorContractCase[] s_contracts = {
        new(
              typeof(global::EncosyTower.Data.Generators.Databases.DatabaseGenerator)
            , "Data/Databases/DatabaseGeneratorTests.cs"
            , "DatabaseTable_GeneratesDatabaseContract"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
        ),
    };

    public string FeaturePath => "Data/Databases";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts => s_contracts;
}
