using System;
using EncosyTower.Common;
using EncosyTower.Tasks;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub.Internals
{
    internal sealed class StatefulContextualHandlerFuncMessage<TState, TMessage>
        : IHandler<TMessage>
        where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, TMessage, PublishingContext, UnityTask> _handler;

        public StatefulContextualHandlerFuncMessage(
              TState state
            , Func<TState, TMessage, PublishingContext, UnityTask> handler
        )
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
                _handler?.Invoke(state, message, context) ?? UnityTask.CompletedTask
            );
        }
    }

    internal sealed class StatefulContextualHandlerFunc<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Func<TState, PublishingContext, UnityTask> _handler;

        public StatefulContextualHandlerFunc(TState state, Func<TState, PublishingContext, UnityTask> handler)
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
                _handler?.Invoke(state, context) ?? UnityTask.CompletedTask
            );
        }
    }

    internal sealed class StatefulContextualHandlerActionMessage<TState, TMessage>
        : IHandler<TMessage>
        where TState : class
    {
        private WeakReference<TState> _state;
        private Action<TState, TMessage, PublishingContext> _handler;

        public StatefulContextualHandlerActionMessage(TState state, Action<TState, TMessage, PublishingContext> handler)
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

            _handler?.Invoke(state, message, context);
            return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
        }
    }

    internal sealed class StatefulContextualHandlerAction<TState, TMessage> : IHandler<TMessage> where TState : class
    {
        private WeakReference<TState> _state;
        private Action<TState, PublishingContext> _handler;

        public StatefulContextualHandlerAction(TState state, Action<TState, PublishingContext> handler)
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

            _handler?.Invoke(state, context);
            return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
        }
    }
}
