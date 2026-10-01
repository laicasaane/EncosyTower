using System;
using EncosyTower.EnumExtensions;

namespace EncosyTower.Persistences
{
    [Flags, EnumExtensions]
    public enum SaveDestination : byte
    {
        None = 0,
        Local = 1 << 0,
        Remote = 1 << 1,
    }
}
