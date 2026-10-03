using EncosyTower.Core.Generators.EnumTemplates;
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

    [TestMethod]
    public Task FieldData_GeneratesDataContract()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<DataGenerator>(
              """
              using EncosyTower.Data;

              namespace TestProject;

              [Data]
              public partial class FieldRow
              {
              #pragma warning disable CS0649
                  [UnityEngine.SerializeField]
                  [EncosyTower.Data.Authoring.DataManualAuthoring(typeof(string))]
                  private int _id;
              #pragma warning restore CS0649
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<DataGenerator>("FieldRow.Data.b55b561775614781.g.cs"),
            }
        );

    [TestMethod]
    public Task FieldTargetedManualAuthoring_MatchesUntargetedOutput()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<DataGenerator>(
              """
              using EncosyTower.Data;

              namespace TestProject;

              [Data]
              public partial class FieldRow
              {
              #pragma warning disable CS0649
                  [UnityEngine.SerializeField]
                  [field: EncosyTower.Data.Authoring.DataManualAuthoring(typeof(string))]
                  private int _id;
              #pragma warning restore CS0649
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<DataGenerator>(
                      "FieldRow.Data.b55b561775614781.g.cs"
                    , testMethod: nameof(FieldData_GeneratesDataContract)
                ),
            }
        );

    [TestMethod]
    public Task PropertyAttributesNamingGeneratedEnumMember_AreSkipped()
        => GeneratorTestHelper.VerifyGeneratedSourcesWithProducersAsync<DataGenerator>(
              $$"""
              using EncosyTower.Data;

              namespace TestProject;

              {{ProducerFixtures.SCREEN_TYPE_TEMPLATE}}

              {{ProducerFixtures.TAG_ATTRIBUTES}}

              [Data]
              public partial class Row
              {
              #pragma warning disable CS0657
                  [DataProperty]
                  [Tag(ScreenType.Lobby)]
                  [field: UnityEngine.SerializeField]
                  [field: Tag(ScreenType.Lobby)]
                  [field: MultiTag(ScreenType.Shop)]
                  public int Id => Get_Id();
              #pragma warning restore CS0657
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<DataGenerator>(
                      "Row.Data.056495b5fa807b39.g.cs"
                    , testMethod: nameof(DataProperty_GeneratesDataContract)
                ),
            }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
        );

    [TestMethod]
    public Task FieldAttributesNamingGeneratedEnumMember_AreSkipped()
        => GeneratorTestHelper.VerifyGeneratedSourcesWithProducersAsync<DataGenerator>(
              $$"""
              using EncosyTower.Data;

              namespace TestProject;

              {{ProducerFixtures.SCREEN_TYPE_TEMPLATE}}

              {{ProducerFixtures.TAG_ATTRIBUTES}}

              [Data]
              public partial class FieldRow
              {
              #pragma warning disable CS0649, CS0657
                  [UnityEngine.SerializeField]
                  [EncosyTower.Data.Authoring.DataManualAuthoring(typeof(string))]
                  [Tag(ScreenType.Lobby)]
                  [property: Tag(ScreenType.Lobby)]
                  [property: MultiTag(ScreenType.Shop)]
                  private int _id;
              #pragma warning restore CS0649, CS0657
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<DataGenerator>(
                      "FieldRow.Data.b55b561775614781.g.cs"
                    , testMethod: nameof(FieldData_GeneratesDataContract)
                ),
            }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
        );
}
