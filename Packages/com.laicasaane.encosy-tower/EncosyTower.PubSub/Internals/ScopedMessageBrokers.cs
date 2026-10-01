using EncosyTower.Collections;

namespace EncosyTower.PubSub.Internals
{
    internal sealed class ScopedMessageBrokers<TScope> : IMessageBroker
    {
        private readonly ArraySet<IScopedMessageBroker<TScope>> _brokers = new();

        public bool Add(IScopedMessageBroker<TScope> broker)
            => _brokers.Add(broker);

        public void Clear(TScope scope)
        {
            var brokers = _brokers.GetItems();

            for (var i = brokers.Length - 1; i >= 0; i--)
            {
                brokers[i]?.Clear(scope);
            }
        }

        public void Dispose()
            => _brokers.Clear();
    }
}
