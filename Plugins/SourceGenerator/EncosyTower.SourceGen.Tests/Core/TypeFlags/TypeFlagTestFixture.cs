using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using EncosyTower.Core.Generators.TypeFlags;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Tests.Core.TypeFlags;

internal static class TypeFlagTestFixture
{
    internal const string ASSEMBLY_NAME = "TestProject";
    internal const string CONSUMER_ASSEMBLY_NAME = "ConsumerProject";

    internal const string ATTRIBUTE_STUB_SOURCE = """
        namespace EncosyTower.TypeFlags
        {
            [System.AttributeUsage(
                  System.AttributeTargets.Class | System.AttributeTargets.Struct
                , AllowMultiple = false
                , Inherited = false
            )]
            public sealed class TypeFlagAttribute : System.Attribute
            {
                public TypeFlagAccess WriteAccess { get; set; }

                public TypeFlagApi Api { get; set; } = TypeFlagApi.Default;

                public bool UseExtensions { get; set; }
            }

            public enum TypeFlagAccess : byte
            {
                Private,
                Internal,
                Public,
            }

            [System.Flags]
            public enum TypeFlagApi : byte
            {
                State = 0,
                Self = 1 << 0,
                Related = 1 << 1,
                Async = 1 << 2,
                Default = Self | Related | Async,
            }

            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public sealed class SkipSourceGeneratorsForAssemblyAttribute : System.Attribute { }
        }

        namespace EncosyTower.CodeGen
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public sealed class SkipSourceGeneratorsForAssemblyAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public sealed class AllowSourceGeneratorsForAssemblyAttribute : System.Attribute
            {
                public AllowSourceGeneratorsForAssemblyAttribute(params string[] namespaces) { }
            }
        }

        """;

