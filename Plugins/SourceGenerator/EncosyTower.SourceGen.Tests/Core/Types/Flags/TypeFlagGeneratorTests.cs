using System.Collections.Immutable;
using EncosyTower.Core.Analyzers.Types.Flags;
using EncosyTower.Core.Types.Flags;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.Types.Flags;

[TestClass]
public sealed class TypeFlagGeneratorTests
{
    internal const string CLASS_OWNER = """
        using EncosyTower.Types;

        namespace MyGame.Audio
        {
            [TypeFlag]
            internal sealed partial class AudioManager
            {
                public bool Register()
                    => s_typeFlag.TryRegister(this);
            }
        }
        """;

    internal const string CLASS_OWNER_CONSUMER = """
        namespace MyGame
        {
            internal static class AudioConsumer
            {
                public static bool TryRead()
                    => MyGame.Audio.AudioManager.TypeFlag.TryGetInstance(out _);
            }
        }
        """;

    internal const string STRUCT_OWNER_PUBLIC = """
        using EncosyTower.Types;

        namespace MyGame.Graphics
        {
            [TypeFlag(WriteAccess = TypeFlagAccess.Public)]
            public readonly partial struct GraphicsPreset
            {
                public readonly int Width;
                public readonly int Height;

                public GraphicsPreset(int width, int height)
                {
                    Width = width;
                    Height = height;
                }

                public void Publish()
                {
                    TypeFlag.SetValue(this);
                    TypeFlag.Enable();
                }
            }
        }
        """;

    internal const string RELATED_ONLY = """
        using EncosyTower.Types;

        namespace MyGame.Settings
        {
            public sealed class SoundProfile { }

            [TypeFlag(Api = TypeFlagApi.Related)]
            public sealed partial class SettingsLoader
            {
                public void Load()
                {
                    s_typeFlag.TryAddObject(new SoundProfile());
                    s_typeFlag.Enable();
                }
            }
        }
        """;

    internal const string INTERNAL_SELF = """
        using EncosyTower.Types;

        namespace MyGame.Services
        {
            [TypeFlag(WriteAccess = TypeFlagAccess.Internal, Api = TypeFlagApi.Self)]
            public sealed partial class SessionService { }

            internal static class AppBootstrap
            {
                public static bool Install()
                    => SessionService.TypeFlag.TryRegister(new SessionService());
            }
        }
        """;

    internal const string NESTED_STATE_MARKER = """
        using EncosyTower.Types;
        using UnityEngine;

        namespace MyGame.UIs
        {
            [TypeFlag]
            public sealed partial class MenuScreen : MonoBehaviour
            {
                private void OnEnable()
                {
                    s_typeFlag.Enable();
                }

                private void OnDisable()
                {
                    s_typeFlag.Disable();
                    Initialized.TypeFlag.Disable();
                }

                private void OnPanelsInitialized()
                {
                    Initialized.TypeFlag.Enable();
                }

                [TypeFlag(WriteAccess = TypeFlagAccess.Internal, Api = TypeFlagApi.State)]
                public readonly partial struct Initialized { }
            }
        }
        """;

    internal const string USE_EXTENSIONS_CLASS = """
        using EncosyTower.Types;

        namespace MyGame.Scores
        {
            [TypeFlag(UseExtensions = true)]
            public sealed partial class ScoreBoard
            {
                public bool Register()
                    => s_typeFlag.TryRegister(this);
            }
        }
        """;

    internal const string USE_EXTENSIONS_INTERNAL_STRUCT = """
        using EncosyTower.Types;

        namespace MyGame.Scores
        {
            [TypeFlag(UseExtensions = true, WriteAccess = TypeFlagAccess.Internal)]
            public readonly partial struct Difficulty { }
        }
        """;

    internal const string USE_EXTENSIONS_PUBLIC = """
        using EncosyTower.Types;

        namespace MyGame.Scores
        {
            [TypeFlag(UseExtensions = true, WriteAccess = TypeFlagAccess.Public)]
            public sealed partial class Leaderboard { }
        }
        """;

