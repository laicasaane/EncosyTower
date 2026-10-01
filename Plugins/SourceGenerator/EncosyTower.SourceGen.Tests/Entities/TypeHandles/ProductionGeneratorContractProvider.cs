using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Entities.TypeHandles;

internal sealed class ProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    private const string TEST_PROJECT_NAMESPACE = "namespace TestProject;";
    private const string RELEVANT_PROJECT_NAMESPACE = "namespace RelevantProject;";

    private static readonly ProductionGeneratorContractCase[] s_contracts = {
        new(
              typeof(global::EncosyTower.Entities.Generators.Entities.TypeHandles.TypeHandleGenerator)
            , "Entities/TypeHandles/TypeHandleGeneratorTests.cs"
            , "ReadOnlyComponentTypeHandle_GeneratesTypeHandleContract"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
        ),
    };

    public string FeaturePath => "Entities/TypeHandles";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts => s_contracts;
}
