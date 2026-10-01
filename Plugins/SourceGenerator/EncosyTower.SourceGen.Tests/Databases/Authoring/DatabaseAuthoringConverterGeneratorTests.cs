using EncosyTower.Data.Generators.Data;
using EncosyTower.Data.Generators.Databases;
using EncosyTower.Data.Generators.DataTableAssets;
using EncosyTower.Databases.Authoring.Generators;

namespace EncosyTower.SourceGen.Tests.Databases.Authoring;

[TestClass]
public class DatabaseAuthoringConverterGeneratorTests
{
    private const string PREFIX = """
        #define ENCOSY_INCLUDE_AUTHORING

        using EncosyTower.Data;
        using EncosyTower.Databases;
        using EncosyTower.Databases.Authoring;

        namespace TestProject;
        """;

    [TestMethod]
    public Task DivergentTableConverters_ProduceSeparateSheets()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<DatabaseAuthoringGenerator>(
              Wrap(
                  """
                  public readonly struct ConvA
                  {
                      public static int[] Convert(string value) => System.Array.Empty<int>();
                  }

                  public readonly struct ConvB
                  {
                      public static int[] Convert(string value) => System.Array.Empty<int>();
                  }

                  [Data]
                  public partial struct Loc : IDataWithId<int>
                  {
                      [DataProperty]
                      public int Id => 1;

                      [DataProperty(typeof(int[]))]
                      public int[] Points => System.Array.Empty<int>();
                  }

                  public sealed class LocTableA : DataTableAssetBase<int, Loc>
                  {
                      protected override int GetId(in Loc entry) => entry.Id;
                  }

                  public sealed class LocTableB : DataTableAssetBase<int, Loc>
                  {
                      protected override int GetId(in Loc entry) => entry.Id;
                  }

                  [Database]
                  public readonly partial struct GameDb
                  {
                      [Table]
                      public readonly LocTableA TableA => default;

                      [Table]
                      public readonly LocTableB TableB => default;
                  }

                  [AuthorDatabase(typeof(GameDb))]
                  [ConverterForTable(typeof(LocTableA), typeof(ConvA))]
                  [ConverterForTable(typeof(LocTableB), typeof(ConvB))]
                  public partial struct GameDbAuthoring { }
                  """
              )
            , new[] {
                ExpectedGeneratedSource.Create<DatabaseAuthoringGenerator>(
                    "GameDbAuthoring.DatabaseAuthoringSheetContainer.9c832a4aef18f2e1.g.cs"
                ),
                ExpectedGeneratedSource.Create<DatabaseAuthoringGenerator>(
                    "LocSheet_0.DatabaseAuthoringSheet.0e6d122bc7bcc6b1.g.cs"
                ),
                ExpectedGeneratedSource.Create<DatabaseAuthoringGenerator>(
                    "LocSheet_1.DatabaseAuthoringSheet.f0098cf7a5326d39.g.cs"
                ),
                ExpectedGeneratedSource.Create<DataGenerator>(
                    "Loc.Data.5b3ec5914df6bebd.g.cs"
                ),
            }
            , new IIncrementalGenerator[] { new DataGenerator() }
        );

    [TestMethod]
    public Task IdenticalTableConverters_ShareOneSheet()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<DatabaseAuthoringGenerator>(
              Wrap(
                  """
                  public readonly struct ConvA
                  {
                      public static int[] Convert(string value) => System.Array.Empty<int>();
                  }

                  [Data]
                  public partial struct Loc : IDataWithId<int>
                  {
                      [DataProperty]
                      public int Id => 1;

                      [DataProperty(typeof(int[]))]
                      public int[] Points => System.Array.Empty<int>();
                  }

                  public sealed class LocTableA : DataTableAssetBase<int, Loc>
                  {
                      protected override int GetId(in Loc entry) => entry.Id;
                  }

                  public sealed class LocTableB : DataTableAssetBase<int, Loc>
                  {
                      protected override int GetId(in Loc entry) => entry.Id;
                  }

                  [Database]
                  public readonly partial struct GameDb
                  {
                      [Table]
                      public readonly LocTableA TableA => default;

                      [Table]
                      public readonly LocTableB TableB => default;
                  }

                  [AuthorDatabase(typeof(GameDb))]
                  [ConverterForTable(typeof(LocTableA), typeof(ConvA))]
                  [ConverterForTable(typeof(LocTableB), typeof(ConvA))]
                  public partial struct GameDbAuthoring { }
                  """
              )
            , new[] {
                ExpectedGeneratedSource.Create<DatabaseAuthoringGenerator>(
                    "GameDbAuthoring.DatabaseAuthoringSheetContainer.9c832a4aef18f2e1.g.cs"
                ),
                ExpectedGeneratedSource.Create<DatabaseAuthoringGenerator>(
                    "LocSheet.DatabaseAuthoringSheet.3db5d3fca458fcbb.g.cs"
                ),
                ExpectedGeneratedSource.Create<DataGenerator>(
                    "Loc.Data.5b3ec5914df6bebd.g.cs"
                ),
            }
            , new IIncrementalGenerator[] { new DataGenerator() }
        );

