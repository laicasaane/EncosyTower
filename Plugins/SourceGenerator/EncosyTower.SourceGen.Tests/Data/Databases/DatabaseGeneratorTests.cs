using EncosyTower.Data.Generators.Databases;

namespace EncosyTower.SourceGen.Tests.Data.Databases;

[TestClass]
public class DatabaseGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<DatabaseGenerator>();

    [TestMethod]
    public Task DatabaseTable_GeneratesDatabaseContract()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<DatabaseGenerator>(
              """
              #define ENCOSY_INCLUDE_AUTHORING

              using EncosyTower.Data;
              using EncosyTower.Databases;

              namespace TestProject;

              public readonly struct Row : IDataWithId<int>
              {
                  public int Id => 1;
              }

              public sealed class Rows : DataTableAssetBase<int, Row>
              {
                  protected override int GetId(in Row entry) => entry.Id;
              }

              [Database(WithInstanceAPI = true)]
              public sealed partial class Db
              {
                  [Table]
                  public Rows Items => Get_Items();
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<DatabaseGenerator>(
                    "Db.Database.53701125f5a04246.g.cs"
                ),
            }
            , verifyDebuggingAliasContract: true
        );

    [TestMethod]
    public Task GeneratedDebuggingAlias_WithGenericTypeParameterName_Compiles()
        => GeneratorTestHelper.VerifyDebuggingAliasCollisionAsync();
}
