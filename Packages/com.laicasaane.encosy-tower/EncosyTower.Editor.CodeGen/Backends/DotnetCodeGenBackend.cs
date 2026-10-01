using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Runs code generators through the isolated temporary .NET workspace.
    /// </summary>
    internal sealed class DotnetCodeGenBackend
    {
        private readonly Func<CancellationToken, Task> _stopRecordedProcessTreeAsync;
        private readonly Func<CancellationToken, Task<DotnetWorkspace>> _createWorkspaceAsync;
        private readonly Func<
              DotnetProcessRequest
            , Action<DotnetProcessOutput>
            , CancellationToken
            , Task<DotnetProcessResult>
        > _runProcessAsync;

        private readonly Func<DotnetWorkspace, GeneratedCodeBatch> _readResult;
        private readonly DotnetCodeGenProgress _progress;

        /// <summary>
        /// Initializes the backend with its workspace, process, result, and progress operations.
        /// </summary>
        internal DotnetCodeGenBackend(
              Func<CancellationToken, Task> stopRecordedProcessTreeAsync
            , Func<CancellationToken, Task<DotnetWorkspace>> createWorkspaceAsync
            , Func<DotnetProcessRequest, Action<DotnetProcessOutput>, CancellationToken, Task<DotnetProcessResult>>
                runProcessAsync
            , Func<DotnetWorkspace, GeneratedCodeBatch> readResult
            , DotnetCodeGenProgress progress
        )
        {
            _stopRecordedProcessTreeAsync = stopRecordedProcessTreeAsync
                ?? throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(
                    nameof(stopRecordedProcessTreeAsync)
                );
            _createWorkspaceAsync = createWorkspaceAsync
                ?? throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(
                    nameof(createWorkspaceAsync)
                );
            _runProcessAsync = runProcessAsync
                ?? throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(
                    nameof(runProcessAsync)
                );
            _readResult = readResult
                ?? throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(readResult));
            _progress = progress
                ?? throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(progress));
        }

        /// <summary>
        /// Recreates the workspace, runs its generated Runner, and reads the complete result batch.
        /// </summary>
        internal async Task<GeneratedCodeBatch> GenerateAsync(CancellationToken token = default)
        {
            _progress.Start();

            try
            {
                _progress.Report(
                      DotnetCodeGenProgress.RECREATE_WORKSPACE_STAGE
                    , "Stopping the verified prior process tree."
                );

                await _stopRecordedProcessTreeAsync(token);

                token.ThrowIfCancellationRequested();

                _progress.Report(
                      DotnetCodeGenProgress.CAPTURE_INPUTS_STAGE
                    , "Capturing Unity inputs and dependency snapshots."
                );

                var workspace = await _createWorkspaceAsync(token);
                ValidateWorkspace(workspace);
                token.ThrowIfCancellationRequested();

                _progress.Report(
                      DotnetCodeGenProgress.PREPARE_STAGE
                    , "Running Prepare discovery, syntax preflight, and project creation."
                );

                var runTask = _runProcessAsync(workspace.PrepareRequest, _progress.Enqueue, token);

                _progress.Report(DotnetCodeGenProgress.EXECUTE_STAGE, "Running implicit restore/build and Execute.");

                await Task.Yield();

                _progress.Report(DotnetCodeGenProgress.WORKER_STAGE, "Running isolated generator workers.");

                var processResult = await runTask;

                if (processResult.ExitCode != 0)
                {
                    throw CreateProcessFailure(workspace, processResult);
                }

                token.ThrowIfCancellationRequested();
                _progress.Report(DotnetCodeGenProgress.APPLY_STAGE, "Validating the generated result batch.");

                var batch = _readResult(workspace);

                if (batch.GeneratedCodes == null || batch.Diagnostics == null)
                {
                    throw ThrowHelper.CreateResultManifestMalformedException(workspace.ResultManifestPath);
                }

                return batch;
            }
            finally
            {
                if (_progress.IsRunning)
                {
                    try
                    {
                        _progress.Report(
                              DotnetCodeGenProgress.CLEANUP_STAGE
                            , "Cleaning process callbacks and progress state."
                        );
                    }
                    finally
                    {
                        _progress.Finish();
                    }
                }
            }
        }

        private void ValidateWorkspace(DotnetWorkspace workspace)
        {
            if (workspace == null)
            {
                throw ThrowHelper.CreateWorkspaceFactoryReturnedNullException();
            }

            if (string.IsNullOrWhiteSpace(workspace.RootPath)
                || string.IsNullOrWhiteSpace(workspace.SelectedSdkVersion)
                || string.IsNullOrWhiteSpace(workspace.ResultManifestPath)
                || string.IsNullOrWhiteSpace(workspace.ProcessManifestPath)
                || workspace.PrepareRequest == null
            )
            {
                goto MALFORMED;
            }

            if (PathsEqual(workspace.PrepareRequest.ProcessManifestPath, workspace.ProcessManifestPath) == false
                || PathsEqual(workspace.PrepareRequest.WorkingDirectory, workspace.RootPath) == false
            )
            {
                goto MALFORMED;
            }

            return;

        MALFORMED:
            throw ThrowHelper.CreateWorkspaceContractMalformedException();
        }

        private static CodeGenRunException CreateProcessFailure(DotnetWorkspace workspace, DotnetProcessResult result)
        {
            return ThrowHelper.CreateDotnetProcessFailedException(workspace, result);
        }

        private static bool PathsEqual(string left, string right)
            => string.Equals(
                  NormalizePath(left)
                , NormalizePath(right)
                , Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
            );

        private static string NormalizePath(string path)
            => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
