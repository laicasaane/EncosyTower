using EncosyTower.Core.Analyzers.PolyEnumStructs;

namespace EncosyTower.SourceGen.Tests.Core.PolyEnumStructs;

[TestClass]
public class PolyEnumStructAnalyzerTests
{
    private const string STUB_ATTRIBUTES = PolyEnumStructsAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<PolyEnumStructAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<PolyEnumStructAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<PolyEnumStructAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task StructWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public partial struct Plain { }
            """);

    [TestMethod]
    public Task PolyEnumStructWithCase_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                public partial struct Good
                {
                    public partial struct Case
                    {
                        public void Run() { }
                    }
                }
            """);

    [TestMethod]
    public Task GenericPolyEnumStruct_WithCase_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                public partial struct Foo<T>
                {
                    public partial struct Case { }
                }
            """);

    [TestMethod]
    public Task GenericPolyEnumStruct_WithoutContainer_Reports0005()
        => RunAsync(
              """
                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumStruct(WithEnumExtensions = true)|}]
                  public partial struct Foo<T>
                  {
                      public partial struct Case { }
                  }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.GenericEnumExtensionsUnsupported)
                .WithLocation(0)
                .WithArguments("Foo")
        );

    [TestMethod]
    public Task GenericPolyEnumStruct_WithValidContainerAndEnumExtensions_NoDiagnostics()
        => RunAsync(
            """
                public static partial class Cases
                {
                    public partial struct Case<T> { }
                }

                [EncosyTower.PolyEnumStructs.PolyEnumStruct(
                      Container = typeof(Cases)
                    , WithEnumExtensions = true
                )]
                public partial struct Result<T> { }
            """
        );

    [TestMethod]
    public Task PolyEnumStructWithoutCases_ReportsMustHaveCaseStructs()
        => RunAsync(
              """
                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumStruct|}]
                  public partial struct Empty { }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.MustHaveCaseStructs).WithLocation(0).WithArguments("Empty")
        );

    [TestMethod]
    public Task IEnumCaseGenericMethod_ReportsIEnumCaseMethodMustNotBeGeneric()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Outer
                  {
                      public partial struct Case
                      {
                          public void Run() { }
                      }

                      public interface IEnumCase
                      {
                          void {|#0:Bar|}<T>();
                      }
                  }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.IEnumCaseMethodMustNotBeGeneric)
                .WithLocation(0)
                .WithArguments("Bar")
        );

    [TestMethod]
    public Task CaseStructGenericMethod_ReportsCaseStructMethodMustNotBeGeneric()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Outer
                  {
                      public partial struct Case
                      {
                          public void {|#0:Bar|}<T>() { }
                      }
                  }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.CaseStructMethodMustNotBeGeneric)
                .WithLocation(0)
                .WithArguments("Bar", "Case")
        );

    [TestMethod]
    public Task WithEnumExtensions_InvalidGenericContainer_Reports0006()
        => RunAsync(
              """
                  public static partial class Cases<T> { public partial struct Case { } }
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct(
                        Container = {|#0:typeof(Cases<>)|}
                      , WithEnumExtensions = true
                  )]
                  public partial struct Result<T> { }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.ContainerMustBeNonGeneric)
                .WithLocation(0).WithArguments("Cases", "Result")
        );

    [TestMethod]
    public Task CrossAssemblyContainer_Reports0007()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct(Container = {|#0:typeof(string)|})]
                  public partial struct Result<T> { }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.ContainerMustBeSameAssembly)
                .WithLocation(0).WithArguments("String", "Result")
        );

    [TestMethod]
    public Task InvalidContainerKind_Reports0008()
        => RunAsync(
              """
                  public enum Cases { }
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct(Container = {|#0:typeof(Cases)|})]
                  public partial struct Result<T> { }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.ContainerKindUnsupported)
                .WithLocation(0).WithArguments("Cases")
        );

    [TestMethod]
    public Task UnknownCaseParameter_Reports0009()
        => RunAsync(
              """
                  public static partial class Cases { public partial struct {|#0:Case|}<TUnknown> { } }
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct(Container = typeof(Cases))]
                  public partial struct Result<TValue, TError> { }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.TypeParameterMappingInvalid)
                .WithLocation(0).WithArguments("Case", "Result")
        );

    [TestMethod]
    public Task CaseConstraintMismatch_Reports0010()
        => RunAsync(
              """
                  public static partial class Cases
                  {
                      public partial struct {|#0:Case|}<TValue> where TValue : struct { }
                  }
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct(Container = typeof(Cases))]
                  public partial struct Result<TValue> where TValue : unmanaged { }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.TypeParameterConstraintsMismatch)
                .WithLocation(0).WithArguments("Case", "Result")
        );

    [TestMethod]
    public Task CaseMissingGenericInterfaceParameter_Reports0011()
        => RunAsync(
              """
                  public static partial class Cases
                  {
                      public partial interface IEnumCase<TValue> { TValue Value => default; }
                      public partial struct {|#0:Case|}<TError> { }
                  }
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct(Container = typeof(Cases))]
                  public partial struct Result<TValue, TError> { }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.CaseMissingInterfaceParameter)
                .WithLocation(0).WithArguments("Case", "TValue")
        );

    [TestMethod]
    public Task InterfaceInsideGenericTarget_Reports0012()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Result<T>
                  {
                      public partial struct Case { }
                      public partial interface {|#0:IEnumCase|} { }
                  }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.InterfaceMustMoveToContainer)
                .WithLocation(0).WithArguments("Result")
        );

    [TestMethod]
    public Task GenericInterfaceMissingCommonMemberParameter_Reports0013()
        => RunAsync(
              """
                  public static partial class Cases
                  {
                      public partial interface {|#0:IEnumCase|}<TValue> { TValue Value => default; }
                      public partial struct A<TValue, TError> { public TError Error => default; }
                      public partial struct B<TValue, TError> { public TError Error => default; }
                  }
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct(Container = typeof(Cases))]
                  public partial struct Result<TValue, TError> { }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.InterfaceMissingMemberParameter)
                .WithLocation(0).WithArguments("IEnumCase", "TError", "Error")
        );

    [TestMethod]
    public Task DuplicateInterfaceMember_Reports0014()
        => RunAsync(
              """
                  public static partial class Cases
                  {
                      public partial interface IEnumCase { int Code => 0; }
                      public partial interface {|#0:IEnumCase|}<T> { int Code => 0; }
                      public partial struct Case<T> { }
                  }
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct(Container = typeof(Cases))]
                  public partial struct Result<T> { }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.InterfaceMemberDuplicated)
                .WithLocation(0).WithArguments("Code")
        );

    [TestMethod]
    public Task DuplicateUndefinedCase_Reports0015()
        => RunAsync(
              """
                  public static partial class Cases { public partial struct {|#0:Undefined|} { } }
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct(Container = typeof(Cases))]
                  public partial struct Result<T> { public partial struct Undefined { } }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.UndefinedCaseDuplicated)
                .WithLocation(0).WithArguments("Result")
        );

    private const string PAYLOAD = "public struct Payload { public string Name; }";

    private static string ExplicitMessage(string named)
        => $$"""
            [EncosyTower.PolyEnumStructs.PolyEnumStruct]
            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
            public partial struct Message
            {
            {{named}}

                public partial struct Empty { }
            }
            """;

    private static DiagnosticResult ManagedReference(params object[] arguments)
        => new DiagnosticResult(PolyEnumStructAnalyzer.CaseFieldHoldsManagedReference)
            .WithLocation(0)
            .WithArguments(arguments);

    [TestMethod]
    public Task ExplicitLayout_ManagedStructField_ReportsCaseFieldHoldsManagedReference()
        => RunAsync(
              PAYLOAD + "\n" + ExplicitMessage("public partial struct Named { public {|#0:Payload|} Value; }")
            , ManagedReference("Message", "Value", "Named", "global::TestProject.Payload")
        );

    [TestMethod]
    public Task ExplicitLayout_ManagedStructRecordParameter_ReportsCaseFieldHoldsManagedReference()
        => RunAsync(
              PAYLOAD + "\n" + ExplicitMessage("public partial record struct Named({|#0:Payload|} Value);")
            , ManagedReference("Message", "Value", "Named", "global::TestProject.Payload")
        );

    [TestMethod]
    public Task ExplicitLayout_PrivateReferenceField_ReportsCaseFieldHoldsManagedReference()
        => RunAsync(
              ExplicitMessage("public partial struct Named { private {|#0:string|} _name; }")
            , ManagedReference("Message", "_name", "Named", "string")
        );

    [TestMethod]
    public Task ExplicitLayout_ReferenceAutoProperty_ReportsCaseFieldHoldsManagedReference()
        => RunAsync(
              ExplicitMessage("public partial struct Named { public {|#0:object|} Value { get; set; } }")
            , ManagedReference("Message", "Value", "Named", "object")
        );

    [TestMethod]
    public Task ExplicitLayout_FieldLikeEvent_ReportsCaseFieldHoldsManagedReference()
        => RunAsync(
              ExplicitMessage("public partial struct Named { public event {|#0:System.Action|} Changed; }")
            , ManagedReference("Message", "Changed", "Named", "global::System.Action")
        );

    [TestMethod]
    public Task ExplicitLayout_ArrayField_ReportsCaseFieldHoldsManagedReference()
        => RunAsync(
              ExplicitMessage("public partial struct Named { public {|#0:int[]|} Values; }")
            , ManagedReference("Message", "Values", "Named", "int[]")
        );

    [TestMethod]
    public Task ExplicitLayout_StructConstrainedTypeParameterField_ReportsCaseFieldHoldsManagedReference()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
                  public partial struct Message<T> where T : struct
                  {
                      public partial struct Named { public {|#0:T|} Value; }

                      public partial struct Empty { }
                  }
              """
            , ManagedReference("Message", "Value", "Named", "T")
        );

    [TestMethod]
    public Task ExplicitLayout_UnmanagedTypeParameterField_ReportsCaseFieldSizeUnknown()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
                  public partial struct Message<T> where T : unmanaged
                  {
                      public partial struct Named { public {|#0:T|} Value; }

                      public partial struct Empty { }
                  }
              """
            , new DiagnosticResult(PolyEnumStructAnalyzer.CaseFieldSizeUnknown)
                .WithLocation(0)
                .WithArguments("Message", "Value", "Named", "T")
        );

    [TestMethod]
    public Task ExplicitLayout_UnmanagedStorage_NoDiagnostics()
        => RunAsync("""
                public enum Tint : byte { Red }

                [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
                public partial struct Message
                {
                    public partial struct Named
                    {
                        public static string Shared;
                        public const string LABEL = "named";

                        public int number;
                        private long _secret;
                        public int? maybe;
                        public (int, long) pair;
                        public Tint tint;

                        public decimal Amount { get; set; }
                    }

                    public partial record struct Point(int X, long Y);

                    public partial struct Nothing { }
                }
            """);

    [TestMethod]
    public Task SequentialLayout_ReferenceStorage_NoDiagnostics()
        => RunAsync("""
                public struct Payload { public string Name; }

                [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                public partial struct Message
                {
                    public partial struct Named
                    {
                        public Payload payload;
                        private string _name;
                        public event System.Action Changed;

                        public object Item { get; set; }
                    }

                    public partial struct Empty { }
                }
            """);

    [TestMethod]
    public Task ExplicitLayout_MissingFieldType_ReportsOnlyCompilerError()
        => RunAsync(
              ExplicitMessage("public partial struct Named { public {|#0:Missing|} Value; }")
            , DiagnosticResult.CompilerError("CS0246").WithLocation(0).WithArguments("Missing")
        );
}
