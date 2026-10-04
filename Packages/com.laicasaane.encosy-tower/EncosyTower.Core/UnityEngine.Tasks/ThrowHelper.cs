using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;

using static EncosyTower.Debugging.ValidationDefines;

namespace UnityEngine.Tasks
{
    static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfCountOutOfRange(int count, int length)
        {
            if ((uint)count > (uint)length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfMillisecondsDelayInvalid(int millisecondsDelay)
        {
            if (millisecondsDelay < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfDelayInvalid(TimeSpan delayTimeSpan)
        {
            if (delayTimeSpan < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(delayTimeSpan));
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfWhenAnyEmpty(int count)
        {
            if (count == 0)
            {
                throw new ArgumentException("The tasks collection must not be empty.", "tasks");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogException(Exception exception)
        {
            StaticLogger.LogException(exception);
        }
    }
}
