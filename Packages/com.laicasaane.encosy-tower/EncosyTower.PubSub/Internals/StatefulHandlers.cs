using System;
using EncosyTower.Common;
using EncosyTower.Tasks;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub.Internals
{
    internal sealed class StatefulHandlerFuncMessage<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, TMessage, UnityTask> _handler;

        public StatefulHandlerFuncMessage(TState state, Func<TState, TMessage, UnityTask> handler)
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);
            DebuggingThrowHelper.ThrowIfNull(handler);

            _state = new(state);
            _handler = handler;
            Id = new(handler, state.GetHashCode());
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
            _state = null;
        }

        public Result<UnityTask, StateUnavailableError> Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested || _state == null)
            {
                return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
            }

            if (_state.TryGetTarget(out var state) == false)
            {
                return Result<UnityTask, StateUnavailableError>.Err(new StateUnavailableError(typeof(TState)));
            }

            return Result<UnityTask, StateUnavailableError>.Succeed(
                _handler?.Invoke(state, message) ?? UnityTask.CompletedTask
            );
        }
    }

    internal sealed class StatefulHandlerFunc<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, UnityTask> _handler;

        public StatefulHandlerFunc(TState state, Func<TState, UnityTask> handler)
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);
            DebuggingThrowHelper.ThrowIfNull(handler);

            _state = new(state);
            _handler = handler;
            Id = new(handler, state.GetHashCode());
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
            _state = null;
        }

        public Result<UnityTask, StateUnavailableError> Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested || _state == null)
            {
                return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
            }

            if (_state.TryGetTarget(out var state) == false)
            {
                return Result<UnityTask, StateUnavailableError>.Err(new StateUnavailableError(typeof(TState)));
            }

            return Result<UnityTask, StateUnavailableError>.Succeed(_handler?.Invoke(state) ?? UnityTask.CompletedTask);
        }
    }

    internal sealed class StatefulHandlerActionMessage<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Action<TState, TMessage> _handler;

        public StatefulHandlerActionMessage(TState state, Action<TState, TMessage> handler)
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);
            DebuggingThrowHelper.ThrowIfNull(handler);

            _state = new(state);
            _handler = handler;
            Id = new(handler, state.GetHashCode());
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
            _state = null;
        }

        public Result<UnityTask, StateUnavailableError> Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested || _state == null)
            {
                return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
            }

            if (_state.TryGetTarget(out var state) == false)
            {
                return Result<UnityTask, StateUnavailableError>.Err(new StateUnavailableError(typeof(TState)));
            }

            _handler?.Invoke(state, message);
            return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
        }
    }

    internal sealed class StatefulHandlerAction<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Action<TState> _handler;

        public StatefulHandlerAction(TState state, Action<TState> handler)
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);
            DebuggingThrowHelper.ThrowIfNull(handler);

            _state = new(state);
            _handler = handler;
            Id = new(handler, state.GetHashCode());
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
            _state = null;
        }

        public Result<UnityTask, StateUnavailableError> Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested || _state == null)
            {
                return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
            }

            if (_state.TryGetTarget(out var state) == false)
            {
                return Result<UnityTask, StateUnavailableError>.Err(new StateUnavailableError(typeof(TState)));
            }

            _handler?.Invoke(state);
            return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
        }
    }
}
