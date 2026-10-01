#if UNITY_EDITOR

using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace EncosyTower.Editor.SourceGen
{
    /// <summary>
    /// Applies a recoverable generated-source compiler argument to one Unity named build target.
    /// </summary>
    /// <remarks>
    /// The operation changes the target's global compiler arguments instead of package response files,
    /// allowing Unity to propagate the capture option to every normal script assembly regardless of package location.
    /// </remarks>
    internal sealed class SourceGenCaptureOperation
    {
        private const int RECOVERY_STATE_VERSION = 2;
        private const string GENERATED_CODE_ROOT_RELATIVE_PATH = "Library/EncosyTower/SourceGen";
        private const string RECOVERY_ROOT_RELATIVE_PATH = "Library/EncosyTower/SourceGen/Recovery";
        private const string RECOVERY_STATE_FILE_NAME = "state.json";
        private const string CURRENT_CAPTURE_DIRECTORY_NAME = "Current";

        private static readonly UTF8Encoding s_strictUtf8 = new(false, true);
        private static readonly UTF8Encoding s_utf8WithoutBom = new(false);
        private static readonly StringComparison s_pathComparison = Path.DirectorySeparatorChar == '\\'
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        private readonly string _projectRoot;
        private readonly int _buildTargetGroup;
        private readonly string _buildTargetName;
        private readonly Func<int, string[]> _getCompilerArguments;
        private readonly Action<int, string[]> _setCompilerArguments;
        private readonly string _recoveryRootPath;
        private readonly string _recoveryStatePath;

        /// <summary>
        /// Creates an operation for the currently selected Unity build target.
        /// </summary>
        /// <param name="projectRoot">The absolute or relative Unity project root.</param>
        internal SourceGenCaptureOperation(string projectRoot)
            : this(
                  projectRoot
                , (int)EditorUserBuildSettings.selectedBuildTargetGroup
                , GetBuildTargetName((int)EditorUserBuildSettings.selectedBuildTargetGroup)
                , GetCompilerArguments
                , SetCompilerArguments
            )
        {
        }

        /// <summary>
        /// Creates an operation with explicit build-target identity and compiler-setting accessors.
        /// </summary>
        /// <param name="projectRoot">The absolute or relative Unity project root.</param>
        /// <param name="buildTargetGroup">The serialized Unity build-target group value.</param>
        /// <param name="buildTargetName">The stable named build-target identity.</param>
        /// <param name="getCompilerArguments">Reads the complete compiler-argument array for a target.</param>
        /// <param name="setCompilerArguments">Replaces the complete compiler-argument array for a target.</param>
        internal SourceGenCaptureOperation(
              string projectRoot
            , int buildTargetGroup
            , string buildTargetName
            , Func<int, string[]> getCompilerArguments
            , Action<int, string[]> setCompilerArguments
        )
        {
            _projectRoot = NormalizeDirectory(projectRoot);
            _buildTargetGroup = buildTargetGroup;
            _buildTargetName = string.IsNullOrWhiteSpace(buildTargetName)
                ? throw new ArgumentException("A named build target is required.", nameof(buildTargetName))
                : buildTargetName;

            _getCompilerArguments = getCompilerArguments
                ?? throw new ArgumentNullException(nameof(getCompilerArguments));

            _setCompilerArguments = setCompilerArguments
                ?? throw new ArgumentNullException(nameof(setCompilerArguments));

            GeneratedCodeRootPath = GetContainedProjectPath(GENERATED_CODE_ROOT_RELATIVE_PATH);
            _recoveryRootPath = GetContainedProjectPath(RECOVERY_ROOT_RELATIVE_PATH);
            _recoveryStatePath = GetContainedPath(_recoveryRootPath, RECOVERY_STATE_FILE_NAME);
        }

        /// <summary>
        /// Gets the retained output directory for the current or recovered capture.
        /// </summary>
        internal string CaptureDirectoryPath { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the project-owned root containing all retained captures.
        /// </summary>
        internal string GeneratedCodeRootPath { get; }

        /// <summary>
        /// Gets whether recovery evidence exists and a new capture must remain blocked.
        /// </summary>
        internal bool HasRecoveryState
            => Directory.Exists(_recoveryRootPath)
                || File.Exists(_recoveryRootPath)
                || File.Exists(_recoveryStatePath);

        /// <summary>
        /// Saves the current compiler arguments for recovery and appends the generated-source output option.
        /// </summary>
        /// <param name="error">Receives an actionable failure description.</param>
        /// <returns><see langword="true"/> when the capture setting is installed and verified.</returns>
        internal bool Prepare(bool retainOutput, out string error)
        {
            error = string.Empty;
            CaptureDirectoryPath = string.Empty;

            try
            {
                if (HasRecoveryState)
                {
                    throw new InvalidOperationException(
                        $"Recovery evidence already exists at '{_recoveryRootPath}'."
                    );
                }

                // Use the target-wide argument array so package-local response files cannot bypass the capture option.
                var originalArguments = CloneAndValidateArguments(
                      _getCompilerArguments(_buildTargetGroup)
                    , "configured compiler arguments"
                );

                if (ContainsGeneratedFilesOption(originalArguments))
                {
                    throw new InvalidOperationException(
                        $"The '{_buildTargetName}' build target already contains a generated-files output option."
                    );
                }

                CaptureDirectoryPath = CreateCaptureDirectory(retainOutput);
                Directory.CreateDirectory(_recoveryRootPath);

                var recoveryState = new RecoveryState {
                    version = RECOVERY_STATE_VERSION,
                    captureDirectoryName = Path.GetFileName(CaptureDirectoryPath),
                    buildTargetGroup = _buildTargetGroup,
                    buildTargetName = _buildTargetName,
                    originalCompilerArguments = originalArguments,
                };

                // Persist recovery first because PlayerSettings survives domain reloads and can trigger compilation.
                WriteAllBytesAtomic(
                      _recoveryStatePath
                    , s_utf8WithoutBom.GetBytes(JsonUtility.ToJson(recoveryState))
                );

                var captureArguments = AppendCaptureArgument(originalArguments, CaptureDirectoryPath);

                // Replace and verify the complete array so later restoration preserves order and every
                // unrelated argument.
                _setCompilerArguments(_buildTargetGroup, captureArguments);
                RequireMatchingArguments(captureArguments, _getCompilerArguments(_buildTargetGroup));
                return true;
            }
            catch (Exception exception)
            {
                var prepareError =
                    $"Cannot prepare generated-source capture for '{_buildTargetName}': {exception.Message}";

                if (File.Exists(_recoveryStatePath))
                {
                    if (Restore(out _, out var restoreError) == false)
                    {
                        error = prepareError + $" Restoration also failed: {restoreError}";
                        return false;
                    }
                }

                error = prepareError;
                return false;
            }
        }

        /// <summary>
        /// Restores the exact saved compiler arguments and removes recovery evidence after verification.
        /// </summary>
        /// <param name="captureDirectoryPath">Receives the retained capture directory.</param>
        /// <param name="error">Receives an actionable failure description.</param>
        /// <returns><see langword="true"/> when restoration and verification both succeed.</returns>
        internal bool Restore(out string captureDirectoryPath, out string error)
        {
            captureDirectoryPath = string.Empty;
            error = string.Empty;

            try
            {
                var recoveryState = ReadAndValidateRecoveryState(out captureDirectoryPath);
                CaptureDirectoryPath = captureDirectoryPath;

                // Restore through PlayerSettings rather than rewriting its asset and risking unrelated
                // project settings.
                _setCompilerArguments(recoveryState.buildTargetGroup, recoveryState.originalCompilerArguments);
                RequireMatchingArguments(
                      recoveryState.originalCompilerArguments
                    , _getCompilerArguments(recoveryState.buildTargetGroup)
                );

                File.Delete(_recoveryStatePath);

                if (Directory.Exists(_recoveryRootPath)
                    && Directory.GetFileSystemEntries(_recoveryRootPath).Length == 0
                )
                {
                    Directory.Delete(_recoveryRootPath);
                }

                return true;
            }
            catch (Exception exception)
            {
                error = $"Cannot restore generated-source capture state at '{_recoveryStatePath}': {exception.Message}";
                return false;
            }
        }

        private RecoveryState ReadAndValidateRecoveryState(out string captureDirectoryPath)
        {
            if (File.Exists(_recoveryStatePath) == false)
            {
                throw new InvalidDataException($"The recovery state does not exist at '{_recoveryStatePath}'.");
            }

            var recoveryStateText = DecodeStrictUtf8(
                  File.ReadAllBytes(_recoveryStatePath)
                , _recoveryStatePath
            );

            RecoveryState recoveryState;

            try
            {
                recoveryState = JsonUtility.FromJson<RecoveryState>(recoveryStateText);
            }
            catch (Exception exception)
            {
                throw new InvalidDataException(
                      $"The recovery state is malformed at '{_recoveryStatePath}'."
                    , exception
                );
            }

            if (recoveryState is null || recoveryState.version != RECOVERY_STATE_VERSION)
            {
                throw new InvalidDataException(
                    $"The recovery state has an unsupported version at '{_recoveryStatePath}'."
                );
            }

            // Refuse cross-target recovery because applying valid arguments to the wrong target would
            // corrupt its settings.
            if (recoveryState.buildTargetGroup != _buildTargetGroup
                || string.Equals(recoveryState.buildTargetName, _buildTargetName, StringComparison.Ordinal) == false
            )
            {
                throw new InvalidDataException(
                    $"The recovery state targets a different build target at '{_recoveryStatePath}'."
                );
            }

            recoveryState.originalCompilerArguments = CloneAndValidateArguments(
                  recoveryState.originalCompilerArguments
                , "recovery compiler arguments"
            );

            if (ContainsGeneratedFilesOption(recoveryState.originalCompilerArguments))
            {
                throw new InvalidDataException(
                    $"The recovery state contains a generated-files output option at '{_recoveryStatePath}'."
                );
            }

            if (IsValidCaptureDirectoryName(recoveryState.captureDirectoryName) == false)
            {
                throw new InvalidDataException(
                    $"The recovery state has an invalid capture directory at '{_recoveryStatePath}'."
                );
            }

            captureDirectoryPath = GetContainedPath(
                  GeneratedCodeRootPath
                , recoveryState.captureDirectoryName
            );
            return recoveryState;
        }

        private string CreateCaptureDirectory(bool retainOutput)
        {
            Directory.CreateDirectory(GeneratedCodeRootPath);
            RejectReparsePoint(GeneratedCodeRootPath);

            if (retainOutput == false)
            {
                var currentDirectoryPath = GetContainedPath(
                      GeneratedCodeRootPath
                    , CURRENT_CAPTURE_DIRECTORY_NAME
                );

                if (string.Equals(Path.GetDirectoryName(currentDirectoryPath), GeneratedCodeRootPath, s_pathComparison)
                    == false
                    || string.Equals(
                          Path.GetFileName(currentDirectoryPath)
                        , CURRENT_CAPTURE_DIRECTORY_NAME
                        , StringComparison.Ordinal
                    ) == false
                )
                {
                    throw new InvalidOperationException(
                        $"The current capture directory is outside the SourceGen output root: '{currentDirectoryPath}'."
                    );
                }

                if (Directory.Exists(currentDirectoryPath))
                {
                    RejectReparsePoints(currentDirectoryPath);
                    Directory.Delete(currentDirectoryPath, true);
                }
                else if (File.Exists(currentDirectoryPath))
                {
                    throw new InvalidOperationException(
                        $"The current capture directory is occupied by a file: '{currentDirectoryPath}'."
                    );
                }

                Directory.CreateDirectory(currentDirectoryPath);
                return currentDirectoryPath;
            }

            // Use Unix milliseconds alone to shorten paths while retaining time ordering and practical uniqueness.
            while (true)
            {
                var now = DateTimeOffset.UtcNow;
                var unixMilliseconds = now.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
                var directoryPath = GetContainedPath(GeneratedCodeRootPath, unixMilliseconds);

                if (Directory.Exists(directoryPath) || File.Exists(directoryPath))
                {
                    continue;
                }

                Directory.CreateDirectory(directoryPath);
                return directoryPath;
            }
        }

        private string GetContainedProjectPath(string relativePath)
            => GetContainedPath(_projectRoot, relativePath);

        private string GetContainedPath(string root, string path)
        {
            var canonicalRoot = NormalizeDirectory(root);
            var canonicalPath = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(canonicalRoot, path));

            // Check directory boundaries, not string prefixes, so sibling paths cannot pass containment validation.
            if (IsStrictDescendant(canonicalPath, canonicalRoot) == false
                || IsStrictDescendant(canonicalPath, _projectRoot) == false
            )
            {
                throw new InvalidOperationException(
                    $"The derived path '{canonicalPath}' is outside the required project directory."
                );
            }

            return canonicalPath;
        }

        private void WriteAllBytesAtomic(string path, byte[] bytes)
        {
            var destinationPath = Path.GetFullPath(path);

            if (IsStrictDescendant(destinationPath, _projectRoot) == false)
            {
                throw new InvalidOperationException(
                    $"The atomic-write destination is outside the Unity project: '{destinationPath}'."
                );
            }

            var directoryPath = Path.GetDirectoryName(destinationPath);

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new InvalidOperationException(
                    $"The atomic-write destination has no directory: '{destinationPath}'."
                );
            }

            Directory.CreateDirectory(directoryPath);
            var temporaryPath = GetContainedPath(
                directoryPath,
                $"{Path.GetFileName(destinationPath)}.encosy-{Guid.NewGuid():N}.tmp"
            );

            // Write beside the destination so replace or move stays on the same volume and is atomic for one file.
            try
            {
                File.WriteAllBytes(temporaryPath, bytes);

                if (File.Exists(destinationPath))
                {
                    File.Replace(temporaryPath, destinationPath, null);
                }
                else
                {
                    File.Move(temporaryPath, destinationPath);
                }
            }
            finally
            {
                TryDeleteFile(temporaryPath);
            }
        }

        private static string[] AppendCaptureArgument(string[] originalArguments, string captureDirectoryPath)
        {
            var result = new string[originalArguments.Length + 1];
            Array.Copy(originalArguments, result, originalArguments.Length);
            result[^1] = $"-generatedfilesout:\"{captureDirectoryPath}\"";
            return result;
        }

        private static string[] CloneAndValidateArguments(string[] arguments, string description)
        {
            if (arguments is null)
            {
                return Array.Empty<string>();
            }

            var result = new string[arguments.Length];

            for (var i = 0; i < arguments.Length; i++)
            {
                result[i] = arguments[i]
                    ?? throw new InvalidDataException($"The {description} contain a null entry.");
            }

            return result;
        }

        private static bool ContainsGeneratedFilesOption(string[] arguments)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                var value = arguments[i].TrimStart();

                if (value.StartsWith("-generatedfilesout:", StringComparison.OrdinalIgnoreCase)
                    || value.StartsWith("/generatedfilesout:", StringComparison.OrdinalIgnoreCase)
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static void RequireMatchingArguments(string[] expected, string[] actual)
        {
            if (actual is null || expected.Length != actual.Length)
            {
                throw new InvalidOperationException("Unity did not retain the expected compiler arguments.");
            }

            for (var i = 0; i < expected.Length; i++)
            {
                if (string.Equals(expected[i], actual[i], StringComparison.Ordinal) == false)
                {
                    throw new InvalidOperationException("Unity did not retain the expected compiler arguments.");
                }
            }
        }

        private static string DecodeStrictUtf8(byte[] bytes, string path)
        {
            try
            {
                return s_strictUtf8.GetString(bytes);
            }
            catch (DecoderFallbackException exception)
            {
                throw new InvalidDataException($"The file is not valid UTF-8: '{path}'.", exception);
            }
        }

        private static bool IsValidCaptureDirectoryName(string value)
        {
            if (string.IsNullOrEmpty(value)
                || Path.IsPathRooted(value)
                || string.Equals(Path.GetFileName(value), value, StringComparison.Ordinal) == false
            )
            {
                return false;
            }

            if (string.Equals(value, CURRENT_CAPTURE_DIRECTORY_NAME, StringComparison.Ordinal))
            {
                return true;
            }

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] < '0' || value[i] > '9')
                {
                    return false;
                }
            }

            return long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var unixMilliseconds)
                && unixMilliseconds >= DateTimeOffset.MinValue.ToUnixTimeMilliseconds()
                && unixMilliseconds <= DateTimeOffset.MaxValue.ToUnixTimeMilliseconds()
            ;
        }

        private static void RejectReparsePoints(string root)
        {
            RejectReparsePoint(root);
            var rootInfo = new DirectoryInfo(root);
            var descendants = rootInfo.GetFileSystemInfos("*", SearchOption.AllDirectories);

            for (var i = 0; i < descendants.Length; i++)
            {
                if ((descendants[i].Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    throw new InvalidOperationException(
                        $"The SourceGen capture directory contains a reparse point: '{descendants[i].FullName}'."
                    );
                }
            }
        }

        private static void RejectReparsePoint(string root)
        {
            var rootInfo = new DirectoryInfo(root);

            if ((rootInfo.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidOperationException(
                    $"The SourceGen capture directory is a reparse point: '{root}'."
                );
            }
        }

        private static bool IsStrictDescendant(string path, string root)
        {
            if (path.Length <= root.Length || path.StartsWith(root, s_pathComparison) == false)
            {
                return false;
            }

            if (root.Length > 0
                && (root[^1] == Path.DirectorySeparatorChar || root[^1] == Path.AltDirectorySeparatorChar)
            )
            {
                return true;
            }

            var separator = path[root.Length];
            return separator == Path.DirectorySeparatorChar || separator == Path.AltDirectorySeparatorChar;
        }

        private static string NormalizeDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A Unity project root is required.", nameof(path));
            }

            var fullPath = Path.GetFullPath(path);
            var pathRoot = Path.GetPathRoot(fullPath) ?? string.Empty;

            while (fullPath.Length > pathRoot.Length
                && (fullPath[^1] == Path.DirectorySeparatorChar || fullPath[^1] == Path.AltDirectorySeparatorChar)
            )
            {
                fullPath = fullPath[..^1];
            }

            return fullPath;
        }

        private static string GetBuildTargetName(int buildTargetGroup)
            => NamedBuildTarget.FromBuildTargetGroup((BuildTargetGroup)buildTargetGroup).TargetName;

        private static string[] GetCompilerArguments(int buildTargetGroup)
            => PlayerSettings.GetAdditionalCompilerArguments(
                NamedBuildTarget.FromBuildTargetGroup((BuildTargetGroup)buildTargetGroup)
            );

        private static void SetCompilerArguments(int buildTargetGroup, string[] arguments)
        {
            PlayerSettings.SetAdditionalCompilerArguments(
                  NamedBuildTarget.FromBuildTargetGroup((BuildTargetGroup)buildTargetGroup)
                , arguments
            );
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
        }

        /// <summary>
        /// Stores the minimum durable state required to undo a persistent compiler-setting change.
        /// </summary>
        [Serializable]
        private sealed class RecoveryState
        {
            public int version;
            public string captureDirectoryName = string.Empty;
            public int buildTargetGroup;
            public string buildTargetName = string.Empty;
            public string[] originalCompilerArguments = Array.Empty<string>();
        }
    }
}

#endif
