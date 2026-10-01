using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Persistence.Persistences;

internal sealed class ProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    private const string TEST_PROJECT_NAMESPACE = "namespace TestProject;";
    private const string RELEVANT_PROJECT_NAMESPACE = "namespace RelevantProject;";

    private static readonly ProductionGeneratorContractCase[] s_contracts = {
        new(
              typeof(global::EncosyTower.Persistence.Generators.PersistGenerator)
            , "Persistence/Persistences/PersistenceGeneratorTests.cs"
            , "PersistAndPersistence_GenerateCleanCombinedOutput"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
            , typeof(global::EncosyTower.Persistence.Generators.PersistenceGenerator)
        ),
        new(
              typeof(global::EncosyTower.Persistence.Generators.PersistenceGenerator)
            , "Persistence/Persistences/PersistenceGeneratorTests.cs"
            , "PersistenceWithAccessor_GeneratesVaultAndAccessor"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
        ),
    };

    public string FeaturePath => "Persistence/Persistences";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts => s_contracts;
}
