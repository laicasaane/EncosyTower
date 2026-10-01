using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub.Internals
{
    internal sealed class Subscription<TMessage> : ISubscription
    {
        public static readonly Subscription<TMessage> None = new();

        private readonly WeakReference<MessageBroker<TMessage>> _broker;
        private readonly int _order;
        private IHandler<TMessage> _handler;

        private Subscription()
        {
        }

        public Subscription([NotNull] MessageBroker<TMessage> broker, [NotNull] IHandler<TMessage> handler, int order)
        {
            DebuggingThrowHelper.ThrowIfNull(broker);
            DebuggingThrowHelper.ThrowIfNull(handler);

            _broker = new WeakReference<MessageBroker<TMessage>>(broker);
            _handler = handler;
            _order = order;
        }

        public void Dispose()
        {
            var handler = Interlocked.Exchange(ref _handler, null);

            if (handler == null)
            {
                return;
            }

            if (_broker.TryGetTarget(out var broker))
            {
                broker.RemoveHandler(handler, _order);
            }

            handler.Dispose();
        }
    }

    internal static class SubscriptionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterTo<TMessage>(
              [NotNull] this Subscription<TMessage> subscription
            , CancellationToken unsubscribeToken
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(subscription);

            unsubscribeToken.Register(static x => ((Subscription<TMessage>)x)?.Dispose(), subscription);
        }
    }
}
