using EncosyTower.Data.CodeRefactors;

namespace EncosyTower.SourceGen.Tests.Data;

[TestClass]
public sealed class DataCodeRefactorDiagnosticAnalyzerTests
{
    [TestMethod]
    public Task DataSerializedField_ReportsFieldCanBeReplaced()
        => AnalyzerTestHelper.VerifyAsync<DataDiagnosticAnalyzer>(
              """
              using EncosyTower.Data;
              using UnityEngine;

              [Data]
              public partial class Row
              {
                  [SerializeField]
                  public int {|#0:value|};
              }
              """
            , [
                new DiagnosticResult("SG_DATA_0010", DiagnosticSeverity.Hidden).WithLocation(0).WithArguments("value"),
            ]
        );

    [TestMethod]
    public Task DataProperty_ReportsPropertyCanBeReplaced()
        => AnalyzerTestHelper.VerifyAsync<DataDiagnosticAnalyzer>(
              """
              using EncosyTower.Data;

              [Data]
              public partial class Row
              {
                  [DataProperty]
                  public int {|#0:Value|} { get; set; }
              }
              """
            , [
                new DiagnosticResult("SG_DATA_0011", DiagnosticSeverity.Hidden).WithLocation(0).WithArguments("Value"),
            ]
        );

    [TestMethod]
    public Task MissingTypeOrMemberMarker_ReportsNothing()
        => AnalyzerTestHelper.VerifyAsync<DataDiagnosticAnalyzer>(
            """
            using EncosyTower.Data;
            using UnityEngine;

            public partial class UnmarkedRow
            {
                [SerializeField]
                public int value;
            }

            [Data]
            public partial class MarkedRow
            {
                public int Value { get; set; }
            }
            """
        );
}
