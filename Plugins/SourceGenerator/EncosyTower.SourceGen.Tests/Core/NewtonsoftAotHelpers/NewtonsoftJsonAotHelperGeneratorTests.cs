using EncosyTower.Core.Generators.NewtonsoftAotHelpers;

namespace EncosyTower.SourceGen.Tests.Core.NewtonsoftAotHelpers;

[TestClass]
public class NewtonsoftJsonAotHelperGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<NewtonsoftJsonAotHelperGenerator>();

    [TestMethod]
    public Task GenericHelper_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<NewtonsoftJsonAotHelperGenerator>(
            """
            using EncosyTower.Serialization.NewtonsoftJson;

            namespace TestProject;

            public class BaseModel { }

            [NewtonsoftJsonAotHelper(typeof(BaseModel))]
            public static partial class Helper<T> { }
            """
        );

    [TestMethod]
    public Task ConcreteHelper_GeneratesSupportedConcreteTypes()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<NewtonsoftJsonAotHelperGenerator>(
              """
              using System.Collections.Generic;
              using EncosyTower.Serialization.NewtonsoftJson;

              namespace TestProject;

              public class BaseModel { }
              public class GenericDerived<T> : BaseModel { }
              public sealed class ConcreteDerived : BaseModel
              {
                  public List<ConcreteDerived> Items = new();
              }

              [NewtonsoftJsonAotHelper(typeof(BaseModel))]
              public static partial class Helper { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<NewtonsoftJsonAotHelperGenerator>(
                    "Helper.NewtonsoftJsonAotHelper.9a4fcae943bfb12b.g.cs"
                ),
            }
        );
}
