using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Logging;
using EncosyTower.Tasks;

using ETDBG = EncosyTower.Debugging;

namespace EncosyTower.PageFlows
{
    internal static partial class Validator
    {
        public static async UnityTask<bool> ValidateTransitionAsync(
              [NotNull] IPageFlow flow
            , [NotNull] ILogger logger
            , PageAsyncOperation asyncOperation
            , CancellationToken token
        )
        {
            ETDBG.ThrowHelper.ThrowIfNull(flow);
            ETDBG.ThrowHelper.ThrowIfNull(logger);

            if (flow.IsInTransition == false)
            {
                return true;
            }

            switch (asyncOperation)
            {
                case PageAsyncOperation.Sequential:
                {
                    await UnityTask.WaitWhile(flow, static state => state.IsInTransition, token);
                    return !token.IsCancellationRequested;
                }

                case PageAsyncOperation.Drop:
                {
                    return false;
                }

                case PageAsyncOperation.DropError:
                {
                    logger.LogError(GetLogCurrentlyInTransition(flow));
                    return false;
                }

                default:
                {
                    throw new InvalidOperationException(GetLogCurrentlyInTransition(flow));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetLogCurrentlyInTransition(IPageFlow flow)
            => $"{flow.GetType()} is currently in transition.";
    }
}
