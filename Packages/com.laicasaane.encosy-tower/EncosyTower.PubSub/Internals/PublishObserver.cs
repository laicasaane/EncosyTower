using System;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub.Internals
{
    internal static class PublishObserver
    {
        internal static void ForgetAndLog(UnityTask<Success<PublishError>> task, ILogger logger)
        {
            ObserveAsync(task, logger).Forget();
        }

        private static async UnityTask ObserveAsync(UnityTask<Success<PublishError>> task, ILogger logger)
        {
            try
            {
                var result = await task;

                if (result.TryGetFailure(out var error))
                {
                    error.Log(logger);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
        }
    }
}
