using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using UnityEngine;

namespace EncosyTower.Editor.Logging
{
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogWarningUndefinedMenuPath(string path)
        {
            StaticDevLogger.LogWarning($"Could not find menu item at path '{path}'");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogWarningUnsupportedRoute(string href)
        {
            StaticDevLogger.LogWarning(
                $"Could not route to '{href}'. " +
                $"Current supported routes are <b>\\open:Project/</b>, <b>\\open:Preferences/</b>, " +
                $" <b>\\menu:</b>."
            );
        }
    }
}
