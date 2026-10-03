using EncosyTower.Entities.Generators.Entities.Lookups;
using EncosyTower.Entities.Generators.Entities.TypeHandles;
using EncosyTower.Entities.Stats.Generators;

namespace EncosyTower.SourceGen.Tests.Entities;

[TestClass]
public sealed class EntitiesContainingTypeTests
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
              "StatCollection moved out of its containing type"
            , static () => new IIncrementalGenerator[] {
                new StatCollectionGenerator(),
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
            , """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              [StatSystem(StatDataSize.Size8)]
              public static partial class StatsApi { }

              {{EDIT}}
              """
            , """
              public partial class Outer
              {
                  [StatCollection(typeof(StatsApi), 1000)]
                  public partial struct Stats : IComponentData
                  {
                      [StatData(StatVariantType.Float)]
                      public partial struct Hp { }
                  }
              }
              """
            , """
              [StatCollection(typeof(StatsApi), 1000)]
              public partial struct Stats : IComponentData
              {
                  [StatData(StatVariantType.Float)]
                  public partial struct Hp { }
              }
              """
            , new[] { "StatCollectionGenerator.Outputs", "StatDataGenerator.Outputs" }
        );

        yield return new(
              "StatData and StatSystem containing type kind"
            , static () => new IIncrementalGenerator[] { new StatDataGenerator(), new StatSystemGenerator() }
            , """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace TestProject;

              {{EDIT}}
              {
                  [StatSystem(StatDataSize.Size8)]
                  public static partial class StatsApi { }

                  [StatData(StatVariantType.Float)]
                  public partial struct Hp { }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "StatDataGenerator.Outputs", "StatSystemGenerator.Outputs" }
        );

        yield return new(
              "StatCollection typeof StatData imported namespace"
            , static () => new IIncrementalGenerator[] {
                new StatCollectionGenerator(),
                new StatDataGenerator(),
                new StatSystemGenerator(),
            }
            , """
              using System;
              using EncosyTower.Entities.Stats;
              using Unity.Entities;

              namespace A { public enum Rank : byte { Low, High } }
              namespace B { public enum Rank : byte { Low, High } }

              namespace TestProject.Api
              {
                  [StatSystem(StatDataSize.Size8)]
                  public static partial class StatsApi { }
              }

              namespace TestProject
              {
                  {{EDIT}}

                  [StatCollection(typeof(Api.StatsApi), 1000)]
                  public partial struct Stats : IComponentData
                  {
                      [StatData(typeof(Rank))]
                      public partial struct Level { }
                  }
              }
              """
            , "using A;"
            , "using B;"
            , new[] { "StatCollectionGenerator.Outputs" }
        );

        yield return new(
              "TypeHandle containing type kind"
            , static () => new IIncrementalGenerator[] { new TypeHandleGenerator() }
            , """
              using EncosyTower.Entities;
              using Unity.Entities;

              namespace TestProject;

              public struct Component : IComponentData { }

              {{EDIT}}
              {
                  [TypeHandle(typeof(Component), true)]
                  public partial struct Handles { }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "TypeHandleGenerator.Outputs" }
        );

        yield return new(
              "TypeHandle namespace"
            , static () => new IIncrementalGenerator[] { new TypeHandleGenerator() }
            , """
              using EncosyTower.Entities;
              using Unity.Entities;

              namespace Shared
              {
                  public struct Component : IComponentData { }
              }

              {{EDIT}}
              {
                  [TypeHandle(typeof(Shared.Component), true)]
                  public partial struct Handles { }
              }
              """
            , ReusedDriverEditCase.TEST_NAMESPACE
            , ReusedDriverEditCase.OTHER_NAMESPACE
            , new[] { "TypeHandleGenerator.Outputs" }
        );

        yield return new(
              "Lookup containing type kind"
            , static () => new IIncrementalGenerator[] { new LookupGenerator() }
            , """
              using EncosyTower.Entities;
              using Unity.Entities;

              namespace TestProject;

              public struct Component : IComponentData { }

              {{EDIT}}
              {
                  [Lookup(typeof(Component), true)]
                  public partial struct Lookups : IComponentLookups { }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "LookupGenerator.Outputs" }
        );

        yield return new(
              "Lookup namespace"
            , static () => new IIncrementalGenerator[] { new LookupGenerator() }
            , """
              using EncosyTower.Entities;
              using Unity.Entities;

              namespace Shared
              {
                  public struct Component : IComponentData { }
              }

              {{EDIT}}
              {
                  [Lookup(typeof(Shared.Component), true)]
                  public partial struct Lookups : IComponentLookups { }
              }
              """
            , ReusedDriverEditCase.TEST_NAMESPACE
            , ReusedDriverEditCase.OTHER_NAMESPACE
            , new[] { "LookupGenerator.Outputs" }
        );
    }

    private static IEnumerable<SpecEqualityCase> CreateEqualityCases()
    {
        yield return new(
              "StatCollectionSpec.containingTypes"
            , static changed => new StatCollectionSpec {
                typeName = "Stats",
                typeNamespace = "TestProject",
                statSystemFullTypeName = "global::TestProject.StatsApi",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "StatCollectionSpec.StatDataSpec.valueTypeFullName"
            , static changed => new StatCollectionSpec.StatDataSpec {
                typeName = "Level",
                fieldName = "level",
                valueType = "Rank",
                valueTypeFullName = changed ? "global::B.Rank" : "global::A.Rank",
            }
        );

        yield return new(
              "StatDataSpec.containingTypes"
            , static changed => new StatDataSpec {
                typeName = "Hp",
                typeNamespace = "TestProject",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "StatSystemSpec.containingTypes"
            , static changed => new StatSystemSpec {
                typeName = "StatsApi",
                typeNamespace = "TestProject",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "TypeHandleSpec.containingTypes"
            , static changed => new TypeHandleSpec {
                structName = "Handles",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "TypeHandleSpec.structFullName"
            , static changed => new TypeHandleSpec {
                structName = "Handles",
                structFullName = changed ? "global::OtherProject.Handles" : "global::TestProject.Handles",
            }
        );

        yield return new(
              "LookupSpec.containingTypes"
            , static changed => new LookupSpec {
                structName = "Lookups",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "LookupSpec.structFullName"
            , static changed => new LookupSpec {
                structName = "Lookups",
                structFullName = changed ? "global::OtherProject.Lookups" : "global::TestProject.Lookups",
            }
        );
    }
}
