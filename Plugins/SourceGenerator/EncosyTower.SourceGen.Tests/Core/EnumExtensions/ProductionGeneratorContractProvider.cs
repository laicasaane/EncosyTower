using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Core.EnumExtensions;

internal sealed class ProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    private const string TEST_PROJECT_NAMESPACE = "namespace TestProject;";
    private const string RELEVANT_PROJECT_NAMESPACE = "namespace RelevantProject;";

    private static readonly ProductionGeneratorContractCase[] s_contracts = {
        new(
              typeof(global::EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsForGenerator)
            , "Core/EnumExtensions/EnumExtensionsForGeneratorTests.cs"
            , "AnnotatedClass_GeneratesExtensions"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
        ),
        new(
              typeof(global::EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator)
            , "Core/EnumExtensions/EnumExtensionsGeneratorTests.cs"
            , "AnnotatedEnum_GeneratesExtensions"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
        ),
    };

    public string FeaturePath => "Core/EnumExtensions";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts => s_contracts;
}
