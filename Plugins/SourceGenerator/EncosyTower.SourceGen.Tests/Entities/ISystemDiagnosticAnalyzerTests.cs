using EncosyTower.Entities.CodeRefactors;

namespace EncosyTower.SourceGen.Tests.Entities;

[TestClass]
public sealed class ISystemDiagnosticAnalyzerTests
{
    private const string I_SYSTEM_ATTRIBUTE_STUB = """
        namespace EncosyTower.Entities
        {
            [System.AttributeUsage(System.AttributeTargets.All)]
            public sealed class ISystemAttribute : System.Attribute { }
        }
        """;

    [TestMethod]
    public Task AnnotatedClass_ReportsTypeCanBeCompleted()
        => AnalyzerTestHelper.VerifyAsync<ISystemDiagnosticAnalyzer>(
              """
              using EncosyTower.Entities;

              [ISystem]
              public partial class {|#0:GameSystem|} { }
              """
            , [
                new DiagnosticResult("SG_ISYSTEM_0001", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("GameSystem"),
            ]
        );

    [TestMethod]
    public Task AnnotatedStruct_ReportsTypeCanBeCompleted()
        => AnalyzerTestHelper.VerifyAsync<ISystemDiagnosticAnalyzer>(
              """
              using EncosyTower.Entities;

              [ISystem]
              public partial struct {|#0:GameSystem|} { }
              """
            , [
                new DiagnosticResult("SG_ISYSTEM_0001", DiagnosticSeverity.Hidden)
                    .WithLocation(0)
                    .WithArguments("GameSystem"),
            ]
        );

    [TestMethod]
    public Task AnnotatedInterface_ReportsNothing()
        => AnalyzerTestHelper.VerifyAsync<ISystemDiagnosticAnalyzer>(
              """
              using EncosyTower.Entities;

              [ISystem]
              public interface IGameSystem { }
              """
            , runtimeReferences: Array.Empty<MetadataReference>()
            , featureLocalStubSource: I_SYSTEM_ATTRIBUTE_STUB
        );

    [TestMethod]
    public Task UnannotatedClass_ReportsNothing()
        => AnalyzerTestHelper.VerifyAsync<ISystemDiagnosticAnalyzer>(
            """
            public partial class GameSystem { }
            """
        );
}
