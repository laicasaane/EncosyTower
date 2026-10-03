using EncosyTower.Core.Generators.EnumExtensions;
using EncosyTower.Core.Generators.EnumTemplates;
using EncosyTower.Core.Generators.UnionIds;
using EncosyTower.Core.Generators.Variants;
using EncosyTower.Data.Generators.Databases;
using EncosyTower.SourceGen.Tests.Processing;
using EncosyTower.SourceGen.Tests.PubSub;

namespace EncosyTower.SourceGen.Tests.Common;

[TestClass]
public sealed class GeneratedTypeClassificationTests
{
    private const string INPUT_ASSEMBLY_NAME = "EncosyTower.SourceGen.Tests.Input";
    private const string ENUM_EXTENSIONS_TOOL = "EncosyTower.Core.Generators.EnumExtensions.EnumExtensionsGenerator";
    private const string ENUM_EXTENSIONS_FOR_TOOL = "EncosyTower.Core.Generators.EnumExtensions."
        + "EnumExtensionsForGenerator";
    private const string UNION_ID_TOOL = "EncosyTower.Core.Generators.UnionIds.UnionIdGenerator";
    private const string VARIANT_STRUCT_TOOL = "EncosyTower.Core.Generators.Variants.VariantStructGenerator";
    private const string DATABASE_TOOL = "EncosyTower.Data.Generators.Databases.DatabaseGenerator";
    private const string PROCESSING_TOOL = "EncosyTower.Processing.Generators.ProcessingRequestGenerator";
    private const string PUBSUB_TOOL = "EncosyTower.PubSub.Generators.PubSubMessageGenerator";

    [TestMethod]
    public async Task EnumTemplate_ClassifiesGeneratedTypesOnly()
    {
        var compilation = await RunAsync(
              ProducerFixtures.SCREEN_TYPE_PRODUCER
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
        );

        AssertTool(compilation, "TestProject.ScreenType", ProducerFixtures.ENUM_TEMPLATE_TOOL);
        AssertTool(compilation, "TestProject.ScreenTypeExtended", ProducerFixtures.ENUM_TEMPLATE_TOOL);
        AssertTool(compilation, "TestProject.IScreenTypeExtensions", ProducerFixtures.ENUM_TEMPLATE_TOOL);
        AssertNotGenerated(compilation, "TestProject.ScreenType_EnumTemplate");
        AssertNotGenerated(compilation, "TestProject.MenuScreen");
        AssertNotGenerated(compilation, "System.DayOfWeek");
    }

    [TestMethod]
    public async Task FlagsEnumerator_IsClassified()
    {
        var compilation = await RunAsync(
              """
              using System;
              using EncosyTower.EnumExtensions;

              namespace TestProject;

              [Flags]
              [EnumExtensions]
              public enum Permission : byte
              {
                  None = 0,
                  Read = 1,
                  Write = 2,
              }
              """
            , new IIncrementalGenerator[] { new EnumExtensionsGenerator() }
        );

        AssertTool(compilation, "TestProject.PermissionExtended+Enumerator", ENUM_EXTENSIONS_TOOL);
    }

    [TestMethod]
    public async Task EnumExtensionsFor_NamesItsOwnGenerator()
    {
        var compilation = await RunAsync(
              """
              using System;
              using EncosyTower.EnumExtensions;

              namespace TestProject;

              [EnumExtensionsFor(typeof(DayOfWeek))]
              public static partial class DayOfWeekExtensions { }
              """
            , new IIncrementalGenerator[] { new EnumExtensionsForGenerator() }
        );

        AssertTool(compilation, "TestProject.DayOfWeekExtended", ENUM_EXTENSIONS_FOR_TOOL);
        AssertTool(compilation, "TestProject.IDayOfWeekExtensions", ENUM_EXTENSIONS_FOR_TOOL);
        AssertNotGenerated(compilation, "TestProject.DayOfWeekExtensions");
    }

