using EncosyTower.Core.Generators.EnumExtensions;

namespace EncosyTower.SourceGen.Tests.Core.EnumExtensions;

[TestClass]
public class EnumExtensionsForGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<EnumExtensionsForGenerator>();

    [TestMethod]
    public Task AnnotatedClass_GeneratesExtensions()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<EnumExtensionsForGenerator>(
              """
              using System;
              using EncosyTower.EnumExtensions;

              namespace TestProject;

              [EnumExtensionsFor(typeof(DayOfWeek))]
              public static partial class DayOfWeekExtensions { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<EnumExtensionsForGenerator>(
                    "DayOfWeekExtensions.EnumExtensionsFor.c36b21572cab282b.g.cs"
                ),
            }
        );

    [TestMethod]
    public Task GlobalNamespaceClass_GeneratesExtensionMethods()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<EnumExtensionsForGenerator>(
              """
              using System;
              using EncosyTower.EnumExtensions;

              [EnumExtensionsFor(typeof(DayOfWeek))]
              public static partial class DayOfWeekExtensions { }

              internal static class Usage
              {
                  public static string Name(DayOfWeek value)
                      => value.ToStringFast();
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<EnumExtensionsForGenerator>(
                    "DayOfWeekExtensions.EnumExtensionsFor.74293e8af73dc5cd.g.cs"
                ),
            }
        );
}
