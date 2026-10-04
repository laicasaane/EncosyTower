using EncosyTower.PageFlows.Analyzers;
using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.PageFlows;

[TestClass]
public sealed class PageFlowScopeCollectionAnalyzerTests
{
    [TestMethod]
    public void DescriptorContract_IsExactAndOrdered()
    {
        var descriptors = new PageFlowScopeCollectionAnalyzer().SupportedDiagnostics;

        CollectionAssert.AreEqual(
              new[] {
                  "SG_PAGEFLOWS_0001",
                  "SG_PAGEFLOWS_0002",
                  "SG_PAGEFLOWS_0003",
                  "SG_PAGEFLOWS_0004",
                  "SG_PAGEFLOWS_0005",
                  "SG_PAGEFLOWS_0006",
              }
            , descriptors.Select(static descriptor => descriptor.Id).ToArray()
        );

        DiagnosticContractTestHelper.VerifyDiagnostics(new PageFlowsDiagnosticContractProvider());
    }

    [TestMethod]
    public Task ValidCollectionAndUnrelatedStruct_ProduceNoDiagnostics()
        => RunAsync("""
            using EncosyTower.PageFlows;

            namespace TestProject;

            [PageFlowScopeCollection]
            public partial struct Scopes
            {
                public PageFlowScope Screen { get; private set; }
            }

            public struct Unrelated
            {
                public PageFlowScope Field;

                public PageFlowScope GetOnly { get; }
            }
            """);

    [TestMethod]
    public Task NonPartialStruct_ReportsNotPartialWithoutNoScope()
        => RunAsync(
              """
              using EncosyTower.PageFlows;

              namespace TestProject;

              [PageFlowScopeCollection]
              public struct {|#0:Scopes|}
              {
                  public int Other { get; set; }
              }
              """
            , new DiagnosticResult(PageFlowScopeCollectionAnalyzer.NotPartial)
                .WithLocation(0)
                .WithArguments("global::TestProject.Scopes", "global::TestProject.Scopes")
        );

    [TestMethod]
    public Task NonPartialContainingType_ReportsNotPartialAtContainingType()
        => RunAsync(
              """
              using EncosyTower.PageFlows;

              namespace TestProject;

              public class {|#0:Outer|}
              {
                  [PageFlowScopeCollection]
                  public partial struct Scopes
                  {
                      public PageFlowScope Screen { get; set; }
                  }
              }
              """
            , new DiagnosticResult(PageFlowScopeCollectionAnalyzer.NotPartial)
                .WithLocation(0)
                .WithArguments("global::TestProject.Outer", "global::TestProject.Outer.Scopes")
        );

    [TestMethod]
    public Task ReadOnlyAndRefStructs_ReportReadOnlyOrRef()
        => RunAsync(
              """
              using EncosyTower.PageFlows;

              namespace TestProject;

              [PageFlowScopeCollection]
              public readonly partial struct {|#0:ReadOnlyScopes|}
              {
                  public PageFlowScope Screen { get => default; set { } }
              }

              [PageFlowScopeCollection]
              public ref partial struct {|#1:RefScopes|}
              {
                  public PageFlowScope Screen { get; set; }
              }
              """
            , new DiagnosticResult(PageFlowScopeCollectionAnalyzer.ReadOnlyOrRef)
                .WithLocation(0)
                .WithArguments("global::TestProject.ReadOnlyScopes")
            , new DiagnosticResult(PageFlowScopeCollectionAnalyzer.ReadOnlyOrRef)
                .WithLocation(1)
                .WithArguments("global::TestProject.RefScopes")
        );

    [TestMethod]
    public Task NoEligibleProperty_ReportsNoScope()
        => RunAsync(
              """
              using EncosyTower.PageFlows;

              namespace TestProject;

              [PageFlowScopeCollection]
              public partial struct {|#0:Scopes|}
              {
                  public static PageFlowScope Static { get; set; }

                  public int Other { get; set; }
              }
              """
            , new DiagnosticResult(PageFlowScopeCollectionAnalyzer.NoScope)
                .WithLocation(0)
                .WithArguments("global::TestProject.Scopes")
        );

    [TestMethod]
    public Task GetOnlyAndInitOnlyProperties_ReportPropertyNotScope()
        => RunAsync(
              """
              using EncosyTower.PageFlows;

              namespace TestProject;

              [PageFlowScopeCollection]
              public partial struct Scopes
              {
                  public PageFlowScope {|#0:GetOnly|} { get; }

                  public PageFlowScope {|#1:InitOnly|} { get; init; }

                  public PageFlowScope Screen { get; set; }
              }
              """
            , new DiagnosticResult(PageFlowScopeCollectionAnalyzer.PropertyNotScope)
                .WithLocation(0)
                .WithArguments("GetOnly", "global::TestProject.Scopes")
            , new DiagnosticResult(PageFlowScopeCollectionAnalyzer.PropertyNotScope)
                .WithLocation(1)
                .WithArguments("InitOnly", "global::TestProject.Scopes")
        );