    internal const string NESTED_GENERIC_OWNER = """
        using EncosyTower.Types;

        namespace MyGame
        {
            public partial class Registry<TKey>
            {
                [TypeFlag]
                public partial struct Slot<TValue>
                {
                    public static bool IsReady()
                        => s_typeFlag.IsEnabled;
                }
            }
        }
        """;

    internal const string DERIVED_HIDING = """
        using EncosyTower.Types;

        namespace TestProject
        {
            [TypeFlag]
            public partial class Base
            {
                public static bool IsBaseReady()
                    => s_typeFlag.IsEnabled;
            }

            [TypeFlag]
            public partial class Derived : Base
            {
                public static bool IsDerivedReady()
                    => s_typeFlag.IsEnabled;
            }
        }
        """;

    internal const string HAND_WRITTEN_GENERIC_BASE = """
        using EncosyTower.Types;
        using UnityEngine;

        namespace MyGame.Markers
        {
            public abstract class SceneMarker<T> : MonoBehaviour
                where T : SceneMarker<T>
            {
                public static TypeFlag<T>.ReadOnly TypeFlag => default;

                protected void Awake()
                {
                    default(TypeFlag<T>).Enable();
                }
            }

            [TypeFlag]
            public sealed partial class SpawnPoint : SceneMarker<SpawnPoint>
            {
                public static bool IsReady()
                    => s_typeFlag.IsEnabled;
            }
        }
        """;

    internal const string PRIVATE_BASE_MEMBERS = """
        using EncosyTower.Types;

        namespace TestProject
        {
            public class Base
            {
                private static int TypeFlag => 0;

                private sealed class TypeFlagAPI { }

                public static object Read()
                    => TypeFlag + new TypeFlagAPI().GetHashCode();
            }

            [TypeFlag]
            public partial class Derived : Base
            {
                public static bool IsReady()
                    => s_typeFlag.IsEnabled;
            }
        }
        """;

    private static readonly string[] s_crefDiagnosticIds = {
        "CS1574",
        "CS1580",
        "CS1581",
        "CS1584",
        "CS1658",
        "CS1723",
    };

