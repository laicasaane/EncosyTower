#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics;
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

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        [Conditional("__ENCOSY_VALIDATION__")]
        internal static void LogException(Exception exception)
        {
            StaticLogger.LogException(exception);
        }
    }
}
