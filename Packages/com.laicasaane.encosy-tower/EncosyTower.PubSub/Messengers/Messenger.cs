using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.PubSub.Internals;
using EncosyTower.Tasks;
using EncosyTower.Vaults;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub
{
    public sealed class Messenger : IDisposable
    {
        private readonly SingletonVault<IMessageBroker> _messageBrokers = new();
        private readonly InterceptorBrokers _interceptorBrokers = new();

        public Messenger([NotNull] ArrayPool<UnityTask> taskArrayPool)
        {
            DebuggingThrowHelper.ThrowIfNull(taskArrayPool);

            Subscriber = new(_messageBrokers, _interceptorBrokers, taskArrayPool);
            Publisher = new(_messageBrokers, _interceptorBrokers, taskArrayPool);
        }

        public MessageSubscriber Subscriber { get; }

        public MessagePublisher Publisher { get; }

        public MessageInterceptors Interceptors => new(_interceptorBrokers);

        public void Dispose()
        {
            _messageBrokers.Dispose();
            _interceptorBrokers.Dispose();
        }
    }
}
