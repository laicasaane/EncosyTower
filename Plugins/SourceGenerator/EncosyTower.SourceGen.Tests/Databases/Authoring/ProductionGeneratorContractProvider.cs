using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Databases.Authoring;

internal sealed class ProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    private const string TEST_PROJECT_NAMESPACE = "namespace TestProject;";
    private const string RELEVANT_PROJECT_NAMESPACE = "namespace RelevantProject;";

    private static readonly ProductionGeneratorContractCase[] s_contracts = {
        new(
              typeof(global::EncosyTower.Databases.Authoring.Generators.DatabaseAuthoringGenerator)
            , "Databases/Authoring/DatabaseAuthoringGeneratorTests.cs"
            , "ValidDatabase_GeneratesSheetContainerAndSheet"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
            , typeof(global::EncosyTower.Data.Generators.Data.DataGenerator)
        ),
    };

    public string FeaturePath => "Databases/Authoring";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts => s_contracts;
}
