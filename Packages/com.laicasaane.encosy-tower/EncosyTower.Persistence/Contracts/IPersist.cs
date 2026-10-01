namespace EncosyTower.Persistences
{
    public interface IPersist
    {
        string Id { get; set; }

        int Version { get; set; }
    }
}
