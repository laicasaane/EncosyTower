#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.Compilation;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Converts Unity compilation and lifecycle events into deferred automatic generation requests.
    /// </summary>
    [InitializeOnLoad]
    internal static class CodeGenAutoTrigger
    {
        /// <summary>
        /// Identifies the session flag that prevents duplicate initial-load requests.
        /// </summary>
        internal const string INITIAL_LOAD_HANDLED_KEY = "EncosyTower.Editor.CodeGen.InitialLoadHandled";

        /// <summary>
        /// Identifies the session value that survives a pending automatic request.
        /// </summary>
        internal const string PENDING_AUTOMATIC_KEY = "EncosyTower.Editor.CodeGen.PendingAutomatic";

        /// <summary>
        /// Identifies the session flag that suppresses compilation caused by committed outputs.
        /// </summary>
        internal const string SUPPRESS_NEXT_COMPILATION_KEY = "EncosyTower.Editor.CodeGen.SuppressNextCompilation";

        private const string SESSION_VALUE = "1";
        private const string PENDING_NORMAL_VALUE = "normal";
        private const string PENDING_FAILED_COMPILATION_VALUE = "failed";

        private static readonly CodeGenCoordinator s_coordinator;

        private static bool s_compilationHadErrors;

        static CodeGenAutoTrigger()
        {
            var workspaceFactory = new DotnetWorkspaceTemplateWriter();
            var processRunner = new DotnetProcessRunner();
            var progress = new DotnetCodeGenProgress();
            var unityBackend = new UnityCodeGenBackend();
            var dotnetBackend = new DotnetCodeGenBackend(
                  token => workspaceFactory.StopRecordedProcessTreesAsync(processRunner, token)
                , token => workspaceFactory.CreateAsync(CodeGenSettings.ReadCurrent().RetainDotnetSolutions, token)
                , processRunner.RunAsync
                , workspaceFactory.ReadResult
                , progress
            );
            var writer = new GeneratedCodeBatchWriter();
            s_coordinator = new CodeGenCoordinator(
                  unityBackend.GenerateAsync
                , dotnetBackend.GenerateAsync
                , writer.Write
            );

            Subscribe();

            var pendingValue = SessionState.GetString(PENDING_AUTOMATIC_KEY, string.Empty);

            if (pendingValue.Length > 0)
            {
                s_coordinator.QueueAutomatic(IsFailedCompilationValue(pendingValue));
                return;
            }

            if (SessionState.GetString(INITIAL_LOAD_HANDLED_KEY, string.Empty).Length == 0)
            {
                SessionState.SetString(INITIAL_LOAD_HANDLED_KEY, SESSION_VALUE);
                s_coordinator.QueueAutomatic(failedCompilation: false);
            }
        }

        /// <summary>
        /// Gets whether the most recently observed Unity compilation contained errors.
        /// </summary>
        internal static bool LastCompilationFailed { get; private set; }

        /// <summary>
        /// Gets whether a code generation request is currently running.
        /// </summary>
        internal static bool IsRunning => s_coordinator.IsRunning;

        /// <summary>
        /// Requests a manual run through the Unity backend.
        /// </summary>
        internal static bool RequestUnity()
            => s_coordinator.RequestUnity();

        /// <summary>
        /// Requests a manual run through the .NET backend.
        /// </summary>
        internal static bool RequestDotnet()
            => s_coordinator.RequestDotnet();

        /// <summary>
        /// Persists an automatic request and whether it originated from a failed compilation.
        /// </summary>
        internal static void SetPendingAutomatic(bool failedCompilation)
        {
            SessionState.SetString(
                  PENDING_AUTOMATIC_KEY
                , failedCompilation ? PENDING_FAILED_COMPILATION_VALUE : PENDING_NORMAL_VALUE
            );
        }

        /// <summary>
        /// Removes the persisted automatic request.
        /// </summary>
        internal static void ClearPendingAutomatic()
        {
            SessionState.EraseString(PENDING_AUTOMATIC_KEY);
        }

        /// <summary>
        /// Marks the next compilation as an expected consequence of committed generated files.
        /// </summary>
        internal static void BeginOutputCompilationSuppression()
        {
            SessionState.SetString(SUPPRESS_NEXT_COMPILATION_KEY, SESSION_VALUE);
        }

        /// <summary>
        /// Removes the expected output-compilation marker.
        /// </summary>
        internal static void ClearOutputCompilationSuppression()
        {
            SessionState.EraseString(SUPPRESS_NEXT_COMPILATION_KEY);
        }

        private static bool IsFailedCompilationValue(string value)
            => string.Equals(value, PENDING_FAILED_COMPILATION_VALUE, StringComparison.Ordinal);

        private static void Subscribe()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting += OnEditorQuitting;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnCompilationStarted(object context)
        {
            s_compilationHadErrors = false;
            LastCompilationFailed = false;
        }

        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            var count = messages?.Length ?? 0;

            for (var i = 0; i < count; i++)
            {
                if (messages[i].type == CompilerMessageType.Error)
                {
                    s_compilationHadErrors = true;
                    return;
                }
            }
        }

        private static void OnCompilationFinished(object context)
        {
            if (SessionState.GetString(SUPPRESS_NEXT_COMPILATION_KEY, string.Empty).Length > 0)
            {
                ClearOutputCompilationSuppression();
                return;
            }

            LastCompilationFailed = s_compilationHadErrors;

            if (s_compilationHadErrors == false)
            {
                SetPendingAutomatic(failedCompilation: false);
                return;
            }

            var settings = CodeGenSettings.ReadCurrent();

            if (settings.AutomaticallyGenerateCode && settings.RunGeneratorsInDotnetSolution)
            {
                s_coordinator.QueueAutomatic(failedCompilation: true);
            }
            else
            {
                s_coordinator.ClearPendingAutomatic();
            }
        }

        private static void OnBeforeAssemblyReload()
        {
            s_coordinator.Cancel();
        }

        private static void OnEditorQuitting()
        {
            s_coordinator.Cancel();
        }

        private static void OnEditorUpdate()
        {
            s_coordinator.Tick();
        }
    }
}

#endif
