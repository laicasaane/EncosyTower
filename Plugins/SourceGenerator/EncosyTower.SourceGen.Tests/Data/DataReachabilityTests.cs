using EncosyTower.Core.Generators.EnumTemplates;
using EncosyTower.Data.Analyzers.Data;
using EncosyTower.Data.Generators.Data;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Data;

[TestClass]
public sealed class DataReachabilityTests
{
    private const string TOOL = ProducerFixtures.ENUM_TEMPLATE_TOOL;

    private const string FIELD_SOURCE = """
        using System.ComponentModel;
        using EncosyTower.Data;
        using UnityEngine;

        namespace TestProject;

        [Data]
        public partial class PlayerData
        {
            [SerializeField]
            [property: DefaultValue(ScreenType.Lobby)]
            private int _level;
        }
        """;

    [TestMethod]
    public Task DataField_GeneratedEnumMember_Reports()
        => VerifyAsync(FIELD_SOURCE, D(11, 29, 11, 45, "TestProject.ScreenType", "[Data]", "_level", TOOL));

    [TestMethod]
    public Task DataProperty_GeneratedEnumMember_Reports()
        => VerifyAsync(
              """
              using System.ComponentModel;
              using EncosyTower.Data;

              namespace TestProject;

              [Data]
              public partial class PlayerData
              {
                  [DataProperty]
                  [field: DefaultValue(ScreenType.Lobby)]
                  public int Level => Get_Level();
              }
              """
            , D(10, 26, 10, 42, "TestProject.ScreenType", "[Data]", "Level", TOOL)
        );

    [TestMethod]
    public Task DataField_HandWrittenNameOfAndTypeOf_NoDiagnostic()
        => VerifyAsync("""
            using System.ComponentModel;
            using EncosyTower.Data;
            using UnityEngine;

            namespace TestProject;

            [Data]
            public partial class PlayerData
            {
                [SerializeField]
                [property: DefaultValue(MenuScreen.Lobby)]
                private int _menu;

                [SerializeField]
                [property: DefaultValue(nameof(ScreenType.Lobby))]
                private int _name;

                [SerializeField]
                [property: DefaultValue(typeof(ScreenType))]
                private int _type;
            }
            """);

    [TestMethod]
    public Task DataField_MultipleDeclarators_ReportsOnce()
        => VerifyAsync(
              FIELD_SOURCE.Replace("private int _level;", "private int _level, _rank;", StringComparison.Ordinal)
            , D(11, 29, 11, 45, "TestProject.ScreenType", "[Data]", "_level", TOOL)
        );

    private static DiagnosticResult D(
          int startLine
        , int startColumn
        , int endLine
        , int endColumn
        , params object[] arguments
    )
        => new DiagnosticResult(DataDiagnosticAnalyzer.ForwardedAttributeUsesGeneratedType)
            .WithSpan(GeneratorAnalyzerTestHelper.INPUT_PATH, startLine, startColumn, endLine, endColumn)
            .WithArguments(arguments);

    private static Task VerifyAsync(string source, params DiagnosticResult[] expected)
        => GeneratorAnalyzerTestHelper.VerifyAsync(
              new[] {
                  ProducerFixtures.ScreenTypeProducer,
                  new NamedSource(GeneratorAnalyzerTestHelper.INPUT_PATH, source),
              }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator(), new DataGenerator() }
            , new DiagnosticAnalyzer[] {
                new DataDiagnosticAnalyzer(),
                new DataFieldAttributeWithTargetsDiagnosticSuppressor(),
                new DataPropertyAttributeWithTargetsDiagnosticSuppressor(),
            }
            , expected
        );
}
