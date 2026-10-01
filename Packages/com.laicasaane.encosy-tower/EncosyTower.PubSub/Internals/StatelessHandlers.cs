using System;
using EncosyTower.Common;
using EncosyTower.Tasks;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub.Internals
{
    internal sealed class HandlerFuncMessage<TMessage> : IHandler<TMessage>
    {
        private Func<TMessage, UnityTask> _handler;

        public HandlerFuncMessage(Func<TMessage, UnityTask> handler)
        {
            DebuggingThrowHelper.ThrowIfNull(handler);

            _handler = handler;
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public Result<UnityTask, StateUnavailableError> Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested)
            {
                return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
            }

            return Result<UnityTask, StateUnavailableError>.Succeed(
                _handler?.Invoke(message) ?? UnityTask.CompletedTask
            );
        }
    }

    internal sealed class HandlerFunc<TMessage> : IHandler<TMessage>
    {
        private Func<UnityTask> _handler;

        public HandlerFunc(Func<UnityTask> handler)
        {
            DebuggingThrowHelper.ThrowIfNull(handler);

            _handler = handler;
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public Result<UnityTask, StateUnavailableError> Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested)
            {
                return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
            }

            return Result<UnityTask, StateUnavailableError>.Succeed(_handler?.Invoke() ?? UnityTask.CompletedTask);
        }
    }

    internal sealed class HandlerActionMessage<TMessage> : IHandler<TMessage>
    {
        private Action<TMessage> _handler;

        public HandlerActionMessage(Action<TMessage> handler)
        {
            DebuggingThrowHelper.ThrowIfNull(handler);

            _handler = handler;
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public Result<UnityTask, StateUnavailableError> Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested)
            {
                return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
            }

            _handler?.Invoke(message);
            return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
        }
    }

    internal sealed class HandlerAction<TMessage> : IHandler<TMessage>
    {
        private Action _handler;

        public HandlerAction(Action handler)
        {
            DebuggingThrowHelper.ThrowIfNull(handler);

            _handler = handler;
            Id = new(handler);
        }

        public DelegateId Id { get; }

        public void Dispose()
        {
            _handler = null;
        }

        public Result<UnityTask, StateUnavailableError> Handle(TMessage message, PublishingContext context)
        {
            if (context.Token.IsCancellationRequested)
            {
                return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
            }

            _handler?.Invoke();
            return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
        }
    }
}
