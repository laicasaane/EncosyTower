using EncosyTower.Data.Generators.DataTableAssets;

namespace EncosyTower.SourceGen.Tests.Data.DataTableAssets;

[TestClass]
public class DataTableAssetGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<DataTableAssetGenerator>();

    [TestMethod]
    public Task AnnotatedTableAsset_GeneratesTableContract()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<DataTableAssetGenerator>(
              """
              using EncosyTower.Data;
              using EncosyTower.Databases;

              namespace TestProject;

              public readonly struct Row : IDataWithId<int>
              {
                  public int Id => 1;
              }

              [DataTableAsset]
              public partial class Rows : DataTableAssetBase<int, Row>
              {
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<DataTableAssetGenerator>(
                    "Rows.DataTableAsset.dc184f61c74396ea.g.cs"
                ),
            }
        );
}
