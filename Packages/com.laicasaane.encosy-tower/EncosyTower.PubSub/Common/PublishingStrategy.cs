namespace EncosyTower.PubSub
{
    public enum PublishingStrategy : byte
    {
        DropIfNoSubscriber = 0,
        WaitForSubscriber,
    }
}
