namespace EncosyTower.PubSub.Internals
{
    internal interface IScopedMessageBroker<TScope>
    {
        void Clear(TScope scope);
    }
}
