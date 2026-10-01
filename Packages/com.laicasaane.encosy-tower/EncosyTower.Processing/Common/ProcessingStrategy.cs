namespace EncosyTower.Processing
{
    public enum ProcessingStrategy : byte
    {
        DropIfNoHandler = 0,
        WaitForHandler,
    }
}
