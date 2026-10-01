using System;

namespace EncosyTower.Persistences
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PersistenceAttribute : Attribute
    {
    }
}
