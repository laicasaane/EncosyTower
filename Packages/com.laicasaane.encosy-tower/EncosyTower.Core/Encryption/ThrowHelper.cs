using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Encryption
{
    internal static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfNotInitialized([DoesNotReturnIf(false)] bool isInitialized)
        {
            if (isInitialized == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Encryption is not initialized.");
        }
    }
}
