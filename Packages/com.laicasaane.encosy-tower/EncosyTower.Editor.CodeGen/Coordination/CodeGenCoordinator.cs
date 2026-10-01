#if UNITY_EDITOR

using System;
using System.Threading;
using System.Threading.Tasks;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Serializes manual and automatic requests across the two backends and shared writer.
    /// </summary>
    internal sealed class CodeGenCoordinator
    {
        private readonly Func<CancellationToken, Task<GeneratedCodeBatch>> _generateInUnityAsync;
        private readonly Func<CancellationToken, Task<GeneratedCodeBatch>> _generateInDotnetAsync;
        private readonly Func<GeneratedCodeBatch, CancellationToken, int> _writeBatch;

        private CancellationTokenSource _activeCancellation;
        private Task _activeTask = Task.CompletedTask;
        private bool _isRunning;
        private bool _hasPendingAutomatic;
        private bool _pendingFromFailedCompilation;

        /// <summary>
        /// Initializes a coordinator with both generation operations and the shared output operation.
        /// </summary>
        internal CodeGenCoordinator(
              Func<CancellationToken, Task<GeneratedCodeBatch>> generateInUnityAsync
            , Func<CancellationToken, Task<GeneratedCodeBatch>> generateInDotnetAsync
            , Func<GeneratedCodeBatch, CancellationToken, int> writeBatch
        )
        {
            _generateInUnityAsync = generateInUnityAsync;
            _generateInDotnetAsync = generateInDotnetAsync;
            _writeBatch = writeBatch;
        }

        /// <summary>
        /// Gets whether a code generation request is currently running.
        /// </summary>
        internal bool IsRunning => _isRunning;

        /// <summary>
        /// Gets the task representing the active request, or a completed task when idle.
        /// </summary>
        internal Task ActiveTask => _activeTask;

        /// <summary>
        /// Starts the Unity generation operation immediately when no other run is active.
        /// </summary>
        internal bool RequestUnity()
            => RequestManual(_generateInUnityAsync, CodeGenAutoTrigger.LastCompilationFailed);

        /// <summary>
        /// Starts the .NET generation operation immediately when no other run is active.
        /// </summary>
        internal bool RequestDotnet()
            => RequestManual(_generateInDotnetAsync, warnUnityMayBeStale: false);

        /// <summary>
        /// Records an automatic request for dispatch when the Editor becomes idle.
        /// </summary>
        internal void QueueAutomatic(bool failedCompilation)
        {
            _hasPendingAutomatic = true;
            _pendingFromFailedCompilation = failedCompilation;
            CodeGenAutoTrigger.SetPendingAutomatic(failedCompilation);
        }

        /// <summary>
        /// Clears both the in-memory and persisted automatic request state.
        /// </summary>
        internal void ClearPendingAutomatic()
        {
            _hasPendingAutomatic = false;
            _pendingFromFailedCompilation = false;
            CodeGenAutoTrigger.ClearPendingAutomatic();
        }

        /// <summary>
        /// Attempts to dispatch pending automatic work using the current settings.
        /// </summary>
        internal void Tick()
        {
            TryDispatchAutomatic(CodeGenSettings.ReadCurrent(), EditorApplicationState.IsBusy);
        }

        /// <summary>
        /// Dispatches eligible automatic work and reports whether a run was started.
        /// </summary>
        internal bool TryDispatchAutomatic(CodeGenSettings.Snapshot settings, bool isEditorBusy)
        {
            if (_hasPendingAutomatic == false || _isRunning || isEditorBusy)
            {
                return false;
            }

            if (settings.AutomaticallyGenerateCode == false)
            {
                ClearPendingAutomatic();
                return false;
            }

            if (_pendingFromFailedCompilation
                && settings.RunGeneratorsInDotnetSolution == false
            )
            {
                ClearPendingAutomatic();
                return false;
            }

            var generateAsync = settings.RunGeneratorsInDotnetSolution
                ? _generateInDotnetAsync
                : _generateInUnityAsync;

            ClearPendingAutomatic();
            Start(generateAsync, warnUnityMayBeStale: false);
            return true;
        }

        /// <summary>
        /// Requests cancellation of the active run.
        /// </summary>
        internal void Cancel()
        {
            _activeCancellation?.Cancel();
        }

        private bool RequestManual(
              Func<CancellationToken, Task<GeneratedCodeBatch>> generateAsync
            , bool warnUnityMayBeStale
        )
        {
            if (_isRunning)
            {
                return false;
            }

            ClearPendingAutomatic();
            Start(generateAsync, warnUnityMayBeStale);
            return true;
        }

        private void Start(
              Func<CancellationToken, Task<GeneratedCodeBatch>> generateAsync
            , bool warnUnityMayBeStale
        )
        {
            _isRunning = true;
            _activeCancellation = new CancellationTokenSource();
            _activeTask = RunAsync(generateAsync, warnUnityMayBeStale, _activeCancellation);
        }

        private async Task RunAsync(
              Func<CancellationToken, Task<GeneratedCodeBatch>> generateAsync
            , bool warnUnityMayBeStale
            , CancellationTokenSource cancellation
        )
        {
            await Task.Yield();

            try
            {
                if (warnUnityMayBeStale)
                {
                    ThrowHelper.LogWarning_StaleUnityGenerators();
                }

                var batch = await generateAsync(cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();
                ThrowHelper.ThrowIfInvalidBatch(batch);
                ThrowHelper.LogDiagnostics(batch.Diagnostics);

                if (batch.AllCandidatesSkipped)
                {
                    ClearPendingAutomatic();
                    CodeGenAutoTrigger.ClearOutputCompilationSuppression();
                    return;
                }

                CodeGenAutoTrigger.BeginOutputCompilationSuppression();
                var changedFileCount = _writeBatch(batch, cancellation.Token);

                if (changedFileCount == 0)
                {
                    ClearPendingAutomatic();
                    CodeGenAutoTrigger.ClearOutputCompilationSuppression();
                }
            }
            catch (OperationCanceledException)
            {
                ClearPendingAutomatic();
                CodeGenAutoTrigger.ClearOutputCompilationSuppression();
            }
            catch (CodeGenRunException exception)
            {
                ClearPendingAutomatic();
                CodeGenAutoTrigger.ClearOutputCompilationSuppression();
                ThrowHelper.LogError(exception.Code, exception.Message);
            }
            catch (Exception exception)
            {
                ClearPendingAutomatic();
                CodeGenAutoTrigger.ClearOutputCompilationSuppression();
                ThrowHelper.LogError("ENCOSY_CODEGEN_UNEXPECTED", exception.ToString());
            }
            finally
            {
                cancellation.Dispose();

                if (ReferenceEquals(_activeCancellation, cancellation))
                {
                    _activeCancellation = null;
                    _isRunning = false;
                }
            }
        }

        /// <summary>
        /// Exposes the Unity Editor busy state behind a testable coordinator boundary.
        /// </summary>
        private static class EditorApplicationState
        {
            /// <summary>
            /// Gets whether Unity is compiling or updating assets.
            /// </summary>
            internal static bool IsBusy
                => UnityEditor.EditorApplication.isCompiling || UnityEditor.EditorApplication.isUpdating;
        }
    }
}

#endif
