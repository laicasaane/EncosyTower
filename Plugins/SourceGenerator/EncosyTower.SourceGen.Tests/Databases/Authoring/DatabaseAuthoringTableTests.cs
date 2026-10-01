using EncosyTower.Databases.Authoring.Analyzers;

namespace EncosyTower.SourceGen.Tests.Databases.Authoring;

[TestClass]
public class DatabaseAuthoringTableTests
{
    private const string STUB_ATTRIBUTES = DatabaseAuthoringAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n    using EncosyTower.Data;\n    using EncosyTower.Data.Authoring;\n    using EncosyTower.Databases;\n    using EncosyTower.Databases.Authoring;\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<DatabaseAuthoringDiagnosticAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<DatabaseAuthoringDiagnosticAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<DatabaseAuthoringDiagnosticAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task ValidConverter_NoDiagnostics()
        => RunAsync("""
                public class IntConv { public int Convert(string s) => 0; }

                public class MyData : IData { }

                public class MyTable : DataTableAssetBase<int, MyData> { }

                [Database(typeof(IntConv))]
                public partial class MyDb
                {
                    [Table]
                    public MyTable Items { get; }
                }

                [AuthorDatabase(typeof(MyDb))]
                public partial class MyAuthor { }
            """);

    [TestMethod]
    public Task PrivateCtorConverter_ReportsMissingDefaultConstructor()
        => RunAsync(
              """
                  public class PrivateCtorConv
                  {
                      private PrivateCtorConv() { }
                      public int Convert(int x) => x;
                  }

                  public class MyData : IData { }

                  public class MyTable : DataTableAssetBase<int, MyData> { }

                  [{|#0:Database(typeof(PrivateCtorConv))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.MissingDefaultConstructor)
                .WithLocation(0)
                .WithArguments("PrivateCtorConv")
        );

    [TestMethod]
    public Task TwoStaticConvert_ReportsStaticConvertMethodAmbiguity()
        => RunAsync(
              """
                  public class StaticAmbig
                  {
                      public static int Convert(int x) => x;
                      public static string Convert(string s) => s;
                  }

                  public class MyData : IData { }

                  public class MyTable : DataTableAssetBase<int, MyData> { }

                  [{|#0:Database(typeof(StaticAmbig))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.StaticConvertMethodAmbiguity)
                .WithLocation(0)
                .WithArguments("StaticAmbig")
        );

    [TestMethod]
    public Task TwoInstanceConvert_ReportsInstancedConvertMethodAmbiguity()
        => RunAsync(
              """
                  public class InstanceAmbig
                  {
                      public int Convert(int x) => x;
                      public string Convert(string s) => s;
                  }

                  public class MyData : IData { }

                  public class MyTable : DataTableAssetBase<int, MyData> { }

                  [{|#0:Database(typeof(InstanceAmbig))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InstancedConvertMethodAmbiguity)
                .WithLocation(0)
                .WithArguments("InstanceAmbig")
        );

    [TestMethod]
    public Task NoConvertMethod_OnMember_ReportsMissingConvertMethod()
        => RunAsync(
              """
                  public class NoConvert { }

                  public class MyData : IData { }

                  public class MyTable : DataTableAssetBase<int, MyData>
                  {
                      [{|#0:DataAuthoringConverter(typeof(NoConvert))|}]
                      public int Foo { get; set; }
                  }

                  [Database]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.MissingConvertMethod)
                .WithLocation(0)
                .WithArguments("NoConvert", "int")
        );

    [TestMethod]
    public Task NoConvertMethod_InArray_ReportsMissingConvertMethodReturnType()
        => RunAsync(
              """
                  public class NoConvert { }

                  public class MyData : IData { }

                  public class MyTable : DataTableAssetBase<int, MyData> { }

                  [{|#0:Database(typeof(NoConvert))|}]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.MissingConvertMethodReturnType)
                .WithLocation(0)
                .WithArguments("NoConvert", "")
        );

    [TestMethod]
    public Task HorizontalScalar_ReportsInvalidSelection()
        => RunAsync(
              """
                  public class MyData : IData
                  {
                      [DataProperty]
                      public int Value { get; set; }
                  }

                  public class MyTable : DataTableAssetBase<int, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table, {|#0:Horizontal(typeof(MyData), nameof(MyData.Value))|}]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidHorizontalCollectionSelection)
                .WithLocation(0)
                .WithArguments("Value", "MyData", "the target property is not a collection")
        );

    [TestMethod]
    public Task HorizontalOnNonTableProperty_ReportsInvalidSelection()
        => RunAsync(
              """
                  public class MyData : IData
                  {
                      [DataProperty, {|#0:Horizontal(typeof(MyData), nameof(MyData.Values))|}]
                      public System.Collections.Generic.List<int> Values { get; set; }
                  }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidHorizontalCollectionSelection)
                .WithLocation(0)
                .WithArguments("Values", "MyData", "the attribute is not on a Table property")
        );

    [TestMethod]
    public Task HorizontalUnreachableTarget_ReportsInvalidSelection()
        => RunAsync(
              """
                  public class MyData : IData
                  {
                      [DataProperty]
                      public System.Collections.Generic.List<int> Values { get; set; }
                  }

                  public class OtherData : IData
                  {
                      [DataProperty]
                      public System.Collections.Generic.List<int> Values { get; set; }
                  }

                  public class MyTable : DataTableAssetBase<int, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table, {|#0:Horizontal(typeof(OtherData), nameof(OtherData.Values))|}]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidHorizontalCollectionSelection)
                .WithLocation(0)
                .WithArguments("Values", "OtherData", "the target type is not reachable from this table")
        );

    [TestMethod]
    public Task HorizontalAbstractTarget_ReportsInvalidSelection()
        => RunAsync(
              """
                  public abstract class BaseData : IData
                  {
                      [DataProperty]
                      public System.Collections.Generic.List<int> Values { get; set; }
                  }

                  public class MyData : BaseData { }
                  public class MyTable : DataTableAssetBase<int, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table, {|#0:Horizontal(typeof(BaseData), nameof(BaseData.Values))|}]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidHorizontalCollectionSelection)
                .WithLocation(0)
                .WithArguments("Values", "BaseData", "the target type must be a non-abstract IData type")
        );

    [TestMethod]
    public Task HorizontalMissingProperty_ReportsInvalidSelection()
        => RunAsync(
              """
                  public class MyData : IData { }

                  public class MyTable : DataTableAssetBase<int, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table, {|#0:Horizontal(typeof(MyData), "Missing")|}]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidHorizontalCollectionSelection)
                .WithLocation(0)
                .WithArguments("Missing", "MyData", "the target property does not exist")
        );

    [TestMethod]
    public Task RepeatedAliasQualifiedHorizontalSelection_IsValid()
        => RunAsync(
              """
                  using H = EncosyTower.Databases.Authoring.HorizontalAttribute;

                  public class MyData : IData
                  {
                      [DataProperty]
                      public System.Collections.Generic.List<int> Values { get; set; }
                  }

                  public class MyTable : DataTableAssetBase<int, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table, H(typeof(MyData), nameof(MyData.Values)), H(typeof(MyData), nameof(MyData.Values))]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
        );

    [TestMethod]
    public Task HorizontalConverterProducedScalar_ReportsInvalidSelection()
        => RunAsync(
              """
                  public class ValuesConverter
                  {
                      public System.Collections.Generic.List<int> Convert(string value) => new();
                  }

                  public class MyData : IData
                  {
                      [DataProperty, DataAuthoringConverter(typeof(ValuesConverter))]
                      public System.Collections.Generic.List<int> Values { get; set; }
                  }

                  public class MyTable : DataTableAssetBase<int, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table, {|#0:Horizontal(typeof(MyData), nameof(MyData.Values))|}]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidHorizontalCollectionSelection)
                .WithLocation(0)
                .WithArguments("Values", "MyData", "the target property is not a collection")
        );

    [TestMethod]
    public Task HorizontalInheritedCollectionOnDerivedRow_IsValid()
        => RunAsync(
              """
                  public abstract class HeroData : IData
                  {
                      [DataProperty]
                      public System.ReadOnlyMemory<int> Multipliers { get; set; }
                  }

                  public sealed class NewHeroData : HeroData { }
                  public class MyTable : DataTableAssetBase<int, NewHeroData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table, Horizontal(typeof(NewHeroData), nameof(NewHeroData.Multipliers))]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
        );

    [TestMethod]
    public Task HorizontalThreeArgumentTableData_IsValid()
        => RunAsync(
              """
                  public class MyData : IData
                  {
                      [DataProperty]
                      public System.Collections.Generic.List<int> Values { get; set; }
                  }

                  public class MyTable : DataTableAssetBase<int, MyData, string> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table, Horizontal(typeof(MyData), nameof(MyData.Values))]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
        );

    [TestMethod]
    public Task HorizontalConvertedIdOnThreeArgumentTable_ReportsUnreachable()
        => RunAsync(
              """
                  public class MyData : IData { }

                  public class ConvertedId : IData
                  {
                      [DataProperty]
                      public System.Collections.Generic.List<int> Values { get; set; }
                  }

                  public class MyTable : DataTableAssetBase<int, MyData, ConvertedId> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table, {|#0:Horizontal(typeof(ConvertedId), nameof(ConvertedId.Values))|}]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidHorizontalCollectionSelection)
                .WithLocation(0)
                .WithArguments("Values", "ConvertedId", "the target type is not reachable from this table")
        );

    [TestMethod]
    public Task HorizontalThroughIntermediateGenericTable_IsValid()
        => RunAsync(
              """
                  public class MyData : IData
                  {
                      [DataProperty]
                      public System.Collections.Generic.List<int> Values { get; set; }
                  }

                  public abstract class IntermediateTable<TData> : DataTableAssetBase<int, TData>
                      where TData : IData { }

                  public class MyTable : IntermediateTable<MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table, Horizontal(typeof(MyData), nameof(MyData.Values))]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
        );

    [TestMethod]
    public Task SameNameNonCanonicalTableBase_DoesNotEstablishReachability()
        => RunAsync(
              """
                  namespace Unrelated
                  {
                      public abstract class DataTableAssetBase<TDataId, TData> { }
                  }

                  public class MyData : IData
                  {
                      [DataProperty]
                      public System.Collections.Generic.List<int> Values { get; set; }
                  }

                  public class MyTable : Unrelated.DataTableAssetBase<int, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table, {|#0:Horizontal(typeof(MyData), nameof(MyData.Values))|}]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor { }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidHorizontalCollectionSelection)
                .WithLocation(0)
                .WithArguments("Values", "MyData", "the target type is not reachable from this table")
        );

    [TestMethod]
    public Task ThreeArgumentTable_GeneratedKeyEqualityCustomizationIsValidated()
        => RunAsync(
              """
                  public class MyKey : IData { }
                  public class MyData : IData { }
                  public class MyTable : DataTableAssetBase<MyKey, MyData, int> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor
                  {
                      public abstract partial class MyDataSheet
                      {
                          public partial class {|#0:__MyKey|}
                          {
                              public bool Equals(__MyKey other) => true;
                          }
                      }
                  }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidGeneratedKeyEqualityCustomization)
                .WithLocation(0)
                .WithArguments(
                    "__MyKey"
                    , "provide public bool Equals(__MyKey), public override bool Equals(object), and public "
                      + "override int GetHashCode() together"
                )
        );

    [TestMethod]
    public Task IncompleteGeneratedKeyEqualityCustomization_ReportsDiagnostic()
        => RunAsync(
              """
                  public class MyKey : IData { }
                  public class MyData : IData { }
                  public class MyTable : DataTableAssetBase<MyKey, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor
                  {
                      public abstract partial class MyDataSheet
                      {
                          public partial class {|#0:__MyKey|}
                          {
                              public bool Equals(__MyKey other) => true;
                          }
                      }
                  }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidGeneratedKeyEqualityCustomization)
                .WithLocation(0)
                .WithArguments(
                    "__MyKey"
                    , "provide public bool Equals(__MyKey), public override bool Equals(object), and public "
                      + "override int GetHashCode() together"
                )
        );

    [TestMethod]
    public Task CompleteGeneratedKeyEqualityCustomization_IsValid()
        => RunAsync(
              """
                  public class MyKey : IData { }
                  public class MyData : IData { }
                  public class MyTable : DataTableAssetBase<MyKey, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor
                  {
                      public abstract partial class MyDataSheet
                      {
                          public partial class __MyKey
                          {
                              public bool Equals(__MyKey other) => true;
                              public override bool Equals(object obj) => obj is __MyKey other && Equals(other);
                              public override int GetHashCode() => 0;
                          }
                      }
                  }
              """
        );

    [TestMethod]
    public Task NonPartialGeneratedKeyEqualityCustomization_ReportsDiagnostic()
        => RunAsync(
              """
                  public class MyKey : IData { }
                  public class MyData : IData { }
                  public class MyTable : DataTableAssetBase<MyKey, MyData> { }

                  [Database]
                  public partial class MyDb
                  {
                      [Table]
                      public MyTable Items { get; }
                  }

                  [AuthorDatabase(typeof(MyDb))]
                  public partial class MyAuthor
                  {
                      public abstract partial class MyDataSheet
                      {
                          public class {|#0:__MyKey|}
                          {
                              public bool Equals(__MyKey other) => true;
                              public override bool Equals(object obj) => obj is __MyKey other && Equals(other);
                              public override int GetHashCode() => 0;
                          }
                      }
                  }
              """
            , new DiagnosticResult(DatabaseAuthoringDiagnosticAnalyzer.InvalidGeneratedKeyEqualityCustomization)
                .WithLocation(0)
                .WithArguments("__MyKey", "the customization type must be partial")
        );
}
