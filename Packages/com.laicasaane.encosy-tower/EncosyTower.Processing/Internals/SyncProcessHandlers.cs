using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Types;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing.Internals.Sync
{
    internal sealed class ProcessHandler<TRequest> : IProcessHandler<TRequest>
    {
        private static readonly TypeId s_typeId;
        private readonly Action<TRequest> _process;

        static ProcessHandler()
        {
            s_typeId = (TypeId)Type<Action<TRequest, ProcessingContext>>.Id;
        }

        public ProcessHandler([NotNull] Action<TRequest> process)
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
        public Result<Success, StateUnavailableError> Process(TRequest arg, ProcessingContext context)
        {
            _process(arg);
            return Result<Success, StateUnavailableError>.Succeed(Success.Yes);
        }
    }

    internal sealed class ContextualProcessHandler<TRequest> : IProcessHandler<TRequest>
    {
        private static readonly TypeId s_typeId;
        private readonly Action<TRequest, ProcessingContext> _process;

        static ContextualProcessHandler()
        {
            s_typeId = (TypeId)Type<Action<TRequest, ProcessingContext>>.Id;
        }

        public ContextualProcessHandler([NotNull] Action<TRequest, ProcessingContext> process)
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
        public Result<Success, StateUnavailableError> Process(TRequest arg, ProcessingContext context)
        {
            _process(arg, context);
            return Result<Success, StateUnavailableError>.Succeed(Success.Yes);
        }
    }

    internal sealed class ProcessHandler<TRequest, TResult> : IProcessHandler<TRequest, TResult>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, TResult> _process;

        static ProcessHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, ProcessingContext, TResult>>.Id;
        }

        public ProcessHandler([NotNull] Func<TRequest, TResult> process)
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
        public Result<TResult, StateUnavailableError> Process(TRequest arg, ProcessingContext context)
        {
            return Result<TResult, StateUnavailableError>.Succeed(_process(arg));
        }
    }

    internal sealed class ContextualProcessHandler<TRequest, TResult>
        : IProcessHandler<TRequest, TResult>
    {
        private static readonly TypeId s_typeId;
        private readonly Func<TRequest, ProcessingContext, TResult> _process;

        static ContextualProcessHandler()
        {
            s_typeId = (TypeId)Type<Func<TRequest, ProcessingContext, TResult>>.Id;
        }

        public ContextualProcessHandler([NotNull] Func<TRequest, ProcessingContext, TResult> process)
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
        public Result<TResult, StateUnavailableError> Process(TRequest arg, ProcessingContext context)
        {
            return Result<TResult, StateUnavailableError>.Succeed(_process(arg, context));
        }
    }
}
