using System.Diagnostics;
using EncosyTower.Logging;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Scenes
{
    static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void LogErrorIfInvalidInEditor(SceneBuildIndex index)
        {
            StaticDevLogger.LogError(
                $"Cannot find scene with build index {index.Index} and name '{index.Name}' " +
                "in the current EditorBuildSettings."
            );
        }
    }
}
