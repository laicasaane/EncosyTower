using EncosyTower.Core.Analyzers.EnumTemplates;

namespace EncosyTower.SourceGen.Tests.Core.EnumTemplates;

[TestClass]
public class EnumTemplateAnalyzerTests
{
    private const string STUB_ATTRIBUTES = EnumTemplatesAnalyzerStubs.ATTRIBUTES;
    private const string LEGACY_MEMBERSHIP_ATTRIBUTE_STUB = """
        namespace EncosyTower.EnumExtensions
        {
            [System.AttributeUsage(System.AttributeTargets.Enum)]
            public sealed class EnumMembersForTemplateAttribute : System.Attribute
            {
                public EnumMembersForTemplateAttribute(System.Type templateType) { }
            }
        }
        """;

    private static string Wrap(string body)
        => $"{STUB_ATTRIBUTES}\nnamespace TestProject\n{{\n    using EncosyTower.EnumExtensions;\n{body}\n}}\n";

    private static Task RunAsync(string body, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<EnumTemplateAnalyzer>(
              Wrap(body)
            , expected
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task EmptyInput_DoesNotThrow()
        => AnalyzerTestHelper.VerifyAsync<EnumTemplateAnalyzer>("");

    [TestMethod]
    public Task AttributeStubOnly_NoDiagnostics()
        => AnalyzerTestHelper.VerifyAsync<EnumTemplateAnalyzer>(
              STUB_ATTRIBUTES
            , runtimeReferences: Array.Empty<MetadataReference>()
        );

    [TestMethod]
    public Task MembershipAttributeWithOneLegacyArgument_ReportsMustSpecifyTypeAndOrder()
        => AnalyzerTestHelper.VerifyAsync<EnumTemplateAnalyzer>(
              """
              using EncosyTower.EnumExtensions;

              [{|#0:EnumMembersForTemplate(typeof(int))|}]
              public enum Values : byte { A }
              """
            , [
                new DiagnosticResult(EnumTemplateAnalyzer.MustSpecifyTypeAndOrder).WithLocation(0),
            ]
            , runtimeReferences: Array.Empty<MetadataReference>()
            , featureLocalStubSource: LEGACY_MEMBERSHIP_ATTRIBUTE_STUB
        );

    [TestMethod]
    public Task TemplateWithoutSuffix_ReportsNotEndWithTemplateSuffix()
        => RunAsync(
              """
                  [EnumTemplate]
                  public readonly struct {|#0:Foo|} { }
              """
            , new DiagnosticResult(EnumTemplateAnalyzer.NotEndWithTemplateSuffix).WithLocation(0)
        );

    [TestMethod]
    public Task TemplateWithEnumTemplateSuffix_NoDiagnostics()
        => RunAsync("""
                [EnumTemplate]
                public readonly struct Foo_EnumTemplate { }
            """);

    [TestMethod]
    public Task TemplateWithTemplateSuffix_NoDiagnostics()
        => RunAsync("""
                [EnumTemplate]
                public readonly struct Foo_Template { }
            """);

    [TestMethod]
    public Task EnumWithIntUnderlyingType_ReportsNotSupportUnderlyingType()
        => RunAsync(
              """
                  [EnumTemplate]
                  public readonly struct Tmpl_EnumTemplate { }

                  [{|#0:EnumMembersForTemplate(typeof(Tmpl_EnumTemplate), 0)|}]
                  public enum FruitInt : int { Apple, Orange }
              """
            , new DiagnosticResult(EnumTemplateAnalyzer.NotSupportUnderlyingType).WithLocation(0)
        );

    [TestMethod]
    public Task EnumWithUintUnderlyingType_NoDiagnostics()
        => RunAsync("""
                [EnumTemplate]
                public readonly struct Tmpl_EnumTemplate { }

                [EnumMembersForTemplate(typeof(Tmpl_EnumTemplate), 0)]
                public enum FruitUint : uint { Apple, Orange }
            """);

    [TestMethod]
    public Task TemplateWithNullTypeArg_ReportsMustBeTypeOfExpression()
        => RunAsync(
              """
                  [EnumTemplate]
                  [{|#0:EnumTemplateMembersFromEnum(null, 0)|}]
                  public readonly struct Tmpl_EnumTemplate { }
              """
            , new DiagnosticResult(EnumTemplateAnalyzer.MustBeTypeOfExpression).WithLocation(0)
        );

    [TestMethod]
    public Task TemplateWithValidTypeArg_NoDiagnostics()
        => RunAsync("""
                public enum SomeEnum : uint { A, B }

                [EnumTemplate]
                [EnumTemplateMembersFromEnum(typeof(SomeEnum), 0)]
                public readonly struct Tmpl_EnumTemplate { }
            """);

    [TestMethod]
    public Task TemplateWithUnboundGenericMember_ReportsNotSupportUnboundGenericType()
        => RunAsync(
              """
                  public class Generic<T> { }

                  [EnumTemplate]
                  [{|#0:EnumTemplateMemberFromType(typeof(Generic<>), 0)|}]
                  public readonly struct Tmpl_EnumTemplate { }
              """
            , new DiagnosticResult(EnumTemplateAnalyzer.NotSupportUnboundGenericType)
                .WithLocation(0)
                .WithArguments("TestProject.Generic<>")
        );

    [TestMethod]
    public Task TemplateWithClosedGenericMember_NoDiagnostics()
        => RunAsync("""
                public class Generic<T> { }

                [EnumTemplate]
                [EnumTemplateMemberFromType(typeof(Generic<int>), 0)]
                public readonly struct Tmpl_EnumTemplate { }
            """);

    [TestMethod]
    public Task NestedTemplateInsideOuter_NoDiagnostics()
        => RunAsync("""
                public class Outer
                {
                    [EnumTemplate]
                    public readonly struct Inner_Template { }
                }
            """);
}
