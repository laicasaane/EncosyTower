#if DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#elif (UNITY_EDITOR || DEBUG) && !DISABLE_ENCOSY_EDITOR_CHECKS
#define __ENCOSY_VALIDATION__
#elif (ENCOSY_RUNTIME_CHECKS || ENCOSY_PROCESSING_RUNTIME_CHECKS) && !DISABLE_ENCOSY_RUNTIME_CHECKS
#define __ENCOSY_VALIDATION__
#else
#define __ENCOSY_NO_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
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
            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                  [NotNull] Func<TState, TRequest, UnityTask> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope, TState>));

                return _hub.Register(new Internals.Async.AsyncProcessByStateHandler<TState, TRequest>(State, process));
            }

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
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
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope, TState>));

                return _hub.Register(
                      new Internals.Async.AsyncProcessByStateHandler<TState, TRequest, TResult>(State, process)
                );
            }

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public Result<ProcessRegistry, ProcessError> Register<TRequest>(
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
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope, TState>));

                return _hub.Register(
                      new Internals.Async.ContextualAsyncProcessByStateHandler<TState, TRequest>(State, process)
                );
            }

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
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
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope, TState>));

                return _hub.Register(
                      new Internals.Async.ContextualAsyncProcessByStateHandler<TState, TRequest, TResult>(
                            State
                          , process
                      )
                );
            }

#pragma warning disable IDE0051
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings_Async();
#pragma warning restore IDE0051
        }
    }
}
