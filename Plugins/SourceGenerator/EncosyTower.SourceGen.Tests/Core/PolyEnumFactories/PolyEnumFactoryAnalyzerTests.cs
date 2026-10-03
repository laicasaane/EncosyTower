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

    [DataTestMethod]
    [DataRow("public readonly partial record struct Factory(int Id, Target {|#0:Value|});")]
    [DataRow("public partial record class Factory(int Id, Target {|#0:Value|});")]
    [DataRow(
          "public readonly partial record struct Factory;\n\n"
            + "public readonly partial record struct Factory(int Id, Target {|#0:Value|});"
    )]
    [DataRow(
          "public readonly partial record struct Factory(int Id, Target {|#0:Value|}) "
            + "{ public Target Value => default; public Factory(Target value) : this(0, value) { } }"
    )]
    public Task RecordFactory_EnumStructNotFirstParameter_ReportsEnumStructMustBeFirstParameter(string factory)
        => RunAsync(
              TargetFactoryBody(factory)
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.EnumStructMustBeFirstParameter)
                .WithLocation(0)
                .WithArguments("Factory", "Target", "Value")
        );

    [DataTestMethod]
    [DataRow("public readonly partial record struct Factory{|#0:(int Id)|};")]
    [DataRow("public readonly partial record struct Factory{|#0:()|};")]
    [DataRow("public partial record class Factory{|#0:(int Id)|};")]
    [DataRow("public readonly partial record struct Factory{|#0:(Target First, Target Second)|};")]
    [DataRow(
          "public readonly partial record struct Factory;\n\n"
            + "public readonly partial record struct Factory{|#0:(int Id)|};"
    )]
    [DataRow("public readonly partial struct Factory { public Factory{|#0:(int id, Target value)|} { } }")]
    [DataRow("public partial struct Factory { public Factory{|#0:()|} { } }")]
    [DataRow("public partial class Factory { public Factory{|#0:(Target value, int id)|} { } }")]
    [DataRow("public partial class Factory { public Factory{|#0:(ref Target value)|} { } }")]
    public Task Factory_ConstructorBlocksFactory_ReportsWrapperNeedsEnumStructConstructor(string factory)
        => RunAsync(
              TargetFactoryBody(factory)
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.WrapperNeedsEnumStructConstructor)
                .WithLocation(0)
                .WithArguments("Factory", "Target")
        );

    [DataTestMethod]
    [DataRow("public readonly partial record struct Factory;")]
    [DataRow("public readonly partial record struct Factory(Target Value);")]
    [DataRow("public readonly partial record struct Factory(Target Value, int Id = 0);")]
    [DataRow(
          "public readonly partial record struct Factory(Target First, Target Second) "
            + "{ public Factory(Target value) : this(value, default) { } }"
    )]
    [DataRow(
          "public partial record class Factory(int Id) "
            + "{ private readonly Target _value; public Factory(Target value) : this(0) { _value = value; } }"
    )]
    [DataRow(
          "public readonly partial struct Factory "
            + "{ private readonly Target _value; public Factory(Target value) { _value = value; } }"
    )]
    [DataRow(
          "public partial class Factory "
            + "{ public Target Value { get; } public Factory(Target value) { Value = value; } }"
    )]
    [DataRow("public readonly partial struct Factory { public Factory(int id) : this() { } }")]
    [DataRow("public partial class Factory { public Factory(int id, Target value) { } }")]
    [DataRow(
          "public partial class Factory "
            + "{ private readonly Target _value; public Factory(in Target value) { _value = value; } }"
    )]
    [DataRow(
          "public partial class Factory { private readonly Target _value; "
            + "public Factory(Target value, int id) { _value = value; } public Factory(object value) { } }"
    )]
    public Task Factory_SupportedConstructorShape_NoDiagnostics(string factory)
        => RunAsync(TargetFactoryBody(factory));

    [DataTestMethod]
    [DataRow(
          "public partial class Factory "
            + "{ private readonly object _boxed; public Factory{|#0:(Target value)|} { _boxed = value; } }"
    )]
    [DataRow(
          "public readonly partial struct Factory "
            + "{ public static Target Default { get; } public Factory{|#0:(Target value)|} { } }"
    )]
    [DataRow(
          "public partial class Factory "
            + "{ public Target Value => default; public Factory{|#0:(in Target value)|} { } }"
    )]
    [DataRow("public partial record class Factory(int Id) { public Factory{|#0:(Target value)|} : this(0) { } }")]
    [DataRow(
          "public partial class Factory "
            + "{ public Factory{|#0:(Target value, int id)|} { } public Factory(object value) { } }"
    )]
    public Task Factory_EnumStructNotStored_ReportsWrapperNeedsEnumStructStorage(string factory)
        => RunAsync(
              TargetFactoryBody(factory)
            , new DiagnosticResult(PolyEnumFactoryAnalyzer.WrapperNeedsEnumStructStorage)
                .WithLocation(0)
                .WithArguments("Factory", "Target")
        );

    private static string TargetFactoryBody(string factory)
        => $$"""
                [EncosyTower.PolyEnumStructs.PolyEnumStruct]
                public partial struct Target
                {
                    public partial struct Case { }
                }

                [EncosyTower.PolyEnumStructs.PolyEnumFactoryFor(typeof(Target))]
                {{factory}}
            """;
}
