using EncosyTower.Entities.Stats.Generators;

namespace EncosyTower.SourceGen.Tests.Entities.Stats;

[TestClass]
public class StatCollectionGeneratorTests
{
    private const string NESTED_ACCESSIBILITY_SOURCE = """
        using System;
        using EncosyTower.Entities.Stats;
        using Unity.Entities;

        namespace TestProject;

        [StatSystem(StatDataSize.Size8)]
        public static partial class StatsApi { }

        {{ACCESS}} partial class Outer
        {
            [StatCollection(typeof(StatsApi), 1000)]
            public partial struct Stats : IComponentData
            {
                [StatData(StatVariantType.Float)]
                public partial struct Hp { }
            }
        }
        """;

    private const string TOP_LEVEL_ACCESSIBILITY_SOURCE = """
        using System;
        using EncosyTower.Entities.Stats;
        using Unity.Entities;

        namespace TestProject;

        [StatSystem(StatDataSize.Size8)]
        public static partial class StatsApi { }

        [StatCollection(typeof(StatsApi), 1000)]
        {{ACCESS}} partial struct Stats : IComponentData
        {
            [StatData(StatVariantType.Float)]
            public partial struct Hp { }
        }
        """;

    private const string NESTED_STATS_SOURCE = """
        using System;
        using EncosyTower.Entities.Stats;
        using Unity.Entities;

        namespace TestProject;

        [StatSystem(StatDataSize.Size8)]
        public static partial class StatsApi { }

        [StatCollection(typeof(StatsApi), 1000)]
        public partial struct Stats : IComponentData
        {
            [StatData(StatVariantType.Float)]
            public partial struct Hp { }

            [StatData(typeof(Rank))]
            public partial struct Level { }
        }
        """;

    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<StatCollectionGenerator>();

    [TestMethod]
    public Task CollectionWithNestedStat_GeneratesAllStatsContracts()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<StatCollectionGenerator>(
              """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              [StatCollection(typeof(StatsApi), 1000)]
              public partial struct Stats : IComponentData
              {
                  [StatData(StatVariantType.Float)]
                  public partial struct Hp { }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<StatCollectionGenerator>(
                    "Stats.StatCollection.664f25990c3c4b5a.g.cs"
                ),
                ExpectedGeneratedSource.Create<StatDataGenerator>(
                    "Hp.StatData.b2ac3783b93b6fc7.g.cs"
                ),
                ExpectedGeneratedSource.Create<StatSystemGenerator>(
                    "StatsApi.StatSystem.abfa7bf43d46f7f6.g.cs"
                ),
            }
            , new IIncrementalGenerator[] {
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
        );

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public Task RejectedNestedStatData_IsSkipped(bool includeRejectedEntry)
    {
        var rejectedEntry = includeRejectedEntry
            ? "    [StatData(typeof(int))]\n    public partial struct Broken { }"
            : string.Empty;

        return GeneratorTestHelper.VerifyGeneratedSourcesAsync<StatCollectionGenerator>(
              $$"""
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              public enum Rank : byte { Low, High }

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              [StatCollection(typeof(StatsApi), 1000)]
              public partial struct Stats : IComponentData
              {
                  [StatData(StatVariantType.Float)]
                  public partial struct Hp { }

                  [StatData(typeof(Rank))]
                  public partial struct Level { }
              {{rejectedEntry}}
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<StatCollectionGenerator>("Stats.StatCollection.664f25990c3c4b5a.g.cs"),
                ExpectedGeneratedSource.Create<StatDataGenerator>("Hp.StatData.b2ac3783b93b6fc7.g.cs"),
                ExpectedGeneratedSource.Create<StatDataGenerator>("Level.StatData.c6ffec46951b8413.g.cs"),
                ExpectedGeneratedSource.Create<StatSystemGenerator>("StatsApi.StatSystem.abfa7bf43d46f7f6.g.cs"),
            }
            , new IIncrementalGenerator[] {
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
        );
    }

