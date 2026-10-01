#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS || ENCOSY_PROCESSING_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest>(
                  [NotNull] Func<TRequest, UnityTask> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Async.AsyncProcessHandler<TRequest>(process), logger);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest, TResult>(
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

                return Register(new Internals.Async.AsyncProcessHandler<TRequest, TResult>(process), logger);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest>(
                  [NotNull] Func<TRequest, ProcessingContext, UnityTask> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Async.ContextualAsyncProcessHandler<TRequest>(process), logger);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest, TResult>(
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

                return Register(new Internals.Async.ContextualAsyncProcessHandler<TRequest, TResult>(process), logger);
            }

            #region    PROCESS - ASYNC
            #endregion ===============

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask ProcessAsync<TRequest>(TRequest request, ProcessingContext context = default)
            {
                IAsyncProcessHandler<TRequest> handler = null;
                var hasCandidate = false;
                var found = IsCreated && TryGet(out handler, out hasCandidate);

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

                ThrowHelper.ThrowIfHandlerIsNotFound(found, Scope, hasCandidate, handler);
                return default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async UnityTask<bool> TryProcessAsync<TRequest>(
                  TRequest request
                , ProcessingContext context = default
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(context.Logger) == false)
                {
                    return false;
                }
#endif

                if (IsCreated == false)
                {
                    return false;
                }

                if (TryGet(out IAsyncProcessHandler<TRequest> handler, out var hasCandidate))
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

                        if (context.WarnNoHandler)
                        {
                            ThrowHelper.LogErrorStateUnavailable(error.StateType, context.Logger);
                        }

                        return false;
                    }

                    await handlerResult.GetValueOrDefault();
                    return true;
                }

                if (context.Strategy == ProcessingStrategy.WaitForHandler)
                {
                    return await WaitThenTryProcessAsync(this, request, context);
                }

                if (context.WarnNoHandler)
                {
                    ThrowHelper.LogErrorHandlerNotFound(Scope, hasCandidate, handler, context.Logger);
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask<TResult> ProcessAsync<TRequest, TResult>(
                  TRequest request
                , ProcessingContext context = default
            )
            {
                IAsyncProcessHandler<TRequest, TResult> handler = null;
                var hasCandidate = false;
                var found = IsCreated && TryGet(out handler, out hasCandidate);

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

                ThrowHelper.ThrowIfHandlerIsNotFound(found, Scope, hasCandidate, handler);
                return default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async UnityTask<Option<TResult>> TryProcessAsync<TRequest, TResult>(
                  TRequest request
                , ProcessingContext context = default
            )
            {
#if __ENCOSY_VALIDATION__
                if (Validate(context.Logger) == false)
                {
                    return Option.None;
                }
#endif

                if (IsCreated == false)
                {
                    return Option.None;
                }

                if (TryGet(out IAsyncProcessHandler<TRequest, TResult> handler, out var hasCandidate))
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

                        if (context.WarnNoHandler)
                        {
                            ThrowHelper.LogErrorStateUnavailable(error.StateType, context.Logger);
                        }

                        return Option.None;
                    }

                    return Option.Some(await handlerResult.GetValueOrDefault());
                }

                if (context.Strategy == ProcessingStrategy.WaitForHandler)
                {
                    return await WaitThenTryProcessAsync<TRequest, TResult>(this, request, context);
                }

                if (context.WarnNoHandler)
                {
                    ThrowHelper.LogErrorHandlerNotFound(Scope, hasCandidate, handler, context.Logger);
                }

                return Option.None;
            }

            #region    TRY GET - ASYNC
            #endregion ===============

            private bool TryGet<TRequest>(out IAsyncProcessHandler<TRequest> result, out bool hasCandidate)
            {
                var id = (TypeId)Type<Func<TRequest, ProcessingContext, UnityTask>>.Id;

                if (TryGet(id, out var candidate))
                {
                    hasCandidate = true;

                    if (candidate is IAsyncProcessHandler<TRequest> handler)
                    {
                        result = handler;
                        return true;
                    }
                }
                else
                {
                    hasCandidate = false;
                }

                result = null;
                return false;
            }

            private bool TryGet<TRequest, TResult>(
                  out IAsyncProcessHandler<TRequest, TResult> result
                , out bool hasCandidate
            )
            {
                var id = (TypeId)Type<Func<TRequest, ProcessingContext, UnityTask<TResult>>>.Id;

                if (TryGet(id, out var candidate))
                {
                    hasCandidate = true;

                    if (candidate is IAsyncProcessHandler<TRequest, TResult> handler)
                    {
                        result = handler;
                        return true;
                    }
                }
                else
                {
                    hasCandidate = false;
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

            private static async UnityTask<bool> WaitThenTryProcessAsync<TRequest>(
                  Hub<TScope> hub
                , TRequest request
                , ProcessingContext context
            )
            {
                while (true)
                {
                    try
                    {
                        await UnityTask.WaitUntil(
                              hub
                            , static x => x.ContainsAsyncHandler<TRequest>()
                            , context.Token
                        );

                        context.Token.ThrowIfCancellationRequested();
                    }
                    catch (OperationCanceledException) when (context.Token.IsCancellationRequested)
                    {
                        return false;
                    }

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
                    return true;
                }
            }

            private static async UnityTask<Option<TResult>> WaitThenTryProcessAsync<TRequest, TResult>(
                  Hub<TScope> hub
                , TRequest request
                , ProcessingContext context
            )
            {
                while (true)
                {
                    try
                    {
                        await UnityTask.WaitUntil(
                              hub
                            , static x => x.ContainsAsyncHandler<TRequest, TResult>()
                            , context.Token
                        );

                        context.Token.ThrowIfCancellationRequested();
                    }
                    catch (OperationCanceledException) when (context.Token.IsCancellationRequested)
                    {
                        return Option.None;
                    }

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

                    return Option.Some(await handlerResult.GetValueOrDefault());
                }
            }
        }
    }
}
