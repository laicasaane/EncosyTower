using System.Collections.Concurrent;
using System.Threading;
using EncosyTower.Logging;
using UnityEditor;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Presents .NET backend stages and queued process output through Unity's progress UI.
    /// </summary>
    internal sealed class DotnetCodeGenProgress
    {
        /// <summary>
        /// Identifies the prior-process cleanup stage.
        /// </summary>
        internal const int RECREATE_WORKSPACE_STAGE = 0;

        /// <summary>
        /// Identifies the Unity input capture stage.
        /// </summary>
        internal const int CAPTURE_INPUTS_STAGE = 1;

        /// <summary>
        /// Identifies the Runner prepare stage.
        /// </summary>
        internal const int PREPARE_STAGE = 2;

        /// <summary>
        /// Identifies the Runner execute stage.
        /// </summary>
        internal const int EXECUTE_STAGE = 3;

        /// <summary>
        /// Identifies the isolated worker stage.
        /// </summary>
        internal const int WORKER_STAGE = 4;

        /// <summary>
        /// Identifies the result validation stage.
        /// </summary>
        internal const int APPLY_STAGE = 5;

        /// <summary>
        /// Identifies the final callback and progress cleanup stage.
        /// </summary>
        internal const int CLEANUP_STAGE = 6;

        private const int LAST_STAGE = CLEANUP_STAGE;
        private const string TITLE = "Encosy Tower CodeGen (.NET)";

        private readonly ConcurrentQueue<DotnetProcessOutput> _outputQueue = new();

        private int _mainThreadId;
        private int _progressId = -1;
        private int _stage;
        private string _description = string.Empty;

        /// <summary>
        /// Gets whether a Unity progress item is active.
        /// </summary>
        internal bool IsRunning => _progressId >= 0;

        /// <summary>
        /// Gets the number of process output lines waiting for main-thread delivery.
        /// </summary>
        internal int PendingOutputCount => _outputQueue.Count;

        /// <summary>
        /// Creates the progress item and begins draining process output on Editor updates.
        /// </summary>
        internal void Start()
        {
            if (IsRunning)
            {
                throw ThrowHelper.CreateProgressAlreadyRunningException();
            }

            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _stage = RECREATE_WORKSPACE_STAGE;
            _description = "Recreating workspace and validating dotnet.";
            _progressId = Progress.Start(TITLE);
            Progress.Report(_progressId, 0f, _description);
            EditorApplication.update += Drain;
        }

        /// <summary>
        /// Advances the active progress item to a named backend stage.
        /// </summary>
        internal void Report(int stage, string description)
        {
            EnsureMainThread();

            if (IsRunning == false)
            {
                throw ThrowHelper.CreateProgressNotRunningException();
            }

            if ((uint)stage > LAST_STAGE)
            {
                throw ThrowHelper.CreateProgressStageOutOfRangeException();
            }

            if (description == null)
            {
                throw global::EncosyTower.Debugging.ThrowHelper.CreateArgumentNullException(nameof(description));
            }

            _stage = stage;
            _description = description;
            Progress.Report(_progressId, (float)stage / LAST_STAGE, description);
        }

        /// <summary>
        /// Queues process output for delivery on Unity's main thread.
        /// </summary>
        internal void Enqueue(DotnetProcessOutput output)
        {
            if (output.Text == null)
            {
                throw ThrowHelper.CreateProcessOutputTextNullException();
            }

            _outputQueue.Enqueue(output);
        }

        /// <summary>
        /// Logs queued process output and refreshes the active progress description.
        /// </summary>
        internal void Drain()
        {
            EnsureMainThread();

            while (_outputQueue.TryDequeue(out var output))
            {
                if (output.IsStandardError)
                {
                    StaticDevLogger.LogWarning(output.Text);
                }
                else
                {
                    StaticDevLogger.LogInfo(output.Text);
                }

                _description = output.Text;
            }

            if (IsRunning)
            {
                Progress.Report(_progressId, (float)_stage / LAST_STAGE, _description);
            }
        }

        /// <summary>
        /// Flushes queued output and removes the active progress item.
        /// </summary>
        internal void Finish()
        {
            EnsureMainThread();

            if (IsRunning == false)
            {
                return;
            }

            EditorApplication.update -= Drain;

            try
            {
                Drain();
            }
            finally
            {
                var progressId = _progressId;

                try
                {
                    Progress.Remove(progressId);
                }
                finally
                {
                    _progressId = -1;
                    _stage = RECREATE_WORKSPACE_STAGE;
                    _description = string.Empty;

                    while (_outputQueue.TryDequeue(out _))
                    {
                        continue;
                    }
                }
            }
        }

        private void EnsureMainThread()
        {
            if (_mainThreadId != 0
                && Thread.CurrentThread.ManagedThreadId != _mainThreadId
            )
            {
                throw ThrowHelper.CreateProgressMainThreadRequiredException();
            }
        }
    }
}
