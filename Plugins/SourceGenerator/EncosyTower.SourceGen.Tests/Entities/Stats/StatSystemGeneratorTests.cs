using EncosyTower.Entities.Stats.Generators;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

[TestClass]
public class StatSystemGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<StatSystemGenerator>();

    [TestMethod]
    public Task Size8System_GeneratesStatSystemContract()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<StatSystemGenerator>(
              """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<StatSystemGenerator>(
                    "StatsApi.StatSystem.abfa7bf43d46f7f6.g.cs"
                ),
            }
        );
}
