using EncosyTower.Core.Analyzers.TypeWraps;

namespace EncosyTower.SourceGen.Tests.Core.TypeWraps;

[TestClass]
public sealed class TypeWrapDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = TypeWrapsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<TypeWrapDiagnosticAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<TypeWrapDiagnosticAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<TypeWrapDiagnosticAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task WrapType_OnPartialStruct_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapType(typeof(int))]
                public partial struct Wrapper { }
            """);

    [TestMethod]
    public Task WrapType_OnPartialClass_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapType(typeof(int))]
                public partial class Wrapper { }
            """);

    [TestMethod]
    public Task WrapType_OnNestedStruct_NoDiagnostics()
        => RunAsync("""
                public partial class Outer
                {
                    [EncosyTower.TypeWraps.WrapType(typeof(int))]
                    public partial struct Wrapper { }
                }
            """);

    [TestMethod]
    public Task WrapType_OnGenericStruct_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapType(typeof(int))]
                public partial struct Wrapper<T> { }
            """);

    [TestMethod]
    public Task WrapType_OnRecord_ReportsWrapTypeOnRecord()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int))|}]
                  public partial record Wrapper;
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapTypeOnRecord).WithLocation(0).WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapType_OnRecordClass_ReportsWrapTypeOnRecord()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int))|}]
                  public partial record class Wrapper(int Value);
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapTypeOnRecord).WithLocation(0).WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapType_NullArg_ReportsNotTypeOfExpression()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(null)|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.NotTypeOfExpression)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapRecord_OnPartialRecord_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapRecord]
                public partial record Wrapper(int Value);
            """);

    [TestMethod]
    public Task WrapRecord_OnPartialRecordStruct_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapRecord]
                public partial record struct Wrapper(int Value);
            """);

    [TestMethod]
    public Task WrapRecord_OnRecordWithoutParameters_ReportsRequiresOneParameter()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapRecord|}]
                  public partial record Wrapper;
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapRecordRequiresOneParameter)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapRecord_OnRecordWithTwoParameters_ReportsRequiresOneParameter()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapRecord|}]
                  public partial record Wrapper(int A, int B);
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapRecordRequiresOneParameter)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapRecord_OnNonRecordClass_ReportsWrapRecordOnNonRecord()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapRecord|}]
                  public partial class Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapRecordOnNonRecord)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapRecord_OnNonRecordStruct_ReportsWrapRecordOnNonRecord()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapRecord|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapRecordOnNonRecord)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapType_OnClassWithBaseClass_ReportsInheritsBaseClass()
        => RunAsync(
              """
                  public class BaseType { }

                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int))|}]
                  public partial class Wrapper : BaseType { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapperInheritsBaseClass)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapRecord_OnRecordClassWithBaseClass_ReportsInheritsBaseClass()
        => RunAsync(
              """
                  public record BaseType;

                  [{|#0:EncosyTower.TypeWraps.WrapRecord|}]
                  public partial record Wrapper(int Value) : BaseType;
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrapperInheritsBaseClass)
                .WithLocation(0)
                .WithArguments("Wrapper")
        );

    [TestMethod]
    public Task WrapType_TwoArgValidIdentifier_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapType(typeof(int), "Inner")]
                public partial struct Wrapper { }
            """);

    [TestMethod]
    public Task WrapType_TwoArgEmptyString_ReportsInvalidMemberName()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int), "")|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.InvalidMemberName)
                .WithLocation(0)
                .WithArguments("Wrapper", "")
        );

    [TestMethod]
    public Task WrapType_TwoArgNullString_ReportsInvalidMemberName()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int), null)|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.InvalidMemberName)
                .WithLocation(0)
                .WithArguments("Wrapper", "")
        );

    [TestMethod]
    public Task WrapType_TwoArgKeyword_ReportsInvalidMemberName()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int), "class")|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.InvalidMemberName)
                .WithLocation(0)
                .WithArguments("Wrapper", "class")
        );

    [TestMethod]
    public Task WrapType_TwoArgStartsWithDigit_ReportsInvalidMemberName()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int), "1bad")|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.InvalidMemberName)
                .WithLocation(0)
                .WithArguments("Wrapper", "1bad")
        );

    [TestMethod]
    public Task WrapType_TwoArgWithSpaces_ReportsInvalidMemberName()
        => RunAsync(
              """
                  [{|#0:EncosyTower.TypeWraps.WrapType(typeof(int), "bad name")|}]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.InvalidMemberName)
                .WithLocation(0)
                .WithArguments("Wrapper", "bad name")
        );

    [TestMethod]
    public Task WrapType_ArrayType_ReportsWrappedTypeMustBeNamedType()
        => RunAsync(
              """
                  [EncosyTower.TypeWraps.WrapType(typeof({|#0:int[]|}))]
                  public partial struct Wrapper { }
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrappedTypeMustBeNamedType)
                .WithLocation(0)
                .WithArguments("WrapType", "Wrapper", "int[]")
        );

    [TestMethod]
    public Task WrapRecord_ArrayParameter_ReportsWrappedTypeMustBeNamedType()
        => RunAsync(
              """
                  [EncosyTower.TypeWraps.WrapRecord]
                  public partial record struct Wrapper({|#0:int[]|} Value);
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrappedTypeMustBeNamedType)
                .WithLocation(0)
                .WithArguments("WrapRecord", "Wrapper", "int[]")
        );

    [TestMethod]
    public Task WrapRecord_TypeParameter_ReportsWrappedTypeMustBeNamedType()
        => RunAsync(
              """
                  [EncosyTower.TypeWraps.WrapRecord]
                  public partial record struct Wrapper<T>({|#0:T|} Value);
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrappedTypeMustBeNamedType)
                .WithLocation(0)
                .WithArguments("WrapRecord", "Wrapper", "T")
        );

    [TestMethod]
    public Task WrapRecord_DynamicParameter_ReportsWrappedTypeMustBeNamedType()
        => RunAsync(
              """
                  [EncosyTower.TypeWraps.WrapRecord]
                  public partial record struct Wrapper({|#0:dynamic|} Value);
              """
            , new DiagnosticResult(TypeWrapDiagnosticAnalyzer.WrappedTypeMustBeNamedType)
                .WithLocation(0)
                .WithArguments("WrapRecord", "Wrapper", "dynamic")
        );

    [TestMethod]
    public Task WrapRecord_ConstructedGenericParameter_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.TypeWraps.WrapRecord]
                public partial record struct Wrapper<T>(System.Collections.Generic.List<T> Value);
            """);

    [TestMethod]
    public Task WrapRecord_MissingParameterType_ReportsOnlyCompilerError()
        => RunAsync(
              """
                  [EncosyTower.TypeWraps.WrapRecord]
                  public partial record struct Wrapper({|#0:Missing|} Value);
              """
            , DiagnosticResult.CompilerError("CS0246").WithLocation(0).WithArguments("Missing")
        );

    [TestMethod]
    public Task WrapType_MissingType_ReportsOnlyCompilerError()
        => RunAsync(
              """
                  [EncosyTower.TypeWraps.WrapType(typeof({|#0:Missing|}))]
                  public partial struct Wrapper { }
              """
            , DiagnosticResult.CompilerError("CS0246").WithLocation(0).WithArguments("Missing")
        );
}
