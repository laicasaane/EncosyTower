using EncosyTower.Core.Analyzers.NewtonsoftAotHelpers;

namespace EncosyTower.SourceGen.Tests.Core.NewtonsoftAotHelpers;

[TestClass]
public class NewtonsoftJsonAotHelperAnalyzerTests
{
    private const string STUB_ATTRIBUTES = NewtonsoftJsonAotHelperAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<NewtonsoftJsonAotHelperAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<NewtonsoftJsonAotHelperAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<NewtonsoftJsonAotHelperAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task ConcreteClassWithBaseTypeArg_NoDiagnostics()
        => RunAsync("""
                public class Base { }

                [EncosyTower.Serialization.NewtonsoftJson.NewtonsoftJsonAotHelper(typeof(Base))]
                public partial class Helper { }
            """);

    [TestMethod]
    public Task ClassWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public abstract class Helper { }
            """);

    [TestMethod]
    public Task GenericClassWithBaseTypeArg_ReportsMustNotBeGeneric()
    {
        Assert.AreEqual("SG_NEWTONSOFT_AOT_HELPER_0003", NewtonsoftJsonAotHelperAnalyzer.MustNotBeGeneric.Id);
        return RunAsync(
              """
                  public class Base { }

                  [{|#0:EncosyTower.Serialization.NewtonsoftJson.NewtonsoftJsonAotHelper(typeof(Base))|}]
                  public partial class Helper<T> { }
              """
            , new DiagnosticResult(NewtonsoftJsonAotHelperAnalyzer.MustNotBeGeneric)
                .WithLocation(0)
                .WithArguments("Helper")
        );
    }

    [TestMethod]
    public Task AbstractClass_ReportsMustNotBeAbstract()
        => RunAsync(
              """
                  public class Base { }

                  [{|#0:EncosyTower.Serialization.NewtonsoftJson.NewtonsoftJsonAotHelper(typeof(Base))|}]
                  public abstract partial class Helper { }
              """
            , new DiagnosticResult(NewtonsoftJsonAotHelperAnalyzer.MustNotBeAbstract)
                .WithLocation(0)
                .WithArguments("Helper")
        );

    [TestMethod]
    public Task AttributeWithoutTypeArg_ReportsBaseTypeMustBeProvided()
        => RunAsync(
              """
                  [{|#0:EncosyTower.Serialization.NewtonsoftJson.NewtonsoftJsonAotHelper|}]
                  public partial class Helper { }
              """
            , new DiagnosticResult(NewtonsoftJsonAotHelperAnalyzer.BaseTypeMustBeProvided)
                .WithLocation(0)
                .WithArguments("Helper")
        );
}
