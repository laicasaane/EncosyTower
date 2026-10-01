using System;
using System.Diagnostics;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.UIElements
{
    static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowFormatException()
            => throw new FormatException(
                "The value of format is not null, an empty string (\"\"), \"N\", \"D\", \"B\", \"P\", or \"X\""
            );
    }
}
