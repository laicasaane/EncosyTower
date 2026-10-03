using Microsoft.CodeAnalysis.CSharp;

namespace EncosyTower.SourceGen.Tests.Common;

[TestClass]
public sealed class GeneratedTypeFullNameTests
{
    private const string SOURCE = """
        namespace TestProject.Models
        {
            public static class StatsApi
            {
                public struct Stat { }
            }

            public class Outer<T>
            {
                public class Middle
                {
                    public enum Inner { }
                }
            }

            public struct Plain { }
        }

        public struct GlobalOuter
        {
            public struct GlobalInner { }
        }
        """;

    [DataTestMethod]
    [DataRow("TestProject.Models.Plain", "TestProject.Models.Plain")]
    [DataRow("TestProject.Models.StatsApi+Stat", "TestProject.Models.StatsApi+Stat")]
    [DataRow("TestProject.Models.Outer`1+Middle+Inner", "TestProject.Models.Outer<T>+Middle+Inner")]
    [DataRow("GlobalOuter", "GlobalOuter")]
    [DataRow("GlobalOuter+GlobalInner", "GlobalOuter+GlobalInner")]
    public void ToGeneratedTypeFullName_JoinsContainingTypesWithPlus(string metadataName, string expected)
    {
        var compilation = CSharpCompilation.Create(
              "GeneratedTypeFullNameTests"
            , new[] { CSharpSyntaxTree.ParseText(SOURCE) }
            , new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) }
        );
        var type = compilation.GetTypeByMetadataName(metadataName);

        Assert.IsNotNull(type, metadataName);
        Assert.AreEqual(expected, type.ToGeneratedTypeFullName());
    }

    [TestMethod]
    public void ToGeneratedTypeFullName_ConstructedGeneric_KeepsTypeArguments()
    {
        var compilation = CSharpCompilation.Create(
              "GeneratedTypeFullNameTests"
            , new[] { CSharpSyntaxTree.ParseText(SOURCE) }
            , new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) }
        );
        var outer = compilation.GetTypeByMetadataName("TestProject.Models.Outer`1");
        var plain = compilation.GetTypeByMetadataName("TestProject.Models.Plain");

        Assert.IsNotNull(outer);
        Assert.IsNotNull(plain);
        Assert.AreEqual("TestProject.Models.Outer<Plain>", outer.Construct(plain).ToGeneratedTypeFullName());
        Assert.AreEqual("TestProject.Models.Outer<int>", outer.Construct(
            compilation.GetSpecialType(SpecialType.System_Int32)
        ).ToGeneratedTypeFullName());
    }
}
