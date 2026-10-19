using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Tasks;

namespace EncosyTower.Processing
{
    partial class Processor
    {
        public readonly partial struct UnityHub<TScope>
            where TScope : UnityEngine.Object
        {
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
                => _hub.Register(process, logger);

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                  [NotNull] Func<TRequest, UnityTask<TResult>> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest<TResult>
#endif
                => _hub.Register(process, logger);

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
                => _hub.Register(process, logger);

            /// <param name="logger">Unused; kept for source compatibility.</param>
            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                  [NotNull] Func<TRequest, ProcessingContext, UnityTask<TResult>> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest<TResult>
#endif
                => _hub.Register(process, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask ProcessAsync<TRequest>(TRequest request, ProcessingContext context = default)
                => _hub.ProcessAsync(request, context);

            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask<Success<ProcessError>> TryProcessAsync<TRequest>(
                  TRequest request
                , ProcessingContext context = default
            )
                => _hub.TryProcessAsync(request, context);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask<TResult> ProcessAsync<TRequest, TResult>(
                  TRequest request
                , ProcessingContext context = default
            )
                => _hub.ProcessAsync<TRequest, TResult>(request, context);

            /// <remarks>
            /// Returns the failure instead of logging it; the caller logs each failed result it reacts to.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask<Result<TResult, ProcessError>> TryProcessAsync<TRequest, TResult>(
                  TRequest request
                , ProcessingContext context = default
            )
                => _hub.TryProcessAsync<TRequest, TResult>(request, context);
        }
    }
}
