#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS || ENCOSY_PROCESSING_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest>(
                  [NotNull] Action<TRequest> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Sync.ProcessHandler<TRequest>(process), logger);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest>(
                  [NotNull] Action<TRequest, ProcessingContext> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Sync.ContextualProcessHandler<TRequest>(process), logger);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest, TResult>(
                  [NotNull] Func<TRequest, TResult> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest<TResult>
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Sync.ProcessHandler<TRequest, TResult>(process), logger);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest, TResult>(
                  [NotNull] Func<TRequest, ProcessingContext, TResult> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest<TResult>
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);

                return Register(new Internals.Sync.ContextualProcessHandler<TRequest, TResult>(process), logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            internal ProcessRegistry Register(IProcessHandler handler, ILogger logger)
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return default;
                }
#endif

                var registry = _map.Register(handler, logger);
                Registries.Add(registry);

                return registry;
            }

            #region    UNREGISTER - TYPE ID
            #endregion ====================

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public bool Unregister(TypeId id, ILogger logger = null)
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return false;
                }
#endif

                return _map.Unregister(id);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Clear(ILogger logger = null)
            {
#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return;
                }
#endif

                _map.Clear();
            }

            #region    PROCESS - SYNC
            #endregion ==============

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Process<TRequest>(TRequest request, ProcessingContext context = default)
            {
                IProcessHandler<TRequest> handler = null;
                var hasCandidate = false;
                var found = IsCreated && TryGet(out handler, out hasCandidate);

                ThrowHelper.ThrowIfHandlerIsNotFound(found, Scope, hasCandidate, handler);

                var handlerResult = handler.Process(request, context);
                ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                if (handlerResult.IsError)
                {
                    var error = handlerResult.GetErrorOrDefault();
                    _map.Unregister(handler);
                    ThrowHelper.ThrowStateUnavailable(error.StateType);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryProcess<TRequest>(TRequest request, ProcessingContext context = default)
            {
#if __ENCOSY_VALIDATION__
                if (Validate(context.Logger) == false)
                {
                    return false;
                }
#endif

                if (TryGet(out IProcessHandler<TRequest> handler, out var hasCandidate) == false)
                {
                    if (context.WarnNoHandler)
                    {
                        ThrowHelper.LogErrorHandlerNotFound(Scope, hasCandidate, handler, context.Logger);
                    }

                    return false;
                }

                var handlerResult = handler.Process(request, context);
                ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                if (handlerResult.IsError)
                {
                    var error = handlerResult.GetErrorOrDefault();
                    _map.Unregister(handler);

                    if (context.WarnNoHandler)
                    {
                        ThrowHelper.LogErrorStateUnavailable(error.StateType, context.Logger);
                    }

                    return false;
                }

                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TResult Process<TRequest, TResult>(TRequest request, ProcessingContext context = default)
            {
                IProcessHandler<TRequest, TResult> handler = null;
                var hasCandidate = false;
                var found = IsCreated && TryGet(out handler, out hasCandidate);

                ThrowHelper.ThrowIfHandlerIsNotFound(found, Scope, hasCandidate, handler);

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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Option<TResult> TryProcess<TRequest, TResult>(TRequest request, ProcessingContext context = default)
            {
#if __ENCOSY_VALIDATION__
                if (Validate(context.Logger) == false)
                {
                    return Option.None;
                }
#endif

                if (TryGet(out IProcessHandler<TRequest, TResult> handler, out var hasCandidate) == false)
                {
                    if (context.WarnNoHandler)
                    {
                        ThrowHelper.LogErrorHandlerNotFound(Scope, hasCandidate, handler, context.Logger);
                    }

                    return Option.None;
                }

                var handlerResult = handler.Process(request, context);
                ThrowHelper.ThrowIfHandlerResultIsInvalid(handlerResult.IsValid, handler.GetType());

                if (handlerResult.IsError)
                {
                    var error = handlerResult.GetErrorOrDefault();
                    _map.Unregister(handler);

                    if (context.WarnNoHandler)
                    {
                        ThrowHelper.LogErrorStateUnavailable(error.StateType, context.Logger);
                    }

                    return Option.None;
                }

                return Option.Some(handlerResult.GetValueOrDefault());
            }

            #region    TRY GET - SYNC
            #endregion ==============

            private bool TryGet<TRequest>(out IProcessHandler<TRequest> result, out bool hasCandidate)
            {
                var id = (TypeId)Type<Action<TRequest, ProcessingContext>>.Id;

                if (TryGet(id, out var candidate))
                {
                    hasCandidate = true;

                    if (candidate is IProcessHandler<TRequest> handler)
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

            private bool TryGet<TRequest, TResult>(out IProcessHandler<TRequest, TResult> result, out bool hasCandidate)
            {
                var id = (TypeId)Type<Func<TRequest, ProcessingContext, TResult>>.Id;

                if (TryGet(id, out var candidate))
                {
                    hasCandidate = true;

                    if (candidate is IProcessHandler<TRequest, TResult> handler)
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

#if __ENCOSY_VALIDATION__
            private bool Validate(ILogger logger)
            {
                if (_map != null)
                {
                    return true;
                }

                ThrowHelper.LogErrorInvalidHub<TScope>(logger);
                return false;
            }
#endif
        }
    }
}
