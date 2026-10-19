using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Types;
using EncosyTower.UnityExtensions;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing
{
    partial class Processor
    {
        public readonly partial struct UnityHub<TScope, TState> : IIsCreated
            where TScope : UnityEngine.Object
            where TState : class
        {
            internal readonly Hub<UnityEntityId<TScope>> _hub;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnityHub(in UnityHub<TScope> hub, [NotNull] TState state)
            {
                DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);

                _hub = hub._hub;
                State = state;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnityHub(in Hub<UnityEntityId<TScope>> hub, [NotNull] TState state)
            {
                DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);

                _hub = hub;
                State = state;
            }

            public bool IsCreated => _hub.IsCreated;

            public UnityEntityId<TScope> Scope => _hub.Scope;

            public TState State { get; }

            public ICollection<ProcessRegistry> Registries => _hub.Registries;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Hub<TNewScope, TState> WithScope<TNewScope>()
                where TNewScope : struct
                => _hub.WithScope<TNewScope>().WithState(State);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Hub<TNewScope, TState> WithScope<TNewScope>(TNewScope scope)
                => _hub.WithScope(scope).WithState(State);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Hub<GlobalScope, TState> WithGlobalScope()
                => _hub.WithGlobalScope().WithState(State);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityHub<TNewScope, TState> WithUnityScope<TNewScope>([NotNull] TNewScope scope)
                where TNewScope : UnityEngine.Object
                => _hub.WithUnityScope(scope).WithState(State);

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                  [NotNull] Action<TState, TRequest> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest
#endif
                => new Hub<UnityEntityId<TScope>, TState>(_hub, State).Register(process, logger);

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                  [NotNull] Action<TState, TRequest, ProcessingContext> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest
#endif
                => new Hub<UnityEntityId<TScope>, TState>(_hub, State).Register(process, logger);

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                  [NotNull] Func<TState, TRequest, TResult> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest<TResult>
#endif
                => new Hub<UnityEntityId<TScope>, TState>(_hub, State).Register(process, logger);

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                  [NotNull] Func<TState, TRequest, ProcessingContext, TResult> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest<TResult>
#endif
                => new Hub<UnityEntityId<TScope>, TState>(_hub, State).Register(process, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Unregister(TypeId id, ILogger logger = null)
                => _hub.Unregister(id, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear(ILogger logger = null)
                => _hub.Clear(logger);
        }
    }
}
