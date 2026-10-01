using EncosyTower.Entities.Stats.Generators;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

[TestClass]
public class StatCollectionGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<StatCollectionGenerator>();

    [TestMethod]
    public Task CollectionWithNestedStat_GeneratesAllStatsContracts()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<StatCollectionGenerator>(
              """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              [StatCollection(typeof(StatsApi), 1000)]
              public partial struct Stats : IComponentData
              {
                  [StatData(StatVariantType.Float)]
                  public partial struct Hp { }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<StatCollectionGenerator>(
                    "Stats.StatCollection.664f25990c3c4b5a.g.cs"
                ),
                ExpectedGeneratedSource.Create<StatDataGenerator>(
                    "Hp.StatData.b2ac3783b93b6fc7.g.cs"
                ),
                ExpectedGeneratedSource.Create<StatSystemGenerator>(
                    "StatsApi.StatSystem.abfa7bf43d46f7f6.g.cs"
                ),
            }
            , new IIncrementalGenerator[] {
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
        );
}
