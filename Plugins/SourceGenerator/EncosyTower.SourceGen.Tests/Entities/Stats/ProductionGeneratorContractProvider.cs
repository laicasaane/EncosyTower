using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

internal sealed class ProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    private const string TEST_PROJECT_NAMESPACE = "namespace TestProject;";
    private const string RELEVANT_PROJECT_NAMESPACE = "namespace RelevantProject;";

    private static readonly ProductionGeneratorContractCase[] s_contracts = {
        new(
              typeof(global::EncosyTower.Entities.Stats.Generators.StatCollectionGenerator)
            , "Entities/Stats/StatCollectionGeneratorTests.cs"
            , "CollectionWithNestedStat_GeneratesAllStatsContracts"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
            , typeof(global::EncosyTower.Entities.Stats.Generators.StatDataGenerator)
            , typeof(global::EncosyTower.Entities.Stats.Generators.StatSystemGenerator)
        ),
        new(
              typeof(global::EncosyTower.Entities.Stats.Generators.StatDataGenerator)
            , "Entities/Stats/StatDataGeneratorTests.cs"
            , "FloatStat_GeneratesStatDataContract"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
        ),
        new(
              typeof(global::EncosyTower.Entities.Stats.Generators.StatSystemGenerator)
            , "Entities/Stats/StatSystemGeneratorTests.cs"
            , "Size8System_GeneratesStatSystemContract"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
        ),
    };

    public string FeaturePath => "Entities/Stats";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts => s_contracts;
}
