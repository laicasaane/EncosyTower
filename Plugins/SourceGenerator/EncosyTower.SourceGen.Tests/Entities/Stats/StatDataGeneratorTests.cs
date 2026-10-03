using EncosyTower.Entities.Stats.Generators;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

[TestClass]
public class StatDataGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<StatDataGenerator>();

    [TestMethod]
    public Task FloatStat_GeneratesStatDataContract()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<StatDataGenerator>(
              """
              using EncosyTower.Entities.Stats;

              namespace TestProject;

              [StatData(StatVariantType.Float)]
              public partial struct Hp { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<StatDataGenerator>(
                    "Hp.StatData.b854d7ee040715f9.g.cs"
                ),
            }
        );

    [TestMethod]
    public Task NoneVariant_IsSkipped()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<StatDataGenerator>(
              """
              using EncosyTower.Entities.Stats;

              namespace TestProject;

              [StatData(StatVariantType.Float)]
              public partial struct Hp { }

              [StatData(StatVariantType.None)]
              public partial struct Nothing { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<StatDataGenerator>(
                      "Hp.StatData.b854d7ee040715f9.g.cs"
                    , testMethod: nameof(FloatStat_GeneratesStatDataContract)
                ),
            }
        );

    [DataTestMethod]
    [DataRow("public partial record struct Hp { }")]
    [DataRow("public partial record struct Hp(int Extra);")]
    [DataRow("public partial record struct Hp();")]
    [DataRow("public readonly partial record struct Hp { }")]
    [DataRow("public readonly partial struct Hp { }")]
    public Task UnsupportedTarget_IsSkipped(string declaration)
        => GeneratorTestHelper.VerifyNoOutputAsync<StatDataGenerator>(
              $$"""
              using EncosyTower.Entities.Stats;

              namespace TestProject;

              [StatData(StatVariantType.Float)]
              {{declaration}}
              """
        );
}
