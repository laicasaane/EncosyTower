using System;
using System.Runtime.ExceptionServices;
using System.Threading;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace UnityEngine.Tasks
{
    public static partial class Awaitables
    {
        public static Awaitable RunOnThreadPool(
              Action action
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
        {
            DebuggingThrowHelper.ThrowIfNull(action);
            return RunOnThreadPoolCore(action, configureAwait, cancellationToken);
        }

        public static Awaitable RunOnThreadPool(
              Action<object> action
            , object state
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
        {
            DebuggingThrowHelper.ThrowIfNull(action);
            return RunOnThreadPoolCore(action, state, configureAwait, cancellationToken);
        }

        public static Awaitable RunOnThreadPool(
              Func<Awaitable> action
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
        {
            DebuggingThrowHelper.ThrowIfNull(action);
            return RunOnThreadPoolCore(action, configureAwait, cancellationToken);
        }

        public static Awaitable RunOnThreadPool(
              Func<object, Awaitable> action
            , object state
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
        {
            DebuggingThrowHelper.ThrowIfNull(action);
            return RunOnThreadPoolCore(action, state, configureAwait, cancellationToken);
        }

        public static Awaitable<T> RunOnThreadPool<T>(
              Func<T> function
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
        {
            DebuggingThrowHelper.ThrowIfNull(function);
            return RunOnThreadPoolCore(function, configureAwait, cancellationToken);
        }

        public static Awaitable<T> RunOnThreadPool<T>(
              Func<Awaitable<T>> function
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
        {
            DebuggingThrowHelper.ThrowIfNull(function);
            return RunOnThreadPoolCore(function, configureAwait, cancellationToken);
        }

        public static Awaitable<T> RunOnThreadPool<T>(
              Func<object, T> function
            , object state
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
        {
            DebuggingThrowHelper.ThrowIfNull(function);
            return RunOnThreadPoolCore(function, state, configureAwait, cancellationToken);
        }

        public static Awaitable<T> RunOnThreadPool<T>(
              Func<object, Awaitable<T>> function
            , object state
            , bool configureAwait = true
            , CancellationToken cancellationToken = default
        )
        {
            DebuggingThrowHelper.ThrowIfNull(function);
            return RunOnThreadPoolCore(function, state, configureAwait, cancellationToken);
        }

        private static async Awaitable RunOnThreadPoolCore(
              Action action
            , bool configureAwait
            , CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Awaitable.BackgroundThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();
            ExceptionDispatchInfo exception = null;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ExceptionDispatchInfo.Capture(ex);
            }

            if (configureAwait)
            {
                await Yield(AwaitablePlayerLoopTiming.Update);
            }

            exception?.Throw();
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static async Awaitable RunOnThreadPoolCore(
              Action<object> action
            , object state
            , bool configureAwait
            , CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Awaitable.BackgroundThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();
            ExceptionDispatchInfo exception = null;

            try
            {
                action(state);
            }
            catch (Exception ex)
            {
                exception = ExceptionDispatchInfo.Capture(ex);
            }

            if (configureAwait)
            {
                await Yield(AwaitablePlayerLoopTiming.Update);
            }

            exception?.Throw();
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static async Awaitable RunOnThreadPoolCore(
              Func<Awaitable> action
            , bool configureAwait
            , CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Awaitable.BackgroundThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();
            ExceptionDispatchInfo exception = null;

            try
            {
                await action();
            }
            catch (Exception ex)
            {
                exception = ExceptionDispatchInfo.Capture(ex);
            }

            if (configureAwait)
            {
                await Yield(AwaitablePlayerLoopTiming.Update);
            }

            exception?.Throw();
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static async Awaitable RunOnThreadPoolCore(
              Func<object, Awaitable> action
            , object state
            , bool configureAwait
            , CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Awaitable.BackgroundThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();
            ExceptionDispatchInfo exception = null;

            try
            {
                await action(state);
            }
            catch (Exception ex)
            {
                exception = ExceptionDispatchInfo.Capture(ex);
            }

            if (configureAwait)
            {
                await Yield(AwaitablePlayerLoopTiming.Update);
            }

            exception?.Throw();
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static async Awaitable<T> RunOnThreadPoolCore<T>(
              Func<T> function
            , bool configureAwait
            , CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Awaitable.BackgroundThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();
            ExceptionDispatchInfo exception = null;
            var result = default(T);

            try
            {
                result = function();
            }
            catch (Exception ex)
            {
                exception = ExceptionDispatchInfo.Capture(ex);
            }

            if (configureAwait)
            {
                await Yield(AwaitablePlayerLoopTiming.Update);
            }

            exception?.Throw();
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }

        private static async Awaitable<T> RunOnThreadPoolCore<T>(
              Func<Awaitable<T>> function
            , bool configureAwait
            , CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Awaitable.BackgroundThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();
            ExceptionDispatchInfo exception = null;
            var result = default(T);

            try
            {
                result = await function();
            }
            catch (Exception ex)
            {
                exception = ExceptionDispatchInfo.Capture(ex);
            }

            if (configureAwait)
            {
                await Yield(AwaitablePlayerLoopTiming.Update);
            }

            exception?.Throw();
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }

        private static async Awaitable<T> RunOnThreadPoolCore<T>(
              Func<object, T> function
            , object state
            , bool configureAwait
            , CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Awaitable.BackgroundThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();
            ExceptionDispatchInfo exception = null;
            var result = default(T);

            try
            {
                result = function(state);
            }
            catch (Exception ex)
            {
                exception = ExceptionDispatchInfo.Capture(ex);
            }

            if (configureAwait)
            {
                await Yield(AwaitablePlayerLoopTiming.Update);
            }

            exception?.Throw();
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }

        private static async Awaitable<T> RunOnThreadPoolCore<T>(
              Func<object, Awaitable<T>> function
            , object state
            , bool configureAwait
            , CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Awaitable.BackgroundThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();
            ExceptionDispatchInfo exception = null;
            var result = default(T);

            try
            {
                result = await function(state);
            }
            catch (Exception ex)
            {
                exception = ExceptionDispatchInfo.Capture(ex);
            }

            if (configureAwait)
            {
                await Yield(AwaitablePlayerLoopTiming.Update);
            }

            exception?.Throw();
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
    }
}
