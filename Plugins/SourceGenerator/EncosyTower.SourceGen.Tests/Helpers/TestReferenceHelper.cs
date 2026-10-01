using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;

namespace EncosyTower.SourceGen.Tests;

internal static class TestReferenceHelper
{
    private static readonly Lazy<ImmutableArray<MetadataReference>> s_runtimeReferences = new(CreateRuntimeReferences);

    internal static ReferenceAssemblies FrameworkReferences => ReferenceAssemblies.Net.Net50;

    internal static ImmutableArray<MetadataReference> RuntimeReferences => s_runtimeReferences.Value;

    internal static async Task<ImmutableArray<MetadataReference>> ResolveCompilationReferencesAsync(
        CancellationToken token = default
    )
    {
        var frameworkReferences = await FrameworkReferences.ResolveAsync(LanguageNames.CSharp, token);
        var references = frameworkReferences.AddRange(RuntimeReferences);

        if (references.Any(static reference => string.Equals(
              Path.GetFileNameWithoutExtension(reference.Display)
            , "Unity.Properties"
            , StringComparison.OrdinalIgnoreCase
        )))
        {
            return references;
        }

        return references.Add(CreateUnityPropertiesReference(frameworkReferences));
    }

    private static ImmutableArray<MetadataReference> CreateRuntimeReferences()
    {
        var paths = UnityDllPaths.All
            .Append(ResolveNewtonsoftJsonPath())
            .Append(typeof(Microsoft.Extensions.Logging.ILogger).Assembly.Location)
            .Where(static path => string.IsNullOrWhiteSpace(path) == false)
            .Select(static path => Path.GetFullPath(path))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return paths
            .Select(static path => MetadataReference.CreateFromFile(path))
            .ToImmutableArray<MetadataReference>();
    }

    private static string ResolveNewtonsoftJsonPath()
    {
        var libraryDirectory = Directory.GetParent(UnityDllPaths.UnityScriptAssembliesPath)?.FullName;

        if (string.IsNullOrWhiteSpace(libraryDirectory) == false)
        {
            var packageCacheDirectory = Path.Combine(libraryDirectory, "PackageCache");

            if (Directory.Exists(packageCacheDirectory))
            {
                var runtimePaths = Directory
                    .EnumerateDirectories(packageCacheDirectory, "com.unity.nuget.newtonsoft-json@*")
                    .Select(static packageDirectory => Path.Combine(packageDirectory, "Runtime", "Newtonsoft.Json.dll"))
                    .Where(static path => File.Exists(path))
                    .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (runtimePaths.Length == 1)
                {
                    return runtimePaths[0];
                }

                if (runtimePaths.Length > 1)
                {
                    var formattedPaths = string.Join(", ", runtimePaths);
                    throw new InvalidOperationException(
                        $"Multiple Unity Newtonsoft.Json runtime assemblies were found: {formattedPaths}"
                    );
                }
            }
        }

        return typeof(Newtonsoft.Json.JsonConvert).Assembly.Location;
    }

    private static MetadataReference CreateUnityPropertiesReference(
        ImmutableArray<MetadataReference> frameworkReferences
    )
    {
        const string SOURCE = """
            namespace Unity.Properties
            {
                public abstract class ContainerPropertyBag<TContainer>
                {
                    protected void AddProperty<TValue>(Property<TContainer, TValue> property) { }
                }

                public abstract class Property<TContainer, TValue>
                {
                    public abstract string Name { get; }

                    public abstract bool IsReadOnly { get; }

                    public abstract TValue GetValue(ref TContainer container);

                    public abstract void SetValue(ref TContainer container, TValue value);
                }

                public static class PropertyBag
                {
                    public static void Register<TContainer>(ContainerPropertyBag<TContainer> propertyBag) { }

                    public static void RegisterIDictionary<TDictionary, TKey, TValue>() { }

                    public static void RegisterIDictionary<TContainer, TDictionary, TKey, TValue>() { }
                }
            }
            """;
        var compilation = CSharpCompilation.Create(
              "Unity.Properties"
            , new[] { CSharpSyntaxTree.ParseText(SOURCE) }
            , frameworkReferences
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);

        if (result.Success == false)
        {
            throw new InvalidOperationException(
                "Could not create Unity.Properties test reference:\n" + string.Join("\n", result.Diagnostics)
            );
        }

        return MetadataReference.CreateFromImage(stream.ToArray());
    }
}
