#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS || ENCOSY_PROCESSING_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using EncosyTower.Tasks;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing
{
    partial class Processor
    {
        public readonly partial struct Hub<TScope, TState>
            where TState : class
        {
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ProcessRegistry Register<TRequest>(
                  [NotNull] Func<TState, TRequest, UnityTask> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return default;
                }
#endif

                return _hub.Register(
                    new Internals.Async.AsyncProcessByStateHandler<TState, TRequest>(State, process),
                    logger
                );
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ProcessRegistry Register<TRequest, TResult>(
                [NotNull] Func<
                      TState
                    , TRequest
                    , UnityTask<TResult>
                > process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest<TResult>
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return default;
                }
#endif

                return _hub.Register(
                    new Internals.Async.AsyncProcessByStateHandler<TState, TRequest, TResult>(State, process),
                    logger
                );
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ProcessRegistry Register<TRequest>(
                  [NotNull] Func<
                      TState
                    , TRequest
                    , ProcessingContext
                    , UnityTask
                > process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return default;
                }
#endif

                return _hub.Register(
                    new Internals.Async.ContextualAsyncProcessByStateHandler<
                        TState,
                        TRequest
                    >(State, process),
                    logger
                );
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public ProcessRegistry Register<TRequest, TResult>(
                [NotNull] Func<
                      TState
                    , TRequest
                    , ProcessingContext
                    , UnityTask<TResult>
                > process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest<TResult>
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return default;
                }
#endif

                return _hub.Register(
                    new Internals.Async.ContextualAsyncProcessByStateHandler<TState, TRequest, TResult>(State, process),
                    logger
                );
            }

#pragma warning disable IDE0051
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings_Async();
#pragma warning restore IDE0051
        }
    }
}
