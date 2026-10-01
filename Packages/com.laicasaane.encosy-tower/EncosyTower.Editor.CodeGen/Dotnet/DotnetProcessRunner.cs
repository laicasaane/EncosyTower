using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Runs an owned .NET process tree with verified cancellation, logging, and cleanup.
    /// </summary>
    internal sealed class DotnetProcessRunner
    {
        private const int POLL_DELAY_MILLISECONDS = 25;
        private const int OUTPUT_DRAIN_TIMEOUT_MILLISECONDS = 2000;

        private static readonly UTF8Encoding s_utf8NoBom = new(false);

        /// <summary>
        /// Verifies and stops the process tree recorded for a prior workspace operation.
        /// </summary>
        internal async Task StopRecordedProcessTreeAsync(
              string workspaceRoot
            , string processManifestPath
            , CancellationToken token = default
        )
        {
            var normalizedRoot = ValidateManifestPath(workspaceRoot, processManifestPath);
            token.ThrowIfCancellationRequested();

            if (File.Exists(processManifestPath) == false)
            {
                return;
            }

            var document = DotnetProcessManifest.Read(processManifestPath, normalizedRoot);
            await StopDocumentProcessTreeAsync(document, processManifestPath, normalizedRoot, token);
        }

        /// <summary>
        /// Runs a process request, streams its output, and tracks all verified descendants.
        /// </summary>
        internal async Task<DotnetProcessResult> RunAsync(
              DotnetProcessRequest request
            , Action<DotnetProcessOutput> onOutput
            , CancellationToken token = default
        )
        {
            var workspaceRoot = ValidateRequest(request, onOutput);
            token.ThrowIfCancellationRequested();

            if (File.Exists(request.ProcessManifestPath))
            {
                throw ThrowHelper.CreateWorkspaceLockedByManifestException(request.ProcessManifestPath);
            }

            PrepareLogFile(request.StandardOutputLogPath);
            PrepareLogFile(request.StandardErrorLogPath);

            using var standardOutputLog = CreateLogWriter(request.StandardOutputLogPath);
            using var standardErrorLog = CreateLogWriter(request.StandardErrorLogPath);
            using var process = new Process {
                StartInfo = CreateStartInfo(request),
                EnableRaisingEvents = true,
            };

            var outputQueue = new ConcurrentQueue<OutputEvent>();

            void StandardOutputHandler(object _, DataReceivedEventArgs args)
                => outputQueue.Enqueue(new OutputEvent(IsStandardError: false, args.Data));

            void StandardErrorHandler(object _, DataReceivedEventArgs args)
                => outputQueue.Enqueue(new OutputEvent(IsStandardError: true, args.Data));

            process.OutputDataReceived += StandardOutputHandler;
            process.ErrorDataReceived += StandardErrorHandler;

            DotnetProcessManifest.Document document = null;
            var standardOutputEnded = false;
            var standardErrorEnded = false;
            var processId = 0;
            var exitCode = 0;

            try
            {
                if (process.Start() == false)
                {
                    throw ThrowHelper.CreateProcessStartException(request.ExecutablePath);
                }

                processId = process.Id;
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var rootEntry = CaptureRootProcess(process, request, workspaceRoot);
                document = DotnetProcessManifest.Create(
                      workspaceRoot
                    , request.OperationName
                    , Path.GetFullPath(request.WorkingDirectory)
                    , rootEntry
                );
                DotnetProcessManifest.WriteAtomic(request.ProcessManifestPath, document, workspaceRoot);

                DateTime? allProcessesExitedAt = null;

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        await StopDocumentProcessTreeAsync(
                              document
                            , request.ProcessManifestPath
                            , workspaceRoot
                            , CancellationToken.None
                        );
                        token.ThrowIfCancellationRequested();
                    }

                    RefreshDescendantManifest(document, request.ProcessManifestPath, workspaceRoot);
                    DrainOutput(
                          outputQueue
                        , standardOutputLog
                        , standardErrorLog
                        , onOutput
                        , ref standardOutputEnded
                        , ref standardErrorEnded
                    );

                    var rootExited = HasExited(process);
                    var anyRecordedProcessAlive = HasAnyRecordedProcessAlive(document, workspaceRoot);

                    if (rootExited && anyRecordedProcessAlive == false)
                    {
                        allProcessesExitedAt ??= DateTime.UtcNow;

                        if ((standardOutputEnded && standardErrorEnded)
                            || DateTime.UtcNow - allProcessesExitedAt.Value
                                >= TimeSpan.FromMilliseconds(OUTPUT_DRAIN_TIMEOUT_MILLISECONDS)
                        )
                        {
                            exitCode = process.ExitCode;
                            break;
                        }
                    }
                    else
                    {
                        allProcessesExitedAt = null;
                    }

                    await Task.Delay(POLL_DELAY_MILLISECONDS);
                }

                DrainOutput(
                      outputQueue
                    , standardOutputLog
                    , standardErrorLog
                    , onOutput
                    , ref standardOutputEnded
                    , ref standardErrorEnded
                );
                DotnetProcessManifest.Delete(request.ProcessManifestPath);

                return new DotnetProcessResult(processId, exitCode);
            }
            catch
            {
                if (document != null && File.Exists(request.ProcessManifestPath))
                {
                    await StopDocumentProcessTreeAsync(
                          document
                        , request.ProcessManifestPath
                        , workspaceRoot
                        , CancellationToken.None
                    );
                }
                else if (processId > 0)
                {
                    TryKill(process);
                    await WaitForExitAsync(process);
                }

                throw;
            }
            finally
            {
                TryCancelOutputRead(process);
                process.OutputDataReceived -= StandardOutputHandler;
                process.ErrorDataReceived -= StandardErrorHandler;

                DrainOutput(
                      outputQueue
                    , standardOutputLog
                    , standardErrorLog
                    , onOutput
                    , ref standardOutputEnded
                    , ref standardErrorEnded
                );
            }
        }

        /// <summary>
        /// Creates a shell-free, redirected process start configuration for a request.
        /// </summary>
        internal static ProcessStartInfo CreateStartInfo(DotnetProcessRequest request)
        {
            var startInfo = new ProcessStartInfo {
                FileName = Path.GetFullPath(request.ExecutablePath),
                WorkingDirectory = Path.GetFullPath(request.WorkingDirectory),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            var arguments = request.Arguments;
            var count = arguments.Length;

            for (var i = 0; i < count; i++)
            {
                startInfo.ArgumentList.Add(arguments[i]);
            }

            return startInfo;
        }

        private static async Task StopDocumentProcessTreeAsync(
              DotnetProcessManifest.Document document
            , string processManifestPath
            , string workspaceRoot
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var liveProcesses = CaptureMatchingLiveProcesses(document, workspaceRoot);

            if (liveProcesses.Count == 0)
            {
                DotnetProcessManifest.Delete(processManifestPath);
                return;
            }

            var count = liveProcesses.Count;

            try
            {
                for (var i = 0; i < count; i++)
                {
                    var process = liveProcesses[i];

                    if (process.Id == document.rootProcessId)
                    {
                        continue;
                    }

                    TryKill(process);
                }

                for (var i = 0; i < count; i++)
                {
                    var process = liveProcesses[i];

                    if (process.Id != document.rootProcessId)
                    {
                        continue;
                    }

                    TryKill(process);
                    break;
                }

                await WaitForCapturedProcessesToExitAsync(liveProcesses, token);
                DotnetProcessManifest.Delete(processManifestPath);
            }
            finally
            {
                for (var i = 0; i < count; i++)
                {
                    liveProcesses[i].Dispose();
                }
            }
        }

        private static List<Process> CaptureMatchingLiveProcesses(
              DotnetProcessManifest.Document document
            , string workspaceRoot
        )
        {
            var liveProcesses = new List<Process>(document.processes.Length);

            try
            {
                var entries = document.processes;
                var count = entries.Length;

                for (var i = 0; i < count; i++)
                {
                    var recorded = entries[i];

                    if (TryOpenCapturedProcess(
                              recorded.processId
                            , workspaceRoot
                            , out var liveProcess
                            , out var actual
                        ) == false
                    )
                    {
                        continue;
                    }

                    if (Matches(recorded, actual) == false)
                    {
                        liveProcess.Dispose();

                        throw ThrowHelper.CreateRecordedProcessIdentityMismatchException(recorded.processId);
                    }

                    liveProcesses.Add(liveProcess);
                }

                return liveProcesses;
            }
            catch
            {
                var count = liveProcesses.Count;

                for (var i = 0; i < count; i++)
                {
                    liveProcesses[i].Dispose();
                }

                throw;
            }
        }

        private static async Task WaitForCapturedProcessesToExitAsync(List<Process> processes, CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                var anyAlive = false;
                var count = processes.Count;

                for (var i = 0; i < count; i++)
                {
                    if (HasExited(processes[i]) == false)
                    {
                        anyAlive = true;
                        break;
                    }
                }

                if (anyAlive == false)
                {
                    return;
                }

                await Task.Delay(POLL_DELAY_MILLISECONDS, token);
            }
        }

        private static async Task WaitForExitAsync(Process process)
        {
            while (HasExited(process) == false)
            {
                await Task.Delay(POLL_DELAY_MILLISECONDS);
            }
        }

        private static void RefreshDescendantManifest(
              DotnetProcessManifest.Document document
            , string processManifestPath
            , string workspaceRoot
        )
        {
            var knownProcessIds = new HashSet<int>();
            var entries = document.processes;
            var count = entries.Length;

            for (var i = 0; i < count; i++)
            {
                knownProcessIds.Add(entries[i].processId);
            }

            var changed = false;
            var searching = true;

            while (searching)
            {
                searching = false;
                var processes = Process.GetProcesses();
                var processCount = processes.Length;

                try
                {
                    for (var i = 0; i < processCount; i++)
                    {
                        var process = processes[i];
                        var processId = process.Id;

                        if (knownProcessIds.Contains(processId)
                            || TryGetParentProcessId(processId, out var parentProcessId) == false
                            || knownProcessIds.Contains(parentProcessId) == false
                        )
                        {
                            continue;
                        }

                        if (TryCaptureProcess(
                                  process
                                , workspaceRoot
                                , out var captured
                                , workspaceMismatchIsError: false
                            ) == false
                        )
                        {
                            continue;
                        }

                        DotnetProcessManifest.Upsert(document, captured);
                        knownProcessIds.Add(processId);
                        changed = true;
                        searching = true;
                    }
                }
                finally
                {
                    for (var i = 0; i < processCount; i++)
                    {
                        processes[i].Dispose();
                    }
                }
            }

            if (changed)
            {
                DotnetProcessManifest.WriteAtomic(processManifestPath, document, workspaceRoot);
            }
        }

        private static bool HasAnyRecordedProcessAlive(DotnetProcessManifest.Document document, string workspaceRoot)
        {
            var entries = document.processes;
            var count = entries.Length;

            for (var i = 0; i < count; i++)
            {
                var recorded = entries[i];

                if (TryCaptureProcess(recorded.processId, workspaceRoot, out var actual) == false)
                {
                    continue;
                }

                if (Matches(recorded, actual) == false)
                {
                    throw ThrowHelper.CreateRecordedProcessIdentityMismatchException(recorded.processId);
                }

                return true;
            }

            return false;
        }

        private static DotnetProcessManifest.ProcessEntry CaptureRootProcess(
              Process process
            , DotnetProcessRequest request
            , string workspaceRoot
        )
        {
            try
            {
                if (TryCaptureProcess(process, workspaceRoot, out var entry))
                {
                    return entry;
                }
            }
            catch (CodeGenRunException) when (HasExited(process))
            {
                // The started root is already gone; retain its known launch identity below.
            }

            if (HasExited(process) == false)
            {
                throw ThrowHelper.CreateProcessIdentityCaptureException(process.Id);
            }

            return new DotnetProcessManifest.ProcessEntry(
                  process.Id
                , Path.GetFullPath(request.ExecutablePath)
                , CreateRecordedCommandLine(request)
                , process.StartTime.ToUniversalTime().Ticks
            );
        }

        private static bool TryCaptureProcess(
              int processId
            , string workspaceRoot
            , out DotnetProcessManifest.ProcessEntry entry
        )
        {
            if (TryOpenCapturedProcess(processId, workspaceRoot, out var process, out entry) == false)
            {
                return false;
            }

            process.Dispose();
            return true;
        }

        private static bool TryOpenCapturedProcess(
              int processId
            , string workspaceRoot
            , out Process process
            , out DotnetProcessManifest.ProcessEntry entry
        )
        {
            process = null;
            entry = null;
            Process candidate = null;

            try
            {
                candidate = Process.GetProcessById(processId);

                if (TryCaptureProcess(candidate, workspaceRoot, out entry) == false)
                {
                    return false;
                }

                process = candidate;
                candidate = null;
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (Win32Exception exception) when (
                exception.NativeErrorCode == 87
                || exception.NativeErrorCode == 1168
            )
            {
                return false;
            }
            catch (Win32Exception exception)
            {
                throw ThrowHelper.CreateProcessIdentityInspectionException(processId, exception);
            }
            finally
            {
                candidate?.Dispose();
            }
        }

        private static bool TryCaptureProcess(
              Process process
            , string workspaceRoot
            , out DotnetProcessManifest.ProcessEntry entry
            , bool workspaceMismatchIsError = true
        )
        {
            entry = null;

            try
            {
                if (process.HasExited)
                {
                    return false;
                }

                var processId = process.Id;
                var executablePath = GetExecutablePath(process);
                var commandLine = GetCommandLine(processId, process);
                var startTimeUtcTicks = process.StartTime.ToUniversalTime().Ticks;

                if (string.IsNullOrWhiteSpace(executablePath)
                    || string.IsNullOrWhiteSpace(commandLine)
                    || CommandLineContainsWorkspace(commandLine, workspaceRoot) == false
                )
                {
                    if (workspaceMismatchIsError == false)
                    {
                        return false;
                    }

                    throw ThrowHelper.CreateProcessWorkspaceIdentityMissingException(processId);
                }

                entry = new DotnetProcessManifest.ProcessEntry(
                      processId
                    , Path.GetFullPath(executablePath)
                    , commandLine
                    , startTimeUtcTicks
                );
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (IOException) when (HasExited(process))
            {
                return false;
            }
            catch (IOException exception)
            {
                throw ThrowHelper.CreateProcessIdentityInspectionException(process.Id, exception);
            }
            catch (UnauthorizedAccessException) when (HasExited(process))
            {
                return false;
            }
            catch (UnauthorizedAccessException exception)
            {
                throw ThrowHelper.CreateProcessIdentityInspectionException(process.Id, exception);
            }
            catch (Win32Exception) when (HasExited(process))
            {
                return false;
            }
            catch (Win32Exception exception) when (
                exception.NativeErrorCode == 87
                || exception.NativeErrorCode == 1168
            )
            {
                return false;
            }
            catch (Win32Exception exception)
            {
                throw ThrowHelper.CreateProcessIdentityInspectionException(process.Id, exception);
            }
        }

        private static bool Matches(
              DotnetProcessManifest.ProcessEntry recorded
            , DotnetProcessManifest.ProcessEntry actual
        )
            => recorded.processId == actual.processId
            && PathsEqual(recorded.executablePath, actual.executablePath)
            && string.Equals(recorded.commandLine, actual.commandLine, StringComparison.Ordinal)
            && recorded.startTimeUtcTicks == actual.startTimeUtcTicks
            ;

        private static void DrainOutput(
              ConcurrentQueue<OutputEvent> outputQueue
            , StreamWriter standardOutputLog
            , StreamWriter standardErrorLog
            , Action<DotnetProcessOutput> onOutput
            , ref bool standardOutputEnded
            , ref bool standardErrorEnded
        )
        {
            while (outputQueue.TryDequeue(out var output))
            {
                if (output.IsEnd)
                {
                    if (output.IsStandardError)
                    {
                        standardErrorEnded = true;
                    }
                    else
                    {
                        standardOutputEnded = true;
                    }

                    continue;
                }

                var writer = output.IsStandardError ? standardErrorLog : standardOutputLog;
                writer.WriteLine(output.Text);
                onOutput(new DotnetProcessOutput(output.IsStandardError, output.Text));
            }

            standardOutputLog.Flush();
            standardErrorLog.Flush();
        }

        private static string ValidateRequest(DotnetProcessRequest request, Action<DotnetProcessOutput> onOutput)
        {
            if (request == null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(request));
            }

            if (onOutput == null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(onOutput));
            }

            if (string.IsNullOrWhiteSpace(request.OperationName)
                || string.IsNullOrWhiteSpace(request.ExecutablePath)
                || string.IsNullOrWhiteSpace(request.WorkingDirectory)
                || string.IsNullOrWhiteSpace(request.StandardOutputLogPath)
                || string.IsNullOrWhiteSpace(request.StandardErrorLogPath)
                || string.IsNullOrWhiteSpace(request.ProcessManifestPath)
                || request.Arguments == null
            )
            {
                throw ThrowHelper.CreateProcessRequestMissingValuesException();
            }

            var executablePath = Path.GetFullPath(request.ExecutablePath);

            if (Path.IsPathFullyQualified(request.ExecutablePath) == false
                || File.Exists(executablePath) == false
            )
            {
                throw ThrowHelper.CreateDotnetExecutableMissingException(executablePath);
            }

            var workspaceRoot = ValidateManifestPath(request.WorkingDirectory, request.ProcessManifestPath);
            ValidateOwnedPath(request.StandardOutputLogPath, workspaceRoot, "stdout log");
            ValidateOwnedPath(request.StandardErrorLogPath, workspaceRoot, "stderr log");

            if (PathsEqual(request.StandardOutputLogPath, request.StandardErrorLogPath)
                || PathsEqual(request.StandardOutputLogPath, request.ProcessManifestPath)
                || PathsEqual(request.StandardErrorLogPath, request.ProcessManifestPath)
            )
            {
                throw ThrowHelper.CreateProcessArtifactPathsNotDistinctException();
            }

            var arguments = request.Arguments;
            var count = arguments.Length;

            for (var i = 0; i < count; i++)
            {
                if (arguments[i] == null)
                {
                    throw ThrowHelper.CreateProcessArgumentNullException(i, request.OperationName);
                }
            }

            return workspaceRoot;
        }

        private static string ValidateManifestPath(string workspaceRoot, string processManifestPath)
        {
            if (string.IsNullOrWhiteSpace(workspaceRoot) || string.IsNullOrWhiteSpace(processManifestPath))
            {
                throw ThrowHelper.CreateProcessManifestPathsRequiredException();
            }

            var normalizedRoot = NormalizeDirectory(workspaceRoot);
            ValidateOwnedPath(processManifestPath, normalizedRoot, "process manifest");
            return normalizedRoot;
        }

        private static void ValidateOwnedPath(string path, string workspaceRoot, string role)
        {
            var normalizedPath = Path.GetFullPath(path);

            if (IsSameOrChild(normalizedPath, workspaceRoot) == false)
            {
                throw ThrowHelper.CreateProcessPathEscapeException(role, normalizedPath, workspaceRoot);
            }
        }

        private static ProcessStartInfo CreateIdentityStartInfo(string executablePath)
            => new() {
                FileName = executablePath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

        private static string GetExecutablePath(Process process)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var builder = new StringBuilder(32768);
                var size = builder.Capacity;

                if (QueryFullProcessImageName(process.Handle, 0, builder, ref size))
                {
                    return builder.ToString();
                }
            }

            return process.MainModule?.FileName;
        }

        private static string GetCommandLine(int processId, Process process)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsCommandLine(process);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetLinuxCommandLine(processId);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return GetMacCommandLine(processId);
            }

            throw ThrowHelper.CreateProcessIdentityPlatformUnsupportedException();
        }

        private static string CreateRecordedCommandLine(DotnetProcessRequest request)
        {
            var builder = new StringBuilder();
            AppendRecordedArgument(builder, Path.GetFullPath(request.ExecutablePath));
            var arguments = request.Arguments;
            var count = arguments.Length;

            for (var i = 0; i < count; i++)
            {
                builder.Append(' ');
                AppendRecordedArgument(builder, arguments[i]);
            }

            return builder.ToString();
        }

        private static void AppendRecordedArgument(StringBuilder builder, string argument)
        {
            builder.Append('"');

            for (var i = 0; i < argument.Length; i++)
            {
                var character = argument[i];

                if (character == '\\' || character == '"')
                {
                    builder.Append('\\');
                }

                builder.Append(character);
            }

            builder.Append('"');
        }

        private static string GetWindowsCommandLine(Process process)
        {
            var status = NtQueryInformationProcess(
                  process.Handle
                , processInformationClass: 60
                , IntPtr.Zero
                , processInformationLength: 0
                , out var length
            );

            if (length <= 0)
            {
                throw ThrowHelper.CreateProcessCommandLineLengthQueryException(status);
            }

            var buffer = Marshal.AllocHGlobal(length);

            try
            {
                status = NtQueryInformationProcess(process.Handle, processInformationClass: 60, buffer, length, out _);

                if (status != 0)
                {
                    throw ThrowHelper.CreateProcessCommandLineQueryException(status);
                }

                var value = Marshal.PtrToStructure<UnicodeString>(buffer);
                return Marshal.PtrToStringUni(value.buffer, value.length / sizeof(char));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static string GetLinuxCommandLine(int processId)
        {
            var bytes = File.ReadAllBytes($"/proc/{processId}/cmdline");
            var builder = new StringBuilder(bytes.Length);
            var start = 0;

            for (var i = 0; i <= bytes.Length; i++)
            {
                if (i < bytes.Length && bytes[i] != 0)
                {
                    continue;
                }

                if (i > start)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(' ');
                    }

                    builder.Append(Encoding.UTF8.GetString(bytes, start, i - start));
                }

                start = i + 1;
            }

            return builder.ToString();
        }

        private static string GetMacCommandLine(int processId)
        {
            var name = new[] { 1, 49, processId };
            var length = UIntPtr.Zero;

            if (Sysctl(name, (uint)name.Length, null, ref length, IntPtr.Zero, UIntPtr.Zero) != 0
                || length == UIntPtr.Zero
            )
            {
                throw ThrowHelper.CreateMacProcessCommandLineQueryException(Marshal.GetLastWin32Error());
            }

            var bytes = new byte[(int)length.ToUInt64()];

            if (Sysctl(name, (uint)name.Length, bytes, ref length, IntPtr.Zero, UIntPtr.Zero) != 0)
            {
                throw ThrowHelper.CreateMacProcessCommandLineQueryException(Marshal.GetLastWin32Error());
            }

            var argumentCount = BitConverter.ToInt32(bytes, 0);
            var index = sizeof(int);

            while (index < bytes.Length && bytes[index] != 0)
            {
                index++;
            }

            while (index < bytes.Length && bytes[index] == 0)
            {
                index++;
            }

            var builder = new StringBuilder();

            for (
                var argumentIndex = 0;
                argumentIndex < argumentCount && index < bytes.Length;
                argumentIndex++
            )
            {
                var start = index;

                while (index < bytes.Length && bytes[index] != 0)
                {
                    index++;
                }

                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(Encoding.UTF8.GetString(bytes, start, index - start));

                while (index < bytes.Length && bytes[index] == 0)
                {
                    index++;
                }
            }

            return builder.ToString();
        }

        private static bool TryGetParentProcessId(int processId, out int parentProcessId)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return TryGetWindowsParentProcessId(processId, out parentProcessId);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return TryGetLinuxParentProcessId(processId, out parentProcessId);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return TryGetMacParentProcessId(processId, out parentProcessId);
            }

            parentProcessId = 0;
            return false;
        }

        private static bool TryGetWindowsParentProcessId(int processId, out int parentProcessId)
        {
            parentProcessId = 0;

            try
            {
                using var process = Process.GetProcessById(processId);
                var information = new ProcessBasicInformation();
                var status = NtQueryInformationProcess(
                      process.Handle
                    , processInformationClass: 0
                    , ref information
                    , Marshal.SizeOf<ProcessBasicInformation>()
                    , out _
                );

                if (status != 0)
                {
                    return false;
                }

                parentProcessId = information.inheritedFromUniqueProcessId.ToInt32();
                return parentProcessId > 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetLinuxParentProcessId(int processId, out int parentProcessId)
        {
            parentProcessId = 0;

            try
            {
                var text = File.ReadAllText($"/proc/{processId}/stat");
                var commandEnd = text.LastIndexOf(')');

                if (commandEnd < 0 || commandEnd + 2 >= text.Length)
                {
                    return false;
                }

                var fields = text[(commandEnd + 2)..].Split(' ');
                return fields.Length > 1
                    && int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out parentProcessId);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetMacParentProcessId(int processId, out int parentProcessId)
        {
            parentProcessId = 0;
            var size = Marshal.SizeOf<ProcessBsdInformation>();
            var buffer = Marshal.AllocHGlobal(size);

            try
            {
                if (ProcPidInfo(processId, 3, 0, buffer, size) != size)
                {
                    return false;
                }

                var information = Marshal.PtrToStructure<ProcessBsdInformation>(buffer);
                parentProcessId = (int)information.parentProcessId;
                return parentProcessId > 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static bool CommandLineContainsWorkspace(string commandLine, string workspaceRoot)
        {
            var normalizedCommandLine = commandLine.Replace('\\', '/');
            var normalizedRoot = NormalizeDirectory(workspaceRoot).Replace('\\', '/');
            return normalizedCommandLine.IndexOf(normalizedRoot, PathComparison) >= 0;
        }

        private static bool HasExited(Process process)
        {
            try
            {
                return process.HasExited;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }

        private static void TryKill(Process process)
        {
            try
            {
                if (process.HasExited == false)
                {
                    process.Kill();
                }
            }
            catch (ArgumentException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                return;
            }
            catch (Win32Exception exception) when (
                exception.NativeErrorCode == 87
                || exception.NativeErrorCode == 1168
            )
            {
                return;
            }
            catch (Win32Exception exception)
            {
                throw ThrowHelper.CreateProcessTerminateException(process.Id, exception);
            }
        }

        private static void TryCancelOutputRead(Process process)
        {
            TryCancelStandardOutputRead(process);
            TryCancelStandardErrorRead(process);
        }

        private static void TryCancelStandardOutputRead(Process process)
        {
            try
            {
                process.CancelOutputRead();
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        private static void TryCancelStandardErrorRead(Process process)
        {
            try
            {
                process.CancelErrorRead();
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        private static void PrepareLogFile(string path)
        {
            var directoryPath = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(path, string.Empty, s_utf8NoBom);
        }

        private static StreamWriter CreateLogWriter(string path)
            => new(path, append: true, s_utf8NoBom) {
                AutoFlush = false,
            };

        private static string NormalizeDirectory(string path)
            => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        private static bool PathsEqual(string left, string right)
            => string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), PathComparison);

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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryFullProcessImageName(
              IntPtr process
            , int flags
            , StringBuilder executablePath
            , ref int size
        );

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(
              IntPtr process
            , int processInformationClass
            , IntPtr processInformation
            , int processInformationLength
            , out int returnLength
        );

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(
              IntPtr process
            , int processInformationClass
            , ref ProcessBasicInformation processInformation
            , int processInformationLength
            , out int returnLength
        );

        [DllImport("libc", EntryPoint = "sysctl", SetLastError = true)]
        private static extern int Sysctl(
              int[] name
            , uint nameLength
            , byte[] oldValue
            , ref UIntPtr oldValueLength
            , IntPtr newValue
            , UIntPtr newValueLength
        );

        [DllImport("libproc", EntryPoint = "proc_pidinfo")]
        private static extern int ProcPidInfo(int processId, int flavor, ulong argument, IntPtr buffer, int bufferSize);

        /// <summary>
        /// Mirrors the Windows native Unicode string descriptor.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct UnicodeString
        {
            /// <summary>
            /// The current string length in bytes.
            /// </summary>
            public ushort length;

            /// <summary>
            /// The buffer capacity in bytes.
            /// </summary>
            public ushort maximumLength;

            /// <summary>
            /// The unmanaged character buffer.
            /// </summary>
            public IntPtr buffer;
        }

        /// <summary>
        /// Mirrors the Windows process information returned by <c>NtQueryInformationProcess</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessBasicInformation
        {
            /// <summary>
            /// The first native reserved slot.
            /// </summary>
            public IntPtr reserved1;

            /// <summary>
            /// The address of the process environment block.
            /// </summary>
            public IntPtr pebBaseAddress;

            /// <summary>
            /// The first slot of the native reserved pointer pair.
            /// </summary>
            public IntPtr reserved2A;

            /// <summary>
            /// The second slot of the native reserved pointer pair.
            /// </summary>
            public IntPtr reserved2B;

            /// <summary>
            /// The native process identifier.
            /// </summary>
            public IntPtr uniqueProcessId;

            /// <summary>
            /// The native parent-process identifier.
            /// </summary>
            public IntPtr inheritedFromUniqueProcessId;
        }

        /// <summary>
        /// Mirrors the macOS process information returned by <c>proc_pidinfo</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct ProcessBsdInformation
        {
            /// <summary>
            /// The native process flags.
            /// </summary>
            public uint flags;

            /// <summary>
            /// The native process status.
            /// </summary>
            public uint status;

            /// <summary>
            /// The native process exit status.
            /// </summary>
            public uint exitStatus;

            /// <summary>
            /// The process identifier.
            /// </summary>
            public uint processId;

            /// <summary>
            /// The parent-process identifier.
            /// </summary>
            public uint parentProcessId;

            /// <summary>
            /// The effective user identifier.
            /// </summary>
            public uint userId;

            /// <summary>
            /// The effective group identifier.
            /// </summary>
            public uint groupId;

            /// <summary>
            /// The real user identifier.
            /// </summary>
            public uint realUserId;

            /// <summary>
            /// The real group identifier.
            /// </summary>
            public uint realGroupId;

            /// <summary>
            /// The saved user identifier.
            /// </summary>
            public uint savedUserId;

            /// <summary>
            /// The saved group identifier.
            /// </summary>
            public uint savedGroupId;

            /// <summary>
            /// The short command name.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string command;

            /// <summary>
            /// The process name.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string name;

            /// <summary>
            /// The number of open files.
            /// </summary>
            public uint fileCount;

            /// <summary>
            /// The process-group identifier.
            /// </summary>
            public uint processGroupId;

            /// <summary>
            /// The job-control counter.
            /// </summary>
            public uint jobControlCount;

            /// <summary>
            /// The controlling terminal device.
            /// </summary>
            public uint terminalDevice;

            /// <summary>
            /// The controlling terminal's process-group identifier.
            /// </summary>
            public uint terminalProcessGroupId;

            /// <summary>
            /// The process scheduling priority adjustment.
            /// </summary>
            public int nice;

            /// <summary>
            /// The whole seconds of the process start time.
            /// </summary>
            public ulong startTimeSeconds;

            /// <summary>
            /// The fractional microseconds of the process start time.
            /// </summary>
            public ulong startTimeMicroseconds;
        }

        /// <summary>
        /// Represents one queued process stream event, including end-of-stream markers.
        /// </summary>
        /// <param name="IsStandardError">Whether the event came from standard error.</param>
        /// <param name="Text">The output text, or <see langword="null"/> at end of stream.</param>
        private readonly record struct OutputEvent(bool IsStandardError, string Text)
        {
            /// <summary>
            /// Gets whether this event marks the end of its output stream.
            /// </summary>
            public bool IsEnd => Text == null;
        }
    }
}
