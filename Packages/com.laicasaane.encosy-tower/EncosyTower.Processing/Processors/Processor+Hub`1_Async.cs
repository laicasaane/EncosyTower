using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Processing.Internals;
using EncosyTower.Tasks;
using EncosyTower.Types;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing
{
    partial class Processor
    {
        public readonly partial struct Hub<TScope>
        {
            #region    REGISTER - ASYNC
            #endregion ================

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                  [NotNull] Func<TRequest, UnityTask> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Async.AsyncProcessHandler<TRequest>(process));
            }

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                [NotNull] Func<
                      TRequest
                    , UnityTask<TResult>
                > process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest<TResult>
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Async.AsyncProcessHandler<TRequest, TResult>(process));
            }

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                  [NotNull] Func<TRequest, ProcessingContext, UnityTask> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Async.ContextualAsyncProcessHandler<TRequest>(process));
            }

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                [NotNull] Func<
                      TRequest
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

                return Register(new Internals.Async.ContextualAsyncProcessHandler<TRequest, TResult>(process));
            }

            #region    PROCESS - ASYNC
            #endregion ===============

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask ProcessAsync<TRequest>(TRequest request, ProcessingContext context = default)
            {
                IAsyncProcessHandler<TRequest> handler = null;
                IProcessHandler candidate = null;
                var found = IsCreated && TryGet(out handler, out candidate);

                if (found)
                {
                    var handlerResult = handler.ProcessAsync(request, context);
                    ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                    if (handlerResult.IsError)
                    {
                        var error = handlerResult.GetErrorOrDefault();
                        _map.Unregister(handler);

                        if (context.Strategy == ProcessingStrategy.WaitForHandler)
                        {
                            return WaitThenProcessDirectAsync(this, request, context);
                        }

                        ThrowHelper.ThrowStateUnavailable(error.StateType);
                    }

                    return handlerResult.GetValueOrDefault();
                }

                if (IsCreated && context.Strategy == ProcessingStrategy.WaitForHandler)
                {
                    return WaitThenProcessDirectAsync(this, request, context);
                }

                ThrowHelper.ThrowIfHandlerIsNotFound(found, Scope, candidate != null, handler);
                return default;
            }

            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async UnityTask<Success<ProcessError>> TryProcessAsync<TRequest>(
                  TRequest request
                , ProcessingContext context = default
            )
            {
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope>));

                if (TryGet(out IAsyncProcessHandler<TRequest> handler, out var candidate))
                {
                    var handlerResult = handler.ProcessAsync(request, context);
                    ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                    if (handlerResult.IsError)
                    {
                        var error = handlerResult.GetErrorOrDefault();
                        _map.Unregister(handler);

                        if (context.Strategy == ProcessingStrategy.WaitForHandler)
                        {
                            return await WaitThenTryProcessAsync(this, request, context);
                        }

                        return ProcessError.CreateStateUnavailable(error.StateType);
                    }

                    await handlerResult.GetValueOrDefault();
                    return Success.Yes;
                }

                if (context.Strategy == ProcessingStrategy.WaitForHandler)
                {
                    return await WaitThenTryProcessAsync(this, request, context);
                }

                return ProcessError.CreateLookupFailed<TScope, TRequest>(candidate);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask<TResult> ProcessAsync<TRequest, TResult>(
                  TRequest request
                , ProcessingContext context = default
            )
            {
                IAsyncProcessHandler<TRequest, TResult> handler = null;
                IProcessHandler candidate = null;
                var found = IsCreated && TryGet(out handler, out candidate);

                if (found)
                {
                    var handlerResult = handler.ProcessAsync(request, context);
                    ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                    if (handlerResult.IsError)
                    {
                        var error = handlerResult.GetErrorOrDefault();
                        _map.Unregister(handler);

                        if (context.Strategy == ProcessingStrategy.WaitForHandler)
                        {
                            return WaitThenProcessDirectAsync<TRequest, TResult>(this, request, context);
                        }

                        ThrowHelper.ThrowStateUnavailable(error.StateType);
                    }

                    return handlerResult.GetValueOrDefault();
                }

                if (IsCreated && context.Strategy == ProcessingStrategy.WaitForHandler)
                {
                    return WaitThenProcessDirectAsync<TRequest, TResult>(this, request, context);
                }

                ThrowHelper.ThrowIfHandlerIsNotFound(found, Scope, candidate != null, handler);
                return default;
            }

            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async UnityTask<Result<TResult, ProcessError>> TryProcessAsync<TRequest, TResult>(
                  TRequest request
                , ProcessingContext context = default
            )
            {
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope>));

                if (TryGet(out IAsyncProcessHandler<TRequest, TResult> handler, out var candidate))
                {
                    var handlerResult = handler.ProcessAsync(request, context);
                    ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                    if (handlerResult.IsError)
                    {
                        var error = handlerResult.GetErrorOrDefault();
                        _map.Unregister(handler);

                        if (context.Strategy == ProcessingStrategy.WaitForHandler)
                        {
                            return await WaitThenTryProcessAsync<TRequest, TResult>(this, request, context);
                        }

                        return ProcessError.CreateStateUnavailable(error.StateType);
                    }

                    return Result<TResult, ProcessError>.Succeed(await handlerResult.GetValueOrDefault());
                }

                if (context.Strategy == ProcessingStrategy.WaitForHandler)
                {
                    return await WaitThenTryProcessAsync<TRequest, TResult>(this, request, context);
                }

                return ProcessError.CreateLookupFailed<TScope, TRequest, TResult>(candidate);
            }

            #region    TRY GET - ASYNC
            #endregion ===============

            private bool TryGet<TRequest>(out IAsyncProcessHandler<TRequest> result, out IProcessHandler candidate)
            {
                var id = (TypeId)Type<Func<TRequest, ProcessingContext, UnityTask>>.Id;

                if (TryGet(id, out candidate) && candidate is IAsyncProcessHandler<TRequest> handler)
                {
                    result = handler;
                    return true;
                }

                result = null;
                return false;
            }

            private bool TryGet<TRequest, TResult>(
                  out IAsyncProcessHandler<TRequest, TResult> result
                , out IProcessHandler candidate
            )
            {
                var id = (TypeId)Type<Func<TRequest, ProcessingContext, UnityTask<TResult>>>.Id;

                if (TryGet(id, out candidate) && candidate is IAsyncProcessHandler<TRequest, TResult> handler)
                {
                    result = handler;
                    return true;
                }

                result = null;
                return false;
            }

            #region    HELPERS - ASYNC
            #endregion ==============

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool ContainsAsyncHandler<TRequest>()
                => TryGet(out IAsyncProcessHandler<TRequest> _, out _);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool ContainsAsyncHandler<TRequest, TResult>()
                => TryGet(out IAsyncProcessHandler<TRequest, TResult> _, out _);

            private static async UnityTask WaitThenProcessDirectAsync<TRequest>(
                  Hub<TScope> hub
                , TRequest request
                , ProcessingContext context
            )
            {
                while (true)
                {
                    await UnityTask.WaitUntil(
                          hub
                        , static x => x.ContainsAsyncHandler<TRequest>()
                        , context.Token
                    );

                    context.Token.ThrowIfCancellationRequested();

                    if (hub.TryGet(out IAsyncProcessHandler<TRequest> handler, out _) == false)
                    {
                        continue;
                    }

                    var handlerResult = handler.ProcessAsync(request, context);
                    ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                    if (handlerResult.IsError)
                    {
                        hub._map.Unregister(handler);
                        continue;
                    }

                    await handlerResult.GetValueOrDefault();
                    return;
                }
            }

            private static async UnityTask<TResult> WaitThenProcessDirectAsync<TRequest, TResult>(
                  Hub<TScope> hub
                , TRequest request
                , ProcessingContext context
            )
            {
                while (true)
                {
                    await UnityTask.WaitUntil(
                          hub
                        , static x => x.ContainsAsyncHandler<TRequest, TResult>()
                        , context.Token
                    );

                    context.Token.ThrowIfCancellationRequested();

                    if (hub.TryGet(out IAsyncProcessHandler<TRequest, TResult> handler, out _) == false)
                    {
                        continue;
                    }

                    var handlerResult = handler.ProcessAsync(request, context);
                    ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                    if (handlerResult.IsError)
                    {
                        hub._map.Unregister(handler);
                        continue;
                    }

                    return await handlerResult.GetValueOrDefault();
                }
            }

            private static async UnityTask<Success<ProcessError>> WaitThenTryProcessAsync<TRequest>(
                  Hub<TScope> hub
                , TRequest request
                , ProcessingContext context
            )
            {
                while (true)
                {
                    await UnityTask.WaitUntil(
                          hub
                        , static x => x.ContainsAsyncHandler<TRequest>()
                        , context.Token
                    );

                    context.Token.ThrowIfCancellationRequested();

                    if (hub.TryGet(out IAsyncProcessHandler<TRequest> handler, out _) == false)
                    {
                        continue;
                    }

                    var handlerResult = handler.ProcessAsync(request, context);
                    ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                    if (handlerResult.IsError)
                    {
                        hub._map.Unregister(handler);
                        continue;
                    }

                    await handlerResult.GetValueOrDefault();
                    return Success.Yes;
                }
            }

            private static async UnityTask<Result<TResult, ProcessError>> WaitThenTryProcessAsync<TRequest, TResult>(
                  Hub<TScope> hub
                , TRequest request
                , ProcessingContext context
            )
            {
                while (true)
                {
                    await UnityTask.WaitUntil(
                          hub
                        , static x => x.ContainsAsyncHandler<TRequest, TResult>()
                        , context.Token
                    );

                    context.Token.ThrowIfCancellationRequested();

                    if (hub.TryGet(out IAsyncProcessHandler<TRequest, TResult> handler, out _) == false)
                    {
                        continue;
                    }

                    var handlerResult = handler.ProcessAsync(request, context);
                    ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                    if (handlerResult.IsError)
                    {
                        hub._map.Unregister(handler);
                        continue;
                    }

                    return Result<TResult, ProcessError>.Succeed(await handlerResult.GetValueOrDefault());
                }
            }
        }
    }
}
