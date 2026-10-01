using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Core.Variants;

internal sealed class ProductionGeneratorContractProvider : IProductionGeneratorContractProvider
{
    private const string TEST_PROJECT_NAMESPACE = "namespace TestProject;";
    private const string RELEVANT_PROJECT_NAMESPACE = "namespace RelevantProject;";

    private static readonly ProductionGeneratorContractCase[] s_contracts = {
        new(
              typeof(global::EncosyTower.Core.Generators.Variants.InternalVariantGenerator)
            , "Core/Variants/VariantGeneratorTests.cs"
            , "Vector2ConverterCall_GeneratesInternalVariant"
            , "UnityEngine.Vector2"
            , "UnityEngine.Vector3"
        ),
        new(
              typeof(global::EncosyTower.Core.Generators.Variants.VariantRegistrationGenerator)
            , "Core/Variants/VariantGeneratorTests.cs"
            , "Vector3Variant_GeneratesStructAndRegistration"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
            , typeof(global::EncosyTower.Core.Generators.Variants.VariantStructGenerator)
        ),
        new(
              typeof(global::EncosyTower.Core.Generators.Variants.VariantStructGenerator)
            , "Core/Variants/VariantGeneratorTests.cs"
            , "Vector3Variant_GeneratesStructAndRegistration"
            , TEST_PROJECT_NAMESPACE
            , RELEVANT_PROJECT_NAMESPACE
            , typeof(global::EncosyTower.Core.Generators.Variants.VariantRegistrationGenerator)
        ),
    };

    public string FeaturePath => "Core/Variants";

    public IReadOnlyList<ProductionGeneratorContractCase> Contracts => s_contracts;
}
