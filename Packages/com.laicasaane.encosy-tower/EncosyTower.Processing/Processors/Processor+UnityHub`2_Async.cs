using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;

namespace EncosyTower.Processing
{
    partial class Processor
    {
        public readonly partial struct UnityHub<TScope, TState>
            where TScope : UnityEngine.Object
            where TState : class
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest>(
                  [NotNull] Func<TState, TRequest, UnityTask> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
                => new Hub<UnityEntityId<TScope>, TState>(_hub, State).Register(process, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest, TResult>(
                  [NotNull] Func<TState, TRequest, UnityTask<TResult>> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest<TResult>
#endif
                => new Hub<UnityEntityId<TScope>, TState>(_hub, State).Register(process, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest>(
                  [NotNull] Func<TState, TRequest, ProcessingContext, UnityTask> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
                => new Hub<UnityEntityId<TScope>, TState>(_hub, State).Register(process, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest, TResult>(
                  [NotNull] Func<TState, TRequest, ProcessingContext, UnityTask<TResult>> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest<TResult>
#endif
                => new Hub<UnityEntityId<TScope>, TState>(_hub, State).Register(process, logger);
        }
    }
}
