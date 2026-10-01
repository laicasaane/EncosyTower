using EncosyTower.Entities.Analyzers.Entities.TypeHandles;

namespace EncosyTower.SourceGen.Tests.Entities.TypeHandles;

[TestClass]
public class TypeHandleDiagnosticAnalyzerTests
{
    private const string STUB_ATTRIBUTES = TypeHandleAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n    using Unity.Entities;\n    using EncosyTower.Entities;\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<TypeHandleDiagnosticAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<TypeHandleDiagnosticAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<TypeHandleDiagnosticAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task TypeHandleOnComponentStruct_NoDiagnostics()
        => RunAsync("""
                public struct MyComp : IComponentData { }

                [TypeHandle(typeof(MyComp))]
                public partial struct MyHandle { }
            """);

    [TestMethod]
    public Task TypeHandleOnBufferStruct_NoDiagnostics()
        => RunAsync("""
                public struct MyBuf : IBufferElementData { }

                [TypeHandle(typeof(MyBuf))]
                public partial struct MyHandle { }
            """);

    [TestMethod]
    public Task TypeHandleOnSharedComponentStruct_NoDiagnostics()
        => RunAsync("""
                public struct MyShared : ISharedComponentData { }

                [TypeHandle(typeof(MyShared))]
                public partial struct MyHandle { }
            """);

    [TestMethod]
    public Task GenericContainerStruct_ReportsGenericContainerNotSupported()
        => RunAsync(
              """
                  public struct MyComp : IComponentData { }

                  [TypeHandle(typeof(MyComp))]
                  public partial struct {|#0:MyHandle|}<T> { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.GenericContainerNotSupported)
                .WithLocation(0)
                .WithArguments("MyHandle")
        );

    [TestMethod]
    public Task TypeHandleWithNullLiteral_ReportsNotTypeOfExpression()
        => RunAsync(
              """
                  [{|#0:TypeHandle(null)|}]
                  public partial struct MyHandle { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.NotTypeOfExpression).WithLocation(0)
        );

    [TestMethod]
    public Task TypeHandleWithUnboundGeneric_ReportsOpenGenericTypeNotSupported()
        => RunAsync(
              """
                  public struct GenericComp<T> : IComponentData where T : unmanaged { }

                  [{|#0:TypeHandle(typeof(GenericComp<>))|}]
                  public partial struct MyHandle { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.OpenGenericTypeNotSupported)
                .WithLocation(0)
                .WithArguments("GenericComp")
        );

    [TestMethod]
    public Task TypeHandleWithManagedClass_ReportsManagedTypeNotSupported()
        => RunAsync(
              """
                  public class ManagedComp : IComponentData { }

                  [{|#0:TypeHandle(typeof(ManagedComp))|}]
                  public partial struct MyHandle { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.ManagedTypeNotSupported)
                .WithLocation(0)
                .WithArguments("ManagedComp")
        );

    [TestMethod]
    public Task TypeHandleWithoutMarkerInterface_ReportsMissingMarkerInterface()
        => RunAsync(
              """
                  public struct PlainStruct { }

                  [{|#0:TypeHandle(typeof(PlainStruct))|}]
                  public partial struct MyHandle { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.MissingMarkerInterface)
                .WithLocation(0)
                .WithArguments("PlainStruct")
        );

    [TestMethod]
    public Task DuplicateTypeHandleAttribute_ReportsDuplicateTypeHandle()
        => RunAsync(
              """
                  public struct MyComp : IComponentData { }

                  [TypeHandle(typeof(MyComp))]
                  [{|#0:TypeHandle(typeof(MyComp))|}]
                  public partial struct MyHandle { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.DuplicateTypeHandle)
                .WithLocation(0)
                .WithArguments("MyComp")
        );

    [TestMethod]
    public Task TypeHandleSharedComponentWithReadOnlyTrue_ReportsReadOnlyIgnoredForSharedComponent()
        => RunAsync(
              """
                  public struct MyShared : ISharedComponentData { }

                  [{|#0:TypeHandle(typeof(MyShared), true)|}]
                  public partial struct MyHandle { }
              """
            , new DiagnosticResult(TypeHandleDiagnosticAnalyzer.ReadOnlyIgnoredForSharedComponent)
                .WithLocation(0)
                .WithArguments("MyShared")
        );
}
