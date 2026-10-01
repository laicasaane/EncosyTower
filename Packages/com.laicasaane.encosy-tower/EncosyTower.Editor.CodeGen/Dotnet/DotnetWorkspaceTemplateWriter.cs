#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Captures Unity inputs and materializes the fixed temporary .NET workspace.
    /// </summary>
    internal sealed class DotnetWorkspaceTemplateWriter
    {
        private const string WORKSPACE_PARENT_NAME = "EncosyTower";
        private const string WORKSPACE_NAME = "CodeGen";
        private const string CURRENT_WORKSPACE_NAME = "Current";
        private const string RUNNER_PROJECT_NAME = "EncosyCodeGenRunner.csproj";
        private const string ROSLYN_VERSION = "4.3.1";
        private const string UNITY3D_VERSION = "3.1.1";

        private static readonly string[] s_requiredTemplatePaths = {
            "Directory.Build.props",
            "Directory.Build.targets",
            "Directory.Packages.props",
            "global.json",
            "EncosyCodeGen.slnx",
            Path.Combine("Runner", RUNNER_PROJECT_NAME),
            Path.Combine("Runner", "Program.cs"),
            Path.Combine("Runner", "ThrowHelper.cs"),
            Path.Combine("Island", "Island.csproj"),
            Path.Combine("Island", "GeneratedIslands.props"),
        };

        private static readonly string[] s_runtimeDirectoryNames = {
            "artifacts",
            "dependencies",
            "islands",
            "logs",
            "manifests",
            "results",
            "sources",
        };

        private static readonly string[] s_hostOwnedTokens = {
            "{{ENCOSY_DOTNET_SDK_VERSION}}",
            "{{ENCOSY_ROSLYN_VERSION}}",
            "{{ENCOSY_UNITY3D_VERSION}}",
        };

        private static readonly UTF8Encoding s_utf8NoBom = new(false);

        private readonly string _projectRoot;
        private readonly string _workspaceParent;

        /// <summary>
        /// Initializes a workspace writer for the current Unity project.
        /// </summary>
        internal DotnetWorkspaceTemplateWriter()
        {
            _projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            _workspaceParent = Path.GetFullPath(
                Path.Combine(_projectRoot, "Library", WORKSPACE_PARENT_NAME, WORKSPACE_NAME)
            );
        }

        internal DotnetWorkspaceTemplateWriter(string workspaceParent)
        {
            _projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            _workspaceParent = Path.GetFullPath(workspaceParent);
        }

        private static StringComparison PathComparison
            => Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        internal async Task StopRecordedProcessTreesAsync(
              DotnetProcessRunner processRunner
            , CancellationToken token = default
        )
        {
            if (processRunner is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(processRunner));
            }

            token.ThrowIfCancellationRequested();

            if (Directory.Exists(_workspaceParent) == false)
            {
                return;
            }

            RejectReparsePoint(_workspaceParent);
            var workspaceRoots = Directory.GetDirectories(_workspaceParent);

            for (var i = 0; i < workspaceRoots.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                var workspaceRoot = workspaceRoots[i];

                if (IsSafeWorkspaceRoot(_workspaceParent, workspaceRoot) == false)
                {
                    continue;
                }

                var processManifestPath = GetProcessManifestPath(workspaceRoot);

                if (File.Exists(processManifestPath) == false)
                {
                    continue;
                }

                RejectReparsePoints(workspaceRoot);
                await processRunner.StopRecordedProcessTreeAsync(
                      workspaceRoot
                    , processManifestPath
                    , token
                ).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Selects an SDK, captures Unity compilation inputs, and recreates the workspace.
        /// </summary>
        internal async Task<DotnetWorkspace> CreateAsync(
              bool retainDotnetSolutions
            , CancellationToken token = default
        )
        {
            var invocationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            token.ThrowIfCancellationRequested();
            var dotnetPath = ResolveDotnetExecutable();
            var sdkVersion = SelectSdkVersion(dotnetPath, token);
            var snapshot = UnityCompilationSnapshot.Capture(sdkVersion, dotnetPath);
            var workspaceRoot = GetWorkspaceRoot(retainDotnetSolutions, invocationTimestamp);
            var templateRoot = ResolveTemplateRoot();

            return await Task.Run(
                () => CreateWorkspace(
                      snapshot.Manifest
                    , dotnetPath
                    , sdkVersion
                    , templateRoot
                    , workspaceRoot
                    , retainDotnetSolutions
                    , token
                )
                , token
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Validates and reads the aggregate result produced by a prepared workspace.
        /// </summary>
        internal GeneratedCodeBatch ReadResult(DotnetWorkspace workspace)
        {
            if (workspace is null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(workspace));
            }

            if (IsSafeWorkspaceRoot(_workspaceParent, workspace.RootPath) == false)
            {
                throw ThrowHelper.CreateWorkspaceResultRootUnexpectedException(workspace.RootPath);
            }

            return DotnetWorkspaceProtocol.ReadResult(workspace.ResultManifestPath, _projectRoot);
        }

        /// <summary>
        /// Resolves the dotnet executable from the current process environment.
        /// </summary>
        internal static string ResolveDotnetExecutable()
        {
#if UNITY_EDITOR_WIN
            var executableName = "dotnet.exe";
#else
            var executableName = "dotnet";
#endif

            var pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            var paths = pathValue.Split(Path.PathSeparator);

            for (var i = 0; i < paths.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i]))
                {
                    continue;
                }

                var candidate = Path.GetFullPath(Path.Combine(paths[i].Trim(), executableName));

                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            throw ThrowHelper.CreateDotnetExecutableMissingException();
        }

        /// <summary>
        /// Selects the newest installed stable SDK reported by the dotnet executable.
        /// </summary>
        internal static string SelectSdkVersion(string dotnetPath, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var startInfo = new ProcessStartInfo {
                FileName = dotnetPath,
                Arguments = "--list-sdks",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using var process = Process.Start(startInfo);

            if (process is null)
            {
                throw ThrowHelper.CreateDotnetSdkListStartException(dotnetPath);
            }

            var standardOutput = process.StandardOutput.ReadToEnd();
            var standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();
            token.ThrowIfCancellationRequested();

            if (process.ExitCode != 0)
            {
                throw ThrowHelper.CreateDotnetSdkListFailedException(
                    dotnetPath,
                    process.ExitCode,
                    standardError
                );
            }

            var versions = ParseSdkVersions(standardOutput);

            if (versions.Count == 0)
            {
                throw ThrowHelper.CreateStableDotnetSdkMissingException();
            }

            return versions[^1].text;
        }

        /// <summary>
        /// Parses and sorts stable SDK versions from <c>dotnet --list-sdks</c> output.
        /// </summary>
        internal static IReadOnlyList<(Version version, string text)> ParseSdkVersions(string standardOutput)
        {
            var result = new List<(Version version, string text)>();

            using var reader = new StringReader(standardOutput ?? string.Empty);

            while (reader.ReadLine() is { } line)
            {
                var separator = line.IndexOf(' ');
                var text = (separator < 0 ? line : line[..separator]).Trim();

                if (text.Contains("-", StringComparison.Ordinal)
                    || Version.TryParse(text, out var version) == false
                )
                {
                    continue;
                }

                result.Add((version, text));
            }

            result.Sort(static (left, right) => left.version.CompareTo(right.version));
            return result;
        }

        /// <summary>
        /// Determines whether a workspace path is the module's fixed location under Library.
        /// </summary>
        internal static bool IsSafeWorkspaceRoot(string workspaceParent, string workspaceRoot)
        {
            var normalizedParent = NormalizeDirectory(workspaceParent);
            var normalizedRoot = NormalizeDirectory(workspaceRoot);

            if (string.Equals(Path.GetDirectoryName(normalizedRoot), normalizedParent, PathComparison) == false)
            {
                return false;
            }

            var workspaceName = Path.GetFileName(normalizedRoot);

            if (string.Equals(workspaceName, CURRENT_WORKSPACE_NAME, StringComparison.Ordinal))
            {
                return true;
            }

            return long.TryParse(
                  workspaceName
                , NumberStyles.None
                , CultureInfo.InvariantCulture
                , out var timestamp
            )
                && timestamp >= 0
                && string.Equals(
                      timestamp.ToString(CultureInfo.InvariantCulture)
                    , workspaceName
                    , StringComparison.Ordinal
                );
        }

        internal static string ResolveTemplateRoot()
        {
            var package = PackageInfo.FindForAssembly(typeof(DotnetWorkspaceTemplateWriter).Assembly);

            if (package is null || string.IsNullOrWhiteSpace(package.resolvedPath))
            {
                throw ThrowHelper.CreateTemplatePackagePathMissingException();
            }

            return Path.GetFullPath(
                Path.Combine(
                    package.resolvedPath
                    , "EncosyTower.Editor.CodeGen"
                    , "Dotnet"
                    , "Templates~"
                    , "Workspace"
                )
            );
        }

        internal static void MaterializeTemplateTree(
              string templateRoot
            , string workspaceRoot
            , string sdkVersion
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var normalizedTemplateRoot = NormalizeDirectory(templateRoot);
            var normalizedWorkspaceRoot = NormalizeDirectory(workspaceRoot);
            var currentTemplatePath = normalizedTemplateRoot;

            try
            {
                if (Directory.Exists(normalizedTemplateRoot) == false)
                {
                    throw ThrowHelper.CreateTemplateRootMissingException(normalizedTemplateRoot);
                }

                RejectTemplateReparsePoint(normalizedTemplateRoot);

                for (var i = 0; i < s_requiredTemplatePaths.Length; i++)
                {
                    currentTemplatePath = Path.Combine(normalizedTemplateRoot, s_requiredTemplatePaths[i]);

                    if (File.Exists(currentTemplatePath) == false)
                    {
                        throw ThrowHelper.CreateRequiredTemplateMissingException(currentTemplatePath);
                    }
                }

                Directory.CreateDirectory(normalizedWorkspaceRoot);
                CopyTemplateDirectory(
                    normalizedTemplateRoot
                    , normalizedTemplateRoot
                    , normalizedWorkspaceRoot
                    , token
                );
            }
            catch (CodeGenRunException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw ThrowHelper.CreateTemplateMaterializationException(currentTemplatePath, exception.Message);
            }

            ApplyHostTemplateValues(normalizedWorkspaceRoot, sdkVersion);
            CreateRuntimeDirectories(normalizedWorkspaceRoot);
        }

        internal static string ComposeTemplate(
              string templatePath
            , string content
            , IReadOnlyList<KeyValuePair<string, string>> replacements
            , IReadOnlyCollection<string> ownedTokens
        )
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < replacements.Count; i++)
            {
                var replacement = replacements[i];

                if (keys.Add(replacement.Key) == false)
                {
                    throw ThrowHelper.CreateDuplicateTemplateTokenException(templatePath, replacement.Key);
                }

                if (content.Contains(replacement.Key, StringComparison.Ordinal) == false)
                {
                    throw ThrowHelper.CreateMissingTemplateTokenException(templatePath, replacement.Key);
                }

                content = content.Replace(replacement.Key, replacement.Value, StringComparison.Ordinal);
            }

            foreach (var token in ownedTokens)
            {
                if (content.Contains(token, StringComparison.Ordinal))
                {
                    throw ThrowHelper.CreateUnresolvedTemplateTokenException(templatePath, token);
                }
            }

            return content;
        }

        private DotnetWorkspace CreateWorkspace(
              DotnetWorkspaceProtocol.InputManifest manifest
            , string dotnetPath
            , string sdkVersion
            , string templateRoot
            , string workspaceRoot
            , bool retainDotnetSolutions
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (Directory.Exists(_workspaceParent))
            {
                RejectReparsePoint(_workspaceParent);
            }

            if (IsSafeWorkspaceRoot(_workspaceParent, workspaceRoot) == false)
            {
                throw ThrowHelper.CreateWorkspaceRecreationPathUnexpectedException(workspaceRoot);
            }

            var processManifestPath = GetProcessManifestPath(workspaceRoot);

            if (File.Exists(processManifestPath))
            {
                throw ThrowHelper.CreateWorkspaceProcessManifestLockedException();
            }

            if (Directory.Exists(workspaceRoot))
            {
                RejectReparsePoints(workspaceRoot);

                if (retainDotnetSolutions)
                {
                    throw ThrowHelper.CreateRetainedWorkspaceExistsException(workspaceRoot);
                }

                Directory.Delete(workspaceRoot, true);
            }
            else if (File.Exists(workspaceRoot))
            {
                throw ThrowHelper.CreateWorkspacePathOccupiedException(workspaceRoot);
            }

            MaterializeTemplateTree(templateRoot, workspaceRoot, sdkVersion, token);
            SnapshotPrecompiledReferences(manifest, workspaceRoot, token);

            token.ThrowIfCancellationRequested();

            var runnerDirectory = Path.Combine(workspaceRoot, "Runner");
            var runnerProjectPath = Path.Combine(runnerDirectory, RUNNER_PROJECT_NAME);

            var inputManifestPath = Path.Combine(workspaceRoot, "manifests", "input.json");
            DotnetWorkspaceProtocol.WriteInput(inputManifestPath, manifest);

            var resultManifestPath = Path.Combine(workspaceRoot, "results", "aggregate-result.json");

            var standardOutputPath = Path.Combine(workspaceRoot, "logs", "prepare.stdout.log");

            var standardErrorPath = Path.Combine(workspaceRoot, "logs", "prepare.stderr.log");

            var arguments = new[] {
                "run",
                "--project",
                runnerProjectPath,
                "--configuration",
                "Release",
                "--no-launch-profile",
                "--property:CodeGenPhase=Prepare",
                "--",
                "--phase",
                "prepare",
                "--input",
                inputManifestPath,
            };

            var request = new DotnetProcessRequest(
                  "Prepare"
                , dotnetPath
                , workspaceRoot
                , arguments
                , standardOutputPath
                , standardErrorPath
                , processManifestPath
            );

            return new DotnetWorkspace(workspaceRoot, sdkVersion, request, resultManifestPath, processManifestPath);
        }

        private static void ApplyHostTemplateValues(string workspaceRoot, string sdkVersion)
        {
            var templates = new[] {
                new KeyValuePair<string, IReadOnlyList<KeyValuePair<string, string>>>(
                      Path.Combine(workspaceRoot, "global.json")
                    , new[] {
                        new KeyValuePair<string, string>(
                            "{{ENCOSY_DOTNET_SDK_VERSION}}"
                            , EscapeJson(sdkVersion)
                        ),
                    }
                ),
                new KeyValuePair<string, IReadOnlyList<KeyValuePair<string, string>>>(
                      Path.Combine(workspaceRoot, "Runner", RUNNER_PROJECT_NAME)
                    , new[] {
                        new KeyValuePair<string, string>(
                            "{{ENCOSY_ROSLYN_VERSION}}"
                            , EscapeXml(ROSLYN_VERSION)
                        ),
                    }
                ),
                new KeyValuePair<string, IReadOnlyList<KeyValuePair<string, string>>>(
                      Path.Combine(workspaceRoot, "Island", "Island.csproj")
                    , new[] {
                        new KeyValuePair<string, string>(
                            "{{ENCOSY_UNITY3D_VERSION}}"
                            , EscapeXml(UNITY3D_VERSION)
                        ),
                    }
                ),
            };

            for (var i = 0; i < templates.Length; i++)
            {
                var path = templates[i].Key;

                try
                {
                    var content = File.ReadAllText(path, Encoding.UTF8);
                    content = ComposeTemplate(path, content, templates[i].Value, s_hostOwnedTokens);
                    File.WriteAllText(path, content, s_utf8NoBom);
                }
                catch (CodeGenRunException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    throw ThrowHelper.CreateTemplateMaterializationException(path, exception.Message);
                }
            }
        }

        private static void CopyTemplateDirectory(
              string templateRoot
            , string sourceDirectory
            , string workspaceRoot
            , CancellationToken token
        )
        {
            FileSystemInfo[] entries;

            try
            {
                entries = new DirectoryInfo(sourceDirectory).GetFileSystemInfos();
            }
            catch (Exception exception)
            {
                throw ThrowHelper.CreateTemplateDirectoryReadException(sourceDirectory, exception.Message);
            }

            for (var i = 0; i < entries.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                var sourcePath = entries[i].FullName;
                RejectTemplateReparsePoint(sourcePath);
                var relativePath = Path.GetRelativePath(templateRoot, sourcePath);
                var destination = Path.GetFullPath(Path.Combine(workspaceRoot, relativePath));
                ThrowHelper.ThrowIfTemplateDestinationEscapes(workspaceRoot, destination, sourcePath);

                if (entries[i] is DirectoryInfo)
                {
                    Directory.CreateDirectory(destination);
                    CopyTemplateDirectory(templateRoot, sourcePath, workspaceRoot, token);
                }
                else
                {
                    try
                    {
                        File.Copy(sourcePath, destination, overwrite: false);
                    }
                    catch (Exception exception)
                    {
                        throw ThrowHelper.CreateTemplateCopyException(sourcePath, exception.Message);
                    }
                }
            }
        }

        private static void CreateRuntimeDirectories(string workspaceRoot)
        {
            for (var i = 0; i < s_runtimeDirectoryNames.Length; i++)
            {
                Directory.CreateDirectory(Path.Combine(workspaceRoot, s_runtimeDirectoryNames[i]));
            }
        }

        private static string EscapeJson(string value)
            => value.Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\"", "\\\"", StringComparison.Ordinal);

        private static string EscapeXml(string value)
            => SecurityElement.Escape(value) ?? string.Empty;

        private static void RejectTemplateReparsePoint(string path)
        {
            var attributes = File.GetAttributes(path);

            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw ThrowHelper.CreateTemplateReparsePointException(path);
            }
        }

        private static void SnapshotPrecompiledReferences(
              DotnetWorkspaceProtocol.InputManifest manifest
            , string workspaceRoot
            , CancellationToken token
        )
        {
            for (var assemblyIndex = 0; assemblyIndex < manifest.assemblies.Length; assemblyIndex++)
            {
                var assembly = manifest.assemblies[assemblyIndex];

                for (var referenceIndex = 0; referenceIndex < assembly.references.Length; referenceIndex++)
                {
                    token.ThrowIfCancellationRequested();
                    var reference = assembly.references[referenceIndex];

                    if (string.Equals(reference.kind, "precompiled", StringComparison.Ordinal) == false)
                    {
                        continue;
                    }

                    var directory = Path.Combine(workspaceRoot, "dependencies", SanitizeFileName(assembly.name));
                    Directory.CreateDirectory(directory);
                    var destination = Path.Combine(
                          directory
                        , $"{referenceIndex:D4}-{Path.GetFileName(reference.path)}"
                    );
                    File.Copy(reference.path, destination, overwrite: false);
                    reference.path = destination;
                }
            }
        }

        private static string SanitizeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var characters = value.ToCharArray();

            for (var i = 0; i < characters.Length; i++)
            {
                if (Array.IndexOf(invalid, characters[i]) >= 0)
                {
                    characters[i] = '_';
                }
            }

            return new string(characters);
        }

        internal string GetWorkspaceRoot(bool retainDotnetSolutions, long invocationTimestamp)
            => Path.Combine(
                  _workspaceParent
                , retainDotnetSolutions
                    ? invocationTimestamp.ToString(CultureInfo.InvariantCulture)
                    : CURRENT_WORKSPACE_NAME
            );

        private static string GetProcessManifestPath(string workspaceRoot)
            => Path.Combine(workspaceRoot, "manifests", "dotnet-process.json");

        private static string NormalizeDirectory(string path)
            => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        private static void RejectReparsePoints(string root)
        {
            RejectReparsePoint(root);
            var rootInfo = new DirectoryInfo(root);

            var descendants = rootInfo.GetFileSystemInfos("*", SearchOption.AllDirectories);

            for (var i = 0; i < descendants.Length; i++)
            {
                if ((descendants[i].Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    throw ThrowHelper.CreateWorkspaceReparsePointException(descendants[i].FullName);
                }
            }
        }

        private static void RejectReparsePoint(string root)
        {
            var rootInfo = new DirectoryInfo(root);

            if ((rootInfo.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw ThrowHelper.CreateWorkspaceRootReparsePointException(root);
            }
        }
    }
}

#endif
