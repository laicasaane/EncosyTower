using EncosyTower.Data.Generators.Data;
using EncosyTower.Databases.Authoring.Generators;
using Microsoft.CodeAnalysis.CSharp;

namespace EncosyTower.SourceGen.Tests.Databases.Authoring;

[TestClass]
public class DatabaseAuthoringGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<DatabaseAuthoringGenerator>();

    [TestMethod]
    public Task ValidDatabase_GeneratesSheetContainerAndSheet()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<DatabaseAuthoringGenerator>(
              """
              #define ENCOSY_INCLUDE_AUTHORING

              using EncosyTower.Collections;
              using EncosyTower.Data;
              using EncosyTower.Databases;
              using EncosyTower.Databases.Authoring;

              namespace TestProject;

              [Data]
              public partial struct Row : IDataWithId<int>
              {
                  [DataProperty]
                  public int Id => 1;

                  [DataProperty]
                  public ListFast<int>.ReadOnly Items => Get_Items();

                  [DataProperty]
                  public HashSetReadOnly<int> Tags => Get_Tags();

                  [DataProperty]
                  public DictionaryReadOnly<int, string> Names => Get_Names();
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

              [AuthorDatabase(typeof(Db))]
              public partial struct DbAuthoring { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<DatabaseAuthoringGenerator>(
                    "DbAuthoring.DatabaseAuthoringSheetContainer.132818c634e20609.g.cs"
                ),
                ExpectedGeneratedSource.Create<DatabaseAuthoringGenerator>(
                    "RowSheet.DatabaseAuthoringSheet.b3ccb078df5b0b1c.g.cs"
                ),
                ExpectedGeneratedSource.Create<DataGenerator>(
                    "Row.Data.056495b5fa807b39.g.cs"
                ),
            }
            , new IIncrementalGenerator[] { new DataGenerator() }
        );

    [TestMethod]
    public async Task CollectionLayouts_AreScopedRecursiveAndTransposed()
    {
        var transposeReferences = await GetTransposeReferencesAsync();

        await GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<DatabaseAuthoringGenerator>(
              """
              #define ENCOSY_INCLUDE_AUTHORING

              using System.Collections.Generic;
              using EncosyTower.Data;
              using EncosyTower.Databases;
              using EncosyTower.Databases.Authoring;
              using ETTranspose = EncosyTower.Databases.Authoring.TransposeAttribute;

              namespace TestProject;

              [Data]
              public partial struct Reward : IData
              {
                  [DataProperty]
                  public int Amount => 1;
              }

              [Data]
              public partial struct Row : IDataWithId<int>
              {
                  [DataProperty]
                  public int Id => 1;

                  [DataProperty]
                  public Dictionary<string, List<Reward>> Rewards => new();

                  [DataProperty]
                  public List<List<Reward>> Waves => new();

                  [DataProperty]
                  public List<Dictionary<string, Reward>> RewardGroups => new();

                  [DataProperty]
                  public Dictionary<string, Dictionary<string, Reward>> RewardMaps => new();
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
                  public readonly Rows Vertical => default;

                  [Table, Horizontal(typeof(Row), nameof(Row.Rewards)), ETTranspose]
                  public readonly Rows Horizontal => default;

                  [Table, Cathei.BakingSheet.Transpose]
                  public readonly Rows BakingSheetTranspose => default;
              }

              [AuthorDatabase(typeof(Db))]
              public partial struct DbAuthoring { }
              """
            , 3
            , new[] {
                "g__CBS.VerticalDictionary<string, g__CBS.VerticalList<__Reward>> Rewards",
                "g__SCG.Dictionary<string, g__CBS.VerticalList<__Reward>> Rewards",
                "g__CBS.VerticalList<g__CBS.VerticalList<__Reward>> Waves",
                "g__CBS.VerticalList<g__CBS.VerticalDictionary<string, __Reward>> RewardGroups",
                "g__CBS.VerticalDictionary<string, g__CBS.VerticalDictionary<string, __Reward>> RewardMaps",
                "[g__CBS.Transpose]\n            public Rows_RowSheet_1_Horizontal Rows_RowSheet_1_Horizontal",
            }
            , new[] {
                "g__SCG.Dictionary<string, g__SCG.List<__Reward>> Rewards",
                "[g__CBS.Transpose]\n            public Rows_RowSheet_0_BakingSheetTranspose",
            }
            , new IIncrementalGenerator[] { new DataGenerator() }
            , additionalReferences: transposeReferences
        );
    }

    [TestMethod]
    public Task HorizontalDataMarkerWithoutExplicitIData_GeneratesDistinctLayouts()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<DatabaseAuthoringGenerator>(
              """
              #define ENCOSY_INCLUDE_AUTHORING

              using System.Collections.Generic;
              using EncosyTower.Data;
              using EncosyTower.Databases;
              using EncosyTower.Databases.Authoring;

              namespace TestProject;

              [Data]
              public partial struct GeneratedFeatureId
              {
                  [DataProperty]
                  public string Kind => Get_Kind();

                  [DataProperty]
                  public int SubId => Get_SubId();
              }

              [Data]
              public partial struct GeneratedReward
              {
                  [DataProperty]
                  public int Amount => Get_Amount();
              }

              [Data]
              public partial struct GeneratedFeatureRow
              {
                  [DataProperty]
                  public GeneratedFeatureId Id => Get_Id();

                  [DataProperty]
                  public Dictionary<string, List<GeneratedReward>> Rewards => Get_Rewards();
              }

              [DataTableAsset]
              public sealed partial class GeneratedFeatureTableAsset
                  : DataTableAssetBase<GeneratedFeatureId, GeneratedFeatureRow>
              {
                  protected override GeneratedFeatureId GetId(in GeneratedFeatureRow entry) => entry.Id;
              }

              [Database]
              public readonly partial struct GeneratedFeatureDatabase
              {
                  [Table]
                  public readonly GeneratedFeatureTableAsset Vertical => default;

                  [Table, Horizontal(typeof(GeneratedFeatureRow), nameof(GeneratedFeatureRow.Rewards))]
                  public readonly GeneratedFeatureTableAsset Horizontal => default;
              }

              [AuthorDatabase(typeof(GeneratedFeatureDatabase))]
              public readonly partial struct GeneratedFeatureAuthoring
              {
              }
              """
            , 3
            , new[] {
                "public abstract partial class GeneratedFeatureRowSheet_0 :",
                "public abstract partial class GeneratedFeatureRowSheet_1 :",
                "public GeneratedFeatureTableAsset_GeneratedFeatureRowSheet_0_Vertical "
                  + "GeneratedFeatureTableAsset_GeneratedFeatureRowSheet_0_Vertical { get; set; }",
                "public GeneratedFeatureTableAsset_GeneratedFeatureRowSheet_1_Horizontal "
                  + "GeneratedFeatureTableAsset_GeneratedFeatureRowSheet_1_Horizontal { get; set; }",
                "g__CBS.VerticalDictionary<string, g__CBS.VerticalList<__GeneratedReward>> Rewards",
                "g__SCG.Dictionary<string, g__CBS.VerticalList<__GeneratedReward>> Rewards",
                "public partial class __GeneratedReward",
                "public int Amount",
            }
            , new[] {
                "public abstract partial class GeneratedFeatureRowSheet :",
            }
            , new IIncrementalGenerator[] { new DataGenerator() }
        );

    [TestMethod]
    public Task HorizontalInvalidDataMarkerSelections_DoNotCreateLayout()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<DatabaseAuthoringGenerator>(
              """
              #define ENCOSY_INCLUDE_AUTHORING

              using System.Collections.Generic;
              using EncosyTower.Data;
              using EncosyTower.Databases;
              using EncosyTower.Databases.Authoring;

              namespace TestProject;

              [Data]
              public partial struct Reward
              {
                  [DataProperty]
                  public int Amount => Get_Amount();
              }

              [Data]
              public abstract partial class AbstractFeatureBase
              {
                  [DataProperty]
                  public Dictionary<string, List<Reward>> Rewards => Get_Rewards();
              }

              [Data]
              public partial class FeatureRow : AbstractFeatureBase
              {
                  [DataProperty]
                  public int Id => Get_Id();

                  [DataProperty]
                  public int Value => Get_Value();
              }

              [Data]
              public partial class UnreachableRow
              {
                  [DataProperty]
                  public Dictionary<string, List<Reward>> Rewards => Get_Rewards();
              }

              [DataTableAsset]
              public sealed partial class FeatureTableAsset : DataTableAssetBase<int, FeatureRow>
              {
                  protected override int GetId(in FeatureRow entry) => entry.Id;
              }

              [Database]
              public readonly partial struct FeatureDatabase
              {
                  [Table]
                  public readonly FeatureTableAsset Vertical => default;

                  [Table]
                  [Horizontal(typeof(UnreachableRow), nameof(UnreachableRow.Rewards))]
                  [Horizontal(typeof(AbstractFeatureBase), nameof(AbstractFeatureBase.Rewards))]
                  [Horizontal(typeof(FeatureRow), nameof(FeatureRow.Value))]
                  public readonly FeatureTableAsset Horizontal => default;
              }

              [AuthorDatabase(typeof(FeatureDatabase))]
              public readonly partial struct FeatureAuthoring
              {
              }
              """
            , 2
            , new[] {
                "public abstract partial class FeatureRowSheet :",
                "public FeatureTableAsset_FeatureRowSheet_Vertical "
                  + "FeatureTableAsset_FeatureRowSheet_Vertical { get; set; }",
                "public FeatureTableAsset_FeatureRowSheet_Horizontal "
                  + "FeatureTableAsset_FeatureRowSheet_Horizontal { get; set; }",
                "g__CBS.VerticalDictionary<string, g__CBS.VerticalList<__Reward>> Rewards",
            }
            , new[] {
                "FeatureRowSheet_0",
                "FeatureRowSheet_1",
                "g__SCG.Dictionary<string, g__CBS.VerticalList<__Reward>> Rewards",
            }
            , new IIncrementalGenerator[] { new DataGenerator() }
        );

    [TestMethod]
    public Task CompositeId_GeneratesValueEquality()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<DatabaseAuthoringGenerator>(
              """
              #define ENCOSY_INCLUDE_AUTHORING

              using EncosyTower.Data;
              using EncosyTower.Databases;
              using EncosyTower.Databases.Authoring;

              namespace TestProject;

              [Data]
              public partial struct Key : IData
              {
                  [DataProperty]
                  public string Kind => Get_Kind();

                  [DataProperty]
                  public int Number => Get_Number();
              }

              [Data]
              public partial struct Row : IDataWithId<Key>
              {
                  [DataProperty]
                  public Key Id => Get_Id();
              }

              [DataTableAsset]
              public partial class Rows : DataTableAssetBase<Key, Row>
              {
                  protected override Key GetId(in Row entry) => entry.Id;
              }

              [Database]
              public readonly partial struct Db
              {
                  [Table]
                  public readonly Rows Items => default;
              }

              [AuthorDatabase(typeof(Db))]
              public partial struct DbAuthoring { }
              """
            , 2
            , new[] {
                "public partial class __Key : g__S.IEquatable<__Key>",
                "public bool Equals(__Key other)",
                "g__S.String.Equals(this.Kind, other.Kind, g__S.StringComparison.Ordinal)",
                "g__SCG.EqualityComparer<int>.Default.Equals(this.Number, other.Number)",
                "public override int GetHashCode()",
            }
            , Array.Empty<string>()
            , new IIncrementalGenerator[] { new DataGenerator() }
        );

    [TestMethod]
    public Task CompositeId_CompatiblePartialEqualityIsPreserved()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<DatabaseAuthoringGenerator>(
              """
              #define ENCOSY_INCLUDE_AUTHORING

              using EncosyTower.Data;
              using EncosyTower.Databases;
              using EncosyTower.Databases.Authoring;

              namespace TestProject;

              [Data]
              public partial struct Key : IData
              {
                  [DataProperty]
                  public string Kind => Get_Kind();
              }

              [Data]
              public partial struct Row : IDataWithId<Key>
              {
                  [DataProperty]
                  public Key Id => Get_Id();
              }

              [DataTableAsset]
              public partial class Rows : DataTableAssetBase<Key, Row>
              {
                  protected override Key GetId(in Row entry) => entry.Id;
              }

              [Database]
              public readonly partial struct Db
              {
                  [Table]
                  public readonly Rows Items => default;
              }

              [AuthorDatabase(typeof(Db))]
              public partial struct DbAuthoring
              {
                  public abstract partial class RowSheet
                  {
                      public partial class __Key
                      {
                          public bool Equals(__Key other)
                              => other is not null
                              && string.Equals(Kind, other.Kind, System.StringComparison.OrdinalIgnoreCase);

                          public override bool Equals(object obj)
                              => obj is __Key other && Equals(other);

                          public override int GetHashCode()
                              => System.StringComparer.OrdinalIgnoreCase.GetHashCode(Kind ?? string.Empty);
                      }
                  }
              }
              """
            , 2
            , new[] {
                "public partial class __Key : g__S.IEquatable<__Key>",
            }
            , new[] {
                "public bool Equals(__Key other)",
                "public override bool Equals(object obj)",
                "public override int GetHashCode()",
            }
            , new IIncrementalGenerator[] { new DataGenerator() }
        );

    [TestMethod]
    public Task CompositeId_IncompletePartialEqualityDoesNotGenerateConflictingMembers()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<DatabaseAuthoringGenerator>(
              """
              #define ENCOSY_INCLUDE_AUTHORING

              using EncosyTower.Data;
              using EncosyTower.Databases;
              using EncosyTower.Databases.Authoring;

              namespace TestProject;

              [Data]
              public partial struct Key : IData
              {
                  [DataProperty]
                  public string Kind => Get_Kind();
              }

              [Data]
              public partial struct Row : IDataWithId<Key>
              {
                  [DataProperty]
                  public Key Id => Get_Id();
              }

              [DataTableAsset]
              public partial class Rows : DataTableAssetBase<Key, Row>
              {
                  protected override Key GetId(in Row entry) => entry.Id;
              }

              [Database]
              public readonly partial struct Db
              {
                  [Table]
                  public readonly Rows Items => default;
              }

              [AuthorDatabase(typeof(Db))]
              public partial struct DbAuthoring
              {
                  public abstract partial class RowSheet
                  {
                      public partial class __Key
                      {
                          public bool Equals(__Key other) => true;
                      }
                  }
              }
              """
            , 2
            , Array.Empty<string>()
            , new[] {
                "public partial class __Key : g__S.IEquatable<__Key>",
                "public bool Equals(__Key other)",
                "public override bool Equals(object obj)",
                "public override int GetHashCode()",
            }
            , new IIncrementalGenerator[] { new DataGenerator() }
        );

    private static async Task<IReadOnlyList<MetadataReference>> GetTransposeReferencesAsync()
    {
        const string TRANSPOSE_ATTRIBUTE =
            "EncosyTower.Databases.Authoring.TransposeAttribute";

        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync();
        var probe = CSharpCompilation.Create("TransposeAttributeProbe", references: references);

        if (probe.GetTypeByMetadataName(TRANSPOSE_ATTRIBUTE) is not null)
        {
            return Array.Empty<MetadataReference>();
        }

        var reference = await GeneratorTestHelper.CompileToReferenceAsync(
              """
              namespace EncosyTower.Databases.Authoring;

              [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
              [System.Diagnostics.Conditional("UNITY_EDITOR")]
              [System.Diagnostics.Conditional("ENCOSY_INCLUDE_AUTHORING")]
              public sealed class TransposeAttribute : System.Attribute { }
              """
            , "EncosyTower.DatabaseAuthoring.TransposeAttribute.Tests"
        );
        return new[] { reference };
    }
}
