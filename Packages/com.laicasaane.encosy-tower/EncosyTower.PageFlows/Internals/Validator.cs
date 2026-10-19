using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Tasks;

using ETDBG = EncosyTower.Debugging;

namespace EncosyTower.PageFlows.Internals
{
    internal static partial class Validator
    {
        private const int SEQUENTIAL_WAIT_WARNING_SECONDS = 5;

        /// <summary>Applies <see cref="PageAsyncOperation"/> when a call finds its flow in transition.</summary>
        /// <remarks>
        /// Returns success at once when the flow is idle. <c>Drop</c> returns the <c>InTransition</c> case;
        /// <c>Exception</c> throws. <c>Sequential</c> waits, then returns the <c>Withdrawn</c> case when an abort is
        /// requested meanwhile, or the <c>Cancelled</c> case when the token is cancelled. A drop marks the abort state
        /// as dropped.
        /// </remarks>
        public static async UnityTask<Success<PageFlowError>> ValidateTransitionAsync(
              [NotNull] IPageFlow flow
            , [NotNull] ILogger logger
            , PageAsyncOperation asyncOperation
            , PageAbortState abort
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNull(flow);
            ETDBG.ThrowHelper.ThrowIfNull(logger);

            if (flow.IsInTransition == false)
            {
                return Success.Yes;
            }

            switch (asyncOperation)
            {
                case PageAsyncOperation.Sequential:
                {
                    try
                    {
                        var deadline = Stopwatch.GetTimestamp() + SEQUENTIAL_WAIT_WARNING_SECONDS * Stopwatch.Frequency;

                        await UnityTask.WaitWhile(
                              (flow, abort, deadline)
                            , static state => state.flow.IsInTransition
                                && state.abort.IsRequested == false
                                && Stopwatch.GetTimestamp() < state.deadline
                            , token
                        );

                        if (flow.IsInTransition && abort.IsRequested == false)
                        {
                            LogWarningLongSequentialWait(logger, flow);

                            await UnityTask.WaitWhile(
                                  (flow, abort)
                                , static state => state.flow.IsInTransition && state.abort.IsRequested == false
                                , token
                            );
                        }
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        return PageFlowError.Cancelled();
                    }

                    if (abort.IsRequested)
                    {
                        return PageFlowError.Withdrawn(abort.RequestOrigin).WithAbortMode(abort.RequestMode);
                    }

                    return Success.Yes;
                }

                case PageAsyncOperation.Drop:
                {
                    abort.MarkDropped();
                    return PageFlowError.InTransition(flow.GetType());
                }

                default:
                {
                    abort.MarkDropped();
                    throw new InvalidOperationException(GetLogCurrentlyInTransition(flow));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetLogCurrentlyInTransition(IPageFlow flow)
            => $"{flow.GetType()} is currently in transition.";

        [MethodImpl(MethodImplOptions.NoInlining)]
        [UnityEngine.HideInCallstack, StackTraceHidden]
        private static void LogWarningLongSequentialWait(ILogger logger, IPageFlow flow)
        {
            logger.LogWarning(
                $"{flow.GetType()} has waited {SEQUENTIAL_WAIT_WARNING_SECONDS} s for its current transition; " +
                $"a callback may be awaiting an operation on its own flow."
            );
        }
    }
}
