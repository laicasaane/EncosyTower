using EncosyTower.Core.Generators.EnumTemplates;
using EncosyTower.Persistence.Analyzers;
using EncosyTower.Persistence.Generators;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Persistence.Persistences;

[TestClass]
public sealed class PersistReachabilityTests
{
    private const string TOOL = ProducerFixtures.ENUM_TEMPLATE_TOOL;

    [TestMethod]
    public Task PersistIdField_GeneratedEnumMember_Reports()
        => VerifyAsync(
              """
              using System.ComponentModel;
              using EncosyTower.Persistences;

              namespace TestProject;

              [Persist]
              public partial class SaveData
              {
                  [property: DefaultValue(ScreenType.Lobby)]
                  private string _id = "";
              }
              """
            , D(9, 29, 9, 45, "TestProject.ScreenType", "[Persist]", "_id", TOOL)
        );

    [TestMethod]
    public Task PersistVersionField_GeneratedEnumMember_Reports()
        => VerifyAsync(
              """
              using System.ComponentModel;
              using EncosyTower.Persistences;

              namespace TestProject;

              [Persist]
              public partial class SaveData
              {
                  [property: DefaultValue(ScreenType.Shop)]
                  private int _version = 0;
              }
              """
            , D(9, 29, 9, 44, "TestProject.ScreenType", "[Persist]", "_version", TOOL)
        );

    [TestMethod]
    public Task PersistIdField_HandWrittenEnumMember_NoDiagnostic()
        => VerifyAsync("""
            using System.ComponentModel;
            using EncosyTower.Persistences;

            namespace TestProject;

            [Persist]
            public partial class SaveData
            {
                [property: DefaultValue(MenuScreen.Lobby)]
                private string _id = "";
            }
            """);

    [TestMethod]
    public Task PersistIdField_HandWrittenIdProperty_NoDiagnostic()
        => VerifyAsync("""
            using System.ComponentModel;
            using EncosyTower.Persistences;

            namespace TestProject;

            [Persist]
            public partial class SaveData
            {
                [property: DefaultValue(ScreenType.Lobby)]
                private string _id = "";

                public string Id { get; set; } = "";

                public string RawId => _id;
            }
            """);

    private static DiagnosticResult D(
          int startLine
        , int startColumn
        , int endLine
        , int endColumn
        , params object[] arguments
    )
        => new DiagnosticResult(PersistenceDiagnosticAnalyzer.ForwardedAttributeUsesGeneratedType)
            .WithSpan(GeneratorAnalyzerTestHelper.INPUT_PATH, startLine, startColumn, endLine, endColumn)
            .WithArguments(arguments);

    private static Task VerifyAsync(string source, params DiagnosticResult[] expected)
        => GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  ProducerFixtures.ScreenTypeProducer,
                  new NamedSource(GeneratorAnalyzerTestHelper.INPUT_PATH, source),
              }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator(), new PersistGenerator() }
            , new DiagnosticAnalyzer[] {
                new PersistenceDiagnosticAnalyzer(),
                new PersistFieldAttributeWithTargetsDiagnosticSuppressor(),
            }
            , expected
        );
}
