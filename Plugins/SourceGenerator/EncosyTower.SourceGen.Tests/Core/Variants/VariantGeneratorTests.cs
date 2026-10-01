using EncosyTower.Core.Generators.Variants;
using CoreInternalVariantGenerator = EncosyTower.Core.Generators.Variants.InternalVariantGenerator;

namespace EncosyTower.SourceGen.Tests.Core.Variants;

[TestClass]
public class VariantGeneratorTests
{
    [TestMethod]
    public Task VariantStructGenerator_EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<VariantStructGenerator>();

    [TestMethod]
    public Task VariantRegistrationGenerator_EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<VariantRegistrationGenerator>();

    [TestMethod]
    public Task InternalVariantGenerator_EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<CoreInternalVariantGenerator>();

    [TestMethod]
    public Task Vector3Variant_GeneratesStructAndRegistration()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<VariantStructGenerator>(
              """
              using EncosyTower.Variants;

              namespace TestProject;

              [Variant(typeof(UnityEngine.Vector3))]
              public readonly partial struct Vector3Variant { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<VariantStructGenerator>(
                    "Vector3Variant.VariantStruct.8ab494a8af038aaa.g.cs"
                ),
                ExpectedGeneratedSource.Create<VariantRegistrationGenerator>(
                    "Input.VariantRegistration.7f31fc048cf62376.g.cs"
                ),
            }
            , new IIncrementalGenerator[] { new VariantRegistrationGenerator() }
        );

    [TestMethod]
    public Task Vector2ConverterCall_GeneratesInternalVariant()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<CoreInternalVariantGenerator>(
              """
              using EncosyTower.Variants;

              namespace TestProject;

              public sealed class Usage
              {
                  public void Execute()
                  {
                      var converter = Variant<UnityEngine.Vector2>.GetConverter();
                  }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<CoreInternalVariantGenerator>(
                    "Vector2.InternalVariant.fb71f2a1d4ae79df.g.cs"
                ),
                ExpectedGeneratedSource.Create<CoreInternalVariantGenerator>(
                    "Input.InternalVariantRegistry.4c1f1240dde07d8c.g.cs"
                ),
            }
        );
}
