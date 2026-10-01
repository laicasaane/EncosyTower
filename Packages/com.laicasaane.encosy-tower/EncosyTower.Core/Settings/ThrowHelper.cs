using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Settings
{
    static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void LogWarningFailedToMoveAsset(
              string path
            , string oldPath
            , UnityEngine.Object instance
        )
            => StaticDevLogger.LogWarningFormat(
                  $"Failed to move previous settings asset '{oldPath}' to '{path}'. " +
                  "A new settings asset will be created."
                , instance
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception CreateInvalidOperationExceptionClassNameFileNameMustMatch(Type type)
            => new InvalidOperationException($"Settings-derived class and filename must match: {type.Name}");
    }
}
