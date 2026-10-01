using System;
using EncosyTower.Common;
using EncosyTower.Tasks;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub.Internals
{
    internal sealed class ContextualHandlerFuncMessage<TMessage> : IHandler<TMessage>
    {
        private Func<TMessage, PublishingContext, UnityTask> _handler;

        public ContextualHandlerFuncMessage(Func<TMessage, PublishingContext, UnityTask> handler)
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
                _handler?.Invoke(message, context) ?? UnityTask.CompletedTask
            );
        }
    }

    internal sealed class ContextualHandlerFunc<TMessage> : IHandler<TMessage>
    {
        private Func<PublishingContext, UnityTask> _handler;

        public ContextualHandlerFunc(Func<PublishingContext, UnityTask> handler)
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
                _handler?.Invoke(context) ?? UnityTask.CompletedTask
            );
        }
    }

    internal sealed class ContextualHandlerActionMessage<TMessage> : IHandler<TMessage>
    {
        private Action<TMessage, PublishingContext> _handler;

        public ContextualHandlerActionMessage(Action<TMessage, PublishingContext> handler)
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

            _handler?.Invoke(message, context);
            return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
        }
    }

    internal sealed class ContextualHandlerAction<TMessage> : IHandler<TMessage>
    {
        private Action<PublishingContext> _handler;

        public ContextualHandlerAction(Action<PublishingContext> handler)
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

            _handler?.Invoke(context);
            return Result<UnityTask, StateUnavailableError>.Succeed(UnityTask.CompletedTask);
        }
    }
}
