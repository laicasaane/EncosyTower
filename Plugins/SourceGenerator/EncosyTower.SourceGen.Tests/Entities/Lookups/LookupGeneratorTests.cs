using EncosyTower.Entities.Generators.Entities.Lookups;

namespace EncosyTower.SourceGen.Tests.Entities.Lookups;

[TestClass]
public class LookupGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<LookupGenerator>();

    [TestMethod]
    public Task ReadOnlyComponentLookup_GeneratesLookupContract()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<LookupGenerator>(
              """
              using EncosyTower.Entities;
              using Unity.Entities;

              namespace TestProject;

              public struct Component : IComponentData { }

              [Lookup(typeof(Component), true)]
              public partial struct Lookups : IComponentLookups { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<LookupGenerator>(
                    "Lookups.EntityLookup.c8276f21cd56b81d.g.cs"
                ),
            }
        );

    [TestMethod]
    public Task PhysicsComponentLookup_InGenericOuterMatchingAlias_GeneratesLatiosContract()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<LookupGenerator>(
              """
              using EncosyTower.Entities;
              using Unity.Entities;

              namespace TestProject;

              public struct Component : IComponentData { }

              public partial class Outer<LP>
              {
                  [Lookup(typeof(Component), true)]
                  public partial struct Lookups : IPhysicsComponentLookups { }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<LookupGenerator>(
                    "Lookups.EntityLookup.8579844b3786a001.g.cs"
                ),
            }
        );
}