    internal const string TYPE_FLAG_STUB_SOURCE = """
        namespace EncosyTower.Tasks
        {
            public readonly struct UnityTask { }

            public readonly struct UnityTask<T> { }
        }

        namespace EncosyTower.Types
        {
            public readonly struct TypeId<T> { }
        }

        namespace EncosyTower.TypeFlags
        {
            using System.Diagnostics.CodeAnalysis;
            using System.Threading;
            using EncosyTower.Tasks;
            using EncosyTower.Types;

            public readonly struct TypeFlag<T>
            {
                public TypeId<T> TypeId => default;

                public bool IsEnabled => false;

                public bool Enable() => false;

                public bool Disable() => false;

                public UnityTask WaitUntilEnabledAsync(CancellationToken token = default) => default;

                public TypeFlagLink<T, TLinked> GetLink<TLinked>() => default;

                public readonly struct ReadOnly
                {
                    public TypeId<T> TypeId => default;

                    public bool IsEnabled => false;

                    public static implicit operator ReadOnly(TypeFlag<T> _) => default;

                    public UnityTask WaitUntilEnabledAsync(CancellationToken token = default) => default;

                    public TypeFlagLink<T, TLinked> GetLink<TLinked>() => default;
                }
            }

            public readonly struct TypeFlagLink<TOwner, TLinked>
            {
                public TypeId<TOwner> OwnerId => default;

                public TypeId<TLinked> LinkedId => default;
            }

            public static class TypeFlagLinkExtensions
            {
                public static bool TryAddObject<TOwner, TObject>(
                      this TypeFlagLink<TOwner, TObject> self
                    , [NotNull] TObject obj
                )
                    where TObject : class
                    => false;

                public static bool TryRemoveObject<TOwner, TObject>(
                      this TypeFlagLink<TOwner, TObject> self
                    , TObject expected
                )
                    where TObject : class
                    => false;

                public static bool TryGetObject<TOwner, TObject>(
                      this TypeFlagLink<TOwner, TObject> self
                    , [MaybeNullWhen(false)] out TObject obj
                )
                    where TObject : class
                {
                    obj = default;
                    return false;
                }

                public static TObject GetObjectOrThrow<TOwner, TObject>(this TypeFlagLink<TOwner, TObject> self)
                    where TObject : class
                    => default;

                public static UnityTask<TObject> GetObjectAsync<TOwner, TObject>(
                      this TypeFlagLink<TOwner, TObject> self
                    , CancellationToken token = default
                )
                    where TObject : class
                    => default;

                public static void SetValue<TOwner, TValue>(this TypeFlagLink<TOwner, TValue> self, TValue value)
                    where TValue : struct
                { }

                public static bool TryRemoveValue<TOwner, TValue>(
                      this TypeFlagLink<TOwner, TValue> self
                    , out TValue value
                )
                    where TValue : struct
                {
                    value = default;
                    return false;
                }

                public static bool TryGetValue<TOwner, TValue>(this TypeFlagLink<TOwner, TValue> self, out TValue value)
                    where TValue : struct
                {
                    value = default;
                    return false;
                }

                public static TValue GetValueOrThrow<TOwner, TValue>(this TypeFlagLink<TOwner, TValue> self)
                    where TValue : struct
                    => default;

                public static UnityTask<TValue> GetValueAsync<TOwner, TValue>(
                      this TypeFlagLink<TOwner, TValue> self
                    , CancellationToken token = default
                )
                    where TValue : struct
                    => default;
            }

            public static class TypeFlagExtensions
            {
                public static bool TryRegister<T>(this TypeFlag<T> self, [NotNull] T instance)
                    where T : class
                    => false;

                public static bool TryUnregister<T>(this TypeFlag<T> self, T instance)
                    where T : class
                    => false;

                public static bool TryGetInstance<T>(this TypeFlag<T> self, [MaybeNullWhen(false)] out T instance)
                    where T : class
                {
                    instance = default;
                    return false;
                }

                public static T GetInstanceOrThrow<T>(this TypeFlag<T> self)
                    where T : class
                    => default;

                public static UnityTask<T> GetInstanceAsync<T>(this TypeFlag<T> self, CancellationToken token = default)
                    where T : class
                    => default;

                public static void SetValue<T>(this TypeFlag<T> self, T value)
                    where T : struct
                { }

                public static bool TryRemoveValue<T>(this TypeFlag<T> self, out T value)
                    where T : struct
                {
                    value = default;
                    return false;
                }

                public static bool TryGetValue<T>(this TypeFlag<T> self, out T value)
                    where T : struct
                {
                    value = default;
                    return false;
                }

                public static T GetValueOrThrow<T>(this TypeFlag<T> self)
                    where T : struct
                    => default;

                public static UnityTask<T> GetValueAsync<T>(this TypeFlag<T> self, CancellationToken token = default)
                    where T : struct
                    => default;
            }

            public static class TypeFlagReadOnlyExtensions
            {
                public static bool TryGetInstance<T>(
                      this TypeFlag<T>.ReadOnly self
                    , [MaybeNullWhen(false)] out T instance
                )
                    where T : class
                {
                    instance = default;
                    return false;
                }

                public static T GetInstanceOrThrow<T>(this TypeFlag<T>.ReadOnly self)
                    where T : class
                    => default;

                public static UnityTask<T> GetInstanceAsync<T>(
                      this TypeFlag<T>.ReadOnly self
                    , CancellationToken token = default
                )
                    where T : class
                    => default;

                public static bool TryGetValue<T>(this TypeFlag<T>.ReadOnly self, out T value)
                    where T : struct
                {
                    value = default;
                    return false;
                }

                public static T GetValueOrThrow<T>(this TypeFlag<T>.ReadOnly self)
                    where T : struct
                    => default;

                public static UnityTask<T> GetValueAsync<T>(
                      this TypeFlag<T>.ReadOnly self
                    , CancellationToken token = default
                )
                    where T : struct
                    => default;
            }
        }

        """;

    internal const string RUNTIME_STUB_SOURCE = ATTRIBUTE_STUB_SOURCE + TYPE_FLAG_STUB_SOURCE;

    private static readonly Lazy<ImmutableArray<MetadataReference>> s_runtimeReferences = new(
        static () => TestReferenceHelper.RuntimeReferences.Where(IsNotEncosyTower).ToImmutableArray()
    );

    internal static ImmutableArray<MetadataReference> RuntimeReferences => s_runtimeReferences.Value;

    internal static async Task<ImmutableArray<MetadataReference>> GetReferencesAsync(CancellationToken token = default)
    {
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync(token);
        return references.Where(IsNotEncosyTower).ToImmutableArray();
    }

