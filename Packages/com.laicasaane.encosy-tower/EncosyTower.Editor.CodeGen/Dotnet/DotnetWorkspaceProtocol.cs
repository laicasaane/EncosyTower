#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EncosyTower.CodeGen;
using UnityEngine;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Defines and validates the JSON contracts exchanged with the temporary .NET Runner.
    /// </summary>
    internal static class DotnetWorkspaceProtocol
    {
        /// <summary>
        /// Defines the supported workspace-manifest schema version.
        /// </summary>
        internal const int SCHEMA_VERSION = 1;

        /// <summary>
        /// Defines the assembly name shared by generated owner-island projects.
        /// </summary>
        internal const string GENERATED_ASSEMBLY_NAME = "EncosyCodeGenIsland";

        /// <summary>
        /// Defines the assembly name of the generated orchestration Runner.
        /// </summary>
        internal const string RUNNER_ASSEMBLY_NAME = "EncosyCodeGenRunner";
        private static readonly StringComparer s_pathComparer = Path.DirectorySeparatorChar == '\\'
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        internal static bool IsPredefinedAssemblyName(string assemblyName)
            => assemblyName switch {
                "Assembly-CSharp-firstpass" => true,
                "Assembly-CSharp-Editor-firstpass" => true,
                "Assembly-CSharp" => true,
                "Assembly-CSharp-Editor" => true,
                _ => false,
            };

        /// <summary>
        /// Validates and writes the Unity compiler-input manifest.
        /// </summary>
        internal static void WriteInput(string path, InputManifest manifest)
        {
            ValidateInput(manifest);
            WriteJsonAtomic(path, manifest);
        }

        /// <summary>
        /// Serializes a manifest as UTF-8 JSON and atomically replaces its destination.
        /// </summary>
        /// <typeparam name="T">The serialized manifest type.</typeparam>
        internal static void WriteJsonAtomic<T>(string path, T value)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw ThrowHelper.CreateManifestPathRequiredException();
            }

            if (value is null)
            {
                throw ThrowHelper.CreateManifestValueRequiredException();
            }

            var fullPath = Path.GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath);

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw ThrowHelper.CreateManifestParentMissingException(fullPath);
            }

            Directory.CreateDirectory(directory);
            var temporaryPath = fullPath + ".tmp";
            var bytes = new UTF8Encoding(false).GetBytes(JsonUtility.ToJson(value, true) + "\n");
            File.WriteAllBytes(temporaryPath, bytes);

            if (File.Exists(fullPath))
            {
                var backupPath = fullPath + ".bak";

                try
                {
                    File.Replace(temporaryPath, fullPath, backupPath);
                }
                finally
                {
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }
                }
            }
            else
            {
                File.Move(temporaryPath, fullPath);
            }
        }

        /// <summary>
        /// Reads, validates, and converts the aggregate .NET result into a generated-code batch.
        /// </summary>
        internal static GeneratedCodeBatch ReadResult(string path, string projectRoot)
        {
            if (File.Exists(path) == false)
            {
                throw ThrowHelper.CreateResultManifestMissingException(path);
            }

            AggregateResultManifest manifest;

            try
            {
                manifest = JsonUtility.FromJson<AggregateResultManifest>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch (Exception exception) when (
                exception is ArgumentException
                || exception is IOException
                || exception is UnauthorizedAccessException
            )
            {
                throw ThrowHelper.CreateResultManifestMalformedException(path, exception);
            }

            ValidateResult(manifest, projectRoot);
            var generatedCodes = new GeneratedCode[manifest.generatedCodes.Length];
            var destinations = new HashSet<string>(s_pathComparer);

            for (var i = 0; i < manifest.generatedCodes.Length; i++)
            {
                var item = manifest.generatedCodes[i];
                var canonicalPath = RequireContainedPath(projectRoot, item.filePath, allowMissing: true);

                if (destinations.Add(canonicalPath) == false)
                {
                    throw ThrowHelper.CreateDuplicateResultPathException(canonicalPath);
                }

                generatedCodes[i] = new GeneratedCode {
                    filePath = canonicalPath,
                    content = item.content,
                };
            }

            var diagnostics = new CodeGenDiagnostic[manifest.diagnostics.Length];

            for (var i = 0; i < manifest.diagnostics.Length; i++)
            {
                var item = manifest.diagnostics[i];
                var severity = item.severity switch {
                    0 => CodeGenDiagnosticSeverity.Info,
                    1 => CodeGenDiagnosticSeverity.Warning,
                    2 => CodeGenDiagnosticSeverity.Error,
                    _ => throw ThrowHelper.CreateInvalidDiagnosticSeverityException(item.severity),
                };
                diagnostics[i] = new CodeGenDiagnostic(
                      severity
                    , item.code
                    , item.message
                    , item.filePath
                    , item.line
                    , item.column
                );
            }

            return new GeneratedCodeBatch(generatedCodes, diagnostics, manifest.allCandidatesSkipped);
        }

        /// <summary>
        /// Returns a canonical project path after enforcing Assets or Packages containment.
        /// </summary>
        internal static string RequireContainedPath(string projectRoot, string path, bool allowMissing)
        {
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                throw ThrowHelper.CreateProjectRootRequiredException();
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw ThrowHelper.CreateProjectPathRequiredException();
            }

            var canonicalRoot = Path.GetFullPath(projectRoot);
            var canonicalPath = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(canonicalRoot, path));

            if (IsContained(canonicalRoot, canonicalPath) == false)
            {
                throw ThrowHelper.CreateProjectPathOutsideRootException(canonicalPath);
            }

            var relativePath = canonicalPath[canonicalRoot.Length..]
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var separator = relativePath.IndexOfAny(
                new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }
            );
            var topLevel = separator < 0 ? relativePath : relativePath[..separator];

            if (string.Equals(topLevel, "Assets", StringComparison.Ordinal) == false
                && string.Equals(topLevel, "Packages", StringComparison.Ordinal) == false
            )
            {
                throw ThrowHelper.CreateProjectPathOutsideOwnedDirectoriesException(canonicalPath);
            }

            if (allowMissing == false && File.Exists(canonicalPath) == false)
            {
                throw ThrowHelper.CreateRequiredFileMissingException(canonicalPath);
            }

            RejectReparseEscape(canonicalRoot, canonicalPath);
            return canonicalPath;
        }

        /// <summary>
        /// Determines whether a canonical path is the supplied root or one of its descendants.
        /// </summary>
        internal static bool IsContained(string root, string path)
        {
            var canonicalRoot = Path.GetFullPath(root)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var canonicalPath = Path.GetFullPath(path);
            var prefix = canonicalRoot + Path.DirectorySeparatorChar;
            return string.Equals(canonicalRoot, canonicalPath, PathComparison)
                || canonicalPath.StartsWith(prefix, PathComparison);
        }

        /// <summary>
        /// Validates a complete Unity compiler-input manifest and its referenced paths.
        /// </summary>
        internal static void ValidateInput(InputManifest manifest)
        {
            if (manifest is null)
            {
                throw ThrowHelper.CreateInputManifestRequiredException();
            }

            RequireSchema(manifest.schemaVersion, "compiler-input");
            RequireText(manifest.projectRoot, "projectRoot");
            RequireText(manifest.unityVersion, "unityVersion");
            RequireText(manifest.unityVersionRoot, "unityVersionRoot");
            RequireText(manifest.dotnetExecutablePath, "dotnetExecutablePath");
            RequireText(manifest.selectedSdkVersion, "selectedSdkVersion");
            RequireArray(manifest.assemblies, "assemblies");

            if (Directory.Exists(manifest.projectRoot) == false
                || Directory.Exists(manifest.unityVersionRoot) == false
                || File.Exists(manifest.dotnetExecutablePath) == false
            )
            {
                throw ThrowHelper.CreateInputEnvironmentUnavailableException();
            }

            var names = new HashSet<string>(StringComparer.Ordinal);
            var outputs = new HashSet<string>(s_pathComparer);
            var sources = new HashSet<string>(s_pathComparer);

            for (var i = 0; i < manifest.assemblies.Length; i++)
            {
                var assembly = manifest.assemblies[i];

                if (assembly is null)
                {
                    throw ThrowHelper.CreateInputAssemblyNullException(i);
                }

                ValidateAssembly(manifest.projectRoot, assembly, names, outputs, sources);
            }
        }

        private static void ValidateAssembly(
              string projectRoot
            , AssemblyInput assembly
            , ISet<string> names
            , ISet<string> outputs
            , ISet<string> sources
        )
        {
            RequireText(assembly.name, "assembly.name");

            var isPredefinedAssembly = IsPredefinedAssemblyName(assembly.name);

            if (isPredefinedAssembly
                && string.Equals(assembly.asmdefPath, string.Empty, StringComparison.Ordinal) == false
            )
            {
                throw ThrowHelper.CreatePredefinedAssemblyHasAsmdefException(assembly.name);
            }
            else if (isPredefinedAssembly == false)
            {
                RequireText(assembly.asmdefPath, "assembly.asmdefPath");
            }

            RequireText(assembly.outputPath, "assembly.outputPath");
            RequireArray(assembly.sourceFiles, "assembly.sourceFiles");
            RequireArray(assembly.defines, "assembly.defines");
            RequireArray(assembly.references, "assembly.references");
            RequireArray(assembly.analyzers, "assembly.analyzers");
            RequireArray(assembly.additionalFiles, "assembly.additionalFiles");

            if (assembly.compilerOptions is null)
            {
                throw ThrowHelper.CreateAssemblyCompilerOptionsMissingException(assembly.name);
            }

            RequireArray(assembly.compilerOptions.responseArguments, "assembly.compilerOptions.responseArguments");
            RequireArray(assembly.compilerOptions.responseFiles, "assembly.compilerOptions.responseFiles");
            RequireText(assembly.compilerOptions.languageVersion, "assembly.compilerOptions.languageVersion");
            RequireText(assembly.compilerOptions.nullable, "assembly.compilerOptions.nullable");

            if (names.Add(assembly.name) == false)
            {
                throw ThrowHelper.CreateDuplicateInputAssemblyNameException(assembly.name);
            }

            var outputPath = Path.GetFullPath(assembly.outputPath);

            if (File.Exists(outputPath) == false)
            {
                throw ThrowHelper.CreateLastGoodAssemblyMissingException(outputPath);
            }

            if (outputs.Add(outputPath) == false)
            {
                throw ThrowHelper.CreateDuplicateInputAssemblyOutputException(outputPath);
            }

            for (var i = 0; i < assembly.sourceFiles.Length; i++)
            {
                var sourcePath = RequireContainedPath(projectRoot, assembly.sourceFiles[i], allowMissing: false);

                if (isPredefinedAssembly
                    && IsContained(Path.Combine(projectRoot, "Assets"), sourcePath) == false
                )
                {
                    throw ThrowHelper.CreatePredefinedAssemblySourceOutsideAssetsException(
                        assembly.name,
                        sourcePath
                    );
                }

                if (sources.Add(sourcePath) == false)
                {
                    throw ThrowHelper.CreateSourceOwnedByMultipleAssembliesException(sourcePath);
                }
            }

            if (isPredefinedAssembly == false)
            {
                RequireContainedPath(projectRoot, assembly.asmdefPath, allowMissing: false);
            }

            ValidateReferences(assembly);
            ValidatePaths(assembly.analyzers, "analyzer");
            ValidatePaths(assembly.additionalFiles, "additional file");
            ValidatePaths(assembly.compilerOptions.responseFiles, "response file");
            ValidateOptionalPath(assembly.compilerOptions.analyzerConfigPath, "analyzer config");
            ValidateOptionalPath(assembly.compilerOptions.analyzerRulesetPath, "analyzer ruleset");
        }

        private static void ValidateReferences(AssemblyInput assembly)
        {
            var paths = new HashSet<string>(s_pathComparer);

            for (var i = 0; i < assembly.references.Length; i++)
            {
                var reference = assembly.references[i];

                if (reference is null
                    || string.IsNullOrWhiteSpace(reference.path)
                    || string.IsNullOrWhiteSpace(reference.kind)
                )
                {
                    throw ThrowHelper.CreateMalformedReferenceException(assembly.name, i);
                }

                var path = Path.GetFullPath(reference.path);

                if (File.Exists(path) == false || paths.Add(path) == false)
                {
                    throw ThrowHelper.CreateMissingOrDuplicateReferenceException(assembly.name, path);
                }
            }
        }

        private static void ValidatePaths(IReadOnlyList<string> values, string kind)
        {
            var paths = new HashSet<string>(s_pathComparer);

            for (var i = 0; i < values.Count; i++)
            {
                var path = string.IsNullOrWhiteSpace(values[i]) ? string.Empty : Path.GetFullPath(values[i]);

                if (string.IsNullOrWhiteSpace(path)
                    || File.Exists(path) == false
                    || paths.Add(path) == false
                )
                {
                    throw ThrowHelper.CreateMissingOrDuplicateInputPathException(kind, values[i]);
                }
            }
        }

        private static void ValidateOptionalPath(string value, string kind)
        {
            if (string.IsNullOrWhiteSpace(value) == false && File.Exists(value) == false)
            {
                throw ThrowHelper.CreateOptionalInputPathMissingException(kind, value);
            }
        }

        private static void ValidateResult(AggregateResultManifest manifest, string projectRoot)
        {
            if (manifest is null)
            {
                throw ThrowHelper.CreateResultManifestEmptyException();
            }

            RequireSchema(manifest.schemaVersion, "aggregate-result");
            RequireText(manifest.projectRoot, "projectRoot");
            RequireArray(manifest.generatedCodes, "generatedCodes");
            RequireArray(manifest.diagnostics, "diagnostics");

            if (string.Equals(
                      Path.GetFullPath(projectRoot)
                    , Path.GetFullPath(manifest.projectRoot)
                    , PathComparison
                ) == false
            )
            {
                throw ThrowHelper.CreateResultProjectRootMismatchException(manifest.projectRoot);
            }

            for (var i = 0; i < manifest.generatedCodes.Length; i++)
            {
                var generatedCode = manifest.generatedCodes[i];

                if (generatedCode is null)
                {
                    throw ThrowHelper.CreateGeneratedResultNullException(i);
                }

                RequireText(generatedCode.filePath, "generatedCode.filePath");

                if (generatedCode.content is null)
                {
                    throw ThrowHelper.CreateGeneratedResultContentNullException(generatedCode.filePath);
                }
            }

            for (var i = 0; i < manifest.diagnostics.Length; i++)
            {
                var diagnostic = manifest.diagnostics[i];

                if (diagnostic is null
                    || string.IsNullOrWhiteSpace(diagnostic.code)
                    || string.IsNullOrWhiteSpace(diagnostic.message)
                    || diagnostic.filePath is null
                    || diagnostic.line < 0
                    || diagnostic.column < 0
                )
                {
                    throw ThrowHelper.CreateDiagnosticMalformedException(i);
                }
            }
        }

        private static void RejectReparseEscape(string root, string path)
        {
            if (File.Exists(path)
                && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0
            )
            {
                throw ThrowHelper.CreateProjectFileReparsePointException(path);
            }

            var current = File.Exists(path) ? Path.GetDirectoryName(path) : path;

            while (string.IsNullOrEmpty(current) == false && IsContained(root, current))
            {
                if (Directory.Exists(current)
                    && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0
                )
                {
                    throw ThrowHelper.CreateProjectPathReparsePointException(current);
                }

                if (string.Equals(Path.GetFullPath(current), Path.GetFullPath(root), PathComparison))
                {
                    break;
                }

                current = Path.GetDirectoryName(current);
            }
        }

        private static void RequireSchema(int value, string manifestName)
        {
            if (value != SCHEMA_VERSION)
            {
                throw ThrowHelper.CreateUnsupportedManifestSchemaException(value, manifestName);
            }
        }

        private static StringComparison PathComparison => Path.DirectorySeparatorChar == '\\'
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        private static void RequireText(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw ThrowHelper.CreateManifestFieldRequiredException(fieldName);
            }
        }

        private static void RequireArray<T>(T[] value, string fieldName)
        {
            if (value is null)
            {
                throw ThrowHelper.CreateManifestArrayRequiredException(fieldName);
            }
        }

        /// <summary>
        /// Captures the Unity compilation environment consumed by the Prepare phase.
        /// </summary>
        [Serializable]
        internal sealed class InputManifest
        {
            /// <summary>
            /// The compiler-input schema version.
            /// </summary>
            public int schemaVersion;

            /// <summary>
            /// The canonical Unity project root.
            /// </summary>
            public string projectRoot = string.Empty;

            /// <summary>
            /// The Unity Editor version that produced the snapshot.
            /// </summary>
            public string unityVersion = string.Empty;

            /// <summary>
            /// The root of the active Unity Editor installation.
            /// </summary>
            public string unityVersionRoot = string.Empty;

            /// <summary>
            /// The resolved dotnet executable path.
            /// </summary>
            public string dotnetExecutablePath = string.Empty;

            /// <summary>
            /// The stable .NET SDK version pinned for the workspace.
            /// </summary>
            public string selectedSdkVersion = string.Empty;

            /// <summary>
            /// The captured Unity Editor compilation assemblies.
            /// </summary>
            public AssemblyInput[] assemblies = Array.Empty<AssemblyInput>();
        }

        /// <summary>
        /// Captures the sources and compiler inputs of one Unity assembly.
        /// </summary>
        [Serializable]
        internal sealed class AssemblyInput
        {
            /// <summary>
            /// The Unity assembly name.
            /// </summary>
            public string name = string.Empty;

            /// <summary>
            /// The canonical path to the owning assembly definition.
            /// </summary>
            public string asmdefPath = string.Empty;

            /// <summary>
            /// The Unity compilation output path.
            /// </summary>
            public string outputPath = string.Empty;

            /// <summary>
            /// The canonical source files compiled into the assembly.
            /// </summary>
            public string[] sourceFiles = Array.Empty<string>();

            /// <summary>
            /// The active Unity scripting defines.
            /// </summary>
            public string[] defines = Array.Empty<string>();

            /// <summary>
            /// The project and metadata references used by the assembly.
            /// </summary>
            public ReferenceInput[] references = Array.Empty<ReferenceInput>();

            /// <summary>
            /// The compiler options required to reproduce the assembly.
            /// </summary>
            public CompilerOptionsInput compilerOptions = new();

            /// <summary>
            /// The Roslyn analyzer paths used by the assembly.
            /// </summary>
            public string[] analyzers = Array.Empty<string>();

            /// <summary>
            /// The Roslyn additional-file paths used by the assembly.
            /// </summary>
            public string[] additionalFiles = Array.Empty<string>();
        }

        /// <summary>
        /// Describes one project or metadata reference and its classification.
        /// </summary>
        [Serializable]
        internal sealed class ReferenceInput
        {
            /// <summary>
            /// The canonical reference path.
            /// </summary>
            public string path = string.Empty;

            /// <summary>
            /// The reference classification used when reproducing Unity inputs.
            /// </summary>
            public string kind = string.Empty;
        }

        /// <summary>
        /// Captures the compiler options needed to reproduce a Unity assembly.
        /// </summary>
        [Serializable]
        internal sealed class CompilerOptionsInput
        {
            /// <summary>
            /// The requested C# language version.
            /// </summary>
            public string languageVersion = string.Empty;

            /// <summary>
            /// The nullable-reference mode parsed from compiler arguments.
            /// </summary>
            public string nullable = string.Empty;

            /// <summary>
            /// Whether unsafe code is enabled.
            /// </summary>
            public bool allowUnsafeCode;

            /// <summary>
            /// The additional compiler arguments supplied by Unity.
            /// </summary>
            public string[] responseArguments = Array.Empty<string>();

            /// <summary>
            /// The response files consumed by Unity compilation.
            /// </summary>
            public string[] responseFiles = Array.Empty<string>();

            /// <summary>
            /// The analyzer configuration path, or an empty string.
            /// </summary>
            public string analyzerConfigPath = string.Empty;

            /// <summary>
            /// The analyzer ruleset path, or an empty string.
            /// </summary>
            public string analyzerRulesetPath = string.Empty;
        }

        /// <summary>
        /// Describes the owner islands emitted by Prepare and consumed by Execute.
        /// </summary>
        [Serializable]
        internal sealed class IslandsManifest
        {
            /// <summary>
            /// The islands-manifest schema version.
            /// </summary>
            public int schemaVersion;

            /// <summary>
            /// The canonical temporary workspace root.
            /// </summary>
            public string workspaceRoot = string.Empty;

            /// <summary>
            /// The aggregate result path written by Execute.
            /// </summary>
            public string resultManifestPath = string.Empty;

            /// <summary>
            /// The deterministic owner-island definitions.
            /// </summary>
            public IslandInput[] islands = Array.Empty<IslandInput>();
        }

        /// <summary>
        /// Defines one isolated owner project and its worker contract.
        /// </summary>
        [Serializable]
        internal sealed class IslandInput
        {
            /// <summary>
            /// The stable identifier assigned to the owner island.
            /// </summary>
            public string islandId = string.Empty;

            /// <summary>
            /// The Unity assembly that owns the copied generator declarations.
            /// </summary>
            public string ownerAssemblyName = string.Empty;

            /// <summary>
            /// The generated owner-island project path.
            /// </summary>
            public string projectPath = string.Empty;

            /// <summary>
            /// The expected owner-island assembly output path.
            /// </summary>
            public string targetPath = string.Empty;

            /// <summary>
            /// The worker input manifest path for this island.
            /// </summary>
            public string workerManifestPath = string.Empty;

            /// <summary>
            /// The source files copied into the owner island.
            /// </summary>
            public string[] sourceFiles = Array.Empty<string>();

            /// <summary>
            /// The owner island's project and metadata references.
            /// </summary>
            public ReferenceInput[] references = Array.Empty<ReferenceInput>();

            /// <summary>
            /// The scripting defines reproduced for the owner island.
            /// </summary>
            public string[] defines = Array.Empty<string>();

            /// <summary>
            /// The compiler options reproduced for the owner island.
            /// </summary>
            public CompilerOptionsInput compilerOptions = new();
        }

        /// <summary>
        /// Carries declarations rejected during Prepare into the aggregate result.
        /// </summary>
        [Serializable]
        internal sealed class SkippedGeneratorsManifest
        {
            /// <summary>
            /// The skipped-generators schema version.
            /// </summary>
            public int schemaVersion;

            /// <summary>
            /// The rejected generator declarations.
            /// </summary>
            public SkippedGenerator[] skippedGenerators = Array.Empty<SkippedGenerator>();
        }

        /// <summary>
        /// Describes one generator declaration rejected before worker execution.
        /// </summary>
        [Serializable]
        internal sealed class SkippedGenerator
        {
            /// <summary>
            /// The declared generator type name.
            /// </summary>
            public string generatorType = string.Empty;

            /// <summary>
            /// The generator declaration path.
            /// </summary>
            public string filePath = string.Empty;

            /// <summary>
            /// The stable diagnostic identifier for the rejection.
            /// </summary>
            public string diagnosticId = string.Empty;

            /// <summary>
            /// The human-readable rejection message.
            /// </summary>
            public string message = string.Empty;

            /// <summary>
            /// The one-based diagnostic line, or zero when unavailable.
            /// </summary>
            public int line;

            /// <summary>
            /// The one-based diagnostic column, or zero when unavailable.
            /// </summary>
            public int column;
        }

        /// <summary>
        /// Defines the inputs for one isolated generator worker process.
        /// </summary>
        [Serializable]
        internal sealed class WorkerManifest
        {
            /// <summary>
            /// The worker-manifest schema version.
            /// </summary>
            public int schemaVersion;

            /// <summary>
            /// The canonical temporary workspace root.
            /// </summary>
            public string workspaceRoot = string.Empty;

            /// <summary>
            /// The canonical Unity project root.
            /// </summary>
            public string projectRoot = string.Empty;

            /// <summary>
            /// The owner-island identifier.
            /// </summary>
            public string islandId = string.Empty;

            /// <summary>
            /// The compiled owner-island assembly path.
            /// </summary>
            public string islandAssemblyPath = string.Empty;

            /// <summary>
            /// The worker's result manifest path.
            /// </summary>
            public string resultPath = string.Empty;

            /// <summary>
            /// The references used when loading the owner island.
            /// </summary>
            public ReferenceInput[] references = Array.Empty<ReferenceInput>();

            /// <summary>
            /// The generator type names assigned to this worker.
            /// </summary>
            public string[] generatorTypes = Array.Empty<string>();
        }

        /// <summary>
        /// Carries the complete set of worker outputs and diagnostics back to Unity.
        /// </summary>
        [Serializable]
        internal sealed class AggregateResultManifest
        {
            /// <summary>
            /// The aggregate-result schema version.
            /// </summary>
            public int schemaVersion;

            /// <summary>
            /// The Unity project root against which result paths are validated.
            /// </summary>
            public string projectRoot = string.Empty;

            /// <summary>
            /// The generated files returned by successful workers.
            /// </summary>
            public GeneratedCodeResult[] generatedCodes = Array.Empty<GeneratedCodeResult>();

            /// <summary>
            /// The diagnostics emitted by Prepare and worker execution.
            /// </summary>
            public DiagnosticResult[] diagnostics = Array.Empty<DiagnosticResult>();

            /// <summary>
            /// Whether every discovered candidate was rejected before execution.
            /// </summary>
            public bool allCandidatesSkipped;
        }

        /// <summary>
        /// Defines one generated file transported from a worker to Unity.
        /// </summary>
        [Serializable]
        internal sealed class GeneratedCodeResult
        {
            /// <summary>
            /// The generated file destination under Assets or Packages.
            /// </summary>
            public string filePath = string.Empty;

            /// <summary>
            /// The complete generated source text.
            /// </summary>
            public string content = string.Empty;
        }

        /// <summary>
        /// Defines one diagnostic transported from the .NET workspace to Unity.
        /// </summary>
        [Serializable]
        internal sealed class DiagnosticResult
        {
            /// <summary>
            /// The numeric diagnostic severity understood by Unity.
            /// </summary>
            public int severity;

            /// <summary>
            /// The stable diagnostic identifier.
            /// </summary>
            public string code = string.Empty;

            /// <summary>
            /// The human-readable diagnostic message.
            /// </summary>
            public string message = string.Empty;

            /// <summary>
            /// The related source path, or an empty string.
            /// </summary>
            public string filePath = string.Empty;

            /// <summary>
            /// The one-based source line, or zero when unavailable.
            /// </summary>
            public int line;

            /// <summary>
            /// The one-based source column, or zero when unavailable.
            /// </summary>
            public int column;
        }
    }
}

#endif
