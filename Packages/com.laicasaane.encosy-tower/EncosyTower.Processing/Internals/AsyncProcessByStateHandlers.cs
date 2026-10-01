using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Tasks;
using EncosyTower.Types;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing.Internals.Async
{
    internal sealed class AsyncProcessByStateHandler<TState, TRequest>
        : IAsyncProcessHandler<TRequest>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, UnityTask> _process;

        static AsyncProcessByStateHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, ProcessingContext, UnityTask>>.Id;
        }

        public AsyncProcessByStateHandler([NotNull] TState state, [NotNull] Func<TState, TRequest, UnityTask> process)
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);
            DebuggingThrowHelper.ThrowIfNull(process);

            _state = new(state);
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
            if (_state.TryGetTarget(out var state))
            {
                return Result<UnityTask, StateUnavailableError>.Succeed(_process(state, request));
            }

            return Result<UnityTask, StateUnavailableError>.Err(new StateUnavailableError(typeof(TState)));
        }
    }

    internal sealed class AsyncProcessByStateHandler<TState, TRequest, TResult>
        : IAsyncProcessHandler<TRequest, TResult>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;

        private readonly Func<TState, TRequest, UnityTask<TResult>> _process;

        static AsyncProcessByStateHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, ProcessingContext, UnityTask<TResult>>>.Id;
        }

        public AsyncProcessByStateHandler(
              [NotNull] TState state
            , [NotNull] Func<TState, TRequest, UnityTask<TResult>> process
        )
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);
            DebuggingThrowHelper.ThrowIfNull(process);

            _state = new(state);
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
            if (_state.TryGetTarget(out var state))
            {
                return Result<UnityTask<TResult>, StateUnavailableError>.Succeed(_process(state, request));
            }

            return Result<UnityTask<TResult>, StateUnavailableError>.Err(new StateUnavailableError(typeof(TState)));
        }
    }

    internal sealed class ContextualAsyncProcessByStateHandler<TState, TRequest>
        : IAsyncProcessHandler<TRequest>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<
              TState
            , TRequest
            , ProcessingContext
            , UnityTask
        > _process;

        static ContextualAsyncProcessByStateHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, ProcessingContext, UnityTask>>.Id;
        }

        public ContextualAsyncProcessByStateHandler(
              [NotNull] TState state
            , [NotNull] Func<
                  TState
                , TRequest
                , ProcessingContext
                , UnityTask
            > process
        )
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);
            DebuggingThrowHelper.ThrowIfNull(process);

            _state = new(state);
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
            if (_state.TryGetTarget(out var state))
            {
                return Result<UnityTask, StateUnavailableError>.Succeed(_process(state, request, context));
            }

            return Result<UnityTask, StateUnavailableError>.Err(new StateUnavailableError(typeof(TState)));
        }
    }

    internal sealed class ContextualAsyncProcessByStateHandler<TState, TRequest, TResult>
        : IAsyncProcessHandler<TRequest, TResult>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;

        private readonly Func<
              TState
            , TRequest
            , ProcessingContext
            , UnityTask<TResult>
        > _process;

        static ContextualAsyncProcessByStateHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, ProcessingContext, UnityTask<TResult>>>.Id;
        }

        public ContextualAsyncProcessByStateHandler(
              [NotNull] TState state
            , [NotNull] Func<
                  TState
                , TRequest
                , ProcessingContext
                , UnityTask<TResult>
              > process
        )
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);
            DebuggingThrowHelper.ThrowIfNull(process);

            _state = new(state);
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
            if (_state.TryGetTarget(out var state))
            {
                return Result<UnityTask<TResult>, StateUnavailableError>.Succeed(_process(state, request, context));
            }

            return Result<UnityTask<TResult>, StateUnavailableError>.Err(new StateUnavailableError(typeof(TState)));
        }
    }
}
