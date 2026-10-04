using EncosyTower.Core.Analyzers.TypeFlags;

namespace EncosyTower.SourceGen.Tests.Core.TypeFlags;

[TestClass]
public sealed class TypeFlagAnalyzerTests
{
    private const string PRODUCER = """
        namespace TestProject
        {
            [System.CodeDom.Compiler.GeneratedCode("EncosyTower.Fake.Generators.BaseGenerator", "1.0")]
            public partial class GeneratedBase
            {
                public partial class Nested { }
            }

            [System.CodeDom.Compiler.GeneratedCode("EncosyTower.Fake.Generators.BaseGenerator", "1.0")]
            public enum GeneratedEnum { A }
        }
        """;

    private const string TOOL = "EncosyTower.Fake.Generators.BaseGenerator";

    private const string OWN_GENERATED_CODE = "System.CodeDom.Compiler.GeneratedCode("
        + "\"EncosyTower.Core.Generators.TypeFlags.TypeFlagGenerator\", \"1.0\")";

    private static readonly NamedSource[] s_stubSources = {
        new("TypeFlagRuntime.cs", TypeFlagTestFixture.RUNTIME_STUB_SOURCE),
    };

    [TestMethod]
    [DataRow("[TypeFlag] internal sealed partial class AudioManager { }")]
    [DataRow("[TypeFlag(WriteAccess = TypeFlagAccess.Public)] public readonly partial struct GraphicsPreset { }")]
    [DataRow("[TypeFlag(Api = TypeFlagApi.Related)] public sealed partial class SettingsLoader { }")]
    [DataRow(
        """
        [TypeFlag(WriteAccess = TypeFlagAccess.Internal, Api = TypeFlagApi.Self)]
        public sealed partial class Service { }
        """
    )]
    [DataRow(
        """
        [TypeFlag]
        public sealed partial class MenuScreen : UnityEngine.MonoBehaviour
        {
            [TypeFlag(WriteAccess = TypeFlagAccess.Internal, Api = TypeFlagApi.State)]
            public readonly partial struct Initialized { }
        }
        """
    )]
    [DataRow("[TypeFlag(UseExtensions = true)] public sealed partial class ScoreBoard { }")]
    [DataRow(
        """
        [TypeFlag(UseExtensions = true, WriteAccess = TypeFlagAccess.Internal)]
        public readonly partial struct Level { }
        """
    )]
    [DataRow("[TypeFlag(UseExtensions = true, WriteAccess = TypeFlagAccess.Public)] public partial class Board { }")]
    [DataRow("public partial class Registry<TKey> { [TypeFlag] public partial struct Slot<TValue> { } }")]
    [DataRow("[TypeFlag] public partial class Base { }\n\n[TypeFlag] public partial class Derived : Base { }")]
    [DataRow("[TypeFlag] public partial struct Owner { }")]
    [DataRow("[TypeFlag] public partial record class Owner { }")]
    [DataRow("[TypeFlag] public partial record struct Owner { }")]
    [DataRow("[TypeFlag] public abstract partial class Owner { }")]
    [DataRow("[TypeFlag] public partial class Owner : UnityEngine.MonoBehaviour { }")]
    [DataRow(
        """
        [TypeFlag(WriteAccess = TypeFlagAccess.Public)]
        public partial class Owner
        {
            private static int s_typeFlag = 1;

            public static int Read()
                => s_typeFlag;
        }
        """
    )]
    [DataRow("[TypeFlag(UseExtensions = true)] public partial class Owner { public sealed class TypeFlagAPI { } }")]
    [DataRow(
        """
        public class Base
        {
            private static int TypeFlag => 0;

            private sealed class TypeFlagAPI { }

            public static object Read()
                => TypeFlag + new TypeFlagAPI().GetHashCode();
        }

        [TypeFlag]
        public partial class Derived : Base { }
        """
    )]
    [DataRow(
        """
        public abstract class SceneMarker<T> : UnityEngine.MonoBehaviour
            where T : SceneMarker<T>
        {
            public static TypeFlag<T>.ReadOnly TypeFlag => default;
        }

        [TypeFlag]
        public sealed partial class SpawnPoint : SceneMarker<SpawnPoint> { }
        """
    )]
    public Task ValidOwners(string declarations)
        => RunAsync(declarations);

    [TestMethod]
    [DataRow("public static partial class {|#0:Owner|} { }", "a static class")]
    [DataRow("public ref partial struct {|#0:Owner|} { }", "a ref struct")]
    public Task UnsupportedOwner(string declaration, string kind)
        => RunAsync(
              "[TypeFlag]\n" + declaration
            , new DiagnosticResult(TypeFlagAnalyzer.UnsupportedOwner)
                .WithLocation(0)
                .WithArguments("TestProject.Owner", kind)
        );

