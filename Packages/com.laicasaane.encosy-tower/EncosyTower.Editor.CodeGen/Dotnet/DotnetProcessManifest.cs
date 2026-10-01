using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Persists and validates ownership information for a temporary .NET process tree.
    /// </summary>
    internal sealed class DotnetProcessManifest
    {
        /// <summary>
        /// Defines the supported process-manifest schema version.
        /// </summary>
        internal const int SCHEMA_VERSION = 1;

        private static readonly UTF8Encoding s_utf8NoBom = new(false);

        /// <summary>
        /// Creates a validated manifest containing the root process.
        /// </summary>
        internal static Document Create(
              string workspaceRoot
            , string operationName
            , string workingDirectory
            , ProcessEntry rootProcess
        )
        {
            var document = new Document(
                  workspaceRoot
                , operationName
                , workingDirectory
                , rootProcess.processId
                , new[] { rootProcess }
            );

            Validate(document, workspaceRoot);
            return document;
        }

        /// <summary>
        /// Reads and validates a process manifest for the expected workspace.
        /// </summary>
        internal static Document Read(string processManifestPath, string expectedWorkspaceRoot)
        {
            string json;

            try
            {
                json = File.ReadAllText(processManifestPath, Encoding.UTF8);
            }
            catch (Exception exception)
            {
                throw ThrowHelper.CreateProcessManifestReadException(processManifestPath, exception);
            }

            Document document;

            try
            {
                document = JsonUtility.FromJson<Document>(json);
            }
            catch (Exception exception)
            {
                throw ThrowHelper.CreateProcessManifestMalformedException(processManifestPath, exception);
            }

            Validate(document, expectedWorkspaceRoot);
            return document;
        }

        /// <summary>
        /// Inserts or replaces a process entry while preserving process-identifier order.
        /// </summary>
        internal static void Upsert(Document document, ProcessEntry process)
        {
            var processes = document.processes;
            var count = processes.Length;
            var entries = new List<ProcessEntry>(count + 1);
            var replaced = false;

            for (var i = 0; i < count; i++)
            {
                var current = processes[i];

                if (current.processId == process.processId)
                {
                    entries.Add(process);
                    replaced = true;
                }
                else
                {
                    entries.Add(current);
                }
            }

            if (replaced == false)
            {
                entries.Add(process);
            }

            entries.Sort(static (left, right) => left.processId.CompareTo(right.processId));
            document.processes = entries.ToArray();
        }

        /// <summary>
        /// Validates and atomically writes a process manifest.
        /// </summary>
        internal static void WriteAtomic(string processManifestPath, Document document, string expectedWorkspaceRoot)
        {
            Validate(document, expectedWorkspaceRoot);

            var directoryPath = Path.GetDirectoryName(processManifestPath);

            if (string.IsNullOrEmpty(directoryPath))
            {
                throw ThrowHelper.CreateProcessManifestPathException(processManifestPath);
            }

            Directory.CreateDirectory(directoryPath);

            var temporaryPath = processManifestPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            var backupPath = processManifestPath + "." + Guid.NewGuid().ToString("N") + ".bak";

            try
            {
                var json = JsonUtility.ToJson(document, prettyPrint: true);
                File.WriteAllText(temporaryPath, json + "\n", s_utf8NoBom);

                if (File.Exists(processManifestPath))
                {
                    File.Replace(temporaryPath, processManifestPath, backupPath);
                    File.Delete(backupPath);
                }
                else
                {
                    File.Move(temporaryPath, processManifestPath);
                }
            }
            catch (Exception exception)
            {
                TryDelete(temporaryPath);
                TryDelete(backupPath);

                throw ThrowHelper.CreateProcessManifestWriteException(processManifestPath, exception);
            }
        }

        /// <summary>
        /// Deletes a completed process manifest.
        /// </summary>
        internal static void Delete(string processManifestPath)
        {
            try
            {
                File.Delete(processManifestPath);
            }
            catch (Exception exception)
            {
                throw ThrowHelper.CreateProcessManifestDeleteException(processManifestPath, exception);
            }
        }

        private static void Validate(Document document, string expectedWorkspaceRoot)
        {
            if (document == null)
            {
                goto MALFORMED;
            }

            if (document.schemaVersion != SCHEMA_VERSION)
            {
                throw ThrowHelper.CreateProcessManifestVersionException(document.schemaVersion);
            }

            if (string.IsNullOrWhiteSpace(document.workspaceRoot)
                || string.IsNullOrWhiteSpace(document.operationName)
                || string.IsNullOrWhiteSpace(document.workingDirectory)
                || document.rootProcessId <= 0
                || document.processes == null
                || document.processes.Length == 0
            )
            {
                goto MALFORMED;
            }

            var workspaceRoot = NormalizeDirectory(document.workspaceRoot);
            var expectedRoot = NormalizeDirectory(expectedWorkspaceRoot);

            if (PathsEqual(workspaceRoot, expectedRoot) == false)
            {
                throw ThrowHelper.CreateProcessManifestWorkspaceLockedException(workspaceRoot, expectedRoot);
            }

            var workingDirectory = NormalizeDirectory(document.workingDirectory);

            if (IsSameOrChild(workingDirectory, workspaceRoot) == false)
            {
                goto MALFORMED;
            }

            var foundRoot = false;
            var previousProcessId = 0;
            var count = document.processes.Length;

            for (var i = 0; i < count; i++)
            {
                var process = document.processes[i];

                if (process == null
                    || process.processId <= previousProcessId
                    || string.IsNullOrWhiteSpace(process.executablePath)
                    || string.IsNullOrWhiteSpace(process.commandLine)
                    || process.startTimeUtcTicks <= 0
                )
                {
                    goto MALFORMED;
                }

                if (process.processId == document.rootProcessId)
                {
                    foundRoot = true;
                }

                previousProcessId = process.processId;
            }

            if (foundRoot)
            {
                return;
            }

        MALFORMED:
            throw ThrowHelper.CreateProcessManifestIdentityMalformedException();
        }

        private static string NormalizeDirectory(string path)
            => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        private static bool PathsEqual(string left, string right)
            => string.Equals(left, right, PathComparison);

        private static bool IsSameOrChild(string path, string root)
        {
            if (PathsEqual(path, root))
            {
                return true;
            }

            return path.StartsWith(root + Path.DirectorySeparatorChar, PathComparison);
        }

        private static StringComparison PathComparison
            => Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        private static void TryDelete(string path)
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
        /// Defines the serialized identity and membership of one owned process tree.
        /// </summary>
        [Serializable]
        internal sealed class Document
        {
            /// <summary>
            /// The process-manifest schema version.
            /// </summary>
            public int schemaVersion;

            /// <summary>
            /// The canonical temporary workspace root.
            /// </summary>
            public string workspaceRoot;

            /// <summary>
            /// The logical operation represented by the process tree.
            /// </summary>
            public string operationName;

            /// <summary>
            /// The working directory shared with the root process.
            /// </summary>
            public string workingDirectory;

            /// <summary>
            /// The identifier of the root process.
            /// </summary>
            public int rootProcessId;

            /// <summary>
            /// The verified processes recorded for this operation.
            /// </summary>
            public ProcessEntry[] processes;

            /// <summary>
            /// Initializes an empty document for Unity JSON deserialization.
            /// </summary>
            public Document()
            {
            }

            /// <summary>
            /// Initializes a complete process-tree document.
            /// </summary>
            public Document(
                  string workspaceRoot
                , string operationName
                , string workingDirectory
                , int rootProcessId
                , ProcessEntry[] processes
            )
            {
                schemaVersion = SCHEMA_VERSION;
                this.workspaceRoot = workspaceRoot;
                this.operationName = operationName;
                this.workingDirectory = workingDirectory;
                this.rootProcessId = rootProcessId;
                this.processes = processes;
            }
        }

        /// <summary>
        /// Defines the identity evidence used to verify one recorded process.
        /// </summary>
        [Serializable]
        internal sealed class ProcessEntry
        {
            /// <summary>
            /// The operating-system process identifier.
            /// </summary>
            public int processId;

            /// <summary>
            /// The canonical path of the process executable.
            /// </summary>
            public string executablePath;

            /// <summary>
            /// The command line captured for workspace association checks.
            /// </summary>
            public string commandLine;

            /// <summary>
            /// The process start time expressed as UTC ticks.
            /// </summary>
            public long startTimeUtcTicks;

            /// <summary>
            /// Initializes an empty entry for Unity JSON deserialization.
            /// </summary>
            public ProcessEntry()
            {
            }

            /// <summary>
            /// Initializes a complete process identity entry.
            /// </summary>
            public ProcessEntry(int processId, string executablePath, string commandLine, long startTimeUtcTicks)
            {
                this.processId = processId;
                this.executablePath = executablePath;
                this.commandLine = commandLine;
                this.startTimeUtcTicks = startTimeUtcTicks;
            }
        }
    }
}
