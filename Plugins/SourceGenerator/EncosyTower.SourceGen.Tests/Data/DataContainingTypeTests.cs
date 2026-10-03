using EncosyTower.Data.Generators.Data;
using EncosyTower.Data.Generators.Databases;
using EncosyTower.Data.Generators.DataTableAssets;
using EncosyTower.SourceGen.Data.Helpers;

namespace EncosyTower.SourceGen.Tests.Data;

[TestClass]
public sealed class DataContainingTypeTests
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
              "Data containing type kind"
            , static () => new IIncrementalGenerator[] { new DataGenerator() }
            , """
              using EncosyTower.Data;

              namespace TestProject;

              {{EDIT}}
              {
                  [Data]
                  public partial class Row
                  {
              #pragma warning disable CS0657
                      [DataProperty]
                      [field: UnityEngine.SerializeField]
                      public int Id => Get_Id();
              #pragma warning restore CS0657
                  }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "DataGenerator.Outputs" }
        );

        yield return new(
              "DataTableAsset containing type kind"
            , static () => new IIncrementalGenerator[] { new DataTableAssetGenerator() }
            , """
              using EncosyTower.Data;
              using EncosyTower.Databases;

              namespace TestProject;

              public readonly struct Row : IDataWithId<int>
              {
                  public int Id => 1;
              }

              {{EDIT}}
              {
                  [DataTableAsset]
                  public partial class Rows : DataTableAssetBase<int, Row> { }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "DataTableAssetGenerator.Outputs" }
        );

        yield return new(
              "DataTableAsset namespace"
            , static () => new IIncrementalGenerator[] { new DataTableAssetGenerator() }
            , """
              using EncosyTower.Data;
              using EncosyTower.Databases;

              namespace Shared
              {
                  public readonly struct Row : IDataWithId<int>
                  {
                      public int Id => 1;
                  }
              }

              {{EDIT}}
              {
                  [DataTableAsset]
                  public partial class Rows : DataTableAssetBase<int, Shared.Row> { }
              }
              """
            , ReusedDriverEditCase.TEST_NAMESPACE
            , ReusedDriverEditCase.OTHER_NAMESPACE
            , new[] { "DataTableAssetGenerator.Outputs" }
        );

        yield return new(
              "Database containing type kind"
            , static () => new IIncrementalGenerator[] { new DatabaseGenerator() }
            , """
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

              {{EDIT}}
              {
                  [Database(WithInstanceAPI = true)]
                  public sealed partial class Db
                  {
                      [Table]
                      public Rows Items => Get_Items();
                  }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "DatabaseGenerator.Outputs" }
        );

        yield return new(
              "Database namespace"
            , static () => new IIncrementalGenerator[] { new DatabaseGenerator() }
            , """
              #define ENCOSY_INCLUDE_AUTHORING

              using EncosyTower.Data;
              using EncosyTower.Databases;

              namespace Shared
              {
                  public readonly struct Row : IDataWithId<int>
                  {
                      public int Id => 1;
                  }

                  public sealed class Rows : DataTableAssetBase<int, Row>
                  {
                      protected override int GetId(in Row entry) => entry.Id;
                  }
              }

              {{EDIT}}
              {
                  [Database(WithInstanceAPI = true)]
                  public sealed partial class Db
                  {
                      [Table]
                      public Shared.Rows Items => Get_Items();
                  }
              }
              """
            , ReusedDriverEditCase.TEST_NAMESPACE
            , ReusedDriverEditCase.OTHER_NAMESPACE
            , new[] { "DatabaseGenerator.Outputs" }
        );
    }

    private static IEnumerable<SpecEqualityCase> CreateEqualityCases()
    {
        yield return new(
              "DataSpec.containingTypes"
            , static changed => new DataSpec {
                typeName = "Row",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "DataTableAssetSpec.containingTypes"
            , static changed => new DataTableAssetSpec {
                className = "Rows",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "DataTableAssetSpec.classFullName"
            , static changed => new DataTableAssetSpec {
                className = "Rows",
                classFullName = changed ? "global::OtherProject.Rows" : "global::TestProject.Rows",
            }
        );

        yield return new(
              "DatabaseSpec.containingTypes"
            , static changed => new DatabaseSpec {
                typeName = "Db",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "DatabaseSpec.typeFullName"
            , static changed => new DatabaseSpec {
                typeName = "Db",
                typeFullName = changed ? "global::OtherProject.Db" : "global::TestProject.Db",
            }
        );

        yield return new(
              "FieldRefData.samePropertyType"
            , static changed => new FieldRefData {
                fieldName = "_name",
                samePropertyType = changed,
            }
        );

        yield return new(
              "PropRefData.fieldTypeName"
            , static changed => new PropRefData {
                propertyName = "Name",
                fieldTypeName = changed ? "global::System.Int64" : "global::System.Int32",
            }
        );
    }
}
