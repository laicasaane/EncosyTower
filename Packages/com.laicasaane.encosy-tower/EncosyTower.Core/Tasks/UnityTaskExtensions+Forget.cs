using System;

namespace EncosyTower.Tasks
{
    public static partial class UnityTaskExtensions
    {
        /// <summary>
        /// Observes <paramref name="task"/> without awaiting it and passes any exception to
        /// <paramref name="exceptionHandler"/>.
        /// </summary>
        /// <param name="task">The task to observe.</param>
        /// <param name="exceptionHandler">
        /// Receives every exception of <paramref name="task"/>, including <see cref="OperationCanceledException"/>;
        /// <c>null</c> to behave as <see cref="Forget(UnityTask)"/>.
        /// </param>
        /// <param name="handleExceptionOnMainThread"><c>true</c> to call the handler on the main thread.</param>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> an exception thrown by <paramref name="exceptionHandler"/> is logged through the Encosy
        /// logger.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.Forget</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static void Forget(
              this UnityTask task
            , Action<Exception> exceptionHandler
            , bool handleExceptionOnMainThread = true
        )
        {
            if (exceptionHandler == null)
            {
                task.Forget();
                return;
            }

            ForgetWithHandlerAsync(task, exceptionHandler, handleExceptionOnMainThread).Forget();
        }

        /// <summary>
        /// Observes <paramref name="task"/> without awaiting it, discards its result, and passes any exception to
        /// <paramref name="exceptionHandler"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="task">The task to observe.</param>
        /// <param name="exceptionHandler">
        /// Receives every exception of <paramref name="task"/>, including <see cref="OperationCanceledException"/>;
        /// <c>null</c> to behave as <see cref="Forget{T}(UnityTask{T})"/>.
        /// </param>
        /// <param name="handleExceptionOnMainThread"><c>true</c> to call the handler on the main thread.</param>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> an exception thrown by <paramref name="exceptionHandler"/> is logged through the Encosy
        /// logger.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.Forget</c>; Unity: none.
        /// </para>
        /// </remarks>
        public static void Forget<T>(
              this UnityTask<T> task
            , Action<Exception> exceptionHandler
            , bool handleExceptionOnMainThread = true
        )
        {
            if (exceptionHandler == null)
            {
                task.Forget();
                return;
            }

            ForgetWithHandlerAsync(task.AsUnityTask(), exceptionHandler, handleExceptionOnMainThread).Forget();
        }

        /// <summary>
        /// Awaits <paramref name="task"/> and passes any exception to <paramref name="exceptionHandler"/>.
        /// </summary>
        /// <param name="task">The task to observe.</param>
        /// <param name="exceptionHandler">The method that receives the exception.</param>
        /// <param name="handleExceptionOnMainThread"><c>true</c> to switch to the main thread before calling the
        /// handler.</param>
        /// <remarks>
        /// Every exception is passed to the handler, including <see cref="OperationCanceledException"/>. An exception
        /// thrown by the switch or by the handler is logged as unobserved instead of propagating.
        /// </remarks>
        /// <summary>
        /// Awaits <paramref name="task"/> and passes any exception to <paramref name="exceptionHandler"/>.
        /// </summary>
        /// <param name="task">The task to observe.</param>
        /// <param name="exceptionHandler">The method that receives the exception.</param>
        /// <param name="handleExceptionOnMainThread"><c>true</c> to switch to the main thread before calling the
        /// handler.</param>
        /// <remarks>
        /// Every exception is passed to the handler, including <see cref="OperationCanceledException"/>. An exception
        /// thrown by the switch or by the handler is logged as unobserved instead of propagating.
        /// </remarks>
        private static async UnityTaskVoid ForgetWithHandlerAsync(
              UnityTask task
            , Action<Exception> exceptionHandler
            , bool handleExceptionOnMainThread
        )
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                try
                {
                    if (handleExceptionOnMainThread)
                    {
                        await UnityTask.SwitchToMainThreadAsync();
                    }

                    exceptionHandler(exception);
                }
                catch (Exception handlerException)
                {
                    ThrowHelper.LogUnobservedException(handlerException);
                }
            }
        }
    }
}
