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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest>(
                  [NotNull] Func<TRequest, UnityTask> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
                => _hub.Register(process, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest, TResult>(
                  [NotNull] Func<TRequest, UnityTask<TResult>> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest<TResult>
#endif
                => _hub.Register(process, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest>(
                  [NotNull] Func<TRequest, ProcessingContext, UnityTask> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IAsyncRequest
#endif
                => _hub.Register(process, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest, TResult>(
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask<bool> TryProcessAsync<TRequest>(TRequest request, ProcessingContext context = default)
                => _hub.TryProcessAsync(request, context);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask<TResult> ProcessAsync<TRequest, TResult>(
                  TRequest request
                , ProcessingContext context = default
            )
                => _hub.ProcessAsync<TRequest, TResult>(request, context);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityTask<Option<TResult>> TryProcessAsync<TRequest, TResult>(
                  TRequest request
                , ProcessingContext context = default
            )
                => _hub.TryProcessAsync<TRequest, TResult>(request, context);
        }
    }
}