    [TestMethod]
    public Task NestedStatDataEnumBecomesStruct_RemovesEntry()
        => GeneratorTestHelper.VerifyCrossFileEditAsync(
              new IIncrementalGenerator[] {
                  new StatCollectionGenerator(),
                  new StatDataGenerator(),
                  new StatSystemGenerator(),
              }
            , new[] {
                new NamedSource("Stats.cs", NESTED_STATS_SOURCE),
                new NamedSource("Rank.cs", "namespace TestProject; public enum Rank : byte { Low, High }"),
            }
            , new[] {
                new NamedSource("Stats.cs", NESTED_STATS_SOURCE),
                new NamedSource("Rank.cs", "namespace TestProject; public struct Rank { }"),
            }
            , new[] {
                "Stats.StatCollection.664f25990c3c4b5a.g.cs",
                "Level.StatData.c6ffec46951b8413.g.cs",
            }
        );

    [TestMethod]
    public Task NestedCollection_GeneratesNamespaceExtensions()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<StatCollectionGenerator>(
              """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              public partial class Outer
              {
                  public enum Rank : byte { Low, High }

                  [StatCollection(typeof(StatsApi), 1000)]
                  public partial struct Stats : IComponentData
                  {
                      [StatData(StatVariantType.Float)]
                      public partial struct Hp { }

                      [StatData(typeof(Rank))]
                      public partial struct Level { }
                  }
              }

              internal sealed class StatsAuthoring : UnityEngine.MonoBehaviour
              {
                  private sealed class Baker : Baker<StatsAuthoring>
                  {
                      public override void Bake(StatsAuthoring authoring)
                      {
                          var entity = GetEntity(authoring, TransformUsageFlags.None);

                          Outer.Stats.Baker.Bake(this, entity)
                              .CreateStat(Outer.Stats.Hp.Params.Create(1f))
                              .CreateStat(Outer.Stats.Level.Params.Create(Outer.Rank.High))
                              .CreateComponentData<Outer.Stats>()
                              .AddComponentToEntity();
                      }
                  }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<StatCollectionGenerator>(
                    "Stats.StatCollection.a8992c15e23b16d3.g.cs"
                ),
                ExpectedGeneratedSource.Create<StatCollectionGenerator>(
                    "Stats.StatCollectionExtensions.4d4ed4b6afcf38cc.g.cs"
                ),
                ExpectedGeneratedSource.Create<StatDataGenerator>(
                    "Hp.StatData.e8085ba9af6b1f18.g.cs"
                ),
                ExpectedGeneratedSource.Create<StatDataGenerator>(
                    "Level.StatData.bd661929e4b32e5c.g.cs"
                ),
                ExpectedGeneratedSource.Create<StatSystemGenerator>(
                    "StatsApi.StatSystem.abfa7bf43d46f7f6.g.cs"
                ),
            }
            , new IIncrementalGenerator[] {
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
        );

    [TestMethod]
    public Task CollectionNestedInInternalType_GeneratesInternalExtensions()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<StatCollectionGenerator>(
              """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              internal partial class Outer
              {
                  [StatCollection(typeof(StatsApi), 1000)]
                  public partial struct Stats : IComponentData
                  {
                      [StatData(StatVariantType.Float)]
                      public partial struct Hp { }
                  }
              }

              internal static class Usage
              {
                  public static void Add(Outer.Stats.Baker<Outer.Stats> baker)
                      => baker.AddComponentToEntity();
              }
              """
            , expectedSourceCount: 2
            , expectedFragments: new[] {
                "\n    static partial class StatsExtensions\n",
                "internal static void AddComponentToEntity<TComponentData>("
                    + "this global::TestProject.Outer.Stats.Baker<TComponentData> baker)",
            }
            , unexpectedFragments: new[] {
                "public static partial class StatsExtensions",
            }
            , additionalGenerators: new IIncrementalGenerator[] {
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
        );

    [TestMethod]
    public Task PrivateNestedCollection_GeneratesStaticHelper()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<StatCollectionGenerator>(
              """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              public partial class Outer
              {
                  [StatCollection(typeof(StatsApi), 1000)]
                  private partial struct Stats : IComponentData
                  {
                      [StatData(StatVariantType.Float)]
                      public partial struct Hp { }
                  }

                  private static void Add(Stats.Baker<Stats> baker)
                      => StatsExtensions.AddComponentToEntity(baker);
              }
              """
            , expectedSourceCount: 1
            , expectedFragments: new[] {
                "private static partial class StatsExtensions",
                "public static void AddComponentToEntity<TComponentData>(Stats.Baker<TComponentData> baker)",
                "StatsExtensions.TryGetBaseValue(options.hp)",
                "StatsExtensions.TryGetCurrentValue(options.hp)",
                "new(StatsExtensions.TryGetValuePair(hp), (g__ETES.StatHandle)handles.hp);",
            }
            , unexpectedFragments: new[] {
                "(this ",
            }
            , additionalGenerators: new IIncrementalGenerator[] {
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
        );

    [TestMethod]
    public Task SameNamedCollections_ShareExtensionsClass()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<StatCollectionGenerator>(
              """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              [StatCollection(typeof(StatsApi), 1000)]
              public partial struct Stats : IComponentData
              {
                  [StatData(StatVariantType.Float)]
                  public partial struct Hp { }
              }

              public partial class A
              {
                  [StatCollection(typeof(StatsApi), 2000)]
                  public partial struct Stats : IComponentData
                  {
                      [StatData(StatVariantType.Float)]
                      public partial struct Hp { }
                  }
              }

              internal partial struct B
              {
                  [StatCollection(typeof(StatsApi), 3000)]
                  public partial struct Stats : IComponentData
                  {
                      [StatData(StatVariantType.Int)]
                      public partial struct Mp { }
                  }
              }

              internal static class Usage
              {
                  public static void Add(
                        Stats.Baker<Stats> top
                      , A.Stats.Baker<A.Stats> a
                      , B.Stats.Baker<B.Stats> b
                  )
                  {
                      top.AddComponentToEntity();
                      a.AddComponentToEntity();
                      b.AddComponentToEntity();
                  }
              }
              """
            , expectedSourceCount: 5
            , expectedFragments: new[] {
                "public static void AddComponentToEntity<TComponentData>(this Stats.Baker<TComponentData> baker)",
                "public static void AddComponentToEntity<TComponentData>("
                    + "this global::TestProject.A.Stats.Baker<TComponentData> baker)",
                "internal static void AddComponentToEntity<TComponentData>("
                    + "this global::TestProject.B.Stats.Baker<TComponentData> baker)",
            }
            , unexpectedFragments: Array.Empty<string>()
            , additionalGenerators: new IIncrementalGenerator[] {
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
        );

    [TestMethod]
    public Task CollectionInGenericType_ProducesNoOutput()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<StatCollectionGenerator>(
              """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              public partial class Outer<T>
              {
                  [StatCollection(typeof(StatsApi), 1000)]
                  public partial struct Stats : IComponentData
                  {
                      [StatData(StatVariantType.Float)]
                      public partial struct Hp { }
                  }
              }
              """
            , expectedSourceCount: 0
            , expectedFragments: Array.Empty<string>()
            , unexpectedFragments: Array.Empty<string>()
            , additionalGenerators: new IIncrementalGenerator[] {
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
        );

    [DataTestMethod]
    [DataRow("public", "internal")]
    [DataRow("internal", "public")]
    public Task NestedCollectionAccessibilityEdit_RegeneratesExtensions(string before, string after)
        => GeneratorTestHelper.VerifyReusedDriverEditAsync(
              new IIncrementalGenerator[] {
                  new StatCollectionGenerator(),
                  new StatDataGenerator(),
                  new StatSystemGenerator(),
              }
            , NESTED_ACCESSIBILITY_SOURCE.Replace("{{ACCESS}}", before)
            , NESTED_ACCESSIBILITY_SOURCE.Replace("{{ACCESS}}", after)
            , new[] { "StatCollectionGenerator.Outputs" }
        );

    [TestMethod]
    public Task InternalCollection_GeneratesInternalExtensions()
        => GeneratorTestHelper.VerifyGeneratedSourcesWithProducersAsync<StatCollectionGenerator>(
              """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              [StatCollection(typeof(StatsApi), 1000)]
              internal partial struct Stats : IComponentData
              {
                  [StatData(StatVariantType.Float)]
                  public partial struct Hp { }
              }

              internal static class Usage
              {
                  public static void Add(Stats.Baker<Stats> baker)
                      => baker.AddComponentToEntity();
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<StatCollectionGenerator>(
                    "Stats.StatCollection.664f25990c3c4b5a.g.cs"
                ),
            }
            , new IIncrementalGenerator[] {
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
        );

    [DataTestMethod]
    [DataRow("public", "internal")]
    [DataRow("internal", "public")]
    public Task TopLevelCollectionAccessibilityEdit_RegeneratesExtensions(string before, string after)
        => GeneratorTestHelper.VerifyReusedDriverEditAsync(
              new IIncrementalGenerator[] {
                  new StatCollectionGenerator(),
                  new StatDataGenerator(),
                  new StatSystemGenerator(),
              }
            , TOP_LEVEL_ACCESSIBILITY_SOURCE.Replace("{{ACCESS}}", before)
            , TOP_LEVEL_ACCESSIBILITY_SOURCE.Replace("{{ACCESS}}", after)
            , new[] { "StatCollectionGenerator.Outputs" }
        );

    [TestMethod]
    public Task NestedStatData_PlainAttributes_GenerateEntries()
        => VerifyNestedStatDataAsync("[StatData(StatVariantType.Float)]", "[StatData(typeof(Rank))]");

    [DataTestMethod]
    [DataRow("[StatDataAttribute(StatVariantType.Float)]", "[StatDataAttribute(typeof(Rank))]")]
    [DataRow("[StatData(EncosyTower.Entities.Stats.StatVariantType.Float)]", "[StatData(typeof(Rank))]")]
    [DataRow("[StatData(global::EncosyTower.Entities.Stats.StatVariantType.Float)]", "[StatData(typeof(Rank))]")]
    [DataRow(
          "[EncosyTower.Entities.Stats.StatDataAttribute(StatVariantType.Float)]"
        , "[global::EncosyTower.Entities.Stats.StatDataAttribute(typeof(Rank))]"
    )]
    [DataRow(
          "[EncosyTower.Entities.Stats.StatData(StatVariantType.Float)]"
        , "[global::EncosyTower.Entities.Stats.StatData(typeof(Rank))]"
    )]
    public Task NestedStatData_AttributeSpellings_ProduceSameEntries(string hpAttribute, string levelAttribute)
        => VerifyNestedStatDataAsync(hpAttribute, levelAttribute);

    [TestMethod]
    public Task UnrelatedStatDataNamespace_QualifiedEntry_GeneratesEntry()
        => VerifyUnrelatedStatDataAsync(string.Empty);

    [TestMethod]
    public Task UnrelatedStatDataAttribute_IsNotAnEntry()
        => VerifyUnrelatedStatDataAsync("[StatData(StatVariantType.Int)] public partial struct NotAStat { }");

    [TestMethod]
    public Task SingleValueEntries_GenerateCompilingCollection()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<StatCollectionGenerator>(
              """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              public enum Rank : byte { Low, High }

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              [StatCollection(typeof(StatsApi), 1000)]
              public partial struct Stats : IComponentData
              {
                  [StatData(StatVariantType.Float)]
                  public partial struct Hp { }

                  [StatData(StatVariantType.Int, SingleValue = true)]
                  public partial struct Gold { }

                  [StatData(typeof(Rank), SingleValue = true)]
                  public partial struct Level { }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<StatCollectionGenerator>(
                    "Stats.StatCollection.664f25990c3c4b5a.g.cs"
                ),
                ExpectedGeneratedSource.Create<StatDataGenerator>(
                      "Hp.StatData.b2ac3783b93b6fc7.g.cs"
                    , testMethod: nameof(CollectionWithNestedStat_GeneratesAllStatsContracts)
                ),
                ExpectedGeneratedSource.Create<StatDataGenerator>(
                    "Gold.StatData.9dc44cfc1855d505.g.cs"
                ),
                ExpectedGeneratedSource.Create<StatDataGenerator>(
                    "Level.StatData.c6ffec46951b8413.g.cs"
                ),
                ExpectedGeneratedSource.Create<StatSystemGenerator>(
                      "StatsApi.StatSystem.abfa7bf43d46f7f6.g.cs"
                    , testMethod: nameof(CollectionWithNestedStat_GeneratesAllStatsContracts)
                ),
            }
            , new IIncrementalGenerator[] {
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
        );

    [DataTestMethod]
    [DataRow("public partial record struct Stats : IComponentData")]
    [DataRow("public partial record struct Stats(int Extra) : IComponentData")]
    [DataRow("public partial record struct Stats() : IComponentData")]
    [DataRow("public readonly partial record struct Stats : IComponentData")]
    [DataRow("public readonly partial struct Stats : IComponentData")]
    public Task UnsupportedCollectionTarget_IsSkipped(string declaration)
        => GeneratorTestHelper.VerifyNoOutputAsync<StatCollectionGenerator>(
              $$"""
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              [StatCollection(typeof(StatsApi), 1000)]
              {{declaration}}
              {
                  [StatData(StatVariantType.Float)]
                  public partial struct Hp { }
              }
              """
        );

    private static Task VerifyNestedStatDataAsync(string hpAttribute, string levelAttribute)
        => GeneratorTestHelper.VerifyGeneratedSourcesWithProducersAsync<StatCollectionGenerator>(
              $$"""
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              public enum Rank : byte { Low, High }

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              [StatCollection(typeof(StatsApi), 1000)]
              public partial struct Stats : IComponentData
              {
                  {{hpAttribute}}
                  public partial struct Hp { }

                  {{levelAttribute}}
                  public partial struct Level { }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<StatCollectionGenerator>(
                      "Stats.StatCollection.664f25990c3c4b5a.g.cs"
                    , testMethod: nameof(NestedStatData_PlainAttributes_GenerateEntries)
                ),
            }
            , new IIncrementalGenerator[] {
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
        );

    private static Task VerifyUnrelatedStatDataAsync(string unrelatedEntry)
        => GeneratorTestHelper.VerifyGeneratedSourcesWithProducersAsync<StatCollectionGenerator>(
              $$"""
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace Unrelated
              {
                  [AttributeUsage(AttributeTargets.Struct)]
                  public sealed class StatDataAttribute : Attribute
                  {
                      public StatDataAttribute(StatVariantType valueType) { }
                  }
              }

              namespace TestProject
              {
                  using Unrelated;

                  [StatSystem(StatDataSize.Size8)]
                  public static partial class StatsApi { }

                  [StatCollection(typeof(StatsApi), 1000)]
                  public partial struct Stats : IComponentData
                  {
                      [EncosyTower.Entities.Stats.StatData(StatVariantType.Float)]
                      public partial struct Hp { }

                      {{unrelatedEntry}}
                  }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<StatCollectionGenerator>(
                      "Stats.StatCollection.664f25990c3c4b5a.g.cs"
                    , testMethod: nameof(UnrelatedStatDataNamespace_QualifiedEntry_GeneratesEntry)
                ),
            }
            , new IIncrementalGenerator[] {
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
        );
}
