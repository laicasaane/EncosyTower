using EncosyTower.Core.Generators.EnumTemplates;

namespace EncosyTower.SourceGen.Tests.Core.EnumTemplates;

[TestClass]
public class EnumTemplateGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<EnumTemplateGenerator>();

    [TestMethod]
    public Task EnumMembers_GenerateEnumFromTemplate()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<EnumTemplateGenerator>(
              """
              using EncosyTower.EnumExtensions;

              namespace TestProject;

              [EnumTemplate]
              public readonly partial struct Value_EnumTemplate { }

              [EnumMembersForTemplate(typeof(Value_EnumTemplate), 0)]
              public enum Values : byte
              {
                  First,
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<EnumTemplateGenerator>(
                    "Value_EnumTemplate.EnumTemplate.76ee53cf124e9c67.g.cs"
                ),
            }
        );

    [TestMethod]
    public Task AlternateName_PreservesAuthoredNormalization()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<EnumTemplateGenerator>(
              """
              using EncosyTower.EnumExtensions;

              namespace TestProject;

              [EnumTemplate]
              [EnumTemplateMemberFromType(typeof(Custom), 0, "Custom", "A-B")]
              public readonly partial struct Value_EnumTemplate { }

              public sealed class Custom { }
              """
            , expectedSourceCount: 1
            , expectedFragments: new[] {
                "A__B = 0,",
            }
            , unexpectedFragments: new[] {
                "I_A_x002DB",
            }
        );
}