    internal static async Task<TypeFlagRun> RunAsync(
          IReadOnlyList<NamedSource> sources
        , TypeFlagRun? previous = null
        , string runtimeSource = RUNTIME_STUB_SOURCE
        , LanguageVersion languageVersion = LanguageVersion.CSharp10
        , IEnumerable<MetadataReference>? additionalReferences = null
        , string assemblyName = ASSEMBLY_NAME
        , DocumentationMode documentationMode = DocumentationMode.Parse
        , CancellationToken token = default
    )
    {
        var parseOptions = CSharpParseOptions.Default
            .WithLanguageVersion(languageVersion)
            .WithDocumentationMode(documentationMode);

        var references = await GetReferencesAsync(token);
        var allSources = new List<NamedSource>(sources.Count + 1) { new("TypeFlagRuntime.cs", runtimeSource) };

        allSources.AddRange(sources);

        if (additionalReferences is not null)
        {
            references = references.AddRange(additionalReferences);
        }

        var compilation = CreateCompilation(
              assemblyName
            , allSources
            , references
            , parseOptions
            , previous?.InputCompilation
            , token
        );

        var driver = previous?.Driver ?? CSharpGeneratorDriver.Create(
              new[] { new TypeFlagGenerator().AsSourceGenerator() }
            , parseOptions: parseOptions
            , driverOptions: new GeneratorDriverOptions(
                  disabledOutputs: default
                , trackIncrementalGeneratorSteps: true
            )
        );

        driver = driver.RunGeneratorsAndUpdateCompilation(
              compilation
            , out var outputCompilation
            , out var diagnostics
            , token
        );

        var result = driver.GetRunResult();

        Assert.AreEqual(
              0
            , diagnostics.Length
            , string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString()))
        );

        Assert.AreEqual(1, result.Results.Length);
        Assert.IsNull(result.Results[0].Exception);
        Assert.AreEqual(0, result.Results[0].Diagnostics.Length);

        return new TypeFlagRun(driver, compilation, outputCompilation, result.Results[0]);
    }

    internal static MetadataReference EmitReference(TypeFlagRun run)
    {
        using var stream = new MemoryStream();
        var result = run.OutputCompilation.Emit(stream);

        Assert.IsTrue(
              result.Success
            , string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.ToString()))
        );

        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    internal static async Task<Compilation> CreateConsumerCompilationAsync(
          MetadataReference library
        , IReadOnlyList<NamedSource> sources
        , CancellationToken token = default
    )
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10);
        var references = await GetReferencesAsync(token);

        return CreateCompilation(
              assemblyName: CONSUMER_ASSEMBLY_NAME
            , sources: sources
            , references: references.Add(library)
            , parseOptions: parseOptions
            , previousCompilation: null
            , token: token
        );
    }

    internal static void AssertCompilerDiagnostics(TypeFlagRun run, params string[] expectedIds)
        => AssertCompilerDiagnostics(run.OutputCompilation, expectedIds);

    internal static void AssertCompilerDiagnostics(Compilation compilation, params string[] expectedIds)
    {
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
            .ToArray();

        var actualIds = diagnostics
            .Select(static diagnostic => diagnostic.Id)
            .OrderBy(static id => id, StringComparer.Ordinal)
            .ToArray();

        var sortedExpectedIds = expectedIds.OrderBy(static id => id, StringComparer.Ordinal).ToArray();

        CollectionAssert.AreEqual(
              sortedExpectedIds
            , actualIds
            , string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString()))
        );
    }

    private static bool IsNotEncosyTower(MetadataReference reference)
        => Path.GetFileName(reference.Display ?? string.Empty)
            .StartsWith("EncosyTower.", StringComparison.OrdinalIgnoreCase) == false;

    private static CSharpCompilation CreateCompilation(
          string assemblyName
        , IReadOnlyList<NamedSource> sources
        , ImmutableArray<MetadataReference> references
        , CSharpParseOptions parseOptions
        , [AllowNull] Compilation previousCompilation
        , CancellationToken token
    )
    {
        var previousTrees = previousCompilation?.SyntaxTrees.ToDictionary(
              static tree => tree.FilePath
            , StringComparer.Ordinal
        ) ?? new Dictionary<string, SyntaxTree>(StringComparer.Ordinal);

        var trees = new List<SyntaxTree>(sources.Count);

        foreach (var source in sources)
        {
            var text = SourceText.From(source.Source, Encoding.UTF8);

            if (previousTrees.TryGetValue(source.Path, out var previousTree)
                && previousTree.GetText(token).ContentEquals(text)
            )
            {
                trees.Add(previousTree);
            }
            else
            {
                trees.Add(CSharpSyntaxTree.ParseText(text, parseOptions, source.Path, token));
            }
        }

        return CSharpCompilation.Create(
              assemblyName
            , trees
            , references
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
        );
    }
}

internal sealed record TypeFlagRun(
      GeneratorDriver Driver
    , Compilation InputCompilation
    , Compilation OutputCompilation
    , GeneratorRunResult Result
);
