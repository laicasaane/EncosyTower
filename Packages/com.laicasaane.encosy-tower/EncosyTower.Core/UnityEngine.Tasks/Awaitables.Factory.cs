using System;
using System.Runtime.CompilerServices;
using System.Threading;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace UnityEngine.Tasks
{
    public static partial class Awaitables
    {
        public static Awaitable CompletedTask
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var source = new AwaitableCompletionSource();
                source.SetResult();
                return source.Awaitable;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable FromException(Exception exception)
        {
            DebuggingThrowHelper.ThrowIfNull(exception);

            if (exception is OperationCanceledException canceledException)
            {
                return FromCanceled(canceledException.CancellationToken);
            }

            var source = new AwaitableCompletionSource();
            source.SetException(exception);
            return source.Awaitable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T> FromException<T>(Exception exception)
        {
            DebuggingThrowHelper.ThrowIfNull(exception);

            if (exception is OperationCanceledException canceledException)
            {
                return FromCanceled<T>(canceledException.CancellationToken);
            }

            var source = new AwaitableCompletionSource<T>();
            source.SetException(exception);
            return source.Awaitable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T> FromResult<T>(T value)
        {
            var source = new AwaitableCompletionSource<T>();
            source.SetResult(value);
            return source.Awaitable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable FromCanceled(CancellationToken cancellationToken = default)
        {
            var source = new AwaitableCompletionSource();
            source.SetException(new OperationCanceledException(cancellationToken));
            return source.Awaitable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Awaitable<T> FromCanceled<T>(CancellationToken cancellationToken = default)
        {
            var source = new AwaitableCompletionSource<T>();
            source.SetException(new OperationCanceledException(cancellationToken));
            return source.Awaitable;
        }
    }
}
