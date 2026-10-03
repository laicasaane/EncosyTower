using EncosyTower.Core.Generators.EnumExtensions;

namespace EncosyTower.SourceGen.Tests.Core.EnumExtensions;

[TestClass]
public class EnumExtensionsGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<EnumExtensionsGenerator>();

    [TestMethod]
    public Task AnnotatedEnum_GeneratesExtensions()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<EnumExtensionsGenerator>(
              """
              using EncosyTower.EnumExtensions;

              namespace TestProject;

              [EnumExtensions]
              public enum Fruit : byte
              {
                  Apple,
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<EnumExtensionsGenerator>("Fruit.EnumExtensions.e1b1e532170a23e4.g.cs"),
            }
        );

    [TestMethod]
    public Task GlobalNamespaceEnum_GeneratesExtensionMethods()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<EnumExtensionsGenerator>(
              """
              using EncosyTower.EnumExtensions;

              [EnumExtensions]
              public enum Fruit : byte
              {
                  Apple,
              }

              internal static class Usage
              {
                  public static string Name(Fruit value)
                      => value.ToStringFast();
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<EnumExtensionsGenerator>(
                    "global__Fruit.EnumExtensions.1c012f239f98b3e5.g.cs"
                ),
            }
        );
}
