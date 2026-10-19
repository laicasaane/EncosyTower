using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
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
            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                  [NotNull] Func<TState, TRequest, UnityTask> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
                => new Hub<UnityEntityId<TScope>, TState>(_hub, State).Register(process, logger);

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                  [NotNull] Func<TState, TRequest, UnityTask<TResult>> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest<TResult>
#endif
                => new Hub<UnityEntityId<TScope>, TState>(_hub, State).Register(process, logger);

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                  [NotNull] Func<TState, TRequest, ProcessingContext, UnityTask> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
                => new Hub<UnityEntityId<TScope>, TState>(_hub, State).Register(process, logger);

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
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