    [TestMethod]
    public Task UnsupportedOwnerSkipsOtherChecks()
        => RunAsync(
              """
              [TypeFlag(WriteAccess = (TypeFlagAccess)9)]
              public static partial class {|#0:Owner|}
              {
                  public static int TypeFlag => 0;
              }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.UnsupportedOwner)
                .WithLocation(0)
                .WithArguments("TestProject.Owner", "a static class")
        );

    [TestMethod]
    [DataRow("public int {|#0:TypeFlag|} => 0;", "TypeFlag")]
    [DataRow("public static void {|#0:TypeFlag|}() { }", "TypeFlag")]
    [DataRow("private static int {|#0:s_typeFlag|} = 1; public static int Read() => s_typeFlag;", "s_typeFlag")]
    [DataRow("public sealed class {|#0:TypeFlagAPI|} { }", "TypeFlagAPI")]
    [DataRow(
        "private struct {|#0:TypeFlagReadWrite|} { } public static object Read() => default(TypeFlagReadWrite);",
        "TypeFlagReadWrite"
    )]
    [DataRow("internal static TypeFlag<Owner>.ReadOnly {|#0:TypeFlag|} => default;", "TypeFlag")]
    public Task OwnMemberConflict(string members, string memberName)
        => RunAsync(
              $$"""
              [TypeFlag]
              public partial class Owner
              {
                  {{members}}
              }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.MemberNameConflict)
                .WithLocation(0)
                .WithArguments("TestProject.Owner", memberName)
        );

    [TestMethod]
    public Task OwnMemberConflict_PositionalRecord()
        => RunAsync(
              """
              [TypeFlag]
              public partial record Settings(int {|#0:TypeFlag|});
              """
            , new DiagnosticResult(TypeFlagAnalyzer.MemberNameConflict)
                .WithLocation(0)
                .WithArguments("TestProject.Settings", "TypeFlag")
        );

    [TestMethod]
    public Task MultipleConflicts()
        => RunAsync(
              """
              [TypeFlag]
              public partial class Owner
              {
                  public static int {|#0:TypeFlag|} => 0;

                  public sealed class {|#1:TypeFlagReadWrite|} { }
              }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.MemberNameConflict)
                .WithLocation(0)
                .WithArguments("TestProject.Owner", "TypeFlag")
            , new DiagnosticResult(TypeFlagAnalyzer.MemberNameConflict)
                .WithLocation(1)
                .WithArguments("TestProject.Owner", "TypeFlagReadWrite")
        );

    [TestMethod]
    [DataRow("TypeFlag")]
    [DataRow("TypeFlagAPI")]
    public Task OwnerNamedAfterMember(string name)
        => RunAsync(
              $$"""
              [TypeFlag]
              public partial class {|#0:{{name}}|} { }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.MemberNameConflict)
                .WithLocation(0)
                .WithArguments($"TestProject.{name}", name)
        );

    [TestMethod]
    public Task InheritedConflict()
        => RunAsync(
              """
              public class Base
              {
                  public int TypeFlag { get; }
              }

              [TypeFlag]
              public partial class {|#0:Derived|} : Base { }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.MemberNameConflict)
                .WithLocation(0)
                .WithArguments("TestProject.Derived", "TypeFlag")
        );

    [TestMethod]
    public Task InheritedConflictThroughMarkedBase()
        => RunAsync(
              """
              public class A
              {
                  public int TypeFlag { get; }
              }

              [TypeFlag]
              public partial class {|#0:B|} : A { }

              [TypeFlag]
              public partial class {|#1:D|} : B { }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.MemberNameConflict)
                .WithLocation(0)
                .WithArguments("TestProject.B", "TypeFlag")
            , new DiagnosticResult(TypeFlagAnalyzer.MemberNameConflict)
                .WithLocation(1)
                .WithArguments("TestProject.D", "TypeFlag")
        );

    [TestMethod]
    public Task OwnGeneratedMembersIgnored()
        => RunAsync($$"""
            [TypeFlag]
            public partial class Owner { }

            public partial class Owner
            {
                [{{OWN_GENERATED_CODE}}]
                public static readonly TypeFlagAPI TypeFlag = default;

                [{{OWN_GENERATED_CODE}}]
                private static readonly TypeFlagReadWrite s_typeFlag = default;

                [{{OWN_GENERATED_CODE}}]
                public readonly struct TypeFlagAPI { }

                [{{OWN_GENERATED_CODE}}]
                private readonly struct TypeFlagReadWrite { }

                public static object Read()
                    => s_typeFlag;
            }
            """);

    [TestMethod]
    public Task ForeignGeneratedMemberConflicts()
        => RunAsync(
              """
              [TypeFlag]
              public partial class {|#0:Owner|}
              {
                  [System.CodeDom.Compiler.GeneratedCode("Other.Tool", "1.0")]
                  public static int TypeFlag => 0;
              }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.MemberNameConflict)
                .WithLocation(0)
                .WithArguments("TestProject.Owner", "TypeFlag")
        );

    [TestMethod]
    public Task UndefinedOption_WriteAccess()
        => RunAsync(
              """
              [TypeFlag({|#0:WriteAccess = (TypeFlagAccess)3|})]
              public partial class Owner { }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.UndefinedOption)
                .WithLocation(0)
                .WithArguments("WriteAccess", "3", "TypeFlagAccess", "TestProject.Owner")
        );

    [TestMethod]
    public Task UndefinedOption_Api()
        => RunAsync(
              """
              [TypeFlag({|#0:Api = (TypeFlagApi)8|})]
              public partial class Owner { }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.UndefinedOption)
                .WithLocation(0)
                .WithArguments("Api", "8", "TypeFlagApi", "TestProject.Owner")
        );

    [TestMethod]
    public Task UndefinedOption_Both()
        => RunAsync(
              """
              [TypeFlag({|#0:WriteAccess = (TypeFlagAccess)3|}, {|#1:Api = (TypeFlagApi)8|})]
              public partial class Owner { }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.UndefinedOption)
                .WithLocation(0)
                .WithArguments("WriteAccess", "3", "TypeFlagAccess", "TestProject.Owner")
            , new DiagnosticResult(TypeFlagAnalyzer.UndefinedOption)
                .WithLocation(1)
                .WithArguments("Api", "8", "TypeFlagApi", "TestProject.Owner")
        );

    [TestMethod]
    public Task UndefinedOptionSkipsMemberChecks()
        => RunAsync(
              """
              [TypeFlag({|#0:WriteAccess = (TypeFlagAccess)3|})]
              public partial class Owner
              {
                  public static int TypeFlag => 0;
              }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.UndefinedOption)
                .WithLocation(0)
                .WithArguments("WriteAccess", "3", "TypeFlagAccess", "TestProject.Owner")
        );

    [TestMethod]
    [DataRow("Api = TypeFlagApi.State")]
    [DataRow("Api = (TypeFlagApi)8")]
    public Task ApiWithUseExtensions(string api)
        => RunAsync(
              $$"""
              [TypeFlag(UseExtensions = true, {|#0:{{api}}|})]
              public partial class Owner { }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.IneffectiveApi)
                .WithLocation(0)
                .WithArguments("TestProject.Owner")
        );

    [TestMethod]
    public Task ApiWithUseExtensionsStillChecksMembers()
        => RunAsync(
              """
              [TypeFlag(UseExtensions = true, {|#0:Api = TypeFlagApi.Self|})]
              public partial class Owner
              {
                  private static int {|#1:s_typeFlag|} = 1;

                  public static int Read()
                      => s_typeFlag;
              }
              """
            , new DiagnosticResult(TypeFlagAnalyzer.IneffectiveApi)
                .WithLocation(0)
                .WithArguments("TestProject.Owner")
            , new DiagnosticResult(TypeFlagAnalyzer.MemberNameConflict)
                .WithLocation(1)
                .WithArguments("TestProject.Owner", "s_typeFlag")
        );

    [TestMethod]
    public Task CompilerOwnedCases_MissingPartial()
        => RunAsync("""
            [TypeFlag]
            public class Owner { }
            """);

    [TestMethod]
    public Task CompilerOwnedCases_RepeatedMarker()
        => RunAsync(
              """
              [TypeFlag]
              public partial class Owner { }

              [{|#0:TypeFlag|}]
              public partial class Owner { }
              """
            , DiagnosticResult.CompilerError("CS0579").WithLocation(0).WithArguments("TypeFlag")
        );

    [TestMethod]
    public Task CompilerOwnedCases_ArgumentError()
        => RunAsync(
              """
              [TypeFlag(WriteAccess = {|#0:Missing|}.Value)]
              public partial class Owner { }
              """
            , DiagnosticResult.CompilerError("CS0103").WithLocation(0).WithArguments("Missing")
        );

    [TestMethod]
    public Task SkippedOrMissingRuntime_ModuleSkip()
        => AnalyzerTestHelper.VerifyAsync<TypeFlagAnalyzer>(
              """
              using EncosyTower.TypeFlags;

              [assembly: EncosyTower.TypeFlags.SkipSourceGeneratorsForAssembly]

              namespace TestProject
              {
                  [TypeFlag]
                  public static partial class Owner { }
              }
              """
            , runtimeReferences: TypeFlagTestFixture.RuntimeReferences
            , featureLocalStubSource: TypeFlagTestFixture.RUNTIME_STUB_SOURCE
        );

    [TestMethod]
    public Task SkippedOrMissingRuntime_MissingTypeFlag()
        => AnalyzerTestHelper.VerifyAsync<TypeFlagAnalyzer>(
              Wrap("""
                  [TypeFlag]
                  public static partial class Owner { }
                  """)
            , runtimeReferences: TypeFlagTestFixture.RuntimeReferences
            , featureLocalStubSource: TypeFlagTestFixture.ATTRIBUTE_STUB_SOURCE
        );

    [TestMethod]
    [DataRow("[TypeFlag]\npublic partial class {|#0:Owner|} : GeneratedBase { }")]
    [DataRow("public class Middle : GeneratedBase { }\n\n[TypeFlag]\npublic partial class {|#0:Owner|} : Middle { }")]
    [DataRow("[TypeFlag]\npublic partial class {|#0:Owner|} : GeneratedBase.Nested { }")]
    public Task GeneratedBase(string declarations)
        => AnalyzerTestHelper.VerifyAsync<TypeFlagAnalyzer>(
              new[] {
                  new NamedSource("Test0.cs", Wrap(declarations)),
                  new NamedSource("Producer.g.cs", PRODUCER),
              }
            , new[] {
                new DiagnosticResult(TypeFlagAnalyzer.GeneratedBase)
                    .WithLocation(0)
                    .WithArguments("TestProject.Owner", "TestProject.GeneratedBase", TOOL),
            }
            , runtimeReferences: TypeFlagTestFixture.RuntimeReferences
            , featureLocalStubSources: s_stubSources
        );

    [TestMethod]
    public Task GeneratedBaseNotReported_GeneratedTypeArgument()
        => AnalyzerTestHelper.VerifyAsync<TypeFlagAnalyzer>(
              new[] {
                  new NamedSource(
                        "Test0.cs"
                      , Wrap("""
                          public class Base<T> { }

                          [TypeFlag]
                          public partial class Owner : Base<GeneratedEnum> { }
                          """)
                  ),
                  new NamedSource("Producer.g.cs", PRODUCER),
              }
            , runtimeReferences: TypeFlagTestFixture.RuntimeReferences
            , featureLocalStubSources: s_stubSources
        );

    [TestMethod]
    public async Task GeneratedBaseNotReported_FromReference()
    {
        var reference = await GeneratorTestHelper.CompileToReferenceAsync(PRODUCER, "TypeFlag.Producer");

        await AnalyzerTestHelper.VerifyAsync<TypeFlagAnalyzer>(
              Wrap("""
                  [TypeFlag]
                  public partial class Owner : GeneratedBase { }
                  """)
            , runtimeReferences: TypeFlagTestFixture.RuntimeReferences.Add(reference)
            , featureLocalStubSource: TypeFlagTestFixture.RUNTIME_STUB_SOURCE
        );
    }

    [TestMethod]
    public Task GeneratedBaseNotReported_NoGeneratedCodeAttribute()
        => AnalyzerTestHelper.VerifyAsync<TypeFlagAnalyzer>(
              new[] {
                  new NamedSource(
                        "Test0.cs"
                      , Wrap("""
                          [TypeFlag]
                          public partial class Owner : PlainBase { }
                          """)
                  ),
                  new NamedSource("PlainBase.g.cs", "namespace TestProject { public class PlainBase { } }"),
              }
            , runtimeReferences: TypeFlagTestFixture.RuntimeReferences
            , featureLocalStubSources: s_stubSources
        );

    [TestMethod]
    public Task UnresolvedBaseLeftToCompiler()
        => RunAsync(
              """
              [TypeFlag]
              public partial class Owner : {|#0:MissingBase|} { }
              """
            , DiagnosticResult.CompilerError("CS0246").WithLocation(0).WithArguments("MissingBase")
        );

    private static string Wrap(string declarations)
        => $"using EncosyTower.TypeFlags;\n\nnamespace TestProject\n{{\n{declarations}\n}}\n";

    private static Task RunAsync(string declarations, params DiagnosticResult[] expected)
        => AnalyzerTestHelper.VerifyAsync<TypeFlagAnalyzer>(
              Wrap(declarations)
            , expected
            , runtimeReferences: TypeFlagTestFixture.RuntimeReferences
            , featureLocalStubSource: TypeFlagTestFixture.RUNTIME_STUB_SOURCE
        );
}
