using EncosyTower.Data.CodeRefactors;

namespace EncosyTower.SourceGen.Tests.Data;

[TestClass]
public sealed class DataCodeFixProviderTests
{
    [TestMethod]
    public Task FieldToPropertyGetInit_AppliesExactFixAndFixAll()
        => CodeFixTestHelper.VerifyAsync<DataDiagnosticAnalyzer, FieldToPropertyGetInitCodeFixProvider>(
              """
              using EncosyTower.Data;
              using UnityEngine;

              [Data]
              public partial class Row
              {
                  [SerializeField]
                  public int {|SG_DATA_0010:value|};

                  private int Get_Value() => 0;
                  private void Set_Value(int value) { }
              }
              """
            , """
              using EncosyTower.Data;
              using UnityEngine;

              [Data]
              public partial class Row
              {
                  [DataProperty(typeof(int))]
                  public int Value { get => Get_Value(); init => Set_Value(value); }

                  private int Get_Value() => 0;
                  private void Set_Value(int value) { }
              }
              """.Replace("[DataProperty(typeof(int))]\n", "[DataProperty(typeof(int))]\r\n")
            , "Replace 'value' with property (getter and init-setter)"
            , fixedDiagnostics: [
                new DiagnosticResult("SG_DATA_0011", DiagnosticSeverity.Hidden)
                    .WithSpan(8, 16, 8, 21)
                    .WithArguments("Value"),
            ]
        );

    [TestMethod]
    public Task FieldToPropertyGetOnly_AppliesExactFixAndFixAll()
        => CodeFixTestHelper.VerifyAsync<DataDiagnosticAnalyzer, FieldToPropertyGetOnlyCodeFixProvider>(
              """
              using EncosyTower.Data;
              using UnityEngine;

              [Data]
              public partial class Row
              {
                  [SerializeField]
                  public int {|SG_DATA_0010:value|};

                  private int Get_Value() => 0;
              }
              """
            , """
              using EncosyTower.Data;
              using UnityEngine;

              [Data]
              public partial class Row
              {
                  [DataProperty(typeof(int))]
                  public int Value => Get_Value();

                  private int Get_Value() => 0;
              }
              """.Replace(
                  "[DataProperty(typeof(int))]\n    public int Value => Get_Value();\n\n",
                  "[DataProperty(typeof(int))]\r\n    public int Value => Get_Value();\r\n\r\n"
              )
            , "Replace 'value' with property (only getter)"
            , fixedDiagnostics: [
                new DiagnosticResult("SG_DATA_0011", DiagnosticSeverity.Hidden)
                    .WithSpan(8, 16, 8, 21)
                    .WithArguments("Value"),
            ]
        );

    [TestMethod]
    public Task PropertyToField_AppliesExactFixAndFixAll()
        => CodeFixTestHelper.VerifyAsync<DataDiagnosticAnalyzer, PropertyToFieldCodeFixProvider>(
              """
              using EncosyTower.Data;
              using UnityEngine;

              [Data]
              public partial class Row
              {
                  [DataProperty]
                  public int {|SG_DATA_0011:Value|} { get; set; }
              }
              """
            , """
              using EncosyTower.Data;
              using UnityEngine;

              [Data]
              public partial class Row
              {
                  [SerializeField]
                  private int _value;
              }
              """.Replace("[SerializeField]\n", "[SerializeField]\r\n")
            , "Replace 'Value' with field"
            , fixedDiagnostics: [
                new DiagnosticResult("SG_DATA_0010", DiagnosticSeverity.Hidden)
                    .WithSpan(8, 17, 8, 23)
                    .WithArguments("_value"),
            ]
        );
}
