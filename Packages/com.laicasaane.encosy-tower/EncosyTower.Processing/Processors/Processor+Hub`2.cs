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
using EncosyTower.Types;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing
{
    public static partial class ProcessorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Processor.Hub<TScope, TState> WithState<TScope, TState>(
              this in Processor.Hub<TScope> hub
            , [NotNull] TState state
        )
            where TState : class
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);

            return new Processor.Hub<TScope, TState>(hub, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Processor.Hub<TScope, TState> WithRegistries<TScope, TState>(
              this in Processor.Hub<TScope, TState> hub
            , ICollection<ProcessRegistry> registries
        )
            where TState : class
        {
            return new Processor.Hub<TScope, TState>(hub._hub.WithRegistries(registries), hub.State);
        }
    }

    partial class Processor
    {
        public readonly partial struct Hub<TScope, TState> : IIsCreated
            where TState : class
        {
            internal readonly Hub<TScope> _hub;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Hub(in Hub<TScope> hub, [NotNull] TState state)
            {
                DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);

                _hub = hub;
                State = state;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Hub(in Hub<TScope> hub, TState state, bool _)
            {
                _hub = hub;
                State = state;
            }

            public bool IsCreated => _hub.IsCreated;

            public TScope Scope => _hub.Scope;

            public TState State { get; }

            public ICollection<ProcessRegistry> Registries => _hub.Registries;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Hub<TNewScope, TState> WithScope<TNewScope>()
                where TNewScope : struct
            {
#if __ENCOSY_VALIDATION__
                if (IsCreated == false)
                {
                    return CreateInvalid(_hub.WithScope<TNewScope>());
                }
#endif

                return _hub.WithScope<TNewScope>().WithState(State);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Hub<TNewScope, TState> WithScope<TNewScope>(TNewScope scope)
            {
#if __ENCOSY_VALIDATION__
                if (IsCreated == false)
                {
                    return CreateInvalid(_hub.WithScope(scope));
                }
#endif

                return _hub.WithScope(scope).WithState(State);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Hub<GlobalScope, TState> WithGlobalScope()
            {
#if __ENCOSY_VALIDATION__
                if (IsCreated == false)
                {
                    return CreateInvalid(_hub.WithGlobalScope());
                }
#endif

                return _hub.WithGlobalScope().WithState(State);
            }

            #region    REGISTER - SYNC
            #endregion ===============

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                  [NotNull] Action<TState, TRequest> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope, TState>));

                return _hub.Register(new Internals.Sync.ProcessByStateHandler<TState, TRequest>(State, process));
            }

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                  [NotNull] Action<TState, TRequest, ProcessingContext> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope, TState>));

                return _hub.Register(
                      new Internals.Sync.ContextualProcessByStateHandler<TState, TRequest>(State, process)
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
                  [NotNull] Func<TState, TRequest, TResult> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest<TResult>
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope, TState>));

                return _hub.Register(
                      new Internals.Sync.ProcessByStateHandler<TState, TRequest, TResult>(State, process)
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
                  [NotNull] Func<TState, TRequest, ProcessingContext, TResult> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest<TResult>
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(process);
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope, TState>));

                return _hub.Register(
                      new Internals.Sync.ContextualProcessByStateHandler<TState, TRequest, TResult>(State, process)
                );
            }

            #region    UNREGISTER - TYPE ID
            #endregion ====================

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public bool Unregister(TypeId id, ILogger logger = null)
            {
                ThrowHelper.ThrowIfHubNotCreated<TScope>(IsCreated, typeof(Hub<TScope, TState>));

                return _hub.Unregister(id, logger);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear(ILogger logger = null)
                => _hub.Clear(logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Hub<TNewScope, TState> CreateInvalid<TNewScope>(in Hub<TNewScope> hub)
                    => new(hub, State, true);
        }
    }
}
