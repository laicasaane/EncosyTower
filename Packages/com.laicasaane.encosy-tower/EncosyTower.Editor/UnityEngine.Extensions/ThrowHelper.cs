using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using UnityEngine;

namespace EncosyTower.Editor.UnityExtensions
{
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void WarningIfValuePropertyNull()
        {
            StaticDevLogger.LogWarning("Could not find the layer index property, was it renamed or removed?");
        }
    }
}