    [TestMethod]
    public Task DataPropertyConverter_TakesPrecedenceOverTableConverter()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<DatabaseAuthoringGenerator>(
              Wrap(
                  """
                  public readonly struct ConvTable
                  {
                      public static int[] Convert(string value) => System.Array.Empty<int>();
                  }

                  public readonly struct ConvProperty
                  {
                      public static int[] Convert(double value) => System.Array.Empty<int>();
                  }

                  [Data]
                  public partial struct Loc : IDataWithId<int>
                  {
                      [DataProperty]
                      public int Id => 1;

                      [DataProperty(typeof(int[]))]
                      public int[] Points => System.Array.Empty<int>();
                  }

                  public sealed class LocTable : DataTableAssetBase<int, Loc>
                  {
                      protected override int GetId(in Loc entry) => entry.Id;
                  }

                  [Database]
                  public readonly partial struct GameDb
                  {
                      [Table]
                      public readonly LocTable Items => default;
                  }

                  [AuthorDatabase(typeof(GameDb))]
                  [ConverterForDataProperty(typeof(Loc), nameof(Loc.Points), typeof(ConvProperty))]
                  [ConverterForTable(typeof(LocTable), typeof(ConvTable))]
                  public partial struct GameDbAuthoring { }
                  """
              )
            , new[] {
                ExpectedGeneratedSource.Create<DatabaseAuthoringGenerator>(
                    "GameDbAuthoring.DatabaseAuthoringSheetContainer.9c832a4aef18f2e1.g.cs"
                ),
                ExpectedGeneratedSource.Create<DatabaseAuthoringGenerator>(
                    "LocSheet.DatabaseAuthoringSheet.3db5d3fca458fcbb.g.cs"
                ),
                ExpectedGeneratedSource.Create<DataGenerator>(
                    "Loc.Data.5b3ec5914df6bebd.g.cs"
                ),
            }
            , new IIncrementalGenerator[] { new DataGenerator() }
        );

    [TestMethod]
    public async Task CrossAssembly_AuthoringResolvesConverters()
    {
        var databaseReference = await GeneratorTestHelper.CompileToReferenceAsync(
              """
              #define ENCOSY_INCLUDE_AUTHORING

              using EncosyTower.Data;
              using EncosyTower.Databases;

              [assembly: System.Runtime.CompilerServices.InternalsVisibleTo(
                  "EncosyTower.SourceGen.Tests.Input"
              )]

              namespace DbProject;

              public readonly struct ConvA
              {
                  public static int[] Convert(string value) => System.Array.Empty<int>();
              }

              [Data]
              public partial struct Loc
              {
                  [DataProperty]
                  public readonly int Id => Get_Id();

                  [DataProperty(typeof(int[]))]
                  public readonly int[] Points => Get_Points();
              }

              [DataTableAsset]
              public partial class LocTable : DataTableAssetBase<int, Loc> { }

              [Database]
              public readonly partial struct GameDb
              {
                  [Table]
                  public readonly LocTable Items => Get_Items();
              }
              """
            , "DbProject"
            , new IIncrementalGenerator[] {
                new DataGenerator(),
                new DataTableAssetGenerator(),
                new DatabaseGenerator(),
            }
        );

        await GeneratorTestHelper.VerifyGeneratedSourcesAsync<DatabaseAuthoringGenerator>(
              """
              using EncosyTower.Databases.Authoring;

              namespace AuthProject;

              [AuthorDatabase(typeof(DbProject.GameDb))]
              [ConverterForTable(typeof(DbProject.LocTable), typeof(DbProject.ConvA))]
              public partial struct GameDbAuthoring { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<DatabaseAuthoringGenerator>(
                    "GameDbAuthoring.DatabaseAuthoringSheetContainer.d56e05817f1bfeb0.g.cs"
                ),
                ExpectedGeneratedSource.Create<DatabaseAuthoringGenerator>(
                    "LocSheet.DatabaseAuthoringSheet.7f19deb38e8a13bb.g.cs"
                ),
            }
            , additionalReferences: new[] { databaseReference }
        );
    }

    private static string Wrap(string body)
        => $"{PREFIX}\n\n{body}";
}
