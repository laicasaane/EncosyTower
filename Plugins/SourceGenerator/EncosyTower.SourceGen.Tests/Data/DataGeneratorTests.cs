using EncosyTower.Data.Generators.Data;

namespace EncosyTower.SourceGen.Tests.Data;

[TestClass]
public class DataGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<DataGenerator>();

    [TestMethod]
    public Task DataProperty_GeneratesDataContract()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<DataGenerator>(
              """
              using EncosyTower.Data;

              namespace TestProject;

              [Data]
              public partial class Row
              {
              #pragma warning disable CS0657
                  [DataProperty]
                  [field: UnityEngine.SerializeField]
                  public int Id => Get_Id();
              #pragma warning restore CS0657
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<DataGenerator>(
                    "Row.Data.056495b5fa807b39.g.cs"
                ),
            }
        );

    [TestMethod]
    public Task ReadOnlyCollections_GenerateMutableBackingFields()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<DataGenerator>(
              """
              using EncosyTower.Collections;
              using EncosyTower.Data;

              namespace TestProject;

              [Data]
              public partial class Row
              {
                  [DataProperty]
                  public ListFast<int>.ReadOnly Items => Get_Items();

                  [DataProperty]
                  public HashSetReadOnly<int> Tags => Get_Tags();

                  [DataProperty]
                  public DictionaryReadOnly<int, string> Names => Get_Names();
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<DataGenerator>(
                    "Row.Data.056495b5fa807b39.g.cs"
                ),
            }
        );
}
