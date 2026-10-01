using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class Program
{
    private const int SCHEMA_VERSION = 1;
    private const string ATTRIBUTE_NAME = "EncosyTower.CodeGen.CodeGeneratorAttribute";
    private const string INTERFACE_NAME = "EncosyTower.CodeGen.ICodeGenerator";
    private const string SOURCE_ROOT_NAMESPACE = "EncosyTower";
    private const string GENERATED_ROOT_NAMESPACE =
        "EncosyTower.Temp_Generated.CodeGen";

#if ENCOSY_CODEGEN_PREPARE
    private const string BUILD_PHASE = "prepare";
#elif ENCOSY_CODEGEN_EXECUTE
    private const string BUILD_PHASE = "execute";
#else
#error The Runner requires CodeGenPhase=Prepare or CodeGenPhase=Execute.
#endif

    private static readonly string[] s_runnerOwnedTokens = {
        "{{ENCOSY_LANGUAGE_VERSION}}",
        "{{ENCOSY_NULLABLE_MODE}}",
        "{{ENCOSY_ALLOW_UNSAFE_BLOCKS}}",
        "{{ENCOSY_DEFINE_CONSTANTS}}",
        "{{ENCOSY_PATH_MAP}}",
        "{{ENCOSY_ADDITIONAL_OPTIONS_PROPERTY}}",
        "{{ENCOSY_CODE_ANALYSIS_RULE_SET_PROPERTY}}",
        "{{ENCOSY_PROJECT_ROOT}}",
        "{{ENCOSY_UNITY_VERSION_ROOT}}",
        "{{ENCOSY_ISLAND_ID}}",
        "{{ENCOSY_SOURCE_ITEMS}}",
        "{{ENCOSY_REFERENCE_ITEMS}}",
        "{{ENCOSY_ANALYZER_ITEMS}}",
        "{{ENCOSY_ADDITIONAL_FILE_ITEMS}}",
        "{{ENCOSY_GLOBAL_ANALYZER_CONFIG_ITEM}}",
        "{{ENCOSY_ISLAND_PROJECT_REFERENCES}}",
        "{{ENCOSY_ISLAND_SOLUTION_ENTRIES}}",
    };

    private static readonly StringComparer s_pathComparer = Path.DirectorySeparatorChar == '\\'
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private static int Main(string[] args)
    {
        try
        {
            var options = ParseArguments(args);
            var phase = Require(options, "--phase");

            var phaseMismatchesBuild = BUILD_PHASE == "prepare"
                && phase != "prepare"
                || BUILD_PHASE == "execute"
                && phase is not ("execute" or "worker");

            ThrowHelper.ThrowIfRunnerPhaseMismatch(phaseMismatchesBuild, phase, BUILD_PHASE);

            return phase switch {
                "prepare" => Prepare(Require(options, "--input")),
                "execute" => Execute(Require(options, "--manifest")),
                "worker" => Worker(Require(options, "--island"), Require(options, "--manifest")),
                _ => ThrowHelper.ThrowUnknownCodeGenPhase(phase),
            };
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static int Prepare(string inputPath)
    {
        using var input = ReadDocument(inputPath, "compiler-input");
        var root = input.RootElement;
        RequireSchema(root, "compiler-input");
        var workspaceRoot = Directory.GetParent(Path.GetDirectoryName(inputPath))!.FullName;
        var projectRoot = RequireString(root, "projectRoot");
        var unityVersionRoot = RequireString(root, "unityVersionRoot");
        var islandProjectTemplatePath = Path.Combine(workspaceRoot, "Island", "Island.csproj");
        var generatedPropsTemplatePath = Path.Combine(workspaceRoot, "Island", "GeneratedIslands.props");
        var solutionTemplatePath = Path.Combine(workspaceRoot, "EncosyCodeGen.slnx");
        var islandProjectTemplate = ReadTemplate(islandProjectTemplatePath);
        var generatedPropsTemplate = ReadTemplate(generatedPropsTemplatePath);
        var solutionTemplate = ReadTemplate(solutionTemplatePath);
        var islands = new List<Island>();
        var skipped = new List<Skip>();
        var copiedNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var assembly in root.GetProperty("assemblies").EnumerateArray())
        {
            var island = PrepareIsland(
                  workspaceRoot
                , projectRoot
                , unityVersionRoot
                , islandProjectTemplatePath
                , islandProjectTemplate
                , assembly
                , skipped
                , copiedNames
            );

            if (island is not null)
            {
                islands.Add(island);
            }
        }

        islands.Sort(static (left, right) => string.CompareOrdinal(left.Id, right.Id));
        WriteSkipped(Path.Combine(workspaceRoot, "manifests", "skipped-generators.json"), skipped);
        WriteIslands(
              Path.Combine(workspaceRoot, "manifests", "islands.json")
            , workspaceRoot
            , projectRoot
            , RequireString(root, "dotnetExecutablePath")
            , islands
        );

        WriteGeneratedProps(
              Path.Combine(workspaceRoot, "manifests", "GeneratedIslands.props")
            , generatedPropsTemplatePath
            , generatedPropsTemplate
            , islands
        );

        WriteSolution(
              Path.Combine(workspaceRoot, "EncosyCodeGen.slnx")
            , solutionTemplatePath
            , solutionTemplate
            , islands
        );

        if (islands.Count == 0)
        {
            WriteAggregate(
                  Path.Combine(workspaceRoot, "results", "aggregate-result.json")
                , projectRoot
                , Array.Empty<Generated>()
                , Array.Empty<CodeDiagnostic>()
                , skipped.Count > 0
            );
            return 0;
        }

        return RunNestedExecute(workspaceRoot, RequireString(root, "dotnetExecutablePath"));
    }

    private static Island PrepareIsland(
          string workspaceRoot
        , string projectRoot
        , string unityVersionRoot
        , string islandProjectTemplatePath
        , string islandProjectTemplate
        , JsonElement assembly
        , List<Skip> skipped
        , HashSet<string> copiedNames
    )
    {
        var owner = RequireString(assembly, "name");
        var parseOptions = CreateParseOptions(assembly);
        var trees = new List<SyntaxTree>();

        foreach (var source in assembly.GetProperty("sourceFiles").EnumerateArray())
        {
            var path = Path.GetFullPath(source.GetString()!);
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path, Encoding.UTF8), parseOptions, path);
            trees.Add(tree);
        }

        var references = ReadReferences(assembly);
        var compilation = CSharpCompilation.Create(
              owner + ".CodeGen.Discovery"
            , trees
            , references
            , CreateCompilationOptions(assembly)
        );
        var attribute = compilation.GetTypeByMetadataName(ATTRIBUTE_NAME);
        var contract = compilation.GetTypeByMetadataName(INTERFACE_NAME);

        if (attribute is null || contract is null)
        {
            return null;
        }

        var candidates = GetAllTypes(compilation.Assembly.GlobalNamespace)
            .Where(type => IsCandidate(type, attribute, contract))
            .OrderBy(static type => type.ToDisplayString(), StringComparer.Ordinal)
            .ToArray();

        if (candidates.Length == 0)
        {
            return null;
        }

        var accepted = new List<INamedTypeSymbol>();

        var candidateCount = candidates.Length;

        for (var candidateIndex = 0; candidateIndex < candidateCount; candidateIndex++)
        {
            var candidate = candidates[candidateIndex];
            var sourceError = ValidateCandidateSource(candidate, attribute, projectRoot, out var candidatePath);

            if (sourceError is not null)
            {
                var location = candidate
                    .Locations
                    .First(static value => value.IsInSource)
                    .GetLineSpan();

                skipped.Add(
                      new Skip(
                            candidate.ToDisplayString()
                          , candidatePath
                          , "ECG4301"
                          , sourceError
                          , location.StartLinePosition.Line + 1
                          , location.StartLinePosition.Character + 1
                      )
                );
                continue;
            }

            var invalid = candidate
                .DeclaringSyntaxReferences
                .Select(static reference => reference.SyntaxTree)
                .Distinct()
                .SelectMany(static tree => tree.GetDiagnostics())
                .FirstOrDefault(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

            if (invalid is not null)
            {
                var span = invalid.Location.GetLineSpan();
                skipped.Add(
                      new Skip(
                            candidate.ToDisplayString()
                          , span.Path
                          , invalid.Id
                          , invalid.GetMessage()
                          , span.StartLinePosition.Line + 1
                          , span.StartLinePosition.Character + 1
                      )
                );
                continue;
            }

            var identity = GetGeneratedMetadataName(candidate);

            ThrowHelper.ThrowIfGeneratorNameCollision(copiedNames.Add(identity) == false, identity);

            accepted.Add(candidate);
        }

        if (accepted.Count == 0)
        {
            return null;
        }

        var id = Sanitize(owner);
        var islandRoot = Path.Combine(workspaceRoot, "islands", id);
        var sourceRoot = Path.Combine(workspaceRoot, "sources", id);
        Directory.CreateDirectory(islandRoot);
        Directory.CreateDirectory(sourceRoot);
        var sourcePaths = RewriteSources(accepted, compilation, sourceRoot, projectRoot);
        var projectPath = Path.Combine(islandRoot, id + ".csproj");
        WriteIslandProject(
              projectPath
            , islandProjectTemplatePath
            , islandProjectTemplate
            , id
            , projectRoot
            , unityVersionRoot
            , sourceRoot
            , assembly
            , sourcePaths
        );
        var targetPath = Path.Combine(workspaceRoot, "artifacts", "islands", id, "EncosyCodeGenIsland.dll");
        var workerManifest = Path.Combine(workspaceRoot, "manifests", id + ".worker.json");
        var resultPath = Path.Combine(workspaceRoot, "results", id + ".json");
        WriteWorkerManifest(
              workerManifest
            , workspaceRoot
            , projectRoot
            , unityVersionRoot
            , id
            , targetPath
            , resultPath
            , assembly
            , accepted
        );
        return new Island(id, projectPath, targetPath, workerManifest, resultPath);
    }

    private static int RunNestedExecute(string workspaceRoot, string dotnetPath)
    {
        var runner = Path.Combine(workspaceRoot, "Runner", "EncosyCodeGenRunner.csproj");
        var manifest = Path.Combine(workspaceRoot, "manifests", "islands.json");
        var startInfo = new ProcessStartInfo(dotnetPath)
        {
            WorkingDirectory = workspaceRoot,
            UseShellExecute = false,
        };

        Add(
              startInfo
            , "run"
            , "--project"
            , runner
            , "--configuration"
            , "Release"
            , "--no-launch-profile"
            , "--property:CodeGenPhase=Execute"
            , "--"
            , "--phase"
            , "execute"
            , "--manifest"
            , manifest
        );
        using var process = Process.Start(startInfo)!;
        process.WaitForExit();
        return process.ExitCode;
    }

    private static int Execute(string manifestPath)
    {
        using var document = ReadDocument(manifestPath, "islands");
        var root = document.RootElement;
        RequireSchema(root, "islands");
        var workspaceRoot = RequireString(root, "workspaceRoot");
        var runnerDll = Assembly.GetExecutingAssembly().Location;
        var dotnetPath = RequireString(root, "dotnetExecutablePath");
        var generated = new List<Generated>();
        var diagnostics = new List<CodeDiagnostic>();
        var islandIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var island in root.GetProperty("islands").EnumerateArray())
        {
            var islandId = RequireString(island, "islandId");

            ThrowHelper.ThrowIfDuplicateIslandId(islandIds.Add(islandId) == false, islandId);

            var targetPath = RequireString(island, "targetPath");
            var workerManifest = RequireString(island, "workerManifestPath");

            ThrowHelper.ThrowIfIslandOutputMissing(File.Exists(targetPath) == false, targetPath);

            var startInfo = new ProcessStartInfo(dotnetPath)
            {
                WorkingDirectory = workspaceRoot,
                UseShellExecute = false,
            };
            Add(startInfo, runnerDll, "--phase", "worker", "--island", targetPath, "--manifest", workerManifest);
            using var process = Process.Start(startInfo)!;
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                return process.ExitCode;
            }

            ReadWorkerResult(RequireString(island, "resultPath"), generated, diagnostics);
        }

        generated.Sort(static (left, right) => string.CompareOrdinal(left.FilePath, right.FilePath));
        var projectRoot = RequireString(root, "projectRoot");
        WriteAggregate(
              Path.Combine(workspaceRoot, "results", "aggregate-result.json")
            , projectRoot
            , generated
            , diagnostics
            , false
        );
        return 0;
    }

    private static int Worker(string islandPath, string manifestPath)
    {
        using var document = ReadDocument(manifestPath, "worker");
        var root = document.RootElement;
        RequireSchema(root, "worker");
        var referenceMap = root
            .GetProperty("references")
            .EnumerateArray()
            .Select(static item => RequireString(item, "path"))
            .Where(File.Exists)
            .GroupBy(Path.GetFileNameWithoutExtension, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.OrdinalIgnoreCase);

        AssemblyLoadContext.Default.Resolving += (_, name) =>
            referenceMap.TryGetValue(name.Name!, out var path)
            ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path)
            : null;
        var island = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(islandPath));
        var generated = new List<Generated>();

        foreach (
            var typeName in root
                .GetProperty("generatorTypes")
                .EnumerateArray()
                .Select(static value => value.GetString()!)
                .OrderBy(static value => value, StringComparer.Ordinal)
        )
        {
            var type = island.GetType(typeName, throwOnError: true)!;
            var instance = Activator.CreateInstance(type, nonPublic: true)!;
            var method = type.GetMethod("Generate", BindingFlags.Instance | BindingFlags.Public)!;
            var values = (Array)method.Invoke(instance, null)!;
            var valueCount = values.Length;

            for (var valueIndex = 0; valueIndex < valueCount; valueIndex++)
            {
                var value = values.GetValue(valueIndex)!;
                var valueType = value.GetType();
                var filePath = (string)valueType.GetField("filePath")!.GetValue(value)!;
                var content = (string)valueType.GetField("content")!.GetValue(value)!;
                generated.Add(new Generated(filePath, content));
            }
        }

        WriteWorkerResult(RequireString(root, "resultPath"), RequireString(root, "islandId"), generated);
        return 0;
    }

    private static CSharpParseOptions CreateParseOptions(JsonElement assembly)
    {
        var options = assembly.GetProperty("compilerOptions");
        var languageText = RequireString(options, "languageVersion");
        LanguageVersionFacts.TryParse(languageText, out var languageVersion);
        var defines = assembly
            .GetProperty("defines")
            .EnumerateArray()
            .Select(static value => value.GetString()!)
            .ToArray();
        return new CSharpParseOptions(languageVersion, DocumentationMode.Parse, SourceCodeKind.Regular, defines);
    }

    private static CSharpCompilationOptions CreateCompilationOptions(JsonElement assembly)
    {
        var options = assembly.GetProperty("compilerOptions");
        return new CSharpCompilationOptions(
              OutputKind.DynamicallyLinkedLibrary
            , allowUnsafe: options.GetProperty("allowUnsafeCode").GetBoolean()
        );
    }

    private static MetadataReference[] ReadReferences(JsonElement assembly)
        => assembly
            .GetProperty("references")
            .EnumerateArray()
            .Select(static reference => RequireString(reference, "path"))
            .Append(RequireString(assembly, "outputPath"))
            .Where(File.Exists)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .Select(static path => MetadataReference.CreateFromFile(path))
            .ToArray();

    private static bool IsUnityFrameworkReference(string unityVersionRoot, string path)
    {
        var frameworkRoot = Path.Combine(unityVersionRoot, "Editor", "Data", "UnityReferenceAssemblies");
        var canonicalRoot = Path
            .GetFullPath(frameworkRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var canonicalPath = Path.GetFullPath(path);
        var prefix = canonicalRoot + Path.DirectorySeparatorChar;
        return string.Equals(canonicalRoot, canonicalPath, StringComparison.Ordinal)
            || canonicalPath.StartsWith(prefix, StringComparison.Ordinal);
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol owner)
    {
        var types = owner.GetTypeMembers();
        var typeCount = types.Length;

        for (var typeIndex = 0; typeIndex < typeCount; typeIndex++)
        {
            var type = types[typeIndex];

            foreach (var nested in GetAllTypes(type))
            {
                yield return nested;
            }
        }

        foreach (var child in owner.GetNamespaceMembers())
        {
            foreach (var type in GetAllTypes(child))
            {
                yield return type;
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamedTypeSymbol owner)
    {
        yield return owner;

        var nestedTypes = owner.GetTypeMembers();
        var nestedTypeCount = nestedTypes.Length;

        for (var nestedTypeIndex = 0; nestedTypeIndex < nestedTypeCount; nestedTypeIndex++)
        {
            var nested = nestedTypes[nestedTypeIndex];

            foreach (var type in GetAllTypes(nested))
            {
                yield return type;
            }
        }
    }

    private static bool IsCandidate(INamedTypeSymbol type, INamedTypeSymbol attribute, INamedTypeSymbol contract)
        => type.Locations.Any(static location => location.IsInSource)
            && type.IsAbstract == false
            && type.Arity == 0
            && type.IsStatic == false
            && type.TypeKind is TypeKind.Class or TypeKind.Struct
            && type.InstanceConstructors.Any(static constructor => constructor.Parameters.Length == 0)
            && type.GetAttributes().Any(value => SymbolEqualityComparer.Default.Equals(value.AttributeClass, attribute))
            && type.AllInterfaces.Any(value => SymbolEqualityComparer.Default.Equals(value, contract));

    private static string ValidateCandidateSource(
          INamedTypeSymbol candidate
        , INamedTypeSymbol attribute
        , string projectRoot
        , out string candidatePath
    )
    {
        var marker = candidate
            .GetAttributes()
            .First(value => SymbolEqualityComparer.Default.Equals(value.AttributeClass, attribute));

        candidatePath = marker.ConstructorArguments.Length == 1
            ? marker.ConstructorArguments[0].Value as string ?? string.Empty
            : string.Empty;

        if (string.IsNullOrWhiteSpace(candidatePath))
        {
            return "The generator attribute did not retain a caller source path.";
        }

        candidatePath = Path.GetFullPath(candidatePath);

        if (IsProjectSource(projectRoot, candidatePath) == false)
        {
            return $"The generator source is outside project Assets/Packages: {candidatePath}";
        }

        var declaredPaths = candidate.DeclaringSyntaxReferences
            .Select(static reference => Path.GetFullPath(reference.SyntaxTree.FilePath));

        return declaredPaths.Contains(candidatePath, s_pathComparer)
            ? null
            : $"The attribute source is not a declaration file: {candidatePath}";
    }

    private static bool IsProjectSource(string projectRoot, string path)
    {
        var relative = Path.GetRelativePath(projectRoot, path);

        if (
            Path.IsPathRooted(relative)
            || relative.Equals("..", StringComparison.Ordinal)
            || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
        )
        {
            return false;
        }

        var topLevel = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
        return topLevel is "Assets" or "Packages";
    }

    private static string[] RewriteSources(
          IEnumerable<INamedTypeSymbol> candidates
        , CSharpCompilation compilation
        , string sourceRoot
        , string projectRoot
    )
    {
        var candidateArray = candidates.ToArray();
        var trees = candidateArray
            .SelectMany(static type => type.DeclaringSyntaxReferences)
            .Select(static reference => reference.SyntaxTree)
            .Distinct()
            .OrderBy(static tree => tree.FilePath, StringComparer.Ordinal)
            .ToArray();
        var copiedTrees = new HashSet<SyntaxTree>(trees);
        var paths = new List<string>(trees.Length);

        for (var i = 0; i < trees.Length; i++)
        {
            var root = trees[i].GetRoot();
            var semanticModel = compilation.GetSemanticModel(trees[i]);
            var rewritten = (CompilationUnitSyntax)new NamespaceRewriter(semanticModel, copiedTrees).Visit(root)!;
            var originalNamespaces = candidateArray
                .Where(
                      type =>
                      type.DeclaringSyntaxReferences.Any(reference => ReferenceEquals(reference.SyntaxTree, trees[i]))
                )
                .Select(static type => type.ContainingNamespace)
                .Where(static value => value.IsGlobalNamespace == false)
                .Select(static value => value.ToDisplayString())
                .Distinct(StringComparer.Ordinal)
                .Where(
                      value => rewritten.Usings.Any(
                            usingDirective =>
                            string.Equals(usingDirective.Name?.ToString(), value, StringComparison.Ordinal)
                      ) == false
                )
                .Select(
                      static value => SyntaxFactory.ParseCompilationUnit(
                            "using " + value + ";" + Environment.NewLine
                      ).Usings[0]
                )
                .ToArray();
            rewritten = rewritten.AddUsings(originalNamespaces);
            var diagnostics = rewritten
                .SyntaxTree
                .GetDiagnostics()
                .Where(static value => value.Severity == DiagnosticSeverity.Error)
                .ToArray();

            ThrowHelper.ThrowIfNamespaceRewriteInvalid(diagnostics.Length != 0, trees[i].FilePath);

            var relative = Path
                .GetRelativePath(projectRoot, trees[i].FilePath)
                .Replace(':', '_');
            var destination = Path.Combine(sourceRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.WriteAllText(destination, rewritten.ToFullString(), new UTF8Encoding(false));
            paths.Add(destination);
        }

        return paths.ToArray();
    }

    private static void WriteIslandProject(
          string path
        , string templatePath
        , string template
        , string id
        , string projectRoot
        , string unityVersionRoot
        , string sourceRoot
        , JsonElement assembly
        , string[] sources
    )
    {
        var options = assembly.GetProperty("compilerOptions");
        var languageVersion = RequireString(options, "languageVersion");
        var nullable = RequireString(options, "nullable");
        var allowUnsafe = options.GetProperty("allowUnsafeCode").GetBoolean();
        var defines = string.Join(
              ";"
            , assembly
                .GetProperty("defines")
                .EnumerateArray()
                .Select(static value => value.GetString()!)
        );

        var responseArguments = string.Join(
              " "
            , options
                .GetProperty("responseArguments")
                .EnumerateArray()
                .Select(static value => value.GetString())
                .Where(static value => string.IsNullOrWhiteSpace(value) == false)
        );

        var additionalOptions = string.IsNullOrWhiteSpace(responseArguments)
            ? string.Empty
            : $"    <AdditionalOptions>$(AdditionalOptions) {Xml(responseArguments)}</AdditionalOptions>";
        var ruleset = RequireStringOrEmpty(options, "analyzerRulesetPath");
        var codeAnalysisRuleSet = string.IsNullOrWhiteSpace(ruleset)
            ? string.Empty
            : $"    <CodeAnalysisRuleSet>{Xml(ruleset)}</CodeAnalysisRuleSet>";
        var sourceItems = new List<string>();

        var sourceCount = sources.Length;

        for (var sourceIndex = 0; sourceIndex < sourceCount; sourceIndex++)
        {
            sourceItems.Add($"    <Compile Include=\"{Xml(sources[sourceIndex])}\" />");
        }

        var referenceItems = new List<string>();

        foreach (var reference in assembly.GetProperty("references").EnumerateArray())
        {
            var referencePath = RequireString(reference, "path");

            if (IsUnityFrameworkReference(unityVersionRoot, referencePath))
            {
                continue;
            }

            var hintPath = GetReferenceHintPath(reference, unityVersionRoot);

            if (string.IsNullOrWhiteSpace(hintPath))
            {
                continue;
            }

            var name = Path.GetFileNameWithoutExtension(referencePath);
            referenceItems.Add($"    <Reference Include=\"{Xml(name)}\" Private=\"false\">");
            referenceItems.Add($"      <HintPath>{Xml(hintPath)}</HintPath>");
            referenceItems.Add("    </Reference>");
        }

        var ownerOutput = RequireString(assembly, "outputPath");
        var ownerName = Path.GetFileNameWithoutExtension(ownerOutput);
        referenceItems.Add($"    <Reference Include=\"{Xml(ownerName)}\" Private=\"false\">");
        referenceItems.Add($"      <HintPath>{Xml(ownerOutput)}</HintPath>");
        referenceItems.Add("    </Reference>");
        var analyzerItems = assembly
            .GetProperty("analyzers")
            .EnumerateArray()
            .Select(static analyzer => $"    <Analyzer Include=\"{Xml(analyzer.GetString()!)}\" />")
            .ToArray();

        var additionalFileItems = assembly
            .GetProperty("additionalFiles")
            .EnumerateArray()
            .Select(static additional => $"    <AdditionalFiles Include=\"{Xml(additional.GetString()!)}\" />")
            .ToArray();
        var analyzerConfig = RequireStringOrEmpty(options, "analyzerConfigPath");
        var globalAnalyzerConfig = string.IsNullOrWhiteSpace(analyzerConfig)
            ? string.Empty
            : $"    <GlobalAnalyzerConfigFiles Include=\"{Xml(analyzerConfig)}\" />";

        var replacements = new[]
        {
            new KeyValuePair<string, string>("{{ENCOSY_LANGUAGE_VERSION}}", Xml(languageVersion)),
            new KeyValuePair<string, string>("{{ENCOSY_NULLABLE_MODE}}", Xml(nullable)),
            new KeyValuePair<string, string>(
                  "{{ENCOSY_ALLOW_UNSAFE_BLOCKS}}"
                , allowUnsafe.ToString().ToLowerInvariant()
            ),
            new KeyValuePair<string, string>("{{ENCOSY_DEFINE_CONSTANTS}}", Xml(defines)),
            new KeyValuePair<string, string>("{{ENCOSY_PATH_MAP}}", Xml(sourceRoot) + "=" + Xml(projectRoot)),
            new KeyValuePair<string, string>("{{ENCOSY_ADDITIONAL_OPTIONS_PROPERTY}}", additionalOptions),
            new KeyValuePair<string, string>("{{ENCOSY_CODE_ANALYSIS_RULE_SET_PROPERTY}}", codeAnalysisRuleSet),
            new KeyValuePair<string, string>("{{ENCOSY_PROJECT_ROOT}}", Xml(projectRoot)),
            new KeyValuePair<string, string>("{{ENCOSY_UNITY_VERSION_ROOT}}", Xml(unityVersionRoot)),
            new KeyValuePair<string, string>("{{ENCOSY_ISLAND_ID}}", Xml(id)),
            new KeyValuePair<string, string>("{{ENCOSY_SOURCE_ITEMS}}", string.Join(Environment.NewLine, sourceItems)),
            new KeyValuePair<string, string>(
                  "{{ENCOSY_REFERENCE_ITEMS}}"
                , string.Join(Environment.NewLine, referenceItems)
            ),
            new KeyValuePair<string, string>(
                  "{{ENCOSY_ANALYZER_ITEMS}}"
                , string.Join(Environment.NewLine, analyzerItems)
            ),
            new KeyValuePair<string, string>(
                  "{{ENCOSY_ADDITIONAL_FILE_ITEMS}}"
                , string.Join(Environment.NewLine, additionalFileItems)
            ),
            new KeyValuePair<string, string>("{{ENCOSY_GLOBAL_ANALYZER_CONFIG_ITEM}}", globalAnalyzerConfig),
        };
        var content = ComposeTemplate(templatePath, template, replacements, s_runnerOwnedTokens);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }

    private static string GetReferenceHintPath(JsonElement reference, string unityVersionRoot)
    {
        var path = RequireString(reference, "path");
        var kind = RequireString(reference, "kind");

        return kind switch {
            "unityEditor" => "$(UnityEditorPath)",
            "unityModule" => "$(UnityModulesPath)/" + Path.GetFileName(path),
            "unityManaged" => "$(UnityManagedPath)/" + Path.GetRelativePath(
                  Path.Combine(unityVersionRoot, "Editor", "Data", "Managed")
                , path
            ),
            "project" or "scriptAssembly" => "$(UnityScriptAssembliesPath)/" +
            Path.GetFileName(path),
            "precompiled" => path,
            "framework" => string.Empty,
            _ => ThrowHelper.ThrowUnknownReferenceKind(kind),
        };
    }

    private static void WriteWorkerManifest(
          string path
        , string workspaceRoot
        , string projectRoot
        , string unityVersionRoot
        , string id
        , string targetPath
        , string resultPath
        , JsonElement assembly
        , IEnumerable<INamedTypeSymbol> generators
    )
    {
        using var json = new AtomicJsonFile(path);
        var writer = json.Writer;
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", SCHEMA_VERSION);
        writer.WriteString("workspaceRoot", workspaceRoot);
        writer.WriteString("projectRoot", projectRoot);
        writer.WriteString("islandId", id);
        writer.WriteString("islandAssemblyPath", targetPath);
        writer.WriteString("resultPath", resultPath);
        writer.WritePropertyName("references");
        writer.WriteStartArray();

        foreach (var reference in assembly.GetProperty("references").EnumerateArray())
        {
            var referencePath = RequireString(reference, "path");

            if (IsUnityFrameworkReference(unityVersionRoot, referencePath))
            {
                continue;
            }

            reference.WriteTo(writer);
        }

        writer.WriteStartObject();
        writer.WriteString("path", RequireString(assembly, "outputPath"));
        writer.WriteString("kind", "owner");
        writer.WriteEndObject();
        writer.WriteEndArray();
        writer.WritePropertyName("generatorTypes");
        writer.WriteStartArray();

        foreach (var generator in generators.OrderBy(static value => value.ToDisplayString(), StringComparer.Ordinal))
        {
            writer.WriteStringValue(GetGeneratedMetadataName(generator));
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        json.Complete();
    }

    private static void WriteSkipped(string path, IEnumerable<Skip> values)
    {
        using var json = new AtomicJsonFile(path);
        var writer = json.Writer;
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", SCHEMA_VERSION);
        writer.WritePropertyName("skippedGenerators");
        writer.WriteStartArray();

        foreach (
            var value in values
                .OrderBy(static value => value.FilePath, StringComparer.Ordinal)
                .ThenBy(static value => value.GeneratorType, StringComparer.Ordinal)
        )
        {
            writer.WriteStartObject();
            writer.WriteString("generatorType", value.GeneratorType);
            writer.WriteString("filePath", value.FilePath);
            writer.WriteString("diagnosticId", value.DiagnosticId);
            writer.WriteString("message", value.Message);
            writer.WriteNumber("line", value.Line);
            writer.WriteNumber("column", value.Column);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        json.Complete();
    }

    private static void WriteIslands(
          string path
        , string workspaceRoot
        , string projectRoot
        , string dotnetPath
        , IEnumerable<Island> islands
    )
    {
        using var json = new AtomicJsonFile(path);
        var writer = json.Writer;
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", SCHEMA_VERSION);
        writer.WriteString("workspaceRoot", workspaceRoot);
        writer.WriteString("projectRoot", projectRoot);
        writer.WriteString("dotnetExecutablePath", dotnetPath);
        writer.WriteString("resultManifestPath", Path.Combine(workspaceRoot, "results", "aggregate-result.json"));
        writer.WritePropertyName("islands");
        writer.WriteStartArray();

        foreach (var island in islands)
        {
            writer.WriteStartObject();
            writer.WriteString("islandId", island.Id);
            writer.WriteString("projectPath", island.ProjectPath);
            writer.WriteString("targetPath", island.TargetPath);
            writer.WriteString("workerManifestPath", island.WorkerManifestPath);
            writer.WriteString("resultPath", island.ResultPath);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        json.Complete();
    }

    private static void WriteGeneratedProps(
          string path
        , string templatePath
        , string template
        , IEnumerable<Island> islands
    )
    {
        var entries = new List<string>();

        foreach (var island in islands)
        {
            entries.Add(
                  $"    <ProjectReference Include=\"{Xml(island.ProjectPath)}\" " +
                  "BuildReference=\"true\" ReferenceOutputAssembly=\"false\" Private=\"false\" />"
            );
        }

        var replacements = new[]
        {
            new KeyValuePair<string, string>(
                  "{{ENCOSY_ISLAND_PROJECT_REFERENCES}}"
                , string.Join(Environment.NewLine, entries)
            ),
        };
        var content = ComposeTemplate(templatePath, template, replacements, s_runnerOwnedTokens);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }

    private static void WriteSolution(string path, string templatePath, string template, IEnumerable<Island> islands)
    {
        var entries = new List<string>
        {
            "  <Project Path=\"Runner/EncosyCodeGenRunner.csproj\" />",
        };

        foreach (var island in islands)
        {
            var root = Path.GetDirectoryName(path)!;
            var relative = Path
                .GetRelativePath(root, island.ProjectPath)
                .Replace('\\', '/');
            entries.Add($"  <Project Path=\"{Xml(relative)}\" />");
        }

        var replacements = new[]
        {
            new KeyValuePair<string, string>(
                  "{{ENCOSY_ISLAND_SOLUTION_ENTRIES}}"
                , string.Join(Environment.NewLine, entries)
            ),
        };
        var content = ComposeTemplate(templatePath, template, replacements, s_runnerOwnedTokens);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }

    private static string ComposeTemplate(
          string templatePath
        , string content
        , IReadOnlyList<KeyValuePair<string, string>> replacements
        , IReadOnlyCollection<string> ownedTokens
    )
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        var replacementCount = replacements.Count;

        for (var replacementIndex = 0; replacementIndex < replacementCount; replacementIndex++)
        {
            var replacement = replacements[replacementIndex];

            ThrowHelper.ThrowIfDuplicateTemplateToken(
                keys.Add(replacement.Key) == false,
                templatePath,
                replacement.Key
            );
            ThrowHelper.ThrowIfMissingTemplateToken(
                  content.Contains(replacement.Key, StringComparison.Ordinal) == false
                , templatePath
                , replacement.Key
            );

            content = content.Replace(replacement.Key, replacement.Value, StringComparison.Ordinal);
        }

        foreach (var token in ownedTokens)
        {
            ThrowHelper.ThrowIfUnresolvedTemplateToken(
                content.Contains(token, StringComparison.Ordinal),
                templatePath,
                token
            );
        }

        var unresolvedIndex = content.IndexOf("{{ENCOSY_", StringComparison.Ordinal);

        if (unresolvedIndex >= 0)
        {
            var unresolvedEnd = content.IndexOf("}}", unresolvedIndex, StringComparison.Ordinal);
            var unresolvedLength = unresolvedEnd < 0
                ? content.Length - unresolvedIndex
                : unresolvedEnd + 2 - unresolvedIndex;
            var unresolvedToken = content.Substring(unresolvedIndex, unresolvedLength);
            ThrowHelper.ThrowIfUnresolvedTemplateToken(true, templatePath, unresolvedToken);
        }

        return content;
    }

    private static string ReadTemplate(string path)
    {
        try
        {
            ThrowHelper.ThrowIfTemplateFileMissing(File.Exists(path) == false, path);

            return File.ReadAllText(path, Encoding.UTF8);
        }
        catch (Exception exception)
        {
            return ThrowHelper.ThrowCannotReadTemplate(path, exception);
        }
    }

    private static void WriteWorkerResult(string path, string islandId, IEnumerable<Generated> generated)
    {
        using var json = new AtomicJsonFile(path);
        var writer = json.Writer;
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", SCHEMA_VERSION);
        writer.WriteString("islandId", islandId);
        WriteGenerated(writer, generated);
        writer.WritePropertyName("diagnostics");
        writer.WriteStartArray();
        writer.WriteEndArray();
        writer.WriteBoolean("allCandidatesSkipped", false);
        writer.WriteEndObject();
        json.Complete();
    }

    private static void WriteAggregate(
          string path
        , string projectRoot
        , IEnumerable<Generated> generated
        , IEnumerable<CodeDiagnostic> diagnostics
        , bool allCandidatesSkipped
    )
    {
        using var json = new AtomicJsonFile(path);
        var writer = json.Writer;
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", SCHEMA_VERSION);
        writer.WriteString("projectRoot", projectRoot);
        WriteGenerated(writer, generated);
        writer.WritePropertyName("diagnostics");
        writer.WriteStartArray();

        foreach (var diagnostic in diagnostics)
        {
            writer.WriteStartObject();
            writer.WriteNumber("severity", diagnostic.Severity);
            writer.WriteString("code", diagnostic.Code);
            writer.WriteString("message", diagnostic.Message);
            writer.WriteString("filePath", diagnostic.FilePath);
            writer.WriteNumber("line", diagnostic.Line);
            writer.WriteNumber("column", diagnostic.Column);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteBoolean("allCandidatesSkipped", allCandidatesSkipped);
        writer.WriteEndObject();
        json.Complete();
    }

    private static void WriteGenerated(Utf8JsonWriter writer, IEnumerable<Generated> generated)
    {
        writer.WritePropertyName("generatedCodes");
        writer.WriteStartArray();

        foreach (var value in generated.OrderBy(static value => value.FilePath, StringComparer.Ordinal))
        {
            writer.WriteStartObject();
            writer.WriteString("filePath", value.FilePath);
            writer.WriteString("content", value.Content);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void ReadWorkerResult(
          string path
        , ICollection<Generated> generated
        , ICollection<CodeDiagnostic> diagnostics
    )
    {
        using var document = ReadDocument(path, "worker-result");
        RequireSchema(document.RootElement, "worker-result");

        foreach (var value in document.RootElement.GetProperty("generatedCodes").EnumerateArray())
        {
            var content = value.GetProperty("content").GetString();
            ThrowHelper.ThrowIfGeneratedContentNull(content is null);
            generated.Add(new Generated(RequireString(value, "filePath"), content!));
        }

        foreach (var value in document.RootElement.GetProperty("diagnostics").EnumerateArray())
        {
            diagnostics.Add(
                  new CodeDiagnostic(
                        value.GetProperty("severity").GetInt32()
                      , RequireString(value, "code")
                      , RequireString(value, "message")
                      , RequireString(value, "filePath")
                      , value.GetProperty("line").GetInt32()
                      , value.GetProperty("column").GetInt32()
                  )
            );
        }
    }

    private static JsonDocument ReadDocument(string path, string name)
    {
        var document = JsonDocument.Parse(File.ReadAllBytes(Path.GetFullPath(path)));
        RequireSchema(document.RootElement, name);
        return document;
    }

    private static void RequireSchema(JsonElement root, string name)
    {
        var version = root.GetProperty("schemaVersion").GetInt32();

        ThrowHelper.ThrowIfUnsupportedSchema(version != SCHEMA_VERSION, name, version, SCHEMA_VERSION);
    }

    private static Dictionary<string, string> ParseArguments(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var i = 0; i < args.Length; i += 2)
        {
            ThrowHelper.ThrowIfMalformedRunnerArguments(
                  i + 1 >= args.Length || args[i].StartsWith("--", StringComparison.Ordinal) == false
            );
            ThrowHelper.ThrowIfDuplicateRunnerArgument(
                result.TryAdd(args[i], args[i + 1]) == false,
                args[i]
            );
        }

        return result;
    }

    private static string Require(IReadOnlyDictionary<string, string> options, string key)
    {
        var found = options.TryGetValue(key, out var value)
            && string.IsNullOrWhiteSpace(value) == false;
        ThrowHelper.ThrowIfRunnerArgumentMissing(found == false, key);
        return value!;
    }

    private static string RequireString(JsonElement owner, string name)
    {
        var value = owner.GetProperty(name).GetString();
        ThrowHelper.ThrowIfJsonPropertyNull(value is null, name);
        return value!;
    }

    private static string RequireStringOrEmpty(JsonElement owner, string name)
        => owner.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static void Add(ProcessStartInfo startInfo, params string[] arguments)
    {
        var argumentCount = arguments.Length;

        for (var argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
        {
            startInfo.ArgumentList.Add(arguments[argumentIndex]);
        }
    }

    private static string Sanitize(string value)
        => string.Concat(value.Select(static character => char.IsLetterOrDigit(character) ? character : '_'));

    private static string RewriteNamespace(INamespaceSymbol value)
        => RewriteNamespace(value.IsGlobalNamespace ? string.Empty : value.ToDisplayString());

    private static string RewriteNamespace(string value)
    {
        if (string.IsNullOrEmpty(value) || string.Equals(value, SOURCE_ROOT_NAMESPACE, StringComparison.Ordinal))
        {
            return GENERATED_ROOT_NAMESPACE;
        }

        if (value.StartsWith(SOURCE_ROOT_NAMESPACE + ".", StringComparison.Ordinal))
        {
            return GENERATED_ROOT_NAMESPACE + value.Substring(SOURCE_ROOT_NAMESPACE.Length);
        }

        return GENERATED_ROOT_NAMESPACE + "." + value;
    }

    private static string GetGeneratedMetadataName(INamedTypeSymbol type)
        => RewriteNamespace(type.ContainingNamespace)
            + "." + GetNestedMetadataName(type);

    private static string GetNestedMetadataName(INamedTypeSymbol type)
    {
        var names = new Stack<string>();

        for (var current = type; current is not null; current = current.ContainingType)
        {
            names.Push(current.MetadataName);
        }

        return string.Join("+", names);
    }

    private static string Xml(string value)
        => System.Security.SecurityElement.Escape(value)!;

    private sealed class NamespaceRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;
        private readonly HashSet<SyntaxTree> _copiedTrees;

        internal NamespaceRewriter(SemanticModel semanticModel, HashSet<SyntaxTree> copiedTrees)
        {
            _semanticModel = semanticModel;
            _copiedTrees = copiedTrees;
        }

        public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var visited = (NamespaceDeclarationSyntax)base.VisitNamespaceDeclaration(node)!;

            if (node.Parent is NamespaceDeclarationSyntax)
            {
                return visited;
            }

            var symbol = _semanticModel.GetDeclaredSymbol(node);
            return symbol is null
                ? visited
                : visited.WithName(SyntaxFactory.ParseName(RewriteNamespace(symbol)).WithTriviaFrom(visited.Name));
        }

        public override SyntaxNode VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            var visited = (FileScopedNamespaceDeclarationSyntax)base
                .VisitFileScopedNamespaceDeclaration(node)!;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            return symbol is null
                ? visited
                : visited.WithName(SyntaxFactory.ParseName(RewriteNamespace(symbol)).WithTriviaFrom(visited.Name));
        }

        public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
        {
            var symbol = _semanticModel.GetSymbolInfo(node).Symbol;

            if (symbol is IAliasSymbol alias)
            {
                symbol = alias.Target;
            }

            if (
                symbol is null
                || HasCopiedDeclaration(symbol) == false
                || symbol.ContainingNamespace is null
                || symbol.ContainingNamespace.IsGlobalNamespace
            )
            {
                return base.VisitQualifiedName(node)!;
            }

            var namespaceText = symbol.ContainingNamespace.ToDisplayString();
            var nodeText = node.ToString();
            var prefix = namespaceText + ".";

            if (nodeText.StartsWith(prefix, StringComparison.Ordinal) == false)
            {
                return base.VisitQualifiedName(node)!;
            }

            var rewritten = SyntaxFactory.ParseName(
                  RewriteNamespace(namespaceText)
                  + nodeText.Substring(namespaceText.Length)
            );
            return rewritten.WithTriviaFrom(node);
        }

        private bool HasCopiedDeclaration(ISymbol symbol)
            => symbol.Locations.Any(
                  location =>
                  location.IsInSource
                  && location.SourceTree is { } tree
                  && _copiedTrees.Contains(tree)
            );
    }

    private sealed class AtomicJsonFile : IDisposable
    {
        private readonly string _path;
        private readonly string _temporaryPath;
        private readonly FileStream _stream;
        private bool _completed;
        private bool _disposed;

        internal AtomicJsonFile(string path)
        {
            _path = Path.GetFullPath(path);
            _temporaryPath = _path + ".tmp";

            if (File.Exists(_temporaryPath))
            {
                File.Delete(_temporaryPath);
            }

            _stream = new FileStream(_temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            Writer = new Utf8JsonWriter(_stream, new JsonWriterOptions { Indented = true });
        }

        internal Utf8JsonWriter Writer { get; }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        internal void Complete()
        {
            Writer.Flush();
            _stream.Flush(flushToDisk: true);
            Writer.Dispose();
            _stream.Dispose();

            if (File.Exists(_path))
            {
                File.Replace(_temporaryPath, _path, null);
            }
            else
            {
                File.Move(_temporaryPath, _path);
            }

            _completed = true;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Writer.Dispose();
                _stream.Dispose();

                if (_completed == false && File.Exists(_temporaryPath))
                {
                    File.Delete(_temporaryPath);
                }
            }

            _disposed = true;
        }
    }

    private sealed class Island
    {
        internal Island(string id, string projectPath, string targetPath, string workerManifestPath, string resultPath)
        {
            Id = id;
            ProjectPath = projectPath;
            TargetPath = targetPath;
            WorkerManifestPath = workerManifestPath;
            ResultPath = resultPath;
        }

        internal string Id { get; }

        internal string ProjectPath { get; }

        internal string TargetPath { get; }

        internal string WorkerManifestPath { get; }

        internal string ResultPath { get; }
    }

    private sealed class Skip
    {
        internal Skip(string generatorType, string filePath, string diagnosticId, string message, int line, int column)
        {
            GeneratorType = generatorType;
            FilePath = filePath;
            DiagnosticId = diagnosticId;
            Message = message;
            Line = line;
            Column = column;
        }

        internal string GeneratorType { get; }

        internal string FilePath { get; }

        internal string DiagnosticId { get; }

        internal string Message { get; }

        internal int Line { get; }

        internal int Column { get; }
    }

    private sealed class Generated
    {
        internal Generated(string filePath, string content)
        {
            FilePath = filePath;
            Content = content;
        }

        internal string FilePath { get; }

        internal string Content { get; }
    }

    private sealed class CodeDiagnostic
    {
        internal CodeDiagnostic(int severity, string code, string message, string filePath, int line, int column)
        {
            Severity = severity;
            Code = code;
            Message = message;
            FilePath = filePath;
            Line = line;
            Column = column;
        }

        internal int Severity { get; }

        internal string Code { get; }

        internal string Message { get; }

        internal string FilePath { get; }

        internal int Line { get; }

        internal int Column { get; }
    }
}
