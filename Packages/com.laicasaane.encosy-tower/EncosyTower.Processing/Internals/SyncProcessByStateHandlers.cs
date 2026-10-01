using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Types;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing.Internals.Sync
{
    internal sealed class ProcessByStateHandler<TState, TRequest> : IProcessHandler<TRequest>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Action<TState, TRequest> _process;

        static ProcessByStateHandler()
        {
            s_typeId = (TypeId)Type<Action<TRequest, ProcessingContext>>.Id;
        }

        public ProcessByStateHandler([NotNull] TState state, [NotNull] Action<TState, TRequest> process)
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
        public Result<Success, StateUnavailableError> Process(TRequest arg, ProcessingContext context)
        {
            if (_state.TryGetTarget(out var state))
            {
                _process(state, arg);
                return Result<Success, StateUnavailableError>.Succeed(Success.Yes);
            }

            return Result<Success, StateUnavailableError>.Err(new StateUnavailableError(typeof(TState)));
        }
    }

    internal sealed class ContextualProcessByStateHandler<TState, TRequest>
        : IProcessHandler<TRequest>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Action<TState, TRequest, ProcessingContext> _process;

        static ContextualProcessByStateHandler()
        {
            s_typeId = (TypeId)Type<Action<TRequest, ProcessingContext>>.Id;
        }

        public ContextualProcessByStateHandler(
              [NotNull] TState state
            , [NotNull] Action<TState, TRequest, ProcessingContext> process
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
        public Result<Success, StateUnavailableError> Process(TRequest arg, ProcessingContext context)
        {
            if (_state.TryGetTarget(out var state))
            {
                _process(state, arg, context);
                return Result<Success, StateUnavailableError>.Succeed(Success.Yes);
            }

            return Result<Success, StateUnavailableError>.Err(new StateUnavailableError(typeof(TState)));
        }
    }

    internal sealed class ProcessByStateHandler<TState, TRequest, TResult>
        : IProcessHandler<TRequest, TResult>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, TResult> _process;

        static ProcessByStateHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, ProcessingContext, TResult>>.Id;
        }

        public ProcessByStateHandler([NotNull] TState state, [NotNull] Func<TState, TRequest, TResult> process)
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
        public Result<TResult, StateUnavailableError> Process(TRequest arg, ProcessingContext context)
        {
            if (_state.TryGetTarget(out var state))
            {
                return Result<TResult, StateUnavailableError>.Succeed(_process(state, arg));
            }

            return Result<TResult, StateUnavailableError>.Err(new StateUnavailableError(typeof(TState)));
        }
    }

    internal sealed class ContextualProcessByStateHandler<TState, TRequest, TResult>
        : IProcessHandler<TRequest, TResult>
        where TState : class
    {
        private static readonly TypeId s_typeId;
        private readonly WeakReference<TState> _state;
        private readonly Func<TState, TRequest, ProcessingContext, TResult> _process;

        static ContextualProcessByStateHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, ProcessingContext, TResult>>.Id;
        }

        public ContextualProcessByStateHandler(
              [NotNull] TState state
            , [NotNull] Func<TState, TRequest, ProcessingContext, TResult> process
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
        public Result<TResult, StateUnavailableError> Process(TRequest arg, ProcessingContext context)
        {
            if (_state.TryGetTarget(out var state))
            {
                return Result<TResult, StateUnavailableError>.Succeed(_process(state, arg, context));
            }

            return Result<TResult, StateUnavailableError>.Err(new StateUnavailableError(typeof(TState)));
        }
    }
}
