using EncosyTower.Data.Generators.Data;
using EncosyTower.Databases.Authoring.Generators;

namespace EncosyTower.SourceGen.Tests.Databases.Authoring;

[TestClass]
public sealed class AuthoringContainingTypeTests
{
    public static IEnumerable<object[]> EditCases
        => CreateEditCases().Select(static editCase => new object[] { editCase });

    public static IEnumerable<object[]> EqualityCases
        => CreateEqualityCases().Select(static equalityCase => new object[] { equalityCase });

    [TestMethod]
    [DynamicData(nameof(EditCases), DynamicDataSourceType.Property)]
    public Task ReusedDriverEdit_RegeneratesOutput(ReusedDriverEditCase editCase)
        => editCase.VerifyAsync();

    [TestMethod]
    [DynamicData(nameof(EqualityCases), DynamicDataSourceType.Property)]
    public void ChangedField_ChangesEqualityAndHash(SpecEqualityCase equalityCase)
        => equalityCase.Verify();

    private static IEnumerable<ReusedDriverEditCase> CreateEditCases()
    {
        yield return new(
              "DatabaseAuthoring containing type kind"
            , static () => new IIncrementalGenerator[] { new DatabaseAuthoringGenerator(), new DataGenerator() }
            , """
              #define ENCOSY_INCLUDE_AUTHORING

              using EncosyTower.Data;
              using EncosyTower.Databases;
              using EncosyTower.Databases.Authoring;

              namespace TestProject;

              [Data]
              public partial struct Row : IDataWithId<int>
              {
                  [DataProperty]
                  public int Id => 1;
              }

              [DataTableAsset]
              public partial class Rows : DataTableAssetBase<int, Row>
              {
                  protected override int GetId(in Row entry) => entry.Id;
              }

              [Database]
              public readonly partial struct Db
              {
                  [Table]
                  public readonly Rows Items => default;
              }

              {{EDIT}}
              {
                  [AuthorDatabase(typeof(Db))]
                  public partial struct DbAuthoring { }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "DatabaseAuthoringGenerator.Outputs" }
        );

        yield return new(
              "DatabaseAuthoring namespace"
            , static () => new IIncrementalGenerator[] { new DatabaseAuthoringGenerator(), new DataGenerator() }
            , """
              #define ENCOSY_INCLUDE_AUTHORING

              using EncosyTower.Data;
              using EncosyTower.Databases;
              using EncosyTower.Databases.Authoring;

              namespace Shared
              {
                  [Data]
                  public partial struct Row : IDataWithId<int>
                  {
                      [DataProperty]
                      public int Id => 1;
                  }

                  [DataTableAsset]
                  public partial class Rows : DataTableAssetBase<int, Row>
                  {
                      protected override int GetId(in Row entry) => entry.Id;
                  }

                  [Database]
                  public readonly partial struct Db
                  {
                      [Table]
                      public readonly Rows Items => default;
                  }
              }

              {{EDIT}}
              {
                  [AuthorDatabase(typeof(Shared.Db))]
                  public partial struct DbAuthoring { }
              }
              """
            , ReusedDriverEditCase.TEST_NAMESPACE
            , ReusedDriverEditCase.OTHER_NAMESPACE
            , new[] { "DatabaseAuthoringGenerator.Outputs" }
        );
    }

    private static IEnumerable<SpecEqualityCase> CreateEqualityCases()
    {
        yield return new(
              "DatabaseSpec.containingTypes"
            , static changed => new DatabaseSpec {
                databaseTypeName = "DbAuthoring",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "SheetInfoSpec.tableName"
            , static changed => new SheetInfoSpec {
                tableName = changed ? "Others" : "Items",
                propertyName = "Items",
            }
        );
    }
}
