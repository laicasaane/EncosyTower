using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Core.Types.Caches;

internal sealed class ProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    private const string TEST_PROJECT_NAMESPACE = "namespace TestProject;";
    private const string RELEVANT_PROJECT_NAMESPACE = "namespace RelevantProject;";

    private static readonly ProductionGeneratorContractCase[] s_contracts = {
        new(
              typeof(global::EncosyTower.Core.Generators.Types.Caches.RuntimeTypeCachesGenerator)
            , "Core/Types/Caches/RuntimeTypeCachesGeneratorTests.cs"
            , "RuntimeTypeCacheCalls_GenerateCaches"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
        ),
    };

    public string FeaturePath => "Core/Types/Caches";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts => s_contracts;
}
