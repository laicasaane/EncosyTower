namespace EncosyTower.Core.Generators.TypeWraps
{
    [Flags]
    public enum InterfaceKind
    {
        None        = 0,
        EquatableT  = 1 << 0,
        Comparable  = 1 << 1,
        ComparableT = 1 << 2,
    }
}
