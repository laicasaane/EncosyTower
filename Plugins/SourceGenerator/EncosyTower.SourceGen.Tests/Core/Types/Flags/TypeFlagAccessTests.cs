namespace EncosyTower.SourceGen.Tests.Core.Types.Flags;

[TestClass]
public sealed class TypeFlagAccessTests
{
    private const string OWNERS = """
        using EncosyTower.Types;

        namespace MyGame
        {
            [TypeFlag]
            public sealed partial class AudioManager
            {
                public bool Register()
                    => s_typeFlag.TryRegister(this);
            }

            [TypeFlag(WriteAccess = TypeFlagAccess.Internal, Api = TypeFlagApi.Self)]
            public sealed partial class SessionService { }

            [TypeFlag(WriteAccess = TypeFlagAccess.Public)]
            public readonly partial struct GraphicsPreset
            {
                public readonly int Width;

                public GraphicsPreset(int width)
                {
                    Width = width;
                }
            }

            [TypeFlag(UseExtensions = true)]
            public sealed partial class ScoreBoard
            {
                public bool Register()
                    => s_typeFlag.TryRegister(this);
            }

            [TypeFlag(UseExtensions = true, WriteAccess = TypeFlagAccess.Internal)]
            public readonly partial struct Difficulty { }
        }
        """;

    [TestMethod]
    [DataRow("_ = AudioManager.s_typeFlag.Enable();", new[] { "CS0122" })]
    [DataRow("_ = default(AudioManager.TypeFlagReadWrite).Enable();", new[] { "CS0122", "CS0122" })]
    [DataRow("_ = AudioManager.TypeFlag.Enable();", new[] { "CS1061" })]
    [DataRow("_ = AudioManager.TypeFlag.IsEnabled;", new string[0])]
    [DataRow("_ = SessionService.TypeFlag.TryRegister(new SessionService());", new string[0])]
    [DataRow("_ = SessionService.TypeFlag.GetInstanceAsync();", new[] { "CS1061" })]
    public async Task SameAssembly(string statement, string[] expectedIds)
    {
        var run = await RunAsync(new NamedSource("Probe.cs", WrapProbe(statement, withImport: false)));

        TypeFlagTestFixture.AssertCompilerDiagnostics(run, expectedIds);
    }

    [TestMethod]
    public async Task OwnerWrites()
    {
        var run = await RunAsync(new NamedSource("Writer.cs", """
            namespace MyGame
            {
                partial class AudioManager
                {
                    public static bool Write()
                        => s_typeFlag.Enable() && s_typeFlag.TryAddObject(new object());
                }
            }
            """));

        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    [DataRow("_ = ScoreBoard.TypeFlag.TryGetInstance(out _);", false, new[] { "CS1061" })]
    [DataRow("_ = ScoreBoard.TypeFlag.TryGetInstance(out _);", true, new string[0])]
    [DataRow("_ = ScoreBoard.TypeFlag.Enable();", true, new[] { "CS1061" })]
    [DataRow("_ = default(EncosyTower.Types.TypeFlag<ScoreBoard>).Enable();", false, new string[0])]
    public async Task UseExtensionsNeedsImport(string statement, bool withImport, string[] expectedIds)
    {
        var run = await RunAsync(new NamedSource("Probe.cs", WrapProbe(statement, withImport)));

        TypeFlagTestFixture.AssertCompilerDiagnostics(run, expectedIds);
    }

    [TestMethod]
    [DataRow("_ = GraphicsPreset.TypeFlag.Enable();", new string[0])]
    [DataRow("_ = SessionService.TypeFlag.IsEnabled;", new string[0])]
    [DataRow("_ = SessionService.TypeFlag.Enable();", new[] { "CS1061" })]
    [DataRow("_ = default(AudioManager.TypeFlagReadWrite);", new[] { "CS0122" })]
    [DataRow("_ = Difficulty.s_typeFlag;", new[] { "CS0117" })]
    public async Task OtherAssembly(string statement, string[] expectedIds)
    {
        var library = TypeFlagTestFixture.EmitReference(await RunAsync());
        var compilation = await TypeFlagTestFixture.CreateConsumerCompilationAsync(
              library
            , [new NamedSource("Probe.cs", WrapProbe(statement, withImport: false))]
        );

        TypeFlagTestFixture.AssertCompilerDiagnostics(compilation, expectedIds);
    }

    private static Task<TypeFlagRun> RunAsync(params NamedSource[] probes)
    {
        var sources = new List<NamedSource>(probes.Length + 1) { new("Owners.cs", OWNERS) };

        sources.AddRange(probes);
        return TypeFlagTestFixture.RunAsync(sources);
    }

    private static string WrapProbe(string statement, bool withImport)
    {
        var import = withImport ? "using EncosyTower.Types;\n\n" : string.Empty;

        return $$"""
            {{import}}namespace MyGame
            {
                public static class Probe
                {
                    public static void Run()
                    {
                        {{statement}}
                    }
                }
            }
            """;
    }
}
