using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Core.PolyEnumFactories;

internal sealed class ProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    private const string TEST_PROJECT_NAMESPACE = "namespace TestProject;";
    private const string RELEVANT_PROJECT_NAMESPACE = "namespace RelevantProject;";

    private static readonly ProductionGeneratorContractCase[] s_contracts = {
        new(
              typeof(global::EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator)
            , "Core/PolyEnumFactories/PolyEnumFactoryGeneratorTests.cs"
            , "GenericErrorFactory_BindsTargetPositionally"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
            , typeof(global::EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator)
        ),
    };

    public string FeaturePath => "Core/PolyEnumFactories";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts => s_contracts;
}