    [TestMethod]
    public Task ScopeField_ReportsFieldIgnoredAndNoScope()
        => RunAsync(
              """
              using EncosyTower.PageFlows;

              namespace TestProject;

              [PageFlowScopeCollection]
              public partial struct {|#1:Scopes|}
              {
                  public PageFlowScope {|#0:Field|};
              }
              """
            , new DiagnosticResult(PageFlowScopeCollectionAnalyzer.FieldIgnored)
                .WithLocation(0)
                .WithArguments("Field", "global::TestProject.Scopes")
            , new DiagnosticResult(PageFlowScopeCollectionAnalyzer.NoScope)
                .WithLocation(1)
                .WithArguments("global::TestProject.Scopes")
        );

    [TestMethod]
    public Task HandImplementation_ReportsAtInterfaceInBaseList()
        => RunAsync(
              """
              using System;
              using EncosyTower.PageFlows;

              namespace TestProject;

              public struct Manual : {|#0:IPageFlowScopeCollection|}
              {
                  public ReadOnlyMemory<string> ScopeIdentifiers => default;

                  public bool TrySetScope(string identifier, PageFlowScope scope)
                      => false;
              }

              public class ManualClass : IDisposable, {|#1:EncosyTower.PageFlows.IPageFlowScopeCollection|}
              {
                  public ReadOnlyMemory<string> ScopeIdentifiers => default;

                  public bool TrySetScope(string identifier, PageFlowScope scope)
                      => false;

                  public void Dispose() { }
              }
              """
            , new DiagnosticResult(PageFlowScopeCollectionAnalyzer.ImplementedByHand)
                .WithLocation(0)
                .WithArguments("global::TestProject.Manual")
            , new DiagnosticResult(PageFlowScopeCollectionAnalyzer.ImplementedByHand)
                .WithLocation(1)
                .WithArguments("global::TestProject.ManualClass")
        );

    [TestMethod]
    public Task AssemblySkipAttribute_SuppressesAllDiagnostics()
        => RunAsync("""
            using System;
            using EncosyTower.PageFlows;

            [assembly: SkipSourceGeneratorsForAssembly]

            namespace TestProject;

            [PageFlowScopeCollection]
            public struct Scopes
            {
                public PageFlowScope Field;
            }

            public struct Manual : IPageFlowScopeCollection
            {
                public ReadOnlyMemory<string> ScopeIdentifiers => default;

                public bool TrySetScope(string identifier, PageFlowScope scope)
                    => false;
            }
            """);

    private static Task RunAsync(string source, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<PageFlowScopeCollectionAnalyzer>(
              [new NamedSource("Scopes.cs", source)]
            , expected
            , featureLocalStubSources: [new NamedSource("PageFlowsMarkers.cs", PageFlowsRuntimeFixture.MarkerSource)]
        );
}

internal sealed class PageFlowsDiagnosticContractProvider : IDiagnosticContractProvider
{
    private const string OWNER = "EncosyTower.PageFlows.Analyzers.PageFlowScopeCollectionAnalyzer";

    public string FeaturePath => "PageFlows";

    public IReadOnlyList<Type> ComponentTypes { get; } = [typeof(PageFlowScopeCollectionAnalyzer)];

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = [
        new(
              OWNER
            , "SG_PAGEFLOWS_0001"
            , "Page flow scope collection is not partial"
            , "'{0}' must be declared partial because '{1}' has [PageFlowScopeCollection]."
            , "PageFlows"
            , DiagnosticSeverity.Error
            , true
            , "The generator adds members to the attributed struct, so the struct and every containing type "
                + "must be partial."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PAGEFLOWS_0002"
            , "Page flow scope collection is readonly or ref"
            , "'{0}' has [PageFlowScopeCollection] and cannot be readonly or ref, because the generated "
                + "TrySetScope assigns its properties."
            , "PageFlows"
            , DiagnosticSeverity.Error
            , true
            , "A page flow scope collection must be a mutable, non-ref struct."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PAGEFLOWS_0003"
            , "Page flow scope collection has no scope"
            , "'{0}' has no instance property of type PageFlowScope with a setter that is not init."
            , "PageFlows"
            , DiagnosticSeverity.Error
            , true
            , "A page flow scope collection needs at least one instance PageFlowScope property with a setter "
                + "that is not init."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PAGEFLOWS_0004"
            , "PageFlowScope property is not a scope"
            , "Property '{0}' of '{1}' is get-only or init-only, so it is not a scope."
            , "PageFlows"
            , DiagnosticSeverity.Warning
            , true
            , "Only PageFlowScope properties with a setter that is not init are scopes."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PAGEFLOWS_0005"
            , "PageFlowScope field is ignored"
            , "Field '{0}' of '{1}' is not a scope; only properties are scopes."
            , "PageFlows"
            , DiagnosticSeverity.Warning
            , true
            , "Page flow scope collections read scopes from properties only."
            , ""
            , Array.Empty<string>()
        ),
        new(
              OWNER
            , "SG_PAGEFLOWS_0006"
            , "IPageFlowScopeCollection is implemented by hand"
            , "'{0}' implements IPageFlowScopeCollection without [PageFlowScopeCollection]; declare a partial "
                + "struct with the attribute instead."
            , "PageFlows"
            , DiagnosticSeverity.Error
            , true
            , "IPageFlowScopeCollection implementations are generated for partial structs that have "
                + "[PageFlowScopeCollection]."
            , ""
            , Array.Empty<string>()
        ),
    ];

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions { get; }
        = Array.Empty<SuppressionDescriptorContract>();
}