    [TestMethod]
    public async Task ClassOwner()
    {
        var run = await RunSourcesAsync(
              new NamedSource("Owner.cs", CLASS_OWNER)
            , new NamedSource("Consumer.cs", CLASS_OWNER_CONSUMER)
        );

        AssertHints(run, "MyGame.Audio.AudioManager");
        await AssertSnapshotAsync(GetSource(run, "MyGame.Audio.AudioManager"), "ClassOwner");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public Task StructOwnerPublic()
        => VerifySnapshotAsync(STRUCT_OWNER_PUBLIC, "MyGame.Graphics.GraphicsPreset", "StructOwnerPublic");

    [TestMethod]
    public Task RelatedOnly()
        => VerifySnapshotAsync(RELATED_ONLY, "MyGame.Settings.SettingsLoader", "RelatedOnly");

    [TestMethod]
    public Task InternalSelf()
        => VerifySnapshotAsync(INTERNAL_SELF, "MyGame.Services.SessionService", "InternalSelf");

    [TestMethod]
    public async Task NestedStateMarker()
    {
        var run = await RunAsync(NESTED_STATE_MARKER);

        AssertHints(run, "MyGame.UIs.MenuScreen", "MyGame.UIs.MenuScreen+Initialized");
        await AssertSnapshotAsync(GetSource(run, "MyGame.UIs.MenuScreen"), "NestedStateMarker.MenuScreen");
        await AssertSnapshotAsync(GetSource(run, "MyGame.UIs.MenuScreen+Initialized"), "NestedStateMarker.Initialized");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public Task UseExtensionsClass()
        => VerifySnapshotAsync(USE_EXTENSIONS_CLASS, "MyGame.Scores.ScoreBoard", "UseExtensionsClass");

    [TestMethod]
    public Task UseExtensionsInternalStruct()
        => VerifySnapshotAsync(
              USE_EXTENSIONS_INTERNAL_STRUCT
            , "MyGame.Scores.Difficulty"
            , "UseExtensionsInternalStruct"
        );

    [TestMethod]
    public Task UseExtensionsPublic()
        => VerifySnapshotAsync(USE_EXTENSIONS_PUBLIC, "MyGame.Scores.Leaderboard", "UseExtensionsPublic");

    [TestMethod]
    public async Task NestedGenericOwner()
    {
        var run = await RunAsync(NESTED_GENERIC_OWNER);

        AssertHints(run, "MyGame.Registry`1+Slot`1");
        await AssertSnapshotAsync(GetSource(run, "MyGame.Registry`1+Slot`1"), "NestedGenericOwner");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task DerivedHiding()
    {
        var run = await RunAsync(DERIVED_HIDING);

        AssertHints(run, "TestProject.Base", "TestProject.Derived");
        await AssertSnapshotAsync(GetSource(run, "TestProject.Base"), "DerivedHiding.Base");
        await AssertSnapshotAsync(GetSource(run, "TestProject.Derived"), "DerivedHiding.Derived");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    [DataRow("public partial struct Owner", "partial struct Owner", true)]
    [DataRow("public readonly partial struct Owner", "partial struct Owner", true)]
    [DataRow("public partial record class Owner", "partial record Owner", false)]
    [DataRow("public partial record struct Owner", "partial record struct Owner", true)]
    [DataRow("public abstract partial class Owner", "partial class Owner", false)]
    [DataRow("public partial class Owner : UnityEngine.MonoBehaviour", "partial class Owner", false)]
    public async Task SupportedShapes(string declaration, string expectedHeader, bool isValueType)
    {
        var run = await RunAsync($$"""
            using EncosyTower.Types;

            namespace TestProject
            {
                [TypeFlag]
                {{declaration}}
                {
                    public static bool IsReady()
                        => s_typeFlag.IsEnabled;
                }
            }
            """);

        AssertHints(run, "TestProject.Owner");

        var source = GetSource(run, "TestProject.Owner");

        AssertHasLine(source, "    " + expectedHeader);
        AssertMemberPresence(source, "public bool TryGetValue(out global::TestProject.Owner value)", isValueType);
        AssertMemberPresence(source, "public bool TryGetInstance(", isValueType == false);
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task RecordClassUnderCSharp9()
    {
        var run = await RunAsync(
              """
              using EncosyTower.Types;

              namespace TestProject
              {
                  [TypeFlag]
                  public partial record Settings
                  {
                      public static bool IsReady()
                          => s_typeFlag.IsEnabled;
                  }
              }
              """
            , languageVersion: LanguageVersion.CSharp9
        );

        AssertHints(run, "TestProject.Settings");
        AssertHasLine(GetSource(run, "TestProject.Settings"), "    partial record Settings");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task GlobalNamespaceOwner()
    {
        var run = await RunAsync("""
            using EncosyTower.Types;

            [TypeFlag]
            public partial class GlobalOwner
            {
                public static bool IsReady()
                    => s_typeFlag.IsEnabled;
            }
            """);

        AssertHints(run, "GlobalOwner");

        var source = GetSource(run, "GlobalOwner");

        Assert.IsFalse(source.Contains("namespace", StringComparison.Ordinal), source);
        AssertHasLine(source, "partial class GlobalOwner");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task EscapedIdentifiers()
    {
        var run = await RunAsync("""
            using EncosyTower.Types;

            namespace MyGame.@event
            {
                [TypeFlag]
                public partial class @class
                {
                    public static bool IsReady()
                        => s_typeFlag.IsEnabled;
                }
            }
            """);

        AssertHints(run, "MyGame.@event.class");

        var source = GetSource(run, "MyGame.@event.class");

        AssertHasLine(source, "namespace MyGame.@event");
        AssertHasLine(source, "    partial class @class");
        StringAssert.Contains(source, "g__ETT.TypeFlag<global::MyGame.@event.@class>");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    [DataRow("State", new[] { "CS1061", "CS1061", "CS1061" })]
    [DataRow("Self", new[] { "CS1061", "CS1061" })]
    [DataRow("Related", new[] { "CS1061", "CS1061" })]
    [DataRow("Async", new[] { "CS1061", "CS1061", "CS1061" })]
    [DataRow("Self | TypeFlagApi.Async", new[] { "CS1061" })]
    [DataRow("Default", new string[0])]
    public async Task ApiGroups(string api, string[] expectedIds)
    {
        var run = await RunSourcesAsync(
              new NamedSource("Owner.cs", $$"""
                  using EncosyTower.Types;

                  namespace TestProject
                  {
                      [TypeFlag(WriteAccess = TypeFlagAccess.Public, Api = TypeFlagApi.{{api}})]
                      public sealed partial class Owner { }
                  }
                  """)
            , new NamedSource("Probes.cs", """
                  namespace TestProject
                  {
                      public static class Probes
                      {
                          public static void Self()
                          {
                              _ = Owner.TypeFlag.TryGetInstance(out _);
                          }

                          public static void SelfAsync()
                          {
                              _ = Owner.TypeFlag.GetInstanceAsync();
                          }

                          public static void Related()
                          {
                              _ = Owner.TypeFlag.TryGetObject<object>(out _);
                          }
                      }
                  }
                  """)
        );

        AssertHints(run, "TestProject.Owner");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run, expectedIds);
    }

    [TestMethod]
    [DataRow("WriteAccess = (TypeFlagAccess)3", false)]
    [DataRow("Api = (TypeFlagApi)8", false)]
    [DataRow("UseExtensions = true, Api = (TypeFlagApi)8", true)]
    public async Task UndefinedOption(string options, bool generates)
    {
        var run = await RunAsync($$"""
            using EncosyTower.Types;

            namespace TestProject
            {
                [TypeFlag({{options}})]
                public partial class Owner { }
            }
            """);

        if (generates)
        {
            AssertHints(run, "TestProject.Owner");
        }
        else
        {
            AssertHints(run);
        }

        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task UndefinedOption_ApiIgnoredWithUseExtensions()
    {
        var expectedRun = await RunAsync(USE_EXTENSIONS_CLASS);
        var run = await RunAsync(USE_EXTENSIONS_CLASS.Replace(
              "[TypeFlag(UseExtensions = true)]"
            , "[TypeFlag(UseExtensions = true, Api = TypeFlagApi.State)]"
            , StringComparison.Ordinal
        ));

        AssertHints(run, "MyGame.Scores.ScoreBoard");
        Assert.AreEqual(
              GetSource(expectedRun, "MyGame.Scores.ScoreBoard")
            , GetSource(run, "MyGame.Scores.ScoreBoard")
        );

        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    [DataRow("public int TypeFlag => 0;")]
    [DataRow("public static void TypeFlag() { }")]
    [DataRow("private static int s_typeFlag = 1; public static int Read() => s_typeFlag;")]
    [DataRow("public sealed class TypeFlagAPI { }")]
    [DataRow("private struct TypeFlagReadWrite { } public static object Read() => default(TypeFlagReadWrite);")]
    [DataRow("internal static TypeFlag<Owner>.ReadOnly TypeFlag => default;")]
    public async Task OwnMemberConflict(string members)
    {
        var run = await RunAsync($$"""
            using EncosyTower.Types;

            namespace TestProject
            {
                [TypeFlag]
                public partial class Owner
                {
                    {{members}}
                }
            }
            """);

        AssertHints(run);
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task OwnMemberConflict_PositionalRecord()
    {
        var run = await RunAsync("""
            using EncosyTower.Types;

            namespace TestProject
            {
                [TypeFlag]
                public partial record Settings(int TypeFlag);
            }
            """);

        AssertHints(run);
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    [DataRow(
        "WriteAccess = TypeFlagAccess.Public",
        "private static int s_typeFlag = 1; public static int Read() => s_typeFlag;"
    )]
    [DataRow("UseExtensions = true", "public sealed class TypeFlagAPI { }")]
    public async Task NameFreeInOtherMode(string options, string members)
    {
        var run = await RunAsync($$"""
            using EncosyTower.Types;

            namespace TestProject
            {
                [TypeFlag({{options}})]
                public partial class Owner
                {
                    {{members}}

                    public static bool IsReady()
                        => TypeFlag.IsEnabled;
                }
            }
            """);

        AssertHints(run, "TestProject.Owner");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    [DataRow("[TypeFlag]", "TypeFlag", false)]
    [DataRow("[TypeFlag]", "TypeFlagAPI", false)]
    [DataRow("[TypeFlag(WriteAccess = TypeFlagAccess.Public)]", "TypeFlagReadWrite", true)]
    public async Task OwnerNamedAfterMember(string marker, string ownerName, bool generates)
    {
        var run = await RunAsync($$"""
            using EncosyTower.Types;

            namespace TestProject
            {
                {{marker}}
                public partial class {{ownerName}} { }
            }
            """);

        if (generates)
        {
            AssertHints(run, $"TestProject.{ownerName}");
        }
        else
        {
            AssertHints(run);
        }

        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task DerivedNestedInMarkedBase()
    {
        var run = await RunAsync("""
            using EncosyTower.Types;

            namespace TestProject
            {
                [TypeFlag]
                public partial class B
                {
                    public static bool IsReady()
                        => s_typeFlag.IsEnabled && D.IsReady();

                    [TypeFlag]
                    private sealed partial class D : B
                    {
                        public static new bool IsReady()
                            => s_typeFlag.IsEnabled;
                    }
                }
            }
            """);

        AssertHints(run, "TestProject.B", "TestProject.B+D");

        var source = GetSource(run, "TestProject.B+D");

        AssertHasMember(source, "public static new readonly TypeFlagAPI TypeFlag = default;");
        AssertHasMember(source, "private static new readonly TypeFlagReadWrite s_typeFlag = default;");
        AssertHasMember(source, "public new readonly struct TypeFlagAPI");
        AssertHasMember(source, "private new readonly struct TypeFlagReadWrite");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task ExtensionsBaseInternalWriter()
    {
        var run = await RunAsync("""
            using EncosyTower.Types;

            namespace TestProject
            {
                [TypeFlag(UseExtensions = true, WriteAccess = TypeFlagAccess.Internal)]
                public partial class Base { }

                [TypeFlag(UseExtensions = true)]
                public partial class Derived : Base
                {
                    public static bool IsReady()
                        => s_typeFlag.IsEnabled;
                }
            }
            """);

        AssertHints(run, "TestProject.Base", "TestProject.Derived");

        var source = GetSource(run, "TestProject.Derived");

        AssertHasMember(
              source
            , "public static new readonly g__ETT.TypeFlag<global::TestProject.Derived>.ReadOnly TypeFlag = default;"
        );

        AssertHasMember(
              source
            , "private static new readonly g__ETT.TypeFlag<global::TestProject.Derived> s_typeFlag = default;"
        );

        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task HandWrittenGenericBase()
    {
        var run = await RunAsync(HAND_WRITTEN_GENERIC_BASE);

        AssertHints(run, "MyGame.Markers.SpawnPoint");

        var source = GetSource(run, "MyGame.Markers.SpawnPoint");

        AssertHasLine(source, "        public static new readonly TypeFlagAPI TypeFlag = default;");
        AssertHasLine(source, "        private static readonly TypeFlagReadWrite s_typeFlag = default;");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task MetadataMarkedBase()
    {
        var libraryRun = await TypeFlagTestFixture.RunAsync(
              [new NamedSource("Base.cs", """
                  using EncosyTower.Types;

                  namespace Library
                  {
                      [TypeFlag]
                      public partial class Base
                      {
                          public static bool IsReady()
                              => s_typeFlag.IsEnabled;
                      }
                  }
                  """)]
            , assemblyName: "LibraryProject"
        );

        var library = TypeFlagTestFixture.EmitReference(libraryRun);
        var run = await TypeFlagTestFixture.RunAsync(
              [new NamedSource("Derived.cs", """
                  using EncosyTower.Types;

                  namespace TestProject
                  {
                      [TypeFlag]
                      public partial class Derived : Library.Base
                      {
                          public static new bool IsReady()
                              => s_typeFlag.IsEnabled;
                      }
                  }
                  """)]
            , runtimeSource: string.Empty
            , additionalReferences: [library]
        );

        AssertHints(run, "TestProject.Derived");

        var source = GetSource(run, "TestProject.Derived");

        AssertHasLine(source, "        public static new readonly TypeFlagAPI TypeFlag = default;");
        AssertHasLine(source, "        private static readonly TypeFlagReadWrite s_typeFlag = default;");
        AssertHasLine(source, "        public new readonly struct TypeFlagAPI");
        AssertHasLine(source, "        private readonly struct TypeFlagReadWrite");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task InheritedConflict()
    {
        var run = await RunAsync("""
            using EncosyTower.Types;

            namespace TestProject
            {
                public class Base
                {
                    public int TypeFlag { get; }
                }

                [TypeFlag]
                public partial class Derived : Base { }
            }
            """);

        AssertHints(run);
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task InheritedConflictThroughMarkedBase()
    {
        var run = await RunAsync("""
            using EncosyTower.Types;

            namespace TestProject
            {
                public class A
                {
                    public int TypeFlag { get; }
                }

                [TypeFlag]
                public partial class B : A { }

                [TypeFlag]
                public partial class D : B { }
            }
            """);

        AssertHints(run);
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task PrivateBaseMembers()
    {
        var run = await RunAsync(PRIVATE_BASE_MEMBERS);

        AssertHints(run, "TestProject.Derived");

        var source = GetSource(run, "TestProject.Derived");

        Assert.IsFalse(source.Contains(" new ", StringComparison.Ordinal), source);
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task UnmarkedDerived()
    {
        var run = await RunAsync("""
            using EncosyTower.Types;

            namespace TestProject
            {
                [TypeFlag]
                public partial class Base
                {
                    public static bool IsReady()
                        => s_typeFlag.IsEnabled;
                }

                public class Derived : Base { }
            }
            """);

        AssertHints(run, "TestProject.Base");
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    [DataRow("public static partial class Owner { }")]
    [DataRow("public ref partial struct Owner { }")]
    public async Task UnsupportedOwner(string declaration)
    {
        var run = await RunAsync($$"""
            using EncosyTower.Types;

            namespace TestProject
            {
                [TypeFlag]
                {{declaration}}
            }
            """);

        AssertHints(run);
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    [DataRow(
        """
        [TypeFlag(WriteAccess = TypeFlagAccess.Public)]
        public class Owner { }
        """,
        "TestProject.Owner"
    )]
    [DataRow(
        """
        public class Outer
        {
            [TypeFlag(WriteAccess = TypeFlagAccess.Public)]
            public partial class Owner { }
        }
        """,
        "TestProject.Outer+Owner"
    )]
    public async Task MissingPartial(string declaration, string metadataName)
    {
        var run = await RunAsync($$"""
            using EncosyTower.Types;

            namespace TestProject
            {
            {{declaration}}
            }
            """);

        AssertHints(run, metadataName);
        TypeFlagTestFixture.AssertCompilerDiagnostics(run, "CS0260");
    }

    [TestMethod]
    public async Task RepeatedMarker()
    {
        var run = await RunAsync("""
            using EncosyTower.Types;

            namespace TestProject
            {
                [TypeFlag]
                public partial class Owner { }

                [TypeFlag]
                public partial class Owner { }
            }
            """);

        AssertHints(run);
        TypeFlagTestFixture.AssertCompilerDiagnostics(run, "CS0579");
    }

    [TestMethod]
    [DataRow("[assembly: EncosyTower.Types.SkipSourceGeneratorsForAssembly]", false)]
    [DataRow("[assembly: EncosyTower.CodeGen.SkipSourceGeneratorsForAssembly]", false)]
    [DataRow(
        """
        [assembly: EncosyTower.CodeGen.SkipSourceGeneratorsForAssembly]
        [assembly: EncosyTower.CodeGen.AllowSourceGeneratorsForAssembly("EncosyTower.Types")]
        """,
        true
    )]
    public async Task SkippedAssembly(string assemblyAttributes, bool generates)
    {
        var run = await RunAsync($$"""
            using EncosyTower.Types;

            {{assemblyAttributes}}

            namespace TestProject
            {
                [TypeFlag(WriteAccess = TypeFlagAccess.Public)]
                public partial class Owner { }
            }
            """);

        if (generates)
        {
            AssertHints(run, "TestProject.Owner");
        }
        else
        {
            AssertHints(run);
        }

        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    public async Task MissingRuntime()
    {
        var run = await RunAsync(
              """
              using EncosyTower.Types;

              namespace TestProject
              {
                  [TypeFlag]
                  public partial class Owner { }
              }
              """
            , runtimeSource: TypeFlagTestFixture.ATTRIBUTE_STUB_SOURCE
        );

        AssertHints(run);
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    [TestMethod]
    [DataRow(
        """
        [TypeFlag]
        public partial class Owner : MissingBase { }
        """
    )]
    [DataRow(
        """
        public partial class Base : MissingBase { }

        [TypeFlag]
        public partial class Owner : Base { }
        """
    )]
    public async Task UnresolvedBase(string declarations)
    {
        var run = await RunAsync($$"""
            using EncosyTower.Types;

            namespace TestProject
            {
            {{declarations}}
            }
            """);

        AssertHints(run);
        TypeFlagTestFixture.AssertCompilerDiagnostics(run, "CS0246");
    }

    [TestMethod]
    [DataRow(CLASS_OWNER)]
    [DataRow(STRUCT_OWNER_PUBLIC)]
    [DataRow(RELATED_ONLY)]
    [DataRow(INTERNAL_SELF)]
    [DataRow(NESTED_STATE_MARKER)]
    [DataRow(USE_EXTENSIONS_CLASS)]
    [DataRow(USE_EXTENSIONS_INTERNAL_STRUCT)]
    [DataRow(USE_EXTENSIONS_PUBLIC)]
    [DataRow(NESTED_GENERIC_OWNER)]
    [DataRow(DERIVED_HIDING)]
    [DataRow(HAND_WRITTEN_GENERIC_BASE)]
    [DataRow(PRIVATE_BASE_MEMBERS)]
    public async Task GeneratedOutputPassesAnalyzer(string source)
    {
        var run = await RunAsync(source);

        Assert.AreNotEqual(0, run.Result.GeneratedSources.Length);

        var diagnostics = await run.OutputCompilation
            .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new TypeFlagAnalyzer()))
            .GetAnalyzerDiagnosticsAsync();

        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    [DataRow(CLASS_OWNER)]
    [DataRow(STRUCT_OWNER_PUBLIC)]
    [DataRow(RELATED_ONLY)]
    [DataRow(INTERNAL_SELF)]
    [DataRow(NESTED_STATE_MARKER)]
    [DataRow(USE_EXTENSIONS_CLASS)]
    [DataRow(USE_EXTENSIONS_INTERNAL_STRUCT)]
    [DataRow(USE_EXTENSIONS_PUBLIC)]
    [DataRow(NESTED_GENERIC_OWNER)]
    [DataRow(DERIVED_HIDING)]
    [DataRow(HAND_WRITTEN_GENERIC_BASE)]
    [DataRow(PRIVATE_BASE_MEMBERS)]
    public async Task GeneratedDocumentationResolves(string source)
    {
        var run = await TypeFlagTestFixture.RunAsync(
              [new NamedSource("Owner.cs", source)]
            , documentationMode: DocumentationMode.Diagnose
        );

        Assert.AreNotEqual(0, run.Result.GeneratedSources.Length);

        var crefDiagnostics = run.OutputCompilation.GetDiagnostics()
            .Where(static diagnostic => s_crefDiagnosticIds.Contains(diagnostic.Id))
            .ToArray();

        Assert.AreEqual(
              0
            , crefDiagnostics.Length
            , string.Join(Environment.NewLine, crefDiagnostics.Select(static diagnostic => diagnostic.ToString()))
        );
    }

    internal static string GetHintName(string metadataName)
        => SourceGenHelpers.BuildSemanticHintName(
              TypeFlagRules.GENERATOR_METADATA_NAME
            , TypeFlagTestFixture.ASSEMBLY_NAME
            , metadataName
            , "TypeFlag"
            , string.Empty
        );

    internal static string GetSource(TypeFlagRun run, string metadataName)
    {
        var hintName = GetHintName(metadataName);

        return run.Result.GeneratedSources
            .Single(source => string.Equals(source.HintName, hintName, StringComparison.Ordinal))
            .SourceText
            .ToString();
    }

    internal static void AssertHints(TypeFlagRun run, params string[] metadataNames)
    {
        var expected = metadataNames
            .Select(static name => GetHintName(name))
            .OrderBy(static hint => hint, StringComparer.Ordinal)
            .ToArray();

        var actual = run.Result.GeneratedSources
            .Select(static source => source.HintName)
            .OrderBy(static hint => hint, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(expected, actual);
    }

    private static Task<TypeFlagRun> RunAsync(
          string source
        , string runtimeSource = TypeFlagTestFixture.RUNTIME_STUB_SOURCE
        , LanguageVersion languageVersion = LanguageVersion.CSharp10
    )
        => TypeFlagTestFixture.RunAsync(
              [new NamedSource("Owner.cs", source)]
            , previous: null
            , runtimeSource: runtimeSource
            , languageVersion: languageVersion
        );

    private static Task<TypeFlagRun> RunSourcesAsync(params NamedSource[] sources)
        => TypeFlagTestFixture.RunAsync(sources);

    private static async Task VerifySnapshotAsync(string source, string metadataName, string snapshotName)
    {
        var run = await RunAsync(source);

        AssertHints(run, metadataName);
        await AssertSnapshotAsync(GetSource(run, metadataName), snapshotName);
        TypeFlagTestFixture.AssertCompilerDiagnostics(run);
    }

    private static void AssertHasLine(string source, string line)
    {
        var lines = source.Split('\n');

        Assert.IsTrue(lines.Contains(line, StringComparer.Ordinal), source);
    }

    private static void AssertHasMember(string source, string text)
        => AssertMemberPresence(source: source, text: text, expected: true);

    private static void AssertMemberPresence(string source, string text, bool expected)
    {
        var lines = source.Split('\n');
        var found = lines.Any(line => line.TrimStart().StartsWith(text, StringComparison.Ordinal));

        Assert.AreEqual(expected, found, source);
    }

    private static async Task AssertSnapshotAsync(string source, string name)
    {
        var path = Path.Combine(
              FindSourceGeneratorRoot()
            , "EncosyTower.SourceGen.Tests"
            , "Core"
            , "Types"
            , "Flags"
            , "Snapshots"
            , $"TypeFlagGeneratorTests.{name}.verified.cs"
        );

        var mismatch = await GeneratedSourceSnapshot.VerifyAsync(source, path);

        Assert.IsNull(mismatch, mismatch);
    }

    private static string FindSourceGeneratorRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (File.Exists(Path.Combine(directory.FullName, "EncosyTower.SourceGen.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate EncosyTower.SourceGen.slnx from '{AppContext.BaseDirectory}'."
        );
    }
}
