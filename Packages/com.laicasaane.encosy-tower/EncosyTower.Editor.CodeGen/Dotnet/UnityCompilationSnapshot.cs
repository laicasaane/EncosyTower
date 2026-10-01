#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

using UnityAssembly = UnityEditor.Compilation.Assembly;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Captures Unity's active Editor compilation inputs for the temporary .NET workspace.
    /// </summary>
    internal sealed class UnityCompilationSnapshot
    {
        /// <summary>
        /// Gets the validated compiler-input manifest.
        /// </summary>
        internal DotnetWorkspaceProtocol.InputManifest Manifest { get; }

        /// <summary>
        /// Initializes a snapshot from a validated compiler-input manifest.
        /// </summary>
        internal UnityCompilationSnapshot(DotnetWorkspaceProtocol.InputManifest manifest)
        {
            Manifest = manifest
                ?? throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(manifest));
        }

        /// <summary>
        /// Captures supported Unity Editor assemblies and their effective compiler inputs.
        /// </summary>
        internal static UnityCompilationSnapshot Capture(string selectedSdkVersion, string dotnetExecutablePath)
        {
            if (string.IsNullOrWhiteSpace(selectedSdkVersion))
            {
                throw ThrowHelper.CreateSelectedSdkVersionRequiredException();
            }

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var editorDirectory = Path.GetDirectoryName(EditorApplication.applicationPath);
            var unityVersionRoot = Directory.GetParent(editorDirectory ?? string.Empty)?.FullName;

            if (string.IsNullOrWhiteSpace(unityVersionRoot))
            {
                throw ThrowHelper.CreateUnityVersionRootMissingException(EditorApplication.applicationPath);
            }

            var definitions = LoadAssemblyDefinitions(projectRoot);
            var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
            var inputs = new List<DotnetWorkspaceProtocol.AssemblyInput>(assemblies.Length);

            for (var i = 0; i < assemblies.Length; i++)
            {
                var input = CaptureAssembly(
                    projectRoot
                    , Path.GetFullPath(unityVersionRoot)
                    , assemblies[i]
                    , definitions
                );

                if (input is not null)
                {
                    inputs.Add(input);
                }
            }

            inputs.Sort(static (left, right) =>
                string.CompareOrdinal(left.name, right.name)
            );

            var manifest = new DotnetWorkspaceProtocol.InputManifest {
                schemaVersion = DotnetWorkspaceProtocol.SCHEMA_VERSION,
                projectRoot = projectRoot,
                unityVersion = Application.unityVersion,
                unityVersionRoot = Path.GetFullPath(unityVersionRoot),
                dotnetExecutablePath = Path.GetFullPath(dotnetExecutablePath),
                selectedSdkVersion = selectedSdkVersion,
                assemblies = inputs.ToArray(),
            };

            DotnetWorkspaceProtocol.ValidateInput(manifest);

            return new UnityCompilationSnapshot(manifest);
        }

        private static DotnetWorkspaceProtocol.AssemblyInput CaptureAssembly(
              string projectRoot
            , string unityVersionRoot
            , UnityAssembly assembly
            , IReadOnlyList<AssemblyDefinition> definitions
        )
        {
            var sourceFiles = GetSortedPaths(assembly.sourceFiles);

            if (sourceFiles.Length == 0 || AreSupportedSources(projectRoot, sourceFiles) == false)
            {
                return null;
            }

            var asmdefPath = string.Empty;

            if (DotnetWorkspaceProtocol.IsPredefinedAssemblyName(assembly.name))
            {
                var assetsRoot = Path.Combine(projectRoot, "Assets");

                for (var i = 0; i < sourceFiles.Length; i++)
                {
                    if (DotnetWorkspaceProtocol.IsContained(assetsRoot, sourceFiles[i]) == false)
                    {
                        return null;
                    }
                }
            }
            else
            {
                asmdefPath = ResolveAssemblyDefinition(projectRoot, assembly.name, sourceFiles, definitions);

                if (string.IsNullOrWhiteSpace(asmdefPath))
                {
                    return null;
                }

                if (IsSupportedSourcePath(projectRoot, asmdefPath) == false)
                {
                    return null;
                }
            }

            var references = new Dictionary<string, DotnetWorkspaceProtocol.ReferenceInput>(StringComparer.Ordinal);

            AddReferencePaths(references, assembly.compiledAssemblyReferences, projectRoot, unityVersionRoot);

            if (assembly.assemblyReferences is { Length: > 0 })
            {
                for (var i = 0; i < assembly.assemblyReferences.Length; i++)
                {
                    var reference = assembly.assemblyReferences[i];

                    if (reference is null || string.IsNullOrWhiteSpace(reference.outputPath))
                    {
                        continue;
                    }

                    var referencePath = Path.GetFullPath(reference.outputPath);
                    references[referencePath] = new DotnetWorkspaceProtocol.ReferenceInput {
                        path = referencePath,
                        kind = "project",
                    };
                }
            }

            var options = assembly.compilerOptions;
            var responseArguments = ReadStringArrayProperty(options, "AdditionalCompilerArguments");

            return new DotnetWorkspaceProtocol.AssemblyInput {
                name = assembly.name,
                asmdefPath = asmdefPath,
                outputPath = Path.GetFullPath(assembly.outputPath),
                sourceFiles = sourceFiles,
                defines = SortDistinct(assembly.defines),
                references = references.Values
                    .OrderBy(static value => value.path, StringComparer.Ordinal)
                    .ThenBy(static value => value.kind, StringComparer.Ordinal)
                    .ToArray(),
                compilerOptions = new DotnetWorkspaceProtocol.CompilerOptionsInput {
                    languageVersion = ReadStringProperty(options, "LanguageVersion", "latest"),
                    nullable = ReadNullable(responseArguments),
                    allowUnsafeCode = ReadBooleanProperty(options, "AllowUnsafeCode"),
                    responseArguments = responseArguments,
                    responseFiles = ReadPathArrayProperty(options, "ResponseFiles"),
                    analyzerConfigPath = ReadPathProperty(options, "AnalyzerConfigPath"),
                    analyzerRulesetPath = ReadPathProperty(options, "RoslynAnalyzerRulesetPath"),
                },
                analyzers = ReadPathArrayProperty(options, "RoslynAnalyzerDllPaths"),
                additionalFiles = ReadPathArrayProperty(options, "RoslynAdditionalFilePaths"),
            };
        }

        private static IReadOnlyList<AssemblyDefinition> LoadAssemblyDefinitions(string projectRoot)
        {
            var definitions = new List<AssemblyDefinition>();
            var guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");

            for (var i = 0; i < guids.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (assetPath.StartsWith("Assets/", StringComparison.Ordinal) == false
                    && assetPath.StartsWith("Packages/", StringComparison.Ordinal) == false
                )
                {
                    continue;
                }

                var fullPath = Path.GetFullPath(Path.Combine(projectRoot, assetPath));

                if (File.Exists(fullPath) == false)
                {
                    continue;
                }

                try
                {
                    var definition = JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(fullPath));

                    if (definition is null || string.IsNullOrWhiteSpace(definition.name))
                    {
                        continue;
                    }

                    definition.path = fullPath;
                    definitions.Add(definition);
                }
                catch
                {
                }
            }

            definitions.Sort(static (left, right) =>
                string.CompareOrdinal(left.path, right.path)
            );
            return definitions;
        }

        private static string ResolveAssemblyDefinition(
              string projectRoot
            , string assemblyName
            , IReadOnlyList<string> sourceFiles
            , IReadOnlyList<AssemblyDefinition> definitions
        )
        {
            for (var i = 0; i < definitions.Count; i++)
            {
                if (string.Equals(definitions[i].name, assemblyName, StringComparison.Ordinal))
                {
                    return definitions[i].path;
                }
            }

            for (var i = 0; i < sourceFiles.Count; i++)
            {
                var directory = Path.GetDirectoryName(sourceFiles[i]);

                while (string.IsNullOrWhiteSpace(directory) == false
                    && DotnetWorkspaceProtocol.IsContained(projectRoot, directory)
                )
                {
                    var candidates = Directory.GetFiles(directory, "*.asmdef");

                    if (candidates.Length == 1)
                    {
                        return Path.GetFullPath(candidates[0]);
                    }

                    if (string.Equals(
                              Path.GetFullPath(directory)
                            , Path.GetFullPath(projectRoot)
                            , StringComparison.Ordinal
                        )
                    )
                    {
                        break;
                    }

                    directory = Path.GetDirectoryName(directory);
                }
            }

            return string.Empty;
        }

        private static string[] GetSortedPaths(string[] values)
        {
            if (values is null || values.Length == 0)
            {
                return Array.Empty<string>();
            }

            var result = new string[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                result[i] = Path.GetFullPath(values[i]);
            }

            Array.Sort(result, StringComparer.Ordinal);
            return result;
        }

        private static bool IsSupportedSourcePath(string projectRoot, string path)
        {
            var assetsRoot = Path.Combine(projectRoot, "Assets");
            var packagesRoot = Path.Combine(projectRoot, "Packages");
            return DotnetWorkspaceProtocol.IsContained(assetsRoot, path)
                || DotnetWorkspaceProtocol.IsContained(packagesRoot, path);
        }

        private static string[] SortDistinct(string[] values)
        {
            if (values is null || values.Length == 0)
            {
                return Array.Empty<string>();
            }

            return values
                .Where(static value => string.IsNullOrWhiteSpace(value) == false)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray();
        }

        private static void AddReferencePaths(
              IDictionary<string, DotnetWorkspaceProtocol.ReferenceInput> result
            , string[] paths
            , string projectRoot
            , string unityVersionRoot
        )
        {
            if (paths is null)
            {
                return;
            }

            for (var i = 0; i < paths.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i]))
                {
                    continue;
                }

                var path = Path.GetFullPath(paths[i]);
                result[path] = new DotnetWorkspaceProtocol.ReferenceInput {
                    path = path,
                    kind = ClassifyReference(projectRoot, unityVersionRoot, path),
                };
            }
        }

        private static string ClassifyReference(string projectRoot, string unityVersionRoot, string path)
        {
            var fileName = Path.GetFileName(path);

            if (string.Equals(fileName, "UnityEditor.dll", StringComparison.OrdinalIgnoreCase))
            {
                return "unityEditor";
            }

            if (string.Equals(fileName, "UnityEngine.dll", StringComparison.OrdinalIgnoreCase)
                || fileName.StartsWith("UnityEngine.", StringComparison.OrdinalIgnoreCase)
            )
            {
                return "unityModule";
            }

            var unityManagedRoot = Path.Combine(unityVersionRoot, "Editor", "Data", "Managed");

            if (DotnetWorkspaceProtocol.IsContained(unityManagedRoot, path))
            {
                return "unityManaged";
            }

            var scriptAssembliesRoot = Path.Combine(projectRoot, "Library", "ScriptAssemblies");

            if (DotnetWorkspaceProtocol.IsContained(scriptAssembliesRoot, path))
            {
                return "scriptAssembly";
            }

            if (DotnetWorkspaceProtocol.IsContained(projectRoot, path))
            {
                return "precompiled";
            }

            return "framework";
        }

        private static bool AreSupportedSources(string projectRoot, IReadOnlyList<string> paths)
        {
            for (var i = 0; i < paths.Count; i++)
            {
                try
                {
                    DotnetWorkspaceProtocol.RequireContainedPath(projectRoot, paths[i], allowMissing: false);
                }
                catch (CodeGenRunException)
                {
                    return false;
                }
            }

            return true;
        }

        private static string ReadStringProperty(object target, string name, string fallback)
        {
            if (target is null)
            {
                return fallback;
            }

            var property = target.GetType().GetProperty(
                name
                , BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase
            );
            var value = property?.GetValue(target)?.ToString();
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static bool ReadBooleanProperty(object target, string name)
        {
            if (target is null)
            {
                return false;
            }

            var property = target.GetType().GetProperty(
                name
                , BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase
            );
            return property?.GetValue(target) is true;
        }

        private static string[] ReadStringArrayProperty(object target, string name)
        {
            if (target is null)
            {
                return Array.Empty<string>();
            }

            var property = target.GetType().GetProperty(
                name
                , BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase
            );

            if (property?.GetValue(target) is not IEnumerable<string> values)
            {
                return Array.Empty<string>();
            }

            return values
                .Where(static value => string.IsNullOrWhiteSpace(value) == false)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray();
        }

        private static string[] ReadPathArrayProperty(object target, string name)
            => ReadStringArrayProperty(target, name)
                .Select(static path => Path.GetFullPath(path))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(static path => path, StringComparer.Ordinal)
                .ToArray();

        private static string ReadPathProperty(object target, string name)
        {
            var value = ReadStringProperty(target, name, string.Empty);
            return string.IsNullOrWhiteSpace(value) ? string.Empty : Path.GetFullPath(value);
        }

        private static string ReadNullable(IReadOnlyList<string> arguments)
        {
            for (var i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];

                if (argument.StartsWith("-nullable:", StringComparison.OrdinalIgnoreCase)
                    || argument.StartsWith("/nullable:", StringComparison.OrdinalIgnoreCase)
                )
                {
                    return argument[(argument.IndexOf(':') + 1)..];
                }
            }

            return "disable";
        }

        /// <summary>
        /// Maps a Unity assembly definition name to its canonical asset path.
        /// </summary>
        [Serializable]
        private sealed class AssemblyDefinition
        {
            /// <summary>
            /// The assembly definition name.
            /// </summary>
            public string name = string.Empty;

            /// <summary>
            /// The canonical path to the assembly definition asset.
            /// </summary>
            [NonSerialized]
            public string path = string.Empty;
        }
    }
}

#endif