    [TestMethod]
    public async Task UnionIdTypeConverter_IsClassified()
    {
        var compilation = await RunAsync(
              """
              using EncosyTower.UnionIds;

              namespace TestProject;

              public enum Kind : byte
              {
                  None,
              }

              [UnionId]
              [UnionIdKind(typeof(Kind), 0)]
              public readonly partial struct Id { }
              """
            , new IIncrementalGenerator[] { new UnionIdGenerator() }
        );

        AssertTool(compilation, "TestProject.Id+TypeConverter", UNION_ID_TOOL);
        AssertTool(compilation, "TestProject.IId_IdKindExtensions", UNION_ID_TOOL);
        AssertNotGenerated(compilation, "TestProject.Id");
    }

    [TestMethod]
    public async Task VariantConverter_IsClassified()
    {
        var compilation = await RunAsync(
              """
              using EncosyTower.Variants;

              namespace TestProject;

              [Variant(typeof(UnityEngine.Vector3))]
              public readonly partial struct Vector3Variant { }
              """
            , new IIncrementalGenerator[] { new VariantStructGenerator(), new VariantRegistrationGenerator() }
        );

        AssertTool(compilation, "TestProject.Vector3Variant+Converter", VARIANT_STRUCT_TOOL);
    }

    [TestMethod]
    public async Task DatabaseTables_AreClassified()
    {
        var compilation = await RunAsync(
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
            , new IIncrementalGenerator[] { new DatabaseGenerator() }
        );

        AssertTool(compilation, "TestProject.Db+Names+Tables", DATABASE_TOOL);
        AssertTool(compilation, "TestProject.Db+Keys+Tables", DATABASE_TOOL);
        AssertTool(compilation, "TestProject.Db+Ids+Tables", DATABASE_TOOL);
        AssertTool(compilation, "TestProject.Db+Instance+Tables", DATABASE_TOOL);
    }

    [TestMethod]
    public async Task ProcessingAsync_IsClassified()
    {
        var run = await ProcessingRequestFixture.RunAsync("""
            using EncosyTower.Processing;

            namespace TestProject;

            [Processing(ApiMode.Async, State = StateMode.Both)]
            public partial class AsyncRequest { }
            """);

        AssertTool(run.OutputCompilation, "TestProject.AsyncRequest+Async", PROCESSING_TOOL);
    }

    [TestMethod]
    public async Task PubSubAsync_IsClassified()
    {
        var run = await PubSubRuntimeFixture.RunAsync("""
            using EncosyTower.PubSub;

            namespace TestProject;

            [PubSub(ApiMode.Both)]
            public partial class Message { }
            """);

        AssertTool(run.OutputCompilation, "TestProject.Message+Async", PUBSUB_TOOL);
        AssertNotGenerated(run.OutputCompilation, "TestProject.Message");
    }

    private static async Task<Compilation> RunAsync(
          string source
        , IReadOnlyList<IIncrementalGenerator> generators
        , CancellationToken token = default
    )
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        var compilation = GeneratorTestHelper.CreateCompilation(source, INPUT_ASSEMBLY_NAME, references);

        return GeneratorTestHelper.RunDriver(GeneratorTestHelper.CreateDriver(generators), compilation, token)
            .OutputCompilation;
    }

    private static INamedTypeSymbol GetNamedType(Compilation compilation, string metadataName)
    {
        var type = compilation.GetTypeByMetadataName(metadataName);
        Assert.IsNotNull(type, metadataName);
        return type;
    }

    private static void AssertTool(Compilation compilation, string metadataName, string expectedTool)
    {
        var type = GetNamedType(compilation, metadataName);

        Assert.IsTrue(type.TryGetGeneratingTool(CancellationToken.None, out var tool), metadataName);
        Assert.AreEqual(expectedTool, tool, metadataName);
    }

    private static void AssertNotGenerated(Compilation compilation, string metadataName)
    {
        var type = GetNamedType(compilation, metadataName);

        Assert.IsFalse(type.TryGetGeneratingTool(CancellationToken.None, out _), metadataName);
    }
}
