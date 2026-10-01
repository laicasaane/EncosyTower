using EncosyTower.Core.Analyzers.EnumExtensions;

namespace EncosyTower.SourceGen.Tests.Core.EnumExtensions;

[TestClass]
public class EnumExtensionsAnalyzerTests
{
    private const string STUB_ATTRIBUTES = EnumExtensionsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<EnumExtensionsAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<EnumExtensionsAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<EnumExtensionsAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task ClassWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public static class Plain { }
            """);

    [TestMethod]
    public Task StaticClassWithEnumTypeArg_NoDiagnostics()
        => RunAsync("""
                public enum Color { Red, Green, Blue }

                [EncosyTower.EnumExtensions.EnumExtensionsFor(typeof(Color))]
                public static class ColorExt { }
            """);

    [TestMethod]
    public Task NonStaticClassWithNonEnumTypeArg_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.EnumExtensions.EnumExtensionsFor(typeof(int))]
                public class NotStatic { }
            """);

    [TestMethod]
    public Task StaticClassWithNonEnumTypeArg_ReportsTypeArgumentMustBeEnum()
        => RunAsync(
              """
                  [{|#0:EncosyTower.EnumExtensions.EnumExtensionsFor(typeof(int))|}]
                  public static class IntExt { }
              """
            , new DiagnosticResult(EnumExtensionsAnalyzer.TypeArgumentMustBeEnum).WithLocation(0).WithArguments("int")
        );
}
