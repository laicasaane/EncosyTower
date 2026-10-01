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
}
