using EncosyTower.Core.Generators.TypeWraps;

namespace EncosyTower.SourceGen.Tests.Core.TypeWraps;

[TestClass]
public class TypeWrapGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<TypeWrapGenerator>();

    [TestMethod]
    public Task WrapTypeStruct_GeneratesWrapper()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapType(typeof(int), "value")]
              public partial struct Id { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Id.TypeWrap.7b1c397e3fee068b.g.cs"),
            }
        );

    [TestMethod]
    public Task WrapRecordStruct_GeneratesWrapper()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapRecord]
              public readonly partial record struct Id(int Value);
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Id.TypeWrap.7b1c397e3fee068b.g.cs"),
            }
        );
}
