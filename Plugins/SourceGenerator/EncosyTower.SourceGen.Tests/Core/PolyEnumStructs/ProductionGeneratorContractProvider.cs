using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Core.PolyEnumStructs;

internal sealed class ProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    private const string TEST_PROJECT_NAMESPACE = "namespace TestProject;";
    private const string RELEVANT_PROJECT_NAMESPACE = "namespace RelevantProject;";

    private static readonly ProductionGeneratorContractCase[] s_contracts = {
        new(
              typeof(global::EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator)
            , "Core/PolyEnumStructs/PolyEnumStructGeneratorTests.cs"
            , "GenericResult_WithContainerAndEnumExtensions_GeneratesExtensions"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
        ),
    };

    public string FeaturePath => "Core/PolyEnumStructs";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts => s_contracts;
}
