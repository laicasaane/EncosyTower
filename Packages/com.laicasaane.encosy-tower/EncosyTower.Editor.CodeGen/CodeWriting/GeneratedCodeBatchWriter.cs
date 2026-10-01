#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using EncosyTower.CodeGen;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Validates and atomically commits a complete generated-code batch to the Unity project.
    /// </summary>
    internal sealed class GeneratedCodeBatchWriter
    {
        private static readonly Encoding s_utf8WithoutBom = new UTF8Encoding(false, true);
        private static readonly StringComparer s_pathComparer = Path.DirectorySeparatorChar == '\\'
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        private static readonly StringComparison s_pathComparison =
            Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        /// <summary>
        /// Provides the operation that refreshes the Unity Asset Database after commit.
        /// </summary>
        /// <remarks>
        /// Kept per writer so tests can inject failures without mutating process-wide static state.
        /// </remarks>
        private readonly Action _refreshAssets;

        /// <summary>
        /// Provides the operation that moves a staged file to a new destination.
        /// </summary>
        /// <remarks>
        /// Kept per writer so tests can inject failures without mutating process-wide static state.
        /// </remarks>
        private readonly Action<string, string> _moveFile;

        /// <summary>
        /// Provides the operation that atomically replaces a destination and creates its backup.
        /// </summary>
        /// <remarks>
        /// Kept per writer so tests can inject failures without mutating process-wide static state.
        /// </remarks>
        private readonly Action<string, string, string> _replaceFile;

        /// <summary>
        /// Provides the operation that reads file attributes during destination validation.
        /// </summary>
        /// <remarks>
        /// Kept per writer so tests can inject failures without mutating process-wide static state.
        /// </remarks>
        private readonly Func<string, FileAttributes> _getAttributes;

        /// <summary>
        /// Initializes a writer that uses the Unity Asset Database and system file operations.
        /// </summary>
        internal GeneratedCodeBatchWriter()
            : this(AssetDatabase.Refresh, File.Move, File.Replace, File.GetAttributes)
        {
        }

        /// <summary>
        /// Initializes a writer with the supplied Asset Database and file operations.
        /// </summary>
        /// <remarks>
        /// Supports deterministic failure-path tests without mutating process-wide static state.
        /// </remarks>
        /// <param name="refreshAssets">
        /// The operation that refreshes the Unity Asset Database.
        /// </param>
        /// <param name="moveFile">The operation that moves a staged file.</param>
        /// <param name="replaceFile">The operation that replaces a destination file.</param>
        /// <param name="getAttributes">The operation that reads file attributes.</param>
        internal GeneratedCodeBatchWriter(
              Action refreshAssets
            , Action<string, string> moveFile
            , Action<string, string, string> replaceFile
            , Func<string, FileAttributes> getAttributes
        )
        {
            _refreshAssets = refreshAssets;
            _moveFile = moveFile;
            _replaceFile = replaceFile;
            _getAttributes = getAttributes;
        }

        /// <summary>
        /// Stages changed outputs, commits them transactionally, and refreshes the Asset Database.
        /// </summary>
        internal int Write(GeneratedCodeBatch batch, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            ValidateBatch(batch);

            var projectRoot = GetProjectRoot();
            var stagingRoot = CreateStagingRoot(projectRoot);
            var writes = new List<StagedWrite>(batch.GeneratedCodes.Length);

            try
            {
                Stage(batch.GeneratedCodes, projectRoot, stagingRoot, writes);
                token.ThrowIfCancellationRequested();

                if (writes.Count == 0)
                {
                    return 0;
                }

                ValidatePredecessors(writes, projectRoot);
                token.ThrowIfCancellationRequested();
                Commit(writes, projectRoot);

                return writes.Count;
            }
            finally
            {
                DeleteDirectoryBestEffort(stagingRoot);
            }
        }

        private static void ValidateBatch(GeneratedCodeBatch batch)
        {
            if (batch.GeneratedCodes is null)
            {
                throw ThrowHelper.CreateGeneratedCodesNullException();
            }

            if (batch.Diagnostics is null)
            {
                throw ThrowHelper.CreateDiagnosticsNullException();
            }

            if (batch.AllCandidatesSkipped && batch.GeneratedCodes.Length > 0)
            {
                throw ThrowHelper.CreateAllSkippedBatchContainsCodeException();
            }

            var diagnostics = batch.Diagnostics;

            for (var i = 0; i < diagnostics.Length; i++)
            {
                var diagnostic = diagnostics[i];

                if (diagnostic.Code is null)
                {
                    throw ThrowHelper.CreateDiagnosticCodeNullException(i);
                }

                if (diagnostic.Message is null)
                {
                    throw ThrowHelper.CreateDiagnosticMessageNullException(i);
                }

                if (diagnostic.FilePath is null)
                {
                    throw ThrowHelper.CreateDiagnosticFilePathNullException(i);
                }
            }
        }

        private static string GetProjectRoot()
        {
            var assetsPath = Path.GetFullPath(Application.dataPath);
            var projectRoot = Directory.GetParent(assetsPath)?.FullName;

            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                throw ThrowHelper.CreateUnityProjectRootResolutionException(assetsPath);
            }

            return Path.GetFullPath(projectRoot);
        }

        private static string CreateStagingRoot(string projectRoot)
        {
            var stagingRoot = Path.Combine(
                  projectRoot
                , "Library"
                , "EncosyTower"
                , "CodeGen"
                , "Staging"
                , Guid.NewGuid().ToString("N")
            );

            try
            {
                Directory.CreateDirectory(stagingRoot);
                return stagingRoot;
            }
            catch (Exception exception)
            {
                throw ThrowHelper.CreateGeneratedCodeStagingDirectoryException(stagingRoot, exception);
            }
        }

        private void Stage(
              GeneratedCode[] generatedCodes
            , string projectRoot
            , string stagingRoot
            , List<StagedWrite> writes
        )
        {
            var destinations = new HashSet<string>(s_pathComparer);

            for (var i = 0; i < generatedCodes.Length; i++)
            {
                var generatedCode = generatedCodes[i];

                if (generatedCode.content is null)
                {
                    throw ThrowHelper.CreateGeneratedContentNullException(i);
                }

                var destinationPath = GetDestinationPath(generatedCode.filePath, projectRoot, i);

                if (destinations.Add(destinationPath) == false)
                {
                    throw ThrowHelper.CreateDuplicateGeneratedDestinationException(i, destinationPath);
                }

                ValidateDestination(destinationPath, projectRoot);
                var contentBytes = GetContentBytes(generatedCode.content, destinationPath);
                var destinationExists = File.Exists(destinationPath);
                var originalBytes = destinationExists
                    ? ReadAllBytes(destinationPath, ThrowHelper.WRITE_STAGING_FAILED)
                    : Array.Empty<byte>();

                if (destinationExists && BytesEqual(originalBytes, contentBytes))
                {
                    continue;
                }

                var stagingPath = Path.Combine(stagingRoot, $"{i:D8}.tmp");
                WriteStagedBytes(stagingPath, contentBytes);
                writes.Add(
                    new StagedWrite(
                          destinationPath
                        , stagingPath
                        , Path.Combine(stagingRoot, $"{i:D8}.bak")
                        , destinationExists
                        , originalBytes
                    )
                );
            }

            writes.Sort(static (left, right) =>
                s_pathComparer.Compare(left.DestinationPath, right.DestinationPath)
            );
        }

        private static string GetDestinationPath(string filePath, string projectRoot, int index)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw ThrowHelper.CreateGeneratedFilePathBlankException(index);
            }

            try
            {
                var combinedPath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(projectRoot, filePath);

                return Path.GetFullPath(combinedPath);
            }
            catch (Exception exception)
            {
                throw ThrowHelper.CreateGeneratedFilePathInvalidException(index, filePath, exception);
            }
        }

        private static byte[] GetContentBytes(string content, string destinationPath)
        {
            try
            {
                return s_utf8WithoutBom.GetBytes(content);
            }
            catch (EncoderFallbackException exception)
            {
                throw ThrowHelper.CreateGeneratedContentUnicodeException(destinationPath, exception);
            }
        }

        private void ValidateDestination(string destinationPath, string projectRoot)
        {
            var assetsRoot = Path.Combine(projectRoot, "Assets");
            var packagesRoot = Path.Combine(projectRoot, "Packages");
            string allowedRoot;

            if (IsStrictDescendant(destinationPath, assetsRoot))
            {
                allowedRoot = assetsRoot;
            }
            else if (IsStrictDescendant(destinationPath, packagesRoot))
            {
                allowedRoot = packagesRoot;
            }
            else
            {
                throw ThrowHelper.CreateGeneratedDestinationOutsideOwnedDirectoriesException(destinationPath);
            }

            ValidateExistingPathComponents(projectRoot, allowedRoot, destinationPath);
        }

        private static bool IsStrictDescendant(string path, string root)
        {
            if (path.Length <= root.Length || path.StartsWith(root, s_pathComparison) == false)
            {
                return false;
            }

            var separator = path[root.Length];
            return separator == Path.DirectorySeparatorChar
                || separator == Path.AltDirectorySeparatorChar;
        }

        private void ValidateExistingPathComponents(string projectRoot, string allowedRoot, string destinationPath)
        {
            ThrowHelper.ThrowIfReparsePoint(
                  (_getAttributes(projectRoot) & FileAttributes.ReparsePoint) == 0
                , projectRoot
            );

            if (Directory.Exists(allowedRoot) == false)
            {
                throw ThrowHelper.CreateGeneratedCodeRootMissingException(allowedRoot);
            }

            ThrowHelper.ThrowIfReparsePoint(
                  (_getAttributes(allowedRoot) & FileAttributes.ReparsePoint) == 0
                , allowedRoot
            );
            var relativePath = destinationPath[allowedRoot.Length..]
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var components = relativePath.Split(
                  new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }
                , StringSplitOptions.RemoveEmptyEntries
            );
            var currentPath = allowedRoot;

            for (var i = 0; i < components.Length; i++)
            {
                currentPath = Path.Combine(currentPath, components[i]);
                var isDestination = i == components.Length - 1;

                if (Directory.Exists(currentPath))
                {
                    ThrowHelper.ThrowIfReparsePoint(
                          (_getAttributes(currentPath) & FileAttributes.ReparsePoint) == 0
                        , currentPath
                    );

                    if (isDestination)
                    {
                        throw ThrowHelper.CreateGeneratedDestinationDirectoryException(destinationPath);
                    }

                    continue;
                }

                if (File.Exists(currentPath))
                {
                    ThrowHelper.ThrowIfReparsePoint(
                          (_getAttributes(currentPath) & FileAttributes.ReparsePoint) == 0
                        , currentPath
                    );

                    if (isDestination == false)
                    {
                        throw ThrowHelper.CreateGeneratedDestinationFileParentException(
                            destinationPath,
                            currentPath
                        );
                    }

                    continue;
                }

                break;
            }
        }

        private static byte[] ReadAllBytes(string path, string errorCode)
        {
            try
            {
                return File.ReadAllBytes(path);
            }
            catch (Exception exception)
            {
                throw ThrowHelper.CreateGeneratedCodeReadException(errorCode, path, exception);
            }
        }

        private static void WriteStagedBytes(string stagingPath, byte[] bytes)
        {
            try
            {
                using var stream = new FileStream(stagingPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            catch (Exception exception)
            {
                throw ThrowHelper.CreateGeneratedCodeStagingWriteException(stagingPath, exception);
            }
        }

        private static bool BytesEqual(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            for (var i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void ValidatePredecessors(List<StagedWrite> writes, string projectRoot)
        {
            for (var i = 0; i < writes.Count; i++)
            {
                var write = writes[i];
                ValidateDestination(write.DestinationPath, projectRoot);

                if (write.DestinationExisted)
                {
                    if (File.Exists(write.DestinationPath) == false
                        || BytesEqual(
                            ReadAllBytes(write.DestinationPath, ThrowHelper.WRITE_COMMIT_FAILED),
                            write.OriginalBytes
                        ) == false
                    )
                    {
                        throw ThrowHelper.CreateGeneratedDestinationChangedException(write.DestinationPath);
                    }
                }
                else if (File.Exists(write.DestinationPath) || Directory.Exists(write.DestinationPath))
                {
                    throw ThrowHelper.CreateGeneratedDestinationAppearedException(write.DestinationPath);
                }
            }
        }

        private void Commit(List<StagedWrite> writes, string projectRoot)
        {
            var createdDirectories = new List<string>();
            Exception commitException = null;
            var failureCode = ThrowHelper.WRITE_COMMIT_FAILED;
            var failureMessage = ThrowHelper.WRITE_COMMIT_FAILED_MESSAGE;

            try
            {
                for (var i = 0; i < writes.Count; i++)
                {
                    CreateDestinationDirectory(Path.GetDirectoryName(writes[i].DestinationPath), createdDirectories);
                }

                ValidatePredecessors(writes, projectRoot);

                for (var i = 0; i < writes.Count; i++)
                {
                    var write = writes[i];

                    if (write.DestinationExisted)
                    {
                        _replaceFile(write.StagingPath, write.DestinationPath, write.BackupPath);
                    }
                    else
                    {
                        _moveFile(write.StagingPath, write.DestinationPath);
                    }

                    write.Committed = true;
                }

                try
                {
                    _refreshAssets();
                }
                catch (Exception exception)
                {
                    failureCode = ThrowHelper.WRITE_REFRESH_FAILED;
                    failureMessage = ThrowHelper.WRITE_REFRESH_FAILED_MESSAGE;
                    throw ThrowHelper.CreateAssetRefreshException(exception);
                }

                return;
            }
            catch (Exception exception)
            {
                commitException = exception;
            }

            var rollbackFailures = Rollback(writes, createdDirectories);
            var message = new StringBuilder(failureMessage);

            if (rollbackFailures.Count > 0)
            {
                message.Append(" Rollback also failed for:");

                for (var i = 0; i < rollbackFailures.Count; i++)
                {
                    message.AppendLine();
                    message.Append(rollbackFailures[i]);
                }
            }

            throw ThrowHelper.CreateGeneratedCodeCommitException(failureCode, message.ToString(), commitException);
        }

        private static void CreateDestinationDirectory(string destinationDirectory, List<string> createdDirectories)
        {
            if (string.IsNullOrEmpty(destinationDirectory) || Directory.Exists(destinationDirectory))
            {
                return;
            }

            var missingDirectories = new Stack<string>();
            var currentDirectory = destinationDirectory;

            while (string.IsNullOrEmpty(currentDirectory) == false
                && Directory.Exists(currentDirectory) == false
            )
            {
                if (File.Exists(currentDirectory))
                {
                    throw ThrowHelper.CreateGeneratedCodeDirectoryCollisionException(currentDirectory);
                }

                missingDirectories.Push(currentDirectory);
                currentDirectory = Path.GetDirectoryName(currentDirectory);
            }

            while (missingDirectories.Count > 0)
            {
                var directory = missingDirectories.Pop();
                Directory.CreateDirectory(directory);
                createdDirectories.Add(directory);
            }
        }

        private List<string> Rollback(List<StagedWrite> writes, List<string> createdDirectories)
        {
            var failures = new List<string>();

            for (var i = writes.Count - 1; i >= 0; i--)
            {
                var write = writes[i];

                if (write.Committed == false)
                {
                    continue;
                }

                try
                {
                    if (write.DestinationExisted)
                    {
                        if (File.Exists(write.DestinationPath))
                        {
                            _replaceFile(write.BackupPath, write.DestinationPath, null);
                        }
                        else
                        {
                            _moveFile(write.BackupPath, write.DestinationPath);
                        }
                    }
                    else if (File.Exists(write.DestinationPath))
                    {
                        File.Delete(write.DestinationPath);
                    }
                }
                catch (Exception exception)
                {
                    failures.Add($"{write.DestinationPath}: {exception.Message}");
                }
            }

            for (var i = createdDirectories.Count - 1; i >= 0; i--)
            {
                var directory = createdDirectories[i];

                try
                {
                    if (Directory.Exists(directory)
                        && Directory.GetFileSystemEntries(directory).Length == 0
                    )
                    {
                        Directory.Delete(directory);
                    }
                }
                catch (Exception exception)
                {
                    failures.Add($"{directory}: {exception.Message}");
                }
            }

            return failures;
        }

        private static void DeleteDirectoryBestEffort(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Tracks one staged output and the state required to commit or roll it back.
        /// </summary>
        private sealed class StagedWrite
        {
            /// <summary>
            /// Initializes the paths, original content, and destination state for one staged write.
            /// </summary>
            public StagedWrite(
                  string destinationPath
                , string stagingPath
                , string backupPath
                , bool destinationExisted
                , byte[] originalBytes
            )
            {
                DestinationPath = destinationPath;
                StagingPath = stagingPath;
                BackupPath = backupPath;
                DestinationExisted = destinationExisted;
                OriginalBytes = originalBytes;
            }

            /// <summary>
            /// Gets the canonical generated-file destination.
            /// </summary>
            public string DestinationPath { get; }

            /// <summary>
            /// Gets the temporary file containing the staged content.
            /// </summary>
            public string StagingPath { get; }

            /// <summary>
            /// Gets the temporary backup path used during replacement.
            /// </summary>
            public string BackupPath { get; }

            /// <summary>
            /// Gets whether the destination existed before staging.
            /// </summary>
            public bool DestinationExisted { get; }

            /// <summary>
            /// Gets the destination bytes captured before commit.
            /// </summary>
            public byte[] OriginalBytes { get; }

            /// <summary>
            /// Gets or sets whether this staged write was committed.
            /// </summary>
            public bool Committed { get; set; }
        }
    }
}

#endif
