#if UNITY_EDITOR

using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using UnityEngine;

namespace EncosyTower.Editor.Serialization.Internals
{
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogWarningOverridableDefault(in OverridableDefaultError error)
        {
            StaticDevLogger.LogWarning(error.ToMessage());
        }
    }
}

#endif
