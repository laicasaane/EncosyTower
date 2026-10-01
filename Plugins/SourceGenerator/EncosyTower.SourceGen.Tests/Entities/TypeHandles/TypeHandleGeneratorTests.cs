using EncosyTower.Entities.Generators.Entities.TypeHandles;

namespace EncosyTower.SourceGen.Tests.Entities.TypeHandles;

[TestClass]
public class TypeHandleGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<TypeHandleGenerator>();

    [TestMethod]
    public Task ReadOnlyComponentTypeHandle_GeneratesTypeHandleContract()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeHandleGenerator>(
              """
              using EncosyTower.Entities;
              using Unity.Entities;

              namespace TestProject;

              public struct Component : IComponentData { }

              [TypeHandle(typeof(Component), true)]
              public partial struct Handles { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeHandleGenerator>(
                    "Handles.EntityTypeHandle.bc64aea11614d439.g.cs"
                ),
            }
        );
}
