namespace EncosyTower.Persistences
{
    public enum SourcePriority : byte
    {
        RemoteThenLocal,
        LocalThenRemote,
        OnlyRemote,
        OnlyLocal,
    }
}
