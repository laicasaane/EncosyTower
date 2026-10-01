using EncosyTower.Core.Generators.Types.Caches;

namespace EncosyTower.SourceGen.Tests.Core.Types.Caches;

[TestClass]
public class RuntimeTypeCachesGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<RuntimeTypeCachesGenerator>();

    [TestMethod]
    public Task RuntimeTypeCacheCalls_GenerateCaches()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<RuntimeTypeCachesGenerator>(
              """
              using System;
              using EncosyTower.Types;

              namespace TestProject;

              public class YourType { }
              public class SomeAttribute : Attribute { }
              public class SpecialAttribute : Attribute { }

              public partial class Usage
              {
                  public void Execute()
                  {
                      const string ASSEMBLY = "EncosyTower.SourceGen.Tests.Input";
                      RuntimeTypeCache.GetInfo<SpecialAttribute>();
                      RuntimeTypeCache.GetTypesDerivedFrom<YourType>();
                      RuntimeTypeCache.GetTypesDerivedFrom<YourType>(ASSEMBLY);
                      RuntimeTypeCache.GetTypesWithAttribute<SomeAttribute>();
                      RuntimeTypeCache.GetFieldsWithAttribute<SomeAttribute>();
                      RuntimeTypeCache.GetMethodsWithAttribute<SomeAttribute>();
                  }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<RuntimeTypeCachesGenerator>(
                    "Usage.RuntimeTypeCache.ac003e4d5245071a.g.cs"
                ),
                ExpectedGeneratedSource.Create<RuntimeTypeCachesGenerator>(
                    "Input.RuntimeTypeCacheHeader.f77403357d7eb1ba.g.cs"
                ),
            }
        );
}
