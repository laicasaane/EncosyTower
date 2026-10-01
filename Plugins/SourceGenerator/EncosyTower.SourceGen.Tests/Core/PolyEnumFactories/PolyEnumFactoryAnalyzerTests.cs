using EncosyTower.Core.Analyzers.PolyEnumFactories;

namespace EncosyTower.SourceGen.Tests.Core.PolyEnumFactories;

[TestClass]
public class PolyEnumFactoryAnalyzerTests
{
    private const string STUB_ATTRIBUTES = PolyEnumFactoriesAnalyzerStubs.ATTRIBUTES;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<PolyEnumFactoryAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<PolyEnumFactoryAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<PolyEnumFactoryAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task ClassWithoutAttribute_NoDiagnostics()
        => RunAsync("""
                public class Plain { }
            """);

    [TestMethod]
    public Task ValidPartialFactory_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                public partial struct Target
                {
                    public partial struct Case { }
                }

                [EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target))]
                public partial class Factory { }
            """);

    [TestMethod]
    public Task NonPartialFactory_ReportsMustBePartial()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target
                  {
                      public partial struct Case { }
                  }

                  [EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target))]
                  public class {|#0:Factory|} { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.MustBePartial).WithLocation(0).WithArguments("Factory")
        );

    [TestMethod]
    public Task GenericFactory_ForNonGenericTarget_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                public partial struct Target
                {
                    public partial struct Case { }
                }

                [EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target))]
                public partial class Factory<T> { }
            """);

    [TestMethod]
    public Task OpenTarget_WithEquivalentSubstitutedConstraints_NoDiagnostics()
        => RunAsync("""
                [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                public partial struct Target<TBase, TDerived>
                    where TBase : class, System.IDisposable, new()
                    where TDerived : TBase
                {
                    public partial struct Case { }
                }

                [EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target<,>))]
                public partial class Factory<UBase, UDerived>
                    where UBase : class, System.IDisposable, new()
                    where UDerived : UBase
                {
                }
            """);

    [TestMethod]
    public Task OpenTarget_WithNonGenericFactory_ReportsArityMismatch()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target<T>
                  {
                      public partial struct Case { }
                  }

                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target<>))|}]
                  public partial class Factory { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.TargetArityMismatch)
                .WithLocation(0)
                .WithArguments("Target", 1, "Factory", 0)
        );

    [TestMethod]
    public Task TargetWithoutPolyEnumStruct_ReportsTargetMustBePolyEnumStruct()
        => RunAsync(
              """
                  public struct Plain { }

                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Plain))|}]
                  public partial class Factory { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.TargetMustBePolyEnumStruct)
                .WithLocation(0)
                .WithArguments("Plain")
        );

    [TestMethod]
    public Task OpenTarget_WithSmallerFactoryArity_ReportsArityMismatch()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target<T1, T2>
                  {
                      public partial struct Case { }
                  }

                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target<,>))|}]
                  public partial class Factory<T> { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.TargetArityMismatch)
                .WithLocation(0)
                .WithArguments("Target", 2, "Factory", 1)
        );

    [TestMethod]
    public Task OpenTarget_WithDifferentConstraints_ReportsConstraintMismatch()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target<T>
                      where T : unmanaged
                  {
                      public partial struct Case { }
                  }

                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target<>))|}]
                  public partial class Factory<U>
                      where U : struct
                  {
                  }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.TargetConstraintMismatch)
                .WithLocation(0)
                .WithArguments("Target", "Factory")
        );

    [TestMethod]
    public Task OpenTarget_WithLargerFactoryArity_ReportsArityMismatch()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target<T>
                  {
                      public partial struct Case { }
                  }

                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target<>))|}]
                  public partial class Factory<U1, U2> { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.TargetArityMismatch)
                .WithLocation(0)
                .WithArguments("Target", 1, "Factory", 2)
        );

    [DataTestMethod]
    [DataRow("class", "class?")]
    [DataRow("unmanaged", "struct")]
    [DataRow("class, new()", "class")]
    [DataRow("System.IDisposable", "System.IComparable")]
    [DataRow("System.IDisposable?", "System.IDisposable")]
    public Task OpenTarget_WithConstraintVariant_ReportsConstraintMismatch(
          string targetConstraint
        , string factoryConstraint
    )
        => RunAsync(
              $$"""
                  #nullable enable
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target<T>
                      where T : {{targetConstraint}}
                  {
                      public partial struct Case { }
                  }

                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target<>))|}]
                  public partial class Factory<U>
                      where U : {{factoryConstraint}}
                  {
                  }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.TargetConstraintMismatch)
                .WithLocation(0)
                .WithArguments("Target", "Factory")
        );

    [TestMethod]
    public Task OpenTarget_WithInterParameterConstraintMismatch_ReportsConstraintMismatch()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target<TBase, TDerived>
                      where TDerived : TBase
                  {
                      public partial struct Case { }
                  }

                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target<,>))|}]
                  public partial class Factory<UBase, UDerived>
                      where UBase : UDerived
                  {
                  }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.TargetConstraintMismatch)
                .WithLocation(0)
                .WithArguments("Target", "Factory")
        );

    [TestMethod]
    public Task TargetWithoutCases_ReportsMustHaveCaseStructs()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target { }

                  [{|#0:EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target))|}]
                  public partial class Factory { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.MustHaveCaseStructs).WithLocation(0).WithArguments("Target")
        );

    [TestMethod]
    public Task CaseCtorWithOutParam_ReportsCaseCtorOutParameterIgnored()
        => RunAsync(
              """
                  [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                  public partial struct Target
                  {
                      public partial struct Case
                      {
                          public {|#0:Case|}(out int x) { x = 0; }
                      }
                  }

                  [EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target))]
                  public partial class Factory { }
              """
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.CaseCtorOutParameterIgnored)
                .WithLocation(0)
                .WithArguments("Case")
        );
}
