#if UNITY_LOCALIZATION

using System.Diagnostics;
using EncosyTower.Logging;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Localization
{
    internal static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ErrorCannotFindLanguage(string value)
        {
            StaticDevLogger.LogError($"Cannot find any language by locale code {value}");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ErrorNotReady()
        {
            StaticDevLogger.LogError("Must call \"L10n.Initialize()\" first.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void InfoChangeLanguage(string value)
        {
            StaticDevLogger.LogInfo($"Change language to {value}");
        }
    }
}

#endif
