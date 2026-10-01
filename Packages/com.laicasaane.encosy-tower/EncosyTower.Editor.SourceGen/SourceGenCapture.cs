#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EncosyTower.Editor.SourceGen
{
    /// <summary>
    /// Coordinates one generated-source capture with Unity's script-compilation lifecycle.
    /// </summary>
    /// <remarks>
    /// The controller restores persistent compiler settings before requesting a cleanup compilation,
    /// so the capture option affects only the intended compilation.
    /// </remarks>
    [InitializeOnLoad]
    internal static class SourceGenCapture
    {
        private const string OUTPUT_MENU_PATH = "Encosy Tower/SourceGen/Output Generated Files";
        private const string REVEAL_MENU_PATH = "Encosy Tower/SourceGen/Reveal In Finder";
        private const string DIALOG_TITLE = "SourceGen Capture";

        private readonly static SourceGenCaptureOperation s_operation;

        private static object s_compilationContext;
        private static int s_compilerErrorCount;
        private static bool s_compilationStarted;
        private static bool s_running;

        static SourceGenCapture()
        {
            try
            {
                s_operation = new SourceGenCaptureOperation(GetProjectRootPath());
            }
            catch (Exception exception)
            {
                DisplayError($"SourceGen capture could not initialize.\n\n{exception}");
                return;
            }

            // Recover before accepting another capture because PlayerSettings survives domain reloads
            // and Editor restarts.
            if (s_operation.HasRecoveryState == false)
            {
                return;
            }

            if (s_operation.Restore(out var capturePath, out var error) == false)
            {
                DisplayError(error);
                return;
            }

            var relativeCapturePath = GetProjectRelativePath(capturePath);

            try
            {
                RequestCleanCompilation();
                EditorUtility.DisplayDialog(
                      DIALOG_TITLE
                    , $"The interrupted SourceGen capture was restored. Generated files remain at "
                        + $"'{relativeCapturePath}'. A clean cleanup compilation was requested."
                    , "OK"
                );
            }
            catch (Exception exception)
            {
                DisplayError(
                    $"The interrupted SourceGen capture was restored and generated files remain at "
                    + $"'{relativeCapturePath}', but Unity could not request the cleanup compilation.\n\n{exception}"
                );
            }
        }

        [MenuItem(OUTPUT_MENU_PATH, priority = 83_71_00_00)]
        private static void OutputGeneratedFiles()
        {
            if (s_operation is null)
            {
                DisplayError("SourceGen capture is unavailable because initialization failed.");
                return;
            }

            // Subscribe before changing PlayerSettings because Unity may schedule compilation as soon
            // as the setting changes.
            s_compilationContext = null;
            s_compilerErrorCount = 0;
            s_compilationStarted = false;
            s_running = true;
            Subscribe();

            var settings = SourceGenSettings.ReadCurrent();

            if (s_operation.Prepare(settings.RetainOutput, out var error) == false)
            {
                Unsubscribe();
                s_running = false;
                ResetCompilationState();
                DisplayError(error);
                return;
            }

            try
            {
                RequestCleanCompilation();
            }
            catch (Exception exception)
            {
                Unsubscribe();
                s_running = false;

                if (s_operation.Restore(out var capturePath, out var restoreError) == false)
                {
                    DisplayError(restoreError);
                    ResetCompilationState();
                    return;
                }

                DisplayError(
                    $"Unity could not request the SourceGen capture compilation. Compiler settings were restored and "
                    + $"generated files remain at '{GetProjectRelativePath(capturePath)}'.\n\n{exception}"
                );
                ResetCompilationState();
            }
        }

        [MenuItem(OUTPUT_MENU_PATH, true)]
        private static bool ValidateOutputGeneratedFiles()
            => EditorApplication.isCompiling == false
                && EditorApplication.isUpdating == false
                && EditorApplication.isPlayingOrWillChangePlaymode == false
                && s_running == false
                && s_operation is not null
                && s_operation.HasRecoveryState == false;

        [MenuItem(REVEAL_MENU_PATH, priority = 83_71_00_01)]
        private static void RevealInFinder()
        {
            if (s_operation is not null)
            {
                EditorUtility.RevealInFinder(s_operation.GeneratedCodeRootPath);
            }
        }

        [MenuItem(REVEAL_MENU_PATH, true)]
        private static bool ValidateRevealInFinder()
            => s_operation is not null && Directory.Exists(s_operation.GeneratedCodeRootPath);

        private static void Subscribe()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }

        private static void Unsubscribe()
        {
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
        }

        private static void OnCompilationStarted(object context)
        {
            if (s_running == false || s_compilationStarted)
            {
                return;
            }

            // Retain the first context so later callbacks can distinguish this capture from unrelated compilations.
            s_compilationContext = context;
            s_compilationStarted = true;
        }

        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            if (s_running == false || s_compilationStarted == false)
            {
                return;
            }

            var count = messages?.Length ?? 0;

            for (var i = 0; i < count; i++)
            {
                if (messages[i].type == CompilerMessageType.Error)
                {
                    s_compilerErrorCount++;
                }
            }
        }

        private static void OnCompilationFinished(object context)
        {
            // Unity identifies a compilation with an opaque context; reference identity is the only
            // exact correlation key.
            if (s_running == false
                || s_compilationStarted == false
                || ReferenceEquals(s_compilationContext, context) == false
            )
            {
                return;
            }

            // Detach first because restoring PlayerSettings schedules the cleanup compilation that
            // must not be captured.
            Unsubscribe();
            s_running = false;
            var compilerErrorCount = s_compilerErrorCount;
            ResetCompilationState();

            if (s_operation.Restore(out var capturePath, out var error) == false)
            {
                DisplayError(error);
                return;
            }

            try
            {
                RequestCleanCompilation();
            }
            catch (Exception exception)
            {
                DisplayError(
                    $"SourceGen compiler settings were restored, but Unity could not request the cleanup compilation. "
                    + $"Generated files remain at '{GetProjectRelativePath(capturePath)}'.\n\n{exception}"
                );
                return;
            }

            var relativeCapturePath = GetProjectRelativePath(capturePath);

            if (compilerErrorCount > 0)
            {
                EditorUtility.DisplayDialog(
                      DIALOG_TITLE
                    , $"The capture compilation finished with {compilerErrorCount} compiler error(s). "
                        + $"Partial generated files were retained at '{relativeCapturePath}'. "
                        + "Review the Unity Console for details."
                    , "OK"
                );
                return;
            }

            try
            {
                var generatedFileCount = Directory.Exists(capturePath)
                    ? Directory.GetFiles(capturePath, "*.cs", SearchOption.AllDirectories).Length
                    : 0;
                Debug.Log(
                    $"SourceGen capture succeeded. Output: '{relativeCapturePath}'. Generated C# files: "
                    + $"{generatedFileCount}."
                );
            }
            catch (Exception exception)
            {
                DisplayError(
                    $"The capture compilation succeeded, but generated files at '{relativeCapturePath}' could not be "
                    + $"counted.\n\n{exception}"
                );
            }
        }

        private static void RequestCleanCompilation()
        {
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
        }

        private static void ResetCompilationState()
        {
            s_compilationContext = null;
            s_compilerErrorCount = 0;
            s_compilationStarted = false;
        }

        private static string GetProjectRootPath()
            => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        private static string GetProjectRelativePath(string path)
            => Path.GetRelativePath(GetProjectRootPath(), path).Replace('\\', '/');

        private static void DisplayError(string error)
        {
            EditorUtility.DisplayDialog(DIALOG_TITLE, error, "OK");
        }
    }
}

#endif
