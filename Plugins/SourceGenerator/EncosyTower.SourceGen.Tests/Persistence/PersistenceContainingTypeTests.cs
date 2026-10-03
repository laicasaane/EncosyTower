using System.Collections.Immutable;
using EncosyTower.Persistence.Generators;

namespace EncosyTower.SourceGen.Tests.Persistence;

[TestClass]
public sealed class PersistenceContainingTypeTests
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
              "Persist containing type kind"
            , static () => new IIncrementalGenerator[] { new PersistGenerator() }
            , """
              using EncosyTower.Persistences;

              namespace TestProject;

              {{EDIT}}
              {
                  [Persist]
                  public partial class PlayerData { }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "PersistGenerator.Outputs" }
        );

        yield return new(
              "Persistence containing type kind"
            , static () => new IIncrementalGenerator[] { new PersistenceGenerator() }
            , """
              using System;
              using EncosyTower.Persistences;

              namespace TestProject;

              {{EDIT}}
              {
                  [Persistence]
                  internal static partial class SavePersistence
                  {
                      internal partial class PersistDirectory
                      {
                          private static partial PersistStoreArgs GetStoreArgs<TData, TStore>(
                                Func<TData> createDataFunc
                          )
                              where TData : IPersist
                              where TStore : PersistStoreBase<TData>
                              => default;
                      }
                  }
              }

              internal sealed class SaveData : IPersist
              {
                  public string Id { get; set; } = "";
                  public int Version { get; set; }
              }

              [PersistAccessor(typeof(Outer.SavePersistence))]
              internal sealed class SaveAccessor : IPersistAccessor
              {
                  internal SaveAccessor(PersistStoreDefault<SaveData> store) { }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "PersistenceGenerator.Outputs" }
        );

        yield return new(
              "Persist namespace"
            , static () => new IIncrementalGenerator[] { new PersistGenerator() }
            , """
              using EncosyTower.Persistences;

              {{EDIT}}
              {
                  [Persist]
                  public partial class PlayerData { }
              }
              """
            , ReusedDriverEditCase.TEST_NAMESPACE
            , ReusedDriverEditCase.OTHER_NAMESPACE
            , new[] { "PersistGenerator.Outputs" }
        );
    }

    private static IEnumerable<SpecEqualityCase> CreateEqualityCases()
    {
        yield return new(
              "PersistSpec.containingTypes"
            , static changed => new PersistSpec {
                typeName = "PlayerData",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "PersistenceSpec.containingTypeDeclarations"
            , static changed => new PersistenceSpec {
                metadataName = "TestProject.Outer.SavePersistence",
                containingTypeDeclarations = ImmutableArray.Create(
                    changed ? "public partial struct Outer" : "public partial class Outer"
                ).AsEquatableArray(),
            }
        );

        yield return new(
              "PersistSpec.typeFullName"
            , static changed => new PersistSpec {
                typeName = "PlayerData",
                typeFullName = changed ? "global::OtherProject.PlayerData" : "global::TestProject.PlayerData",
            }
        );
    }
}
