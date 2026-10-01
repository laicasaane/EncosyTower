using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Tasks;
using EncosyTower.Types;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing.Internals.Async
{
    internal sealed class AsyncProcessHandler<TRequest> : IAsyncProcessHandler<TRequest>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, UnityTask> _process;

        static AsyncProcessHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, ProcessingContext, UnityTask>>.Id;
        }

        public AsyncProcessHandler([NotNull] Func<TRequest, UnityTask> process)
        {
            DebuggingThrowHelper.ThrowIfNull(process);
            _process = process;
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<UnityTask, StateUnavailableError> ProcessAsync(TRequest request, ProcessingContext context)
        {
            return Result<UnityTask, StateUnavailableError>.Succeed(_process(request));
        }
    }

    internal sealed class ContextualAsyncProcessHandler<TRequest>
        : IAsyncProcessHandler<TRequest>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, ProcessingContext, UnityTask> _process;

        static ContextualAsyncProcessHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, ProcessingContext, UnityTask>>.Id;
        }

        public ContextualAsyncProcessHandler([NotNull] Func<TRequest, ProcessingContext, UnityTask> process)
        {
            DebuggingThrowHelper.ThrowIfNull(process);
            _process = process;
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<UnityTask, StateUnavailableError> ProcessAsync(TRequest request, ProcessingContext context)
        {
            return Result<UnityTask, StateUnavailableError>.Succeed(_process(request, context));
        }
    }

    internal sealed class AsyncProcessHandler<TRequest, TResult>
        : IAsyncProcessHandler<TRequest, TResult>
    {
        private static readonly TypeId s_typeId;

        private readonly Func<TRequest, UnityTask<TResult>> _process;

        static AsyncProcessHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, ProcessingContext, UnityTask<TResult>>>.Id;
        }

        public AsyncProcessHandler([NotNull] Func<TRequest, UnityTask<TResult>> process)
        {
            DebuggingThrowHelper.ThrowIfNull(process);
            _process = process;
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<UnityTask<TResult>, StateUnavailableError> ProcessAsync(
              TRequest request
            , ProcessingContext context
        )
        {
            return Result<UnityTask<TResult>, StateUnavailableError>.Succeed(_process(request));
        }
    }

    internal sealed class ContextualAsyncProcessHandler<TRequest, TResult>
        : IAsyncProcessHandler<TRequest, TResult>
    {
        private static readonly TypeId s_typeId;

        private readonly Func<
              TRequest
            , ProcessingContext
            , UnityTask<TResult>
        > _process;

        static ContextualAsyncProcessHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, ProcessingContext, UnityTask<TResult>>>.Id;
        }

        public ContextualAsyncProcessHandler([NotNull] Func<TRequest, ProcessingContext, UnityTask<TResult>> process)
        {
            DebuggingThrowHelper.ThrowIfNull(process);
            _process = process;
        }

        public TypeId Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_typeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<UnityTask<TResult>, StateUnavailableError> ProcessAsync(
              TRequest request
            , ProcessingContext context
        )
        {
            return Result<UnityTask<TResult>, StateUnavailableError>.Succeed(_process(request, context));
        }
    }
}
