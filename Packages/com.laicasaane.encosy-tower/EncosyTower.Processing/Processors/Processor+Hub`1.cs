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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Processing.Internals;
using EncosyTower.Types;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing
{
    public static partial class ProcessorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Processor.Hub<TScope> WithRegistries<TScope>(
              this in Processor.Hub<TScope> hub
            , ICollection<ProcessRegistry> registries
        )
        {
            return new Processor.Hub<TScope>(hub.Scope, hub._map, registries ?? EmptyRegistries.Default);
        }
    }

    partial class Processor
    {
        public readonly partial struct Hub<TScope> : IIsCreated
        {
            internal readonly ProcessHandlerMap _map;
            internal readonly ICollection<ProcessRegistry> _registries;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Hub(TScope scope, [NotNull] ProcessHandlerMap map)
            {
                DebuggingThrowHelper.ThrowIfNull(map);

                _map = map;
                _registries = EmptyRegistries.Default;
                Scope = scope;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Hub(
                  TScope scope
                , [NotNull] ProcessHandlerMap map
                , [NotNull] ICollection<ProcessRegistry> registries
            )
            {
                DebuggingThrowHelper.ThrowIfNull(map);
                DebuggingThrowHelper.ThrowIfNull(registries);

                _map = map;
                _registries = registries;
                Scope = scope;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Hub(TScope scope, ProcessHandlerMap map, ICollection<ProcessRegistry> registries, bool _)
            {
                _map = map;
                _registries = registries;
                Scope = scope;
            }

            public bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _map != null;
            }

            public TScope Scope { get; }

            public ICollection<ProcessRegistry> Registries => _registries ?? EmptyRegistries.Default;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Hub<TNewScope> WithScope<TNewScope>()
                where TNewScope : struct
            {
#if __ENCOSY_VALIDATION__
                if (IsCreated == false)
                {
                    return CreateInvalid(default(TNewScope));
                }
#endif

                return _map.Owner.Scope<TNewScope>().WithRegistries(Registries);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Hub<TNewScope> WithScope<TNewScope>(TNewScope scope)
            {
#if __ENCOSY_VALIDATION__
                if (IsCreated == false)
                {
                    return CreateInvalid(scope);
                }
#endif

                return _map.Owner.Scope(scope).WithRegistries(Registries);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Hub<GlobalScope> WithGlobalScope()
            {
#if __ENCOSY_VALIDATION__
                if (IsCreated == false)
                {
                    return CreateInvalid(default(GlobalScope));
                }
#endif

                return _map.Owner.Global().WithRegistries(Registries);
            }

            #region    REGISTER - SYNC
            #endregion ===============

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                  [NotNull] Action<TRequest> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Sync.ProcessHandler<TRequest>(process));
            }

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                  [NotNull] Action<TRequest, ProcessingContext> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Sync.ContextualProcessHandler<TRequest>(process));
            }

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                  [NotNull] Func<TRequest, TResult> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest<TResult>
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Sync.ProcessHandler<TRequest, TResult>(process));
            }

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                  [NotNull] Func<TRequest, ProcessingContext, TResult> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest<TResult>
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Sync.ContextualProcessHandler<TRequest, TResult>(process));
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            internal Result<ProcessRegistry, ProcessError> Register(IProcessHandler handler)
            {
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope>));

                var result = _map.Register(handler);

                if (result.TryGetValue(out var registry))
                {
                    Registries.Add(registry);
                }

                return result;
            }

            #region    UNREGISTER - TYPE ID
            #endregion ====================

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public bool Unregister(TypeId id, ILogger logger = null)
            {
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope>));

                return _map.Unregister(id);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Clear(ILogger logger = null)
            {
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope>));

                _map.Clear();
            }

            #region    PROCESS - SYNC
            #endregion ==============

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Process<TRequest>(TRequest request, ProcessingContext context = default)
            {
                IProcessHandler<TRequest> handler = null;
                IProcessHandler candidate = null;
                var found = IsCreated && TryGet(out handler, out candidate);

                ThrowHelper.ThrowIfHandlerIsNotFound(found, Scope, candidate != null, handler);

                var handlerResult = handler.Process(request, context);
                ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                if (handlerResult.IsError)
                {
                    var error = handlerResult.GetErrorOrDefault();
                    _map.Unregister(handler);
                    ThrowHelper.ThrowStateUnavailable(error.StateType);
                }
            }

            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Success<ProcessError> TryProcess<TRequest>(TRequest request, ProcessingContext context = default)
            {
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope>));

                if (TryGet(out IProcessHandler<TRequest> handler, out var candidate) == false)
                {
                    return ProcessError.CreateLookupFailed<TScope, TRequest>(candidate);
                }

                var handlerResult = handler.Process(request, context);
                ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                if (handlerResult.IsError)
                {
                    var error = handlerResult.GetErrorOrDefault();
                    _map.Unregister(handler);

                    return ProcessError.CreateStateUnavailable(error.StateType);
                }

                return Success.Yes;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TResult Process<TRequest, TResult>(TRequest request, ProcessingContext context = default)
            {
                IProcessHandler<TRequest, TResult> handler = null;
                IProcessHandler candidate = null;
                var found = IsCreated && TryGet(out handler, out candidate);

                ThrowHelper.ThrowIfHandlerIsNotFound(found, Scope, candidate != null, handler);

                var handlerResult = handler.Process(request, context);
                ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                if (handlerResult.IsError)
                {
                    var error = handlerResult.GetErrorOrDefault();
                    _map.Unregister(handler);
                    ThrowHelper.ThrowStateUnavailable(error.StateType);
                }

                return handlerResult.GetValueOrDefault();
            }

            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<TResult, ProcessError> TryProcess<TRequest, TResult>(
                  TRequest request
                , ProcessingContext context = default
            )
            {
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope>));

                if (TryGet(out IProcessHandler<TRequest, TResult> handler, out var candidate) == false)
                {
                    return ProcessError.CreateLookupFailed<TScope, TRequest, TResult>(candidate);
                }

                var handlerResult = handler.Process(request, context);
                ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                if (handlerResult.IsError)
                {
                    var error = handlerResult.GetErrorOrDefault();
                    _map.Unregister(handler);

                    return ProcessError.CreateStateUnavailable(error.StateType);
                }

                return Result<TResult, ProcessError>.Succeed(handlerResult.GetValueOrDefault());
            }

            #region    TRY GET - SYNC
            #endregion ==============

            private bool TryGet<TRequest>(out IProcessHandler<TRequest> result, out IProcessHandler candidate)
            {
                var id = (TypeId)Type<Action<TRequest, ProcessingContext>>.Id;

                if (TryGet(id, out candidate) && candidate is IProcessHandler<TRequest> handler)
                {
                    result = handler;
                    return true;
                }

                result = null;
                return false;
            }

            private bool TryGet<TRequest, TResult>(
                  out IProcessHandler<TRequest, TResult> result
                , out IProcessHandler candidate
            )
            {
                var id = (TypeId)Type<Func<TRequest, ProcessingContext, TResult>>.Id;

                if (TryGet(id, out candidate) && candidate is IProcessHandler<TRequest, TResult> handler)
                {
                    result = handler;
                    return true;
                }

                result = null;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool TryGet(TypeId typeId, out IProcessHandler handler)
            {
                if (_map != null)
                {
                    return _map.TryGet(typeId, out handler);
                }

                handler = null;
                return false;
            }

            #region    HELPERS - SYNC
            #endregion ==============

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Hub<TNewScope> CreateInvalid<TNewScope>(TNewScope scope)
                => new(scope, null, _registries, true);
        }
    }
}
