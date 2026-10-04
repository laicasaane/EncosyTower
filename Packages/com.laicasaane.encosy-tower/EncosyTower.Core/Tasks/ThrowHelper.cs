using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Tasks
{
    internal static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfCountOutOfRange([DoesNotReturnIf(false)] bool validCount)
        {
            if (validCount == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException()
                => new("count");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfDelayNegative([DoesNotReturnIf(false)] bool validDelay, string paramName)
        {
            if (validDelay == false)
            {
                throw CreateException(paramName);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException(string paramName)
                => new(paramName, "The delay must not be negative.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfWhenAnyEmpty([DoesNotReturnIf(true)] bool isEmpty)
        {
            if (isEmpty)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException()
                => new("The tasks collection must not be empty.", "tasks");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfConcurrentMoveNext([DoesNotReturnIf(true)] bool isPending)
        {
            if (isPending)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("MoveNextAsync was called before the previous call completed.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogUnobservedException(Exception exception)
        {
            StaticLogger.LogException(exception);
        }
    }
}
