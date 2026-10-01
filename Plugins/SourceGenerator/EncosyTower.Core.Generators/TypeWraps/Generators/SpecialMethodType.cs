namespace EncosyTower.Core.Generators.TypeWraps
{
    [Flags]
    public enum SpecialMethodType
    {
        None        = 0,
        CompareTo   = 1 << 0,
        Equals      = 1 << 1,
        GetHashCode = 1 << 2,
        ToString    = 1 << 3,
    }
}
