using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
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
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowTokenMismatch()
            => throw new InvalidOperationException("The token does not match the current task operation.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowNotCompleted()
            => throw new InvalidOperationException("The task has not completed yet.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowContinuationAlreadyRegistered()
            => throw new InvalidOperationException("A task can be awaited only once.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowAlreadyCompleted()
            => throw new InvalidOperationException("The completion source has already been completed.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowEnumeratorDisposed()
            => throw new ObjectDisposedException("IUnityTaskAsyncEnumerator");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowOperationCanceled(CancellationToken token)
            => throw new OperationCanceledException(token);

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowTimeout(TimeSpan timeout)
            => throw new TimeoutException($"The operation did not complete within {timeout}.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowTimerDisposed()
            => throw new ObjectDisposedException(nameof(PlayerLoopTimer));

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowTimeoutControllerDisposed()
            => throw new ObjectDisposedException(nameof(TimeoutController));

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogUnobservedException(Exception exception)
        {
            StaticLogger.LogException(exception);
        }
    }
}
